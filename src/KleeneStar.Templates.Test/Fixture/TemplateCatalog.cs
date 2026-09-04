using KleeneStar.Core.WebWorkspaceTemplate;
using System.Reflection;

namespace KleeneStar.Templates.Test
{
    /// <summary>
    /// The templates this plugin ships, found the way the core's
    /// <see cref="KleeneStar.Core.WebManager.WorkspaceTemplateManager"/> finds them.
    /// </summary>
    /// <remarks>
    /// The selection is deliberately the manager's own - public, concrete, assignable to
    /// <see cref="IWorkspaceTemplate"/> - rather than a hand-written list of the seven classes.
    /// A template that stops satisfying that filter stops being offered, silently and without a
    /// build error, so the tests have to ask the same question the manager asks: a list written
    /// out here would keep answering with a template the installation no longer sees.
    /// </remarks>
    internal static class TemplateCatalog
    {
        /// <summary>
        /// The keys of the templates shipped in this plugin.
        /// </summary>
        /// <remarks>
        /// A key is what a created workspace records itself as coming from, so it outlives
        /// renames of the class that defines it - which makes the set of them a contract with
        /// every installation out there, and worth writing down once where a change to it has to
        /// be deliberate.
        /// </remarks>
        public static readonly string[] ShippedKeys =
        [
            "kleenestar.templates.servicedesk",
            "kleenestar.templates.development",
            "kleenestar.templates.cmdb",
            "kleenestar.templates.finance",
            "kleenestar.templates.humanresources",
            "kleenestar.templates.productmanagement",
            "kleenestar.templates.procurement"
        ];

        /// <summary>
        /// Gets the assembly the templates live in - the plugin's own, not the test's.
        /// </summary>
        public static Assembly Assembly { get; } = typeof(KleeneStarPlugin).Assembly;

        /// <summary>
        /// Gets the types the manager would discover, in a stable order.
        /// </summary>
        public static IReadOnlyList<Type> Types { get; } =
        [
            .. Assembly
                .GetTypes()
                .Where(x => x.IsClass && !x.IsAbstract && x.IsPublic)
                .Where(x => typeof(IWorkspaceTemplate).IsAssignableFrom(x))
                .OrderBy(x => x.FullName, StringComparer.Ordinal)
        ];

        /// <summary>
        /// Gets the instantiated templates, in the same order as <see cref="Types"/>.
        /// </summary>
        public static IReadOnlyList<IWorkspaceTemplate> Templates { get; } =
            [.. Types.Select(x => (IWorkspaceTemplate)Activator.CreateInstance(x))];

        /// <summary>
        /// Returns the shipped template with the supplied key.
        /// </summary>
        /// <param name="key">The stable key of the template.</param>
        /// <returns>The template.</returns>
        public static IWorkspaceTemplate ByKey(string key)
        {
            return Templates.Single(x => x.Key == key);
        }

        /// <summary>
        /// Returns the slug a template's key is built from, e.g. <c>servicedesk</c>.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns>The slug.</returns>
        public static string SlugOf(IWorkspaceTemplate template)
        {
            return template.Key["kleenestar.templates.".Length..];
        }
    }
}
