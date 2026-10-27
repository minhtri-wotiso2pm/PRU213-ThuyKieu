using System.Collections.Generic;
using System.Linq;
using ThuyKieu.Environment;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThuyKieu.Core.Editor
{
    // The forest's materials and textures (Chapter 5 only; nothing shared is changed): the ground from Poly Haven's Brown
    // Mud 02 at 2K with its AO and roughness, the scanned fern and mossy rocks (Poly Haven, CC0, see
    // Art/Environment/Chapter05/CREDITS.md), and the bamboo's own drawn textures. Every texture and material is an asset
    // updated in place: drawn and packed files are rewritten only when their pixels change, materials keep their GUIDs.
    public static partial class Chapter05Forest
    {
        public const string ArtRoot = "Assets/_Game/Art/Environment/Chapter05/PolyHaven";
        private const string GroundSource = ArtRoot + "/brown_mud_02/brown_mud_02_";
        private const string FernSource = ArtRoot + "/fern_02/fern_02";
        private const string RockSource = ArtRoot + "/rock_moss_set_01/rock_moss_set_01";
        /// <summary>The drier, crumbling earth of the bank faces (a texture the project already has; read as it is imported).</summary>
        private const string FaceSource = StoryArtKit.TextureRoot + "/brown_mud_dry/brown_mud_dry_";
        private const float LeafCutoff = 0.45f;

        // ------------------------------------------------------------------ ground

        /// <summary>
        /// The one material of the forest floor, the way and its banks (Ch5ForestGround): Brown Mud 02 at its real scale (a
        /// repeat covers 2.6 m: small stones and crumbs of earth at their size), its AO and roughness packed for the shader,
        /// the drier earth on the faces; trodden, damp and leafy places from the ground's vertex colours.
        /// </summary>
        private static Material FloorMaterial()
        {
            string path = Folder + "/C5F_Floor.mat";
            var shader = Shader.Find("ThuyKieu/Ch5ForestGround");
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool create = material == null;
            if (create) material = new Material(shader);
            else material.shader = shader;
            material.shaderKeywords = new string[0];
            material.SetTexture("_BaseMap", Source(GroundSource + "diff_2k.jpg", TextureImporterType.Default, true, 2048));
            material.SetTextureScale("_BaseMap", Vector2.one / 2.6f);
            material.SetTexture("_BumpMap", Source(GroundSource + "nor_gl_2k.jpg", TextureImporterType.NormalMap, false, 2048));
            material.SetTexture("_MetallicGlossMap", Pack("C5F_GroundARM", 2048, (ao, rough) => new Color(ao, rough, 0f, 1f), GroundSource + "ao_2k.jpg", GroundSource + "rough_2k.jpg"));
            material.SetTexture("_DetailAlbedoMap", AssetDatabase.LoadAssetAtPath<Texture2D>(FaceSource + "diff_1k.jpg"));
            material.SetTextureScale("_DetailAlbedoMap", Vector2.one / 2.2f);
            material.SetTexture("_DetailNormalMap", AssetDatabase.LoadAssetAtPath<Texture2D>(FaceSource + "nor_gl_1k.jpg"));
            material.SetColor("_BaseColor", new Color(0.7f, 0.61f, 0.49f));
            material.SetFloat("_BumpScale", 1.1f);
            material.SetFloat("_DetailNormalMapScale", 1.2f);
            material.SetFloat("_Smoothness", 0.42f);
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_OcclusionStrength", 0.45f); // at night the light is mostly ambient: the full AO would darken the floor more than it shapes it
            material.SetFloat("_Parallax", 0.45f);
            material.SetColor("_SpecColor", new Color(1.12f, 1.08f, 1.02f)); // packed earth: a little paler and greyer than the leafy floor
            material.SetColor("_EmissionColor", new Color(0.36f, 0.27f, 0.17f));
            material.SetFloat("_ClearCoatMask", 0.5f);
            material.SetFloat("_DetailAlbedoMapScale", 0.2f);
            material.SetFloat("_ClearCoatSmoothness", 0.6f);
            material.enableInstancing = true;
            if (create) AssetDatabase.CreateAsset(material, path);
            else EditorUtility.SetDirty(material);
            return material;
        }

        // ------------------------------------------------------------------ Poly Haven sources

        /// <summary>One of the downloaded maps, imported for what it is: colour (sRGB) or data (linear), normal maps as normal maps.</summary>
        private static Texture2D Source(string path, TextureImporterType type, bool colour, int size)
        {
            if (AssetImporter.GetAtPath(path) is TextureImporter importer
                && (importer.textureType != type || importer.sRGBTexture != colour || importer.maxTextureSize != size || importer.anisoLevel != 8 || importer.wrapMode != TextureWrapMode.Repeat))
            {
                importer.textureType = type;
                importer.sRGBTexture = colour;
                importer.maxTextureSize = size;
                importer.anisoLevel = 8;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.SaveAndReimport();
            }
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null) throw new System.InvalidOperationException("Missing " + path);
            return texture;
        }

        /// <summary>
        /// Two greyscale maps (red channel) packed into one texture by <paramref name="pack"/>, kept as a PNG and imported
        /// as linear data (rewritten only when its pixels change).
        /// </summary>
        private static Texture2D Pack(string name, int size, System.Func<float, float, Color> pack, string a, string b)
        {
            var first = Read(a, size);
            var second = Read(b, size);
            var pixels = new Color32[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = pack(first[i].r / 255f, second[i].r / 255f);
            return Store(name, size, size, pixels, false, true, false);
        }

        /// <summary>A colour map with its alpha from another map (the fern's cut-out), as one RGBA PNG.</summary>
        private static Texture2D WithAlpha(string name, int size, string colour, string alpha)
        {
            var rgb = Read(colour, size);
            var cut = Read(alpha, size);
            for (int i = 0; i < rgb.Length; i++) rgb[i].a = cut[i].r;
            Dilate(rgb, size, size, 12);
            return Store(name, size, size, rgb, true, false, true);
        }

        private static Color32[] Read(string path, int size)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            texture.LoadImage(System.IO.File.ReadAllBytes(path));
            if (texture.width != size || texture.height != size) throw new System.InvalidOperationException($"{path} is {texture.width}×{texture.height}, not {size}");
            var pixels = texture.GetPixels32();
            Object.DestroyImmediate(texture);
            return pixels;
        }

        /// <summary>
        /// A drawn or packed texture kept as a PNG in the forest's folder: written only when its pixels changed (so the file,
        /// its GUID and its import stay), imported as colour or data, with mipmaps that keep a cut-out's coverage.
        /// </summary>
        /// <summary>Cut-outs whose mipmaps may thin with distance as usual (the canopy far overhead, the skyline cards, litter, bushes): keeping their coverage costs more than it shows.</summary>
        private static readonly HashSet<string> Thinning = new HashSet<string> { "C5F_CanopyClump", "C5F_SkylineTex", "C5F_LitterTex", "C5F_Shrub" };

        private static Texture2D Store(string name, int width, int height, Color32[] pixels, bool colour, bool repeat, bool cutout, bool normal = false)
        {
            bool coverage = cutout && !Thinning.Contains(name);
            string path = Folder + "/" + name + ".png";
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            texture.Apply();
            byte[] png = texture.EncodeToPNG();
            Object.DestroyImmediate(texture);
            if (!System.IO.File.Exists(path) || !System.IO.File.ReadAllBytes(path).SequenceEqual(png))
            {
                System.IO.File.WriteAllBytes(path, png);
                AssetDatabase.ImportAsset(path);
            }
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            var type = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            var wrap = repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            int size = Mathf.Max(width, height);
            if (importer.textureType != type || importer.wrapMode != wrap || importer.sRGBTexture != (colour && !normal) || importer.alphaIsTransparency != cutout
                || !importer.mipmapEnabled || importer.mipMapsPreserveCoverage != coverage || (coverage && !Mathf.Approximately(importer.alphaTestReferenceValue, LeafCutoff))
                || importer.maxTextureSize != size || importer.anisoLevel != 4)
            {
                importer.textureType = type;
                importer.sRGBTexture = colour && !normal;
                importer.alphaIsTransparency = cutout;
                importer.wrapMode = wrap;
                importer.mipmapEnabled = true;
                // Cut-outs keep the same share of opaque pixels down the mip chain, so leaves do not thin away with distance.
                importer.mipMapsPreserveCoverage = coverage;
                if (coverage) importer.alphaTestReferenceValue = LeafCutoff;
                importer.maxTextureSize = size;
                importer.anisoLevel = 4;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        /// <summary>
        /// Fills the colour of transparent pixels from their opaque neighbours (a few rings out, then the average), so the
        /// mipmaps of a cut-out blend into leaf colour at its edges instead of a white or black fringe.
        /// </summary>
        private static void Dilate(Color32[] pixels, int width, int height, int rings)
        {
            var filled = new bool[pixels.Length];
            long r = 0, g = 0, b = 0, n = 0;
            for (int i = 0; i < pixels.Length; i++)
            {
                filled[i] = pixels[i].a > 8;
                if (!filled[i]) continue;
                r += pixels[i].r; g += pixels[i].g; b += pixels[i].b; n++;
            }
            if (n == 0) return;
            var next = new bool[pixels.Length];
            for (int ring = 0; ring < rings; ring++)
            {
                System.Array.Copy(filled, next, filled.Length);
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        int i = y * width + x;
                        if (filled[i]) continue;
                        int cr = 0, cg = 0, cb = 0, count = 0;
                        for (int dy = -1; dy <= 1; dy++)
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                int xx = x + dx, yy = y + dy;
                                if (xx < 0 || yy < 0 || xx >= width || yy >= height) continue;
                                int j = yy * width + xx;
                                if (!filled[j]) continue;
                                cr += pixels[j].r; cg += pixels[j].g; cb += pixels[j].b; count++;
                            }
                        if (count == 0) continue;
                        pixels[i] = new Color32((byte)(cr / count), (byte)(cg / count), (byte)(cb / count), pixels[i].a);
                        next[i] = true;
                    }
                System.Array.Copy(next, filled, filled.Length);
            }
            var average = new Color32((byte)(r / n), (byte)(g / n), (byte)(b / n), 0);
            for (int i = 0; i < pixels.Length; i++)
                if (!filled[i]) pixels[i] = new Color32(average.r, average.g, average.b, pixels[i].a);
        }

        /// <summary>The meshes of a downloaded model (no materials, cameras or animation imported), with Mesh LODs made on import when asked.</summary>
        private static Mesh[] ModelMeshes(string path, bool lods)
        {
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            if (importer.materialImportMode != ModelImporterMaterialImportMode.None || importer.importAnimation || importer.importCameras
                || importer.importLights || importer.generateMeshLods != lods || importer.isReadable || importer.useFileScale || !Mathf.Approximately(importer.globalScale, 1f))
            {
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.importAnimation = false;
                importer.importCameras = false;
                importer.importLights = false;
                importer.generateMeshLods = lods;
                importer.isReadable = false;
                // The scans are in metres in a file that calls its unit a centimetre: read as they are, they come out true to size.
                importer.useFileScale = false;
                importer.globalScale = 1f;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Mesh>().OrderBy(m => m.name).ToArray();
        }

        /// <summary>URP Lit for a scanned asset: albedo (with cut-out), normal, metallic 0 with smoothness from 1 − roughness and AO, from one packed mask.</summary>
        private static Material Scanned(string name, Texture2D albedo, Texture2D normal, Texture2D mask, Color tint, float smoothness, bool cutout)
        {
            string path = Folder + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool create = material == null;
            if (create) material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.shaderKeywords = new string[0];
            material.SetTexture("_BaseMap", albedo);
            material.SetColor("_BaseColor", tint);
            material.SetTexture("_BumpMap", normal);
            material.SetFloat("_BumpScale", 1f);
            material.EnableKeyword("_NORMALMAP");
            // URP Lit reads metallic from r and smoothness from a of this map (channel 0 = metallic alpha), occlusion from g.
            material.SetTexture("_MetallicGlossMap", mask);
            material.SetFloat("_SmoothnessTextureChannel", 0f);
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_Metallic", 0f);
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.SetTexture("_OcclusionMap", mask);
            material.SetFloat("_OcclusionStrength", 1f);
            material.EnableKeyword("_OCCLUSIONMAP");
            material.SetFloat("_Cull", cutout ? 0f : 2f);
            material.SetFloat("_AlphaClip", cutout ? 1f : 0f);
            material.SetFloat("_AlphaToMask", cutout ? 1f : 0f); // with MSAA, cut-out edges are smoothed too
            material.SetFloat("_Cutoff", 0.5f);
            if (cutout)
            {
                material.EnableKeyword("_ALPHATEST_ON");
                material.renderQueue = (int)RenderQueue.AlphaTest;
                material.SetOverrideTag("RenderType", "TransparentCutout");
            }
            material.doubleSidedGI = cutout;
            material.enableInstancing = true;
            if (create) AssetDatabase.CreateAsset(material, path);
            else EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>
        /// The scans are stored lying on their backs, up along the mesh's z (their files keep the turn in the object, not
        /// the mesh): this stands one up. Which way z points is read off the fern, whose clumps stand on z = 0.
        /// </summary>
        private static Quaternion ScanUpright => BambooGrove.Upright(FernMeshes()[0]);

        /// <summary>The four scanned fern clumps (about 800–2,400 triangles each, a Mesh LOD chain each).</summary>
        private static Mesh[] FernMeshes() => ModelMeshes(FernSource + ".fbx", true);

        private static Material FernMaterial() => Scanned("C5F_FernScan",
            WithAlpha("C5F_FernAlbedo", 2048, FernSource + "_diff_2k.jpg", FernSource + "_alpha_2k.jpg"),
            Source(FernSource + "_nor_gl_2k.jpg", TextureImporterType.NormalMap, false, 2048),
            Pack("C5F_FernMask", 2048, (ao, rough) => new Color(0f, ao, 0f, 1f - rough), FernSource + "_ao_2k.jpg", FernSource + "_rough_2k.jpg"),
            new Color(0.62f, 0.68f, 0.56f), 0.55f, true);

        /// <summary>The six scanned mossy rocks (5,000–16,500 triangles each at full detail; a Mesh LOD chain each, so the distant ones cost little).</summary>
        private static Mesh[] RockMeshes() => ModelMeshes(RockSource + ".fbx", true);

        private static Material RockMaterial() => Scanned("C5F_RockMoss",
            Source(RockSource + "_diff_2k.jpg", TextureImporterType.Default, true, 2048),
            Source(RockSource + "_nor_gl_2k.jpg", TextureImporterType.NormalMap, false, 2048),
            Pack("C5F_RockMask", 2048, (ao, rough) => new Color(0f, ao, 0f, 1f - rough), RockSource + "_ao_2k.jpg", RockSource + "_rough_2k.jpg"),
            new Color(0.78f, 0.78f, 0.74f), 0.6f, false);

        // ------------------------------------------------------------------ rocks

        /// <summary>
        /// One scanned rock in place of the drawn boulder the course was laid out with (StoryArtKit.Rock: the same size
        /// and the same rise above <paramref name="position"/>, 0.7 of <paramref name="size"/>.y up and 0.22 down), for the
        /// rocks that are part of the course (a sidestep, a squeeze, the fork, the ditch stones): solid ones have the convex
        /// hull of the same scan as collider, on the ground layer so the NavMesh goes round them.
        /// </summary>
        private static GameObject ScanRock(Vector3 position, Vector3 size, float yaw, Transform parent, int variant, bool solid = false)
        {
            var meshes = RockMeshes();
            var mesh = meshes[variant % meshes.Length];
            var go = new GameObject("Rock");
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(position + Vector3.up * size.y * 0.24f, Quaternion.Euler(0f, yaw, 0f) * ScanUpright);
            // In the mesh's own axes: x across, y (lying) the depth, z (lying) the height.
            Vector3 extent = mesh.bounds.size;
            go.transform.localScale = new Vector3(size.x * 1.15f / extent.x, size.z * 1.15f / extent.y, size.y * 0.92f / extent.z);
            go.transform.position -= go.transform.rotation * Vector3.Scale(mesh.bounds.center, go.transform.localScale);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = RockMaterial();
            if (!solid) return go;
            go.layer = StorySceneKit.EnvironmentLayer;
            go.AddComponent<MeshCollider>().sharedMesh = mesh;
            go.GetComponent<MeshCollider>().convex = true;
            return go;
        }

        /// <summary>
        /// Rock where the ground would show it: outcrops sunk into the tall cut banks (half or more inside the earth, never
        /// standing proud of the face where she runs), a few stones at the feet of low banks, groups of boulders out on the
        /// forest floor and down the slopes (never singly, never spread evenly), and the stones of the ravine. None has a
        /// collider: those in the banks lie inside the bank's own, the rest are out of her reach.
        /// </summary>
        private static void Rocks(Transform parent, Layout layout, ForestGround ground)
        {
            var meshes = RockMeshes();
            var material = RockMaterial();
            if (meshes.Length == 0) return;
            var root = new GameObject("Rocks").transform;
            root.SetParent(parent, false);
            var random = new System.Random(43);
            float R() => (float)random.NextDouble();
            int placed = 0;

            void Place(Vector3 at, float size, float sink, Vector3 up, bool shadows)
            {
                var mesh = meshes[random.Next(meshes.Length)];
                Vector3 size3 = ScanUpright * mesh.bounds.size;
                var extent = new Vector3(Mathf.Abs(size3.x), Mathf.Abs(size3.y), Mathf.Abs(size3.z));
                float scale = size / Mathf.Max(0.01f, Mathf.Max(extent.x, extent.z));
                var go = new GameObject("Rock");
                go.transform.SetParent(root, false);
                var turn = Quaternion.FromToRotation(Vector3.up, Vector3.Slerp(Vector3.up, up, 0.5f)) * Quaternion.Euler(0f, R() * 360f, 0f);
                // The bottom of the scan (stood up) sinks into the ground by the given share of its height.
                Vector3 bottom = ScanUpright * mesh.bounds.center - Vector3.up * extent.y * 0.5f;
                go.transform.SetPositionAndRotation(at - turn * (bottom * scale) - up * extent.y * scale * sink, turn * ScanUpright);
                go.transform.localScale = Vector3.one * scale;
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = go.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
                placed++;
            }

            Vector3 NormalAt(Vector3 p)
            {
                const float e = 0.35f;
                float h = ground.Height(p.x, p.z);
                return new Vector3(h - ground.Height(p.x + e, p.z), e, h - ground.Height(p.x, p.z + e)).normalized;
            }

            foreach (int way in new[] { 1, 2 })
            {
                float from = way == 1 ? layout.StartAt + 8f : layout.ThickForkFrom, to = way == 1 ? layout.BridgeNear - 8f : layout.ThickForkTo;
                var marks = layout.Marks.Where(m => m.Way == 0 || m.Way == way).Select(m => m.S).ToList();
                // The banks: every few metres, one side or the other.
                for (float s = from; s < to; s += 5f + R() * 9f)
                {
                    if (marks.Any(m => Mathf.Abs(m - s) < 5f)) continue;
                    Vector3 centre = layout.At(way, s), dir = layout.DirAt(way, s);
                    int side = random.Next(2) == 0 ? -1 : 1;
                    var outward = new Vector3(dir.z, 0f, -dir.x) * side;
                    Vector3 edge = centre + outward * layout.HalfAt(way, s);
                    if (ground.InCutAt(edge.x, edge.z)) continue;
                    float rise = ground.Height(edge.x + outward.x * 1.2f, edge.z + outward.z * 1.2f) - centre.y;
                    if (rise > 1.9f)
                    {
                        // An outcrop in the face: find where the face is at a height of 0.5–1.3 m, then push the rock into it.
                        float want = 0.5f + R() * 0.8f, e = 0.2f;
                        while (e < 1.6f && ground.Height(edge.x + outward.x * e, edge.z + outward.z * e) - centre.y < want) e += 0.05f;
                        float size = 0.5f + R() * 0.45f;
                        Vector3 at = edge + outward * (e + size * 0.35f);
                        at.y = centre.y + want - size * 0.25f;
                        Place(at, size, 0f, (-outward + Vector3.up).normalized, true);
                    }
                    else
                    {
                        // Stones at the foot of a low bank, half in the earth.
                        int stones = 1 + random.Next(3);
                        for (int i = 0; i < stones; i++)
                        {
                            Vector3 at = edge + outward * (0.25f + R() * 0.25f) + dir * (R() - 0.5f) * 1.2f;
                            at.y = ground.Height(at.x, at.z);
                            Place(at, 0.22f + R() * 0.25f, 0.45f, NormalAt(at), false);
                        }
                    }
                }
                // Out on the floor: groups of two to four, every twenty-odd metres, on one side.
                for (float s = from + 10f; s < to; s += 18f + R() * 20f)
                {
                    Vector3 centre = layout.At(way, s), dir = layout.DirAt(way, s);
                    int side = random.Next(2) == 0 ? -1 : 1;
                    var outward = new Vector3(dir.z, 0f, -dir.x) * side;
                    Vector3 middle = centre + outward * (layout.HalfAt(way, s) + 2.8f + R() * 7f) + dir * (R() - 0.5f) * 4f;
                    int count = 2 + random.Next(3);
                    for (int i = 0; i < count; i++)
                    {
                        Vector3 at = middle + new Vector3(R() - 0.5f, 0f, R() - 0.5f) * 2.6f;
                        if (ground.InCutAt(at.x, at.z) || ground.Corridor(at.x, at.z, out _) < 2.2f) continue;
                        at.y = ground.Height(at.x, at.z);
                        Place(at, (i == 0 ? 1.2f : 0.6f) + R() * (i == 0 ? 0.9f : 0.6f), 0.4f + R() * 0.15f, NormalAt(at), false);
                    }
                }
            }
            // The ravine: stones along the stream and on its banks.
            float mid = (layout.BridgeNear + layout.BridgeFar) * 0.5f;
            for (int i = 0; i < 12; i++)
            {
                Vector3 at = layout.MoonAt(mid) + layout.RightAt(mid) * ((R() - 0.5f) * 40f) + layout.DirAt(1, mid) * ((R() - 0.5f) * 4f);
                at.y = ground.Height(at.x, at.z);
                Place(at, 0.8f + R() * 1.2f, 0.35f, NormalAt(at), false);
            }
            Debug.Log($"[Chapter05Forest] rocks: {placed} placed from {meshes.Length} scanned meshes ({string.Join(", ", meshes.Select(m => $"{m.name} {m.bounds.size.x:F1}×{m.bounds.size.y:F1}×{m.bounds.size.z:F1} lods {m.lodCount}"))})");
        }

        // ------------------------------------------------------------------ bamboo materials

        /// <summary>0 fresh green · 1 old olive · 2 yellowed and dead · 3 dark and mossy.</summary>
        private static readonly Color[] CulmTints = { new Color(0.6f, 0.7f, 0.42f), new Color(0.68f, 0.66f, 0.45f), new Color(0.84f, 0.75f, 0.53f), new Color(0.45f, 0.49f, 0.37f) };

        private static Material CulmMaterial(int variant)
        {
            // Old culms sway less than young ones; dead ones are stiff.
            var material = Foliage("C5F_Culm" + variant, Store("C5F_CulmTex", 128, 512, DrawCulm, true, true, false), CulmTints[variant], 0.62f, false, true,
                variant == 2 ? 0.18f : variant == 0 ? 0.4f : 0.32f, 0f, 0f);
            // Ridged nodes and fine grooves, and the sheen of the wet skin varying with them (smoothness in the albedo's alpha).
            material.SetTexture("_BumpMap", Store("C5F_CulmNormal", 128, 512, DrawCulmNormal, false, true, false, true));
            material.SetFloat("_BumpScale", variant == 3 ? 0.85f : 0.7f);
            material.EnableKeyword("_NORMALMAP");
            material.SetFloat("_SmoothnessTextureChannel", 1f);
            material.EnableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material LeafMaterial() => Foliage("C5F_Leaves", LeafAtlas(), new Color(0.54f, 0.62f, 0.42f), 0.32f, true, true, 0.4f, 0.04f, 0f);
        /// <summary>The same leaves further back: no moonlight through them from behind (a second shadow look-up for every leaf pixel there).</summary>
        private static Material FarLeafMaterial() => Foliage("C5F_LeavesFar", LeafAtlas(), new Color(0.54f, 0.62f, 0.42f), 0.32f, true, true, 0.4f, 0.04f, 0f);
        private static Texture2D LeafAtlas() => Store("C5F_LeafSpray", 1024, 1024, (p, w, h) => DrawSprays(p, w, h, false), true, false, true);
        private static Material DryLeafMaterial() => Foliage("C5F_LeavesDry", Store("C5F_LeafSprayDry", 1024, 1024, (p, w, h) => DrawSprays(p, w, h, true), true, false, true), new Color(0.8f, 0.7f, 0.52f), 0.3f, true, true, 0f, 0f, 0.15f);
        private static Material CanopyMaterial() => Foliage("C5F_Canopy", Store("C5F_CanopyClump", 512, 512, DrawCanopy, true, false, true), new Color(0.48f, 0.56f, 0.38f), 0.25f, true, true, 0.45f, 0.05f, 0f);
        private static Material TuftMaterial() => Foliage("C5F_Ferns", Store("C5F_Fern", 512, 512, DrawFern, true, false, true), new Color(0.56f, 0.66f, 0.44f), 0.15f, true, true, 0.08f, 0.03f, 0.18f);
        private static Material ShrubMaterial() => Foliage("C5F_Shrubs", Store("C5F_Shrub", 512, 512, DrawShrub, true, false, true), new Color(0.52f, 0.6f, 0.42f), 0.2f, true, true, 0.08f, 0.025f, 0.18f);
        private static Material ThicketMaterial() => Foliage("C5F_Thicket", Store("C5F_ThicketTex", 512, 512, DrawThicket, true, true, true), new Color(0.62f, 0.6f, 0.48f), 0.2f, true, true, 0.05f, 0.015f, 0.08f);
        private static Material LitterMaterial() => Foliage("C5F_Litter", Store("C5F_LitterTex", 256, 256, DrawLitter, true, false, true), new Color(0.84f, 0.74f, 0.6f), 0.3f, true, true, 0f, 0f, 0f);
        private static Material RootMaterial() => Foliage("C5F_Roots", Store("C5F_CulmTex", 128, 512, DrawCulm, true, true, false), new Color(0.45f, 0.36f, 0.27f), 0.2f, false, true, 0f, 0f, 0f);
        // The far forest: dark, so the fog gives it its colour and depth.
        private static Material SkylineMaterial() => Foliage("C5F_Skyline", Store("C5F_SkylineTex", 512, 1024, DrawSkyline, true, false, true), new Color(0.3f, 0.34f, 0.28f), 0.05f, true, true, 0.25f, 0f, 0f);

        /// <summary>
        /// A bamboo material (Ch5Bamboo: URP Lit's surface with the night wind): how far its culms' tips swing, how much its
        /// leaves flutter and how much moonlight comes through them from behind. An existing asset is updated in place.
        /// </summary>
        private static Material Foliage(string name, Texture2D texture, Color tint, float smoothness, bool cutout, bool twoSided, float sway, float flutter, float through)
        {
            string path = Folder + "/" + name + ".mat";
            var shader = Shader.Find("ThuyKieu/Ch5Bamboo");
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool create = material == null;
            if (create) material = new Material(shader);
            else material.shader = shader;
            material.shaderKeywords = new string[0];
            material.SetTexture("_BaseMap", texture);
            material.SetColor("_BaseColor", tint);
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_Cull", twoSided ? 0f : 2f);
            material.SetFloat("_Parallax", sway);
            material.SetFloat("_ClearCoatMask", flutter);
            material.SetFloat("_ClearCoatSmoothness", through);
            material.SetFloat("_AlphaClip", cutout ? 1f : 0f);
            material.SetFloat("_AlphaToMask", cutout ? 1f : 0f); // with MSAA, cut-out edges are smoothed too
            material.SetFloat("_Cutoff", LeafCutoff);
            material.SetFloat("_SmoothnessTextureChannel", 0f);
            if (cutout)
            {
                material.EnableKeyword("_ALPHATEST_ON");
                material.renderQueue = (int)RenderQueue.AlphaTest;
                material.SetOverrideTag("RenderType", "TransparentCutout");
                // Leaves: matte at night (no highlights, no reflections), as URP Lit's leaves were.
                material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
                material.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
            }
            else
            {
                material.renderQueue = -1;
                material.SetOverrideTag("RenderType", "");
            }
            material.doubleSidedGI = twoSided;
            material.enableInstancing = true;
            if (create) AssetDatabase.CreateAsset(material, path);
            else EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>A drawn texture: <paramref name="draw"/> paints on transparent pixels; cut-outs get their edges dilated.</summary>
        private static Texture2D Store(string name, int width, int height, System.Action<Color32[], int, int> draw, bool colour, bool repeat, bool cutout, bool normal = false)
        {
            var pixels = new Color32[width * height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(128, 128, 128, (byte)(cutout ? 0 : 255));
            draw(pixels, width, height);
            if (cutout) Dilate(pixels, width, height, 6);
            return Store(name, width, height, pixels, colour, repeat, cutout, normal);
        }

        // ------------------------------------------------------------------ drawn textures

        /// <summary>
        /// One internode of a culm, tinted by the material: fine lengthwise fibres and streaks, small pale lenticels, the
        /// dark ridged node with the pale scar of the fallen sheath just above it, the white waxy bloom below the next node,
        /// rain runs from the node down, dirt low and lichen in small patches. Alpha is the sheen: the wet internode smooth,
        /// node, bloom, lichen and dirt rough.
        /// </summary>
        private static void DrawCulm(Color32[] pixels, int width, int height)
        {
            var random = new System.Random(3);
            var fibre = new float[width];
            var rain = new float[width];
            for (int x = 0; x < width; x++)
            {
                fibre[x] = 0.88f + (float)random.NextDouble() * 0.12f;
                rain[x] = random.Next(10) == 0 ? 0.55f + (float)random.NextDouble() * 0.3f : 1f;
            }
            for (int x = 1; x < width - 1; x++) fibre[x] = (fibre[x - 1] + fibre[x] * 2f + fibre[x + 1]) * 0.25f;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    float v = y / (float)height, u = x / (float)width;
                    float tone = fibre[x] * (0.92f + 0.08f * ForestNoise.Perlin(u * 40f, v * 3f)) * Mathf.Lerp(1.03f, 0.92f, v);
                    var c = new Color(0.86f, 0.9f, 0.7f) * tone;
                    float sheen = 0.78f + 0.22f * ForestNoise.Perlin(u * 24f + 9f, v * 6f);
                    if (rain[x] < 1f) { c *= Mathf.Lerp(rain[x], 1f, Mathf.Pow(v, 0.7f)); sheen *= 1.1f; }
                    // Lenticels: tiny pale dashes along the grain.
                    if (ForestNoise.Perlin(u * 90f, v * 14f) > 0.83f) c = Color.Lerp(c, new Color(0.93f, 0.92f, 0.8f), 0.35f);
                    float node = v < 0.022f ? 1f : v > 0.99f ? 1f : 0f;
                    if (node > 0f) { c *= 0.45f; sheen = 0.25f; }
                    else if (v < 0.034f) { c = Color.Lerp(c * 0.7f, new Color(0.55f, 0.5f, 0.36f), 0.4f); sheen = 0.3f; }         // the ridge's shoulder
                    else if (v < 0.07f) { c = Color.Lerp(c, new Color(0.9f, 0.87f, 0.72f), 0.45f * (1f - (v - 0.034f) / 0.036f)); sheen = 0.4f; } // sheath scar
                    else if (v > 0.9f) { c = Color.Lerp(c, new Color(0.9f, 0.93f, 0.88f), 0.32f * (v - 0.9f) / 0.09f); sheen *= 0.55f; }        // waxy bloom
                    float dirt = ForestNoise.Perlin(u * 6f + 31f, v * 9f) * (1.2f - v);
                    if (dirt > 0.7f) { c *= 0.8f; sheen *= 0.5f; }
                    if (ForestNoise.Perlin(u * 18f + 7f, v * 18f) > 0.8f) { c = Color.Lerp(c * 0.78f, new Color(0.62f, 0.66f, 0.55f), 0.32f); sheen = 0.15f; } // lichen
                    c.a = Mathf.Clamp01(sheen);
                    pixels[y * width + x] = c;
                }
        }

        /// <summary>The culm's relief as a tangent-space normal map: the ridge at each node, the sheath scar above it, fine grooves along the culm.</summary>
        private static void DrawCulmNormal(Color32[] pixels, int width, int height)
        {
            float H(int x, int y)
            {
                x = (x % width + width) % width;
                y = (y % height + height) % height;
                float v = y / (float)height;
                float ridge = v < 0.034f ? Mathf.Sin(Mathf.PI * v / 0.034f) : v > 0.988f ? 0.5f * Mathf.Sin(Mathf.PI * (v - 0.988f) / 0.012f) : 0f;
                float scar = v > 0.04f && v < 0.052f ? 0.25f * Mathf.Sin(Mathf.PI * (v - 0.04f) / 0.012f) : 0f;
                return ridge * 2.4f + scar + ForestNoise.Perlin(x * 0.55f, y * 0.012f) * 0.3f;
            }
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    float dx = H(x + 1, y) - H(x - 1, y), dy = H(x, y + 1) - H(x, y - 1);
                    var n = new Vector3(-dx, -dy, 2f).normalized;
                    pixels[y * width + x] = new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f);
                }
        }

        /// <summary>
        /// Four leaf sprays in the quarters of the atlas, each a twig from the left edge (where it joins its branch) with
        /// long narrow bamboo leaves along it, alternate and drooping, crowded into a fan at its end: dense and fresh,
        /// sparse and hanging, mixed light and dark, and one older with browned tips.
        /// </summary>
        private static void DrawSprays(Color32[] pixels, int width, int height, bool dry)
        {
            for (int variant = 0; variant < 4; variant++)
            {
                var random = new System.Random((dry ? 120 : 110) + variant);
                float ox = (variant & 1) * 0.5f, oy = ((variant >> 1) & 1) * 0.5f;
                float R() => (float)random.NextDouble();
                int leaves = variant == 1 ? 7 : variant == 0 ? 12 : 10;
                float sag = variant == 1 ? 0.06f : 0.025f;
                // The twig, sagging a little.
                Vector2 Twig(float t) => new Vector2(ox + 0.5f * (0.03f + 0.84f * t), oy + 0.5f * (0.5f - sag * t * t));
                for (int k = 0; k < 24; k++)
                {
                    Vector2 a = Twig(k / 24f), b = Twig((k + 1) / 24f);
                    Line(pixels, width, height, a.x, a.y, b.x, b.y, 0.0022f * (1.4f - k / 24f), dry ? new Color(0.45f, 0.36f, 0.22f) : new Color(0.4f, 0.4f, 0.24f));
                }
                for (int i = 0; i < leaves; i++)
                {
                    // Spaced along the twig, closer and more at its end; alternate sides, leaning forward, drooping.
                    float t = 0.15f + 0.8f * Mathf.Pow((i + R() * 0.6f) / leaves, 0.75f);
                    Vector2 at = Twig(t);
                    float side = i % 2 == 0 ? 1f : -1f;
                    float angle = side * (18f + R() * 32f + t * 10f) * Mathf.Deg2Rad - (variant == 1 ? 0.25f : 0.1f) * (side > 0 ? 0f : 1f);
                    float length = (0.17f + R() * 0.09f) * (1.1f - 0.25f * Mathf.Abs(t - 0.6f)) * 0.5f * (variant == 1 ? 1.15f : 1f);
                    Color c = dry ? Color.Lerp(new Color(0.75f, 0.6f, 0.35f), new Color(0.5f, 0.38f, 0.22f), R())
                                  : Color.Lerp(new Color(0.52f, 0.72f, 0.33f), new Color(0.28f, 0.44f, 0.18f), variant == 2 ? R() : R() * 0.7f);
                    Color tip = variant == 3 && !dry && R() < 0.6f ? new Color(0.62f, 0.52f, 0.3f) : c;
                    Leaf(pixels, width, height, at.x, at.y, angle, length, length * 0.13f, c, tip);
                }
            }
        }

        /// <summary>A clump of the canopy: twigs from all over the card, each with a fan of drooping leaves, light and dark.</summary>
        private static void DrawCanopy(Color32[] pixels, int width, int height)
        {
            var random = new System.Random(17);
            for (int t = 0; t < 16; t++)
            {
                float u = 0.18f + 0.64f * (float)random.NextDouble(), v = 0.18f + 0.64f * (float)random.NextDouble();
                float heading = (float)random.NextDouble() * Mathf.PI * 2f;
                for (int i = 0; i < 9; i++)
                {
                    float angle = heading + ((float)random.NextDouble() - 0.5f) * 2.2f;
                    Color c = Color.Lerp(new Color(0.48f, 0.66f, 0.32f), new Color(0.22f, 0.36f, 0.15f), (float)random.NextDouble());
                    float length = 0.14f + (float)random.NextDouble() * 0.1f;
                    Leaf(pixels, width, height, u, v, angle, length, length * 0.16f, c, c);
                }
            }
        }

        /// <summary>A bush: broad leaves fanning up and out from the foot, the ones behind darker.</summary>
        private static void DrawShrub(Color32[] pixels, int width, int height)
        {
            var random = new System.Random(23);
            for (int i = 0; i < 70; i++)
            {
                float t = i / 70f;
                float angle = (-70f + (float)random.NextDouble() * 140f + 90f) * Mathf.Deg2Rad;
                float reach = 0.2f + (float)random.NextDouble() * 0.55f;
                float u = 0.5f + Mathf.Cos(angle) * reach * 0.6f, v = 0.05f + Mathf.Sin(angle) * reach;
                Color c = Color.Lerp(new Color(0.22f, 0.32f, 0.15f), new Color(0.48f, 0.62f, 0.3f), t * 0.6f + (float)random.NextDouble() * 0.4f);
                float length = 0.12f + (float)random.NextDouble() * 0.07f;
                Leaf(pixels, width, height, u, v, angle + ((float)random.NextDouble() - 0.5f) * 0.8f, length, length * 0.42f, c, c);
            }
        }

        /// <summary>
        /// Ferns for the middle distance: fronds arching up and out from the bottom middle, each a midrib with narrow
        /// leaflets along it, shorter towards the tip (a fern, not a palm's fan).
        /// </summary>
        private static void DrawFern(Color32[] pixels, int width, int height)
        {
            var random = new System.Random(21);
            for (int f = 0; f < 9; f++)
            {
                float lean = (-62f + f * 15.5f + (float)random.NextDouble() * 8f) * Mathf.Deg2Rad;
                float length = 0.5f + (float)random.NextDouble() * 0.38f;
                Color c = Color.Lerp(new Color(0.48f, 0.66f, 0.3f), new Color(0.28f, 0.43f, 0.19f), (float)random.NextDouble());
                Vector2 previous = new Vector2(0.5f, 0.03f);
                for (int k = 1; k <= 30; k++)
                {
                    float t = k / 30f;
                    float bend = lean * (1f + t * 0.7f);
                    var p = new Vector2(0.5f + Mathf.Sin(bend) * length * t, 0.03f + Mathf.Cos(bend) * length * t * (1f - 0.25f * t));
                    Line(pixels, width, height, previous.x, previous.y, p.x, p.y, 0.002f, c * 0.75f);
                    previous = p;
                    if (k < 4) continue;
                    float leaflet = 0.075f * Mathf.Sin(Mathf.PI * Mathf.Min(1f, t * 1.1f)) * (1f - t * 0.4f);
                    Leaf(pixels, width, height, p.x, p.y, bend + Mathf.PI * 0.42f, leaflet, leaflet * 0.24f, c, c);
                    Leaf(pixels, width, height, p.x, p.y, bend - Mathf.PI * 0.42f, leaflet, leaflet * 0.24f, c, c);
                }
            }
        }

        /// <summary>
        /// The tangle at the foot of a bamboo hedge (repeats across): thin culms and their dark, thorny side branches crossing
        /// every way, dense low down and thinning upward, a few leaves, a little light between.
        /// </summary>
        private static void DrawThicket(Color32[] pixels, int width, int height)
        {
            var random = new System.Random(71);
            float R() => (float)random.NextDouble();
            // Thin culm bases standing up through it.
            for (int i = 0; i < 9; i++)
            {
                float u = R(), lean = (R() - 0.5f) * 0.08f;
                Line(pixels, width, height, u, 0f, u + lean, 1f, 0.006f + R() * 0.006f, new Color(0.5f, 0.52f, 0.34f) * (0.7f + R() * 0.3f));
            }
            // Branches crossing, each with short thorns; more of them low.
            for (int i = 0; i < 140; i++)
            {
                float v = Mathf.Pow(R(), 1.6f) * 0.95f, u = R();
                float angle = (R() - 0.5f) * Mathf.PI * 1.2f + (R() < 0.5f ? 0f : Mathf.PI);
                float length = 0.08f + R() * 0.2f;
                float u1 = u + Mathf.Cos(angle) * length, v1 = v + Mathf.Sin(angle) * length * 0.7f;
                var c = new Color(0.32f, 0.3f, 0.2f) * (0.7f + R() * 0.5f);
                // Wrap across the card so it repeats without a seam.
                for (float shift = -1f; shift <= 1f; shift += 1f) Line(pixels, width, height, u + shift, v, u1 + shift, v1, 0.0025f, c);
                for (int k = 1; k < 4; k++)
                {
                    float t = k / 4f, tu = Mathf.Lerp(u, u1, t), tv = Mathf.Lerp(v, v1, t);
                    float thorn = angle + (k % 2 == 0 ? 1f : -1f) * 0.9f;
                    for (float shift = -1f; shift <= 1f; shift += 1f)
                        Line(pixels, width, height, tu + shift, tv, tu + shift + Mathf.Cos(thorn) * 0.018f, tv + Mathf.Sin(thorn) * 0.018f, 0.0015f, c * 0.9f);
                }
            }
            for (int i = 0; i < 45; i++)
            {
                float u = R(), v = 0.15f + R() * 0.8f;
                Color c = Color.Lerp(new Color(0.45f, 0.6f, 0.3f), new Color(0.3f, 0.4f, 0.2f), R());
                float length = 0.05f + R() * 0.04f;
                Leaf(pixels, width, height, u, v, R() * Mathf.PI * 2f, length, length * 0.15f, c, c);
            }
        }

        /// <summary>Fallen bamboo leaves, brown, straw and many dark ones (the black of rotted leaves on wet earth).</summary>
        private static void DrawLitter(Color32[] pixels, int width, int height)
        {
            var random = new System.Random(41);
            for (int i = 0; i < 16; i++)
            {
                Color c = random.Next(3) == 0 ? new Color(0.24f, 0.19f, 0.13f) : Color.Lerp(new Color(0.72f, 0.58f, 0.34f), new Color(0.5f, 0.37f, 0.22f), (float)random.NextDouble());
                float length = 0.26f + 0.14f * (float)random.NextDouble();
                Leaf(pixels, width, height, 0.15f + 0.7f * (float)random.NextDouble(), 0.15f + 0.7f * (float)random.NextDouble(),
                    (float)random.NextDouble() * Mathf.PI * 2f, length, length * 0.16f, c, c);
            }
        }

        /// <summary>
        /// The far forest as one picture: culms of every thickness, some leaning, a mass of leaves from the middle up and
        /// wisps lower down; the bottom is solid with undergrowth so it stands on the ground.
        /// </summary>
        private static void DrawSkyline(Color32[] pixels, int width, int height)
        {
            var random = new System.Random(29);
            var dark = new Color(0.32f, 0.36f, 0.28f);
            for (int i = 0; i < 24; i++)
            {
                float u = (float)random.NextDouble(), lean = ((float)random.NextDouble() - 0.5f) * 0.12f;
                float top = 0.55f + (float)random.NextDouble() * 0.43f, thick = 0.003f + (float)random.NextDouble() * 0.007f;
                Line(pixels, width, height, u, 0f, u + lean, top, thick, dark * (0.8f + (float)random.NextDouble() * 0.4f));
            }
            for (int i = 0; i < 520; i++)
            {
                float v = 0.42f + Mathf.Sqrt((float)random.NextDouble()) * 0.56f;
                float u = (float)random.NextDouble();
                Color c = Color.Lerp(new Color(0.3f, 0.38f, 0.22f), new Color(0.5f, 0.6f, 0.34f), (float)random.NextDouble());
                float length = 0.04f + (float)random.NextDouble() * 0.045f;
                Leaf(pixels, width, height, u, v, (float)random.NextDouble() * Mathf.PI * 2f, length, length * 0.3f, c, c);
            }
            for (int i = 0; i < 120; i++)
            {
                float u = (float)random.NextDouble(), v = (float)random.NextDouble() * 0.12f;
                float length = 0.035f + (float)random.NextDouble() * 0.045f;
                Leaf(pixels, width, height, u, v, Mathf.PI * (0.25f + 0.5f * (float)random.NextDouble()), length, length * 0.5f, new Color(0.26f, 0.32f, 0.18f), new Color(0.26f, 0.32f, 0.18f));
            }
        }

        /// <summary>
        /// Leaf shadow for the moonlight (a light cookie, tiling): mostly shade, light in broken patches and flecks, as the
        /// canopy lets it through. Made of whole waves across the tile, so it repeats without a seam.
        /// </summary>
        private static void DrawCookie(Color32[] pixels, int width, int height)
        {
            var random = new System.Random(61);
            var waves = new List<(int a, int b, float phase, float weight)>();
            for (int i = 0; i < 14; i++)
                waves.Add((random.Next(-7, 8), random.Next(-7, 8), (float)random.NextDouble() * Mathf.PI * 2f, i < 5 ? 1f : 0.45f));
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    float u = x / (float)width, v = y / (float)height, n = 0f, total = 0f;
                    foreach (var (a, b, phase, weight) in waves) { n += Mathf.Sin(2f * Mathf.PI * (a * u + b * v) + phase) * weight; total += weight; }
                    n = n / total * 0.5f + 0.5f;
                    float lit = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.52f, 0.66f, n));
                    float value = 0.2f + 0.8f * lit;
                    pixels[y * width + x] = new Color(value, value, value, 1f);
                }
        }

        /// <summary>
        /// A narrow pointed leaf from (u, v) along <paramref name="angle"/> (radians from +u): widest a third of the way out,
        /// a fine pale midrib, the far half shading towards <paramref name="tip"/>; edges anti-aliased into the alpha.
        /// </summary>
        private static void Leaf(Color32[] pixels, int width, int height, float u, float v, float angle, float length, float leafWidth, Color color, Color tip)
        {
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)), across = new Vector2(-dir.y, dir.x);
            Vector2 start = new Vector2(u * width, v * height);
            float l = length * width, w = Mathf.Max(1.2f, leafWidth * width);
            int x0 = Mathf.FloorToInt(Mathf.Min(start.x, start.x + dir.x * l) - w), x1 = Mathf.CeilToInt(Mathf.Max(start.x, start.x + dir.x * l) + w);
            int y0 = Mathf.FloorToInt(Mathf.Min(start.y, start.y + dir.y * l) - w), y1 = Mathf.CeilToInt(Mathf.Max(start.y, start.y + dir.y * l) + w);
            for (int y = Mathf.Max(0, y0); y <= Mathf.Min(height - 1, y1); y++)
                for (int x = Mathf.Max(0, x0); x <= Mathf.Min(width - 1, x1); x++)
                {
                    Vector2 d = new Vector2(x + 0.5f, y + 0.5f) - start;
                    float along = Vector2.Dot(d, dir) / l, side = Mathf.Abs(Vector2.Dot(d, across));
                    if (along < 0f || along > 1f) continue;
                    // Narrow at the stalk, widest at a third, a long tapering point.
                    float edge = w * 0.5f * Mathf.Pow(Mathf.Sin(Mathf.PI * Mathf.Pow(along, 0.6f)), 0.9f);
                    float cover = Mathf.Clamp01(edge - side + 0.5f);
                    if (cover <= 0f) continue;
                    Color c = Color.Lerp(color, tip, Mathf.Clamp01((along - 0.55f) / 0.45f));
                    c *= 0.88f + 0.12f * (side / Mathf.Max(0.001f, edge));
                    if (side < 0.6f && along < 0.9f) c = Color.Lerp(c, c * 1.25f, 0.5f); // the pale midrib
                    int i = y * width + x;
                    float a = Mathf.Max(pixels[i].a / 255f, cover);
                    var old = (Color)pixels[i];
                    c = pixels[i].a > 0 ? Color.Lerp(old, c, cover) : c;
                    c.a = a;
                    pixels[i] = c;
                }
        }

        private static void Line(Color32[] pixels, int width, int height, float u0, float v0, float u1, float v1, float thickness, Color color)
        {
            int steps = Mathf.CeilToInt(Vector2.Distance(new Vector2(u0 * width, v0 * height), new Vector2(u1 * width, v1 * height)));
            int r = Mathf.Max(1, Mathf.RoundToInt(thickness * width));
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)Mathf.Max(1, steps);
                int cx = Mathf.RoundToInt(Mathf.Lerp(u0, u1, t) * width), cy = Mathf.RoundToInt(Mathf.Lerp(v0, v1, t) * height);
                for (int y = cy - r; y <= cy + r; y++)
                    for (int x = cx - r; x <= cx + r; x++)
                        if (x >= 0 && y >= 0 && x < width && y < height) { var c = color; c.a = 1f; pixels[y * width + x] = c; }
            }
        }
    }
}
