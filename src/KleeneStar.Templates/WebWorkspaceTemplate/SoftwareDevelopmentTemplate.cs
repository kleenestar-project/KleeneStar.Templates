using KleeneStar.Core.WebWorkspaceTemplate;
using KleeneStar.Model.Entities;
using System.Collections.Generic;

namespace KleeneStar.Templates.WebWorkspaceTemplate
{
    /// <summary>
    /// A workspace for building software: where the code lives, what is being built from it, and
    /// what still has to be done to it.
    /// </summary>
    /// <remarks>
    /// It is the one template whose classes span three object kinds - the work items are issues,
    /// the specification is a document, and a release is announced as a post - which makes it the
    /// useful example of a workspace that is more than a ticket list.
    /// </remarks>
    public sealed class SoftwareDevelopmentTemplate : WorkspaceTemplateBase
    {
        /// <inheritdoc/>
        protected override string Slug => "development";

        /// <inheritdoc/>
        protected override string IconName => "dev";

        /// <inheritdoc/>
        public override string SuggestedKey => "DEV";

        /// <inheritdoc/>
        public override int Order => 20;

        /// <inheritdoc/>
        public override IEnumerable<string> Categories => ["Engineering"];

        /// <inheritdoc/>
        public override IEnumerable<WorkspaceTemplateClass> Classes =>
        [
            Class("Task", "task"),
            Class("Bug", "bug"),
            Class("Sprint", "sprint"),
            Class("Repository", "repo"),
            Class("BuildPipeline", "build"),
            Class("Documentation", "doc", ObjectKind.Document),
            Class("Release", "release", ObjectKind.Blog)
        ];
    }
}
