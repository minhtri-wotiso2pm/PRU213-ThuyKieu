using System.Collections.Generic;
using ThuyKieu.Environment;
using ThuyKieu.Gameplay.Stealth;
using UnityEngine;
using UnityEngine.Rendering;
using static ThuyKieu.Core.Editor.LauTuBaKit;
using static ThuyKieu.Core.Editor.LauTuBaMeshKit;
using static ThuyKieu.Core.Editor.StorySceneKit;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Furniture, ceramics, cloth and lanterns for Tú Bà's house, built at the scale of real people: planed wood with
    /// softened edges, turned legs and pots, cushions that sag, curtains that hang in folds. The project's own
    /// high-detail props (canopy bed, cabinet, tea table set, jars, oil lamp, altar, palms) are used as lighter copies
    /// where they fit. Furniture that a body would bump into gets one box collider on the Environment layer (it is also
    /// cover from eyes); small things have none, so nothing in the house is an invisible wall.
    /// </summary>
    public static partial class LauTuBaHouse
    {
        private const string PropBed = "Assets/_Game/Prefabs/Props/Bed.prefab";
        private const string PropCabinet = "Assets/_Game/Prefabs/Props/Cabinet.prefab";
        private const string PropTableSet = "Assets/_Game/Prefabs/Props/TableSet.prefab";
        private const string PropJars = "Assets/_Game/Prefabs/Props/CeramicJar.prefab";
        private const string PropOilLamp = "Assets/_Game/Prefabs/Props/OilLamp.prefab";
        private const string PropAltar = "Assets/_Game/Prefabs/Props/Altar.prefab";
        private const string PropAreca = "Assets/_Game/Prefabs/Vegetation/ArecaPalm.prefab";
        private const string PropBanana = "Assets/_Game/Prefabs/Vegetation/BananaTree.prefab";

        private static Material _silkRed, _silkDeep, _silkGold, _brass, _bronze, _celadon, _porcelain, _brownGlaze, _terracotta, _ironPot,
            _cotton, _reed, _paperWarm, _ink, _ember, _soot, _woodWorn, _woodDusty;

        /// <summary>Materials of the furnishings (cloth, ceramics, metal): plain colours with distinct sheen, no textures.</summary>
        private static void DecorMaterials()
        {
            _silkRed = Mat("LTB_SilkRed", new Color(0.4f, 0.07f, 0.06f), 0.42f);
            _silkDeep = Mat("LTB_SilkDeep", new Color(0.24f, 0.05f, 0.05f), 0.38f);
            _silkGold = Mat("LTB_SilkGold", new Color(0.62f, 0.44f, 0.18f), 0.5f);
            _brass = MetalMat("LTB_BrassMetal", new Color(0.72f, 0.54f, 0.26f), 0.8f, 0.58f);
            _bronze = MetalMat("LTB_BronzeMetal", new Color(0.36f, 0.27f, 0.17f), 0.75f, 0.4f);
            _celadon = Mat("LTB_Celadon", new Color(0.46f, 0.58f, 0.5f), 0.82f);
            _porcelain = Mat("LTB_Porcelain", new Color(0.84f, 0.85f, 0.86f), 0.86f);
            _brownGlaze = Mat("LTB_BrownGlaze", new Color(0.26f, 0.14f, 0.07f), 0.72f);
            _terracotta = StoryArtKit.Surface("LTB_Terracotta", "brown_mud_02", new Color(0.66f, 0.38f, 0.26f), 0.6f, 0.12f, 0.5f, 0.3f, 0f, Folder);
            _ironPot = MetalMat("LTB_IronPot", new Color(0.09f, 0.085f, 0.08f), 0.6f, 0.32f);
            _cotton = Mat("LTB_Cotton", new Color(0.6f, 0.55f, 0.46f), 0.08f);
            _reed = StoryArtKit.Surface("LTB_ReedMat", "planks_brown_10", new Color(0.66f, 0.55f, 0.36f), 0.35f, 0.12f, 0.6f, 0.4f, 0f, Folder);
            _paperWarm = Mat("LTB_PaperWarm", new Color(0.84f, 0.78f, 0.64f), 0.06f);
            _ink = Mat("LTB_Ink", new Color(0.05f, 0.045f, 0.04f), 0.2f);
            _ember = EmissiveMat("LTB_Embers", new Color(1f, 0.32f, 0.08f), 2.4f);
            _soot = StoryArtKit.Surface("LTB_Soot", "brown_mud_02", new Color(0.2f, 0.17f, 0.15f), 1.2f, 0.05f, 0.8f, 0.6f, 0f, Folder);
            _woodWorn = StoryArtKit.Surface("LTB_WoodWorn", "planks_brown_10", new Color(0.44f, 0.33f, 0.25f), 0.9f, 0.12f, 0.9f, 0.35f, 0f, Folder);
            _woodDusty = Dusty(StoryArtKit.Surface("LTB_WoodDusty", "planks_brown_10", new Color(0.44f, 0.33f, 0.25f), 0.9f, 0.12f, 0.9f, 0.35f, 0f, Folder), 0.75f);
        }

        // ------------------------------------------------------------------ placing pieces

        /// <summary>A mesh placed in the house, without collider unless asked for.</summary>
        private static GameObject Piece(string name, Mesh mesh, Material material, Transform parent, Vector3 position, Quaternion rotation, Vector3? scale = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(position, rotation);
            if (scale.HasValue) go.transform.localScale = scale.Value;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
            go.layer = 0;
            go.isStatic = true;
            return go;
        }

        /// <summary>A planed timber part with softened edges, centred at <paramref name="centre"/> in the frame of <paramref name="frame"/>.</summary>
        private static GameObject Wood(string name, Transform frame, Vector3 centre, Vector3 size, Material material, float edge = 0.008f)
        {
            var go = Piece(name, RoundedBox(size, edge), material, frame, frame.TransformPoint(centre), frame.rotation);
            return go;
        }

        /// <summary>One box collider for a piece of furniture (Environment layer: bodies stop, eyes are blocked).</summary>
        private static void Solid(Transform frame, Vector3 centre, Vector3 size)
        {
            var go = new GameObject("Collider");
            go.transform.SetParent(frame, false);
            go.transform.localPosition = centre;
            go.layer = EnvironmentLayer;
            go.AddComponent<BoxCollider>().size = size;
            go.isStatic = true;
        }

        private static Transform Frame(string name, Transform parent, Vector3 floor, float yaw)
        {
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.SetPositionAndRotation(floor, Quaternion.Euler(0f, yaw, 0f));
            return root;
        }

        /// <summary>A lighter copy of one of the project's props, standing at <paramref name="floor"/>; optional collider from its bounds.</summary>
        private static GameObject LiteProp(string prefabPath, int triangles, Vector3 floor, float yaw, float scale, Transform parent, bool solid, string name = null)
        {
            var mesh = Lite(prefabPath, triangles);
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var source = prefab.GetComponentInChildren<MeshFilter>().transform;
            var go = new GameObject(name ?? System.IO.Path.GetFileNameWithoutExtension(prefabPath));
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(floor, Quaternion.Euler(0f, yaw, 0f));
            var visual = new GameObject("Mesh");
            visual.transform.SetParent(go.transform, false);
            // Keep the prefab's own placement of its mesh (pivot, rotation and import scale), then our scale on top.
            visual.transform.localPosition = prefab.transform.InverseTransformPoint(source.position) * scale;
            visual.transform.localRotation = Quaternion.Inverse(prefab.transform.rotation) * source.rotation;
            visual.transform.localScale = Vector3.Scale(source.lossyScale, Vector3.one * scale);
            visual.AddComponent<MeshFilter>().sharedMesh = mesh;
            visual.AddComponent<MeshRenderer>().sharedMaterials = MaterialsOf(prefabPath);
            visual.isStatic = true;
            if (solid)
            {
                var bounds = visual.GetComponent<MeshRenderer>().bounds;
                var box = new GameObject("Collider");
                box.transform.SetParent(go.transform, false);
                box.transform.position = bounds.center;
                box.layer = EnvironmentLayer;
                var local = Quaternion.Inverse(go.transform.rotation) * bounds.size;
                box.AddComponent<BoxCollider>().size = new Vector3(Mathf.Abs(local.x), Mathf.Abs(local.y), Mathf.Abs(local.z)) * 0.96f;
                box.isStatic = true;
            }
            return go;
        }

        // ------------------------------------------------------------------ architecture

        /// <summary>
        /// A veranda or hall pillar: carved stone base, lacquered shaft, a turned capital (đấu) flaring under the beam with
        /// a thin square abacus, and where an eave beam runs along <paramref name="beamAlong"/>, two curved bracket arms
        /// reaching out along it to carry the beam (which may sit <paramref name="beamOffset"/> to one side of the pillar's axis:
        /// the arms drift across to meet it). Capsule collider for the shaft.
        /// </summary>
        private static void Column(string name, Vector3 foot, float radius, float height, Transform parent, Vector3 beamAlong = default, Vector3 beamOffset = default, bool yard = false)
        {
            var root = Frame(name, parent, foot, 0f);
            var stone = Lathe("ColumnBase", new[] { new Vector2(0.27f, 0f), new Vector2(0.27f, 0.05f), new Vector2(0.25f, 0.09f), new Vector2(0.2f, 0.15f), new Vector2(0.17f, 0.2f), new Vector2(0.16f, 0.22f), new Vector2(0f, 0.22f) }, 28, capBottom: false);
            Piece("Base", stone, yard ? PlinthYard : Plinth, root, foot, Quaternion.identity, Vector3.one * (radius / 0.13f));
            float shaftHeight = height - 0.22f - 0.16f;
            var shaft = Lathe("ColumnShaft", new[] { new Vector2(1f, 0f), new Vector2(0.97f, 0.5f), new Vector2(0.92f, 1f) }, 24, capBottom: false);
            Piece("Shaft", shaft, yard ? PillarWoodYard : PillarWood, root, foot + Vector3.up * 0.22f, Quaternion.identity, new Vector3(radius, shaftHeight, radius));
            Piece("Capital", CapitalMesh(radius, height, beamAlong, beamOffset), PillarWood, root, foot, Quaternion.identity);
            var collider = new GameObject("Collider");
            collider.transform.SetParent(root, false);
            collider.transform.position = foot + Vector3.up * height * 0.5f;
            collider.layer = EnvironmentLayer;
            var capsule = collider.AddComponent<CapsuleCollider>();
            capsule.radius = radius;
            capsule.height = height;
        }

        /// <summary>The capital, abacus and (along a beam) the two bracket arms of a pillar, from its foot, as joinery.</summary>
        private static Mesh CapitalMesh(float radius, float height, Vector3 beamAlong, Vector3 beamOffset)
        {
            string axis = beamAlong == Vector3.zero ? "0" : Mathf.Abs(beamAlong.x) > 0.5f ? "x" : "z";
            string key = $"Joinery_Capital_{radius:F2}_{height:F2}_{axis}_{beamOffset.x:F2}_{beamOffset.z:F2}_v2".Replace('.', 'p');
            if (LauTuBaMeshKit.Has(key, out var cached)) return cached;
            var j = new LauTuBaJoinery();
            float beamBottom = height - 0.24f, r = radius * 0.94f;
            j.Turned(Vector3.up * (beamBottom - 0.17f), new[] { new Vector2(r, 0f), new Vector2(r * 1.06f, 0.03f), new Vector2(r * 1.02f, 0.05f), new Vector2(r * 1.2f, 0.09f),
                new Vector2(r * 1.42f, 0.135f), new Vector2(r * 1.5f, 0.15f), new Vector2(r * 1.5f, 0.155f) }, 24);
            j.Add(RoundedBox(new Vector3(radius * 3.1f, 0.05f, radius * 3.1f), 0.012f), Matrix4x4.Translate(Vector3.up * (beamBottom - 0.025f)));
            if (axis != "0")
            {
                var d = axis == "x" ? Vector3.right : Vector3.forward;
                foreach (float s in new[] { -1f, 1f })
                {
                    // Out from the shaft, rising in a curve to meet the beam's underside, the tip curling down a little.
                    var path = LauTuBaJoinery.Smooth(new[] { d * s * (radius * 0.85f) + Vector3.up * (beamBottom - 0.4f), d * s * (radius + 0.1f) + Vector3.up * (beamBottom - 0.27f) + beamOffset * 0.5f,
                        d * s * (radius + 0.3f) + Vector3.up * (beamBottom - 0.08f) + beamOffset, d * s * (radius + 0.44f) + Vector3.up * (beamBottom - 0.005f) + beamOffset,
                        d * s * (radius + 0.5f) + Vector3.up * (beamBottom - 0.03f) + beamOffset }, 6);
                    j.Sweep(path, LauTuBaJoinery.EasedRect(0.08f, 0.11f, 0.016f, 2), Vector3.up, 0, t => Mathf.Lerp(1.15f, 0.75f, t));
                }
            }
            return LauTuBaMeshKit.Keep(key, j.Build(key));
        }

        private static Texture2D _lanternSilk;
        private static Texture2D LanternSilkMap => _lanternSilk != null ? _lanternSilk : _lanternSilk = LauTuBaTextures.LanternSilk();

        /// <summary>
        /// A silk lantern on a cord: lacquered caps, a ribbed body that glows only while its candle burns (LanternGlow),
        /// a red cord and tassel. The light is warm candlelight through red silk, soft, not a red flood.
        /// </summary>
        private static Light SilkLantern(string name, Vector3 position, Transform parent, float range, float intensity, float cord = 0.42f, float size = 1f, bool shadows = false)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.position = position + Vector3.up * cord;
            var frame = root.transform;
            var body = Lathe("LanternBody", new[]
            {
                new Vector2(0.09f, 0f), new Vector2(0.15f, 0.04f), new Vector2(0.19f, 0.11f), new Vector2(0.205f, 0.2f),
                new Vector2(0.2f, 0.29f), new Vector2(0.17f, 0.37f), new Vector2(0.1f, 0.42f)
            }, 32, ribs: 12, ribDepth: 0.06f, capBottom: false);
            // The silk shows its ribs and hoops as darker lines and glows most at the belly; its brightness is set on its
            // own, apart from the light it throws on the house.
            var silkMap = LanternSilkMap;
            // Lit silk shows only its glow: a near-black base keeps the candle 0.2 m inside it from burning the red to cream,
            // and the glow stays under 1 because the grade adds about one stop of exposure before tone mapping.
            var silkLit = EmissiveMat("LTB_LanternSilkLit", new Color(0.05f, 0.01f, 0.008f), 0.45f);
            silkLit.SetTexture("_BaseMap", silkMap);
            silkLit.SetTexture("_EmissionMap", silkMap);
            silkLit.SetColor("_EmissionColor", new Color(1f, 0.13f, 0.03f) * 0.38f);
            var silkDark = Mat("LTB_LanternSilk", new Color(0.36f, 0.06f, 0.05f), 0.35f);
            silkDark.SetTexture("_BaseMap", silkMap);
            var bodyGo = Piece("Body", body, silkLit, frame, position + Vector3.down * 0.21f * size, Quaternion.identity, Vector3.one * size);
            bodyGo.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            var cap = Lathe("LanternCap", new[] { new Vector2(0.11f, 0f), new Vector2(0.12f, 0.02f), new Vector2(0.1f, 0.045f), new Vector2(0.03f, 0.06f), new Vector2(0f, 0.06f) }, 20);
            Piece("CapTop", cap, Lacquer, frame, position + Vector3.up * 0.205f * size, Quaternion.identity, Vector3.one * size);
            Piece("CapBottom", cap, Lacquer, frame, position + Vector3.down * 0.215f * size, Quaternion.Euler(180f, 0f, 0f), Vector3.one * size);
            // Two thin hoops where the bamboo ribs are bound.
            foreach (float y in new[] { -0.1f, 0.1f })
                Piece("Hoop", Ring("LanternHoop", 0.198f, 0.004f, 360f, 32, 4), Timber, frame, position + Vector3.up * y * size, Quaternion.Euler(90f, 0f, 0f), Vector3.one * size);
            Piece("Cord", Lathe("Cord", new[] { new Vector2(0.006f, 0f), new Vector2(0.006f, 1f) }, 6, capBottom: false), Ridge, frame, position + Vector3.up * 0.26f * size, Quaternion.identity, new Vector3(1f, cord - 0.26f * size, 1f));
            var tassel = Lathe("Tassel", new[] { new Vector2(0.012f, 0f), new Vector2(0.04f, 0.02f), new Vector2(0.03f, 0.14f), new Vector2(0.012f, 0.18f), new Vector2(0.018f, 0.2f), new Vector2(0f, 0.22f) }, 12);
            Piece("Tassel", tassel, _silkRed, frame, position + Vector3.down * (0.27f + 0.22f) * size, Quaternion.identity, Vector3.one * size);
            // None of the lantern casts a shadow from its own candle: the caps and tassel would black out the pool of
            // light under it and print a lantern on the eaves.
            foreach (var part in root.GetComponentsInChildren<Renderer>()) part.shadowCastingMode = ShadowCastingMode.Off;
            root.AddComponent<LanternSway>();
            var lightObject = new GameObject("Light");
            lightObject.transform.SetParent(frame, false);
            lightObject.transform.position = position;
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.47f, 0.22f);
            light.range = range;
            light.intensity = intensity;
            light.shadows = shadows ? LightShadows.Soft : LightShadows.None;
            light.shadowStrength = 0.7f;
            var glow = root.AddComponent<LanternGlow>();
            Set(glow, "_light", light);
            Set(glow, "_body", bodyGo.GetComponent<Renderer>());
            Set(glow, "_lit", silkLit);
            Set(glow, "_unlit", silkDark);
            return light;
        }

        // ------------------------------------------------------------------ ceramics and small things

        private static Mesh VaseMesh => Lathe("Vase", new[] { new Vector2(0.06f, 0f), new Vector2(0.075f, 0.01f), new Vector2(0.11f, 0.08f), new Vector2(0.12f, 0.15f), new Vector2(0.09f, 0.24f), new Vector2(0.04f, 0.3f), new Vector2(0.035f, 0.36f), new Vector2(0.05f, 0.4f), new Vector2(0.044f, 0.405f) }, 24);
        private static Mesh JarMesh => Lathe("Jar", new[] { new Vector2(0.14f, 0f), new Vector2(0.2f, 0.06f), new Vector2(0.27f, 0.26f), new Vector2(0.26f, 0.4f), new Vector2(0.19f, 0.52f), new Vector2(0.15f, 0.56f), new Vector2(0.165f, 0.6f), new Vector2(0.14f, 0.605f) }, 28);
        private static Mesh BowlMesh => Lathe("Bowl", new[] { new Vector2(0.03f, 0f), new Vector2(0.035f, 0.01f), new Vector2(0.06f, 0.025f), new Vector2(0.075f, 0.05f), new Vector2(0.07f, 0.052f), new Vector2(0.03f, 0.02f), new Vector2(0f, 0.018f) }, 20, capBottom: true);
        private static Mesh CupMesh => Lathe("Cup", new[] { new Vector2(0.018f, 0f), new Vector2(0.022f, 0.005f), new Vector2(0.03f, 0.03f), new Vector2(0.032f, 0.042f), new Vector2(0.028f, 0.043f), new Vector2(0f, 0.01f) }, 16);
        private static Mesh TeapotMesh => Lathe("Teapot", new[] { new Vector2(0.04f, 0f), new Vector2(0.065f, 0.02f), new Vector2(0.075f, 0.05f), new Vector2(0.065f, 0.085f), new Vector2(0.035f, 0.1f), new Vector2(0.025f, 0.105f), new Vector2(0.012f, 0.125f), new Vector2(0f, 0.13f) }, 20);
        private static Mesh TrayMesh => Lathe("Tray", new[] { new Vector2(0.16f, 0f), new Vector2(0.17f, 0.012f), new Vector2(0.172f, 0.022f), new Vector2(0.162f, 0.022f), new Vector2(0.155f, 0.01f), new Vector2(0f, 0.01f) }, 28);
        private static Mesh PotMesh => Lathe("CookPot", new[] { new Vector2(0.12f, 0f), new Vector2(0.2f, 0.04f), new Vector2(0.23f, 0.14f), new Vector2(0.22f, 0.22f), new Vector2(0.24f, 0.235f), new Vector2(0.22f, 0.24f) }, 24);
        private static Mesh PlanterMesh => Lathe("Planter", new[] { new Vector2(0.22f, 0f), new Vector2(0.3f, 0.06f), new Vector2(0.36f, 0.3f), new Vector2(0.38f, 0.44f), new Vector2(0.42f, 0.47f), new Vector2(0.41f, 0.5f), new Vector2(0.36f, 0.49f) }, 28);

        /// <summary>A tea set on a tray: teapot with spout and handle, four cups.</summary>
        private static void TeaSet(Transform parent, Vector3 surface, float yaw, Material glaze)
        {
            var frame = Frame("TeaSet", parent, surface, yaw);
            Piece("Tray", TrayMesh, Lacquer, frame, surface, frame.rotation);
            Vector3 pot = frame.TransformPoint(new Vector3(-0.04f, 0.01f, 0.02f));
            Piece("Teapot", TeapotMesh, glaze, frame, pot, frame.rotation);
            Piece("Spout", Lathe("Spout", new[] { new Vector2(0.012f, 0f), new Vector2(0.008f, 0.07f), new Vector2(0.006f, 0.08f) }, 8, capBottom: false), glaze, frame,
                pot + frame.rotation * new Vector3(0.06f, 0.04f, 0f), frame.rotation * Quaternion.Euler(0f, 0f, -55f));
            Piece("Handle", Ring("TeapotHandle", 0.035f, 0.006f, 200f, 12, 6), glaze, frame, pot + frame.rotation * new Vector3(-0.07f, 0.055f, 0f), frame.rotation * Quaternion.Euler(0f, 0f, 80f));
            for (int i = 0; i < 4; i++)
            {
                float a = 0.6f + i * 0.55f;
                Piece("Cup", CupMesh, glaze, frame, frame.TransformPoint(new Vector3(Mathf.Cos(a) * 0.1f + 0.03f, 0.01f, Mathf.Sin(a) * 0.1f - 0.02f)), frame.rotation);
            }
        }

        private static void Vase(Transform parent, Vector3 floor, float scale, Material glaze, float yaw = 0f) =>
            Piece("Vase", VaseMesh, glaze, parent, floor, Quaternion.Euler(0f, yaw, 0f), Vector3.one * scale);

        private static GameObject Jar(Transform parent, Vector3 floor, float scale, Material glaze, bool solid)
        {
            var go = Piece("Jar", JarMesh, glaze, parent, floor, Quaternion.Euler(0f, floor.x * 53f, 0f), Vector3.one * scale);
            if (solid)
            {
                var capsule = go.AddComponent<CapsuleCollider>();
                capsule.center = new Vector3(0f, 0.3f, 0f);
                capsule.radius = 0.26f;
                capsule.height = 0.6f;
                go.layer = EnvironmentLayer;
            }
            return go;
        }

        private static void Books(Transform parent, Vector3 surface, float yaw, int count, int seed)
        {
            var random = new System.Random(seed);
            var frame = Frame("Books", parent, surface, yaw);
            float y = 0f;
            var covers = new[] { Mat("LTB_BookBrown", new Color(0.3f, 0.2f, 0.12f), 0.15f), Mat("LTB_BookIndigo", new Color(0.13f, 0.16f, 0.24f), 0.15f), Mat("LTB_BookOchre", new Color(0.55f, 0.42f, 0.22f), 0.15f) };
            for (int i = 0; i < count; i++)
            {
                float h = 0.012f + (float)random.NextDouble() * 0.01f;
                var book = Wood("Book", frame, new Vector3((float)random.NextDouble() * 0.02f, y + h * 0.5f, (float)random.NextDouble() * 0.02f), new Vector3(0.17f, h, 0.25f), covers[random.Next(covers.Length)], 0.002f);
                book.transform.rotation = frame.rotation * Quaternion.Euler(0f, (float)random.NextDouble() * 10f - 5f, 0f);
                y += h;
            }
        }

        // ------------------------------------------------------------------ cloth

        /// <summary>A curtain hung from a rod at <paramref name="top"/>, falling in folds; <paramref name="gather"/> ties it back.</summary>
        private static Transform PleatedCurtain(string name, Vector3 top, float width, float drop, float yaw, Material cloth, Transform parent, float gather = 0f, bool rod = true)
        {
            var frame = Frame(name, parent, top, yaw);
            int folds = Mathf.Max(3, Mathf.RoundToInt(width / 0.22f));
            var mesh = Pleats($"Pleats_{width:F2}_{drop:F2}_{gather:F2}".Replace('.', 'p'), width, drop, folds, 0.035f, gather);
            var cloth3d = Piece("Cloth", mesh, cloth, frame, frame.TransformPoint(new Vector3(-width * 0.5f, 0f, 0f)), frame.rotation);
            cloth3d.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.TwoSided;
            if (rod) Wood("Rod", frame, new Vector3(0f, 0.02f, 0f), new Vector3(width + 0.12f, 0.035f, 0.035f), Timber, 0.012f);
            return frame;
        }

        private static GameObject Cushion(string name, Transform frame, Vector3 centre, Vector3 size, Material cloth, float puff = 0.02f) =>
            Piece(name, RoundedBox(size, Mathf.Min(size.y * 0.45f, 0.05f), puff), cloth, frame, frame.TransformPoint(centre), frame.rotation);

        /// <summary>A round bolster pillow (gối ôm) lying along X.</summary>
        private static void Bolster(Transform frame, Vector3 centre, float length, float radius, Material cloth)
        {
            var mesh = Lathe("Bolster", new[] { new Vector2(0f, 0f), new Vector2(0.7f, 0.01f), new Vector2(0.95f, 0.05f), new Vector2(1f, 0.15f), new Vector2(1f, 0.85f), new Vector2(0.95f, 0.95f), new Vector2(0.7f, 0.99f), new Vector2(0f, 1f) }, 20);
            Piece("Bolster", mesh, cloth, frame, frame.TransformPoint(centre - new Vector3(length * 0.5f, 0f, 0f)), frame.rotation * Quaternion.Euler(0f, 0f, -90f), new Vector3(radius, length, radius));
            foreach (float x in new[] { -length * 0.5f, length * 0.5f })
                Piece("BolsterEnd", Lathe("BolsterEnd", new[] { new Vector2(1f, 0f), new Vector2(1f, 1f) }, 20, capBottom: false), _silkGold, frame,
                    frame.TransformPoint(centre + new Vector3(x - 0.01f, 0f, 0f)), frame.rotation * Quaternion.Euler(0f, 0f, -90f), new Vector3(radius * 1.02f, 0.02f, radius * 1.02f));
        }

        // ------------------------------------------------------------------ furniture

        private static Mesh TurnedLeg => Lathe("TurnedLeg", new[]
        {
            new Vector2(0.034f, 0f), new Vector2(0.04f, 0.03f), new Vector2(0.03f, 0.08f), new Vector2(0.026f, 0.35f), new Vector2(0.03f, 0.62f),
            new Vector2(0.034f, 0.66f), new Vector2(0.03f, 0.7f), new Vector2(0.032f, 1f)
        }, 16, capTop: true);




        /// <summary>A low tea table for sitting on the floor or on a sập (kỷ trà), with a tea set.</summary>
        private static Transform TeaTable(string name, Transform parent, Vector3 floor, float yaw, Material glaze, bool solid = true)
        {
            var frame = Table(name, parent, floor, yaw, 0.72f, 0.5f, 0.34f, Lacquer, false, true, solid);
            TeaSet(frame, frame.TransformPoint(new Vector3(0.05f, 0.34f, 0f)), yaw + 15f, glaze);
            return frame;
        }

        /// <summary>A wooden trunk (rương): body, a slightly wider lid, iron or brass corner straps and a hasp. Solid.</summary>
        private static Transform Trunk(string name, Transform parent, Vector3 floor, float yaw, float width, float height, float depth, bool worn = false)
        {
            var frame = Frame(name, parent, floor, yaw);
            var wood = worn ? _woodWorn : Lacquer;
            var metal = worn ? Iron : _brass;
            Wood("Body", frame, new Vector3(0f, height * 0.42f, 0f), new Vector3(width, height * 0.84f, depth), wood, 0.012f);
            Wood("Lid", frame, new Vector3(0f, height * 0.92f, 0f), new Vector3(width + 0.02f, height * 0.16f, depth + 0.02f), wood, 0.015f);
            foreach (float x in new[] { -1f, 1f })
                Wood("Strap", frame, new Vector3(x * (width * 0.5f - 0.08f), height * 0.5f, 0f), new Vector3(0.04f, height + 0.01f, depth + 0.025f), metal, 0.004f);
            Wood("Hasp", frame, new Vector3(0f, height * 0.78f, depth * 0.5f + 0.012f), new Vector3(0.06f, 0.1f, 0.012f), metal, 0.003f);
            Solid(frame, new Vector3(0f, height * 0.5f, 0f), new Vector3(width, height, depth));
            return frame;
        }

        /// <summary>A moon lute (đàn nguyệt) lying on the stage: round sound box, long fretted neck, tuning head.</summary>
        private static void MoonLute(Transform parent, Vector3 position, float yaw)
        {
            var frame = Frame("MoonLute", parent, position, yaw);
            var body = Lathe("LuteBody", new[] { new Vector2(0.17f, 0f), new Vector2(0.18f, 0.01f), new Vector2(0.18f, 0.05f), new Vector2(0.17f, 0.06f), new Vector2(0f, 0.06f) }, 32);
            Piece("Body", body, Lacquer, frame, position, frame.rotation);
            Piece("Face", Lathe("LuteFace", new[] { new Vector2(0.165f, 0f), new Vector2(0f, 0.001f) }, 32, capBottom: false), Timber, frame, position + Vector3.up * 0.061f, frame.rotation);
            Wood("Neck", frame, new Vector3(0.52f, 0.035f, 0f), new Vector3(0.75f, 0.03f, 0.04f), Timber, 0.008f);
            for (int i = 0; i < 8; i++) Wood("Fret", frame, new Vector3(0.25f + i * 0.075f, 0.053f, 0f), new Vector3(0.008f, 0.008f, 0.045f), _bronze, 0.002f);
            Wood("Head", frame, new Vector3(0.98f, 0.05f, 0f), new Vector3(0.2f, 0.035f, 0.06f), Lacquer, 0.01f);
            Piece("Tassel", Lathe("LuteTassel", new[] { new Vector2(0.01f, 0f), new Vector2(0.025f, 0.02f), new Vector2(0.02f, 0.12f), new Vector2(0f, 0.13f) }, 10), _silkRed, frame,
                frame.TransformPoint(new Vector3(1.08f, 0.02f, 0.06f)), frame.rotation * Quaternion.Euler(80f, 0f, 0f));
        }

        /// <summary>A shelf unit with a few things on it (a working house: bowls, jars, boxes, books).</summary>
        private static Transform Shelves(string name, Transform parent, Vector3 floor, float yaw, float width, float height, int levels, Material wood, int seed, bool kitchen)
        {
            var frame = Frame(name, parent, floor, yaw);
            const float depth = 0.36f;
            foreach (float x in new[] { -1f, 1f }) Wood("Side", frame, new Vector3(x * (width * 0.5f - 0.02f), height * 0.5f, 0f), new Vector3(0.04f, height, depth), wood, 0.006f);
            var random = new System.Random(seed);
            for (int i = 0; i < levels; i++)
            {
                float y = 0.08f + i * (height - 0.12f) / (levels - 1);
                Wood("Board", frame, new Vector3(0f, y, 0f), new Vector3(width - 0.04f, 0.03f, depth), wood, 0.005f);
                if (i == levels - 1) continue;
                for (float x = -width * 0.5f + 0.15f; x < width * 0.5f - 0.12f; x += 0.16f + (float)random.NextDouble() * 0.14f)
                {
                    var p = frame.TransformPoint(new Vector3(x, y + 0.015f, (float)random.NextDouble() * 0.08f - 0.04f));
                    switch (random.Next(kitchen ? 4 : 3))
                    {
                        case 0: Piece("Bowl", BowlMesh, kitchen ? _terracotta : _porcelain, frame, p, frame.rotation, Vector3.one * (0.9f + (float)random.NextDouble() * 0.4f)); break;
                        case 1: Vase(frame, p, 0.45f + (float)random.NextDouble() * 0.2f, kitchen ? _brownGlaze : _celadon); break;
                        case 2: if (kitchen) Jar(frame, p, 0.28f, _terracotta, false); else Books(frame, p, (float)random.NextDouble() * 10f, 3 + random.Next(4), random.Next()); break;
                        default: Piece("Pot", PotMesh, _ironPot, frame, p, frame.rotation, Vector3.one * 0.5f); break;
                    }
                }
            }
            Solid(frame, new Vector3(0f, height * 0.5f, 0f), new Vector3(width, height, depth));
            return frame;
        }

        /// <summary>A glazed planter with a young palm from the project's vegetation (lighter copy).</summary>
        private static void Planter(Transform parent, Vector3 floor, string palm, float scale, Material glaze)
        {
            var pot = Piece("Planter", PlanterMesh, glaze, parent, floor, Quaternion.Euler(0f, floor.x * 41f, 0f));
            var capsule = pot.AddComponent<CapsuleCollider>();
            capsule.center = new Vector3(0f, 0.25f, 0f);
            capsule.radius = 0.38f;
            capsule.height = 0.5f;
            pot.layer = EnvironmentLayer;
            Piece("Soil", Lathe("PlanterSoil", new[] { new Vector2(0.36f, 0f), new Vector2(0f, 0.01f) }, 24, capBottom: false), _soot, parent, floor + Vector3.up * 0.46f, Quaternion.identity);
            if (palm == PropBanana) Prop(palm, floor + Vector3.up * 0.45f, floor.z * 31f, scale, parent);
            else LiteProp(palm, 40000, floor + Vector3.up * 0.45f, floor.z * 31f, scale, parent, false);
        }

        /// <summary>The yard's frangipani (cây đại): bark, leaves and cream flowers, and flowers fallen on the bed around it.</summary>
        private static void FrangipaniTree(Transform parent, Vector3 foot, int seed)
        {
            var (bark, leaves, flowers) = Frangipani(seed);
            var barkMaterial = StoryArtKit.Surface("LTB_Bark", "dark_wood", new Color(0.46f, 0.42f, 0.37f), 0.5f, 0.12f, 1.2f, 0.5f, 0f, Folder);
            var leafMaterial = TwoSided(Mat("LTB_FrangipaniLeaf", new Color(0.2f, 0.37f, 0.12f), 0.45f));
            var flowerMaterial = TwoSided(Mat("LTB_FrangipaniFlower", new Color(0.96f, 0.92f, 0.8f), 0.25f));
            var rotation = Quaternion.Euler(0f, seed * 37f, 0f);
            Piece("Trunk", bark, barkMaterial, parent, foot, rotation);
            Piece("Leaves", leaves, leafMaterial, parent, foot, rotation).GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.TwoSided;
            Piece("Flowers", flowers, flowerMaterial, parent, foot, rotation).GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            Piece("FallenOnBed", FallenFlowers(seed, 1.2f, 14), flowerMaterial, parent, foot, Quaternion.identity).GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            Piece("FallenOnPaving", FallenFlowers(seed + 1, 2.6f, 10), flowerMaterial, parent, new Vector3(foot.x + 1.2f, 0.002f, foot.z - 1.4f), Quaternion.identity).GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
        }

        private static Material TwoSided(Material material)
        {
            material.SetFloat("_Cull", 0f);
            material.doubleSidedGI = true;
            UnityEditor.EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>A stone bench (ghế đá) at the edge of the yard: a slab on two blocks. Solid.</summary>
        private static void StoneBench(Transform parent, Vector3 floor, float yaw)
        {
            var frame = Frame("StoneBench", parent, floor, yaw);
            Wood("Seat", frame, new Vector3(0f, 0.43f, 0f), new Vector3(1.6f, 0.1f, 0.45f), Plinth, 0.02f);
            foreach (float x in new[] { -0.58f, 0.58f }) Wood("Leg", frame, new Vector3(x, 0.19f, 0f), new Vector3(0.18f, 0.38f, 0.4f), Plinth, 0.015f);
            Solid(frame, new Vector3(0f, 0.24f, 0f), new Vector3(1.6f, 0.48f, 0.45f));
        }

        /// <summary>
        /// A bamboo blind let down over a bay of the slatted screen: thin slats bound by cords, rolled at the top. It blocks
        /// eyes as it blocks light (an Environment collider on the screen's plane, which already stops bodies).
        /// </summary>
        private static void BambooBlind(string name, Vector3 topCentre, float width, float drop, Transform parent)
        {
            var frame = Frame(name, parent, topCentre, 0f);
            Wood("Roll", frame, new Vector3(0f, 0f, -0.04f), new Vector3(width, 0.07f, 0.07f), Bamboo, 0.03f);
            for (float y = -0.08f; y > -drop; y -= 0.045f)
                Wood("Slat", frame, new Vector3(0f, y, -0.06f), new Vector3(width - 0.04f, 0.032f, 0.008f), Bamboo, 0.003f);
            foreach (float x in new[] { -width * 0.3f, width * 0.3f })
                Wood("Cord", frame, new Vector3(x, -drop * 0.5f, -0.068f), new Vector3(0.006f, drop, 0.006f), _silkRed, 0.002f);
            var block = new GameObject("SightBlock");
            block.transform.SetParent(frame, false);
            block.transform.localPosition = new Vector3(0f, -drop * 0.5f, 0.02f);
            block.layer = EnvironmentLayer;
            block.AddComponent<BoxCollider>().size = new Vector3(width, drop, 0.04f);
        }

        /// <summary>
        /// Standing water on the flagstones in two layers: a damp margin (darker, half-gloss stone) spreading past the
        /// water, and the water, deepest in the middle and thinning to a soft wandering rim with stone tops showing
        /// through. Both are alpha-blended URP Lit, reflections fading with the alpha: the stone shows where the water is
        /// thin, lanterns and sky reflect where it is deep, and there is no paper-cut edge or flat grey sheet.
        /// In the rain (Chapter 4) the water is stirred by ripples.
        /// </summary>
        private static void Puddle(Transform parent, Vector3 position, Vector2 size, float yaw)
        {
            int seed = Mathf.Abs(Mathf.RoundToInt(position.x * 13f + position.z * 7f)) % 6;
            var rotation = Quaternion.Euler(0f, yaw, 0f);
            var damp = Piece("PuddleDamp", PuddleQuad, WaterMaterial(seed, true), parent, position + Vector3.up * 0.003f, rotation, new Vector3(size.x * 0.85f, 1f, size.y * 0.85f));
            var water = Piece("PuddleWater", PuddleQuad, WaterMaterial(seed, false), parent, position + Vector3.up * 0.005f, rotation, new Vector3(size.x * 0.6f, 1f, size.y * 0.6f));
            foreach (var go in new[] { damp, water }) go.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
        }

        /// <summary>A flat square, −1..1 in X and Z, UVs 0..1, facing up.</summary>
        private static Mesh PuddleQuad
        {
            get
            {
                const string key = "PuddleQuad_v1";
                if (LauTuBaMeshKit.Has(key, out var mesh)) return mesh;
                var j = new LauTuBaJoinery();
                j.Grid(1, 1, (u, v) => new Vector3(u * 2f - 1f, 0f, v * 2f - 1f), (u, v) => Vector3.up, (u, v) => new Vector2(u, v));
                return LauTuBaMeshKit.Keep(key, j.Build(key));
            }
        }

        /// <summary>Transparent water or damp-stone material for <see cref="Puddle"/>; Chapter 3 and 4 get their own.</summary>
        private static Material WaterMaterial(int seed, bool damp)
        {
            string name = (damp ? "LTB_PuddleDamp_" : "LTB_PuddleWater_") + seed + (_night ? "_Night" : "");
            var material = Mat(name, damp ? new Color(0.13f, 0.12f, 0.11f, 0.6f) : new Color(0.018f, 0.02f, 0.024f, 0.92f), damp ? 0.55f : 0.94f);
            material.SetTexture("_BaseMap", LauTuBaTextures.PuddleMask(seed + (damp ? 10 : 0)));
            material.SetTextureScale("_BaseMap", Vector2.one);
            if (!damp && _night)
            {
                material.SetTexture("_BumpMap", LauTuBaTextures.RippleNormal());
                material.SetFloat("_BumpScale", 0.35f);
                material.EnableKeyword("_NORMALMAP");
            }
            else
            {
                material.SetTexture("_BumpMap", null);
                material.DisableKeyword("_NORMALMAP");
            }
            return SeeThrough(material);
        }

        /// <summary>Alpha-blended URP Lit: a film of water on stone or wood, reflections and all fading out with its alpha.</summary>
        private static Material SeeThrough(Material material)
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            // Highlights fade with the alpha too: with "preserve specular" the whole quad would show the sky as a pale square.
            material.SetFloat("_BlendModePreserveSpecular", 0f);
            material.SetFloat("_ZWrite", 0f);
            material.SetFloat("_ReceiveShadows", 1f);
            UnityEditor.BaseShaderGUI.SetupMaterialBlendMode(material);
            UnityEditor.EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>The wet film for <see cref="WetBand"/>: darker and glossier than what it lies on, wettest at one edge.</summary>
        /// <param name="covered">Under a roof (veranda boards): damp, not standing water, and it does not mirror the sky.</param>
        private static Material WetMaterial(bool covered = false)
        {
            var material = covered
                ? Mat("LTB_DampBoards" + (_night ? "_Night" : ""), new Color(0.04f, 0.03f, 0.025f, _night ? 0.55f : 0.35f), 0.45f)
                : Mat("LTB_WetFilm" + (_night ? "_Night" : ""), new Color(0.05f, 0.045f, 0.04f, _night ? 0.7f : 0.5f), _night ? 0.82f : 0.68f);
            material.SetTexture("_BaseMap", LauTuBaTextures.DampEdge());
            material.SetTextureScale("_BaseMap", Vector2.one);
            return SeeThrough(material);
        }

        /// <summary>
        /// A wet band on the ground along <paramref name="a"/> → <paramref name="b"/> (its wettest edge), drying out across
        /// <paramref name="width"/> towards <paramref name="across"/>. One mesh, no collider, no shadow.
        /// </summary>
        private static void WetBand(string name, Vector3 a, Vector3 b, Vector3 across, float width, Material wet, Transform parent)
        {
            Vector3 along = b - a;
            float length = along.magnitude;
            Vector3 side = across.normalized * width;
            string key = $"WetBand_{a.x:F2}_{a.z:F2}_{b.x:F2}_{b.z:F2}_{side.x:F2}_{side.z:F2}_{a.y:F3}".Replace('.', 'p').Replace('-', 'm');
            if (!LauTuBaMeshKit.Has(key, out var mesh))
            {
                var j = new LauTuBaJoinery();
                j.Grid(Mathf.Max(1, Mathf.RoundToInt(length / 2f)), 1, (u, v) => along * u + side * v, (u, v) => Vector3.up, (u, v) => new Vector2(u * length / 2.5f, v), 0,
                    Vector3.Cross(side, along).y < 0f);
                mesh = LauTuBaMeshKit.Keep(key, j.Build(key));
            }
            var go = Piece(name, mesh, wet, parent, a, Quaternion.identity);
            go.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
        }

        /// <summary>
        /// A four-leaf folding screen (bình phong): lacquered frames with a carved band at the foot and silk panels
        /// with a darker border. One collider for the whole screen (it blocks bodies and sight at full height).
        /// </summary>
        /// <summary>One leaf of a folding screen as joinery: stiles with turned finials and small feet, three rails, a
        /// lower panel cut in a cloud line along its top.</summary>
        private static Mesh ScreenLeafMesh()
        {
            const string key = "Joinery_ScreenLeaf_v1";
            if (LauTuBaMeshKit.Has(key, out var cached)) return cached;
            var j = new LauTuBaJoinery();
            foreach (float x in new[] { -0.23f, 0.23f })
            {
                j.Member(new Vector3(x, 0.03f, 0f), new Vector3(x, 1.84f, 0f), 0.042f, 0.042f, 0.008f, Vector3.forward);
                j.Turned(new Vector3(x, 1.84f, 0f), new[] { new Vector2(0.02f, 0f), new Vector2(0.026f, 0.02f), new Vector2(0.018f, 0.04f), new Vector2(0.024f, 0.07f), new Vector2(0.01f, 0.1f), new Vector2(0f, 0.105f) }, 10);
                j.Add(RoundedBox(new Vector3(0.07f, 0.05f, 0.1f), 0.012f), Matrix4x4.Translate(new Vector3(x, 0.025f, 0f)));
            }
            foreach (float y in new[] { 0.08f, 0.44f, 1.8f })
                j.Member(new Vector3(-0.23f, y, 0f), new Vector3(0.23f, y, 0f), 0.034f, 0.034f, 0.006f, Vector3.up);
            var panel = new List<Vector2> { new Vector2(-0.21f, 0.1f), new Vector2(0.21f, 0.1f) };
            for (int i = 0; i <= 16; i++) { float t = i / 16f, x = Mathf.Lerp(0.21f, -0.21f, t); panel.Add(new Vector2(x, 0.4f - 0.04f * Mathf.Abs(Mathf.Sin(t * Mathf.PI * 3f)))); }
            j.Board(panel.ToArray(), 0.016f, Matrix4x4.identity, 1);
            return LauTuBaMeshKit.Keep(key, j.Build(key));
        }

        private static Transform Screen(string name, Vector3 floorCentre, float yaw, Transform parent, Color silk)
        {
            var root = Frame(name, parent, floorCentre, yaw);
            var cloth = Mat("LTB_ScreenSilk_" + ColorUtility.ToHtmlStringRGB(silk), silk, 0.32f);
            var border = Mat("LTB_ScreenBorder_" + ColorUtility.ToHtmlStringRGB(silk), silk * 0.45f, 0.3f);
            for (int i = 0; i < 4; i++)
            {
                var panel = new GameObject("Leaf").transform;
                panel.SetParent(root, false);
                panel.localPosition = new Vector3(-0.75f + i * 0.5f, 0f, (i % 2) * 0.12f);
                panel.localRotation = Quaternion.Euler(0f, i % 2 == 0 ? 14f : -14f, 0f);
                Furniture("Joinery", ScreenLeafMesh(), panel, Lacquer, Timber);
                Wood("Border", panel, new Vector3(0f, 1.12f, 0f), new Vector3(0.42f, 1.36f, 0.01f), border, 0.002f);
                Wood("Silk", panel, new Vector3(0f, 1.12f, 0f), new Vector3(0.36f, 1.28f, 0.014f), cloth, 0.002f);
            }
            var block = new GameObject("ScreenBlock");
            block.transform.SetParent(root, false);
            block.transform.localPosition = new Vector3(0f, 0.92f, 0.06f);
            block.layer = EnvironmentLayer;
            block.AddComponent<BoxCollider>().size = new Vector3(2f, 1.8f, 0.2f);
            return root;
        }

        /// <summary>Makes a fill light count for less in <see cref="LightExposure"/> (it stands in for bounce or moonlight).</summary>
        private static Light Weighted(Light light, float weight)
        {
            Set(light.gameObject.AddComponent<ExposureWeight>(), "_weight", weight);
            return light;
        }

        private static Material MetalMat(string name, Color color, float metallic, float smoothness)
        {
            var material = Mat(name, color, smoothness);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            UnityEditor.EditorUtility.SetDirty(material);
            return material;
        }
    }
}
