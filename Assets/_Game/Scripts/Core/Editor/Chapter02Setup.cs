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

        [MenuItem("ThuyKieu/Chapter 2/Create playable scene")]
        public static void Create()
        {
            var scene = NewScene(ScenePath);
            StoryAnimationSetup.Controller();
            var s = CreateSystems(2, "Trên đường theo Mã Giám Sinh", "chapter_2", "Chapter03_TuBaHouse",
                new Vector3(RoadX(2), 0.05f, 2), 0f, "road");
            var world = new GameObject("World").transform;
            var nav = NavMesh(world);

            Ground(world);
            Vegetation(world);
            Vector3 tea = TeaHouse(world, out Transform bench, out BoxCollider listenZone);
            Gate(world, out GameObject[] nightLights);

            // Characters
            var shared = StoryAnimationSetup.Controller();
            var ma = RiggedActor("MaGiamSinh", "MaGiamSinh", "Assets/_Game/Art/Characters/MaGiamSinh/Models/MaGiamSinh_Rigged.fbx",
                "Assets/_Game/Art/Characters/MaGiamSinh/Tectures", 1.78f, new Vector3(RoadX(5.5f) + 0.6f, 0, 5.5f), 0f, Color.white, shared);
            var servant = StaticActor("TuyTung", "TuyTung", "Assets/_Game/Art/Characters/LinhCanh/Static/LinhCanh.obj",
                "Assets/_Game/Art/Characters/LinhCanh/Static/texture_pbr_20250901.png", 1.72f, new Vector3(RoadX(4.2f) - 0.9f, 0, 4.2f), 0f);

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
            var presets = new (string scene, Color sun, float intensity, Vector3 angles, Color ambient, Color fog, float density)[]
            {
                ("Road_Departure", new Color(1f, 0.64f, 0.4f), 1.3f, new Vector3(16, -62, 0), new Color(0.42f, 0.34f, 0.36f), new Color(0.78f, 0.56f, 0.46f), 0.011f),
                ("Road_RestStop", new Color(1f, 0.44f, 0.26f), 0.85f, new Vector3(5, -74, 0), new Color(0.36f, 0.28f, 0.36f), new Color(0.46f, 0.3f, 0.34f), 0.015f),
                ("Road_LamTriGate", new Color(0.52f, 0.64f, 1f), 0.38f, new Vector3(42, 30, 0), new Color(0.1f, 0.12f, 0.2f), new Color(0.05f, 0.06f, 0.11f), 0.022f)
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
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            RenderSettings.ambientLight = presets[0].ambient;
            RenderSettings.fogColor = presets[0].fog;
        }

        // ------------------------------------------------------------------ environment

        private static void Ground(Transform world)
        {
            var grass = Material("Ch2Grass", new Color(0.31f, 0.37f, 0.17f));
            Block("Ground", new Vector3(0, -0.25f, 75), new Vector3(120, 0.5f, 210), grass, world);

            // Red-earth road as a generated ribbon mesh following the curve.
            var road = new GameObject("Road");
            road.transform.SetParent(world, false);
            var mesh = new Mesh { name = "RoadRibbon" };
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            for (float z = -10; z <= GateZ + 6; z += 1f)
            {
                float x = RoadX(z);
                Vector3 tangent = new Vector3(RoadX(z + 0.5f) - RoadX(z - 0.5f), 0, 1).normalized;
                Vector3 side = Vector3.Cross(Vector3.up, tangent);
                float width = 1.9f + 0.25f * Mathf.Sin(z * 0.37f);
                vertices.Add(new Vector3(x, 0.012f, z) - side * width);
                vertices.Add(new Vector3(x, 0.012f, z) + side * width);
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
            AssetDatabase.CreateAsset(mesh, MaterialFolder + "/Ch2RoadRibbon.asset");
            road.AddComponent<MeshFilter>().sharedMesh = mesh;
            road.AddComponent<MeshRenderer>().sharedMaterial = Material("Ch2RoadEarth", new Color(0.4f, 0.29f, 0.2f), 0.05f);

            // Rice paddies on the west with low earthen dikes, water catching the dusk sky.
            var water = Material("Ch2PaddyWater", new Color(0.32f, 0.38f, 0.36f), 0.92f);
            var dike = Material("Ch2Dike", new Color(0.33f, 0.28f, 0.17f));
            var rice = Material("Ch2Rice", new Color(0.42f, 0.5f, 0.18f));
            var random = new System.Random(2);
            for (int row = 0; row < 7; row++)
                for (int col = 0; col < 3; col++)
                {
                    float z = 6 + row * 17f;
                    float x = -26 - col * 13f;
                    Block("Paddy", new Vector3(x, 0.02f, z), new Vector3(12f, 0.04f, 16f), water, world, false);
                    Block("Dike", new Vector3(x + 6.2f, 0.1f, z), new Vector3(0.5f, 0.2f, 16.4f), dike, world, false);
                    Block("Dike", new Vector3(x, 0.1f, z + 8.2f), new Vector3(12.4f, 0.2f, 0.5f), dike, world, false);
                    if (random.NextDouble() < 0.55)
                        for (int r = 0; r < 18; r++)
                            Block("RiceTuft", new Vector3(x - 5 + (float)random.NextDouble() * 10, 0.2f, z - 7 + (float)random.NextDouble() * 14), new Vector3(0.25f, 0.4f, 0.25f), rice, world, false);
                }

            // Distant hills, hazed by fog.
            var hill = Material("Ch2Hill", new Color(0.2f, 0.26f, 0.22f));
            foreach (var (x, z, sx, sy) in new[] { (-80f, 40f, 60f, 22f), (-70f, 120f, 70f, 28f), (70f, 90f, 60f, 18f), (40f, 190f, 90f, 30f), (-30f, 200f, 80f, 26f), (80f, 10f, 50f, 16f) })
            {
                var mound = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                mound.name = "Hill";
                Object.DestroyImmediate(mound.GetComponent<Collider>());
                mound.transform.SetParent(world, false);
                mound.transform.position = new Vector3(x, -sy * 0.35f, z);
                mound.transform.localScale = new Vector3(sx, sy, sx * 0.8f);
                mound.GetComponent<Renderer>().sharedMaterial = hill;
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

        private static void Vegetation(Transform world)
        {
            var random = new System.Random(22);
            var root = new GameObject("Vegetation").transform;
            root.SetParent(world, false);
            string[] plants = { "Assets/_Game/Prefabs/Vegetation/Bamboo.prefab", "Assets/_Game/Prefabs/Vegetation/Bamboo.prefab", "Assets/_Game/Prefabs/Vegetation/Bamboo.prefab",
                "Assets/_Game/Prefabs/Vegetation/ArecaPalm.prefab", "Assets/_Game/Prefabs/Vegetation/BananaTree.prefab" };
            for (float z = -8; z < GateZ - 6; z += 2.6f)
            {
                foreach (int side in new[] { -1, 1 })
                {
                    if (side == 1 && Mathf.Abs(z - TeaZ) < 12) continue;
                    if (side == -1 && random.NextDouble() < 0.35) continue; // gaps open the view onto the paddies
                    float offset = 4.2f + (float)random.NextDouble() * 7f;
                    var plant = Prop(plants[random.Next(plants.Length)], new Vector3(RoadX(z) + side * offset, 0, z + (float)random.NextDouble() * 2f), (float)random.NextDouble() * 360f, 0.8f + (float)random.NextDouble() * 0.6f, root);
                    if (plant != null) foreach (var t in plant.GetComponentsInChildren<Transform>()) t.gameObject.layer = 0;
                }
            }
            var fence = "Assets/_Game/Prefabs/Architecture/Fence.prefab";
            for (float z = 20; z < 60; z += 3f) Prop(fence, new Vector3(RoadX(z) - 3.2f, 0, z), 0, 1f, root);
            var stone = Material("Ch2Stone", new Color(0.36f, 0.35f, 0.33f));
            for (int i = 0; i < 26; i++)
            {
                float z = (float)random.NextDouble() * GateZ;
                var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.name = "Rock";
                Object.DestroyImmediate(rock.GetComponent<Collider>());
                rock.transform.SetParent(root, false);
                rock.transform.position = new Vector3(RoadX(z) + (random.NextDouble() < 0.5 ? -1 : 1) * (2.6f + (float)random.NextDouble() * 2f), 0.05f, z);
                rock.transform.localScale = new Vector3(0.5f + (float)random.NextDouble() * 0.7f, 0.3f + (float)random.NextDouble() * 0.3f, 0.5f + (float)random.NextDouble() * 0.6f);
                rock.GetComponent<Renderer>().sharedMaterial = stone;
            }
        }

        private static Vector3 TeaHouse(Transform world, out Transform bench, out BoxCollider listenZone)
        {
            Vector3 c = new Vector3(RoadX(TeaZ) + 5.2f, 0, TeaZ);
            var root = new GameObject("TeaHouse").transform;
            root.SetParent(world, false);
            var wood = Material("Ch2TeaWood", new Color(0.28f, 0.16f, 0.08f));
            var thatch = Material("Ch2Thatch", new Color(0.52f, 0.41f, 0.22f));
            var bamboo = Material("Ch2BambooPole", new Color(0.55f, 0.52f, 0.24f));
            var weave = Material("Ch2Weave", new Color(0.47f, 0.38f, 0.2f));
            Block("Platform", c + new Vector3(0, 0.08f, 0), new Vector3(5.6f, 0.16f, 5f), wood, root);
            foreach (var p in new[] { new Vector3(-2.6f, 0, -2.3f), new Vector3(2.6f, 0, -2.3f), new Vector3(-2.6f, 0, 2.3f), new Vector3(2.6f, 0, 2.3f) })
                Cylinder("Post", c + p, 0.09f, 2.5f, wood, root);
            Roof("ThatchRoof", c + new Vector3(0, 3.35f, 0), 6.4f, 5.8f, 28f, thatch, wood, root, false);
            Prop("Assets/_Game/Prefabs/Props/TableSet.prefab", c + new Vector3(-0.3f, 0.16f, 0.4f), 90, 0.9f, root);
            Prop("Assets/_Game/Prefabs/Props/CeramicJar.prefab", c + new Vector3(2.1f, 0.16f, 1.8f), 0, 1f, root);
            Prop("Assets/_Game/Prefabs/Props/CeramicJar.prefab", c + new Vector3(2.3f, 0.16f, 1.2f), 40, 0.8f, root);
            Lantern("TeaLantern", c + new Vector3(-2.4f, 2.3f, -2.1f), new Color(1f, 0.55f, 0.25f), root, 9f, 2.6f);
            Lantern("ScreenLantern", c + new Vector3(2.9f, 2.1f, 1.6f), new Color(1f, 0.5f, 0.22f), root, 7f, 2f);
            Block("Signboard", c + new Vector3(-2.75f, 2.0f, 0), new Vector3(0.06f, 0.5f, 1.4f), Material("Ch2Sign", new Color(0.55f, 0.12f, 0.08f)), root, false);

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

        private static void Gate(Transform world, out GameObject[] nightLights)
        {
            var root = new GameObject("LamTriGate").transform;
            root.SetParent(world, false);
            float x = RoadX(GateZ);
            var red = Material("Ch2GateRed", new Color(0.45f, 0.08f, 0.06f));
            var stone = Material("Ch2GateStone", new Color(0.3f, 0.28f, 0.26f));
            var tiles = Material("Ch2GateTiles", new Color(0.16f, 0.12f, 0.12f));
            Block("PillarL", new Vector3(x - 2.6f, 2.5f, GateZ), new Vector3(0.7f, 5f, 0.7f), red, root);
            Block("PillarR", new Vector3(x + 2.6f, 2.5f, GateZ), new Vector3(0.7f, 5f, 0.7f), red, root);
            Block("Lintel", new Vector3(x, 4.7f, GateZ), new Vector3(6.6f, 0.6f, 0.8f), red, root);
            Roof("GateRoof", new Vector3(x, 5.9f, GateZ), 7.6f, 2.6f, 26f, tiles, tiles, root);
            Block("Door", new Vector3(x, 2.1f, GateZ), new Vector3(4.5f, 4.2f, 0.2f), Material("Ch2Door", new Color(0.25f, 0.1f, 0.05f)), root);
            Block("WallL", new Vector3(x - 14f, 2f, GateZ), new Vector3(22f, 4f, 0.8f), stone, root);
            Block("WallR", new Vector3(x + 14f, 2f, GateZ), new Vector3(22f, 4f, 0.8f), stone, root);
            var lights = new List<GameObject>();
            foreach (float side in new[] { -2.6f, 2.6f })
                lights.Add(Lantern("GateLantern", new Vector3(x + side, 3.6f, GateZ - 0.6f), new Color(1f, 0.18f, 0.1f), root, 9f, 2.6f));
            // The pleasure house beyond: a tall silhouette ringed with red lanterns.
            var house = Material("Ch2HouseDark", new Color(0.09f, 0.07f, 0.08f));
            Block("LauXanh", new Vector3(x + 6f, 6f, GateZ + 22f), new Vector3(16f, 12f, 10f), house, root, false);
            Roof("LauXanhRoof", new Vector3(x + 6f, 13.2f, GateZ + 22f), 18f, 11f, 25f, tiles, tiles, root);
            for (int i = 0; i < 7; i++)
                lights.Add(Lantern("LauLantern", new Vector3(x - 1.5f + i * 2.5f, 7.5f, GateZ + 16.6f), new Color(1f, 0.12f, 0.12f), root, i % 2 == 0 ? 8f : 0f, 2f));
            nightLights = lights.ToArray();
        }
    }
}
