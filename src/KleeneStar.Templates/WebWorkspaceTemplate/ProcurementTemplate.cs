using KleeneStar.Core.WebWorkspaceTemplate;
using KleeneStar.Model.Entities;
using System.Collections.Generic;
using static KleeneStar.Templates.WebWorkspaceTemplate.SharedStructures;
using static KleeneStar.Templates.WebWorkspaceTemplate.TemplateStructure;

namespace KleeneStar.Templates.WebWorkspaceTemplate
{
    /// <summary>
    /// A workspace for buying things: what was ordered, from whom, and whether it arrived.
    /// </summary>
    /// <remarks>
    /// The structure follows the purchase-to-pay process: a supplier is qualified before it is
    /// ordered from, a tender awards the order where the value calls for one, a purchase order
    /// is approved and then delivered - possibly in parts - and the goods receipt is inspected.
    /// The invoice is the one finance keeps as well, plus the three-way match against order and
    /// delivery.
    /// </remarks>
    public sealed class ProcurementTemplate : WorkspaceTemplateBase
    {
        /// <summary>
        /// Gets the short name of the template, <c>procurement</c>, from which its key and its i18n
        /// keys are derived.
        /// </summary>
        protected override string Slug => "procurement";

        /// <summary>
        /// Gets the file name of the icon, <c>proc</c> - the icon of the seeded procurement workspace.
        /// </summary>
        protected override string IconName => "proc";

        /// <summary>
        /// Gets the key proposed for a new procurement workspace, <c>PROC</c>.
        /// </summary>
        public override string SuggestedKey => "PROC";

        /// <summary>
        /// Gets the display order: last of the shipped templates.
        /// </summary>
        public override int Order => 70;

        /// <summary>
        /// Gets the category the workspace is filed under, operations.
        /// </summary>
        public override IEnumerable<string> Categories => ["Operations"];

        /// <summary>
        /// The categories of what is bought.
        /// </summary>
        private static readonly string[] PurchaseCategories =
        [
            "IT hardware", "Software & licenses", "Office supplies", "Services", "Facilities", "Raw materials", "Other"
        ];

        /// <summary>
        /// Gets the classes the workspace starts with, along the purchase-to-pay process: purchase
        /// orders, suppliers, contracts, tenders, deliveries and invoices - each with its structure.
        /// </summary>
        public override IEnumerable<WorkspaceTemplateClass> Classes =>
        [
            Class("PurchaseOrder", "po",
                fields:
                [
                    Field("PO Number", FieldType.Text, "The number of the purchase order.", required: true),
                    Field("Supplier", FieldType.Reference, "Who the order is placed with.", required: true),
                    Field("Category", FieldType.Selection, "What is bought.", PurchaseCategories),
                    Field("Net Amount", FieldType.Number, "The order value before tax.", required: true),
                    Field("Currency", FieldType.Selection, "The currency of the order.", Currencies),
                    Field("Cost Center", FieldType.Text, "Where the cost is booked."),
                    Field("Buyer", FieldType.User, "Who places and follows up the order."),
                    Field("Requested Delivery", FieldType.Date, "When the goods are needed."),
                    Field("Incoterms", FieldType.Selection, "Who bears transport and risk.", ["EXW", "FCA", "CPT", "DAP", "DDP"], details: true),
                    Field("Payment Terms", FieldType.Selection, "The agreed payment terms.", PaymentTerms, details: true),
                    Field("Order Lines", FieldType.Multiline, "What is ordered, in which quantity, at which price.", details: true)
                ],
                priorities:
                [
                    Priority("Urgent", "Needed immediately; operations are affected."),
                    Priority("High", "Needed soon."),
                    Priority("Normal", "Ordered in the usual lead time."),
                    Priority("Low", "No particular deadline.")
                ],
                workflow: Workflow
                (
                    "Purchase Order",
                    "Drafted, approved, ordered, delivered and closed.",
                    [
                        Status("Draft", WorkspaceTemplateStatus.ToDo, "Being drafted."),
                        Status("Approval", WorkspaceTemplateStatus.Waiting, "Waiting for approval."),
                        Status("Ordered", WorkspaceTemplateStatus.InProgress, "Sent to the supplier."),
                        Status("Partially Delivered", WorkspaceTemplateStatus.InProgress, "Part of the order has arrived."),
                        Status("Delivered", WorkspaceTemplateStatus.Done, "Everything has arrived."),
                        Status("Closed", WorkspaceTemplateStatus.Done, "Delivered and invoiced.", end: true),
                        Status("Cancelled", WorkspaceTemplateStatus.Done, "Withdrawn.", end: true)
                    ],
                    [
                        Transition("Submit", "Draft", "Approval"),
                        Transition("Approve", "Approval", "Ordered"),
                        Transition("Return", "Approval", "Draft"),
                        Transition("Partial delivery", "Ordered", "Partially Delivered"),
                        Transition("Complete delivery", "Ordered", "Delivered"),
                        Transition("Complete delivery", "Partially Delivered", "Delivered"),
                        Transition("Close", "Delivered", "Closed"),
                        Transition("Cancel", "Draft", "Cancelled"),
                        Transition("Cancel", "Approval", "Cancelled"),
                        Transition("Cancel", "Ordered", "Cancelled")
                    ]
                ),
                calendars: [BusinessHours],
                slas:
                [
                    Sla
                    (
                        "Order approval",
                        "A purchase order is approved within two working days.",
                        SlaPriority.Medium,
                        BusinessHoursCalendar,
                        [Target("Approval", SlaTargetKind.Approval, 2, SlaTargetUnit.BusinessDays)],
                        [ForPriority("High"), ForPriority("Normal"), ForPriority("Low")],
                        pauseOn: []
                    ),
                    Sla
                    (
                        "Urgent order approval",
                        "An urgent purchase order is approved within four working hours.",
                        SlaPriority.High,
                        BusinessHoursCalendar,
                        [Target("Approval", SlaTargetKind.Approval, 4, SlaTargetUnit.Hours)],
                        [ForPriority("Urgent")],
                        pauseOn: []
                    )
                ]),

            Class("Supplier", "supplier",
                fields:
                [
                    Field("Supplier Number", FieldType.Text, "The supplier's number in the ERP.", required: true),
                    Field("Category", FieldType.MultiSelection, "What the supplier delivers.", PurchaseCategories),
                    Field("Contact Person", FieldType.Text, "Who to talk to at the supplier."),
                    Field("Email", FieldType.Text, "The contact's e-mail address."),
                    Field("Phone", FieldType.Text, "The contact's phone number."),
                    Field("Country", FieldType.Text, "Where the supplier is based."),
                    Field("Rating", FieldType.Rating, "How well the supplier performs.", onCreate: false),
                    Field("Certifications", FieldType.MultiSelection, "The certifications the supplier holds.", ["ISO 9001", "ISO 14001", "ISO/IEC 27001", "ISO 45001"], details: true),
                    Field("Payment Terms", FieldType.Selection, "The standard payment terms.", PaymentTerms, details: true)
                ],
                priorities: [],
                workflow: Workflow
                (
                    "Supplier qualification",
                    "Qualified before it is ordered from, blocked when it fails.",
                    [
                        Status("Prospect", WorkspaceTemplateStatus.ToDo, "Known, not yet qualified."),
                        Status("Qualification", WorkspaceTemplateStatus.InProgress, "Being assessed: references, certifications, credit check."),
                        Status("Approved", WorkspaceTemplateStatus.InProgress, "May be ordered from."),
                        Status("Blocked", WorkspaceTemplateStatus.Waiting, "Temporarily not to be ordered from."),
                        Status("Inactive", WorkspaceTemplateStatus.Done, "No longer a supplier.", end: true)
                    ],
                    [
                        Transition("Qualify", "Prospect", "Qualification"),
                        Transition("Approve", "Qualification", "Approved"),
                        Transition("Decline", "Qualification", "Inactive"),
                        Transition("Block", "Approved", "Blocked"),
                        Transition("Unblock", "Blocked", "Approved"),
                        Transition("Deactivate", "Approved", "Inactive")
                    ]
                ),
                calendars: [],
                slas: []),

            Class("Contract", "contract",
                fields: ContractFields("Supplier"),
                workflow: ContractWorkflow(),
                calendars: [],
                slas: []),

            Class("Tender", "tender",
                fields:
                [
                    Field("Tender Number", FieldType.Text, "The reference of the tender.", required: true),
                    Field("Procedure", FieldType.Selection, "The award procedure.", ["Open", "Restricted", "Negotiated", "Direct award"]),
                    Field("Estimated Value", FieldType.Number, "The expected value of the contract."),
                    Field("Currency", FieldType.Selection, "The currency of the value.", Currencies),
                    Field("Submission Deadline", FieldType.Date, "Until when bids are accepted.", required: true),
                    Field("Evaluation Criteria", FieldType.Multiline, "How the bids are weighed."),
                    Field("Bids Received", FieldType.Number, "How many bids came in.", onCreate: false),
                    Field("Awarded Supplier", FieldType.Reference, "Who won the tender.", onCreate: false),
                    Field("Tender Documents", FieldType.Attachment, "The call for tenders and its annexes.", details: true)
                ],
                workflow: Workflow
                (
                    "Tender",
                    "Prepared, published, evaluated and awarded.",
                    [
                        Status("Preparation", WorkspaceTemplateStatus.ToDo, "The call for tenders is being written."),
                        Status("Published", WorkspaceTemplateStatus.Waiting, "Published; waiting for bids."),
                        Status("Evaluation", WorkspaceTemplateStatus.InProgress, "The bids are being evaluated."),
                        Status("Awarded", WorkspaceTemplateStatus.Done, "Awarded to a supplier.", end: true),
                        Status("Cancelled", WorkspaceTemplateStatus.Done, "Cancelled without award.", end: true)
                    ],
                    [
                        Transition("Publish", "Preparation", "Published"),
                        Transition("Close bidding", "Published", "Evaluation"),
                        Transition("Award", "Evaluation", "Awarded"),
                        Transition("Cancel", "Published", "Cancelled"),
                        Transition("Cancel", "Evaluation", "Cancelled")
                    ]
                ),
                calendars: [],
                slas: []),

            Class("Delivery", "delivery",
                fields:
                [
                    Field("Purchase Order", FieldType.Reference, "The order the delivery belongs to.", required: true),
                    Field("Delivery Note", FieldType.Text, "The number of the supplier's delivery note."),
                    Field("Delivery Date", FieldType.Date, "When the goods arrived.", required: true),
                    Field("Received By", FieldType.User, "Who accepted the goods."),
                    Field("Quantity", FieldType.Number, "How much arrived."),
                    Field("Condition", FieldType.Radio, "In which condition the goods arrived.", ["Complete and undamaged", "Incomplete", "Damaged"], onCreate: false),
                    Field("Goods Receipt", FieldType.Attachment, "The signed delivery note or goods receipt.", details: true)
                ],
                priorities: [],
                workflow: Workflow
                (
                    "Goods Receipt",
                    "Expected, received, inspected - and accepted or returned.",
                    [
                        Status("Expected", WorkspaceTemplateStatus.ToDo, "Announced by the supplier."),
                        Status("Received", WorkspaceTemplateStatus.InProgress, "Arrived; not yet inspected."),
                        Status("Inspection", WorkspaceTemplateStatus.InProgress, "Being checked for quantity and quality."),
                        Status("Accepted", WorkspaceTemplateStatus.Done, "Accepted and booked into stock.", end: true),
                        Status("Returned", WorkspaceTemplateStatus.Done, "Sent back to the supplier.", end: true)
                    ],
                    [
                        Transition("Receive", "Expected", "Received"),
                        Transition("Inspect", "Received", "Inspection"),
                        Transition("Accept", "Inspection", "Accepted"),
                        Transition("Return", "Inspection", "Returned")
                    ]
                ),
                calendars: [BusinessHours],
                slas:
                [
                    Sla
                    (
                        "Goods receipt inspection",
                        "A delivery is inspected within two working days of its arrival, inside the notice period for defects.",
                        SlaPriority.Medium,
                        BusinessHoursCalendar,
                        [Target("Inspection", SlaTargetKind.Resolution, 2, SlaTargetUnit.BusinessDays)]
                    )
                ]),

            Class("Invoice", "invoice",
                fields: InvoiceFields(threeWayMatch: true),
                priorities: InvoicePriorities(),
                workflow: InvoiceWorkflow(),
                calendars: [BusinessHours],
                slas: InvoiceSlas())
        ];
    }
}
