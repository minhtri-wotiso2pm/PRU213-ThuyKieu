using System;
using System.Linq;
using System.Reflection;
using ThuyKieu.Gameplay.Chase;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ThuyKieu.Core.Editor.StorySceneKit;

namespace ThuyKieu.Core.Editor
{
    public static class Chapter05ObstacleSetup
    {
        internal static void Configure(ChaseObstacle obstacle)
        {
            Vector3 foot = obstacle.transform.position;
            bool eastLane = foot.x > 80f && foot.z > 10f;
            bool upperSide = foot.z > (foot.z < 10f ? 4f : 25f);
            Vector3 fall = eastLane ? new Vector3(0f, 0f, 80f) : new Vector3(upperSide ? -80f : 80f, 0f, 0f);
            float shift = foot.z < 10f ? (upperSide ? -0.15f : 0.15f) : (upperSide ? -2f : 1.8f);
            Vector3 offset = eastLane ? new Vector3(-0.3f, 0f, 0f) : new Vector3(0f, 0f, shift);
            Set(obstacle, "_fall", fall);
            Set(obstacle, "_fallOffset", offset);
            var data = new SerializedObject(obstacle);
            var highlight = (GameObject)data.FindProperty("_highlight").objectReferenceValue;
            if (highlight == null)
            {
                highlight = Marker(obstacle.transform, 1.8f);
                highlight.transform.localScale = Vector3.one * 0.14f;
                Set(obstacle, "_highlight", highlight);
            }
        }

        [MenuItem("ThuyKieu/Chapter 5/Make toppled obstacles visible")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            var scene = SceneManager.GetSceneByPath(Chapter05Setup.ScenePath);
            if (!scene.isLoaded) scene = EditorSceneManager.OpenScene(Chapter05Setup.ScenePath, OpenSceneMode.Additive);
            var obstacles = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<ChaseObstacle>(true)).ToArray();
            if (obstacles.Length != 7) throw new InvalidOperationException("Expected seven chase obstacles.");
            foreach (var obstacle in obstacles) Configure(obstacle);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save obstacle changes.");
            AssetDatabase.SaveAssets();
            foreach (var obstacle in obstacles) Validate(obstacle);
            Debug.Log("Obstacle checks passed: all seven props remain above the ground, cover the running lane, and reset correctly.");
        }

        private static void Validate(ChaseObstacle original)
        {
            var copy = UnityEngine.Object.Instantiate(original.gameObject);
            try
            {
                var obstacle = copy.GetComponent<ChaseObstacle>();
                var flags = BindingFlags.Instance | BindingFlags.NonPublic;
                typeof(ChaseObstacle).GetMethod("Awake", flags).Invoke(obstacle, null);
                var data = new SerializedObject(obstacle);
                var visual = (Transform)data.FindProperty("_visual").objectReferenceValue;
                Vector3 uprightPosition = visual.localPosition;
                Quaternion uprightRotation = visual.localRotation;
                Quaternion end = uprightRotation * Quaternion.Euler(data.FindProperty("_fall").vector3Value);
                typeof(ChaseObstacle).GetMethod("FallenPosition", flags).Invoke(obstacle, new object[] { end });
                var renderers = visual.GetComponentsInChildren<Renderer>();
                Bounds bounds = renderers[0].bounds;
                foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
                Vector3 foot = obstacle.transform.position;
                bool eastLane = foot.x > 80f && foot.z > 10f;
                float road = eastLane ? 82f : foot.z < 10f ? 4f : 25f;
                if (bounds.min.y < foot.y + 0.02f) throw new InvalidOperationException(original.name + " sinks into the ground.");
                if (eastLane ? bounds.min.x > road || bounds.max.x < road : bounds.min.z > road || bounds.max.z < road)
                    throw new InvalidOperationException(original.name + " does not cover the running lane.");
                typeof(ChaseObstacle).GetProperty("Toppled").SetValue(obstacle, true);
                if (Vector3.Distance(obstacle.SlowCenter, bounds.center) > 0.001f)
                    throw new InvalidOperationException(original.name + " slowdown does not match its debris.");
                if (original.name.Contains("xe kéo")) CapturePreview(copy, foot);
                obstacle.ResetObstacle();
                if (Vector3.Distance(visual.localPosition, uprightPosition) > 0.001f || Quaternion.Angle(visual.localRotation, uprightRotation) > 0.01f)
                    throw new InvalidOperationException(original.name + " does not reset correctly.");
            }
            finally { UnityEngine.Object.DestroyImmediate(copy); }
        }

        private static void CapturePreview(GameObject obstacle, Vector3 foot)
        {
            var preview = new PreviewRenderUtility();
            Material floorMaterial = null;
            try
            {
                preview.AddSingleGO(UnityEngine.Object.Instantiate(obstacle));
                var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.transform.position = foot + new Vector3(0f, -0.02f, 1f);
                floor.transform.localScale = new Vector3(6f, 0.03f, 2.6f);
                floorMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                floorMaterial.SetColor("_BaseColor", new Color(0.25f, 0.28f, 0.32f));
                floor.GetComponent<Renderer>().sharedMaterial = floorMaterial;
                preview.AddSingleGO(floor);
                preview.camera.nearClipPlane = 0.01f;
                preview.camera.farClipPlane = 100f;
                preview.camera.orthographic = true;
                preview.camera.orthographicSize = 2.2f;
                preview.camera.transform.position = foot + new Vector3(-3.5f, 3f, -4f);
                preview.camera.transform.LookAt(foot + new Vector3(0f, 0.4f, 0.8f));
                preview.lights[0].intensity = 1.4f;
                preview.lights[0].transform.rotation = Quaternion.Euler(40f, 30f, 0f);
                preview.lights[1].intensity = 0.6f;
                preview.BeginStaticPreview(new Rect(0f, 0f, 640f, 480f));
                preview.Render(true);
                var image = preview.EndStaticPreview();
                System.IO.File.WriteAllBytes(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ThuyKieu-obstacle-preview.png"), image.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(image);
            }
            finally
            {
                preview.Cleanup();
                if (floorMaterial != null) UnityEngine.Object.DestroyImmediate(floorMaterial);
            }
        }
    }
}
