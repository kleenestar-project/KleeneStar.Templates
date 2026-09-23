using KleeneStar.Core.WebWorkspaceTemplate;
using KleeneStar.Model.Entities;
using System.Collections.Generic;
using static KleeneStar.Templates.WebWorkspaceTemplate.TemplateStructure;

namespace KleeneStar.Templates.WebWorkspaceTemplate
{
    /// <summary>
    /// A workspace for building software: where the code lives, what is being built from it, and
    /// what still has to be done to it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// It is the one template whose classes span three object kinds - the work items are issues,
    /// the specification is a document, and a release is announced as a post - which makes it the
    /// useful example of a workspace that is more than a ticket list.
    /// </para>
    /// <para>
    /// The work items follow an agile flow (to do, in progress, in review, done) with story
    /// points, a component and a fix version; a bug adds severity, reproduction steps and the
    /// environment it was found in, and is triaged within a working day. Sprints, repositories
    /// and pipelines are records rather than work, so they carry neither a priority nor an
    /// agreement. The specification is filled in as a form and goes through a review cycle.
    /// </para>
    /// </remarks>
    public sealed class SoftwareDevelopmentTemplate : WorkspaceTemplateBase
    {
        /// <summary>
        /// Gets the short name of the template, <c>development</c>, from which its key and its i18n
        /// keys are derived.
        /// </summary>
        protected override string Slug => "development";

        /// <summary>
        /// Gets the file name of the icon, <c>dev</c> - the icon of the seeded development workspace.
        /// </summary>
        protected override string IconName => "dev";

        /// <summary>
        /// Gets the key proposed for a new software development workspace, <c>DEV</c>.
        /// </summary>
        public override string SuggestedKey => "DEV";

        /// <summary>
        /// Gets the display order: second, after the service desk.
        /// </summary>
        public override int Order => 20;

        /// <summary>
        /// Gets the category the workspace is filed under, engineering.
        /// </summary>
        public override IEnumerable<string> Categories => ["Engineering"];

        /// <summary>
        /// The parts of the product work is sorted into.
        /// </summary>
        private static readonly string[] Components = ["Frontend", "Backend", "API", "Database", "Infrastructure", "Documentation"];

        /// <summary>
        /// The environments software runs in.
        /// </summary>
        private static readonly string[] Environments = ["Production", "Staging", "Test", "Development"];

        /// <summary>
        /// The agile flow of a work item.
        /// </summary>
        private static WorkspaceTemplateWorkflow AgileWorkflow() => Workflow
        (
            "Agile flow",
            "To do, in progress, in review, done - with a blocked state for work that cannot go on.",
            [
                Status("To Do", WorkspaceTemplateStatus.ToDo, "In the backlog or the sprint, not yet started."),
                Status("In Progress", WorkspaceTemplateStatus.InProgress, "Being implemented."),
                Status("Blocked", WorkspaceTemplateStatus.Waiting, "Cannot go on until something else is resolved."),
                Status("In Review", WorkspaceTemplateStatus.InProgress, "Implemented; in code review or testing."),
                Status("Done", WorkspaceTemplateStatus.Done, "Meets the definition of done.", end: true)
            ],
            [
                Transition("Start", "To Do", "In Progress"),
                Transition("Block", "In Progress", "Blocked"),
                Transition("Unblock", "Blocked", "In Progress"),
                Transition("Submit for review", "In Progress", "In Review"),
                Transition("Request changes", "In Review", "In Progress"),
                Transition("Complete", "In Review", "Done"),
                Transition("Reopen", "Done", "To Do")
            ]
        );

        /// <summary>
        /// Gets the classes the workspace starts with: the work items (tasks, bugs, sprints), the
        /// records of what is built (repositories, pipelines), and the specification, documentation
        /// and release notes - each with its structure.
        /// </summary>
        public override IEnumerable<WorkspaceTemplateClass> Classes =>
        [
            Class("Task", "task",
                fields:
                [
                    Field("Story Points", FieldType.Estimate, "The relative effort, in story points."),
                    Field("Component", FieldType.Selection, "The part of the product the work concerns.", Components),
                    Field("Sprint", FieldType.Reference, "The sprint the task is planned into.", onCreate: false),
                    Field("Fix Version", FieldType.Text, "The version the work ships in.", details: true),
                    Field("Acceptance Criteria", FieldType.Multiline, "What has to be true for the task to be done.", details: true)
                ],
                priorities:
                [
                    Priority("Highest", "Has to be done before anything else."),
                    Priority("High", "Important for the current goal."),
                    Priority("Medium", "Normal importance."),
                    Priority("Low", "Can wait."),
                    Priority("Lowest", "Only if nothing else is left.")
                ],
                workflow: AgileWorkflow(),
                calendars: [],
                slas: []),

            Class("Bug", "bug",
                fields:
                [
                    Field("Severity", FieldType.Selection, "How badly the defect affects the product, independent of how soon it is fixed.", ["S1 - Critical", "S2 - Major", "S3 - Moderate", "S4 - Minor"], required: true),
                    Field("Steps to Reproduce", FieldType.Multiline, "What has to be done to see the defect.", required: true),
                    Field("Expected Result", FieldType.Multiline, "What should have happened."),
                    Field("Actual Result", FieldType.Multiline, "What happened instead."),
                    Field("Environment", FieldType.Selection, "Where the defect was found.", Environments),
                    Field("Affected Version", FieldType.Text, "The version the defect was found in."),
                    Field("Component", FieldType.Selection, "The part of the product the defect is in.", Components),
                    Field("Fix Version", FieldType.Text, "The version the fix ships in.", details: true, onCreate: false),
                    Field("Root Cause", FieldType.Multiline, "Why the defect happened.", details: true, onCreate: false),
                    Field("Story Points", FieldType.Estimate, "The relative effort of the fix.", details: true, onCreate: false)
                ],
                priorities:
                [
                    Priority("Blocker", "Blocks development, testing or a release."),
                    Priority("Critical", "Crashes, data loss or a security issue."),
                    Priority("Major", "Major loss of function."),
                    Priority("Minor", "Minor loss of function or a UI issue."),
                    Priority("Trivial", "Cosmetic.")
                ],
                workflow: Workflow
                (
                    "Defect lifecycle",
                    "Reported, confirmed, fixed, verified - or dismissed.",
                    [
                        Status("New", WorkspaceTemplateStatus.ToDo, "Reported, not yet triaged."),
                        Status("Confirmed", WorkspaceTemplateStatus.ToDo, "Reproduced and accepted for fixing."),
                        Status("In Progress", WorkspaceTemplateStatus.InProgress, "Being fixed."),
                        Status("Needs Info", WorkspaceTemplateStatus.Waiting, "The reporter was asked for more information."),
                        Status("In Review", WorkspaceTemplateStatus.InProgress, "Fixed; in code review or testing."),
                        Status("Resolved", WorkspaceTemplateStatus.Done, "Fixed and verified.", end: true),
                        Status("Won't Fix", WorkspaceTemplateStatus.Done, "Accepted as is, a duplicate or not reproducible.", end: true)
                    ],
                    [
                        Transition("Confirm", "New", "Confirmed"),
                        Transition("Ask reporter", "New", "Needs Info"),
                        Transition("Reporter replied", "Needs Info", "New"),
                        Transition("Dismiss", "New", "Won't Fix"),
                        Transition("Start fix", "Confirmed", "In Progress"),
                        Transition("Submit for review", "In Progress", "In Review"),
                        Transition("Request changes", "In Review", "In Progress"),
                        Transition("Resolve", "In Review", "Resolved"),
                        Transition("Reopen", "Resolved", "Confirmed")
                    ]
                ),
                calendars: [BusinessHours],
                slas:
                [
                    Sla
                    (
                        "Bug triage",
                        "A reported bug is triaged within one working day; a blocker or critical bug is fixed within three.",
                        SlaPriority.High,
                        BusinessHoursCalendar,
                        [
                            Target("Triage", SlaTargetKind.Response, 1, SlaTargetUnit.BusinessDays),
                            Target("Fix", SlaTargetKind.Resolution, 3, SlaTargetUnit.BusinessDays)
                        ],
                        [ForPriority("Blocker"), ForPriority("Critical")]
                    )
                ]),

            Class("Sprint", "sprint",
                fields:
                [
                    Field("Sprint Goal", FieldType.Multiline, "What the sprint is meant to achieve.", required: true),
                    Field("Start Date", FieldType.Date, "The first day of the sprint.", required: true),
                    Field("End Date", FieldType.Date, "The last day of the sprint.", required: true),
                    Field("Capacity", FieldType.Number, "The story points the team can take on."),
                    Field("Completed Points", FieldType.Number, "The story points done by the end.", onCreate: false),
                    Field("Retrospective", FieldType.Multiline, "What went well and what to change.", details: true, onCreate: false)
                ],
                priorities: [],
                workflow: Workflow
                (
                    "Sprint",
                    "Planned, running, finished.",
                    [
                        Status("Planned", WorkspaceTemplateStatus.ToDo, "Being planned."),
                        Status("Active", WorkspaceTemplateStatus.InProgress, "Running."),
                        Status("Completed", WorkspaceTemplateStatus.Done, "Finished and reviewed.", end: true)
                    ],
                    [
                        Transition("Start sprint", "Planned", "Active"),
                        Transition("Complete sprint", "Active", "Completed")
                    ]
                ),
                calendars: [],
                slas: []),

            Class("Repository", "repo",
                fields:
                [
                    Field("Repository URL", FieldType.Text, "Where the repository is hosted.", required: true),
                    Field("Default Branch", FieldType.Text, "The branch releases are built from."),
                    Field("Visibility", FieldType.Radio, "Who may read the repository.", ["Private", "Internal", "Public"]),
                    Field("Language", FieldType.Selection, "The main programming language.", ["C#", "Java", "JavaScript", "TypeScript", "Python", "Go", "Rust", "Other"]),
                    Field("License", FieldType.Selection, "The license the code is published under.", ["Proprietary", "MIT", "Apache-2.0", "GPL-3.0", "Other"], details: true),
                    Field("Maintainer", FieldType.User, "Who reviews and merges changes.")
                ],
                priorities: [],
                workflow: RecordWorkflow("Active", "Archived"),
                calendars: [],
                slas: []),

            Class("BuildPipeline", "build",
                fields:
                [
                    Field("Repository", FieldType.Reference, "The repository the pipeline builds.", required: true),
                    Field("Trigger", FieldType.MultiSelection, "What starts a run.", ["Push", "Pull request", "Schedule", "Tag", "Manual"]),
                    Field("Branch Pattern", FieldType.Text, "The branches the pipeline runs on, e.g. main or release/*."),
                    Field("Target Environment", FieldType.Selection, "Where the pipeline deploys to.", Environments),
                    Field("Health", FieldType.TrafficLight, "How reliably the pipeline has been passing."),
                    Field("Owner", FieldType.User, "Who keeps the pipeline working.")
                ],
                priorities: [],
                workflow: Workflow
                (
                    "Pipeline lifecycle",
                    "A pipeline in use, paused, or retired.",
                    [
                        Status("Active", WorkspaceTemplateStatus.InProgress, "Runs on its triggers."),
                        Status("Disabled", WorkspaceTemplateStatus.Waiting, "Paused; does not run."),
                        Status("Retired", WorkspaceTemplateStatus.Done, "No longer used.", end: true)
                    ],
                    [
                        Transition("Disable", "Active", "Disabled"),
                        Transition("Enable", "Disabled", "Active"),
                        Transition("Retire", "Disabled", "Retired")
                    ]
                ),
                calendars: [],
                slas: []),

            Class("Documentation", "doc", ObjectKind.Document),

            // the same kind as Documentation - it stands in the same page tree - but filled in
            // through the input mask of its forms instead of written as prose
            Class("Specification", "requirement", ObjectKind.Document, ObjectRenderer.Form,
                fields:
                [
                    Field("Purpose", FieldType.Multiline, "What the specified part of the product is for.", required: true),
                    Field("Scope", FieldType.Multiline, "What is in and out of scope."),
                    Field("Functional Requirements", FieldType.Multiline, "What the product has to do."),
                    Field("Non-Functional Requirements", FieldType.Multiline, "How well it has to do it - performance, security, availability."),
                    Field("Acceptance Criteria", FieldType.Multiline, "How fulfilment is verified."),
                    Field("Stakeholders", FieldType.Text, "Who has to agree to the specification.", details: true)
                ],
                priorities: MoscowPriorities),

            Class("Release", "release", ObjectKind.Blog)
        ];
    }
}
