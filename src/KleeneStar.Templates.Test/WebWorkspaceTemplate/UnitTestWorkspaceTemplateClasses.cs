using KleeneStar.Model.Entities;
using System.Text.RegularExpressions;

namespace KleeneStar.Templates.Test.WebWorkspaceTemplate
{
    /// <summary>
    /// Provides unit tests for the classes the shipped templates declare - the part of a template
    /// somebody actually decided, and the part that is written into a workspace unchanged.
    /// </summary>
    /// <remarks>
    /// The manager copies each declaration into a <c>Class</c> row verbatim: the name, the
    /// description key, the icon, the kind, the portal visibility and the access modifier. There
    /// is no validation on the way, so whatever is wrong here is wrong in every workspace created
    /// from the template afterwards, and fixing the template does not go back and fix them.
    /// </remarks>
    public class UnitTestWorkspaceTemplateClasses
    {
        /// <summary>
        /// The kinds an object may be filed as. The kind is a free string in the data layer, so
        /// nothing but this stops a typo from producing a class no overview view can present.
        /// </summary>
        private static readonly string[] KnownKinds =
            [ObjectKind.Issue, ObjectKind.Document, ObjectKind.Blog, ObjectKind.Asset];

        /// <summary>
        /// The shape of an icon path.
        /// </summary>
        private static readonly Regex IconPath = new(@"^/kleenestar/assets/icons/[a-z0-9]+\.svg$", RegexOptions.Compiled);

        /// <summary>
        /// Every template starts its workspace with classes, and none of them is nameless.
        /// </summary>
        /// <remarks>
        /// A template with no classes is one click's worth of difference from the empty
        /// workspace, and a class with a blank name is skipped by the manager without comment.
        /// </remarks>
        [Fact]
        public void EveryTemplateDeclaresClasses()
        {
            foreach (var template in TemplateCatalog.Templates)
            {
                var classes = template.Classes?.ToList();

                Assert.True(classes is { Count: > 0 }, $"{template.Key} declares no class.");
                Assert.All(classes, x => Assert.False(string.IsNullOrWhiteSpace(x.Name)));
            }
        }

        /// <summary>
        /// No template declares one name twice, compared the way the manager compares them.
        /// </summary>
        /// <remarks>
        /// Applying a template skips a name the workspace already carries, and the set it checks
        /// against is case-insensitive - so a template declaring both <c>Change</c> and
        /// <c>change</c> would create the first and silently drop the second.
        /// </remarks>
        [Fact]
        public void ClassNamesAreUniquePerTemplate()
        {
            foreach (var template in TemplateCatalog.Templates)
            {
                var names = template.Classes.Select(x => x.Name).ToList();

                Assert.Equal(names.Count, names.Distinct(StringComparer.OrdinalIgnoreCase).Count());
            }
        }

        /// <summary>
        /// Class names are written the way an administrator sees them: one word, capitalized, no
        /// whitespace and nothing to trim.
        /// </summary>
        /// <remarks>
        /// The name is data rather than a caption - it is not translated and it is what the
        /// workspace shows - so a stray space is not caught anywhere later.
        /// </remarks>
        [Fact]
        public void ClassNamesAreWellFormed()
        {
            foreach (var declaration in TemplateCatalog.Templates.SelectMany(x => x.Classes))
            {
                Assert.Matches("^[A-Z][A-Za-z]*$", declaration.Name);
            }
        }

        /// <summary>
        /// Every declared class holds a kind the product knows how to present.
        /// </summary>
        [Fact]
        public void ClassKindsAreKnown()
        {
            foreach (var template in TemplateCatalog.Templates)
            {
                foreach (var declaration in template.Classes)
                {
                    Assert.Contains(declaration.Kind, KnownKinds);
                    Assert.Equal(ObjectKind.Normalize(declaration.Kind), declaration.Kind);
                }
            }
        }

        /// <summary>
        /// Every declared class carries an icon of the expected shape.
        /// </summary>
        [Fact]
        public void ClassIconsAreDeclared()
        {
            foreach (var declaration in TemplateCatalog.Templates.SelectMany(x => x.Classes))
            {
                Assert.Matches(IconPath, declaration.Icon);
            }
        }

        /// <summary>
        /// A class's description key is derived from the template's slug and the class's own
        /// name, lower cased.
        /// </summary>
        /// <remarks>
        /// This is the pairing that drifts: renaming a class in the template without renaming its
        /// entry in the language files leaves the key itself standing on the card. The rule is
        /// asserted on the result so that a hand-written description is caught too.
        /// </remarks>
        [Fact]
        public void ClassDescriptionsFollowTheClassName()
        {
            foreach (var template in TemplateCatalog.Templates)
            {
                var slug = TemplateCatalog.SlugOf(template);

                foreach (var declaration in template.Classes)
                {
                    var expected = $"{TemplateResources.PluginId}:template.{slug}.class.{declaration.Name.ToLowerInvariant()}";

                    Assert.Equal(expected, declaration.Description);
                }
            }
        }

        /// <summary>
        /// Nothing shipped here starts life restricted.
        /// </summary>
        /// <remarks>
        /// A template describes a workspace somebody is setting up, not the permissions they will
        /// give it - narrowing a class is a decision for afterwards, and a template that made it
        /// in advance would hide classes from the very administrator who just created them.
        /// </remarks>
        [Fact]
        public void ClassesAreCreatedPublic()
        {
            foreach (var declaration in TemplateCatalog.Templates.SelectMany(x => x.Classes))
            {
                Assert.Equal(AccessModifier.Public, declaration.AccessModifier);
            }
        }

        /// <summary>
        /// The service desk is the only template that opens classes to the customer portal, and
        /// it opens exactly the three request types.
        /// </summary>
        /// <remarks>
        /// Portal visibility is what decides whether people outside the organization can file and
        /// read an object of a class, so a template that sets it by accident publishes internal
        /// work to customers the moment the workspace is created.
        /// </remarks>
        [Fact]
        public void OnlyTheServiceDeskIsPortalVisible()
        {
            var visible = TemplateCatalog.Templates
                .SelectMany(x => x.Classes.Select(y => (x.Key, y.Name, y.PortalVisible)))
                .Where(x => x.PortalVisible)
                .ToList();

            Assert.All(visible, x => Assert.Equal("kleenestar.templates.servicedesk", x.Key));

            Assert.Equal(
                new[] { "Incident", "ServiceRequest", "Ticket" },
                visible.Select(x => x.Name).OrderBy(x => x, StringComparer.Ordinal));
        }

        /// <summary>
        /// Sealing a class is the exception, and the configuration database is where it is made.
        /// </summary>
        /// <remarks>
        /// A sealed class may not be specialized further, which is the right answer for the
        /// governance a configuration is held to and the wrong one for everything a workspace is
        /// expected to grow.
        /// </remarks>
        [Fact]
        public void SealingIsTheException()
        {
            var sealedClasses = TemplateCatalog.Templates
                .SelectMany(x => x.Classes.Select(y => (x.Key, y.Name, y.Sealed)))
                .Where(x => x.Sealed)
                .ToList();

            var single = Assert.Single(sealedClasses);

            Assert.Equal("kleenestar.templates.cmdb", single.Key);
            Assert.Equal("Policy", single.Name);
        }

        /// <summary>
        /// The configuration database is the template whose central class holds assets rather
        /// than issues - the one thing that distinguishes it from every other catalogue of work.
        /// </summary>
        [Fact]
        public void TheConfigurationDatabaseHoldsAssets()
        {
            var cmdb = TemplateCatalog.ByKey("kleenestar.templates.cmdb");
            var asset = cmdb.Classes.Single(x => x.Name == "Asset");

            Assert.Equal(ObjectKind.Asset, asset.Kind);
            Assert.Equal("Asset", cmdb.Classes.First().Name);
        }

        /// <summary>
        /// Software development spans three kinds of object, which is what makes it the example
        /// of a workspace that is more than a ticket list.
        /// </summary>
        [Fact]
        public void DevelopmentSpansThreeKinds()
        {
            var development = TemplateCatalog.ByKey("kleenestar.templates.development");
            var kinds = development.Classes.Select(x => x.Kind).Distinct().OrderBy(x => x, StringComparer.Ordinal);

            Assert.Equal(new[] { ObjectKind.Blog, ObjectKind.Document, ObjectKind.Issue }, kinds);
        }
    }
}
