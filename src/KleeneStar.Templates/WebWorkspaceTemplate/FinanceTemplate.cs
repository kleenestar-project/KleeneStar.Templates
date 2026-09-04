using KleeneStar.Core.WebWorkspaceTemplate;
using System.Collections.Generic;

namespace KleeneStar.Templates.WebWorkspaceTemplate
{
    /// <summary>
    /// A workspace for money: what was planned, what was spent, and who agreed to it.
    /// </summary>
    public sealed class FinanceTemplate : WorkspaceTemplateBase
    {
        /// <inheritdoc/>
        protected override string Slug => "finance";

        /// <inheritdoc/>
        protected override string IconName => "fin";

        /// <inheritdoc/>
        public override string SuggestedKey => "FIN";

        /// <inheritdoc/>
        public override int Order => 40;

        /// <inheritdoc/>
        public override IEnumerable<string> Categories => ["Finance"];

        /// <inheritdoc/>
        public override IEnumerable<WorkspaceTemplateClass> Classes =>
        [
            Class("Budget", "budget"),
            Class("Invoice", "invoice"),
            Class("CostCenter", "costcenter"),
            Class("Contract", "contract"),
            Class("Forecast", "forecast"),
            Class("Approval", "approval")
        ];
    }
}
