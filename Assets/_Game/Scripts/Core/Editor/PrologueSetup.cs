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
    /// Builds Chapter00_Prologue: the modern classroom and its school (far west, around x −600) and the Vương family house on the morning
    /// of the arrest (around the origin): Kiều's room in the west wing, the courtyard, the main house and the gate.
    /// </summary>
    public static class PrologueSetup
    {
        public const string SceneName = "Chapter00_Prologue";
        public const string ScenePath = "Assets/_Game/Scenes/" + SceneName + ".unity";
        // Far enough west that the school, its town and hills stay beyond the camera's far plane (260 m) from the Vương house.
        public static readonly Vector3 ClassOrigin = new Vector3(-600f, 0f, 0f);

        private const string Characters = "Assets/_Game/Art/Characters/";
        private static Material _plaster, _wood, _tiles, _ridge, _stone, _red, _planks, _brick;

        [MenuItem("ThuyKieu/Prologue/Create playable scene")]
        public static void Create()
        {
            var scene = NewScene(ScenePath);
            var shared = StoryAnimationSetup.Controller();
            var s = CreateSystems(ChapterCatalog.Prologue, "Trang sách", "prologue", "Chapter01_GiaBien", ClassOrigin + new Vector3(2.9f, 0.05f, -1.15f), 0f, "classroom");
            Grade(s.Fx, -0.22f, 6f, 4f, 2f, new Color(0.5f, 0.52f, 0.6f), new Color(0.78f, 0.72f, 0.62f));
            Set(s.Fx, "_baseBloom", 0f);
            UseSky(s.CameraComponent, Sky("PrologueSky", "kloppenheim_06_puresky", 0.78f, new Color(0.58f, 0.6f, 0.62f), 60f));
            // The camera-mounted spotlight creates a moving white hotspot on classroom desks/windows.
            Set(s.Shots, "_keyLight", 0f);
            var world = new GameObject("World").transform;
            var nav = NavMesh(world);
            var director = s.Director;
            InitMaterials();

            // ------------------------------------------------------------ the classroom
            var room = ClassroomKit.Build(world, ClassOrigin, false);
            // CreateSystems uses a spacious outdoor camera by default. Start this
            // interior scene inside the rear wall, already at the classroom framing.
            s.CameraComponent.transform.SetPositionAndRotation(
                ClassOrigin + new Vector3(3.28f, 1.92f, -3.42f),
                Quaternion.Euler(10f, 0f, 0f));
            var look = ClassroomKit.StudentBody(s);
            // Ngân uses the dedicated female-student model; NuSinhToxic is the retired protagonist model.
            var ngan = ClassroomKit.SeatedGirl("Ngan", "Ngan", 1.38f, room.NganSeat.position);
            Set(ngan, "_lockFacing", true);
            var teacher = ModernActor("ThayGiao", "ThayGiao", "ThayGiao", 1.72f, room.TeacherSpot.position, 180f, Color.white);
            var classmates = Classmates(room);
            Explore(Anchor(world, "BoardTarget", room.BoardAnchor.position), director, "pl_board", "Đọc mấy câu thơ trên bảng", new Vector3(2.4f, 1.6f, 1.2f));
            var nganTalk = Point("NganTalkStand", room.NganSeat.position + new Vector3(0.75f, 0f, -0.72f), 315f, room.Root);
            Set(Explore(ngan.gameObject, director, "pl_ngan", "Nói chuyện với Ngân", Vector3.one * 1.8f), "_standPoint", nganTalk);
            Set(Explore(Anchor(world, "WindowTarget", room.WindowAnchor.position), director, "pl_window", "Nhìn ra cửa sổ", new Vector3(1.2f, 1.6f, 1.8f)), "_standPoint", room.WindowStand);
            Set(Explore(Anchor(world, "BookTarget", room.Seat.position + new Vector3(0, 0, 0.1f)), director, "pl_book", "Cất quyển Truyện Kiều vào cặp", new Vector3(1.2f, 1.6f, 1.2f)), "_standPoint", room.Seat);

            // ------------------------------------------------------------ the Vương house
            Ground(world);
            KieuRoom(world, director, out var wake);
            Courtyard(world, out var courtyard, out var gate, out var rainProps);
            // The family stands round the breakfast table on the veranda, facing where Kiều comes out into the courtyard:
            // parents either side of the table, the boy in front of it, each with room to breathe and none behind a pillar.
            // Vương Ông needs clear floor behind him for the authored collapse clip; the previous spot put his fall
            // through the rear wall and breakfast table.
            var father = CharacterActor("VuongOng", "VuongOng", "VuongOng", 1.66f, new Vector3(2.05f, 0.27f, 4.75f), 180f);
            var mother = RiggedActor("MeKieu", "MeKieu", Characters + "MeThuyKieu/Models/MeThuyKieu_Rigged.fbx", Characters + "MeThuyKieu/Tectures", 1.56f, new Vector3(-1.1f, 0.27f, 5.3f), 150f, Color.white, shared);
            var brother = RiggedActor("VuongQuan", "VuongQuan", Characters + "ThuyVan/Models/ThuyVan_Rigged.fbx", Characters + "ThuyVan/Textures", 1.52f, new Vector3(0.55f, 0.27f, 4.3f), 180f, Color.white, shared);
            var officials = new[]
            {
                CharacterActor("QuanSai", "QuanSai", "LinhCanh", 1.8f, new Vector3(-0.4f, 0f, -8f), 0f),
                CharacterActor("QuanSai2", "QuanSai", "LinhCanh", 1.78f, new Vector3(0.6f, 0f, -8f), 0f)
            };
            // Fit() leaves these rigs 8–14 cm above the floor (padded skinned bounds): stand them on it.
            foreach (var actor in new[] { father, mother, brother, officials[0], officials[1] })
            {
                GroundFeet(actor.GetComponentInChildren<Animator>().transform, actor.gameObject, actor.transform.position.y);
                actor.GetComponentInChildren<Animator>().gameObject.AddComponent<AnimatorRootAnchor>();
            }
            var marks = new GameObject("VuongMarks").transform;
            marks.SetParent(world, false);
            var fatherSpot = Point("FatherSpot", father.transform.position, father.transform.eulerAngles.y, marks);
            var brotherSpot = Point("BrotherSpot", brother.transform.position, brother.transform.eulerAngles.y, marks);
            var motherSpot = Point("MotherSpot", mother.transform.position, mother.transform.eulerAngles.y, marks);
            var motherRain = Point("MotherRainSpot", new Vector3(0.6f, 0.27f, 4.45f), 180f, marks); // veranda edge, looking to the gate
            var officialSpots = new[] { Point("OfficialSpotA", new Vector3(-1.15f, 0f, 3.35f), 0f, marks), Point("OfficialSpotB", new Vector3(1.15f, 0f, 3.35f), 0f, marks) };

            // ------------------------------------------------------------ light & atmosphere
            var sun = Sun("Sun", new Color(1f, 0.9f, 0.76f), 0.82f, new Vector3(34f, 250f, 0f));
            RenderSettings.ambientLight = new Color(0.46f, 0.45f, 0.43f);
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
            Set(worldScript, "_lookAtHands", LookAtHandsClip());
            Set(worldScript, "_kneelDown", KneelDownClip());
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
                "<color=#E8C27A>Tương tác:</color> đến gần thứ có dấu sáng vàng rồi bấm <color=#E8C27A>E</color>\n" +
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
            // The officials seize the father and the brother and drag them out through the gate into the lane.
            var arrest = Child<EscortSequence>(sequences, "arrest");
            Set(arrest, "_id", "arrest");
            Set(arrest, "_guards", new Object[] { officials[0], officials[1] });
            Set(arrest, "_captives", new Object[] { father, brother });
            Set(arrest, "_draggedWalk", DraggedWalkClip());
            Set(arrest, "_mourner", mother);
            Set(arrest, "_mournerFall", KneelDownClip());
            Set(arrest, "_speed", 2.2f);
            // Two lanes through the gate: the father on the east side, the boy on the west, so the pairs never cross.
            Set(arrest, "_exits", new Object[] { Point("ArrestA", new Vector3(0.9f, 0f, -10.4f), 180f, marks), Point("ArrestB", new Vector3(-0.7f, 0f, -9.6f), 180f, marks) });
            Set(arrest, "_gateways", new Object[] { Point("ArrestGateA", new Vector3(0.7f, 0f, -7f), 180f, marks), Point("ArrestGateB", new Vector3(-0.6f, 0f, -7f), 180f, marks) });

            Save(scene, ScenePath);
            // Brings the generated house in line with the painted panels (bars for windows, longans for palms).
            ThuyKieu.Environment.Editor.PaintedLookUpgrade.ApplyPrologue();
        }

        // ------------------------------------------------------------------ characters


        private static StoryActor[] Classmates(ClassroomKit.Parts room)
        {
            // Mostly the seated boy, a few girls; light tints so the copies do not read as twins.
            string[] models = { "HocSinhNgoi", "HocSinhNgoi", "HocSinhNu" };
            Color[] tints = { Color.white, new Color(0.86f, 0.9f, 1f), new Color(1f, 0.93f, 0.86f), new Color(0.92f, 1f, 0.92f) };
            // Two front rows and the back corner: enough to fill the room without crowding her desk.
            int[] picks = { 0, 3, 6, 9, 11, 13, 16, 20 };
            var list = new System.Collections.Generic.List<StoryActor>();
            for (int i = 0; i < picks.Length && picks[i] < room.ClassmateSeats.Count; i++)
            {
                var seat = room.ClassmateSeats[picks[i]];
                string model = models[i % models.Length];
                var classmate = model == "HocSinhNu"
                    ? ClassroomKit.SeatedGirl("BanHoc" + i, "BanHoc", 1.26f + (i % 3) * 0.04f, seat.position)
                    : ModernActor("BanHoc" + i, "BanHoc", model, 1.26f + (i % 3) * 0.04f,
                        seat.position + Vector3.down * 0.1f, 0f, tints[i % tints.Length], seat: true);
                // Background pupils keep facing the board. StoryDirector otherwise turns every
                // registered actor toward each speaker, making seated rows swivel into each other.
                Set(classmate, "_lockFacing", true);
                list.Add(classmate);
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

        /// <summary>Aged ochre lime wash, as in the painted montage panels (also applied by PaintedLookUpgrade).</summary>
        public static readonly Color PlasterTint = new Color(0.9f, 0.78f, 0.56f);

        private static void InitMaterials()
        {
            _plaster = Surface("PlPlaster", "plastered_wall_02", PlasterTint, 2.6f, 0.08f, 1.1f, 0.45f, 0.8f);
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

        // How far the writing desk in Kiều's room stands above the low table it is built from.
        private const float DeskRaise = 0.3f;

        /// <summary>
        /// The dressing-table mirror is set for an adult: its disc is lowered to Kiều's eye height (1.3 m) and tipped
        /// forward a little about the arm holding it, and it reflects at full resolution so her face reads clearly.
        /// </summary>
        private static void FitMirrorToKieu(Transform mirrorPlane)
        {
            Transform vanity = mirrorPlane.parent;
            Vector3 pivot = vanity.TransformPoint(new Vector3(0f, 1.43f, -0.1f));
            foreach (var part in new[] { "MirrorRim", "MirrorBack", "MirrorFace", "MirrorPlane" })
                vanity.Find(part).RotateAround(pivot, vanity.right, 6f);
            foreach (var part in new[] { "MirrorRim", "MirrorBack", "MirrorFace", "MirrorPlane", "MirrorArm" })
                vanity.Find(part).position += Vector3.down * 0.15f;
            Set(mirrorPlane.GetComponent<ThuyKieu.Environment.PlanarMirror>(), "_resolutionScale", 1f);
            var face = vanity.Find("MirrorFace").GetComponent<Renderer>().sharedMaterial;
            face.SetFloat("_Haze", 0.06f);
            face.SetFloat("_RimDarken", 0.3f);
            EditorUtility.SetDirty(face);
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

            // Canopy bed in the north-west corner (head to the north wall), a rug, a Ming standing screen against the
            // north wall beside the bed head, a candle at the bed's foot and a clothes cabinet on the east wall.
            Prop("Assets/_Game/Prefabs/Props/Bed.prefab", new Vector3(-9.48f, 0.06f, 5.03f), 90, 0.95f, root);
            Block("Rug", new Vector3(-8.1f, 0.065f, 2.9f), new Vector3(1.8f, 0.01f, 2.4f), Material("PlRug", new Color(0.45f, 0.14f, 0.12f)), root, false);
            Prop("Assets/_Game/Prefabs/Props/BinhPhong.prefab", new Vector3(-7.95f, 0.06f, 5.72f), 0f, 1f, root);
            StoryPropKit.Candle(new Vector3(-8.55f, 0.06f, 3.95f), root, 0f, 0.9f, 1.05f);
            Prop("Assets/_Game/Prefabs/Props/Cabinet.prefab", new Vector3(-6.86f, 0.06f, 4.65f), 270, 1f, root);
            // Dressing table with the bronze mirror (pl_guong). The mirror really reflects: standing at it, Kiều sees her face.
            var mirrorFace = StoryPropKit.Vanity(new Vector3(-9.72f, 0.06f, 1.0f), 90f, root, 41);
            FitMirrorToKieu(mirrorFace);
            var mirrorStand = Point("MirrorStand", new Vector3(-9.02f, 0.06f, 1.0f), 270f, root);
            var mirror = new GameObject("MirrorTarget");
            mirror.transform.SetParent(root, false);
            mirror.transform.position = new Vector3(-9.1f, 0.06f, 1.0f);
            var mirrorTarget = Explore(mirror, director, "pl_guong", "Soi chiếc gương đồng", new Vector3(1.2f, 1.6f, 1.2f));
            Set(mirrorTarget, "_standPoint", mirrorStand);
            Point("Focus_Mirror", mirrorFace.position, 0, root);
            // Over her right shoulder into the glass: the back of her head in the foreground, her face and the hand on her
            // cheek in the mirror, which sits above the dialogue panel. Closer than 0.9 m to her head the dialogue
            // camera would hide her (and with her, her reflection).
            Look("CAM_Mirror", new Vector3(-8.25f, 1.45f, 1.55f), mirrorFace.position + Vector3.down * 0.22f, root);
            var glow = new GameObject("MirrorFill");
            glow.transform.SetParent(root, false);
            glow.transform.position = new Vector3(-9.35f, 1.75f, 0.55f);
            var fill = glow.AddComponent<Light>();
            fill.type = LightType.Point; fill.color = new Color(1f, 0.88f, 0.72f); fill.range = 2.2f; fill.intensity = 0.9f; fill.shadows = LightShadows.None;
            // The hồ cầm leaning against the west wall (pl_dan).
            StoryPropKit.HoCam(new Vector3(-9.78f, 0.06f, 3.1f), 90f, root);
            var dan = new GameObject("DanTarget");
            dan.transform.SetParent(root, false);
            dan.transform.position = new Vector3(-9.2f, 0.06f, 3.1f);
            var danStand = Point("DanStand", new Vector3(-9.02f, 0.06f, 3.1f), 270f, root);
            Set(Explore(dan, director, "pl_dan", "Cây hồ cầm", new Vector3(1.2f, 1.6f, 1.2f)), "_standPoint", danStand);
            // From behind Kiều's left shoulder: the hồ cầm and touching hand stay visible while her sleeve hangs down,
            // instead of pointing straight into the lens like a rigid tube.
            Look("CAM_Dan", new Vector3(-8.55f, 1.15f, 3.85f), new Vector3(-9.65f, 0.78f, 3.1f), root);
            // Writing desk under the window, Kim Trọng's letter on it (pl_tho).
            // A writing desk at standing height (the low table raised by DeskRaise, its cushion gone): Kiều takes the
            // letter from it standing, and it lies near the front edge, within her reach.
            var writingDesk = StoryPropKit.LowTable(new Vector3(-8.2f, 0.06f, 0.55f), 0f, root, false, 7, backCushion: false);
            writingDesk.transform.localScale = new Vector3(1f, (0.385f + DeskRaise) / 0.385f, 1f);
            Object.DestroyImmediate(writingDesk.transform.Find("Cushion").gameObject);
            Block("Letter", new Vector3(-8.3f, 0.45f + DeskRaise, 0.68f), new Vector3(0.26f, 0.004f, 0.34f), Material("PlLetter", new Color(0.9f, 0.85f, 0.72f)), root, false, 12f);
            ClassroomKit.WorldText(root, "LetterText", "<i>Chú ta mất ở Liêu Dương,\nta phải đi hộ tang nửa năm.\nLời thề dưới trăng,\nta không quên.</i>\n<size=80%>— Kim Trọng</size>",
                new Vector3(-8.3f, 0.454f + DeskRaise, 0.68f), 192f, new Vector2(0.22f, 0.3f), 0.022f, new Color(0.15f, 0.1f, 0.08f), true);
            Block("PaperStack", new Vector3(-7.95f, 0.455f + DeskRaise, 0.68f), new Vector3(0.22f, 0.02f, 0.28f), Material("PlPaperStack", new Color(0.88f, 0.84f, 0.74f)), root, false, -6f);
            Block("InkStone", new Vector3(-7.85f, 0.46f + DeskRaise, 0.4f), new Vector3(0.12f, 0.03f, 0.18f), Material("PlInk", new Color(0.05f, 0.05f, 0.06f), 0.7f), root, false);
            Block("Brush", new Vector3(-8.0f, 0.48f + DeskRaise, 0.36f), new Vector3(0.012f, 0.012f, 0.22f), _wood, root, false, 70f);
            StoryPropKit.FlowerVase(new Vector3(-8.62f, 0.445f + DeskRaise, 0.4f), root, 5, 0.7f);
            var desk = new GameObject("DeskTarget");
            desk.transform.SetParent(root, false);
            desk.transform.position = new Vector3(-8.2f, 0.06f, 1.25f);
            var deskStand = Point("DeskStand", new Vector3(-8.2f, 0.06f, 1.38f), 180f, root);
            Set(Explore(desk, director, "pl_tho", "Bàn viết bên cửa sổ", new Vector3(1.2f, 1.6f, 1.2f)), "_standPoint", deskStand);
            Look("CAM_Desk", new Vector3(-7.7f, 1.2f + DeskRaise, 1.25f), new Vector3(-8.3f, 0.45f + DeskRaise, 0.68f), root);
            // A girl's room: her Ming embroidery frame by the window, a painting over a potted plum, a beamed ceiling.
            Prop("Assets/_Game/Prefabs/Props/KhungTheu.prefab", new Vector3(-6.95f, 0.06f, 1.25f), 90f, 1f, root);
            StoryPropKit.Scroll(new Vector3(-9.98f, 1.75f, 2.2f), 90f, root, 13);
            Prop("Assets/_Game/Prefabs/Props/ChauMai.prefab", new Vector3(-9.72f, 0.06f, 2.15f), 90f, 0.85f, root);
            Block("Ceiling", new Vector3(-8.25f, 3.18f, 3f), new Vector3(3.8f, 0.05f, 6.2f), _planks, root, false);
            for (int i = 0; i < 4; i++)
                Block("CeilingBeam", new Vector3(-8.25f, 3.08f, 0.6f + i * 1.6f), new Vector3(3.8f, 0.14f, 0.16f), _wood, root, false);
            // Morning sun through the lattice window: a shadowed beam lays the lattice pattern across the floor.
            var sunBeam = new GameObject("WindowSun");
            sunBeam.transform.SetParent(root, false);
            sunBeam.transform.position = new Vector3(-7.7f, 3.0f, -1.7f);
            sunBeam.transform.rotation = Quaternion.LookRotation(new Vector3(-8.5f, 0.1f, 1.9f) - sunBeam.transform.position);
            var beam = sunBeam.AddComponent<Light>();
            beam.type = LightType.Spot; beam.color = new Color(1f, 0.88f, 0.66f); beam.intensity = 12f; beam.range = 7f;
            beam.spotAngle = 48f; beam.innerSpotAngle = 30f; beam.shadows = LightShadows.Soft;
            Dust("WindowDust", new Vector3(-8.3f, 1.2f, 1.2f), new Vector3(1.2f, 1.6f, 1.8f), root, new Color(1f, 0.9f, 0.7f, 0.35f), 60);
            // Door out to the courtyard (pl_san).
            var door = new GameObject("DoorTarget");
            door.transform.SetParent(root, false);
            door.transform.position = new Vector3(-6.9f, 0.06f, 2.9f);
            Explore(door, director, "pl_san", "Ra sân", new Vector3(1f, 1.8f, 1.4f));
            StoryPropKit.Curtain(new Vector3(-6.32f, 2.3f, 2.9f), 1.1f, 1.6f, 90f, new Color(0.75f, 0.6f, 0.3f), root);
            var day = new GameObject("RoomDaylight");
            day.transform.SetParent(root, false);
            day.transform.position = new Vector3(-8.2f, 2.4f, 2.6f);
            var light = day.AddComponent<Light>();
            light.type = LightType.Point; light.color = new Color(1f, 0.9f, 0.76f); light.range = 6f; light.intensity = 1.4f; light.shadows = LightShadows.None;
            Look("CAM_Wide_KieuRoom", new Vector3(-6.9f, 2.3f, 0.6f), new Vector3(-8.8f, 0.7f, 4.6f), root);
            wake = Point("WakeSpot", new Vector3(-8.2f, 0.08f, 3.4f), 165f, root);
        }

        /// <summary>
        /// What the main door frames from the courtyard: a shallow lit bay of the hall with the ancestral altar facing
        /// the door under a lacquered plaque, instead of a black void.
        /// </summary>
        /// <summary>The ordinary walk the captives are hurried along with (the escort's _draggedWalk).</summary>
        public static AnimationClip DraggedWalkClip() =>
            AssetDatabase.LoadAllAssetsAtPath("Assets/_Game/Animations/Shared/Walking.fbx").OfType<AnimationClip>().FirstOrDefault(c => !c.name.StartsWith("__preview"));

        private const string LookAtHandsPath = "Assets/_Game/Animations/ThuyKieu/TK-LookAtHands.fbx";

        /// <summary>
        /// Kiều studying her new hands, authored in ArtSource/ThuyKieu/ThuyKieu_LookAtHands.blend on the same rig as her
        /// walk: imported as a one-shot Humanoid clip. The export is the bare skeleton (no "Armature" node above the hips),
        /// so it builds its own avatar from that skeleton rather than copying the walk model's.
        /// </summary>
        public static AnimationClip LookAtHandsClip() => KieuClip(LookAtHandsPath, "TK-LookAtHands");

        /// <summary>Kiều shoved to her knees (TK-KneelDown, authored in the same .blend), held on its last pose.</summary>
        public static AnimationClip KneelDownClip() => KieuClip("Assets/_Game/Animations/ThuyKieu/TK-KneelDown.fbx", "TK-KneelDown");

        private static AnimationClip KieuClip(string path, string name)
        {
            if (AssetImporter.GetAtPath(path) is not ModelImporter importer) return null;
            if (importer.animationType != ModelImporterAnimationType.Human || importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel
                || importer.clipAnimations.Length == 0)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.importAnimation = true;
                importer.clipAnimations = new ModelImporterClipAnimation[0];
                importer.SaveAndReimport();
                var clips = importer.defaultClipAnimations;
                foreach (var clip in clips)
                {
                    clip.name = name;
                    clip.loopTime = false;
                    clip.lockRootRotation = clip.lockRootHeightY = clip.lockRootPositionXZ = true;
                    clip.keepOriginalPositionY = true;
                }
                importer.clipAnimations = clips;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().FirstOrDefault(c => !c.name.StartsWith("__preview"));
        }

        /// <summary>Gives the open prologue scene's world script the hands clip without rebuilding the scene.</summary>
        [MenuItem("ThuyKieu/Prologue/Assign look-at-hands clip")]
        public static void AssignLookAtHands()
        {
            var world = Object.FindFirstObjectByType<PrologueWorld>(FindObjectsInactive.Include);
            if (world == null) return;
            Set(world, "_lookAtHands", LookAtHandsClip());
            Set(world, "_kneelDown", KneelDownClip());
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(world.gameObject.scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(world.gameObject.scene);
        }

        public static void HallAltar(Transform root)
        {
            var dim = Material("PlHallWall", new Color(0.42f, 0.35f, 0.27f));
            Block("HallFloor", new Vector3(0f, 0.27f, 6.95f), new Vector3(2.5f, 0.02f, 1.5f), _planks, root, false);
            Block("HallBack", new Vector3(0f, 1.8f, 7.75f), new Vector3(2.7f, 3.6f, 0.1f), dim, root);
            foreach (int side in new[] { -1, 1 })
                Block("HallSide", new Vector3(side * 1.25f, 1.8f, 6.95f), new Vector3(0.1f, 3.6f, 1.7f), dim, root);
            Block("HallCeiling", new Vector3(0f, 3.02f, 6.95f), new Vector3(2.5f, 0.05f, 1.7f), _wood, root, false);
            Prop("Assets/_Game/Prefabs/Props/Altar.prefab", new Vector3(0f, 0.28f, 7.35f), 180f, 1f, root);
            Block("HallPlaque", new Vector3(0f, 2.35f, 7.68f), new Vector3(1.3f, 0.38f, 0.04f), _red, root, false);
            ClassroomKit.WorldText(root, "HallPlaqueText", "<b>PHÚC ĐỨC</b>", new Vector3(0f, 2.35f, 7.655f), 0f, new Vector2(1.2f, 0.32f), 0.15f, new Color(0.95f, 0.75f, 0.3f));
            var glow = new GameObject("HallLamp");
            glow.transform.SetParent(root, false);
            glow.transform.position = new Vector3(0f, 1.9f, 7.0f);
            var lamp = glow.AddComponent<Light>();
            lamp.type = LightType.Point; lamp.color = new Color(1f, 0.78f, 0.5f); lamp.range = 2.6f; lamp.intensity = 1.6f; lamp.shadows = LightShadows.None;
        }

        /// <summary>The east wing's courtyard face: a framed double door and two lattice windows (it stays closed).</summary>
        public static void EastWingFront(Transform root)
        {
            const float face = 6.48f;
            foreach (int side in new[] { -1, 1 })
            {
                Block("EastWingDoorPost", new Vector3(face, 1.15f, 3f + side * 0.62f), new Vector3(0.1f, 2.3f, 0.12f), _red, root, false);
                Block("EastWingDoorLeaf", new Vector3(face - 0.01f, 1.1f, 3f + side * 0.29f), new Vector3(0.05f, 2.2f, 0.56f), _wood, root, false);
                Block("EastWingDoorRing", new Vector3(face - 0.05f, 1.15f, 3f + side * 0.06f), new Vector3(0.02f, 0.08f, 0.08f), Material("PlBronze", new Color(0.55f, 0.4f, 0.2f), 0.6f), root, false);
            }
            Block("EastWingDoorLintel", new Vector3(face, 2.35f, 3f), new Vector3(0.12f, 0.12f, 1.36f), _red, root, false);
            var frame = Material("PlLatticeFrame", new Color(0.22f, 0.08f, 0.05f), 0.4f);
            var paper = Emissive("PlWindowDay", new Color(0.95f, 0.9f, 0.76f), 0.55f);
            foreach (float z in new[] { 1.1f, 4.9f }) Lattice("EastWingWindow", new Vector3(face - 0.03f, 1.7f, z), 1.1f, 0.9f, 270f, frame, paper, root);
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
                Block("GateLeaf", new Vector3(side * 1.95f, 1.2f, -6.3f), new Vector3(0.08f, 2.3f, 1.4f), _wood, root, true, side * 8f);
            Couplets(new Vector3(-1.9f, 1.7f, -7.25f), new Vector3(1.9f, 1.7f, -7.25f), 0f, root); // glyphs face the lane
            gate = Point("GatePoint", new Vector3(0f, 0f, -8.5f), 0f, root);

            // Main house.
            Block("HouseBack", new Vector3(0f, 1.8f, 11.6f), new Vector3(13f, 3.6f, 0.3f), _plaster, root);
            Block("HouseSideW", new Vector3(-6.4f, 1.8f, 8.8f), new Vector3(0.3f, 3.6f, 5.8f), _plaster, root);
            Block("HouseSideE", new Vector3(6.4f, 1.8f, 8.8f), new Vector3(0.3f, 3.6f, 5.8f), _plaster, root);
            var frontW = Block("HouseFrontW", new Vector3(-3.7f, 1.8f, 6f), new Vector3(5.4f, 3.6f, 0.3f), _plaster, root);
            var frontE = Block("HouseFrontE", new Vector3(3.7f, 1.8f, 6f), new Vector3(5.4f, 3.6f, 0.3f), _plaster, root);
            Block("HouseDoorLintel", new Vector3(0f, 3.05f, 6f), new Vector3(2f, 1.1f, 0.32f), _red, root);
            HallAltar(root);
            foreach (var wall in new[] { frontW, frontE }) StoryPropKit.TimberFrame(wall, _wood, 2f);
            var frame = Material("PlLatticeFrame", new Color(0.22f, 0.08f, 0.05f), 0.4f);
            var paper = Emissive("PlWindowDay", new Color(0.95f, 0.9f, 0.76f), 0.55f);
            Lattice("HouseWindow", new Vector3(-3.6f, 1.8f, 5.83f), 1.6f, 1.2f, 180f, frame, paper, root);
            Lattice("HouseWindow", new Vector3(3.6f, 1.8f, 5.83f), 1.6f, 1.2f, 180f, frame, paper, root);
            Couplets(new Vector3(-1.25f, 1.7f, 5.83f), new Vector3(1.25f, 1.7f, 5.83f), 0f, root); // glyphs face the courtyard
            Block("Veranda", new Vector3(0f, 0.13f, 5.2f), new Vector3(13f, 0.26f, 1.9f), _stone, root);
            for (int i = 0; i < 4; i++) Pillar("VerandaPillar", new Vector3(-4.5f + i * 3f, 0.26f, 4.45f), 0.16f, 3.5f, _red, _stone, root);
            Block("VerandaBeam", new Vector3(0f, 3.8f, 4.45f), new Vector3(13.4f, 0.2f, 0.22f), _wood, root, false);
            Roof("HouseRoof", new Vector3(0f, 5.4f, 8.4f), 14.2f, 9f, 26f, _tiles, _ridge, root);
            // Breakfast laid out on the veranda, centred on the main door.
            StoryPropKit.LowTable(new Vector3(0f, 0.26f, 5.15f), 0f, root, true, 3);
            for (int i = 0; i < 4; i++)
                Cylinder("RiceBowl", new Vector3(-0.35f + i * 0.24f, 0.62f, 5.1f + (i % 2) * 0.12f), 0.06f, 0.05f, Material("PlBowl", new Color(0.9f, 0.88f, 0.82f), 0.5f), root, false);
            // East wing (kitchen and store), closed.
            Block("EastWing", new Vector3(8.6f, 1.6f, 3f), new Vector3(4.2f, 3.2f, 6f), _plaster, root);
            Roof("EastWingRoof", new Vector3(8.6f, 4.4f, 3f), 4.8f, 6.8f, 26f, _tiles, _ridge, root, false);
            EastWingFront(root);
            // Yard: well, jars, trees, plants, a washing line.
            Prop("Assets/_Game/Prefabs/Props/Well.prefab", new Vector3(5.8f, 0f, -2.6f), 20, 1f, root);
            Jars(new Vector3(8.8f, 0f, -4.8f), 4, root, 711);
            Jars(new Vector3(-9.8f, 0f, -5.6f), 2, root, 712);
            Prop("Assets/_Game/Prefabs/Vegetation/ArecaPalm.prefab", new Vector3(-8.8f, 0f, -3.2f), 0, 0.95f, root);
            Prop("Assets/_Game/Prefabs/Vegetation/BananaTree.prefab", new Vector3(9.2f, 0f, -1.2f), 40, 0.85f, root);
            Prop("Assets/_Game/Prefabs/Vegetation/ArecaPalm.prefab", new Vector3(4f, 0f, -14f), 70, 1.1f, root);
            Prop("Assets/_Game/Prefabs/Vegetation/BananaTree.prefab", new Vector3(-5f, 0f, -13f), 10, 1f, root);
            // Potted plums along the veranda, between the pillars and clear of the door.
            // Keep the east planter outside Vương Ông's sideways collapse arc (his animated bones extend beyond the capsule).
            foreach (float x in new[] { -5.7f, -3f, 4.35f, 5.7f }) Prop("Assets/_Game/Prefabs/Props/ChauMai.prefab", new Vector3(x, 0.26f, 4.95f), 0f, 1.1f, root);
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
