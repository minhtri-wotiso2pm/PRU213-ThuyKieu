using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ThuyKieu.Core
{
    public class InkStatePersistence : MonoBehaviour
    {
        public static InkStatePersistence Instance { get; private set; }
        public string StateJson { get; private set; }
        /// <summary>Chapter the saved state is positioned at (the story stops right before its first line).</summary>
        public int NextChapter { get; private set; }
        /// <summary>Journal clues gathered so far, carried into the next chapter.</summary>
        public List<string> Clues { get; } = new List<string>();
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        private void OnDestroy() { if (Instance == this) Instance = null; }
        public void Save(Ink.Runtime.Story story) => Save(story, 2);

        public void Save(Ink.Runtime.Story story, int nextChapter)
        {
            StateJson = story.state.ToJson();
            NextChapter = nextChapter;
            File.WriteAllText(Path.Combine(Application.persistentDataPath, "Chapter" + (nextChapter - 1) + ".inkstate.json"), StateJson);
        }

        /// <summary>Returns the saved state only if it leads into the requested chapter.</summary>
        public bool TryGetStateFor(int chapter, out string json)
        {
            json = NextChapter == chapter ? StateJson : null;
            return !string.IsNullOrEmpty(json);
        }
    }
}
