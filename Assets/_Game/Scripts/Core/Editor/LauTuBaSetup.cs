using System.Collections.Generic;
using System.Linq;
using ThuyKieu.Core.Story;
using ThuyKieu.Environment;
using ThuyKieu.Gameplay.Chapters.LauTuBa;
using ThuyKieu.Gameplay.House;
using ThuyKieu.Gameplay.Sequences;
using ThuyKieu.Gameplay.Stealth;
using ThuyKieu.Interaction;
using ThuyKieu.Player;
using ThuyKieu.Presentation;
using ThuyKieu.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using static ThuyKieu.Core.Editor.LauTuBaKit;
using static ThuyKieu.Core.Editor.StorySceneKit;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Builds the first-person Chapters 3–4 in Tú Bà's house (Chapter03_LauTuBa, Chapter04_LauTuBaDem) and links them
    /// into the chapter chain (Ch2 → Ch3 → Ch4 → Ch5). The older scenes of these chapters are left untouched.
    ///   Menu: ThuyKieu ▸ Lầu Tú Bà ▸ Rebuild Chapters 3–4 (first person)
    ///   Batch: -executeMethod ThuyKieu.Core.Editor.LauTuBaSetup.RebuildBatch
    /// </summary>
    public static class LauTuBaSetup
    {
        public const string Chapter3Scene = "Chapter03_LauTuBa";
        public const string Chapter4Scene = "Chapter04_LauTuBaDem";
        public const string Chapter3Path = "Assets/_Game/Scenes/" + Chapter3Scene + ".unity";
        public const string Chapter4Path = "Assets/_Game/Scenes/" + Chapter4Scene + ".unity";
        private const int MirrorOnlyLayer = 31;

        [MenuItem("ThuyKieu/Lầu Tú Bà/Rebuild Chapters 3–4 (first person)")]
        public static void Rebuild()
        {
            CreateChapter3();
            CreateChapter4();
            LinkChain();
        }

        /// <summary>Batch rebuild; with -onlyChapter4 the Chapter 3 scene is left as it is.</summary>
        public static void RebuildBatch()
        {
            try
            {
                if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-onlyChapter4") >= 0) { CreateChapter4(); LinkChain(); }
                else Rebuild();
                EditorApplication.Exit(0);
            }
            catch (System.Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }

        /// <summary>Stills of both chapters from every LOOK_* viewpoint: -executeMethod ...LauTuBaSetup.LookBatch -lookOut &lt;folder&gt;</summary>
        public static void LookBatch()
        {
            try
            {
                var args = System.Environment.GetCommandLineArgs();
                int index = System.Array.IndexOf(args, "-lookOut");
                string folder = index >= 0 && index + 1 < args.Length ? args[index + 1] : "Tools/LauTuBa/Looks";
                StoryLookCapture.Capture(Chapter3Path, folder);
                StoryLookCapture.Capture(Chapter4Path, folder);
                EditorApplication.Exit(0);
            }
            catch (System.Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }

        // ------------------------------------------------------------------ Chapter 3: afternoon after rain → dusk

        public static void CreateChapter3()
        {
            var spawn = new Vector3(0f, Floor + 0.05f, -18.15f);
            var (scene, s, folder) = Begin(Chapter3Path, 3, "Tú Bà", "chapter_3_lau", Chapter4Scene, spawn, 0f, "after_rain");
            try
            {
                var world = new GameObject("World").transform;
                var house = LauTuBaHouse.Build(world, false, s.Audio, s.Player);
                var nav = Nav(world);
                DayLook(s);
                var sun = Sun("Sun", new Color(1f, 0.84f, 0.66f), 1.05f, new Vector3(30f, -38f, 0f));
                Weather(world, false);
                Probes(world, false);
                Looks(world);
                // By day no lamp is lit; dusk lights them (Chapter03LauWorld).
                foreach (var light in house.YardLanterns.Concat(house.CorridorLanterns)) light.enabled = false;
                foreach (var lamp in new[] { house.KieuLamp, house.TuBaLamp, house.OfficeLamp, house.ArchiveLamp, house.LamLamp }) lamp.Light.enabled = false;

                // People.
                var tuBa = CharacterActor("TuBa", "TuBa", "TuBa", 1.62f, new Vector3(0.3f, Floor, -16.6f), 180f);
                var ma = RiggedActor("MaGiamSinh", "MaGiamSinh", "Assets/_Game/Art/Characters/MaGiamSinh/Models/MaGiamSinh_Rigged.fbx",
                    "Assets/_Game/Art/Characters/MaGiamSinh/Tectures", 1.78f, new Vector3(-1.4f, Floor, -17.3f), 120f, Color.white, StoryAnimationSetup.Controller());
                var hong = CharacterActor("CoHong", "CoHong", "LauXanh3", 1.6f, new Vector3(16.0f, Floor, 2.2f), 180f);
                var lam = CharacterActor("CoLam", "CoLam", "LauXanh1", 1.6f, new Vector3(-17.5f, Floor, -3.6f), 90f);
                var maKieu = RiggedActor("MaKieu", "MaKieu", "Assets/_Game/Art/Characters/ThuyKieu/Models/ThuyKieu_Rigged.fbx",
                    "Assets/_Game/Art/Characters/ThuyKieu/Tectures", 1.64f, new Vector3(-16.5f, 0.12f, 14.8f), 0f, new Color(0.72f, 0.6f, 0.85f), StoryAnimationSetup.Controller());
                var guard = CharacterActor("NguoiCanh", "NguoiCanh", "LinhCanh", 1.76f, new Vector3(0f, Floor, 9.5f), 90f);
                // The house at work in the late afternoon: guests over tea in the hall, a girl pouring, another up on the gallery.
                Patron("KhachTra", GuestRig, GuestTextures, 1.72f, new Vector3(2.64f, Floor, -11.2f), 90f, new Color(0.45f, 0.5f, 0.75f), true, new Vector3(3.3f, Floor, -11.2f));
                Patron("KhachSanh", GuestRig, GuestTextures, 1.75f, new Vector3(8.43f, Floor, -14f), -90f, new Color(0.7f, 0.55f, 0.4f), true, new Vector3(7f, Floor, -14f));
                Patron("CoRotTra", GirlRig(1), GirlTextures(1), 1.58f, new Vector3(3.3f, Floor, -10.35f), 180f, new Color(0.78f, 0.92f, 0.82f), false, new Vector3(3.3f, Floor, -11.2f));
                Patron("CoTrenLau", GirlRig(3), GirlTextures(3), 1.58f, new Vector3(-2.4f, LauTuBaHouse.GalleryFloor, -6.75f), 0f, new Color(0.95f, 0.8f, 0.86f), false, Vector3.zero);
                var director = s.Director;
                Anchor(hong.gameObject, director, "c3_girl_hong", "Nói chuyện với cô gái áo hồng", Vector3.one);
                Anchor(lam.gameObject, director, "c3_girl_lam", "Nói chuyện với cô gái áo lam", Vector3.one);
                Anchor(maKieu.gameObject, director, "c3_makieu_bep", "Nói chuyện với người đàn bà trong bếp", Vector3.one);
                Anchor(Spot("CurtainSpot", new Vector3(-20.55f, Floor + 0.2f, 2.7f), world), director, "c3_room", "Vén rèm cửa sổ", new Vector3(0.7f, 2f, 1.8f));
                Anchor(Spot("GateSpot", new Vector3(0f, Floor, -18.75f), world), director, "c3_gate", "Đẩy thử cổng", new Vector3(3.2f, 2.6f, 0.6f));
                var box = Anchor(Spot("BoxSpot", new Vector3(8.45f, Floor + 0.55f, 13.4f), world), director, "c3_box", "Mở hộp gỗ sơn son", new Vector3(0.7f, 0.7f, 0.8f));
                var drawer = Anchor(Spot("DrawerSpot", new Vector3(11.8f, Floor + 0.35f, 15.55f), world), director, "c3_drawer", "Kéo ngăn bàn sổ sách", new Vector3(1f, 0.6f, 0.4f));
                var cabinet = Anchor(Spot("CabinetSpot", new Vector3(20.25f, Floor + 0.4f, 15.2f), world), director, "c3_cabinet", "Xem tủ khoá đồng", new Vector3(0.6f, 1.6f, 1.4f));

                // The guard: by day he only minds the rear range (the rooms behind the north screen).
                var route = Route(world, "GuardRouteDay",
                    (new Vector3(0f, Floor, 9.5f), 90f, 2f), (new Vector3(10.5f, Floor, 9.5f), 0f, 3f), (new Vector3(10.5f, Floor, -3.5f), 270f, 2f),
                    (new Vector3(2.4f, 0f, 0f), 0f, 3f), (new Vector3(-10.5f, Floor, -3.0f), 90f, 2f), (new Vector3(-10.5f, Floor, 9.5f), 90f, 2.5f));
                var watch = Watch(guard, s, route.Points, route.Pauses, 1.62f, 9f, 40f, true, true, false, GuardLinesDay());
                var tuBaEyes = Watch(tuBa, s, null, null, 1.5f, 7f, 50f, false, false, false, null);
                var zones = new GameObject("RestrictedZones").transform;
                zones.SetParent(world, false);
                Chapter03Setup.Zone(zones, "Phòng sổ sách", new Vector3(14.5f, Floor + 1f, 15f), new Vector3(12.9f, 2.5f, 7.9f));
                Chapter03Setup.Zone(zones, "Phòng Tú Bà", new Vector3(2.5f, Floor + 1f, 15f), new Vector3(10.9f, 2.5f, 7.9f));
                Chapter03Setup.Zone(zones, "Kho vải", new Vector3(-7f, Floor + 1f, 15f), new Vector3(7.9f, 2.5f, 7.9f));

                var ch3 = s.Root.AddComponent<Chapter03LauWorld>();
                Set(ch3, "_director", director);
                Set(ch3, "_navMesh", nav);
                Set(ch3, "_inventory", s.Player.GetComponent<KieuInventory>());
                Set(ch3, "_guard", watch);
                Set(ch3, "_tuBa", tuBa);
                Set(ch3, "_ma", ma);
                Set(ch3, "_hong", hong);
                Set(ch3, "_lam", lam);
                Set(ch3, "_maKieu", maKieu);
                Set(ch3, "_tuBaEyes", tuBaEyes);
                Set(ch3, "_catch", Catch(s, CatchScene.Ending.Drag, watch, tuBaEyes));
                Set(ch3, "_gateLeft", house.GateLeft);
                Set(ch3, "_gateRight", house.GateRight);
                Set(ch3, "_kieuDoor", house.KieuDoor);
                Set(ch3, "_innerDoor", house.InnerDoor);
                Set(ch3, "_officeDoor", house.OfficeDoor);
                Set(ch3, "_tuBaDoor", house.TuBaDoor);
                Set(ch3, "_allDoors", AllDoors(house));
                Set(ch3, "_windowCurtain", LauTuBaHouse.KieuWindowCurtain);
                Set(ch3, "_sun", sun);
                Set(ch3, "_lanterns", house.YardLanterns.Concat(house.CorridorLanterns).Cast<Object>().ToArray());
                Set(ch3, "_dayFills", house.Fills.Cast<Object>().ToArray());
                Set(ch3, "_roomLamps", new[] { house.KieuLamp, house.TuBaLamp, house.OfficeLamp, house.ArchiveLamp, house.LamLamp }.Select(l => (Object)l.Light).ToArray());
                Set(ch3, "_arrivalTuBa", Point("ArrivalTuBa", new Vector3(0.3f, Floor, -16.6f), 180f, world));
                Set(ch3, "_arrivalMa", Point("ArrivalMa", new Vector3(-1.4f, Floor, -17.3f), 120f, world));
                Set(ch3, "_hallSpot", house.HallSpot);
                Set(ch3, "_tuBaInsideB", Point("TuBaInOffice", new Vector3(9.3f, Floor, 15.0f), 90f, world));
                Set(ch3, "_maInsideB", Point("MaInOffice", new Vector3(9.9f, Floor, 16.7f), 150f, world));
                Set(ch3, "_officeDesk", house.Ledger);
                Set(ch3, "_officeDoorOutside", house.OfficeDoorOutside);
                Set(ch3, "_tuBaRoomSpot", Point("TuBaRoomSpot", new Vector3(4.6f, Floor, 14.6f), 90f, world));
                Set(ch3, "_kieuRoomSpawn", Point("KieuRoomSpawn", new Vector3(-15.0f, Floor + 0.05f, 4.4f), 270f, world));
                Set(ch3, "_officeSpawn", Point("OfficeSpawn", new Vector3(11.0f, Floor + 0.05f, 12.4f), 0f, world));
                Set(ch3, "_tuBaRoomSpawn", Point("TuBaRoomSpawn", new Vector3(5.6f, Floor + 0.05f, 14.4f), 270f, world));
                Set(ch3, "_kieuDoorOutside", house.KieuDoorOutside);
                Set(ch3, "_kieuRoom", Box3(world, "KieuRoomVolume", new Vector3(-16.5f, Floor + 1f, 3.75f), new Vector3(8.9f, 2.5f, 8.4f)));
                Set(ch3, "_office", Box3(world, "OfficeVolume", new Vector3(14.5f, Floor + 1f, 15f), new Vector3(12.9f, 2.5f, 7.9f)));
                Set(ch3, "_tuBaRoom", Box3(world, "TuBaRoomVolume", new Vector3(2.5f, Floor + 1f, 15f), new Vector3(10.9f, 2.5f, 7.9f)));
                Set(ch3, "_guardRoute", route.Points);
                Set(ch3, "_boxTarget", box);
                Set(ch3, "_drawerTarget", drawer);
                Set(ch3, "_cabinetTarget", cabinet);

                var sequences = Sequences(s);
                var tutorial = Child<TutorialSequence>(sequences, "tutorial_lau");
                Set(tutorial, "_id", "tutorial_lau");
                Set(tutorial, "_body", "Bạn nhìn bằng chính mắt Kiều.\n\n" +
                    "<color=#E8C27A>Nhìn</color> vào đồ vật hay người rồi bấm <color=#E8C27A>E</color>. Với cửa và then: <color=#E8C27A>bấm E</color> là giật mạnh (kêu to), <color=#E8C27A>giữ E</color> là làm khẽ.\n" +
                    "<color=#E8C27A>Ctrl</color> khom người: bước nhẹ, khó bị thấy. <color=#E8C27A>Shift</color> chạy: nhanh nhưng vang xa. Đá sân ướt vang hơn sàn gỗ.\n\n" +
                    "Dãy nhà sau (phòng Tú Bà, kho vải, phòng sổ sách) là <color=#F08080>khu cấm</color>. Viền đỏ ở mép màn hình là có người đang để ý bạn.");
                Set(tutorial, "_controlsHint", "WASD: đi · Shift: chạy · Ctrl: khom · E: tương tác · Chuột phải: nhìn kỹ");
                var contract = Child<DocumentSequence>(sequences, "document_contract");
                Set(contract, "_id", "document_contract");
                Set(contract, "_view", s.Hud.GetComponent<DocumentView>());
                Set(contract, "_title", "VĂN TỰ BÁN THÂN");
                Set(contract, "_body", "Người bán: <b>Mã Giám Sinh</b>, người huyện Lâm Thanh.\nNgười mua: <b>Tú Bà</b>, chủ lầu xanh Lâm Tri.\n\n" +
                    "Người bị bán: <b>Vương Thúy Kiều</b>, mười lăm tuổi,\nbiết chữ, giỏi đàn, giỏi thơ.\n\nGiá bán: <b>bốn trăm năm mươi lạng bạc</b>.\n" +
                    "Từ nay người mua toàn quyền định đoạt nơi ở\nvà công việc của người bị bán.\n\nĐiểm chỉ làm bằng.");
                Set(contract, "_sealed", true);
                Drawer(sequences);
                Chapter03Setup.Fx(sequences, "panic", "panic", "heartbeat", "none", 3.2f, 0.12f, 3);
                Chapter03Setup.Fx(sequences, "kieu_voice", "spirit", "memory", "none", 2f, 0f, 1);
                // Recorded footsteps and doors, shared with Chapter 4 (§17), without changing what Chapter 3's watchers hear.
                Debug.Log("[LauTuBaSetup] Chapter 3 audio\n" + Ch3AudioSync.Apply(s.Audio));

                Save(scene, Chapter3Path);
            }
            finally { AssetFolder = folder; }
        }

        // ------------------------------------------------------------------ Chapter 4: the night

        public static void CreateChapter4()
        {
            var spawn = new Vector3(-15.5f, Floor + 0.05f, 3.6f);
            var (scene, s, folder) = Begin(Chapter4Path, 4, "Không còn đường lui", "chapter_4_dem", Chapter05Setup.SceneName, spawn, 90f, "rain_house");
            try
            {
                var world = new GameObject("World").transform;
                var house = LauTuBaHouse.Build(world, true, s.Audio, s.Player);
                var nav = Nav(world);
                var moon = StoryArtKit.NightLook(s.Fx, s.CameraComponent, "LTB_SkyNight", 30f, new Vector3(38f, 125f, 0f));
                // The shared night grade (Ch4, Ch5) tints every shadow blue; inside the house the shadows stay warm-neutral so
                // the lantern red carries and the yard reads as cool by its own light. Same exposure and contrast.
                StoryArtKit.Grade(s.Fx, 0.95f, 11f, -2f, -1f, new Color(0.43f, 0.42f, 0.46f), new Color(0.9f, 0.63f, 0.42f));
                // Intensity is what the guards count under the open sky (LightExposure): kept. Only its colour is calmed,
                // so the yard reads as cool against the warm verandas rather than blue-white.
                moon.intensity = 0.5f;
                moon.color = new Color(0.6f, 0.64f, 0.76f);
                // A rain night: low cloud lit faintly by the moon behind it, no clear disc.
                StoryArtKit.UseSky(s.CameraComponent, StoryArtKit.Sky("LTB_SkyRain", "kloppenheim_07_puresky", 0.32f, new Color(0.45f, 0.5f, 0.62f), 40f, Folder));
                RenderSettings.ambientSkyColor = new Color(0.23f, 0.26f, 0.33f);
                RenderSettings.ambientEquatorColor = new Color(0.18f, 0.18f, 0.21f);
                // A little more air at night, greyer than the moonlight, so the far gallery separates from the near veranda.
                RenderSettings.fogDensity = 0.021f;
                RenderSettings.fogColor = new Color(0.085f, 0.09f, 0.11f);
                Weather(world, true);
                Probes(world, true);
                Looks(world);
                // The corridor stays dark until the house wakes (J2).
                foreach (var light in house.CorridorLanterns) light.enabled = false;
                var director = s.Director;

                // People: Tú Bà (at Kiều's door for the opening, then asleep on her sập), the drunk guard, the gate guard,
                // chị Lam behind her padlocked door, Mã Kiều asleep by the stove, guests behind the paper windows.
                // Tú Bà carries no lantern of her own; the guard's is moved into his right hand at runtime (HandLantern).
                var tuBa = CharacterActor("TuBa", "TuBa", "TuBa", 1.62f, house.KieuDoorOutside.position, 270f);
                var guard = CharacterActor("NguoiCanh", "NguoiCanh", "LinhCanh", 1.76f, new Vector3(-6f, Floor, -7.5f), 0f);
                Lantern("GuardLantern", guard.transform.position + guard.transform.right * 0.35f + Vector3.up * 1.1f, new Color(1f, 0.55f, 0.2f), guard.transform, 4.6f, 1.35f);
                var gateGuard = CharacterActor("NguoiCanhCong", "NguoiCanh", "LinhCanh", 1.74f, new Vector3(2.4f, Floor, -17.9f), 0f);
                var lam = CharacterActor("CoLam", "CoLam", "LauXanh1", 1.6f, new Vector3(-12.3f, Floor, -1.6f), 90f);
                var maKieu = RiggedActor("MaKieu", "MaKieu", "Assets/_Game/Art/Characters/ThuyKieu/Models/ThuyKieu_Rigged.fbx",
                    "Assets/_Game/Art/Characters/ThuyKieu/Tectures", 1.64f, new Vector3(-16.45f, 0.16f, 17.6f), 90f, new Color(0.72f, 0.6f, 0.85f), StoryAnimationSetup.Controller());
                foreach (var (name, position, yaw, tint) in new[] { ("KhachA", new Vector3(12.75f, Floor, -6.2f), 90f, new Color(0.55f, 0.6f, 0.85f)), ("KhachB", new Vector3(12.8f, Floor, -1.8f), 80f, new Color(0.8f, 0.6f, 0.45f)) })
                    RiggedActor(name, "Khach", "Assets/_Game/Art/Characters/MaGiamSinh/Models/MaGiamSinh_Rigged.fbx", "Assets/_Game/Art/Characters/MaGiamSinh/Tectures", 1.74f, position, yaw, tint, StoryAnimationSetup.Controller());
                // Late at night only one girl is still up, by a lantern on the north gallery, far above the yard.
                Patron("CoTrenLauDem", GirlRig(3), GirlTextures(3), 1.58f, new Vector3(4.0f, LauTuBaHouse.GalleryFloor, 8.5f), 180f, new Color(0.95f, 0.8f, 0.86f), false, Vector3.zero);
                var figure = MirrorFigure(world);

                var sleeper = tuBa.gameObject.AddComponent<TuBaSleeper>();
                Set(sleeper, "_actor", tuBa);
                Set(sleeper, "_audio", s.Audio);
                Set(sleeper, "_hud", s.Hud);
                // The key ring on the hip that faces the room when she lies on her sập (head to +Z, the slats to the west: her
                // left hip), and the hand's target on the keys themselves, wherever her pose puts them.
                var keyRing = LauTuBaKeyRing.AtBelt(tuBa, true, out var keyGrip, out var keyBunch);
                var theftSpot = new GameObject("KeyTheft");
                theftSpot.transform.SetParent(world, false);
                theftSpot.transform.position = keyGrip.position;
                theftSpot.layer = 2;
                var theftBox = theftSpot.AddComponent<BoxCollider>();
                theftBox.isTrigger = true;
                theftBox.size = new Vector3(0.42f, 0.34f, 0.42f);
                var theft = theftSpot.AddComponent<KeyTheft>();
                Set(theft, "_sleeper", sleeper);
                Set(theft, "_inventory", s.Player.GetComponent<KieuInventory>());
                Set(theft, "_keyRing", keyRing);
                Set(theft, "_bunch", keyBunch.gameObject);
                Set(theft, "_grip", keyGrip);
                Set(theft, "_audio", s.Audio);
                Set(theft, "_player", s.Player);
                // Decided with the team: 12 s of work on her out-breaths in all (about 20–30 s with the waiting), in three stages.
                Set(theft, "_seconds", 12f);
                // The close-up: Kiều leans in over the keys (no hand shown), the bunch moves with the work.
                var theftView = theftSpot.AddComponent<KeyTheftView>();
                Set(theftView, "_camera", s.Camera);
                Set(theftView, "_actor", tuBa);
                Set(theftView, "_sleeper", sleeper);
                Set(theftView, "_ringRoot", keyRing.transform);
                Set(theftView, "_bunch", keyBunch);
                Set(theftView, "_grip", keyGrip);
                Set(theftView, "_hintMaterial", LauTuBaKit.UnlitMat("LTB_PullHint", new Color(0.96f, 0.86f, 0.6f, 0.75f)));
                Set(theft, "_view", theftView);

                var routes = new GameObject("GuardRoutes").transform;
                routes.SetParent(world, false);
                // Before the alarm (docs/concept/CLAUDE_CH4_GUARDS_DISTRACTION_REVISION.md §2): the guard stands aslant Tú Bà's
                // door, east of it, looking west down the corridor the way anyone from the west wing must come; the gate guard
                // keeps the gate. A pebble draws the guard off his post; he checks, looks round, and walks back.
                var post = Route(routes, "GuardPost", (new Vector3(2.6f, Floor, 9.6f), 262f, 999f));
                var gatePost = Route(routes, "GatePost", (new Vector3(2.4f, Floor, -17.9f), 0f, 999f));
                // After the alarm: the guard walks the corridor past the store, Tú Bà's door and the office, down the east
                // veranda a little and back; the gate guard goes from the gate through the hall, along the west veranda past
                // Kiều's door to the kitchen passage and back. Their rounds cross at the north-west corner, at different times.
                var corridor = Route(routes, "GuardAlarm",
                    (new Vector3(-4.0f, Floor, 9.7f), 270f, 3f), (new Vector3(0f, Floor, 10.0f), 0f, 2f), (new Vector3(5.5f, Floor, 9.6f), 90f, 1.5f),
                    (new Vector3(10.6f, Floor, 9.9f), 0f, 3f), (new Vector3(10.5f, Floor, 5.0f), 180f, 2f));
                var gateRound = Route(routes, "GateAlarm",
                    (new Vector3(2.4f, Floor, -17.9f), 0f, 3f), (new Vector3(0f, Floor, -12.5f), 0f, 2f), (new Vector3(-6f, Floor, -7.5f), 0f, 1f),
                    (new Vector3(-10.5f, Floor, -3.0f), 0f, 2f), (new Vector3(-10.5f, Floor, 9.0f), 270f, 3f), (new Vector3(-15.5f, Floor, 9.6f), 270f, 2f),
                    (new Vector3(-10.5f, Floor, 0.5f), 180f, 1f));
                var guardWatch = Watch(guard, s, post.Points, post.Pauses, 1.62f, 9f, 60f, true, true, true, GuardLinesNight());
                var gateWatch = Watch(gateGuard, s, gatePost.Points, gatePost.Pauses, 1.5f, 7.5f, 60f, true, true, false, GuardLinesNight());
                // Chapter 4's eyes (§6): 120° in front, side vision to 180° within 4 m, anyone plainly in view within 2 m
                // (front or side) made out within a second however still, crouched or dark, and a bump from behind felt.
                // A pebble's lure: he looks round eight spots within 3 m of where it fell before he goes back (45–60 s away from
                // his post on the real routes), and stops falling for more than three in five minutes.
                foreach (var night in new[] { guardWatch, gateWatch })
                {
                    Set(night, "_peripheralHalfAngle", 90f);
                    Set(night, "_peripheralRange", 4f);
                    Set(night, "_nearRange", 2.1f);
                    Set(night, "_nearFloor", 1.4f);
                    Set(night, "_touchSense", true);
                    Set(night, "_lureChecks", 8);
                    night.gameObject.AddComponent<DoorUser>();
                }
                var tuBaWatch = Watch(tuBa, s, null, null, 1.5f, 5.5f, 45f, true, false, false, TuBaLines());
                Set(tuBaWatch, "_ally", guardWatch);
                Set(tuBaWatch, "_investigateSpeed", 1.0f);
                // Doors the watchers leave alone: Kiều's room (hers), chị Lam's and Hồng's (the story's), the gates and the back door.
                foreach (var door in new[] { house.KieuDoor, house.LamDoor, house.HongDoor, house.GateLeft, house.GateRight, house.BackDoor }) Set(door, "_npcCanUse", false);
                // Kiều's room is the one safe place at night: the room's own box, watched from the porch outside its door.
                var safe = Box3(world, "KieuSafeRoom", new Vector3(-16.5f, Floor + 1f, 3.75f), new Vector3(8.9f, 2.5f, 8.4f)).gameObject.AddComponent<SafeZone>();
                Set(safe, "_volume", safe.GetComponent<BoxCollider>());
                Set(safe, "_porch", house.KieuDoorOutside);
                Set(safe, "_player", s.Player);
                // Night ears: hearing follows what each one is doing, and soft sounds must come three times to bring them over.
                foreach (var night in new[] { guardWatch, gateWatch, tuBaWatch }) Set(night, "_carefulListener", true);
                // Recorded Foley for this chapter only (footsteps by floor and gait, doors, bolts): its ids win over the shared sounds.
                var foley = AssetDatabase.LoadAssetAtPath<ThuyKieu.Presentation.FoleyBank>(Ch4Foley.BankPath);
                if (foley != null) Set(s.Audio, "_foley", foley);
                else Debug.LogWarning("[LauTuBaSetup] " + Ch4Foley.BankPath + " missing: Chapter 4 keeps the shared sounds (run Ch4Foley.BuildBatch first)");
                var zones = new GameObject("NightZones").transform;
                zones.SetParent(world, false);
                // At night anywhere outside Kiều's own room is somewhere she should not be; inside it the watchers ignore her.
                Chapter03Setup.Zone(zones, "Ngoài phòng", new Vector3(4.5f, 1.5f, 0f), new Vector3(33.2f, 4f, 38.6f), true);
                Chapter03Setup.Zone(zones, "Ngoài phòng", new Vector3(-16.55f, 1.5f, -9.75f), new Vector3(9.1f, 4f, 18.6f), true);
                Chapter03Setup.Zone(zones, "Ngoài phòng", new Vector3(-16.55f, 1.5f, 13.5f), new Vector3(9.1f, 4f, 11.2f), true);
                NoiseZone("CourtyardRain", new Vector3(0f, 1.5f, 1f), new Vector3(18.4f, 4f, 14.4f), 0.8f, zones);
                NoiseZone("GuestsMusic", new Vector3(15.5f, 1.5f, -4.5f), new Vector3(12f, 4f, 9.2f), 0.6f, zones);

                var lamNodes = new[]
                {
                    Point("LamNode0", new Vector3(-10.7f, Floor, -3.0f), 90f, world),
                    Point("LamNode1", new Vector3(-10.5f, Floor, 4.0f), 0f, world),
                    Point("LamNode2", new Vector3(-16.0f, Floor, 9.6f), 0f, world),
                    Point("LamNode3", new Vector3(-16.5f, 0.12f, 14.6f), 180f, world)
                };
                var companion = lam.gameObject.AddComponent<LamCompanion>();
                Set(companion, "_lam", lam);
                Set(companion, "_player", s.Player);
                Set(companion, "_nodes", lamNodes.Cast<Object>().ToArray());
                Set(companion, "_watchers", new Object[] { guardWatch, gateWatch, tuBaWatch });
                Set(companion, "_hud", s.Hud);
                var lamTalk = Anchor(lam.gameObject, director, "c4_colam", "Nói với chị Lam", Vector3.one, true);
                Anchor(Spot("CabinetSpot", new Vector3(20.25f, Floor + 0.4f, 15.2f), world), director, "c4_cabinet", "Tra chìa mở tủ khoá đồng", new Vector3(0.6f, 1.6f, 1.4f));
                Anchor(Spot("DrawerSpot", new Vector3(11.8f, Floor + 0.35f, 15.55f), world), director, "c4_ledger", "Xé trang sổ nợ trong ngăn bàn", new Vector3(1f, 0.6f, 0.4f));

                var ch4 = s.Root.AddComponent<Chapter04LauWorld>();
                Set(ch4, "_director", director);
                Set(ch4, "_navMesh", nav);
                Set(ch4, "_inventory", s.Player.GetComponent<KieuInventory>());
                Set(ch4, "_steps", s.Player.GetComponent<FirstPersonFootsteps>());
                var pebbles = s.Player.gameObject.AddComponent<PebbleThrow>();
                Set(pebbles, "_player", s.Player);
                Set(pebbles, "_camera", s.CameraComponent);
                Set(pebbles, "_audio", s.Audio);
                Set(pebbles, "_surfaces", 1 << EnvironmentLayer);
                Set(pebbles, "_hearingRadius", 10f);
                Set(pebbles, "_capacity", 3);
                Set(pebbles, "_pebbleMaterial", Mat("LTB_Bead", new Color(0.5f, 0.49f, 0.46f), 0.35f));
                Set(ch4, "_pebbles", pebbles);
                Set(ch4, "_pebblePile", PebbleHeap(world, s, pebbles, new Vector3(-11.55f, Floor, 6.45f)));
                Set(ch4, "_safeZone", safe);
                Set(ch4, "_guard", guardWatch);
                Set(ch4, "_gateGuard", gateWatch);
                Set(ch4, "_tuBaWatch", tuBaWatch);
                Set(ch4, "_catch", Catch(s, CatchScene.Ending.Shove, guardWatch, gateWatch, tuBaWatch));
                Set(ch4, "_tuBa", tuBa);
                Set(ch4, "_sleeper", sleeper);
                Set(ch4, "_keyTheft", theft);
                Set(ch4, "_lam", lam);
                Set(ch4, "_lamCompanion", companion);
                Set(ch4, "_maKieu", maKieu);
                Set(ch4, "_mirrorFigure", figure);
                Set(ch4, "_kieuDoor", house.KieuDoor);
                Set(ch4, "_lamDoor", house.LamDoor);
                Set(ch4, "_hongDoor", house.HongDoor);
                Set(ch4, "_tuBaDoor", house.TuBaDoor);
                Set(ch4, "_innerDoor", house.InnerDoor);
                Set(ch4, "_officeDoor", house.OfficeDoor);
                Set(ch4, "_backDoor", house.BackDoor);
                Set(ch4, "_allDoors", AllDoors(house));
                Set(ch4, "_hairpin", house.Hairpin);
                Set(ch4, "_lamps", new Object[] { house.KieuLamp, house.LamLamp, house.TuBaLamp, house.OfficeLamp, house.ArchiveLamp });
                Set(ch4, "_officeLamp", house.OfficeLamp);
                Set(ch4, "_archiveLamp", house.ArchiveLamp);
                Set(ch4, "_corridorLanterns", house.CorridorLanterns.Cast<Object>().ToArray());
                Set(ch4, "_mirrorFace", house.MirrorFace);
                Set(ch4, "_mirrorCloth", house.MirrorCloth);
                Set(ch4, "_lamTalk", lamTalk);
                Set(ch4, "_spawnRoom", Point("SpawnRoom", new Vector3(-15.5f, Floor + 0.05f, 3.6f), 90f, world));
                Set(ch4, "_spawnStore", Point("SpawnStore", new Vector3(-7.0f, Floor + 0.05f, 13.6f), 90f, world));
                Set(ch4, "_spawnArchive", Point("SpawnArchive", new Vector3(18.0f, Floor + 0.05f, 13.8f), 0f, world));
                Set(ch4, "_sapFeet", house.SapFeet);
                Set(ch4, "_tuBaDoorInside", house.TuBaDoorInside);
                Set(ch4, "_tuBaDoorOutside", house.TuBaDoorOutside);
                Set(ch4, "_kieuDoorOutside", house.KieuDoorOutside);
                Set(ch4, "_hallSpot", house.HallSpot);
                Set(ch4, "_officeDoorOutside", house.OfficeDoorOutside);
                Set(ch4, "_officeInside", house.OfficeInside);
                Set(ch4, "_storeSlats", house.StoreSlats);
                Set(ch4, "_cabinetFront", house.Cabinet);
                Set(ch4, "_kitchen", Point("KitchenWait", new Vector3(-16.8f, 0.17f, 14.8f), 270f, world));
                Set(ch4, "_lamRoom", Point("LamRoom", new Vector3(-17.5f, Floor, -3.6f), 90f, world));
                Set(ch4, "_j2Zone", Box3(world, "J2Zone", new Vector3(0f, Floor + 1f, 7.5f), new Vector3(42f, 3f, 7.4f)));
                Set(ch4, "_tuBaRoom", Box3(world, "TuBaRoomVolume", new Vector3(2.5f, Floor + 1f, 15f), new Vector3(10.9f, 2.5f, 7.9f)));
                Set(ch4, "_storeRoom", Box3(world, "StoreVolume", new Vector3(-7f, Floor + 1f, 15f), new Vector3(7.9f, 2.5f, 7.9f)));
                Set(ch4, "_guestShadow", ShadowOnPaper("GuestShadow", new Vector3(11.96f, Floor + 1.35f, -6.2f), 90f, new Vector3(0.75f, 0f, 0f), new Vector3(-0.75f, 0f, 0f), new Vector2(0.62f, 1.05f), world));
                Set(ch4, "_doorShadow", ShadowOnPaper("TuBaDoorShadow", new Vector3(0f, Floor + 1.15f, 11.1f), 0f, new Vector3(-0.55f, 0f, 0f), new Vector3(0.35f, 0f, 0f), new Vector2(0.7f, 1.55f), world));
                SetRound(ch4, "_guardPost", post);
                SetRound(ch4, "_guardAlarm", corridor);
                SetRound(ch4, "_gatePost", gatePost);
                SetRound(ch4, "_gateAlarm", gateRound);

                var sequences = Sequences(s);
                var tutorial = Child<TutorialSequence>(sequences, "tutorial_dem");
                Set(tutorial, "_id", "tutorial_dem");
                Set(tutorial, "_body", "Đêm nay ngoài phòng bạn là <color=#F08080>nơi không được ở</color>.\n\n" +
                    "Nghe trước khi nhìn: tiếng bước chân, ánh đèn lồng lắc lư, tiếng ngáy của Tú Bà. <color=#E8C27A>Tiếng ngáy ngừng</color> là mụ sắp tỉnh.\n" +
                    "Bóng tối che bạn, ánh đèn làm lộ bạn. Khom người, đi chậm, <color=#E8C27A>giữ E</color> để mở cửa và nâng then thật khẽ.\n" +
                    "<color=#E8C27A>Q</color>: ném sỏi. Lính đi tới chỗ viên sỏi rơi, không phải chỗ bạn đứng. Hết sỏi thì nhặt ở đống sỏi cạnh cửa phòng.\n" +
                    "Phòng mình là nơi duy nhất an toàn: không ai vào đó bắt bạn.\n\n" +
                    "Bị đuổi: cắt tầm nhìn ở góc tường, cột, rèm rồi mới trốn. <color=#F08080>Ai thấy bạn chui vào chỗ nấp sẽ tìm ra bạn.</color>");
                Set(tutorial, "_controlsHint", "WASD: đi · Shift: chạy · Ctrl: khom · E: tương tác (giữ: làm khẽ) · Chuột phải: nhìn kỹ");
                var deal = Child<DocumentSequence>(sequences, "document_deal");
                Set(deal, "_id", "document_deal");
                Set(deal, "_view", s.Hud.GetComponent<DocumentView>());
                Set(deal, "_title", "GIAO KÈO");
                Set(deal, "_body", Chapter04Setup.DealText);
                Set(deal, "_sealed", true);
                Drawer(sequences);
                Chapter03Setup.Fx(sequences, "kieu_voice", "spirit", "memory", "none", 2f, 0f, 1);

                Save(scene, Chapter4Path);
            }
            finally { AssetFolder = folder; }
        }

        // ------------------------------------------------------------------ review stills

        /// <summary>Eye-level views of the places that matter, shot without Play Mode (StoryLookCapture shoots every LOOK_*).</summary>
        private static void Looks(Transform world)
        {
            float eye = Floor + 1.56f;
            foreach (var (name, from, to) in new[]
            {
                ("LOOK_01_Hero", new Vector3(-11.4f, eye, 5.6f), new Vector3(8f, 1.2f, -3f)),
                ("LOOK_02_KieuRoom", new Vector3(-12.6f, eye, 4.4f), new Vector3(-20f, Floor + 1.1f, 6.2f)),
                ("LOOK_03_KieuWindow", new Vector3(-18.6f, eye, 2.7f), new Vector3(-21.5f, 1.6f, 2.7f)),
                ("LOOK_04_Corridor", new Vector3(-11f, eye, 9.5f), new Vector3(21f, Floor + 1f, 9.5f)),
                ("LOOK_05_StoreSlats", new Vector3(-3.7f, eye, 15.4f), new Vector3(-1.9f, Floor + 0.6f, 15.6f)),
                ("LOOK_06_TuBaRoom", new Vector3(0.1f, eye, 11.6f), new Vector3(-1.9f, Floor + 0.5f, 16f)),
                ("LOOK_07_Archive", new Vector3(15.9f, eye, 12.0f), new Vector3(20.5f, Floor + 1.2f, 15.6f)),
                ("LOOK_08_Office", new Vector3(8.6f, eye, 11.8f), new Vector3(12f, Floor + 0.8f, 16.5f)),
                ("LOOK_09_Kitchen", new Vector3(-12.0f, 1.68f, 12.0f), new Vector3(-19f, 0.9f, 16.5f)),
                ("LOOK_10_Courtyard", new Vector3(10.5f, eye, -8.0f), new Vector3(-8f, 0.8f, 6f)),
                ("LOOK_11_Hall", new Vector3(0f, eye, -8.0f), new Vector3(0f, Floor + 1.2f, -19f)),
                ("LOOK_12_Alley", new Vector3(-22.9f, 1.6f, -15f), new Vector3(-22.9f, 1.0f, 16f)),
                ("LOOK_13_LamRoom", new Vector3(-12.6f, eye, -3.0f), new Vector3(-19f, Floor + 0.6f, -6.5f)),
                ("LOOK_14_WestVeranda", new Vector3(-10.5f, eye, -8.5f), new Vector3(-10.5f, Floor + 1.2f, 8f)),
                ("LOOK_15_HallToYard", new Vector3(0f, eye, -17.4f), new Vector3(0f, 1.0f, 2.5f)),
                ("LOOK_16_HongRoom", new Vector3(12.6f, eye, 5.6f), new Vector3(20f, Floor + 1.0f, 2f)),
                ("LOOK_17_TuBaBed", new Vector3(6.4f, eye, 13.4f), new Vector3(-1.9f, Floor + 0.6f, 16.2f)),
                // Close-ups of the pieces nearest the player's eye: furniture, railing, door, pillar.
                ("LOOK_18_Chair", new Vector3(-17.0f, Floor + 1.15f, -14.2f), new Vector3(-18.0f, Floor + 0.5f, -15.6f)),
                ("LOOK_19_Table", new Vector3(4.6f, Floor + 1.35f, -9.6f), new Vector3(3.3f, Floor + 0.6f, -11.2f)),
                ("LOOK_20_Sap", new Vector3(0.2f, Floor + 1.3f, 13.2f), new Vector3(-1.9f, Floor + 0.35f, 15.4f)),
                ("LOOK_21_Railing", new Vector3(-10.6f, Floor + 1.1f, 0.0f), new Vector3(-9.12f, Floor + 0.4f, 1.6f)),
                ("LOOK_22_Door", new Vector3(-10.0f, Floor + 1.4f, 4.0f), new Vector3(-12f, Floor + 1.1f, 5.6f)),
                ("LOOK_23_Pillar", new Vector3(-10.4f, Floor + 1.3f, 0.9f), new Vector3(-9.12f, Floor + 1.6f, 2.3f)),
                ("LOOK_24_Desk", new Vector3(10.3f, Floor + 1.4f, 14.2f), new Vector3(11.8f, Floor + 0.6f, 16f)),
                ("LOOK_25_HallGroup", new Vector3(5.2f, Floor + 1.4f, -11.8f), new Vector3(7f, Floor + 0.6f, -14f)),
                // Whole-house views (from the yard into the hall, across the yard to the far range and what rises above
                // it), the sập from a side the lamp does not hide, and the eave, beam and capital overhead.
                ("LOOK_26_YardToHall", new Vector3(1.2f, 1.6f, 4.0f), new Vector3(0f, Floor + 1.4f, -14f)),
                ("LOOK_27_AcrossYard", new Vector3(-2.0f, eye, -8.2f), new Vector3(2f, 3.5f, 11f)),
                ("LOOK_28_SapClear", new Vector3(-2.6f, Floor + 1.25f, 12.0f), new Vector3(-1.9f, Floor + 0.35f, 15.4f)),
                ("LOOK_29_EaveCorner", new Vector3(-10.6f, eye, -3.0f), new Vector3(-9.0f, Floor + 2.8f, -1.5f)),
                // Standing water and the drip line close up, from the yard.
                ("LOOK_30_Puddle", new Vector3(1.6f, 1.6f, -0.9f), new Vector3(-0.8f, 0f, -3.6f))
            })
                Point(name, from, 0f, world).rotation = Quaternion.LookRotation(to - from);
        }

        // ------------------------------------------------------------------ chain

        /// <summary>Chapter 2 now hands over to the first-person Chapter 3 (one field in the Chapter 2 scene).</summary>
        public static void LinkChain()
        {
            var scene = EditorSceneManager.OpenScene(Chapter02Setup.ScenePath);
            var director = Object.FindAnyObjectByType<StoryDirector>();
            if (director != null)
            {
                Set(director, "_nextScene", Chapter3Scene);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            EditorSceneManager.OpenScene(Chapter3Path);
        }

        // ------------------------------------------------------------------ shared setup

        private static (UnityEngine.SceneManagement.Scene, Systems, string) Begin(string path, int chapter, string title, string knot, string next, Vector3 spawn, float yaw, string ambience)
        {
            // Only these two generated scenes are replaced; the older Chapter 3–4 scenes are never touched.
            if (System.IO.File.Exists(path)) AssetDatabase.DeleteAsset(path);
            string folder = AssetFolder;
            AssetFolder = Folder;
            System.IO.Directory.CreateDirectory(Folder);
            var scene = NewScene(path);
            var s = CreateSystems(chapter, title, knot, next, spawn, yaw, ambience);
            FirstPerson(s);
            return (scene, s, folder);
        }

        private static void FirstPerson(Systems s)
        {
            Set(s.Player, "_firstPerson", true);
            Set(s.Hud, "_compact", true);
            Set(s.Player, "walkSpeed", 1.55f);
            Set(s.Player, "runSpeed", 3.8f);
            Set(s.Player, "_crouchSpeed", 0.95f);
            Set(s.Player, "_allowCrouch", true);
            if (s.Player.GetComponent<KieuInventory>() == null) s.Player.gameObject.AddComponent<KieuInventory>();
            var steps = s.Player.gameObject.AddComponent<FirstPersonFootsteps>();
            Set(steps, "_audio", s.Audio);
            Set(s.Camera, "_firstPerson", true);
            Set(s.Camera, "_body", s.Player);
            Set(s.Camera, "_fov", 72f);
            Set(s.Camera, "_dialogueFov", 0f);
            Set(s.Camera, "minPitch", -80f);
            Set(s.Camera, "maxPitch", 80f);
            Set(s.Camera, "mouseSensitivity", 0.1f);
            s.CameraComponent.nearClipPlane = 0.04f;
            s.CameraComponent.fieldOfView = 72f;
            s.CameraComponent.cullingMask = ~(1 << MirrorOnlyLayer);
            Set(s.Interaction, "_lookOnly", true);
            Set(s.Interaction, "detectionOrigin", s.CameraComponent.transform);
            Set(s.Interaction, "detectionHeight", 0f);
            Set(s.Interaction, "interactionDistance", 2.3f);
            // No cinematic dialogue shots: conversations happen through her eyes.
            Set(s.Director, "_shots", null);
            Object.DestroyImmediate(s.Shots.gameObject);
            Set(s.Observe, "_normalFov", 72f);
            Set(s.Observe, "_observeFov", 44f);
            var reticle = s.CameraComponent.gameObject.AddComponent<FirstPersonReticle>();
            Set(reticle, "_interaction", s.Interaction);
            Set(reticle, "_camera", s.CameraComponent);
        }

        private static NarrowNavMesh Nav(Transform world)
        {
            var go = new GameObject("NarrowNavMesh");
            go.transform.SetParent(world, false);
            var nav = go.AddComponent<NarrowNavMesh>();
            // The whole 42 × 38 m house and the alley.
            Set(nav, "_centre", new Vector3(-1.8f, 1.5f, 0f));
            Set(nav, "_size", new Vector3(47.6f, 6f, 41f));
            return nav;
        }

        private static void DayLook(Systems s)
        {
            // Overcast late afternoon after rain: soft warm light, cool grey shadows, wet surfaces catching the sky.
            StoryArtKit.Grade(s.Fx, 0.6f, 9f, -2f, 6f, new Color(0.52f, 0.51f, 0.52f), new Color(0.8f, 0.66f, 0.5f));
            StoryArtKit.UseSky(s.CameraComponent, StoryArtKit.Sky("LTB_SkyDay", "kloppenheim_06_puresky", 0.85f, new Color(0.66f, 0.66f, 0.68f), 210f, Folder));
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.76f, 0.76f, 0.78f);
            // Warm from the walls and floors: shadows indoors read brown, not blue.
            RenderSettings.ambientEquatorColor = new Color(0.68f, 0.58f, 0.47f);
            RenderSettings.ambientGroundColor = new Color(0.42f, 0.34f, 0.26f);
            RenderSettings.ambientLight = new Color(0.62f, 0.6f, 0.58f);
            RenderSettings.fogColor = new Color(0.58f, 0.56f, 0.55f);
            RenderSettings.fogDensity = 0.016f;
        }

        /// <summary>Rain only where there is sky (the yard and the alley), drips along every eave, splashes on the stones.</summary>
        private static void Weather(Transform world, bool night)
        {
            var root = new GameObject("Weather").transform;
            root.SetParent(world, false);
            if (night)
            {
                Rain(root, "RainYard", new Vector3(0f, 9f, 1f), new Vector3(16.9f, 12.9f), 2200);
                Rain(root, "RainAlley", new Vector3(-22.9f, 9f, 0f), new Vector3(3.4f, 38.8f), 600);
                Splashes(root, "SplashYard", new Vector3(0f, 0.03f, 1f), new Vector3(16.9f, 12.9f), 320);
                Splashes(root, "SplashAlley", new Vector3(-22.9f, 0.03f, 0f), new Vector3(3.3f, 38.5f), 170);
            }
            // Eaves drip in both chapters (Chapter 3 is just after the rain).
            // From the upper roofs' tile ends, about 5.7 m up.
            float eave = 5.65f;
            Drips(root, "DripWest", new Vector3(-8.55f, eave, 1f), new Vector3(0.1f, 13.2f), night ? 90 : 22);
            Drips(root, "DripEast", new Vector3(8.55f, eave, 1f), new Vector3(0.1f, 13.2f), night ? 90 : 22);
            Drips(root, "DripSouth", new Vector3(0f, eave, -5.55f), new Vector3(17.2f, 0.1f), night ? 80 : 18);
            Drips(root, "DripNorth", new Vector3(0f, eave, 7.55f), new Vector3(17.2f, 0.1f), night ? 80 : 18);
        }

        private static void Rain(Transform parent, string name, Vector3 centre, Vector2 area, int rate)
        {
            var holder = new GameObject(name + "Root").transform;
            holder.SetParent(parent, false);
            holder.position = centre - Vector3.up * 9f;
            var ps = StoryArtKit.Rain(name, holder, rate);
            var main = ps.main;
            main.playOnAwake = true;
            main.prewarm = true;
            main.loop = true;
            var shape = ps.shape;
            shape.scale = new Vector3(area.x, area.y, 0.5f);
            // Not a curtain of white lines: thinner, shorter, fainter streaks that differ drop to drop, falling a little
            // aslant in a light wind, and fading out within a few metres of the eye so the near rain never veils the view.
            main.startSize = new ParticleSystem.MinMaxCurve(0.009f, 0.02f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(13f, 19f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.66f, 0.72f, 0.82f, 0.12f), new Color(0.8f, 0.85f, 0.94f, 0.32f));
            var drift = ps.velocityOverLifetime;
            drift.enabled = true;
            drift.space = ParticleSystemSimulationSpace.World;
            drift.x = new ParticleSystem.MinMaxCurve(0.5f, 1.3f);
            drift.y = new ParticleSystem.MinMaxCurve(0f, 0f);
            drift.z = new ParticleSystem.MinMaxCurve(-0.35f, 0.35f);
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.velocityScale = 0.024f;
            renderer.sharedMaterial = RainMaterial(renderer.sharedMaterial);
        }

        private static Material RainMaterial(Material shared)
        {
            string path = Folder + "/LTB_Rain.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool create = material == null;
            if (create) material = new Material(shared);
            const float near = 0.8f, far = 4f;
            material.SetFloat("_CameraFadingEnabled", 1f);
            material.SetFloat("_CameraNearFadeDistance", near);
            material.SetFloat("_CameraFarFadeDistance", far);
            material.SetVector("_CameraFadeParams", new Vector4(near, 1f / (far - near), 0f, 0f));
            material.EnableKeyword("_FADING_ON");
            if (create) AssetDatabase.CreateAsset(material, path);
            else EditorUtility.SetDirty(material);
            return material;
        }

        private static void Splashes(Transform parent, string name, Vector3 centre, Vector2 area, int rate)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = centre;
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.22f;
            main.startSpeed = 0.4f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
            main.startColor = new Color(0.8f, 0.86f, 0.95f, 0.35f);
            main.maxParticles = rate;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.6f;
            var emission = ps.emission;
            emission.rateOverTime = rate;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(area.x, 0.01f, area.y);
            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.4f), new Keyframe(1f, 1.4f)));
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
            renderer.sharedMaterial = SplashMaterial();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        private static void Drips(Transform parent, string name, Vector3 centre, Vector2 line, int rate)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = centre;
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.8f;
            main.startSpeed = 0.2f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.035f);
            main.startColor = new Color(0.78f, 0.85f, 0.95f, 0.5f);
            main.gravityModifier = 1f;
            main.maxParticles = rate * 2;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.prewarm = true;
            var emission = ps.emission;
            emission.rateOverTime = rate;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(line.x, 0.01f, line.y);
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.velocityScale = 0.05f;
            renderer.lengthScale = 1.2f;
            renderer.sharedMaterial = DropMaterial();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        private static Material DropMaterial()
        {
            string path = Folder + "/LTB_Drop.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetTexture("_BaseMap", StoryArtKit.GeneratedTexture("LTB_DropDot", StoryArtKit.DrawDot, 32));
            material.SetColor("_BaseColor", new Color(0.8f, 0.86f, 0.95f, 0.6f));
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)RenderQueue.Transparent;
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Material SplashMaterial()
        {
            string path = Folder + "/LTB_Splash.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetTexture("_BaseMap", StoryArtKit.GeneratedTexture("LTB_SplashRing", DrawRing, 64));
            material.SetColor("_BaseColor", new Color(0.85f, 0.9f, 1f, 0.5f));
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)RenderQueue.Transparent;
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void DrawRing(Color32[] pixels, int size)
        {
            float r = size * 0.5f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r)) / r;
                    float a = Mathf.Clamp01(1f - Mathf.Abs(d - 0.75f) * 6f);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
        }

        /// <summary>Wet stones and lacquer need something to reflect: two probes rendered once when the scene starts.</summary>
        private static void Probes(Transform world, bool night)
        {
            // The yard and the corridor, and one for each block of rooms so glossy wood and lacquer indoors reflect warm
            // rooms, not the sky.
            foreach (var (name, centre, size) in new[] { ("ProbeYard", new Vector3(0f, 1.6f, 0f), new Vector3(24.4f, 6f, 20.4f)), ("ProbeCorridor", new Vector3(0f, Floor + 1.4f, 9.5f), new Vector3(42f, 3.5f, 3.1f)),
                ("ProbeRear", new Vector3(0f, Floor + 1.5f, 15f), new Vector3(42f, 3.2f, 8f)), ("ProbeWest", new Vector3(-16.5f, Floor + 1.5f, -0.5f), new Vector3(9f, 3.2f, 17f)),
                ("ProbeEast", new Vector3(16.5f, Floor + 1.5f, -0.5f), new Vector3(9f, 3.2f, 17f)), ("ProbeHall", new Vector3(0f, Floor + 1.5f, -14f), new Vector3(42f, 3.2f, 10f)) })
            {
                var go = new GameObject(name);
                go.transform.SetParent(world, false);
                go.transform.position = centre;
                var probe = go.AddComponent<ReflectionProbe>();
                probe.mode = ReflectionProbeMode.Realtime;
                probe.refreshMode = ReflectionProbeRefreshMode.OnAwake;
                probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
                probe.size = size;
                probe.resolution = 128;
                probe.boxProjection = true;
                // At night the yard probe sees mostly cloudy sky: at grazing angles the wet stone and the veranda boards would
                // mirror it blue-white, so it is kept low; the indoor probes see the lamps.
                probe.intensity = night ? (name == "ProbeYard" ? 0.22f : 0.4f) : 0.75f;
            }
        }

        private struct RouteData { public Object[] Points; public float[] Pauses; }

        private static RouteData Route(Transform parent, string name, params (Vector3 position, float yaw, float pause)[] stops)
        {
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            var points = new List<Object>();
            var pauses = new List<float>();
            for (int i = 0; i < stops.Length; i++)
            {
                points.Add(Point(name + "_" + i, stops[i].position, stops[i].yaw, root));
                pauses.Add(stops[i].pause);
            }
            return new RouteData { Points = points.ToArray(), Pauses = pauses.ToArray() };
        }

        /// <summary>The grab played when a watcher really reaches her (see <see cref="CatchScene"/>), with this chapter's ending.</summary>
        private static CatchScene Catch(Systems s, CatchScene.Ending ending, params Watchman[] watchers)
        {
            var scene = s.Root.AddComponent<CatchScene>();
            Set(scene, "_cameraController", s.Camera);
            Set(scene, "_player", s.Player);
            Set(scene, "_audio", s.Audio);
            Set(scene, "_fx", s.Fx);
            Set(scene, "_hud", s.Hud);
            Set(scene, "_watchers", watchers);
            Set(scene, "_ending", (int)ending);
            return scene;
        }

        private const string GuestRig = "Assets/_Game/Art/Characters/MaGiamSinh/Models/MaGiamSinh_Rigged.fbx";
        private const string GuestTextures = "Assets/_Game/Art/Characters/MaGiamSinh/Tectures";
        private static string GirlRig(int which) => $"Assets/_Game/Art/Characters/LauXanh{which}/Rigged/LauXanh{which}_Rigged.fbx";
        private static string GirlTextures(int which) => $"Assets/_Game/Art/Characters/LauXanh{which}/Rigged";

        /// <summary>
        /// Someone who only shows what the house is for (<see cref="BackgroundPatron"/>): tinted apart from the story's
        /// people, no navigation (they never walk; one may stand up on the gallery) and no eyes.
        /// </summary>
        private static void Patron(string name, string rig, string textures, float height, Vector3 at, float yaw, Color tint, bool sits, Vector3 faces)
        {
            var actor = RiggedActor(name, "Khach", rig, textures, height, at, yaw, tint, StoryAnimationSetup.Controller());
            var agent = actor.GetComponent<NavMeshAgent>();
            if (agent != null) Object.DestroyImmediate(agent);
            var patron = actor.gameObject.AddComponent<BackgroundPatron>();
            Set(patron, "_sits", sits);
            Set(patron, "_faces", faces);
        }

        private static Watchman Watch(StoryActor actor, Systems s, Object[] waypoints, float[] pauses, float eyeHeight, float range, float halfAngle,
            bool restrictedOnly, bool canChase, bool drunk, (string[] suspicious, string[] investigate, string[] chase, string[] search, string[] giveUp)? lines)
        {
            var eyes = Point("Eyes", actor.transform.position + Vector3.up * eyeHeight, 0f, actor.transform);
            eyes.localRotation = Quaternion.identity;
            var watch = actor.gameObject.AddComponent<Watchman>();
            if (waypoints != null) Set(watch, "_waypoints", waypoints);
            if (pauses != null) SetFloats(watch, "_pauses", pauses);
            Set(watch, "_eyes", eyes);
            Set(watch, "_player", s.Player);
            Set(watch, "_range", range);
            Set(watch, "_halfAngle", halfAngle);
            Set(watch, "_onlyInRestrictedZones", restrictedOnly);
            Set(watch, "_canChase", canChase);
            Set(watch, "_drunk", drunk);
            Set(watch, "_walls", 1 << EnvironmentLayer);
            Set(watch, "_hud", s.Hud);
            Set(watch, "_speakerId", actor.SpeakerId);
            if (lines.HasValue)
            {
                Set(watch, "_suspiciousLines", lines.Value.suspicious);
                Set(watch, "_investigateLines", lines.Value.investigate);
                Set(watch, "_chaseLines", lines.Value.chase);
                Set(watch, "_searchLines", lines.Value.search);
                Set(watch, "_giveUpLines", lines.Value.giveUp);
            }
            var steps = actor.gameObject.AddComponent<ActorFootsteps>();
            Set(steps, "_audio", s.Audio);
            if (actor.SpeakerId == "TuBa") { Set(steps, "_volume", 0.45f); Set(steps, "_pitch", 1.1f); }
            return watch;
        }

        private static void SetRound(Object target, string property, RouteData route)
        {
            Set(target, property + ".Points", route.Points);
            SetFloats(target, property + ".Pauses", route.Pauses);
        }

        /// <summary>
        /// The gravel heap outside Kiều's room (§3): a shallow clay dish heaped with small grey stones against the wall by her
        /// door, where she cannot miss it; E fills her hand up to three.
        /// </summary>
        private static PebblePile PebbleHeap(Transform parent, Systems s, PebbleThrow pebbles, Vector3 at)
        {
            var root = new GameObject("PebbleHeap");
            root.transform.SetParent(parent, false);
            root.transform.position = at;
            var dish = Cylinder("Dish", at + Vector3.up * 0.04f, 0.24f, 0.08f, Mat("LTB_Clay", new Color(0.42f, 0.25f, 0.16f), 0.2f), root.transform, false);
            dish.transform.localScale = new Vector3(0.48f, 0.04f, 0.48f);
            var stone = Mat("LTB_Bead", new Color(0.5f, 0.49f, 0.46f), 0.35f);
            var random = new System.Random(4);
            for (int i = 0; i < 14; i++)
            {
                float angle = (float)random.NextDouble() * Mathf.PI * 2f, reach = (float)random.NextDouble() * 0.17f;
                float size = 0.035f + (float)random.NextDouble() * 0.025f;
                var pebble = StoryArtKit.Sphere("Stone", at + new Vector3(Mathf.Cos(angle) * reach, 0.085f + (0.17f - reach) * 0.25f, Mathf.Sin(angle) * reach),
                    new Vector3(size, size * 0.7f, size * 1.1f), stone, root.transform);
                pebble.transform.rotation = Quaternion.Euler(0f, (float)random.NextDouble() * 180f, 0f);
            }
            var box = root.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.center = Vector3.up * 0.15f;
            box.size = new Vector3(0.6f, 0.4f, 0.6f);
            root.layer = 2;
            var pile = root.AddComponent<PebblePile>();
            Set(pile, "_pebbles", pebbles);
            Set(pile, "_audio", s.Audio);
            return pile;
        }

        private static void SetFloats(Object target, string property, float[] values)
        {
            var serialized = new SerializedObject(target);
            var field = serialized.FindProperty(property);
            field.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) field.GetArrayElementAtIndex(i).floatValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static (string[], string[], string[], string[], string[]) GuardLinesDay() => (
            new[] { "Hửm?", "Ai đấy?" },
            new[] { "Cô em đi đâu thế? Lại đây xem nào." },
            new[] { "Đứng lại! Chỗ này cấm!" },
            new[] { "Vừa thấy bóng ai lẻn vào đây..." },
            new[] { "Chắc con mèo.", "Hoa mắt rồi." });

        private static (string[], string[], string[], string[], string[]) GuardLinesNight() => (
            new[] { "Hửm? (nấc)", "...Ai?", "Có tiếng gì thế nhỉ..." },
            new[] { "Đứa nào ở đó? Ra đây!", "Để tao xem nào... (nấc)" },
            new[] { "Đứng lại! Con ranh, đứng lại!", "Bắt được rồi nhé!" },
            new[] { "Chạy đâu cho thoát...", "Tao thấy mày rồi... ở đâu đây..." },
            new[] { "Chắc con mèo...", "Rượu ngấm rồi. Hoa cả mắt." });

        private static (string[], string[], string[], string[], string[]) TuBaLines() => (
            new[] { "Đứa nào lảng vảng ngoài kia?" },
            new[] { "Ra đây cho ta xem mặt!" },
            new[] { "Nó kia rồi! Thằng Tư! Bắt lấy nó!" },
            new[] { "Con ranh... trốn đâu?" },
            new[] { "Hừ." });

        private static Transform Sequences(Systems s)
        {
            var sequences = new GameObject("Sequences").transform;
            sequences.SetParent(s.Root.transform, false);
            return sequences;
        }

        private static void Drawer(Transform sequences)
        {
            var drawer = Child<ActionBeatSequence>(sequences, "search_drawer");
            Set(drawer, "_id", "search_drawer");
            Set(drawer, "_gesture", 6);
            Set(drawer, "_sfx", "paper");
            Set(drawer, "_shake", 0f);
        }

        /// <summary>Explore anchor without the floating marker: in first person the reticle says what can be used.</summary>
        private static ExploreTarget Anchor(GameObject go, StoryDirector director, string id, string label, Vector3 size, bool hidden = false)
        {
            var target = Explore(go, director, id, label, size, hidden);
            var marker = go.transform.Find("InteractMarker");
            if (marker != null) Object.DestroyImmediate(marker.gameObject);
            Set(target, "_highlight", null);
            return target;
        }

        private static GameObject Spot(string name, Vector3 position, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            return go;
        }

        private static BoxCollider Box3(Transform parent, string name, Vector3 centre, Vector3 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = centre;
            go.layer = 2;
            var box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = size;
            return box;
        }

        private static Object[] AllDoors(LauTuBaHouse.Refs house) => new Object[]
        {
            house.GateLeft, house.GateRight, house.KieuDoor, house.LamDoor, house.HongDoor, house.GuestDoor, house.BackDoor, house.StoreDoor,
            house.TuBaDoor, house.InnerDoor, house.OfficeDoor
        };

        /// <summary>A soft human shadow that can pass across a lit paper surface (see <see cref="PaperShadow"/>).</summary>
        private static PaperShadow ShadowOnPaper(string name, Vector3 centre, float yaw, Vector3 from, Vector3 to, Vector2 size, Transform parent)
        {
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.SetPositionAndRotation(centre, Quaternion.Euler(0f, yaw, 0f));
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "Shape";
            Object.DestroyImmediate(quad.GetComponent<Collider>());
            quad.transform.SetParent(root, false);
            quad.transform.localScale = new Vector3(size.x, size.y, 1f);
            var renderer = quad.GetComponent<Renderer>();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.sharedMaterial = ShadowMaterial();
            var shadow = root.gameObject.AddComponent<PaperShadow>();
            Set(shadow, "_shape", renderer);
            Set(shadow, "_from", from);
            Set(shadow, "_to", to);
            return shadow;
        }

        private static Material ShadowMaterial()
        {
            string path = Folder + "/LTB_PaperShadow.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.SetTexture("_BaseMap", StoryArtKit.GeneratedTexture("LTB_Silhouette", DrawFigure, 128));
            material.SetColor("_BaseColor", new Color(0.02f, 0.015f, 0.01f, 0.7f));
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_Cull", 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        /// <summary>Head and shoulders, blurred as a shadow on paper is.</summary>
        private static void DrawFigure(Color32[] pixels, int size)
        {
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float u = (x + 0.5f) / size, v = (y + 0.5f) / size;
                    float head = 1f - Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.8f)) / 0.13f;
                    float body = 1f - Mathf.Sqrt(Mathf.Pow((u - 0.5f) / 0.33f, 2f) + Mathf.Pow((v - 0.3f) / 0.46f, 2f));
                    float a = Mathf.Clamp01(Mathf.Max(head, body) * 4f);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
        }

        /// <summary>A figure in Kiều's clothes seen only in the bronze mirror (its layer is hidden from the main camera).</summary>
        private static GameObject MirrorFigure(Transform world)
        {
            var actor = RiggedActor("MirrorFigure", "KieuBong", "Assets/_Game/Art/Characters/ThuyKieu/Models/ThuyKieu_Rigged.fbx",
                "Assets/_Game/Art/Characters/ThuyKieu/Tectures", 1.6f, new Vector3(12f, Floor, 9f), 0f, new Color(0.62f, 0.66f, 0.72f), StoryAnimationSetup.Controller());
            var go = actor.gameObject;
            Object.DestroyImmediate(actor.GetComponent<NavMeshAgent>());
            Object.DestroyImmediate(actor.GetComponent<Collider>());
            foreach (var t in go.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = MirrorOnlyLayer;
            // The archive is dark and no lamp reaches behind her: the figure is drawn unlit, its clothes a cold pale grey,
            // so the hazy bronze can show it at all. (Updated in place so the scene's reference survives a rebuild.)
            int index = 0;
            foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                var materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] == null) continue;
                    string path = Folder + "/LTB_MirrorFigure_" + index++ + ".mat";
                    var pale = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (pale == null) { pale = new Material(Shader.Find("Universal Render Pipeline/Unlit")); AssetDatabase.CreateAsset(pale, path); }
                    pale.shader = Shader.Find("Universal Render Pipeline/Unlit");
                    pale.SetTexture("_BaseMap", materials[i].HasProperty("_BaseMap") ? materials[i].GetTexture("_BaseMap") : null);
                    pale.SetColor("_BaseColor", new Color(0.46f, 0.49f, 0.55f));
                    EditorUtility.SetDirty(pale);
                    materials[i] = pale;
                }
                renderer.sharedMaterials = materials;
            }
            // Not a character of the story: no speaker id, nothing the director should address.
            Object.DestroyImmediate(actor);
            go.transform.SetParent(world, true);
            go.SetActive(false);
            return go;
        }
    }
}
