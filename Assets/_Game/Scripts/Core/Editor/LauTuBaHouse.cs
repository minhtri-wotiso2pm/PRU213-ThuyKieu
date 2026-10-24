using System.Collections.Generic;
using ThuyKieu.Environment;
using ThuyKieu.Gameplay.House;
using ThuyKieu.Gameplay.Stealth;
using ThuyKieu.Player;
using ThuyKieu.Presentation;
using UnityEngine;
using static ThuyKieu.Core.Editor.LauTuBaKit;
using static ThuyKieu.Core.Editor.StorySceneKit;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Tú Bà's house for the first-person Chapters 3–4 (plan: docs/concept/lau-tu-ba-expanded-layout.md, 42 × 38 m).
    /// One storey around an 18 × 14 m rain-wet courtyard: 3 m roofed verandas west, east and south, a slatted screen on
    /// the north side with the 3 m service corridor behind it, the west wing (Kiều, chị Lam), the east wing (Hồng, the
    /// guests' room), the hall and gate to the south, and the rear range (kitchen, cloth store, Tú Bà, the ledger office
    /// with its archive) opening onto the corridor. A back alley runs along the west wall.
    /// Unity axes: x west→east (−21…21), z south→north (−19…19); plan y (north→south) = 19 − z.
    /// Every door stands in a real opening; raised floors are reached by steps.
    /// </summary>
    public static partial class LauTuBaHouse
    {
        /// <summary>The lines the house is laid out on (metres, Unity axes).</summary>
        public static class Plan
        {
            public const float West = -21f, East = 21f, North = 19f, South = -19f;
            /// <summary>Rear range front wall (corridor | rooms) and the courtyard screen (yard | corridor).</summary>
            public const float RearFront = 11f, Screen = 8f;
            /// <summary>Veranda faces of the wings, and the courtyard edges.</summary>
            public const float WingFace = 12f, YardEdge = 9f, YardSouth = -6f;
            /// <summary>The hall's north edge (rooms' south walls, front pillars).</summary>
            public const float HallFront = -9f;
            /// <summary>Partition between the two rooms of each wing.</summary>
            public const float WingSplit = -0.5f;
            /// <summary>Partitions of the rear range: kitchen | store | Tú Bà | office | archive.</summary>
            public const float KitchenStore = -11f, StoreTuBa = -3f, TuBaOffice = 8f, OfficeArchive = 15f;
            /// <summary>The alley outside the west wall.</summary>
            public const float AlleyWest = -24.6f, AlleyEast = -21.15f;
        }

        public sealed class Refs
        {
            public HouseDoor GateLeft, GateRight, KieuDoor, LamDoor, HongDoor, GuestDoor, BackDoor, StoreDoor, TuBaDoor, InnerDoor, OfficeDoor;
            public Transform KieuInside, LamInside, HongInside, OfficeInside, TuBaInside, KitchenInside;
            public Transform SapFeet, TuBaDoorInside, TuBaDoorOutside, KieuDoorOutside, LamDoorOutside, HallSpot, GateInside, OfficeDoorOutside;
            public Transform MirrorFace, MirrorCloth, Cabinet, Ledger, LacquerBox, BackDoorFront, Stove, StoreSlats;
            public LampFlame OfficeLamp, ArchiveLamp, KieuLamp, TuBaLamp, LamLamp;
            public PickupItem Hairpin;
            public GameObject KeyHook;
            public readonly List<HideSpot> Hides = new List<HideSpot>();
            public readonly List<Light> CorridorLanterns = new List<Light>();
            public readonly List<Light> YardLanterns = new List<Light>();
            /// <summary>Soft lights standing in for bounced light (daylight in Chapter 3, moon spill in Chapter 4).</summary>
            public readonly List<Light> Fills = new List<Light>();
            public Transform Root;
        }

        private static Refs _refs;
        private static bool _night;
        private static StoryAudio _audio;
        private static KieuInventory _inventory;
        private static PlayerMovement _player;

        public static Refs Build(Transform world, bool night, StoryAudio audio, PlayerMovement player)
        {
            _refs = new Refs();
            _night = night;
            _audio = audio;
            _player = player;
            _inventory = player.GetComponent<KieuInventory>();
            if (_inventory == null) _inventory = player.gameObject.AddComponent<KieuInventory>();
            InitMaterials(night);
            LauTuBaMeshKit.Begin();
            DecorMaterials();
            var root = new GameObject("LauTuBa").transform;
            root.SetParent(world, false);
            _refs.Root = root;
            Floors(root);
            OuterWalls(root);
            WestWing(root);
            EastWing(root);
            RearRange(root);
            CourtyardEdges(root);
            UpperStorey(root);
            Roofs(root);
            Rooms(root);
            KeepWoodUnbatched(root);
            return _refs;
        }

        // ------------------------------------------------------------------ floors

        private static void Floors(Transform root)
        {
            var floors = Group(root, "Floors");
            // The courtyard, flagstones wet from the rain, and the alley outside the west wall.
            FloorSlab("Courtyard", -9f, -6f, 9f, 8f, 0f, 0.3f, WetStone, Surface.WetStone, floors);
            // The alley is beaten mud after the rain, not flagstones: it sounds and carries like mud.
            FloorSlab("Alley", Plan.AlleyWest, -19.4f, Plan.AlleyEast, 19.4f, 0f, 0.3f, Mud, Surface.Mud, floors);
            // Outside the gate: a stone apron down to the lane (seen when the gate stands open in Chapter 3).
            FloorSlab("GateApron", -3.2f, -22f, 3.2f, -19f, 0f, 0.3f, Stone, Surface.Stone, floors);
            // Raised wooden floors: verandas, corridor, rooms. Brick floor in the hall (gạch Bát Tràng).
            FloorSlab("VerandaWest", -12f, -9f, -9f, 8f, Floor, 0.5f, FloorBoards, Surface.Wood, floors);
            FloorSlab("VerandaEast", 9f, -9f, 12f, 8f, Floor, 0.5f, FloorBoards, Surface.Wood, floors);
            FloorSlab("VerandaSouth", -9f, -9f, 9f, -6f, Floor, 0.5f, FloorBoards, Surface.Wood, floors);
            FloorSlab("Corridor", -21f, 8f, 21f, 11f, Floor, 0.5f, FloorBoards, Surface.Wood, floors);
            FloorSlab("KieuRoom", -21f, -0.5f, -12f, 8f, Floor, 0.5f, FloorBoards, Surface.Wood, floors);
            FloorSlab("LamRoom", -21f, -9f, -12f, -0.5f, Floor, 0.5f, FloorBoards, Surface.Wood, floors);
            FloorSlab("HongRoom", 12f, -0.5f, 21f, 8f, Floor, 0.5f, FloorBoards, Surface.Wood, floors);
            FloorSlab("GuestRoom", 12f, -9f, 21f, -0.5f, Floor, 0.5f, FloorBoards, Surface.Mat, floors);
            FloorSlab("Hall", -21f, -19f, 21f, -9f, Floor, 0.5f, Brick, Surface.Stone, floors);
            FloorSlab("Store", -11f, 11f, -3f, 19f, Floor, 0.5f, FloorBoards, Surface.Wood, floors);
            FloorSlab("TuBaRoom", -3f, 11f, 8f, 19f, Floor, 0.5f, FloorBoards, Surface.Wood, floors);
            FloorSlab("Office", 8f, 11f, 21f, 19f, Floor, 0.5f, FloorBoards, Surface.Wood, floors);
            // The kitchen is a step down: beaten earth.
            FloorSlab("Kitchen", -21f, 11f, -11f, 19f, 0.12f, 0.4f, Earth, Surface.Earth, floors);
            // Stone kerb where the raised floors meet the yard.
            Box("KerbWest", new Vector3(-9.12f, 0f, -6f), new Vector3(-8.98f, Floor + 0.02f, 8f), PlinthYard, floors, false);
            Box("KerbEast", new Vector3(8.98f, 0f, -6f), new Vector3(9.12f, Floor + 0.02f, 8f), PlinthYard, floors, false);
            Box("KerbSouth", new Vector3(-9.12f, 0f, -6.12f), new Vector3(9.12f, Floor + 0.02f, -5.98f), PlinthYard, floors, false);
            Box("KerbNorth", new Vector3(-9f, 0f, 7.98f), new Vector3(9f, Floor + 0.02f, 8.12f), PlinthYard, floors, false);
            // Steps up from the yard: middle of the west, east and south sides, and through the gap in the north screen.
            Steps("StepsWest", new Vector3(-9f, 0f, 1f), Vector3.left, 1.4f, Floor, floors);
            Steps("StepsEast", new Vector3(9f, 0f, 1f), Vector3.right, 1.4f, Floor, floors);
            Steps("StepsSouth", new Vector3(0f, 0f, -6f), Vector3.back, 1.6f, Floor, floors);
            Steps("StepsNorth", new Vector3(0f, 0f, 8f), Vector3.forward, 1.4f, Floor, floors);
            // Into the kitchen from the corridor and the store: one low step (the earth floor is lower).
            Steps("StepKitchen", new Vector3(-14f, 0.12f, 11f), Vector3.back, 1.2f, Floor - 0.12f, floors, 1);
        }

        // ------------------------------------------------------------------ walls

        private const float Outer = 0.3f, Inner = 0.2f;

        private static void OuterWalls(Transform root)
        {
            var walls = Group(root, "OuterWalls");
            float h = Floor + 3.0f;
            // West (distance along from the south end): chị Lam's and Kiều's barred windows over the alley, the kitchen's
            // back door and a high vent.
            Wall("West", new Vector2(Plan.West, Plan.South), new Vector2(Plan.West, Plan.North), 0f, h, Outer, PlasterDirty, walls, Plinth,
                Opening.Window(14.0f, 15.2f, Floor + 0.9f, Floor + 2.05f),
                Opening.Window(21.0f, 22.4f, Floor + 0.85f, Floor + 2.1f),
                new Opening(33.0f, 34.2f, 0.12f, 0.12f + DoorHeight),
                Opening.Window(35.6f, 36.4f, 1.9f, 2.5f));
            Wall("North", new Vector2(Plan.West, Plan.North), new Vector2(Plan.East, Plan.North), 0f, h, Outer, PlasterDirty, walls, Plinth);
            Wall("East", new Vector2(Plan.East, Plan.North), new Vector2(Plan.East, Plan.South), 0f, h, Outer, PlasterDirty, walls, Plinth);
            // South (distance along from the east end): the gate, and four high paper windows that light the hall.
            Wall("South", new Vector2(Plan.East, Plan.South), new Vector2(Plan.West, Plan.South), 0f, h, Outer, PlasterDirty, walls, Plinth,
                Opening.Window(4.5f, 6.5f, Floor + 1.0f, Floor + 2.1f),
                Opening.Window(12.5f, 14.5f, Floor + 1.0f, Floor + 2.1f),
                new Opening(19.5f, 22.5f, Floor, Floor + 2.7f),
                Opening.Window(27.5f, 29.5f, Floor + 1.0f, Floor + 2.1f),
                Opening.Window(35.5f, 37.5f, Floor + 1.0f, Floor + 2.1f));
            foreach (float x in new[] { -15.5f, -7.5f, 7.5f, 15.5f })
                PaperWindow("HallWindow", new Vector3(x, Floor + 1.55f, Plan.South), 2f, 1.1f, 0f, false, walls);
            // The neighbour's wall across the alley: old stone below, plaster above, moss at its foot.
            Box("AlleyWallBase", new Vector3(-25f, 0f, -19.4f), new Vector3(Plan.AlleyWest, 1.1f, 19.4f), WallStone, walls);
            Box("AlleyWall", new Vector3(-25f, 1.1f, -19.4f), new Vector3(Plan.AlleyWest, 3.6f, 19.4f), PlasterDirty, walls);
            Box("AlleyWallCap", new Vector3(-25.15f, 3.6f, -19.4f), new Vector3(-24.45f, 3.75f, 19.4f), Tiles, walls, false);
            Box("AlleyEndNorth", new Vector3(Plan.AlleyWest, 0f, 19.3f), new Vector3(Plan.AlleyEast, 3.6f, 19.6f), PlasterDirty, walls);
            Box("AlleyEndSouth", new Vector3(Plan.AlleyWest, 0f, -19.6f), new Vector3(Plan.AlleyEast, 3.6f, -19.3f), PlasterDirty, walls);

            // Bars in the west windows; Kiều sees the alley she cannot reach.
            Bars("BarsLam", new Vector3(Plan.West, Floor + 1.47f, -4.4f), 1.2f, 1.15f, 90f, walls);
            Bars("BarsKieu", new Vector3(Plan.West, Floor + 1.47f, 2.7f), 1.4f, 1.25f, 90f, walls);
            Bars("BarsKitchen", new Vector3(Plan.West, 2.2f, 17.0f), 0.8f, 0.6f, 90f, walls);

            // Main gate: two heavy leaves, barred at night.
            var gateWood = Planks;
            _refs.GateLeft = Door("GateLeft", DoorKind.Hinged, new Vector3(-1.5f, Floor, Plan.South), Vector3.right, 1.5f, 2.68f, -95f, gateWood, walls, _audio, _inventory, "cổng chính");
            _refs.GateRight = Door("GateRight", DoorKind.Hinged, new Vector3(1.5f, Floor, Plan.South), Vector3.left, 1.5f, 2.68f, 95f, gateWood, walls, _audio, _inventory, "cổng chính");
            foreach (var leaf in new[] { _refs.GateLeft, _refs.GateRight }) Set(leaf, "_playerCanUse", false);
            // Where the two leaves meet: a batten on the inside face so no sky shows between them.
            Box("GateMeet", new Vector3(-0.07f, Floor, -18.92f), new Vector3(0.07f, Floor + 2.68f, -18.86f), Timber, walls, false);
            _refs.GateInside = Point("GateInside", new Vector3(0f, Floor, -18f), 0f, walls);
        }

        private static void WestWing(Transform root)
        {
            var wing = Group(root, "WestWing");
            float h = 3.05f;
            // Veranda face (x −12, distance from the south end): chị Lam's window and door, Kiều's window and door.
            Wall("VerandaFace", new Vector2(-Plan.WingFace, Plan.HallFront), new Vector2(-Plan.WingFace, Plan.Screen), Floor, h, Inner, Plaster, wing, Plinth,
                Opening.Window(1.8f, 3.4f),
                Opening.Door(5.4f, 6.6f),
                Opening.Window(10.6f, 12.2f),
                Opening.Door(14.0f, 15.2f));
            Wall("KieuLam", new Vector2(Plan.West, Plan.WingSplit), new Vector2(-Plan.WingFace, Plan.WingSplit), Floor, h, Inner, Plaster, wing);
            Wall("LamSouth", new Vector2(Plan.West, Plan.HallFront), new Vector2(-Plan.WingFace, Plan.HallFront), Floor, h, Inner, Plaster, wing);
            Wall("KieuNorth", new Vector2(Plan.West, Plan.Screen), new Vector2(-Plan.WingFace, Plan.Screen), Floor, h, Inner, Plaster, wing);
            PaperWindow("LamWindow", new Vector3(-Plan.WingFace, Floor + 1.42f, -6.4f), 1.6f, 1.15f, 90f, true, wing);
            PaperWindow("KieuWindow", new Vector3(-Plan.WingFace, Floor + 1.42f, 2.4f), 1.6f, 1.15f, 90f, true, wing);

            // Doors. Lock state is set per chapter; the inside points tell the doors which side is which.
            _refs.KieuInside = Point("KieuInside", new Vector3(-13.5f, Floor, 5.6f), 0f, wing);
            _refs.LamInside = Point("LamInside", new Vector3(-13.5f, Floor, -3.0f), 0f, wing);
            _refs.KieuDoorOutside = Point("KieuDoorOutside", new Vector3(-10.9f, Floor, 5.6f), 90f, wing);
            _refs.LamDoorOutside = Point("LamDoorOutside", new Vector3(-10.9f, Floor, -3.0f), 90f, wing);
            _refs.KieuDoor = Door("KieuDoor", DoorKind.Hinged, new Vector3(-11.98f, Floor, 5.0f), Vector3.forward, 1.2f, DoorHeight, -100f, Planks, wing, _audio, _inventory, "cửa phòng", DoorLock.None, _refs.KieuInside);
            _refs.LamDoor = Door("LamDoor", DoorKind.Hinged, new Vector3(-11.98f, Floor, -3.6f), Vector3.forward, 1.2f, DoorHeight, -100f, Planks, wing, _audio, _inventory, "cửa phòng chị Lam", DoorLock.None, _refs.LamInside);
            // Chị Lam's door was broken once and patched: a fresh plank nailed across.
            var patch = Block("PatchPlank", Vector3.zero, new Vector3(0.03f, 0.16f, 1.15f), Mat("LTB_FreshPlank", new Color(0.72f, 0.58f, 0.42f), 0.2f), _refs.LamDoor.transform.Find("Leaf"), false);
            patch.transform.localPosition = new Vector3(0.05f, 1.25f, 0.6f);
            patch.transform.localRotation = Quaternion.Euler(8f, 0f, 0f);
        }

        private static void EastWing(Transform root)
        {
            var wing = Group(root, "EastWing");
            float h = 3.05f;
            // Veranda face (x 12, distance from the south end): the guests' windows and sliding door, Hồng's window and door.
            Wall("VerandaFace", new Vector2(Plan.WingFace, Plan.HallFront), new Vector2(Plan.WingFace, Plan.Screen), Floor, h, Inner, Plaster, wing, Plinth,
                Opening.Window(2.0f, 3.6f),
                Opening.Door(4.4f, 5.6f),
                Opening.Window(6.4f, 8.0f),
                Opening.Window(10.6f, 12.2f),
                Opening.Door(14.0f, 15.2f));
            Wall("HongGuests", new Vector2(Plan.WingFace, Plan.WingSplit), new Vector2(Plan.East, Plan.WingSplit), Floor, h, Inner, Plaster, wing);
            Wall("HongNorth", new Vector2(Plan.WingFace, Plan.Screen), new Vector2(Plan.East, Plan.Screen), Floor, h, Inner, Plaster, wing);
            Wall("GuestsSouth", new Vector2(Plan.WingFace, Plan.HallFront), new Vector2(Plan.East, Plan.HallFront), Floor, h, Inner, Plaster, wing);
            // The guests' room is bright: its paper windows glow and show shadows moving behind them.
            PaperWindow("GuestWindowA", new Vector3(Plan.WingFace, Floor + 1.42f, -6.2f), 1.6f, 1.15f, -90f, true, wing);
            PaperWindow("GuestWindowB", new Vector3(Plan.WingFace, Floor + 1.42f, -1.8f), 1.6f, 1.15f, -90f, true, wing);
            PaperWindow("HongWindow", new Vector3(Plan.WingFace, Floor + 1.42f, 2.4f), 1.6f, 1.15f, -90f, true, wing);
            _refs.HongInside = Point("HongInside", new Vector3(13.5f, Floor, 5.6f), 0f, wing);
            _refs.HongDoor = Door("HongDoor", DoorKind.Hinged, new Vector3(11.98f, Floor, 6.2f), Vector3.back, 1.2f, DoorHeight, -100f, Planks, wing, _audio, _inventory, "cửa phòng Hồng", DoorLock.None, _refs.HongInside);
            var guestInside = Point("GuestInside", new Vector3(13.5f, Floor, -4.0f), 0f, wing);
            _refs.GuestDoor = Door("GuestDoor", DoorKind.Sliding, new Vector3(11.85f, Floor, -3.4f), Vector3.back, 1.2f, DoorHeight, 0f, PaperLit, wing, _audio, _inventory, "cửa phòng khách", DoorLock.None, guestInside, true);
        }

        private static void RearRange(Transform root)
        {
            var range = Group(root, "RearRange");
            float h = 3.05f;
            // Corridor face of the rear range (z 11, distance from the west end): kitchen window and door, a store vent and
            // door, Tú Bà's sliding door on the axis and her lit windows, the office door and windows.
            Wall("CorridorFace", new Vector2(Plan.West, Plan.RearFront), new Vector2(Plan.East, Plan.RearFront), Floor, h, Inner, Plaster, range, Plinth,
                Opening.Window(2.0f, 3.4f),
                Opening.Door(6.4f, 7.6f),
                Opening.Window(11.5f, 12.5f, 1.9f, 2.5f),
                Opening.Door(14.4f, 15.6f),
                Opening.Door(20.3f, 21.7f),
                Opening.Window(23.4f, 25.2f, 0.75f, 2.0f),
                Opening.Window(26.4f, 28.2f, 0.75f, 2.0f),
                Opening.Door(31.4f, 32.6f),
                Opening.Window(33.8f, 35.4f),
                Opening.Window(38.0f, 40.0f));
            PaperWindow("KitchenWindow", new Vector3(-18.3f, Floor + 1.42f, Plan.RearFront), 1.4f, 1.15f, 180f, false, range);
            PaperWindow("TuBaWindowW", new Vector3(3.3f, Floor + 1.37f, Plan.RearFront), 1.8f, 1.25f, 180f, true, range);
            PaperWindow("TuBaWindowE", new Vector3(6.3f, Floor + 1.37f, Plan.RearFront), 1.8f, 1.25f, 180f, true, range);
            PaperWindow("OfficeWindow", new Vector3(13.6f, Floor + 1.42f, Plan.RearFront), 1.6f, 1.15f, 180f, false, range);
            PaperWindow("ArchiveWindow", new Vector3(18.0f, Floor + 1.42f, Plan.RearFront), 2.0f, 1.15f, 180f, false, range);

            // Kitchen | store: an open doorway with a cloth hanging in it, one step up into the store.
            Wall("KitchenStore", new Vector2(Plan.KitchenStore, Plan.RearFront), new Vector2(Plan.KitchenStore, Plan.North), Floor, h, Inner, Plaster, range, null, Opening.Door(3.6f, 4.8f));
            Steps("StepStore", new Vector3(Plan.KitchenStore, 0.12f, 15.2f), Vector3.right, 1.2f, Floor - 0.12f, range, 1);
            // Store | Tú Bà: plaster below, slats at eye height (a crack to watch her through), plaster above.
            Wall("StoreTuBa", new Vector2(Plan.StoreTuBa, Plan.RearFront), new Vector2(Plan.StoreTuBa, Plan.North), Floor, h, Inner, Plaster, range, null, new Opening(1.4f, 6.6f, 1.05f, 2.35f));
            Slats("StoreSlats", new Vector3(Plan.StoreTuBa, Floor, 12.5f), new Vector3(Plan.StoreTuBa, Floor, 17.5f), 1.05f, 2.35f, range, 0.13f);
            _refs.StoreSlats = Point("StoreSlatsView", new Vector3(-3.5f, Floor, 15.4f), 90f, range);
            // Tú Bà | office: door B, bolted on her side.
            Wall("TuBaOffice", new Vector2(Plan.TuBaOffice, Plan.RearFront), new Vector2(Plan.TuBaOffice, Plan.North), Floor, h, Inner, Plaster, range, null, Opening.Door(4.0f, 5.2f));
            // Office | archive: a lattice screen with a doorway in the middle.
            Wall("OfficeArchive", new Vector2(Plan.OfficeArchive, Plan.RearFront), new Vector2(Plan.OfficeArchive, Plan.North), Floor, h, Inner, Plaster, range, null,
                new Opening(0.3f, 2.6f, 0.9f, 2.4f), Opening.Door(3.4f, 4.6f), new Opening(5.4f, 7.7f, 0.9f, 2.4f));
            Slats("ArchiveSlatsA", new Vector3(Plan.OfficeArchive, Floor, 11.3f), new Vector3(Plan.OfficeArchive, Floor, 13.6f), 0.9f, 2.4f, range);
            Slats("ArchiveSlatsB", new Vector3(Plan.OfficeArchive, Floor, 16.4f), new Vector3(Plan.OfficeArchive, Floor, 18.7f), 0.9f, 2.4f, range);

            _refs.TuBaInside = Point("TuBaInside", new Vector3(2.5f, Floor, 14.5f), 0f, range);
            _refs.OfficeInside = Point("OfficeInside", new Vector3(11.5f, Floor, 14.0f), 0f, range);
            _refs.KitchenInside = Point("KitchenInside", new Vector3(-16f, 0.12f, 15f), 0f, range);
            _refs.TuBaDoorInside = Point("TuBaDoorInside", new Vector3(0f, Floor, 11.75f), 180f, range);
            _refs.TuBaDoorOutside = Point("TuBaDoorOutside", new Vector3(0f, Floor, 10.2f), 180f, range);
            _refs.OfficeDoorOutside = Point("OfficeDoorOutside", new Vector3(11.0f, Floor, 10.15f), 0f, range);

            // Store door: half open, warped.
            _refs.StoreDoor = Door("StoreDoor", DoorKind.Hinged, new Vector3(-6.6f, Floor, 11.02f), Vector3.right, 1.2f, DoorHeight, -70f, Planks, range, _audio, _inventory, "cửa kho vải", DoorLock.None, Point("StoreInside", new Vector3(-6.0f, Floor, 13.0f), 0f, range));
            // A: sliding paper door, the axis of the house (lined up with the gap in the courtyard screen and the gate).
            _refs.TuBaDoor = Door("TuBaDoor", DoorKind.Sliding, new Vector3(0.7f, Floor, 11.15f), Vector3.left, 1.4f, DoorHeight, 0f, PaperLit, range, _audio, _inventory, "cửa phòng Tú Bà", DoorLock.None, _refs.TuBaInside, true);
            // B: wooden door, bolt on Tú Bà's side.
            _refs.InnerDoor = Door("InnerDoor", DoorKind.Hinged, new Vector3(Plan.TuBaOffice + 0.02f, Floor, 15.0f), Vector3.forward, 1.2f, DoorHeight, 100f, Planks, range, _audio, _inventory, "cửa thông sang phòng sổ sách", DoorLock.None, _refs.TuBaInside);
            // C: office door from the corridor; key lock outside, bolt inside.
            _refs.OfficeDoor = Door("OfficeDoor", DoorKind.Hinged, new Vector3(10.4f, Floor, 11.02f), Vector3.right, 1.2f, DoorHeight, -100f, Planks, range, _audio, _inventory, "cửa phòng sổ sách", DoorLock.None, _refs.OfficeInside);
            // Back door: padlock and a crossbar; opens onto the alley.
            _refs.BackDoor = Door("BackDoor", DoorKind.Hinged, new Vector3(Plan.West, 0.12f, 14.0f), Vector3.forward, 1.2f, DoorHeight, -95f, Planks, range, _audio, _inventory, "cửa sau", DoorLock.None, _refs.KitchenInside, false, false);
            var bar = Block("CrossBar", new Vector3(-20.78f, 1.25f, 14.6f), new Vector3(0.12f, 0.12f, 1.7f), Timber, _refs.BackDoor.transform, false);
            bar.name = "CrossBar";
            Block("BarBracket", new Vector3(-20.8f, 1.25f, 13.8f), new Vector3(0.14f, 0.2f, 0.1f), Iron, range, false);
            Block("BarBracket", new Vector3(-20.8f, 1.25f, 15.4f), new Vector3(0.14f, 0.2f, 0.1f), Iron, range, false);
            _refs.BackDoorFront = Point("BackDoorFront", new Vector3(-20.1f, 0.12f, 14.6f), -90f, range);
        }

        private static void CourtyardEdges(Transform root)
        {
            var edges = Group(root, "CourtyardEdges");
            float pillarHeight = GalleryBeamTop - Floor; // the pillars carry the beams under the gallery floor
            // A pillar about every 3 m along the yard edges, leaving the steps clear.
            var sideZ = new[] { -8.9f, -6.0f, -3.0f, -0.3f, 2.3f, 5.1f, 7.9f };
            foreach (float z in sideZ) Column("PillarW", new Vector3(-9.12f, Floor, z), 0.14f, pillarHeight, edges, Vector3.forward, default, true);
            foreach (float z in sideZ) Column("PillarE", new Vector3(9.12f, Floor, z), 0.14f, pillarHeight, edges, Vector3.forward, default, true);
            foreach (float x in new[] { -6.0f, -3.0f, -1.15f, 1.15f, 3.0f, 6.0f }) Column("PillarS", new Vector3(x, Floor, -6.12f), 0.14f, pillarHeight, edges, Vector3.right, default, true);
            // Hall front: taller pillars between the hall and the south veranda; inside the hall a second row carries the
            // span of its roof.
            foreach (float x in new[] { -9f, -5f, -1.6f, 1.6f, 5f, 9f }) Column("PillarHall", new Vector3(x, Floor, -9.1f), 0.17f, 3.05f, edges, Vector3.right);
            foreach (float x in new[] { -15f, -4.5f, 4.5f, 15f }) Column("PillarHallInner", new Vector3(x, Floor, -14f), 0.17f, 3.05f, edges);

            // Railings along the yard, open only at the steps.
            Railing("RailW1", new Vector3(-9.1f, Floor, -6f), new Vector3(-9.1f, Floor, 0.3f), edges);
            Railing("RailW2", new Vector3(-9.1f, Floor, 1.7f), new Vector3(-9.1f, Floor, 7.9f), edges);
            Railing("RailE1", new Vector3(9.1f, Floor, -6f), new Vector3(9.1f, Floor, 0.3f), edges);
            Railing("RailE2", new Vector3(9.1f, Floor, 1.7f), new Vector3(9.1f, Floor, 7.9f), edges);
            Railing("RailS1", new Vector3(-9.1f, Floor, -6.1f), new Vector3(-0.8f, Floor, -6.1f), edges);
            Railing("RailS2", new Vector3(0.8f, Floor, -6.1f), new Vector3(9.1f, Floor, -6.1f), edges);

            // North screen: posts and slats between the yard and the corridor, with the gap on the house's axis.
            Slats("NorthScreenW", new Vector3(-9f, Floor, 8.06f), new Vector3(-0.75f, Floor, 8.06f), 0.15f, 2.55f, edges, 0.12f);
            Slats("NorthScreenE", new Vector3(0.75f, Floor, 8.06f), new Vector3(9f, Floor, 8.06f), 0.15f, 2.55f, edges, 0.12f);
            foreach (float x in new[] { -9f, -6.2f, -3.4f, -0.75f, 0.75f, 3.4f, 6.2f, 9f })
                Box("ScreenPost", new Vector3(x - 0.08f, Floor, 7.98f), new Vector3(x + 0.08f, GalleryBeamTop - 0.24f, 8.14f), Timber, edges);
            Box("ScreenHeader", new Vector3(-9.1f, Floor + 2.55f, 8.0f), new Vector3(9.1f, GalleryBeamTop - 0.24f, 8.12f), Timber, edges, false);

            // The verandas and the corridor are roofed by the gallery floor above (see UpperStorey); behind the wing rooms
            // the corridor is indoors, under a flat ceiling.
            CeilingPanel("CeilingCorridorWest", Plan.West, Plan.Screen, -Plan.WingFace, Plan.RearFront, Floor + 2.98f, edges, true);
            CeilingPanel("CeilingCorridorEast", Plan.WingFace, Plan.Screen, Plan.East, Plan.RearFront, Floor + 2.98f, edges, true);
        }

        private static void Roofs(Transform root)
        {
            var roofs = Group(root, "Roofs");
            // Each roof sits on the upper eave beam along the yard (tiles 15 cm above its top) and rises at its pitch to the
            // ridge. The main house to the north is pitched steeper (35°) so its ridge stands highest; the wings run on
            // into the north and south roofs until their ridges and gables are inside them, so the corners close.
            float onBeam = UpperEave + 0.15f, tan30 = Mathf.Tan(30f * Mathf.Deg2Rad), tan35 = Mathf.Tan(35f * Mathf.Deg2Rad);
            float wingRidge = onBeam + (15f - 9.12f) * tan30;
            TileRoof("RoofWest", new Vector3(-15f, wingRidge, 0.45f), 25.5f, 6.4f, 30f, false, roofs);
            TileRoof("RoofEast", new Vector3(15f, wingRidge, 0.45f), 25.5f, 6.4f, 30f, false, roofs);
            TileRoof("RoofNorth", new Vector3(0f, onBeam + (13.5f - Plan.Screen) * tan35, 13.5f), 42.8f, 5.9f, 35f, true, roofs, true, 5.8f);
            TileRoof("RoofSouth", new Vector3(0f, onBeam + (12.5f - 6.12f) * tan30, -12.5f), 42.8f, 6.9f, 30f, true, roofs, true, 5.8f);
            // Gate roof, a little higher than the hall wall, so the way out reads as the way out.
            Roof("RoofGate", new Vector3(0f, 5.2f, -19.4f), 4.8f, 1.8f, 28f, Tiles, Ridge, roofs);
            // Ceilings of the rooms and the hall.
            float ceiling = Floor + 2.98f;
            CeilingPanel("CeilingKieu", Plan.West, Plan.WingSplit, -Plan.WingFace, Plan.Screen, ceiling, roofs);
            CeilingPanel("CeilingLam", Plan.West, Plan.HallFront, -Plan.WingFace, Plan.WingSplit, ceiling, roofs);
            CeilingPanel("CeilingHong", Plan.WingFace, Plan.WingSplit, Plan.East, Plan.Screen, ceiling, roofs);
            CeilingPanel("CeilingGuests", Plan.WingFace, Plan.HallFront, Plan.East, Plan.WingSplit, ceiling, roofs);
            CeilingPanel("CeilingHall", Plan.West, Plan.South, Plan.East, Plan.HallFront, ceiling, roofs, true);
            CeilingPanel("CeilingKitchen", Plan.West, Plan.RearFront, Plan.KitchenStore, Plan.North, ceiling, roofs);
            CeilingPanel("CeilingStore", Plan.KitchenStore, Plan.RearFront, Plan.StoreTuBa, Plan.North, ceiling, roofs);
            CeilingPanel("CeilingTuBa", Plan.StoreTuBa, Plan.RearFront, Plan.TuBaOffice, Plan.North, ceiling, roofs);
            CeilingPanel("CeilingOffice", Plan.TuBaOffice, Plan.RearFront, Plan.East, Plan.North, ceiling, roofs);
        }

        /// <summary>
        /// Static batching merges meshes into world space when the scene starts and the batched draws lose the object
        /// transform the wood shader reads to turn the grain along each piece (and the UV3 grain is not carried into world
        /// space): in Play Mode and builds the pillars showed swirling, cross-grain figure that the editor never showed.
        /// Wood keeps its own transform; everything else still batches.
        /// </summary>
        private static void KeepWoodUnbatched(Transform root)
        {
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                bool wood = false;
                foreach (var material in renderer.sharedMaterials) wood |= material != null && material.IsKeywordEnabled("_GRAIN_FOLLOWS_OBJECT");
                if (!wood) continue;
                var flags = UnityEditor.GameObjectUtility.GetStaticEditorFlags(renderer.gameObject);
                UnityEditor.GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, flags & ~UnityEditor.StaticEditorFlags.BatchingStatic);
            }
        }

        // ------------------------------------------------------------------ helpers

        internal static Transform Group(Transform parent, string name)
        {
            var go = new GameObject(name).transform;
            go.SetParent(parent, false);
            return go;
        }

        internal static void SetLock(HouseDoor door, DoorLock state) => Set(door, "_lock", (int)state);
    }
}
