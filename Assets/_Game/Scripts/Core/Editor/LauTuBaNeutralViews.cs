using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// QA for models: shoots every LOOK_* point of a Lầu Tú Bà scene under neutral light, so shape, construction and
    /// texturing can be judged apart from the game's lighting. Every scene light, probe and fog is switched off; a flat
    /// grey ambient and a white key and fill without shadows (roofs must not black out the rooms) light everything
    /// evenly. Same position, 72° field of view and 1920×1080 as ViewTour's Play Mode stills. The scene is never saved.
    /// Batch: -executeMethod ThuyKieu.Core.Editor.LauTuBaNeutralViews.Batch -neutralScene &lt;path&gt; -neutralOut &lt;folder&gt; -neutralPrefix &lt;name&gt;
    /// </summary>
    public static class LauTuBaNeutralViews
    {
        public static void Batch()
        {
            string scene = Arg("-neutralScene", "Assets/_Game/Scenes/Chapter03_LauTuBa.unity");
            string output = Arg("-neutralOut", "Tools/LauTuBa/Views/neutral");
            string prefix = Arg("-neutralPrefix", "C3");
            Directory.CreateDirectory(output);
            ShaderUtil.allowAsyncCompilation = false;
            EditorSceneManager.OpenScene(scene, OpenSceneMode.Single);

            foreach (var light in Object.FindObjectsByType<Light>(FindObjectsInactive.Include)) light.enabled = false;
            foreach (var probe in Object.FindObjectsByType<ReflectionProbe>(FindObjectsInactive.Include)) probe.enabled = false;
            foreach (var volume in Object.FindObjectsByType<Volume>(FindObjectsInactive.Include)) volume.enabled = false;
            // The player's body and the weather would only get in the way of the furniture.
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include).Where(t => t.name == "Player" || t.name == "Weather"))
                foreach (var renderer in t.GetComponentsInChildren<Renderer>(true)) renderer.enabled = false;
            RenderSettings.fog = false;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.42f, 0.42f, 0.42f);
            RenderSettings.reflectionIntensity = 0.4f;
            Key("NeutralKey", new Vector3(42f, -35f, 0f), 1.1f);
            Key("NeutralFill", new Vector3(25f, 150f, 0f), 0.35f);

            var camera = new GameObject("NeutralCamera").AddComponent<Camera>();
            camera.fieldOfView = 72f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 300f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.5f, 0.5f, 0.5f);
            var looks = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include).Where(t => t.name.StartsWith("LOOK_")).OrderBy(t => t.name).ToArray();
            foreach (var look in looks)
            {
                camera.transform.SetPositionAndRotation(look.position, look.rotation);
                Shoot(camera, Path.Combine(output, $"NEUTRAL_{prefix}_{look.name.Substring(5)}.png"));
            }
            Debug.Log($"[NeutralViews] {looks.Length} views written to {output}");
            EditorApplication.Exit(0);
        }

        private static void Key(string name, Vector3 angles, float intensity)
        {
            var light = new GameObject(name).AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Color.white;
            light.intensity = intensity;
            light.shadows = LightShadows.None;
            light.transform.rotation = Quaternion.Euler(angles);
        }

        private static string Arg(string name, string fallback)
        {
            var args = System.Environment.GetCommandLineArgs();
            int at = System.Array.IndexOf(args, name);
            return at >= 0 && at + 1 < args.Length ? args[at + 1] : fallback;
        }

        private static void Shoot(Camera camera, string path)
        {
            const int width = 1920, height = 1080;
            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            camera.targetTexture = target;
            camera.Render();
            camera.Render();
            RenderTexture.active = target;
            var image = new Texture2D(width, height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            image.Apply();
            File.WriteAllBytes(path, image.EncodeToPNG());
            RenderTexture.active = null;
            camera.targetTexture = null;
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(image);
        }
    }
}
