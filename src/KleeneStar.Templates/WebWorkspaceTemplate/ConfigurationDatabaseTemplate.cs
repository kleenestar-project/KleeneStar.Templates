using KleeneStar.Core.WebWorkspaceTemplate;
using KleeneStar.Model.Entities;
using System.Collections.Generic;

namespace KleeneStar.Templates.WebWorkspaceTemplate
{
    /// <summary>
    /// A workspace for keeping track of what an installation is made of: the assets, how they
    /// depend on each other, and what has to be true about them.
    /// </summary>
    /// <remarks>
    /// Its central class holds assets rather than issues, which is what distinguishes a
    /// configuration database from a work-item workspace: the objects are the things themselves,
    /// and the issues beside them are what happened to those things.
    /// </remarks>
    public sealed class ConfigurationDatabaseTemplate : WorkspaceTemplateBase
    {
        /// <inheritdoc/>
        protected override string Slug => "cmdb";

        /// <inheritdoc/>
        public override string SuggestedKey => "CMDB";

        /// <inheritdoc/>
        public override int Order => 30;

        /// <inheritdoc/>
        public override IEnumerable<string> Categories => ["Infrastructure", "Compliance"];

        /// <inheritdoc/>
        public override IEnumerable<WorkspaceTemplateClass> Classes =>
        [
            Class("Asset", "asset", ObjectKind.Asset),
            Class("Relationship", "rel"),
            Class("ChangeRequest", "change"),
            Class("Vulnerability", "vuln"),
            Class("Compliance", "compliance"),
            Class("Policy", "policy", @sealed: true),
            Class("Approval", "approval")
        ];
    }
}
