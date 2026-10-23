using KleeneStar.Core.WebIntegration;
using KleeneStar.Templates.WebIntegration;
using System.Text;

namespace KleeneStar.Templates.Test.WebIntegration
{
    /// <summary>
    /// Tests the development integrations: each reads its system's payload into events on the
    /// objects named in it, with the other system's own identity of the event as occurrence.
    /// </summary>
    public class UnitTestDevIntegrations
    {
        /// <summary>
        /// Builds a delivery.
        /// </summary>
        /// <param name="json">The body.</param>
        /// <returns>The delivery.</returns>
        private static IntegrationDelivery Delivery(string json)
        {
            return new IntegrationDelivery { Body = Encoding.UTF8.GetBytes(json) };
        }

        /// <summary>
        /// A GitLab push names the objects of its commit messages, once per commit.
        /// </summary>
        [Fact]
        public void GitLab_PushNamesTheCommitsObjects()
        {
            var events = new GitLabIntegration().Translate(Delivery("""
                { "object_kind": "push", "ref": "refs/heads/feature/DEV-7-login", "project": { "id": 42 },
                  "commits": [ { "id": "a1", "message": "DEV-7: login form" }, { "id": "b2", "message": "fix DEV-8 and DEV-7" } ] }
                """));

            Assert.Equal(["DEV-7", "DEV-7", "DEV-8"], events.Select(x => x.ObjectKey).Order());
            Assert.All(events, x => Assert.Equal("pushed", x.Event));
            Assert.Equal(3, events.Select(x => x.Occurrence).Distinct().Count());
        }

        /// <summary>
        /// A GitLab pipeline reports its status; the same run and status is the same occurrence.
        /// </summary>
        [Fact]
        public void GitLab_PipelineReportsItsStatus()
        {
            const string body = """
                { "object_kind": "pipeline", "project": { "id": 42 },
                  "object_attributes": { "id": 900, "status": "failed", "ref": "DEV-9-cache" },
                  "commit": { "message": "speed up" } }
                """;

            var first = Assert.Single(new GitLabIntegration().Translate(Delivery(body)));
            var second = Assert.Single(new GitLabIntegration().Translate(Delivery(body)));

            Assert.Equal(("DEV-9", "pipeline.failed"), (first.ObjectKey, first.Event));
            Assert.Equal(first.Occurrence, second.Occurrence);
        }

        /// <summary>
        /// A GitLab merge request and a successful deployment become their events; an unknown
        /// kind raises nothing.
        /// </summary>
        [Fact]
        public void GitLab_MergeRequestAndDeployment()
        {
            var merged = Assert.Single(new GitLabIntegration().Translate(Delivery("""
                { "object_kind": "merge_request", "project": { "id": 1 }, "object_attributes": { "iid": 5, "action": "merge", "title": "Resolve SD-12" } }
                """)));

            Assert.Equal(("SD-12", "merge_request.merge"), (merged.ObjectKey, merged.Event));

            var deployed = Assert.Single(new GitLabIntegration().Translate(Delivery("""
                { "object_kind": "deployment", "status": "success", "deployment_id": 77, "commit_title": "REL-3 go live", "project": { "id": 1 } }
                """)));

            Assert.Equal(("REL-3", "deployed"), (deployed.ObjectKey, deployed.Event));
            Assert.Empty(new GitLabIntegration().Translate(Delivery("""{ "object_kind": "wiki_page" }""")));
            Assert.Empty(new GitLabIntegration().Translate(Delivery("not json")));
        }

        /// <summary>
        /// Jenkins reports the end of a build once, whether it says COMPLETED or FINALIZED, with
        /// the keys of its parameters and branch.
        /// </summary>
        [Fact]
        public void Jenkins_ReportsTheEndOnce()
        {
            static IntegrationEvent End(string phase) => Assert.Single(new JenkinsIntegration().Translate(Delivery($$"""
                { "name": "app-build", "build": { "number": 31, "phase": "{{phase}}", "status": "FAILURE",
                  "parameters": { "TICKET": "DEV-4" }, "scm": { "branch": "main" } } }
                """)));

            var completed = End("COMPLETED");
            var finalized = End("FINALIZED");

            Assert.Equal(("DEV-4", "pipeline.failed"), (completed.ObjectKey, completed.Event));
            Assert.Equal(completed.Occurrence, finalized.Occurrence);
        }

        /// <summary>
        /// Automic reports a job's result for the object its reference names.
        /// </summary>
        [Fact]
        public void Automic_ReportsTheJobResult()
        {
            var ok = Assert.Single(new AutomicIntegration().Translate(Delivery("""{ "run_id": 1001, "object_name": "JOBS.NIGHTLY", "status": "ENDED_OK", "reference": "OPS-15" }""")));
            var failed = Assert.Single(new AutomicIntegration().Translate(Delivery("""{ "run_id": 1002, "status": "ENDED_NOT_OK", "reference": "OPS-15" }""")));

            Assert.Equal(("OPS-15", "job.succeeded"), (ok.ObjectKey, ok.Event));
            Assert.Equal("job.failed", failed.Event);
        }

        /// <summary>
        /// The plugin registers its integrations with the core's catalog.
        /// </summary>
        [Fact]
        public void Register_AddsTheIntegrationsToTheCatalog()
        {
            DevIntegrations.Register();

            Assert.NotNull(IntegrationCatalog.Get("gitlab"));
            Assert.NotNull(IntegrationCatalog.Get("jenkins"));
            Assert.NotNull(IntegrationCatalog.Get("automic"));
        }
    }
}
