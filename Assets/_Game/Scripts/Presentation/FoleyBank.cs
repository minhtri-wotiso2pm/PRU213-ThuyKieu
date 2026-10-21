using System;
using UnityEngine;

namespace ThuyKieu.Presentation
{
    /// <summary>
    /// Recorded one-shots for one scene's effects, by sound id (several takes each), set on that scene's
    /// <see cref="StoryAudio"/>. An id found here wins over the shared <see cref="AudioBank"/> clip of the same name, so a
    /// chapter can swap in recorded Foley without changing what other chapters hear. Takes are drawn at random, never the
    /// same take twice in a row. Edited and listed by the Editor tool that builds it (sources and licences in CREDITS.md
    /// next to the asset).
    /// </summary>
    [CreateAssetMenu(menuName = "ThuyKieu/Foley Bank")]
    public class FoleyBank : ScriptableObject
    {
        [Serializable]
        public class Group
        {
            public string Id;
            public AudioClip[] Takes;
            [Tooltip("Level in the game relative to the clips (set so the group sits where the sound it replaces sat).")]
            public float Gain = 1f;
            [NonSerialized] public int Last = -1;
        }

        [SerializeField] private Group[] _groups = new Group[0];

        public Group[] Groups { get => _groups; set => _groups = value; }

        public bool Has(string id) => Find(id) != null;

        /// <summary>A take for this id (not the one played last) and its group's level, or null when the bank has none.</summary>
        public AudioClip Pick(string id, out float gain)
        {
            var group = Find(id);
            gain = group != null ? group.Gain : 1f;
            if (group == null) return null;
            int count = group.Takes.Length;
            int index = UnityEngine.Random.Range(0, count);
            if (count > 1 && index == group.Last) index = (index + 1 + UnityEngine.Random.Range(0, count - 1)) % count;
            group.Last = index;
            return group.Takes[index];
        }

        private Group Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var group in _groups)
                if (group != null && group.Id == id && group.Takes != null && group.Takes.Length > 0) return group;
            return null;
        }
    }
}
