using KleeneStar.Core.WebIntegration;
using KleeneStar.Templates.WebWorkspaceTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace KleeneStar.Templates.WebIntegration
{
    /// <summary>
    /// The development integrations the plugin ships: GitLab, Jenkins and Automic. Each reads
    /// what its system posts to an incoming webhook and names the objects it concerns by the
    /// keys found in commit messages, branch names, titles and parameters (<c>DEV-42</c>).
    /// </summary>
    /// <remarks>
    /// The events are what rules narrow to (<c>integration.received</c> with the event as
    /// parameter): <c>pushed</c>, <c>merge_request.open|merge|close</c>,
    /// <c>pipeline.running|success|failed</c>, <c>deployed</c>, <c>job.succeeded|failed</c>.
    /// The occurrence is the other system's own identity of the event - a commit, a pipeline
    /// run and its status, a build number - so a redelivery raises nothing.
    /// </remarks>
    public static partial class DevIntegrations
    {
        /// <summary>
        /// Gets the integrations.
        /// </summary>
        public static IReadOnlyList<IIntegration> All { get; } = [new GitLabIntegration(), new JenkinsIntegration(), new AutomicIntegration()];

        [GeneratedRegex(@"\b[A-Z][A-Z0-9]{1,9}-\d+\b")]
        private static partial Regex KeyRegex();

        /// <summary>
        /// Registers the integrations with the core.
        /// </summary>
        public static void Register()
        {
            foreach (var integration in All)
            {
                IntegrationCatalog.Register(integration);
            }
        }

        /// <summary>
        /// Finds the object keys in texts.
        /// </summary>
        /// <param name="texts">The texts.</param>
        /// <returns>The keys, each once.</returns>
        internal static IReadOnlyList<string> Keys(IEnumerable<string> texts)
        {
            return [.. texts
                .Where(x => !string.IsNullOrEmpty(x))
                .SelectMany(x => KeyRegex().Matches(x).Select(m => m.Value))
                .Distinct(StringComparer.Ordinal)];
        }

        /// <summary>
        /// Parses a delivery's body.
        /// </summary>
        /// <param name="delivery">The delivery.</param>
        /// <returns>The root object, or <see langword="null"/>.</returns>
        internal static JsonElement? Parse(IntegrationDelivery delivery)
        {
            try
            {
                var root = JsonDocument.Parse(delivery?.Body ?? []).RootElement;

                return root.ValueKind == JsonValueKind.Object ? root : null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Reads a property path as text.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="path">The property names.</param>
        /// <returns>The value, or <see langword="null"/>.</returns>
        internal static string Read(JsonElement element, params string[] path)
        {
            var current = element;

            foreach (var name in path)
            {
                if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(name, out current))
                {
                    return null;
                }
            }

            return current.ValueKind switch
            {
                JsonValueKind.String => current.GetString(),
                JsonValueKind.Number => current.GetRawText(),
                _ => null
            };
        }

        /// <summary>
        /// Returns every string inside an element - parameters, nested values.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The strings.</returns>
        internal static IEnumerable<string> Strings(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    yield return element.GetString();
                    break;
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject())
                    {
                        foreach (var value in Strings(property.Value))
                        {
                            yield return value;
                        }
                    }

                    break;
                case JsonValueKind.Array:
                    foreach (var item in element.EnumerateArray())
                    {
                        foreach (var value in Strings(item))
                        {
                            yield return value;
                        }
                    }

                    break;
            }
        }

        /// <summary>
        /// Builds the events of a delivery: one per object key found.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <param name="name">The event.</param>
        /// <param name="occurrence">The occurrence in the other system.</param>
        /// <returns>The events.</returns>
        internal static IReadOnlyList<IntegrationEvent> Events(IEnumerable<string> keys, string name, string occurrence)
        {
            return [.. keys.Select(key => new IntegrationEvent(key, name, $"{occurrence}:{key}"))];
        }
    }

    /// <summary>
    /// GitLab: push, merge request, pipeline and deployment hooks. Configure the project's
    /// webhook with the hook's address and the token appended as <c>&amp;token=</c>.
    /// </summary>
    public sealed class GitLabIntegration : IIntegration
    {
        /// <inheritdoc/>
        public string Key => "gitlab";

        /// <inheritdoc/>
        public string Label => TemplateText.Key("GitLab (push, merge request, pipeline, deployment)");

        /// <inheritdoc/>
        public IReadOnlyList<IntegrationEvent> Translate(IntegrationDelivery delivery)
        {
            if (DevIntegrations.Parse(delivery) is not JsonElement root)
            {
                return [];
            }

            var project = DevIntegrations.Read(root, "project", "id") ?? DevIntegrations.Read(root, "project_id") ?? "0";

            switch (DevIntegrations.Read(root, "object_kind"))
            {
                case "push":
                    if (!root.TryGetProperty("commits", out var commits) || commits.ValueKind != JsonValueKind.Array)
                    {
                        return [];
                    }

                    return [.. commits.EnumerateArray().SelectMany(commit => DevIntegrations.Events
                    (
                        DevIntegrations.Keys([DevIntegrations.Read(commit, "message"), DevIntegrations.Read(root, "ref")]),
                        "pushed",
                        $"gitlab:{project}:commit:{DevIntegrations.Read(commit, "id")}"
                    ))];
                case "merge_request":
                    var action = DevIntegrations.Read(root, "object_attributes", "action") ?? "update";

                    return DevIntegrations.Events
                    (
                        DevIntegrations.Keys([DevIntegrations.Read(root, "object_attributes", "title"), DevIntegrations.Read(root, "object_attributes", "description"), DevIntegrations.Read(root, "object_attributes", "source_branch")]),
                        "merge_request." + action,
                        $"gitlab:{project}:mr:{DevIntegrations.Read(root, "object_attributes", "iid")}:{action}"
                    );
                case "pipeline":
                    var status = DevIntegrations.Read(root, "object_attributes", "status") ?? "unknown";

                    return DevIntegrations.Events
                    (
                        DevIntegrations.Keys([DevIntegrations.Read(root, "commit", "message"), DevIntegrations.Read(root, "object_attributes", "ref")]),
                        "pipeline." + status,
                        $"gitlab:{project}:pipeline:{DevIntegrations.Read(root, "object_attributes", "id")}:{status}"
                    );
                case "deployment":
                    var state = DevIntegrations.Read(root, "status");

                    return state != "success" ? [] : DevIntegrations.Events
                    (
                        DevIntegrations.Keys([DevIntegrations.Read(root, "commit_title"), DevIntegrations.Read(root, "ref")]),
                        "deployed",
                        $"gitlab:{project}:deployment:{DevIntegrations.Read(root, "deployment_id")}"
                    );
                default:
                    return [];
            }
        }
    }

    /// <summary>
    /// Jenkins: the build notifications of the Notification plugin (JSON). Keys are read from
    /// the job name, the build parameters, the branch and the commit messages.
    /// </summary>
    public sealed class JenkinsIntegration : IIntegration
    {
        /// <inheritdoc/>
        public string Key => "jenkins";

        /// <inheritdoc/>
        public string Label => TemplateText.Key("Jenkins (build notifications)");

        /// <inheritdoc/>
        public IReadOnlyList<IntegrationEvent> Translate(IntegrationDelivery delivery)
        {
            if (DevIntegrations.Parse(delivery) is not JsonElement root || !root.TryGetProperty("build", out var build))
            {
                return [];
            }

            var phase = DevIntegrations.Read(build, "phase")?.ToUpperInvariant();
            var status = DevIntegrations.Read(build, "status")?.ToUpperInvariant();

            // a build reports COMPLETED and FINALIZED; both are its end, and it ends once
            var (name, stage) = phase switch
            {
                "STARTED" or "QUEUED" => ("pipeline.running", "start"),
                "COMPLETED" or "FINALIZED" => (status switch
                {
                    "SUCCESS" => "pipeline.success",
                    "FAILURE" => "pipeline.failed",
                    _ => "pipeline." + (status ?? "unknown").ToLowerInvariant()
                }, "end"),
                _ => (null, null)
            };

            if (name is null)
            {
                return [];
            }

            var texts = new List<string> { DevIntegrations.Read(root, "name"), DevIntegrations.Read(build, "scm", "branch") };

            if (build.TryGetProperty("parameters", out var parameters))
            {
                texts.AddRange(DevIntegrations.Strings(parameters));
            }

            if (build.TryGetProperty("scm", out var scm) && scm.TryGetProperty("changes", out var changes))
            {
                texts.AddRange(DevIntegrations.Strings(changes));
            }

            return DevIntegrations.Events(DevIntegrations.Keys(texts), name, $"jenkins:{DevIntegrations.Read(root, "name")}:{DevIntegrations.Read(build, "number")}:{stage}");
        }
    }

    /// <summary>
    /// Automic: a job's end as the Automation Engine's notification posts it -
    /// <c>{ "run_id", "object_name", "status", "reference" }</c>, where <c>reference</c> (or any
    /// parameter value) names the object.
    /// </summary>
    public sealed class AutomicIntegration : IIntegration
    {
        /// <inheritdoc/>
        public string Key => "automic";

        /// <inheritdoc/>
        public string Label => TemplateText.Key("Automic (job results)");

        /// <inheritdoc/>
        public IReadOnlyList<IntegrationEvent> Translate(IntegrationDelivery delivery)
        {
            if (DevIntegrations.Parse(delivery) is not JsonElement root)
            {
                return [];
            }

            var status = DevIntegrations.Read(root, "status")?.ToUpperInvariant() ?? "UNKNOWN";
            var name = status switch
            {
                "ENDED_OK" or "ENDED_OK_OR_INACTIVE" => "job.succeeded",
                _ when status.StartsWith("ENDED_NOT_OK", StringComparison.Ordinal) || status.Contains("ABEND", StringComparison.Ordinal) || status.Contains("CANCEL", StringComparison.Ordinal) => "job.failed",
                _ => "job." + status.ToLowerInvariant()
            };

            return DevIntegrations.Events
            (
                DevIntegrations.Keys(DevIntegrations.Strings(root)),
                name,
                $"automic:{DevIntegrations.Read(root, "run_id")}:{status}"
            );
        }
    }
}
