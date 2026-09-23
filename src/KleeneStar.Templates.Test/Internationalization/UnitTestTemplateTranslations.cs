using KleeneStar.Templates.WebWorkspaceTemplate;

namespace KleeneStar.Templates.Test.Internationalization
{
    /// <summary>
    /// Provides unit tests for the language resources: whether every key a template states is
    /// actually translated, in both cultures, exactly once.
    /// </summary>
    /// <remarks>
    /// A missing translation is not an error anywhere in the product - the resolver hands the key
    /// back and the wizard prints <c>kleenestar.templates:template.finance.name</c> on the card
    /// where the name should be. Nor is a duplicated key: the first definition wins and the
    /// second is simply never reached, which is why the tests read the file as a list of entries
    /// rather than as a dictionary.
    /// </remarks>
    public class UnitTestTemplateTranslations
    {
        /// <summary>
        /// Returns every internationalization key the shipped templates state, without their
        /// plugin prefix - the keys the language files have to carry.
        /// </summary>
        /// <returns>The keys.</returns>
        private static IReadOnlyCollection<string> UsedKeys()
        {
            var keys = new List<string>();

            foreach (var template in TemplateCatalog.Templates)
            {
                keys.Add(template.Name);
                keys.Add(template.Description);
                keys.AddRange(template.Classes.Select(x => x.Description));
            }

            // the defaults of every kind are shipped too - a template gets them for whatever it
            // leaves open, whether or not a shipped one does
            foreach (var (kind, renderer) in new[] { ("issue", (string)null), ("asset", null), ("document", "form"), ("document", null), ("blog", null) })
            {
                TemplateStructure.Resolve(new Core.WebWorkspaceTemplate.WorkspaceTemplateClass { Name = "Probe", Kind = kind, Renderer = renderer }, null, null, null, null, null);
            }

            // the structure of the classes is shipped as keys derived from its English text;
            // declaring the classes above is what derived them
            keys.AddRange(TemplateText.Texts.Keys);

            return [.. keys.Select(TemplateResources.Unqualify).Distinct(StringComparer.Ordinal)];
        }

        /// <summary>
        /// The English file says what the code says: every derived key is translated into
        /// English as exactly the text it was derived from.
        /// </summary>
        /// <remarks>
        /// The structure texts are written in English in the code and shipped as keys, so the
        /// English entry is a copy - and a copy that drifted would show a text nobody wrote. Two
        /// different texts folding into one key would share one translation, which is why a key
        /// has to come from exactly one text.
        /// </remarks>
        [Fact]
        public void TheEnglishFileRepeatsTheCode()
        {
            _ = UsedKeys();

            var english = TemplateResources.Read("en");

            foreach (var (key, texts) in TemplateText.Texts)
            {
                var text = Assert.Single(texts);

                // the message is the line to add, so a changed text is fixed by copying it into
                // the English file and its translation into the German one
                Assert.True(english.TryGetValue(key, out var translated), $"'{key}' is not in the English file - add: {key}={text}");
                Assert.True(text == translated, $"'{key}' reads '{translated}' in the English file, but '{text}' in the code.");
            }
        }

        /// <summary>
        /// Both language files ship inside the assembly.
        /// </summary>
        /// <remarks>
        /// They are embedded resources, and an embedded resource that stops being included is not
        /// a build error - it is an installation in which the whole plugin speaks in keys.
        /// </remarks>
        [Theory]
        [InlineData("de")]
        [InlineData("en")]
        public void TheLanguageFileIsEmbedded(string culture)
        {
            var name = TemplateResources.ResourceName(culture);

            Assert.False(string.IsNullOrEmpty(name), $"The language resource '{culture}' is not embedded.");
            Assert.NotEmpty(TemplateResources.Entries(culture));
        }

        /// <summary>
        /// Every key a template states is translated.
        /// </summary>
        [Theory]
        [InlineData("de")]
        [InlineData("en")]
        public void EveryStatedKeyIsTranslated(string culture)
        {
            var translations = TemplateResources.Read(culture);
            var missing = UsedKeys().Where(x => !translations.ContainsKey(x)).OrderBy(x => x, StringComparer.Ordinal);

            Assert.Empty(missing);
        }

        /// <summary>
        /// Nothing is translated to nothing.
        /// </summary>
        [Theory]
        [InlineData("de")]
        [InlineData("en")]
        public void NoTranslationIsEmpty(string culture)
        {
            foreach (var (key, value) in TemplateResources.Entries(culture))
            {
                Assert.False(string.IsNullOrWhiteSpace(value), $"'{key}' is translated to nothing in '{culture}'.");
            }
        }

        /// <summary>
        /// No key is defined twice.
        /// </summary>
        /// <remarks>
        /// A second definition is dead the moment it is written: the reader keeps the first, so
        /// correcting a translation by adding a line below leaves the wrong one in the interface
        /// and no trace of why.
        /// </remarks>
        [Theory]
        [InlineData("de")]
        [InlineData("en")]
        public void NoKeyIsDefinedTwice(string culture)
        {
            var duplicates = TemplateResources.Entries(culture)
                .GroupBy(x => x.Key, StringComparer.Ordinal)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key)
                .OrderBy(x => x, StringComparer.Ordinal);

            Assert.Empty(duplicates);
        }

        /// <summary>
        /// The language files carry nothing beyond what the plugin and its templates state.
        /// </summary>
        /// <remarks>
        /// A key nothing reads is what a dropped template leaves behind, and it is invisible from
        /// either end: the file still parses and the wizard still looks right.
        /// </remarks>
        [Theory]
        [InlineData("de")]
        [InlineData("en")]
        public void NothingIsTranslatedInVain(string culture)
        {
            var expected = new HashSet<string>(UsedKeys(), StringComparer.Ordinal)
            {
                "plugin.name",
                "plugin.description"
            };

            var orphaned = TemplateResources.Read(culture).Keys
                .Where(x => !expected.Contains(x))
                .OrderBy(x => x, StringComparer.Ordinal);

            Assert.Empty(orphaned);
        }

        /// <summary>
        /// The cultures are translations of each other, not two different files.
        /// </summary>
        [Fact]
        public void TheCulturesCarryTheSameKeys()
        {
            var de = TemplateResources.Read("de").Keys.OrderBy(x => x, StringComparer.Ordinal);
            var en = TemplateResources.Read("en").Keys.OrderBy(x => x, StringComparer.Ordinal);

            Assert.Equal(en, de);
        }

        /// <summary>
        /// The plugin says what it is in both cultures.
        /// </summary>
        /// <remarks>
        /// Its own name and description are stated as keys on the plugin attributes rather than
        /// derived by the base class, so they are the pair the convention does not protect.
        /// </remarks>
        [Theory]
        [InlineData("de")]
        [InlineData("en")]
        public void ThePluginIsDescribed(string culture)
        {
            var translations = TemplateResources.Read(culture);

            Assert.True(translations.ContainsKey("plugin.name"));
            Assert.True(translations.ContainsKey("plugin.description"));
        }
    }
}
