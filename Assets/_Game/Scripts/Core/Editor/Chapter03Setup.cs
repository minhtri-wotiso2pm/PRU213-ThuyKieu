using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Chapters;
using ThuyKieu.Gameplay.Sequences;
using ThuyKieu.Gameplay.Stealth;
using ThuyKieu.UI;
using UnityEditor;
using UnityEngine;
using static ThuyKieu.Core.Editor.StorySceneKit;
using static ThuyKieu.Core.Editor.StoryArtKit;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Builds Chapter03_TuBaHouse: a lantern-hung courtyard house that looks like a home and works like a cage —
    /// Kiều's room, the girls, Tú Bà's room with a window behind a planter, the ledger office and a gate locked from outside.
    /// </summary>
    public static class Chapter03Setup
    {
        public const string ScenePath = "Assets/_Game/Scenes/Chapter03_TuBaHouse.unity";

        private static Material _floor, _plaster, _red, _wood, _tiles, _ridge, _planks, _stone;

        /// <summary>
        /// Chapter 4 rebuilds this house at night: the back door is cut into the west wall, daytime-only
        /// interactions are skipped and the office ones are renamed for the night's tasks.
        /// </summary>
        public static bool Night { get; set; }

        private static readonly System.Collections.Generic.Dictionary<string, (string Id, string Label)> NightExplore =
            new System.Collections.Generic.Dictionary<string, (string, string)>
            {
                { "c3_drawer", ("c4_ledger", "Xé trang sổ nợ trong ngăn bàn") },
                { "c3_cabinet", ("c4_cabinet", "Mở tủ khóa đồng") }
            };

        /// <summary>Explore anchor for the house: Chapter 3 ids by day, their Chapter 4 counterparts (or nothing) at night.</summary>
        private static void HouseExplore(GameObject go, StoryDirector director, string id, string label, Vector3 size)
        {
            if (Night)
            {
                if (!NightExplore.TryGetValue(id, out var night)) return;
                (id, label) = night;
            }
            Explore(go, director, id, label, size);
        }

        /// <summary>Shared surfaces of the house (assets are reused by name).</summary>
        public static void InitMaterials()
        {
            _floor = Surface("Ch3Courtyard", "brick_floor", new Color(0.92f, 0.78f, 0.7f), 2.2f, 0.18f, 1.1f, 0.4f);
            _plaster = Surface("Ch3Plaster", "plastered_wall_02", new Color(0.98f, 0.88f, 0.72f), 2.6f, 0.08f, 1.2f, 0.5f, 1f);
            _red = Surface("Ch3Lacquer", "lacquered_cherry_wood", new Color(1f, 0.38f, 0.32f), 1.2f, 0.5f, 0.8f, 0.2f);
            _wood = Surface("Ch3DarkWood", "dark_wood", new Color(0.75f, 0.6f, 0.5f), 1.2f, 0.3f, 1f, 0.25f);
            _planks = Surface("Ch3FloorPlanks", "old_wooden_floor_02", new Color(0.8f, 0.66f, 0.55f), 2f, 0.3f, 1f, 0.35f);
            _tiles = Surface("Ch3Tiles", "clay_roof_tiles_02", new Color(0.62f, 0.42f, 0.38f), 1.4f, 0.25f, 1.2f, 0.35f);
            _ridge = Material("Ch3Ridge", new Color(0.18f, 0.08f, 0.07f));
            _stone = Surface("Ch3StoneBase", "japanese_stone_wall", new Color(0.82f, 0.8f, 0.76f), 1.6f, 0.15f, 1f, 0.4f, 0.5f);
        }

        public static Material Plaster => _plaster;
        public static Material Wood => _wood;
        public static Material Planks => _planks;
        public static Material Stone => _stone;
        public static Material Lacquer => _red;
        public static Material Tiles => _tiles;
        public static Material Ridge => _ridge;

        [MenuItem("ThuyKieu/Chapter 3/Create playable scene")]
        public static void Create()
        {
            Night = false;
            var scene = NewScene(ScenePath);
            var shared = StoryAnimationSetup.Controller();
            var s = CreateSystems(3, "Tú Bà", "chapter_3", Chapter04Setup.SceneName, new Vector3(0, 0.05f, -9.6f), 0f, "house");
            s.CameraComponent.backgroundColor = new Color(0.78f, 0.66f, 0.58f);
            // Late-afternoon warmth that turns to dusk later: honeyed highlights, rose-violet shadows.
            Grade(s.Fx, 0.2f, 12f, 6f, 9f, new Color(0.5f, 0.4f, 0.55f), new Color(0.7f, 0.55f, 0.42f));
            UseSky(s.CameraComponent, Sky("Ch3Sky", "kloppenheim_06_puresky", 1.05f, new Color(0.52f, 0.5f, 0.48f), 210f));
            var world = new GameObject("World").transform;
            var nav = NavMesh(world);
            InitMaterials();

            Shell(world);
            var director = s.Director;
            KieuRoom(world, director, out Transform playerRoom);
            Hall(world);
            TuBaRoom(world, director, out BoxCollider listenZone, out Transform listenStart, out Transform tuBaSpot, out Transform maSpot, out GameObject tuBaDoor);
            Office(world, director);
            var gateDoor = GateAndCourtyard(world, director);

            // Camera framing: rooms close and low, the veranda a little tighter, the window under Tú Bà's room intimate.
            var zones = new GameObject("CameraZones").transform;
            zones.SetParent(world, false);
            StoryArtKit.Zone("KieuRoom", new Vector3(-10.5f, 1.5f, 7.8f), new Vector3(6f, 4f, 9.4f), 2.5f, 0.35f, 58f, 0.05f, zones);
            StoryArtKit.Zone("TuBaRoom", new Vector3(10.5f, 1.5f, 7.8f), new Vector3(6f, 4f, 9.4f), 2.5f, 0.35f, 58f, 0.05f, zones);
            StoryArtKit.Zone("Office", new Vector3(10.5f, 1.5f, -7.2f), new Vector3(6f, 4f, 10.4f), 2.5f, 0.35f, 58f, 0.05f, zones);
            StoryArtKit.Zone("Veranda", new Vector3(-10f, 1.5f, -4f), new Vector3(7f, 4f, 13.5f), 3f, 0.4f, 55f, 0f, zones);
            StoryArtKit.Zone("UnderWindow", new Vector3(5.8f, 1.5f, 6.5f), new Vector3(3.6f, 4f, 6f), 2.6f, 0.45f, 54f, 0f, zones, 1);

            // Characters
            var tuBa = StaticActor("TuBa", "TuBa", "Assets/_Game/Art/Characters/TuBa/StaticV2/TuBa.obj",
                "Assets/_Game/Art/Characters/TuBa/StaticV2/texture_pbr_20250901.png", 1.62f, new Vector3(0, 0, -6.4f), 180f, 42f);
            var ma = RiggedActor("MaGiamSinh", "MaGiamSinh", "Assets/_Game/Art/Characters/MaGiamSinh/Models/MaGiamSinh_Rigged.fbx",
                "Assets/_Game/Art/Characters/MaGiamSinh/Tectures", 1.78f, new Vector3(1.7f, 0, -8.6f), -60f, Color.white, shared);
            var hong = RiggedActor("CoHong", "CoHong", "Assets/_Game/Art/Characters/ThuyKieu/Models/ThuyKieu_Rigged.fbx",
                "Assets/_Game/Art/Characters/ThuyKieu/Tectures", 1.6f, new Vector3(-1.5f, 0, -1.9f), -120f, new Color(1f, 0.72f, 0.8f), shared);
            var lam = RiggedActor("CoLam", "CoLam", "Assets/_Game/Art/Characters/MeThuyKieu/Models/MeThuyKieu_Rigged.fbx",
                "Assets/_Game/Art/Characters/MeThuyKieu/Tectures", 1.6f, new Vector3(-6.1f, 0, -1.5f), 90f, new Color(0.7f, 0.82f, 1f), shared);
            var maKieu = RiggedActor("MaKieu", "MaKieu", "Assets/_Game/Art/Characters/ThuyKieu/Models/ThuyKieu_Rigged.fbx",
                "Assets/_Game/Art/Characters/ThuyKieu/Tectures", 1.64f, new Vector3(-10.4f, 0, -9.4f), 45f, new Color(0.72f, 0.6f, 0.85f), shared);
            var guard = StaticActor("NguoiCanh", "NguoiCanh", "Assets/_Game/Art/Characters/LinhCanh/Static/LinhCanh.obj",
                "Assets/_Game/Art/Characters/LinhCanh/Static/texture_pbr_20250901.png", 1.76f, new Vector3(2.5f, 0, -7.5f), 0f);

            Explore(hong.gameObject, director, "c3_girl_hong", "Nói chuyện với cô gái áo hồng", Vector3.one * 1.9f);
            Explore(lam.gameObject, director, "c3_girl_lam", "Nói chuyện với cô gái áo lam", Vector3.one * 1.9f);
            Explore(maKieu.gameObject, director, "c3_makieu", "Đến gặp cô gái lạ", Vector3.one * 1.9f);
            var maKieuGlow = new GameObject("MaKieuGlow");
            maKieuGlow.transform.SetParent(maKieu.transform, false);
            maKieuGlow.transform.localPosition = new Vector3(0, 2.4f, 0.4f);
            var glow = maKieuGlow.AddComponent<Light>();
            glow.type = LightType.Point; glow.color = new Color(0.75f, 0.55f, 1f); glow.range = 4f; glow.intensity = 2.5f;

            // Guard: patrol between the courtyard, the office door and the windows.
            var vision = guard.gameObject.AddComponent<VisionSensor>();
            var eyes = Point("Eyes", guard.transform.position + Vector3.up * 1.62f, 0, guard.transform);
            Set(vision, "_eyes", eyes);
            Set(vision, "_player", s.Player);
            Set(vision, "_range", 8f);
            Set(vision, "_angle", 80f);
            Set(vision, "_secondsToSpot", 1.4f);
            Set(vision, "_onlyInRestrictedZones", true);
            Set(vision, "_occluders", 1 << EnvironmentLayer);
            var patrolRoot = new GameObject("GuardRoute").transform;
            patrolRoot.SetParent(world, false);
            var route = new Object[]
            {
                Point("A_Courtyard", new Vector3(0.5f, 0, -7.5f), 0, patrolRoot),
                Point("B_OfficeDoor", new Vector3(5.6f, 0, -4.6f), 90, patrolRoot),
                Point("C_Centre", new Vector3(0.8f, 0, 2.6f), 0, patrolRoot),
                Point("D_Hall", new Vector3(2.0f, 0, 9.4f), 0, patrolRoot)
            };
            var patrol = guard.gameObject.AddComponent<GuardPatrol>();
            Set(patrol, "_waypoints", route);
            Set(patrol, "_vision", vision);
            var cone = VisionCone(guard.transform, eyes, 8f, 80f);

            // Tú Bà and Mã Giám Sinh glance at the side window while they talk: a listener standing up is seen
            // over the sill, one crouched below it is not. Their sight only works during the eavesdrop.
            var tuBaSight = RoomSight(tuBa, 1.5f, s.Player);
            var maSight = RoomSight(ma, 1.62f, s.Player);

            // Camera anchors
            Point("CAM_Wide_TuBaCourtyard", new Vector3(-5.5f, 3.4f, -11.2f), 0, world).rotation = Quaternion.Euler(14, 28, 0);
            Point("CAM_CU_Kieu_Listens", new Vector3(5.2f, 1.2f, 4.9f), 0, world).rotation = Quaternion.Euler(6, 40, 0);

            // Sun & atmosphere
            var sun = Sun("Sun", new Color(1f, 0.86f, 0.68f), 1.15f, new Vector3(40, -35, 0));
            RenderSettings.ambientLight = new Color(0.45f, 0.38f, 0.35f);
            RenderSettings.fogColor = new Color(0.78f, 0.66f, 0.58f);
            RenderSettings.fogDensity = 0.008f;

            var worldScript = s.Root.AddComponent<Chapter03World>();
            Set(worldScript, "_director", director);
            Set(worldScript, "_navMesh", nav);
            Set(worldScript, "_tuBa", tuBa);
            Set(worldScript, "_ma", ma);
            Set(worldScript, "_maKieu", maKieu);
            Set(worldScript, "_guard", patrol);
            Set(worldScript, "_tuBaRoom", tuBaSpot);
            Set(worldScript, "_maRoom", maSpot);
            Set(worldScript, "_playerRoom", playerRoom);
            Set(worldScript, "_maKieuMarker", maKieuGlow);
            Set(worldScript, "_sun", sun);
            Set(worldScript, "_visionCone", cone);
            Set(worldScript, "_tuBaDoor", tuBaDoor);
            Set(worldScript, "_roomSensors", new Object[] { tuBaSight, maSight });

            // Sequences
            var sequences = new GameObject("Sequences").transform;
            sequences.SetParent(s.Root.transform, false);
            var tutorial = Child<TutorialSequence>(sequences, "tutorial_house");
            Set(tutorial, "_id", "tutorial_house");
            Set(tutorial, "_body", "<color=#E8C27A>Tự do khám phá</color> ngôi nhà theo thứ tự bạn muốn. Danh sách việc có thể làm hiện ở góc trái.\n\n" +
                "Sân trong, hành lang và phòng bạn là nơi được đi lại. <color=#F08080>Phòng sổ sách</color> và <color=#F08080>lối dưới cửa sổ phòng Tú Bà</color> là khu vực cấm.\n\n" +
                "Người canh có <color=#F08080>tầm nhìn hình nón</color> đỏ dưới chân, chỉ đáng sợ khi bạn ở khu vực cấm. Nếu hắn nghi, hắn sẽ đi lại gần để xem; <color=#E8C27A>khom người (Ctrl)</color>, nấp sau tường, bồn hoa và canh lúc hắn quay đi.");
            var gate = Child<ActionBeatSequence>(sequences, "try_gate");
            Set(gate, "_id", "try_gate");
            Set(gate, "_gesture", 2);
            Set(gate, "_sfx", "lock_rattle");
            Set(gate, "_shake", 0.2f);
            Set(gate, "_animatedProp", gateDoor);
            Set(gate, "_propOffset", new Vector3(0, 0, -0.05f));
            var drawer = Child<ActionBeatSequence>(sequences, "search_drawer");
            Set(drawer, "_id", "search_drawer");
            Set(drawer, "_gesture", 6);
            Set(drawer, "_sfx", "paper");
            Set(drawer, "_shake", 0f);
            var contract = Child<DocumentSequence>(sequences, "document_contract");
            Set(contract, "_id", "document_contract");
            Set(contract, "_view", s.Hud.GetComponent<DocumentView>());
            Set(contract, "_title", "VĂN TỰ BÁN THÂN");
            Set(contract, "_body", "Người bán: <b>Mã Giám Sinh</b>, người huyện Lâm Thanh.\nNgười mua: <b>Tú Bà</b>, chủ lầu xanh Lâm Tri.\n\n" +
                "Người bị bán: <b>Vương Thúy Kiều</b>, mười lăm tuổi,\nbiết chữ, giỏi đàn, giỏi thơ.\n\nGiá bán: <b>bốn trăm năm mươi lạng bạc</b>.\n" +
                "Từ nay người mua toàn quyền định đoạt nơi ở\nvà công việc của người bị bán.\n\nĐiểm chỉ làm bằng.");
            Set(contract, "_sealed", true);
            var eavesdrop = Child<EavesdropSequence>(sequences, "eavesdrop_c3");
            Set(eavesdrop, "_id", "eavesdrop_c3");
            Set(eavesdrop, "_zone", listenZone);
            Set(eavesdrop, "_start", listenStart);
            Set(eavesdrop, "_sensors", new Object[] { vision, tuBaSight, maSight });
            Set(eavesdrop, "_glancers", new Object[] { tuBa, ma });
            Set(eavesdrop, "_glanceTarget", Point("WindowGlance", new Vector3(7.5f, 1.5f, 6.5f), 0, world));
            Set(eavesdrop, "_glanceLines", new[] { "Hừm... ngoài cửa sổ có ai đấy?", "Khoan đã. Hình như có tiếng động." });
            Set(eavesdrop, "_guard", patrol);
            Set(eavesdrop, "_seconds", 8f);
            Set(eavesdrop, "_objective", "Lẻn qua bồn hoa, khom người dưới bậu cửa sổ phòng Tú Bà và nghe lén");
            Set(eavesdrop, "_caughtDetail", "Bạn đã bị thấy dưới cửa sổ. Canh lúc người canh quay đi, và luôn khom người: đứng thẳng là người trong phòng nhìn qua bậu cửa thấy ngay.");
            Set(eavesdrop, "_musicAfter", "tuba_warm");
            Fx(sequences, "panic", "panic", "heartbeat", "none", 3.2f, 0.18f, 3);
            Fx(sequences, "kieu_voice", "spirit", "memory", "none", 2f, 0f, 1);

            Save(scene, ScenePath);
        }

        internal static void Fx(Transform parent, string id, string mood, string sfx, string music, float hold, float shake, int repeat)
        {
            var fx = Child<FxSequence>(parent, id);
            Set(fx, "_id", id);
            Set(fx, "_mood", mood);
            Set(fx, "_sfx", sfx);
            Set(fx, "_music", music);
            Set(fx, "_hold", hold);
            Set(fx, "_shake", shake);
            Set(fx, "_repeatSfx", repeat);
        }

        /// <summary>Short-range sight for a character in Tú Bà's room, blocked by walls and the window sill.</summary>
        internal static VisionSensor RoomSight(ThuyKieu.Core.Story.StoryActor actor, float eyeHeight, Object player)
        {
            var sensor = actor.gameObject.AddComponent<VisionSensor>();
            var eyes = new GameObject("Eyes").transform;
            eyes.SetParent(actor.transform, false);
            eyes.localPosition = Vector3.up * eyeHeight;
            Set(sensor, "_eyes", eyes);
            Set(sensor, "_player", player);
            Set(sensor, "_range", 6f);
            Set(sensor, "_angle", 70f);
            Set(sensor, "_secondsToSpot", 0.8f);
            Set(sensor, "_crouchFactor", 1f);
            Set(sensor, "_onlyInRestrictedZones", false);
            Set(sensor, "_occluders", 1 << EnvironmentLayer);
            return sensor;
        }

        /// <summary>
        /// Soft cone of lantern light on the ground showing where the guard looks: bright at his feet, fading out
        /// towards the edge of his sight. The chapter worlds tint it warm while he is calm and red when he is suspicious.
        /// </summary>
        internal static Renderer VisionCone(Transform guard, Transform eyes, float range, float angle, string assetName = "Ch3VisionCone")
        {
            var cone = new GameObject("VisionCone");
            cone.transform.SetParent(eyes, false);
            cone.transform.localPosition = new Vector3(0, -1.58f, 0);
            var mesh = new Mesh { name = assetName };
            int segments = 20;
            var vertices = new Vector3[segments + 2];
            var uvs = new Vector2[segments + 2];
            var triangles = new int[segments * 3];
            vertices[0] = Vector3.zero;
            uvs[0] = new Vector2(0.5f, 0.5f);
            for (int i = 0; i <= segments; i++)
            {
                float a = Mathf.Deg2Rad * (-angle * 0.5f + angle * i / segments);
                vertices[i + 1] = new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a)) * range;
                uvs[i + 1] = new Vector2(0.5f + Mathf.Sin(a) * 0.5f, 0.5f + Mathf.Cos(a) * 0.5f);
                if (i < segments) { triangles[i * 3] = 0; triangles[i * 3 + 1] = i + 1; triangles[i * 3 + 2] = i + 2; }
            }
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            SaveAsset(mesh, MaterialFolder + "/" + assetName + ".asset");
            cone.AddComponent<MeshFilter>().sharedMesh = mesh;
            var material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.SetFloat("_Surface", 1);
            material.SetFloat("_Blend", 0);
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            material.SetTexture("_BaseMap", StoryArtKit.GeneratedTexture("VisionConeFade", StoryArtKit.DrawDot, 128));
            material.SetColor("_BaseColor", new Color(1f, 0.62f, 0.3f, 0.2f));
            SaveAsset(material, MaterialFolder + "/" + assetName + ".mat");
            var renderer = cone.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            cone.transform.localPosition += Vector3.up * 0.02f;
            return renderer;
        }

        private static void SaveAsset(Object asset, string path)
        {
            if (AssetDatabase.LoadAssetAtPath<Object>(path) != null) AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(asset, path);
        }

        // ------------------------------------------------------------------ architecture

        internal static void Shell(Transform world)
        {
            Block("CourtyardFloor", new Vector3(0, -0.1f, 0), new Vector3(28f, 0.2f, 26f), _floor, world);
            Block("WallNorth", new Vector3(0, 2f, 12.6f), new Vector3(28f, 4f, 0.4f), _plaster, world);
            Block("WallEast", new Vector3(13.6f, 2f, 0), new Vector3(0.4f, 4f, 25.6f), _plaster, world);
            if (!Night) Block("WallWest", new Vector3(-13.6f, 2f, 0), new Vector3(0.4f, 4f, 25.6f), _plaster, world);
            else
            {
                // The kitchen's back door (z -10.25 to -8.95) opens onto the back yard of Chapter 5.
                Block("WallWest", new Vector3(-13.6f, 2f, 1.88f), new Vector3(0.4f, 4f, 21.65f), _plaster, world);
                Block("WallWestSouth", new Vector3(-13.6f, 2f, -11.53f), new Vector3(0.4f, 4f, 2.55f), _plaster, world);
                Block("BackDoorLintel", new Vector3(-13.6f, 3.2f, -9.6f), new Vector3(0.42f, 1.6f, 1.3f), _plaster, world);
            }
            Block("WallSouthL", new Vector3(-8f, 2f, -12.6f), new Vector3(12f, 4f, 0.4f), _plaster, world);
            Block("WallSouthR", new Vector3(8f, 2f, -12.6f), new Vector3(12f, 4f, 0.4f), _plaster, world);
            foreach (var (x, z, w, d) in new[] { (0f, 12.4f, 28f, 1f), (0f, -12.4f, 28f, 1f) })
                Block("WallCap", new Vector3(x, 4.1f, z), new Vector3(w, 0.2f, d), _tiles, world);
            Block("WallCapE", new Vector3(13.6f, 4.1f, 0), new Vector3(1f, 0.2f, 25.6f), _tiles, world);
            Block("WallCapW", new Vector3(-13.6f, 4.1f, 0), new Vector3(1f, 0.2f, 25.6f), _tiles, world);
            foreach (var (x, z, w, d) in new[] { (0f, 12.4f, 28.2f, 0.3f), (0f, -12.4f, 28.2f, 0.3f), (13.6f, 0f, 0.3f, 25.8f), (-13.6f, 0f, 0.3f, 25.8f) })
                Block("WallCapRidge", new Vector3(x, 4.26f, z), new Vector3(w, 0.12f, d), _ridge, world, false);
            // Stone skirting where the walls meet the yard.
            Block("SkirtNorth", new Vector3(0, 0.22f, 12.36f), new Vector3(28f, 0.44f, 0.12f), _stone, world, false);
            Block("SkirtEast", new Vector3(13.36f, 0.22f, 0), new Vector3(0.12f, 0.44f, 25.6f), _stone, world, false);
            Block("SkirtWest", new Vector3(-13.36f, 0.22f, 0), new Vector3(0.12f, 0.44f, 25.6f), _stone, world, false);
            Block("SkirtSouthL", new Vector3(-8f, 0.22f, -12.36f), new Vector3(12f, 0.44f, 0.12f), _stone, world, false);
            Block("SkirtSouthR", new Vector3(8f, 0.22f, -12.36f), new Vector3(12f, 0.44f, 0.12f), _stone, world, false);
            // Flagstone path from the gate to the hall, edged with a kerb.
            var path = Surface("Ch3PathStone", "monastery_stone_floor", new Color(0.85f, 0.82f, 0.78f), 2.4f, 0.2f, 1.1f, 0.35f);
            Block("CentrePath", new Vector3(0, 0.012f, -2f), new Vector3(2.4f, 0.024f, 21f), path, world, false);
            foreach (int side in new[] { -1, 1 })
                Block("PathKerb", new Vector3(side * 1.25f, 0.03f, -2f), new Vector3(0.12f, 0.06f, 21f), _stone, world, false);
        }

        internal static void KieuRoom(Transform world, StoryDirector director, out Transform spawn)
        {
            var root = new GameObject("KieuRoom").transform;
            root.SetParent(world, false);
            Block("Floor", new Vector3(-10.5f, 0.02f, 7.8f), new Vector3(6f, 0.04f, 9.4f), _planks, root);
            Fadeable(Block("WallS", new Vector3(-10.5f, 1.75f, 3f), new Vector3(6.2f, 3.5f, 0.3f), _plaster, root));
            Fadeable(Block("WallE_South", new Vector3(-7.5f, 1.75f, 4.5f), new Vector3(0.3f, 3.5f, 3f), _plaster, root));
            Fadeable(Block("WallE_North", new Vector3(-7.5f, 1.75f, 9.95f), new Vector3(0.3f, 3.5f, 5.1f), _plaster, root));
            Fadeable(Block("DoorLintel", new Vector3(-7.5f, 3.1f, 6.7f), new Vector3(0.32f, 0.8f, 1.4f), _red, root));
            Prop("Assets/_Game/Prefabs/Props/Bed.prefab", new Vector3(-12.2f, 0, 10.4f), 90, 1f, root);
            Prop("Assets/_Game/Prefabs/Props/OilLamp.prefab", new Vector3(-8.2f, 0, 11.6f), 0, 1f, root);
            Prop("Assets/_Game/Prefabs/Props/CeramicJar.prefab", new Vector3(-12.9f, 0, 3.6f), 0, 1f, root);
            Lantern("RoomLamp", new Vector3(-13.15f, 2.1f, 8f), new Color(1f, 0.55f, 0.3f), root, 7f, 1.6f);
            // Furnishing: a rug, a lacquered folding screen and a dressing table make the cage look like a home.
            Block("Rug", new Vector3(-10.4f, 0.045f, 6.4f), new Vector3(2.8f, 0.01f, 1.9f), Material("Ch3Rug", new Color(0.5f, 0.12f, 0.1f)), root, false);
            Block("RugBorder", new Vector3(-10.4f, 0.04f, 6.4f), new Vector3(3.0f, 0.008f, 2.1f), Material("Ch3RugBorder", new Color(0.72f, 0.55f, 0.22f)), root, false);
            for (int i = 0; i < 4; i++)
            {
                var panel = Block("ScreenPanel", new Vector3(-12.9f + i * 0.02f, 0.95f, 5.0f + i * 0.62f), new Vector3(0.05f, 1.8f, 0.6f), _red, root, false, i % 2 == 0 ? 12f : -12f);
                Block("ScreenPaper", panel.transform.position + new Vector3(0.03f, 0.05f, 0), new Vector3(0.02f, 1.4f, 0.48f), Material("Ch3ScreenPaper", new Color(0.92f, 0.86f, 0.72f)), root, false, i % 2 == 0 ? 12f : -12f);
            }
            Block("DressingTable", new Vector3(-8.3f, 0.4f, 9.6f), new Vector3(0.6f, 0.8f, 1.2f), _wood, root);
            Block("Mirror", new Vector3(-8.15f, 1.15f, 9.6f), new Vector3(0.05f, 0.7f, 0.55f), Material("Ch3Bronze", new Color(0.7f, 0.55f, 0.3f), 0.85f), root, false);
            Prop("Assets/_Game/Prefabs/Props/CeramicJar.prefab", new Vector3(-8.3f, 0.8f, 9.2f), 0, 0.35f, root);
            // Carved lotus window over iron bars (c3_window).
            Block("WindowFrame", new Vector3(-10.5f, 1.7f, 12.38f), new Vector3(1.8f, 1.3f, 0.08f), _red, root, false);
            for (float x = -0.75f; x <= 0.76f; x += 0.25f)
                Block("Bar", new Vector3(-10.5f + x, 1.7f, 12.33f), new Vector3(0.04f, 1.2f, 0.04f), Material("Ch3Iron", new Color(0.12f, 0.12f, 0.13f), 0.6f), root, false);
            var window = new GameObject("WindowSpot");
            window.transform.SetParent(root, false);
            window.transform.position = new Vector3(-10.5f, 0, 11.4f);
            HouseExplore(window, director, "c3_window", "Xem cửa sổ", new Vector3(1.8f, 1.6f, 1.4f));
            Point("Focus_Window", new Vector3(-10.5f, 1.7f, 12.3f), 0, root);
            spawn = Point("PlayerRoomSpawn", new Vector3(-9.6f, 0.05f, 6.7f), 90, root);
            // Courtyard-facing windows and door couplets ride with their walls when the camera looks through.
            FadeWith(root.Find("WallS").gameObject, Lattice("CourtWindow", new Vector3(-10.5f, 1.7f, 2.83f), 1.6f, 1.2f, 180f, LatticeFrame(), WindowPaper(), root));
            var doorBoards = new GameObject("DoorCouplets").transform;
            doorBoards.SetParent(root, false);
            Couplets(new Vector3(-7.33f, 1.6f, 5.82f), new Vector3(-7.33f, 1.6f, 7.58f), -90f, doorBoards);
            FadeWith(root.Find("WallE_North").gameObject, Lattice("CourtWindow", new Vector3(-7.33f, 1.7f, 10.4f), 1.4f, 1.2f, 90f, LatticeFrame(), WindowPaper(), root), doorBoards.gameObject);
            // Veranda pillars along the west wing.
            for (float z = -10; z <= 2; z += 3f) Pillar("VerandaPillar", new Vector3(-6.6f, 0, z), 0.16f, 3.2f, _red, _stone, root);
            Block("VerandaBeam", new Vector3(-6.6f, 3.25f, -4f), new Vector3(0.25f, 0.2f, 13f), _wood, root, false);
            Hide(Roof("VerandaEave", new Vector3(-9.6f, 4.2f, -4f), 8f, 13.5f, 22f, _tiles, _ridge, root, false), new Vector3(-9.6f, 1.5f, -4f), new Vector3(8f, 4f, 13.5f));
            Hide(Roof("RoomEave", new Vector3(-10.6f, 4.4f, 7.8f), 6.6f, 10f, 24f, _tiles, _ridge, root, false), new Vector3(-10.5f, 1.5f, 7.8f), new Vector3(6f, 4f, 9.4f));
        }

        internal static void Hall(Transform world)
        {
            var root = new GameObject("MainHall").transform;
            root.SetParent(world, false);
            Block("HallFloor", new Vector3(0, 0.1f, 10.8f), new Vector3(14f, 0.2f, 3.6f), _planks, root);
            for (int i = -2; i <= 2; i++) Pillar("HallPillar", new Vector3(i * 3f, 0, 9.1f), 0.18f, 3.6f, _red, _stone, root);
            Block("Balcony", new Vector3(0, 3.7f, 10.8f), new Vector3(14f, 0.25f, 3.6f), _wood, root, false);
            Block("BalconyRail", new Vector3(0, 4.25f, 9.1f), new Vector3(14f, 0.8f, 0.1f), _red, root, false);
            Block("UpperFacade", new Vector3(0, 5.4f, 12.2f), new Vector3(14f, 3.2f, 0.4f), _plaster, root, false);
            Roof("HallRoof", new Vector3(0, 7.7f, 10.8f), 15.4f, 4.6f, 26f, _tiles, _ridge, root);
            Prop("Assets/_Game/Prefabs/Props/Altar.prefab", new Vector3(0, 0.2f, 11.8f), 180, 1f, root);
            Prop("Assets/_Game/Prefabs/Props/TableSet.prefab", new Vector3(-3.5f, 0.2f, 10.8f), 0, 1f, root);
            for (int i = -2; i <= 2; i++) Lantern("HallLantern", new Vector3(i * 3f + 1.5f, 3.1f, 8.8f), new Color(1f, 0.16f, 0.1f), root, i % 2 == 0 ? 6f : 0f, 1.8f);
            // Strings of lanterns across the courtyard, hung from ropes between the wings.
            for (float z = -8; z <= 4; z += 6f)
            {
                Rope(new Vector3(-7.4f, 4.4f, z), new Vector3(7.4f, 4.4f, z), 0.5f, root);
                for (float x = -6; x <= 6; x += 3f)
                    Lantern("CourtLantern", new Vector3(x, 3.97f - 0.5f * 4f * ((x + 7.4f) / 14.8f) * (1f - (x + 7.4f) / 14.8f), z), new Color(1f, 0.2f, 0.12f), root, 0f, 0f);
            }
            // Upper storey of the lầu: lattice windows, silk swags on the balcony and bamboo blinds below.
            var frame = LatticeFrame();
            var paper = WindowPaper();
            for (int i = 0; i < 5; i++) Lattice("UpperWindow", new Vector3(-5.2f + i * 2.6f, 5.3f, 11.97f), 1.4f, 1.6f, 180f, frame, paper, root);
            for (int i = 0; i < 4; i++)
                Drape("BalconySilk", new Vector3(-6f + i * 3f, 4.55f, 9.0f), new Vector3(-3f + i * 3f, 4.55f, 9.0f), 0.35f,
                    i % 2 == 0 ? new Color(0.8f, 0.14f, 0.22f) : new Color(0.92f, 0.6f, 0.68f), root);
            foreach (float x in new[] { -4.5f, 4.5f }) Blind("HallBlind", new Vector3(x, 3.5f, 8.95f), 2.4f, 0.9f, 0f, root);
            Couplets(new Vector3(-3f, 2f, 8.86f), new Vector3(3f, 2f, 8.86f), 0f, root);
            Block("HallPlaque", new Vector3(0, 3.3f, 9.05f), new Vector3(2.4f, 0.5f, 0.06f), Material("CoupletRed", new Color(0.45f, 0.05f, 0.04f), 0.55f), root, false);
            for (int i = 0; i < 4; i++)
                Block("PlaqueGlyph", new Vector3(-0.84f + i * 0.56f, 3.3f, 9.01f), new Vector3(0.3f, 0.3f, 0.01f), Emissive("CoupletGold", new Color(0.85f, 0.62f, 0.22f), 0.25f), root, false);
        }

        internal static void TuBaRoom(Transform world, StoryDirector director, out BoxCollider listenZone, out Transform listenStart, out Transform tuBaSpot, out Transform maSpot, out GameObject door)
        {
            var root = new GameObject("TuBaRoom").transform;
            root.SetParent(world, false);
            Block("Floor", new Vector3(10.5f, 0.02f, 7.8f), new Vector3(6f, 0.04f, 9.4f), _planks, root);
            Fadeable(Block("WallS", new Vector3(10.5f, 1.75f, 3f), new Vector3(6.2f, 3.5f, 0.3f), _plaster, root));
            // West wall with a window (sill at 1.2 m: hides a crouching listener, not a standing one) and a door.
            Fadeable(Block("WallW_A", new Vector3(7.5f, 1.75f, 4f), new Vector3(0.3f, 3.5f, 2f), _plaster, root));
            Fadeable(Block("WallW_WindowLow", new Vector3(7.5f, 0.6f, 6.5f), new Vector3(0.3f, 1.2f, 3f), _plaster, root));
            Fadeable(Block("WallW_WindowHigh", new Vector3(7.5f, 2.75f, 6.5f), new Vector3(0.3f, 1.5f, 3f), _plaster, root));
            // Lamp-lit paper lattice: from the yard it is obvious someone is inside talking.
            FadeWith(root.Find("WallW_WindowHigh").gameObject, Lattice("TuBaWindow", new Vector3(7.36f, 1.6f, 6.5f), 2.9f, 0.8f, -90f, LatticeFrame(), WindowPaper(), root));
            // Door: open while they walk in, barred by Chapter03World once both are inside.
            door = Block("TuBaDoor", new Vector3(7.5f, 1.35f, 10.1f), new Vector3(0.12f, 2.7f, 1.4f), Surface("Ch3GateWood", "planks_brown_10", new Color(0.55f, 0.32f, 0.22f), 1.6f, 0.25f, 1.2f), root);
            door.isStatic = false;
            Fadeable(door);
            Fadeable(Block("WallW_B", new Vector3(7.5f, 1.75f, 8.7f), new Vector3(0.3f, 3.5f, 1.4f), _plaster, root));
            Fadeable(Block("WallW_C", new Vector3(7.5f, 1.75f, 11.65f), new Vector3(0.3f, 3.5f, 1.7f), _plaster, root));
            Fadeable(Block("DoorLintel", new Vector3(7.5f, 3.1f, 10.1f), new Vector3(0.32f, 0.8f, 1.4f), _red, root));
            Prop("Assets/_Game/Prefabs/Props/TableSet.prefab", new Vector3(11f, 0, 6.2f), 0, 1f, root);
            Lantern("TuBaLamp", new Vector3(13.15f, 2.1f, 7f), new Color(1f, 0.25f, 0.15f), root, 7f, 1.8f);
            Hide(Roof("TuBaEave", new Vector3(10.6f, 4.4f, 7.8f), 6.6f, 10f, 24f, _tiles, _ridge, root, false), new Vector3(10.5f, 1.5f, 7.8f), new Vector3(6f, 4f, 9.4f));
            tuBaSpot = Point("TuBaSpot", new Vector3(10.4f, 0, 6.9f), 270, root);
            maSpot = Point("MaSpot", new Vector3(9.8f, 0, 5.1f), 300, root);
            FadeWith(root.Find("WallS").gameObject, Lattice("CourtWindow", new Vector3(10.5f, 1.7f, 2.83f), 1.6f, 1.2f, 180f, LatticeFrame(), WindowPaper(), root));

            // Planter shielding the window from the courtyard (1.25 m hides a crouching listener from the patrol route).
            var planter = Material("Ch3Planter", new Color(0.36f, 0.3f, 0.26f));
            Block("Planter", new Vector3(4.9f, 0.625f, 6.6f), new Vector3(0.7f, 1.25f, 4.4f), planter, root);
            var bloom = Material("Ch3Blossom", new Color(0.95f, 0.45f, 0.6f));
            var leaf = Material("Ch3Leaf", new Color(0.2f, 0.36f, 0.16f));
            var random = new System.Random(33);
            for (float z = 4.6f; z <= 8.6f; z += 0.22f)
            {
                var bush = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bush.name = "Shrub";
                Object.DestroyImmediate(bush.GetComponent<Collider>());
                bush.transform.SetParent(root, false);
                float size = 0.26f + (float)random.NextDouble() * 0.16f;
                bush.transform.position = new Vector3(4.9f + ((float)random.NextDouble() - 0.5f) * 0.3f, 1.27f + size * 0.25f, z);
                bush.transform.localScale = new Vector3(size, size * 0.8f, size);
                bush.GetComponent<Renderer>().sharedMaterial = leaf;
                if (random.NextDouble() < 0.45)
                {
                    var flower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    flower.name = "Blossom";
                    Object.DestroyImmediate(flower.GetComponent<Collider>());
                    flower.transform.SetParent(root, false);
                    flower.transform.position = bush.transform.position + new Vector3(((float)random.NextDouble() - 0.5f) * 0.15f, size * 0.38f, 0);
                    flower.transform.localScale = Vector3.one * 0.09f;
                    flower.GetComponent<Renderer>().sharedMaterial = bloom;
                }
            }

            listenZone = null;
            listenStart = null;
            if (Night) return;
            var zone = new GameObject("ListenZone");
            zone.transform.SetParent(root, false);
            zone.transform.position = new Vector3(6.6f, 0.8f, 6.5f);
            zone.layer = 2;
            listenZone = zone.AddComponent<BoxCollider>();
            listenZone.isTrigger = true;
            listenZone.size = new Vector3(1.3f, 1.6f, 3.2f);
            // The prompt sits at the corner of the planter, outside the forbidden strip: from here the player has to sneak in.
            var spot = new GameObject("WindowListenSpot");
            spot.transform.SetParent(root, false);
            spot.transform.position = new Vector3(4.4f, 0, 3.6f);
            HouseExplore(spot, director, "c3_eavesdrop", "Nghe tiếng nói từ phòng Tú Bà", new Vector3(1.8f, 1.6f, 1.8f));
            listenStart = Point("ListenStart", new Vector3(4.3f, 0.05f, 3.4f), 30, root);
            Zone(root, "Dưới cửa sổ phòng Tú Bà", new Vector3(6.3f, 1f, 6.6f), new Vector3(2.6f, 2f, 4.8f));
        }

        internal static void Office(Transform world, StoryDirector director)
        {
            var root = new GameObject("LedgerOffice").transform;
            root.SetParent(world, false);
            Block("Floor", new Vector3(10.5f, 0.02f, -7.2f), new Vector3(6f, 0.04f, 10.4f), _planks, root);
            Fadeable(Block("WallN", new Vector3(10.5f, 1.75f, -2f), new Vector3(6.2f, 3.5f, 0.3f), _plaster, root));
            Fadeable(Block("WallW_South", new Vector3(7.5f, 1.75f, -8.95f), new Vector3(0.3f, 3.5f, 7.1f), _plaster, root));
            Fadeable(Block("WallW_North", new Vector3(7.5f, 1.75f, -2.9f), new Vector3(0.3f, 3.5f, 1.8f), _plaster, root));
            Fadeable(Block("DoorLintel", new Vector3(7.5f, 3.1f, -4.6f), new Vector3(0.32f, 0.8f, 1.6f), _red, root));
            Hide(Roof("OfficeEave", new Vector3(10.6f, 4.4f, -7.2f), 6.6f, 11f, 24f, _tiles, _ridge, root, false), new Vector3(10.5f, 1.5f, -7.2f), new Vector3(6f, 4f, 10.4f));
            Lantern("OfficeLamp", new Vector3(13.15f, 2.1f, -7f), new Color(1f, 0.6f, 0.3f), root, 7f, 1.4f);

            Block("Desk", new Vector3(11.6f, 0.475f, -7.2f), new Vector3(0.9f, 0.95f, 1.9f), _wood, root);
            Block("Ledger", new Vector3(11.6f, 0.97f, -7.4f), new Vector3(0.35f, 0.05f, 0.45f), Material("Ch3LedgerCover", new Color(0.3f, 0.2f, 0.12f)), root, false);
            var drawer = new GameObject("DrawerSpot");
            drawer.transform.SetParent(root, false);
            drawer.transform.position = new Vector3(10.6f, 0, -7.2f);
            HouseExplore(drawer, director, "c3_drawer", "Lục ngăn kéo bàn", new Vector3(1.2f, 1.4f, 1.6f));
            Point("Focus_Drawer", new Vector3(11.6f, 0.9f, -7.2f), 0, root);

            Block("SideTable", new Vector3(8.5f, 0.4f, -10.9f), new Vector3(1.2f, 0.8f, 0.7f), _wood, root);
            Block("LacquerBox", new Vector3(8.5f, 0.93f, -10.9f), new Vector3(0.48f, 0.26f, 0.34f), Material("Ch3LacquerBox", new Color(0.7f, 0.08f, 0.06f), 0.75f), root, false);
            var box = new GameObject("BoxSpot");
            box.transform.SetParent(root, false);
            box.transform.position = new Vector3(8.5f, 0, -10f);
            HouseExplore(box, director, "c3_box", "Mở hộp gỗ sơn son", new Vector3(1.3f, 1.4f, 1.2f));
            Point("Focus_Box", new Vector3(8.5f, 1f, -10.9f), 0, root);

            Prop("Assets/_Game/Prefabs/Props/Cabinet.prefab", new Vector3(12.9f, 0, -3.2f), 270, 1f, root);
            var cabinet = new GameObject("CabinetSpot");
            cabinet.transform.SetParent(root, false);
            cabinet.transform.position = new Vector3(12f, 0, -3.2f);
            HouseExplore(cabinet, director, "c3_cabinet", "Thử mở tủ khóa đồng", new Vector3(1.2f, 1.4f, 1.4f));
            Point("Focus_Cabinet", new Vector3(12.9f, 1.2f, -3.2f), 0, root);
            FadeWith(root.Find("WallN").gameObject, Lattice("CourtWindow", new Vector3(10.5f, 1.7f, -1.83f), 1.6f, 1.2f, 0f, LatticeFrame(), WindowPaper(), root));
            FadeWith(root.Find("WallW_South").gameObject, Lattice("CourtWindow", new Vector3(7.33f, 1.8f, -9.6f), 1.4f, 1.1f, -90f, LatticeFrame(), WindowPaper(), root));

            Zone(root, "Phòng sổ sách", new Vector3(10.5f, 1f, -7.25f), new Vector3(5.8f, 2f, 10.2f));
        }

        internal static Transform GateAndCourtyard(Transform world, StoryDirector director)
        {
            var root = new GameObject("GateAndCourtyard").transform;
            root.SetParent(world, false);
            var door = Block("GateDoors", new Vector3(0, 2f, -12.6f), new Vector3(4f, 4f, 0.25f), Surface("Ch3GateWood", "planks_brown_10", new Color(0.55f, 0.32f, 0.22f), 1.6f, 0.25f, 1.2f), root);
            // Brass studs ride on the door so they rattle with it.
            var brass = Material("Ch3Brass", new Color(0.72f, 0.52f, 0.22f), 0.75f);
            for (int row = 0; row < 5; row++)
                for (int col = -3; col <= 3; col++)
                {
                    if (col == 0) continue;
                    var stud = Sphere("Stud", new Vector3(col * 0.28f, 0.6f + row * 0.7f, -12.47f), new Vector3(0.06f, 0.06f, 0.06f), brass, door.transform);
                    stud.transform.position = new Vector3(col * 0.28f, 0.6f + row * 0.7f, -12.47f);
                    stud.transform.localScale = new Vector3(0.06f / 4f, 0.06f / 4f, 0.06f / 0.25f);
                }
            Block("GateSeam", new Vector3(0, 2f, -12.45f), new Vector3(0.05f, 3.9f, 0.05f), _ridge, root, false);
            Block("GateLock", new Vector3(0, 1.4f, -12.42f), new Vector3(0.28f, 0.18f, 0.08f), Emissive("Ch3Brass", new Color(0.8f, 0.6f, 0.25f), 0.4f), root, false);
            Block("GatePillarL", new Vector3(-2.3f, 2.4f, -12.6f), new Vector3(0.6f, 4.8f, 0.6f), _red, root);
            Block("GatePillarR", new Vector3(2.3f, 2.4f, -12.6f), new Vector3(0.6f, 4.8f, 0.6f), _red, root);
            Roof("GateRoof", new Vector3(0, 5.7f, -12.6f), 6.2f, 2.2f, 26f, _tiles, _ridge, root);
            Lantern("GateLanternL", new Vector3(-1.6f, 3.3f, -12.1f), new Color(1f, 0.16f, 0.1f), root, 6f, 1.6f);
            Lantern("GateLanternR", new Vector3(1.6f, 3.3f, -12.1f), new Color(1f, 0.16f, 0.1f), root, 0f, 0f);
            var gateSpot = new GameObject("GateSpot");
            gateSpot.transform.SetParent(root, false);
            gateSpot.transform.position = new Vector3(0, 0, -11.6f);
            HouseExplore(gateSpot, director, "c3_gate", "Kiểm tra cổng chính", new Vector3(3f, 1.6f, 1.2f));
            Point("Focus_Gate", new Vector3(0, 1.6f, -12.4f), 0, root);

            Prop("Assets/_Game/Prefabs/Props/Well.prefab", new Vector3(-3f, 0, -3f), 0, 1f, root);
            int seed = 0;
            foreach (var p in new[] { new Vector3(-4.5f, 0, 6f), new Vector3(4.5f, 0, -9f), new Vector3(-4.6f, 0, -9.5f), new Vector3(3.5f, 0, 1.5f) })
                PottedPlant(p, 1.1f, root, seed++, seed % 2 == 0);
            Dressing(root);
            if (!Night) Block("MaKieuBench", new Vector3(-11.3f, 0.4f, -10.4f), new Vector3(1.6f, 0.12f, 0.45f), _wood, root);
            return door.transform;
        }

        /// <summary>Lived-in clutter for the courtyard: jars, bonsai, baskets, trees in the corners, drifting leaves and motes.</summary>
        private static void Dressing(Transform root)
        {
            Jars(new Vector3(-8.4f, 0, -11.4f), 3, root, 3);
            Jars(new Vector3(6.4f, 0, -11.6f), 2, root, 5);
            Jars(new Vector3(12.5f, 0, 1.8f), 2, root, 8);

            // Bonsai on carved stands either side of the hall steps.
            foreach (float x in new[] { -5.8f, 5.6f })
            {
                Block("BonsaiStand", new Vector3(x, 0.35f, 7.6f), new Vector3(0.7f, 0.7f, 0.7f), _wood, root);
                Block("BonsaiStandTop", new Vector3(x, 0.72f, 7.6f), new Vector3(0.8f, 0.05f, 0.8f), _red, root, false);
                PottedPlant(new Vector3(x, 0.74f, 7.6f), 0.55f, root, (int)(x * 10), true);
            }
            // Hall step and a stone basin by the well.
            Block("HallStep", new Vector3(0, 0.05f, 8.85f), new Vector3(14.2f, 0.1f, 0.35f), _stone, root, false);
            Cylinder("WaterBasin", new Vector3(-1.6f, 0, -4.1f), 0.38f, 0.42f, _stone, root);
            Cylinder("BasinWater", new Vector3(-1.6f, 0.38f, -4.1f), 0.32f, 0.03f, Material("Ch3Water", new Color(0.16f, 0.2f, 0.2f), 0.95f), root, false);
            Block("StoneBench", new Vector3(-4.6f, 0.4f, -1.2f), new Vector3(1.5f, 0.12f, 0.5f), _stone, root);
            foreach (float dx in new[] { -0.55f, 0.55f }) Block("StoneBenchLeg", new Vector3(-4.6f + dx, 0.17f, -1.2f), new Vector3(0.2f, 0.34f, 0.4f), _stone, root, false);

            // Baskets and firewood against the office wall.
            var weave = Surface("Ch3Basket", "planks_brown_10", new Color(0.95f, 0.8f, 0.5f), 0.35f, 0.15f, 1.4f);
            foreach (var p in new[] { new Vector3(6.9f, 0, -6.8f), new Vector3(6.85f, 0, -7.5f), new Vector3(6.9f, 0.44f, -7.15f) })
                Cylinder("Basket", p, 0.28f, 0.42f, weave, root, false);
            for (int i = 0; i < 9; i++)
            {
                var log = Cylinder("Firewood", Vector3.zero, 0.06f, 0.8f, _wood, root, false);
                log.transform.SetPositionAndRotation(new Vector3(6.95f, 0.07f + (i / 3) * 0.12f, -9.2f - (i % 3) * 0.13f), Quaternion.Euler(0, 0, 90));
            }

            // Trees soften the corners.
            Prop("Assets/_Game/Prefabs/Vegetation/BananaTree.prefab", new Vector3(12.3f, 0, 0.4f), 40, 0.9f, root);
            if (!Night) Prop("Assets/_Game/Prefabs/Vegetation/ArecaPalm.prefab", new Vector3(-12.6f, 0, -12f), 0, 0.85f, root); // the kitchen stands there at night
            Prop("Assets/_Game/Prefabs/Vegetation/BananaTree.prefab", new Vector3(-12.4f, 0, 1.9f), 200, 0.75f, root);

            Leaves("CourtyardLeaves", new Vector3(0, 5f, -2f), new Vector3(22f, 1f, 18f), root, new Color(0.55f, 0.5f, 0.2f), 18);
            if (!Night) Dust("CourtyardMotes", new Vector3(0, 1.8f, -1f), new Vector3(20f, 3f, 18f), root, new Color(1f, 0.85f, 0.62f, 0.3f), 160);
            Dust("HallMotes", new Vector3(0, 1.8f, 10.8f), new Vector3(13f, 2.5f, 3f), root, new Color(1f, 0.75f, 0.5f, 0.35f), 60);
        }

        private static Material LatticeFrame() => Material("Ch3LatticeFrame", new Color(0.2f, 0.06f, 0.04f), 0.4f);
        private static Material WindowPaper() => Emissive("Ch3WindowPaper", new Color(0.95f, 0.78f, 0.55f), 0.35f);

        internal static void Hide(GameObject roof, Vector3 centre, Vector3 size)
        {
            foreach (var t in roof.GetComponentsInChildren<Transform>()) t.gameObject.layer = 0;
            var hider = roof.AddComponent<ThuyKieu.Environment.RoofHider>();
            Set(hider, "_centre", centre);
            Set(hider, "_size", size);
        }

        internal static void Zone(Transform parent, string label, Vector3 centre, Vector3 size)
        {
            var go = new GameObject("RestrictedZone_" + label);
            go.transform.SetParent(parent, false);
            go.transform.position = centre;
            go.layer = 2;
            var box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = size;
            var zone = go.AddComponent<RestrictedZone>();
            Set(zone, "_label", label);
        }
    }
}
