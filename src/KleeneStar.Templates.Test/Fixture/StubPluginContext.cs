using Microsoft.Extensions.Configuration;
using System.Reflection;
using WebExpress.WebCore.WebComponent;
using WebExpress.WebCore.WebEndpoint;
using WebExpress.WebCore.WebPlugin;

namespace KleeneStar.Templates.Test
{
    /// <summary>
    /// The plugin a discovery test scans, standing in for the one the framework's plugin manager
    /// would hand the template manager.
    /// </summary>
    /// <remarks>
    /// Only <see cref="Assembly"/> carries anything: it is the single member discovery reads, and
    /// pointing it at the plugin assembly is what makes the test scan the real templates rather
    /// than a stand-in for them.
    /// </remarks>
    internal sealed class StubPluginContext : IPluginContext
    {
        /// <summary>
        /// Gets the id of the plugin; none, the tests address the stub by its name.
        /// </summary>
        public IComponentId PluginId => null;

        /// <summary>
        /// Gets the name of the plugin, the id of the templates plugin it stands in for.
        /// </summary>
        public string PluginName => TemplateResources.PluginId;

        /// <summary>
        /// Gets the description of the plugin; none, discovery does not read it.
        /// </summary>
        public string Description => null;

        /// <summary>
        /// Gets the manufacturer of the plugin; none, discovery does not read it.
        /// </summary>
        public string Manufacturer => null;

        /// <summary>
        /// Gets the copyright of the plugin; none, discovery does not read it.
        /// </summary>
        public string Copyright => null;

        /// <summary>
        /// Gets the version of the plugin; none, discovery does not read it.
        /// </summary>
        public string Version => null;

        /// <summary>
        /// Gets the license of the plugin; none, discovery does not read it.
        /// </summary>
        public string License => null;

        /// <summary>
        /// Gets the icon of the plugin; none, discovery does not read it.
        /// </summary>
        public IRoute Icon => null;

        /// <summary>
        /// Gets the assembly discovery scans - the real templates plugin, so the tests find the
        /// shipped templates rather than a stand-in for them.
        /// </summary>
        public Assembly Assembly => TemplateCatalog.Assembly;

        /// <summary>
        /// Gets the settings section of the plugin; none, templates read no settings.
        /// </summary>
        public IConfiguration Settings => null;
    }
}
