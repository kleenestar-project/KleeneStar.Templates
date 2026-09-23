using KleeneStar.Core.WebWorkspaceTemplate;
using KleeneStar.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KleeneStar.Templates.WebWorkspaceTemplate
{
    /// <summary>
    /// The vocabulary the templates of this plugin describe the structure of a class in, and the
    /// defaults each object type starts with.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A class is more than a name: an issue needs a lifecycle to move through, a scale to be
    /// ranked on, the handful of fields every piece of work has, a form to be filed and read
    /// through, and - where somebody is waiting - the hours and the promises its clock runs by.
    /// The core applies what a template declares and nothing else, so the defaults are this
    /// plugin's: <see cref="Resolve"/> fills whatever a template leaves open from the object
    /// type, and each template states only what is particular to its domain.
    /// </para>
    /// <para>
    /// The defaults follow the kind and the renderer, because that is what decides whether a
    /// class has structure at all. A prose class - a document or a post written in the editor -
    /// has no fields, so it gets none of it. A structured class gets a workflow, the base fields
    /// of its kind and, as issues, assets and form-rendered documents are ranked, a priority
    /// scale. Calendars and agreements belong to the kinds whose objects someone waits on -
    /// issues; an asset or a document keeps no service clock.
    /// </para>
    /// <para>
    /// The templates are written in English and shipped as internationalization keys: the last
    /// step of <see cref="Resolve"/> turns every name, description and option into the key
    /// derived from its text (<see cref="TemplateText"/>), and the core writes each in the
    /// language of whoever creates the workspace. The class names stay as they are.
    /// </para>
    /// </remarks>
    public static class TemplateStructure
    {
        /// <summary>
        /// The name of the field the workflow of a class is bound to.
        /// </summary>
        public const string StatusField = "Status";

        /// <summary>
        /// The name of the field the priority scale of a class is chosen on.
        /// </summary>
        public const string PriorityField = "Priority";

        /// <summary>
        /// The name of the business-hours calendar.
        /// </summary>
        public const string BusinessHoursCalendar = "Business hours";

        /// <summary>
        /// The name of the round-the-clock calendar.
        /// </summary>
        public const string AlwaysOnCalendar = "24/7";

        /// <summary>
        /// Declares a field.
        /// </summary>
        /// <param name="name">The field name.</param>
        /// <param name="type">The field type.</param>
        /// <param name="description">What the field records.</param>
        /// <param name="options">The choices of a selecting field.</param>
        /// <param name="details">Whether the field goes on the details tab rather than the
        /// general one.</param>
        /// <param name="required">Whether it has to be answered.</param>
        /// <param name="onCreate">Whether it is asked when an object is created.</param>
        /// <param name="portal">Whether customers are asked it in the portal.</param>
        /// <returns>The field.</returns>
        public static WorkspaceTemplateField Field
        (
            string name,
            FieldType type,
            string description,
            string[] options = null,
            bool details = false,
            bool required = false,
            bool onCreate = true,
            bool portal = false
        )
        {
            return new WorkspaceTemplateField
            {
                Name = name,
                Type = type,
                Description = description,
                Options = options ?? [],
                Tab = details ? WorkspaceTemplateField.DetailsTab : WorkspaceTemplateField.GeneralTab,
                Required = required,
                OnCreate = onCreate,
                Portal = portal
            };
        }

        /// <summary>
        /// Declares a priority.
        /// </summary>
        /// <param name="name">The priority name.</param>
        /// <param name="description">What it means.</param>
        /// <returns>The priority.</returns>
        public static WorkspaceTemplatePriority Priority(string name, string description)
        {
            return new WorkspaceTemplatePriority { Name = name, Description = description };
        }

        /// <summary>
        /// Declares a workflow state, with the icon of its category.
        /// </summary>
        /// <param name="name">The state name.</param>
        /// <param name="category">The status category.</param>
        /// <param name="description">What the state means.</param>
        /// <param name="end">Whether the workflow ends in it.</param>
        /// <returns>The state.</returns>
        public static WorkspaceTemplateStatus Status(string name, string category, string description, bool end = false)
        {
            return new WorkspaceTemplateStatus
            {
                Name = name,
                Category = category,
                Description = description,
                IsEnd = end,
                Icon = category switch
                {
                    WorkspaceTemplateStatus.InProgress => "/kleenestar/assets/icons/state-progress.svg",
                    WorkspaceTemplateStatus.Waiting => "/kleenestar/assets/icons/status-category-waiting.svg",
                    WorkspaceTemplateStatus.Done => end
                        ? "/kleenestar/assets/icons/state-closed.svg"
                        : "/kleenestar/assets/icons/state-resolved.svg",
                    _ => "/kleenestar/assets/icons/state-new.svg"
                }
            };
        }

        /// <summary>
        /// Declares a transition.
        /// </summary>
        /// <param name="name">What the button moving an object reads.</param>
        /// <param name="from">The state it leaves.</param>
        /// <param name="to">The state it enters.</param>
        /// <returns>The transition.</returns>
        public static WorkspaceTemplateTransition Transition(string name, string from, string to)
        {
            return new WorkspaceTemplateTransition { Name = name, From = from, To = to };
        }

        /// <summary>
        /// Declares a workflow.
        /// </summary>
        /// <param name="name">The workflow name.</param>
        /// <param name="description">What it describes.</param>
        /// <param name="statuses">The states, the first being the start.</param>
        /// <param name="transitions">The transitions.</param>
        /// <returns>The workflow.</returns>
        public static WorkspaceTemplateWorkflow Workflow
        (
            string name,
            string description,
            WorkspaceTemplateStatus[] statuses,
            WorkspaceTemplateTransition[] transitions
        )
        {
            return new WorkspaceTemplateWorkflow
            {
                Name = name,
                Description = description,
                Statuses = statuses,
                Transitions = transitions
            };
        }

        /// <summary>
        /// Declares a service-level agreement.
        /// </summary>
        /// <param name="name">The policy name.</param>
        /// <param name="description">What it promises.</param>
        /// <param name="priority">How pressing it is.</param>
        /// <param name="calendar">The calendar its clock runs in.</param>
        /// <param name="targets">Its time targets.</param>
        /// <param name="scope">Which objects it covers.</param>
        /// <param name="escalations">Its escalation levels.</param>
        /// <param name="pauseOn">The states its clock stops in; null for every waiting state
        /// of the class's workflow.</param>
        /// <returns>The agreement.</returns>
        public static WorkspaceTemplateSla Sla
        (
            string name,
            string description,
            SlaPriority priority,
            string calendar,
            WorkspaceTemplateSlaTarget[] targets,
            WorkspaceTemplateSlaScope[] scope = null,
            WorkspaceTemplateSlaEscalation[] escalations = null,
            string[] pauseOn = null
        )
        {
            return new WorkspaceTemplateSla
            {
                Name = name,
                Description = description,
                Priority = priority,
                Calendar = calendar,
                Targets = targets,
                Scope = scope ?? [],
                Escalations = escalations ?? [],
                PauseOn = pauseOn
            };
        }

        /// <summary>
        /// Declares a time target of an agreement.
        /// </summary>
        public static WorkspaceTemplateSlaTarget Target(string name, SlaTargetKind kind, int value, SlaTargetUnit unit)
        {
            return new WorkspaceTemplateSlaTarget(name, kind, value, unit);
        }

        /// <summary>
        /// Declares the scope rule that ties an agreement to one priority of the class.
        /// </summary>
        public static WorkspaceTemplateSlaScope ForPriority(string priority)
        {
            return new WorkspaceTemplateSlaScope(SlaScopeRuleType.Priority, priority);
        }

        /// <summary>
        /// Declares an escalation level of an agreement.
        /// </summary>
        public static WorkspaceTemplateSlaEscalation Escalate(int after, SlaTargetUnit unit, string notify)
        {
            return new WorkspaceTemplateSlaEscalation(after, unit, notify);
        }

        /// <summary>
        /// Gets the business-hours calendar: Monday to Friday, 08:00 to 18:00 in Central
        /// European time, observing the German nationwide public holidays.
        /// </summary>
        public static WorkspaceTemplateCalendar BusinessHours { get; } = new()
        {
            Name = BusinessHoursCalendar,
            Description = "Monday to Friday, 08:00 to 18:00 Central European time, without the German nationwide public holidays.",
            TimeZone = "Europe/Berlin",
            Region = "DE",
            IsDefault = true,
            BusinessHours =
            [
                .. new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday }
                    .Select(day => new WorkspaceTemplateBusinessHours(day, new TimeOnly(8, 0), new TimeOnly(18, 0)))
            ],
            Holidays =
            [
                new(new DateOnly(2026, 1, 1), "New Year's Day"),
                new(new DateOnly(2026, 4, 3), "Good Friday"),
                new(new DateOnly(2026, 4, 6), "Easter Monday"),
                new(new DateOnly(2026, 5, 1), "Labour Day"),
                new(new DateOnly(2026, 5, 14), "Ascension Day"),
                new(new DateOnly(2026, 5, 25), "Whit Monday"),
                new(new DateOnly(2026, 10, 3), "German Unity Day"),
                new(new DateOnly(2026, 12, 25), "Christmas Day"),
                new(new DateOnly(2026, 12, 26), "Boxing Day"),
                new(new DateOnly(2027, 1, 1), "New Year's Day"),
                new(new DateOnly(2027, 3, 26), "Good Friday"),
                new(new DateOnly(2027, 3, 29), "Easter Monday"),
                new(new DateOnly(2027, 5, 1), "Labour Day"),
                new(new DateOnly(2027, 5, 6), "Ascension Day"),
                new(new DateOnly(2027, 5, 17), "Whit Monday"),
                new(new DateOnly(2027, 10, 3), "German Unity Day"),
                new(new DateOnly(2027, 12, 25), "Christmas Day"),
                new(new DateOnly(2027, 12, 26), "Boxing Day")
            ]
        };

        /// <summary>
        /// Gets the round-the-clock calendar, for what may not wait for the next working day.
        /// </summary>
        public static WorkspaceTemplateCalendar AlwaysOn { get; } = new()
        {
            Name = AlwaysOnCalendar,
            Description = "Around the clock, every day of the year - for outages that cannot wait for the next working day.",
            TimeZone = "UTC",
            BusinessHours =
            [
                .. Enum.GetValues<DayOfWeek>()
                    .Select(day => new WorkspaceTemplateBusinessHours(day, new TimeOnly(0, 0), new TimeOnly(23, 59)))
            ]
        };

        /// <summary>
        /// Gets the general-purpose priority scale.
        /// </summary>
        public static IReadOnlyList<WorkspaceTemplatePriority> DefaultPriorities { get; } =
        [
            Priority("Critical", "Blocks the business or many people; everything else waits."),
            Priority("High", "Important and time-sensitive; handled before routine work."),
            Priority("Medium", "Normal importance; handled in the usual order."),
            Priority("Low", "Can wait; handled when capacity allows.")
        ];

        /// <summary>
        /// Gets the MoSCoW scale for what is decided rather than fixed - scope, requirements,
        /// features.
        /// </summary>
        public static IReadOnlyList<WorkspaceTemplatePriority> MoscowPriorities { get; } =
        [
            Priority("Must have", "Without it the result is not acceptable."),
            Priority("Should have", "Important, but the result works without it."),
            Priority("Could have", "Desirable if time and budget allow."),
            Priority("Won't have", "Agreed to be out of scope this time.")
        ];

        /// <summary>
        /// Returns the lifecycle a structured issue starts with.
        /// </summary>
        public static WorkspaceTemplateWorkflow IssueWorkflow() => Workflow
        (
            "Standard lifecycle",
            "Open, worked on, possibly waiting for someone else, and done.",
            [
                Status("Open", WorkspaceTemplateStatus.ToDo, "Recorded, not yet started."),
                Status("In Progress", WorkspaceTemplateStatus.InProgress, "Somebody is working on it."),
                Status("Waiting", WorkspaceTemplateStatus.Waiting, "Blocked on somebody outside the team."),
                Status("Done", WorkspaceTemplateStatus.Done, "The work is complete.", end: true),
                Status("Cancelled", WorkspaceTemplateStatus.Done, "Will not be done.", end: true)
            ],
            [
                Transition("Start", "Open", "In Progress"),
                Transition("Wait", "In Progress", "Waiting"),
                Transition("Resume", "Waiting", "In Progress"),
                Transition("Complete", "In Progress", "Done"),
                Transition("Reopen", "Done", "Open"),
                Transition("Cancel", "Open", "Cancelled"),
                Transition("Cancel", "In Progress", "Cancelled")
            ]
        );

        /// <summary>
        /// Returns the lifecycle an asset starts with, from planning to retirement.
        /// </summary>
        public static WorkspaceTemplateWorkflow AssetWorkflow() => Workflow
        (
            "Asset lifecycle",
            "From the plan to acquire an asset to its disposal.",
            [
                Status("Planned", WorkspaceTemplateStatus.ToDo, "Planned, not yet ordered."),
                Status("Ordered", WorkspaceTemplateStatus.ToDo, "Ordered, not yet received."),
                Status("In Stock", WorkspaceTemplateStatus.InProgress, "Received and available, not yet deployed."),
                Status("In Use", WorkspaceTemplateStatus.InProgress, "Deployed and in operation."),
                Status("In Maintenance", WorkspaceTemplateStatus.Waiting, "Temporarily out of operation for repair or service."),
                Status("Retired", WorkspaceTemplateStatus.Done, "Taken out of operation and disposed of.", end: true)
            ],
            [
                Transition("Order", "Planned", "Ordered"),
                Transition("Receive", "Ordered", "In Stock"),
                Transition("Deploy", "In Stock", "In Use"),
                Transition("Send to maintenance", "In Use", "In Maintenance"),
                Transition("Return to service", "In Maintenance", "In Use"),
                Transition("Return to stock", "In Use", "In Stock"),
                Transition("Retire", "In Use", "Retired"),
                Transition("Retire", "In Stock", "Retired"),
                Transition("Retire", "In Maintenance", "Retired")
            ]
        );

        /// <summary>
        /// Returns the review cycle of a document filled in as a form.
        /// </summary>
        public static WorkspaceTemplateWorkflow ReviewWorkflow() => Workflow
        (
            "Review cycle",
            "Written, reviewed, approved - and eventually superseded.",
            [
                Status("Draft", WorkspaceTemplateStatus.ToDo, "Being written."),
                Status("In Review", WorkspaceTemplateStatus.InProgress, "Submitted to the reviewers."),
                Status("Approved", WorkspaceTemplateStatus.Done, "Reviewed and approved; this is the valid version."),
                Status("Obsolete", WorkspaceTemplateStatus.Done, "Superseded or withdrawn.", end: true)
            ],
            [
                Transition("Submit for review", "Draft", "In Review"),
                Transition("Request changes", "In Review", "Draft"),
                Transition("Approve", "In Review", "Approved"),
                Transition("Revise", "Approved", "Draft"),
                Transition("Withdraw", "Approved", "Obsolete")
            ]
        );

        /// <summary>
        /// Returns a two-state lifecycle for master data - a record that is either in use or
        /// no longer.
        /// </summary>
        /// <param name="active">The name of the state in use.</param>
        /// <param name="inactive">The name of the state no longer in use.</param>
        public static WorkspaceTemplateWorkflow RecordWorkflow(string active = "Active", string inactive = "Inactive") => Workflow
        (
            "Record lifecycle",
            "A record that is either in use or no longer.",
            [
                Status(active, WorkspaceTemplateStatus.InProgress, "The record is current and in use."),
                Status(inactive, WorkspaceTemplateStatus.Done, "No longer in use; kept for reference.", end: true)
            ],
            [
                Transition("Deactivate", active, inactive),
                Transition("Reactivate", inactive, active)
            ]
        );

        /// <summary>
        /// Returns the fields every structured class of a kind starts with.
        /// </summary>
        /// <param name="kind">The object kind.</param>
        /// <returns>The base fields, before the priority and status fields are decided.</returns>
        private static IReadOnlyList<WorkspaceTemplateField> BaseFields(string kind)
        {
            if (string.Equals(kind, ObjectKind.Asset, StringComparison.OrdinalIgnoreCase))
            {
                return
                [
                    Field("Description", FieldType.Text, "What the asset is and what it is used for."),
                    Field(StatusField, FieldType.Workflow, "Where the asset is in its lifecycle."),
                    Field(PriorityField, FieldType.Priority, "How critical the asset is for the business."),
                    Field("Owner", FieldType.User, "Who is accountable for the asset."),
                    Field("Location", FieldType.Text, "Where the asset is, physically or logically."),
                    Field("Labels", FieldType.Tag, "Keywords for filtering and grouping.", details: true)
                ];
            }

            if (string.Equals(kind, ObjectKind.Document, StringComparison.OrdinalIgnoreCase)
                || string.Equals(kind, ObjectKind.Blog, StringComparison.OrdinalIgnoreCase))
            {
                return
                [
                    Field(StatusField, FieldType.Workflow, "Where the document is in its review cycle."),
                    Field(PriorityField, FieldType.Priority, "How important the document is."),
                    Field("Owner", FieldType.User, "Who keeps the document current."),
                    Field("Version", FieldType.Text, "The version of the document, e.g. 1.2."),
                    Field("Next Review", FieldType.Date, "When the document is due to be reviewed again.", details: true),
                    Field("Labels", FieldType.Tag, "Keywords for filtering and grouping.", details: true)
                ];
            }

            return
            [
                Field("Description", FieldType.Text, "What this is about, in as much detail as needed.", portal: true),
                Field(StatusField, FieldType.Workflow, "Where the item is in its lifecycle."),
                Field(PriorityField, FieldType.Priority, "How urgent the item is."),
                Field("Assignee", FieldType.User, "Who is working on it.", onCreate: false),
                Field("Due Date", FieldType.Date, "When it has to be done.", details: true),
                Field("Labels", FieldType.Tag, "Keywords for filtering and grouping.", details: true)
            ];
        }

        /// <summary>
        /// Returns whether a class of the kind and renderer is written as prose, and therefore
        /// has no structure.
        /// </summary>
        /// <param name="kind">The object kind.</param>
        /// <param name="renderer">The renderer the template names, or null to follow the kind.</param>
        public static bool IsProse(string kind, string renderer)
        {
            if (!string.IsNullOrWhiteSpace(renderer))
            {
                return string.Equals(renderer, ObjectRenderer.Prose, StringComparison.OrdinalIgnoreCase);
            }

            return string.Equals(kind, ObjectKind.Document, StringComparison.OrdinalIgnoreCase)
                || string.Equals(kind, ObjectKind.Blog, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns whether objects of the kind are kept on a service clock - the kinds that have
        /// calendars and agreements.
        /// </summary>
        /// <param name="kind">The object kind.</param>
        public static bool HasServiceLevels(string kind)
        {
            return !string.Equals(kind, ObjectKind.Document, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(kind, ObjectKind.Blog, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(kind, ObjectKind.Asset, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Completes a class declaration with the defaults of its kind.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Every part a template leaves as <see langword="null"/> is taken from the kind; an
        /// empty collection is an answer ("this class has none") and is kept. The fields are
        /// merged rather than replaced: the base fields of the kind come first, a declared field
        /// of the same name takes the base field's place, and the rest follow in the order
        /// declared.
        /// </para>
        /// <para>
        /// The status field exists exactly when there is a workflow, and the priority field
        /// exactly when there is a scale - a field offering nothing to choose would only be a
        /// question nobody can answer. The default agreement pauses in every waiting state of
        /// the class's own workflow, whatever the template named them.
        /// </para>
        /// </remarks>
        public static WorkspaceTemplateClass Resolve
        (
            WorkspaceTemplateClass declared,
            IReadOnlyList<WorkspaceTemplateField> fields,
            IReadOnlyList<WorkspaceTemplatePriority> priorities,
            WorkspaceTemplateWorkflow workflow,
            IReadOnlyList<WorkspaceTemplateCalendar> calendars,
            IReadOnlyList<WorkspaceTemplateSla> slas
        )
        {
            var prose = IsProse(declared.Kind, declared.Renderer);
            var asset = string.Equals(declared.Kind, ObjectKind.Asset, StringComparison.OrdinalIgnoreCase);
            var document = !asset && !HasServiceLevels(declared.Kind);
            var serviced = HasServiceLevels(declared.Kind);

            var effectiveWorkflow = workflow
                ?? (prose ? null : asset ? AssetWorkflow() : document ? ReviewWorkflow() : IssueWorkflow());

            var effectivePriorities = priorities ?? (prose ? [] : DefaultPriorities);

            var effectiveCalendars = calendars ?? (prose || !serviced ? [] : [BusinessHours]);

            var waiting = (effectiveWorkflow?.Statuses ?? [])
                .Where(x => x.Category == WorkspaceTemplateStatus.Waiting)
                .Select(x => x.Name)
                .ToArray();

            var effectiveSlas = (slas ?? (prose || !serviced || effectiveCalendars.Count == 0
                ? []
                :
                [
                    Sla
                    (
                        "Standard",
                        "Everything of this class is answered within a working day and completed within a working week.",
                        SlaPriority.Medium,
                        BusinessHoursCalendar,
                        [
                            Target("First response", SlaTargetKind.Response, 1, SlaTargetUnit.BusinessDays),
                            Target("Resolution", SlaTargetKind.Resolution, 5, SlaTargetUnit.BusinessDays)
                        ]
                    )
                ]))
                .Select(x => x.PauseOn is null
                    ? new WorkspaceTemplateSla
                    {
                        Name = x.Name,
                        Description = x.Description,
                        Priority = x.Priority,
                        Calendar = x.Calendar,
                        Notifications = x.Notifications,
                        Targets = x.Targets,
                        Scope = x.Scope,
                        Escalations = x.Escalations,
                        PauseOn = waiting
                    }
                    : x)
                .ToList();

            var effectiveFields = prose && fields is null
                ? []
                : Merge(prose ? [] : BaseFields(declared.Kind), fields ?? [])
                    .Where(x => x.Type != FieldType.Workflow || effectiveWorkflow is not null)
                    .Where(x => x.Type != FieldType.Priority || effectivePriorities.Count > 0)
                    .ToList();

            return Localize(new WorkspaceTemplateClass
            {
                Name = declared.Name,
                Description = declared.Description,
                Icon = declared.Icon,
                Kind = declared.Kind,
                Renderer = declared.Renderer,
                PortalVisible = declared.PortalVisible,
                Sealed = declared.Sealed,
                AccessModifier = declared.AccessModifier,
                Fields = effectiveFields,
                Priorities = effectivePriorities,
                Workflow = effectiveWorkflow,
                Calendars = effectiveCalendars,
                Slas = effectiveSlas
            });
        }

        /// <summary>
        /// Turns every text of a class's structure into its internationalization key
        /// (<see cref="TemplateText.Key"/>), so the core writes it in the language of whoever
        /// creates the workspace.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Names are translated as well as descriptions. A state, a priority or a field name is
        /// data once it is written - a workflow field stores the state's name - but it is
        /// written once, at creation, and never re-read as a key, so a German workspace gets
        /// German states and keeps them whatever language its readers use later. References
        /// between the parts (a transition's ends, an agreement's calendar, pause states and
        /// priority scope) are keyed the same way as what they refer to, so they still match;
        /// the core matches them before it translates.
        /// </para>
        /// <para>
        /// Two things stay as they are. The class name, because a class name is an identifier
        /// routes and portal request-type keys are built from, and a field named after a system
        /// attribute of the object (<c>Description</c>, <c>Summary</c>), because that name is
        /// what binds its answer to the object itself rather than to a value row. Option values
        /// that are codes - all capitals, digits, a currency - are not language and stay too.
        /// </para>
        /// </remarks>
        /// <param name="source">The class with English texts.</param>
        /// <returns>The class with keys.</returns>
        private static WorkspaceTemplateClass Localize(WorkspaceTemplateClass source)
        {
            static string K(string text) => TemplateText.Key(text);

            return new WorkspaceTemplateClass
            {
                Name = source.Name,
                Description = source.Description,
                Icon = source.Icon,
                Kind = source.Kind,
                Renderer = source.Renderer,
                PortalVisible = source.PortalVisible,
                Sealed = source.Sealed,
                AccessModifier = source.AccessModifier,
                Fields =
                [
                    .. source.Fields.Select(x => new WorkspaceTemplateField
                    {
                        Name = SystemAttributes.Contains(x.Name) ? x.Name : K(x.Name),
                        Description = K(x.Description),
                        Icon = x.Icon,
                        Type = x.Type,
                        Options = [.. x.Options.Select(o => IsCode(o) ? o : K(o))],
                        Required = x.Required,
                        Tab = K(x.Tab),
                        OnCreate = x.OnCreate,
                        Portal = x.Portal
                    })
                ],
                Priorities =
                [
                    .. source.Priorities.Select(x => new WorkspaceTemplatePriority
                    {
                        Name = K(x.Name),
                        Description = K(x.Description),
                        Icon = x.Icon
                    })
                ],
                Workflow = source.Workflow is null
                    ? null
                    : new WorkspaceTemplateWorkflow
                    {
                        Name = K(source.Workflow.Name),
                        Description = K(source.Workflow.Description),
                        Statuses =
                        [
                            .. source.Workflow.Statuses.Select(x => new WorkspaceTemplateStatus
                            {
                                Name = K(x.Name),
                                Description = K(x.Description),
                                Icon = x.Icon,
                                Category = x.Category,
                                IsEnd = x.IsEnd
                            })
                        ],
                        Transitions =
                        [
                            .. source.Workflow.Transitions.Select(x => new WorkspaceTemplateTransition
                            {
                                Name = K(x.Name),
                                Description = K(x.Description),
                                From = K(x.From),
                                To = K(x.To)
                            })
                        ]
                    },
                Calendars =
                [
                    .. source.Calendars.Select(x => new WorkspaceTemplateCalendar
                    {
                        Name = K(x.Name),
                        Description = K(x.Description),
                        TimeZone = x.TimeZone,
                        Region = x.Region,
                        IsDefault = x.IsDefault,
                        BusinessHours = x.BusinessHours,
                        Holidays = [.. x.Holidays.Select(h => new WorkspaceTemplateHoliday(h.Date, K(h.Name)))]
                    })
                ],
                Slas =
                [
                    .. source.Slas.Select(x => new WorkspaceTemplateSla
                    {
                        Name = K(x.Name),
                        Description = K(x.Description),
                        Priority = x.Priority,
                        Calendar = K(x.Calendar),
                        PauseOn = [.. x.PauseOn.Select(K)],
                        Notifications = x.Notifications,
                        Targets = [.. x.Targets.Select(t => t with { Name = K(t.Name) })],
                        Scope = [.. x.Scope.Select(r => r with { Value = IsCode(r.Value) ? r.Value : K(r.Value) })],
                        Escalations = [.. x.Escalations.Select(e => e with { Notify = K(e.Notify) })]
                    })
                ]
            };
        }

        /// <summary>
        /// The field names that bind to a system attribute of the object and therefore keep
        /// their name in every language.
        /// </summary>
        private static readonly HashSet<string> SystemAttributes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Description",
            "Summary"
        };

        /// <summary>
        /// Returns whether an option value is a code rather than a word - a currency, a version,
        /// a standard's number - which reads the same in every language.
        /// </summary>
        /// <param name="value">The option value.</param>
        /// <returns>True for a code.</returns>
        private static bool IsCode(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                || !value.Any(char.IsLower)
                || value.StartsWith("ISO", StringComparison.Ordinal);
        }

        /// <summary>
        /// Merges declared fields into the base fields: same name replaces in place, the rest
        /// is appended in declaration order.
        /// </summary>
        private static IEnumerable<WorkspaceTemplateField> Merge(IReadOnlyList<WorkspaceTemplateField> baseFields, IReadOnlyList<WorkspaceTemplateField> declared)
        {
            var byName = declared
                .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var field in baseFields)
            {
                used.Add(field.Name);

                yield return byName.TryGetValue(field.Name, out var replacement) ? replacement : field;
            }

            foreach (var field in declared)
            {
                if (used.Add(field.Name))
                {
                    yield return field;
                }
            }
        }
    }
}
