using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KleeneStar.Templates.WebWorkspaceTemplate
{
    /// <summary>
    /// Turns the English text a template is written in into the internationalization key it is
    /// shipped as.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The structure of a class - its fields, states, priorities, calendars and agreements - is
    /// hundreds of names and sentences. Written as keys, a template would read as a list of
    /// identifiers nobody can review; written as English, it would create a German service desk
    /// with English states. So the templates are written in English, and the key is derived from
    /// the text, the way gettext uses the source string as its message id: the same text is the
    /// same key everywhere, and one translation serves every class that uses it.
    /// </para>
    /// <para>
    /// The key is the text in lower case with everything but letters and digits folded into
    /// underscores, under <c>text.</c>. A text too long for a readable key keeps its first words
    /// and gets a hash of the whole appended, so two sentences that start alike stay apart. The
    /// English file carries the text as its own translation; the tests hold code and file in
    /// step, so the file is never edited by hand where the code says otherwise.
    /// </para>
    /// <para>
    /// The core resolves the keys once, when the workspace is created, in the language of whoever
    /// creates it - as it already did for the class descriptions.
    /// </para>
    /// </remarks>
    public static class TemplateText
    {
        /// <summary>
        /// The plugin the keys belong to.
        /// </summary>
        public const string PluginId = "kleenestar.templates";

        /// <summary>
        /// The prefix of every derived key, inside the plugin's namespace.
        /// </summary>
        public const string Prefix = "text.";

        /// <summary>
        /// The longest slug kept as it is; a longer one is shortened and hashed.
        /// </summary>
        private const int MaxSlug = 48;

        /// <summary>
        /// Every key derived so far, with the text it was derived from.
        /// </summary>
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _texts = new(StringComparer.Ordinal);

        /// <summary>
        /// Gets every key derived so far, each with the texts it was derived from - one text
        /// per key, unless two different texts happened to fold into the same key.
        /// </summary>
        public static IReadOnlyDictionary<string, IReadOnlyCollection<string>> Texts =>
            _texts.ToDictionary(x => x.Key, x => (IReadOnlyCollection<string>)[.. x.Value.Keys], StringComparer.Ordinal);

        /// <summary>
        /// Returns the fully qualified key of a text, or the text itself when it is empty or
        /// already a key.
        /// </summary>
        /// <param name="text">The English text.</param>
        /// <returns>The key, e.g. <c>kleenestar.templates:text.in_progress</c>.</returns>
        public static string Key(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || IsKey(text))
            {
                return text;
            }

            var key = Prefix + Slug(text);

            _texts.GetOrAdd(key, _ => new ConcurrentDictionary<string, byte>(StringComparer.Ordinal)).TryAdd(text, 0);

            return PluginId + ":" + key;
        }

        /// <summary>
        /// Returns whether a text is already an internationalization key of some plugin.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>True for <c>plugin.id:some.key</c>.</returns>
        public static bool IsKey(string text)
        {
            var separator = text.IndexOf(':');

            return separator > 0
                && !text.Contains(' ')
                && text[..separator].All(c => char.IsLetterOrDigit(c) || c == '.' || c == '_' || c == '-');
        }

        /// <summary>
        /// Folds a text into the key segment.
        /// </summary>
        private static string Slug(string text)
        {
            var builder = new StringBuilder(text.Length);
            var underscore = false;

            foreach (var c in text.ToLowerInvariant())
            {
                if (c is >= 'a' and <= 'z' or >= '0' and <= '9')
                {
                    builder.Append(c);
                    underscore = false;
                }
                else if (!underscore && builder.Length > 0)
                {
                    builder.Append('_');
                    underscore = true;
                }
            }

            var slug = builder.ToString().TrimEnd('_');

            if (slug.Length == 0)
            {
                return "x" + Hash(text);
            }

            return slug.Length <= MaxSlug
                ? slug
                : slug[..40].TrimEnd('_') + "_" + Hash(text);
        }

        /// <summary>
        /// Returns a stable hash of a text (FNV-1a over its UTF-8 bytes), as eight hex digits.
        /// </summary>
        private static string Hash(string text)
        {
            var hash = 2166136261u;

            foreach (var b in Encoding.UTF8.GetBytes(text))
            {
                hash = (hash ^ b) * 16777619u;
            }

            return hash.ToString("x8");
        }
    }
}
