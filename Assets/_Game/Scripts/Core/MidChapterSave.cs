using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ThuyKieu.Core
{
    /// <summary>
    /// Save inside a chapter: written whenever the story hands control back to the player (each explore checkpoint), so
    /// "Tiếp tục" resumes at the last hub instead of the chapter's start. Holds the Ink state, where Kiều stood, the
    /// journal clues and the last presentation tags (scene, objective, music, mood) so the world can be set up again.
    /// One slot; finishing a chapter clears it.
    /// </summary>
    public static class MidChapterSave
    {
        [Serializable]
        public sealed class Data
        {
            public int Chapter;
            public string Ink;
            public Vector3 Position;
            public float Yaw;
            public List<string> Clues = new List<string>();
            /// <summary>"key:value" presentation tags replayed on resume, in the order they were last seen.</summary>
            public List<string> Tags = new List<string>();
            public string SavedAt;
        }

        private static string Path => System.IO.Path.Combine(Application.persistentDataPath, "midchapter.json");

        public static void Write(Data data)
        {
            data.SavedAt = DateTime.Now.ToString("HH:mm dd/MM");
            try { File.WriteAllText(Path, JsonUtility.ToJson(data)); }
            catch (Exception exception) { Debug.LogWarning("[MidChapterSave] " + exception.Message); }
        }

        /// <summary>The saved checkpoint, if there is one for this chapter.</summary>
        public static Data Load(int chapter)
        {
            var data = Peek();
            return data != null && data.Chapter == chapter ? data : null;
        }

        public static Data Peek()
        {
            try { return File.Exists(Path) ? JsonUtility.FromJson<Data>(File.ReadAllText(Path)) : null; }
            catch (Exception exception) { Debug.LogWarning("[MidChapterSave] " + exception.Message); return null; }
        }

        public static void Clear()
        {
            if (File.Exists(Path)) File.Delete(Path);
        }
    }
}
