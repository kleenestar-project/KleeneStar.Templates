using KleeneStar.Automation;
using KleeneStar.Core.WebWorkspaceTemplate;
using KleeneStar.Model.Entities;
using KleeneStar.Templates.Test.Fixture;
using KleeneStar.Templates.WebWorkspaceTemplate;

namespace KleeneStar.Templates.Test.WebWorkspaceTemplate
{
    /// <summary>
    /// Tests the automation rules a template of this plugin sets up when the core applied it:
    /// acting as the creator, through the automation plugin, skipping what names no class or does
    /// not validate, and adding nothing twice; without a creator the rules wait disabled.
    /// </summary>
    [Collection("NonParallelTests")]
    public class UnitTestTemplateAutomations
    {
        private static readonly Guid WorkspaceId = Guid.Parse("7E0A7E00-0000-4000-8000-000000000001");
        private static readonly Guid ClassId = Guid.Parse("7E0A7E00-0000-4000-8000-000000000002");
        private static readonly Guid AuthorId = Guid.Parse("7E0A7E00-0000-4000-8000-000000000003");

        /// <summary>
        /// A template with a valid rule, a rule naming a missing class and a rule that does not validate.
        /// </summary>
        private sealed class ProbeTemplate : WorkspaceTemplateBase
        {
            protected override string Slug => "automation-probe";

            public override string SuggestedKey => "APR";

            public override IEnumerable<WorkspaceTemplateClass> Classes => [Class("Ticket", "ticket")];

            public override IEnumerable<WorkspaceTemplateAutomation> Automations =>
            [
                new WorkspaceTemplateAutomation { Name = "Welcome", Class = "Ticket", Trigger = "object.created", Actions = "comment Welcome, {creator}." },
                new WorkspaceTemplateAutomation { Name = "Lost", Class = "Missing", Trigger = "object.created", Actions = "comment Lost." },
                new WorkspaceTemplateAutomation { Name = "Broken", Class = "Ticket", Trigger = "object.created", Actions = "fly to the moon" }
            ];
        }

        /// <summary>
        /// Seeds a workspace with its class and the creator.
        /// </summary>
        /// <param name="database">The database.</param>
        private static void Seed(string database)
        {
            TemplateHubFixture.Initialize(database);

            using var db = TemplateHubFixture.CreateDbContext(database);

            db.Workspaces.Add(new Workspace { Id = WorkspaceId, Key = "APR", Name = "Automation probe" });
            db.Classes.Add(new Class { Id = ClassId, Name = "Ticket", WorkspaceId = WorkspaceId });
            db.Identities.Add(new Identity { Id = AuthorId, Name = "Author", UserName = "author", Email = "author@example.com", PasswordHash = "x" });
            db.SaveChanges();
        }

        /// <summary>
        /// Announces an application of the probe template.
        /// </summary>
        /// <param name="identityId">The creator.</param>
        /// <returns>The application.</returns>
        private static WorkspaceTemplateApplication Applied(Guid identityId)
        {
            return new WorkspaceTemplateApplication(new ProbeTemplate(), WorkspaceId, identityId, null, new WorkspaceTemplateProcessResult());
        }

        /// <summary>
        /// The valid rule is created, acting as the creator and naming them; the others are skipped
        /// with a reason; a second application adds nothing.
        /// </summary>
        [Fact]
        public void Apply_SetsUpTheRules()
        {
            Seed(nameof(Apply_SetsUpTheRules));

            var application = Applied(AuthorId);
            var created = TemplateAutomations.Apply(application);

            var rule = Assert.Single(created);
            Assert.Equal("Welcome", rule.Name);
            Assert.Equal(ClassId, rule.ClassId);
            Assert.Equal(AuthorId, rule.RunAsId);
            Assert.Equal(AutomationRuleState.Active, rule.State);
            Assert.Equal("comment Welcome, author.", rule.Actions);

            Assert.Contains(application.Processes.Skipped, x => x.StartsWith("Lost", StringComparison.Ordinal));
            Assert.Contains(application.Processes.Skipped, x => x.StartsWith("Broken", StringComparison.Ordinal));
            Assert.Single(AutomationHub.AutomationManager.GetRules(WorkspaceId));

            Assert.Empty(TemplateAutomations.Apply(Applied(AuthorId)));
            Assert.Single(AutomationHub.AutomationManager.GetRules(WorkspaceId));
        }

        /// <summary>
        /// Without a creator there is nobody to act as: the rule waits disabled.
        /// </summary>
        [Fact]
        public void Apply_WithoutCreatorDisablesTheRules()
        {
            Seed(nameof(Apply_WithoutCreatorDisablesTheRules));

            var rule = Assert.Single(TemplateAutomations.Apply(Applied(Guid.Empty)));

            Assert.Equal(AutomationRuleState.Disabled, rule.State);
            Assert.Null(rule.RunAsId);
        }
    }
}
