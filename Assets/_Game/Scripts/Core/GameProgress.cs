using System.IO;
using UnityEngine;

namespace ThuyKieu.Core
{
    /// <summary>
    /// Persistent progress across sessions: the furthest chapter reached and the one last played.
    /// Ink choices are carried by the per-chapter files InkStatePersistence already writes.
    /// </summary>
    public static class GameProgress
    {
        private const string ReachedKey = "tk.reachedChapter";
        private const string LastKey = "tk.lastChapter";
        private const string UnlockKey = "tk.unlockAll";

        public static int Reached => PlayerPrefs.GetInt(ReachedKey, 0);
        public static int Last => PlayerPrefs.GetInt(LastKey, 0);
        public static bool HasSave => PlayerPrefs.HasKey(LastKey);
        /// <summary>The epilogue has been reached at least once.</summary>
        public static bool Finished => PlayerPrefs.GetInt(FinishedKey, 0) == 1;
        private const string FinishedKey = "tk.finished";

        /// <summary>Editor and development builds unlock every chapter for testing; "unlock" in the console does it in release.</summary>
        public static bool UnlockAll
        {
            get => Application.isEditor || Debug.isDebugBuild || PlayerPrefs.GetInt(UnlockKey, 0) == 1;
            set { PlayerPrefs.SetInt(UnlockKey, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static bool IsUnlocked(int chapter) => chapter <= 1 || UnlockAll || Finished || chapter <= Reached;

        public static void MarkFinished()
        {
            PlayerPrefs.SetInt(FinishedKey, 1);
            PlayerPrefs.Save();
        }

        public static void MarkPlaying(int chapter)
        {
            PlayerPrefs.SetInt(LastKey, chapter);
            if (chapter > Reached) PlayerPrefs.SetInt(ReachedKey, chapter);
            PlayerPrefs.Save();
        }

        public static void Reset()
        {
            PlayerPrefs.DeleteKey(ReachedKey);
            PlayerPrefs.DeleteKey(LastKey);
            PlayerPrefs.DeleteKey(UnlockKey);
            PlayerPrefs.DeleteKey(FinishedKey);
            PlayerPrefs.Save();
        }

        /// <summary>Ink state saved when the previous chapter was completed, if any.</summary>
        public static string LoadInkStateFor(int chapter)
        {
            string path = Path.Combine(Application.persistentDataPath, "Chapter" + (chapter - 1) + ".inkstate.json");
            return File.Exists(path) ? File.ReadAllText(path) : null;
        }
    }
}
