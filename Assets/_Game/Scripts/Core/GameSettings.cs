using System;
using UnityEngine;

namespace ThuyKieu.Core
{
    /// <summary>
    /// Player options, stored in PlayerPrefs. Audio, dialogue, camera and screen effects read the values directly;
    /// display settings and the master volume are applied here. <see cref="Changed"/> fires after every edit.
    /// </summary>
    public static class GameSettings
    {
        private const string Prefix = "tk.settings.";

        public static event Action Changed;

        // ---------------------------------------------------------------- audio (0–1)
        public static float Master { get => Get("master", 1f); set => Set("master", Mathf.Clamp01(value)); }
        public static float Music { get => Get("music", 0.8f); set => Set("music", Mathf.Clamp01(value)); }
        public static float Effects { get => Get("sfx", 0.9f); set => Set("sfx", Mathf.Clamp01(value)); }
        public static float Ambience { get => Get("ambience", 0.8f); set => Set("ambience", Mathf.Clamp01(value)); }
        public static float Voice { get => Get("voice", 1f); set => Set("voice", Mathf.Clamp01(value)); }

        // ---------------------------------------------------------------- dialogue
        /// <summary>Typewriter speed multiplier: 0.5 slow … 2 fast; 4 shows lines at once.</summary>
        public static float TextSpeed { get => Get("textSpeed", 1f); set => Set("textSpeed", Mathf.Clamp(value, 0.5f, 4f)); }
        public static bool VoiceOn { get => Get("voiceOn", 1f) > 0.5f; set => Set("voiceOn", value ? 1f : 0f); }
        public static bool AutoAdvance { get => Get("autoAdvance", 0f) > 0.5f; set => Set("autoAdvance", value ? 1f : 0f); }

        // ---------------------------------------------------------------- controls
        /// <summary>Mouse look multiplier, 0.25–3.</summary>
        public static float MouseSensitivity { get => Get("mouse", 1f); set => Set("mouse", Mathf.Clamp(value, 0.25f, 3f)); }
        public static bool InvertY { get => Get("invertY", 0f) > 0.5f; set => Set("invertY", value ? 1f : 0f); }
        /// <summary>Camera shakes and jolts in panic and action beats (0 turns them off).</summary>
        public static float ShakeAmount { get => Get("shake", 1f); set => Set("shake", Mathf.Clamp01(value)); }

        // ---------------------------------------------------------------- display
        /// <summary>Image brightness multiplier, 0.5–2; 1 preserves the chapter's authored grade.</summary>
        public static float Brightness { get => Mathf.Clamp(Get("brightness", 1f), 0.5f, 2f); set => Set("brightness", Mathf.Clamp(value, 0.5f, 2f)); }
        public static int ResolutionIndex { get => PlayerPrefs.GetInt(Prefix + "resolution", -1); set { PlayerPrefs.SetInt(Prefix + "resolution", value); Save(); } }
        public static bool Fullscreen { get => Get("fullscreen", 1f) > 0.5f; set => Set("fullscreen", value ? 1f : 0f); }
        public static int Quality { get => PlayerPrefs.GetInt(Prefix + "quality", QualitySettings.GetQualityLevel()); set { PlayerPrefs.SetInt(Prefix + "quality", value); Save(); } }
        public static bool VSync { get => Get("vsync", 1f) > 0.5f; set => Set("vsync", value ? 1f : 0f); }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            // QA runs keep the project's window and quality settings.
            if (Application.isBatchMode) return;
            ApplyDisplay();
            AudioListener.volume = Master;
        }

        /// <summary>Distinct resolutions the monitor supports, largest last.</summary>
        public static Resolution[] Resolutions()
        {
            var all = Screen.resolutions;
            var list = new System.Collections.Generic.List<Resolution>();
            foreach (var r in all)
                if (!list.Exists(o => o.width == r.width && o.height == r.height) && r.width >= 1024) list.Add(r);
            if (list.Count == 0) list.Add(Screen.currentResolution);
            return list.ToArray();
        }

        public static void ApplyDisplay()
        {
            if (Application.isEditor) { QualitySettings.vSyncCount = VSync ? 1 : 0; return; }
            int quality = Mathf.Clamp(Quality, 0, QualitySettings.names.Length - 1);
            if (QualitySettings.GetQualityLevel() != quality) QualitySettings.SetQualityLevel(quality, true);
            QualitySettings.vSyncCount = VSync ? 1 : 0;
            var resolutions = Resolutions();
            int index = ResolutionIndex;
            var mode = Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            if (index >= 0 && index < resolutions.Length) Screen.SetResolution(resolutions[index].width, resolutions[index].height, mode);
            else Screen.fullScreenMode = mode;
        }

        public static void ResetAll()
        {
            foreach (string key in new[] { "master", "music", "sfx", "ambience", "voice", "textSpeed", "voiceOn", "autoAdvance", "mouse", "invertY", "shake", "brightness", "resolution", "fullscreen", "quality", "vsync" })
                PlayerPrefs.DeleteKey(Prefix + key);
            Save();
            ApplyDisplay();
        }

        private static float Get(string key, float fallback) => PlayerPrefs.GetFloat(Prefix + key, fallback);

        private static void Set(string key, float value)
        {
            PlayerPrefs.SetFloat(Prefix + key, value);
            Save();
        }

        private static void Save()
        {
            PlayerPrefs.Save();
            AudioListener.volume = Master;
            Changed?.Invoke();
        }
    }
}
