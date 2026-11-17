using KleeneStar.Core.WebWorkspaceTemplate;
using KleeneStar.Model.Entities;
using System.Collections.Generic;
using static KleeneStar.Templates.WebWorkspaceTemplate.TemplateStructure;

namespace KleeneStar.Templates.WebWorkspaceTemplate
{
    /// <summary>
    /// The structures more than one template ships: a change is the same act in a service desk
    /// and in a configuration database, an approval the same decision in finance and in
    /// operations, an invoice and a contract the same paper in finance and in procurement.
    /// </summary>
    /// <remarks>
    /// Each is a set of factories rather than shared instances, so a template that wants to
    /// add to one gets its own copy to add to.
    /// </remarks>
    public static class SharedStructures
    {
        /// <summary>
        /// Gets the ITIL change types.
        /// </summary>
        public static readonly string[] ChangeTypes = ["Standard", "Normal", "Emergency"];

        /// <summary>
        /// Gets the ITIL impact scale, from the widest to the narrowest.
        /// </summary>
        public static readonly string[] ImpactScale = ["1 - Extensive", "2 - Significant", "3 - Moderate", "4 - Minor"];

        /// <summary>
        /// Gets the ITIL urgency scale, from the most to the least urgent.
        /// </summary>
        public static readonly string[] UrgencyScale = ["1 - Critical", "2 - High", "3 - Medium", "4 - Low"];

        /// <summary>
        /// Gets the currencies a monetary field offers.
        /// </summary>
        public static readonly string[] Currencies = ["EUR", "USD", "GBP", "CHF"];

        /// <summary>
        /// Gets the payment terms a monetary document offers.
        /// </summary>
        public static readonly string[] PaymentTerms = ["Due on receipt", "Net 14", "Net 30", "Net 60", "Net 90"];

        /// <summary>
        /// Returns the fields of an ITIL change (change enablement): what changes, why, how it is
        /// done and undone, and how risky it is.
        /// </summary>
        public static IReadOnlyList<WorkspaceTemplateField> ChangeFields() =>
        [
            Field("Change Type", FieldType.Radio, "Standard: pre-authorized and low risk. Normal: assessed and authorized by the change authority. Emergency: expedited to restore or protect a service.", ChangeTypes, required: true),
            Field("Reason for Change", FieldType.Multiline, "The business reason for the change and what happens if it is not made.", required: true),
            Field("Risk", FieldType.Selection, "The assessed risk of the change.", ["Low", "Medium", "High", "Very high"]),
            Field("Impact", FieldType.Selection, "How widely the change affects services and users.", ImpactScale),
            Field("Affected Service", FieldType.Text, "The service the change is made to."),
            Field("Configuration Item", FieldType.Reference, "The configuration item the change is made to."),
            Field("Planned Start", FieldType.Date, "When implementation is planned to start."),
            Field("Planned End", FieldType.Date, "When implementation is planned to be finished."),
            Field("Change Manager", FieldType.User, "Who is accountable for the change.", onCreate: false),
            Field("CAB Required", FieldType.Boolean, "Whether the change advisory board has to authorize the change.", details: true),
            Field("Implementation Plan", FieldType.Multiline, "The steps that carry out the change.", details: true),
            Field("Backout Plan", FieldType.Multiline, "How the change is undone if it fails.", details: true),
            Field("Test Plan", FieldType.Multiline, "How success of the change is verified.", details: true),
            Field("Post-Implementation Review", FieldType.Multiline, "Whether the change achieved its goal, and what was learned.", details: true, onCreate: false)
        ];

        /// <summary>
        /// Returns the ITIL change lifecycle: assessed, authorized, scheduled, implemented and
        /// reviewed before it is closed.
        /// </summary>
        public static WorkspaceTemplateWorkflow ChangeWorkflow() => Workflow
        (
            "Change enablement",
            "The ITIL change lifecycle: from the request through assessment, authorization and implementation to the review.",
            [
                Status("New", WorkspaceTemplateStatus.ToDo, "Requested, not yet assessed."),
                Status("Assessment", WorkspaceTemplateStatus.InProgress, "Risk, impact and plans are being assessed."),
                Status("Authorization", WorkspaceTemplateStatus.Waiting, "Waiting for the change authority or the CAB."),
                Status("Scheduled", WorkspaceTemplateStatus.ToDo, "Authorized and planned into the change schedule."),
                Status("Implementation", WorkspaceTemplateStatus.InProgress, "Being carried out."),
                Status("Review", WorkspaceTemplateStatus.InProgress, "Implemented; the outcome is being reviewed."),
                Status("Closed", WorkspaceTemplateStatus.Done, "Reviewed and closed.", end: true),
                Status("Rejected", WorkspaceTemplateStatus.Done, "Not authorized.", end: true),
                Status("Cancelled", WorkspaceTemplateStatus.Done, "Withdrawn before implementation.", end: true)
            ],
            [
                Transition("Assess", "New", "Assessment"),
                Transition("Request authorization", "Assessment", "Authorization"),
                Transition("Authorize", "Authorization", "Scheduled"),
                Transition("Reject", "Authorization", "Rejected"),
                Transition("Rework", "Authorization", "Assessment"),
                Transition("Implement", "Scheduled", "Implementation"),
                Transition("Complete", "Implementation", "Review"),
                Transition("Roll back", "Implementation", "Review"),
                Transition("Close", "Review", "Closed"),
                Transition("Cancel", "New", "Cancelled"),
                Transition("Cancel", "Assessment", "Cancelled"),
                Transition("Cancel", "Scheduled", "Cancelled")
            ]
        );

        /// <summary>
        /// Returns the change priority scale.
        /// </summary>
        public static IReadOnlyList<WorkspaceTemplatePriority> ChangePriorities() =>
        [
            Priority("Critical", "Needed to restore or protect a service now; handled as an emergency change."),
            Priority("High", "Needed soon to avoid significant impact."),
            Priority("Medium", "Planned into the next regular change window."),
            Priority("Low", "Implemented when convenient.")
        ];

        /// <summary>
        /// Returns the agreements of a change: an emergency change is authorized within hours,
        /// around the clock; a normal one within a working week.
        /// </summary>
        public static IReadOnlyList<WorkspaceTemplateSla> ChangeSlas() =>
        [
            Sla
            (
                "Emergency change",
                "An emergency change is authorized by the emergency change authority within four hours, day and night.",
                SlaPriority.Critical,
                AlwaysOnCalendar,
                [
                    Target("Authorization", SlaTargetKind.Approval, 4, SlaTargetUnit.Hours),
                    Target("Implementation", SlaTargetKind.Implementation, 24, SlaTargetUnit.Hours)
                ],
                [ForPriority("Critical")],
                [Escalate(2, SlaTargetUnit.Hours, "Change Manager"), Escalate(3, SlaTargetUnit.Hours, "Head of IT Operations")],
                // the clock measures the authorization, so it does not stop while one is awaited
                pauseOn: []
            ),
            Sla
            (
                "Normal change",
                "A normal change is authorized within five working days of its assessment.",
                SlaPriority.Medium,
                BusinessHoursCalendar,
                [Target("Authorization", SlaTargetKind.Approval, 5, SlaTargetUnit.BusinessDays)],
                [ForPriority("High"), ForPriority("Medium"), ForPriority("Low")],
                pauseOn: []
            )
        ];

        /// <summary>
        /// Returns the fields of an approval: what is to be decided, by whom, and the decision.
        /// </summary>
        /// <param name="monetary">Whether the approval is about an amount of money.</param>
        public static IReadOnlyList<WorkspaceTemplateField> ApprovalFields(bool monetary)
        {
            var fields = new List<WorkspaceTemplateField>
            {
                Field("Subject", FieldType.Reference, "The item the approval is about.", required: true),
                Field("Approver", FieldType.User, "Who decides.", required: true),
                Field("Justification", FieldType.Multiline, "Why the approval is requested.")
            };

            if (monetary)
            {
                fields.Add(Field("Amount", FieldType.Number, "The amount to be approved.", required: true));
                fields.Add(Field("Currency", FieldType.Selection, "The currency of the amount.", Currencies));
            }

            fields.Add(Field("Decision", FieldType.Radio, "What was decided.", ["Approved", "Rejected", "Deferred"], onCreate: false));
            fields.Add(Field("Decision Date", FieldType.Date, "When it was decided.", onCreate: false));
            fields.Add(Field("Comments", FieldType.Multiline, "The reasoning behind the decision.", onCreate: false, details: true));

            return fields;
        }

        /// <summary>
        /// Returns the approval lifecycle: requested, then decided one way or the other.
        /// </summary>
        public static WorkspaceTemplateWorkflow ApprovalWorkflow() => Workflow
        (
            "Approval",
            "Requested, then approved or rejected.",
            [
                Status("Requested", WorkspaceTemplateStatus.ToDo, "Waiting for the approver's decision."),
                Status("More Information", WorkspaceTemplateStatus.Waiting, "The approver asked the requester for more information."),
                Status("Approved", WorkspaceTemplateStatus.Done, "The approver agreed.", end: true),
                Status("Rejected", WorkspaceTemplateStatus.Done, "The approver declined.", end: true)
            ],
            [
                Transition("Approve", "Requested", "Approved"),
                Transition("Reject", "Requested", "Rejected"),
                Transition("Ask for information", "Requested", "More Information"),
                Transition("Resubmit", "More Information", "Requested")
            ]
        );

        /// <summary>
        /// Returns the agreement of an approval: decided within two working days.
        /// </summary>
        public static IReadOnlyList<WorkspaceTemplateSla> ApprovalSlas() =>
        [
            Sla
            (
                "Approval decision",
                "An approval is decided within two working days.",
                SlaPriority.Medium,
                BusinessHoursCalendar,
                [Target("Decision", SlaTargetKind.Approval, 2, SlaTargetUnit.BusinessDays)],
                escalations: [Escalate(1, SlaTargetUnit.BusinessDays, "Approver's manager")]
            )
        ];

        /// <summary>
        /// Returns the fields of an incoming invoice.
        /// </summary>
        /// <param name="threeWayMatch">Whether the invoice is matched against an order and its
        /// delivery, as procurement does.</param>
        public static IReadOnlyList<WorkspaceTemplateField> InvoiceFields(bool threeWayMatch)
        {
            var fields = new List<WorkspaceTemplateField>
            {
                Field("Invoice Number", FieldType.Text, "The number the supplier gave the invoice.", required: true),
                Field("Supplier", FieldType.Text, "Who issued the invoice.", required: true),
                Field("Invoice Date", FieldType.Date, "The date on the invoice.", required: true),
                Field("Due Date", FieldType.Date, "When the invoice has to be paid."),
                Field("Net Amount", FieldType.Number, "The amount before tax.", required: true),
                Field("Tax Rate", FieldType.Selection, "The VAT rate applied.", ["0%", "7%", "19%"]),
                Field("Gross Amount", FieldType.Number, "The amount including tax."),
                Field("Currency", FieldType.Selection, "The currency of the invoice.", Currencies),
                Field("Payment Terms", FieldType.Selection, "The agreed payment terms.", PaymentTerms, details: true),
                Field("Cost Center", FieldType.Text, "Where the cost is booked.", details: true),
                Field("Invoice Document", FieldType.Attachment, "The scanned or electronic invoice.")
            };

            if (threeWayMatch)
            {
                fields.Add(Field("Purchase Order", FieldType.Reference, "The order the invoice bills."));
                fields.Add(Field("Delivery", FieldType.Reference, "The delivery the invoice bills.", details: true));
                fields.Add(Field("Matched", FieldType.Boolean, "Whether invoice, order and goods receipt agree (three-way match).", onCreate: false));
            }

            return fields;
        }

        /// <summary>
        /// Returns the invoice lifecycle: checked, approved and paid - or disputed.
        /// </summary>
        public static WorkspaceTemplateWorkflow InvoiceWorkflow() => Workflow
        (
            "Invoice processing",
            "From receipt through verification and approval to payment.",
            [
                Status("Received", WorkspaceTemplateStatus.ToDo, "Received, not yet checked."),
                Status("Verification", WorkspaceTemplateStatus.InProgress, "Checked for correctness against order and delivery."),
                Status("Approval", WorkspaceTemplateStatus.Waiting, "Waiting for approval of payment."),
                Status("Disputed", WorkspaceTemplateStatus.Waiting, "Queried with the supplier."),
                Status("Approved for Payment", WorkspaceTemplateStatus.InProgress, "Approved and scheduled for payment."),
                Status("Paid", WorkspaceTemplateStatus.Done, "The payment has been made.", end: true),
                Status("Rejected", WorkspaceTemplateStatus.Done, "Rejected and returned to the supplier.", end: true)
            ],
            [
                Transition("Verify", "Received", "Verification"),
                Transition("Submit for approval", "Verification", "Approval"),
                Transition("Dispute", "Verification", "Disputed"),
                Transition("Resolve dispute", "Disputed", "Verification"),
                Transition("Approve", "Approval", "Approved for Payment"),
                Transition("Reject", "Approval", "Rejected"),
                Transition("Reject", "Disputed", "Rejected"),
                Transition("Pay", "Approved for Payment", "Paid")
            ]
        );

        /// <summary>
        /// Returns the invoice priority scale.
        /// </summary>
        public static IReadOnlyList<WorkspaceTemplatePriority> InvoicePriorities() =>
        [
            Priority("Urgent", "Due shortly or eligible for an early-payment discount."),
            Priority("Standard", "Processed in the usual payment run.")
        ];

        /// <summary>
        /// Returns the agreement of an invoice: approved within five working days, so it is paid
        /// within its terms.
        /// </summary>
        public static IReadOnlyList<WorkspaceTemplateSla> InvoiceSlas() =>
        [
            Sla
            (
                "Invoice processing",
                "An invoice is verified and approved within five working days and paid within thirty days.",
                SlaPriority.Medium,
                BusinessHoursCalendar,
                [
                    Target("Approval", SlaTargetKind.Approval, 5, SlaTargetUnit.BusinessDays),
                    Target("Payment", SlaTargetKind.Resolution, 30, SlaTargetUnit.Days)
                ],
                // waiting for the approval is what the clock measures; only a dispute with the
                // supplier stops it
                pauseOn: ["Disputed"]
            )
        ];

        /// <summary>
        /// Returns the fields of a contract.
        /// </summary>
        /// <param name="counterparty">What the other party is called - customer, supplier.</param>
        public static IReadOnlyList<WorkspaceTemplateField> ContractFields(string counterparty) =>
        [
            Field("Contract Number", FieldType.Text, "The reference of the contract.", required: true),
            Field(counterparty, FieldType.Text, "The other party to the contract.", required: true),
            Field("Contract Owner", FieldType.User, "Who is accountable for the contract."),
            Field("Start Date", FieldType.Date, "When the contract takes effect.", required: true),
            Field("End Date", FieldType.Date, "When the contract ends, unless renewed."),
            Field("Notice Period", FieldType.Selection, "How long before the end it has to be terminated.", ["1 month", "3 months", "6 months", "12 months"]),
            Field("Auto Renewal", FieldType.Boolean, "Whether the contract renews unless terminated."),
            Field("Annual Value", FieldType.Number, "What the contract is worth per year."),
            Field("Currency", FieldType.Selection, "The currency of the value.", Currencies),
            Field("Payment Terms", FieldType.Selection, "The agreed payment terms.", PaymentTerms, details: true),
            Field("Contract Document", FieldType.Attachment, "The signed contract.", details: true)
        ];

        /// <summary>
        /// Returns the contract lifecycle: negotiated, in force, and ended one way or another.
        /// </summary>
        public static WorkspaceTemplateWorkflow ContractWorkflow() => Workflow
        (
            "Contract lifecycle",
            "From the draft through negotiation and signature to its end.",
            [
                Status("Draft", WorkspaceTemplateStatus.ToDo, "Being drafted."),
                Status("Negotiation", WorkspaceTemplateStatus.InProgress, "Terms are being negotiated with the other party."),
                Status("Signature", WorkspaceTemplateStatus.Waiting, "Agreed; waiting for signatures."),
                Status("Active", WorkspaceTemplateStatus.InProgress, "Signed and in force."),
                Status("Expired", WorkspaceTemplateStatus.Done, "Ended at its end date.", end: true),
                Status("Terminated", WorkspaceTemplateStatus.Done, "Ended by notice.", end: true)
            ],
            [
                Transition("Negotiate", "Draft", "Negotiation"),
                Transition("Send for signature", "Negotiation", "Signature"),
                Transition("Renegotiate", "Signature", "Negotiation"),
                Transition("Sign", "Signature", "Active"),
                Transition("Expire", "Active", "Expired"),
                Transition("Terminate", "Active", "Terminated"),
                Transition("Abandon", "Negotiation", "Terminated")
            ]
        );
    }
}
