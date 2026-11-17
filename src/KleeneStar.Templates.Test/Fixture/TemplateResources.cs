using System.Text;

namespace KleeneStar.Templates.Test
{
    /// <summary>
    /// Reads the plugin's embedded language resources - the files the display names and the
    /// descriptions of the templates actually come from.
    /// </summary>
    /// <remarks>
    /// The files on disk are not what ships; the embedded copies are, and a renamed folder or a
    /// dropped <c>EmbeddedResource</c> entry takes them out of the assembly without failing the
    /// build. The resources are therefore read out of the plugin assembly's manifest, by suffix
    /// rather than by full name, so the tests survive a rename of the project but not the loss
    /// of a translation.
    /// </remarks>
    internal static class TemplateResources
    {
        /// <summary>
        /// The id of the plugin, and the prefix every internationalization key of it carries.
        /// </summary>
        public const string PluginId = "kleenestar.templates";

        /// <summary>
        /// The cultures the plugin is translated into.
        /// </summary>
        public static readonly string[] Cultures = ["de", "en"];

        /// <summary>
        /// Returns the name of the manifest resource holding the supplied culture, or
        /// <see langword="null"/> when it is not embedded.
        /// </summary>
        /// <param name="culture">The culture, e.g. <c>en</c>.</param>
        /// <returns>The manifest resource name.</returns>
        public static string ResourceName(string culture)
        {
            return TemplateCatalog.Assembly
                .GetManifestResourceNames()
                .SingleOrDefault(x => x.EndsWith("." + culture, StringComparison.Ordinal));
        }

        /// <summary>
        /// Returns the entries of a language file in the order they are written, duplicates
        /// included.
        /// </summary>
        /// <remarks>
        /// In order and with duplicates kept, because a key defined twice is not an error the
        /// resource reader reports - the second definition is simply never reached - and finding
        /// that is the point of one of the tests.
        /// </remarks>
        /// <param name="culture">The culture to read.</param>
        /// <returns>The entries.</returns>
        public static IReadOnlyList<(string Key, string Value)> Entries(string culture)
        {
            var name = ResourceName(culture)
                ?? throw new InvalidOperationException($"The language resource '{culture}' is not embedded.");

            using var stream = TemplateCatalog.Assembly.GetManifestResourceStream(name);
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

            var entries = new List<(string, string)>();

            while (reader.ReadLine() is { } line)
            {
                var trimmed = line.Trim();

                if (trimmed.Length == 0 || trimmed.StartsWith('#'))
                {
                    continue;
                }

                var separator = trimmed.IndexOf('=');

                if (separator < 0)
                {
                    continue;
                }

                entries.Add((trimmed[..separator].Trim(), trimmed[(separator + 1)..].Trim()));
            }

            return entries;
        }

        /// <summary>
        /// Returns a language file as a lookup, the first definition of a key winning the way
        /// the resource reader lets it win.
        /// </summary>
        /// <param name="culture">The culture to read.</param>
        /// <returns>The translations.</returns>
        public static IReadOnlyDictionary<string, string> Read(string culture)
        {
            var translations = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var (key, value) in Entries(culture))
            {
                translations.TryAdd(key, value);
            }

            return translations;
        }

        /// <summary>
        /// Strips the plugin prefix off an internationalization key, leaving what the language
        /// file is keyed by.
        /// </summary>
        /// <param name="key">The key as a template states it, e.g.
        /// <c>kleenestar.templates:template.finance.name</c>.</param>
        /// <returns>The key without its plugin prefix.</returns>
        public static string Unqualify(string key)
        {
            var separator = key.IndexOf(':');

            return separator < 0 ? key : key[(separator + 1)..];
        }
    }
}
