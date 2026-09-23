using KleeneStar.Core.WebWorkspaceTemplate;
using KleeneStar.Model.Entities;
using System.Collections.Generic;
using static KleeneStar.Templates.WebWorkspaceTemplate.TemplateStructure;

namespace KleeneStar.Templates.WebWorkspaceTemplate
{
    /// <summary>
    /// A workspace for the people of an organization: who they are, where they belong, and what
    /// is arranged for them.
    /// </summary>
    /// <remarks>
    /// The records - employees, units, positions - move through the employee lifecycle from
    /// pre-boarding to leaving; the processes - onboarding, absences, trainings - are work with
    /// a deadline. An onboarding has to be ready before the first day, an absence request is
    /// decided within three working days. Nothing personal beyond what the process needs is
    /// asked: the fields are the ones HR works with, not a personnel file.
    /// </remarks>
    public sealed class HumanResourcesTemplate : WorkspaceTemplateBase
    {
        /// <summary>
        /// Gets the short name of the template, <c>humanresources</c>, from which its key and its
        /// i18n keys are derived.
        /// </summary>
        protected override string Slug => "humanresources";

        /// <summary>
        /// Gets the file name of the icon, <c>hr</c> - the icon of the seeded HR workspace.
        /// </summary>
        protected override string IconName => "hr";

        /// <summary>
        /// Gets the key proposed for a new human resources workspace, <c>HR</c>.
        /// </summary>
        public override string SuggestedKey => "HR";

        /// <summary>
        /// Gets the display order: after finance, among the business templates.
        /// </summary>
        public override int Order => 50;

        /// <summary>
        /// Gets the category the workspace is filed under, human resources.
        /// </summary>
        public override IEnumerable<string> Categories => ["HumanResources"];

        /// <summary>
        /// The departments of an organization.
        /// </summary>
        private static readonly string[] Departments =
        [
            "Management", "Finance", "Human Resources", "IT", "Engineering", "Sales", "Marketing", "Operations", "Customer Service"
        ];

        /// <summary>
        /// Gets the classes the workspace starts with: the employee, organization unit and position
        /// records, and the onboarding, absence and training processes - each with its structure.
        /// </summary>
        public override IEnumerable<WorkspaceTemplateClass> Classes =>
        [
            Class("Employee", "employee",
                fields:
                [
                    Field("Personnel Number", FieldType.Text, "The internal employee number.", required: true),
                    Field("Department", FieldType.Selection, "The department the employee works in.", Departments, required: true),
                    Field("Job Title", FieldType.Text, "The employee's job title."),
                    Field("Manager", FieldType.User, "The employee's line manager."),
                    Field("Employment Type", FieldType.Radio, "How the employee is employed.", ["Full-time", "Part-time", "Fixed-term", "Contractor", "Intern"]),
                    Field("Entry Date", FieldType.Date, "The first working day."),
                    Field("Exit Date", FieldType.Date, "The last working day, once known.", onCreate: false),
                    Field("Work Location", FieldType.Text, "Where the employee usually works."),
                    Field("Work Email", FieldType.Text, "The business e-mail address.", details: true),
                    Field("Work Phone", FieldType.Text, "The business phone number.", details: true)
                ],
                priorities: [],
                workflow: Workflow
                (
                    "Employee lifecycle",
                    "From the signed contract to the last working day.",
                    [
                        Status("Pre-boarding", WorkspaceTemplateStatus.ToDo, "Contract signed, not yet started."),
                        Status("Active", WorkspaceTemplateStatus.InProgress, "Employed and working."),
                        Status("On Leave", WorkspaceTemplateStatus.Waiting, "On extended leave, e.g. parental leave."),
                        Status("Offboarding", WorkspaceTemplateStatus.InProgress, "Notice given; leaving is being arranged."),
                        Status("Left", WorkspaceTemplateStatus.Done, "No longer employed.", end: true)
                    ],
                    [
                        Transition("Start", "Pre-boarding", "Active"),
                        Transition("Go on leave", "Active", "On Leave"),
                        Transition("Return", "On Leave", "Active"),
                        Transition("Give notice", "Active", "Offboarding"),
                        Transition("Leave", "Offboarding", "Left"),
                        Transition("Withdraw", "Pre-boarding", "Left")
                    ]
                ),
                calendars: [],
                slas: []),

            Class("OrganizationUnit", "orgunit",
                fields:
                [
                    Field("Unit Code", FieldType.Text, "The short code of the unit.", required: true),
                    Field("Head", FieldType.User, "Who leads the unit."),
                    Field("Parent Unit", FieldType.Reference, "The unit this one belongs to."),
                    Field("Cost Center", FieldType.Text, "Where the unit's costs are booked."),
                    Field("Headcount", FieldType.Number, "How many people the unit has.", onCreate: false)
                ],
                priorities: [],
                workflow: RecordWorkflow("Active", "Dissolved"),
                calendars: [],
                slas: []),

            Class("Position", "position",
                fields:
                [
                    Field("Position Code", FieldType.Text, "The identifier of the position in the staffing plan.", required: true),
                    Field("Department", FieldType.Selection, "The department the position belongs to.", Departments, required: true),
                    Field("Grade", FieldType.Selection, "The salary grade.", ["Junior", "Professional", "Senior", "Lead", "Principal", "Executive"]),
                    Field("FTE", FieldType.Number, "The full-time equivalent, e.g. 1.0 or 0.5."),
                    Field("Hiring Manager", FieldType.User, "Who decides on the hire."),
                    Field("Posting Date", FieldType.Date, "When the position was advertised.", onCreate: false),
                    Field("Target Start", FieldType.Date, "When the position should be filled."),
                    Field("Job Description", FieldType.Multiline, "What the position is responsible for.", details: true)
                ],
                workflow: Workflow
                (
                    "Recruiting",
                    "From the approved opening to the signed contract.",
                    [
                        Status("Open", WorkspaceTemplateStatus.ToDo, "Approved, not yet advertised."),
                        Status("Recruiting", WorkspaceTemplateStatus.InProgress, "Advertised; candidates are being interviewed."),
                        Status("Offer", WorkspaceTemplateStatus.Waiting, "An offer is out; waiting for the candidate."),
                        Status("Filled", WorkspaceTemplateStatus.Done, "The contract is signed.", end: true),
                        Status("Cancelled", WorkspaceTemplateStatus.Done, "The position is not filled.", end: true)
                    ],
                    [
                        Transition("Advertise", "Open", "Recruiting"),
                        Transition("Make offer", "Recruiting", "Offer"),
                        Transition("Offer declined", "Offer", "Recruiting"),
                        Transition("Offer accepted", "Offer", "Filled"),
                        Transition("Cancel", "Open", "Cancelled"),
                        Transition("Cancel", "Recruiting", "Cancelled")
                    ]
                ),
                calendars: [],
                slas: []),

            Class("Onboarding", "onboarding",
                fields:
                [
                    Field("New Hire", FieldType.Text, "The name of the person starting.", required: true),
                    Field("Start Date", FieldType.Date, "The first working day.", required: true),
                    Field("Department", FieldType.Selection, "The department the person joins.", Departments),
                    Field("Manager", FieldType.User, "The person's line manager."),
                    Field("Buddy", FieldType.User, "The colleague who helps the person settle in."),
                    Field("Equipment", FieldType.MultiSelection, "What has to be ready on the first day.", ["Laptop", "Monitor", "Phone", "Headset", "Access card", "Desk"]),
                    Field("Accounts", FieldType.MultiSelection, "The accounts that have to be set up.", ["E-mail", "VPN", "ERP", "CRM", "Ticket system", "Code hosting"]),
                    Field("Welcome Plan", FieldType.Multiline, "What happens in the first weeks.", details: true)
                ],
                workflow: Workflow
                (
                    "Onboarding",
                    "Prepared before the first day, accompanied through probation.",
                    [
                        Status("Planned", WorkspaceTemplateStatus.ToDo, "The contract is signed; preparation has not started."),
                        Status("Preparation", WorkspaceTemplateStatus.InProgress, "Equipment and accounts are being prepared."),
                        Status("Probation", WorkspaceTemplateStatus.InProgress, "The person has started and is in probation."),
                        Status("Completed", WorkspaceTemplateStatus.Done, "Probation passed; onboarding is complete.", end: true),
                        Status("Cancelled", WorkspaceTemplateStatus.Done, "The person did not start.", end: true)
                    ],
                    [
                        Transition("Prepare", "Planned", "Preparation"),
                        Transition("First day", "Preparation", "Probation"),
                        Transition("Complete", "Probation", "Completed"),
                        Transition("Cancel", "Planned", "Cancelled"),
                        Transition("Cancel", "Preparation", "Cancelled")
                    ]
                ),
                calendars: [BusinessHours],
                slas:
                [
                    Sla
                    (
                        "Ready for the first day",
                        "Equipment and accounts are prepared within five working days, so everything is ready when the person starts.",
                        SlaPriority.High,
                        BusinessHoursCalendar,
                        [Target("Preparation", SlaTargetKind.Fulfillment, 5, SlaTargetUnit.BusinessDays)]
                    )
                ]),

            Class("Absence", "absence",
                fields:
                [
                    Field("Absence Type", FieldType.Selection, "What kind of absence it is.", ["Vacation", "Sick leave", "Parental leave", "Special leave", "Unpaid leave", "Training", "Remote work"], required: true),
                    Field("Period", FieldType.DateRange, "From when to when.", required: true),
                    Field("Days", FieldType.Number, "How many working days it takes."),
                    Field("Substitute", FieldType.User, "Who stands in."),
                    Field("Approver", FieldType.User, "Who approves the absence."),
                    Field("Remarks", FieldType.Multiline, "Anything the approver should know.", details: true)
                ],
                priorities: [],
                workflow: Workflow
                (
                    "Absence request",
                    "Requested, decided, taken.",
                    [
                        Status("Requested", WorkspaceTemplateStatus.ToDo, "Waiting for the approver."),
                        Status("Approved", WorkspaceTemplateStatus.InProgress, "Approved; not yet taken."),
                        Status("Taken", WorkspaceTemplateStatus.Done, "The absence has been taken.", end: true),
                        Status("Rejected", WorkspaceTemplateStatus.Done, "Not approved.", end: true),
                        Status("Cancelled", WorkspaceTemplateStatus.Done, "Withdrawn by the employee.", end: true)
                    ],
                    [
                        Transition("Approve", "Requested", "Approved"),
                        Transition("Reject", "Requested", "Rejected"),
                        Transition("Withdraw", "Requested", "Cancelled"),
                        Transition("Withdraw", "Approved", "Cancelled"),
                        Transition("Record as taken", "Approved", "Taken")
                    ]
                ),
                calendars: [BusinessHours],
                slas:
                [
                    Sla
                    (
                        "Absence decision",
                        "An absence request is decided within three working days.",
                        SlaPriority.Medium,
                        BusinessHoursCalendar,
                        [Target("Decision", SlaTargetKind.Approval, 3, SlaTargetUnit.BusinessDays)]
                    )
                ]),

            Class("Training", "training",
                fields:
                [
                    Field("Participant", FieldType.User, "Who attends.", required: true),
                    Field("Training Type", FieldType.Selection, "What kind of training it is.", ["Classroom", "Online course", "Conference", "Certification", "Coaching"]),
                    Field("Provider", FieldType.Text, "Who offers the training."),
                    Field("Date", FieldType.Date, "When the training takes place."),
                    Field("Duration", FieldType.Number, "How long it takes, in hours."),
                    Field("Cost", FieldType.Number, "What it costs."),
                    Field("Certificate", FieldType.Attachment, "The certificate of attendance or the credential.", onCreate: false),
                    Field("Valid Until", FieldType.Date, "When a certification expires.", details: true, onCreate: false)
                ],
                priorities: [],
                workflow: Workflow
                (
                    "Training",
                    "Requested, approved, attended.",
                    [
                        Status("Requested", WorkspaceTemplateStatus.ToDo, "Waiting for approval."),
                        Status("Approved", WorkspaceTemplateStatus.ToDo, "Approved and booked."),
                        Status("Attended", WorkspaceTemplateStatus.Done, "Attended; evidence is on file.", end: true),
                        Status("Rejected", WorkspaceTemplateStatus.Done, "Not approved.", end: true),
                        Status("Cancelled", WorkspaceTemplateStatus.Done, "Did not take place.", end: true)
                    ],
                    [
                        Transition("Approve", "Requested", "Approved"),
                        Transition("Reject", "Requested", "Rejected"),
                        Transition("Record attendance", "Approved", "Attended"),
                        Transition("Cancel", "Approved", "Cancelled")
                    ]
                ),
                calendars: [],
                slas: [])
        ];
    }
}
