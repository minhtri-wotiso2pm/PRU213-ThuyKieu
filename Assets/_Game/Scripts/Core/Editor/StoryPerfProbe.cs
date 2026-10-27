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
        /// <summary>"-perfQuick": each view once, without switching groups off.</summary>
        private static bool _quick;

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
            // "-perfLau": only the first-person Chapters 3–4; "-perfCh5": only Chapter 5 (its bamboo chase views).
            var args = System.Environment.GetCommandLineArgs();
            var scenes = args.Contains("-perfCh5") ? new[] { Chapter05Setup.ScenePath }
                : args.Contains("-perfLau") ? new[] { LauTuBaSetup.Chapter3Path, LauTuBaSetup.Chapter4Path }
                : new[] { PrologueSetup.ScenePath, Chapter01StoryUpgrade.ScenePath, Chapter02Setup.ScenePath, Chapter03Setup.ScenePath, Chapter04Setup.ScenePath, LauTuBaSetup.Chapter3Path, LauTuBaSetup.Chapter4Path, Chapter05Setup.ScenePath, Chapter06Setup.ScenePath, Chapter07Setup.ScenePath, EpilogueSetup.ScenePath };
            int set = Array.IndexOf(args, "-perfSet");
            var restore = set >= 0 && set + 1 < args.Length ? TrySettings(args[set + 1], report) : null;
            try
            {
                foreach (string scene in scenes)
                    Probe(scene, report);
            }
            finally { restore?.Invoke(); }
            return report.ToString();
        }

        /// <summary>
        /// "-perfSet scale=1,msaa=4,shadow=2048,extra=2048,soft=2,ssaoHalf=1,cascades=4,distance=75": pipeline settings
        /// tried for this run only, put back after it (nothing is saved). Returns what puts them back.
        /// </summary>
        private static Action TrySettings(string spec, StringBuilder report)
        {
            var asset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (asset == null) return null;
            var undo = new List<Action>();
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Settings/PC_Renderer.asset");
            var ssao = renderer != null ? renderer.rendererFeatures.FirstOrDefault(f => f != null && f.GetType().Name.Contains("Occlusion")) : null;
            foreach (string pair in spec.Split(','))
            {
                var kv = pair.Split('=');
                if (kv.Length != 2) continue;
                float value = float.Parse(kv[1], System.Globalization.CultureInfo.InvariantCulture);
                switch (kv[0])
                {
                    case "scale": { float old = asset.renderScale; asset.renderScale = value; undo.Add(() => asset.renderScale = old); break; }
                    case "msaa": { int old = asset.msaaSampleCount; asset.msaaSampleCount = (int)value; undo.Add(() => asset.msaaSampleCount = old); break; }
                    case "shadow": { int old = asset.mainLightShadowmapResolution; asset.mainLightShadowmapResolution = (int)value; undo.Add(() => asset.mainLightShadowmapResolution = old); break; }
                    case "extra": { int old = asset.additionalLightsShadowmapResolution; asset.additionalLightsShadowmapResolution = (int)value; undo.Add(() => asset.additionalLightsShadowmapResolution = old); break; }
                    case "cascades": { int old = asset.shadowCascadeCount; asset.shadowCascadeCount = (int)value; undo.Add(() => asset.shadowCascadeCount = old); break; }
                    case "distance": { float old = asset.shadowDistance; asset.shadowDistance = value; undo.Add(() => asset.shadowDistance = old); break; }
                    case "soft":
                    {
                        // A fresh snapshot each time: an older one would write back the settings changed since it was taken.
                        var assetObject = new SerializedObject(asset);
                        int old = assetObject.FindProperty("m_SoftShadowQuality").intValue;
                        assetObject.FindProperty("m_SoftShadowQuality").intValue = (int)value;
                        assetObject.ApplyModifiedPropertiesWithoutUndo();
                        undo.Add(() => { var again = new SerializedObject(asset); again.FindProperty("m_SoftShadowQuality").intValue = old; again.ApplyModifiedPropertiesWithoutUndo(); });
                        break;
                    }
                    case "ssaoHalf":
                    {
                        if (ssao == null) break;
                        var featureObject = new SerializedObject(ssao);
                        var p = featureObject.FindProperty("m_Settings.Downsample");
                        bool old = p.boolValue;
                        p.boolValue = value > 0.5f;
                        featureObject.ApplyModifiedPropertiesWithoutUndo();
                        undo.Add(() => { p.boolValue = old; featureObject.ApplyModifiedPropertiesWithoutUndo(); });
                        break;
                    }
                }
            }
            report.AppendLine("== settings tried: " + spec);
            return () => { for (int i = undo.Count - 1; i >= 0; i--) undo[i](); };
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
                .OrderBy(t => t.name).Take(scenePath.Contains("LauTuBa") || scenePath.Contains("Chapter05") ? 16 : 5).Select(t => (t.name, t.position, t.rotation)));

            // As the game window renders: the pipeline's MSAA on the target (a render texture of 1× would turn it off).
            var target = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = Mathf.Max(1, (GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset)?.msaaSampleCount ?? 1)
            };
            if (System.Environment.GetCommandLineArgs().Contains("-perfQuick")) _quick = true;
            var sync = new Texture2D(1, 1, TextureFormat.RGB24, false);
            var ssao = (GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset) != null
                ? AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Settings/PC_Renderer.asset")?.rendererFeatures.FirstOrDefault(f => f != null && f.GetType().Name.Contains("Occlusion"))
                : null;
            var suns = UnityEngine.Object.FindObjectsByType<Light>().Where(l => l.type == LightType.Directional && l.shadows != LightShadows.None).ToArray();
            var lamps = UnityEngine.Object.FindObjectsByType<Light>().Where(l => l.type != LightType.Directional && l.shadows != LightShadows.None).ToArray();
            var toggles = new (string, Action<bool>)[]
            {
                ("grass off", on => SetActive(on, "Grass", "Rice")),
                ("vegetation off", on => SetActive(on, "Vegetation")),
                ("shadows off", on => { foreach (var sun in suns) sun.shadows = on ? LightShadows.Soft : LightShadows.None; }),
                ("lamp shadows off", on => { foreach (var lamp in lamps) lamp.shadows = on ? LightShadows.Hard : LightShadows.None; }),
                ("ssao off", on => { if (ssao != null) ssao.SetActive(on); })
            }.ToList();
            // Chapter 5: what each layer of the bamboo (grown at load, see BambooGrove) and the obstacle cue lights cost.
            var grove = UnityEngine.Object.FindAnyObjectByType<ThuyKieu.Environment.BambooGrove>();
            if (grove != null)
            {
                foreach (string prefix in new[] { "BambooNear_", "BambooSide_", "BambooLeaves_", "BambooBack_", "BambooRoof_", "BambooLow_", "BambooSkyline_" })
                {
                    var group = grove.transform.Cast<Transform>().Where(t => t.name.StartsWith(prefix)).Select(t => t.GetComponent<Renderer>()).Where(r => r != null).ToArray();
                    toggles.Add((prefix.Substring(6).TrimEnd('_').ToLowerInvariant() + " off", on => { foreach (var r in group) r.enabled = on; }));
                }
                var cues = GameObject.Find("ObstacleCues");
                if (cues != null) toggles.Add(("cue lights off", on => cues.SetActive(on)));
                var ferns = grove.transform.Cast<Transform>().Where(t => t.name.StartsWith("Ferns_")).SelectMany(t => t.GetComponentsInChildren<Renderer>()).ToArray();
                toggles.Add(("ferns off", on => { foreach (var r in ferns) r.enabled = on; }));
                var rocks = GameObject.Find("Rocks");
                if (rocks != null) toggles.Add(("rocks off", on => rocks.SetActive(on)));
                // The cost of the forest floor's own material, against plain URP Lit.
                var groundRoot = UnityEngine.Object.FindAnyObjectByType<ThuyKieu.Environment.ForestGround>();
                var floors = groundRoot != null ? groundRoot.GetComponentsInChildren<MeshRenderer>(true) : new MeshRenderer[0];
                var plain = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                var floorMaterial = floors.Length > 0 ? floors[0].sharedMaterial : null;
                if (floorMaterial != null) toggles.Add(("floor plain", on => { foreach (var r in floors) r.sharedMaterial = on ? floorMaterial : plain; }));
                // The cost of the bamboo's own shader (the wind), against URP Lit on the same materials.
                var windy = grove.GetComponentsInChildren<Renderer>(true).SelectMany(r => r.sharedMaterials).Where(m => m != null && m.shader.name == "ThuyKieu/Ch5Bamboo").Distinct().ToArray();
                var bamboo = Shader.Find("ThuyKieu/Ch5Bamboo");
                var lit = Shader.Find("Universal Render Pipeline/Lit");
                if (windy.Length > 0) toggles.Add(("bamboo lit", on => { foreach (var m in windy) { int queue = m.renderQueue; m.shader = on ? bamboo : lit; m.renderQueue = queue; } }));
            }
            foreach (var (name, position, rotation) in views)
            {
                camera.transform.SetPositionAndRotation(position, rotation);
                var line = new StringBuilder($"   {name,-28} {Measure(camera, target, sync),6:F2} ms");
                foreach (var (label, apply) in _quick ? new List<(string, Action<bool>)>() : toggles)
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
