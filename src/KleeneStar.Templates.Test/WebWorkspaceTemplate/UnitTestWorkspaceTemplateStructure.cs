using KleeneStar.Core.WebWorkspaceTemplate;
using KleeneStar.Model.Entities;
using KleeneStar.Templates.WebWorkspaceTemplate;

namespace KleeneStar.Templates.Test.WebWorkspaceTemplate
{
    /// <summary>
    /// Provides unit tests for the structure the shipped templates declare for their classes -
    /// fields, priorities, workflows, calendars and agreements.
    /// </summary>
    /// <remarks>
    /// The core writes what a template declares and checks little on the way: a transition
    /// naming a state that does not exist is dropped, an agreement naming an unknown calendar
    /// falls back to the default one, a pause state that is not declared never pauses anything,
    /// a scope naming a priority the class does not have covers nothing. All of that fails
    /// quietly in every workspace created from the template, so it is checked here, where the
    /// declaration is.
    /// </remarks>
    public class UnitTestWorkspaceTemplateStructure
    {
        /// <summary>
        /// The status categories every installation carries.
        /// </summary>
        private static readonly string[] Categories =
        [
            WorkspaceTemplateStatus.ToDo,
            WorkspaceTemplateStatus.InProgress,
            WorkspaceTemplateStatus.Waiting,
            WorkspaceTemplateStatus.Done
        ];

        /// <summary>
        /// Returns the English text a structure text was written as - the templates ship keys
        /// (<see cref="TemplateText"/>), and a test that asks for a state or a priority by name
        /// asks in the language the template was written in.
        /// </summary>
        /// <param name="text">A derived key, or a text that is none.</param>
        /// <returns>The English text.</returns>
        private static string En(string text)
        {
            if (string.IsNullOrEmpty(text) || !text.StartsWith(TemplateText.PluginId + ":", StringComparison.Ordinal))
            {
                return text;
            }

            return TemplateText.Texts.TryGetValue(TemplateResources.Unqualify(text), out var texts)
                ? texts.Single()
                : text;
        }

        /// <summary>
        /// Every class of every shipped template, with the template it belongs to.
        /// </summary>
        private static IEnumerable<(string Template, WorkspaceTemplateClass Class)> AllClasses =>
            TemplateCatalog.Templates.SelectMany(t => t.Classes.Select(c => (t.Key, c)));

        /// <summary>
        /// The classes whose objects are structured - everything not written as prose.
        /// </summary>
        private static IEnumerable<(string Template, WorkspaceTemplateClass Class)> StructuredClasses =>
            AllClasses.Where(x => !TemplateStructure.IsProse(x.Class.Kind, x.Class.Renderer));

        /// <summary>
        /// A prose class has no structure at all: its body is written in the editor, and a
        /// field, a form or a lifecycle would be something nobody ever sees.
        /// </summary>
        [Fact]
        public void ProseClassesCarryNoStructure()
        {
            foreach (var (template, @class) in AllClasses.Where(x => TemplateStructure.IsProse(x.Class.Kind, x.Class.Renderer)))
            {
                var name = $"{template}/{@class.Name}";

                Assert.True(@class.Fields.Count == 0, $"{name} declares fields.");
                Assert.True(@class.Priorities.Count == 0, $"{name} declares priorities.");
                Assert.True(@class.Workflow is null, $"{name} declares a workflow.");
                Assert.True(@class.Calendars.Count == 0, $"{name} declares calendars.");
                Assert.True(@class.Slas.Count == 0, $"{name} declares agreements.");
            }
        }

        /// <summary>
        /// Every structured class has fields and a lifecycle with a status field bound to it -
        /// the field is what makes the lifecycle visible on an object.
        /// </summary>
        [Fact]
        public void StructuredClassesHaveFieldsAndALifecycle()
        {
            foreach (var (template, @class) in StructuredClasses)
            {
                var name = $"{template}/{@class.Name}";

                Assert.True(@class.Fields.Count > 0, $"{name} declares no field.");
                Assert.True(@class.Workflow is { Statuses.Count: > 0 }, $"{name} declares no lifecycle.");
                Assert.True(@class.Fields.Count(x => x.Type == FieldType.Workflow) == 1, $"{name} needs exactly one status field.");
            }
        }

        /// <summary>
        /// A priority field exists exactly where there is a scale to choose from.
        /// </summary>
        [Fact]
        public void PriorityFieldFollowsTheScale()
        {
            foreach (var (template, @class) in AllClasses)
            {
                var hasField = @class.Fields.Any(x => x.Type == FieldType.Priority);

                Assert.True(hasField == @class.Priorities.Count > 0, $"{template}/{@class.Name}: priority field and scale disagree.");
            }
        }

        /// <summary>
        /// Issues, assets and form-rendered documents are ranked unless the template says the
        /// class is a record that is not - which it says with an empty scale, never by
        /// accident.
        /// </summary>
        [Fact]
        public void AssetsAndIssuesAreRankedByDefault()
        {
            var cmdb = TemplateCatalog.ByKey("kleenestar.templates.cmdb").Classes.Single(x => x.Name == "Asset");
            var spec = TemplateCatalog.ByKey("kleenestar.templates.development").Classes.Single(x => x.Name == "Specification");
            var incident = TemplateCatalog.ByKey("kleenestar.templates.servicedesk").Classes.Single(x => x.Name == "Incident");

            Assert.NotEmpty(cmdb.Priorities);
            Assert.NotEmpty(spec.Priorities);
            Assert.NotEmpty(incident.Priorities);
        }

        /// <summary>
        /// Names are unique within a class the way the managers compare them: fields,
        /// priorities, states and calendars.
        /// </summary>
        [Fact]
        public void NamesAreUniquePerClass()
        {
            foreach (var (template, @class) in AllClasses)
            {
                var name = $"{template}/{@class.Name}";

                AssertUnique(@class.Fields.Select(x => x.Name), $"{name} fields");
                AssertUnique(@class.Priorities.Select(x => x.Name), $"{name} priorities");
                AssertUnique((@class.Workflow?.Statuses ?? []).Select(x => x.Name), $"{name} states");
                AssertUnique(@class.Calendars.Select(x => x.Name), $"{name} calendars");
                AssertUnique(@class.Slas.Select(x => x.Name), $"{name} agreements");
            }
        }

        /// <summary>
        /// Every state has one of the installation's categories, every transition connects two
        /// declared states, every state can be reached from the start, and the workflow can end.
        /// </summary>
        [Fact]
        public void WorkflowsAreWellFormed()
        {
            foreach (var (template, @class) in AllClasses.Where(x => x.Class.Workflow is not null))
            {
                var name = $"{template}/{@class.Name}";
                var workflow = @class.Workflow;
                var states = workflow.Statuses.Select(x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

                Assert.All(workflow.Statuses, x => Assert.Contains(x.Category, Categories));
                Assert.All(workflow.Transitions, x =>
                {
                    Assert.True(states.Contains(x.From), $"{name}: transition '{x.Name}' leaves the undeclared state '{x.From}'.");
                    Assert.True(states.Contains(x.To), $"{name}: transition '{x.Name}' enters the undeclared state '{x.To}'.");
                });

                Assert.True(workflow.Statuses.Any(x => x.IsEnd), $"{name}: the workflow never ends.");

                // every state is reachable from the start state
                var reached = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { workflow.Statuses[0].Name };
                var grew = true;

                while (grew)
                {
                    grew = false;

                    foreach (var transition in workflow.Transitions.Where(x => reached.Contains(x.From)))
                    {
                        grew |= reached.Add(transition.To);
                    }
                }

                Assert.True(reached.SetEquals(states), $"{name}: unreachable states {string.Join(", ", states.Except(reached))}.");
            }
        }

        /// <summary>
        /// Every selecting field offers something to select.
        /// </summary>
        [Fact]
        public void SelectingFieldsOfferOptions()
        {
            var selecting = new[]
            {
                FieldType.Selection, FieldType.MultiSelection, FieldType.Radio, FieldType.Choice, FieldType.Tile
            };

            foreach (var (template, @class) in AllClasses)
            {
                foreach (var field in @class.Fields.Where(x => selecting.Contains(x.Type)))
                {
                    Assert.True(field.Options.Count >= 2, $"{template}/{@class.Name}.{field.Name} offers fewer than two options.");
                    AssertUnique(field.Options, $"{template}/{@class.Name}.{field.Name} options");
                }
            }
        }

        /// <summary>
        /// A portal-visible class asks customers something, so the self-service form the core
        /// derives from the portal fields is not empty.
        /// </summary>
        [Fact]
        public void PortalClassesAskCustomersSomething()
        {
            foreach (var (template, @class) in AllClasses.Where(x => x.Class.PortalVisible))
            {
                Assert.True(@class.Fields.Any(x => x.Portal), $"{template}/{@class.Name} is portal-visible and asks customers nothing.");
            }
        }

        /// <summary>
        /// Agreements only on the kinds that keep a service clock, each running in a calendar of
        /// its own class, pausing only in states the class has, scoped only to priorities the
        /// class has, and with at least one target.
        /// </summary>
        [Fact]
        public void AgreementsReferToTheirClass()
        {
            foreach (var (template, @class) in AllClasses)
            {
                var name = $"{template}/{@class.Name}";

                if (!TemplateStructure.HasServiceLevels(@class.Kind))
                {
                    Assert.True(@class.Slas.Count == 0 && @class.Calendars.Count == 0, $"{name}: a {@class.Kind} keeps no service clock.");
                    continue;
                }

                var calendars = @class.Calendars.Select(x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var states = (@class.Workflow?.Statuses ?? []).Select(x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var priorities = @class.Priorities.Select(x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

                Assert.True(@class.Slas.Count == 0 || calendars.Count > 0, $"{name}: agreements without a calendar.");
                Assert.True(@class.Calendars.Count(x => x.IsDefault) <= 1, $"{name}: more than one default calendar.");

                foreach (var sla in @class.Slas)
                {
                    Assert.True(sla.Targets.Count > 0, $"{name}/{sla.Name}: no target.");
                    Assert.All(sla.Targets, x => Assert.True(x.Value > 0, $"{name}/{sla.Name}: target '{x.Name}' is not positive."));
                    Assert.True(sla.Calendar is null || calendars.Contains(sla.Calendar), $"{name}/{sla.Name}: unknown calendar '{sla.Calendar}'.");
                    Assert.All(sla.PauseOn, x => Assert.True(states.Contains(x), $"{name}/{sla.Name}: pauses in the undeclared state '{x}'."));
                    Assert.All(sla.Scope.Where(x => x.Type == SlaScopeRuleType.Priority), x =>
                        Assert.True(priorities.Contains(x.Value), $"{name}/{sla.Name}: scoped to the unknown priority '{x.Value}'."));
                }
            }
        }

        /// <summary>
        /// An agreement that measures an approval does not stop its clock in the state the
        /// approval is given from - that is the very wait it measures.
        /// </summary>
        /// <remarks>
        /// The default pauses an agreement in every waiting state of its workflow, which is right
        /// for "waiting for the customer" and wrong for "waiting for the approver"; the approval
        /// and authorization states are waiting states too, so an agreement with an approval
        /// target has to name its pause states itself.
        /// </remarks>
        [Fact]
        public void ApprovalClocksDoNotPauseWhileApprovalIsAwaited()
        {
            foreach (var (template, @class) in AllClasses.Where(x => x.Class.Workflow is not null))
            {
                var awaiting = @class.Workflow.Transitions
                    .Where(x => En(x.Name).StartsWith("Approve", StringComparison.OrdinalIgnoreCase)
                        || En(x.Name).StartsWith("Authorize", StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.From)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                Assert.True(awaiting.Count > 0 || !@class.Slas.Any(x => x.Targets.Any(t => t.Kind == SlaTargetKind.Approval))
                    || @class.Workflow.Transitions.Count > 0, "unreachable");

                foreach (var sla in @class.Slas.Where(x => x.Targets.Any(t => t.Kind == SlaTargetKind.Approval)))
                {
                    Assert.All(sla.PauseOn, x => Assert.False(awaiting.Contains(x), $"{template}/{@class.Name}/{En(sla.Name)} pauses in '{En(x)}', where the approval it measures is awaited."));
                }
            }
        }

        /// <summary>
        /// Every weekday of a calendar is listed once, with a working day that has a length.
        /// </summary>
        [Fact]
        public void CalendarsAreWellFormed()
        {
            foreach (var (template, @class) in AllClasses)
            {
                foreach (var calendar in @class.Calendars)
                {
                    var name = $"{template}/{@class.Name}/{calendar.Name}";

                    Assert.NotEmpty(calendar.BusinessHours);
                    AssertUnique(calendar.BusinessHours.Select(x => x.Day.ToString()), $"{name} weekdays");
                    Assert.All(calendar.BusinessHours, x => Assert.NotEqual(x.Start, x.End));
                    Assert.False(string.IsNullOrWhiteSpace(calendar.TimeZone), $"{name} names no time zone.");
                }
            }
        }

        /// <summary>
        /// The service desk follows ITIL: incidents are prioritized from impact and urgency on the
        /// P1-P5 scale and kept on a clock per priority, the critical ones around the clock;
        /// problems become known errors; changes are typed and authorized.
        /// </summary>
        [Fact]
        public void ServiceDeskFollowsItil()
        {
            var classes = TemplateCatalog.ByKey("kleenestar.templates.servicedesk").Classes.ToDictionary(x => x.Name);

            var incident = classes["Incident"];

            Assert.Equal(["P1 - Critical", "P2 - High", "P3 - Moderate", "P4 - Low", "P5 - Planning"], incident.Priorities.Select(x => En(x.Name)));
            Assert.Contains(incident.Fields, x => En(x.Name) == "Impact" && x.Required);
            Assert.Contains(incident.Fields, x => En(x.Name) == "Urgency" && x.Required);
            Assert.Contains(incident.Fields, x => En(x.Name) == "Major Incident" && x.Type == FieldType.Boolean);
            Assert.Contains(incident.Workflow.Statuses, x => En(x.Name) == "On Hold" && x.Category == WorkspaceTemplateStatus.Waiting);

            var p1 = Assert.Single(incident.Slas, x => x.Scope.Any(s => En(s.Value) == "P1 - Critical"));

            Assert.Equal(TemplateStructure.AlwaysOnCalendar, En(p1.Calendar));
            Assert.Contains("On Hold", p1.PauseOn.Select(En));

            // every incident priority is covered by an agreement
            Assert.All(incident.Priorities, p => Assert.Contains(incident.Slas, s => s.Scope.Any(r => r.Value == p.Name)));

            Assert.Contains(classes["Problem"].Workflow.Statuses, x => En(x.Name) == "Known Error");
            Assert.Contains(classes["Problem"].Fields, x => En(x.Name) == "Root Cause" && !x.OnCreate);

            var changeType = Assert.Single(classes["Change"].Fields, x => En(x.Name) == "Change Type");

            Assert.Equal(["Standard", "Normal", "Emergency"], changeType.Options.Select(En));
            Assert.Contains(classes["Change"].Workflow.Statuses, x => En(x.Name) == "Authorization");

            Assert.Contains(classes["ServiceRequest"].Fields, x => En(x.Name) == "Catalog Item" && x.Portal);
        }

        /// <summary>
        /// A class that declares its own scale, lifecycle, calendars or agreements gets exactly
        /// those - the defaults of its kind fill only what was left open, and an empty list means
        /// none rather than "the default".
        /// </summary>
        [Fact]
        public void EmptyMeansNone()
        {
            var sprint = TemplateCatalog.ByKey("kleenestar.templates.development").Classes.Single(x => x.Name == "Sprint");

            Assert.Empty(sprint.Priorities);
            Assert.Empty(sprint.Calendars);
            Assert.Empty(sprint.Slas);
            Assert.DoesNotContain(sprint.Fields, x => x.Type == FieldType.Priority);
        }

        /// <summary>
        /// The templates ship keys, not English: every name and description of the structure is
        /// a derived key of this plugin, except the two field names that bind to a system
        /// attribute of the object and the option values that are codes.
        /// </summary>
        [Fact]
        public void StructureTextsAreKeys()
        {
            static bool Keyed(string text) => string.IsNullOrEmpty(text) || TemplateText.IsKey(text);

            foreach (var (template, @class) in AllClasses)
            {
                var name = $"{template}/{@class.Name}";

                Assert.All(@class.Fields.Where(x => x.Name is not ("Description" or "Summary")), x => Assert.True(Keyed(x.Name), $"{name}: field '{x.Name}' is no key."));
                Assert.All(@class.Fields, x => Assert.True(Keyed(x.Description) && Keyed(x.Tab), $"{name}: field '{x.Name}' has text that is no key."));
                Assert.All(@class.Priorities, x => Assert.True(Keyed(x.Name) && Keyed(x.Description), $"{name}: priority '{x.Name}' is no key."));
                Assert.All(@class.Workflow?.Statuses ?? [], x => Assert.True(Keyed(x.Name) && Keyed(x.Description), $"{name}: state '{x.Name}' is no key."));
                Assert.All(@class.Workflow?.Transitions ?? [], x => Assert.True(Keyed(x.Name), $"{name}: transition '{x.Name}' is no key."));
                Assert.All(@class.Calendars, x => Assert.True(Keyed(x.Name) && Keyed(x.Description) && x.Holidays.All(h => Keyed(h.Name)), $"{name}: calendar '{x.Name}' has text that is no key."));
                Assert.All(@class.Slas, x => Assert.True(Keyed(x.Name) && Keyed(x.Description) && x.Targets.All(t => Keyed(t.Name)) && x.Escalations.All(e => Keyed(e.Notify)), $"{name}: agreement '{x.Name}' has text that is no key."));
            }
        }

        /// <summary>
        /// Within a class, what the reader tells apart stays apart in every language: two fields,
        /// two states, two priorities, two calendars or two agreements never translate to the
        /// same name - the workflow field stores the state's name, and two states of one name
        /// could not be told apart again.
        /// </summary>
        [Theory]
        [InlineData("de")]
        [InlineData("en")]
        public void TranslatedNamesStayUniquePerClass(string culture)
        {
            var translations = TemplateResources.Read(culture);

            string T(string text) => text is not null && text.StartsWith(TemplateText.PluginId + ":", StringComparison.Ordinal)
                && translations.TryGetValue(TemplateResources.Unqualify(text), out var value)
                    ? value
                    : text;

            foreach (var (template, @class) in AllClasses)
            {
                var name = $"{template}/{@class.Name} ({culture})";

                AssertUnique(@class.Fields.Select(x => T(x.Name)), $"{name} fields");
                AssertUnique(@class.Priorities.Select(x => T(x.Name)), $"{name} priorities");
                AssertUnique((@class.Workflow?.Statuses ?? []).Select(x => T(x.Name)), $"{name} states");
                AssertUnique(@class.Calendars.Select(x => T(x.Name)), $"{name} calendars");
                AssertUnique(@class.Slas.Select(x => T(x.Name)), $"{name} agreements");

                foreach (var field in @class.Fields)
                {
                    AssertUnique(field.Options.Select(T), $"{name} options of {T(field.Name)}");
                }
            }
        }

        /// <summary>
        /// Asserts that the names are unique, ignoring case.
        /// </summary>
        private static void AssertUnique(IEnumerable<string> names, string what)
        {
            var duplicates = names
                .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            Assert.True(duplicates.Count == 0, $"{what}: duplicates {string.Join(", ", duplicates)}.");
        }
    }
}
