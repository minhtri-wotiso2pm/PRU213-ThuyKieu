using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Turns an image-to-3D prop export (Art/Environment/Props/&lt;Name&gt;.fbx with its PBR maps embedded) into a game prop the
    /// same way the existing ones are set up: textures extracted to &lt;Name&gt;.fbm, a URP Lit M_&lt;Name&gt; material remapped onto the
    /// model, no animation, mesh LODs on, and Prefabs/Props/&lt;Name&gt;.prefab with the model grounded, centred and scaled to its
    /// real size under a root carrying a box collider. Safe to re-run.
    ///
    ///   Menu: ThuyKieu/Tools/Set up generated props (Prologue)
    /// </summary>
    public static class GeneratedPropSetup
    {
        public enum Fit { Height, Length }

        private const string ModelFolder = "Assets/_Game/Art/Environment/Props";
        private const string MaterialFolder = "Assets/_Game/Materials/Environment";
        private const string PrefabFolder = "Assets/_Game/Prefabs/Props";

        [MenuItem("ThuyKieu/Tools/Set up generated props (Prologue)")]
        public static void SetupPrologueProps()
        {
            Setup("KhungTheu", Fit.Length, 1.2f);
            Setup("BinhPhong", Fit.Height, 1.5f);
            Setup("ChauMai", Fit.Height, 1f);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("ThuyKieu/Tools/Set up generated props (Chapters 1–2)")]
        public static void SetupRoadProps()
        {
            Setup("TuiBac", Fit.Height, 0.18f);
            Setup("CayNhan", Fit.Height, 6f);
            Setup("QuanNuoc", Fit.Length, 6f);
            AssetDatabase.SaveAssets();
        }

        /// <param name="size">Real height, or real length of the longest horizontal side, in metres.</param>
        public static GameObject Setup(string name, Fit fit, float size)
        {
            string modelPath = ModelFolder + "/" + name + ".fbx";
            var importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
            if (importer == null) throw new FileNotFoundException("Missing generated model", modelPath);

            string textureFolder = ModelFolder + "/" + name + ".fbm";
            if (!AssetDatabase.IsValidFolder(textureFolder))
            {
                importer.ExtractTextures(textureFolder);
                AssetDatabase.Refresh();
            }
            // Exports name their maps differently (texture_pbr_* or Tripo's *_BaseColor); only the colour map is required.
            string albedo = Map(textureFolder, "texture_pbr_20250901.png", "_BaseColor", "_Albedo", "_Diffuse")
                ?? throw new FileNotFoundException("Missing colour map in", textureFolder);
            string normal = Map(textureFolder, "texture_pbr_20250901_normal.png", "_Normal", "_normal");
            string metallic = Map(textureFolder, "texture_pbr_20250901_metallic.png", "_Metallic", "_metallic");
            ConfigureTexture(albedo, false);
            if (normal != null) ConfigureTexture(normal, true);
            if (metallic != null) ConfigureTexture(metallic, false);

            var material = UpsertMaterial(name, albedo, normal, metallic);
            importer.animationType = ModelImporterAnimationType.None;
            importer.importAnimation = false;
            importer.generateMeshLods = true;
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            foreach (string source in model.GetComponentsInChildren<Renderer>(true).SelectMany(r => r.sharedMaterials).Where(m => m != null).Select(m => m.name).Distinct())
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), source), material);
            importer.SaveAndReimport();
            return SavePrefab(name, AssetDatabase.LoadAssetAtPath<GameObject>(modelPath), fit, size);
        }

        /// <summary>The first image in the folder with the exact name, or whose name contains one of the markers.</summary>
        private static string Map(string folder, string exact, params string[] markers)
        {
            if (File.Exists(folder + "/" + exact)) return folder + "/" + exact;
            foreach (string file in Directory.GetFiles(folder, "*.png").Concat(Directory.GetFiles(folder, "*.jpg")))
                if (markers.Any(m => Path.GetFileNameWithoutExtension(file).EndsWith(m))) return file.Replace('\\', '/');
            return null;
        }

        private static void ConfigureTexture(string path, bool normalMap)
        {
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer) throw new FileNotFoundException("Missing extracted texture", path);
            var type = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
            if (importer.textureType == type && importer.maxTextureSize <= 2048) return;
            importer.textureType = type;
            importer.maxTextureSize = Math.Min(importer.maxTextureSize, 2048);
            importer.SaveAndReimport();
        }

        private static Material UpsertMaterial(string name, string albedo, string normal, string metallic)
        {
            string path = MaterialFolder + "/M_" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(albedo));
            material.SetTexture("_BumpMap", normal != null ? AssetDatabase.LoadAssetAtPath<Texture2D>(normal) : null);
            material.SetTexture("_MetallicGlossMap", metallic != null ? AssetDatabase.LoadAssetAtPath<Texture2D>(metallic) : null);
            if (normal != null) material.EnableKeyword("_NORMALMAP"); else material.DisableKeyword("_NORMALMAP");
            if (metallic != null) material.EnableKeyword("_METALLICSPECGLOSSMAP"); else material.DisableKeyword("_METALLICSPECGLOSSMAP");
            material.SetFloat("_Smoothness", 0.25f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static GameObject SavePrefab(string name, GameObject modelAsset, Fit fit, float size)
        {
            var root = new GameObject(name);
            try
            {
                var model = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
                model.transform.SetParent(root.transform, false);
                Bounds bounds = BoundsOf(model);
                float measured = fit == Fit.Height ? bounds.size.y : Mathf.Max(bounds.size.x, bounds.size.z);
                model.transform.localScale *= size / measured;
                bounds = BoundsOf(model);
                model.transform.localPosition -= new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
                bounds = BoundsOf(model);
                var collider = root.AddComponent<BoxCollider>();
                collider.center = bounds.center;
                collider.size = bounds.size;
                root.layer = StorySceneKit.EnvironmentLayer;
                return PrefabUtility.SaveAsPrefabAsset(root, PrefabFolder + "/" + name + ".prefab");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static Bounds BoundsOf(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            Bounds bounds = renderers[0].bounds;
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            return bounds;
        }
    }
}
