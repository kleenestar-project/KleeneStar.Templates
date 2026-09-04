using KleeneStar.Core.WebWorkspaceTemplate;
using KleeneStar.Templates.WebWorkspaceTemplate;
using System.Text.RegularExpressions;
using WebExpress.WebUI.WebIcon;

namespace KleeneStar.Templates.Test.WebWorkspaceTemplate
{
    /// <summary>
    /// Provides unit tests for the catalogue this plugin ships: which templates it offers, and
    /// whether each of them still satisfies what the core expects of one.
    /// </summary>
    /// <remarks>
    /// None of this is checked by the compiler. A template is discovered by interface and built
    /// by reflection, so the rules that decide whether it is offered at all - public, concrete,
    /// parameterless - hold at run time or not at all, and the ones that decide whether it is
    /// offered correctly - a unique key, a unique order, an internationalization key that matches
    /// the one in the language file - fail as a raw key on a card rather than as an error.
    /// </remarks>
    public class UnitTestWorkspaceTemplateCatalog
    {
        /// <summary>
        /// The shape of an icon path: the host's own workspace icons, by name.
        /// </summary>
        private static readonly Regex IconPath = new(@"^/kleenestar/assets/icons/[a-z0-9]+\.svg$", RegexOptions.Compiled);

        /// <summary>
        /// The plugin offers exactly the templates it is known to offer.
        /// </summary>
        /// <remarks>
        /// A key is what a workspace records itself as coming from, so dropping or renaming one
        /// orphans every workspace created from it. That makes the set worth pinning: adding a
        /// template is one line here, and losing one is a failure rather than a quietly shorter
        /// wizard.
        /// </remarks>
        [Fact]
        public void ShippedCatalogueIsComplete()
        {
            var keys = TemplateCatalog.Templates.Select(x => x.Key).ToList();

            Assert.Equal(
                TemplateCatalog.ShippedKeys.OrderBy(x => x, StringComparer.Ordinal),
                keys.OrderBy(x => x, StringComparer.Ordinal));
        }

        /// <summary>
        /// Every template satisfies the contract discovery imposes: public, concrete, sealed, and
        /// constructible without arguments.
        /// </summary>
        /// <remarks>
        /// The manager filters on the first three and calls <c>Activator.CreateInstance</c> for
        /// the fourth, catching what it throws - so a template that loses any of them is dropped
        /// from the catalogue without a word, and the wizard simply offers one card fewer.
        /// </remarks>
        [Fact]
        public void EveryTemplateIsDiscoverable()
        {
            Assert.NotEmpty(TemplateCatalog.Types);

            foreach (var type in TemplateCatalog.Types)
            {
                Assert.True(type.IsPublic, $"{type.Name} is not public.");
                Assert.False(type.IsAbstract, $"{type.Name} is abstract.");
                Assert.True(type.IsSealed, $"{type.Name} is not sealed.");
                Assert.True(typeof(WorkspaceTemplateBase).IsAssignableFrom(type), $"{type.Name} does not derive from the base.");

                var constructor = type.GetConstructor(Type.EmptyTypes);

                Assert.True(constructor is not null, $"{type.Name} has no public parameterless constructor.");
                Assert.IsAssignableFrom<IWorkspaceTemplate>(Activator.CreateInstance(type));
            }
        }

        /// <summary>
        /// The base class is not itself a template.
        /// </summary>
        /// <remarks>
        /// It is abstract, which is what keeps it out of the discovery filter. Were it not, the
        /// manager would try to build it, and a template with no slug is one whose keys read
        /// <c>kleenestar.templates.</c> and whose card is blank.
        /// </remarks>
        [Fact]
        public void TheBaseClassIsNotATemplate()
        {
            Assert.True(typeof(WorkspaceTemplateBase).IsAbstract);
            Assert.DoesNotContain(typeof(WorkspaceTemplateBase), TemplateCatalog.Types);
        }

        /// <summary>
        /// Keys are unique, lower case and prefixed by the plugin that ships them.
        /// </summary>
        [Fact]
        public void KeysAreUniqueAndPrefixed()
        {
            var keys = TemplateCatalog.Templates.Select(x => x.Key).ToList();

            Assert.Equal(keys.Count, keys.Distinct(StringComparer.OrdinalIgnoreCase).Count());

            foreach (var key in keys)
            {
                Assert.StartsWith(TemplateResources.PluginId + ".", key, StringComparison.Ordinal);
                Assert.Equal(key.ToLowerInvariant(), key);
                Assert.NotEmpty(TemplateCatalog.SlugOf(TemplateCatalog.ByKey(key)));
            }
        }

        /// <summary>
        /// The two internationalization keys of a template are derived from its own key.
        /// </summary>
        /// <remarks>
        /// The base class builds all three from one slug, which is what keeps them in step - this
        /// asserts the result rather than the mechanism, so a template that spells its name out
        /// by hand is caught as well.
        /// </remarks>
        [Fact]
        public void NamesAndDescriptionsFollowTheKey()
        {
            foreach (var template in TemplateCatalog.Templates)
            {
                var slug = TemplateCatalog.SlugOf(template);

                Assert.Equal($"{TemplateResources.PluginId}:template.{slug}.name", template.Name);
                Assert.Equal($"{TemplateResources.PluginId}:template.{slug}.description", template.Description);
            }
        }

        /// <summary>
        /// Every template proposes a workspace key, and no two propose the same one.
        /// </summary>
        /// <remarks>
        /// The proposal is only a default the administrator may overwrite, but two templates
        /// proposing <c>SD</c> would collide on the second create by default rather than by
        /// accident.
        /// </remarks>
        [Fact]
        public void SuggestedKeysAreUniqueAndUpperCase()
        {
            var suggested = TemplateCatalog.Templates.Select(x => x.SuggestedKey).ToList();

            Assert.Equal(suggested.Count, suggested.Distinct(StringComparer.OrdinalIgnoreCase).Count());

            foreach (var key in suggested)
            {
                Assert.False(string.IsNullOrWhiteSpace(key));
                Assert.Matches("^[A-Z]+$", key);
            }
        }

        /// <summary>
        /// The catalogue has one order, not two templates claiming the same place.
        /// </summary>
        /// <remarks>
        /// Equal orders fall back to the key, so a duplicate is not a crash - it is a wizard
        /// whose cards reshuffle alphabetically the moment somebody renames one.
        /// </remarks>
        [Fact]
        public void OrdersAreUnique()
        {
            var orders = TemplateCatalog.Templates.Select(x => x.Order).ToList();

            Assert.Equal(orders.Count, orders.Distinct().Count());
            Assert.All(orders, x => Assert.True(x > 0));
        }

        /// <summary>
        /// Every template is filed under at least one category, and none of them is blank or
        /// repeated within a template.
        /// </summary>
        [Fact]
        public void CategoriesAreDeclared()
        {
            foreach (var template in TemplateCatalog.Templates)
            {
                var categories = template.Categories?.ToList();

                Assert.True(categories is { Count: > 0 }, $"{template.Key} declares no category.");
                Assert.All(categories, x => Assert.False(string.IsNullOrWhiteSpace(x)));
                Assert.All(categories, x => Assert.Equal(x.Trim(), x));
                Assert.Equal(categories.Count, categories.Distinct(StringComparer.Ordinal).Count());
            }
        }

        /// <summary>
        /// Every template carries an icon, and it is one of the host's workspace icons.
        /// </summary>
        /// <remarks>
        /// A workspace created from a template is meant to look like the one the template was
        /// modelled on, which is what the shared icon path buys - and an icon that resolves to
        /// nothing is a card with a hole in it rather than an error.
        /// </remarks>
        [Fact]
        public void IconsPointAtTheWorkspaceIcons()
        {
            foreach (var template in TemplateCatalog.Templates)
            {
                var icon = Assert.IsType<ImageIcon>(template.Icon);

                Assert.NotNull(icon.Uri);
                Assert.Matches(IconPath, icon.Uri.ToString());
            }
        }
    }
}
