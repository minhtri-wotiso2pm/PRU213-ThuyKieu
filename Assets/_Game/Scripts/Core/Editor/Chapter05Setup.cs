using System.Collections.Generic;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Chapters;
using ThuyKieu.Gameplay.Chase;
using ThuyKieu.Gameplay.Sequences;
using ThuyKieu.Gameplay.Stealth;
using TMPro;
using UnityEditor;
using UnityEngine;
using static ThuyKieu.Core.Editor.StorySceneKit;
using static ThuyKieu.Core.Editor.StoryArtKit;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Builds Chapter05_Escape: the back yard behind Tú Bà's house (guard with a lantern, chained dog, laundry lines),
    /// the back alley that forks into a dark shortcut and a lantern-lit night market, the bamboo forest
    /// run and the temple gate in the rain.
    /// Layout (metres): yard x 0–18 · alley z 2.5–5.5 to x 80 · market lane x 40–44 north to the market street
    /// z 21–29 · lane x 80–84 back south · clearing x 80–92 · forest rail x 88–148 · temple gate x 156.
    /// </summary>
    public static class Chapter05Setup
    {
        public const string SceneName = "Chapter05_Escape";
        public const string ScenePath = "Assets/_Game/Scenes/" + SceneName + ".unity";

        private static Material _plaster, _wood, _tiles, _ridge, _ground, _stone, _red, _cloth;

        [MenuItem("ThuyKieu/Chapter 5/Create playable scene")]
        public static void Create()
        {
            var scene = NewScene(ScenePath);
            var start = new Vector3(1.3f, 0.05f, 0f);
            var s = CreateSystems(5, "Đào thoát", "chapter_5", Chapter06Setup.SceneName, start, 90f, "night");
            var moon = NightLook(s.Fx, s.CameraComponent, "Ch5SkyNight", 60f, new Vector3(34, 120, 0));
            Set(s.Shots, "_keyLight", 1.6f);
            var world = new GameObject("World").transform;
            var nav = NavMesh(world);
            var director = s.Director;

            _plaster = Surface("Ch5Plaster", "plastered_wall_02", new Color(0.86f, 0.8f, 0.7f), 2.6f, 0.08f, 1.2f, 0.5f, 1f);
            _wood = Surface("Ch5Wood", "dark_wood", new Color(0.7f, 0.58f, 0.48f), 1.2f, 0.3f, 1f, 0.25f);
            _tiles = Surface("Ch5Tiles", "clay_roof_tiles_02", new Color(0.5f, 0.38f, 0.36f), 1.4f, 0.25f, 1.2f, 0.35f);
            _ridge = Material("Ch5Ridge", new Color(0.12f, 0.07f, 0.06f));
            _ground = Surface("Ch5Ground", "brown_mud_dry", new Color(0.72f, 0.64f, 0.56f), 3f, 0.12f, 1f, 0.45f);
            _stone = Surface("Ch5Stone", "japanese_stone_wall", new Color(0.7f, 0.7f, 0.68f), 1.6f, 0.15f, 1f, 0.4f, 0.5f);
            _red = Surface("Ch5Lacquer", "lacquered_cherry_wood", new Color(0.9f, 0.35f, 0.3f), 1.2f, 0.5f, 0.8f, 0.2f);
            _cloth = Material("Ch5Cloth", new Color(0.62f, 0.66f, 0.72f));

            Block("TownGround", new Vector3(46f, -0.1f, 14f), new Vector3(92f, 0.2f, 44f), _ground, world);
            var yard = BackYard(world, director, s, out var guard, out var vision, out var cone, out var dog, out var gateZone, out var gateLeaf, out var gateBlocker, out var tuBa);
            Town(world, s, out var obstacles, out var finish, out var shortcut, out var market, out var crowd);
            Forest(world, out var rail, out var rainSpot);

            // ------------------------------------------------------------ characters
            var coLam = CharacterActor("CoLam", "CoLam", "LauXanh1", 1.6f, start - new Vector3(0.9f, 0, 0.6f), 90f);
            var follower = coLam.gameObject.AddComponent<Follower>();
            Set(follower, "_player", s.Player);
            var pursuers = new List<StoryActor>();
            var pursuerStarts = new List<Object>();
            for (int i = 0; i < 3; i++)
            {
                var pursuer = CharacterActor("GiaNhan" + i, "GiaNhan", "LinhCanh",
                    1.72f + i * 0.03f, new Vector3(11.5f + i, 0, 2.5f + i * 1.6f), 90f);
                StoryPropKit.Torch(pursuer.transform, new Vector3(0.32f, 1.2f, 0.22f));
                pursuers.Add(pursuer);
                pursuerStarts.Add(Point("PursuerStart" + i, new Vector3(13f + i * 0.8f, 0, 3.2f + i * 0.9f), 90f, world));
            }

            // ------------------------------------------------------------ cameras
            Look("CAM_Wide_BackYard", new Vector3(2.4f, 2.7f, -6.2f), new Vector3(9f, 1f, 1.5f), world);
            Look("CAM_Wide_BambooEdge", new Vector3(80f, 2.6f, -0.5f), new Vector3(94f, 1.8f, 6f), world);
            Look("CAM_Hero_Kieu_Rain", rainSpot.position + new Vector3(2.1f, 1.45f, -0.9f), rainSpot.position + Vector3.up * 1.4f, world);
            Look("CAM_Wide_TempleGate", new Vector3(147.5f, 1.7f, 6.2f), new Vector3(158f, 2.3f, 7f), world);
            var chaseCam = Point("ForestChaseCamera", rail[0].position, 90f, world);
            // Review viewpoints for StoryLookCapture (not used in play).
            Look("LOOK_Alley", new Vector3(30f, 1.8f, 4f), new Vector3(42f, 1.4f, 4f), world);
            Look("LOOK_Market", new Vector3(50f, 2.4f, 25f), new Vector3(62f, 1f, 25f), world);
            Look("LOOK_Forest", new Vector3(110f, 1.6f, 7.5f), new Vector3(122f, 1.2f, 6f), world);
            Look("LOOK_YardClose", new Vector3(3f, 1.7f, -3.5f), new Vector3(8f, 1f, 1f), world);

            var zones = new GameObject("CameraZones").transform;
            zones.SetParent(world, false);
            StoryArtKit.Zone("Alley", new Vector3(49f, 1.5f, 4f), new Vector3(62f, 4f, 3f), 2.9f, 0.3f, 60f, 0.1f, zones);
            StoryArtKit.Zone("MarketLane", new Vector3(42f, 1.5f, 13f), new Vector3(4f, 4f, 16f), 3f, 0.3f, 60f, 0.1f, zones);
            StoryArtKit.Zone("EastLane", new Vector3(82f, 1.5f, 17f), new Vector3(4f, 4f, 16f), 3f, 0.3f, 60f, 0.1f, zones);

            // ------------------------------------------------------------ light & atmosphere
            var rain = Rain("Rain", s.CameraComponent.transform);

            var stamina = s.Player.gameObject.AddComponent<Stamina>();
            Set(stamina, "_player", s.Player);

            var worldScript = s.Root.AddComponent<Chapter05World>();
            Set(worldScript, "_director", director);
            Set(worldScript, "_navMesh", nav);
            Set(worldScript, "_guard", guard);
            Set(worldScript, "_visionCone", cone);
            Set(worldScript, "_tuBa", tuBa);
            Set(worldScript, "_companion", follower);
            Set(worldScript, "_pursuers", pursuers.ToArray());
            Set(worldScript, "_soKhanh", Chapter05PursuerSetup.CreateSoKhanh(worldScript.transform));
            Set(worldScript, "_gateBlocker", gateBlocker);
            Set(worldScript, "_rain", rain);
            Set(worldScript, "_rainSpot", rainSpot);
            Set(worldScript, "_moon", moon);
            Set(worldScript, "_stamina", stamina);

            // ------------------------------------------------------------ sequences
            var sequences = new GameObject("Sequences").transform;
            sequences.SetParent(s.Root.transform, false);
            var tutorial = Child<TutorialSequence>(sequences, "tutorial_escape");
            Set(tutorial, "_id", "tutorial_escape");
            Set(tutorial, "_body", "<color=#E8C27A>Sân sau:</color> người canh cầm đèn lồng đi tuần, con chó xích cạnh cửa ngõ.\n\n" +
                "• Đi trong bóng tối, sau dây phơi quần áo; <color=#E8C27A>khom người (Ctrl)</color> khi đi gần con chó — đi thẳng lưng hay chạy là nó sủa.\n" +
                "• Ở cửa ngõ, <color=#E8C27A>giữ E</color> để đẩy cửa. Thả tay trước khi vạch \"tiếng bản lề\" đầy, không thì cửa kêu.\n" +
                "• <color=#E8C27A>E</color> để nấp sau đống rơm, đống củi, chuồng gà.");
            Set(tutorial, "_controlsHint", "WASD: đi · Ctrl: khom · E (giữ): đẩy cửa · E: nấp · J: sổ tay");
            var sneak = Child<SneakOutSequence>(sequences, "sneak_out");
            Set(sneak, "_id", "sneak_out");
            Set(sneak, "_start", Point("SneakStart", start, 90f, yard));
            Set(sneak, "_guard", guard);
            Set(sneak, "_dog", dog);
            Set(sneak, "_gateZone", gateZone);
            Set(sneak, "_gateLeaf", gateLeaf);
            var chase = Child<ChaseSequence>(sequences, "chase_alley");
            Set(chase, "_id", "chase_alley");
            Set(chase, "_start", Point("ChaseStart", new Vector3(20.5f, 0.05f, 4f), 90f, world));
            Set(chase, "_pursuers", pursuers.ToArray());
            Set(chase, "_dog", dog.GetComponent<DogPursuer>());
            Set(chase, "_pursuerStarts", pursuerStarts.ToArray());
            Set(chase, "_obstacles", obstacles.ToArray());
            Set(chase, "_finish", finish);
            Set(chase, "_shortcut", shortcut);
            Set(chase, "_market", market);
            Set(chase, "_crowd", crowd);
            Set(chase, "_stamina", stamina);
            var route = new List<Object>();
            for (float x = 22f; x <= 82f; x += 4f) route.Add(Point("AutoRoute", new Vector3(x, 0.05f, 4f), 90f, world));
            Set(chase, "_autopilotRoute", route.ToArray());
            var forest = Child<ForestRunSequence>(sequences, "chase_forest");
            Set(forest, "_id", "chase_forest");
            Set(forest, "_rail", rail.ToArray());
            Set(forest, "_pursuers", pursuers.ToArray());
            Set(forest, "_dog", dog.GetComponent<DogPursuer>());
            Set(forest, "_camera", chaseCam);
            Chapter03Setup.Fx(sequences, "kieu_voice", "spirit", "memory", "none", 2f, 0f, 1);

            Save(scene, ScenePath);
        }

        private static void Look(string name, Vector3 position, Vector3 target, Transform parent)
        {
            Point(name, position, 0, parent).rotation = Quaternion.LookRotation(target - position);
        }

        // ------------------------------------------------------------------ back yard

        private static Transform BackYard(Transform world, StoryDirector director, StorySceneKit.Systems s,
            out GuardPatrol patrol, out VisionSensor vision, out Renderer cone, out DogSentry dog, out BoxCollider gateZone,
            out Transform gateLeaf, out GameObject gateBlocker, out StoryActor tuBa)
        {
            var root = new GameObject("BackYard").transform;
            root.SetParent(world, false);
            Block("YardFloor", new Vector3(9f, 0.01f, 0f), new Vector3(18f, 0.02f, 16f), Surface("Ch5Yard", "brown_mud_02", new Color(0.62f, 0.55f, 0.48f), 2.4f, 0.1f), root, false);
            // The back of Tú Bà's house with the kitchen door Kiều just came through.
            StoryPropKit.TimberFrame(Block("HouseBack", new Vector3(-0.3f, 3f, 0f), new Vector3(0.6f, 6f, 16.4f), _plaster, root), null, 2.2f);
            Block("BackDoorFrame", new Vector3(0.05f, 1.25f, 0f), new Vector3(0.1f, 2.5f, 1.5f), _wood, root, false);
            Block("BackDoorDark", new Vector3(0.02f, 1.2f, 0f), new Vector3(0.05f, 2.35f, 1.25f), Material("Ch5Dark", new Color(0.02f, 0.02f, 0.03f)), root, false);
            Roof("HouseEave", new Vector3(-1.4f, 6.4f, 0f), 3.6f, 17f, 26f, _tiles, _ridge, root, false);
            Wall(root, new Vector3(9f, 0, 8f), new Vector3(18.4f, 3f, 0.35f));
            Wall(root, new Vector3(9f, 0, -8f), new Vector3(18.4f, 3f, 0.35f));
            Wall(root, new Vector3(18f, 0, -2.4f), new Vector3(0.35f, 3f, 11.2f));
            Wall(root, new Vector3(18f, 0, 6.4f), new Vector3(0.35f, 3f, 3.2f));
            Block("GateLintel", new Vector3(18f, 2.75f, 4f), new Vector3(0.45f, 0.5f, 1.9f), _wood, root);
            Roof("GateRoof", new Vector3(18f, 3.35f, 4f), 1.6f, 2.6f, 26f, _tiles, _ridge, root, false);

            // Alley gate: a hinged leaf that swings out, and an invisible bar (not in the NavMesh) until it is open.
            var hinge = new GameObject("GateHinge").transform;
            hinge.SetParent(root, false);
            hinge.position = new Vector3(18f, 0f, 3.2f);
            Block("GateLeaf", new Vector3(18f, 1.25f, 4f), new Vector3(0.1f, 2.5f, 1.55f), Surface("Ch3GateWood", "planks_brown_10", new Color(0.55f, 0.32f, 0.22f), 1.6f, 0.25f, 1.2f), hinge, false);
            gateLeaf = hinge;
            gateBlocker = Block("GateBlocker", new Vector3(18f, 1.3f, 4f), new Vector3(0.3f, 2.6f, 1.6f), _wood, root);
            gateBlocker.layer = 0;
            gateBlocker.isStatic = false;
            Object.DestroyImmediate(gateBlocker.GetComponent<Renderer>());
            var zone = new GameObject("GateZone");
            zone.transform.SetParent(root, false);
            zone.transform.position = new Vector3(17.15f, 0.8f, 4f);
            zone.layer = 2;
            gateZone = zone.AddComponent<BoxCollider>();
            gateZone.isTrigger = true;
            gateZone.size = new Vector3(1.4f, 1.6f, 1.8f);
            Lantern("GateLantern", new Vector3(17.5f, 2.6f, 5.3f), new Color(1f, 0.45f, 0.22f), root, 5.5f, 1.4f);
            Lantern("DoorLantern", new Vector3(0.6f, 2.5f, 1.2f), new Color(1f, 0.45f, 0.22f), root, 4.5f, 1.1f);

            // Cover: laundry lines, haystack, woodpile, a chicken coop, jars and a well.
            StoryPropKit.ClothLine(new Vector3(5.2f, 0, -1.4f), new Vector3(12.8f, 0, -1.4f), root, 51);
            StoryPropKit.ClothLine(new Vector3(5.2f, 0, 2.2f), new Vector3(12.8f, 0, 2.2f), root, 52, 2); // a gap to slip through
            StoryPropKit.Haystack(new Vector3(4.8f, 0f, -6.2f), 1.1f, 1.7f, root);
            StoryPropKit.Woodpile(new Vector3(7.2f, 0f, 6.9f), 0f, 2.4f, root, 53, 5);
            Block("Coop", new Vector3(12.5f, 0.6f, -6.9f), new Vector3(2f, 1.2f, 1.4f), _wood, root);
            Roof("CoopRoof", new Vector3(12.5f, 1.45f, -6.9f), 2.3f, 1.7f, 20f, _tiles, _ridge, root);
            Coop(root, new Vector3(12.5f, 0f, -6.9f));
            YardDressing(root);
            Prop("Assets/_Game/Prefabs/Props/Well.prefab", new Vector3(9.6f, 0, -4.4f), 0, 1f, root);
            Jars(new Vector3(15.6f, 0, -6.8f), 3, root, 21);
            Jars(new Vector3(2.2f, 0, -6.8f), 2, root, 22);
            Hide(root, s.Player, "Nấp sau đống rơm", new Vector3(4.8f, 0.05f, -4.8f), 0f);
            Hide(root, s.Player, "Nấp sau đống củi", new Vector3(7.2f, 0.05f, 5.9f), 180f);
            Hide(root, s.Player, "Nấp sau chuồng gà", new Vector3(12.5f, 0.05f, -5.7f), 0f);

            // The chained dog by the gate.
            var dogRoot = new GameObject("Dog");
            dogRoot.transform.SetParent(root, false);
            dogRoot.transform.SetPositionAndRotation(new Vector3(15.4f, 0f, 6.3f), Quaternion.Euler(0, 200f, 0));
            var fur = Material("Ch5Fur", new Color(0.32f, 0.24f, 0.17f));
            Sphere("Body", dogRoot.transform.position + new Vector3(0, 0.38f, 0), new Vector3(0.38f, 0.36f, 0.75f), fur, dogRoot.transform).transform.localRotation = Quaternion.identity;
            var head = Sphere("Head", dogRoot.transform.TransformPoint(new Vector3(0, 0.58f, 0.42f)), new Vector3(0.26f, 0.24f, 0.32f), fur, dogRoot.transform);
            Sphere("Snout", dogRoot.transform.TransformPoint(new Vector3(0, 0.54f, 0.6f)), new Vector3(0.12f, 0.1f, 0.16f), Material("Ch5Snout", new Color(0.1f, 0.08f, 0.07f)), dogRoot.transform);
            foreach (var leg in new[] { new Vector3(-0.12f, 0, 0.22f), new Vector3(0.12f, 0, 0.22f), new Vector3(-0.12f, 0, -0.24f), new Vector3(0.12f, 0, -0.24f) })
                Cylinder("Leg", dogRoot.transform.TransformPoint(leg), 0.045f, 0.28f, fur, dogRoot.transform, false);
            var tail = Sphere("Tail", dogRoot.transform.TransformPoint(new Vector3(0, 0.5f, -0.42f)), new Vector3(0.06f, 0.06f, 0.26f), fur, dogRoot.transform);
            tail.transform.localRotation = Quaternion.Euler(-35f, 0, 0);
            foreach (float x in new[] { -0.08f, 0.08f })
                Sphere("Ear", dogRoot.transform.TransformPoint(new Vector3(x, 0.72f, 0.38f)), new Vector3(0.07f, 0.1f, 0.04f), fur, head.transform);
            Block("Kennel", new Vector3(16.6f, 0.5f, 7.1f), new Vector3(1.1f, 1f, 1.1f), _wood, root);
            Roof("KennelRoof", new Vector3(16.6f, 1.25f, 7.1f), 1.4f, 1.4f, 30f, _tiles, _ridge, root);
            Block("KennelDoor", new Vector3(16.04f, 0.38f, 7.1f), new Vector3(0.02f, 0.55f, 0.45f), Material("Ch5Dark", new Color(0.02f, 0.02f, 0.03f)), root, false);
            StoryArtKit.Rope(new Vector3(16.1f, 0.4f, 6.9f), new Vector3(15.5f, 0.45f, 6.4f), 0.1f, root);
            dog = dogRoot.AddComponent<DogSentry>();
            Set(dog, "_player", s.Player);
            Set(dog, "_audio", s.Audio);
            Set(dog, "_head", head.transform);
            Chapter05DogSetup.ConfigureDog(dog);

            // The guard on his round with a lantern.
            var guard = CharacterActor("NguoiCanh", "NguoiCanh", "LinhCanh", 1.76f, new Vector3(4f, 0, -4f), 90f);
            vision = guard.gameObject.AddComponent<VisionSensor>();
            var eyes = Point("Eyes", guard.transform.position + Vector3.up * 1.62f, 0, guard.transform);
            eyes.localRotation = Quaternion.identity;
            Set(vision, "_eyes", eyes);
            Set(vision, "_player", s.Player);
            Set(vision, "_range", 7.5f);
            Set(vision, "_angle", 80f);
            Set(vision, "_secondsToSpot", 1.5f);
            Set(vision, "_onlyInRestrictedZones", false);
            Set(vision, "_occluders", 1 << EnvironmentLayer);
            var route = new GameObject("GuardRoute").transform;
            route.SetParent(root, false);
            patrol = guard.gameObject.AddComponent<GuardPatrol>();
            Set(patrol, "_waypoints", new Object[]
            {
                Point("A", new Vector3(4f, 0, -4f), 0, route),
                Point("B", new Vector3(11.5f, 0, -5f), 0, route),
                Point("C", new Vector3(15f, 0, 1f), 0, route),
                Point("D", new Vector3(10.5f, 0, 5.2f), 0, route),
                Point("E", new Vector3(3.6f, 0, 4.2f), 0, route)
            });
            Set(patrol, "_vision", vision);
            Set(patrol, "_speed", 1.1f);
            Set(patrol, "_pause", 2.4f);
            cone = Chapter03Setup.VisionCone(guard.transform, eyes, 7.5f, 80f, "Ch5VisionCone");
            Lantern("GuardLantern", guard.transform.position + guard.transform.right * 0.35f + Vector3.up * 1.15f, new Color(1f, 0.55f, 0.2f), guard.transform, 4.5f, 1.3f);
            AddAlertIcon(guard.transform, vision, patrol);

            tuBa = CharacterActor("TuBa", "TuBa", "TuBa", 1.62f, new Vector3(1.1f, 0, 1.4f), 100f);
            return root;
        }

        /// <summary>Slatted front, a little door and a few hens for the chicken coop.</summary>
        private static void Coop(Transform root, Vector3 foot)
        {
            var slat = Material("Ch5CoopSlat", new Color(0.32f, 0.24f, 0.17f));
            for (float x = -0.9f; x <= 0.91f; x += 0.18f)
                Block("CoopSlat", foot + new Vector3(x, 0.6f, 0.72f), new Vector3(0.05f, 1.2f, 0.04f), slat, root, false);
            Block("CoopDoor", foot + new Vector3(0.45f, 0.35f, 0.74f), new Vector3(0.45f, 0.55f, 0.03f), _wood, root, false);
            var feathers = new[] { Material("Ch5Hen", new Color(0.55f, 0.38f, 0.22f)), Material("Ch5HenDark", new Color(0.2f, 0.16f, 0.12f)) };
            var comb = Material("Ch5Comb", new Color(0.7f, 0.1f, 0.08f));
            foreach (var (dx, dz, yaw) in new[] { (-0.5f, 1.2f, 30f), (0.3f, 1.5f, -60f), (0.9f, 1.05f, 160f) })
            {
                var hen = new GameObject("Hen").transform;
                hen.SetParent(root, false);
                hen.SetPositionAndRotation(foot + new Vector3(dx, 0, dz), Quaternion.Euler(0, yaw, 0));
                Sphere("Body", hen.TransformPoint(new Vector3(0, 0.18f, 0)), new Vector3(0.22f, 0.2f, 0.3f), feathers[(int)(dx * 10) & 1], hen);
                Sphere("Head", hen.TransformPoint(new Vector3(0, 0.33f, 0.13f)), Vector3.one * 0.1f, feathers[(int)(dx * 10) & 1], hen);
                Sphere("Comb", hen.TransformPoint(new Vector3(0, 0.39f, 0.13f)), new Vector3(0.02f, 0.05f, 0.06f), comb, hen);
                Sphere("Tail", hen.TransformPoint(new Vector3(0, 0.27f, -0.15f)), new Vector3(0.1f, 0.16f, 0.08f), feathers[1], hen);
            }
        }

        /// <summary>Weeds along the walls, puddles, sacks and baskets by the kitchen door, a stone basin by the well.</summary>
        private static void YardDressing(Transform root)
        {
            Grass("Ch5YardWeeds", root, new Rect(0.2f, -7.9f, 17.6f, 15.8f), 1800, p =>
                p.y < -7.3f || (p.y > 7.3f && p.x > 2f) || (p.x > 17.3f && (p.y < 2.9f || p.y > 5.1f)) || (p.x < 0.9f && Mathf.Abs(p.y) > 1.2f),
                505, 0.42f, new Color(0.42f, 0.55f, 0.3f), new Color(0.58f, 0.6f, 0.33f), "Ch5Weeds");
            StoryPropKit.Puddle(new Vector3(3.2f, 0.02f, 0.6f), new Vector2(1.5f, 0.9f), 15f, root);
            StoryPropKit.Puddle(new Vector3(10.4f, 0.02f, 0.4f), new Vector2(1.1f, 0.8f), -30f, root);
            StoryPropKit.Puddle(new Vector3(14.6f, 0.02f, -3.2f), new Vector2(1.4f, 1f), 50f, root);
            StoryPropKit.Sacks(new Vector3(1.4f, 0.01f, -2.8f), 3, root, 54);
            StoryPropKit.Baskets(new Vector3(1.2f, 0.01f, 3.6f), 90f, root, 55);
            StoryPropKit.Strings(new Vector3(0.25f, 2.9f, -1.9f), root, 56);
            Cylinder("WashBasin", new Vector3(8.4f, 0, -5.6f), 0.4f, 0.35f, _stone, root);
            Cylinder("BasinWater", new Vector3(8.4f, 0.33f, -5.6f), 0.34f, 0.02f, Material("PropPuddle", new Color(0.05f, 0.06f, 0.07f), 0.96f), root, false);
        }

        private static void Wall(Transform parent, Vector3 foot, Vector3 size)
        {
            Block("YardWall", foot + Vector3.up * size.y * 0.5f, size, _plaster, parent);
            Block("YardWallFooting", foot + Vector3.up * 0.22f, new Vector3(size.x + 0.08f, 0.44f, size.z + 0.08f), _stone, parent, false);
            Block("YardWallCap", foot + Vector3.up * (size.y + 0.08f), new Vector3(size.x + 0.3f, 0.16f, size.z + 0.3f), _tiles, parent, false);
        }

        // ------------------------------------------------------------------ town: alley, market, lanes

        private static void Town(Transform world, StorySceneKit.Systems s, out List<ChaseObstacle> obstacles, out BoxCollider finish,
            out BoxCollider shortcut, out BoxCollider market, out BoxCollider crowd)
        {
            var root = new GameObject("Town").transform;
            root.SetParent(world, false);
            var random = new System.Random(55);
            var street = Surface("Ch5Street", "monastery_stone_floor", new Color(0.68f, 0.66f, 0.62f), 2.4f, 0.2f, 1.1f, 0.35f);
            Block("AlleyFloor", new Vector3(49f, 0.012f, 4f), new Vector3(62f, 0.024f, 3f), street, root, false);
            Block("MarketFloor", new Vector3(62f, 0.012f, 25f), new Vector3(44f, 0.024f, 8f), street, root, false);

            // Rows of houses facing the streets (their bodies are the walls of the maze).
            Row(root, new Vector3(18.4f, 0, 2.5f), new Vector3(80f, 0, 2.5f), 0f, random, 0.25f);            // south of the alley
            Row(root, new Vector3(18.4f, 0, 5.5f), new Vector3(40f, 0, 5.5f), 180f, random, 0.2f);           // north of the alley, west part
            Row(root, new Vector3(44f, 0, 5.5f), new Vector3(80f, 0, 5.5f), 180f, random, 0.2f);             // north of the alley, east part
            Row(root, new Vector3(40f, 0, 11.5f), new Vector3(40f, 0, 21f), 90f, random, 0.3f);              // west of the market lane
            Row(root, new Vector3(44f, 0, 11.5f), new Vector3(44f, 0, 21f), 270f, random, 0.3f);             // east of the market lane
            Row(root, new Vector3(50f, 0, 21f), new Vector3(80f, 0, 21f), 0f, random, 0.5f);                 // south of the market
            Row(root, new Vector3(40f, 0, 29f), new Vector3(84f, 0, 29f), 180f, random, 0.6f);               // north of the market
            Row(root, new Vector3(84f, 0, 12f), new Vector3(84f, 0, 29f), 270f, random, 0.3f);               // east of the east lane
            Block("MarketWestEnd", new Vector3(39.6f, 1.6f, 25f), new Vector3(0.6f, 3.2f, 8f), _plaster, root);
            Block("ClearingSouth", new Vector3(86f, 1.5f, -2.3f), new Vector3(12f, 3f, 0.6f), _stone, root);
            Block("LanePocketWall", new Vector3(79.7f, 1.6f, 13.25f), new Vector3(0.6f, 3.2f, 3.5f), _plaster, root); // closes the gap between the house rows

            // Lanterns: the alley is dark, the market glows.
            for (float x = 30f; x < 80f; x += 16f) Lantern("AlleyLantern", new Vector3(x, 2.5f, 2.9f), new Color(1f, 0.4f, 0.22f), root, 4f, 0.9f);
            for (float x = 46f; x < 82f; x += 6f)
            {
                Lantern("MarketLantern", new Vector3(x, 2.9f, 21.6f), new Color(1f, 0.2f, 0.12f), root, (x % 12f) < 6f ? 7f : 0f, 1.6f);
                Lantern("MarketLantern", new Vector3(x + 3f, 2.9f, 28.4f), new Color(1f, 0.55f, 0.2f), root, (x % 12f) < 6f ? 0f : 7f, 1.6f);
            }
            Stalls(root, random);

            // Obstacles to knock over: three in the alley, four on the market route.
            obstacles = new List<ChaseObstacle>
            {
                Obstacle(root, s, "Xô đổ xe kéo", new Vector3(28f, 0, 3.0f), new Vector3(-80, 0, 0), true),
                Obstacle(root, s, "Xô đổ chồng thúng", new Vector3(52f, 0, 5.0f), new Vector3(80, 0, 0), false),
                Obstacle(root, s, "Xô đổ giá nồi niêu", new Vector3(66f, 0, 3.0f), new Vector3(-80, 0, 0), false),
                Obstacle(root, s, "Xô đổ xe hàng", new Vector3(52f, 0, 22.0f), new Vector3(-80, 0, 0), true),
                Obstacle(root, s, "Xô đổ thúng rau", new Vector3(61f, 0, 28.0f), new Vector3(80, 0, 0), false),
                Obstacle(root, s, "Xô đổ xe hàng", new Vector3(71f, 0, 22.0f), new Vector3(-80, 0, 0), true),
                Obstacle(root, s, "Xô đổ chồng thúng", new Vector3(83.2f, 0, 18f), new Vector3(0, 0, 80), false)
            };

            finish = Trigger(root, "ChaseFinish", new Vector3(83f, 1f, 4.5f), new Vector3(6f, 2f, 13f));
            shortcut = Trigger(root, "RouteShortcut", new Vector3(62f, 1f, 4f), new Vector3(34f, 2f, 3f));
            market = Trigger(root, "RouteMarket", new Vector3(62f, 1f, 25f), new Vector3(44f, 2f, 8f));
            crowd = market;

            // Night market crowd: people standing at the stalls (pursuers push through them more slowly).
            var shared = StoryAnimationSetup.Controller();
            string[] models = { "ThuyKieu/Models/ThuyKieu_Rigged.fbx", "MeThuyKieu/Models/MeThuyKieu_Rigged.fbx", "MaGiamSinh/Models/MaGiamSinh_Rigged.fbx" };
            string[] textures = { "ThuyKieu/Tectures", "MeThuyKieu/Tectures", "MaGiamSinh/Tectures" };
            Color[] tints = { new Color(0.8f, 0.7f, 0.55f), new Color(0.55f, 0.65f, 0.6f), new Color(0.7f, 0.55f, 0.6f), new Color(0.6f, 0.6f, 0.75f) };
            var spots = new[] { new Vector3(47.5f, 0, 22.9f), new Vector3(55.5f, 0, 27.1f), new Vector3(58f, 0, 22.9f), new Vector3(64.5f, 0, 27.1f), new Vector3(68f, 0, 22.9f), new Vector3(75.5f, 0, 27.1f) };
            for (int i = 0; i < spots.Length; i++)
            {
                int m = i % 3;
                RiggedActor("NguoiDan" + i, "NguoiDan", "Assets/_Game/Art/Characters/" + models[m], "Assets/_Game/Art/Characters/" + textures[m],
                    1.6f + (m == 2 ? 0.14f : 0f), spots[i], spots[i].z < 25f ? 0f : 180f, tints[i % tints.Length], shared);
            }
        }

        private static void Row(Transform parent, Vector3 from, Vector3 to, float yaw, System.Random random, float litWindows)
        {
            Vector3 along = to - from;
            float length = along.magnitude;
            along /= length;
            Quaternion facing = Quaternion.Euler(0, yaw, 0);
            Vector3 front = facing * Vector3.forward;   // the street side
            const float depth = 6f;
            for (float d = 0f; d < length - 0.5f;)
            {
                float width = Mathf.Min(length - d, 4.5f + (float)random.NextDouble() * 2.5f);
                Vector3 centre = from + along * (d + width * 0.5f) - front * depth * 0.5f;
                float height = 3f + (float)random.NextDouble() * 1.2f;
                Vector3 size = Mathf.Abs(along.x) > 0.5f ? new Vector3(width, height, depth) : new Vector3(depth, height, width);
                Block("House", centre + Vector3.up * height * 0.5f, size, random.NextDouble() < 0.5 ? _plaster : _wood, parent);
                Roof("HouseRoof", centre + Vector3.up * (height + 0.75f), Mathf.Abs(along.x) > 0.5f ? width + 0.5f : depth + 0.6f,
                    Mathf.Abs(along.x) > 0.5f ? depth + 0.6f : width + 0.5f, 24f, _tiles, _ridge, parent, Mathf.Abs(along.x) > 0.5f);
                Vector3 face = from + along * (d + width * 0.5f) + front * 0.02f;
                StoryPropKit.Facade(parent, face, front, width, height, random, random.NextDouble() < litWindows, _stone, false);
                d += width;
            }
        }

        private static void Stalls(Transform parent, System.Random random)
        {
            var canopy = new[] { Material("Ch5CanopyRed", new Color(0.55f, 0.12f, 0.1f)), Material("Ch5CanopyIndigo", new Color(0.15f, 0.18f, 0.32f)), Material("Ch5CanopyOchre", new Color(0.6f, 0.42f, 0.15f)) };
            var goods = new[] { Material("Ch5Greens", new Color(0.25f, 0.45f, 0.18f)), Material("Ch5Fruit", new Color(0.8f, 0.45f, 0.12f)), Material("Ch5Cloth2", new Color(0.7f, 0.65f, 0.5f)) };
            for (float x = 48f; x < 80f; x += 5.5f)
                foreach (int side in new[] { -1, 1 })
                {
                    float z = side < 0 ? 21.9f : 28.1f;
                    if (random.NextDouble() < 0.2) continue;
                    Block("StallTable", new Vector3(x, 0.4f, z), new Vector3(2f, 0.8f, 0.9f), _wood, parent);
                    Block("StallCanopy", new Vector3(x, 2.3f, z - side * 0.2f), new Vector3(2.4f, 0.06f, 1.5f), canopy[random.Next(canopy.Length)], parent, false);
                    foreach (float dx in new[] { -1.05f, 1.05f }) Cylinder("StallPole", new Vector3(x + dx, 0, z - side * 0.55f), 0.035f, 2.3f, _wood, parent, false);
                    Goods(parent, new Vector3(x, 0.8f, z), random, goods);
                    var hanging = Lantern("StallLantern", new Vector3(x + 0.9f, 2.25f, z - side * 0.85f), new Color(1f, 0.3f, 0.15f), parent, 0f, 0f);
                    hanging.transform.localScale = Vector3.one * 0.7f;
                }
        }

        /// <summary>Three heaps on a stall table: fruit, bundled greens, bolts of cloth or small jars.</summary>
        private static void Goods(Transform parent, Vector3 top, System.Random random, Material[] goods)
        {
            Color[] bolts = { new Color(0.6f, 0.15f, 0.12f), new Color(0.2f, 0.28f, 0.45f), new Color(0.7f, 0.6f, 0.35f) };
            for (int g = 0; g < 3; g++)
            {
                Vector3 c = top + new Vector3(-0.6f + g * 0.6f, 0f, 0f);
                switch (random.Next(4))
                {
                    case 0:
                        for (int i = 0; i < 9; i++)
                            Sphere("Fruit", c + new Vector3(((i % 3) - 1) * 0.12f, 0.08f + (i / 6) * 0.1f, ((i / 3) % 2 - 0.5f) * 0.14f), Vector3.one * 0.13f, goods[1], parent);
                        break;
                    case 1:
                        for (int i = 0; i < 3; i++)
                            Fallen(parent, c + new Vector3((i - 1) * 0.14f, 0.06f, -0.2f), c + new Vector3((i - 1) * 0.16f, 0.06f, 0.2f), 0.06f, goods[0]);
                        break;
                    case 2:
                        for (int i = 0; i < 3; i++)
                        {
                            float y = 0.07f + (i == 2 ? 0.12f : 0f), z = -0.07f + (i % 2) * 0.13f + (i == 2 ? 0.065f : 0f);
                            Fallen(parent, c + new Vector3(-0.25f, y, z), c + new Vector3(0.25f, y, z), 0.065f, Material("Ch5Bolt" + i, bolts[i]));
                        }
                        break;
                    default:
                        Sphere("Jar", c + new Vector3(-0.1f, 0.12f, 0), new Vector3(0.2f, 0.24f, 0.2f), goods[2], parent);
                        Sphere("Jar", c + new Vector3(0.12f, 0.1f, 0.05f), new Vector3(0.16f, 0.2f, 0.16f), Material("Ch5JarBlue", new Color(0.22f, 0.3f, 0.5f), 0.7f), parent);
                        break;
                }
            }
        }

        private static ChaseObstacle Obstacle(Transform parent, StorySceneKit.Systems s, string label, Vector3 foot, Vector3 fall, bool cart)
        {
            var go = new GameObject("Obstacle_" + label);
            go.transform.SetParent(parent, false);
            go.transform.position = foot;
            go.layer = 2;
            var box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(2.4f, 1.8f, 2.4f);
            box.center = Vector3.up * 0.9f;
            var visual = new GameObject("Visual").transform;
            visual.SetParent(go.transform, false);
            if (cart)
            {
                Block("CartBed", foot + new Vector3(0, 0.75f, 0), new Vector3(1.6f, 0.12f, 0.9f), _wood, visual, false);
                Block("CartLoad", foot + new Vector3(0, 1.05f, 0), new Vector3(1.4f, 0.5f, 0.75f), Material("Ch5Sacks", new Color(0.62f, 0.55f, 0.4f)), visual, false);
                foreach (float dx in new[] { -0.7f, 0.7f }) Cylinder("CartLeg", foot + new Vector3(dx, 0, 0), 0.05f, 0.75f, _wood, visual, false);
            }
            else
                for (int i = 0; i < 3; i++)
                    Cylinder("Basket", foot + new Vector3(0, i * 0.42f, 0), 0.32f - i * 0.03f, 0.42f, Surface("Ch3Basket", "planks_brown_10", new Color(0.95f, 0.8f, 0.5f), 0.35f, 0.15f, 1.4f), visual, false);
            var obstacle = go.AddComponent<ChaseObstacle>();
            Set(obstacle, "_player", s.Player);
            Set(obstacle, "_audio", s.Audio);
            Set(obstacle, "_visual", visual);
            Set(obstacle, "_fall", fall);
            Set(obstacle, "_label", label);
            return obstacle;
        }

        private static BoxCollider Trigger(Transform parent, string name, Vector3 centre, Vector3 size)
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

        // ------------------------------------------------------------------ forest & temple

        private static void Forest(Transform world, out List<Transform> rail, out Transform rainSpot)
        {
            var root = new GameObject("BambooForest").transform;
            root.SetParent(world, false);
            Block("ForestGround", new Vector3(128f, -0.1f, 6f), new Vector3(76f, 0.2f, 40f), Surface("Ch5ForestFloor", "brown_mud_02", new Color(0.5f, 0.48f, 0.4f), 2.6f, 0.15f, 1.2f, 0.5f), root);
            var points = new[] { new Vector3(88f, 0.05f, 6f), new Vector3(98f, 0.05f, 5f), new Vector3(108f, 0.05f, 7.5f), new Vector3(118f, 0.05f, 6f),
                new Vector3(128f, 0.05f, 4.5f), new Vector3(138f, 0.05f, 6f), new Vector3(147f, 0.05f, 7f) };
            rail = new List<Transform>();
            for (int i = 0; i < points.Length; i++)
            {
                Vector3 next = i + 1 < points.Length ? points[i + 1] : points[i] + Vector3.right;
                float yaw = Quaternion.LookRotation(next - points[i]).eulerAngles.y;
                rail.Add(Point("Rail" + i, points[i], yaw, root));
            }
            var path = Surface("Ch5ForestPath", "grass_path_2", new Color(0.7f, 0.66f, 0.55f), 2f, 0.15f, 1f, 0.4f);
            for (int i = 1; i < points.Length; i++)
            {
                Vector3 mid = (points[i - 1] + points[i]) * 0.5f;
                float length = Vector3.Distance(points[i - 1], points[i]);
                Block("ForestPath", new Vector3(mid.x, 0.01f, mid.z), new Vector3(2.6f, 0.02f, length + 1f), path, root, false,
                    Quaternion.LookRotation(points[i] - points[i - 1]).eulerAngles.y);
            }

            // Bamboo: a dense, uneven grove close to the path thinning into a dark wall of culms behind
            // (the models are heavy: coarse LODs, and shadows only from the clumps right by the path).
            var random = new System.Random(77);
            for (float x = 89f; x < 153f; x += 1.55f)
                foreach (int side in new[] { -1, 1 })
                {
                    float offset = 2.4f + Mathf.Pow((float)random.NextDouble(), 1.6f) * 9f;
                    Bamboo(root, new Vector3(x + (float)random.NextDouble() * 1.2f, 0, RailZ(points, x) + side * offset), random, offset > 4.5f);
                    if (x % 3.1f < 1.55f)
                        Bamboo(root, new Vector3(x + (float)random.NextDouble() * 2f, 0, RailZ(points, x) + side * (11f + (float)random.NextDouble() * 9f)), random, true, 3f);
                }
            Grass("ForestUndergrowth", root, new Rect(88f, -14f, 66f, 40f), 6500, p =>
            {
                float d = Mathf.Abs(p.y - RailZ(points, p.x));
                return d > 1.45f && d < 12f;
            }, 707, 0.55f, new Color(0.3f, 0.42f, 0.22f), new Color(0.42f, 0.48f, 0.26f), "Ch5Undergrowth");
            // Fallen culms beside the path; mist lying low between the clumps.
            var culm = Surface("Ch5Culm", "leafy_grass", new Color(0.62f, 0.62f, 0.4f), 0.5f, 0.2f, 0.8f, 0.3f);
            for (int i = 0; i < 26; i++)
            {
                float x = 90f + (float)random.NextDouble() * 60f;
                int side = random.NextDouble() < 0.5 ? -1 : 1;
                Vector3 a = new Vector3(x, 0.06f, RailZ(points, x) + side * (1.8f + (float)random.NextDouble() * 5f));
                Vector3 b = a + Quaternion.Euler(0, (float)random.NextDouble() * 360f, 0) * Vector3.forward * (1.5f + (float)random.NextDouble() * 2.5f);
                Fallen(root, a, b, 0.04f, culm);
            }
            Leaves("BambooLeaves", new Vector3(120f, 6f, 6f), new Vector3(60f, 2f, 20f), root, new Color(0.45f, 0.55f, 0.25f), 40);
            Dust("ForestMist", new Vector3(120f, 0.5f, 6f), new Vector3(64f, 0.8f, 16f), root, new Color(0.7f, 0.76f, 0.88f, 0.14f), 180);
            Fireflies("ForestFireflies", new Vector3(120f, 1.2f, 6f), new Vector3(60f, 1.4f, 14f), root, 50);

            // Where the quick-time prompts fire, the obstacle is really there (a little ahead of the prompt).
            float total = 0f;
            for (int i = 1; i < points.Length; i++) total += Vector3.Distance(points[i - 1], points[i]);
            Vector3 Along(float distance)
            {
                for (int i = 1; i < points.Length; i++)
                {
                    float segment = Vector3.Distance(points[i - 1], points[i]);
                    if (distance <= segment) return Vector3.Lerp(points[i - 1], points[i], distance / segment);
                    distance -= segment;
                }
                return points[points.Length - 1];
            }
            Vector3 jump = Along(0.16f * total + 3f), left = Along(0.34f * total + 3f), right = Along(0.58f * total + 3f), climb = Along(0.8f * total + 3f);
            Fallen(root, jump + new Vector3(0, 0.14f, -2.2f), jump + new Vector3(0.4f, 0.12f, 2.2f), 0.09f, culm);
            Rock(jump + new Vector3(0.3f, -0.05f, 1.9f), new Vector3(0.7f, 0.4f, 0.6f), 30f, root, 3);
            Fallen(root, left + new Vector3(0, 0.1f, -3.2f), left + new Vector3(0.2f, 1.75f, 0.5f), 0.07f, culm);   // leans in from the right: duck left
            Fallen(root, right + new Vector3(0, 0.1f, 3.2f), right + new Vector3(0.2f, 1.75f, -0.5f), 0.07f, culm); // from the left: dodge right
            var earth = Surface("Ch5Bank", "brown_mud_02", new Color(0.45f, 0.4f, 0.32f), 2f, 0.15f, 1.2f, 0.4f);
            foreach (int side in new[] { -1, 1 })
                Sphere("Bank", climb + new Vector3(1.5f, -0.6f, side * 2.6f), new Vector3(5f, 2.2f, 3.2f), earth, root);
            var rootWood = Surface("Ch5Root", "dark_wood", new Color(0.5f, 0.42f, 0.34f), 1f, 0.2f);
            for (int i = 0; i < 4; i++)
                Fallen(root, climb + new Vector3(i * 0.55f, 0.05f + i * 0.04f, -1.6f), climb + new Vector3(i * 0.55f + 0.2f, 0.05f + i * 0.04f, 1.6f), 0.05f, rootWood);

            // Temple gate at the end of the path, where Kiều collapses in the rain.
            var temple = new GameObject("TempleGate").transform;
            temple.SetParent(root, false);
            Block("TempleSteps", new Vector3(157f, 0.15f, 7f), new Vector3(3f, 0.3f, 5f), _stone, temple);
            Block("TempleSteps2", new Vector3(158f, 0.45f, 7f), new Vector3(2f, 0.3f, 5f), _stone, temple);
            foreach (float z in new[] { 5.2f, 8.8f }) Pillar("TemplePillar", new Vector3(159f, 0.6f, z), 0.2f, 3.4f, _red, _stone, temple);
            Roof("TempleGateRoof", new Vector3(159f, 4.4f, 7f), 2.4f, 5.6f, 26f, _tiles, _ridge, temple, false);
            Block("TempleWallL", new Vector3(159f, 1.6f, 2.2f), new Vector3(0.5f, 3.2f, 5.6f), _plaster, temple);
            Block("TempleWallR", new Vector3(159f, 1.6f, 11.8f), new Vector3(0.5f, 3.2f, 5.6f), _plaster, temple);
            Lantern("TempleLantern", new Vector3(158.2f, 3.2f, 7f), new Color(1f, 0.6f, 0.3f), temple, 8f, 1.8f);
            foreach (float z in new[] { 2.2f, 11.8f })
            {
                Block("TempleWallCap", new Vector3(159f, 3.28f, z), new Vector3(0.9f, 0.16f, 5.9f), _tiles, temple, false);
                Block("TempleWallFooting", new Vector3(159f, 0.25f, z), new Vector3(0.6f, 0.5f, 5.7f), _stone, temple, false);
            }
            foreach (float z in new[] { 4.1f, 9.9f }) StoryPropKit.StoneLantern(new Vector3(156.2f, 0f, z), 90f, temple, 4.5f, 1.1f);
            Block("TempleSign", new Vector3(158.85f, 3.75f, 7f), new Vector3(0.06f, 0.45f, 1.6f), Material("CoupletRed", new Color(0.45f, 0.05f, 0.04f), 0.55f), temple, false);
            rainSpot = Point("RainSpot", new Vector3(150f, 0.05f, 7f), 90f, root);
        }

        private static void Bamboo(Transform root, Vector3 position, System.Random random, bool noShadow, float lodBias = 2f)
        {
            var plant = Prop("Assets/_Game/Prefabs/Vegetation/Bamboo.prefab", position, (float)random.NextDouble() * 360f, 0.75f + (float)random.NextDouble() * 0.6f, root);
            if (plant == null) return;
            foreach (var t in plant.GetComponentsInChildren<Transform>()) t.gameObject.layer = 0;
            foreach (var renderer in plant.GetComponentsInChildren<Renderer>())
            {
                renderer.meshLodSelectionBias = lodBias;
                if (noShadow) renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        /// <summary>Cylinder lying between two points: fallen culms, roots, produce bundles.</summary>
        private static void Fallen(Transform root, Vector3 a, Vector3 b, float radius, Material material)
        {
            var go = Cylinder("Fallen", Vector3.zero, radius, 1f, material, root, false);
            go.transform.position = (a + b) * 0.5f;
            go.transform.rotation = Quaternion.FromToRotation(Vector3.up, (b - a).normalized);
            go.transform.localScale = new Vector3(radius * 2f, Vector3.Distance(a, b) * 0.5f, radius * 2f);
        }

        private static float RailZ(Vector3[] points, float x)
        {
            for (int i = 1; i < points.Length; i++)
                if (x <= points[i].x) return Mathf.Lerp(points[i - 1].z, points[i].z, Mathf.InverseLerp(points[i - 1].x, points[i].x, x));
            return points[points.Length - 1].z;
        }

        // ------------------------------------------------------------------ shared bits

        internal static void Hide(Transform parent, Object player, string label, Vector3 inside, float yaw)
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
            Set(spot, "_player", player);
            Set(spot, "_inside", Point("Inside", inside, yaw, go.transform));
            Set(spot, "_label", label);
            Set(spot, "_sfx", "step");
        }

        internal static void AddAlertIcon(Transform guard, VisionSensor vision, GuardPatrol patrol)
        {
            var go = new GameObject("AlertIcon");
            go.transform.SetParent(guard, false);
            go.transform.localPosition = Vector3.up * 2.25f;
            var text = go.AddComponent<TextMeshPro>();
            text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            text.fontSize = 5f;
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold;
            text.outlineWidth = 0.2f;
            text.text = string.Empty;
            ((RectTransform)go.transform).sizeDelta = new Vector2(1f, 1f);
            var icon = go.AddComponent<Gameplay.Stealth.AlertIcon>();
            Set(icon, "_vision", vision);
            Set(icon, "_patrol", patrol);
            Set(icon, "_text", text);
        }
    }
}
