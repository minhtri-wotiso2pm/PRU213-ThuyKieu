using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ThuyKieu.Player.Editor
{
    /// <summary>
    /// Moves Tripo's real 4K textures onto Kiều's rigged hanfu mesh. Tripo re-bakes the atlas for every export, so the
    /// 4K maps do not fit the UVs of the rigged mesh (made from an earlier 2K export of the same 50k-triangle model,
    /// then rigged and edited in Blender). For every texel of the rigged mesh's UV layout this finds the same point on
    /// the 4K export's surface and samples its maps there. The edits painted on the old 2K base colour (skin tone,
    /// blush, seam padding) are kept by re-applying their difference from the original 2K Tripo bake.
    /// Writes new files next to the old ones and points the URP materials at them; the old textures stay untouched.
    /// </summary>
    public static class KieuTextureTransfer
    {
        private const string Root = "Assets/_Game/Art/Characters/ThuyKieu/";
        private const string RiggedModel = Root + "Models/ThuyKieu_Walk.fbx";
        private const string RiggedMeshName = "tripo_node_f1bebe54";
        private const string SourceModel = Root + "Source_4K/tripo_convert_393af9de-c739-4d69-aff3-ddba32f753a8.fbx";
        private const string SourceMaps = Root + "Source_4K/tripo_convert_393af9de-c739-4d69-aff3-ddba32f753a8.fbm/";
        private const string SourceBase = SourceMaps + "tripo_node_78ac6be6-140f-4853-aba5-58186960a56c_BaseColor.png";
        private const string SourceNormal = SourceMaps + "tripo_node_78ac6be6-140f-4853-aba5-58186960a56c_Normal_Bake.png";
        private const string SourceMetallic = SourceMaps + "traditional_hanfu_3d_model_metallic.PNG";
        private const string SourceRoughness = SourceMaps + "traditional_hanfu_3d_model_roughness.PNG";

        private const string Maps = Root + "Tectures/Hanfu/";
        private const string OriginalBase2K = Maps + "tripo_node_78ac6be6-140f-4853-aba5-58186960a56c_BaseColor.png";
        private const string HanfuEdited2K = Maps + "ThuyKieu_Hanfu_BaseColor_Padded.png";
        private const string SkinEdited2K = Maps + "ThuyKieu_Skin_BaseColor.png";

        private const string HanfuOut = Maps + "ThuyKieu_Hanfu_BaseColor_Tripo4K.png";
        private const string SkinOut = Maps + "ThuyKieu_Skin_BaseColor_Tripo4K.png";
        private const string MetallicOut = Maps + "ThuyKieu_Hanfu_MetallicSmoothness_Tripo2K.png";
        private const string NormalOut = Maps + "ThuyKieu_Hanfu_Normal_Tripo2K.png";

        private const string HanfuMaterial = Root + "Materials/ThuyKieu_Hanfu_URP.mat";
        private const string SkinMaterial = Root + "Materials/ThuyKieu_Skin_URP.mat";

        private const int Size = 4096;
        private const float Cell = 0.015f;

        [MenuItem("ThuyKieu/Characters/Transfer Thúy Kiều Tripo 4K textures")]
        public static void Run() => Debug.Log(Transfer());

        /// <summary>How well the rigged mesh lands on the 4K export, without writing anything.</summary>
        public static string CheckAlignment()
        {
            var log = new System.Text.StringBuilder();
            var source = LoadMesh(SourceModel, null);
            Align(LoadMesh(RiggedModel, RiggedMeshName).vertices, new TriangleGrid(source.vertices, source.triangles, Cell), out _, out _, log);
            return log.ToString();
        }

        /// <summary>How many rigged triangles have a one-to-one counterpart in the 4K export.</summary>
        public static string CheckTriangleMatch()
        {
            var log = new System.Text.StringBuilder();
            var rigged = LoadMesh(RiggedModel, RiggedMeshName);
            var source = LoadMesh(SourceModel, null);
            var grid = new TriangleGrid(source.vertices, source.triangles, Cell);
            Align(rigged.vertices, grid, out float scale, out Vector3 offset, log);
            MatchTriangles(rigged.vertices, rigged.triangles, source.vertices, source.triangles, scale, offset, log);
            return log.ToString();
        }

        public static string Transfer() => Transfer(true);

        /// <param name="keepEdits">Re-apply the old 2K paint-overs (skin tone, blush, padding) on top of the 4K maps.</param>
        public static string Transfer(bool keepEdits)
        {
            var log = new System.Text.StringBuilder();
            var rigged = LoadMesh(RiggedModel, RiggedMeshName);
            var source = LoadMesh(SourceModel, null);
            Vector3[] rv = rigged.vertices, sv = source.vertices, rn = rigged.normals, sn = source.normals;
            Vector2[] ruv = rigged.uv, suv = source.uv;
            Vector4[] rt = rigged.tangents, st = source.tangents;
            int[] rtri = rigged.triangles, stri = source.triangles;

            var grid = new TriangleGrid(sv, stri, Cell);
            Align(rv, grid, out float scale, out Vector3 offset, log);
            var order = new int[rtri.Length / 3];
            var match = MatchTriangles(rv, rtri, sv, stri, scale, offset, log, order);

            int baseW, normalW, metalW, roughW, origW, hanfuW, skinW;
            var baseMap = Read(SourceBase, out baseW);
            var normalMap = Read(SourceNormal, out normalW);
            var metalMap = Read(SourceMetallic, out metalW);
            var roughMap = Read(SourceRoughness, out roughW);
            var original = Read(OriginalBase2K, out origW);
            var hanfuEdit = Read(HanfuEdited2K, out hanfuW);
            var skinEdit = Read(SkinEdited2K, out skinW);

            var colour = new Color[Size * Size];
            var normal = new Vector3[Size * Size];
            var metal = new float[Size * Size];
            var smooth = new float[Size * Size];
            var covered = new bool[Size * Size];
            int texels = 0;
            double error = 0;

            for (int t = 0; t < rtri.Length; t += 3)
            {
                int a = rtri[t], b = rtri[t + 1], c = rtri[t + 2];
                int twin = match[t / 3];
                int[] corner = twin >= 0 ? Perms[order[t / 3]] : null;
                Vector2 ua = ruv[a] * Size, ub = ruv[b] * Size, uc = ruv[c] * Size;
                float area = Cross(ub - ua, uc - ua);
                if (Mathf.Abs(area) < 1e-6f) continue;
                int x0 = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(ua.x, Mathf.Min(ub.x, uc.x)))), x1 = Mathf.Min(Size - 1, Mathf.CeilToInt(Mathf.Max(ua.x, Mathf.Max(ub.x, uc.x))));
                int y0 = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(ua.y, Mathf.Min(ub.y, uc.y)))), y1 = Mathf.Min(Size - 1, Mathf.CeilToInt(Mathf.Max(ua.y, Mathf.Max(ub.y, uc.y))));
                for (int y = y0; y <= y1; y++)
                    for (int x = x0; x <= x1; x++)
                    {
                        var p = new Vector2(x + 0.5f, y + 0.5f);
                        float w0 = Cross(ub - p, uc - p) / area, w1 = Cross(uc - p, ua - p) / area, w2 = 1f - w0 - w1;
                        // A small tolerance closes the cracks between neighbouring UV triangles.
                        const float edge = -0.02f;
                        if (w0 < edge || w1 < edge || w2 < edge) continue;
                        int i = y * Size + x;
                        if (covered[i]) continue;
                        int sa, sb, sc; Vector3 bary; float distance = 0f;
                        if (twin >= 0)
                        {
                            // The same triangle in the 4K export: carry the texel over by its barycentric position.
                            sa = stri[twin + corner[0]]; sb = stri[twin + corner[1]]; sc = stri[twin + corner[2]];
                            bary = new Vector3(Mathf.Max(0f, w0), Mathf.Max(0f, w1), Mathf.Max(0f, w2));
                            bary /= bary.x + bary.y + bary.z;
                        }
                        else
                        {
                            Vector3 position = (rv[a] * w0 + rv[b] * w1 + rv[c] * w2) * scale + offset;
                            Vector3 facing = rn[a] * w0 + rn[b] * w1 + rn[c] * w2;
                            distance = grid.Closest(position, facing, out int hit, out bary);
                            sa = stri[hit]; sb = stri[hit + 1]; sc = stri[hit + 2];
                        }
                        Vector2 uv = suv[sa] * bary.x + suv[sb] * bary.y + suv[sc] * bary.z;

                        colour[i] = Bilinear(baseMap, baseW, uv);
                        metal[i] = Bilinear(metalMap, metalW, uv).r;
                        smooth[i] = 1f - Bilinear(roughMap, roughW, uv).r;
                        // Tangent-space normal: Tripo's tangent frame → object space → the rigged mesh's tangent frame.
                        Color nc = Bilinear(normalMap, normalW, uv);
                        Vector3 ts = new Vector3(nc.r * 2f - 1f, nc.g * 2f - 1f, nc.b * 2f - 1f);
                        Vector3 objectNormal = FromTangent(ts, sn, st, sa, sb, sc, bary);
                        normal[i] = ToTangent(objectNormal, rn, rt, a, b, c, new Vector3(w0, w1, w2));
                        covered[i] = true;
                        texels++;
                        error += distance;
                    }
            }
            log.Append("texels " + texels + " (" + (100.0 * texels / (Size * Size)).ToString("F1") + "% of the atlas), mean surface gap " + (error / Mathf.Max(1, texels) * 1000).ToString("F2") + " mm\n");

            Dilate(colour, covered, Size, 12);
            var hanfu = new Color[colour.Length];
            var skin = new Color[colour.Length];
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    int i = y * Size + x;
                    var uv = new Vector2((x + 0.5f) / Size, (y + 0.5f) / Size);
                    Color o = Bilinear(original, origW, uv);
                    hanfu[i] = keepEdits ? Clamp(colour[i] + (Bilinear(hanfuEdit, hanfuW, uv) - o)) : Clamp(colour[i]);
                    skin[i] = keepEdits ? Clamp(colour[i] + (Bilinear(skinEdit, skinW, uv) - o)) : Clamp(colour[i]);
                }
            Write(HanfuOut, hanfu, Size, true, false);
            Write(SkinOut, skin, Size, true, false);

            // Metallic, smoothness and normal: the sources are 2K and 1K, so 2K outputs keep all their detail.
            int half = Size / 2;
            var ms = new Color[half * half];
            var nm = new Color[half * half];
            var halfCovered = new bool[half * half];
            for (int y = 0; y < half; y++)
                for (int x = 0; x < half; x++)
                {
                    float m = 0, s = 0; Vector3 n = Vector3.zero; int count = 0;
                    for (int dy = 0; dy < 2; dy++)
                        for (int dx = 0; dx < 2; dx++)
                        {
                            int i = (y * 2 + dy) * Size + x * 2 + dx;
                            if (!covered[i]) continue;
                            m += metal[i]; s += smooth[i]; n += normal[i]; count++;
                        }
                    int j = y * half + x;
                    if (count == 0) { ms[j] = new Color(0f, 0f, 0f, 0.3f); nm[j] = new Color(0.5f, 0.5f, 1f, 1f); continue; }
                    halfCovered[j] = true;
                    ms[j] = new Color(m / count, m / count, m / count, s / count);
                    n = n.sqrMagnitude > 1e-6f ? n.normalized : Vector3.forward;
                    nm[j] = new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f);
                }
            Dilate(ms, halfCovered, half, 8);
            var normalCovered = (bool[])halfCovered.Clone();
            Dilate(nm, normalCovered, half, 8);
            Write(MetallicOut, ms, half, false, false);
            Write(NormalOut, nm, half, false, true);

            AssignMaterials();
            log.Append("wrote " + HanfuOut + ", " + SkinOut + ", " + MetallicOut + ", " + NormalOut + "\n");
            return log.ToString();
        }

        // ------------------------------------------------------------------ alignment

        /// <summary>
        /// Uniform scale and offset taking the rigged mesh onto the 4K export (iterative closest point). Pairs far
        /// from the surface are ignored: the sleeves were reshaped in Blender and have no counterpart.
        /// </summary>
        private static void Align(Vector3[] vertices, TriangleGrid grid, out float scale, out Vector3 offset, System.Text.StringBuilder log)
        {
            Vector3 rc = Vector3.zero;
            foreach (var v in vertices) rc += v;
            rc /= vertices.Length;
            scale = 1f;
            offset = grid.Centre - rc;
            float limit = 0.05f;
            for (int iteration = 0; iteration < 12; iteration++)
            {
                var from = new List<Vector3>();
                var to = new List<Vector3>();
                for (int i = 0; i < vertices.Length; i += 3)
                {
                    Vector3 p = vertices[i] * scale + offset;
                    float d = grid.Closest(p, out _, out _, out Vector3 point);
                    if (d > limit) continue;
                    from.Add(vertices[i]);
                    to.Add(point);
                }
                Vector3 fm = Vector3.zero, tm = Vector3.zero;
                for (int i = 0; i < from.Count; i++) { fm += from[i]; tm += to[i]; }
                fm /= from.Count; tm /= to.Count;
                double num = 0, den = 0;
                for (int i = 0; i < from.Count; i++) { num += Vector3.Dot(from[i] - fm, to[i] - tm); den += (from[i] - fm).sqrMagnitude; }
                scale = (float)(num / den);
                offset = tm - fm * scale;
                limit = Mathf.Max(0.006f, limit * 0.7f);
            }
            double sum = 0; int near = 0;
            for (int i = 0; i < vertices.Length; i += 3)
            {
                float d = grid.Closest(vertices[i] * scale + offset, out _, out _);
                if (d < 0.004f) near++;
                sum += d;
            }
            log.Append("alignment scale " + scale.ToString("F4") + " offset " + offset.ToString("F4") + ", mean gap " + (sum / (vertices.Length / 3) * 1000).ToString("F2") + " mm, "
                       + (100.0 * near / (vertices.Length / 3)).ToString("F1") + "% of vertices within 4 mm\n");
        }

        // ------------------------------------------------------------------ triangle correspondence

        /// <summary>
        /// Both meshes come from the same 50k-triangle Tripo model, so most rigged triangles exist unchanged in the 4K
        /// export. For each one this finds that triangle and the vertex order mapping its corners, so a texel can be
        /// carried over by its barycentric position — exact even where vertices moved a little (eyelids, lips), where a
        /// nearest-point search lands on the wrong layer. Returns -1 for triangles reshaped in Blender.
        /// </summary>
        private static int[] MatchTriangles(Vector3[] rv, int[] rtri, Vector3[] sv, int[] stri, float scale, Vector3 offset, System.Text.StringBuilder log, int[] order = null)
        {
            const float cell = 0.004f, tolerance = 0.0025f;
            var buckets = new Dictionary<Vector3Int, List<int>>();
            Vector3Int Key(Vector3 p) => new Vector3Int(Mathf.FloorToInt(p.x / cell), Mathf.FloorToInt(p.y / cell), Mathf.FloorToInt(p.z / cell));
            for (int t = 0; t < stri.Length; t += 3)
            {
                var key = Key((sv[stri[t]] + sv[stri[t + 1]] + sv[stri[t + 2]]) / 3f);
                if (!buckets.TryGetValue(key, out var list)) buckets[key] = list = new List<int>();
                list.Add(t);
            }
            var match = new int[rtri.Length / 3];
            int found = 0;
            for (int t = 0; t < rtri.Length; t += 3)
            {
                Vector3 a = rv[rtri[t]] * scale + offset, b = rv[rtri[t + 1]] * scale + offset, c = rv[rtri[t + 2]] * scale + offset;
                var key = Key((a + b + c) / 3f);
                float best = tolerance * tolerance * 3f; int bestT = -1, bestP = 0;
                for (int dz = -1; dz <= 1; dz++)
                    for (int dy = -1; dy <= 1; dy++)
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            if (!buckets.TryGetValue(key + new Vector3Int(dx, dy, dz), out var list)) continue;
                            foreach (int s in list)
                            {
                                Vector3 sa = sv[stri[s]], sb = sv[stri[s + 1]], sc = sv[stri[s + 2]];
                                Vector3[] q = { sa, sb, sc };
                                for (int p = 0; p < Perms.Length; p++)
                                {
                                    var m = Perms[p];
                                    float da = (q[m[0]] - a).sqrMagnitude, db = (q[m[1]] - b).sqrMagnitude, dc = (q[m[2]] - c).sqrMagnitude;
                                    if (da > tolerance * tolerance || db > tolerance * tolerance || dc > tolerance * tolerance) continue;
                                    float d = da + db + dc;
                                    if (d < best) { best = d; bestT = s; bestP = p; }
                                }
                            }
                        }
                match[t / 3] = bestT;
                if (order != null) order[t / 3] = bestP;
                if (bestT >= 0) found++;
            }
            log.Append("triangle match " + found + " / " + match.Length + " (" + (100.0 * found / match.Length).ToString("F1") + "%)\n");
            return match;
        }

        private static readonly int[][] Perms = { new[] { 0, 1, 2 }, new[] { 1, 2, 0 }, new[] { 2, 0, 1 }, new[] { 0, 2, 1 }, new[] { 2, 1, 0 }, new[] { 1, 0, 2 } };

        // ------------------------------------------------------------------ tangent frames

        private static Vector3 FromTangent(Vector3 ts, Vector3[] normals, Vector4[] tangents, int a, int b, int c, Vector3 w)
        {
            Vector3 n = (normals[a] * w.x + normals[b] * w.y + normals[c] * w.z).normalized;
            Vector4 t4 = tangents[a] * w.x + tangents[b] * w.y + tangents[c] * w.z;
            Vector3 t = ((Vector3)t4 - n * Vector3.Dot(n, t4)).normalized;
            Vector3 bt = Vector3.Cross(n, t) * (tangents[a].w < 0f ? -1f : 1f);
            return (t * ts.x + bt * ts.y + n * ts.z).normalized;
        }

        private static Vector3 ToTangent(Vector3 normal, Vector3[] normals, Vector4[] tangents, int a, int b, int c, Vector3 w)
        {
            Vector3 n = (normals[a] * w.x + normals[b] * w.y + normals[c] * w.z).normalized;
            Vector4 t4 = tangents[a] * w.x + tangents[b] * w.y + tangents[c] * w.z;
            Vector3 t = ((Vector3)t4 - n * Vector3.Dot(n, t4)).normalized;
            Vector3 bt = Vector3.Cross(n, t) * (tangents[a].w < 0f ? -1f : 1f);
            var result = new Vector3(Vector3.Dot(normal, t), Vector3.Dot(normal, bt), Vector3.Dot(normal, n));
            return result.z > 0.05f ? result.normalized : Vector3.forward;
        }

        // ------------------------------------------------------------------ images

        private static Mesh LoadMesh(string path, string name)
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
                if (asset is Mesh mesh && (name == null || mesh.name == name))
                {
                    var copy = Object.Instantiate(mesh);
                    if (copy.tangents.Length != copy.vertexCount) copy.RecalculateTangents();
                    return copy;
                }
            throw new FileNotFoundException("No mesh " + name + " in " + path);
        }

        private static Color[] Read(string path, out int width)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            texture.LoadImage(File.ReadAllBytes(path));
            width = texture.width;
            var pixels = texture.GetPixels();
            Object.DestroyImmediate(texture);
            return pixels;
        }

        private static Color Bilinear(Color[] pixels, int width, Vector2 uv)
        {
            float x = Mathf.Clamp(uv.x * width - 0.5f, 0f, width - 1.001f), y = Mathf.Clamp(uv.y * width - 0.5f, 0f, width - 1.001f);
            int x0 = (int)x, y0 = (int)y;
            float fx = x - x0, fy = y - y0;
            Color a = pixels[y0 * width + x0], b = pixels[y0 * width + x0 + 1], c = pixels[(y0 + 1) * width + x0], d = pixels[(y0 + 1) * width + x0 + 1];
            return Color.Lerp(Color.Lerp(a, b, fx), Color.Lerp(c, d, fx), fy);
        }

        /// <summary>Grows covered texels outwards so mip levels and filtering never bleed the empty atlas into the seams.</summary>
        private static void Dilate(Color[] pixels, bool[] covered, int width, int passes)
        {
            var next = new List<int>();
            for (int pass = 0; pass < passes; pass++)
            {
                next.Clear();
                for (int y = 0; y < width; y++)
                    for (int x = 0; x < width; x++)
                    {
                        int i = y * width + x;
                        if (covered[i]) continue;
                        Color sum = Color.clear; int count = 0;
                        if (x > 0 && covered[i - 1]) { sum += pixels[i - 1]; count++; }
                        if (x < width - 1 && covered[i + 1]) { sum += pixels[i + 1]; count++; }
                        if (y > 0 && covered[i - width]) { sum += pixels[i - width]; count++; }
                        if (y < width - 1 && covered[i + width]) { sum += pixels[i + width]; count++; }
                        if (count == 0) continue;
                        pixels[i] = sum / count;
                        next.Add(i);
                    }
                foreach (int i in next) covered[i] = true;
            }
        }

        private static Color Clamp(Color c) => new Color(Mathf.Clamp01(c.r), Mathf.Clamp01(c.g), Mathf.Clamp01(c.b), 1f);

        private static void Write(string path, Color[] pixels, int width, bool srgb, bool isNormal)
        {
            var texture = new Texture2D(width, width, TextureFormat.RGBA32, false, true);
            texture.SetPixels(pixels);
            texture.Apply(false);
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = srgb;
            importer.maxTextureSize = width;
            importer.mipmapEnabled = true;
            importer.anisoLevel = 8;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }

        private static void AssignMaterials()
        {
            var hanfu = AssetDatabase.LoadAssetAtPath<Material>(HanfuMaterial);
            var skin = AssetDatabase.LoadAssetAtPath<Material>(SkinMaterial);
            var hanfuBase = AssetDatabase.LoadAssetAtPath<Texture2D>(HanfuOut);
            var skinBase = AssetDatabase.LoadAssetAtPath<Texture2D>(SkinOut);
            hanfu.SetTexture("_BaseMap", hanfuBase);
            hanfu.SetTexture("_MainTex", hanfuBase);
            hanfu.SetTexture("_MetallicGlossMap", AssetDatabase.LoadAssetAtPath<Texture2D>(MetallicOut));
            hanfu.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(NormalOut));
            skin.SetTexture("_BaseMap", skinBase);
            skin.SetTexture("_MainTex", skinBase);
            EditorUtility.SetDirty(hanfu);
            EditorUtility.SetDirty(skin);
            AssetDatabase.SaveAssets();
        }

        private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

        // ------------------------------------------------------------------ closest point on a mesh

        private sealed class TriangleGrid
        {
            private readonly Vector3[] _v;
            private readonly int[] _t;
            private readonly float _cell;
            private readonly Vector3 _min;
            private readonly int _nx, _ny, _nz;
            private readonly List<int>[] _cells;
            private readonly Vector3[] _faceNormals;
            public Vector3 Centre { get; }

            public TriangleGrid(Vector3[] vertices, int[] triangles, float cell)
            {
                _v = vertices; _t = triangles; _cell = cell;
                var bounds = new Bounds(vertices[0], Vector3.zero);
                foreach (var v in vertices) bounds.Encapsulate(v);
                Centre = Vector3.zero;
                foreach (var v in vertices) Centre += v;
                Centre /= vertices.Length;
                _min = bounds.min - Vector3.one * cell;
                _nx = Mathf.CeilToInt(bounds.size.x / cell) + 3; _ny = Mathf.CeilToInt(bounds.size.y / cell) + 3; _nz = Mathf.CeilToInt(bounds.size.z / cell) + 3;
                _cells = new List<int>[_nx * _ny * _nz];
                _faceNormals = new Vector3[triangles.Length / 3];
                for (int t = 0; t < triangles.Length; t += 3)
                {
                    _faceNormals[t / 3] = Vector3.Cross(vertices[triangles[t + 1]] - vertices[triangles[t]], vertices[triangles[t + 2]] - vertices[triangles[t]]).normalized;
                    Vector3 a = vertices[triangles[t]], b = vertices[triangles[t + 1]], c = vertices[triangles[t + 2]];
                    Vector3Int lo = CellOf(Vector3.Min(a, Vector3.Min(b, c))), hi = CellOf(Vector3.Max(a, Vector3.Max(b, c)));
                    for (int z = lo.z; z <= hi.z; z++)
                        for (int y = lo.y; y <= hi.y; y++)
                            for (int x = lo.x; x <= hi.x; x++)
                            {
                                int i = (z * _ny + y) * _nx + x;
                                (_cells[i] ??= new List<int>()).Add(t);
                            }
                }
            }

            private Vector3Int CellOf(Vector3 p)
            {
                Vector3 q = (p - _min) / _cell;
                return new Vector3Int(Mathf.Clamp((int)q.x, 0, _nx - 1), Mathf.Clamp((int)q.y, 0, _ny - 1), Mathf.Clamp((int)q.z, 0, _nz - 1));
            }

            public float Closest(Vector3 p, out int triangle, out Vector3 bary) => Closest(p, out triangle, out bary, out _);

            /// <summary>
            /// Nearest surface point facing the same way as <paramref name="facing"/>. Eyelids, eyeballs, lips and hair
            /// strands lie within a millimetre of each other; the plain nearest point often lands on the wrong layer.
            /// Falls back to the plain nearest point when no matching surface is within a centimetre of it.
            /// </summary>
            public float Closest(Vector3 p, Vector3 facing, out int triangle, out Vector3 bary)
            {
                float plain = Closest(p, out triangle, out bary, out _);
                if (facing.sqrMagnitude < 1e-8f) return plain;
                facing.Normalize();
                if (Vector3.Dot(_faceNormals[triangle / 3], facing) > 0.3f) return plain;
                float limit = plain + 0.01f, bestSq = limit * limit;
                int found = -1; Vector3 foundBary = bary;
                Vector3Int c = CellOf(p);
                int rings = Mathf.CeilToInt(limit / _cell) + 1;
                for (int z = c.z - rings; z <= c.z + rings; z++)
                    for (int y = c.y - rings; y <= c.y + rings; y++)
                        for (int x = c.x - rings; x <= c.x + rings; x++)
                        {
                            if (x < 0 || y < 0 || z < 0 || x >= _nx || y >= _ny || z >= _nz) continue;
                            var list = _cells[(z * _ny + y) * _nx + x];
                            if (list == null) continue;
                            foreach (int t in list)
                            {
                                if (Vector3.Dot(_faceNormals[t / 3], facing) <= 0.3f) continue;
                                Vector3 q = ClosestOnTriangle(p, _v[_t[t]], _v[_t[t + 1]], _v[_t[t + 2]], out Vector3 w);
                                float d = (q - p).sqrMagnitude;
                                if (d >= bestSq) continue;
                                bestSq = d; found = t; foundBary = w;
                            }
                        }
                if (found < 0) return plain;
                triangle = found; bary = foundBary;
                return Mathf.Sqrt(bestSq);
            }

            /// <summary>Nearest surface point: searches growing shells of cells until no closer triangle can exist.</summary>
            public float Closest(Vector3 p, out int triangle, out Vector3 bary, out Vector3 point)
            {
                Vector3Int c = CellOf(p);
                float bestSq = float.MaxValue;
                triangle = 0; bary = new Vector3(1f, 0f, 0f); point = p;
                int maxRing = Mathf.Max(_nx, Mathf.Max(_ny, _nz));
                for (int ring = 0; ring <= maxRing; ring++)
                {
                    // Everything in this shell is at least (ring - 1) cells away.
                    float reach = Mathf.Max(0f, ring - 1) * _cell;
                    if (bestSq < reach * reach) break;
                    for (int z = c.z - ring; z <= c.z + ring; z++)
                        for (int y = c.y - ring; y <= c.y + ring; y++)
                            for (int x = c.x - ring; x <= c.x + ring; x++)
                            {
                                if (Mathf.Max(Mathf.Abs(x - c.x), Mathf.Max(Mathf.Abs(y - c.y), Mathf.Abs(z - c.z))) != ring) continue;
                                if (x < 0 || y < 0 || z < 0 || x >= _nx || y >= _ny || z >= _nz) continue;
                                var list = _cells[(z * _ny + y) * _nx + x];
                                if (list == null) continue;
                                foreach (int t in list)
                                {
                                    Vector3 q = ClosestOnTriangle(p, _v[_t[t]], _v[_t[t + 1]], _v[_t[t + 2]], out Vector3 w);
                                    float d = (q - p).sqrMagnitude;
                                    if (d >= bestSq) continue;
                                    bestSq = d; triangle = t; bary = w; point = q;
                                }
                            }
                }
                return Mathf.Sqrt(bestSq);
            }

            /// <summary>Closest point on triangle abc (Ericson, Real-Time Collision Detection 5.1.5) with its barycentric weights.</summary>
            private static Vector3 ClosestOnTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c, out Vector3 w)
            {
                Vector3 ab = b - a, ac = c - a, ap = p - a;
                float d1 = Vector3.Dot(ab, ap), d2 = Vector3.Dot(ac, ap);
                if (d1 <= 0f && d2 <= 0f) { w = new Vector3(1, 0, 0); return a; }
                Vector3 bp = p - b;
                float d3 = Vector3.Dot(ab, bp), d4 = Vector3.Dot(ac, bp);
                if (d3 >= 0f && d4 <= d3) { w = new Vector3(0, 1, 0); return b; }
                float vc = d1 * d4 - d3 * d2;
                if (vc <= 0f && d1 >= 0f && d3 <= 0f) { float v = d1 / (d1 - d3); w = new Vector3(1 - v, v, 0); return a + ab * v; }
                Vector3 cp = p - c;
                float d5 = Vector3.Dot(ab, cp), d6 = Vector3.Dot(ac, cp);
                if (d6 >= 0f && d5 <= d6) { w = new Vector3(0, 0, 1); return c; }
                float vb = d5 * d2 - d1 * d6;
                if (vb <= 0f && d2 >= 0f && d6 <= 0f) { float v = d2 / (d2 - d6); w = new Vector3(1 - v, 0, v); return a + ac * v; }
                float va = d3 * d6 - d5 * d4;
                if (va <= 0f && (d4 - d3) >= 0f && (d5 - d6) >= 0f) { float v = (d4 - d3) / ((d4 - d3) + (d5 - d6)); w = new Vector3(0, 1 - v, v); return b + (c - b) * v; }
                float denom = 1f / (va + vb + vc);
                float vv = vb * denom, ww = vc * denom;
                w = new Vector3(1 - vv - ww, vv, ww);
                return a + ab * vv + ac * ww;
            }
        }
    }
}
