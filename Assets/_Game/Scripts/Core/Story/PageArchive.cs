using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ThuyKieu.Core.Story
{
    /// <summary>
    /// The pages kept between playthroughs: every full verse the player has ever held. A new run starts holding them,
    /// so replaying really is "knowing more in advance" — the game's own theme.
    /// </summary>
    public static class PageArchive
    {
        private const string Key = "ThuyKieu.TrangSach.Archive";

        public static IReadOnlyCollection<string> Ids => Load();

        public static void Remember(string id)
        {
            var ids = Load();
            if (!ids.Add(id)) return;
            PlayerPrefs.SetString(Key, string.Join(",", ids));
            PlayerPrefs.Save();
        }

        /// <summary>Gives a fresh story every page kept from earlier runs (never re-grants one already torn out).</summary>
        public static int ApplyTo(Ink.Runtime.Story story)
        {
            int given = 0;
            foreach (string id in Load())
                if (PageCatalog.TryGet(id, out var page) && !page.Unusable && PageCatalog.State(story, id) < PageCatalog.Held)
                {
                    story.variablesState[id] = PageCatalog.Held;
                    given++;
                }
            return given;
        }

        public static void Clear() { PlayerPrefs.DeleteKey(Key); PlayerPrefs.Save(); }

        private static HashSet<string> Load()
        {
            string raw = PlayerPrefs.GetString(Key, string.Empty);
            return new HashSet<string>(raw.Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()));
        }
    }
}
