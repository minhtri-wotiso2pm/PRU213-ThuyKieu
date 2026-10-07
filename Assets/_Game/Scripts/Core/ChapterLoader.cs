using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThuyKieu.Core
{
    /// <summary>
    /// Starts chapters from menus, the dev console or the command line: prepares the carried Ink state
    /// (fresh or the choices saved when the previous chapter was finished), clears any pause and loads the scene.
    /// </summary>
    public static class ChapterLoader
    {
        /// <summary>Optional scene loader with a transition (the game shell fades out first).</summary>
        public static Action<string> SceneLoader;

        public static bool LoadChapter(int number, bool keepChoices) => LoadChapter(ChapterCatalog.Get(number), keepChoices);

        public static bool LoadChapter(ChapterCatalog.Entry entry, bool keepChoices)
        {
            if (!ChapterCatalog.InBuild(entry))
            {
                Debug.LogWarning("[ChapterLoader] Chapter not in build settings: " + (entry != null ? entry.Scene : "?"));
                return false;
            }
            var session = InkStatePersistence.Ensure();
            string json = keepChoices ? GameProgress.LoadInkStateFor(entry.Number) : null;
            if (json != null) session.Restore(json, entry.Number);
            else session.Clear();
            return LoadScene(entry.Scene);
        }

        /// <summary>Last chapter played, with the choices saved on finishing the one before it.</summary>
        public static bool Continue() => GameProgress.HasSave && LoadChapter(GameProgress.Last, true);

        public static bool LoadMainMenu() => LoadScene(ChapterCatalog.MainMenuScene);

        public static bool LoadScene(string scene)
        {
            if (!Application.CanStreamedLevelBeLoaded(scene))
            {
                Debug.LogWarning("[ChapterLoader] Scene not in build settings: " + scene);
                return false;
            }
            ClearPause();
            if (SceneLoader != null) SceneLoader(scene);
            else SceneManager.LoadScene(scene);
            return true;
        }

        public static void ClearPause()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            GamePause.IsPaused = false;
            GamePause.ShellOverlayOpen = false;
        }

        public static void Quit()
        {
            ClearPause();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
