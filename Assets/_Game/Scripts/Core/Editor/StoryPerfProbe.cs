using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Debug = UnityEngine.Debug;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Rough GPU+CPU cost per frame of the story scenes from their camera anchors, without Play Mode, plus what
    /// each heavy group (grass, vegetation, shadows, SSAO) costs when switched off. Numbers are for before/after
    /// comparisons on the same machine, not absolute frame rates.
    ///   -executeMethod ThuyKieu.Core.Editor.StoryPerfProbe.Batch -perfOut &lt;file&gt;
    /// </summary>
    public static class StoryPerfProbe
    {
        private const int Frames = 24;

        public static void Batch()
        {
            try
            {
                var args = System.Environment.GetCommandLineArgs();
                int index = Array.IndexOf(args, "-perfOut");
                string file = index >= 0 && index + 1 < args.Length ? args[index + 1] : "Temp/PerfProbe.txt";
                File.WriteAllText(file, Run());
                EditorApplication.Exit(0);
            }
            catch (Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }

        [MenuItem("ThuyKieu/Tools/Perf probe (Ch1-6)")]
        public static void RunFromMenu() => Debug.Log(Run());

        public static string Run()
        {
            var report = new StringBuilder();
            foreach (string scene in new[] { PrologueSetup.ScenePath, Chapter01Setup.ScenePath, Chapter02Setup.ScenePath, Chapter03Setup.ScenePath, Chapter04Setup.ScenePath, Chapter05Setup.ScenePath, Chapter06Setup.ScenePath, Chapter07Setup.ScenePath, EpilogueSetup.ScenePath })
                Probe(scene, report);
            return report.ToString();
        }

        private static void Probe(string scenePath, StringBuilder report)
        {
            EditorSceneManager.OpenScene(scenePath);
            var camera = Camera.main;
            if (camera == null) return;
            report.AppendLine("== " + Path.GetFileNameWithoutExtension(scenePath));
            var world = GameObject.Find("World");
            if (world != null)
                foreach (Transform child in world.transform)
                    report.AppendLine($"   tris {child.name,-24} {Triangles(child.gameObject),9:N0}  renderers {child.GetComponentsInChildren<Renderer>().Length}");
            int lights = UnityEngine.Object.FindObjectsByType<Light>().Count(l => l.enabled && l.gameObject.activeInHierarchy && l.type != LightType.Directional);
            report.AppendLine($"   active point/spot lights {lights}");

            var views = new List<(string, Vector3, Quaternion)> { ("main", camera.transform.position, camera.transform.rotation) };
            views.AddRange(UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include)
                .Where(t => t.name.StartsWith("CAM_Wide") || t.name.StartsWith("WS_") || t.name.StartsWith("LOOK_"))
                .OrderBy(t => t.name).Take(5).Select(t => (t.name, t.position, t.rotation)));

            var target = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
            var sync = new Texture2D(1, 1, TextureFormat.RGB24, false);
            var ssao = (GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset) != null
                ? AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Settings/PC_Renderer.asset")?.rendererFeatures.FirstOrDefault(f => f != null && f.GetType().Name.Contains("Occlusion"))
                : null;
            var suns = UnityEngine.Object.FindObjectsByType<Light>().Where(l => l.type == LightType.Directional && l.shadows != LightShadows.None).ToArray();
            var toggles = new (string, Action<bool>)[]
            {
                ("grass off", on => SetActive(on, "Grass", "Rice")),
                ("vegetation off", on => SetActive(on, "Vegetation")),
                ("shadows off", on => { foreach (var sun in suns) sun.shadows = on ? LightShadows.Soft : LightShadows.None; }),
                ("ssao off", on => { if (ssao != null) ssao.SetActive(on); })
            };
            foreach (var (name, position, rotation) in views)
            {
                camera.transform.SetPositionAndRotation(position, rotation);
                var line = new StringBuilder($"   {name,-28} {Measure(camera, target, sync),6:F2} ms");
                foreach (var (label, apply) in toggles)
                {
                    apply(false);
                    line.Append($" | {label} {Measure(camera, target, sync),6:F2}");
                    apply(true);
                }
                report.AppendLine(line.ToString());
            }
            if (ssao != null) ssao.SetActive(true);
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(sync);
        }

        private static float Measure(Camera camera, RenderTexture target, Texture2D sync)
        {
            var previous = camera.targetTexture;
            camera.targetTexture = target;
            for (int i = 0; i < 3; i++) Render(camera, target, sync);
            var watch = Stopwatch.StartNew();
            for (int i = 0; i < Frames; i++) Render(camera, target, sync);
            watch.Stop();
            camera.targetTexture = previous;
            return (float)watch.Elapsed.TotalMilliseconds / Frames;
        }

        private static void Render(Camera camera, RenderTexture target, Texture2D sync)
        {
            camera.Render();
            // Reading back one pixel waits for the GPU, so the time includes the frame's GPU work.
            RenderTexture.active = target;
            sync.ReadPixels(new Rect(0, 0, 1, 1), 0, 0, false);
            RenderTexture.active = null;
        }

        private static void SetActive(bool active, params string[] names)
        {
            foreach (var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
                if (Array.IndexOf(names, t.name) >= 0 && t.parent != null && (t.parent.name == "World" || t.parent.name == "Vegetation"))
                    t.gameObject.SetActive(active);
        }

        private static long Triangles(GameObject root)
        {
            long total = 0;
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>()) total += Indices(filter.sharedMesh);
            foreach (var skinned in root.GetComponentsInChildren<SkinnedMeshRenderer>()) total += Indices(skinned.sharedMesh);
            return total / 3;
        }

        private static long Indices(Mesh mesh)
        {
            long count = 0;
            if (mesh != null) for (int i = 0; i < mesh.subMeshCount; i++) count += mesh.GetIndexCount(i);
            return count;
        }
    }
}
