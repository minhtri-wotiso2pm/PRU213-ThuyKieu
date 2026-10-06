using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Renders still views of the story scenes without entering Play Mode, for art reviews.
    /// Shoots the main camera's saved pose plus every transform named CAM_*, WS_* or LOOK_*.
    ///   -executeMethod ThuyKieu.Core.Editor.StoryLookCapture.Batch -lookOut &lt;folder&gt;
    /// </summary>
    public static class StoryLookCapture
    {
        private static readonly string[] Scenes =
        {
            Chapter01Setup.ScenePath, Chapter02Setup.ScenePath, Chapter03Setup.ScenePath
        };

        public static void Batch()
        {
            try
            {
                var args = System.Environment.GetCommandLineArgs();
                int index = Array.IndexOf(args, "-lookOut");
                string folder = index >= 0 && index + 1 < args.Length ? args[index + 1] : "Temp/LookCapture";
                foreach (string scene in Scenes) Capture(scene, folder);
                EditorApplication.Exit(0);
            }
            catch (System.Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }

        public static void Capture(string scenePath, string folder)
        {
            Directory.CreateDirectory(folder);
            EditorSceneManager.OpenScene(scenePath);
            var camera = Camera.main;
            if (camera == null) return;
            string prefix = Path.GetFileNameWithoutExtension(scenePath).Substring(0, 9);
            Shoot(camera, Path.Combine(folder, prefix + "_00_main.png"));
            var views = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include)
                .Where(t => t.name.StartsWith("CAM_") || t.name.StartsWith("WS_") || t.name.StartsWith("LOOK_"))
                .OrderBy(t => t.name).ToArray();
            Vector3 position = camera.transform.position;
            Quaternion rotation = camera.transform.rotation;
            foreach (var view in views)
            {
                camera.transform.SetPositionAndRotation(view.position, view.rotation);
                Shoot(camera, Path.Combine(folder, prefix + "_" + view.name + ".png"));
            }
            camera.transform.SetPositionAndRotation(position, rotation);
        }

        private static void Shoot(Camera camera, string file, int width = 1600, int height = 900)
        {
            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var previous = camera.targetTexture;
            camera.targetTexture = target;
            camera.Render();
            camera.targetTexture = previous;
            RenderTexture.active = target;
            var image = new Texture2D(width, height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            image.Apply();
            RenderTexture.active = null;
            File.WriteAllBytes(file, image.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(image);
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
        }
    }
}
