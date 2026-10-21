using System;
using System.Linq;
using ThuyKieu.Core.Editor;
using ThuyKieu.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThuyKieu.Environment.Editor
{
    /// <summary>
    /// Brings the Vương house in line with the painted montage panels (Resources/StoryPanels): ochre lime-plaster walls
    /// with tiled copings, wet red brick underfoot, terracotta roofs, vertical-bar windows dark against a warm lamp, a dirt
    /// road and fields beyond the gate instead of a black wall, no palms inside the yard, and a grey-teal rain grade that
    /// keeps the night readable. Chapter 1 is dressed in its scene (rebuilt on every run, replaced objects are only
    /// deactivated); the Prologue house is generated, so it only needs its shared materials retinted. Re-running is harmless.
    /// </summary>
    public static class PaintedLookUpgrade
    {
        private const string Look = Chapter01LookUpgrade.LookAssets;
        private const string Root = "PaintedLook";
        private const float GateHalf = 2.05f, YardHalf = 9.65f, GateZ = -13.1f;
        private const string LonganTree = "Assets/_Game/Prefabs/Props/CayNhan.prefab";
        private const string SilverPouch = "Assets/_Game/Prefabs/Props/TuiBac.prefab";
        private const string TeaStall = "Assets/_Game/Prefabs/Props/QuanNuoc.prefab";

        [MenuItem("ThuyKieu/Chapter 1/Apply painted look")]
        public static void ApplyChapter01()
        {
            var scene = Open(Chapter01StoryUpgrade.ScenePath);
            var house = GameObject.Find("VuongGia").transform;
            StorySceneKit.AssetFolder = Look;
            try
            {
                var brick = StoryArtKit.Surface("Ch1BrickWet", "brick_floor", new Color(0.52f, 0.34f, 0.27f), 1.6f, 0.38f, 1f, 0.35f, 0.5f, Look);
                var plaster = StoryArtKit.Surface("Ch1PlasterOchre", "plastered_wall_02", new Color(0.8f, 0.68f, 0.46f), 2.6f, 0.1f, 1.1f, 0.5f, 0.9f, Look);
                var tiles = StoryArtKit.Surface("Ch1RoofTerracotta", "clay_roof_tiles_02", new Color(0.6f, 0.37f, 0.28f), 1.4f, 0.5f, 1.2f, 0.35f, 0f, Look);
                var ridge = StorySceneKit.Material("Ch1RidgeDark", new Color(0.2f, 0.12f, 0.09f), 0.4f);

                Retint(house, "WetStone", brick, t => !t.name.StartsWith("ColumnBase"));
                Retint(house, "Chapter01Courtyard", plaster);
                Retint(house, "LimePlaster", plaster);
                Retint(house, "RoofTile", tiles);
                foreach (var name in new[] { "BackBoundary", "SideBoundary-1", "SideBoundary1" }) Use(Find(house, name), plaster);

                var old = house.Find(Root);
                if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
                var root = new GameObject(Root).transform;
                root.SetParent(house, false);

                Windows(house, root);
                YardWalls(house, root, plaster, tiles, ridge);
                Outside(root);
                ReplacePalms(house, root);
                PouchModel();
                Grade();
            }
            finally { StorySceneKit.AssetFolder = StorySceneKit.MaterialFolder; }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        /// <summary>The Prologue house already has brick, plaster walls with tiled copings and dirt lanes: only the walls warm to ochre.</summary>
        [MenuItem("ThuyKieu/Prologue/Apply painted look")]
        public static void ApplyPrologue()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode before the painted look.");
            StoryArtKit.Surface("PlPlaster", "plastered_wall_02", PrologueSetup.PlasterTint, 2.6f, 0.08f, 1.1f, 0.45f, 0.8f, StorySceneKit.MaterialFolder);
            AssetDatabase.SaveAssets();
        }

        private static UnityEngine.SceneManagement.Scene Open(string path)
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode before the painted look.");
            var active = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (active.path == path) return active;
            if (active.isDirty) throw new InvalidOperationException("Save or discard the changes in " + active.path + " first.");
            return EditorSceneManager.OpenScene(path);
        }

        // ------------------------------------------------------------------ materials

        private static void Retint(Transform root, string from, Material to, Func<Transform, bool> accept = null)
        {
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (accept != null && !accept(renderer.transform)) continue;
                var materials = renderer.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < materials.Length; i++)
                    if (materials[i] != null && materials[i].name == from) { materials[i] = to; changed = true; }
                if (changed) renderer.sharedMaterials = materials;
            }
        }

        private static void Use(Transform target, Material material)
        {
            if (target != null && target.TryGetComponent(out Renderer renderer)) renderer.sharedMaterial = material;
        }

        private static Transform Find(Transform root, string name)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true)) if (t.name == name) return t;
            return null;
        }

        // ------------------------------------------------------------------ dressing

        /// <summary>
        /// Shoji-like paper panes become dark openings behind close vertical wooden bars (song gỗ); the two front windows
        /// beside the door glow with the oil lamp inside, as in the panels.
        /// </summary>
        private static void Windows(Transform house, Transform root)
        {
            var dark = StorySceneKit.Material("Ch1WindowGap", new Color(0.09f, 0.06f, 0.04f), 0.1f);
            var lamp = new Color(0.9f, 0.45f, 0.16f);
            var lit = StorySceneKit.Emissive("Ch1WindowLamp", lamp, 0.35f);
            // Emissive() keeps an existing asset as it is: retune it so a re-run takes the current glow.
            lit.SetColor("_BaseColor", lamp);
            lit.SetColor("_EmissionColor", lamp * 0.35f);
            EditorUtility.SetDirty(lit);
            var bar = StoryArtKit.Surface("Ch1WindowBar", "dark_wood", new Color(0.62f, 0.46f, 0.34f), 1.2f, 0.3f, 1f, 0.25f, 0f, Look);
            var structure = Find(house, "Structure");
            var bars = new GameObject("WindowBars").transform;
            bars.SetParent(root, false);
            for (int i = 0; i < structure.childCount; i++)
            {
                var window = structure.GetChild(i);
                if (!window.name.Contains("Window")) continue;
                bool glow = window.name == "FrontWindow-1_0" || window.name == "FrontWindow1_0";
                Retint(window, "WarmPaper", glow ? lit : dark);
                var renderers = window.GetComponentsInChildren<Renderer>();
                var bounds = renderers[0].bounds;
                foreach (var r in renderers) bounds.Encapsulate(r.bounds);
                bool alongX = bounds.size.x > bounds.size.z;
                float width = alongX ? bounds.size.x : bounds.size.z;
                float depth = (alongX ? bounds.size.z : bounds.size.x) + 0.04f;
                int count = Mathf.RoundToInt(width / 0.11f);
                for (int b = 1; b < count; b++)
                {
                    float offset = -width * 0.5f + width * b / count;
                    Vector3 position = bounds.center + (alongX ? Vector3.right : Vector3.forward) * offset;
                    Vector3 size = alongX ? new Vector3(0.035f, bounds.size.y - 0.08f, depth) : new Vector3(depth, bounds.size.y - 0.08f, 0.035f);
                    StorySceneKit.Block("Bar", position, size, bar, bars, false);
                }
            }
        }

        /// <summary>
        /// The bamboo fences either side of the gate become ochre lime walls; every yard wall gets a small tiled coping.
        /// The flat wall behind the gate keeps its collider (the yard edge) but is no longer drawn, so the road shows through.
        /// </summary>
        private static void YardWalls(Transform house, Transform root, Material plaster, Material tiles, Material ridge)
        {
            var walls = new GameObject("YardWalls").transform;
            walls.SetParent(root, false);
            foreach (var name in new[] { "Fence-1", "Fence1" })
            {
                var fence = Find(house, name);
                if (fence != null) fence.gameObject.SetActive(false);
            }
            var behindGate = Find(house, "CourtyardGate");
            if (behindGate != null && behindGate.TryGetComponent(out Renderer hidden)) hidden.enabled = false;

            const float height = 2.3f;
            float span = YardHalf - GateHalf;
            foreach (float side in new[] { -1f, 1f })
            {
                float x = side * (GateHalf + span * 0.5f);
                StorySceneKit.Block("GateWall", new Vector3(x, height * 0.5f, GateZ), new Vector3(span, height, 0.35f), plaster, walls);
                Coping("GateWallCoping", new Vector3(x, height, GateZ), new Vector3(span, 0f, 0.62f), tiles, ridge, walls);
                // Side walls run from the gate wall to the house wings.
                Coping("SideWallCoping", new Vector3(side * 9.5f, 2f, -7f), new Vector3(0.62f, 0f, 12f), tiles, ridge, walls);
            }
            foreach (var t in walls.GetComponentsInChildren<Renderer>()) t.shadowCastingMode = ShadowCastingMode.On;
        }

        /// <summary>
        /// A flat tiled cap along a wall top with a dark ridge strip; no curled ridge ends, which would reach into the gateway.
        /// </summary>
        private static void Coping(string name, Vector3 top, Vector3 size, Material tiles, Material ridge, Transform parent)
        {
            StorySceneKit.Block(name, top + Vector3.up * 0.06f, new Vector3(size.x, 0.12f, size.z), tiles, parent, false);
            Vector3 strip = size.x > size.z ? new Vector3(size.x, 0.07f, 0.14f) : new Vector3(0.14f, 0.07f, size.z);
            StorySceneKit.Block(name + "Ridge", top + Vector3.up * 0.15f, strip, ridge, parent, false);
        }

        /// <summary>A muddy lane between wet fields leading away from the gate, trees fading into rain mist.</summary>
        private static void Outside(Transform root)
        {
            var outside = new GameObject("BeyondGate").transform;
            outside.SetParent(root, false);
            var road = StoryArtKit.Surface("Ch1RoadMud", "brown_mud_02", new Color(0.4f, 0.31f, 0.24f), 3f, 0.3f, 1f, 0.45f, 0f, Look);
            var field = StoryArtKit.Surface("Ch1Field", "leafy_grass", new Color(0.4f, 0.48f, 0.34f), 4f, 0.25f, 1f, 0.4f, 0f, Look);
            StorySceneKit.Block("Lane", new Vector3(0f, -0.04f, -38f), new Vector3(3.4f, 0.1f, 48f), road, outside, false);
            StorySceneKit.Block("FieldWest", new Vector3(-26.7f, -0.08f, -38f), new Vector3(50f, 0.1f, 48f), field, outside, false);
            StorySceneKit.Block("FieldEast", new Vector3(26.7f, -0.08f, -38f), new Vector3(50f, 0.1f, 48f), field, outside, false);
            StoryArtKit.Grass("LaneGrass", outside, new Rect(-22f, -60f, 44f, 45f), 3400, p => Mathf.Abs(p.x) > 1.9f, 61, 0.5f,
                new Color(0.36f, 0.46f, 0.3f), new Color(0.46f, 0.5f, 0.32f), "Ch1LaneGrass");
            foreach (var puddle in new[] { new Vector3(0.4f, 0.02f, -18f), new Vector3(-0.5f, 0.02f, -24f), new Vector3(0.3f, 0.02f, -31f) })
                StoryPropKit.Puddle(puddle, new Vector2(1.4f, 0.9f), puddle.z * 7f, outside);

            string[] trees = { LonganTree, LonganTree, "Assets/_Game/Prefabs/Vegetation/Bamboo.prefab", "Assets/_Game/Prefabs/Vegetation/BananaTree.prefab" };
            var place = new System.Random(83);
            for (int i = 0; i < 26; i++)
            {
                float side = i % 2 == 0 ? -1f : 1f;
                var position = new Vector3(side * (4f + (float)place.NextDouble() * 22f), 0f, -17f - (float)place.NextDouble() * 34f);
                string kind = trees[place.Next(trees.Length)];
                float scale = kind == LonganTree ? 0.8f + (float)place.NextDouble() * 0.4f : 1f + (float)place.NextDouble() * 0.7f;
                var tree = StorySceneKit.Prop(kind, position, place.Next(360), scale, outside);
                if (tree == null) continue;
                foreach (var renderer in tree.GetComponentsInChildren<Renderer>()) renderer.shadowCastingMode = ShadowCastingMode.Off;
                foreach (var collider in tree.GetComponentsInChildren<Collider>()) UnityEngine.Object.DestroyImmediate(collider);
            }
            StoryArtKit.Dust("FieldMist", new Vector3(0f, 0.9f, -32f), new Vector3(40f, 1.8f, 30f), outside, new Color(0.7f, 0.76f, 0.82f, 0.18f), 160);
        }

        /// <summary>The areca palms read as tropical next to the panels' plums and pot plants: hide them, pots take the yard spot.</summary>
        private static void ReplacePalms(Transform house, Transform root)
        {
            foreach (var t in house.GetComponentsInChildren<Transform>(true))
                if (t.name.StartsWith("ArecaPalm")) t.gameObject.SetActive(false);
            var pots = new GameObject("Pots").transform;
            pots.SetParent(root, false);
            StoryArtKit.PottedPlant(new Vector3(7.6f, 0f, -9.6f), 1.15f, pots, 91, true);
            StoryArtKit.PottedPlant(new Vector3(8.4f, 0f, -10.6f), 0.85f, pots, 92);
            StoryArtKit.PottedPlant(new Vector3(-7.6f, 0f, -10.2f), 1f, pots, 93, true);
        }

        /// <summary>
        /// The generated hemp pouch goes into the silver group; Chapter01World stands it on the table top at runtime
        /// (the table was rescaled) in place of the blockout pieces.
        /// </summary>
        private static void PouchModel()
        {
            var silver = GameObject.Find("SilverOnTable");
            if (silver == null) silver = Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(), g => g.name == "SilverOnTable" && g.scene.IsValid());
            if (silver == null) return;
            var old = silver.transform.Find("PouchModel");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            var pouch = silver.transform.Find("Pouch");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SilverPouch);
            if (pouch == null || prefab == null) return;
            var model = (GameObject)PrefabUtility.InstantiatePrefab(prefab, silver.transform);
            model.name = "PouchModel";
            model.transform.SetPositionAndRotation(pouch.position, Quaternion.Euler(0f, 200f, 0f));
            foreach (var collider in model.GetComponentsInChildren<Collider>()) UnityEngine.Object.DestroyImmediate(collider);
        }

        /// <summary>
        /// Chapter 2's roadside was areca palms and palm-like bamboo, which read as tropical next to the painted panels:
        /// the palms and every other bamboo clump become old longan trees, in place, without regenerating the scene.
        /// </summary>
        [MenuItem("ThuyKieu/Chapter 2/Apply painted look (longan trees)")]
        public static void ApplyChapter02()
        {
            var scene = Open(Chapter02Setup.ScenePath);
            var vegetation = GameObject.Find("Vegetation");
            var longan = AssetDatabase.LoadAssetAtPath<GameObject>(LonganTree);
            if (vegetation == null || longan == null) throw new InvalidOperationException("Chapter 2 vegetation or the longan prefab is missing.");
            var random = new System.Random(31);
            int bamboo = 0, replaced = 0;
            // The swap runs once: a re-run would thin out the bamboo that is left.
            bool swapped = vegetation.transform.Cast<Transform>().Any(t => t.name.StartsWith("CayNhan"));
            foreach (Transform plant in swapped ? new Transform[0] : vegetation.transform.Cast<Transform>().ToArray())
            {
                var source = PrefabUtility.GetCorrespondingObjectFromSource(plant.gameObject);
                string path = source != null ? AssetDatabase.GetAssetPath(source) : string.Empty;
                bool palm = path.EndsWith("ArecaPalm.prefab");
                bool swapBamboo = path.EndsWith("Bamboo.prefab") && bamboo++ % 2 == 0;
                if (!palm && !swapBamboo) continue;
                var renderer = plant.GetComponentInChildren<Renderer>();
                var shadows = renderer != null ? renderer.shadowCastingMode : ShadowCastingMode.Off;
                var tree = (GameObject)PrefabUtility.InstantiatePrefab(longan, vegetation.transform);
                tree.transform.SetPositionAndRotation(plant.position, Quaternion.Euler(0f, random.Next(360), 0f));
                tree.transform.localScale = Vector3.one * (0.75f + (float)random.NextDouble() * 0.45f);
                foreach (var collider in tree.GetComponentsInChildren<Collider>()) UnityEngine.Object.DestroyImmediate(collider);
                foreach (var r in tree.GetComponentsInChildren<Renderer>()) { r.shadowCastingMode = shadows; r.meshLodSelectionBias = 2f; }
                UnityEngine.Object.DestroyImmediate(plant.gameObject);
                replaced++;
            }
            Debug.Log("[PaintedLook] Chapter 2: " + replaced + " palms and bamboo clumps became longan trees.");
            TeaStallModel();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        /// <summary>
        /// The generated thatched tea stall replaces the blockout one: its signboard front turned to the road, its floor
        /// sunk to the blockout platform the story stands Kiều on. Everything the scene uses stays: the platform collider,
        /// the bench Kiều is told to sit on, the lanterns' light, and the woven screen she hides behind to listen.
        /// </summary>
        private static void TeaStallModel()
        {
            var tea = GameObject.Find("TeaHouse");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TeaStall);
            if (tea == null || prefab == null) return;
            var platform = tea.transform.Find("Platform");
            if (platform == null) return;
            var old = tea.transform.Find("TeaStallModel");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);

            string[] blockout = { "Platform", "PlatformFootings", "Post", "ThatchRoof", "TableSet", "CeramicJar", "Signboard", "SignGlyph",
                "Stove", "StoveMouth", "Kettle", "Firewood", "Jar", "JarLid", "Stool" };
            foreach (Transform part in tea.transform)
                if (Array.Exists(blockout, b => part.name.StartsWith(b)))
                    foreach (var renderer in part.GetComponentsInChildren<Renderer>()) renderer.enabled = false;

            var model = (GameObject)PrefabUtility.InstantiatePrefab(prefab, tea.transform);
            model.name = "TeaStallModel";
            Vector3 centre = platform.position;
            // The model's front (+Z, the signboard) faces the road, which runs on the platform's west (−X) side.
            model.transform.SetPositionAndRotation(new Vector3(centre.x, 0f, centre.z), Quaternion.Euler(0f, -90f, 0f));
            foreach (var collider in model.GetComponentsInChildren<Collider>()) UnityEngine.Object.DestroyImmediate(collider);
            // Sink it so its floor boards meet the blockout platform top.
            float floor = FloorHeight(model, centre);
            float top = platform.GetComponent<Renderer>().bounds.max.y;
            model.transform.position += Vector3.up * (top - floor);
            foreach (var renderer in model.GetComponentsInChildren<Renderer>()) renderer.shadowCastingMode = ShadowCastingMode.On;
        }

        /// <summary>The lowest walkable surface a little off the model's centre (temporary mesh colliders for the probe).</summary>
        private static float FloorHeight(GameObject model, Vector3 centre)
        {
            var probes = new System.Collections.Generic.List<Collider>();
            foreach (var filter in model.GetComponentsInChildren<MeshFilter>()) probes.Add(filter.gameObject.AddComponent<MeshCollider>());
            Physics.SyncTransforms();
            float floor = 0f;
            float best = float.MaxValue;
            foreach (var offset in new[] { new Vector3(0.6f, 0f, 0.8f), new Vector3(-0.6f, 0f, -0.8f), new Vector3(0.9f, 0f, -0.5f) })
                foreach (var hit in Physics.RaycastAll(centre + offset + Vector3.up * 2.5f, Vector3.down, 3f))
                    if (hit.collider.transform.IsChildOf(model.transform) && hit.point.y > 0.05f && hit.point.y < 1f && hit.point.y < best) best = hit.point.y;
            if (best < float.MaxValue) floor = best;
            foreach (var probe in probes) UnityEngine.Object.DestroyImmediate(probe);
            return floor;
        }

        /// <summary>Grey-teal rain with warm lamp highlights: less saturated and less blue than the old night, and lighter in the shadows.</summary>
        private static void Grade()
        {
            var camera = Camera.main;
            if (camera != null && camera.TryGetComponent(out ScreenFx fx))
                StoryArtKit.Grade(fx, 1.2f, 6f, -14f, -2f, new Color(0.44f, 0.48f, 0.52f), new Color(0.84f, 0.65f, 0.44f));
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.32f, 0.36f, 0.42f);
            RenderSettings.ambientEquatorColor = new Color(0.2f, 0.22f, 0.26f);
            RenderSettings.ambientGroundColor = new Color(0.14f, 0.12f, 0.1f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = new Color(0.17f, 0.2f, 0.24f);
            RenderSettings.fogDensity = 0.022f;
            var moon = GameObject.Find("Moonlight");
            if (moon != null && moon.TryGetComponent(out Light light)) { light.color = new Color(0.62f, 0.68f, 0.76f); light.intensity = 0.6f; }
            var rain = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/Chapter01Rain.mat");
            if (rain != null) { rain.SetColor("_BaseColor", new Color(0.74f, 0.78f, 0.82f, 0.35f)); EditorUtility.SetDirty(rain); }
        }
    }
}
