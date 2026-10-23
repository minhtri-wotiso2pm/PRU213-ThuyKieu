using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// One-off import of the team's modern-day characters from the ThuyKieuKiemTien project (teacher, seated boy, seated girl).
    /// The source OBJs are 1.5 M triangles each, so this reads them from that project, decimates with Unity's mesh LOD generator
    /// (its levels index the original vertices, so UVs stay exact), and saves a compact mesh + prefab under
    /// Art/Characters/Modern/&lt;name&gt;. Textures were copied next to them at 2048 px. Static, unrigged meshes.
    ///   Batch: -executeMethod ThuyKieu.Core.Editor.ModernCharacterImport.RunBatch
    /// </summary>
    public static class ModernCharacterImport
    {
        public const string Folder = "Assets/_Game/Art/Characters/Modern";
        private const string Source = "../ThuyKieuKiemTien/ThuyKieuKiemTien/thuykieu2.0/Assets/_Game/Art/Characters";
        private const int TargetTriangles = 45000;

        private static readonly (string Name, string Obj)[] Models =
        {
            ("ThayGiao", "GiaoVien/ThayGiao.obj"),
            ("HocSinhNgoi", "HocSinh/HocSinhNgoi.obj"),
            ("NuSinhToxic", "NhanVatChinh/NuSinhToxic.obj"),
        };

        public static string PrefabPath(string name) => $"{Folder}/{name}/{name}.prefab";
        public static string BaseMapPath(string name) => $"{Folder}/{name}/{name}_Base.png";
        public static string NormalMapPath(string name) => $"{Folder}/{name}/{name}_Normal.png";

        [MenuItem("ThuyKieu/Tools/Import modern characters from ThuyKieuKiemTien")]
        public static void Run()
        {
            foreach (var (name, obj) in Models)
            {
                string source = Path.Combine(Source, obj);
                if (!File.Exists(source)) throw new FileNotFoundException("Source model not found", Path.GetFullPath(source));
                var full = ReadObj(source);
                var compact = Decimate(full, name);
                UnityEngine.Object.DestroyImmediate(full);
                Save(name, compact);
            }
            AssetDatabase.SaveAssets();
        }

        public static void RunBatch()
        {
            try { Run(); EditorApplication.Exit(0); }
            catch (Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }

        /// <summary>Triangulated OBJ → mesh with one vertex per (position, uv) pair, mirrored on X into Unity's left-handed space.</summary>
        private static Mesh ReadObj(string path)
        {
            var positions = new List<Vector3>();
            var uvs = new List<Vector2>();
            var wedges = new Dictionary<long, int>();
            var vertexPositions = new List<int>();
            var vertexUvs = new List<int>();
            var triangles = new List<int>();
            var corner = new int[4];
            foreach (string line in File.ReadLines(path))
            {
                if (line.StartsWith("v ", StringComparison.Ordinal))
                {
                    var p = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    positions.Add(new Vector3(-Float(p[1]), Float(p[2]), Float(p[3])));
                }
                else if (line.StartsWith("vt ", StringComparison.Ordinal))
                {
                    var p = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    uvs.Add(new Vector2(Float(p[1]), Float(p[2])));
                }
                else if (line.StartsWith("f ", StringComparison.Ordinal))
                {
                    var p = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    int count = Mathf.Min(p.Length - 1, 4);
                    for (int i = 0; i < count; i++)
                    {
                        var parts = p[i + 1].Split('/');
                        int v = int.Parse(parts[0]) - 1;
                        int t = parts.Length > 1 && parts[1].Length > 0 ? int.Parse(parts[1]) - 1 : 0;
                        long key = ((long)v << 32) | (uint)t;
                        if (!wedges.TryGetValue(key, out int index))
                        {
                            index = vertexPositions.Count;
                            wedges[key] = index;
                            vertexPositions.Add(v);
                            vertexUvs.Add(t);
                        }
                        corner[i] = index;
                    }
                    // Mirroring X flips the winding, so emit the corners in reverse.
                    for (int i = 1; i + 1 < count; i++) { triangles.Add(corner[0]); triangles.Add(corner[i + 1]); triangles.Add(corner[i]); }
                }
            }

            // Smooth normals per position (not per wedge), so UV seams do not show as hard edges.
            var normalsByPosition = new Vector3[positions.Count];
            for (int i = 0; i < triangles.Count; i += 3)
            {
                int a = vertexPositions[triangles[i]], b = vertexPositions[triangles[i + 1]], c = vertexPositions[triangles[i + 2]];
                var n = Vector3.Cross(positions[b] - positions[a], positions[c] - positions[a]);
                normalsByPosition[a] += n; normalsByPosition[b] += n; normalsByPosition[c] += n;
            }
            var vertices = new Vector3[vertexPositions.Count];
            var normals = new Vector3[vertexPositions.Count];
            var uv = new Vector2[vertexPositions.Count];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = positions[vertexPositions[i]];
                normals[i] = normalsByPosition[vertexPositions[i]].normalized;
                uv[i] = uvs.Count > 0 ? uvs[vertexUvs[i]] : Vector2.zero;
            }
            var mesh = new Mesh { indexFormat = IndexFormat.UInt32 };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uv);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static float Float(string text) => float.Parse(text, CultureInfo.InvariantCulture);

        private static Mesh Decimate(Mesh full, string name)
        {
            MeshLodUtility.GenerateMeshLods(full, 12);
            // The LOD levels sit one after another in the index buffer; read it raw rather than through GetIndices.
            uint[] indices;
            using (var data = Mesh.AcquireReadOnlyMeshData(full)) indices = data[0].GetIndexData<uint>().ToArray();
            int lod = 0;
            for (int i = 0; i < full.lodCount; i++)
            {
                lod = i;
                if (full.GetLod(0, i).indexCount / 3 <= TargetTriangles) break;
            }
            var range = full.GetLod(0, lod);
            var vertices = full.vertices;
            var normals = full.normals;
            var uv = full.uv;
            var remap = new Dictionary<int, int>();
            var outVertices = new List<Vector3>();
            var outNormals = new List<Vector3>();
            var outUv = new List<Vector2>();
            var outTriangles = new List<int>((int)range.indexCount);
            for (int i = 0; i < (int)range.indexCount; i++)
            {
                int source = (int)indices[(int)range.indexStart + i];
                if (!remap.TryGetValue(source, out int target))
                {
                    target = outVertices.Count;
                    remap[source] = target;
                    outVertices.Add(vertices[source]);
                    outNormals.Add(normals[source]);
                    outUv.Add(uv[source]);
                }
                outTriangles.Add(target);
            }
            var mesh = new Mesh { name = name, indexFormat = outVertices.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
            mesh.SetVertices(outVertices);
            mesh.SetNormals(outNormals);
            mesh.SetUVs(0, outUv);
            mesh.SetTriangles(outTriangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            Debug.Log($"[ModernCharacters] {name}: {full.vertexCount} verts / {full.GetLod(0, 0).indexCount / 3} tris → LOD {lod} of {full.lodCount}: " +
                      $"{outVertices.Count} verts / {outTriangles.Count / 3} tris · size {mesh.bounds.size}");
            return mesh;
        }

        private static void Save(string name, Mesh mesh)
        {
            string folder = $"{Folder}/{name}";
            Directory.CreateDirectory(folder);
            string meshPath = $"{folder}/{name}_Mesh.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if (existing != null) { EditorUtility.CopySerialized(mesh, existing); UnityEngine.Object.DestroyImmediate(mesh); mesh = existing; }
            else AssetDatabase.CreateAsset(mesh, meshPath);

            AssetDatabase.ImportAsset(BaseMapPath(name));
            AssetDatabase.ImportAsset(NormalMapPath(name));
            if (AssetImporter.GetAtPath(NormalMapPath(name)) is TextureImporter normalImporter && normalImporter.textureType != TextureImporterType.NormalMap)
            { normalImporter.textureType = TextureImporterType.NormalMap; normalImporter.SaveAndReimport(); }

            string materialPath = $"{folder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(material, materialPath); }
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(BaseMapPath(name)));
            material.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(NormalMapPath(name)));
            material.EnableKeyword("_NORMALMAP");
            material.SetFloat("_Smoothness", 0.25f);
            EditorUtility.SetDirty(material);

            var go = new GameObject(name);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
            PrefabUtility.SaveAsPrefabAsset(go, PrefabPath(name));
            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
