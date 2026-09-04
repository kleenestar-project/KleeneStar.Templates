using KleeneStar.Core.WebWorkspaceTemplate;
using System.Collections.Generic;

namespace KleeneStar.Templates.WebWorkspaceTemplate
{
    /// <summary>
    /// A workspace for the people of an organization: who they are, where they belong, and what
    /// is arranged for them.
    /// </summary>
    public sealed class HumanResourcesTemplate : WorkspaceTemplateBase
    {
        /// <inheritdoc/>
        protected override string Slug => "humanresources";

        /// <inheritdoc/>
        protected override string IconName => "hr";

        /// <inheritdoc/>
        public override string SuggestedKey => "HR";

        /// <inheritdoc/>
        public override int Order => 50;

        /// <inheritdoc/>
        public override IEnumerable<string> Categories => ["HumanResources"];

        /// <inheritdoc/>
        public override IEnumerable<WorkspaceTemplateClass> Classes =>
        [
            Class("Employee", "employee"),
            Class("OrganizationUnit", "orgunit"),
            Class("Position", "position"),
            Class("Onboarding", "onboarding"),
            Class("Absence", "absence"),
            Class("Training", "training")
        ];
    }
}
