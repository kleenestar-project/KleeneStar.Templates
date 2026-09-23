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
        /// Declares one class of the template, with its structure.
        /// </summary>
        /// <remarks>
        /// Every structural part left <see langword="null"/> is taken from the defaults of the
        /// class's kind (<see cref="TemplateStructure.Resolve"/>); an empty collection says the
        /// class has none of it. <paramref name="fields"/> is merged into the base fields of the
        /// kind rather than replacing them, so a template names only what is particular to the
        /// class.
        /// </remarks>
        /// <param name="name">The class name.</param>
        /// <param name="icon">The file name of the icon, without path or extension.</param>
        /// <param name="kind">The kind of object the class holds.</param>
        /// <param name="renderer">The renderer its objects are read and written through, or null
        /// to follow the default of the kind - which is what a template that does not care
        /// leaves it at.</param>
        /// <param name="portalVisible">Whether objects of the class are offered in the customer
        /// portal.</param>
        /// <param name="sealed">Whether the class may not be specialized further.</param>
        /// <param name="fields">The fields particular to the class.</param>
        /// <param name="priorities">The priority scale, or null for the kind's.</param>
        /// <param name="workflow">The lifecycle, or null for the kind's.</param>
        /// <param name="calendars">The calendars, or null for the kind's.</param>
        /// <param name="slas">The service-level agreements, or null for the kind's.</param>
        /// <returns>The declared class.</returns>
        protected WorkspaceTemplateClass Class
        (
            string name,
            string icon,
            string kind = ObjectKind.Issue,
            string renderer = null,
            bool portalVisible = false,
            bool @sealed = false,
            IReadOnlyList<WorkspaceTemplateField> fields = null,
            IReadOnlyList<WorkspaceTemplatePriority> priorities = null,
            WorkspaceTemplateWorkflow workflow = null,
            IReadOnlyList<WorkspaceTemplateCalendar> calendars = null,
            IReadOnlyList<WorkspaceTemplateSla> slas = null
        )
        {
            return TemplateStructure.Resolve
            (
                new WorkspaceTemplateClass
                {
                    Name = name,
                    Description = "kleenestar.templates:template." + Slug + ".class." + name.ToLowerInvariant(),
                    Icon = "/kleenestar/assets/icons/" + icon + ".svg",
                    Kind = kind,
                    Renderer = renderer,
                    PortalVisible = portalVisible,
                    Sealed = @sealed
                },
                fields,
                priorities,
                workflow,
                calendars,
                slas
            );
        }
    }
}
