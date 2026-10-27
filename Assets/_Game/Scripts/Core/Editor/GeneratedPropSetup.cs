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

        [MenuItem("ThuyKieu/Tools/Set up generated props (Truyện Kiều book, room art)")]
        public static void SetupStoryProps()
        {
            Setup("baitruyen", Fit.Length, 0.42f);
            Setup("kieuform", Fit.Height, 0.16f);
            Setup("tranhphongkieu", Fit.Height, 1.3f);
            Setup("caudoi", Fit.Height, 1.5f);
            Setup("cauchuc", Fit.Length, 1f);
            BuildOpenBook();
            BuildCoupletPanels();
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// The câu đối export carries both boards in one mesh, but a couplet hangs one board either side of a doorway.
        /// Splits a mid mesh LOD of it (the full mesh is 1.5 M triangles) into CauDoi_Trai and CauDoi_Phai, the boards on
        /// the left and right as seen from in front, each pivoted at its centre and facing -Z like the stand-in boards.
        /// </summary>
        public static void BuildCoupletPanels()
        {
            const int Lod = 4;
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelFolder + "/caudoi.fbx");
            var source = model.GetComponentInChildren<MeshFilter>().sharedMesh;
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/M_caudoi.mat");
            // Into the model's own space with its import scale applied (the asset root sits at the origin).
            Matrix4x4 toModel = model.GetComponentInChildren<MeshFilter>().transform.localToWorldMatrix;

            Vector3[] vertices = source.vertices;
            Vector3[] normals = source.normals;
            Vector2[] uvs = source.uv;
            var range = source.GetLod(0, Mathf.Min(Lod, source.lodCount - 1));
            int[] indices;
            using (var data = Mesh.AcquireReadOnlyMeshData(source))
                indices = data[0].indexFormat == UnityEngine.Rendering.IndexFormat.UInt32
                    ? data[0].GetIndexData<int>().GetSubArray((int)range.indexStart, (int)range.indexCount).ToArray()
                    : data[0].GetIndexData<ushort>().GetSubArray((int)range.indexStart, (int)range.indexCount).ToArray().Select(i => (int)i).ToArray();

            // In the export the boards face +Z, so the board on the viewer's left lies on +X.
            float middle = source.bounds.center.x;
            foreach (var (name, onLeft) in new[] { ("CauDoi_Trai", true), ("CauDoi_Phai", false) })
            {
                Mesh panel = Panel(vertices, normals, uvs, indices, toModel, x => (x > middle) == onLeft, name);
                string meshPath = ModelFolder + "/" + name + ".asset";
                AssetDatabase.DeleteAsset(meshPath);
                AssetDatabase.CreateAsset(panel, meshPath);
                SavePanel(panel, material);
            }
        }

        /// <summary>One board: the triangles on one side, re-indexed, turned to face -Z and centred on its own middle.</summary>
        private static Mesh Panel(Vector3[] vertices, Vector3[] normals, Vector2[] uvs, int[] indices, Matrix4x4 toModel,
            Func<float, bool> side, string name)
        {
            var remap = new System.Collections.Generic.Dictionary<int, int>();
            var outVertices = new System.Collections.Generic.List<Vector3>();
            var outNormals = new System.Collections.Generic.List<Vector3>();
            var outUvs = new System.Collections.Generic.List<Vector2>();
            var outIndices = new System.Collections.Generic.List<int>();
            var turn = Quaternion.Euler(0f, 180f, 0f);
            for (int t = 0; t < indices.Length; t += 3)
            {
                float x = (vertices[indices[t]].x + vertices[indices[t + 1]].x + vertices[indices[t + 2]].x) / 3f;
                if (!side(x)) continue;
                for (int k = 0; k < 3; k++)
                {
                    int index = indices[t + k];
                    if (!remap.TryGetValue(index, out int mapped))
                    {
                        mapped = outVertices.Count;
                        remap[index] = mapped;
                        outVertices.Add(turn * toModel.MultiplyPoint3x4(vertices[index]));
                        outNormals.Add(turn * toModel.MultiplyVector(normals[index]).normalized);
                        outUvs.Add(uvs[index]);
                    }
                    outIndices.Add(mapped);
                }
            }
            // The export stands the two boards in a shallow V; square this one up to the wall. Its width is the direction
            // of greatest spread across the floor plane (the leading eigenvector of the x/z covariance).
            var bounds = GeometryUtility.CalculateBounds(outVertices.ToArray(), Matrix4x4.identity);
            float xx = 0f, xz = 0f, zz = 0f;
            foreach (var v in outVertices)
            {
                float dx = v.x - bounds.center.x, dz = v.z - bounds.center.z;
                xx += dx * dx; xz += dx * dz; zz += dz * dz;
            }
            float widthAngle = 0.5f * Mathf.Atan2(2f * xz, xx - zz) * Mathf.Rad2Deg;
            var square = Quaternion.Euler(0f, widthAngle, 0f);
            for (int i = 0; i < outVertices.Count; i++)
            {
                outVertices[i] = square * (outVertices[i] - bounds.center);
                outNormals[i] = square * outNormals[i];
            }
            var mesh = new Mesh { name = name, indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            mesh.SetVertices(outVertices);
            mesh.SetNormals(outNormals);
            mesh.SetUVs(0, outUvs);
            mesh.SetTriangles(outIndices, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }

        /// <summary>A board prefab sized like the stand-in couplets: 1.6 m tall, no wider than 0.42 m.</summary>
        private static void SavePanel(Mesh mesh, Material material)
        {
            var root = new GameObject(mesh.name);
            try
            {
                root.AddComponent<MeshFilter>().sharedMesh = mesh;
                root.AddComponent<MeshRenderer>().sharedMaterial = material;
                Vector3 size = mesh.bounds.size;
                root.transform.localScale = Vector3.one * Mathf.Min(1.6f / size.y, 0.42f / size.x);
                root.layer = StorySceneKit.EnvironmentLayer;
                PrefabUtility.SaveAsPrefabAsset(root, PrefabFolder + "/" + mesh.name + ".prefab");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        /// <summary>
        /// The classroom copy of Truyện Kiều: the open textbook laid on its back, with the illustration of Kiều holding her
        /// đàn nguyệt pressed flat onto the blank left page under the title, as in the printed edition.
        /// </summary>
        public static GameObject BuildOpenBook()
        {
            var root = new GameObject("SachTruyenKieu");
            try
            {
                var book = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ModelFolder + "/baitruyen.fbx"));
                book.transform.SetParent(root.transform, false);
                // The export stands open towards +Z with the spine at the back; the illustration goes in its own frame,
                // centred in the blank part of the left page, turned with that page and squashed to a print.
                var art = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ModelFolder + "/kieuform.fbx"));
                art.transform.SetParent(book.transform, false);
                const float artHeight = 0.37f;
                float artScale = artHeight / BoundsOf(art).size.y;
                art.transform.localScale = new Vector3(artScale, artScale, artScale * 0.04f);
                // Measured on the export: the title page (+X) leans 27° towards the spine and its surface passes through
                // (0.25, *, 0.12); the print sits about 8 mm proud of it so the curved paper never covers it.
                art.transform.localRotation = Quaternion.Euler(0f, -27f, 0f);
                art.transform.localPosition = new Vector3(0.248f, 0.1f, 0.127f);
                // Lay it on the desk, pages up, the top of the text away from the reader.
                book.transform.localRotation = Quaternion.Euler(-90f, 180f, 0f);
                book.transform.localScale *= 0.42f / Mathf.Max(BoundsOf(book).size.x, BoundsOf(book).size.z);
                // The export is open only part way, its pages standing in a steep V; flatten that (its local Z is now
                // up) so it lies open on the desk the way a book left mid-lesson does.
                book.transform.localScale = Vector3.Scale(book.transform.localScale, new Vector3(1f, 1f, 0.5f));
                Bounds bounds = BoundsOf(book);
                book.transform.localPosition -= new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
                bounds = BoundsOf(book);
                var collider = root.AddComponent<BoxCollider>();
                collider.center = bounds.center;
                collider.size = bounds.size;
                root.layer = StorySceneKit.EnvironmentLayer;
                return PrefabUtility.SaveAsPrefabAsset(root, PrefabFolder + "/SachTruyenKieu.prefab");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
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
