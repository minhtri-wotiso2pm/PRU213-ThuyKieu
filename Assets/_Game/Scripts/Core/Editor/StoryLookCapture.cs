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
    ///   -executeMethod ThuyKieu.Core.Editor.StoryLookCapture.Batch -lookOut &lt;folder&gt; [-lookScenes 4,5,6]
    /// </summary>
    public static class StoryLookCapture
    {
        private static readonly string[] Scenes =
        {
            PrologueSetup.ScenePath, Chapter01Setup.ScenePath, Chapter02Setup.ScenePath, Chapter03Setup.ScenePath,
            Chapter04Setup.ScenePath, Chapter05Setup.ScenePath, Chapter06Setup.ScenePath, Chapter07Setup.ScenePath, EpilogueSetup.ScenePath
        };

        public static void Batch()
        {
            try
            {
                var args = System.Environment.GetCommandLineArgs();
                int index = Array.IndexOf(args, "-lookOut");
                string folder = index >= 0 && index + 1 < args.Length ? args[index + 1] : "Temp/LookCapture";
                // Optional filter: -lookScenes 4,5,6 shoots only those chapters.
                int filter = Array.IndexOf(args, "-lookScenes");
                var only = filter >= 0 && filter + 1 < args.Length ? args[filter + 1].Split(',') : null;
                foreach (string scene in Scenes)
                    if (only == null || only.Any(n => Path.GetFileName(scene).StartsWith("Chapter0" + n.Trim())))
                        Capture(scene, folder);
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
            // The chapter grade is a runtime volume built in ScreenFx.Awake; build it here too so stills match Play Mode.
            // The scene is never saved afterwards.
            foreach (var fx in UnityEngine.Object.FindObjectsByType<ThuyKieu.Presentation.ScreenFx>())
                typeof(ThuyKieu.Presentation.ScreenFx).GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.Invoke(fx, null);
            // Throwaway frame: shaders still compiling asynchronously render as cyan placeholders on the first pass.
            Shoot(camera, Path.Combine(folder, prefix + "_00_main.png"));
            Shoot(camera, Path.Combine(folder, prefix + "_00_main.png"));
            var views = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include)
                .Where(t => t.name.StartsWith("CAM_") || t.name.StartsWith("WS_") || t.name.StartsWith("LOOK_"))
                .OrderBy(t => t.name).ToArray();
            Vector3 position = camera.transform.position;
            Quaternion rotation = camera.transform.rotation;
            var roofs = UnityEngine.Object.FindObjectsByType<ThuyKieu.Environment.RoofHider>();
            foreach (var view in views)
            {
                camera.transform.SetPositionAndRotation(view.position, view.rotation);
                // Play hides a room's roof while the player is inside; do the same for a camera inside the room.
                foreach (var roof in roofs) SetRoof(roof, view.position);
                Shoot(camera, Path.Combine(folder, prefix + "_" + view.name + ".png"));
            }
            foreach (var roof in roofs) SetRoof(roof, new Vector3(9999f, 0f, 0f));
            camera.transform.SetPositionAndRotation(position, rotation);
        }

        private static void SetRoof(ThuyKieu.Environment.RoofHider roof, Vector3 point)
        {
            var serialized = new SerializedObject(roof);
            var bounds = new Bounds(serialized.FindProperty("_centre").vector3Value, serialized.FindProperty("_size").vector3Value + Vector3.up * 2f);
            bool inside = bounds.Contains(point);
            foreach (var renderer in roof.GetComponentsInChildren<Renderer>())
                renderer.shadowCastingMode = inside ? UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly : UnityEngine.Rendering.ShadowCastingMode.On;
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
