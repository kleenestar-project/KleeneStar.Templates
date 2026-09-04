using KleeneStar.Core.WebWorkspaceTemplate;
using KleeneStar.Model.Entities;
using System.Collections.Generic;
using WebExpress.WebCore.WebIcon;
using WebExpress.WebUI.WebIcon;

namespace KleeneStar.Templates.WebWorkspaceTemplate
{
    /// <summary>
    /// The common shape of the templates shipped here: the naming convention that ties a
    /// template's key to its translations, and a shorthand for declaring a class.
    /// </summary>
    /// <remarks>
    /// Every template in this plugin names itself once, in <see cref="Slug"/>, and everything
    /// else follows from it - the key, the two i18n keys and the icon path. A template that
    /// spelled all four out would let them drift apart, and a drifted i18n key shows up as the
    /// raw key on a card rather than as an error.
    /// </remarks>
    public abstract class WorkspaceTemplateBase : IWorkspaceTemplate
    {
        /// <summary>
        /// Gets the short name of the template, e.g. <c>servicedesk</c>. Lower case, no dots.
        /// </summary>
        protected abstract string Slug { get; }

        /// <summary>
        /// Gets the stable key of the template.
        /// </summary>
        public string Key => "kleenestar.templates." + Slug;

        /// <summary>
        /// Gets the internationalization key of the display name.
        /// </summary>
        public string Name => "kleenestar.templates:template." + Slug + ".name";

        /// <summary>
        /// Gets the internationalization key of the description.
        /// </summary>
        public string Description => "kleenestar.templates:template." + Slug + ".description";

        /// <summary>
        /// Gets the icon of the template. The icons are the host's own workspace icons, so a
        /// workspace created from a template looks like the one the template was modelled on.
        /// </summary>
        public virtual IIcon Icon => ImageIcon.FromString("/kleenestar/assets/icons/" + IconName + ".svg");

        /// <summary>
        /// Gets the file name of the icon, without path or extension. Defaults to the slug.
        /// </summary>
        protected virtual string IconName => Slug;

        /// <summary>
        /// Gets the suggested key of the workspace.
        /// </summary>
        public abstract string SuggestedKey { get; }

        /// <summary>
        /// Gets the categories the workspace is filed under.
        /// </summary>
        public virtual IEnumerable<string> Categories => [];

        /// <summary>
        /// Gets the display order among the templates offered.
        /// </summary>
        public virtual int Order => 100;

        /// <summary>
        /// Gets the classes the workspace starts with.
        /// </summary>
        public abstract IEnumerable<WorkspaceTemplateClass> Classes { get; }

        /// <summary>
        /// Declares one class of the template.
        /// </summary>
        /// <param name="name">The class name.</param>
        /// <param name="icon">The file name of the icon, without path or extension.</param>
        /// <param name="kind">The kind of object the class holds.</param>
        /// <param name="portalVisible">Whether objects of the class are offered in the customer
        /// portal.</param>
        /// <param name="sealed">Whether the class may not be specialized further.</param>
        /// <returns>The declared class.</returns>
        protected WorkspaceTemplateClass Class
        (
            string name,
            string icon,
            string kind = ObjectKind.Issue,
            bool portalVisible = false,
            bool @sealed = false
        )
        {
            return new WorkspaceTemplateClass
            {
                Name = name,
                Description = "kleenestar.templates:template." + Slug + ".class." + name.ToLowerInvariant(),
                Icon = "/kleenestar/assets/icons/" + icon + ".svg",
                Kind = kind,
                PortalVisible = portalVisible,
                Sealed = @sealed
            };
        }
    }
}
