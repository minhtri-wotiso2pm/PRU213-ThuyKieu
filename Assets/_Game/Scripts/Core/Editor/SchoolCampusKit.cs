using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using static ThuyKieu.Core.Editor.StorySceneKit;
using static ThuyKieu.Core.Editor.StoryArtKit;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// The high school around <see cref="ClassroomKit"/>'s classroom, in coordinates relative to the classroom origin
    /// (x east, z north). The classroom is the middle ground-floor room of block A: three storeys, open corridors on
    /// both long sides, a red tiled roof. East of it lies the yard with the flagpole, flame and tropical almond trees
    /// and the basketball court; block B closes the yard to the north; the bike shed and the garden lie west. A fenced
    /// boundary keeps the player inside. Beyond the fence: streets of tube houses, rice fields to the west, a hazy city
    /// and a ring of hills, so every view ends in landscape instead of an edge.
    /// Mesh assets are built relative to the origin, so the prologue and the epilogue can share them.
    /// </summary>
    public static class SchoolCampusKit
    {
        private const float FloorHeight = 3.6f;
        private const float CorridorWidth = 2.8f;
        private const float YardTop = -0.15f;
        private const float LandTop = -0.45f;
        private const float West = -24f, East = 36f, South = -30f, North = 40f;
        private const float GateHalfWidth = 3.2f;
        private const string SchoolName = "TRƯỜNG THPT NGUYỄN DU";

        private static Material _wall, _trim, _frame, _glass, _door, _roof, _corridor, _paving, _lawn, _metal, _trunk, _leaves, _soil;
        private static Mesh _unitRoof;

        /// <summary>Local frame of one school block: x across (+x is the main corridor side), z along its length.</summary>
        private readonly struct Wing
        {
            public readonly Transform Root;
            public readonly Vector3 Origin;
            public readonly float Yaw;
            public readonly float Half;

            public Wing(Transform root, Vector3 origin, float yaw, float depth)
            {
                Root = root;
                Origin = origin;
                Yaw = yaw;
                Half = depth * 0.5f;
            }

            public Vector3 At(float x, float y, float z) => Origin + Quaternion.Euler(0f, Yaw, 0f) * new Vector3(x, y, z);

            public GameObject Box(string name, float x, float y, float z, Vector3 size, Material material, bool solid = false)
                => Block(name, At(x, y, z), size, material, Root, solid, Yaw);
        }

        public static void Build(Transform parent, Vector3 o, bool sunset)
        {
            var root = new GameObject("SchoolCampus").transform;
            root.SetParent(parent, false);
            root.position = o;
            InitMaterials(sunset);

            Grounds(root, o);
            // Block A: the classroom (ground room index 2) builds itself; room 5 is the stair hall.
            var blockA = new Wing(root, o, 0f, 10.4f);
            BuildWing(blockA, "A", new[] { -23.75f, -14.2f, -4.7f, 4.7f, 14.2f, 23.75f, 28.25f }, 3, true, 2, 5);
            SchoolSign(blockA, 2);
            // Block B along the north of the yard, its corridor facing south onto it.
            var blockB = new Wing(root, o + new Vector3(23f, 0f, 34.7f), 90f, 7.4f);
            BuildWing(blockB, "B", new[] { -11f, -3.67f, 3.67f, 11f }, 2, false, -1, -1);

            Yard(root, o);
            Garden(root, o);
            Fence(root, o);
            if (sunset) Fireflies("EveningMotes", o + new Vector3(18f, 2f, 0f), new Vector3(12f, 3f, 20f), root, 25);

            var surroundings = new GameObject("SchoolSurroundings").transform;
            surroundings.SetParent(root, false);
            Streets(surroundings, o);
            Town(surroundings, o);
            Countryside(surroundings, o);
            Horizon(surroundings, o);
            // Far scenery casts no shadows: the epilogue's low sunset sun would lay the town's shadow over the classroom.
            foreach (var renderer in surroundings.GetComponentsInChildren<Renderer>(true))
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            foreach (var t in root.GetComponentsInChildren<Transform>(true)) t.gameObject.isStatic = true;
        }

        private static void InitMaterials(bool sunset)
        {
            _wall = Surface("ClassWall", "plastered_wall_02", new Color(1f, 0.9f, 0.62f), 2.2f, 0.12f, 0.6f, 0.25f);
            _trim = Surface("SchoolTrim", "plastered_wall_02", new Color(0.97f, 0.95f, 0.9f), 2.2f, 0.12f, 0.4f, 0.2f);
            _frame = Material("ClassFrame", new Color(0.24f, 0.42f, 0.36f), 0.45f);
            _glass = Material("SchoolGlass", new Color(0.16f, 0.22f, 0.26f), 0.88f);
            _door = Material("SchoolDoor", new Color(0.3f, 0.48f, 0.4f), 0.35f);
            _roof = Surface("SchoolRoof", "clay_roof_tiles_02", new Color(0.8f, 0.44f, 0.34f), 1.4f, 0.25f, 1.2f, 0.3f);
            _corridor = Surface("ClassCorridor", "brick_floor", new Color(0.92f, 0.78f, 0.66f), 1.2f, 0.25f, 0.8f, 0.3f);
            _paving = Surface("SchoolPaving", "plastered_wall_02", new Color(0.8f, 0.8f, 0.78f), 3.2f, 0.15f, 0.5f, 0.3f);
            _lawn = Surface("SchoolLawn", "leafy_grass", new Color(0.62f, 0.74f, 0.45f), 3f, 0.08f, 0.6f, 0.45f);
            _metal = Material("ClassMetal", new Color(0.18f, 0.2f, 0.22f), 0.5f);
            _trunk = Material("ClassTrunk", new Color(0.32f, 0.25f, 0.2f));
            _leaves = Material("ClassFlameLeaves", sunset ? new Color(0.3f, 0.4f, 0.2f) : new Color(0.35f, 0.55f, 0.25f), 0.1f);
            _soil = Material("SchoolSoil", new Color(0.28f, 0.2f, 0.15f));
        }

        // ------------------------------------------------------------------ blocks

        /// <summary>
        /// A school block: room masses with doors and windows on their corridor sides, open corridors with columns,
        /// upper walkways with parapets and a gabled tile roof. <paramref name="rooms"/> are the room boundaries along z.
        /// </summary>
        private static void BuildWing(Wing w, string id, float[] rooms, int floors, bool backCorridor, int skipGround, int stairRoom)
        {
            float z0 = rooms[0], z1 = rooms[rooms.Length - 1];
            float length = z1 - z0, zc = (z0 + z1) * 0.5f;
            float top = floors * FloorHeight;
            float outer = w.Half + CorridorWidth;

            for (int i = 0; i < rooms.Length - 1; i++)
            {
                if (i == skipGround) continue;
                w.Box("GroundRoom", 0f, FloorHeight * 0.5f, (rooms[i] + rooms[i + 1]) * 0.5f, new Vector3(w.Half * 2f, FloorHeight, rooms[i + 1] - rooms[i]), _wall, true);
            }
            w.Box("UpperFloors", 0f, (FloorHeight + top) * 0.5f, zc, new Vector3(w.Half * 2f, top - FloorHeight, length), _wall);

            foreach (int side in backCorridor ? new[] { 1, -1 } : new[] { 1 })
            {
                w.Box("CorridorFloor", side * (w.Half + CorridorWidth * 0.5f), -0.1f, zc, new Vector3(CorridorWidth, 0.2f, length + 0.6f), _corridor, true);
                // A pale kerb where the corridor steps down to the yard.
                w.Box("CorridorKerb", side * (outer - 0.08f), -0.08f, zc, new Vector3(0.16f, 0.2f, length + 0.6f), _trim);
                for (int f = 1; f <= floors; f++)
                {
                    w.Box("CorridorSlab", side * (w.Half + CorridorWidth * 0.5f + 0.05f), f * FloorHeight - 0.1f, zc, new Vector3(CorridorWidth + 0.1f, 0.2f, length + 0.6f), _trim);
                    if (f == floors) continue;
                    w.Box("Parapet", side * (outer - 0.08f), f * FloorHeight + 0.5f, zc, new Vector3(0.14f, 1f, length + 0.6f), _wall);
                    w.Box("ParapetCap", side * (outer - 0.08f), f * FloorHeight + 1.04f, zc, new Vector3(0.24f, 0.08f, length + 0.6f), _frame);
                }
                int bays = Mathf.Max(1, Mathf.RoundToInt(length / 4.75f));
                for (int k = 0; k <= bays; k++)
                    w.Box("Column", side * (outer - 0.18f), top * 0.5f, z0 + length * k / bays, new Vector3(0.36f, top, 0.36f), _wall, true);
            }

            // Facades: doors and windows onto the corridors (the blind back of block B gets windows only).
            for (int i = 0; i < rooms.Length - 1; i++)
                for (int f = 0; f < floors; f++)
                    foreach (int side in new[] { 1, -1 })
                    {
                        if (f == 0 && i == skipGround) continue;
                        bool corridor = side > 0 || backCorridor;
                        if (i == stairRoom) StairFace(w, side, f * FloorHeight, rooms[i], rooms[i + 1], f == 0);
                        else RoomFace(w, side, f * FloorHeight, rooms[i], rooms[i + 1], corridor);
                    }
            foreach (int end in new[] { -1, 1 })
                for (int f = 0; f < floors; f++)
                    foreach (float x in new[] { -w.Half * 0.45f, w.Half * 0.45f })
                        EndWindow(w, x, f * FloorHeight, end > 0 ? z1 : z0, end);

            var roof = UnitRoof(id + "Roof", w.At(0f, top, zc), new Vector3(outer + 0.7f, (outer + 0.7f) * 0.42f, length + 1f), w.Yaw, w.Root);
            roof.GetComponent<MeshRenderer>().sharedMaterials = new[] { _roof, _wall };
            foreach (int side in new[] { -1, 1 })
                w.Box("Fascia", side * (outer + 0.62f), top + 0.02f, zc, new Vector3(0.1f, 0.28f, length + 1f), _trim);
        }

        private static void RoomFace(Wing w, int side, float y0, float a, float b, bool door)
        {
            if (door)
            {
                float dz = a + 1.25f;
                w.Box("DoorFrame", side * (w.Half + 0.02f), y0 + 1.15f, dz, new Vector3(0.04f, 2.3f, 1.25f), _trim);
                w.Box("Door", side * (w.Half + 0.045f), y0 + 1.07f, dz, new Vector3(0.04f, 2.12f, 1.05f), _door);
            }
            float start = door ? a + 2.4f : a + 0.6f, end = b - 0.6f;
            int count = door ? 2 : 3;
            for (int k = 0; k < count; k++)
                Window(w, side, y0, start + (end - start) * (k + 0.5f) / count);
        }

        private static void Window(Wing w, int side, float y0, float z)
        {
            w.Box("WindowFrame", side * (w.Half + 0.02f), y0 + 1.7f, z, new Vector3(0.04f, 1.6f, 1.75f), _frame);
            w.Box("WindowGlass", side * (w.Half + 0.04f), y0 + 1.7f, z, new Vector3(0.04f, 1.42f, 1.57f), _glass);
            w.Box("WindowMullion", side * (w.Half + 0.065f), y0 + 1.7f, z, new Vector3(0.03f, 1.42f, 0.05f), _frame);
            w.Box("WindowSill", side * (w.Half + 0.07f), y0 + 0.87f, z, new Vector3(0.14f, 0.06f, 1.85f), _trim);
        }

        /// <summary>The stair hall: an open doorway below, windows climbing half a storey per floor.</summary>
        private static void StairFace(Wing w, int side, float y0, float a, float b, bool ground)
        {
            float zc = (a + b) * 0.5f;
            if (ground) w.Box("StairDoorway", side * (w.Half + 0.02f), y0 + 1.25f, zc, new Vector3(0.04f, 2.5f, 1.8f), _glass);
            w.Box("StairWindowFrame", side * (w.Half + 0.02f), y0 + 2.6f, zc + 0.6f, new Vector3(0.04f, 1.1f, 1.3f), _frame);
            w.Box("StairWindow", side * (w.Half + 0.04f), y0 + 2.6f, zc + 0.6f, new Vector3(0.04f, 0.95f, 1.15f), _glass);
        }

        private static void EndWindow(Wing w, float x, float y0, float z, int end)
        {
            w.Box("EndWindowFrame", x, y0 + 1.75f, z + end * 0.02f, new Vector3(1.5f, 1.4f, 0.04f), _frame);
            w.Box("EndWindow", x, y0 + 1.75f, z + end * 0.04f, new Vector3(1.34f, 1.24f, 0.04f), _glass);
            w.Box("EndWindowSill", x, y0 + 1.02f, z + end * 0.07f, new Vector3(1.6f, 0.06f, 0.14f), _trim);
        }

        /// <summary>The school's name in yellow on a red board along the top walkway, read from the yard.</summary>
        private static void SchoolSign(Wing w, int floor)
        {
            float x = w.Half + CorridorWidth + 0.02f;
            float y = floor * FloorHeight + 0.5f;
            w.Box("SchoolNameBoard", x, y, 0f, new Vector3(0.05f, 0.82f, 9.6f), Material("ClassBanner", new Color(0.72f, 0.1f, 0.08f), 0.3f));
            var text = ClassroomKit.WorldText(w.Root, "SchoolNameText", "<b>" + SchoolName + "</b>", w.At(x + 0.04f, y, 0f), w.Yaw + 270f,
                new Vector2(9.3f, 0.75f), 0.55f, new Color(1f, 0.86f, 0.3f));
            text.textWrappingMode = TextWrappingModes.NoWrap;
        }

        // ------------------------------------------------------------------ grounds

        private static void Grounds(Transform root, Vector3 o)
        {
            // One solid paved base over the whole campus; lawns and court markings lie on it as thin skins.
            Block("CampusGround", o + new Vector3((West + East) * 0.5f, YardTop - 0.1f, (South + North) * 0.5f),
                new Vector3(East - West, 0.2f, North - South), _paving, root);
            Block("GardenLawn", o + new Vector3((West + -8.1f) * 0.5f, YardTop + 0.005f, (South + North) * 0.5f),
                new Vector3(-8.1f - West - 0.4f, 0.01f, North - South - 0.4f), _lawn, root, false);
            Block("NorthLawn", o + new Vector3(2f, YardTop + 0.005f, 34.4f), new Vector3(20f, 0.01f, 10.8f), _lawn, root, false);
            var grass = new GameObject("CampusGrass").transform;
            grass.SetParent(root, false);
            grass.position = o + Vector3.up * (YardTop + 0.01f);
            Grass("SchoolGardenGrass", grass, new Rect(West + 0.4f, South + 0.4f, -8.5f - West, North - South - 0.8f), 1800,
                p => p.x < -20f || p.x > -12.5f || p.y > 8f || p.y < -22f, 811, 0.32f, new Color(0.5f, 0.66f, 0.32f), new Color(0.62f, 0.66f, 0.34f), "SchoolGrass");
        }

        private static void Yard(Transform root, Vector3 o)
        {
            Vector3 Y(float x, float z) => o + new Vector3(x, YardTop, z);
            // Flagpole on a stepped plinth, in line with the classroom windows and the gate.
            var stone = _trim;
            Block("FlagPlinth", Y(23f, 0f) + Vector3.up * 0.1f, new Vector3(2.6f, 0.2f, 2.6f), stone, root);
            Block("FlagPlinth", Y(23f, 0f) + Vector3.up * 0.3f, new Vector3(1.9f, 0.2f, 1.9f), stone, root);
            Block("FlagPlinth", Y(23f, 0f) + Vector3.up * 0.5f, new Vector3(1.2f, 0.2f, 1.2f), stone, root);
            Cylinder("FlagPole", Y(23f, 0f) + Vector3.up * 0.6f, 0.06f, 10.4f, Material("SchoolPole", new Color(0.92f, 0.92f, 0.9f), 0.7f), root, false);
            var flagRed = Material("ClassFlagRed", new Color(0.78f, 0.06f, 0.05f), 0.2f);
            var starYellow = Material("ClassFlagStar", new Color(1f, 0.85f, 0.1f), 0.2f);
            Vector3 flag = Y(23f, -1.1f) + Vector3.up * 10.2f;
            Block("YardFlag", flag, new Vector3(0.03f, 1.4f, 2.1f), flagRed, root, false);
            ClassroomKit.Star(root, flag + new Vector3(-0.02f, 0f, 0f), 0.42f, starYellow, "YardFlagStarW").transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            ClassroomKit.Star(root, flag + new Vector3(0.02f, 0f, 0f), 0.42f, starYellow, "YardFlagStarE").transform.rotation = Quaternion.Euler(0f, 270f, 0f);

            // Shade trees in round brick planters whose rims are the pupils' benches.
            Tree(root, Y(14f, -11f), 4.4f, 2.6f, false, 31);
            Tree(root, Y(14f, 12f), 4.2f, 2.4f, false, 32);
            Tree(root, Y(30.5f, -10f), 4f, 2.6f, true, 33);
            Tree(root, Y(30.5f, 13f), 4.2f, 2.8f, true, 34);
            Tree(root, Y(19f, 22.5f), 3.8f, 2.2f, false, 35);
            foreach (var (x, z, yaw) in new[] { (17.5f, -6f, 0f), (17.5f, 6f, 0f), (27f, -5.5f, 90f), (27f, 5.5f, 90f) })
                Bench(root, Y(x, z), yaw);

            // Basketball court on the south side of the yard.
            var line = Material("SchoolCourtLine", new Color(0.95f, 0.95f, 0.92f), 0.1f);
            Vector3 court = Y(24f, -23f) + Vector3.up * 0.012f;
            Block("CourtLine", court + new Vector3(0f, 0f, -5f), new Vector3(16f, 0.01f, 0.08f), line, root, false);
            Block("CourtLine", court + new Vector3(0f, 0f, 5f), new Vector3(16f, 0.01f, 0.08f), line, root, false);
            Block("CourtLine", court + new Vector3(-8f, 0f, 0f), new Vector3(0.08f, 0.01f, 10f), line, root, false);
            Block("CourtLine", court + new Vector3(8f, 0f, 0f), new Vector3(0.08f, 0.01f, 10f), line, root, false);
            Block("CourtLine", court, new Vector3(0.08f, 0.01f, 10f), line, root, false);
            foreach (int side in new[] { -1, 1 })
            {
                Vector3 foot = Y(24f + side * 8.6f, -23f);
                Cylinder("HoopPole", foot, 0.08f, 3.3f, _metal, root);
                Block("HoopBoard", foot + new Vector3(-side * 0.25f, 3.3f, 0f), new Vector3(0.05f, 1.05f, 1.8f), Material("SchoolBoard", new Color(0.95f, 0.95f, 0.95f), 0.4f), root, false);
                Cylinder("HoopRim", foot + new Vector3(-side * 0.5f, 3.05f, 0f), 0.23f, 0.03f, Material("SchoolRim", new Color(0.9f, 0.35f, 0.1f), 0.5f), root, false);
            }
            // Bins by the corridor, as every Vietnamese school yard has.
            var bin = Material("ClassBin", new Color(0.2f, 0.45f, 0.3f), 0.4f);
            foreach (float z in new[] { -16f, 9f, 20f }) Cylinder("YardBin", Y(8.8f, z), 0.25f, 0.7f, bin, root);
        }

        private static void Tree(Transform root, Vector3 foot, float trunkHeight, float crown, bool layered, int seed)
        {
            var random = new System.Random(seed);
            Cylinder("Planter", foot, 1.45f, 0.48f, _trim, root);
            Cylinder("PlanterSoil", foot, 1.3f, 0.5f, _soil, root, false);
            Cylinder("TreeTrunk", foot + Vector3.up * 0.4f, 0.26f, trunkHeight, _trunk, root, false);
            Vector3 top = foot + Vector3.up * (trunkHeight + 0.4f);
            if (layered)
            {
                // Tropical almond (bàng): flat tiers of leaves, wide at the bottom.
                for (int tier = 0; tier < 3; tier++)
                {
                    float r = crown * (1.25f - tier * 0.3f);
                    Sphere("BangTier", top + Vector3.up * (tier * 1.1f - 0.6f), new Vector3(r * 2f, 0.7f, r * 2f), _leaves, root);
                    Cylinder("BangTrunk", top + Vector3.up * (tier * 1.1f - 1.2f), 0.16f, 1.2f, _trunk, root, false);
                }
                return;
            }
            // Flame tree (phượng): a broad umbrella of soft clumps.
            Sphere("Crown", top + Vector3.up * 0.5f, new Vector3(crown * 1.6f, crown * 0.7f, crown * 1.6f), _leaves, root);
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI * 0.25f + (float)random.NextDouble() * 0.4f;
                float reach = crown * (0.75f + (float)random.NextDouble() * 0.35f);
                var p = top + new Vector3(Mathf.Cos(angle) * reach, (float)random.NextDouble() * 0.8f, Mathf.Sin(angle) * reach);
                float s = crown * (0.75f + (float)random.NextDouble() * 0.35f);
                Sphere("Crown", p, new Vector3(s * 1.3f, s * 0.6f, s * 1.3f), _leaves, root);
            }
        }

        private static void Bench(Transform root, Vector3 foot, float yaw)
        {
            var rot = Quaternion.Euler(0f, yaw, 0f);
            Block("BenchSeat", foot + Vector3.up * 0.44f, new Vector3(1.8f, 0.08f, 0.45f), _trim, root, true, yaw);
            foreach (int side in new[] { -1, 1 })
                Block("BenchLeg", foot + rot * new Vector3(side * 0.7f, 0.2f, 0f), new Vector3(0.12f, 0.4f, 0.4f), _trim, root, false, yaw);
        }

        /// <summary>West of block A: the bike shed (bikes in rows under a tin roof), a lawn, palms and bamboo along the fence.</summary>
        private static void Garden(Transform root, Vector3 o)
        {
            var tin = Material("SchoolTin", new Color(0.36f, 0.55f, 0.62f), 0.55f);
            float x0 = -19.5f, x1 = -13.5f, z0 = -21f, z1 = 7f;
            Vector3 c = o + new Vector3((x0 + x1) * 0.5f, 0f, (z0 + z1) * 0.5f);
            Block("BikeShedFloor", c + Vector3.up * (YardTop + 0.03f), new Vector3(x1 - x0, 0.06f, z1 - z0), _paving, root);
            for (float z = z0 + 0.2f; z <= z1; z += 3.5f)
            {
                Cylinder("ShedPost", o + new Vector3(x0 + 0.2f, YardTop, z), 0.06f, 2.3f, _metal, root);
                Cylinder("ShedPost", o + new Vector3(x1 - 0.2f, YardTop, z), 0.06f, 2.7f, _metal, root);
            }
            var shedRoof = Block("ShedRoof", c + Vector3.up * 2.6f, new Vector3(x1 - x0 + 0.6f, 0.05f, z1 - z0 + 0.6f), tin, root, false);
            shedRoof.transform.rotation = Quaternion.Euler(0f, 0f, 4f);
            var random = new System.Random(901);
            var paints = new[] { Material("BikeRed", new Color(0.7f, 0.12f, 0.1f), 0.5f), Material("BikeBlue", new Color(0.15f, 0.3f, 0.6f), 0.5f),
                Material("BikePink", new Color(0.9f, 0.5f, 0.6f), 0.5f), Material("BikeBlack", new Color(0.1f, 0.1f, 0.1f), 0.5f) };
            for (float z = z0 + 0.8f; z < z1 - 0.5f; z += 0.75f)
            {
                if (random.NextDouble() < 0.25) continue;
                float bx = (x0 + x1) * 0.5f + (float)random.NextDouble() * 0.3f;
                foreach (float wx in new[] { -0.52f, 0.52f })
                {
                    var wheel = Cylinder("BikeWheel", o + new Vector3(bx + wx, 0f, z), 0.32f, 0.04f, _metal, root, false);
                    wheel.transform.SetPositionAndRotation(o + new Vector3(bx + wx, YardTop + 0.35f, z), Quaternion.Euler(90f, 0f, 0f));
                }
                var paint = paints[random.Next(paints.Length)];
                Block("BikeFrame", o + new Vector3(bx, YardTop + 0.62f, z), new Vector3(1f, 0.05f, 0.05f), paint, root, false);
                Block("BikeSeat", o + new Vector3(bx - 0.3f, YardTop + 0.9f, z), new Vector3(0.24f, 0.05f, 0.1f), _metal, root, false);
                Block("BikeBars", o + new Vector3(bx + 0.45f, YardTop + 1f, z), new Vector3(0.04f, 0.04f, 0.5f), _metal, root, false);
            }
            // Palms and bamboo along the west fence, a banana clump by the north lawn.
            foreach (var (z, yaw) in new[] { (-26f, 10f), (-10f, 120f), (14f, 200f), (24f, 60f), (34f, 300f) })
                Prop("Assets/_Game/Prefabs/Vegetation/ArecaPalm.prefab", o + new Vector3(-21.5f, YardTop, z), yaw, 1.1f, root);
            foreach (float z in new[] { -18f, -2f, 18f, 30f })
                Prop("Assets/_Game/Prefabs/Vegetation/Bamboo.prefab", o + new Vector3(-22.5f, YardTop, z), z * 7f, 1.2f, root);
            Prop("Assets/_Game/Prefabs/Vegetation/BananaTree.prefab", o + new Vector3(-4f, YardTop, 36f), 30f, 1.1f, root);
            Prop("Assets/_Game/Prefabs/Vegetation/BananaTree.prefab", o + new Vector3(9f, YardTop, 37f), 140f, 1f, root);
            Tree(root, o + new Vector3(-11.5f, YardTop, 16f), 3.8f, 2.2f, true, 36);
            Tree(root, o + new Vector3(-11.5f, YardTop, -26f), 3.6f, 2f, false, 37);
        }

        /// <summary>The boundary: a low plastered wall with piers and green railings, the gate and the guard's lodge in the east.</summary>
        private static void Fence(Transform root, Vector3 o)
        {
            var bars = new List<Matrix4x4>();
            FenceRun(root, o, new Vector3(West, 0f, South), new Vector3(East, 0f, South), bars);
            FenceRun(root, o, new Vector3(West, 0f, North), new Vector3(East, 0f, North), bars);
            FenceRun(root, o, new Vector3(West, 0f, South), new Vector3(West, 0f, North), bars);
            FenceRun(root, o, new Vector3(East, 0f, South), new Vector3(East, 0f, -GateHalfWidth - 0.35f), bars);
            FenceRun(root, o, new Vector3(East, 0f, GateHalfWidth + 0.35f), new Vector3(East, 0f, North), bars);

            // Gate: two piers under a sign beam, the sliding gate closed.
            foreach (int side in new[] { -1, 1 })
                Block("GatePier", o + new Vector3(East, 1.5f, side * (GateHalfWidth + 0.35f)), new Vector3(0.75f, 3.6f, 0.75f), _trim, root);
            Block("GateBeam", o + new Vector3(East, 3.65f, 0f), new Vector3(0.6f, 0.75f, GateHalfWidth * 2f + 1.5f), _trim, root, false);
            var red = Material("ClassBanner", new Color(0.72f, 0.1f, 0.08f), 0.3f);
            foreach (int side in new[] { -1, 1 })
            {
                Block("GateSign", o + new Vector3(East + side * 0.32f, 3.65f, 0f), new Vector3(0.04f, 0.6f, GateHalfWidth * 2f + 1.2f), red, root, false);
                var text = ClassroomKit.WorldText(root, "GateSignText", "<b>" + SchoolName + "</b>", o + new Vector3(East + side * 0.35f, 3.65f, 0f),
                    side > 0 ? 270f : 90f, new Vector2(GateHalfWidth * 2f + 1f, 0.55f), 0.4f, new Color(1f, 0.86f, 0.3f));
                text.textWrappingMode = TextWrappingModes.NoWrap;
            }
            for (float z = -GateHalfWidth + 0.1f; z < GateHalfWidth; z += 0.16f)
                bars.Add(Matrix4x4.TRS(new Vector3(East, YardTop + 1f, z), Quaternion.identity, new Vector3(0.03f, 1.9f, 0.03f)));
            foreach (float y in new[] { 0.15f, 1.0f, 1.9f })
                Block("GateRail", o + new Vector3(East, YardTop + y, 0f), new Vector3(0.06f, 0.06f, GateHalfWidth * 2f), _frame, root, false);
            Block("GateCollider", o + new Vector3(East, 1.2f, 0f), new Vector3(0.3f, 2.8f, GateHalfWidth * 2f), _trim, root).GetComponent<Renderer>().enabled = false;

            // The guard's lodge just inside the gate.
            Vector3 lodge = o + new Vector3(33.2f, YardTop, -7f);
            Block("GuardLodge", lodge + Vector3.up * 1.4f, new Vector3(3f, 2.8f, 3.2f), _wall, root);
            Block("GuardLodgeRoof", lodge + Vector3.up * 2.9f, new Vector3(3.6f, 0.2f, 3.8f), _trim, root, false);
            Block("GuardLodgeWindow", lodge + new Vector3(-1.52f, 1.5f, 0.4f), new Vector3(0.04f, 1f, 1.4f), _glass, root, false);
            Block("GuardLodgeDoor", lodge + new Vector3(-0.3f, 1.05f, 1.62f), new Vector3(0.9f, 2.1f, 0.04f), _door, root, false);

            var barMesh = CombinedCubes("SchoolFenceBars", bars);
            var go = new GameObject("FenceBars");
            go.transform.SetParent(root, false);
            go.transform.position = o;
            go.AddComponent<MeshFilter>().sharedMesh = barMesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = _frame;
        }

        /// <summary>One straight stretch of fence between two origin-relative points (axis aligned).</summary>
        private static void FenceRun(Transform root, Vector3 o, Vector3 a, Vector3 b, List<Matrix4x4> bars)
        {
            Vector3 mid = (a + b) * 0.5f;
            float length = Vector3.Distance(a, b);
            bool alongX = Mathf.Abs(b.x - a.x) > Mathf.Abs(b.z - a.z);
            Vector3 Size(float along, float y, float across) => alongX ? new Vector3(along, y, across) : new Vector3(across, y, along);
            // The base reaches down to the street level outside.
            Block("FenceBase", o + mid + Vector3.up * ((LandTop + YardTop + 0.65f) * 0.5f), Size(length, YardTop + 0.65f - LandTop, 0.26f), _wall, root);
            Block("FenceBaseCap", o + mid + Vector3.up * (YardTop + 0.68f), Size(length, 0.06f, 0.34f), _trim, root, false);
            foreach (float y in new[] { 1.15f, 2.05f })
                Block("FenceRail", o + mid + Vector3.up * (YardTop + y), Size(length, 0.05f, 0.05f), _frame, root, false);
            Block("FenceCollider", o + mid + Vector3.up * 1.2f, Size(length, 2.8f, 0.3f), _trim, root).GetComponent<Renderer>().enabled = false;
            Vector3 dir = (b - a).normalized;
            int piers = Mathf.Max(1, Mathf.RoundToInt(length / 3.2f));
            for (int i = 0; i <= piers; i++)
                Block("FencePier", o + a + dir * (length * i / piers) + Vector3.up * ((LandTop + YardTop + 2.3f) * 0.5f),
                    new Vector3(0.38f, YardTop + 2.3f - LandTop, 0.38f), _wall, root, false);
            for (float d = 0.1f; d < length; d += 0.18f)
                bars.Add(Matrix4x4.TRS(a + dir * d + Vector3.up * (YardTop + 1.38f), Quaternion.identity, new Vector3(0.025f, 1.4f, 0.025f)));
        }

        // ------------------------------------------------------------------ outside the fence

        private static void Streets(Transform root, Vector3 o)
        {
            var land = Surface("SchoolLand", "leafy_grass", new Color(0.5f, 0.62f, 0.38f), 6f, 0.05f, 0.5f, 0.6f);
            var asphalt = Material("SchoolAsphalt", new Color(0.2f, 0.2f, 0.21f), 0.25f);
            var pavement = Surface("SchoolPavement", "monastery_stone_floor", new Color(0.72f, 0.68f, 0.64f), 1.6f, 0.15f, 0.7f, 0.4f);
            var paint = Material("SchoolRoadPaint", new Color(0.92f, 0.92f, 0.88f), 0.2f);
            Block("LandGround", o + new Vector3(6f, LandTop - 0.1f, 5f), new Vector3(560f, 0.2f, 560f), land, root, false);
            // East street past the gate, south street, a lane to the north; a dirt track along the west fence.
            Road(root, o + new Vector3(43.5f, 0f, 5f), new Vector3(8f, 0f, 200f), asphalt, pavement, paint, false);
            Road(root, o + new Vector3(5f, 0f, -37.5f), new Vector3(200f, 0f, 8f), asphalt, pavement, paint, true);
            Road(root, o + new Vector3(30f, 0f, 45.5f), new Vector3(140f, 0f, 6f), asphalt, pavement, paint, true);
            Block("DirtTrack", o + new Vector3(West - 2.6f, LandTop - 0.045f, 5f), new Vector3(4.4f, 0.1f, 200f),
                Surface("SchoolTrack", "brown_mud_dry", new Color(0.78f, 0.68f, 0.56f), 3f, 0.1f, 0.8f, 0.4f), root, false);
            // Concrete power poles with sagging-free cables along the east street.
            var concrete = Material("SchoolPoleConcrete", new Color(0.62f, 0.6f, 0.57f), 0.1f);
            var cable = Material("SchoolCable", new Color(0.08f, 0.08f, 0.08f), 0.2f);
            for (float z = -85f; z <= 95f; z += 30f)
            {
                Cylinder("PowerPole", o + new Vector3(48.6f, -0.3f, z), 0.13f, 8.5f, concrete, root, false);
                Block("PowerArm", o + new Vector3(48.6f, 7.9f, z), new Vector3(1.4f, 0.1f, 0.1f), concrete, root, false);
                if (z + 30f <= 95f)
                    foreach (float x in new[] { -0.6f, 0f, 0.6f })
                        Block("PowerCable", o + new Vector3(48.6f + x, 7.98f, z + 15f), new Vector3(0.02f, 0.02f, 30f), cable, root, false);
            }
        }

        private static void Road(Transform root, Vector3 centre, Vector3 size, Material asphalt, Material pavement, Material paint, bool alongX)
        {
            float width = alongX ? size.z : size.x, length = alongX ? size.x : size.z;
            Vector3 Size(float along, float y, float across) => alongX ? new Vector3(along, y, across) : new Vector3(across, y, along);
            Vector3 Across(float d) => alongX ? new Vector3(0f, 0f, d) : new Vector3(d, 0f, 0f);
            Block("Road", centre + Vector3.up * (LandTop + 0.03f), Size(length, 0.1f, width), asphalt, root, false);
            foreach (int side in new[] { -1, 1 })
                Block("Pavement", centre + Across(side * (width * 0.5f + 1.4f)) + Vector3.up * (LandTop + 0.1f), Size(length, 0.2f, 2.8f), pavement, root, false);
            for (float d = -length * 0.5f + 2f; d < length * 0.5f; d += 6f)
                Block("RoadDash", centre + (alongX ? new Vector3(d, 0f, 0f) : new Vector3(0f, 0f, d)) + Vector3.up * (LandTop + 0.085f), Size(2.6f, 0.01f, 0.14f), paint, root, false);
        }

        private static readonly Color[] HousePaints =
        {
            new Color(1f, 0.86f, 0.55f), new Color(0.62f, 0.82f, 0.7f), new Color(0.95f, 0.72f, 0.7f), new Color(0.93f, 0.92f, 0.88f),
            new Color(0.62f, 0.76f, 0.88f), new Color(0.95f, 0.68f, 0.42f), new Color(0.78f, 0.86f, 0.5f)
        };

        private static readonly Color[] SignColors = { new Color(0.8f, 0.15f, 0.12f), new Color(0.15f, 0.35f, 0.7f), new Color(0.95f, 0.75f, 0.15f) };

        /// <summary>Rows of narrow tube houses (nhà ống) facing the streets, a second row of taller ones behind.</summary>
        private static void Town(Transform root, Vector3 o)
        {
            var random = new System.Random(4242);
            // East street, west-facing fronts; gaps where the south street and the north lane meet it.
            HouseRow(root, o, random, new Vector3(50.5f, 0f, -85f), Vector3.forward, 175f, 270f, z => z > -45f && z < -30f || z > 41f && z < 51f);
            HouseRow(root, o, random, new Vector3(-30f, 0f, -44.5f), Vector3.right, 70f, 0f, null);
            HouseRow(root, o, random, new Vector3(-30f, 0f, 51f), Vector3.right, 70f, 180f, null);
            HouseRow(root, o, random, new Vector3(52f, 0f, 51f), Vector3.right, 50f, 180f, null);
            // Taller blocks behind the street fronts, simple masses for depth.
            for (int i = 0; i < 26; i++)
            {
                bool east = i < 14;
                Vector3 p = east ? new Vector3(70f + (float)random.NextDouble() * 12f, 0f, -80f + i * 12.5f)
                                 : new Vector3(-30f + (i - 14) * 9f, 0f, -62f - (float)random.NextDouble() * 10f);
                float h = 9f + (float)random.NextDouble() * 12f;
                var paint = Material("TownPaint" + random.Next(HousePaints.Length), HousePaints[random.Next(HousePaints.Length)], 0.1f);
                Block("TownBlock", o + p + Vector3.up * (LandTop + h * 0.5f), new Vector3(7f + (float)random.NextDouble() * 5f, h, 8f + (float)random.NextDouble() * 5f), paint, root, false);
            }
            // Street trees on the far pavements.
            for (float z = -80f; z <= 90f; z += 11f)
                if (!(z > -45f && z < -30f) && !(z > 41f && z < 51f)) StreetTree(root, o + new Vector3(48.4f, LandTop + 0.2f, z), (int)z);
            for (float x = -26f; x <= 38f; x += 11f)
            {
                StreetTree(root, o + new Vector3(x, LandTop + 0.2f, -42.9f), (int)x + 500);
                StreetTree(root, o + new Vector3(x, LandTop + 0.2f, 49.6f), (int)x + 900);
            }
        }

        private static void HouseRow(Transform root, Vector3 o, System.Random random, Vector3 start, Vector3 along, float length, float yaw, System.Func<float, bool> skip)
        {
            for (float d = 0f; d < length;)
            {
                float width = 3.8f + (float)random.NextDouble() * 1.8f;
                Vector3 p = start + along * (d + width * 0.5f);
                d += width;
                if (skip != null && skip(along.z != 0f ? p.z : p.x)) continue;
                House(root, o + p + Vector3.up * LandTop, yaw, width, 2 + random.Next(4), random);
            }
        }

        private static void House(Transform root, Vector3 front, float yaw, float width, int floors, System.Random random)
        {
            var rot = Quaternion.Euler(0f, yaw, 0f);
            Vector3 P(float x, float y, float z) => front + rot * new Vector3(x, y, z);
            int paintIndex = random.Next(HousePaints.Length);
            var paint = Material("TownPaint" + paintIndex, HousePaints[paintIndex], 0.1f);
            var trim = Material("TownTrim", new Color(0.92f, 0.92f, 0.9f), 0.15f);
            var dark = Material("TownOpening", new Color(0.12f, 0.13f, 0.15f), 0.7f);
            var shutter = Material("TownShutter", new Color(0.55f, 0.56f, 0.58f), 0.4f);
            float height = floors * 3.3f + 0.3f;
            // Local +z faces the street.
            Block("TownHouse", P(0f, height * 0.5f, -6f), new Vector3(width - 0.08f, height, 12f), paint, root, false, yaw);
            Block("Shopfront", P(0f, 1.4f, 0.03f), new Vector3(width - 0.8f, 2.8f, 0.06f), random.NextDouble() < 0.5 ? shutter : dark, root, false, yaw);
            if (random.NextDouble() < 0.6)
            {
                int sign = random.Next(SignColors.Length);
                Block("ShopSign", P(0f, 3.05f, 0.1f), new Vector3(width - 0.5f, 0.55f, 0.08f), Material("TownSign" + sign, SignColors[sign], 0.3f), root, false, yaw);
            }
            for (int f = 1; f < floors; f++)
            {
                float y = f * 3.3f;
                Block("Balcony", P(0f, y, 0.45f), new Vector3(width - 0.3f, 0.16f, 0.9f), trim, root, false, yaw);
                Block("BalconyRail", P(0f, y + 0.5f, 0.86f), new Vector3(width - 0.3f, 0.85f, 0.05f), random.NextDouble() < 0.5 ? trim : _frame, root, false, yaw);
                Block("FacadeOpening", P(0f, y + 1.5f, 0.03f), new Vector3(width * 0.62f, 2f, 0.06f), dark, root, false, yaw);
            }
            if (random.NextDouble() < 0.4)
            {
                UnitRoof("TownRoof", P(0f, height, -6f), new Vector3(width * 0.5f + 0.15f, 1.3f, 12.2f), yaw, root).GetComponent<MeshRenderer>().sharedMaterials = new[] { _roof, paint };
                return;
            }
            Block("RoofParapet", P(0f, height + 0.4f, -0.1f), new Vector3(width - 0.08f, 0.8f, 0.2f), paint, root, false, yaw);
            // The stainless water tank on the roof.
            if (random.NextDouble() < 0.6)
                Cylinder("WaterTank", P(0f, height, -4f), 0.45f, 1.1f, Material("TownTank", new Color(0.8f, 0.82f, 0.84f), 0.85f), root, false);
        }

        private static void StreetTree(Transform root, Vector3 foot, int seed)
        {
            var random = new System.Random(seed);
            float h = 3f + (float)random.NextDouble();
            Cylinder("StreetTreeTrunk", foot, 0.16f, h, _trunk, root, false);
            float s = 2.2f + (float)random.NextDouble();
            Sphere("StreetTreeCrown", foot + Vector3.up * (h + s * 0.25f), new Vector3(s * 1.4f, s, s * 1.4f), _leaves, root);
        }

        /// <summary>West of the school: rice paddies cut by dikes, bamboo clumps and a few tiled village houses.</summary>
        private static void Countryside(Transform root, Vector3 o)
        {
            var paddy = Surface("SchoolPaddy", "leafy_grass", new Color(0.6f, 0.78f, 0.36f), 2.5f, 0.1f, 0.7f, 0.5f);
            var dike = Surface("SchoolDike", "brown_mud_dry", new Color(0.66f, 0.58f, 0.44f), 2f, 0.1f, 0.8f, 0.3f);
            float x0 = -150f, x1 = West - 5f, z0 = -110f, z1 = 120f;
            Block("Paddies", o + new Vector3((x0 + x1) * 0.5f, LandTop + 0.01f, (z0 + z1) * 0.5f), new Vector3(x1 - x0, 0.02f, z1 - z0), paddy, root, false);
            for (float x = x1 - 14f; x > x0; x -= 14f)
                Block("Dike", o + new Vector3(x, LandTop + 0.06f, (z0 + z1) * 0.5f), new Vector3(0.7f, 0.12f, z1 - z0), dike, root, false);
            for (float z = z0 + 12f; z < z1; z += 16f)
                Block("Dike", o + new Vector3((x0 + x1) * 0.5f, LandTop + 0.06f, z), new Vector3(x1 - x0, 0.12f, 0.7f), dike, root, false);
            var random = new System.Random(77);
            var plaster = Surface("SchoolVillageWall", "plastered_wall_02", new Color(0.94f, 0.9f, 0.8f), 2.4f, 0.1f, 0.8f, 0.4f);
            foreach (var (x, z, yaw) in new[] { (-62f, -40f, 10f), (-75f, 22f, 95f), (-58f, 62f, -15f), (-95f, -12f, 80f), (-110f, 48f, 5f), (-88f, -70f, 30f) })
            {
                Vector3 p = o + new Vector3(x, LandTop, z);
                Block("VillageHouse", p + Vector3.up * 1.6f, new Vector3(8f, 3.2f, 5f), plaster, root, false, yaw);
                UnitRoof("VillageRoof", p + Vector3.up * 3.2f, new Vector3(3.2f, 1.9f, 9f), yaw + 90f, root).GetComponent<MeshRenderer>().sharedMaterials = new[] { _roof, plaster };
                for (int i = 0; i < 3; i++)
                    Prop("Assets/_Game/Prefabs/Vegetation/Bamboo.prefab", p + new Vector3((float)random.NextDouble() * 14f - 7f, 0f, 6f + (float)random.NextDouble() * 4f), random.Next(360), 1.3f, root);
                Prop("Assets/_Game/Prefabs/Vegetation/ArecaPalm.prefab", p + new Vector3(-6f, 0f, -4f), random.Next(360), 1.2f, root);
            }
        }

        /// <summary>The far distance: a hazy city to the east and south, a ring of hills all round.</summary>
        private static void Horizon(Transform root, Vector3 o)
        {
            var random = new System.Random(1990);
            var tower = new[] { Material("CityTowerA", new Color(0.82f, 0.82f, 0.8f), 0.4f), Material("CityTowerB", new Color(0.7f, 0.74f, 0.78f), 0.5f),
                Material("CityTowerC", new Color(0.86f, 0.8f, 0.7f), 0.3f) };
            for (int i = 0; i < 22; i++)
            {
                float angle = Mathf.Lerp(-100f, 40f, i / 21f) * Mathf.Deg2Rad + (float)random.NextDouble() * 0.08f;
                float radius = 120f + (float)random.NextDouble() * 45f;
                float h = 18f + (float)random.NextDouble() * 38f;
                var p = o + new Vector3(Mathf.Cos(angle) * radius, LandTop + h * 0.5f, Mathf.Sin(angle) * radius);
                Block("CityTower", p, new Vector3(10f + (float)random.NextDouble() * 10f, h, 10f + (float)random.NextDouble() * 10f), tower[random.Next(tower.Length)], root, false, random.Next(90));
            }
            var hill = Surface("SchoolHill", "leafy_grass", new Color(0.4f, 0.5f, 0.42f), 30f, 0.05f, 0.4f, 0.6f);
            for (int i = 0; i < 16; i++)
            {
                float angle = i * Mathf.PI * 2f / 16f + (float)random.NextDouble() * 0.2f;
                float radius = 195f + (float)random.NextDouble() * 25f;
                float sx = 70f + (float)random.NextDouble() * 50f, sy = 26f + (float)random.NextDouble() * 22f;
                Sphere("Hill", o + new Vector3(Mathf.Cos(angle) * radius, LandTop - sy * 0.3f, Mathf.Sin(angle) * radius), new Vector3(sx, sy, sx * 0.8f), hill, root);
            }
        }

        // ------------------------------------------------------------------ meshes

        /// <summary>
        /// A gabled roof scaled from a unit mesh: slopes from x ±1 at y 0 to the ridge at y 1, z −0.5…0.5, both sides
        /// visible; submesh 0 the tiles, submesh 1 the gable ends.
        /// </summary>
        private static GameObject UnitRoof(string name, Vector3 baseCentre, Vector3 scale, float yaw, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(baseCentre, Quaternion.Euler(0f, yaw, 0f));
            go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = RoofMesh();
            go.AddComponent<MeshRenderer>().sharedMaterials = new[] { _roof, _wall };
            return go;
        }

        private static Mesh RoofMesh()
        {
            string path = AssetFolder + "/SchoolUnitRoof.asset";
            if (_unitRoof != null && AssetDatabase.GetAssetPath(_unitRoof) == path) return _unitRoof;
            _unitRoof = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (_unitRoof != null) return _unitRoof;
            var vertices = new List<Vector3>();
            var tiles = new List<int>();
            var gables = new List<int>();
            void Face(List<int> triangles, params Vector3[] corners)
            {
                // Each face twice, once per winding, so slopes and gables read from inside and below as well.
                for (int pass = 0; pass < 2; pass++)
                {
                    int b = vertices.Count;
                    vertices.AddRange(corners);
                    for (int i = 1; i < corners.Length - 1; i++)
                        if (pass == 0) triangles.AddRange(new[] { b, b + i, b + i + 1 });
                        else triangles.AddRange(new[] { b, b + i + 1, b + i });
                }
            }
            Face(tiles, new Vector3(-1f, 0f, -0.5f), new Vector3(0f, 1f, -0.5f), new Vector3(0f, 1f, 0.5f), new Vector3(-1f, 0f, 0.5f));
            Face(tiles, new Vector3(1f, 0f, -0.5f), new Vector3(1f, 0f, 0.5f), new Vector3(0f, 1f, 0.5f), new Vector3(0f, 1f, -0.5f));
            Face(gables, new Vector3(-1f, 0f, -0.48f), new Vector3(1f, 0f, -0.48f), new Vector3(0f, 1f, -0.48f));
            Face(gables, new Vector3(-1f, 0f, 0.48f), new Vector3(0f, 1f, 0.48f), new Vector3(1f, 0f, 0.48f));
            // Separate vertices per face give flat normals; the reversed copy gets the opposite normal.
            var mesh = new Mesh { name = "SchoolUnitRoof" };
            mesh.SetVertices(vertices);
            mesh.subMeshCount = 2;
            mesh.SetTriangles(tiles, 0);
            mesh.SetTriangles(gables, 1);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, path);
            _unitRoof = mesh;
            return mesh;
        }

        /// <summary>Many thin boxes (railing bars) merged into one mesh asset, in coordinates relative to the campus origin.</summary>
        private static Mesh CombinedCubes(string name, List<Matrix4x4> boxes)
        {
            var temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var cube = temp.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(temp);
            var combine = new CombineInstance[boxes.Count];
            for (int i = 0; i < boxes.Count; i++) combine[i] = new CombineInstance { mesh = cube, transform = boxes[i] };
            var mesh = new Mesh { name = name, indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            mesh.CombineMeshes(combine, true, true);
            mesh.RecalculateBounds();
            string path = AssetFolder + "/" + name + ".asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }
    }
}
