using UnityEditor;
using UnityEditor.SceneManagement;

namespace ThuyKieu.Editor
{
    /// <summary>Quick scene switching from the ThuyKieu menu, so nobody hunts through folders.</summary>
    public static class SceneMenu
    {
        private const string Levels = "Assets/_Project/Scenes/Levels/";
        private const string Sandbox = "Assets/_Project/Scenes/Sandbox/";

        [MenuItem("ThuyKieu/Scenes/KieuHome Interior", priority = 0)]
        private static void OpenInterior() => Open(Levels + "KieuHome_Interior.unity");

        [MenuItem("ThuyKieu/Scenes/KieuHome Exterior", priority = 1)]
        private static void OpenExterior() => Open(Levels + "KieuHome_Exterior.unity");

        [MenuItem("ThuyKieu/Scenes/Sandbox - CoreTest", priority = 50)]
        private static void OpenCoreTest() => Open(Sandbox + "CoreTest.unity");

        private static void Open(string path)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(path);
            }
        }
    }
}
