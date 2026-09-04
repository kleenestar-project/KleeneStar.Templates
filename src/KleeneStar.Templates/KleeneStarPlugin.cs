using KleeneStar.Core;
using WebExpress.WebCore.WebAttribute;
using WebExpress.WebCore.WebPlugin;

namespace KleeneStar.Templates
{
    /// <summary>
    /// Plugin entry point of the workspace-template library.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The plugin contributes no application, no page and no endpoint - it ships nothing but a
    /// set of classes describing what a workspace can be created as. That is deliberate: the
    /// templates are the reason a plugin exists here at all, and putting them in one of their own
    /// makes the catalogue exactly as long as the set of installed plugins, which is what lets an
    /// installation add its own shapes or drop the ones shipped here without touching the core.
    /// </para>
    /// <para>
    /// The core's <c>WorkspaceTemplateManager</c> discovers them by scanning the assembly of
    /// every plugin it is told about, so nothing has to be registered here.
    /// </para>
    /// <para>
    /// It names the core's application all the same. A plugin without one is refused by the
    /// framework - a plugin that belongs to nothing has nowhere to be reached from - so this
    /// declares the application the templates extend rather than one of its own. The
    /// application itself is defined in the core; naming it here only associates the two.
    /// </para>
    /// </remarks>
    [Name("kleenestar.templates:plugin.name")]
    [Description("kleenestar.templates:plugin.description")]
    [Application<KleeneStarApplication>()]
    [Dependency("webexpress.webapp")]
    [Dependency("kleenestar.core")]
    public sealed class KleeneStarPlugin : IPlugin
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public KleeneStarPlugin()
        {
        }

        /// <summary>
        /// Called when the plugin starts working. Run is called concurrently.
        /// </summary>
        public void Run()
        {
        }
    }
}
