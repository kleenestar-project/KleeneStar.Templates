using KleeneStar.Core.WebWorkspaceTemplate;
using System.Collections.Generic;

namespace KleeneStar.Templates.WebWorkspaceTemplate
{
    /// <summary>
    /// A workspace for buying things: what was ordered, from whom, and whether it arrived.
    /// </summary>
    public sealed class ProcurementTemplate : WorkspaceTemplateBase
    {
        /// <inheritdoc/>
        protected override string Slug => "procurement";

        /// <inheritdoc/>
        protected override string IconName => "proc";

        /// <inheritdoc/>
        public override string SuggestedKey => "PROC";

        /// <inheritdoc/>
        public override int Order => 70;

        /// <inheritdoc/>
        public override IEnumerable<string> Categories => ["Operations"];

        /// <inheritdoc/>
        public override IEnumerable<WorkspaceTemplateClass> Classes =>
        [
            Class("PurchaseOrder", "po"),
            Class("Supplier", "supplier"),
            Class("Contract", "contract"),
            Class("Tender", "tender"),
            Class("Delivery", "delivery"),
            Class("Invoice", "invoice")
        ];
    }
}
