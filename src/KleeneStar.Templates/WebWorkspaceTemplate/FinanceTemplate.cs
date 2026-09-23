using KleeneStar.Core.WebWorkspaceTemplate;
using KleeneStar.Model.Entities;
using System.Collections.Generic;
using static KleeneStar.Templates.WebWorkspaceTemplate.SharedStructures;
using static KleeneStar.Templates.WebWorkspaceTemplate.TemplateStructure;

namespace KleeneStar.Templates.WebWorkspaceTemplate
{
    /// <summary>
    /// A workspace for money: what was planned, what was spent, and who agreed to it.
    /// </summary>
    /// <remarks>
    /// Every class that moves money carries an amount and a currency and passes an approval
    /// before it takes effect: a budget is submitted and approved per fiscal year, an invoice
    /// is verified, approved and paid within its terms, an approval is decided within two
    /// working days. Cost centers are master data; contracts run from negotiation to their end;
    /// a forecast is a reviewed scenario for a period.
    /// </remarks>
    public sealed class FinanceTemplate : WorkspaceTemplateBase
    {
        /// <summary>
        /// Gets the short name of the template, <c>finance</c>, from which its key and its i18n keys
        /// are derived.
        /// </summary>
        protected override string Slug => "finance";

        /// <summary>
        /// Gets the file name of the icon, <c>fin</c> - the icon of the seeded finance workspace.
        /// </summary>
        protected override string IconName => "fin";

        /// <summary>
        /// Gets the key proposed for a new finance workspace, <c>FIN</c>.
        /// </summary>
        public override string SuggestedKey => "FIN";

        /// <summary>
        /// Gets the display order: after the technical templates, first of the business ones.
        /// </summary>
        public override int Order => 40;

        /// <summary>
        /// Gets the category the workspace is filed under, finance.
        /// </summary>
        public override IEnumerable<string> Categories => ["Finance"];

        /// <summary>
        /// The fiscal years a plan is made for.
        /// </summary>
        private static readonly string[] FiscalYears = ["2026", "2027", "2028"];

        /// <summary>
        /// The periods a plan is broken down into.
        /// </summary>
        private static readonly string[] Periods = ["Q1", "Q2", "Q3", "Q4", "Full year"];

        /// <summary>
        /// Gets the classes the workspace starts with: budgets, invoices, cost centers, contracts,
        /// forecasts and approvals - each with its structure.
        /// </summary>
        public override IEnumerable<WorkspaceTemplateClass> Classes =>
        [
            Class("Budget", "budget",
                fields:
                [
                    Field("Fiscal Year", FieldType.Selection, "The fiscal year the budget is for.", FiscalYears, required: true),
                    Field("Period", FieldType.Selection, "The period within the year.", Periods),
                    Field("Cost Center", FieldType.Reference, "The cost center the budget is allocated to.", required: true),
                    Field("Budget Owner", FieldType.User, "Who may spend the budget and answers for it."),
                    Field("Amount", FieldType.Number, "The approved amount.", required: true),
                    Field("Currency", FieldType.Selection, "The currency of the amount.", Currencies),
                    Field("Spent", FieldType.Number, "What has been spent so far.", onCreate: false),
                    Field("Utilization", FieldType.TrafficLight, "Whether spending is on plan.", onCreate: false),
                    Field("Justification", FieldType.Multiline, "What the money is for.", details: true)
                ],
                priorities: [],
                workflow: Workflow
                (
                    "Budget cycle",
                    "Planned, approved, spent against and closed at the end of the period.",
                    [
                        Status("Draft", WorkspaceTemplateStatus.ToDo, "Being planned."),
                        Status("Submitted", WorkspaceTemplateStatus.Waiting, "Submitted for approval."),
                        Status("Approved", WorkspaceTemplateStatus.InProgress, "Approved; spending runs against it."),
                        Status("Frozen", WorkspaceTemplateStatus.Waiting, "Temporarily blocked for further spending."),
                        Status("Closed", WorkspaceTemplateStatus.Done, "The period is over and the budget is settled.", end: true),
                        Status("Rejected", WorkspaceTemplateStatus.Done, "Not approved.", end: true)
                    ],
                    [
                        Transition("Submit", "Draft", "Submitted"),
                        Transition("Approve", "Submitted", "Approved"),
                        Transition("Return", "Submitted", "Draft"),
                        Transition("Reject", "Submitted", "Rejected"),
                        Transition("Freeze", "Approved", "Frozen"),
                        Transition("Release", "Frozen", "Approved"),
                        Transition("Close", "Approved", "Closed")
                    ]
                ),
                calendars: [],
                slas: []),

            Class("Invoice", "invoice",
                fields: InvoiceFields(threeWayMatch: false),
                priorities: InvoicePriorities(),
                workflow: InvoiceWorkflow(),
                calendars: [BusinessHours],
                slas: InvoiceSlas()),

            Class("CostCenter", "costcenter",
                fields:
                [
                    Field("Code", FieldType.Text, "The cost center number in the chart of accounts.", required: true),
                    Field("Responsible", FieldType.User, "Who answers for the costs booked here.", required: true),
                    Field("Department", FieldType.Text, "The organizational unit the cost center belongs to."),
                    Field("Parent", FieldType.Reference, "The cost center this one rolls up into."),
                    Field("Valid From", FieldType.Date, "From when costs may be booked here."),
                    Field("Valid Until", FieldType.Date, "Until when costs may be booked here.", details: true)
                ],
                priorities: [],
                workflow: RecordWorkflow("Active", "Closed"),
                calendars: [],
                slas: []),

            Class("Contract", "contract",
                fields: ContractFields("Counterparty"),
                workflow: ContractWorkflow(),
                calendars: [],
                slas: []),

            Class("Forecast", "forecast",
                fields:
                [
                    Field("Fiscal Year", FieldType.Selection, "The fiscal year forecast.", FiscalYears, required: true),
                    Field("Period", FieldType.Selection, "The period forecast.", Periods),
                    Field("Scenario", FieldType.Radio, "Which scenario the figures describe.", ["Base case", "Best case", "Worst case"]),
                    Field("Revenue", FieldType.Number, "The expected revenue."),
                    Field("Cost", FieldType.Number, "The expected cost."),
                    Field("Currency", FieldType.Selection, "The currency of the figures.", Currencies),
                    Field("Confidence", FieldType.Slider, "How confident the forecast is, in percent."),
                    Field("Assumptions", FieldType.Multiline, "What the figures rest on.", details: true),
                    Field("Owner", FieldType.User, "Who prepared the forecast.")
                ],
                priorities: [],
                workflow: ReviewWorkflow(),
                calendars: [],
                slas: []),

            Class("Approval", "approval",
                fields: ApprovalFields(monetary: true),
                priorities: InvoicePriorities(),
                workflow: ApprovalWorkflow(),
                calendars: [BusinessHours],
                slas: ApprovalSlas())
        ];
    }
}
