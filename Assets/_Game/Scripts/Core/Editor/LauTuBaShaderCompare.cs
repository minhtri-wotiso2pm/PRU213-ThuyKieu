using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// QA for the house's surfaces: the same texture on two boards side by side under one veranda lantern of the night
    /// scene, one with URP Lit (mesh UVs scaled to the same metres per tile), one with WorldTriplanarLit. "matched" uses
    /// identical inputs (no ARM map, same constant smoothness) to check the triplanar shader lights like URP Lit;
    /// "production" puts the house's real material on the triplanar board. The scene is opened, never saved.
    /// Batch: -executeMethod ThuyKieu.Core.Editor.LauTuBaShaderCompare.Batch -compareOut &lt;folder&gt;
    /// </summary>
    public static class LauTuBaShaderCompare
    {
        private const string Scene = "Assets/_Game/Scenes/Chapter04_LauTuBaDem.unity";

        public static void Batch()
        {
            string output = "Tools/LauTuBa/Views/shader_compare";
            var args = System.Environment.GetCommandLineArgs();
            int at = System.Array.IndexOf(args, "-compareOut");
            if (at >= 0 && at + 1 < args.Length) output = args[at + 1];
            Directory.CreateDirectory(output);
            ShaderUtil.allowAsyncCompilation = false;
            EditorSceneManager.OpenScene(Scene, OpenSceneMode.Single);

            // Wood: two upright boards 2.4 m long, 1 m tall, facing the camera across the west veranda, the lantern at
            // (-9.6, 2.5, -4.5) midway between them.
            var timber = AssetDatabase.LoadAssetAtPath<Material>(LauTuBaKit.Folder + "/LTB_Timber.mat");
            Pair(output, "wood", timber, new Vector3(-11.75f, 1.15f, -6.3f), new Vector3(-11.75f, 1.15f, -2.7f), new Vector3(0.1f, 1f, 2.4f),
                new Vector3(-9.3f, 1.55f, -4.5f), new Vector3(-11.75f, 1.15f, -4.5f), 0, 2.4f, 1f);
            // Stone: two slabs on the yard paving, lit by the same lantern from the veranda and by the moon.
            var paving = AssetDatabase.LoadAssetAtPath<Material>(LauTuBaKit.Folder + "/LTB_Paving_Night.mat");
            Pair(output, "stone", paving, new Vector3(-7.4f, 0.03f, -5.9f), new Vector3(-7.4f, 0.03f, -3.1f), new Vector3(1.6f, 0.05f, 1.6f),
                new Vector3(-5.2f, 1.45f, -4.5f), new Vector3(-7.4f, 0f, -4.5f), 1, 1.6f, 1.6f);
            Debug.Log("[ShaderCompare] written to " + output);
            EditorApplication.Exit(0);
        }

        /// <param name="face">0: the board's +X face is seen (UV spans z by y), 1: the top face (x by z).</param>
        private static void Pair(string output, string label, Material production, Vector3 urpAt, Vector3 triplanarAt, Vector3 size,
            Vector3 eye, Vector3 lookAt, int face, float uSpan, float vSpan)
        {
            float metres = 1f / Mathf.Max(0.0001f, production.GetTextureScale("_BaseMap").x);
            var urp = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            urp.SetTexture("_BaseMap", production.GetTexture("_BaseMap"));
            urp.SetTexture("_BumpMap", production.GetTexture("_BumpMap"));
            urp.EnableKeyword("_NORMALMAP");
            urp.SetFloat("_BumpScale", production.GetFloat("_BumpScale"));
            urp.SetColor("_BaseColor", production.GetColor("_BaseColor"));
            urp.SetTextureScale("_BaseMap", new Vector2(uSpan / metres, vSpan / metres));
            // Matched triplanar: the production material without the ARM map, grain turn, macro variation or grime.
            var matched = new Material(production);
            matched.DisableKeyword("_ARM_MAP");
            matched.DisableKeyword("_GRAIN_FOLLOWS_OBJECT");
            matched.SetFloat("_Parallax", 0f);
            matched.SetFloat("_OcclusionStrength", 0f);
            float smoothness = production.IsKeywordEnabled("_ARM_MAP") ? production.GetFloat("_Smoothness") * 0.4f : production.GetFloat("_Smoothness");
            matched.SetFloat("_Smoothness", smoothness);
            urp.SetFloat("_Smoothness", smoothness);

            var a = Board("URP_Lit", urpAt, size, urp);
            var b = Board("Triplanar", triplanarAt, size, matched);
            var camera = new GameObject("CompareCamera").AddComponent<Camera>();
            camera.transform.position = eye;
            camera.transform.LookAt(lookAt);
            camera.fieldOfView = 72f;
            camera.nearClipPlane = 0.05f;
            Shoot(camera, Path.Combine(output, $"COMPARE_{label}_matched.png"));
            b.GetComponent<Renderer>().sharedMaterial = production;
            Shoot(camera, Path.Combine(output, $"COMPARE_{label}_production.png"));
            Object.DestroyImmediate(camera.gameObject);
            Object.DestroyImmediate(a);
            Object.DestroyImmediate(b);
        }

        private static GameObject Board(string name, Vector3 centre, Vector3 size, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.position = centre;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        private static void Shoot(Camera camera, string path)
        {
            var target = new RenderTexture(1600, 900, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            camera.targetTexture = target;
            // The first frame after a material change can show stale materials in edit mode: render once to settle.
            camera.Render();
            camera.Render();
            RenderTexture.active = target;
            var image = new Texture2D(1600, 900, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0);
            image.Apply();
            File.WriteAllBytes(path, image.EncodeToPNG());
            RenderTexture.active = null;
            camera.targetTexture = null;
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(image);
        }
    }
}
