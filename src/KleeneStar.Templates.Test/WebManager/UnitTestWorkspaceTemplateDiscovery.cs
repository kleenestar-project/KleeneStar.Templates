using KleeneStar.Core.WebManager;
using KleeneStar.Core.WebWorkspaceTemplate;
using System.Reflection;
using WebExpress.WebCore;
using WebExpress.WebCore.WebComponent;

namespace KleeneStar.Templates.Test.WebManager
{
    /// <summary>
    /// Provides unit tests for the half of <see cref="WorkspaceTemplateManager"/> the core's own
    /// tests leave out: scanning a real plugin assembly and finding the templates in it.
    /// </summary>
    /// <remarks>
    /// The core tests what the manager does with a template - applying it, refusing an unknown
    /// key - against a probe, because the core ships no templates to find. This is the plugin
    /// that does, so the scan can be pointed at something real here: the manager is built without
    /// a component hub, which leaves its two registration passes idle, and handed the plugin
    /// directly.
    /// </remarks>
    public class UnitTestWorkspaceTemplateDiscovery
    {
        /// <summary>
        /// Builds a manager with no component hub and lets it scan this plugin.
        /// </summary>
        /// <remarks>
        /// Both the constructor sweep and the <c>AddPlugin</c> subscription hang off the plugin
        /// manager, which a unit test has no instance of - so registration is driven directly,
        /// through the private method the two passes share. That is the seam, and the only one:
        /// everything asserted afterwards goes through the manager's public surface.
        /// </remarks>
        /// <param name="pluginContext">The plugin that was scanned.</param>
        /// <returns>The manager, with this plugin's templates registered.</returns>
        private static WorkspaceTemplateManager Scan(out StubPluginContext pluginContext)
        {
            var constructor = typeof(WorkspaceTemplateManager).GetConstructor
            (
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                [typeof(IComponentHub), typeof(IHttpServerContext)],
                null
            ) ?? throw new InvalidOperationException("The private constructor was renamed.");

            var manager = (WorkspaceTemplateManager)constructor.Invoke([null, null]);

            var register = typeof(WorkspaceTemplateManager).GetMethod("Register", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("The registration method was renamed.");

            pluginContext = new StubPluginContext();

            register.Invoke(manager, [pluginContext]);

            return manager;
        }

        /// <summary>
        /// Scanning this plugin finds every template it ships, and nothing else.
        /// </summary>
        [Fact]
        public void ScanningThePluginFindsTheTemplates()
        {
            using var manager = Scan(out _);

            Assert.Equal(
                TemplateCatalog.ShippedKeys.OrderBy(x => x, StringComparer.Ordinal),
                manager.WorkspaceTemplates.Select(x => x.Template.Key).OrderBy(x => x, StringComparer.Ordinal));
        }

        /// <summary>
        /// The catalogue is handed out in the order the templates ask to be offered in.
        /// </summary>
        [Fact]
        public void TheCatalogueIsOrdered()
        {
            using var manager = Scan(out _);

            var offered = manager.WorkspaceTemplates.Select(x => x.Template).ToList();

            Assert.Equal(
                offered.OrderBy(x => x.Order).ThenBy(x => x.Key, StringComparer.OrdinalIgnoreCase).Select(x => x.Key),
                offered.Select(x => x.Key));
        }

        /// <summary>
        /// Every registration names the plugin it came from and the type that defined it.
        /// </summary>
        /// <remarks>
        /// The manager hands out contexts rather than templates so the source can be named and,
        /// more to the point, so a removed plugin takes exactly its own templates with it.
        /// </remarks>
        [Fact]
        public void EveryRegistrationNamesItsPlugin()
        {
            using var manager = Scan(out var pluginContext);

            foreach (var context in manager.WorkspaceTemplates)
            {
                Assert.Same(pluginContext, context.PluginContext);
                Assert.Equal(context.Template.GetType(), context.TemplateType);
                Assert.Contains(context.TemplateType, TemplateCatalog.Types);
            }

            Assert.Equal(
                manager.WorkspaceTemplates.Select(x => x.Template.Key),
                manager.GetWorkspaceTemplates(pluginContext).Select(x => x.Template.Key));
        }

        /// <summary>
        /// Each shipped template is answered by its key, whatever case it is asked for in.
        /// </summary>
        /// <remarks>
        /// The lookup is what turns the key a workspace recorded back into the template that
        /// shaped it, and the key travels through a payload - so it is compared without regard to
        /// case.
        /// </remarks>
        [Fact]
        public void EveryTemplateIsFoundByKey()
        {
            using var manager = Scan(out _);

            foreach (var key in TemplateCatalog.ShippedKeys)
            {
                Assert.NotNull(manager.GetWorkspaceTemplate(key));
                Assert.NotNull(manager.GetWorkspaceTemplate(key.ToUpperInvariant()));
            }

            Assert.Null(manager.GetWorkspaceTemplate("kleenestar.templates.uninstalled"));
        }

        /// <summary>
        /// Registration announces each template once.
        /// </summary>
        /// <remarks>
        /// The event is how the rest of the installation learns a template arrived, and it fires
        /// after the registry is filled rather than during - the handler is entitled to ask the
        /// manager what it now holds.
        /// </remarks>
        [Fact]
        public void RegistrationAnnouncesEachTemplate()
        {
            var announced = new List<IWorkspaceTemplateContext>();

            var constructor = typeof(WorkspaceTemplateManager).GetConstructor
            (
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                [typeof(IComponentHub), typeof(IHttpServerContext)],
                null
            );

            using var manager = (WorkspaceTemplateManager)constructor.Invoke([null, null]);

            manager.AddWorkspaceTemplate += (_, e) => announced.Add(e);

            typeof(WorkspaceTemplateManager)
                .GetMethod("Register", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(manager, [new StubPluginContext()]);

            Assert.Equal(
                TemplateCatalog.ShippedKeys.OrderBy(x => x, StringComparer.Ordinal),
                announced.Select(x => x.Template.Key).OrderBy(x => x, StringComparer.Ordinal));
        }

        /// <summary>
        /// Scanning the same plugin twice does not register its templates twice.
        /// </summary>
        /// <remarks>
        /// Discovery runs from two places - the sweep over the plugins already installed and the
        /// event for the ones that arrive later - and a plugin that reaches both would otherwise
        /// appear in the wizard in duplicate.
        /// </remarks>
        [Fact]
        public void ScanningTwiceRegistersOnce()
        {
            using var manager = Scan(out var pluginContext);

            typeof(WorkspaceTemplateManager)
                .GetMethod("Register", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(manager, [pluginContext]);

            Assert.Equal(TemplateCatalog.ShippedKeys.Length, manager.WorkspaceTemplates.Count());
        }
    }
}
