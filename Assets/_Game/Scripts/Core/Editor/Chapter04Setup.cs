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
    /// Builds Chapter04_TuBaNight: the Chapter 3 house by night — moonlight, pools of lantern light between deep shadows,
    /// guests in the hall, a drunk guard with a lantern, hiding places, and a kitchen with the back door.
    /// </summary>
    public static class Chapter04Setup
    {
        public const string SceneName = "Chapter04_TuBaNight";
        public const string ScenePath = "Assets/_Game/Scenes/" + SceneName + ".unity";

        [MenuItem("ThuyKieu/Chapter 4/Create playable scene")]
        public static void Create()
        {
            var scene = NewScene(ScenePath);
            var shared = StoryAnimationSetup.Controller();
            var spawn = new Vector3(-10.2f, 0.05f, 6.7f);
            var s = CreateSystems(4, "Không còn đường lui", "chapter_4", Chapter05Setup.SceneName, spawn, 90f, "night_house");
            NightLook(s.Fx, s.CameraComponent, "Ch4SkyNight", 0f, new Vector3(38, 60, 0));
            Set(s.Shots, "_keyLight", 1.6f);
            var world = new GameObject("World").transform;
            var nav = NavMesh(world);
            var director = s.Director;

            Chapter03Setup.Night = true;
            try
            {
                Chapter03Setup.InitMaterials();
                Chapter03Setup.Shell(world);
                Chapter03Setup.KieuRoom(world, director, out Transform playerRoom);
                Chapter03Setup.Hall(world);
                Chapter03Setup.TuBaRoom(world, director, out _, out _, out Transform tuBaChair, out _, out GameObject tuBaDoor);
                Chapter03Setup.Office(world, director);
                Chapter03Setup.GateAndCourtyard(world, director);
                Object.DestroyImmediate(tuBaDoor);
                var backDoor = Kitchen(world, director);
                NightLights(world);
                var sleepSpot = NightDressing(world);
                Fireflies("GardenFireflies", new Vector3(0, 1.4f, -2f), new Vector3(18f, 1.6f, 16f), world, 40);

                var zones = new GameObject("CameraZones").transform;
                zones.SetParent(world, false);
                // Rooms: pulled back and raised so the camera looks down into the room over the faded wall.
                StoryArtKit.Zone("KieuRoom", new Vector3(-10.5f, 1.5f, 7.8f), new Vector3(6f, 4f, 9.4f), 3.3f, 0.3f, 60f, 0.35f, zones);
                StoryArtKit.Zone("TuBaRoom", new Vector3(10.5f, 1.5f, 7.8f), new Vector3(6f, 4f, 9.4f), 3.3f, 0.3f, 60f, 0.35f, zones);
                StoryArtKit.Zone("Office", new Vector3(10.5f, 1.5f, -7.2f), new Vector3(6f, 4f, 10.4f), 3.3f, 0.3f, 60f, 0.35f, zones);
                StoryArtKit.Zone("Kitchen", new Vector3(-11.2f, 1.5f, -9.5f), new Vector3(4.4f, 4f, 5.8f), 3f, 0.3f, 60f, 0.35f, zones);
                StoryArtKit.Zone("Veranda", new Vector3(-10f, 1.5f, -2f), new Vector3(7f, 4f, 9f), 3.4f, 0.4f, 57f, 0.15f, zones);

                // Night rules: anywhere outside Kiều's room and the west veranda is off limits.
                var restricted = new GameObject("NightZones").transform;
                restricted.SetParent(world, false);
                // The yard counts for the guard but does not nag: the rooms and the hall are where the banner warns.
                Chapter03Setup.Zone(restricted, "Sân trong", new Vector3(3.7f, 1f, -1.8f), new Vector3(19.4f, 2f, 21.2f), true);
                Chapter03Setup.Zone(restricted, "Sảnh tiếp khách", new Vector3(0f, 1f, 10.8f), new Vector3(14f, 2f, 3.6f));
                Chapter03Setup.Zone(restricted, "Phòng Tú Bà", new Vector3(10.5f, 1f, 7.8f), new Vector3(5.8f, 2f, 9.2f));
                Chapter03Setup.Zone(restricted, "Nhà bếp", new Vector3(-11.2f, 1f, -9.5f), new Vector3(4.4f, 2f, 5.8f));

                // ------------------------------------------------------------ characters
                var tuBa = StaticActor("TuBa", "TuBa", "Assets/_Game/Art/Characters/TuBa/StaticV2/TuBa.obj",
                    "Assets/_Game/Art/Characters/TuBa/StaticV2/texture_pbr_20250901.png", 1.62f, new Vector3(-8.4f, 0, 6.7f), 270f, 42f);
                var guard = StaticActor("NguoiCanh", "NguoiCanh", "Assets/_Game/Art/Characters/LinhCanh/Static/LinhCanh.obj",
                    "Assets/_Game/Art/Characters/LinhCanh/Static/texture_pbr_20250901.png", 1.76f, new Vector3(-6.6f, 0, 7.6f), 270f);
                var doorGuard = StaticActor("NguoiCanhCua", "NguoiCanh", "Assets/_Game/Art/Characters/LinhCanh/Static/LinhCanh.obj",
                    "Assets/_Game/Art/Characters/LinhCanh/Static/texture_pbr_20250901.png", 1.74f, new Vector3(-6.9f, 0, 5.4f), 200f);
                var coLam = RiggedActor("CoLam", "CoLam", "Assets/_Game/Art/Characters/MeThuyKieu/Models/MeThuyKieu_Rigged.fbx",
                    "Assets/_Game/Art/Characters/MeThuyKieu/Tectures", 1.6f, new Vector3(-7.4f, 0, -4.2f), 90f, new Color(0.7f, 0.82f, 1f), shared);
                var guests = new[]
                {
                    RiggedActor("KhachA", "Khach", "Assets/_Game/Art/Characters/MaGiamSinh/Models/MaGiamSinh_Rigged.fbx",
                        "Assets/_Game/Art/Characters/MaGiamSinh/Tectures", 1.76f, new Vector3(-1.8f, 0.2f, 10.6f), 100f, new Color(0.55f, 0.6f, 0.85f), shared),
                    RiggedActor("KhachB", "Khach", "Assets/_Game/Art/Characters/MaGiamSinh/Models/MaGiamSinh_Rigged.fbx",
                        "Assets/_Game/Art/Characters/MaGiamSinh/Tectures", 1.72f, new Vector3(0.4f, 0.2f, 10.9f), 260f, new Color(0.8f, 0.6f, 0.45f), shared)
                };

                Explore(coLam.gameObject, director, "c4_colam", "Nói chuyện với cô gái áo lam", Vector3.one * 1.9f);
                var observable = tuBa.gameObject.AddComponent<ObservableTarget>();
                Set(observable, "_director", director);
                Set(observable, "_exploreId", "c4_observe");
                Set(observable, "_label", "Quan sát đường đi của Tú Bà");
                Set(observable, "_requiredSeconds", 3.2f);
                Set(observable, "_maxDistance", 22f);
                Set(observable, "_focusPoint", Point("ObserveFocus", tuBa.transform.position + Vector3.up * 1.3f, 0, tuBa.transform));
                var keyTarget = Explore(tuBa.gameObject, director, "c4_key", "Lấy chùm chìa ở thắt lưng Tú Bà", Vector3.one * 1.9f, true);

                // Tú Bà sees the courtyard and her room whenever she is awake and walking about.
                var tuBaSight = Chapter03Setup.RoomSight(tuBa, 1.5f, s.Player);
                Set(tuBaSight, "_range", 7.5f);
                Set(tuBaSight, "_angle", 80f);
                Set(tuBaSight, "_secondsToSpot", 1.5f);
                Set(tuBaSight, "_crouchFactor", 0.5f);
                Set(tuBaSight, "_onlyInRestrictedZones", true);

                // The drunk guard with his lantern.
                var vision = guard.gameObject.AddComponent<VisionSensor>();
                var eyes = Point("Eyes", guard.transform.position + Vector3.up * 1.62f, 0, guard.transform);
                eyes.localRotation = Quaternion.identity;
                Set(vision, "_eyes", eyes);
                Set(vision, "_player", s.Player);
                Set(vision, "_range", 8f);
                Set(vision, "_angle", 80f);
                Set(vision, "_secondsToSpot", 1.7f);
                Set(vision, "_onlyInRestrictedZones", true);
                Set(vision, "_occluders", 1 << EnvironmentLayer);
                var route = new GameObject("GuardRoute").transform;
                route.SetParent(world, false);
                var patrol = guard.gameObject.AddComponent<GuardPatrol>();
                Set(patrol, "_waypoints", new Object[]
                {
                    Point("A_Gate", new Vector3(0.5f, 0, -8.6f), 0, route),
                    Point("B_OfficeDoor", new Vector3(5.4f, 0, -4.6f), 90, route),
                    Point("C_TuBaWing", new Vector3(4.2f, 0, 4.2f), 0, route),
                    Point("D_HallFront", new Vector3(-1.5f, 0, 7.6f), 0, route),
                    Point("E_Well", new Vector3(-4.6f, 0, -0.6f), 180, route),
                    Point("F_KitchenDoor", new Vector3(-7.4f, 0, -9.7f), 270, route)
                });
                Set(patrol, "_vision", vision);
                Set(patrol, "_speed", 1.15f);
                Set(patrol, "_pause", 2.6f);
                Set(patrol, "_drunk", true);
                var cone = Chapter03Setup.VisionCone(guard.transform, eyes, 8f, 80f, "Ch4VisionCone");
                Lantern("GuardLantern", guard.transform.position + guard.transform.right * 0.35f + Vector3.up * 1.15f, new Color(1f, 0.55f, 0.2f), guard.transform, 4.5f, 1.3f);
                Chapter05Setup.AddAlertIcon(guard.transform, vision, patrol);

                var doorSight = doorGuard.gameObject.AddComponent<VisionSensor>();
                var doorEyes = Point("Eyes", doorGuard.transform.position + Vector3.up * 1.6f, 0, doorGuard.transform);
                doorEyes.localRotation = Quaternion.identity;
                Set(doorSight, "_eyes", doorEyes);
                Set(doorSight, "_player", s.Player);
                Set(doorSight, "_range", 5.5f);
                Set(doorSight, "_angle", 110f);
                Set(doorSight, "_secondsToSpot", 1.2f);
                Set(doorSight, "_onlyInRestrictedZones", false);
                Set(doorSight, "_occluders", 1 << EnvironmentLayer);
                Chapter05Setup.AddAlertIcon(doorGuard.transform, doorSight, null);

                // ------------------------------------------------------------ hiding places
                var hides = new GameObject("HidingPlaces").transform;
                hides.SetParent(world, false);
                Block("Wardrobe", new Vector3(13.05f, 1.05f, -11f), new Vector3(0.7f, 2.1f, 1.3f), Chapter03Setup.Wood, hides);
                Block("WardrobeSeam", new Vector3(12.69f, 1.05f, -11f), new Vector3(0.02f, 1.9f, 0.02f), Material("Ch3Ridge", Color.black), hides, false);
                Hide(hides, s.Player, "Trốn vào tủ áo", new Vector3(12.25f, 0.05f, -11f), 90f, "door");
                Hide(hides, s.Player, "Nấp sau chum vại", new Vector3(6.4f, 0.05f, -10.8f), 180f, "step");
                Hide(hides, s.Player, "Nấp sau chum nước", new Vector3(-8.3f, 0.05f, -12.0f), 0f, "step");
                Hide(hides, s.Player, "Nấp sau bếp lò", new Vector3(-11.5f, 0.05f, -7.5f), 270f, "step");
                Hide(hides, s.Player, "Nấp sau mành tre", new Vector3(4.5f, 0.25f, 9.75f), 180f, "paper");
                Hide(hides, s.Player, "Nấp sau thành giếng", new Vector3(-3.9f, 0.05f, -3.8f), 45f, "step");
                Block("Screen", new Vector3(12.6f, 0.95f, 10.0f), new Vector3(1.6f, 1.9f, 0.08f), Chapter03Setup.Lacquer, hides);
                Hide(hides, s.Player, "Nấp sau bình phong", new Vector3(12.6f, 0.05f, 10.6f), 180f, "paper");

                // ------------------------------------------------------------ pebbles
                var pebbles = s.Player.gameObject.AddComponent<PebbleThrow>();
                Set(pebbles, "_player", s.Player);
                Set(pebbles, "_camera", s.CameraComponent);
                Set(pebbles, "_audio", s.Audio);
                Set(pebbles, "_surfaces", 1 << EnvironmentLayer);
                Set(pebbles, "_pebbleMaterial", Material("Ch4Pebble", new Color(0.4f, 0.38f, 0.35f)));

                // ------------------------------------------------------------ cameras
                Point("CAM_Wide_KieuRoomNight", new Vector3(-12.7f, 2.3f, 4.2f), 0, world).rotation = Quaternion.LookRotation(new Vector3(-8.4f, 1.3f, 6.7f) - new Vector3(-12.7f, 2.3f, 4.2f));
                // Three-quarter view from the foot of the sập: Tú Bà asleep on it, Kiều's reaching hand, the keys at her belt.
                var pickShot = Point("PickpocketShot", sleepSpot.position + new Vector3(-1.9f, 1.55f, -1.2f), 0, world);
                pickShot.rotation = Quaternion.LookRotation(sleepSpot.position + new Vector3(-0.15f, 0.55f, 0.9f) - pickShot.position);
                Point("Focus_BackDoor", new Vector3(-13.4f, 1.4f, -9.6f), 0, world);
                // Review viewpoints for StoryLookCapture (not used in play).
                Look("LOOK_KieuRoom", new Vector3(-8.0f, 2.6f, 4.0f), new Vector3(-11.6f, 0.8f, 9.4f), world);
                Look("LOOK_TuBaRoom", new Vector3(8.2f, 2.5f, 4.2f), new Vector3(12.6f, 0.6f, 8.6f), world);
                Look("LOOK_Office", new Vector3(8.2f, 2.5f, -3.0f), new Vector3(12.4f, 1f, -9.6f), world);
                Look("LOOK_Kitchen", new Vector3(-9.6f, 2.5f, -7.2f), new Vector3(-12.6f, 0.9f, -10.8f), world);
                Look("LOOK_Courtyard", new Vector3(5.5f, 3.2f, -10.5f), new Vector3(-2f, 0.6f, 3f), world);
                var hallSpot = Point("TuBaHallSpot", new Vector3(-0.8f, 0.2f, 9.9f), 0, world);

                // ------------------------------------------------------------ light & atmosphere
                var worldScript = s.Root.AddComponent<Chapter04World>();
                Set(worldScript, "_director", director);
                Set(worldScript, "_navMesh", nav);
                Set(worldScript, "_tuBa", tuBa);
                Set(worldScript, "_tuBaSight", tuBaSight);
                Set(worldScript, "_guard", patrol);
                Set(worldScript, "_visionCone", cone);
                Set(worldScript, "_doorGuard", doorGuard);
                Set(worldScript, "_doorGuardSight", doorSight);
                Set(worldScript, "_hallSpot", hallSpot);
                Set(worldScript, "_roomSpot", tuBaChair);
                Set(worldScript, "_sleepSpot", sleepSpot);
                Set(worldScript, "_keyRing", StoryPropKit.KeyRing(tuBa.transform.Find("Visual"), new Vector3(0.24f, 0.86f, 0.06f)));
                Set(worldScript, "_playerRoom", playerRoom);
                Set(worldScript, "_keyTarget", keyTarget);
                Set(worldScript, "_guests", guests);
                Set(worldScript, "_pebbles", pebbles);

                // ------------------------------------------------------------ sequences
                var sequences = new GameObject("Sequences").transform;
                sequences.SetParent(s.Root.transform, false);
                var tutorial = Child<TutorialSequence>(sequences, "tutorial_night");
                Set(tutorial, "_id", "tutorial_night");
                Set(tutorial, "_body", "<color=#E8C27A>Đêm nay chỉ phòng bạn và hiên phía tây là an toàn.</color> Ra sân là người canh có thể thấy bạn; sảnh, bếp và các phòng là khu vực cấm.\n\n" +
                    "Đi trong <color=#E8C27A>bóng tối</color> giữa các quầng đèn: đứng dưới đèn lồng là bị thấy rất nhanh (xem thanh ánh sáng dưới màn hình). Khom người (Ctrl) để rón rén.\n" +
                    "<color=#E8C27A>E</color> để nấp sau chum vại, mành tre, tủ áo. <color=#E8C27A>Q</color> ném sỏi để dụ người canh đi chỗ khác.\n" +
                    "Dấu <color=#E8C27A>?</color> trên đầu là đang nghi, dấu <color=#F08080>!</color> là sắp bị phát hiện.");
                Set(tutorial, "_controlsHint", "WASD: đi · Ctrl: khom · E: tương tác / trốn · Q: ném sỏi · Chuột phải: quan sát · J: sổ tay");
                var pick = Child<PickpocketSequence>(sequences, "pickpocket");
                Set(pick, "_id", "pickpocket");
                Set(pick, "_sleeper", tuBa);
                Set(pick, "_shot", pickShot);
                // Kiều kneels at the side of the sập, level with Tú Bà's waist.
                var stand = Point("PickpocketStand", sleepSpot.position + new Vector3(-1.05f, 0.01f, 0.85f), 90f, world);
                Set(pick, "_stand", stand);
                var deal = Child<DocumentSequence>(sequences, "document_deal");
                Set(deal, "_id", "document_deal");
                Set(deal, "_view", s.Hud.GetComponent<DocumentView>());
                Set(deal, "_title", "GIAO KÈO");
                Set(deal, "_body", DealText);
                Set(deal, "_sealed", true);
                var drawer = Child<ActionBeatSequence>(sequences, "search_drawer");
                Set(drawer, "_id", "search_drawer");
                Set(drawer, "_gesture", 6);
                Set(drawer, "_sfx", "paper");
                Set(drawer, "_shake", 0f);
                Chapter03Setup.Fx(sequences, "kieu_voice", "spirit", "memory", "none", 2f, 0f, 1);

                Save(scene, ScenePath);
            }
            finally { Chapter03Setup.Night = false; }
        }

        public const string DealText =
            "Lập ngày mùng chín tháng Giêng.\n\nBên giao người: <b>Mã Giám Sinh</b>, chuyên lo việc \"hỏi vợ\" ở các huyện.\n" +
            "Bên nhận người: <b>Tú Bà</b>, lầu xanh Lâm Tri.\n\nMỗi người giao đến, Tú Bà trả <b>bốn trăm năm mươi lạng</b>.\n\n" +
            "<s>Lý thị — huyện Đông Thành</s>\n<s>Trần thị — huyện Vũ Lăng</s>\n<s>Lưu thị — huyện Thanh Hà</s>\n\nHai bên điểm chỉ làm bằng.";

        /// <summary>Kitchen nook in the south-west corner with the back door in the west wall.</summary>
        private static Transform Kitchen(Transform world, StoryDirector director)
        {
            var root = new GameObject("Kitchen").transform;
            root.SetParent(world, false);
            var plaster = Chapter03Setup.Plaster;
            Block("KitchenFloor", new Vector3(-11.2f, 0.015f, -9.5f), new Vector3(4.4f, 0.03f, 5.8f), Chapter03Setup.Stone, root, false);
            Block("KitchenWallN", new Vector3(-11.2f, 1.6f, -6.6f), new Vector3(4.4f, 3.2f, 0.25f), plaster, root);
            Block("KitchenWallE_South", new Vector3(-9f, 1.6f, -11.6f), new Vector3(0.25f, 3.2f, 1.6f), plaster, root);
            Block("KitchenWallE_North", new Vector3(-9f, 1.6f, -7.6f), new Vector3(0.25f, 3.2f, 2f), plaster, root);
            Block("KitchenLintel", new Vector3(-9f, 2.9f, -9.7f), new Vector3(0.27f, 0.6f, 2.2f), Chapter03Setup.Wood, root);

            // Stove with embers and a low orange glow, firewood, water jars and a shelf of bowls.
            Block("Stove", new Vector3(-12.7f, 0.42f, -7.3f), new Vector3(1.2f, 0.84f, 0.9f), Chapter03Setup.Stone, root);
            Block("Embers", new Vector3(-12.7f, 0.86f, -7.3f), new Vector3(0.5f, 0.04f, 0.4f), Emissive("Ch4Embers", new Color(1f, 0.35f, 0.1f), 2.2f), root, false);
            var fire = new GameObject("StoveGlow");
            fire.transform.SetParent(root, false);
            fire.transform.position = new Vector3(-12.5f, 1.1f, -7.4f);
            var glow = fire.AddComponent<Light>();
            glow.type = LightType.Point; glow.color = new Color(1f, 0.45f, 0.18f); glow.range = 4.2f; glow.intensity = 1.1f;
            for (int i = 0; i < 8; i++)
            {
                var log = Cylinder("Firewood", Vector3.zero, 0.06f, 0.8f, Chapter03Setup.Wood, root, false);
                log.transform.SetPositionAndRotation(new Vector3(-10.0f - (i % 4) * 0.13f, 0.07f + (i / 4) * 0.12f, -7.2f), Quaternion.Euler(90, 0, 0));
            }
            Jars(new Vector3(-12.9f, 0, -11.6f), 3, root, 11);
            Block("Shelf", new Vector3(-13.2f, 1.3f, -10.9f), new Vector3(0.35f, 0.05f, 1.2f), Chapter03Setup.Wood, root, false);
            for (int i = 0; i < 4; i++)
                Cylinder("Bowl", new Vector3(-13.2f, 1.33f, -11.35f + i * 0.3f), 0.1f, 0.06f, Material("Ch4Bowl", new Color(0.85f, 0.85f, 0.8f), 0.6f), root, false);
            Lantern("KitchenLamp", new Vector3(-10.6f, 2.5f, -8.2f), new Color(1f, 0.5f, 0.25f), root, 3.5f, 0.8f);

            // The back door: two planks with a cross bar and a padlock.
            var door = Block("BackDoor", new Vector3(-13.47f, 1.2f, -9.6f), new Vector3(0.1f, 2.4f, 1.3f), Surface("Ch3GateWood", "planks_brown_10", new Color(0.55f, 0.32f, 0.22f), 1.6f, 0.25f, 1.2f), root);
            Block("BackDoorBar", new Vector3(-13.38f, 1.25f, -9.6f), new Vector3(0.08f, 0.12f, 1.5f), Chapter03Setup.Lacquer, root, false);
            Block("Padlock", new Vector3(-13.32f, 1.1f, -9.6f), new Vector3(0.06f, 0.14f, 0.1f), Emissive("Ch3Brass", new Color(0.8f, 0.6f, 0.25f), 0.4f), root, false);
            var find = new GameObject("BackDoorSpot");
            find.transform.SetParent(root, false);
            find.transform.position = new Vector3(-12.7f, 0, -9.6f);
            Explore(find, director, "c4_backdoor", "Xem cánh cửa sau", new Vector3(1.4f, 1.6f, 1.8f));
            var leave = new GameObject("LeaveSpot");
            leave.transform.SetParent(root, false);
            leave.transform.position = new Vector3(-12.6f, 0, -9.6f);
            Explore(leave, director, "c4_leave", "Rời khỏi nơi này", new Vector3(1.4f, 1.6f, 1.8f));
            return door.transform;
        }

        /// <summary>
        /// Lived-in detail for the night house: timber-framed walls, furniture in every room (Tú Bà's sập where she
        /// dozes, chests, shelves, scrolls), a working kitchen, puddles and weeds in the yard, stone lanterns at the hall.
        /// Returns the spot on the sập where Tú Bà lies down.
        /// </summary>
        private static Transform NightDressing(Transform world)
        {
            var root = new GameObject("NightDressing").transform;
            root.SetParent(world, false);
            foreach (string wall in new[] { "KieuRoom/WallS", "KieuRoom/WallE_South", "KieuRoom/WallE_North", "TuBaRoom/WallS", "TuBaRoom/WallW_A",
                         "TuBaRoom/WallW_B", "TuBaRoom/WallW_C", "LedgerOffice/WallN", "LedgerOffice/WallW_South", "LedgerOffice/WallW_North",
                         "Kitchen/KitchenWallN", "Kitchen/KitchenWallE_South", "Kitchen/KitchenWallE_North" })
            {
                var found = world.Find(wall);
                if (found != null) StoryPropKit.TimberFrame(found.gameObject);
            }

            // The outer walls get the same exposed frame on the inside of each room.
            foreach (var (from, to, inward) in new[]
            {
                (new Vector3(-13.4f, 0, 3.1f), new Vector3(-13.4f, 0, 12.4f), Vector3.right),   // Kiều: west
                (new Vector3(-13.4f, 0, 12.4f), new Vector3(-7.6f, 0, 12.4f), Vector3.back),    // Kiều: north
                (new Vector3(13.4f, 0, 3.1f), new Vector3(13.4f, 0, 12.4f), Vector3.left),      // Tú Bà: east
                (new Vector3(7.6f, 0, 12.4f), new Vector3(13.4f, 0, 12.4f), Vector3.back),      // Tú Bà: north
                (new Vector3(13.4f, 0, -12.4f), new Vector3(13.4f, 0, -2.1f), Vector3.left),    // office: east
                (new Vector3(7.6f, 0, -12.4f), new Vector3(13.4f, 0, -12.4f), Vector3.forward), // office: south
                (new Vector3(-13.4f, 0, -12.4f), new Vector3(-9.1f, 0, -12.4f), Vector3.forward) // kitchen: south
            })
                StoryPropKit.WallPanels(from, to, 3.5f, inward, root);

            // Kiều's room: a tea table on mats, a chest at the foot of the bed, a scroll, curtains and a candle.
            StoryPropKit.LowTable(new Vector3(-10.9f, 0.04f, 8.6f), 90f, root, true, 4);
            StoryPropKit.Chest(new Vector3(-12.7f, 0.04f, 8.2f), 90f, root, 0.85f);
            StoryPropKit.Scroll(new Vector3(-13.29f, 1.75f, 6.6f), 90f, root, 1);
            StoryPropKit.Curtain(new Vector3(-10.5f, 2.55f, 12.2f), 2.3f, 1.95f, 180f, new Color(0.5f, 0.16f, 0.16f), root);
            StoryPropKit.Candle(new Vector3(-8.15f, 0.04f, 10.75f), root, 3.2f, 0.8f);

            // Tú Bà's room: the sập she dozes on, chests of money and silks, a scroll of peonies, incense.
            StoryPropKit.Sap(new Vector3(12.75f, 0.04f, 8.2f), 0f, root, 2.1f, 1.15f);
            StoryPropKit.Chest(new Vector3(9.1f, 0.04f, 11.85f), 180f, root);
            StoryPropKit.Chest(new Vector3(12.95f, 0.04f, 3.75f), 270f, root, 0.8f);
            StoryPropKit.Scroll(new Vector3(13.29f, 1.8f, 5.6f), -90f, root, 2, 0.6f, 1.4f);
            StoryPropKit.Candle(new Vector3(8.2f, 0.04f, 8.2f), root, 3f, 0.7f, 1f);
            var sleepSpot = Point("TuBaSleepSpot", new Vector3(12.75f, 0.04f, 7.38f), 0f, root); // forward = towards the pillow

            // Ledger office: shelves of account books, a second shelf of boxes.
            StoryPropKit.Bookshelf(new Vector3(13.2f, 0.04f, -8.9f), -90f, root, 11, 1.8f, 2.2f);
            StoryPropKit.Bookshelf(new Vector3(10.4f, 0.04f, -12.15f), 0f, root, 12, 1.6f, 2f);
            var desk = new GameObject("DeskTop").transform;
            desk.SetParent(root, false);
            StoryPropKit.Abacus(desk, new Vector3(11.55f, 0.95f, -6.6f), 90f);
            StoryPropKit.Candle(new Vector3(8.3f, 0.04f, -5.3f), root, 0f);

            // Kitchen: strings of garlic and chilli, rice sacks, a shelf of bowls and jars.
            StoryPropKit.Strings(new Vector3(-11.9f, 2.95f, -10.7f), root, 21);
            StoryPropKit.Strings(new Vector3(-10.1f, 2.95f, -11.7f), root, 22);
            StoryPropKit.Sacks(new Vector3(-10.1f, 0.03f, -11.75f), 4, root, 23);
            StoryPropKit.WallShelf(new Vector3(-11.4f, 1.55f, -6.92f), 180f, 1.6f, root, 24);
            StoryPropKit.Baskets(new Vector3(-12.8f, 0.03f, -8.6f), 90f, root, 25);

            // Yard: rain left puddles that catch the lanterns; weeds along the south wall and round the well.
            StoryPropKit.Puddle(new Vector3(-2.4f, 0.01f, -6.2f), new Vector2(1.6f, 1f), 20f, root);
            StoryPropKit.Puddle(new Vector3(3.3f, 0.01f, -3.3f), new Vector2(1.2f, 0.8f), -35f, root);
            StoryPropKit.Puddle(new Vector3(-0.6f, 0.025f, 2.4f), new Vector2(0.9f, 0.7f), 60f, root);
            StoryPropKit.Puddle(new Vector3(5.2f, 0.01f, -10.2f), new Vector2(1.4f, 0.9f), 10f, root);
            Grass("Ch4Weeds", root, new Rect(-9f, -12.4f, 16.5f, 13f), 1400, p =>
                (p.y < -11.75f && Mathf.Abs(p.x) > 2.8f && p.x < 7.3f) ||
                (Vector2.Distance(p, new Vector2(-3f, -3f)) is float d && d > 1.05f && d < 1.6f), 404, 0.38f,
                new Color(0.42f, 0.55f, 0.3f), new Color(0.55f, 0.58f, 0.32f), "Ch4Weeds");
            foreach (float x in new[] { -3.7f, 3.7f }) StoryPropKit.StoneLantern(new Vector3(x, 0f, 7.75f), 0f, root);
            return sleepSpot;
        }

        /// <summary>Pools of red lantern light over the courtyard with dark gaps between them to sneak through.</summary>
        private static void NightLights(Transform world)
        {
            var root = new GameObject("NightLights").transform;
            root.SetParent(world, false);
            foreach (var p in new[] { new Vector3(-3f, 3.4f, -2f), new Vector3(3f, 3.4f, -8f), new Vector3(0f, 3.4f, 4f), new Vector3(6.5f, 3.2f, 1.5f), new Vector3(0f, 3.2f, -11.4f) })
            {
                var go = new GameObject("CourtGlow");
                go.transform.SetParent(root, false);
                go.transform.position = p;
                var light = go.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.42f, 0.25f);
                light.range = 6f;
                light.intensity = 2f;
                // Shadowed pools: lamplight stops at walls and pillars, so the dark gaps between pools are real.
                light.shadows = LightShadows.Soft;
                light.shadowStrength = 0.85f;
            }
        }

        private static void Look(string name, Vector3 position, Vector3 target, Transform parent)
        {
            Point(name, position, 0, parent).rotation = Quaternion.LookRotation(target - position);
        }

        private static void Hide(Transform parent, Object player, string label, Vector3 inside, float yaw, string sfx)
        {
            var go = new GameObject("Hide_" + label);
            go.transform.SetParent(parent, false);
            go.transform.position = inside;
            go.layer = 2;
            var box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(1.6f, 1.8f, 1.6f);
            box.center = Vector3.up * 0.9f;
            var spot = go.AddComponent<HideSpot>();
            var point = Point("Inside", inside, yaw, go.transform);
            Set(spot, "_player", player);
            Set(spot, "_inside", point);
            Set(spot, "_label", label);
            Set(spot, "_sfx", sfx);
        }
    }
}
