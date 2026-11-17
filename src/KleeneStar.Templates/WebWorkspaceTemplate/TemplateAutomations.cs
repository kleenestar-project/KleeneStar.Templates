using KleeneStar.Automation;
using KleeneStar.Core;
using KleeneStar.Core.WebWorkspaceTemplate;
using KleeneStar.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WebExpress.WebIndex.Queries;

namespace KleeneStar.Templates.WebWorkspaceTemplate
{
    /// <summary>
    /// Sets up the automation rules a template of this plugin brings
    /// (<see cref="WorkspaceTemplateBase.Automations"/>) when the core has applied it: the core
    /// announces the application (<c>IWorkspaceTemplateManager.TemplateApplied</c>) and knows no
    /// automation; the rules are written through the automation plugin.
    /// </summary>
    /// <remarks>
    /// Every rule passes the same validation as one written by hand, and what does not pass is
    /// skipped and reported in the result rather than stored half-working. Applying a template
    /// again adds what is missing, not a second copy of a rule.
    /// </remarks>
    public static class TemplateAutomations
    {
        private static int _connected;

        /// <summary>
        /// Listens to the core's template manager. Called once by the plugin; a second call does nothing.
        /// </summary>
        public static void Connect()
        {
            if (Interlocked.Exchange(ref _connected, 1) == 1)
            {
                return;
            }

            CoreHub.WorkspaceTemplateManager.TemplateApplied += (_, application) => Apply(application);
        }

        /// <summary>
        /// Sets up the rules of an applied template.
        /// </summary>
        /// <param name="application">The application the core announced.</param>
        /// <returns>The rules created.</returns>
        public static IReadOnlyList<AutomationRule> Apply(WorkspaceTemplateApplication application)
        {
            var created = new List<AutomationRule>();

            if (application?.Template is not WorkspaceTemplateBase template)
            {
                return created;
            }

            var automations = template.Automations?.ToList() ?? [];

            if (automations.Count == 0)
            {
                return created;
            }

            var processes = application.Processes;
            var manager = AutomationHub.AutomationManager;

            if (manager is null)
            {
                processes?.Skipped.AddRange(automations.Select(x => $"{x.Name}: the automation plugin is not available."));

                return created;
            }

            var culture = application.Culture;
            var identityId = application.IdentityId;
            var creator = identityId == Guid.Empty ? null : CoreHub.IdentityManager.GetIdentity(identityId)?.UserName;
            var classes = CoreHub.ClassManager
                .GetClasses(new Query<Class>().WhereEquals(x => x.WorkspaceId, application.WorkspaceId))
                .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var automation in automations)
            {
                Class @class = null;

                if (automation.Class is not null && !classes.TryGetValue(automation.Class, out @class))
                {
                    processes?.Skipped.Add($"{automation.Name}: the workspace has no class '{automation.Class}'.");
                    continue;
                }

                var name = WorkspaceTemplateProcesses.Text(automation.Name, culture);

                // applying a template again adds what is missing, not a second copy of each rule
                if (manager.GetRules(application.WorkspaceId).Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var rule = new AutomationRule
                {
                    Id = Guid.NewGuid(),
                    WorkspaceId = application.WorkspaceId,
                    ClassId = @class?.Id,
                    Name = name,
                    Description = WorkspaceTemplateProcesses.Text(automation.Description, culture),
                    Trigger = automation.Trigger,
                    TriggerParameter = string.IsNullOrWhiteSpace(automation.Parameter) ? null : WorkspaceTemplateProcesses.Resolve(automation.Parameter, creator, culture),
                    Condition = automation.Condition,
                    Actions = WorkspaceTemplateProcesses.Resolve(automation.Actions, creator, culture),
                    RunAsId = identityId == Guid.Empty ? null : identityId,
                    CreatedById = identityId == Guid.Empty ? null : identityId,

                    // a rule with nobody to act as would fail its first job; it waits for one
                    State = identityId == Guid.Empty ? AutomationRuleState.Disabled : AutomationRuleState.Active
                };

                var problems = manager.Validate(rule);

                if (problems.Count > 0)
                {
                    processes?.Skipped.Add($"{rule.Name}: {string.Join(" ", problems)}");
                    continue;
                }

                manager.AddRule(rule);
                created.Add(rule);
            }

            return created;
        }
    }
}
