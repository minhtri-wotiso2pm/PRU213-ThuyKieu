using System.Collections.Generic;
using System.Linq;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Chapters;
using ThuyKieu.Gameplay.Chase;
using ThuyKieu.Gameplay.Sequences;
using ThuyKieu.UI;
using UnityEditor;
using UnityEngine;
using static ThuyKieu.Core.Editor.StorySceneKit;
using static ThuyKieu.Core.Editor.StoryArtKit;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Builds Chapter07_HuyenDuong, the Lâm Tri yamen: the street and outer gate (south, z ≈ −14), the outer court where
    /// the crowd gathers and the witnesses wait (z −12…2), the inner gate with the complaint drum, and the hall on its
    /// stone platform (z 8…20) with the magistrate's bench, the plaque and two rows of yamen runners.
    /// </summary>
    public static class Chapter07Setup
    {
        public const string SceneName = "Chapter07_HuyenDuong";
        public const string ScenePath = "Assets/_Game/Scenes/" + SceneName + ".unity";
        private const string Characters = "Assets/_Game/Art/Characters/";
        private static Material _plaster, _wood, _tiles, _ridge, _stone, _red, _planks, _brick;

        [MenuItem("ThuyKieu/Chapter 7/Create playable scene")]
        public static void Create()
        {
            var scene = NewScene(ScenePath);
            var shared = StoryAnimationSetup.Controller();
            var spawn = new Vector3(0f, 0.05f, -9.5f);
            var s = CreateSystems(7, "Nghịch mệnh", "chapter_7", "Chapter08_Epilogue", spawn, 0f, "yamen");
            s.CameraComponent.backgroundColor = new Color(0.78f, 0.76f, 0.7f);
            Grade(s.Fx, 0.18f, 10f, 2f, 6f, new Color(0.45f, 0.46f, 0.55f), new Color(0.82f, 0.7f, 0.55f));
            UseSky(s.CameraComponent, Sky("Ch7Sky", "kloppenheim_06_puresky", 1.05f, new Color(0.64f, 0.63f, 0.6f), 210f));
            Set(s.Shots, "_keyLight", 0.7f);
            var world = new GameObject("World").transform;
            var nav = NavMesh(world);
            var director = s.Director;

            _plaster = Surface("Ch7Plaster", "plastered_wall_02", new Color(0.95f, 0.9f, 0.8f), 2.6f, 0.08f, 1.2f, 0.5f, 1f);
            _wood = Surface("Ch7Wood", "dark_wood", new Color(0.72f, 0.58f, 0.46f), 1.2f, 0.3f, 1f, 0.25f);
            _tiles = Surface("Ch7Tiles", "clay_roof_tiles_02", new Color(0.55f, 0.42f, 0.4f), 1.4f, 0.25f, 1.2f, 0.35f);
            _ridge = Material("Ch7Ridge", new Color(0.15f, 0.07f, 0.06f));
            _stone = Surface("Ch7Stone", "monastery_stone_floor", new Color(0.8f, 0.78f, 0.74f), 2.2f, 0.2f, 1.1f, 0.35f);
            _red = Surface("Ch7Lacquer", "lacquered_cherry_wood", new Color(0.92f, 0.32f, 0.26f), 1.2f, 0.5f, 0.8f, 0.2f);
            _planks = Surface("Ch7Planks", "old_wooden_floor_02", new Color(0.78f, 0.64f, 0.52f), 2f, 0.3f, 1f, 0.35f);
            _brick = Surface("Ch7Brick", "brick_floor", new Color(0.88f, 0.7f, 0.6f), 1.6f, 0.2f, 1f, 0.35f);

            Block("Ground", new Vector3(0f, -0.1f, 2f), new Vector3(44f, 0.2f, 48f), Surface("Ch7Ground", "brown_mud_dry", new Color(0.8f, 0.72f, 0.62f), 3f, 0.12f, 1f, 0.45f), world);
            Walls(world);
            OuterCourt(world, director, out var drum);
            Hall(world);
            Street(world, shared);

            // ------------------------------------------------------------ characters
            var kim = CharacterActor("KimTrong", "KimTrong", "KimTrong", 1.76f, new Vector3(-1.2f, 0f, -10.2f), 0f);
            var follower = kim.gameObject.AddComponent<Follower>();
            Set(follower, "_player", s.Player);
            Set(follower, "_distance", 1.8f);
            var tuBa = CharacterActor("TuBa", "TuBa", "TuBa", 1.62f, new Vector3(7.4f, 0f, -2.6f), 230f);
            var ma = RiggedActor("MaGiamSinh", "MaGiamSinh", Characters + "MaGiamSinh/Models/MaGiamSinh_Rigged.fbx", Characters + "MaGiamSinh/Tectures",
                1.78f, new Vector3(8.4f, 0.3f, -1.6f), 240f, Color.white, shared);
            var baNuoc = RiggedActor("BaHangNuoc", "BaHangNuoc", Characters + "MeThuyKieu/Models/MeThuyKieu_Rigged.fbx", Characters + "MeThuyKieu/Tectures",
                1.56f, new Vector3(-8.2f, 0.3f, -4.4f), 100f, new Color(0.9f, 0.75f, 0.55f), shared);
            var thuLai = RiggedActor("ThuLai", "ThuLai", Characters + "MaGiamSinh/Models/MaGiamSinh_Rigged.fbx", Characters + "MaGiamSinh/Tectures",
                1.74f, new Vector3(-5.6f, 0.5f, 15.2f), 90f, new Color(0.6f, 0.66f, 0.78f), shared);
            var chiLam = CharacterActor("ChiLam", "ChiLam", "LauXanh1", 1.6f, new Vector3(-8.4f, 0.3f, -2.9f), 110f);
            var magistrate = RiggedActor("QuanHuyen", "QuanHuyen", Characters + "MaGiamSinh/Models/MaGiamSinh_Rigged.fbx", Characters + "MaGiamSinh/Tectures",
                1.76f, new Vector3(0f, 0.8f, 17.55f), 180f, new Color(0.95f, 0.45f, 0.38f), shared);
            for (int i = 0; i < 6; i++)
            {
                int side = i % 2 == 0 ? -1 : 1;
                float z = 11f + (i / 2) * 2.2f;
                var runner = CharacterActor("NhaDich" + i, "NhaDich", "LinhCanh", 1.78f, new Vector3(side * 4.4f, 0.5f, z), side < 0 ? 90f : 270f);
                Cylinder("Stave", new Vector3(side * 4.4f + side * -0.35f, 0.5f, z + 0.25f), 0.03f, 2.1f, Material("Ch7Stave", new Color(0.45f, 0.08f, 0.05f), 0.4f), runner.transform, false);
            }
            CharacterActor("NhaDichCong", "NhaDich", "LinhCanh", 1.78f, new Vector3(5.6f, 0f, -0.2f), 200f);
            var father = CharacterActor("VuongOng", "VuongOng", "VuongOng", 1.66f, new Vector3(0f, 0f, -16f), 0f);
            var brother = RiggedActor("VuongQuan", "VuongQuan", Characters + "MaGiamSinh/Models/MaGiamSinh_Rigged.fbx", Characters + "MaGiamSinh/Tectures",
                1.48f, new Vector3(1f, 0f, -16f), 0f, new Color(0.72f, 0.86f, 0.7f), shared);
            var mother = RiggedActor("MeKieu", "MeKieu", Characters + "MeThuyKieu/Models/MeThuyKieu_Rigged.fbx", Characters + "MeThuyKieu/Tectures",
                1.56f, new Vector3(-1f, 0f, -16f), 0f, Color.white, shared);
            Crowd(world, shared);

            // The real Kiều for the last scene: Kiều's own body in translucent gold light.
            var apparitionRoot = new GameObject("KieuThatApparition").transform;
            apparitionRoot.SetParent(world, false);
            var apparition = Body(apparitionRoot, "KieuThat", Characters + "ThuyKieu/Models/ThuyKieu_Rigged.fbx", Characters + "ThuyKieu/Tectures", 1.6f, Color.white, shared);
            var ghost = GhostMaterial();
            foreach (var renderer in apparition.GetComponentsInChildren<Renderer>(true))
                renderer.sharedMaterials = Enumerable.Repeat(ghost, renderer.sharedMaterials.Length).ToArray();
            var glow = new GameObject("Glow");
            glow.transform.SetParent(apparition.transform, false);
            glow.transform.localPosition = Vector3.up * 1.2f;
            var glowLight = glow.AddComponent<Light>();
            glowLight.type = LightType.Point; glowLight.color = new Color(1f, 0.82f, 0.5f); glowLight.range = 4f; glowLight.intensity = 2.2f; glowLight.shadows = LightShadows.None;
            Dust("SpiritMotes", apparition.transform.position + Vector3.up * 1.2f, new Vector3(1.2f, 2f, 1.2f), apparition.transform, new Color(1f, 0.85f, 0.5f, 0.6f), 60);

            // ------------------------------------------------------------ explore targets
            var witnesses = new GameObject("WitnessesTarget");
            witnesses.transform.SetParent(world, false);
            witnesses.transform.position = new Vector3(-7.2f, 0f, -3.6f);
            Explore(witnesses, director, "c7_nhan_chung", "Động viên các nhân chứng", new Vector3(2.6f, 1.8f, 2.6f));
            Explore(kim.gameObject, director, "c7_kim_trong", "Bàn kế với Kim Trọng", Vector3.one * 1.8f);
            var corner = new GameObject("CornerTarget");
            corner.transform.SetParent(world, false);
            corner.transform.position = new Vector3(5.2f, 0f, -3.4f);
            Explore(corner, director, "c7_quan_sat", "Để ý Tú Bà ở góc sân", new Vector3(2f, 1.8f, 2f));
            Explore(drum, director, "c7_trong", "Đánh trống kêu oan", new Vector3(2f, 1.8f, 1.6f));

            // ------------------------------------------------------------ marks & cameras
            var marks = new GameObject("Ch7Marks").transform;
            marks.SetParent(world, false);
            var gatePlayer = Point("GatePlayer", spawn, 0f, marks);
            var exit = Point("Exit", new Vector3(0f, 0f, -17f), 180f, marks);
            var hallPlayer = Point("HallPlayer", new Vector3(0f, 0.55f, 12.2f), 0f, marks);
            var hallKim = Point("HallKim", new Vector3(-1.5f, 0.5f, 11.2f), 20f, marks);
            var hallTuBa = Point("HallTuBa", new Vector3(2.4f, 0.5f, 13.2f), 290f, marks);
            var hallMa = Point("HallMa", new Vector3(2.6f, 0.5f, 11.7f), 285f, marks);
            var hallWitnesses = new[] { Point("HallWitnessA", new Vector3(-2.5f, 0.5f, 13.4f), 75f, marks), Point("HallWitnessB", new Vector3(-2.7f, 0.5f, 12.1f), 80f, marks), Point("HallWitnessC", new Vector3(-2.6f, 0.5f, 10.8f), 70f, marks) };
            var reunionPlayer = Point("ReunionPlayer", new Vector3(0f, 0.05f, -9.6f), 180f, marks);
            var reunionKim = Point("ReunionKim", new Vector3(-1.8f, 0f, -9.2f), 160f, marks);
            var reunionFather = Point("ReunionFather", new Vector3(0.1f, 0f, -11.4f), 0f, marks);
            var reunionBrother = Point("ReunionBrother", new Vector3(1.2f, 0f, -11.2f), 340f, marks);
            var reunionMother = Point("ReunionMother", new Vector3(-0.9f, 0f, -11.3f), 20f, marks);
            Look("CAM_Wide_YamenGate", new Vector3(-4.6f, 2.6f, -10.7f), new Vector3(0.9f, 1.15f, -5.2f), world);
            Look("CAM_Witnesses", new Vector3(-5.5f, 1.65f, -3.7f), new Vector3(-8.4f, 1.35f, -3.6f), world);
            Look("CAM_Drum", new Vector3(-0.5f, 1.6f, -1.3f), new Vector3(2.6f, 1.3f, 0.9f), world);
            Look("CAM_Wide_Hall", new Vector3(1.1f, 2.7f, 9.3f), new Vector3(0f, 1.6f, 16.8f), world);
            Look("CAM_CU_QuanHuyen", new Vector3(0.42f, 2.2f, 15.25f), new Vector3(-0.05f, 1.95f, 17.55f), world);
            var spiritCamera = Point("CAM_Spirit", new Vector3(0f, 1.6f, -8f), 0f, world);
            var spiritSpot = Point("SpiritSpot", new Vector3(0f, 0f, -7.3f), 180f, marks);
            Zone("Hall", new Vector3(0f, 2f, 14f), new Vector3(14f, 5f, 12f), 3f, 0.35f, 58f, 0.4f, world);

            // ------------------------------------------------------------ light
            var sun = Sun("Sun", new Color(1f, 0.9f, 0.76f), 1.3f, new Vector3(38f, 150f, 0f));
            RenderSettings.ambientLight = new Color(0.56f, 0.55f, 0.52f);
            RenderSettings.fogColor = new Color(0.78f, 0.76f, 0.7f);
            RenderSettings.fogDensity = 0.008f;

            var worldScript = s.Root.AddComponent<Chapter07World>();
            Set(worldScript, "_director", director);
            Set(worldScript, "_navMesh", nav);
            Set(worldScript, "_kim", kim);
            Set(worldScript, "_kimFollower", follower);
            Set(worldScript, "_baNuoc", baNuoc);
            Set(worldScript, "_thuLai", thuLai);
            Set(worldScript, "_chiLam", chiLam);
            Set(worldScript, "_tuBa", tuBa);
            Set(worldScript, "_ma", ma);
            Set(worldScript, "_father", father);
            Set(worldScript, "_brother", brother);
            Set(worldScript, "_mother", mother);
            Set(worldScript, "_apparition", apparition);
            Set(worldScript, "_spiritCamera", spiritCamera);
            Set(worldScript, "_spiritSpot", spiritSpot);
            Set(worldScript, "_magistrate", magistrate);
            Set(worldScript, "_sun", sun);
            Set(worldScript, "_gatePlayer", gatePlayer);
            Set(worldScript, "_exit", exit);
            Set(worldScript, "_hallPlayer", hallPlayer);
            Set(worldScript, "_hallKim", hallKim);
            Set(worldScript, "_hallTuBa", hallTuBa);
            Set(worldScript, "_hallMa", hallMa);
            Set(worldScript, "_hallWitnesses", hallWitnesses.Cast<Object>().ToArray());
            Set(worldScript, "_reunionPlayer", reunionPlayer);
            Set(worldScript, "_reunionKim", reunionKim);
            Set(worldScript, "_reunionFather", reunionFather);
            Set(worldScript, "_reunionBrother", reunionBrother);
            Set(worldScript, "_reunionMother", reunionMother);

            // ------------------------------------------------------------ sequences
            var sequences = new GameObject("Sequences").transform;
            sequences.SetParent(s.Root.transform, false);
            Tutorial(sequences, "tutorial_gate",
                "Trước khi đánh trống, bạn có thể chuẩn bị:\n" +
                "• <color=#E8C27A>Động viên các nhân chứng</color> để không ai lùi bước khi bị dọa.\n" +
                "• <color=#E8C27A>Bàn kế với Kim Trọng</color> (nếu chàng đủ tin bạn): chàng sẽ cứu nguy một lần.\n" +
                "• <color=#E8C27A>Để ý Tú Bà</color>: có khi thấy được điều mụ muốn giấu.\n\n" +
                "Đánh trống kêu oan rồi thì không quay lại được.",
                "WASD: đi · E: tương tác · Chuột phải: quan sát · J: sổ tay");
            Tutorial(sequences, "tutorial_trial",
                "<color=#E8C27A>ĐỐI CHẤT</color>\n" +
                "• Mỗi người khai bốn câu. Chọn câu bằng <color=#E8C27A>1–4</color> hoặc chuột.\n" +
                "• <color=#E8C27A>Q — Hỏi vặn:</color> người khai nói thêm, đôi khi tự sửa lời.\n" +
                "• <color=#E8C27A>E — Đưa bằng chứng:</color> chọn giấy tờ hay nhân chứng chứng minh câu đó sai.\n" +
                "• Đúng: lòng tin của quan tăng. Sai: quan mất kiên nhẫn. Hết lòng tin thì phải làm lại vòng.\n" +
                "• Bác bỏ được <color=#E8C27A>câu then chốt</color> là qua vòng. Vạch thêm các chỗ sai khác để phán quyết trọn vẹn hơn.",
                "1–4: chọn lời khai · Q: hỏi vặn · E: đưa bằng chứng · Backspace: cất hồ sơ · Esc: tạm dừng");
            var view = s.Hud.GetComponent<TrialView>();
            for (int round = 1; round <= 3; round++)
            {
                var trial = Child<TrialSequence>(sequences, "trial_" + round);
                Set(trial, "_id", "trial_" + round);
                Set(trial, "_view", view);
                Set(trial, "_round", round);
            }
            Chapter03Setup.Fx(sequences, "kieu_voice", "spirit", "memory", "none", 1.6f, 0f, 1);
            var light = Child<LightSequence>(sequences, "light");
            Set(light, "_id", "light");

            Save(scene, ScenePath);
        }

        private static void Tutorial(Transform parent, string id, string body, string controls)
        {
            var tutorial = Child<TutorialSequence>(parent, id);
            Set(tutorial, "_id", id);
            Set(tutorial, "_body", body);
            Set(tutorial, "_controlsHint", controls);
        }

        private static void Look(string name, Vector3 position, Vector3 target, Transform parent)
        {
            Point(name, position, 0, parent).rotation = Quaternion.LookRotation(target - position);
        }

        private static Material GhostMaterial()
        {
            string path = AssetFolder + "/Ch7Ghost.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetFloat("_Surface", 1);
            material.SetFloat("_Blend", 0);
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            material.SetColor("_BaseColor", new Color(1f, 0.86f, 0.55f, 0.42f));
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(1f, 0.75f, 0.4f) * 1.4f);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        // ------------------------------------------------------------------ architecture

        private static void Wall(Transform root, string name, Vector3 centre, Vector3 size)
        {
            var wall = Block(name, centre, size, _plaster, root);
            Vector3 top = centre + Vector3.up * size.y * 0.5f;
            Block(name + "Cap", top + Vector3.up * 0.08f, new Vector3(size.x + 0.36f, 0.16f, size.z + 0.36f), _tiles, root, false);
            Block(name + "CapRidge", top + Vector3.up * 0.2f, new Vector3(size.x + 0.06f, 0.1f, size.z + 0.06f), _ridge, root, false);
            Block(name + "Footing", new Vector3(centre.x, 0.22f, centre.z), new Vector3(size.x + 0.1f, 0.44f, size.z + 0.1f), _stone, root, false);
        }

        private static void Walls(Transform world)
        {
            var root = new GameObject("YamenWalls").transform;
            root.SetParent(world, false);
            Wall(root, "WallW", new Vector3(-14f, 1.5f, 4f), new Vector3(0.5f, 3f, 32f));
            Wall(root, "WallE", new Vector3(14f, 1.5f, 4f), new Vector3(0.5f, 3f, 32f));
            Wall(root, "WallN", new Vector3(0f, 1.5f, 20.2f), new Vector3(28.5f, 3f, 0.5f));
            Wall(root, "WallSW", new Vector3(-8.2f, 1.5f, -12f), new Vector3(11.6f, 3f, 0.5f));
            Wall(root, "WallSE", new Vector3(8.2f, 1.5f, -12f), new Vector3(11.6f, 3f, 0.5f));
            // Outer gate: three bays under a tiled roof, the plaque above.
            foreach (float x in new[] { -2.4f, 2.4f }) Pillar("OuterGatePillar", new Vector3(x, 0, -12f), 0.24f, 4f, _red, _stone, root);
            Roof("OuterGateRoof", new Vector3(0f, 4.6f, -12f), 6.4f, 2.4f, 26f, _tiles, _ridge, root);
            Block("OuterPlaque", new Vector3(0f, 3.7f, -12.2f), new Vector3(2.6f, 0.6f, 0.06f), Material("Ch7Plaque", new Color(0.12f, 0.08f, 0.05f), 0.5f), root, false);
            ClassroomKit.WorldText(root, "OuterPlaqueText", "<b>HUYỆN ĐƯỜNG LÂM TRI</b>", new Vector3(0f, 3.7f, -12.24f), 0f, new Vector2(2.5f, 0.5f), 0.26f, new Color(0.9f, 0.72f, 0.3f));
            foreach (int side in new[] { -1, 1 })
                Block("OuterGateLeaf", new Vector3(side * 2.1f, 1.5f, -11.1f), new Vector3(0.1f, 3f, 1.6f), _red, root, true, side * 10f);
            StoryPropKit.StoneLantern(new Vector3(-3.4f, 0f, -13.4f), 0f, root);
            StoryPropKit.StoneLantern(new Vector3(3.4f, 0f, -13.4f), 0f, root);
        }

        /// <summary>Outer court: flagstone path, side galleries (the witnesses wait under the west one), the inner gate and the drum.</summary>
        private static void OuterCourt(Transform world, StoryDirector director, out GameObject drum)
        {
            var root = new GameObject("OuterCourt").transform;
            root.SetParent(world, false);
            Block("CourtPaving", new Vector3(0f, 0.01f, -5f), new Vector3(27f, 0.02f, 14f), _brick, root, false);
            Block("CourtPath", new Vector3(0f, 0.02f, -2f), new Vector3(3f, 0.02f, 20f), _stone, root, false);
            foreach (int side in new[] { -1, 1 })
            {
                float x = side * 10.6f;
                Block("GalleryFloor", new Vector3(x, 0.15f, -5f), new Vector3(6.2f, 0.3f, 12f), _stone, root);
                for (float z = -10f; z <= 0.1f; z += 2.5f) Pillar("GalleryPillar", new Vector3(x - side * 2.8f, 0.3f, z), 0.15f, 3f, _red, _stone, root);
                Roof("GalleryRoof", new Vector3(x, 4.1f, -5f), 6.8f, 12.6f, 22f, _tiles, _ridge, root, false);
                for (int i = 0; i < 3; i++) Cylinder("Bench", new Vector3(x - side * 1.6f, 0.3f, -8f + i * 3f), 0.2f, 0.42f, _wood, root, false);
            }
            // Inner gate (nghi môn) dividing the outer court from the hall court.
            Wall(root, "InnerWallW", new Vector3(-8.6f, 1.4f, 2f), new Vector3(10.8f, 2.8f, 0.4f));
            Wall(root, "InnerWallE", new Vector3(8.6f, 1.4f, 2f), new Vector3(10.8f, 2.8f, 0.4f));
            foreach (float x in new[] { -3.2f, -1.6f, 1.6f, 3.2f }) Pillar("InnerGatePillar", new Vector3(x, 0, 2f), 0.2f, 3.6f, _red, _stone, root);
            Roof("InnerGateRoof", new Vector3(0f, 4.3f, 2f), 7.6f, 2.6f, 26f, _tiles, _ridge, root);
            Couplets(new Vector3(-1.5f, 1.9f, 1.75f), new Vector3(1.5f, 1.9f, 1.75f), 180f, root);
            drum = StoryPropKit.DrumStand(new Vector3(2.6f, 0f, 0.9f), 90f, root, false);
            var drumTarget = new GameObject("DrumTarget");
            drumTarget.transform.SetParent(root, false);
            drumTarget.transform.position = new Vector3(2.2f, 0f, -0.4f);
            drum = drumTarget;
            StoryPropKit.NoticeBoard(new Vector3(-4.6f, 0f, 0.8f), 180f, root, 77);
            StoryPropKit.StoneLantern(new Vector3(-4.6f, 0f, -3f), 0f, root);
            StoryPropKit.StoneLantern(new Vector3(4.6f, 0f, -3f), 0f, root);
            PottedPlant(new Vector3(-3.3f, 0f, -11.3f), 1f, root, 701, true);
            PottedPlant(new Vector3(3.3f, 0f, -11.3f), 1f, root, 702, true);
            Prop("Assets/_Game/Prefabs/Vegetation/ArecaPalm.prefab", new Vector3(-6.5f, 0f, -8.6f), 20, 1f, root);
            Prop("Assets/_Game/Prefabs/Vegetation/BananaTree.prefab", new Vector3(6.8f, 0f, -9.4f), 50, 0.9f, root);
            Leaves("CourtLeaves", new Vector3(0f, 4f, -4f), new Vector3(18f, 1f, 10f), root, new Color(0.6f, 0.52f, 0.2f), 12);
        }

        /// <summary>The hall: a stone platform, red columns, the bench under the plaque, the clerk's desk, a painted screen.</summary>
        private static void Hall(Transform world)
        {
            var root = new GameObject("Hall").transform;
            root.SetParent(world, false);
            Block("HallCourt", new Vector3(0f, 0.01f, 5f), new Vector3(27f, 0.02f, 6f), _brick, root, false);
            Block("HallPlatform", new Vector3(0f, 0.25f, 14f), new Vector3(16f, 0.5f, 12.4f), _stone, root);
            for (int i = 0; i < 4; i++) Block("HallStep", new Vector3(0f, 0.06f + i * 0.12f, 7.6f + i * 0.16f), new Vector3(5f, 0.12f, 0.4f), _stone, root, false);
            Block("HallFloor", new Vector3(0f, 0.505f, 14.4f), new Vector3(15.6f, 0.01f, 11f), _planks, root, false);
            Block("HallBack", new Vector3(0f, 2.5f, 19.8f), new Vector3(16f, 4f, 0.3f), _plaster, root);
            Block("HallSideW", new Vector3(-7.9f, 2.5f, 14.6f), new Vector3(0.3f, 4f, 10.4f), _plaster, root);
            Block("HallSideE", new Vector3(7.9f, 2.5f, 14.6f), new Vector3(0.3f, 4f, 10.4f), _plaster, root);
            foreach (var wall in new[] { "HallBack", "HallSideW", "HallSideE" }) StoryPropKit.TimberFrame(root.Find(wall).gameObject, _wood, 2.2f);
            for (int i = -3; i <= 3; i++) if (i != 0) Pillar("HallColumn", new Vector3(i * 2.4f, 0.5f, 8.6f), 0.22f, 4.2f, _red, _stone, root);
            for (int i = -2; i <= 2; i += 4) Pillar("HallInnerColumn", new Vector3(i * 2.4f, 0.5f, 15f), 0.22f, 4.2f, _red, _stone, root);
            Roof("HallRoof", new Vector3(0f, 6.6f, 14f), 17.4f, 13.4f, 26f, _tiles, _ridge, root);
            Block("HallCeiling", new Vector3(0f, 4.55f, 14.2f), new Vector3(15.8f, 0.1f, 11.6f), _wood, root, false);
            // Dais, bench and plaque.
            Block("Dais", new Vector3(0f, 0.65f, 17.6f), new Vector3(5.6f, 0.3f, 2.8f), _red, root);
            Block("Bench", new Vector3(0f, 1.25f, 16.7f), new Vector3(3.2f, 0.9f, 0.9f), _red, root);
            Block("BenchCloth", new Vector3(0f, 1.2f, 16.22f), new Vector3(3.24f, 0.8f, 0.02f), Material("Ch7Cloth", new Color(0.62f, 0.06f, 0.04f), 0.2f), root, false);
            Block("BenchChair", new Vector3(0f, 1.0f, 18.1f), new Vector3(0.9f, 0.4f, 0.6f), _wood, root, false);
            Block("BenchChairBack", new Vector3(0f, 1.6f, 18.4f), new Vector3(0.9f, 1.2f, 0.08f), _wood, root, false);
            Block("SealBox", new Vector3(0.9f, 1.78f, 16.7f), new Vector3(0.3f, 0.18f, 0.3f), Material("Ch7Brass", new Color(0.78f, 0.6f, 0.25f), 0.7f), root, false);
            Block("Gavel", new Vector3(-0.4f, 1.73f, 16.6f), new Vector3(0.22f, 0.08f, 0.1f), _wood, root, false);
            Cylinder("WritTube", new Vector3(-1.1f, 1.7f, 16.7f), 0.12f, 0.36f, Material("Ch7Tube", new Color(0.2f, 0.1f, 0.06f), 0.5f), root, false);
            for (int i = 0; i < 5; i++)
                Block("Writ", new Vector3(-1.15f + (i - 2) * 0.03f, 2.08f, 16.7f), new Vector3(0.02f, 0.36f, 0.05f), Material("Ch7Writ", new Color(0.82f, 0.22f, 0.12f)), root, false, (i - 2) * 6f);
            Block("Screen", new Vector3(0f, 2.6f, 19.55f), new Vector3(6f, 3.2f, 0.1f), Material("Ch7Screen", new Color(0.78f, 0.7f, 0.52f), 0.2f), root, false);
            Block("ScreenSun", new Vector3(1.6f, 3.4f, 19.48f), new Vector3(0.8f, 0.8f, 0.02f), Material("Ch7ScreenSun", new Color(0.8f, 0.18f, 0.1f)), root, false);
            Block("ScreenWaves", new Vector3(0f, 1.6f, 19.48f), new Vector3(5.6f, 0.8f, 0.02f), Material("Ch7ScreenWaves", new Color(0.3f, 0.45f, 0.5f)), root, false);
            Block("HallPlaque", new Vector3(0f, 4.1f, 19.35f), new Vector3(4f, 0.7f, 0.08f), Material("Ch7Plaque", new Color(0.12f, 0.08f, 0.05f), 0.5f), root, false);
            ClassroomKit.WorldText(root, "HallPlaqueText", "<b>MINH KÍNH CAO HUYỀN</b>", new Vector3(0f, 4.1f, 19.3f), 0f, new Vector2(3.8f, 0.6f), 0.3f, new Color(0.92f, 0.74f, 0.32f));
            // Clerk's desk on the west side.
            Block("ClerkDesk", new Vector3(-5f, 0.9f, 15.2f), new Vector3(1.2f, 0.8f, 0.7f), _wood, root);
            Block("ClerkLedger", new Vector3(-5f, 1.32f, 15.2f), new Vector3(0.35f, 0.04f, 0.45f), Material("Ch6Ledger", new Color(0.3f, 0.2f, 0.12f)), root, false, 8f);
            // Weapon racks and banners.
            foreach (int side in new[] { -1, 1 })
            {
                Block("Rack", new Vector3(side * 6.6f, 1.4f, 18.6f), new Vector3(1.6f, 1.8f, 0.2f), _wood, root, false);
                for (int i = 0; i < 4; i++) Cylinder("RackPole", new Vector3(side * 6.6f - 0.6f + i * 0.4f, 0.5f, 18.5f), 0.025f, 2.4f, Material("Ch7Stave", new Color(0.45f, 0.08f, 0.05f), 0.4f), root, false);
                Block("Banner", new Vector3(side * 5.6f, 3.2f, 8.4f), new Vector3(0.6f, 1.6f, 0.02f), Material("Ch7Banner", new Color(0.7f, 0.08f, 0.06f), 0.2f), root, false);
            }
            foreach (float x in new[] { -6.6f, 6.6f })
            {
                var lamp = new GameObject("HallLamp");
                lamp.transform.SetParent(root, false);
                lamp.transform.position = new Vector3(x, 3.4f, 13f);
                var light = lamp.AddComponent<Light>();
                light.type = LightType.Point; light.color = new Color(1f, 0.82f, 0.6f); light.range = 10f; light.intensity = 2.2f; light.shadows = LightShadows.None;
            }
            HallLighting(root);
            HallDressing(root);
            Lantern("HallLantern", new Vector3(-2.4f, 3.9f, 9f), new Color(1f, 0.3f, 0.18f), root, 0f, 0f);
            Lantern("HallLantern", new Vector3(2.4f, 3.9f, 9f), new Color(1f, 0.3f, 0.18f), root, 0f, 0f);
        }

        /// <summary>Morning light falling in through the open front, a fill under the ceiling and a key on the bench.</summary>
        private static void HallLighting(Transform root)
        {
            Light Add(string name, Vector3 at, Vector3 aim, LightType type, Color color, float intensity, float range, float angle, LightShadows shadows)
            {
                var go = new GameObject(name);
                go.transform.SetParent(root, false);
                go.transform.position = at;
                go.transform.rotation = Quaternion.LookRotation(aim - at);
                var light = go.AddComponent<Light>();
                light.type = type; light.color = color; light.intensity = intensity; light.range = range; light.shadows = shadows;
                if (type == LightType.Spot) { light.spotAngle = angle; light.innerSpotAngle = angle * 0.55f; }
                return light;
            }
            Add("HallDaylight", new Vector3(0f, 4.2f, 6.4f), new Vector3(0f, 0.5f, 13.5f), LightType.Spot, new Color(1f, 0.9f, 0.74f), 14f, 16f, 70f, LightShadows.Soft);
            Add("HallFill", new Vector3(0f, 3.9f, 12.6f), new Vector3(0f, 0f, 12.6f), LightType.Point, new Color(1f, 0.86f, 0.7f), 1.8f, 11f, 0f, LightShadows.None);
            Add("BenchKey", new Vector3(1.6f, 4.2f, 14.2f), new Vector3(0f, 1.9f, 17.6f), LightType.Spot, new Color(1f, 0.84f, 0.62f), 9f, 8f, 38f, LightShadows.Soft);
        }

        /// <summary>The furniture of a working courtroom.</summary>
        private static void HallDressing(Transform root)
        {
            // Ceiling beams and purlins: the hall stops reading as a flat box.
            for (int i = 0; i < 5; i++)
                Block("HallBeam", new Vector3(0f, 4.38f, 9.4f + i * 2.4f), new Vector3(15.6f, 0.22f, 0.24f), _wood, root, false);
            foreach (float x in new[] { -4.8f, 0f, 4.8f })
                Block("HallPurlin", new Vector3(x, 4.28f, 14.2f), new Vector3(0.18f, 0.16f, 11.4f), _wood, root, false);
            // The screen behind the bench in a lacquered frame with a gilt edge.
            Block("ScreenFrame", new Vector3(0f, 2.6f, 19.6f), new Vector3(6.4f, 3.5f, 0.08f), _red, root, false);
            Block("ScreenGilt", new Vector3(0f, 2.6f, 19.57f), new Vector3(6.2f, 3.3f, 0.02f), Material("Ch7Gilt", new Color(0.78f, 0.58f, 0.22f), 0.7f), root, false);
            // "Silence" and "Withdraw" placards on poles either side of the dais, as in every yamen.
            StoryPropKit.Placard(new Vector3(-3.4f, 0.5f, 16.6f), 180f, root, "TÚC TĨNH");
            StoryPropKit.Placard(new Vector3(-4.1f, 0.5f, 16.9f), 180f, root, "HỒI TỴ");
            StoryPropKit.Placard(new Vector3(3.4f, 0.5f, 16.6f), 180f, root, "TÚC TĨNH");
            StoryPropKit.Placard(new Vector3(4.1f, 0.5f, 16.9f), 180f, root, "HỒI TỴ");
            // A reed mat where the plaintiff kneels, the clerk's stool, an incense burner on the bench end.
            Block("PlaintiffMat", new Vector3(0f, 0.515f, 12.4f), new Vector3(1.1f, 0.012f, 0.8f), Material("Ch7Mat", new Color(0.72f, 0.6f, 0.36f), 0.1f), root, false);
            StoryPropKit.Stool(new Vector3(-5f, 0.5f, 14.4f), 0f, root);
            StoryPropKit.Stool(new Vector3(5f, 0.5f, 14.4f), 0f, root);
            Block("Brush", new Vector3(-4.8f, 1.34f, 15.05f), new Vector3(0.02f, 0.02f, 0.2f), _wood, root, false, 20f);
            StoryPropKit.IncenseBurner(new Vector3(1.4f, 1.7f, 16.75f), root);
            for (int i = -1; i <= 1; i += 2)
            {
                Lantern("HallLantern", new Vector3(i * 4.8f, 3.85f, 12.6f), new Color(1f, 0.3f, 0.18f), root, 0f, 0f);
                Block("HallBannerSide", new Vector3(i * 7.7f, 2.6f, 12f), new Vector3(0.02f, 2.2f, 0.7f), Material("Ch7Banner", new Color(0.7f, 0.08f, 0.06f), 0.2f), root, false);
            }
        }

        /// <summary>The street outside the outer gate where the family comes back from the prison.</summary>
        private static void Street(Transform world, RuntimeAnimatorController shared)
        {
            var root = new GameObject("Street").transform;
            root.SetParent(world, false);
            Block("StreetStones", new Vector3(0f, 0.012f, -17f), new Vector3(30f, 0.024f, 9f), _stone, root, false);
            var random = new System.Random(77);
            for (float x = -15f; x < 14f; x += 5.5f)
            {
                if (Mathf.Abs(x + 2.75f) < 4f) continue;
                float height = 3.4f + (float)random.NextDouble();
                Block("House", new Vector3(x + 2.75f, height * 0.5f, -23.5f), new Vector3(5.2f, height, 5f), random.NextDouble() < 0.5 ? _plaster : _wood, root);
                Roof("HouseRoof", new Vector3(x + 2.75f, height + 0.75f, -23.5f), 5.6f, 5.6f, 24f, _tiles, _ridge, root);
                StoryPropKit.Facade(root, new Vector3(x + 2.75f, 0f, -20.98f), Vector3.forward, 5.2f, height, random, false, _stone);
            }
        }

        private static void Crowd(Transform world, RuntimeAnimatorController shared)
        {
            string[] models = { "ThuyKieu/Models/ThuyKieu_Rigged.fbx", "MeThuyKieu/Models/MeThuyKieu_Rigged.fbx", "MaGiamSinh/Models/MaGiamSinh_Rigged.fbx" };
            string[] textures = { "ThuyKieu/Tectures", "MeThuyKieu/Tectures", "MaGiamSinh/Tectures" };
            Color[] tints = { new Color(0.85f, 0.75f, 0.6f), new Color(0.55f, 0.68f, 0.62f), new Color(0.72f, 0.58f, 0.62f), new Color(0.62f, 0.62f, 0.78f) };
            var spots = new[]
            {
                new Vector3(-4.2f, 0f, -8.4f), new Vector3(-3.4f, 0f, -6.4f), new Vector3(3.6f, 0f, -7.6f), new Vector3(4.6f, 0f, -5.6f), new Vector3(-5.6f, 0f, -10.6f),
                new Vector3(5.4f, 0f, -10.2f), new Vector3(-2.6f, 0f, 4.4f), new Vector3(2.8f, 0f, 4.8f), new Vector3(-4.2f, 0f, 5.6f), new Vector3(4.4f, 0f, 5.2f),
                new Vector3(-6f, 0f, -15.4f), new Vector3(6.4f, 0f, -16f)
            };
            for (int i = 0; i < spots.Length; i++)
            {
                int m = i % 3;
                float yaw = spots[i].z > 2f ? 0f : spots[i].z < -12f ? 0f : (spots[i].x < 0 ? 60f : 300f);
                RiggedActor("NguoiDan" + i, "NguoiDan", Characters + models[m], Characters + textures[m], 1.58f + (m == 2 ? 0.16f : 0f),
                    spots[i], yaw, tints[i % tints.Length], shared);
            }
        }
    }
}
