using System.IO;
using UnityEditor;
using UnityEngine;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Small textures for Tú Bà's house that no CC0 set in the project covers, generated (not scanned): a silk and a
    /// cotton weave (albedo variation + normal) for curtains, cushions and clothes, and the lantern silk whose bamboo
    /// ribs and hoops show as darker lines when the candle shines through. Deterministic, regenerated on every rebuild.
    /// </summary>
    public static class LauTuBaTextures
    {
        public const string Folder = "Assets/_Game/Art/Environment/LauTuBa/Textures";

        /// <summary>Weave of <paramref name="silk"/> (fine, sheen streaks) or cotton (coarse, uneven threads).</summary>
        public static (Texture2D albedo, Texture2D normal) Weave(bool silk)
        {
            string name = silk ? "LTB_SilkWeave" : "LTB_CottonWeave";
            const int size = 256;
            int period = silk ? 4 : 6;
            var random = new System.Random(silk ? 11 : 23);
            // Per-thread thickness: slubs in the weft (rows) and a few heavier warp threads (columns).
            var weft = new float[size];
            var warp = new float[size];
            for (int i = 0; i < size; i++) { weft[i] = (float)random.NextDouble(); warp[i] = (float)random.NextDouble(); }
            var height = new float[size * size];
            var albedo = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float wx = Mathf.Sin(Mathf.PI * 2f * x / period), wy = Mathf.Sin(Mathf.PI * 2f * y / period);
                    bool over = ((x / period) + (y / period)) % 2 == 0;
                    float thread = over ? 0.5f + 0.5f * Mathf.Abs(wx) : 0.5f + 0.5f * Mathf.Abs(wy);
                    float slub = weft[y / period] * (silk ? 0.35f : 0.6f) + warp[x / period] * (silk ? 0.1f : 0.3f);
                    height[y * size + x] = thread * 0.7f + slub * 0.3f;
                    // Silk: long soft sheen bands along the weft; cotton: blotchy uneven dye.
                    float band = silk ? 0.94f + 0.06f * Mathf.Sin(y * 0.11f + weft[(y / 16) % size] * 3f) : 0.9f + 0.1f * weft[(x / 24 + y / 24) % size];
                    float v = band * (0.92f + 0.08f * thread) * (1f - slub * (silk ? 0.05f : 0.1f));
                    albedo[y * size + x] = new Color(v, v, v, 1f);
                }
            return (Save(name + "_albedo", albedo, size, size, false), Save(name + "_normal", ToNormal(height, size, silk ? 1.5f : 2.5f), size, size, true));
        }

        /// <summary>
        /// Lantern silk, laid out on the lathe body's UVs (u around, v from the bottom cap to the top one): brightest at
        /// the belly, dimmer towards the caps, a thin dark line on each of the 12 ribs and on the two binding hoops.
        /// </summary>
        public static Texture2D LanternSilk()
        {
            const int width = 256, height = 128;
            var pixels = new Color[width * height];
            var random = new System.Random(5);
            var slub = new float[height];
            for (int i = 0; i < height; i++) slub[i] = (float)random.NextDouble();
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width, v = (float)y / height;
                    float belly = 0.45f + 0.55f * Mathf.Sin(Mathf.PI * v);
                    float rib = Mathf.Abs(Mathf.Repeat(u * 12f + 0.5f, 1f) - 0.5f); // 0 on a rib
                    float ribLine = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.02f, 0.07f, rib));
                    float lobe = 0.8f + 0.2f * Mathf.Sin(Mathf.PI * Mathf.Clamp01(rib * 2f)); // brighter mid-panel
                    float hoop = 1f;
                    foreach (float h in new[] { 0.27f, 0.73f }) hoop = Mathf.Min(hoop, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.006f, 0.02f, Mathf.Abs(v - h))));
                    float weave = 0.95f + 0.05f * slub[y];
                    float value = belly * lobe * weave * Mathf.Lerp(0.3f, 1f, ribLine) * Mathf.Lerp(0.45f, 1f, hoop);
                    pixels[y * width + x] = new Color(value, value, value, 1f);
                }
            return Save("LTB_LanternSilk", pixels, width, height, false, TextureWrapMode.Clamp);
        }


        /// <summary>
        /// Water standing on flagstones, mapped over a puddle mesh's UVs (0..1 across it): the alpha is deep in the middle
        /// and thins out over a wandering, soft rim (no paper-cut edge); a few dry stone tops break the surface near the
        /// rim. RGB stays white (the material colours it).
        /// </summary>
        public static Texture2D PuddleMask(int seed)
        {
            const int size = 256;
            var random = new System.Random(seed);
            var phase = new float[6];
            for (int i = 0; i < phase.Length; i++) phase[i] = (float)random.NextDouble() * 6.3f;
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float u = (x + 0.5f) / size * 2f - 1f, v = (y + 0.5f) / size * 2f - 1f;
                    float angle = Mathf.Atan2(v, u), r = Mathf.Sqrt(u * u + v * v);
                    float edge = 0.78f + 0.09f * Mathf.Sin(angle * 2f + phase[0]) + 0.05f * Mathf.Sin(angle * 5f + phase[1]) + 0.03f * Mathf.Sin(angle * 9f + phase[2]);
                    // Small-scale breakup: stone tops showing through where the water is thin.
                    float grain = Mathf.PerlinNoise(u * 4.1f + phase[3], v * 4.1f + phase[4]) - 0.5f;
                    float depth = Mathf.Clamp01((edge - r) / 0.32f + grain * 0.55f);
                    float a = Mathf.SmoothStep(0f, 1f, depth);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            return Save("LTB_PuddleMask_" + seed, pixels, size, size, false, TextureWrapMode.Clamp);
        }

        /// <summary>A gentle ripple normal for standing water in the rain (rings from drops, overlapping), tiling.</summary>
        public static Texture2D RippleNormal()
        {
            const int size = 256;
            var random = new System.Random(41);
            var height = new float[size * size];
            for (int drop = 0; drop < 22; drop++)
            {
                float cx = (float)random.NextDouble() * size, cy = (float)random.NextDouble() * size, radius = 6f + (float)random.NextDouble() * 18f;
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        float dx = Mathf.Repeat(x - cx + size * 0.5f, size) - size * 0.5f, dy = Mathf.Repeat(y - cy + size * 0.5f, size) - size * 0.5f;
                        float d = Mathf.Sqrt(dx * dx + dy * dy);
                        if (d > radius * 1.6f) continue;
                        height[y * size + x] += Mathf.Sin((d - radius) * 0.9f) * Mathf.Exp(-Mathf.Abs(d - radius) * 0.25f) * 0.5f;
                    }
            }
            return Save("LTB_RippleNormal", ToNormal(height, size, 0.6f), size, size, true);
        }

        /// <summary>
        /// Lamp-lit window paper, laid over the paper panel's UVs: fibres and slubs in the paper, the light strongest
        /// where the lamp stands behind it (<paramref name="hot"/>, 0..1 across) and falling off towards the frame.
        /// </summary>
        public static Texture2D LitPaper(int seed, Vector2 hot, float spread)
        {
            const int size = 128;
            var random = new System.Random(seed);
            var fibres = new float[size];
            for (int i = 0; i < size; i++) fibres[i] = (float)random.NextDouble();
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float u = (x + 0.5f) / size, v = (y + 0.5f) / size;
                    float d = Vector2.Distance(new Vector2(u, v), hot) / spread;
                    float glow = 0.42f + 0.58f * Mathf.Exp(-d * d);
                    float frame = Mathf.SmoothStep(0f, 1f, Mathf.Min(Mathf.Min(u, 1f - u), Mathf.Min(v, 1f - v)) / 0.12f);
                    float fibre = 0.9f + 0.1f * fibres[(x * 7 + y / 3) % size] * Mathf.PerlinNoise(u * 18f + seed, v * 3f);
                    float value = glow * Mathf.Lerp(0.72f, 1f, frame) * fibre;
                    pixels[y * size + x] = new Color(value, value * 0.96f, value * 0.9f, 1f);
                }
            return Save("LTB_LitPaper_" + seed, pixels, size, size, false, TextureWrapMode.Clamp);
        }

        /// <summary>A strip that is wettest at one edge (v = 0) and dries out across it, with a ragged drying line.</summary>
        public static Texture2D DampEdge()
        {
            const int width = 256, height = 64;
            var pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width, v = (float)y / height;
                    float line = 0.55f + 0.25f * (Mathf.PerlinNoise(u * 9f, 0.3f) - 0.5f) * 2f;
                    float a = Mathf.SmoothStep(1f, 0f, Mathf.Clamp01(v / Mathf.Max(0.1f, line)));
                    pixels[y * width + x] = new Color(1f, 1f, 1f, a * 0.85f);
                }
            return Save("LTB_DampEdge", pixels, width, height, false);
        }

        private static Color[] ToNormal(float[] height, int size, float strength)
        {
            var normals = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = height[y * size + (x + 1) % size] - height[y * size + (x - 1 + size) % size];
                    float dy = height[((y + 1) % size) * size + x] - height[((y - 1 + size) % size) * size + x];
                    var n = new Vector3(-dx * strength, -dy * strength, 1f).normalized;
                    normals[y * size + x] = new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f);
                }
            return normals;
        }

        /// <summary>
        /// URP Lit's layout of a Poly Haven ARM map (AO/Roughness/Metal in R/G/B): metallic in R, AO in G (read by
        /// _OcclusionMap), smoothness = 1 − roughness in A. Same data, repacked; made once per map.
        /// </summary>
        public static Texture2D MetallicSmoothness(Texture2D arm)
        {
            string source = AssetDatabase.GetAssetPath(arm);
            string name = Path.GetFileNameWithoutExtension(source) + "_urp";
            string path = Folder + "/" + name + ".png";
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing != null && File.GetLastWriteTimeUtc(path) >= File.GetLastWriteTimeUtc(source)) return existing;
            var read = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            read.LoadImage(File.ReadAllBytes(source));
            var pixels = read.GetPixels();
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(pixels[i].b, pixels[i].r, 0f, 1f - pixels[i].g);
            int width = read.width, height = read.height;
            Object.DestroyImmediate(read);
            return Save(name, pixels, width, height, false, TextureWrapMode.Repeat, true);
        }

        /// <summary>
        /// Old bronze for Tú Bà's keys (<see cref="LauTuBaKeyRing"/>), laid out by wear rather than by place: the lower half
        /// (v &lt; 0.45) is metal rubbed bright and fairly smooth (edges, where keys rub the ring), the upper half (v &gt; 0.55)
        /// dark patina with oxide blotches and a few green specks, rougher and a little less metallic; a soft band between.
        /// Returns the colour map and URP Lit's metallic (R) / smoothness (A) map. Clamped, not tiled.
        /// </summary>
        public static (Texture2D albedo, Texture2D metal) KeyBronze()
        {
            const int size = 128;
            var random = new System.Random(57);
            float ox = (float)random.NextDouble() * 50f, oy = (float)random.NextDouble() * 50f;
            float Noise(float x, float y)
            {
                float sum = 0f, amp = 0.5f, freq = 1f;
                for (int o = 0; o < 4; o++) { sum += amp * Mathf.PerlinNoise(ox + x * freq, oy + y * freq); amp *= 0.5f; freq *= 2.1f; }
                return sum / 0.9375f;
            }
            var albedo = new Color[size * size];
            var metal = new Color[size * size];
            var bright = new Color(0.78f, 0.6f, 0.34f);
            var bronze = new Color(0.46f, 0.33f, 0.18f);
            var oxide = new Color(0.19f, 0.15f, 0.1f);
            var verdigris = new Color(0.29f, 0.35f, 0.27f);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)size, v = y / (float)size;
                    float worn = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.4f, 0.6f, v));
                    float n = Noise(u * 6f, v * 6f), fine = Noise(u * 23f + 7f, v * 23f + 3f);
                    float blotch = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.45f, 0.7f, n));
                    bool speck = Noise(u * 31f + 13f, v * 31f + 5f) > 0.74f;
                    Color aged = Color.Lerp(bronze, oxide, blotch * 0.8f);
                    if (speck) aged = Color.Lerp(aged, verdigris, 0.6f);
                    Color polished = bright * (0.9f + 0.18f * fine);
                    Color c = Color.Lerp(aged * (0.9f + 0.2f * fine), polished, worn);
                    c.a = 1f;
                    albedo[y * size + x] = c;
                    float smooth = Mathf.Lerp(0.26f + 0.2f * (1f - blotch) + 0.06f * fine, 0.62f + 0.14f * fine, worn);
                    float metallic = Mathf.Lerp(speck ? 0.55f : 0.9f - 0.25f * blotch, 1f, worn);
                    metal[y * size + x] = new Color(metallic, 0f, 0f, smooth);
                }
            return (Save("LTB_KeyBronze_albedo", albedo, size, size, false, TextureWrapMode.Clamp),
                Save("LTB_KeyBronze_metal", metal, size, size, false, TextureWrapMode.Clamp, true));
        }

        private static Texture2D Save(string name, Color[] pixels, int width, int height, bool normal, TextureWrapMode wrap = TextureWrapMode.Repeat, bool linear = false)
        {
            Directory.CreateDirectory(Folder);
            string path = Folder + "/" + name + ".png";
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false, normal || linear);
            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            if (AssetImporter.GetAtPath(path) is TextureImporter importer)
            {
                importer.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
                importer.sRGBTexture = !normal && !linear;
                importer.wrapMode = wrap;
                importer.anisoLevel = 4;
                importer.mipmapEnabled = true;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}
