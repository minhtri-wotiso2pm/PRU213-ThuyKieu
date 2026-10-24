using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// QA contact sheet for the props the house could reuse: each prefab is rendered alone under a neutral key light, and
    /// its size and triangle count are listed, so choosing between "reuse" and "build" is made from pictures and numbers.
    /// Batch: -executeMethod ThuyKieu.Core.Editor.LauTuBaAssetSheet.Batch → Tools/LauTuBa/AssetSheet.
    /// </summary>
    public static class LauTuBaAssetSheet
    {
        private const string Out = "Tools/LauTuBa/AssetSheet";

        public static void Batch()
        {
            try { Run(); EditorApplication.Exit(0); }
            catch (System.Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }

        [MenuItem("ThuyKieu/Lầu Tú Bà/Asset Contact Sheet")]
        public static void Run()
        {
            Directory.CreateDirectory(Out);
            var report = new StringBuilder();
            var prefabs = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Game/Prefabs/Props", "Assets/_Game/Prefabs/Vegetation", "Assets/_Game/Prefabs/Architecture" })
                .Select(AssetDatabase.GUIDToAssetPath).ToArray();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var key = new GameObject("Key").AddComponent<Light>();
            key.type = LightType.Directional;
            key.intensity = 1.4f;
            key.transform.rotation = Quaternion.Euler(40f, -35f, 0f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.45f, 0.45f, 0.48f);
            var camera = new GameObject("Camera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.22f, 0.22f, 0.24f);
            camera.fieldOfView = 35f;
            var target = new RenderTexture(640, 640, 24);
            camera.targetTexture = target;
            foreach (string path in prefabs)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                var renderers = instance.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0) { Object.DestroyImmediate(instance); continue; }
                var bounds = renderers[0].bounds;
                foreach (var r in renderers) bounds.Encapsulate(r.bounds);
                long triangles = 0;
                foreach (var filter in instance.GetComponentsInChildren<MeshFilter>()) if (filter.sharedMesh != null) triangles += filter.sharedMesh.triangles.Length / 3;
                foreach (var skinned in instance.GetComponentsInChildren<SkinnedMeshRenderer>()) if (skinned.sharedMesh != null) triangles += skinned.sharedMesh.triangles.Length / 3;
                float radius = bounds.extents.magnitude;
                camera.transform.position = bounds.center + new Vector3(0.75f, 0.45f, -1f).normalized * radius / Mathf.Sin(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
                camera.transform.LookAt(bounds.center);
                camera.nearClipPlane = radius * 0.05f;
                camera.farClipPlane = radius * 10f;
                camera.Render();
                RenderTexture.active = target;
                var picture = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
                picture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
                RenderTexture.active = null;
                string name = Path.GetFileNameWithoutExtension(path);
                File.WriteAllBytes(Path.Combine(Out, name + ".png"), picture.EncodeToPNG());
                Object.DestroyImmediate(picture);
                report.AppendLine($"{name,-22} {triangles,10:N0} tris  size {bounds.size.x:F2} × {bounds.size.y:F2} × {bounds.size.z:F2} m  materials {renderers.Sum(r => r.sharedMaterials.Length)}  ({path})");
                Object.DestroyImmediate(instance);
            }
            File.WriteAllText(Path.Combine(Out, "Sheet.txt"), report.ToString());
            Debug.Log("[AssetSheet]\n" + report);
        }
    }
}
