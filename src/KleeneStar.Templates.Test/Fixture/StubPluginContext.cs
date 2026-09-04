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
        /// <inheritdoc/>
        public IComponentId PluginId => null;

        /// <inheritdoc/>
        public string PluginName => TemplateResources.PluginId;

        /// <inheritdoc/>
        public string Description => null;

        /// <inheritdoc/>
        public string Manufacturer => null;

        /// <inheritdoc/>
        public string Copyright => null;

        /// <inheritdoc/>
        public string Version => null;

        /// <inheritdoc/>
        public string License => null;

        /// <inheritdoc/>
        public IRoute Icon => null;

        /// <inheritdoc/>
        public Assembly Assembly => TemplateCatalog.Assembly;
    }
}
