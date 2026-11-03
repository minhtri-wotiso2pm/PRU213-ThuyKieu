using System.Collections.Generic;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Chapters;
using ThuyKieu.Gameplay.Sequences;
using ThuyKieu.Gameplay.Stealth;
using ThuyKieu.Interaction;
using ThuyKieu.UI;
using UnityEditor;
using UnityEngine;
using static ThuyKieu.Core.Editor.StorySceneKit;
using static ThuyKieu.Core.Editor.StoryArtKit;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Builds Chapter02_RoadToMaGiamSinh: a red-earth road winding through bamboo and rice fields at dusk,
    /// a roadside tea house with a woven bamboo screen, and the lantern-lit gate of Lâm Tri.
    /// </summary>
    public static class Chapter02Setup
    {
        public const string ScenePath = "Assets/_Game/Scenes/Chapter02_RoadToMaGiamSinh.unity";
        private const float TeaZ = 100f;
        private const float GateZ = 146f;

        public static float RoadX(float z) => 6f * Mathf.Sin(z / 22f);

        [InitializeOnLoadMethod]
        private static void AutoApplyBaHangNuoc()
        {
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
                const string texPath = "Assets/_Game/Art/Characters/BaHangNuoc/Textures/texture_pbr_20250901.png";
                var targetTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                var existingMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/Story/BaHangNuocStory.mat");
                if (targetTex != null && existingMat != null && existingMat.GetTexture("_BaseMap") == targetTex && existingMat.GetColor("_BaseColor") == Color.white)
                    return;
                string prevScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path;
                UpdateTeaHouse();
                if (!string.IsNullOrEmpty(prevScene) && prevScene != ScenePath && System.IO.File.Exists(prevScene))
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(prevScene, UnityEditor.SceneManagement.OpenSceneMode.Single);
            };
        }

        [MenuItem("ThuyKieu/Chapter 2/Update Tea House")]
        public static void UpdateTeaHouse()
        {
            var ch2 = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(ScenePath, UnityEditor.SceneManagement.OpenSceneMode.Single);
            AssetFolder = MaterialFolder;
            var world = GameObject.Find("World").transform;
            var oldTea = GameObject.Find("World/TeaHouse");
            var oldWay = GameObject.Find("World/TeaWaystation");
            var oldLife = GameObject.Find("World/RoadsideLife");
            if (oldTea != null) Object.DestroyImmediate(oldTea);
            if (oldWay != null) Object.DestroyImmediate(oldWay);
            if (oldLife != null) Object.DestroyImmediate(oldLife);
            TeaHouse(world, out Transform bench, out BoxCollider listenZone);
            var ch2World = Object.FindAnyObjectByType<Chapter02World>();
            if (ch2World != null) Set(ch2World, "_bench", bench);
            var eavesGo = GameObject.Find("StorySystems/Sequences/eavesdrop_c2");
            var eavesdrop = eavesGo != null ? eavesGo.GetComponent<EavesdropSequence>() : null;
            if (eavesdrop != null) { Set(eavesdrop, "_zone", listenZone); Set(eavesdrop, "_start", bench); }
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(ch2);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("ThuyKieu/Chapter 2/Create playable scene")]
        public static void Create()
        {
            var scene = NewScene(ScenePath);
            StoryAnimationSetup.Controller();
            var s = CreateSystems(2, "Trên đường theo Mã Giám Sinh", "chapter_2", LauTuBaSetup.Chapter3Scene,
                new Vector3(RoadX(2), 0.05f, 2), 0f, "road");
            var world = new GameObject("World").transform;
            var nav = NavMesh(world);
            // Golden-hour grade: warm highlights, cool violet shadows, filmic so lanterns and sky keep their colour.
            Grade(s.Fx, 0.25f, 14f, 4f, 7f, new Color(0.42f, 0.44f, 0.62f), new Color(0.72f, 0.56f, 0.4f));
            UseSky(s.CameraComponent, DuskSky());

            Ground(world);
            Vegetation(world);
            Vector3 tea = TeaHouse(world, out Transform bench, out BoxCollider listenZone);
            Zone("TeaHouse", tea + new Vector3(0.8f, 1.5f, 0), new Vector3(8.5f, 4f, 7f), 2.7f, 0.38f, 52f, 0f, world);
            Gate(world, out GameObject[] nightLights);
            nightLights = AddNightLife(world, nightLights);

            // Characters
            var shared = StoryAnimationSetup.Controller();
            var ma = RiggedActor("MaGiamSinh", "MaGiamSinh", "Assets/_Game/Art/Characters/MaGiamSinh/Models/MaGiamSinh_Rigged.fbx",
                "Assets/_Game/Art/Characters/MaGiamSinh/Tectures", 1.78f, new Vector3(RoadX(5.5f) + 0.6f, 0, 5.5f), 0f, Color.white, shared);
            var servant = CharacterActor("TuyTung", "TuyTung", "LinhCanh", 1.72f, new Vector3(RoadX(4.2f) - 0.9f, 0, 4.2f), 0f);

            var askTarget = Explore(ma.gameObject, s.Director, "c2_ask", "Bắt chuyện với Mã Giám Sinh", Vector3.one * 1.9f);
            var observable = ma.gameObject.AddComponent<ObservableTarget>();
            Set(observable, "_director", s.Director);
            Set(observable, "_exploreId", "c2_observe");
            Set(observable, "_label", "Quan sát Mã Giám Sinh");
            var focus = Point("ObserveFocus", ma.transform.position + Vector3.up * 1.3f, 0, ma.transform);
            Set(observable, "_focusPoint", focus);

            // Dropped note: an unscaled anchor that the world drops when the escort passes waypoint 4.
            var paper = new GameObject("DroppedNote");
            paper.transform.SetParent(world, false);
            paper.transform.position = new Vector3(RoadX(40), 0.03f, 40);
            var sheet = Block("Sheet", paper.transform.position, new Vector3(0.24f, 0.006f, 0.17f), Material("Ch2Paper", new Color(0.92f, 0.86f, 0.7f)), paper.transform, false);
            sheet.transform.localPosition = Vector3.zero;
            var paperTarget = Explore(paper, s.Director, "c2_paper", "Nhặt mẩu giấy", new Vector3(1.2f, 0.8f, 1.2f), true);
            paper.SetActive(false);
            Point("Focus_Paper", paper.transform.position, 0, paper.transform);

            // Escort path along the road to the tea house.
            var pathRoot = new GameObject("EscortPath").transform;
            pathRoot.SetParent(world, false);
            var path = new List<Transform>();
            for (float z = 12; z < TeaZ - 4; z += 10) path.Add(Point("WP_" + path.Count, new Vector3(RoadX(z) + 0.5f, 0, z), 0, pathRoot));
            path.Add(Point("WP_TeaHouse", new Vector3(RoadX(TeaZ) + 2.6f, 0, TeaZ + 1.5f), 180, pathRoot));

            // Eavesdrop staging behind the bamboo screen.
            var maWall = Point("MaBehindWall", tea + new Vector3(6.4f, 0, 0.9f), 180, world);
            var servantWall = Point("ServantBehindWall", tea + new Vector3(6.5f, 0, -0.9f), 0, world);
            var maSensor = Sensor(ma.gameObject, s.Player, 9f, 110f, false);
            var servantSensor = Sensor(servant.gameObject, s.Player, 9f, 110f, false);
            var glanceTarget = Point("GlanceTarget", listenZone.bounds.center, 0, world);

            // Lâm Tri gate staging.
            var gatePlayer = Point("GatePlayer", new Vector3(RoadX(GateZ - 12), 0.05f, GateZ - 12), 0, world);
            var gateMa = Point("GateMa", new Vector3(RoadX(GateZ - 9.5f) + 0.8f, 0, GateZ - 9.5f), 0, world);
            var gateServant = Point("GateServant", new Vector3(RoadX(GateZ - 10.5f) - 1f, 0, GateZ - 10.5f), 0, world);
            Point("CAM_Wide_LamTri", new Vector3(RoadX(GateZ - 18) - 2.5f, 2.2f, GateZ - 18), 0, world).rotation = Quaternion.Euler(-4, 8, 0);
            Point("CAM_Wide_Road", new Vector3(RoadX(-4) + 3.5f, 2.6f, -4), 0, world).rotation = Quaternion.Euler(8, -14, 0);
            Point("CAM_Wide_TeaHouse", tea + new Vector3(-7f, 2.8f, -6.5f), 0, world).rotation = Quaternion.Euler(12, 48, 0);
            Point("CAM_CU_Kieu_Listens", listenZone.bounds.center + new Vector3(-1.1f, 0.4f, -1.4f), 0, world).rotation = Quaternion.Euler(8, 35, 0);

            // Sun & atmosphere
            var sun = Sun("Sun", new Color(1f, 0.62f, 0.38f), 1.25f, new Vector3(14, -60, 0));
            RenderSettings.fogDensity = 0.012f;

            // Systems that need the scene objects
            var worldScript = s.Root.AddComponent<Chapter02World>();
            Set(worldScript, "_director", s.Director);
            Set(worldScript, "_navMesh", nav);
            Set(worldScript, "_ma", ma);
            Set(worldScript, "_servant", servant);
            Set(worldScript, "_observe", s.Observe);
            Set(worldScript, "_path", path.ToArray());
            Set(worldScript, "_paper", paperTarget);
            Set(worldScript, "_bench", bench);
            Set(worldScript, "_maBehindWall", maWall);
            Set(worldScript, "_servantBehindWall", servantWall);
            Set(worldScript, "_gatePlayer", gatePlayer);
            Set(worldScript, "_gateMa", gateMa);
            Set(worldScript, "_gateServant", gateServant);
            Set(worldScript, "_sun", sun);
            Set(worldScript, "_nightLights", nightLights);
            Lighting(worldScript);

            // Story sequences
            var sequences = new GameObject("Sequences").transform;
            sequences.SetParent(s.Root.transform, false);
            var tutorial = Child<TutorialSequence>(sequences, "tutorial_road");
            Set(tutorial, "_id", "tutorial_road");
            Set(tutorial, "_body", "<color=#E8C27A>Đi theo Mã Giám Sinh</color> và tìm hiểu hắn — nhưng đừng để hắn sinh nghi.\n\n" +
                "<color=#E8C27A>Giữ chuột phải</color>: quan sát kỹ. Giữ tâm nhìn lên mục tiêu cho đến khi đầy.\n" +
                "<color=#E8C27A>E</color>: bắt chuyện, nhặt đồ.   <color=#E8C27A>Ctrl / C</color>: khom người, đi rón rén.\n" +
                "<color=#E8C27A>J</color>: sổ tay manh mối.\n\nThanh <color=#F08080>Nghi ngờ</color> tăng khi bạn tụt lại quá xa, hoặc nhìn chằm chằm khi hắn đang nhìn lại.");
            var document = Child<DocumentSequence>(sequences, "document_paper");
            Set(document, "_id", "document_paper");
            Set(document, "_view", s.Hud.GetComponent<DocumentView>());
            Set(document, "_title", "Mẩu giấy gấp tư");
            Set(document, "_body", "Gửi Bà T. ở Lâm Tri,\n\nHàng đã nhận, giao trong ba ngày.\nNhan sắc hơn người, cầm kỳ thi họa đều thạo.\n\nGiá: <b>bốn trăm năm mươi lạng</b>.\nTiền trao — người giao.\n\n— Mã");
            Set(document, "_sealed", true);
            Set(document, "_prop", paper);
            var eavesdrop = Child<EavesdropSequence>(sequences, "eavesdrop_c2");
            Set(eavesdrop, "_id", "eavesdrop_c2");
            Set(eavesdrop, "_zone", listenZone);
            Set(eavesdrop, "_start", bench);
            Set(eavesdrop, "_sensors", new Object[] { maSensor, servantSensor });
            Set(eavesdrop, "_glancers", new Object[] { ma, servant });
            Set(eavesdrop, "_glanceTarget", glanceTarget);
            Set(eavesdrop, "_seconds", 7f);
            Set(eavesdrop, "_objective", "Khom người sát vách tre và nghe lén cuộc nói chuyện");
            Set(eavesdrop, "_caughtDetail", "Mã Giám Sinh liếc qua vách tre và thấy bạn. Giữ người thấp dưới tấm phên đan.");
            Fx(sequences, "memory_flash", "memory", "memory", null, 1.8f, 0.25f, 1);
            Fx(sequences, "imagine_run", "imagine", "heartbeat", null, 1.2f, 0f, 1);
            var deduction = Child<DeductionSequence>(sequences, "deduction_c2");
            Set(deduction, "_id", "deduction_c2");
            Set(deduction, "_view", s.Hud.GetComponent<DeductionView>());

            Save(scene, ScenePath);
        }

        private static VisionSensor Sensor(GameObject owner, ThuyKieu.Player.PlayerMovement player, float range, float angle, bool restricted)
        {
            var eyes = Point("Eyes", owner.transform.position + Vector3.up * 1.6f, owner.transform.eulerAngles.y, owner.transform);
            var sensor = owner.AddComponent<VisionSensor>();
            Set(sensor, "_eyes", eyes);
            Set(sensor, "_player", player);
            Set(sensor, "_range", range);
            Set(sensor, "_angle", angle);
            Set(sensor, "_secondsToSpot", 1.2f);
            Set(sensor, "_onlyInRestrictedZones", restricted);
            Set(sensor, "_occluders", 1 << EnvironmentLayer);
            sensor.Active = false;
            return sensor;
        }

        private static void Fx(Transform parent, string id, string mood, string sfx, string music, float hold, float shake, int repeat)
        {
            var fx = Child<FxSequence>(parent, id);
            Set(fx, "_id", id);
            Set(fx, "_mood", mood);
            Set(fx, "_sfx", sfx);
            Set(fx, "_music", music ?? string.Empty);
            Set(fx, "_hold", hold);
            Set(fx, "_shake", shake);
            Set(fx, "_repeatSfx", repeat);
        }

        private static void Lighting(Chapter02World world)
        {
            var serialized = new SerializedObject(world);
            var list = serialized.FindProperty("_lighting");
            var presets = new (string scene, Color sun, float intensity, Vector3 angles, Color ambient, Color fog, float density, Material sky)[]
            {
                ("Road_Departure", new Color(1f, 0.64f, 0.4f), 1.3f, new Vector3(16, -62, 0), new Color(0.42f, 0.36f, 0.38f), new Color(0.72f, 0.58f, 0.5f), 0.009f, DuskSky()),
                ("Road_RestStop", new Color(1f, 0.44f, 0.26f), 0.85f, new Vector3(5, -74, 0), new Color(0.36f, 0.29f, 0.36f), new Color(0.5f, 0.36f, 0.38f), 0.012f, Sky("Ch2SkyLate", "evening_road_01_puresky", 0.6f, new Color(0.55f, 0.45f, 0.45f), 40f)),
                ("Road_LamTriGate", new Color(0.52f, 0.64f, 1f), 0.38f, new Vector3(42, 30, 0), new Color(0.12f, 0.14f, 0.22f), new Color(0.06f, 0.075f, 0.12f), 0.018f, Sky("Ch2SkyNight", "kloppenheim_02_puresky", 0.9f, new Color(0.45f, 0.5f, 0.62f), 0f))
            };
            list.arraySize = presets.Length;
            for (int i = 0; i < presets.Length; i++)
            {
                var element = list.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("Scene").stringValue = presets[i].scene;
                element.FindPropertyRelative("Sun").colorValue = presets[i].sun;
                element.FindPropertyRelative("Intensity").floatValue = presets[i].intensity;
                element.FindPropertyRelative("Angles").vector3Value = presets[i].angles;
                element.FindPropertyRelative("Ambient").colorValue = presets[i].ambient;
                element.FindPropertyRelative("Fog").colorValue = presets[i].fog;
                element.FindPropertyRelative("FogDensity").floatValue = presets[i].density;
                element.FindPropertyRelative("Sky").objectReferenceValue = presets[i].sky;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            RenderSettings.ambientLight = presets[0].ambient;
            RenderSettings.fogColor = presets[0].fog;
            RenderSettings.fogDensity = presets[0].density;
        }

        // ------------------------------------------------------------------ environment

        private static Material DuskSky() => Sky("Ch2SkyDusk", "evening_road_01_puresky", 0.85f, new Color(0.58f, 0.48f, 0.42f), 40f);

        /// <summary>Horizontal distance from a ground point to the road centreline (good enough near the road).</summary>
        private static float RoadDistance(Vector2 p) => Mathf.Abs(p.x - RoadX(p.y));

        private static void Ground(Transform world)
        {
            // Ground reaches far past the play corridor so the horizon is land and haze, never an edge.
            var grass = Surface("Ch2Grass", "leafy_grass", new Color(0.78f, 0.82f, 0.62f), 3.2f, 0.12f, 1f, 0.45f);
            Block("Ground", new Vector3(0, -0.25f, 75), new Vector3(420, 0.5f, 460), grass, world);

            // Red-earth road as generated ribbons following the curve: a worn grassy verge under the earth track.
            var earth = Surface("Ch2RoadEarth", "brown_mud_dry", new Color(0.95f, 0.72f, 0.58f), 2.6f, 0.08f, 1.1f, 0.35f);
            var verge = Surface("Ch2RoadVerge", "grass_path_2", new Color(0.86f, 0.8f, 0.66f), 3f, 0.1f, 1f, 0.4f);
            Ribbon(world, "RoadVerge", 0.008f, 1.15f, verge);
            Ribbon(world, "Road", 0.014f, 0f, earth);

            // Rice paddies on the west with low earthen dikes, water catching the dusk sky.
            var water = Material("Ch2PaddyWater", new Color(0.2f, 0.25f, 0.26f), 0.96f);
            var dike = Surface("Ch2Dike", "brown_mud_02", new Color(0.85f, 0.78f, 0.65f), 1.6f, 0.15f);
            for (int row = 0; row < 7; row++)
                for (int col = 0; col < 3; col++)
                {
                    float z = 6 + row * 17f;
                    float x = -26 - col * 13f;
                    Block("Paddy", new Vector3(x, 0.02f, z), new Vector3(12f, 0.04f, 16f), water, world, false);
                    Block("Dike", new Vector3(x + 6.2f, 0.1f, z), new Vector3(0.5f, 0.2f, 16.4f), dike, world, false);
                    Block("Dike", new Vector3(x, 0.1f, z + 8.2f), new Vector3(12.4f, 0.2f, 0.5f), dike, world, false);
                }
            // Young rice standing in the water (a few fields lie fallow).
            var rice = new GameObject("Rice").transform;
            rice.SetParent(world, false);
            Grass("Ch2Rice", rice, new Rect(-70, -2, 50, 122), 9000, p =>
            {
                float cx = Mathf.Repeat(p.x + 32f, 13f), cz = Mathf.Repeat(p.y + 2f, 17f);
                int field = Mathf.FloorToInt((p.x + 32f) / 13f) * 31 + Mathf.FloorToInt((p.y + 2f) / 17f);
                return p.x < -19.6f && cx > 0.6f && cx < 12.2f && cz > 0.6f && cz < 16.2f && Mathf.Abs(field % 4) != 1;
            }, 12, 0.55f, new Color(0.55f, 0.72f, 0.28f), new Color(0.68f, 0.76f, 0.32f), "RiceBlades");

            // Rolling hills in a ring, hazed by fog, so every direction has a horizon with depth.
            var hill = Surface("Ch2Hill", "leafy_grass", new Color(0.42f, 0.5f, 0.4f), 30f, 0.05f, 0.4f, 0.6f);
            var random = new System.Random(4);
            for (int i = 0; i < 26; i++)
            {
                float angle = i / 26f * Mathf.PI * 2f + (float)random.NextDouble() * 0.2f;
                float radius = 150f + (float)random.NextDouble() * 70f;
                float sx = 50f + (float)random.NextDouble() * 60f, sy = 14f + (float)random.NextDouble() * 22f;
                var mound = Sphere("Hill", new Vector3(Mathf.Cos(angle) * radius, -sy * 0.3f, 75f + Mathf.Sin(angle) * radius * 1.1f),
                    new Vector3(sx, sy, sx * 0.8f), hill, world);
                mound.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            // Invisible side walls keep the player near the road corridor.
            var wall = Material("Ch2Invisible", new Color(0, 0, 0, 0));
            foreach (int side in new[] { -1, 1 })
                for (float z = -10; z < GateZ + 6; z += 8)
                {
                    var w = Block("Boundary", new Vector3(RoadX(z + 4) + side * 15f, 1.5f, z + 4), new Vector3(0.5f, 3f, 8.4f), wall, world);
                    Object.DestroyImmediate(w.GetComponent<MeshRenderer>());
                    w.layer = 0; // blocks the player only, never the camera
                }
            var start = Block("BoundaryStart", new Vector3(RoadX(-9), 1.5f, -9.5f), new Vector3(32f, 3f, 0.5f), wall, world);
            Object.DestroyImmediate(start.GetComponent<MeshRenderer>());
            start.layer = 0;
        }

        /// <summary>Ground-hugging strip following the road curve; <paramref name="extra"/> widens it for the verge.</summary>
        private static void Ribbon(Transform world, string name, float height, float extra, Material material)
        {
            var road = new GameObject(name);
            road.transform.SetParent(world, false);
            var mesh = new Mesh { name = name + "Ribbon" };
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            for (float z = -10; z <= GateZ + 6; z += 1f)
            {
                float x = RoadX(z);
                Vector3 tangent = new Vector3(RoadX(z + 0.5f) - RoadX(z - 0.5f), 0, 1).normalized;
                Vector3 side = Vector3.Cross(Vector3.up, tangent);
                // Irregular edges: two incommensurate wobbles per side.
                float left = 1.9f + extra + 0.25f * Mathf.Sin(z * 0.37f) + 0.18f * Mathf.Sin(z * 1.31f + extra);
                float right = 1.9f + extra + 0.25f * Mathf.Sin(z * 0.37f) + 0.18f * Mathf.Sin(z * 0.97f + 2f + extra);
                vertices.Add(new Vector3(x, height, z) - side * left);
                vertices.Add(new Vector3(x, height, z) + side * right);
                uvs.Add(new Vector2(0, z * 0.25f));
                uvs.Add(new Vector2(1, z * 0.25f));
                int i = vertices.Count - 4;
                if (i >= 0) triangles.AddRange(new[] { i, i + 2, i + 1, i + 1, i + 2, i + 3 });
            }
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, MaterialFolder + "/Ch2" + name + "Ribbon.asset");
            road.AddComponent<MeshFilter>().sharedMesh = mesh;
            road.AddComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static void Vegetation(Transform world)
        {
            var random = new System.Random(22);
            var root = new GameObject("Vegetation").transform;
            root.SetParent(world, false);
            string[] plants = { "Assets/_Game/Prefabs/Vegetation/Bamboo.prefab", "Assets/_Game/Prefabs/Vegetation/Bamboo.prefab", "Assets/_Game/Prefabs/Vegetation/Bamboo.prefab",
                "Assets/_Game/Prefabs/Props/CayNhan.prefab", "Assets/_Game/Prefabs/Vegetation/BananaTree.prefab" };
            void Plant(string path, Vector3 position, float yaw, float scale)
            {
                var plant = Prop(path, position, yaw, scale, root);
                if (plant == null) return;
                foreach (var t in plant.GetComponentsInChildren<Transform>()) t.gameObject.layer = 0;
                // The tree models are very dense: favour coarser mesh LODs, and only trees near the road cast shadows.
                bool nearRoad = Mathf.Abs(position.x - RoadX(position.z)) < 12f;
                foreach (var renderer in plant.GetComponentsInChildren<Renderer>())
                {
                    renderer.meshLodSelectionBias = 2f;
                    if (!nearRoad) renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }
            for (float z = -8; z < GateZ - 6; z += 2.6f)
            {
                foreach (int side in new[] { -1, 1 })
                {
                    if (side == 1 && Mathf.Abs(z - TeaZ) < 12) continue;
                    if (side == -1 && random.NextDouble() < 0.35) continue; // gaps open the view onto the paddies
                    float offset = 4.2f + (float)random.NextDouble() * 7f;
                    Plant(plants[random.Next(plants.Length)], new Vector3(RoadX(z) + side * offset, 0, z + (float)random.NextDouble() * 2f), (float)random.NextDouble() * 360f, 0.8f + (float)random.NextDouble() * 0.6f);
                }
            }
            // A deeper grove on the east so the view past the road is woods rather than open lawn.
            for (float z = -20; z < GateZ + 10; z += 3.4f)
                for (int layer = 0; layer < 2; layer++)
                {
                    if (Mathf.Abs(z - TeaZ) < 8 && layer == 0) continue;
                    float offset = 14f + layer * 9f + (float)random.NextDouble() * 8f;
                    Plant(plants[random.Next(plants.Length)], new Vector3(RoadX(z) + offset, 0, z + (float)random.NextDouble() * 3f), (float)random.NextDouble() * 360f, 0.9f + (float)random.NextDouble() * 0.7f);
                }
            // Lone longans and banana clumps on the paddy dikes to the west.
            for (int i = 0; i < 18; i++)
                Plant(plants[3 + random.Next(2)], new Vector3(-20f - (float)random.NextDouble() * 45f, 0, (float)random.NextDouble() * 120f), random.Next(360), 0.9f + (float)random.NextDouble() * 0.5f);

            var fence = "Assets/_Game/Prefabs/Architecture/Fence.prefab";
            for (float z = 20; z < 60; z += 3f) Prop(fence, new Vector3(RoadX(z) - 3.2f, 0, z), 0, 1f, root);

            // Boulders along the verges, half sunk into the ground.
            for (int i = 0; i < 34; i++)
            {
                float z = (float)random.NextDouble() * GateZ;
                float size = 0.5f + (float)random.NextDouble() * 0.9f;
                Rock(new Vector3(RoadX(z) + (random.NextDouble() < 0.5 ? -1 : 1) * (2.8f + (float)random.NextDouble() * 3f), -0.05f, z),
                    new Vector3(size * (1f + (float)random.NextDouble() * 0.6f), size * 0.7f, size), random.Next(360), root, i);
            }

            // Grass: dense beside the road, thinning out into the fields.
            var grass = new GameObject("Grass").transform;
            grass.SetParent(root, false);
            var grassRandom = new System.Random(9);
            Grass("Ch2Grass", grass, new Rect(-30, -14, 75, GateZ + 10), 26000, p =>
            {
                float d = RoadDistance(p);
                if (d < 2.3f) return false;                                                // the earth track stays bare
                if (p.x < -19.4f && p.x > -72f && p.y > -3f && p.y < 126f) return false;   // paddies
                float fromRoad = p.x - RoadX(p.y);
                if (Mathf.Abs(p.y - TeaZ) < 5f && fromRoad > 1.5f && fromRoad < 10f) return false; // tea house floor
                if (Mathf.Abs(p.y - GateZ) < 3f) return false;                               // gate threshold
                return grassRandom.NextDouble() < Mathf.Lerp(1f, 0.25f, Mathf.InverseLerp(2.5f, 18f, d));
            }, 31, 0.5f);

            // Leaves drifting out of the bamboo along the road, and warm motes in the low sun.
            for (float z = 10; z < GateZ; z += 30f)
                Leaves("FallingLeaves", new Vector3(RoadX(z), 4f, z), new Vector3(14f, 2f, 20f), root, new Color(0.62f, 0.6f, 0.28f), 24);
            Dust("DuskMotes", new Vector3(RoadX(40), 1.8f, 40), new Vector3(12f, 3f, 90f), root, new Color(1f, 0.85f, 0.6f, 0.35f), 220);
        }

        /// <summary>Night-only life at the Lâm Tri end: fireflies over the verges, toggled with the night lights.</summary>
        private static GameObject[] AddNightLife(Transform world, GameObject[] nightLights)
        {
            var list = new List<GameObject>(nightLights);
            for (float z = GateZ - 40; z < GateZ; z += 20f)
                list.Add(Fireflies("Fireflies", new Vector3(RoadX(z), 1.2f, z), new Vector3(20f, 1.6f, 18f), world, 50).gameObject);
            return list.ToArray();
        }

        private static Vector3 TeaHouse(Transform world, out Transform bench, out BoxCollider listenZone)
        {
            Vector3 c = new Vector3(RoadX(TeaZ) + 5.2f, 0, TeaZ);
            var root = new GameObject("TeaHouse").transform;
            root.SetParent(world, false);
            var wood = Surface("Ch2TeaWood", "dark_wood", new Color(0.9f, 0.75f, 0.62f), 1.4f, 0.25f, 1f, 0.3f, 0.4f);
            var thatch = Surface("Ch2Thatch", "leafy_grass", new Color(1f, 0.82f, 0.5f), 1.1f, 0.05f, 1.6f, 0.5f);
            var bamboo = Surface("Ch2BambooPole", "planks_brown_10", new Color(1f, 0.92f, 0.6f), 0.7f, 0.3f);
            var weave = Surface("Ch2Weave", "planks_brown_10", new Color(0.95f, 0.82f, 0.52f), 0.45f, 0.15f, 1.4f);
            var stone = Surface("Ch2Stone", "japanese_stone_wall", new Color(0.82f, 0.8f, 0.76f), 1.8f, 0.15f, 1f, 0.4f);
            Block("Platform", c + new Vector3(0, 0.08f, 0), new Vector3(5.6f, 0.16f, 5f), wood, root);
            Block("PlatformFootings", c + new Vector3(0, 0.02f, 0), new Vector3(5.9f, 0.08f, 5.3f), stone, root, false);
            foreach (var p in new[] { new Vector3(-2.6f, 0, -2.3f), new Vector3(2.6f, 0, -2.3f), new Vector3(-2.6f, 0, 2.3f), new Vector3(2.6f, 0, 2.3f) })
                StoryArtKit.Pillar("Post", c + p, 0.09f, 2.5f, bamboo, stone, root);
            Roof("ThatchRoof", c + new Vector3(0, 3.35f, 0), 6.4f, 5.8f, 28f, thatch, wood, root, false);
            Prop("Assets/_Game/Prefabs/Props/TableSet.prefab", c + new Vector3(-0.3f, 0.16f, 0.4f), 90, 0.9f, root);
            Prop("Assets/_Game/Prefabs/Props/CeramicJar.prefab", c + new Vector3(2.1f, 0.16f, 1.8f), 0, 1f, root);
            Prop("Assets/_Game/Prefabs/Props/CeramicJar.prefab", c + new Vector3(2.3f, 0.16f, 1.2f), 40, 0.8f, root);
            Lantern("TeaLantern", c + new Vector3(-2.4f, 2.3f, -2.1f), new Color(1f, 0.55f, 0.25f), root, 9f, 2.6f);
            Lantern("ScreenLantern", c + new Vector3(2.9f, 2.1f, 1.6f), new Color(1f, 0.5f, 0.22f), root, 7f, 2f);
            Block("Signboard", c + new Vector3(-2.75f, 2.0f, 0), new Vector3(0.06f, 0.5f, 1.4f), Material("Ch2Sign", new Color(0.55f, 0.12f, 0.08f)), root, false);
            for (int i = 0; i < 3; i++)
                Block("SignGlyph", c + new Vector3(-2.79f, 2.0f, -0.4f + i * 0.4f), new Vector3(0.01f, 0.24f, 0.2f), Emissive("CoupletGold", new Color(0.85f, 0.62f, 0.22f), 0.25f), root, false);

            // The working corner of the tea house: a clay stove with a kettle, firewood, water jars and stools.
            var clay = Surface("Ch2Clay", "brown_mud_02", new Color(0.82f, 0.55f, 0.42f), 0.8f, 0.1f);
            Cylinder("Stove", c + new Vector3(1.6f, 0.16f, -1.7f), 0.36f, 0.55f, clay, root);
            Cylinder("StoveMouth", c + new Vector3(1.6f, 0.26f, -2.02f), 0.14f, 0.2f, Emissive("Embers", new Color(1f, 0.35f, 0.08f), 2f), root, false);
            StoryArtKit.Sphere("Kettle", c + new Vector3(1.6f, 0.82f, -1.7f), new Vector3(0.32f, 0.26f, 0.32f), Material("Ch2Kettle", new Color(0.12f, 0.1f, 0.09f), 0.6f), root);
            for (int i = 0; i < 6; i++)
            {
                var log = Cylinder("Firewood", c + new Vector3(2.35f, 0.2f + (i / 3) * 0.14f, -1.2f - (i % 3) * 0.14f), 0.06f, 0.7f, wood, root, false);
                log.transform.rotation = Quaternion.Euler(0, 0, 90);
                log.transform.position = c + new Vector3(2.35f, 0.23f + (i / 3) * 0.13f, -1.15f - (i % 3) * 0.14f);
            }
            StoryArtKit.Jars(c + new Vector3(-2.2f, 0.16f, 1.9f), 3, root, 7);
            foreach (var p in new[] { new Vector3(-1.2f, 0, 1.6f), new Vector3(0.7f, 0, 1.7f), new Vector3(-1.5f, 0, -0.4f) })
                Cylinder("Stool", c + p + Vector3.up * 0.16f, 0.17f, 0.36f, bamboo, root, false);
            // Stepping stones from the road to the platform.
            for (int i = 0; i < 4; i++)
                Rock(new Vector3(RoadX(TeaZ) + 2.2f + i * 0.75f, -0.08f, TeaZ - 0.3f + (i % 2) * 0.3f), new Vector3(0.55f, 0.18f, 0.45f), i * 47f, root, i + 2);
            Dust("TeaHouseMotes", c + new Vector3(0, 1.6f, 0), new Vector3(5f, 2f, 4.5f), root, new Color(1f, 0.8f, 0.55f, 0.4f), 60);
            TeaHouseDetails(world, root, c, wood, thatch, bamboo, weave, stone, clay);
            RoadsideLife(world);

            // Bench where Kiều is told to sit (the c2_eavesdrop interaction).
            Block("Bench", c + new Vector3(-1.6f, 0.4f, -1.5f), new Vector3(1.6f, 0.12f, 0.45f), wood, root);
            var benchAnchor = new GameObject("BenchSeat");
            benchAnchor.transform.SetParent(root, false);
            benchAnchor.transform.position = c + new Vector3(-1.6f, 0.2f, -2.1f);
            benchAnchor.transform.rotation = Quaternion.Euler(0, 90, 0);
            bench = benchAnchor.transform;
            var director = Object.FindAnyObjectByType<StoryDirector>();
            Explore(benchAnchor, director, "c2_eavesdrop", "Nghỉ chân ở quán nước", new Vector3(2f, 1.4f, 1.6f));

            // Woven bamboo screen: solid to 1.15 m (hides a crouching listener), poles above it.
            float wallX = c.x + 3.6f;
            Block("ScreenWeave", new Vector3(wallX, 0.575f, c.z), new Vector3(0.12f, 1.15f, 8f), weave, root);
            for (float z = -4f; z <= 4f; z += 0.32f) Cylinder("ScreenPole", new Vector3(wallX, 0, c.z + z), 0.035f, 2.3f, bamboo, root, false);
            Block("ScreenRail", new Vector3(wallX, 2.1f, c.z), new Vector3(0.08f, 0.08f, 8f), bamboo, root, false);

            var zone = new GameObject("ListenZone");
            zone.transform.SetParent(root, false);
            zone.transform.position = new Vector3(wallX - 0.6f, 0.8f, c.z);
            zone.layer = 2;
            listenZone = zone.AddComponent<BoxCollider>();
            listenZone.isTrigger = true;
            listenZone.size = new Vector3(1.0f, 1.6f, 6f);
            return c;
        }

        /// <summary>Small signs of people along the road: a roadside shrine, haystacks, a cart and a milestone.</summary>
        private static void RoadsideLife(Transform world)
        {
            var root = new GameObject("RoadsideLife").transform;
            root.SetParent(world, false);
            var stone = Surface("Ch2Stone", "japanese_stone_wall", new Color(0.82f, 0.8f, 0.76f), 1.8f, 0.15f, 1f, 0.4f);
            var tiles = Surface("Ch2GateTiles", "clay_roof_tiles_02", new Color(0.55f, 0.42f, 0.38f), 1.4f, 0.25f, 1.2f, 0.3f);
            var red = Surface("Ch2GateRed", "lacquered_cherry_wood", new Color(1f, 0.42f, 0.36f), 1.2f, 0.45f);
            var straw = Surface("Ch2Straw", "leafy_grass", new Color(1f, 0.85f, 0.5f), 0.9f, 0.05f, 1.6f, 0.5f);
            var wood = Surface("Ch2TeaWood", "dark_wood", new Color(0.9f, 0.75f, 0.62f), 1.4f, 0.25f, 1f, 0.3f, 0.4f);

            // Miếu thổ địa: a tiny shrine on a stone base with incense glowing in front.
            float sz = 64f;
            Vector3 shrine = new Vector3(RoadX(sz) + 3.6f, 0, sz);
            StoryArtKit.Plinth("ShrineBase", shrine, new Vector3(1.1f, 0.7f, 0.9f), stone, stone, root);
            Block("ShrineBody", shrine + new Vector3(0, 1.05f, 0.1f), new Vector3(0.8f, 0.7f, 0.6f), red, root, false);
            Block("ShrineNiche", shrine + new Vector3(0, 1.0f, -0.21f), new Vector3(0.4f, 0.42f, 0.02f), Material("ShrineDark", new Color(0.05f, 0.03f, 0.02f)), root, false);
            Roof("ShrineRoof", shrine + new Vector3(0, 1.62f, 0.1f), 1.1f, 0.9f, 28f, tiles, Material("Ch2RoofRidge", new Color(0.2f, 0.12f, 0.1f)), root);
            for (int i = -1; i <= 1; i++)
            {
                Cylinder("Incense", shrine + new Vector3(i * 0.05f, 0.7f, -0.38f), 0.006f, 0.22f, Material("IncenseStick", new Color(0.55f, 0.15f, 0.1f)), root, false);
                StoryArtKit.Sphere("IncenseTip", shrine + new Vector3(i * 0.05f, 0.925f, -0.38f), Vector3.one * 0.015f, Emissive("IncenseGlow", new Color(1f, 0.3f, 0.05f), 4f), root);
            }
            Cylinder("IncenseBowl", shrine + new Vector3(0, 0.7f, -0.38f), 0.09f, 0.06f, Material("Ch2Kettle", new Color(0.12f, 0.1f, 0.09f), 0.6f), root, false);

            // Haystacks by the paddies.
            foreach (var p in new[] { new Vector3(-18.5f, 0, 18f), new Vector3(-18.2f, 0, 21.5f), new Vector3(-19f, 0, 74f), new Vector3(-31f, 0, 103f) })
            {
                Cylinder("Haystack", p, 0.9f, 1.2f, straw, root);
                StoryArtKit.Sphere("HaystackTop", p + Vector3.up * 1.2f, new Vector3(1.8f, 1.3f, 1.8f), straw, root);
                Cylinder("HayPole", p, 0.04f, 2.6f, wood, root, false);
            }

            // An ox cart left by the tea house.
            Vector3 cart = new Vector3(RoadX(TeaZ - 9f) + 4.2f, 0, TeaZ - 9f);
            var bed = Block("CartBed", cart + new Vector3(0, 0.75f, 0), new Vector3(1.3f, 0.12f, 2.4f), wood, root, true, 12f);
            Block("CartSide", cart + bed.transform.right * -0.62f + Vector3.up * 0.95f, new Vector3(0.06f, 0.3f, 2.4f), wood, root, false, 12f);
            Block("CartSide", cart + bed.transform.right * 0.62f + Vector3.up * 0.95f, new Vector3(0.06f, 0.3f, 2.4f), wood, root, false, 12f);
            foreach (int side in new[] { -1, 1 })
            {
                var wheel = Cylinder("CartWheel", cart + bed.transform.right * side * 0.75f + new Vector3(0, 0.55f, 0), 0.55f, 0.08f, wood, root, false);
                wheel.transform.position = cart + bed.transform.right * side * 0.75f + Vector3.up * 0.55f;
                wheel.transform.rotation = bed.transform.rotation * Quaternion.Euler(0, 0, 90);
            }
            var shaft = Block("CartShaft", cart + bed.transform.forward * 1.9f + Vector3.up * 0.45f, new Vector3(0.08f, 0.08f, 1.8f), wood, root, false, 12f);
            shaft.transform.rotation = bed.transform.rotation * Quaternion.Euler(-14, 0, 0);
            StoryArtKit.Sphere("Sack", cart + Vector3.up * 0.98f + bed.transform.forward * -0.4f, new Vector3(0.55f, 0.35f, 0.45f), Material("Sackcloth", new Color(0.62f, 0.52f, 0.38f)), root);
            StoryArtKit.Sphere("Sack", cart + Vector3.up * 0.98f + bed.transform.forward * 0.3f, new Vector3(0.5f, 0.32f, 0.42f), Material("Sackcloth", new Color(0.62f, 0.52f, 0.38f)), root);

            // A stone milestone half-way.
            Block("Milestone", new Vector3(RoadX(30f) - 2.7f, 0.35f, 30f), new Vector3(0.3f, 0.7f, 0.18f), stone, root);
        }

        private static void TeaHouseDetails(Transform world, Transform root, Vector3 c, Material wood, Material thatch, Material bamboo, Material weave, Material stone, Material clay)
        {
            var ceramic = Material("Ch2Ceramic", new Color(0.84f, 0.8f, 0.72f), 0.65f);
            var darkCeramic = Material("Ch2DarkCeramic", new Color(0.38f, 0.24f, 0.16f), 0.55f);
            var cloth = Material("Ch2BannerCloth", new Color(0.76f, 0.64f, 0.44f), 0.15f);
            var bannerRed = Material("Ch2BannerTrim", new Color(0.58f, 0.14f, 0.1f), 0.2f);

            // Extra middle bamboo posts, roof beams, rafters and hanging bamboo sun-blind (mành tre).
            foreach (var p in new[] { new Vector3(-2.6f, 0, 0f), new Vector3(2.6f, 0, 0f), new Vector3(0f, 0, 2.3f) })
                StoryArtKit.Pillar("PostMid", c + p, 0.085f, 2.5f, bamboo, stone, root);
            foreach (float z in new[] { -2.3f, 0f, 2.3f })
                Block("Crossbeam", c + new Vector3(0, 2.48f, z), new Vector3(5.4f, 0.09f, 0.09f), bamboo, root, false);
            foreach (float x in new[] { -2.6f, 0f, 2.6f })
                Block("Purlin", c + new Vector3(x, 2.56f, 0), new Vector3(0.09f, 0.09f, 4.8f), bamboo, root, false);
            Block("ManhTre", c + new Vector3(-2.62f, 2.12f, 0.6f), new Vector3(0.03f, 0.68f, 2.1f), weave, root, false);
            Cylinder("ManhTreRoll", c + new Vector3(-2.64f, 1.76f, -0.45f), 0.045f, 2.1f, bamboo, root, false).transform.rotation = Quaternion.Euler(90, 0, 0);

            // Half-height woven bamboo walls (vách liếp tre) on the north and east-back corners to shelter the stall.
            Block("NorthWeaveWall", c + new Vector3(-0.3f, 0.68f, 2.32f), new Vector3(4.6f, 1.05f, 0.06f), weave, root);
            Block("NorthWallRail", c + new Vector3(-0.3f, 1.22f, 2.32f), new Vector3(4.65f, 0.06f, 0.08f), bamboo, root, false);
            Block("EastCornerWeave", c + new Vector3(2.6f, 0.68f, -1.15f), new Vector3(0.06f, 1.05f, 2.3f), weave, root);
            Block("EastCornerRail", c + new Vector3(2.6f, 1.22f, -1.15f), new Vector3(0.08f, 0.06f, 2.35f), bamboo, root, false);

            // Wooden tea counter (quầy hàng nước) with shelves, woven trays (mẹt tre), teapot and cups.
            Block("TeaCounter", c + new Vector3(1.55f, 0.52f, 0.35f), new Vector3(0.75f, 0.72f, 1.85f), wood, root);
            Block("TeaCounterTop", c + new Vector3(1.55f, 0.9f, 0.35f), new Vector3(0.84f, 0.05f, 1.95f), wood, root, false);
            Cylinder("MetTreA", c + new Vector3(1.52f, 0.93f, 0.75f), 0.28f, 0.025f, weave, root, false);
            Cylinder("MetTreB", c + new Vector3(1.56f, 0.93f, -0.1f), 0.24f, 0.025f, weave, root, false);
            StoryArtKit.Sphere("AmTraCounter", c + new Vector3(1.5f, 1.02f, 0.72f), new Vector3(0.18f, 0.15f, 0.18f), darkCeramic, root);
            for (int i = 0; i < 4; i++)
                Cylinder("ChenTra", c + new Vector3(1.38f + (i % 2) * 0.12f, 0.95f, 0.52f + (i / 2) * 0.12f), 0.032f, 0.045f, ceramic, root, false);
            for (int i = 0; i < 3; i++)
                StoryArtKit.Sphere("BanhQue", c + new Vector3(1.54f + (i - 1) * 0.08f, 0.97f, -0.1f), new Vector3(0.09f, 0.045f, 0.09f), cloth, root);

            // Traditional bamboo slatted bed (chõng tre) & low tea table with teapot and cups near Kiều's bench.
            Block("ChongTreTop", c + new Vector3(-1.35f, 0.46f, 1.15f), new Vector3(1.55f, 0.06f, 0.92f), bamboo, root);
            foreach (float dx in new[] { -0.68f, 0.68f })
                foreach (float dz in new[] { -0.38f, 0.38f })
                    Cylinder("ChongLeg", c + new Vector3(-1.35f + dx, 0.16f, 1.15f + dz), 0.04f, 0.3f, bamboo, root, false);
            Cylinder("TrayOnChong", c + new Vector3(-1.3f, 0.49f, 1.15f), 0.22f, 0.02f, weave, root, false);
            StoryArtKit.Sphere("TeapotChong", c + new Vector3(-1.32f, 0.56f, 1.15f), new Vector3(0.15f, 0.12f, 0.15f), darkCeramic, root);
            for (int i = 0; i < 3; i++)
                Cylinder("CupChong", c + new Vector3(-1.18f, 0.51f, 1.05f + i * 0.1f), 0.028f, 0.04f, ceramic, root, false);

            // Tea table right next to Kiều's resting bench so she sits at a proper roadside table.
            Block("BenchTeaTable", c + new Vector3(-0.55f, 0.48f, -1.5f), new Vector3(0.65f, 0.04f, 0.55f), wood, root);
            foreach (float dx in new[] { -0.26f, 0.26f })
                foreach (float dz in new[] { -0.21f, 0.21f })
                    Block("BenchTableLeg", c + new Vector3(-0.55f + dx, 0.31f, -1.5f + dz), new Vector3(0.04f, 0.3f, 0.04f), wood, root, false);
            StoryArtKit.Sphere("BenchTeapot", c + new Vector3(-0.55f, 0.56f, -1.48f), new Vector3(0.14f, 0.11f, 0.14f), darkCeramic, root);
            Cylinder("BenchCupA", c + new Vector3(-0.68f, 0.5f, -1.55f), 0.028f, 0.04f, ceramic, root, false);
            Cylinder("BenchCupB", c + new Vector3(-0.42f, 0.5f, -1.42f), 0.028f, 0.04f, ceramic, root, false);

            // Big terracotta rainwater jar (chum nước mưa) with wooden lid and bamboo dipper by the front post.
            StoryArtKit.Sphere("RainwaterJar", c + new Vector3(-2.35f, 0.52f, -2.05f), new Vector3(0.62f, 0.72f, 0.62f), clay, root);
            Cylinder("JarLid", c + new Vector3(-2.35f, 0.88f, -2.05f), 0.24f, 0.04f, wood, root, false);

            // Roadside bamboo sign pole, cloth banner ("TRÀ QUÁN") and warm beacon lantern at the road edge.
            Vector3 signBase = c + new Vector3(-3.35f, 0f, -2.45f);
            Cylinder("RoadSignPole", signBase, 0.055f, 3.3f, bamboo, root, false);
            Block("RoadSignArm", signBase + new Vector3(0.45f, 3.05f, 0f), new Vector3(1.05f, 0.05f, 0.05f), bamboo, root, false);
            Block("RoadBanner", signBase + new Vector3(0.42f, 2.35f, 0f), new Vector3(0.52f, 1.25f, 0.02f), cloth, root, false);
            Block("RoadBannerTop", signBase + new Vector3(0.42f, 2.98f, 0f), new Vector3(0.58f, 0.06f, 0.03f), bannerRed, root, false);
            Block("RoadBannerBottom", signBase + new Vector3(0.42f, 1.72f, 0f), new Vector3(0.58f, 0.06f, 0.03f), bannerRed, root, false);
            Lantern("RoadBeaconLantern", signBase + new Vector3(-0.1f, 2.85f, 0f), new Color(1f, 0.58f, 0.24f), root, 14f, 3.1f);
            Lantern("InteriorWarmLantern", c + new Vector3(0.4f, 2.35f, 0.2f), new Color(1f, 0.62f, 0.28f), root, 10f, 2.2f);

            // Shade tree overarching the tea house and bà hàng nước tending the counter.
            Prop("Assets/_Game/Prefabs/Props/CayNhan.prefab", c + new Vector3(-3.4f, 0f, 2.9f), 35f, 1.15f, root);
            var shared = StoryAnimationSetup.Controller();
            var owner = RiggedActor("BaHangNuoc", "BaHangNuoc", "Assets/_Game/Art/Characters/BaHangNuoc/Models/BaHangNuoc_Rigged.fbx",
                "Assets/_Game/Art/Characters/BaHangNuoc/Textures", 1.56f, c + new Vector3(2.18f, 0.16f, 0.35f), 270f, Color.white, shared);
            owner.transform.SetParent(root, true);
            GroundFeet(owner.GetComponentInChildren<Animator>().transform, owner.gameObject, c.y + 0.16f);
            if (owner.GetComponentInChildren<Animator>() != null)
                owner.GetComponentInChildren<Animator>().gameObject.AddComponent<AnimatorRootAnchor>();
            if (owner.GetComponent<UnityEngine.AI.NavMeshAgent>() != null)
                Object.DestroyImmediate(owner.GetComponent<UnityEngine.AI.NavMeshAgent>());

            // Mid-road rest shelter & signpost at z = 58 so players see a clear rest stop & lantern trail right after the paper drop.
            Vector3 mid = new Vector3(RoadX(58f) + 3.8f, 0f, 58f);
            var wayRoot = new GameObject("TeaWaystation").transform;
            wayRoot.SetParent(world, false);
            Block("WayBench", mid + new Vector3(0f, 0.36f, 0f), new Vector3(1.5f, 0.08f, 0.48f), bamboo, wayRoot);
            foreach (float dx in new[] { -0.6f, 0.6f })
                Cylinder("WayBenchLeg", mid + new Vector3(dx, 0f, 0f), 0.045f, 0.34f, bamboo, wayRoot, false);
            Cylinder("WaySignPole", mid + new Vector3(-1.1f, 0f, -0.8f), 0.05f, 2.4f, bamboo, wayRoot, false);
            Lantern("WayLantern", mid + new Vector3(-1.1f, 2.15f, -0.8f), new Color(1f, 0.56f, 0.24f), wayRoot, 11f, 2.5f);
            StoryArtKit.Jars(mid + new Vector3(0.95f, 0f, 0.3f), 2, wayRoot, 19);
        }

        private static void Gate(Transform world, out GameObject[] nightLights)
        {
            var root = new GameObject("LamTriGate").transform;
            root.SetParent(world, false);
            float x = RoadX(GateZ);
            var red = Surface("Ch2GateRed", "lacquered_cherry_wood", new Color(1f, 0.42f, 0.36f), 1.2f, 0.45f);
            var stone = Surface("Ch2GateStone", "japanese_stone_wall", new Color(0.75f, 0.74f, 0.72f), 2.4f, 0.15f, 1.1f, 0.45f, 0.7f);
            var tiles = Surface("Ch2GateTiles", "clay_roof_tiles_02", new Color(0.55f, 0.42f, 0.38f), 1.4f, 0.25f, 1.2f, 0.3f);
            var ridge = Material("Ch2RoofRidge", new Color(0.2f, 0.12f, 0.1f));
            var planks = Surface("Ch2Door", "planks_brown_10", new Color(0.55f, 0.32f, 0.22f), 1.6f, 0.25f, 1.2f);
            var brass = Material("Ch2Brass", new Color(0.72f, 0.52f, 0.22f), 0.75f);
            StoryArtKit.Pillar("PillarL", new Vector3(x - 2.6f, 0, GateZ), 0.36f, 5f, red, stone, root);
            StoryArtKit.Pillar("PillarR", new Vector3(x + 2.6f, 0, GateZ), 0.36f, 5f, red, stone, root);
            Block("Lintel", new Vector3(x, 4.7f, GateZ), new Vector3(6.6f, 0.6f, 0.8f), red, root);
            Block("Plaque", new Vector3(x, 4.7f, GateZ - 0.42f), new Vector3(2.2f, 0.42f, 0.04f), Material("CoupletRed", new Color(0.45f, 0.05f, 0.04f), 0.55f), root, false);
            for (int i = 0; i < 3; i++)
                Block("PlaqueGlyph", new Vector3(x - 0.6f + i * 0.6f, 4.7f, GateZ - 0.45f), new Vector3(0.3f, 0.26f, 0.01f), Emissive("CoupletGold", new Color(0.85f, 0.62f, 0.22f), 0.25f), root, false);
            Roof("GateRoof", new Vector3(x, 5.9f, GateZ), 7.6f, 2.6f, 26f, tiles, ridge, root);
            Block("Door", new Vector3(x, 2.1f, GateZ), new Vector3(4.5f, 4.2f, 0.2f), planks, root);
            // Brass studs in rows and two ring pulls.
            for (int row = 0; row < 5; row++)
                for (int col = -3; col <= 3; col++)
                    if (col != 0) StoryArtKit.Sphere("Stud", new Vector3(x + col * 0.3f, 0.8f + row * 0.7f, GateZ - 0.11f), Vector3.one * 0.07f, brass, root);
            foreach (int side in new[] { -1, 1 })
            {
                var ring = Cylinder("DoorRing", new Vector3(x + side * 0.28f, 1.9f, GateZ - 0.13f), 0.16f, 0.02f, brass, root, false);
                ring.transform.position = new Vector3(x + side * 0.28f, 1.9f, GateZ - 0.13f);
                ring.transform.rotation = Quaternion.Euler(90, 0, 0);
            }
            Block("DoorSeam", new Vector3(x, 2.1f, GateZ - 0.105f), new Vector3(0.04f, 4.2f, 0.01f), ridge, root, false);
            foreach (int side in new[] { -1, 1 })
            {
                Block(side < 0 ? "WallL" : "WallR", new Vector3(x + side * 14f, 2f, GateZ), new Vector3(22f, 4f, 0.8f), stone, root);
                Block("WallCap", new Vector3(x + side * 14f, 4.12f, GateZ), new Vector3(22.2f, 0.24f, 1.2f), tiles, root, false);
                Block("WallCapRidge", new Vector3(x + side * 14f, 4.28f, GateZ), new Vector3(22.2f, 0.1f, 0.3f), ridge, root, false);
            }
            StoryArtKit.Couplets(new Vector3(x - 2.6f, 2.6f, GateZ - 0.4f), new Vector3(x + 2.6f, 2.6f, GateZ - 0.4f), 0f, root);
            var lights = new List<GameObject>();
            foreach (float side in new[] { -2.6f, 2.6f })
                lights.Add(Lantern("GateLantern", new Vector3(x + side, 3.6f, GateZ - 0.75f), new Color(1f, 0.18f, 0.1f), root, 9f, 2.6f));

            // The pleasure house beyond: two storeys with a balcony, glowing lattice windows and red lanterns.
            var plaster = Surface("Ch2LauPlaster", "plastered_wall_02", new Color(0.55f, 0.42f, 0.4f), 2.5f, 0.1f, 1f, 0.4f, 0.6f);
            var windowGlow = Emissive("Ch2WindowGlow", new Color(1f, 0.55f, 0.25f), 1.2f);
            var frame = Material("Ch2LatticeFrame", new Color(0.18f, 0.06f, 0.04f), 0.4f);
            Vector3 lau = new Vector3(x + 6f, 0, GateZ + 22f);
            Block("LauXanh", lau + new Vector3(0, 3f, 0), new Vector3(16f, 6f, 10f), plaster, root, false);
            Block("LauBalcony", lau + new Vector3(0, 6.1f, -5.6f), new Vector3(16.4f, 0.2f, 1.4f), red, root, false);
            Block("LauRail", lau + new Vector3(0, 6.6f, -6.25f), new Vector3(16.4f, 0.8f, 0.08f), red, root, false);
            Block("LauUpper", lau + new Vector3(0, 8.6f, 0.5f), new Vector3(14f, 5f, 9f), plaster, root, false);
            Roof("LauXanhEave", lau + new Vector3(0, 6.5f, 0), 17.6f, 11.2f, 20f, tiles, ridge, root);
            Roof("LauXanhRoof", lau + new Vector3(0, 12.4f, 0.5f), 15.6f, 10f, 25f, tiles, ridge, root);
            for (int i = 0; i < 6; i++)
            {
                StoryArtKit.Lattice("LauWindow", lau + new Vector3(-6.2f + i * 2.5f, 2.4f, -5.02f), 1.3f, 1.5f, 0f, frame, windowGlow, root);
                if (i < 5) StoryArtKit.Lattice("LauWindowUp", lau + new Vector3(-5f + i * 2.5f, 8.4f, -4.02f), 1.2f, 1.6f, 0f, frame, windowGlow, root);
            }
            for (int i = 0; i < 3; i++)
                StoryArtKit.Drape("LauDrape", lau + new Vector3(-8f + i * 5.4f, 7f, -6.3f), lau + new Vector3(-8f + (i + 1) * 5.4f, 7f, -6.3f), 0.6f,
                    i % 2 == 0 ? new Color(0.75f, 0.12f, 0.2f) : new Color(0.9f, 0.55f, 0.65f), root);
            for (int i = 0; i < 7; i++)
                lights.Add(Lantern("LauLantern", new Vector3(x - 1.5f + i * 2.5f, 7.5f, GateZ + 16.6f), new Color(1f, 0.12f, 0.12f), root, i % 2 == 0 ? 8f : 0f, 2f));
            nightLights = lights.ToArray();
        }
    }
}
