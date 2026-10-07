using System.Text;
using System.Text.RegularExpressions;

namespace ThuyKieu.Dialogue
{
    /// <summary>
    /// Stable id of a spoken line: the voice clip for (speaker, text) is stored as Resources/Voice/&lt;key&gt;.
    /// The same key is computed by the manifest exporter in the Editor and by <see cref="VoicePlayer"/> at runtime,
    /// so editing a line in Ink simply leaves it unvoiced until the clips are regenerated.
    /// </summary>
    public static class VoiceLine
    {
        public const string ResourceFolder = "Voice";
        private static readonly Regex RichText = new Regex("<[^>]+>");
        private static readonly Regex Spaces = new Regex(@"\s+");

        /// <summary>Line text as it is spoken: rich-text tags removed, whitespace collapsed.</summary>
        public static string Normalize(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return Spaces.Replace(RichText.Replace(text, string.Empty), " ").Trim();
        }

        /// <summary>FNV-1a 64-bit hash of "speaker|text", as 16 hex digits.</summary>
        public static string Key(string speakerId, string text)
        {
            const ulong offset = 14695981039346656037UL, prime = 1099511628211UL;
            ulong hash = offset;
            foreach (byte b in Encoding.UTF8.GetBytes((speakerId ?? "Narrator") + "|" + Normalize(text)))
            {
                hash ^= b;
                hash *= prime;
            }
            return hash.ToString("x16");
        }
    }
}
