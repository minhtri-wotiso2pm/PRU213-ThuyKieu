using System.IO;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Chapters;
using ThuyKieu.Gameplay.Chase;
using ThuyKieu.Gameplay.Sequences;
using ThuyKieu.Gameplay.Stealth;
using ThuyKieu.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static ThuyKieu.Core.Editor.StorySceneKit;
using static ThuyKieu.Core.Editor.StoryArtKit;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Builds Chapter06_KimTrong: Bạch Vân temple at dawn (north, z 30–46), the Lâm Tri street below it (z 14–22) with the
    /// tea stall, the Phúc Ký silver shop and the magistrate's gate, and Kim Trọng's uncle's study on the south side.
    /// Also creates the Chapter 7 placeholder.
    /// </summary>
    public static class Chapter06Setup
    {
        public const string SceneName = "Chapter06_KimTrong";
        public const string ScenePath = "Assets/_Game/Scenes/" + SceneName + ".unity";
        public const string PlaceholderName = "Chapter07_Placeholder";
        public const string PlaceholderPath = "Assets/_Game/Scenes/" + PlaceholderName + ".unity";

        private static Material _plaster, _wood, _tiles, _ridge, _stone, _red, _planks;

        [MenuItem("ThuyKieu/Chapter 6/Create playable scene")]
        public static void Create()
        {
            var scene = NewScene(ScenePath);
            var shared = StoryAnimationSetup.Controller();
            var spawn = new Vector3(-7f, 0.36f, 39f); // on the veranda boards
            var s = CreateSystems(6, "Kim Trọng", "chapter_6", PlaceholderName, spawn, 90f, "temple");
            s.CameraComponent.backgroundColor = new Color(0.75f, 0.72f, 0.66f);
            Grade(s.Fx, 0.15f, 10f, 4f, 6f, new Color(0.45f, 0.45f, 0.55f), new Color(0.75f, 0.62f, 0.48f));
            UseSky(s.CameraComponent, Sky("Ch6Sky", "kloppenheim_06_puresky", 0.95f, new Color(0.6f, 0.6f, 0.58f), 140f));
            var world = new GameObject("World").transform;
            var nav = NavMesh(world);
            var director = s.Director;

            _plaster = Surface("Ch6Plaster", "plastered_wall_02", new Color(0.95f, 0.9f, 0.8f), 2.6f, 0.08f, 1.2f, 0.5f, 1f);
            _wood = Surface("Ch6Wood", "dark_wood", new Color(0.75f, 0.62f, 0.5f), 1.2f, 0.3f, 1f, 0.25f);
            _tiles = Surface("Ch6Tiles", "clay_roof_tiles_02", new Color(0.62f, 0.45f, 0.4f), 1.4f, 0.25f, 1.2f, 0.35f);
            _ridge = Material("Ch6Ridge", new Color(0.16f, 0.08f, 0.07f));
            _stone = Surface("Ch6Stone", "monastery_stone_floor", new Color(0.82f, 0.8f, 0.76f), 2.2f, 0.2f, 1.1f, 0.35f);
            _red = Surface("Ch6Lacquer", "lacquered_cherry_wood", new Color(0.95f, 0.38f, 0.32f), 1.2f, 0.5f, 0.8f, 0.2f);
            _planks = Surface("Ch6Planks", "old_wooden_floor_02", new Color(0.8f, 0.68f, 0.56f), 2f, 0.3f, 1f, 0.35f);

            Block("Ground", new Vector3(0f, -0.1f, 20f), new Vector3(72f, 0.2f, 56f), Surface("Ch6Ground", "brown_mud_dry", new Color(0.82f, 0.74f, 0.64f), 3f, 0.12f, 1f, 0.45f), world);
            Temple(world);
            Street(world);
            Study(world, director, out var studyPlayer, out var studyKim, out var nightPlayer, out var nightKim, out var lamp);
            SilverShop(world);
            MagistrateGate(world);
            TeaStall(world);

            // ------------------------------------------------------------ characters
            var kim = StaticActor("KimTrong", "KimTrong", "Assets/_Game/Art/Characters/KimTrong/Static/KimTrong.obj",
                "Assets/_Game/Art/Characters/KimTrong/Static/texture_pbr_20250901.png", 1.76f, new Vector3(0f, 0, 27f), 0f, 40f);
            var follower = kim.gameObject.AddComponent<Follower>();
            Set(follower, "_player", s.Player);
            Set(follower, "_distance", 1.8f);
            var coLam = RiggedActor("CoLam", "CoLam", "Assets/_Game/Art/Characters/MeThuyKieu/Models/MeThuyKieu_Rigged.fbx",
                "Assets/_Game/Art/Characters/MeThuyKieu/Tectures", 1.6f, new Vector3(-13.4f, 0, 4.6f), 60f, new Color(0.7f, 0.82f, 1f), shared);
            var baNuoc = RiggedActor("BaHangNuoc", "BaHangNuoc", "Assets/_Game/Art/Characters/MeThuyKieu/Models/MeThuyKieu_Rigged.fbx",
                "Assets/_Game/Art/Characters/MeThuyKieu/Tectures", 1.56f, new Vector3(-20f, 0, 21.3f), 180f, new Color(0.9f, 0.75f, 0.55f), shared);
            var thuLai = RiggedActor("ThuLai", "ThuLai", "Assets/_Game/Art/Characters/MaGiamSinh/Models/MaGiamSinh_Rigged.fbx",
                "Assets/_Game/Art/Characters/MaGiamSinh/Tectures", 1.74f, new Vector3(29.4f, 0, 18f), 270f, new Color(0.6f, 0.66f, 0.78f), shared);
            var chuBac = RiggedActor("ChuHieuBac", "ChuHieuBac", "Assets/_Game/Art/Characters/MaGiamSinh/Models/MaGiamSinh_Rigged.fbx",
                "Assets/_Game/Art/Characters/MaGiamSinh/Tectures", 1.7f, new Vector3(15f, 0, 11.3f), 0f, new Color(0.85f, 0.72f, 0.4f), shared);
            var messenger = RiggedActor("GiaNhan", "GiaNhan", "Assets/_Game/Art/Characters/MaGiamSinh/Models/MaGiamSinh_Rigged.fbx",
                "Assets/_Game/Art/Characters/MaGiamSinh/Tectures", 1.7f, new Vector3(-9f, 0, 12.4f), 180f, new Color(0.55f, 0.55f, 0.55f), shared);
            var searcher = RiggedActor("NguoiCuaTuBa", "NguoiCanh", "Assets/_Game/Art/Characters/MaGiamSinh/Models/MaGiamSinh_Rigged.fbx",
                "Assets/_Game/Art/Characters/MaGiamSinh/Tectures", 1.78f, new Vector3(-24f, 0, 18f), 90f, new Color(0.45f, 0.32f, 0.26f), shared);
            Crowd(world, shared);

            Explore(baNuoc.gameObject, director, "c6_ba_nuoc", "Nói chuyện với bà hàng nước", Vector3.one * 1.9f);
            Explore(thuLai.gameObject, director, "c6_thu_lai", "Gặp thư lại huyện đường", Vector3.one * 1.9f);
            Explore(chuBac.gameObject, director, "c6_hieu_bac", "Hỏi chủ hiệu bạc", Vector3.one * 1.9f);
            Explore(coLam.gameObject, director, "c6_co_lam", "Nói chuyện với chị Lam", Vector3.one * 1.9f);
            var desk = new GameObject("DossierDesk");
            desk.transform.SetParent(world, false);
            desk.transform.position = new Vector3(-9f, 0f, 8.2f);
            Explore(desk, director, "c6_dossier", "Sắp xếp hồ sơ cùng Kim Trọng", new Vector3(2.4f, 1.6f, 1.6f));

            // One of Tú Bà's men combing the street: he only notices someone who runs.
            var vision = searcher.gameObject.AddComponent<VisionSensor>();
            var eyes = Point("Eyes", searcher.transform.position + Vector3.up * 1.64f, 0, searcher.transform);
            eyes.localRotation = Quaternion.identity;
            Set(vision, "_eyes", eyes);
            Set(vision, "_player", s.Player);
            Set(vision, "_range", 10f);
            Set(vision, "_angle", 100f);
            Set(vision, "_secondsToSpot", 1.1f);
            Set(vision, "_onlyInRestrictedZones", false);
            Set(vision, "_onlyWhenConspicuous", true);
            Set(vision, "_occluders", 1 << EnvironmentLayer);
            var route = new GameObject("SearcherRoute").transform;
            route.SetParent(world, false);
            var patrol = searcher.gameObject.AddComponent<GuardPatrol>();
            Set(patrol, "_waypoints", new Object[]
            {
                Point("A", new Vector3(-26f, 0, 18f), 90, route), Point("B", new Vector3(-6f, 0, 16.5f), 90, route),
                Point("C", new Vector3(8f, 0, 19.5f), 90, route), Point("D", new Vector3(24f, 0, 17f), 270, route)
            });
            Set(patrol, "_vision", vision);
            Set(patrol, "_speed", 1.2f);
            Set(patrol, "_pause", 3f);
            Chapter05Setup.AddAlertIcon(searcher.transform, vision, patrol);

            // ------------------------------------------------------------ cameras & spots
            Look("CAM_Wide_TempleYard", new Vector3(4.5f, 2.6f, 33.5f), new Vector3(-4.5f, 1.2f, 39.5f), world);
            Look("CAM_Wide_Study", new Vector3(-4.4f, 2.3f, 12.6f), new Vector3(-10.5f, 0.9f, 6.5f), world);
            var kimArrival = Point("KimArrival", new Vector3(0f, 0, 27f), 0f, world);
            var kimTemple = Point("KimTempleSpot", new Vector3(-3.8f, 0, 39.3f), 270f, world);
            var streetPlayer = Point("StreetStart", new Vector3(-9f, 0.05f, 15.6f), 90f, world);

            // ------------------------------------------------------------ light & atmosphere
            var sun = Sun("Sun", new Color(1f, 0.86f, 0.7f), 1.15f, new Vector3(30, 125, 0));
            RenderSettings.ambientLight = new Color(0.44f, 0.42f, 0.4f);
            RenderSettings.fogColor = new Color(0.75f, 0.72f, 0.66f);
            RenderSettings.fogDensity = 0.007f;

            var worldScript = s.Root.AddComponent<Chapter06World>();
            Set(worldScript, "_director", director);
            Set(worldScript, "_navMesh", nav);
            Set(worldScript, "_kimTrong", kim);
            Set(worldScript, "_kimFollower", follower);
            Set(worldScript, "_kimArrival", kimArrival);
            Set(worldScript, "_kimTempleSpot", kimTemple);
            Set(worldScript, "_coLam", coLam);
            Set(worldScript, "_messenger", messenger);
            Set(worldScript, "_searcher", patrol);
            Set(worldScript, "_studyPlayer", studyPlayer);
            Set(worldScript, "_studyKim", studyKim);
            Set(worldScript, "_streetPlayer", streetPlayer);
            Set(worldScript, "_nightPlayer", nightPlayer);
            Set(worldScript, "_nightKim", nightKim);
            Set(worldScript, "_sun", sun);
            Set(worldScript, "_studyLamp", lamp);

            // ------------------------------------------------------------ sequences
            var sequences = new GameObject("Sequences").transform;
            sequences.SetParent(s.Root.transform, false);
            var tutorial = Child<TutorialSequence>(sequences, "tutorial_street");
            Set(tutorial, "_id", "tutorial_street");
            Set(tutorial, "_body", "<color=#E8C27A>Lập hồ sơ:</color> tìm người làm chứng và bằng chứng trên phố huyện, theo thứ tự bạn muốn.\n\n" +
                "• Mỗi người cần thấy <color=#E8C27A>đúng thứ họ hiểu được</color>: đưa sai giấy tờ hai lần là họ không nói nữa.\n" +
                "• Kim Trọng đi cạnh bạn; khi chàng đủ tin bạn, chàng sẽ đứng ra nói giúp.\n" +
                "• Người của Tú Bà đang tìm bạn trên phố. <color=#F08080>Đừng chạy</color> — đi chậm thì lẫn vào đám đông.\n" +
                "• Đủ hai nơi rồi thì có thể về thư phòng sắp xếp hồ sơ.");
            Set(tutorial, "_controlsHint", "WASD: đi · E: nói chuyện / tương tác · Chuột phải: quan sát · J: sổ tay · Đừng chạy trên phố");
            var deal = Child<DocumentSequence>(sequences, "document_deal_kt");
            Set(deal, "_id", "document_deal_kt");
            Set(deal, "_view", s.Hud.GetComponent<DocumentView>());
            Set(deal, "_title", "GIAO KÈO");
            Set(deal, "_body", Chapter04Setup.DealText);
            Set(deal, "_sealed", true);
            var compare = Child<ComparisonSequence>(sequences, "doi_chieu");
            Set(compare, "_id", "doi_chieu");
            Set(compare, "_view", s.Hud.GetComponent<DossierView>());
            var dossier = Child<DossierSequence>(sequences, "ho_so");
            Set(dossier, "_id", "ho_so");
            Set(dossier, "_view", s.Hud.GetComponent<DossierView>());
            Chapter03Setup.Fx(sequences, "kieu_voice", "spirit", "memory", "none", 2f, 0f, 1);

            Save(scene, ScenePath);
            CreatePlaceholder();
        }

        private static void Look(string name, Vector3 position, Vector3 target, Transform parent)
        {
            Point(name, position, 0, parent).rotation = Quaternion.LookRotation(target - position);
        }

        // ------------------------------------------------------------------ temple

        private static void Temple(Transform world)
        {
            var root = new GameObject("BachVanTemple").transform;
            root.SetParent(world, false);
            Block("TempleYard", new Vector3(0f, 0.012f, 38f), new Vector3(24f, 0.024f, 16f), _stone, root, false);
            Block("TempleWallW", new Vector3(-12.2f, 1.4f, 38f), new Vector3(0.4f, 2.8f, 16.4f), _plaster, root);
            Block("TempleWallE", new Vector3(12.2f, 1.4f, 38f), new Vector3(0.4f, 2.8f, 16.4f), _plaster, root);
            Block("TempleWallN", new Vector3(0f, 1.4f, 46.2f), new Vector3(24.8f, 2.8f, 0.4f), _plaster, root);
            Block("TempleWallSW", new Vector3(-7.2f, 1.4f, 30f), new Vector3(10f, 2.8f, 0.4f), _plaster, root);
            Block("TempleWallSE", new Vector3(7.2f, 1.4f, 30f), new Vector3(10f, 2.8f, 0.4f), _plaster, root);
            foreach (float x in new[] { -2.2f, 2.2f }) Pillar("GatePillar", new Vector3(x, 0, 30f), 0.2f, 3.6f, _red, _stone, root);
            Roof("TempleGateRoof", new Vector3(0f, 4.1f, 30f), 5.6f, 2.2f, 26f, _tiles, _ridge, root);
            // Main hall with its veranda, where Kiều wakes.
            Block("HallPlatform", new Vector3(0f, 0.2f, 44f), new Vector3(16f, 0.4f, 4f), _stone, root);
            Block("HallBack", new Vector3(0f, 2.2f, 45.8f), new Vector3(16f, 4f, 0.4f), _plaster, root);
            for (int i = -3; i <= 3; i++) Pillar("HallPillar", new Vector3(i * 2.5f, 0.4f, 42.3f), 0.17f, 3.4f, _red, _stone, root);
            Roof("HallRoof", new Vector3(0f, 5.2f, 44f), 18f, 6.2f, 26f, _tiles, _ridge, root);
            Prop("Assets/_Game/Prefabs/Props/Altar.prefab", new Vector3(0f, 0.4f, 45.2f), 180, 1f, root);
            Cylinder("IncenseBurner", new Vector3(0f, 0f, 36.5f), 0.45f, 0.9f, Material("Ch6Bronze", new Color(0.42f, 0.32f, 0.18f), 0.6f), root);
            Dust("IncenseSmoke", new Vector3(0f, 1.6f, 36.5f), new Vector3(0.6f, 1.5f, 0.6f), root, new Color(0.9f, 0.9f, 0.88f, 0.25f), 30);
            // A wooden veranda bench under the west eave: the corner where the monks laid Kiều.
            Block("Veranda", new Vector3(-8.5f, 0.15f, 39f), new Vector3(6f, 0.3f, 4f), _planks, root);
            Roof("VerandaEave", new Vector3(-8.5f, 3.4f, 39f), 6.6f, 4.6f, 18f, _tiles, _ridge, root, false);
            foreach (float z in new[] { 37.2f, 40.8f }) Pillar("VerandaPost", new Vector3(-5.8f, 0.3f, z), 0.12f, 2.8f, _wood, _stone, root);
            Block("Mat", new Vector3(-8.2f, 0.31f, 39.2f), new Vector3(1.8f, 0.02f, 0.9f), Material("Ch6Mat", new Color(0.72f, 0.62f, 0.4f)), root, false);
            Prop("Assets/_Game/Prefabs/Vegetation/BananaTree.prefab", new Vector3(10f, 0, 33.5f), 30, 0.8f, root);
            Prop("Assets/_Game/Prefabs/Vegetation/ArecaPalm.prefab", new Vector3(-10.5f, 0, 32.5f), 0, 0.8f, root);
            Lantern("TempleLanternL", new Vector3(-2.2f, 3f, 30.6f), new Color(1f, 0.6f, 0.3f), root, 0f, 0f);
            Lantern("TempleLanternR", new Vector3(2.2f, 3f, 30.6f), new Color(1f, 0.6f, 0.3f), root, 0f, 0f);
            Leaves("TempleLeaves", new Vector3(0f, 5f, 38f), new Vector3(20f, 1f, 14f), root, new Color(0.55f, 0.5f, 0.2f), 14);
            // Steps down to the street between two walls.
            Block("StepsWallW", new Vector3(-3.3f, 1.2f, 26f), new Vector3(0.4f, 2.4f, 8f), _plaster, root);
            Block("StepsWallE", new Vector3(3.3f, 1.2f, 26f), new Vector3(0.4f, 2.4f, 8f), _plaster, root);
            for (int i = 0; i < 6; i++) Block("Step", new Vector3(0f, 0.01f, 23f + i * 1.2f), new Vector3(6f, 0.02f, 0.25f), _stone, root, false);
        }

        // ------------------------------------------------------------------ street

        private static void Street(Transform world)
        {
            var root = new GameObject("LamTriStreet").transform;
            root.SetParent(world, false);
            Block("StreetStones", new Vector3(0f, 0.012f, 18f), new Vector3(66f, 0.024f, 8f), _stone, root, false);
            var random = new System.Random(61);
            Row(root, new Vector3(-32f, 0, 22f), new Vector3(-3.5f, 0, 22f), 180f, random);
            Row(root, new Vector3(3.5f, 0, 22f), new Vector3(32f, 0, 22f), 180f, random);
            Row(root, new Vector3(-32f, 0, 14f), new Vector3(-15f, 0, 14f), 0f, random);
            Row(root, new Vector3(-3f, 0, 14f), new Vector3(11f, 0, 14f), 0f, random);
            Row(root, new Vector3(19f, 0, 14f), new Vector3(32f, 0, 14f), 0f, random);
            Block("StreetEndWest", new Vector3(-32.4f, 1.6f, 18f), new Vector3(0.6f, 3.2f, 8f), _plaster, root);
            for (float x = -28f; x < 30f; x += 9f)
            {
                Lantern("StreetLantern", new Vector3(x, 2.8f, 21.4f), new Color(1f, 0.3f, 0.18f), root, 0f, 0f);
                Rope(new Vector3(x - 4f, 3.3f, 14.4f), new Vector3(x + 4f, 3.3f, 21.6f), 0.4f, root);
            }
            Jars(new Vector3(-27f, 0, 21.2f), 3, root, 31);
            Jars(new Vector3(6f, 0, 14.8f), 2, root, 32);
            Jars(new Vector3(23f, 0, 21.3f), 2, root, 33);
            PottedPlant(new Vector3(-3.6f, 0, 21.4f), 1f, root, 4, true);
            PottedPlant(new Vector3(3.6f, 0, 21.4f), 1f, root, 5, true);
        }

        private static void Row(Transform parent, Vector3 from, Vector3 to, float yaw, System.Random random)
        {
            Vector3 along = (to - from).normalized;
            float length = Vector3.Distance(from, to);
            Vector3 front = Quaternion.Euler(0, yaw, 0) * Vector3.forward;
            const float depth = 6f;
            for (float d = 0f; d < length - 0.5f;)
            {
                float width = Mathf.Min(length - d, 4.5f + (float)random.NextDouble() * 2.5f);
                Vector3 centre = from + along * (d + width * 0.5f) - front * depth * 0.5f;
                float height = 3.2f + (float)random.NextDouble() * 1.2f;
                Block("House", centre + Vector3.up * height * 0.5f, new Vector3(width, height, depth), random.NextDouble() < 0.5 ? _plaster : _wood, parent);
                Roof("HouseRoof", centre + Vector3.up * (height + 0.75f), width + 0.5f, depth + 0.6f, 24f, _tiles, _ridge, parent);
                Vector3 face = from + along * (d + width * 0.5f) + front * 0.02f;
                Block("Door", face + Vector3.up * 1.05f, new Vector3(1f, 2.1f, 0.05f), Material("Ch6Door", new Color(0.3f, 0.18f, 0.1f)), parent, false);
                if (random.NextDouble() < 0.5)
                    Block("Shutter", face + along * (width * 0.3f) + Vector3.up * 1.6f, new Vector3(0.9f, 0.7f, 0.05f), _wood, parent, false);
                if (random.NextDouble() < 0.35)
                    Block("Awning", face + front * 0.6f + Vector3.up * 2.35f, new Vector3(width * 0.8f, 0.05f, 1.2f), Material("Ch6Awning", new Color(0.55f, 0.42f, 0.25f)), parent, false);
                d += width;
            }
        }

        private static void Study(Transform world, StoryDirector director, out Transform studyPlayer, out Transform studyKim,
            out Transform nightPlayer, out Transform nightKim, out GameObject lamp)
        {
            var root = new GameObject("KimStudy").transform;
            root.SetParent(world, false);
            Block("StudyFloor", new Vector3(-9f, 0.015f, 8f), new Vector3(12f, 0.03f, 12f), _planks, root, false);
            Block("StudyFrontW", new Vector3(-12.3f, 1.6f, 14f), new Vector3(5.4f, 3.2f, 0.3f), _plaster, root);
            Block("StudyFrontE", new Vector3(-5.7f, 1.6f, 14f), new Vector3(5.4f, 3.2f, 0.3f), _plaster, root);
            Block("StudyDoorLintel", new Vector3(-9f, 2.85f, 14f), new Vector3(1.4f, 0.7f, 0.32f), _wood, root);
            Block("StudyBack", new Vector3(-9f, 1.6f, 2f), new Vector3(12f, 3.2f, 0.3f), _plaster, root);
            Block("StudyWallW", new Vector3(-15f, 1.6f, 8f), new Vector3(0.3f, 3.2f, 12f), _plaster, root);
            Block("StudyWallE", new Vector3(-3f, 1.6f, 8f), new Vector3(0.3f, 3.2f, 12f), _plaster, root);
            Block("InnerRoomWall", new Vector3(-12f, 1.6f, 6.4f), new Vector3(0.2f, 3.2f, 3.2f), _wood, root);
            Roof("StudyRoof", new Vector3(-9f, 4.1f, 8f), 12.8f, 12.8f, 22f, _tiles, _ridge, root);
            Block("Signboard", new Vector3(-9f, 3.5f, 14.2f), new Vector3(2.2f, 0.45f, 0.06f), Material("Ch6Sign", new Color(0.4f, 0.08f, 0.06f), 0.5f), root, false);
            // Bookshelves, the desk with papers and inkstone, a lamp.
            for (int i = 0; i < 4; i++)
            {
                Block("Bookshelf", new Vector3(-12.5f + i * 2.2f, 1.2f, 2.5f), new Vector3(1.9f, 2.4f, 0.5f), _wood, root);
                for (int shelf = 0; shelf < 3; shelf++)
                    Block("Books", new Vector3(-12.5f + i * 2.2f, 0.55f + shelf * 0.7f, 2.78f), new Vector3(1.6f, 0.35f, 0.06f),
                        Material(shelf % 2 == 0 ? "Ch6BooksA" : "Ch6BooksB", shelf % 2 == 0 ? new Color(0.55f, 0.42f, 0.3f) : new Color(0.32f, 0.25f, 0.18f)), root, false);
            }
            Block("Desk", new Vector3(-9f, 0.4f, 7.2f), new Vector3(2.4f, 0.8f, 1.1f), _red, root);
            for (int i = 0; i < 5; i++)
                Block("Paper", new Vector3(-9.8f + i * 0.4f, 0.81f, 7.1f + (i % 2) * 0.12f), new Vector3(0.3f, 0.005f, 0.4f), Material("Ch6Paper", new Color(0.92f, 0.86f, 0.7f)), root, false, i * 9f - 15f);
            Block("Inkstone", new Vector3(-8.1f, 0.83f, 7.3f), new Vector3(0.18f, 0.04f, 0.26f), Material("Ch6Ink", new Color(0.05f, 0.05f, 0.06f), 0.7f), root, false);
            Prop("Assets/_Game/Prefabs/Props/OilLamp.prefab", new Vector3(-10.4f, 0.8f, 7.4f), 0, 0.6f, root);
            Prop("Assets/_Game/Prefabs/Props/Bed.prefab", new Vector3(-13.6f, 0, 6f), 90, 0.9f, root);
            var day = new GameObject("StudyDaylight");
            day.transform.SetParent(root, false);
            day.transform.position = new Vector3(-9f, 2.6f, 9f);
            var dayLight = day.AddComponent<Light>();
            dayLight.type = LightType.Point; dayLight.color = new Color(1f, 0.9f, 0.75f); dayLight.range = 9f; dayLight.intensity = 0.9f;
            lamp = Lantern("StudyLamp", new Vector3(-9.6f, 2.2f, 7.6f), new Color(1f, 0.6f, 0.3f), root, 6f, 1.8f);
            studyPlayer = Point("StudyPlayer", new Vector3(-8.2f, 0.05f, 9.2f), 200f, root);
            studyKim = Point("StudyKim", new Vector3(-10.1f, 0f, 8.7f), 140f, root);
            nightPlayer = Point("NightPlayer", new Vector3(-8.3f, 0.05f, 8.9f), 220f, root);
            nightKim = Point("NightKim", new Vector3(-10.2f, 0f, 8.1f), 110f, root);
        }

        private static void SilverShop(Transform world)
        {
            var root = new GameObject("PhucKySilver").transform;
            root.SetParent(world, false);
            Block("ShopFloor", new Vector3(15f, 0.015f, 10f), new Vector3(8f, 0.03f, 8f), _planks, root, false);
            Block("ShopBack", new Vector3(15f, 1.7f, 6f), new Vector3(8f, 3.4f, 0.3f), _wood, root);
            Block("ShopWallW", new Vector3(11f, 1.7f, 10f), new Vector3(0.3f, 3.4f, 8f), _plaster, root);
            Block("ShopWallE", new Vector3(19f, 1.7f, 10f), new Vector3(0.3f, 3.4f, 8f), _plaster, root);
            Roof("ShopRoof", new Vector3(15f, 4.2f, 10f), 8.8f, 8.8f, 24f, _tiles, _ridge, root);
            Block("Counter", new Vector3(15f, 0.5f, 12.6f), new Vector3(5f, 1f, 0.7f), _red, root);
            Block("Scale", new Vector3(14f, 1.08f, 12.6f), new Vector3(0.5f, 0.16f, 0.3f), Material("Ch6Brass", new Color(0.75f, 0.58f, 0.25f), 0.7f), root, false);
            for (int i = 0; i < 6; i++)
                Block("Ingot", new Vector3(15.6f + (i % 3) * 0.22f, 1.04f + (i / 3) * 0.07f, 12.5f), new Vector3(0.18f, 0.07f, 0.1f), Material("Ch6Silver", new Color(0.82f, 0.82f, 0.85f), 0.8f), root, false);
            Block("ShopSign", new Vector3(15f, 3.3f, 14.1f), new Vector3(3f, 0.6f, 0.08f), Material("Ch6SignGold", new Color(0.18f, 0.12f, 0.06f), 0.4f), root, false);
            for (int i = 0; i < 3; i++)
                Block("SignGlyph", new Vector3(14f + i, 3.3f, 14.16f), new Vector3(0.4f, 0.4f, 0.01f), Emissive("CoupletGold", new Color(0.85f, 0.62f, 0.22f), 0.25f), root, false);
            Lantern("ShopLantern", new Vector3(12f, 2.8f, 13.6f), new Color(1f, 0.45f, 0.2f), root, 5f, 1f);
        }

        private static void MagistrateGate(Transform world)
        {
            var root = new GameObject("MagistrateGate").transform;
            root.SetParent(world, false);
            Block("YamenWall", new Vector3(33f, 2.2f, 18f), new Vector3(1.2f, 4.4f, 9f), _plaster, root);
            Block("YamenDoors", new Vector3(32.35f, 1.6f, 18f), new Vector3(0.12f, 3.2f, 3.2f), _red, root, false);
            foreach (float z in new[] { 16f, 20f }) Pillar("YamenPillar", new Vector3(32.2f, 0, z), 0.22f, 4f, _red, _stone, root);
            Roof("YamenRoof", new Vector3(32.6f, 4.9f, 18f), 3f, 9.4f, 26f, _tiles, _ridge, root, false);
            Block("YamenPlaque", new Vector3(32.25f, 3.7f, 18f), new Vector3(0.06f, 0.6f, 2.4f), Material("CoupletRed", new Color(0.45f, 0.05f, 0.04f), 0.55f), root, false);
            Cylinder("Drum", new Vector3(31.2f, 1.1f, 20.6f), 0.5f, 0.6f, _red, root);
            Block("ClerkDesk", new Vector3(30.4f, 0.4f, 18f), new Vector3(0.8f, 0.8f, 1.6f), _wood, root);
            Block("Ledger", new Vector3(30.4f, 0.82f, 18.2f), new Vector3(0.35f, 0.04f, 0.45f), Material("Ch6Ledger", new Color(0.3f, 0.2f, 0.12f)), root, false);
            Lantern("YamenLantern", new Vector3(31.8f, 3.3f, 15.8f), new Color(1f, 0.2f, 0.12f), root, 0f, 0f);
            Lantern("YamenLantern", new Vector3(31.8f, 3.3f, 20.2f), new Color(1f, 0.2f, 0.12f), root, 0f, 0f);
        }

        private static void TeaStall(Transform world)
        {
            var root = new GameObject("TeaStall").transform;
            root.SetParent(world, false);
            Block("TeaTable", new Vector3(-20f, 0.4f, 20.2f), new Vector3(2f, 0.8f, 0.8f), _wood, root);
            Block("TeaCanopy", new Vector3(-20f, 2.3f, 20.6f), new Vector3(3f, 0.06f, 2f), Material("Ch6Canopy", new Color(0.5f, 0.38f, 0.22f)), root, false);
            foreach (float x in new[] { -21.3f, -18.7f }) Cylinder("CanopyPole", new Vector3(x, 0, 19.7f), 0.04f, 2.3f, _wood, root, false);
            Cylinder("Stove", new Vector3(-21.6f, 0f, 21.2f), 0.3f, 0.5f, Surface("Ch2Clay", "brown_mud_02", new Color(0.82f, 0.55f, 0.42f), 0.8f, 0.1f), root);
            Sphere("Kettle", new Vector3(-21.6f, 0.7f, 21.2f), new Vector3(0.3f, 0.24f, 0.3f), Material("Ch6Kettle", new Color(0.12f, 0.1f, 0.09f), 0.6f), root);
            foreach (var p in new[] { new Vector3(-20.8f, 0, 19.2f), new Vector3(-19.2f, 0, 19.2f) })
                Cylinder("Stool", p, 0.17f, 0.4f, _wood, root, false);
            for (int i = 0; i < 4; i++) Cylinder("TeaCup", new Vector3(-20.6f + i * 0.35f, 0.8f, 20.1f), 0.05f, 0.07f, Material("Ch6Cup", new Color(0.86f, 0.84f, 0.78f), 0.5f), root, false);
        }

        private static void Crowd(Transform world, RuntimeAnimatorController shared)
        {
            string[] models = { "ThuyKieu/Models/ThuyKieu_Rigged.fbx", "MeThuyKieu/Models/MeThuyKieu_Rigged.fbx", "MaGiamSinh/Models/MaGiamSinh_Rigged.fbx" };
            string[] textures = { "ThuyKieu/Tectures", "MeThuyKieu/Tectures", "MaGiamSinh/Tectures" };
            Color[] tints = { new Color(0.85f, 0.75f, 0.6f), new Color(0.55f, 0.68f, 0.62f), new Color(0.72f, 0.58f, 0.62f), new Color(0.62f, 0.62f, 0.78f) };
            var spots = new[] { new Vector3(-26f, 0, 15.2f), new Vector3(-14f, 0, 21f), new Vector3(-2f, 0, 15.3f), new Vector3(7f, 0, 21.1f), new Vector3(21f, 0, 15.2f), new Vector3(26f, 0, 21f), new Vector3(-29f, 0, 20.8f) };
            for (int i = 0; i < spots.Length; i++)
            {
                int m = i % 3;
                RiggedActor("NguoiDan" + i, "NguoiDan", "Assets/_Game/Art/Characters/" + models[m], "Assets/_Game/Art/Characters/" + textures[m],
                    1.58f + (m == 2 ? 0.16f : 0f), spots[i], spots[i].z < 18f ? 0f : 180f, tints[i % tints.Length], shared);
            }
        }

        // ------------------------------------------------------------------ Chapter 7 placeholder

        private static void CreatePlaceholder()
        {
            if (File.Exists(PlaceholderPath)) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Main Camera", typeof(Camera)) { tag = "MainCamera" };
            camera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            camera.GetComponent<Camera>().backgroundColor = new Color(0.03f, 0.02f, 0.04f);
            var canvas = new GameObject("Canvas", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var text = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            text.transform.SetParent(canvas.transform, false);
            var rect = (RectTransform)text.transform;
            rect.anchorMin = new Vector2(0.1f, 0.2f); rect.anchorMax = new Vector2(0.9f, 0.8f); rect.offsetMin = rect.offsetMax = Vector2.zero;
            var tmp = text.GetComponent<TextMeshProUGUI>();
            tmp.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 40;
            tmp.text = "<color=#E8C27A>CHƯƠNG 7</color>\nNGHỊCH MỆNH\n\n<size=26>Chương 1–6 đã hoàn thành. Hồ sơ của bạn đã được lưu.\nChương 7 đang được phát triển.</size>";
            Save(scene, PlaceholderPath);
        }
    }
}
