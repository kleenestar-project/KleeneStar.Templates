using KleeneStar.Core.WebWorkspaceTemplate;
using KleeneStar.Model.Entities;
using System.Collections.Generic;
using static KleeneStar.Templates.WebWorkspaceTemplate.SharedStructures;
using static KleeneStar.Templates.WebWorkspaceTemplate.TemplateStructure;

namespace KleeneStar.Templates.WebWorkspaceTemplate
{
    /// <summary>
    /// A workspace for keeping track of what an installation is made of: the assets, how they
    /// depend on each other, and what has to be true about them.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Its central class holds assets rather than issues, which is what distinguishes a
    /// configuration database from a work-item workspace: the objects are the things themselves,
    /// and the issues beside them are what happened to those things.
    /// </para>
    /// <para>
    /// The structure follows ITIL service configuration management. An asset is a
    /// configuration item with a type, an environment, a support group and the technical data
    /// operations looks up, ranked by business criticality and moving through the asset
    /// lifecycle. A relationship connects two of them with a typed dependency. A change request
    /// is the same change enablement the service desk runs. A vulnerability is remediated within
    /// a deadline set by its severity; compliance controls are assessed against a framework and
    /// policies are reviewed and approved documents of record.
    /// </para>
    /// </remarks>
    public sealed class ConfigurationDatabaseTemplate : WorkspaceTemplateBase
    {
        /// <summary>
        /// Gets the short name of the template, <c>cmdb</c>, from which its key, its i18n keys
        /// and its icon (<c>cmdb.svg</c>) are derived.
        /// </summary>
        protected override string Slug => "cmdb";

        /// <summary>
        /// Gets the key proposed for a new configuration database workspace, <c>CMDB</c>.
        /// </summary>
        public override string SuggestedKey => "CMDB";

        /// <summary>
        /// Gets the display order: third, after the service desk and software development, which
        /// it is the inventory for.
        /// </summary>
        public override int Order => 30;

        /// <summary>
        /// Gets the categories the workspace is filed under: infrastructure, and compliance for the
        /// controls and policies it keeps.
        /// </summary>
        public override IEnumerable<string> Categories => ["Infrastructure", "Compliance"];

        /// <summary>
        /// The support groups a configuration item is looked after by.
        /// </summary>
        private static readonly string[] SupportGroups =
        [
            "Server Operations", "Network Operations", "Database Administration", "Workplace Services", "Cloud Operations", "Application Support"
        ];

        /// <summary>
        /// Gets the classes the workspace starts with: the configuration items and their
        /// relationships, the change requests and approvals acting on them, and the vulnerabilities,
        /// compliance controls and policies measured against them - each with its structure.
        /// </summary>
        public override IEnumerable<WorkspaceTemplateClass> Classes =>
        [
            Class("Asset", "asset", ObjectKind.Asset,
                fields:
                [
                    Field("CI Type", FieldType.Selection, "What kind of configuration item this is.",
                        ["Server", "Virtual Machine", "Network Device", "Storage", "Database", "Application", "Cloud Service", "Workstation", "Laptop", "Mobile Device", "Software License"],
                        required: true),
                    Field("Environment", FieldType.Selection, "Which environment the item belongs to.", ["Production", "Staging", "Test", "Development"]),
                    Field("Support Group", FieldType.Selection, "The group that operates and repairs the item.", SupportGroups),
                    Field("Manufacturer", FieldType.Text, "Who made the item."),
                    Field("Model", FieldType.Text, "The model or edition."),
                    Field("Serial Number", FieldType.Text, "The manufacturer's serial number or license key.", details: true),
                    Field("Hostname", FieldType.Text, "The network name of the item.", details: true),
                    Field("IP Address", FieldType.Text, "The primary network address.", details: true),
                    Field("Operating System", FieldType.Text, "The operating system and version.", details: true),
                    Field("Purchase Date", FieldType.Date, "When the item was acquired.", details: true),
                    Field("Warranty Until", FieldType.Date, "When the warranty or support contract ends.", details: true),
                    Field("Cost", FieldType.Number, "What the item cost.", details: true)
                ],
                priorities:
                [
                    Priority("Business critical", "An outage stops a business-critical service."),
                    Priority("High", "An outage severely degrades a service."),
                    Priority("Medium", "An outage affects a group of users."),
                    Priority("Low", "An outage affects a single user or nobody.")
                ]),

            Class("Relationship", "rel",
                fields:
                [
                    Field("Source", FieldType.Reference, "The configuration item the relationship starts at.", required: true),
                    Field("Relationship Type", FieldType.Selection, "How the source relates to the target.",
                        ["Depends on", "Runs on", "Hosts", "Connected to", "Uses", "Backs up", "Member of"],
                        required: true),
                    Field("Target", FieldType.Reference, "The configuration item the relationship ends at.", required: true),
                    Field("Verified On", FieldType.Date, "When the relationship was last confirmed by discovery or audit.", onCreate: false)
                ],
                priorities: [],
                workflow: RecordWorkflow("Active", "Retired"),
                calendars: [],
                slas: []),

            Class("ChangeRequest", "change",
                fields: ChangeFields(),
                priorities: ChangePriorities(),
                workflow: ChangeWorkflow(),
                calendars: [BusinessHours, AlwaysOn],
                slas: ChangeSlas()),

            Class("Vulnerability", "vuln",
                fields:
                [
                    Field("CVE ID", FieldType.Text, "The identifier in the CVE catalog, e.g. CVE-2026-12345."),
                    Field("Severity", FieldType.Selection, "The severity by CVSS rating.", ["Critical", "High", "Medium", "Low", "None"], required: true),
                    Field("CVSS Score", FieldType.Number, "The CVSS base score, 0.0 to 10.0."),
                    Field("Affected Item", FieldType.Reference, "The configuration item that is vulnerable.", required: true),
                    Field("Exploit Available", FieldType.Boolean, "Whether a public exploit is known."),
                    Field("Discovered On", FieldType.Date, "When the vulnerability was found."),
                    Field("Remediation", FieldType.Multiline, "How the vulnerability is removed or mitigated.", onCreate: false),
                    Field("Risk Acceptance", FieldType.Multiline, "Why the risk is accepted, and by whom, if it is not remediated.", details: true, onCreate: false)
                ],
                workflow: Workflow
                (
                    "Vulnerability management",
                    "Found, assessed, remediated and verified - or accepted as a risk.",
                    [
                        Status("New", WorkspaceTemplateStatus.ToDo, "Reported by a scan, an advisory or a person."),
                        Status("Triaged", WorkspaceTemplateStatus.ToDo, "Assessed and assigned for remediation."),
                        Status("Remediation", WorkspaceTemplateStatus.InProgress, "Being patched, reconfigured or mitigated."),
                        Status("Verification", WorkspaceTemplateStatus.InProgress, "Remediated; the fix is being verified by a rescan."),
                        Status("Closed", WorkspaceTemplateStatus.Done, "Remediated and verified.", end: true),
                        Status("Risk Accepted", WorkspaceTemplateStatus.Done, "Not remediated; the risk is formally accepted.", end: true),
                        Status("False Positive", WorkspaceTemplateStatus.Done, "Turned out not to apply.", end: true)
                    ],
                    [
                        Transition("Triage", "New", "Triaged"),
                        Transition("Dismiss", "New", "False Positive"),
                        Transition("Remediate", "Triaged", "Remediation"),
                        Transition("Accept risk", "Triaged", "Risk Accepted"),
                        Transition("Verify", "Remediation", "Verification"),
                        Transition("Still vulnerable", "Verification", "Remediation"),
                        Transition("Close", "Verification", "Closed")
                    ]
                ),
                calendars: [AlwaysOn],
                slas:
                [
                    Sla
                    (
                        "Critical vulnerability",
                        "A critical vulnerability is triaged within a day and remediated within seven days.",
                        SlaPriority.Critical,
                        AlwaysOnCalendar,
                        [
                            Target("Triage", SlaTargetKind.Response, 24, SlaTargetUnit.Hours),
                            Target("Remediation", SlaTargetKind.Resolution, 7, SlaTargetUnit.Days)
                        ],
                        [ForPriority("Critical")],
                        [Escalate(3, SlaTargetUnit.Days, "Information Security Officer")]
                    ),
                    Sla
                    (
                        "High vulnerability",
                        "A high vulnerability is remediated within thirty days.",
                        SlaPriority.High,
                        AlwaysOnCalendar,
                        [Target("Remediation", SlaTargetKind.Resolution, 30, SlaTargetUnit.Days)],
                        [ForPriority("High")]
                    ),
                    Sla
                    (
                        "Medium and low vulnerability",
                        "A medium or low vulnerability is remediated within ninety days.",
                        SlaPriority.Medium,
                        AlwaysOnCalendar,
                        [Target("Remediation", SlaTargetKind.Resolution, 90, SlaTargetUnit.Days)],
                        [ForPriority("Medium"), ForPriority("Low")]
                    )
                ]),

            Class("Compliance", "compliance",
                fields:
                [
                    Field("Control ID", FieldType.Text, "The identifier of the control in its framework, e.g. A.8.8.", required: true),
                    Field("Framework", FieldType.Selection, "The framework the control belongs to.", ["ISO/IEC 27001", "SOC 2", "GDPR", "BSI IT-Grundschutz", "NIS2", "PCI DSS", "Internal"], required: true),
                    Field("Control Owner", FieldType.User, "Who is accountable for the control."),
                    Field("Compliance Status", FieldType.TrafficLight, "Whether the control is met.", onCreate: false),
                    Field("Last Assessment", FieldType.Date, "When the control was last assessed.", onCreate: false),
                    Field("Next Assessment", FieldType.Date, "When the control is due to be assessed again."),
                    Field("Evidence", FieldType.Attachment, "The proof that the control is met.", onCreate: false),
                    Field("Findings", FieldType.Multiline, "What the assessment found.", details: true, onCreate: false)
                ],
                workflow: Workflow
                (
                    "Control assessment",
                    "A control is assessed, remediated where it falls short, and confirmed.",
                    [
                        Status("Planned", WorkspaceTemplateStatus.ToDo, "Scheduled for assessment."),
                        Status("In Assessment", WorkspaceTemplateStatus.InProgress, "Being assessed; evidence is collected."),
                        Status("Remediation", WorkspaceTemplateStatus.InProgress, "Not met; findings are being remediated."),
                        Status("Compliant", WorkspaceTemplateStatus.Done, "Met and evidenced."),
                        Status("Not Applicable", WorkspaceTemplateStatus.Done, "Excluded from the scope, with justification.", end: true)
                    ],
                    [
                        Transition("Assess", "Planned", "In Assessment"),
                        Transition("Confirm", "In Assessment", "Compliant"),
                        Transition("Record findings", "In Assessment", "Remediation"),
                        Transition("Reassess", "Remediation", "In Assessment"),
                        Transition("Schedule reassessment", "Compliant", "Planned"),
                        Transition("Exclude", "Planned", "Not Applicable")
                    ]
                ),
                calendars: [BusinessHours],
                slas:
                [
                    Sla
                    (
                        "Finding remediation",
                        "A finding is remediated within sixty working days of the assessment.",
                        SlaPriority.Medium,
                        BusinessHoursCalendar,
                        [Target("Remediation", SlaTargetKind.Resolution, 60, SlaTargetUnit.BusinessDays)]
                    )
                ]),

            Class("Policy", "policy", @sealed: true,
                fields:
                [
                    Field("Policy Owner", FieldType.User, "Who is accountable for the policy.", required: true),
                    Field("Version", FieldType.Text, "The version of the policy, e.g. 2.1."),
                    Field("Scope", FieldType.Multiline, "What and whom the policy applies to."),
                    Field("Effective Date", FieldType.Date, "When the policy takes effect."),
                    Field("Next Review", FieldType.Date, "When the policy is due to be reviewed."),
                    Field("Policy Document", FieldType.Attachment, "The approved policy text.", details: true)
                ],
                priorities: [],
                workflow: ReviewWorkflow(),
                calendars: [],
                slas: []),

            Class("Approval", "approval",
                fields: ApprovalFields(monetary: false),
                workflow: ApprovalWorkflow(),
                calendars: [BusinessHours],
                slas: ApprovalSlas())
        ];
    }
}
