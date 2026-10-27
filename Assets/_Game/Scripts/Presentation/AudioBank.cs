using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThuyKieu.Presentation
{
    /// <summary>
    /// Finds the clip for a music, ambience or effect id.
    /// Order: a recorded clip (Resources/StoryAudio/Recorded/&lt;kind&gt;/&lt;id&gt;), then the clip baked from ProceduralAudio
    /// (Resources/StoryAudio/&lt;kind&gt;/&lt;id&gt;, written by AudioBankBaker), then synthesis as a last resort.
    /// Synthesising a 40-second bed on the main thread stalls a frame for up to a second, so shipped builds rely on the baked files.
    /// </summary>
    public static class AudioBank
    {
        public const string Music = "music", Ambience = "amb", Sfx = "sfx";
        public const string ResourceRoot = "StoryAudio";

        private static readonly Dictionary<string, AudioClip> Cache = new Dictionary<string, AudioClip>();

        public static AudioClip Get(string kind, string id)
        {
            if (string.IsNullOrEmpty(id) || id == "none") return null;
            string key = kind + "/" + id;
            if (Cache.TryGetValue(key, out AudioClip clip)) return clip;
            clip = Resources.Load<AudioClip>(ResourceRoot + "/Recorded/" + key)
                ?? Resources.Load<AudioClip>(ResourceRoot + "/" + key)
                ?? Synthesise(kind)(id);
            if (clip == null) Debug.LogWarning("AudioBank: no clip for " + key);
            // A miss is not cached: with domain reload off the static cache outlives Play sessions, and one
            // transient failure would otherwise mute the id for the whole Editor session.
            if (clip != null) Cache[key] = clip;
            return clip;
        }

        public static Func<string, AudioClip> Synthesise(string kind) =>
            kind == Music ? ProceduralAudio.Music : kind == Ambience ? (Func<string, AudioClip>)ProceduralAudio.Ambience : ProceduralAudio.Sfx;
    }
}
