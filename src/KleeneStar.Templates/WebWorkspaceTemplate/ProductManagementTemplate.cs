using KleeneStar.Core.WebWorkspaceTemplate;
using System.Collections.Generic;

namespace KleeneStar.Templates.WebWorkspaceTemplate
{
    /// <summary>
    /// A workspace for deciding what to build: what was asked for, what it is worth, and when it
    /// is meant to arrive.
    /// </summary>
    public sealed class ProductManagementTemplate : WorkspaceTemplateBase
    {
        /// <inheritdoc/>
        protected override string Slug => "productmanagement";

        /// <inheritdoc/>
        protected override string IconName => "pm";

        /// <inheritdoc/>
        public override string SuggestedKey => "PM";

        /// <inheritdoc/>
        public override int Order => 60;

        /// <inheritdoc/>
        public override IEnumerable<string> Categories => ["Development"];

        /// <inheritdoc/>
        public override IEnumerable<WorkspaceTemplateClass> Classes =>
        [
            Class("Feature", "feature"),
            Class("Requirement", "requirement"),
            Class("Roadmap", "roadmap"),
            Class("ReleasePlan", "releaseplan"),
            Class("UseCase", "usecase"),
            Class("Stakeholder", "stakeholder")
        ];
    }
}
