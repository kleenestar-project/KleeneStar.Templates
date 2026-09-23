using KleeneStar.Core.WebWorkspaceTemplate;
using KleeneStar.Model.Entities;
using System.Collections.Generic;
using static KleeneStar.Templates.WebWorkspaceTemplate.SharedStructures;
using static KleeneStar.Templates.WebWorkspaceTemplate.TemplateStructure;

namespace KleeneStar.Templates.WebWorkspaceTemplate
{
    /// <summary>
    /// A workspace for running a service desk: what users report, what it turns out to be, and
    /// what is done about it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Three of its classes are marked portal-visible, because a service desk is the one
    /// workspace whose work is started by the people outside it: a ticket, an incident and a
    /// service request are the request types customers file, and the rest of the workspace is
    /// what the team does with them.
    /// </para>
    /// <para>
    /// The structure follows the ITIL 4 practices the classes stand for. An incident is
    /// prioritized from impact and urgency on the five-step P1-P5 scale, has a major-incident
    /// flag and a resolution code, pauses its clock while on hold, and runs on the
    /// round-the-clock calendar for P1 and P2. A service request is fulfilled from a catalog
    /// item, optionally after an approval. A problem is investigated to a root cause and kept
    /// as a known error with a workaround until a change removes it. A change is typed standard,
    /// normal or emergency, is assessed, authorized, scheduled, implemented and reviewed. The
    /// ticket is the service desk's single point of contact - an interaction not yet classified.
    /// </para>
    /// </remarks>
    public sealed class ServiceDeskTemplate : WorkspaceTemplateBase
    {
        /// <summary>
        /// Gets the short name of the template, <c>servicedesk</c>, from which its key and its i18n
        /// keys are derived.
        /// </summary>
        protected override string Slug => "servicedesk";

        /// <summary>
        /// Gets the file name of the icon, <c>sd</c> - the icon of the seeded service desk workspace.
        /// </summary>
        protected override string IconName => "sd";

        /// <summary>
        /// Gets the key proposed for a new service desk workspace, <c>SD</c>.
        /// </summary>
        public override string SuggestedKey => "SD";

        /// <summary>
        /// Gets the display order: first, because a service desk is the most common reason to set a
        /// workspace up.
        /// </summary>
        public override int Order => 10;

        /// <summary>
        /// Gets the category the workspace is filed under, support.
        /// </summary>
        public override IEnumerable<string> Categories => ["Support"];

        /// <summary>
        /// The support groups work is assigned to - the tiers of a service desk.
        /// </summary>
        private static readonly string[] AssignmentGroups =
        [
            "Service Desk (1st level)",
            "Application Support (2nd level)",
            "Infrastructure (2nd level)",
            "Network (2nd level)",
            "Vendor (3rd level)"
        ];

        /// <summary>
        /// The categories a reported issue is sorted into.
        /// </summary>
        private static readonly string[] IssueCategories =
        [
            "Hardware", "Software", "Network", "Access & Identity", "Email & Collaboration", "Printing", "Other"
        ];

        /// <summary>
        /// The channels a contact arrives through.
        /// </summary>
        private static readonly string[] Channels = ["Portal", "Email", "Phone", "Chat", "Walk-in", "Monitoring"];

        /// <summary>
        /// The ITIL priority scale, derived from impact and urgency.
        /// </summary>
        private static IReadOnlyList<WorkspaceTemplatePriority> ItilPriorities() =>
        [
            Priority("P1 - Critical", "Extensive impact and critical urgency: a business-critical service is down."),
            Priority("P2 - High", "Significant impact or high urgency: a service is severely degraded."),
            Priority("P3 - Moderate", "Moderate impact: a service is degraded for a group of users."),
            Priority("P4 - Low", "Minor impact: a single user is affected and a workaround exists."),
            Priority("P5 - Planning", "No current impact; handled as planned work.")
        ];

        /// <summary>
        /// Gets the classes the workspace starts with, one per ITIL practice it runs - ticket,
        /// incident, service request, problem and change - plus a knowledge base and an announcement
        /// channel, each with its structure.
        /// </summary>
        public override IEnumerable<WorkspaceTemplateClass> Classes =>
        [
            Class("Ticket", "ticket", portalVisible: true,
                fields:
                [
                    Field("Description", FieldType.Text, "What the contact is about, in the words of whoever raised it.", required: true, portal: true),
                    Field("Category", FieldType.Selection, "What area the contact concerns.", IssueCategories, portal: true),
                    Field("Channel", FieldType.Tile, "How the contact reached the service desk.", Channels),
                    Field("Assignment Group", FieldType.Selection, "The support group handling it.", AssignmentGroups, onCreate: false),
                    Field("Classification", FieldType.Radio, "What the contact turned out to be once triaged.", ["Incident", "Service request", "Question", "Complaint", "Compliment"], onCreate: false)
                ],
                workflow: Workflow
                (
                    "Service desk intake",
                    "The single point of contact: a contact is triaged, answered or turned into an incident or request.",
                    [
                        Status("New", WorkspaceTemplateStatus.ToDo, "Received, not yet looked at."),
                        Status("Triage", WorkspaceTemplateStatus.InProgress, "The service desk is classifying and answering it."),
                        Status("Waiting for Customer", WorkspaceTemplateStatus.Waiting, "The service desk asked the requester for information."),
                        Status("Answered", WorkspaceTemplateStatus.Done, "Answered or handed over as an incident or request."),
                        Status("Closed", WorkspaceTemplateStatus.Done, "Nothing more to do; kept for reference.", end: true)
                    ],
                    [
                        Transition("Triage", "New", "Triage"),
                        Transition("Ask customer", "Triage", "Waiting for Customer"),
                        Transition("Customer replied", "Waiting for Customer", "Triage"),
                        Transition("Answer", "Triage", "Answered"),
                        Transition("Reopen", "Answered", "Triage"),
                        Transition("Close", "Answered", "Closed")
                    ]
                ),
                calendars: [BusinessHours],
                slas:
                [
                    Sla
                    (
                        "Service desk contact",
                        "Every contact is acknowledged within four working hours and answered or handed over within two working days.",
                        SlaPriority.Medium,
                        BusinessHoursCalendar,
                        [
                            Target("First response", SlaTargetKind.Response, 4, SlaTargetUnit.Hours),
                            Target("Resolution", SlaTargetKind.Resolution, 2, SlaTargetUnit.BusinessDays)
                        ]
                    )
                ]),

            Class("Incident", "incident", portalVisible: true,
                fields:
                [
                    Field("Description", FieldType.Text, "What is not working, since when, and what was tried.", required: true, portal: true),
                    Field("Impact", FieldType.Selection, "How widely the business is affected.", ImpactScale, required: true),
                    Field("Urgency", FieldType.Selection, "How quickly the business needs it restored.", UrgencyScale, required: true, portal: true),
                    Field("Category", FieldType.Selection, "What area the incident concerns.", IssueCategories, portal: true),
                    Field("Affected Service", FieldType.Text, "The business service that is disrupted.", portal: true),
                    Field("Configuration Item", FieldType.Reference, "The configuration item that failed, if known."),
                    Field("Channel", FieldType.Tile, "How the incident was reported.", Channels),
                    Field("Assignment Group", FieldType.Selection, "The support group resolving it.", AssignmentGroups),
                    Field("Major Incident", FieldType.Boolean, "Whether the incident is handled by the major incident procedure.", details: true),
                    Field("Related Problem", FieldType.Reference, "The problem the incident is linked to.", details: true, onCreate: false),
                    Field("Resolution Code", FieldType.Selection, "How the incident was resolved.", ["Solved (permanently)", "Solved (workaround)", "Not reproducible", "Resolved by caller", "Duplicate", "Out of scope"], details: true, onCreate: false),
                    Field("Resolution Notes", FieldType.Multiline, "What restored the service.", details: true, onCreate: false),
                    Field("Satisfaction", FieldType.Rating, "How satisfied the caller was with the resolution.", details: true, onCreate: false)
                ],
                priorities: ItilPriorities(),
                workflow: Workflow
                (
                    "Incident management",
                    "The ITIL incident lifecycle: restore normal service operation as quickly as possible.",
                    [
                        Status("New", WorkspaceTemplateStatus.ToDo, "Logged, not yet assigned."),
                        Status("Assigned", WorkspaceTemplateStatus.ToDo, "Assigned to a support group, not yet worked on."),
                        Status("In Progress", WorkspaceTemplateStatus.InProgress, "Being diagnosed and resolved."),
                        Status("On Hold", WorkspaceTemplateStatus.Waiting, "Waiting for the caller, a vendor or a change; the clock is paused."),
                        Status("Resolved", WorkspaceTemplateStatus.Done, "Service restored; waiting for the caller's confirmation."),
                        Status("Closed", WorkspaceTemplateStatus.Done, "Confirmed and closed.", end: true),
                        Status("Cancelled", WorkspaceTemplateStatus.Done, "Logged in error or a duplicate.", end: true)
                    ],
                    [
                        Transition("Assign", "New", "Assigned"),
                        Transition("Start work", "Assigned", "In Progress"),
                        Transition("Start work", "New", "In Progress"),
                        Transition("Put on hold", "In Progress", "On Hold"),
                        Transition("Resume", "On Hold", "In Progress"),
                        Transition("Resolve", "In Progress", "Resolved"),
                        Transition("Reopen", "Resolved", "In Progress"),
                        Transition("Close", "Resolved", "Closed"),
                        Transition("Cancel", "New", "Cancelled"),
                        Transition("Cancel", "Assigned", "Cancelled")
                    ]
                ),
                calendars: [BusinessHours, AlwaysOn],
                slas:
                [
                    Sla
                    (
                        "Incident P1 - Critical",
                        "A critical incident is responded to within 15 minutes and resolved within 4 hours, around the clock.",
                        SlaPriority.Critical,
                        AlwaysOnCalendar,
                        [
                            Target("First response", SlaTargetKind.Response, 15, SlaTargetUnit.Minutes),
                            Target("Status update", SlaTargetKind.Update, 1, SlaTargetUnit.Hours),
                            Target("Resolution", SlaTargetKind.Resolution, 4, SlaTargetUnit.Hours)
                        ],
                        [ForPriority("P1 - Critical")],
                        [
                            Escalate(15, SlaTargetUnit.Minutes, "Service Desk Lead"),
                            Escalate(30, SlaTargetUnit.Minutes, "Major Incident Manager"),
                            Escalate(60, SlaTargetUnit.Minutes, "Head of IT Operations")
                        ]
                    ),
                    Sla
                    (
                        "Incident P2 - High",
                        "A high-priority incident is responded to within 30 minutes and resolved within 8 hours, around the clock.",
                        SlaPriority.High,
                        AlwaysOnCalendar,
                        [
                            Target("First response", SlaTargetKind.Response, 30, SlaTargetUnit.Minutes),
                            Target("Status update", SlaTargetKind.Update, 2, SlaTargetUnit.Hours),
                            Target("Resolution", SlaTargetKind.Resolution, 8, SlaTargetUnit.Hours)
                        ],
                        [ForPriority("P2 - High")],
                        [Escalate(1, SlaTargetUnit.Hours, "Service Desk Lead"), Escalate(4, SlaTargetUnit.Hours, "Head of IT Operations")]
                    ),
                    Sla
                    (
                        "Incident P3 - Moderate",
                        "A moderate incident is responded to within 4 working hours and resolved within 3 working days.",
                        SlaPriority.Medium,
                        BusinessHoursCalendar,
                        [
                            Target("First response", SlaTargetKind.Response, 4, SlaTargetUnit.Hours),
                            Target("Resolution", SlaTargetKind.Resolution, 3, SlaTargetUnit.BusinessDays)
                        ],
                        [ForPriority("P3 - Moderate")]
                    ),
                    Sla
                    (
                        "Incident P4/P5 - Low",
                        "A low-priority incident is responded to within one working day and resolved within 5 working days.",
                        SlaPriority.Low,
                        BusinessHoursCalendar,
                        [
                            Target("First response", SlaTargetKind.Response, 1, SlaTargetUnit.BusinessDays),
                            Target("Resolution", SlaTargetKind.Resolution, 5, SlaTargetUnit.BusinessDays)
                        ],
                        [ForPriority("P4 - Low"), ForPriority("P5 - Planning")]
                    )
                ]),

            Class("ServiceRequest", "servicerequest", portalVisible: true,
                fields:
                [
                    Field("Description", FieldType.Text, "What is requested and what for.", portal: true),
                    Field("Catalog Item", FieldType.Selection, "The standard service requested from the service catalog.",
                        ["Access request", "Password reset", "Software installation", "Hardware request", "New user account", "Mailbox or distribution list", "Information request", "Other"],
                        required: true, portal: true),
                    Field("Requested For", FieldType.User, "Who the service is for, if not the requester.", portal: true),
                    Field("Needed By", FieldType.Date, "When the service is needed.", portal: true),
                    Field("Quantity", FieldType.Number, "How many are requested.", portal: true),
                    Field("Approval Required", FieldType.Boolean, "Whether the request has to be approved before fulfilment."),
                    Field("Approver", FieldType.User, "Who approves the request.", onCreate: false),
                    Field("Cost Center", FieldType.Text, "Where the cost of the service is booked.", details: true),
                    Field("Fulfilment Group", FieldType.Selection, "The group fulfilling the request.", AssignmentGroups, onCreate: false),
                    Field("Fulfilment Notes", FieldType.Multiline, "What was done to fulfil the request.", details: true, onCreate: false)
                ],
                priorities:
                [
                    Priority("High", "Needed to be able to work; fulfilled first."),
                    Priority("Normal", "Fulfilled in the usual order."),
                    Priority("Low", "Nice to have; fulfilled when capacity allows.")
                ],
                workflow: Workflow
                (
                    "Request fulfilment",
                    "The ITIL request lifecycle: approved where needed, then fulfilled from the catalog.",
                    [
                        Status("Submitted", WorkspaceTemplateStatus.ToDo, "Submitted, not yet reviewed."),
                        Status("Awaiting Approval", WorkspaceTemplateStatus.Waiting, "Waiting for the approver."),
                        Status("Approved", WorkspaceTemplateStatus.ToDo, "Approved and queued for fulfilment."),
                        Status("Fulfilment", WorkspaceTemplateStatus.InProgress, "Being fulfilled."),
                        Status("Pending Customer", WorkspaceTemplateStatus.Waiting, "Waiting for the requester."),
                        Status("Fulfilled", WorkspaceTemplateStatus.Done, "Delivered to the requester."),
                        Status("Closed", WorkspaceTemplateStatus.Done, "Confirmed and closed.", end: true),
                        Status("Rejected", WorkspaceTemplateStatus.Done, "Not approved.", end: true)
                    ],
                    [
                        Transition("Request approval", "Submitted", "Awaiting Approval"),
                        Transition("Approve", "Awaiting Approval", "Approved"),
                        Transition("Reject", "Awaiting Approval", "Rejected"),
                        Transition("Start fulfilment", "Submitted", "Fulfilment"),
                        Transition("Start fulfilment", "Approved", "Fulfilment"),
                        Transition("Ask customer", "Fulfilment", "Pending Customer"),
                        Transition("Customer replied", "Pending Customer", "Fulfilment"),
                        Transition("Fulfil", "Fulfilment", "Fulfilled"),
                        Transition("Reopen", "Fulfilled", "Fulfilment"),
                        Transition("Close", "Fulfilled", "Closed")
                    ]
                ),
                calendars: [BusinessHours],
                slas:
                [
                    Sla
                    (
                        "Standard service request",
                        "A request is acknowledged within one working day, approved within two and fulfilled within five.",
                        SlaPriority.Medium,
                        BusinessHoursCalendar,
                        [
                            Target("First response", SlaTargetKind.Response, 1, SlaTargetUnit.BusinessDays),
                            Target("Approval", SlaTargetKind.Approval, 2, SlaTargetUnit.BusinessDays),
                            Target("Fulfilment", SlaTargetKind.Fulfillment, 5, SlaTargetUnit.BusinessDays)
                        ],
                        // the approval is measured, so only waiting for the requester stops the clock
                        pauseOn: ["Pending Customer"]
                    )
                ]),

            Class("Problem", "problem",
                fields:
                [
                    Field("Description", FieldType.Text, "The symptoms the related incidents share.", required: true),
                    Field("Impact", FieldType.Selection, "How widely the business is affected.", ImpactScale),
                    Field("Urgency", FieldType.Selection, "How quickly the cause has to be removed.", UrgencyScale),
                    Field("Affected Service", FieldType.Text, "The business service the problem disrupts."),
                    Field("Configuration Item", FieldType.Reference, "The configuration item suspected to cause it."),
                    Field("Problem Manager", FieldType.User, "Who is accountable for the investigation."),
                    Field("Assignment Group", FieldType.Selection, "The support group investigating it.", AssignmentGroups),
                    Field("Root Cause", FieldType.Multiline, "The underlying cause, once identified.", onCreate: false),
                    Field("Workaround", FieldType.Multiline, "How the impact is reduced until the cause is removed.", onCreate: false),
                    Field("Known Error", FieldType.Boolean, "Whether the problem is recorded as a known error: cause and workaround are documented.", onCreate: false),
                    Field("Permanent Fix", FieldType.Reference, "The change that removes the cause.", details: true, onCreate: false)
                ],
                priorities: ItilPriorities(),
                workflow: Workflow
                (
                    "Problem management",
                    "The ITIL problem lifecycle: identified, analysed to its root cause, kept as a known error, and resolved by a change.",
                    [
                        Status("New", WorkspaceTemplateStatus.ToDo, "Identified from incidents or trends."),
                        Status("Investigation", WorkspaceTemplateStatus.InProgress, "The root cause is being analysed."),
                        Status("Known Error", WorkspaceTemplateStatus.Waiting, "Root cause and workaround documented; waiting for a permanent fix."),
                        Status("Fix in Progress", WorkspaceTemplateStatus.InProgress, "A change removing the cause is being implemented."),
                        Status("Resolved", WorkspaceTemplateStatus.Done, "The cause is removed; watching for recurrence."),
                        Status("Closed", WorkspaceTemplateStatus.Done, "Nothing more to do; kept for reference.", end: true)
                    ],
                    [
                        Transition("Investigate", "New", "Investigation"),
                        Transition("Record known error", "Investigation", "Known Error"),
                        Transition("Fix", "Known Error", "Fix in Progress"),
                        Transition("Fix", "Investigation", "Fix in Progress"),
                        Transition("Resolve", "Fix in Progress", "Resolved"),
                        Transition("Recur", "Resolved", "Investigation"),
                        Transition("Close", "Resolved", "Closed"),
                        Transition("Close", "Known Error", "Closed")
                    ]
                ),
                calendars: [BusinessHours],
                slas:
                [
                    Sla
                    (
                        "Problem analysis",
                        "A problem is taken up within two working days and its root cause is identified within ten.",
                        SlaPriority.Medium,
                        BusinessHoursCalendar,
                        [
                            Target("First response", SlaTargetKind.Response, 2, SlaTargetUnit.BusinessDays),
                            Target("Root cause identified", SlaTargetKind.Custom, 10, SlaTargetUnit.BusinessDays)
                        ],
                        pauseOn: []
                    )
                ]),

            Class("Change", "change",
                fields: ChangeFields(),
                priorities: ChangePriorities(),
                workflow: ChangeWorkflow(),
                calendars: [BusinessHours, AlwaysOn],
                slas: ChangeSlas()),

            Class("Knowledge", "knowledge", ObjectKind.Document),
            Class("Announcement", "release", ObjectKind.Blog)
        ];
    }
}
