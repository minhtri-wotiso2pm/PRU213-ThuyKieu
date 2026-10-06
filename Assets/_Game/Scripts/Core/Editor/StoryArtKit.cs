using System;
using System.Collections.Generic;
using System.IO;
using ThuyKieu.Environment;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static ThuyKieu.Core.Editor.StorySceneKit;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Set-dressing helpers for the story scenes: CC0-textured world-space surfaces, HDRI skies, rocks,
    /// grass, carved details and ambient particles. Everything generated here lives in the regenerated
    /// story material folder; source textures live in Art/Environment (see Textures/CREDITS.md).
    /// </summary>
    public static class StoryArtKit
    {
        public const string TextureRoot = "Assets/_Game/Art/Environment/Textures";
        public const string SkyRoot = "Assets/_Game/Art/Environment/Sky";
        public const string TriplanarShader = "ThuyKieu/WorldTriplanarLit";

        // ------------------------------------------------------------------ surfaces

        /// <summary>World-space textured material. <paramref name="metresPerTile"/> is the real size one texture repeat covers.</summary>
        public static Material Surface(string name, string texture, Color tint, float metresPerTile, float smoothness = 0.2f,
            float normal = 1f, float variation = 0.35f, float grime = 0f, string folder = null)
        {
            string path = (folder ?? AssetFolder) + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool create = material == null;
            if (create) material = new Material(Shader.Find(TriplanarShader));
            else material.shader = Shader.Find(TriplanarShader);
            material.SetTexture("_BaseMap", Texture(texture, false));
            material.SetTexture("_BumpMap", Texture(texture, true));
            material.SetTextureScale("_BaseMap", Vector2.one / Mathf.Max(0.05f, metresPerTile));
            material.SetColor("_BaseColor", tint);
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_BumpScale", normal);
            material.SetFloat("_Parallax", variation);
            material.SetFloat("_OcclusionStrength", grime);
            material.enableInstancing = true;
            if (create) AssetDatabase.CreateAsset(material, path);
            else EditorUtility.SetDirty(material);
            return material;
        }

        public static Texture2D Texture(string texture, bool normal)
        {
            string path = TextureRoot + "/" + texture + "/" + texture + (normal ? "_nor_gl_1k.jpg" : "_diff_1k.jpg");
            if (AssetImporter.GetAtPath(path) is TextureImporter importer)
            {
                var type = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
                if (importer.textureType != type || importer.maxTextureSize != 1024 || importer.anisoLevel != 4)
                {
                    importer.textureType = type;
                    importer.maxTextureSize = 1024;
                    importer.anisoLevel = 4;
                    importer.wrapMode = TextureWrapMode.Repeat;
                    importer.SaveAndReimport();
                }
            }
            var result = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (result == null) throw new InvalidOperationException("Missing texture " + path);
            return result;
        }

        // ------------------------------------------------------------------ sky

        /// <summary>Panoramic HDRI skybox (Poly Haven "puresky" images have no ground baked in).</summary>
        public static Material Sky(string name, string hdr, float exposure, Color tint, float rotation, string folder = null)
        {
            string texturePath = SkyRoot + "/" + hdr + "_2k.hdr";
            if (AssetImporter.GetAtPath(texturePath) is TextureImporter importer && (importer.mipmapEnabled || importer.maxTextureSize != 2048))
            {
                importer.textureShape = TextureImporterShape.Texture2D;
                importer.mipmapEnabled = false;
                importer.maxTextureSize = 2048;
                importer.wrapModeU = TextureWrapMode.Repeat;
                importer.wrapModeV = TextureWrapMode.Clamp;
                importer.SaveAndReimport();
            }
            string path = (folder ?? AssetFolder) + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(Shader.Find("Skybox/Panoramic")); AssetDatabase.CreateAsset(material, path); }
            material.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
            material.SetFloat("_Mapping", 1f);
            material.SetFloat("_ImageType", 0f);
            material.SetFloat("_Exposure", exposure);
            material.SetFloat("_Rotation", rotation);
            material.SetColor("_Tint", tint);
            EditorUtility.SetDirty(material);
            return material;
        }

        public static void UseSky(Camera camera, Material sky)
        {
            RenderSettings.skybox = sky;
            if (camera != null) camera.clearFlags = CameraClearFlags.Skybox;
        }

        /// <summary>Chapter colour grade: filmic tonemapping, exposure, warmth and split-toned shadows/highlights.</summary>
        public static void Grade(ThuyKieu.Presentation.ScreenFx fx, float exposure, float contrast, float saturation, float temperature, Color shadows, Color highlights)
        {
            Set(fx, "_filmic", true);
            Set(fx, "_baseExposure", exposure);
            Set(fx, "_baseContrast", contrast);
            Set(fx, "_baseSaturation", saturation);
            Set(fx, "_temperature", temperature);
            Set(fx, "_shadowTint", shadows);
            Set(fx, "_highlightTint", highlights);
        }

        // ------------------------------------------------------------------ architecture details

        /// <summary>Lacquered pillar on a carved stone plinth (chân tảng) with a bracket block on top.</summary>
        public static GameObject Pillar(string name, Vector3 foot, float radius, float height, Material shaft, Material stone, Transform parent, bool solid = true)
        {
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.position = foot;
            Cylinder("Plinth", foot, radius * 1.9f, 0.16f, stone, root, false);
            Cylinder("PlinthTop", foot + Vector3.up * 0.16f, radius * 1.45f, 0.08f, stone, root, false);
            var column = Cylinder("Shaft", foot + Vector3.up * 0.2f, radius, height - 0.2f, shaft, root, solid);
            column.name = name;
            Block("Bracket", foot + Vector3.up * (height - 0.06f), new Vector3(radius * 2.6f, 0.14f, radius * 2.6f), Darken(shaft), root, false);
            return root.gameObject;
        }

        /// <summary>Raised stone foundation with a step band, the way old houses sit above the yard.</summary>
        public static void Plinth(string name, Vector3 centre, Vector3 size, Material stone, Material top, Transform parent)
        {
            Block(name, centre + Vector3.up * size.y * 0.5f, size, stone, parent);
            Block(name + "Cap", centre + Vector3.up * (size.y + 0.01f), new Vector3(size.x + 0.08f, 0.04f, size.z + 0.08f), top, parent, false);
        }

        /// <summary>Lattice window: a frame with crossed slats, set into (not cut out of) a wall face.</summary>
        public static GameObject Lattice(string name, Vector3 centre, float width, float height, float yaw, Material frame, Material paper, Transform parent)
        {
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.SetPositionAndRotation(centre, Quaternion.Euler(0, yaw, 0));
            Child(root, "Paper", Vector3.zero, new Vector3(width, height, 0.02f), paper);
            Child(root, "Top", new Vector3(0, height * 0.5f, 0.02f), new Vector3(width + 0.12f, 0.08f, 0.06f), frame);
            Child(root, "Bottom", new Vector3(0, -height * 0.5f, 0.02f), new Vector3(width + 0.12f, 0.08f, 0.06f), frame);
            Child(root, "Left", new Vector3(-width * 0.5f, 0, 0.02f), new Vector3(0.08f, height, 0.06f), frame);
            Child(root, "Right", new Vector3(width * 0.5f, 0, 0.02f), new Vector3(0.08f, height, 0.06f), frame);
            int columns = Mathf.Max(2, Mathf.RoundToInt(width / 0.22f));
            int rows = Mathf.Max(2, Mathf.RoundToInt(height / 0.22f));
            for (int i = 1; i < columns; i++) Child(root, "SlatV", new Vector3(-width * 0.5f + width * i / columns, 0, 0.025f), new Vector3(0.025f, height, 0.03f), frame);
            for (int i = 1; i < rows; i++) Child(root, "SlatH", new Vector3(0, -height * 0.5f + height * i / rows, 0.025f), new Vector3(width, 0.025f, 0.03f), frame);
            return root.gameObject;
        }

        /// <summary>Pair of red couplet boards (câu đối) with gold characters suggested by small blocks.</summary>
        public static void Couplets(Vector3 left, Vector3 right, float yaw, Transform parent)
        {
            var board = Material("CoupletRed", new Color(0.45f, 0.05f, 0.04f), 0.55f);
            var gold = Emissive("CoupletGold", new Color(0.85f, 0.62f, 0.22f), 0.25f);
            foreach (var p in new[] { left, right })
            {
                var root = new GameObject("Couplet").transform;
                root.SetParent(parent, false);
                root.SetPositionAndRotation(p, Quaternion.Euler(0, yaw, 0));
                Child(root, "Board", Vector3.zero, new Vector3(0.34f, 1.6f, 0.04f), board);
                for (int i = 0; i < 6; i++) Child(root, "Glyph", new Vector3(0, 0.62f - i * 0.25f, -0.025f), new Vector3(0.16f, 0.13f, 0.01f), gold);
            }
        }

        /// <summary>Hanging bamboo blind (mành tre), slightly rolled at the top.</summary>
        public static void Blind(string name, Vector3 top, float width, float drop, float yaw, Transform parent)
        {
            var slat = Material("BlindBamboo", new Color(0.62f, 0.5f, 0.28f), 0.25f);
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.SetPositionAndRotation(top, Quaternion.Euler(0, yaw, 0));
            var roll = Cylinder("Roll", Vector3.zero, 0.06f, width, slat, root, false);
            roll.transform.localPosition = Vector3.zero;
            roll.transform.localRotation = Quaternion.Euler(0, 0, 90);
            for (float y = 0.08f; y < drop; y += 0.045f)
                Child(root, "Slat", new Vector3(0, -y, 0), new Vector3(width, 0.03f, 0.012f), slat);
            Child(root, "Cord", new Vector3(-width * 0.3f, -drop * 0.5f, -0.02f), new Vector3(0.01f, drop, 0.01f), Material("BlindCord", new Color(0.3f, 0.12f, 0.08f)));
            Child(root, "Cord", new Vector3(width * 0.3f, -drop * 0.5f, -0.02f), new Vector3(0.01f, drop, 0.01f), Material("BlindCord", new Color(0.3f, 0.12f, 0.08f)));
        }

        /// <summary>Silk drape hanging in a shallow swag between two points (balconies of the lầu xanh).</summary>
        public static void Drape(string name, Vector3 a, Vector3 b, float sag, Color color, Transform parent)
        {
            var silk = Material("Silk_" + ColorUtility.ToHtmlStringRGB(color), color, 0.55f);
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            const int segments = 10;
            for (int i = 0; i < segments; i++)
            {
                float t0 = i / (float)segments, t1 = (i + 1f) / segments;
                Vector3 p0 = Vector3.Lerp(a, b, t0) - Vector3.up * sag * 4f * t0 * (1 - t0);
                Vector3 p1 = Vector3.Lerp(a, b, t1) - Vector3.up * sag * 4f * t1 * (1 - t1);
                var piece = Block("Silk", (p0 + p1) * 0.5f - Vector3.up * 0.12f, new Vector3(0.02f, 0.28f, Vector3.Distance(p0, p1) + 0.02f), silk, root, false);
                piece.transform.rotation = Quaternion.LookRotation(p1 - p0, Vector3.up);
                piece.transform.localScale = new Vector3(0.02f, 0.28f, Vector3.Distance(p0, p1) + 0.02f);
            }
        }

        /// <summary>Thin rope between two points (lantern strings, washing lines).</summary>
        public static void Rope(Vector3 a, Vector3 b, float sag, Transform parent)
        {
            var rope = Material("Rope", new Color(0.18f, 0.12f, 0.08f));
            const int segments = 8;
            for (int i = 0; i < segments; i++)
            {
                float t0 = i / (float)segments, t1 = (i + 1f) / segments;
                Vector3 p0 = Vector3.Lerp(a, b, t0) - Vector3.up * sag * 4f * t0 * (1 - t0);
                Vector3 p1 = Vector3.Lerp(a, b, t1) - Vector3.up * sag * 4f * t1 * (1 - t1);
                var piece = Block("Rope", (p0 + p1) * 0.5f, Vector3.one, rope, parent, false);
                piece.transform.rotation = Quaternion.LookRotation(p1 - p0, Vector3.up);
                piece.transform.localScale = new Vector3(0.018f, 0.018f, Vector3.Distance(p0, p1) + 0.01f);
                piece.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            }
        }

        /// <summary>Terracotta pot with a layered shrub: several overlapping leaf clusters instead of a single ball.</summary>
        public static void PottedPlant(Vector3 position, float size, Transform parent, int seed, bool flowers = false)
        {
            var random = new Random(seed);
            var pot = Surface("PotTerracotta", "brown_mud_02", new Color(0.78f, 0.42f, 0.28f), 0.6f, 0.15f, 0.6f);
            var root = new GameObject("PottedPlant").transform;
            root.SetParent(parent, false);
            root.position = position;
            Cylinder("Pot", position, 0.32f * size, 0.55f * size, pot, root);
            Cylinder("PotRim", position + Vector3.up * 0.52f * size, 0.37f * size, 0.07f * size, pot, root, false);
            Cylinder("Soil", position + Vector3.up * 0.5f * size, 0.3f * size, 0.04f * size, Material("Soil", new Color(0.16f, 0.1f, 0.06f)), root, false);
            var leaves = new[] { Material("LeafDark", new Color(0.12f, 0.24f, 0.09f), 0.35f), Material("LeafMid", new Color(0.2f, 0.34f, 0.12f), 0.35f) };
            for (int i = 0; i < 9; i++)
            {
                float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                float r = (float)random.NextDouble() * 0.28f * size;
                var cluster = Sphere("Leaves", position + new Vector3(Mathf.Cos(angle) * r, (0.75f + (float)random.NextDouble() * 0.55f) * size, Mathf.Sin(angle) * r),
                    Vector3.one * (0.28f + (float)random.NextDouble() * 0.2f) * size, leaves[i % 2], root);
                cluster.transform.rotation = Quaternion.Euler(random.Next(360), random.Next(360), random.Next(360));
            }
            if (!flowers) return;
            var bloom = Material("BloomPink", new Color(0.95f, 0.5f, 0.62f), 0.3f);
            for (int i = 0; i < 7; i++)
            {
                float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                Sphere("Flower", position + new Vector3(Mathf.Cos(angle) * 0.26f * size, (1.0f + (float)random.NextDouble() * 0.35f) * size, Mathf.Sin(angle) * 0.26f * size),
                    Vector3.one * 0.08f * size, bloom, root);
            }
        }

        /// <summary>Cluster of glazed water jars (chum) with lids.</summary>
        public static void Jars(Vector3 position, int count, Transform parent, int seed)
        {
            var random = new Random(seed);
            var glaze = Material("JarGlaze", new Color(0.32f, 0.2f, 0.12f), 0.65f);
            var lid = Material("JarLid", new Color(0.2f, 0.14f, 0.08f), 0.3f);
            for (int i = 0; i < count; i++)
            {
                float s = 0.7f + (float)random.NextDouble() * 0.5f;
                Vector3 p = position + new Vector3(((float)random.NextDouble() - 0.5f) * 1.2f, 0, ((float)random.NextDouble() - 0.5f) * 1.2f);
                Sphere("Jar", p + Vector3.up * 0.38f * s, new Vector3(0.62f, 0.78f, 0.62f) * s, glaze, parent, true);
                Cylinder("JarLid", p + Vector3.up * 0.72f * s, 0.2f * s, 0.05f * s, lid, parent, false);
            }
        }

        // ------------------------------------------------------------------ rocks

        private static readonly Dictionary<int, Mesh> RockMeshes = new Dictionary<int, Mesh>();

        /// <summary>Noise-displaced sphere: an irregular boulder instead of a squashed ball.</summary>
        public static GameObject Rock(Vector3 position, Vector3 scale, float yaw, Transform parent, int variant, bool solid = false)
        {
            var go = Block("Rock", position, scale, Surface("RockMoss", "mossy_rock", new Color(0.85f, 0.85f, 0.82f), 1.6f, 0.15f, 1.2f, 0.4f), parent, solid, yaw);
            go.GetComponent<MeshFilter>().sharedMesh = RockMesh(variant % 4);
            if (solid) Object.DestroyImmediate(go.GetComponent<BoxCollider>());
            if (solid) go.AddComponent<MeshCollider>().sharedMesh = go.GetComponent<MeshFilter>().sharedMesh;
            return go;
        }

        private static Mesh RockMesh(int variant)
        {
            string path = AssetFolder + "/Rock_" + variant + ".asset";
            if (RockMeshes.TryGetValue(variant, out Mesh cached) && cached != null && AssetDatabase.GetAssetPath(cached) == path) return cached;
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null) return RockMeshes[variant] = existing;
            var temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var mesh = Object.Instantiate(temp.GetComponent<MeshFilter>().sharedMesh);
            Object.DestroyImmediate(temp);
            var vertices = mesh.vertices;
            float seed = variant * 13.7f;
            var displaced = new Dictionary<Vector3, Vector3>();
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 v = vertices[i];
                if (!displaced.TryGetValue(v, out Vector3 moved))
                {
                    Vector3 n = v.normalized;
                    float noise = Mathf.PerlinNoise(n.x * 2.1f + seed, n.z * 2.1f + n.y * 1.3f) * 0.38f
                                + Mathf.PerlinNoise(n.x * 5.3f + seed, n.y * 5.3f + n.z) * 0.12f;
                    moved = n * (0.38f + noise);
                    moved.y = moved.y < 0 ? moved.y * 0.25f : moved.y * 0.8f; // flat bottom sits on the ground
                    displaced[v] = moved;
                }
                vertices[i] = moved;
            }
            mesh.name = "Rock_" + variant;
            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, path);
            return RockMeshes[variant] = mesh;
        }

        // ------------------------------------------------------------------ grass

        /// <summary>
        /// Scatters crossed-quad grass tufts where <paramref name="accept"/> allows, merged into a few large meshes.
        /// Two tints alternate so the field is not one flat colour.
        /// </summary>
        public static void Grass(string name, Transform parent, Rect area, int count, Func<Vector2, bool> accept, int seed, float height = 0.45f,
            Color? tintA = null, Color? tintB = null, string key = "GrassBlades")
        {
            var random = new Random(seed);
            var materials = new[] { GrassMaterial(key + "A", tintA ?? new Color(0.82f, 0.9f, 0.55f)), GrassMaterial(key + "B", tintB ?? new Color(0.95f, 0.88f, 0.55f)) };
            var lists = new[] { new List<CombineInstance>(), new List<CombineInstance>() };
            var tuft = TuftMesh();
            for (int i = 0; i < count; i++)
            {
                var p = new Vector2(area.xMin + (float)random.NextDouble() * area.width, area.yMin + (float)random.NextDouble() * area.height);
                if (accept != null && !accept(p)) continue;
                float s = height * (0.6f + (float)random.NextDouble() * 0.8f);
                var matrix = Matrix4x4.TRS(new Vector3(p.x, 0f, p.y), Quaternion.Euler(0, random.Next(360), 0), new Vector3(s * 1.2f, s, s * 1.2f));
                lists[random.Next(2)].Add(new CombineInstance { mesh = tuft, transform = matrix });
            }
            for (int m = 0; m < 2; m++)
            {
                // Chunk so each merged mesh stays under 65k vertices and can be culled.
                for (int start = 0, chunk = 0; start < lists[m].Count; start += 4000, chunk++)
                {
                    var part = lists[m].GetRange(start, Mathf.Min(4000, lists[m].Count - start)).ToArray();
                    var mesh = new Mesh { name = name + "_" + m + "_" + chunk };
                    mesh.CombineMeshes(part, true, true);
                    mesh.RecalculateBounds();
                    AssetDatabase.CreateAsset(mesh, AssetFolder + "/" + mesh.name + ".asset");
                    var go = new GameObject(mesh.name);
                    go.transform.SetParent(parent, false);
                    go.AddComponent<MeshFilter>().sharedMesh = mesh;
                    var renderer = go.AddComponent<MeshRenderer>();
                    renderer.sharedMaterial = materials[m];
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    go.isStatic = true;
                }
            }
        }

        private static Mesh _tuft;

        private static Mesh TuftMesh()
        {
            string path = AssetFolder + "/GrassTuft.asset";
            if (_tuft != null && AssetDatabase.GetAssetPath(_tuft) == path) return _tuft;
            _tuft = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (_tuft != null) return _tuft;
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            for (int q = 0; q < 3; q++)
            {
                float angle = q * 60f * Mathf.Deg2Rad;
                Vector3 side = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 0.5f;
                int b = vertices.Count;
                vertices.AddRange(new[] { -side, side, side + Vector3.up, -side + Vector3.up });
                // Upward normals light the blades like the ground beneath them (no dark backfaces).
                normals.AddRange(new[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up });
                uvs.AddRange(new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) });
                triangles.AddRange(new[] { b, b + 2, b + 1, b, b + 3, b + 2 });
            }
            _tuft = new Mesh { name = "GrassTuft" };
            _tuft.SetVertices(vertices);
            _tuft.SetNormals(normals);
            _tuft.SetUVs(0, uvs);
            _tuft.SetTriangles(triangles, 0);
            _tuft.RecalculateBounds();
            AssetDatabase.CreateAsset(_tuft, path);
            return _tuft;
        }

        private static Material GrassMaterial(string name, Color tint)
        {
            string path = AssetFolder + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetTexture("_BaseMap", GeneratedTexture("GrassBlades", DrawBlades));
            material.SetColor("_BaseColor", tint);
            material.SetFloat("_AlphaClip", 1f);
            material.SetFloat("_Cutoff", 0.45f);
            material.EnableKeyword("_ALPHATEST_ON");
            material.SetFloat("_Cull", 0f);
            material.SetFloat("_Smoothness", 0.1f);
            material.SetFloat("_SpecularHighlights", 0f);
            material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
            material.renderQueue = (int)RenderQueue.AlphaTest;
            material.SetOverrideTag("RenderType", "TransparentCutout");
            material.doubleSidedGI = true;
            material.enableInstancing = true;
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void DrawBlades(Color32[] pixels, int size)
        {
            var random = new Random(5);
            for (int b = 0; b < 30; b++)
            {
                float x0 = 0.08f + (float)random.NextDouble() * 0.84f;
                float height = 0.45f + (float)random.NextDouble() * 0.53f;
                float lean = ((float)random.NextDouble() - 0.5f) * 0.35f;
                float width = 0.012f + (float)random.NextDouble() * 0.014f;
                float shade = 0.55f + (float)random.NextDouble() * 0.45f;
                for (int y = 0; y < size * height; y++)
                {
                    float t = y / (size * height);
                    float cx = (x0 + lean * t * t) * size;
                    float half = width * size * (1 - t * 0.92f);
                    for (int x = Mathf.FloorToInt(cx - half); x <= Mathf.CeilToInt(cx + half); x++)
                    {
                        if (x < 0 || x >= size) continue;
                        float tone = shade * (0.55f + 0.45f * t);
                        pixels[y * size + x] = new Color(0.62f * tone + 0.18f, 0.85f * tone + 0.1f, 0.38f * tone + 0.04f, 1f);
                    }
                }
            }
        }

        // ------------------------------------------------------------------ particles

        /// <summary>Slow sunlit dust motes drifting in a volume (courtyards, interiors).</summary>
        public static ParticleSystem Dust(string name, Vector3 centre, Vector3 size, Transform parent, Color color, int count = 140)
        {
            var ps = Particles(name, centre, parent, ParticleMaterial("ParticleDust", false));
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(8f, 14f);
            main.startSpeed = 0.03f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.015f, 0.04f);
            main.startColor = color;
            main.maxParticles = count;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.prewarm = true;
            var emission = ps.emission;
            emission.rateOverTime = count / 11f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = size;
            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.08f;
            noise.frequency = 0.25f;
            FadeInOut(ps);
            return ps;
        }

        /// <summary>Blinking fireflies over fields and water at night.</summary>
        public static ParticleSystem Fireflies(string name, Vector3 centre, Vector3 size, Transform parent, int count = 60)
        {
            var ps = Particles(name, centre, parent, ParticleMaterial("ParticleGlow", true));
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(4f, 7f);
            main.startSpeed = 0.15f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.09f);
            main.startColor = new Color(0.85f, 1f, 0.45f, 1f);
            main.maxParticles = count;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.prewarm = true;
            var emission = ps.emission;
            emission.rateOverTime = count / 5f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = size;
            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.5f;
            noise.frequency = 0.4f;
            var colour = ps.colorOverLifetime;
            colour.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.15f), new GradientAlphaKey(0.1f, 0.35f), new GradientAlphaKey(1f, 0.55f), new GradientAlphaKey(0.2f, 0.75f), new GradientAlphaKey(0f, 1f) });
            colour.color = gradient;
            return ps;
        }

        /// <summary>Leaves drifting down and tumbling (bamboo groves, the courtyard tree).</summary>
        public static ParticleSystem Leaves(string name, Vector3 centre, Vector3 size, Transform parent, Color color, int count = 30)
        {
            var ps = Particles(name, centre, parent, ParticleMaterial("ParticleLeaf", false, "Leaf", DrawLeaf));
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 10f);
            main.startSpeed = 0.05f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.07f, 0.13f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new ParticleSystem.MinMaxGradient(color, Color.Lerp(color, new Color(0.55f, 0.4f, 0.15f), 0.6f));
            main.gravityModifier = 0.04f;
            main.maxParticles = count;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.prewarm = true;
            var emission = ps.emission;
            emission.rateOverTime = count / 8f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = size;
            var rotation = ps.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-2f, 2f);
            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.4f;
            noise.frequency = 0.3f;
            FadeInOut(ps);
            return ps;
        }

        private static ParticleSystem Particles(string name, Vector3 centre, Transform parent, Material material)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = centre;
            var ps = go.AddComponent<ParticleSystem>();
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return ps;
        }

        private static void FadeInOut(ParticleSystem ps)
        {
            var colour = ps.colorOverLifetime;
            colour.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) });
            colour.color = gradient;
        }

        private static Material ParticleMaterial(string name, bool additive, string textureName = "SoftDot", Action<Color32[], int> draw = null)
        {
            string path = AssetFolder + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            material.SetTexture("_BaseMap", GeneratedTexture(textureName, draw ?? DrawDot));
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", additive ? 2f : 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", additive ? (float)BlendMode.One : (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)RenderQueue.Transparent;
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void DrawDot(Color32[] pixels, int size)
        {
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), Vector2.one * size * 0.5f) / (size * 0.5f);
                    float a = Mathf.Clamp01(1 - d);
                    pixels[y * size + x] = new Color(1, 1, 1, a * a);
                }
        }

        private static void DrawLeaf(Color32[] pixels, int size)
        {
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float u = (x + 0.5f) / size * 2 - 1, v = (y + 0.5f) / size * 2 - 1;
                    float width = 0.42f * Mathf.Sin(Mathf.Clamp01((v + 1) * 0.5f) * Mathf.PI);
                    bool inside = Mathf.Abs(u) < width;
                    float rib = Mathf.Abs(u) < 0.03f ? 0.75f : 1f;
                    pixels[y * size + x] = inside ? new Color(rib, rib, rib, 1) : new Color(1, 1, 1, 0);
                }
        }

        private static Texture2D GeneratedTexture(string name, Action<Color32[], int> draw, int size = 256)
        {
            string path = AssetFolder + "/" + name + ".png";
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing != null) return existing;
            var pixels = new Color32[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, 255, 255, 0);
            draw(pixels, size);
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = true;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        // ------------------------------------------------------------------ small helpers

        public static GameObject Sphere(string name, Vector3 position, Vector3 scale, Material material, Transform parent, bool solid = false)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.layer = solid ? EnvironmentLayer : 0;
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (!solid) Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        private static GameObject Child(Transform root, string name, Vector3 local, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(root, false);
            go.transform.localPosition = local;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        private static Material Darken(Material source)
        {
            string name = source.name + "_Dark";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(AssetFolder + "/" + name + ".mat");
            if (existing != null) return existing;
            var copy = new Material(source) { name = name };
            copy.SetColor("_BaseColor", source.GetColor("_BaseColor") * 0.55f);
            AssetDatabase.CreateAsset(copy, AssetFolder + "/" + name + ".mat");
            return copy;
        }

        /// <summary>Marks walls the orbit camera may look through (hidden while between camera and player).</summary>
        public static void Fadeable(params GameObject[] objects)
        {
            foreach (var go in objects) if (go != null && go.GetComponent<CameraFadeable>() == null) go.AddComponent<CameraFadeable>();
        }

        /// <summary>Fadeable wall whose mounted details (windows, boards) hide together with it.</summary>
        public static void FadeWith(GameObject wall, params GameObject[] linked)
        {
            var fade = wall.GetComponent<CameraFadeable>() ?? wall.AddComponent<CameraFadeable>();
            Set(fade, "_linked", linked);
        }

        /// <summary>Camera framing override for a region (rooms frame tighter, open roads wider).</summary>
        public static CameraZone Zone(string name, Vector3 centre, Vector3 size, float distance, float shoulder, float fov, float height, Transform parent, int priority = 0)
        {
            var go = new GameObject("CameraZone_" + name);
            go.transform.SetParent(parent, false);
            go.transform.position = centre;
            var zone = go.AddComponent<CameraZone>();
            Set(zone, "_size", size);
            Set(zone, "_distance", distance);
            Set(zone, "_shoulder", shoulder);
            Set(zone, "_fov", fov);
            Set(zone, "_height", height);
            Set(zone, "_priority", priority);
            return zone;
        }
    }
}
