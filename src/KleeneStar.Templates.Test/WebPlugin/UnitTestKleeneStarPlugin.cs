using KleeneStar.Core;
using System.Reflection;
using WebExpress.WebCore.WebAttribute;
using WebExpress.WebCore.WebPlugin;

namespace KleeneStar.Templates.Test.WebPlugin
{
    /// <summary>
    /// Provides unit tests for <see cref="KleeneStarPlugin"/> - the declarations that decide
    /// whether this plugin is loaded at all.
    /// </summary>
    /// <remarks>
    /// Everything asserted here is an attribute, which is to say none of it is checked by the
    /// compiler and all of it is checked by the framework at start-up. A plugin that fails those
    /// checks is not an error the developer sees: it is dropped with a warning in the log, and
    /// the only symptom is a wizard offering no templates.
    /// </remarks>
    public class UnitTestKleeneStarPlugin
    {
        /// <summary>
        /// Returns the values a plugin attribute was declared with, read off the metadata.
        /// </summary>
        /// <remarks>
        /// Off the metadata rather than off an instance of the attribute, because these
        /// attributes keep nothing: their constructors take the value and discard it, so the
        /// declared argument is only readable as <see cref="CustomAttributeData"/>.
        /// </remarks>
        /// <param name="attributeType">The attribute to read.</param>
        /// <returns>The first constructor argument of each declaration.</returns>
        private static IReadOnlyList<string> Declared(Type attributeType)
        {
            return
            [
                .. typeof(KleeneStarPlugin)
                    .GetCustomAttributesData()
                    .Where(x => x.AttributeType == attributeType)
                    .Where(x => x.ConstructorArguments.Count > 0)
                    .Select(x => x.ConstructorArguments[0].Value as string)
            ];
        }

        /// <summary>
        /// The plugin is a plugin the framework can build: public, sealed and parameterless.
        /// </summary>
        [Fact]
        public void ThePluginIsConstructible()
        {
            var type = typeof(KleeneStarPlugin);

            Assert.True(type.IsPublic);
            Assert.True(type.IsSealed);
            Assert.True(typeof(IPlugin).IsAssignableFrom(type));
            Assert.NotNull(type.GetConstructor(Type.EmptyTypes));
        }

        /// <summary>
        /// The plugin names an application.
        /// </summary>
        /// <remarks>
        /// It defines none of its own - it ships templates and nothing else - but a plugin that
        /// belongs to no application is refused, so it names the core's. Losing this attribute
        /// costs the installation every template without costing the build a thing.
        /// </remarks>
        [Fact]
        public void ThePluginNamesAnApplication()
        {
            Assert.NotNull(typeof(KleeneStarPlugin).GetCustomAttribute<ApplicationAttribute<KleeneStarApplication>>());
        }

        /// <summary>
        /// The plugin declares what it needs: the web application framework, and the core whose
        /// template contract it implements.
        /// </summary>
        /// <remarks>
        /// The dependencies are what put this plugin after the core in the load order. Scanning
        /// it before the core has registered its component managers finds the templates and has
        /// nowhere to put them.
        /// </remarks>
        [Fact]
        public void ThePluginDeclaresItsDependencies()
        {
            var dependencies = Declared(typeof(DependencyAttribute));

            Assert.Contains("webexpress.webapp", dependencies);
            Assert.Contains("kleenestar.core", dependencies);
        }

        /// <summary>
        /// The plugin says what it is, in keys that are translated.
        /// </summary>
        [Fact]
        public void ThePluginIsNamedInBothCultures()
        {
            var name = Assert.Single(Declared(typeof(NameAttribute)));
            var description = Assert.Single(Declared(typeof(DescriptionAttribute)));

            Assert.Equal($"{TemplateResources.PluginId}:plugin.name", name);
            Assert.Equal($"{TemplateResources.PluginId}:plugin.description", description);

            foreach (var culture in TemplateResources.Cultures)
            {
                var translations = TemplateResources.Read(culture);

                Assert.True(translations.ContainsKey(TemplateResources.Unqualify(name)));
                Assert.True(translations.ContainsKey(TemplateResources.Unqualify(description)));
            }
        }

        /// <summary>
        /// The plugin's id is the one its templates prefix their keys with.
        /// </summary>
        /// <remarks>
        /// The framework derives the id from the assembly name, and the internationalization
        /// prefix is that id - so renaming the assembly silently unhooks every translation in it.
        /// </remarks>
        [Fact]
        public void ThePluginIdMatchesTheTranslationPrefix()
        {
            Assert.Equal(TemplateResources.PluginId, TemplateCatalog.Assembly.GetName().Name.ToLowerInvariant());
        }
    }
}
