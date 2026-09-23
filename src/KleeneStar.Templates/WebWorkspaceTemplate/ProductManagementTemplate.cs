using KleeneStar.Core.WebWorkspaceTemplate;
using KleeneStar.Model.Entities;
using System.Collections.Generic;
using static KleeneStar.Templates.WebWorkspaceTemplate.TemplateStructure;

namespace KleeneStar.Templates.WebWorkspaceTemplate
{
    /// <summary>
    /// A workspace for deciding what to build: what was asked for, what it is worth, and when it
    /// is meant to arrive.
    /// </summary>
    /// <remarks>
    /// Scope is ranked, not scheduled, so features, requirements and use cases carry the MoSCoW
    /// scale rather than an urgency. A feature is scored the RICE way - reach, impact,
    /// confidence, effort - and moves from idea through discovery to release. Requirements are
    /// reviewed and verified; the roadmap is published in now / next / later horizons; a release
    /// plan runs to its code freeze; stakeholders are mapped by influence and interest.
    /// </remarks>
    public sealed class ProductManagementTemplate : WorkspaceTemplateBase
    {
        /// <summary>
        /// Gets the short name of the template, <c>productmanagement</c>, from which its key and its
        /// i18n keys are derived.
        /// </summary>
        protected override string Slug => "productmanagement";

        /// <summary>
        /// Gets the file name of the icon, <c>pm</c> - the icon of the seeded product management
        /// workspace.
        /// </summary>
        protected override string IconName => "pm";

        /// <summary>
        /// Gets the key proposed for a new product management workspace, <c>PM</c>.
        /// </summary>
        public override string SuggestedKey => "PM";

        /// <summary>
        /// Gets the display order: after human resources, before procurement.
        /// </summary>
        public override int Order => 60;

        /// <summary>
        /// Gets the category the workspace is filed under, development - the side of the house it
        /// decides for.
        /// </summary>
        public override IEnumerable<string> Categories => ["Development"];

        /// <summary>
        /// The areas of the product.
        /// </summary>
        private static readonly string[] ProductAreas = ["Core", "User experience", "Integrations", "Reporting", "Administration", "Mobile"];

        /// <summary>
        /// The three-step scale stakeholders are mapped on.
        /// </summary>
        private static readonly string[] Level = ["High", "Medium", "Low"];

        /// <summary>
        /// Gets the classes the workspace starts with: features, requirements, the roadmap, release
        /// plans, use cases and stakeholders - each with its structure.
        /// </summary>
        public override IEnumerable<WorkspaceTemplateClass> Classes =>
        [
            Class("Feature", "feature",
                fields:
                [
                    Field("Problem Statement", FieldType.Multiline, "Which customer problem the feature solves.", required: true),
                    Field("Product Area", FieldType.Selection, "The part of the product it belongs to.", ProductAreas),
                    Field("Product Manager", FieldType.User, "Who owns the feature."),
                    Field("Reach", FieldType.Number, "How many customers it affects per quarter (RICE)."),
                    Field("Impact", FieldType.Selection, "How much it moves the needle for each of them (RICE).", ["3 - Massive", "2 - High", "1 - Medium", "0.5 - Low", "0.25 - Minimal"]),
                    Field("Confidence", FieldType.Selection, "How sure the estimates are (RICE).", ["100%", "80%", "50%"]),
                    Field("Effort", FieldType.Estimate, "The effort in person-months (RICE)."),
                    Field("Target Release", FieldType.Reference, "The release plan it is meant to ship in.", onCreate: false),
                    Field("Success Metric", FieldType.Text, "How success is measured once it is shipped.", details: true)
                ],
                priorities: MoscowPriorities,
                workflow: Workflow
                (
                    "Feature lifecycle",
                    "From the idea through discovery and delivery to the release.",
                    [
                        Status("Idea", WorkspaceTemplateStatus.ToDo, "Proposed, not yet assessed."),
                        Status("Discovery", WorkspaceTemplateStatus.InProgress, "The problem and the solution are being validated."),
                        Status("Ready", WorkspaceTemplateStatus.ToDo, "Specified and ready for development."),
                        Status("In Development", WorkspaceTemplateStatus.InProgress, "Being built."),
                        Status("Released", WorkspaceTemplateStatus.Done, "Shipped to customers.", end: true),
                        Status("Rejected", WorkspaceTemplateStatus.Done, "Not pursued.", end: true)
                    ],
                    [
                        Transition("Discover", "Idea", "Discovery"),
                        Transition("Reject", "Idea", "Rejected"),
                        Transition("Reject", "Discovery", "Rejected"),
                        Transition("Define", "Discovery", "Ready"),
                        Transition("Start development", "Ready", "In Development"),
                        Transition("Release", "In Development", "Released")
                    ]
                ),
                calendars: [],
                slas: []),

            Class("Requirement", "requirement",
                fields:
                [
                    Field("Requirement ID", FieldType.Text, "The identifier the requirement is traced by, e.g. REQ-042.", required: true),
                    Field("Requirement Type", FieldType.Radio, "What kind of requirement it is.", ["Functional", "Non-functional", "Constraint"], required: true),
                    Field("Feature", FieldType.Reference, "The feature the requirement belongs to."),
                    Field("Source", FieldType.Text, "Who or what the requirement comes from."),
                    Field("Acceptance Criteria", FieldType.Multiline, "How fulfilment is recognized.", required: true),
                    Field("Verification Method", FieldType.Selection, "How fulfilment is verified.", ["Test", "Inspection", "Demonstration", "Analysis"], details: true)
                ],
                priorities: MoscowPriorities,
                workflow: Workflow
                (
                    "Requirement lifecycle",
                    "Written, reviewed, approved, implemented and verified.",
                    [
                        Status("Draft", WorkspaceTemplateStatus.ToDo, "Being written."),
                        Status("In Review", WorkspaceTemplateStatus.InProgress, "Being reviewed by the stakeholders."),
                        Status("Approved", WorkspaceTemplateStatus.ToDo, "Agreed; waiting for implementation."),
                        Status("Implemented", WorkspaceTemplateStatus.InProgress, "Implemented; waiting for verification."),
                        Status("Verified", WorkspaceTemplateStatus.Done, "Verified as fulfilled.", end: true),
                        Status("Rejected", WorkspaceTemplateStatus.Done, "Not accepted.", end: true)
                    ],
                    [
                        Transition("Submit for review", "Draft", "In Review"),
                        Transition("Request changes", "In Review", "Draft"),
                        Transition("Approve", "In Review", "Approved"),
                        Transition("Reject", "In Review", "Rejected"),
                        Transition("Implement", "Approved", "Implemented"),
                        Transition("Verify", "Implemented", "Verified"),
                        Transition("Fails verification", "Implemented", "Approved")
                    ]
                ),
                calendars: [],
                slas: []),

            Class("Roadmap", "roadmap",
                fields:
                [
                    Field("Horizon", FieldType.Radio, "How firm the item on the roadmap is.", ["Now", "Next", "Later"], required: true),
                    Field("Timeframe", FieldType.DateRange, "The period the roadmap item is planned for."),
                    Field("Theme", FieldType.Text, "The strategic theme it serves."),
                    Field("Objective", FieldType.Multiline, "The outcome it aims for."),
                    Field("Owner", FieldType.User, "Who is accountable for it.")
                ],
                priorities: [],
                workflow: Workflow
                (
                    "Roadmap item",
                    "Drafted, published, achieved or dropped.",
                    [
                        Status("Draft", WorkspaceTemplateStatus.ToDo, "Not yet communicated."),
                        Status("Published", WorkspaceTemplateStatus.InProgress, "On the published roadmap."),
                        Status("Achieved", WorkspaceTemplateStatus.Done, "The objective is reached.", end: true),
                        Status("Dropped", WorkspaceTemplateStatus.Done, "Taken off the roadmap.", end: true)
                    ],
                    [
                        Transition("Publish", "Draft", "Published"),
                        Transition("Achieve", "Published", "Achieved"),
                        Transition("Drop", "Published", "Dropped"),
                        Transition("Drop", "Draft", "Dropped")
                    ]
                ),
                calendars: [],
                slas: []),

            Class("ReleasePlan", "releaseplan",
                fields:
                [
                    Field("Version", FieldType.Text, "The version number of the release, e.g. 2.4.0.", required: true),
                    Field("Release Type", FieldType.Radio, "How big the release is.", ["Major", "Minor", "Patch"]),
                    Field("Planned Date", FieldType.Date, "When the release is planned to ship.", required: true),
                    Field("Code Freeze", FieldType.Date, "From when only fixes are accepted."),
                    Field("Release Manager", FieldType.User, "Who coordinates the release."),
                    Field("Scope", FieldType.Multiline, "What the release contains."),
                    Field("Release Notes", FieldType.Multiline, "What changed, for customers.", details: true, onCreate: false)
                ],
                workflow: Workflow
                (
                    "Release",
                    "Planned, built, frozen and shipped.",
                    [
                        Status("Planning", WorkspaceTemplateStatus.ToDo, "The scope is being decided."),
                        Status("In Progress", WorkspaceTemplateStatus.InProgress, "The scope is being built."),
                        Status("Code Freeze", WorkspaceTemplateStatus.InProgress, "Only fixes are accepted; the release is being stabilized."),
                        Status("Released", WorkspaceTemplateStatus.Done, "Shipped.", end: true),
                        Status("Cancelled", WorkspaceTemplateStatus.Done, "Will not ship.", end: true)
                    ],
                    [
                        Transition("Start", "Planning", "In Progress"),
                        Transition("Freeze", "In Progress", "Code Freeze"),
                        Transition("Unfreeze", "Code Freeze", "In Progress"),
                        Transition("Ship", "Code Freeze", "Released"),
                        Transition("Cancel", "Planning", "Cancelled"),
                        Transition("Cancel", "In Progress", "Cancelled")
                    ]
                ),
                calendars: [],
                slas: []),

            Class("UseCase", "usecase",
                fields:
                [
                    Field("Primary Actor", FieldType.Text, "Who starts the use case.", required: true),
                    Field("Goal", FieldType.Text, "What the actor wants to achieve."),
                    Field("Preconditions", FieldType.Multiline, "What has to be true before it starts."),
                    Field("Main Flow", FieldType.Multiline, "The steps of the successful path.", required: true),
                    Field("Alternative Flows", FieldType.Multiline, "Variations and error paths.", details: true),
                    Field("Postconditions", FieldType.Multiline, "What is true once it succeeded.", details: true),
                    Field("Feature", FieldType.Reference, "The feature it belongs to.", details: true)
                ],
                priorities: MoscowPriorities,
                workflow: ReviewWorkflow(),
                calendars: [],
                slas: []),

            Class("Stakeholder", "stakeholder",
                fields:
                [
                    Field("Organization", FieldType.Text, "The company or department the stakeholder belongs to."),
                    Field("Role", FieldType.Text, "The stakeholder's role, e.g. customer, sponsor, user."),
                    Field("Email", FieldType.Text, "How to reach the stakeholder."),
                    Field("Influence", FieldType.Radio, "How much the stakeholder can shape decisions.", Level),
                    Field("Interest", FieldType.Radio, "How much the stakeholder cares about the product.", Level),
                    Field("Engagement", FieldType.Selection, "How the stakeholder is engaged.", ["Manage closely", "Keep satisfied", "Keep informed", "Monitor"])
                ],
                priorities: [],
                workflow: RecordWorkflow(),
                calendars: [],
                slas: [])
        ];
    }
}
