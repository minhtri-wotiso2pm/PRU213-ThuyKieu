using System.Linq;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Chapters;
using ThuyKieu.Gameplay.Sequences;
using UnityEditor;
using UnityEngine;
using static ThuyKieu.Core.Editor.StorySceneKit;
using static ThuyKieu.Core.Editor.StoryArtKit;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Builds Chapter00_Prologue: the modern classroom (west, around x −60) and the Vương family house on the morning
    /// of the arrest (around the origin): Kiều's room in the west wing, the courtyard, the main house and the gate.
    /// </summary>
    public static class PrologueSetup
    {
        public const string SceneName = "Chapter00_Prologue";
        public const string ScenePath = "Assets/_Game/Scenes/" + SceneName + ".unity";
        public static readonly Vector3 ClassOrigin = new Vector3(-60f, 0f, 0f);

        private const string Characters = "Assets/_Game/Art/Characters/";
        private static Material _plaster, _wood, _tiles, _ridge, _stone, _red, _planks, _brick;

        [MenuItem("ThuyKieu/Prologue/Create playable scene")]
        public static void Create()
        {
            var scene = NewScene(ScenePath);
            var shared = StoryAnimationSetup.Controller();
            var s = CreateSystems(ChapterCatalog.Prologue, "Trang sách", "prologue", "Chapter01_GiaBien", ClassOrigin + new Vector3(2.9f, 0.05f, -1.15f), 0f, "classroom");
            Grade(s.Fx, 0.2f, 8f, 6f, 4f, new Color(0.5f, 0.52f, 0.6f), new Color(0.82f, 0.74f, 0.6f));
            UseSky(s.CameraComponent, Sky("PrologueSky", "kloppenheim_06_puresky", 1f, new Color(0.62f, 0.64f, 0.66f), 60f));
            Set(s.Shots, "_keyLight", 0.6f);
            var world = new GameObject("World").transform;
            var nav = NavMesh(world);
            var director = s.Director;
            InitMaterials();

            // ------------------------------------------------------------ the classroom
            var room = ClassroomKit.Build(world, ClassOrigin, false);
            var look = ClassroomKit.StudentBody(s);
            // Ngân: the purple-dressed rig warmed to coral so she never reads as the student in blue.
            var ngan = RiggedActor("Ngan", "Ngan", Characters + "LauXanh3/Rigged/LauXanh3_Rigged.fbx", Characters + "LauXanh3/Rigged", 1.58f,
                room.NganSeat.position, 0f, new Color(1f, 0.78f, 0.66f), StoryAnimationSetup.CharacterController("LauXanh3"));
            var teacher = RiggedActor("CoGiao", "CoGiao", Characters + "MeThuyKieu/Models/MeThuyKieu_Rigged.fbx", Characters + "MeThuyKieu/Tectures",
                1.6f, room.TeacherSpot.position, 180f, new Color(0.78f, 0.8f, 1f), shared);
            var classmates = Classmates(room, shared);
            Explore(Anchor(world, "BoardTarget", room.BoardAnchor.position), director, "pl_board", "Đọc mấy câu thơ trên bảng", new Vector3(2.4f, 1.6f, 1.2f));
            Explore(ngan.gameObject, director, "pl_ngan", "Nói chuyện với Ngân", Vector3.one * 1.8f);
            Explore(Anchor(world, "WindowTarget", room.WindowAnchor.position), director, "pl_window", "Nhìn ra cửa sổ", new Vector3(1.2f, 1.6f, 1.8f));
            Explore(Anchor(world, "BookTarget", room.Seat.position + new Vector3(0, 0, 0.1f)), director, "pl_book", "Cất quyển Truyện Kiều vào cặp", new Vector3(1.2f, 1.6f, 1.2f));

            // ------------------------------------------------------------ the Vương house
            Ground(world);
            KieuRoom(world, director, out var wake);
            Courtyard(world, out var courtyard, out var gate, out var rainProps);
            var father = CharacterActor("VuongOng", "VuongOng", "VuongOng", 1.66f, new Vector3(0.6f, 0.27f, 5.75f), 180f);
            var mother = RiggedActor("MeKieu", "MeKieu", Characters + "MeThuyKieu/Models/MeThuyKieu_Rigged.fbx", Characters + "MeThuyKieu/Tectures", 1.56f, new Vector3(-0.6f, 0.27f, 5.6f), 160f, Color.white, shared);
            var brother = RiggedActor("VuongQuan", "VuongQuan", Characters + "MaGiamSinh/Models/MaGiamSinh_Rigged.fbx", Characters + "MaGiamSinh/Tectures", 1.48f, new Vector3(1.8f, 0.27f, 5.5f), 200f, new Color(0.72f, 0.86f, 0.7f), shared);
            var officials = new[]
            {
                CharacterActor("QuanSai", "QuanSai", "LinhCanh", 1.8f, new Vector3(-0.4f, 0f, -8f), 0f),
                CharacterActor("QuanSai2", "QuanSai", "LinhCanh", 1.78f, new Vector3(0.6f, 0f, -8f), 0f)
            };
            var marks = new GameObject("VuongMarks").transform;
            marks.SetParent(world, false);
            var fatherSpot = Point("FatherSpot", father.transform.position, 180f, marks);
            var brotherSpot = Point("BrotherSpot", brother.transform.position, 200f, marks);
            var motherSpot = Point("MotherSpot", mother.transform.position, 160f, marks);
            var motherRain = Point("MotherRainSpot", new Vector3(-0.2f, 0.27f, 5.1f), 180f, marks);
            var officialSpots = new[] { Point("OfficialSpotA", new Vector3(-0.3f, 0f, 3.7f), 0f, marks), Point("OfficialSpotB", new Vector3(1.9f, 0f, 3.6f), 0f, marks) };

            // ------------------------------------------------------------ light & atmosphere
            var sun = Sun("Sun", new Color(1f, 0.9f, 0.76f), 1.3f, new Vector3(34f, 250f, 0f));
            RenderSettings.ambientLight = new Color(0.6f, 0.59f, 0.56f);
            RenderSettings.fogColor = new Color(0.72f, 0.74f, 0.74f);
            RenderSettings.fogDensity = 0.006f;
            var rain = Rain("PrologueRain", courtyard);
            rain.transform.position = new Vector3(0f, 10f, 0f);

            // ------------------------------------------------------------ world logic
            var worldScript = s.Root.AddComponent<PrologueWorld>();
            Set(worldScript, "_director", director);
            Set(worldScript, "_navMesh", nav);
            Set(worldScript, "_look", look);
            Set(worldScript, "_seat", room.Seat);
            Set(worldScript, "_ngan", ngan);
            Set(worldScript, "_teacher", teacher);
            Set(worldScript, "_classmates", classmates.Cast<Object>().ToArray());
            Set(worldScript, "_classDoor", room.Door);
            Set(worldScript, "_bookGlow", room.BookGlow);
            Set(worldScript, "_wake", wake);
            Set(worldScript, "_courtyard", courtyard);
            Set(worldScript, "_father", father);
            Set(worldScript, "_brother", brother);
            Set(worldScript, "_mother", mother);
            Set(worldScript, "_fatherSpot", fatherSpot);
            Set(worldScript, "_brotherSpot", brotherSpot);
            Set(worldScript, "_motherSpot", motherSpot);
            Set(worldScript, "_motherRainSpot", motherRain);
            Set(worldScript, "_officials", officials.Cast<Object>().ToArray());
            Set(worldScript, "_officialSpots", officialSpots.Cast<Object>().ToArray());
            Set(worldScript, "_gate", gate);
            Set(worldScript, "_sun", sun);
            Set(worldScript, "_rain", rain);
            Set(worldScript, "_rainProps", rainProps);

            // ------------------------------------------------------------ sequences
            var sequences = new GameObject("Sequences").transform;
            sequences.SetParent(s.Root.transform, false);
            Tutorial(sequences, "tutorial_classroom",
                "<color=#E8C27A>Đi lại:</color> WASD · <color=#E8C27A>xoay camera:</color> chuột\n" +
                "<color=#E8C27A>Tương tác:</color> đến gần thứ có dấu ◆ vàng rồi bấm <color=#E8C27A>E</color>\n" +
                "<color=#E8C27A>Lời thoại:</color> Space / click để đọc tiếp, 1–4 để chọn câu trả lời\n\n" +
                "Thu dọn sách vở trước khi về. Còn vài phút — cứ thong thả nhìn quanh lớp.",
                "WASD: đi · E: tương tác · Space: tiếp lời thoại · Esc: tạm dừng");
            Tutorial(sequences, "tutorial_room",
                "<color=#E8C27A>Quan sát:</color> giữ <color=#E8C27A>chuột phải</color> để nhìn kỹ một thứ.\n" +
                "<color=#E8C27A>Sổ tay:</color> bấm <color=#E8C27A>J</color> để xem lại những gì đã biết.\n\n" +
                "Đây là phòng của Thúy Kiều. Hãy xem quanh phòng để hiểu mình đang ở đâu — rồi ra sân.",
                "WASD: đi · E: xem · Chuột phải: quan sát · J: sổ tay");
            Chapter03Setup.Fx(sequences, "book_glow", "spirit", "memory", "none", 1.8f, 0.08f, 1);
            Chapter03Setup.Fx(sequences, "kieu_voice", "spirit", "memory", "none", 1.6f, 0f, 1);
            Chapter03Setup.Fx(sequences, "wake_panic", "panic", "heartbeat", "morning", 2.6f, 0.2f, 3);
            var intoRoom = Child<WorldBeatSequence>(sequences, "into_room");
            Set(intoRoom, "_id", "into_room");
            Set(intoRoom, "_world", worldScript);
            var arrest = Child<ProcessionSequence>(sequences, "arrest");
            Set(arrest, "_id", "arrest");
            Set(arrest, "_actors", new Object[] { officials[0], officials[1], father, brother });
            Set(arrest, "_marks", new Object[]
            {
                Point("ArrestA", new Vector3(-0.6f, 0f, -6.1f), 180f, marks), Point("ArrestB", new Vector3(0.9f, 0f, -6.4f), 180f, marks),
                Point("ArrestFather", new Vector3(0.1f, 0f, -5.2f), 180f, marks), Point("ArrestBrother", new Vector3(1.3f, 0f, -5.5f), 180f, marks)
            });
            Set(arrest, "_speed", 1.7f);
            Set(arrest, "_timeout", 5f);
            Set(arrest, "_sfx", "stumble");
            Set(arrest, "_shake", 0.15f);

            Save(scene, ScenePath);
        }

        // ------------------------------------------------------------------ characters


        private static StoryActor[] Classmates(ClassroomKit.Parts room, RuntimeAnimatorController shared)
        {
            string[] models = { "ThuyKieu/Models/ThuyKieu_Rigged.fbx", "MaGiamSinh/Models/MaGiamSinh_Rigged.fbx", "MeThuyKieu/Models/MeThuyKieu_Rigged.fbx" };
            string[] textures = { "ThuyKieu/Tectures", "MaGiamSinh/Tectures", "MeThuyKieu/Tectures" };
            Color[] tints = { new Color(0.95f, 0.96f, 1f), new Color(0.82f, 0.88f, 1f), new Color(0.9f, 0.95f, 1f), new Color(0.78f, 0.84f, 0.95f) };
            // Two front rows and the back corner: enough to fill the room without crowding her desk.
            int[] picks = { 0, 3, 6, 9, 11, 13, 16, 20 };
            var list = new System.Collections.Generic.List<StoryActor>();
            for (int i = 0; i < picks.Length && picks[i] < room.ClassmateSeats.Count; i++)
            {
                var seat = room.ClassmateSeats[picks[i]];
                int m = i % 3;
                list.Add(RiggedActor("BanHoc" + i, "BanHoc", Characters + models[m], Characters + textures[m], m == 1 ? 1.7f : 1.58f,
                    seat.position, 0f, tints[i % tints.Length], shared));
            }
            return list.ToArray();
        }

        private static void Tutorial(Transform parent, string id, string body, string controls)
        {
            var tutorial = Child<TutorialSequence>(parent, id);
            Set(tutorial, "_id", id);
            Set(tutorial, "_body", body);
            Set(tutorial, "_controlsHint", controls);
        }

        private static GameObject Anchor(Transform parent, string name, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            return go;
        }

        private static void Look(string name, Vector3 position, Vector3 target, Transform parent)
        {
            Point(name, position, 0, parent).rotation = Quaternion.LookRotation(target - position);
        }

        // ------------------------------------------------------------------ the Vương house

        private static void InitMaterials()
        {
            _plaster = Surface("PlPlaster", "plastered_wall_02", new Color(0.96f, 0.92f, 0.82f), 2.6f, 0.08f, 1.1f, 0.45f, 0.8f);
            _wood = Surface("PlWood", "dark_wood", new Color(0.74f, 0.6f, 0.48f), 1.2f, 0.3f, 1f, 0.25f);
            _tiles = Surface("PlTiles", "clay_roof_tiles_02", new Color(0.66f, 0.46f, 0.4f), 1.4f, 0.25f, 1.2f, 0.35f);
            _ridge = Material("PlRidge", new Color(0.18f, 0.09f, 0.07f));
            _stone = Surface("PlStone", "monastery_stone_floor", new Color(0.82f, 0.8f, 0.76f), 2.2f, 0.2f, 1.1f, 0.35f);
            _red = Surface("PlLacquer", "lacquered_cherry_wood", new Color(0.92f, 0.36f, 0.3f), 1.2f, 0.5f, 0.8f, 0.2f);
            _planks = Surface("PlPlanks", "old_wooden_floor_02", new Color(0.82f, 0.7f, 0.58f), 2f, 0.3f, 1f, 0.35f);
            _brick = Surface("PlBrick", "brick_floor", new Color(0.92f, 0.72f, 0.6f), 1.6f, 0.2f, 1f, 0.35f);
        }

        private static void Ground(Transform world)
        {
            Block("Ground", new Vector3(0f, -0.1f, 0f), new Vector3(34f, 0.2f, 36f), Surface("PlGround", "brown_mud_dry", new Color(0.8f, 0.72f, 0.62f), 3f, 0.12f, 1f, 0.45f), world);
            Block("CourtBrick", new Vector3(0f, 0.01f, -0.5f), new Vector3(21.6f, 0.02f, 13f), _brick, world, false);
            Grass("VillageGrass", world, new Rect(-17f, -18f, 34f, 11f), 1600, p => Mathf.Abs(p.x) > 2.2f || p.y < -15f, 701, 0.4f,
                new Color(0.42f, 0.58f, 0.28f), new Color(0.6f, 0.62f, 0.3f), "PlGrass");
        }

        private static void Wall(Transform root, string name, Vector3 centre, Vector3 size)
        {
            var wall = Block(name, centre, size, _plaster, root);
            Vector3 top = centre + Vector3.up * size.y * 0.5f;
            Block(name + "Cap", top + Vector3.up * 0.08f, new Vector3(size.x + 0.3f, 0.16f, size.z + 0.3f), _tiles, root, false);
            Block(name + "Footing", new Vector3(centre.x, 0.2f, centre.z), new Vector3(size.x + 0.08f, 0.4f, size.z + 0.08f), _stone, root, false);
            StoryPropKit.TimberFrame(wall, _wood, 2.4f);
        }

        /// <summary>Kiều's room in the west wing: bed, bronze mirror, the hồ cầm, the writing desk with Kim Trọng's letter.</summary>
        private static void KieuRoom(Transform world, StoryDirector director, out Transform wake)
        {
            var root = new GameObject("KieuRoom").transform;
            root.SetParent(world, false);
            Block("RoomFloor", new Vector3(-8.25f, 0.03f, 3f), new Vector3(3.6f, 0.06f, 6f), _planks, root);
            Block("RoomWallW", new Vector3(-10.1f, 1.6f, 3f), new Vector3(0.2f, 3.2f, 6.2f), _plaster, root);
            Block("RoomWallN", new Vector3(-8.25f, 1.6f, 6.1f), new Vector3(3.9f, 3.2f, 0.2f), _plaster, root);
            var south = Block("RoomWallS", new Vector3(-8.25f, 1.6f, -0.1f), new Vector3(3.9f, 3.2f, 0.2f), _plaster, root);
            var eastS = Block("RoomWallE_S", new Vector3(-6.4f, 1.6f, 1.15f), new Vector3(0.2f, 3.2f, 2.3f), _plaster, root);
            var eastN = Block("RoomWallE_N", new Vector3(-6.4f, 1.6f, 4.8f), new Vector3(0.2f, 3.2f, 2.6f), _plaster, root);
            Block("RoomDoorLintel", new Vector3(-6.4f, 2.75f, 2.9f), new Vector3(0.22f, 0.9f, 1.2f), _red, root);
            Fadeable(south, eastS, eastN);
            foreach (var wall in new[] { south, eastS, eastN }) StoryPropKit.TimberFrame(wall, _wood, 1.8f);
            var frame = Material("PlLatticeFrame", new Color(0.22f, 0.08f, 0.05f), 0.4f);
            var paper = Emissive("PlWindowDay", new Color(0.95f, 0.9f, 0.76f), 0.55f);
            FadeWith(south, Lattice("RoomWindow", new Vector3(-8.2f, 1.7f, 0.02f), 1.4f, 1.1f, 180f, frame, paper, root));
            Chapter03Setup.Hide(Roof("RoomRoof", new Vector3(-8.25f, 4.4f, 3f), 4.6f, 7f, 26f, _tiles, _ridge, root, false), new Vector3(-8.25f, 1.5f, 3f), new Vector3(3.6f, 4f, 6f));
            Zone("KieuRoom", new Vector3(-8.25f, 1.5f, 3f), new Vector3(3.6f, 4f, 6f), 2.1f, 0.25f, 60f, 0.35f, root, 1);

            // Bed with a gauze canopy, a rug, a folding screen.
            Prop("Assets/_Game/Prefabs/Props/Bed.prefab", new Vector3(-8.8f, 0.06f, 5.05f), 90, 0.95f, root);
            Block("Rug", new Vector3(-8.1f, 0.065f, 2.9f), new Vector3(1.8f, 0.01f, 2.4f), Material("PlRug", new Color(0.45f, 0.14f, 0.12f)), root, false);
            for (int i = 0; i < 3; i++)
            {
                var panel = Block("ScreenPanel", new Vector3(-7.2f, 0.9f, 4.0f + i * 0.55f), new Vector3(0.05f, 1.7f, 0.52f), _red, root, false, i % 2 == 0 ? 12f : -12f);
                Block("ScreenSilk", panel.transform.position + new Vector3(0.03f, 0.05f, 0), new Vector3(0.02f, 1.3f, 0.42f), Material("PlScreenSilk", new Color(0.9f, 0.84f, 0.7f)), root, false, i % 2 == 0 ? 12f : -12f);
            }
            // Bronze mirror on its stand (pl_guong).
            var bronze = Material("PlBronze", new Color(0.72f, 0.56f, 0.3f), 0.9f);
            Block("MirrorStand", new Vector3(-9.7f, 0.45f, 1.0f), new Vector3(0.4f, 0.9f, 0.5f), _wood, root);
            var disc = Cylinder("MirrorDisc", new Vector3(-9.6f, 1.3f, 1.0f), 0.24f, 0.03f, bronze, root, false);
            disc.transform.rotation = Quaternion.Euler(0, 0, 90f);
            disc.transform.position = new Vector3(-9.55f, 1.4f, 1.0f);
            var mirror = new GameObject("MirrorTarget");
            mirror.transform.SetParent(root, false);
            mirror.transform.position = new Vector3(-9.0f, 0.06f, 1.1f);
            Explore(mirror, director, "pl_guong", "Soi chiếc gương đồng", new Vector3(1.2f, 1.6f, 1.2f));
            Point("Focus_Mirror", new Vector3(-9.55f, 1.4f, 1.0f), 0, root);
            Look("CAM_Mirror", new Vector3(-9.45f, 1.48f, 1.05f), new Vector3(-8.75f, 1.45f, 1.15f), root);
            // The hồ cầm leaning against the west wall (pl_dan).
            var body = Cylinder("DanBody", new Vector3(-9.85f, 0.06f, 3.1f), 0.11f, 0.16f, _red, root, false);
            body.transform.rotation = Quaternion.Euler(0, 0, -12f);
            var neck = Block("DanNeck", new Vector3(-9.82f, 0.75f, 3.1f), new Vector3(0.035f, 1.25f, 0.035f), _wood, root, false);
            neck.transform.rotation = Quaternion.Euler(0, 0, -12f);
            Block("DanString", new Vector3(-9.79f, 0.75f, 3.1f), new Vector3(0.004f, 1.2f, 0.004f), Material("PlString", new Color(0.9f, 0.85f, 0.7f), 0.6f), root, false);
            var dan = new GameObject("DanTarget");
            dan.transform.SetParent(root, false);
            dan.transform.position = new Vector3(-9.2f, 0.06f, 3.1f);
            Explore(dan, director, "pl_dan", "Cây hồ cầm", new Vector3(1.2f, 1.6f, 1.2f));
            Look("CAM_Dan", new Vector3(-8.8f, 1.35f, 2.4f), new Vector3(-9.8f, 0.8f, 3.1f), root);
            // Writing desk under the window, Kim Trọng's letter on it (pl_tho).
            StoryPropKit.LowTable(new Vector3(-8.2f, 0.06f, 0.55f), 0f, root, false, 7);
            Block("Letter", new Vector3(-8.25f, 0.43f, 0.55f), new Vector3(0.24f, 0.004f, 0.3f), Material("PlLetter", new Color(0.93f, 0.88f, 0.74f)), root, false, 12f);
            Block("InkStone", new Vector3(-7.85f, 0.44f, 0.5f), new Vector3(0.12f, 0.03f, 0.18f), Material("PlInk", new Color(0.05f, 0.05f, 0.06f), 0.7f), root, false);
            var desk = new GameObject("DeskTarget");
            desk.transform.SetParent(root, false);
            desk.transform.position = new Vector3(-8.2f, 0.06f, 1.25f);
            Explore(desk, director, "pl_tho", "Bàn viết bên cửa sổ", new Vector3(1.2f, 1.6f, 1.2f));
            Look("CAM_Desk", new Vector3(-7.7f, 1.45f, 1.5f), new Vector3(-8.25f, 0.42f, 0.55f), root);
            // Door out to the courtyard (pl_san).
            var door = new GameObject("DoorTarget");
            door.transform.SetParent(root, false);
            door.transform.position = new Vector3(-6.9f, 0.06f, 2.9f);
            Explore(door, director, "pl_san", "Ra sân", new Vector3(1f, 1.8f, 1.4f));
            StoryPropKit.Curtain(new Vector3(-6.32f, 2.3f, 2.9f), 1.1f, 1.6f, 90f, new Color(0.75f, 0.6f, 0.3f), root);
            StoryPropKit.IncenseBurner(new Vector3(-9.6f, 0.06f, 5.9f), root);
            PottedPlant(new Vector3(-6.9f, 0.06f, 5.6f), 0.7f, root, 31, true);
            var day = new GameObject("RoomDaylight");
            day.transform.SetParent(root, false);
            day.transform.position = new Vector3(-8.2f, 2.4f, 2.6f);
            var light = day.AddComponent<Light>();
            light.type = LightType.Point; light.color = new Color(1f, 0.9f, 0.76f); light.range = 6f; light.intensity = 1.1f; light.shadows = LightShadows.None;
            Look("CAM_Wide_KieuRoom", new Vector3(-6.9f, 2.3f, 0.6f), new Vector3(-8.8f, 0.7f, 4.6f), root);
            wake = Point("WakeSpot", new Vector3(-8.2f, 0.08f, 3.4f), 165f, root);
        }

        /// <summary>The courtyard, the main house with its veranda and breakfast table, the east wing, the gate onto the lane.</summary>
        private static void Courtyard(Transform world, out Transform playerSpot, out Transform gate, out GameObject rainProps)
        {
            var root = new GameObject("VuongCourtyard").transform;
            root.SetParent(world, false);
            Wall(root, "CourtWallW", new Vector3(-11f, 1.1f, 2.5f), new Vector3(0.4f, 2.2f, 19f));
            Wall(root, "CourtWallE", new Vector3(11f, 1.1f, 2.5f), new Vector3(0.4f, 2.2f, 19f));
            Wall(root, "CourtWallN", new Vector3(0f, 1.1f, 12.2f), new Vector3(22.4f, 2.2f, 0.4f));
            Wall(root, "CourtWallSW", new Vector3(-6.3f, 1.1f, -7f), new Vector3(9.4f, 2.2f, 0.4f));
            Wall(root, "CourtWallSE", new Vector3(6.3f, 1.1f, -7f), new Vector3(9.4f, 2.2f, 0.4f));
            // Gate: lacquered posts, a small tiled roof, the leaves standing open.
            foreach (float x in new[] { -1.7f, 1.7f }) Pillar("GatePost", new Vector3(x, 0, -7f), 0.18f, 3f, _red, _stone, root);
            Roof("GateRoof", new Vector3(0f, 3.4f, -7f), 4.4f, 1.8f, 26f, _tiles, _ridge, root);
            foreach (int side in new[] { -1, 1 })
                Block("GateLeaf", new Vector3(side * 1.95f, 1.2f, -6.3f), new Vector3(0.08f, 2.3f, 1.4f), _wood, root, false, side * 8f);
            Couplets(new Vector3(-1.9f, 1.7f, -7.25f), new Vector3(1.9f, 1.7f, -7.25f), 180f, root);
            gate = Point("GatePoint", new Vector3(0f, 0f, -8.5f), 0f, root);

            // Main house.
            Block("HouseBack", new Vector3(0f, 1.8f, 11.6f), new Vector3(13f, 3.6f, 0.3f), _plaster, root);
            Block("HouseSideW", new Vector3(-6.4f, 1.8f, 8.8f), new Vector3(0.3f, 3.6f, 5.8f), _plaster, root);
            Block("HouseSideE", new Vector3(6.4f, 1.8f, 8.8f), new Vector3(0.3f, 3.6f, 5.8f), _plaster, root);
            var frontW = Block("HouseFrontW", new Vector3(-3.7f, 1.8f, 6f), new Vector3(5.4f, 3.6f, 0.3f), _plaster, root);
            var frontE = Block("HouseFrontE", new Vector3(3.7f, 1.8f, 6f), new Vector3(5.4f, 3.6f, 0.3f), _plaster, root);
            Block("HouseDoorLintel", new Vector3(0f, 3.05f, 6f), new Vector3(2f, 1.1f, 0.32f), _red, root);
            Block("HouseDoorDark", new Vector3(0f, 1.25f, 6.25f), new Vector3(2f, 2.5f, 0.05f), Material("PlDark", new Color(0.06f, 0.04f, 0.03f)), root, false);
            foreach (var wall in new[] { frontW, frontE }) StoryPropKit.TimberFrame(wall, _wood, 2f);
            var frame = Material("PlLatticeFrame", new Color(0.22f, 0.08f, 0.05f), 0.4f);
            var paper = Emissive("PlWindowDay", new Color(0.95f, 0.9f, 0.76f), 0.55f);
            Lattice("HouseWindow", new Vector3(-3.6f, 1.8f, 5.83f), 1.6f, 1.2f, 180f, frame, paper, root);
            Lattice("HouseWindow", new Vector3(3.6f, 1.8f, 5.83f), 1.6f, 1.2f, 180f, frame, paper, root);
            Couplets(new Vector3(-1.25f, 1.7f, 5.83f), new Vector3(1.25f, 1.7f, 5.83f), 180f, root);
            Block("Veranda", new Vector3(0f, 0.13f, 5.2f), new Vector3(13f, 0.26f, 1.9f), _stone, root);
            for (int i = 0; i < 4; i++) Pillar("VerandaPillar", new Vector3(-4.5f + i * 3f, 0.26f, 4.45f), 0.16f, 3.5f, _red, _stone, root);
            Block("VerandaBeam", new Vector3(0f, 3.8f, 4.45f), new Vector3(13.4f, 0.2f, 0.22f), _wood, root, false);
            Roof("HouseRoof", new Vector3(0f, 5.4f, 8.4f), 14.2f, 9f, 26f, _tiles, _ridge, root);
            // Breakfast laid out on the veranda.
            StoryPropKit.LowTable(new Vector3(0.6f, 0.26f, 5.15f), 0f, root, true, 3);
            for (int i = 0; i < 4; i++)
                Cylinder("RiceBowl", new Vector3(0.25f + i * 0.24f, 0.62f, 5.1f + (i % 2) * 0.12f), 0.06f, 0.05f, Material("PlBowl", new Color(0.9f, 0.88f, 0.82f), 0.5f), root, false);
            // East wing (kitchen and store), closed.
            Block("EastWing", new Vector3(8.6f, 1.6f, 3f), new Vector3(4.2f, 3.2f, 6f), _plaster, root);
            Roof("EastWingRoof", new Vector3(8.6f, 4.4f, 3f), 4.8f, 6.8f, 26f, _tiles, _ridge, root, false);
            Block("EastWingDoor", new Vector3(6.48f, 1.1f, 3f), new Vector3(0.05f, 2.2f, 1.1f), _wood, root, false);
            // Yard: well, jars, trees, plants, a washing line.
            Prop("Assets/_Game/Prefabs/Props/Well.prefab", new Vector3(5.8f, 0f, -2.6f), 20, 1f, root);
            Jars(new Vector3(8.8f, 0f, -4.8f), 4, root, 711);
            Jars(new Vector3(-9.8f, 0f, -5.6f), 2, root, 712);
            Prop("Assets/_Game/Prefabs/Vegetation/ArecaPalm.prefab", new Vector3(-8.8f, 0f, -3.2f), 0, 0.95f, root);
            Prop("Assets/_Game/Prefabs/Vegetation/BananaTree.prefab", new Vector3(9.2f, 0f, -1.2f), 40, 0.85f, root);
            Prop("Assets/_Game/Prefabs/Vegetation/ArecaPalm.prefab", new Vector3(4f, 0f, -14f), 70, 1.1f, root);
            Prop("Assets/_Game/Prefabs/Vegetation/BananaTree.prefab", new Vector3(-5f, 0f, -13f), 10, 1f, root);
            for (int i = 0; i < 4; i++) PottedPlant(new Vector3(-5.5f + i * 3.5f, 0.26f, 4.55f), 0.7f, root, 720 + i, i % 2 == 0);
            StoryPropKit.ClothLine(new Vector3(-5.5f, 2.1f, -3.5f), new Vector3(-1.8f, 2.1f, -4.8f), root, 730);
            StoryPropKit.Baskets(new Vector3(7.2f, 0f, 0.2f), 30f, root, 731);
            playerSpot = Point("CourtyardSpot", new Vector3(0.6f, 0.05f, 2.4f), 0f, root);
            Look("CAM_Wide_Courtyard", new Vector3(-3.2f, 2.6f, -3.6f), new Vector3(0.8f, 1f, 4.8f), root);
            Look("CAM_Gate", new Vector3(1.6f, 1.9f, 3.4f), new Vector3(0f, 1.4f, -7.5f), root);
            // Rain-day dressing: puddles on the bricks.
            var wet = new GameObject("RainProps");
            wet.transform.SetParent(root, false);
            StoryPropKit.Puddle(new Vector3(-2.4f, 0.03f, -1.5f), new Vector2(1.6f, 0.9f), 20f, wet.transform);
            StoryPropKit.Puddle(new Vector3(3.2f, 0.03f, 0.8f), new Vector2(1.2f, 0.8f), -15f, wet.transform);
            StoryPropKit.Puddle(new Vector3(0.2f, 0.03f, -5.2f), new Vector2(1.4f, 0.7f), 5f, wet.transform);
            rainProps = wet;
        }
    }
}
