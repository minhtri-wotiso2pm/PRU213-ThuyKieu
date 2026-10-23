using System;
using System.IO;
using System.Linq;
using ThuyKieu.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Main menu scene and quick chapter testing from the Editor.
    ///   Build:   ThuyKieu > Menu > Build Main Menu scene   (batch: -executeMethod ThuyKieu.Core.Editor.MainMenuSetup.BuildBatch)
    ///   Test:    ThuyKieu > Play Chapter > Chapter N       (enters Play mode in that chapter without changing the open scene)
    /// The menu itself is drawn at runtime by GameShell, so the scene only holds a camera.
    /// </summary>
    [InitializeOnLoad]
    public static class MainMenuSetup
    {
        public const string ScenePath = "Assets/_Game/Scenes/MainMenu.unity";
        public const string ConfigPath = "Assets/_Game/Resources/GameShellConfig.asset";
        private const string StartSceneKey = "ThuyKieu.PlayChapterStartScene";

        static MainMenuSetup()
        {
            // Play Chapter swaps the play-mode start scene only for one session of Play mode.
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(StartSceneKey, false))
                {
                    EditorSceneManager.playModeStartScene = null;
                    SessionState.SetBool(StartSceneKey, false);
                }
            };
        }

        [MenuItem("ThuyKieu/Menu/Build Main Menu scene")]
        public static void Build()
        {
            EnsureConfig();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            cameraObject.AddComponent<AudioListener>();
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = Color.black;
            RenderSettings.skybox = null;
            EditorSceneManager.SaveScene(scene, ScenePath);

            var scenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("[MainMenu] Built " + ScenePath + " as build scene 0.");
        }

        public static void BuildBatch()
        {
            try { Build(); EditorApplication.Exit(0); }
            catch (Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }

        /// <summary>The shell loads its Vietnamese font from Resources so it can boot in any scene.</summary>
        private static void EnsureConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<GameShellConfig>(ConfigPath);
            if (config == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));
                config = ScriptableObject.CreateInstance<GameShellConfig>();
                AssetDatabase.CreateAsset(config, ConfigPath);
            }
            config.Font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(StorySceneKit.FontPath);
            if (config.Font == null) Debug.LogWarning("[MainMenu] Missing font " + StorySceneKit.FontPath);
            EditorUtility.SetDirty(config);
        }

        // ---------------------------------------------------------------- Play Chapter

        [MenuItem("ThuyKieu/Play Chapter/Main Menu", priority = 0)] private static void PlayMenu() => Play(ScenePath);
        [MenuItem("ThuyKieu/Play Chapter/Chapter 1 – Gia biến", priority = 20)] private static void Play1() => PlayChapter(1);
        [MenuItem("ThuyKieu/Play Chapter/Chapter 2 – Trên đường", priority = 21)] private static void Play2() => PlayChapter(2);
        [MenuItem("ThuyKieu/Play Chapter/Chapter 3 – Tú Bà", priority = 22)] private static void Play3() => PlayChapter(3);
        [MenuItem("ThuyKieu/Play Chapter/Chapter 4 – Không còn đường lui", priority = 23)] private static void Play4() => PlayChapter(4);
        [MenuItem("ThuyKieu/Play Chapter/Chapter 5 – Đào thoát", priority = 24)] private static void Play5() => PlayChapter(5);
        [MenuItem("ThuyKieu/Play Chapter/Chapter 6 – Kim Trọng", priority = 25)] private static void Play6() => PlayChapter(6);
        [MenuItem("ThuyKieu/Play Chapter/Chapter 7 (placeholder)", priority = 26)] private static void Play7() => PlayChapter(7);

        private static void PlayChapter(int number)
        {
            var entry = ChapterCatalog.Get(number);
            var path = EditorBuildSettings.scenes.Select(s => s.path).FirstOrDefault(p => Path.GetFileNameWithoutExtension(p) == entry.Scene)
                       ?? "Assets/_Game/Scenes/" + entry.Scene + ".unity";
            Play(path);
        }

        private static void Play(string path)
        {
            if (EditorApplication.isPlaying) { Debug.LogWarning("[PlayChapter] Stop Play mode first."); return; }
            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            if (scene == null) { Debug.LogError("[PlayChapter] Scene not found: " + path); return; }
            EditorSceneManager.playModeStartScene = scene;
            SessionState.SetBool(StartSceneKey, true);
            EditorApplication.EnterPlaymode();
        }
    }
}
