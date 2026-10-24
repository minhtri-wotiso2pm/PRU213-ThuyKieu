using ThuyKieu.Environment;
using ThuyKieu.Gameplay.House;
using ThuyKieu.Gameplay.Stealth;
using UnityEngine;
using UnityEngine.Rendering;
using static ThuyKieu.Core.Editor.LauTuBaKit;
using static ThuyKieu.Core.Editor.LauTuBaMeshKit;
using static ThuyKieu.Core.Editor.StorySceneKit;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Furniture and fittings of Tú Bà's house. Every piece has a job: cover to crouch behind, a place to hide, a thing to
    /// read or take, a light that makes a pool to avoid. Rooms are laid out in zones (sleeping, a personal table, storage,
    /// receiving) and say who lives in them: the guests' room and the hall are lacquer and silk, the girls' rooms are
    /// modest and each different, the kitchen and store are earth, soot and rough wood.
    /// </summary>
    public static partial class LauTuBaHouse
    {
        public static Transform KieuWindowCurtain;
        public static Transform ArchiveCabinetDoor;

        private static void Rooms(Transform root)
        {
            var rooms = Group(root, "Rooms");
            KieuRoom(Group(rooms, "Kieu"));
            LamRoom(Group(rooms, "Lam"));
            JarCorner(Group(rooms, "Jars"));
            HongRoom(Group(rooms, "Hong"));
            GuestRoom(Group(rooms, "Guests"));
            Hall(Group(rooms, "Hall"));
            Corridor(Group(rooms, "Corridor"));
            Kitchen(Group(rooms, "Kitchen"));
            Store(Group(rooms, "Store"));
            TuBaRoom(Group(rooms, "TuBa"));
            Office(Group(rooms, "Office"));
            Yard(Group(rooms, "Yard"));
            Panelling(Group(rooms, "Panelling"));
            Fills(Group(rooms, "FillLights"));
        }

        /// <summary>
        /// Soft lights standing in for bounced light (no lightmaps: Chapter 3's sun moves into dusk). By day a warm bounce
        /// under the eaves and in each room; by night a faint cool spill of moonlight into rooms with no lamp, so shapes
        /// and doorways still read in the dark. They count for little with the guards (<see cref="Weighted"/>).
        /// </summary>
        private static void Fills(Transform root)
        {
            if (_night)
            {
                var moon = new Color(0.5f, 0.56f, 0.72f);
                foreach (var p in new[] { new Vector3(18f, Floor + 2.3f, 15f), new Vector3(-7f, Floor + 2.3f, 15f), new Vector3(-15f, Floor + 2.3f, 9.5f),
                             new Vector3(15f, Floor + 2.3f, 9.5f), new Vector3(11.5f, Floor + 2.3f, 13f), new Vector3(-16.5f, Floor + 2.3f, -4.5f),
                             new Vector3(-12f, Floor + 2.6f, -14f), new Vector3(12f, Floor + 2.6f, -14f), new Vector3(3f, Floor + 2.3f, 16.5f), new Vector3(-16f, 2.4f, 14f) })
                    _refs.Fills.Add(Weighted(RoomLight("MoonSpill", p, moon, 7f, 0.42f, root), 0.35f));
                // The kitchen's barred window lets in a little more: the far corner by the back door stays legible.
                _refs.Fills.Add(Weighted(RoomLight("MoonSpillKitchen", new Vector3(-20f, 2.2f, 16.6f), moon, 6.5f, 0.5f, root), 0.35f));
                return;
            }
            var day = new Color(1f, 0.9f, 0.78f);
            foreach (var p in new[]
            {
                new Vector3(-10.5f, Floor + 2.4f, -4f), new Vector3(-10.5f, Floor + 2.4f, 4f), new Vector3(10.5f, Floor + 2.4f, -4f), new Vector3(10.5f, Floor + 2.4f, 4f),
                new Vector3(0f, Floor + 2.4f, -7.5f), new Vector3(-10f, Floor + 2.4f, 9.5f), new Vector3(10f, Floor + 2.4f, 9.5f),
                new Vector3(-16.5f, Floor + 2.4f, 3.8f), new Vector3(-16.5f, Floor + 2.4f, -4.8f), new Vector3(16.5f, Floor + 2.4f, 3.8f), new Vector3(16.5f, Floor + 2.4f, -4.8f),
                new Vector3(-16f, Floor + 2.6f, -14f), new Vector3(-8f, Floor + 2.6f, -14f), new Vector3(0f, Floor + 2.6f, -14f), new Vector3(8f, Floor + 2.6f, -14f), new Vector3(16f, Floor + 2.6f, -14f),
                new Vector3(-16f, 2.4f, 15f), new Vector3(-7f, Floor + 2.4f, 15f), new Vector3(0f, Floor + 2.4f, 14f), new Vector3(5f, Floor + 2.4f, 16.5f),
                new Vector3(11.5f, Floor + 2.4f, 15f), new Vector3(18f, Floor + 2.4f, 15f)
            })
                _refs.Fills.Add(Weighted(RoomLight("DayBounce", p, day, 8f, 0.95f, root), 0.3f));
        }

        /// <summary>Dark timber panelling on the inner face of the main room walls (the frame of a wooden house).</summary>
        private static void Panelling(Transform root)
        {
            foreach (var (from, to, inward) in new[]
            {
                (new Vector3(-20.84f, Floor, -0.4f), new Vector3(-20.84f, Floor, 7.9f), Vector3.right),    // Kiều: west
                (new Vector3(-20.9f, Floor, 7.88f), new Vector3(-12.15f, Floor, 7.88f), Vector3.back),     // Kiều: north
                (new Vector3(-20.84f, Floor, -8.9f), new Vector3(-20.84f, Floor, -0.6f), Vector3.right),   // Lam: west
                (new Vector3(20.84f, Floor, -0.4f), new Vector3(20.84f, Floor, 7.9f), Vector3.left),      // Hồng: east
                (new Vector3(20.84f, Floor, -8.9f), new Vector3(20.84f, Floor, -0.6f), Vector3.left),     // guests: east
                (new Vector3(-2.9f, Floor, 18.84f), new Vector3(7.9f, Floor, 18.84f), Vector3.back),      // Tú Bà: north
                (new Vector3(8.1f, Floor, 18.84f), new Vector3(14.9f, Floor, 18.84f), Vector3.back),      // office: north
                (new Vector3(20.84f, Floor, 11.1f), new Vector3(20.84f, Floor, 18.9f), Vector3.left)      // archive: east
            })
                StoryPropKit.WallPanels(from, to, 2.98f, inward, root);
        }

        /// <summary>A hanging silk lantern whose body only glows while its candle is lit (see <see cref="SilkLantern"/>).</summary>
        private static Light HouseLantern(string name, Vector3 position, Transform parent, float range, float intensity, bool shadows = false) =>
            SilkLantern(name, position, parent, range, intensity, 0.42f, 1f, shadows);

        // ------------------------------------------------------------------ west wing

        private static void KieuRoom(Transform room)
        {
            // Small and given, not owned, in four zones: the curtained sleeping corner (NW), the barred window over the
            // alley behind a faded curtain (W), her writing table with poems on a reed mat (centre), the dressing table with
            // the hairpin and the trunk with the new silk dress and its price slip ("Áo lụa – 12 lạng – ghi sổ") (S). The
            // wardrobe stands just inside the door.
            var faded = Mat("LTB_SilkFaded", new Color(0.38f, 0.12f, 0.11f), 0.3f);
            var sap = Sap("KieuSap", room, new Vector3(-19.4f, Floor, 7.2f), 90f, 2f, 1.1f, false);
            Cushion("Pillow", sap, new Vector3(0f, 0.5f, 0.78f), new Vector3(0.5f, 0.09f, 0.26f), _cotton, 0.02f);
            Cushion("Blanket", sap, new Vector3(0f, 0.46f, -0.45f), new Vector3(0.9f, 0.06f, 0.5f), Mat("LTB_QuiltIndigo", new Color(0.18f, 0.22f, 0.32f), 0.15f), 0.012f);
            PleatedCurtain("BedCurtain", new Vector3(-19.4f, Floor + 2.35f, 5.85f), 2.3f, 2.15f, 0f, faded, room);
            // Window curtain hung from its north edge so that drawing it gathers it there (Chapter 3 opens it).
            var windowCurtain = Frame("WindowCurtain", room, new Vector3(-20.76f, Floor + 2.2f, 3.5f), 90f);
            Piece("Cloth", Pleats("Pleats_KieuWindow", 1.6f, 1.7f, 7, 0.03f), faded, windowCurtain, windowCurtain.position, windowCurtain.rotation)
                .GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.TwoSided;
            Wood("CurtainRod", room, new Vector3(-20.74f, Floor + 2.22f, 2.7f), new Vector3(0.035f, 0.035f, 1.75f), Timber, 0.012f);
            KieuWindowCurtain = windowCurtain;
            var vanity = StoryPropKit.Vanity(new Vector3(-17.6f, Floor, -0.22f), 0f, room, 41);
            var trunk = Trunk("KieuTrunk", room, new Vector3(-20.4f, Floor, 0.6f), 90f, 0.8f, 0.45f, 0.5f);
            Cushion("Dress", trunk, new Vector3(0f, 0.49f, 0f), new Vector3(0.56f, 0.06f, 0.4f), Mat("LTB_SilkPink", new Color(0.82f, 0.46f, 0.54f), 0.5f), 0.01f);
            PriceSlip(new Vector3(-20.35f, Floor + 0.535f, 0.55f), 20f, room);
            // The writing table: poem sheets weighted with the ink stone, a brush, a cup of tea gone cold.
            Covering(Piece("ReedMat", RoundedBox(new Vector3(2.0f, 0.012f, 1.6f), 0.004f), _reed, room, new Vector3(-15.6f, Floor + 0.006f, 2.8f), Quaternion.identity).transform, Surface.Mat);
            var table = Table("KieuTable", room, new Vector3(-15.6f, Floor + 0.012f, 2.8f), 0f, 0.9f, 0.52f, 0.32f, Timber, false, false);
            for (int i = 0; i < 3; i++)
                Piece("PoemSheet", RoundedBox(new Vector3(0.22f, 0.002f, 0.3f), 0.001f), _paperWarm, table, table.TransformPoint(new Vector3(-0.2f + i * 0.04f, 0.322f + i * 0.002f, 0.02f * i)), table.rotation * Quaternion.Euler(0f, i * 9f - 6f, 0f));
            Piece("InkStone", RoundedBox(new Vector3(0.1f, 0.02f, 0.14f), 0.006f), _ink, table, table.TransformPoint(new Vector3(0.12f, 0.33f, 0.1f)), table.rotation);
            Piece("Brush", Lathe("Brush", new[] { new Vector2(0.004f, 0f), new Vector2(0.004f, 0.22f) }, 6, capBottom: false), Timber, table, table.TransformPoint(new Vector3(0.05f, 0.335f, -0.05f)), table.rotation * Quaternion.Euler(0f, 0f, 88f));
            Piece("Cup", CupMesh, _brownGlaze, table, table.TransformPoint(new Vector3(0.32f, 0.32f, -0.12f)), table.rotation);
            foreach (float x in new[] { -0.75f, 0.75f })
                Piece("FloorCushion", RoundedBox(new Vector3(0.44f, 0.07f, 0.44f), 0.03f, 0.015f), faded, room, new Vector3(-15.6f + x, Floor + 0.04f, 2.8f), Quaternion.identity);
            Shelves("KieuShelf", room, new Vector3(-14.2f, Floor, 7.72f), 180f, 1.0f, 1.3f, 3, Timber, 42, false);
            // Her other clothes on a peg by the door.
            Wood("Peg", room, new Vector3(-12.14f, Floor + 1.85f, 4.0f), new Vector3(0.1f, 0.025f, 0.025f), Timber, 0.008f);
            PleatedCurtain("HangingDress", new Vector3(-12.2f, Floor + 1.84f, 4.0f), 0.42f, 1.05f, 90f, Mat("LTB_SilkTeal", new Color(0.2f, 0.34f, 0.34f), 0.32f), room, 0.3f, false);
            // Scratches at the door's latch side, at hand height: someone tried to lift this latch before her.
            for (int i = 0; i < 5; i++)
            {
                var scratch = Block("Scratch", new Vector3(-12.11f, Floor + 1.02f + i * 0.035f, 6.32f + i * 0.012f), new Vector3(0.006f, 0.008f, 0.22f - i * 0.02f),
                    Mat("LTB_Scratch", new Color(0.82f, 0.74f, 0.6f), 0.05f), room, false);
                scratch.transform.rotation = Quaternion.Euler(0f, 0f, 9f - i * 4f);
            }
            var wardrobeDoor = Wardrobe("KieuWardrobe", new Vector3(-12.5f, Floor, 7.3f), -90f, room);
            _refs.Hides.Add(HideSpot("KieuWardrobe", new Vector3(-12.5f, Floor, 7.3f), -90f, new Vector3(-12.5f, Floor + 1f, 7.3f), new Vector3(0.9f, 2f, 1.1f),
                _player, "Trốn vào tủ áo", "door", room, wardrobeDoor, 32f).WithKind(HideKind.Wardrobe));
            _refs.Hides.Add(HideSpot("KieuBed", new Vector3(-19.4f, Floor, 6.25f), 180f, new Vector3(-19.4f, Floor + 0.9f, 6.05f), new Vector3(2.2f, 1.8f, 0.6f),
                _player, "Nấp sau rèm giường", "cloth", room, null, 40f).WithKind(HideKind.Curtain));
            _refs.KieuLamp = Lamp("KieuLamp", new Vector3(-16.6f, Floor, 5.6f), room, 4.6f, 0.75f);
            // The hairpin on the dressing table (Chapter 4 uses it on the latch).
            var pin = new GameObject("Hairpin");
            pin.transform.SetParent(room, false);
            pin.transform.position = vanity.position + new Vector3(0.28f, -0.32f, 0.12f);
            var pinVisual = new GameObject("Visual").transform;
            pinVisual.SetParent(pin.transform, false);
            var gold = EmissiveMat("LTB_Gold", new Color(0.82f, 0.62f, 0.28f), 0.25f);
            var shaft = Block("Shaft", Vector3.zero, new Vector3(0.012f, 0.008f, 0.2f), gold, pinVisual, false);
            shaft.transform.localPosition = new Vector3(0f, 0.005f, 0f);
            shaft.transform.localRotation = Quaternion.Euler(0f, 30f, 0f);
            var head = StoryArtKit.Sphere("Head", Vector3.zero, Vector3.one * 0.03f, gold, pinVisual);
            head.transform.localPosition = new Vector3(0.05f, 0.012f, 0.085f);
            var box = pin.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(0.35f, 0.2f, 0.35f);
            pin.layer = 2;
            _refs.Hairpin = pin.AddComponent<PickupItem>();
            Set(_refs.Hairpin, "_item", "tram");
            Set(_refs.Hairpin, "_label", "Lấy cây trâm");
            Set(_refs.Hairpin, "_visual", pinVisual.gameObject);
            Set(_refs.Hairpin, "_inventory", _inventory);
            Set(_refs.Hairpin, "_audio", _audio);
        }

        private static void LamRoom(Transform room)
        {
            // Lived in for years: the sleeping mat with a worn quilt (NW), the old trunk, mended clothes on a line, the
            // table under the window with an unsent letter to her mother, a broken comb and her sewing (S), incense for
            // the dead on a little shelf, the screen in the south-west corner.
            var quilt = Mat("LTB_QuiltBlue", new Color(0.22f, 0.29f, 0.4f), 0.15f);
            var bed = Frame("LamBed", room, new Vector3(-19.6f, Floor, -2.1f), 0f);
            Piece("SleepMat", RoundedBox(new Vector3(1.0f, 0.03f, 1.8f), 0.01f), _reed, bed, bed.position + Vector3.up * 0.015f, bed.rotation);
            Cushion("Quilt", bed, new Vector3(0.02f, 0.06f, -0.2f), new Vector3(0.84f, 0.06f, 1.0f), quilt, 0.015f);
            Cushion("Pillow", bed, new Vector3(0f, 0.07f, 0.66f), new Vector3(0.44f, 0.09f, 0.22f), _cotton, 0.02f);
            Trunk("LamTrunk", room, new Vector3(-20.4f, Floor, -4.9f), 90f, 0.7f, 0.4f, 0.45f, true);
            var table = Table("LamTable", room, new Vector3(-15.2f, Floor, -6.6f), 0f, 0.8f, 0.48f, 0.32f, _woodWorn, false, false);
            var paper = Mat("LTB_Letter", new Color(0.86f, 0.8f, 0.66f), 0.05f);
            Piece("Letter", RoundedBox(new Vector3(0.3f, 0.004f, 0.23f), 0.001f), paper, table, table.TransformPoint(new Vector3(-0.15f, 0.322f, 0f)), table.rotation * Quaternion.Euler(0f, 8f, 0f));
            var horn = Mat("LTB_Horn", new Color(0.3f, 0.2f, 0.12f), 0.5f);
            Piece("CombHalf", RoundedBox(new Vector3(0.1f, 0.012f, 0.06f), 0.003f), horn, table, table.TransformPoint(new Vector3(0.18f, 0.326f, 0.05f)), table.rotation);
            Piece("CombHalf", RoundedBox(new Vector3(0.06f, 0.012f, 0.06f), 0.003f), horn, table, table.TransformPoint(new Vector3(0.3f, 0.326f, 0.02f)), table.rotation * Quaternion.Euler(0f, 25f, 0f));
            Piece("InkStone", RoundedBox(new Vector3(0.1f, 0.02f, 0.14f), 0.006f), _ink, table, table.TransformPoint(new Vector3(0.28f, 0.33f, -0.12f)), table.rotation);
            Piece("FloorCushion", RoundedBox(new Vector3(0.44f, 0.07f, 0.44f), 0.03f, 0.015f), quilt, room, new Vector3(-15.2f, Floor + 0.04f, -5.95f), Quaternion.identity);
            Piece("Basket", Lathe("Basket", new[] { new Vector2(0.14f, 0f), new Vector2(0.18f, 0.03f), new Vector2(0.2f, 0.12f), new Vector2(0.19f, 0.13f), new Vector2(0.16f, 0.05f), new Vector2(0f, 0.04f) }, 20), _reed, room, new Vector3(-16.2f, Floor, -6.3f), Quaternion.identity);
            Cushion("Cloth", table, new Vector3(-0.95f, 0.12f, 0.25f), new Vector3(0.22f, 0.04f, 0.18f), Mat("LTB_CottonRed", new Color(0.5f, 0.16f, 0.12f), 0.1f), 0.01f);
            // Incense for her mother on a small wall shelf.
            Wood("AltarShelf", room, new Vector3(-17.4f, Floor + 1.45f, -8.82f), new Vector3(0.5f, 0.03f, 0.22f), Timber, 0.006f);
            Piece("IncenseCup", BowlMesh, _terracotta, room, new Vector3(-17.5f, Floor + 1.465f, -8.82f), Quaternion.identity, Vector3.one * 0.8f);
            Piece("Bowl", CupMesh, _porcelain, room, new Vector3(-17.25f, Floor + 1.465f, -8.82f), Quaternion.identity);
            // Mended clothes on a line across the north-east corner.
            Wood("ClothesLine", room, new Vector3(-14.2f, Floor + 1.75f, -1.0f), new Vector3(2.6f, 0.02f, 0.02f), Bamboo, 0.008f);
            PleatedCurtain("Mended", new Vector3(-14.8f, Floor + 1.74f, -1.0f), 0.5f, 0.8f, 0f, _cotton, room, 0.15f, false);
            PleatedCurtain("Mended", new Vector3(-13.9f, Floor + 1.74f, -1.0f), 0.42f, 0.95f, 0f, quilt, room, 0.25f, false);
            var screen = Screen("LamScreen", new Vector3(-19.9f, Floor, -7.4f), 90f, room, new Color(0.3f, 0.34f, 0.42f));
            _refs.Hides.Add(HideSpot("LamScreen", new Vector3(-20.55f, Floor, -7.4f), 90f, new Vector3(-20.2f, Floor + 0.9f, -7.4f), new Vector3(1.2f, 1.8f, 1.8f),
                _player, "Nấp sau bình phong", "paper", room, screen, 45f).WithCrouch().WithKind(HideKind.Screen));
            _refs.LamLamp = Lamp("LamLamp", new Vector3(-18.2f, Floor, -5.4f), room, 3.6f, 0.45f);
        }

        private static void JarCorner(Transform room)
        {
            // The water-jar corner at the south end of the west veranda, where the hall begins: glazed jars, a washboard,
            // a bucket, a towel pole. The jars are low cover; behind them is a place to hide.
            LiteProp(PropJars, 30000, new Vector3(-10.6f, Floor, -8.45f), 0f, 0.95f, room, true, "WaterJars");
            Wood("WashBoard", room, new Vector3(-9.55f, Floor + 0.3f, -7.6f), new Vector3(0.04f, 0.6f, 0.5f), _woodWorn, 0.01f);
            Solid(room, new Vector3(-9.55f, Floor + 0.3f, -7.6f), new Vector3(0.1f, 0.6f, 0.5f));
            Piece("Bucket", Lathe("Bucket", new[] { new Vector2(0.15f, 0f), new Vector2(0.17f, 0.28f), new Vector2(0.16f, 0.28f), new Vector2(0.14f, 0.02f), new Vector2(0f, 0.02f) }, 20), _woodWorn, room, new Vector3(-9.6f, Floor, -6.9f), Quaternion.identity);
            Wood("TowelPole", room, new Vector3(-11.85f, Floor + 1.6f, -7.6f), new Vector3(0.03f, 0.03f, 1.6f), Bamboo, 0.012f);
            PleatedCurtain("Towel", new Vector3(-11.85f, Floor + 1.6f, -7.9f), 0.4f, 0.55f, 90f, _cotton, room, 0f, false);
            _refs.Hides.Add(HideSpot("Jars", new Vector3(-11.65f, Floor, -8.6f), 45f, new Vector3(-11.0f, Floor + 0.7f, -8.2f), new Vector3(2.0f, 1.4f, 1.2f),
                _player, "Nấp sau dãy chum", "step", room, null, 50f).WithCrouch().WithKind(HideKind.Low));
        }

        // ------------------------------------------------------------------ east wing

        private static void HongRoom(Transform room)
        {
            // Hồng has been here longest and made the room hers: the pink-curtained bed (NE), her own cabinet with a flower
            // vase (E), the dressing table crowded with pots (S), a rack of dresses, embroidery in progress, and a little
            // tea table for the girls who come to talk (centre).
            var pink = Mat("LTB_SilkPinkDeep", new Color(0.62f, 0.26f, 0.32f), 0.42f);
            StoryPropKit.Vanity(new Vector3(17.6f, Floor, -0.22f), 0f, room, 61);
            for (int i = 0; i < 4; i++)
                Piece("Pot", Lathe("CosmeticPot", new[] { new Vector2(0.03f, 0f), new Vector2(0.035f, 0.02f), new Vector2(0.03f, 0.04f), new Vector2(0.02f, 0.045f), new Vector2(0f, 0.05f) }, 14),
                    i % 2 == 0 ? _porcelain : _celadon, room, new Vector3(17.15f + i * 0.09f, Floor + 0.76f, -0.3f), Quaternion.identity);
            var sap = Sap("HongSap", room, new Vector3(19.4f, Floor, 7.2f), 90f, 2f, 1.1f, false);
            Cushion("Pillow", sap, new Vector3(0f, 0.5f, 0.78f), new Vector3(0.5f, 0.09f, 0.26f), pink, 0.02f);
            Cushion("Quilt", sap, new Vector3(0f, 0.46f, -0.35f), new Vector3(0.95f, 0.06f, 0.7f), pink, 0.012f);
            PleatedCurtain("BedCurtain", new Vector3(19.4f, Floor + 2.35f, 5.85f), 2.3f, 2.15f, 0f, pink, room, 0.35f);
            LiteProp(PropCabinet, 40000, new Vector3(20.5f, Floor, 3.4f), -90f, 0.95f, room, true, "HongCabinet");
            StoryPropKit.FlowerVase(new Vector3(20.5f, Floor + 1.72f, 3.4f), room, 62, 0.8f);
            // A dress rack: a pole on two feet with silks hanging from it.
            var rack = Frame("DressRack", room, new Vector3(15.5f, Floor, 7.5f), 0f);
            foreach (float x in new[] { -0.6f, 0.6f }) Wood("Post", rack, new Vector3(x, 0.8f, 0f), new Vector3(0.04f, 1.6f, 0.04f), Lacquer, 0.006f);
            foreach (float x in new[] { -0.6f, 0.6f }) Wood("Foot", rack, new Vector3(x, 0.02f, 0f), new Vector3(0.06f, 0.04f, 0.4f), Lacquer, 0.006f);
            Wood("Pole", rack, new Vector3(0f, 1.58f, 0f), new Vector3(1.32f, 0.035f, 0.035f), Lacquer, 0.012f);
            PleatedCurtain("Dress", rack.TransformPoint(new Vector3(-0.3f, 1.57f, 0f)), 0.5f, 1.1f, 0f, pink, room, 0.25f, false);
            PleatedCurtain("Dress", rack.TransformPoint(new Vector3(0.25f, 1.57f, 0f)), 0.46f, 1.2f, 0f, Mat("LTB_SilkTeal", new Color(0.2f, 0.34f, 0.34f), 0.32f), room, 0.3f, false);
            Solid(rack, new Vector3(0f, 0.8f, 0f), new Vector3(1.3f, 1.6f, 0.3f));
            StoryPropKit.EmbroideryFrame(new Vector3(14.6f, Floor, 2.0f), 60f, room, 63);
            var tea = TeaTable("HongTea", room, new Vector3(17.2f, Floor, 3.4f), 0f, _porcelain);
            foreach (var offset in new[] { new Vector3(0f, 0.04f, -0.55f), new Vector3(-0.65f, 0.04f, 0f) })
                Cushion("FloorCushion", tea, offset, new Vector3(0.44f, 0.07f, 0.44f), pink, 0.015f);
            StoryPropKit.Scroll(new Vector3(20.82f, Floor + 1.7f, 1.2f), -90f, room, 4);
            Lamp("HongLamp", new Vector3(14.0f, Floor, 0.9f), room, 4.6f, 1.1f);
        }

        private static void GuestRoom(Transform room)
        {
            // Where guests drink: the carved table set with its benches (centre), a wine stand (SE), the screen (E),
            // cushions by the windows, lights of their own that throw the guests' shadows on the paper.
            LiteProp(PropTableSet, 60000, new Vector3(16.5f, Floor, -4.75f), 90f, 1f, room, true, "GuestTable");
            var corner = Table("WineStand", room, new Vector3(20.35f, Floor, -8.3f), 90f, 0.8f, 0.45f, 0.62f, Lacquer, true, false);
            Jar(corner, corner.TransformPoint(new Vector3(-0.15f, 0.62f, 0f)), 0.45f, _brownGlaze, false);
            Vase(corner, corner.TransformPoint(new Vector3(0.22f, 0.62f, 0f)), 0.6f, _porcelain);
            foreach (var p in new[] { new Vector3(13.2f, Floor, -6.4f), new Vector3(13.2f, Floor, -1.9f), new Vector3(13.9f, Floor, -1.2f) })
                Piece("FloorCushion", RoundedBox(new Vector3(0.5f, 0.08f, 0.5f), 0.035f, 0.02f), _silkRed, room, p + Vector3.up * 0.04f, Quaternion.Euler(0f, p.z * 20f, 0f));
            Screen("GuestScreen", new Vector3(20.4f, Floor, -4.7f), -90f, room, new Color(0.62f, 0.14f, 0.12f));
            StoryPropKit.Scroll(new Vector3(20.82f, Floor + 1.7f, -1.8f), -90f, room, 5);
            Planter(room, new Vector3(20.3f, Floor, -1.2f), PropBanana, 0.16f, _celadon);
            RoomLight("GuestLight", new Vector3(15.0f, Floor + 2.4f, -6.2f), new Color(1f, 0.62f, 0.34f), 6.5f, 1.7f, room);
            RoomLight("GuestLight", new Vector3(15.0f, Floor + 2.4f, -1.8f), new Color(1f, 0.62f, 0.34f), 6.5f, 1.4f, room);
        }

        private static void Hall(Transform room)
        {
            // The front of the house, ordered and rich (42 × 10 m): the ancestor altar with armchairs at the west end, two
            // receiving groups on rugs either side of the axis, close enough to read from the yard steps, two tea tables in
            // front of them by the open front, deep red curtains tied back in the front bays, a raised stage with a drum and a moon lute at the east end,
            // display tables between the high windows, and on the axis the gate, its board, vases and the red runner up to
            // the yard steps.
            LiteProp(PropAltar, 60000, new Vector3(-19.4f, Floor, -14f), 90f, 1f, room, true, "Altar");
            Armchair("AltarChair", room, new Vector3(-18.0f, Floor, -15.6f), 60f, Lacquer, _silkRed);
            Armchair("AltarChair", room, new Vector3(-18.0f, Floor, -12.4f), 120f, Lacquer, _silkRed);
            StoryPropKit.Scroll(new Vector3(-20.82f, Floor + 1.9f, -14f), 90f, room, 8, 0.8f, 1.6f);
            foreach (float x in new[] { -7f, 7f })
            {
                var rug = Frame("ReceivingRug", room, new Vector3(x, Floor, -14f), 0f);
                Covering(rug, Surface.Mat);
                Piece("RugBorder", RoundedBox(new Vector3(4.2f, 0.012f, 3.4f), 0.004f), _silkGold, rug, rug.position + Vector3.up * 0.006f, Quaternion.identity);
                Piece("RugField", RoundedBox(new Vector3(4.0f, 0.014f, 3.2f), 0.004f), Mat("LTB_RugRed", new Color(0.32f, 0.07f, 0.06f), 0.12f), rug, rug.position + Vector3.up * 0.008f, Quaternion.identity);
                LiteProp(PropTableSet, 60000, new Vector3(x, Floor + 0.014f, -14f), 0f, 1f, room, true, "HallTable");
                Armchair("HostChair", room, new Vector3(x - 1.55f, Floor + 0.014f, -14f), 90f, Lacquer, _silkRed);
                Armchair("HostChair", room, new Vector3(x + 1.55f, Floor + 0.014f, -14f), -90f, Lacquer, _silkRed);
            }
            // Tea tables for guests waiting by the open front, clear of the walk along the axis and the night round.
            foreach (float x in new[] { -3.3f, 3.3f })
            {
                var table = Table("GuestTable", room, new Vector3(x, Floor, -11.2f), 0f, 0.9f, 0.9f, 0.8f, Lacquer, true, true);
                TeaSet(table, table.TransformPoint(new Vector3(0f, 0.8f, 0f)), x * 10f, _celadon);
                Armchair("GuestChair", room, new Vector3(x - 0.78f, Floor, -11.2f), 90f, Lacquer, _silkRed);
                Armchair("GuestChair", room, new Vector3(x + 0.78f, Floor, -11.2f), -90f, Lacquer, _silkRed);
            }
            // Curtains tied back to the pillars in the front bays (the axis bay stays open): they frame the hall from the
            // yard without hiding anyone (no collider: they must not look like cover the guards cannot see through).
            var curtain = Mat("LTB_SilkCurtain", new Color(0.36f, 0.05f, 0.045f), 0.38f);
            foreach (var (from, to) in new[] { (-9f, -5f), (-5f, -1.6f), (1.6f, 5f), (5f, 9f) })
            {
                PleatedCurtain("BayCurtain", new Vector3(from + 0.55f, Floor + 2.9f, -9.3f), 1f, 2.78f, 0f, curtain, room, 0.55f, false);
                PleatedCurtain("BayCurtain", new Vector3(to - 0.55f, Floor + 2.9f, -9.3f), 1f, 2.78f, 180f, curtain, room, 0.55f, false);
            }
            var stage = Frame("Stage", room, new Vector3(18.6f, Floor, -14f), 90f);
            Wood("Platform", stage, new Vector3(0f, 0.075f, 0f), new Vector3(3.6f, 0.15f, 2.2f), Lacquer, 0.02f);
            Solid(stage, new Vector3(0f, 0.075f, 0f), new Vector3(3.6f, 0.15f, 2.2f));
            Covering(Piece("StageMat", RoundedBox(new Vector3(3.2f, 0.012f, 1.8f), 0.004f), Mat("LTB_RugRed", new Color(0.32f, 0.07f, 0.06f), 0.12f), stage, stage.TransformPoint(new Vector3(0f, 0.156f, 0f)), stage.rotation).transform, Surface.Mat);
            StoryPropKit.DrumStand(stage.TransformPoint(new Vector3(1.1f, 0.15f, -0.4f)), 290f, room, false);
            MoonLute(stage, stage.TransformPoint(new Vector3(-0.6f, 0.17f, 0.1f)), 160f);
            foreach (float x in new[] { -0.9f, 0.3f })
                Piece("FloorCushion", RoundedBox(new Vector3(0.48f, 0.08f, 0.48f), 0.035f, 0.02f), _silkRed, stage, stage.TransformPoint(new Vector3(x, 0.2f, 0.45f)), stage.rotation);
            StoryPropKit.Scroll(new Vector3(20.82f, Floor + 1.9f, -14f), -90f, room, 6, 0.7f, 1.5f);
            // Display tables between the high windows, each with a vase and a small potted palm.
            foreach (float x in new[] { -11.5f, 11.5f })
            {
                var console = Table("Console", room, new Vector3(x, Floor, -18.6f), 0f, 1.5f, 0.45f, 0.85f, Lacquer, true, true);
                Vase(console, console.TransformPoint(new Vector3(-0.4f, 0.85f, 0f)), 1.0f, _celadon);
                Piece("Bowl", BowlMesh, _brass, console, console.TransformPoint(new Vector3(0.35f, 0.85f, 0f)), console.rotation, Vector3.one * 1.6f);
            }
            Screen("HallScreenW", new Vector3(-3.6f, Floor, -17.4f), 0f, room, new Color(0.62f, 0.45f, 0.26f));
            Screen("HallScreenE", new Vector3(3.6f, Floor, -17.4f), 0f, room, new Color(0.62f, 0.45f, 0.26f));
            StoryArtKit.Couplets(new Vector3(-1.6f, Floor + 1.9f, -8.93f), new Vector3(1.6f, Floor + 1.9f, -8.93f), 180f, room);
            StoryPropKit.Stool(new Vector3(2.4f, Floor, -18.1f), 200f, room);
            // The axis of the house: a lacquered board with a gold border over the gate, tall porcelain vases either side,
            // a red runner from the gate to the yard steps.
            var board = Frame("HoanhPhi", room, new Vector3(0f, Floor + 2.82f, -18.86f), 0f);
            Wood("Border", board, Vector3.zero, new Vector3(2.5f, 0.56f, 0.06f), _silkGold, 0.015f);
            Wood("Field", board, new Vector3(0f, 0f, 0.012f), new Vector3(2.36f, 0.44f, 0.05f), Lacquer, 0.01f);
            foreach (float x in new[] { -2.5f, 2.5f }) Vase(room, new Vector3(x, Floor, -18.5f), 3.1f, _porcelain, x * 30f);
            Covering(Piece("Runner", RoundedBox(new Vector3(1.4f, 0.012f, 9.4f), 0.004f), Mat("LTB_RugRed", new Color(0.32f, 0.07f, 0.06f), 0.12f), room, new Vector3(0f, Floor + 0.006f, -13.9f), Quaternion.identity).transform, Surface.Mat);
            // The gate's bar and chain: the way out everyone can see and nobody uses.
            Box("GateBar", new Vector3(-1.8f, Floor + 1.3f, -18.86f), new Vector3(1.8f, Floor + 1.48f, -18.7f), Timber, room, false);
            Box("GateChain", new Vector3(-0.08f, Floor + 0.9f, -18.84f), new Vector3(0.08f, Floor + 1.7f, -18.76f), Iron, room, false);
            _refs.HallSpot = Point("HallSpot", new Vector3(-2.6f, Floor, -12.5f), 0f, room);
        }

        // ------------------------------------------------------------------ rear range

        private static void Corridor(Transform room)
        {
            // The 3 m service corridor: glazed water jars against the screen (cover from the yard and along the corridor),
            // benches by the walls, bamboo blinds let down over two bays of the screen (they break the long view down the
            // corridor and block eyes as they block light), lantern hooks dark until the house wakes.
            foreach (var (x, yaw, scale) in new[] { (-16.5f, 0f, 0.95f), (-6.5f, 180f, 0.85f), (4.5f, 10f, 0.9f), (14.5f, 175f, 0.95f) })
                LiteProp(PropJars, 30000, new Vector3(x, Floor, 8.62f), yaw, scale, room, true, "CorridorJars");
            foreach (float x in new[] { -3.0f, 17.6f })
            {
                var bench = Frame("Bench", room, new Vector3(x, Floor, 10.62f), 0f);
                Wood("Seat", bench, new Vector3(0f, 0.4f, 0f), new Vector3(1.4f, 0.05f, 0.38f), _woodWorn, 0.012f);
                foreach (float leg in new[] { -0.6f, 0.6f }) Wood("Leg", bench, new Vector3(leg, 0.19f, 0f), new Vector3(0.06f, 0.38f, 0.32f), _woodWorn, 0.008f);
                Solid(bench, new Vector3(0f, 0.21f, 0f), new Vector3(1.4f, 0.42f, 0.38f));
            }
            foreach (float x in new[] { -4.8f, 4.8f }) BambooBlind("Blind", new Vector3(x, Floor + 2.5f, 8.0f), 2.6f, 1.45f, room);
            // Lantern hooks along the corridor: dark tonight until the house wakes (J2 lights them).
            foreach (float x in new[] { -15f, -8f, 7.5f, 15f })
                _refs.CorridorLanterns.Add(HouseLantern("CorridorLantern", new Vector3(x, Floor + 2.2f, 9.6f), room, 5.5f, 3.2f));
        }

        private static void Kitchen(Transform room)
        {
            float y = 0.12f;
            // Earth, soot and smoke: the clay stove with two fire mouths and iron pots in the north-west corner under its
            // smoke hood, the wall above it smoked; Mã Kiều's mat by the warm stove; the work table, shelves of bowls and
            // crocks, firewood by the back door, water jars, strings of garlic and chillies from the beams.
            var clay = StoryArtKit.Surface("LTB_ClayStove", "brown_mud_dry", new Color(0.5f, 0.36f, 0.27f), 1.1f, 0.06f, 1f, 0.5f, 0.8f, Folder);
            var stove = Frame("Stove", room, new Vector3(-18.4f, y, 18.2f), 0f);
            Wood("StoveBody", stove, new Vector3(0f, 0.42f, 0f), new Vector3(1.5f, 0.84f, 1.15f), clay, 0.06f);
            Solid(stove, new Vector3(0f, 0.42f, 0f), new Vector3(1.5f, 0.84f, 1.15f));
            foreach (float x in new[] { -0.38f, 0.38f })
            {
                Wood("FireMouth", stove, new Vector3(x, 0.32f, -0.575f), new Vector3(0.32f, 0.3f, 0.02f), _soot, 0.03f);
                Wood("Embers", stove, new Vector3(x, 0.22f, -0.57f), new Vector3(0.24f, 0.07f, 0.02f), _ember, 0.01f);
                Piece("Pot", PotMesh, _ironPot, stove, stove.TransformPoint(new Vector3(x, 0.84f, 0.05f)), stove.rotation, Vector3.one * (x < 0 ? 1.1f : 0.85f));
            }
            Piece("Lid", Lathe("PotLid", new[] { new Vector2(0.25f, 0f), new Vector2(0.2f, 0.05f), new Vector2(0.04f, 0.08f), new Vector2(0.03f, 0.11f), new Vector2(0f, 0.11f) }, 20), _woodWorn, stove,
                stove.TransformPoint(new Vector3(-0.38f, 0.84f + 0.26f, 0.05f)), stove.rotation);
            // A tapered plaster hood (wide over the pots, narrowing to the flue), blackened by years of smoke.
            Piece("Hood", Lathe("SmokeHood", new[] { new Vector2(1f, 0f), new Vector2(0.92f, 0.08f), new Vector2(0.45f, 0.55f), new Vector2(0.32f, 0.62f), new Vector2(0.32f, 1f) }, 4, capBottom: true), _soot, stove,
                stove.TransformPoint(new Vector3(0f, 1.75f, 0.12f)), stove.rotation * Quaternion.Euler(0f, 45f, 0f), new Vector3(0.95f, 1.15f, 0.7f));
            var smokedPlaster = StoryArtKit.Surface("LTB_SmokedPlaster", "plastered_wall_02", new Color(0.34f, 0.29f, 0.24f), 2f, 0.04f, 1.2f, 0.8f, 1f, Folder);
            Box("SmokedWallN", new Vector3(-19.7f, y + 0.85f, 18.83f), new Vector3(-17.1f, y + 2.85f, 18.84f), smokedPlaster, room, false);
            _refs.Stove = Point("Stove", new Vector3(-18.4f, y, 17.2f), 0f, room);
            var glow = new GameObject("StoveGlow");
            glow.transform.SetParent(room, false);
            glow.transform.position = new Vector3(-18.4f, y + 0.4f, 17.1f);
            var light = glow.AddComponent<Light>();
            light.type = LightType.Point; light.color = new Color(1f, 0.42f, 0.16f); light.range = 5.5f; light.intensity = _night ? 1.5f : 0.5f;
            StoryPropKit.Woodpile(new Vector3(-20.3f, y, 17.0f), 90f, 1.6f, room, 31);
            LiteProp(PropJars, 30000, new Vector3(-11.8f, y, 18.15f), 90f, 0.85f, room, true, "KitchenJars");
            Shelves("KitchenShelves", room, new Vector3(-16.4f, y, 11.25f), 0f, 1.4f, 1.7f, 4, _woodWorn, 33, true);
            StoryPropKit.Strings(new Vector3(-15.5f, y + 2.9f, 14.5f), room, 34);
            StoryPropKit.Strings(new Vector3(-13.0f, y + 2.9f, 16.0f), room, 35);
            StoryPropKit.Sacks(new Vector3(-12.3f, y, 12.2f), 3, room, 36);
            var work = Table("WorkTable", room, new Vector3(-17.5f, y, 13.0f), 0f, 1.4f, 0.6f, 0.8f, _woodWorn, false, false);
            Piece("Bowl", BowlMesh, _terracotta, work, work.TransformPoint(new Vector3(-0.35f, 0.8f, 0f)), work.rotation, Vector3.one * 2f);
            Wood("Board", work, new Vector3(0.25f, 0.815f, 0.02f), new Vector3(0.45f, 0.03f, 0.3f), _woodWorn, 0.006f);
            Jar(work, work.TransformPoint(new Vector3(0.55f, 0f, -0.5f)), 0.55f, _terracotta, true);
            Piece("WaterBucket", Lathe("Bucket", new[] { new Vector2(0.15f, 0f), new Vector2(0.17f, 0.28f), new Vector2(0.16f, 0.28f), new Vector2(0.14f, 0.02f), new Vector2(0f, 0.02f) }, 20), _woodWorn, room, new Vector3(-20.2f, y, 15.9f), Quaternion.identity);
            // The great water vat (lu) in the corner by the back door with its coconut-shell dipper, and a low bench where the
            // vegetables are picked over on a winnowing tray (nia).
            Jar(room, new Vector3(-20.25f, y, 12.35f), 1.25f, _terracotta, true);
            Wood("VatLid", room, new Vector3(-20.25f, y + 0.83f, 12.35f), new Vector3(0.62f, 0.03f, 0.62f), _woodWorn, 0.01f);
            Piece("Dipper", Lathe("Dipper", new[] { new Vector2(0.07f, 0f), new Vector2(0.08f, 0.04f), new Vector2(0.07f, 0.06f), new Vector2(0f, 0.06f) }, 14), _woodWorn, room, new Vector3(-20.1f, y + 0.905f, 12.3f), Quaternion.Euler(180f, 0f, 0f));
            var bench = Table("LowBench", room, new Vector3(-13.7f, y, 16.6f), 0f, 0.9f, 0.32f, 0.36f, _woodWorn, false, false);
            Piece("Nia", Lathe("Nia", new[] { new Vector2(0.42f, 0f), new Vector2(0.44f, 0.035f), new Vector2(0.425f, 0.04f), new Vector2(0.4f, 0.008f), new Vector2(0f, 0.006f) }, 28), _reed, bench,
                bench.TransformPoint(new Vector3(0f, 0.36f, 0f)), bench.rotation);
            foreach (var (x, z) in new[] { (-0.12f, 0.05f), (0.1f, -0.08f), (0.02f, 0.14f), (0.2f, 0.1f) })
                Piece("Greens", Lathe("Greens", new[] { new Vector2(0.08f, 0f), new Vector2(0.07f, 0.035f), new Vector2(0.035f, 0.06f), new Vector2(0f, 0.065f) }, 10), Mat("LTB_Greens", new Color(0.22f, 0.36f, 0.14f), 0.3f), bench, bench.TransformPoint(new Vector3(x, 0.4f, z)), bench.rotation);
            // Mã Kiều's sleeping mat by the warm stove.
            Piece("KitchenMat", RoundedBox(new Vector3(2.0f, 0.03f, 1.0f), 0.01f), _reed, room, new Vector3(-15.6f, y + 0.015f, 17.6f), Quaternion.identity);
            _refs.Hides.Add(HideSpot("KitchenJars", new Vector3(-12.6f, y, 17.9f), 90f, new Vector3(-12.2f, y + 0.7f, 17.8f), new Vector3(1.6f, 1.4f, 1.4f),
                _player, "Nấp sau chum nước", "step", room, null, 50f).WithCrouch().WithKind(HideKind.Low));
            // A cloth hangs in the doorway to the store.
            PleatedCurtain("DoorCloth", new Vector3(Plan.KitchenStore, Floor + 2.1f, 15.2f), 1.2f, 1.5f, 90f, Mat("LTB_CottonBrown", new Color(0.34f, 0.28f, 0.22f), 0.08f), room);
        }

        private static void Store(Transform room)
        {
            // The cloth store: shelves of bolts along the north wall up to the beams, trunks, a drying rack with lengths of
            // cloth, a heap of bolts to crawl behind, dust in the air; the east wall's slats look onto Tú Bà's sập.
            var colours = new[] { new Color(0.6f, 0.14f, 0.12f), new Color(0.22f, 0.3f, 0.46f), new Color(0.78f, 0.62f, 0.4f), new Color(0.3f, 0.42f, 0.26f), new Color(0.82f, 0.52f, 0.58f) };
            for (int shelf = 0; shelf < 4; shelf++)
            {
                float y = Floor + 0.35f + shelf * 0.62f;
                Wood("Shelf", room, new Vector3(-8.0f, y - 0.015f, 18.5f), new Vector3(5.0f, 0.03f, 0.65f), _woodDusty, 0.006f);
                for (int i = 0; i < 12; i++)
                {
                    var bolt = Cylinder("Bolt", new Vector3(-10.25f + i * 0.4f, y, 18.5f), 0.14f, 0.55f,
                        Mat("LTB_Cloth" + ((i + shelf) % colours.Length), colours[(i + shelf) % colours.Length], 0.3f), room, false);
                    bolt.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                    bolt.transform.position = new Vector3(-10.25f + i * 0.4f, y + 0.15f, 18.5f);
                }
            }
            Box("ShelfBlock", new Vector3(-10.55f, Floor, 18.15f), new Vector3(-5.45f, Floor + 2.7f, 18.85f), Timber, room).GetComponent<Renderer>().enabled = false;
            foreach (float x in new[] { -10.5f, -8.0f, -5.5f }) Wood("ShelfPost", room, new Vector3(x, Floor + 1.35f, 18.5f), new Vector3(0.06f, 2.7f, 0.65f), _woodWorn, 0.008f);
            var lower = Trunk("StoreTrunk", room, new Vector3(-9.9f, Floor, 12.0f), 0f, 0.9f, 0.55f, 0.55f, true);
            Trunk("StoreTrunk", lower, lower.TransformPoint(new Vector3(0.05f, 0.55f, 0f)), 10f, 0.75f, 0.45f, 0.5f, true);
            Trunk("StoreTrunk", room, new Vector3(-8.7f, Floor, 11.9f), -8f, 0.8f, 0.5f, 0.5f, true);
            // Drying rack with lengths of cloth.
            var rack = Frame("DryingRack", room, new Vector3(-5.2f, Floor, 17.4f), 0f);
            foreach (float x in new[] { -0.9f, 0.9f }) Wood("Post", rack, new Vector3(x, 0.9f, 0f), new Vector3(0.05f, 1.8f, 0.05f), _woodWorn, 0.006f);
            Wood("Pole", rack, new Vector3(0f, 1.78f, 0f), new Vector3(1.9f, 0.04f, 0.04f), Bamboo, 0.015f);
            for (int i = 0; i < 3; i++)
                PleatedCurtain("ClothLength", rack.TransformPoint(new Vector3(-0.6f + i * 0.6f, 1.77f, 0f)), 0.5f, 1.3f + i * 0.1f, 0f, Mat("LTB_Cloth" + (i + 1), colours[i + 1], 0.3f), room, 0.1f, false);
            Solid(rack, new Vector3(0f, 0.9f, 0f), new Vector3(1.9f, 1.8f, 0.3f));
            StoryArtKit.Dust("StoreDust", new Vector3(-7f, Floor + 1.4f, 15f), new Vector3(7.4f, 2.4f, 7.4f), room, new Color(0.9f, 0.82f, 0.68f, 0.35f), 80);
            for (int i = 0; i < 6; i++)
            {
                var bolt = Cylinder("HeapBolt", Vector3.zero, 0.16f, 1.1f, Mat("LTB_Cloth" + (i % colours.Length), colours[i % colours.Length], 0.3f), room, i < 3);
                bolt.transform.rotation = Quaternion.Euler(0f, 0f, 90f) * Quaternion.Euler(0f, i * 17f, 0f);
                bolt.transform.position = new Vector3(-8.4f + (i % 3) * 0.05f, Floor + 0.17f + (i / 3) * 0.3f, 15.6f + (i % 3) * 0.32f);
            }
            _refs.Hides.Add(HideSpot("ClothHeap", new Vector3(-9.5f, Floor, 16.0f), 90f, new Vector3(-8.7f, Floor + 0.6f, 16.0f), new Vector3(1.8f, 1.2f, 1.4f),
                _player, "Nấp sau đống vải", "cloth", room, null, 50f).WithCrouch().WithKind(HideKind.Low));
        }

        private static void TuBaRoom(Transform room)
        {
            // The room of someone with money and say (11 × 8 m), the reference room of the house: the lacquered sập against
            // the store's slats (watched from the dark next door) with the lamp at its foot; a red rug with a gold border;
            // the tea table and cushions in the middle; the console table with incense, vases and a scroll on the north
            // wall; the canopy bed in the north-east corner; the carved cabinet by the windows; her account desk.
            // Paths kept clear: door A → the foot of the sập, and door A → door B across the middle.
            Sap("Sap", room, new Vector3(-1.9f, Floor, 15.4f), 0f, 2.1f, 1.15f, true);
            _refs.SapFeet = Point("SapFeet", new Vector3(-1.9f, Floor, 14.55f), 0f, room);
            var rug = Frame("Rug", room, new Vector3(0.7f, Floor, 15.2f), 0f);
            Covering(rug, Surface.Mat);
            Piece("RugBorder", RoundedBox(new Vector3(6.4f, 0.012f, 4.6f), 0.004f), _silkGold, rug, rug.position + Vector3.up * 0.006f, Quaternion.identity);
            Piece("RugField", RoundedBox(new Vector3(6.2f, 0.014f, 4.4f), 0.004f), Mat("LTB_RugRed", new Color(0.32f, 0.07f, 0.06f), 0.12f), rug, rug.position + Vector3.up * 0.008f, Quaternion.identity);
            var tea = TeaTable("TeaTable", room, new Vector3(1.6f, Floor + 0.014f, 15.6f), 0f, _celadon);
            foreach (var offset in new[] { new Vector3(0f, 0.04f, -0.55f), new Vector3(0.65f, 0.04f, 0f), new Vector3(-0.65f, 0.04f, 0f) })
                Cushion("FloorCushion", tea, offset, new Vector3(0.48f, 0.08f, 0.48f), _silkRed, 0.02f);
            LiteProp(PropOilLamp, 8000, tea.TransformPoint(new Vector3(-0.24f, 0.34f, 0.12f)), 0f, 1f, room, false, "OilLamp");
            // The console on the north wall: incense, a pair of vases, brass candlesticks, the scroll above.
            var console = Table("Console", room, new Vector3(1.6f, Floor, 18.55f), 0f, 1.7f, 0.45f, 0.88f, Lacquer, true, true);
            Piece("IncenseBowl", Lathe("IncenseBowl", new[] { new Vector2(0.06f, 0f), new Vector2(0.1f, 0.02f), new Vector2(0.11f, 0.09f), new Vector2(0.12f, 0.1f), new Vector2(0f, 0.08f) }, 20), _bronze, console, console.TransformPoint(new Vector3(0f, 0.88f, 0f)), console.rotation);
            foreach (float x in new[] { -0.55f, 0.55f })
            {
                Vase(console, console.TransformPoint(new Vector3(x, 0.88f, 0.02f)), 0.85f, x < 0 ? _celadon : _porcelain);
                Piece("Candlestick", Lathe("Candlestick", new[] { new Vector2(0.05f, 0f), new Vector2(0.05f, 0.015f), new Vector2(0.015f, 0.04f), new Vector2(0.012f, 0.2f), new Vector2(0.035f, 0.21f), new Vector2(0.035f, 0.225f), new Vector2(0f, 0.225f) }, 14),
                    _brass, console, console.TransformPoint(new Vector3(x * 0.55f, 0.88f, -0.1f)), console.rotation);
            }
            StoryPropKit.Scroll(new Vector3(1.6f, Floor + 1.85f, 18.82f), 180f, room, 7, 0.6f, 1.4f);
            StoryPropKit.IncenseBurner(new Vector3(-0.2f, Floor, 18.4f), room);
            Vase(room, new Vector3(-2.45f, Floor, 18.4f), 1.9f, _celadon);
            LiteProp(PropBed, 60000, new Vector3(5.2f, Floor, 18.2f), 180f, 1f, room, true, "CanopyBed");
            LiteProp(PropCabinet, 40000, new Vector3(7.45f, Floor, 12.6f), -90f, 1f, room, true, "TuBaCabinet");
            Vase(room, new Vector3(7.45f, Floor + 1.81f, 12.35f), 0.7f, _porcelain);
            Piece("Bowl", BowlMesh, _brass, room, new Vector3(7.45f, Floor + 1.81f, 12.9f), Quaternion.identity, Vector3.one * 1.4f);
            // Her account desk under the windows: ledgers, brush pot, abacus, the chair she sits in to count.
            var desk = Table("AccountDesk", room, new Vector3(3.6f, Floor, 12.2f), 0f, 1.3f, 0.6f, 0.78f, Lacquer, true, true);
            Books(desk, desk.TransformPoint(new Vector3(-0.35f, 0.78f, 0.05f)), 4f, 4, 77);
            Piece("BrushPot", Lathe("BrushPot", new[] { new Vector2(0.05f, 0f), new Vector2(0.055f, 0.13f), new Vector2(0.05f, 0.135f), new Vector2(0f, 0.12f) }, 16), _porcelain, desk, desk.TransformPoint(new Vector3(0.45f, 0.78f, 0.12f)), desk.rotation);
            StoryPropKit.Abacus(Group(room, "AccountDeskTop"), new Vector3(3.75f, Floor + 0.78f, 12.15f), 0f);
            Armchair("DeskChair", room, new Vector3(3.6f, Floor, 12.95f), 180f, Lacquer, _silkRed);
            Screen("TuBaScreen", new Vector3(1.4f, Floor, 12.5f), 0f, room, new Color(0.62f, 0.45f, 0.26f));
            _refs.TuBaLamp = Lamp("TuBaLamp", new Vector3(-0.6f, Floor, 14.0f), room, 4.8f, 1.15f);
            // By day and at dusk the room keeps two small lights of its own: candles on the altar console, a lamp by the bed.
            // At night (the key theft) the room is left to the one lamp above.
            if (!_night)
            {
                RoomLight("TuBaConsoleCandles", new Vector3(1.6f, Floor + 1.15f, 18.2f), new Color(1f, 0.6f, 0.3f), 3.2f, 1.3f, room);
                RoomLight("TuBaBedLamp", new Vector3(3.9f, Floor + 0.95f, 17.4f), new Color(1f, 0.55f, 0.28f), 2.8f, 1.1f, room);
            }
            // Hook where the key ring hangs by day (empty at night: she wears it).
            _refs.KeyHook = Block("KeyHook", new Vector3(-2.86f, Floor + 1.6f, 11.6f), new Vector3(0.06f, 0.05f, 0.05f), Iron, room, false);
        }

        private static void Office(Transform room)
        {
            // The ledger office in two parts. Work room (x 8–15): the clerk's carved desk with ledger, abacus, ink stone and
            // brush pot, his chair, shelves of account books along the north wall, document boxes by the window, the red
            // lacquer box on a side table by door B, a bench for whoever waits. Archive (x 15–21) behind the lattice: the
            // brass-locked cabinet, the covered bronze mirror, a wardrobe, storage shelves, a screen in the corner.
            var desk = Table("Desk", room, new Vector3(11.8f, Floor, 16.0f), 180f, 1.8f, 0.85f, 0.78f, Lacquer, true, true);
            Wood("DeskDrawer", desk, new Vector3(0f, 0.62f, 0.43f), new Vector3(0.6f, 0.12f, 0.02f), Timber, 0.004f);
            Piece("DrawerPull", BowlMesh, _brass, desk, desk.TransformPoint(new Vector3(0f, 0.62f, 0.445f)), desk.rotation * Quaternion.Euler(90f, 0f, 0f), Vector3.one * 0.3f);
            _refs.Ledger = Point("Ledger", new Vector3(11.8f, Floor, 15.0f), 0f, room);
            Wood("LedgerBook", room, new Vector3(12.08f, Floor + 0.805f, 15.97f), new Vector3(0.45f, 0.05f, 0.4f), Mat("LTB_LedgerCover", new Color(0.32f, 0.22f, 0.13f), 0.2f), 0.006f);
            StoryPropKit.Abacus(Group(room, "DeskTop"), new Vector3(11.35f, Floor + 0.78f, 16.02f), 180f);
            Piece("InkStone", RoundedBox(new Vector3(0.12f, 0.025f, 0.18f), 0.006f), _ink, room, new Vector3(12.55f, Floor + 0.79f, 16.07f), Quaternion.identity);
            Piece("BrushPot", Lathe("BrushPot", new[] { new Vector2(0.05f, 0f), new Vector2(0.055f, 0.13f), new Vector2(0.05f, 0.135f), new Vector2(0f, 0.12f) }, 16), _porcelain, room, new Vector3(12.55f, Floor + 0.78f, 15.77f), Quaternion.identity);
            for (int i = 0; i < 4; i++)
                Piece("Brush", Lathe("Brush", new[] { new Vector2(0.004f, 0f), new Vector2(0.004f, 0.22f) }, 6, capBottom: false), Timber, room,
                    new Vector3(12.54f + (i % 2) * 0.02f, Floor + 0.8f, 15.76f + (i / 2) * 0.02f), Quaternion.Euler(6f - i * 4f, i * 40f, 4f));
            Armchair("ClerkChair", room, new Vector3(11.8f, Floor, 16.85f), 180f, Timber, _cotton);
            _refs.OfficeLamp = Lamp("OfficeLamp", new Vector3(10.95f, Floor + 0.78f, 16.2f), room, 4.6f, 1.05f, 0.18f);
            foreach (float x in new[] { 9.7f, 11.7f, 13.7f }) StoryPropKit.Bookshelf(new Vector3(x, Floor, 18.75f), 180f, room, 71 + (int)x, 1.6f, 2.3f);
            // Document boxes stacked under the window.
            for (int i = 0; i < 3; i++)
                Wood("DocBox", room, new Vector3(14.55f, Floor + 0.13f + i * 0.25f, 11.6f + (i % 2) * 0.04f), new Vector3(0.36f, 0.24f, 0.5f), i == 1 ? Lacquer : Timber, 0.01f);
            Solid(room, new Vector3(14.55f, Floor + 0.38f, 11.62f), new Vector3(0.4f, 0.76f, 0.52f));
            // A bench for whoever waits to be paid or to pay, by the door from the corridor.
            var bench = Frame("WaitingBench", room, new Vector3(9.1f, Floor, 11.5f), 0f);
            Wood("Seat", bench, new Vector3(0f, 0.4f, 0f), new Vector3(1.3f, 0.05f, 0.36f), Timber, 0.012f);
            foreach (float leg in new[] { -0.55f, 0.55f }) Wood("Leg", bench, new Vector3(leg, 0.19f, 0f), new Vector3(0.06f, 0.38f, 0.3f), Timber, 0.008f);
            Solid(bench, new Vector3(0f, 0.21f, 0f), new Vector3(1.3f, 0.42f, 0.36f));
            // The lacquered box where the bill of sale is kept, on a side table by door B.
            var side = Table("SideTable", room, new Vector3(8.45f, Floor, 13.4f), 90f, 1.0f, 0.62f, 0.72f, Timber, true, false);
            Wood("LacquerBox", side, new Vector3(0f, 0.835f, 0f), new Vector3(0.5f, 0.23f, 0.4f), Mat("LTB_RedLacquerBox", new Color(0.36f, 0.05f, 0.04f), 0.42f), 0.012f);
            Wood("LacquerLid", side, new Vector3(0f, 0.96f, 0f), new Vector3(0.53f, 0.03f, 0.43f), Mat("LTB_RedLacquerBox", new Color(0.36f, 0.05f, 0.04f), 0.42f), 0.012f);
            _refs.LacquerBox = Point("LacquerBox", new Vector3(9.1f, Floor, 13.4f), 0f, room);
            StoryPropKit.Scroll(new Vector3(8.14f, Floor + 1.7f, 12.2f), 90f, room, 9);

            // Archive.
            ArchiveCabinetDoor = Cabinet(new Vector3(20.62f, Floor, 15.2f), room);
            _refs.Cabinet = Point("Cabinet", new Vector3(19.4f, Floor, 15.2f), 90f, room);
            MirrorStand(new Vector3(20.55f, Floor, 16.7f), -125f, room);
            // A small oil lamp left burning by the mirror: the only light back here, and the flame that leans before J1.
            _refs.ArchiveLamp = Lamp("ArchiveLamp", new Vector3(19.8f, Floor, 17.8f), room, 3.6f, 0.7f, 0.95f);
            var wardrobeDoor = Wardrobe("ArchiveWardrobe", new Vector3(16.1f, Floor, 18.45f), 180f, room);
            _refs.Hides.Add(HideSpot("ArchiveWardrobe", new Vector3(16.1f, Floor, 18.45f), 180f, new Vector3(16.1f, Floor + 1f, 18.35f), new Vector3(1.1f, 2f, 0.9f),
                _player, "Trốn vào tủ áo", "door", room, wardrobeDoor, 32f).WithKind(HideKind.Wardrobe));
            Shelves("ArchiveShelves", room, new Vector3(18.6f, Floor, 18.65f), 180f, 1.6f, 1.9f, 4, TimberDusty, 74, false);
            var archiveScreen = Screen("ArchiveScreen", new Vector3(17.6f, Floor, 12.2f), 0f, room, new Color(0.42f, 0.32f, 0.22f));
            _refs.Hides.Add(HideSpot("ArchiveScreen", new Vector3(17.6f, Floor, 11.75f), 0f, new Vector3(17.6f, Floor + 0.9f, 11.95f), new Vector3(1.8f, 1.8f, 0.8f),
                _player, "Nấp sau bình phong", "paper", room, archiveScreen, 45f).WithCrouch().WithKind(HideKind.Screen));
        }

        private static void Yard(Transform room)
        {
            // The 18 × 14 m courtyard: a frangipani (cây đại) off-centre in a round stone bed as the main tree, the stone
            // basin with its rockery, palms in glazed pots at the corners, rain jars and stone benches at the edges, and
            // open paving between them for the paths and the guards' lines of sight.
            var bed = Lathe("TreeBed", new[] { new Vector2(1.45f, 0f), new Vector2(1.5f, 0.05f), new Vector2(1.5f, 0.42f), new Vector2(1.42f, 0.48f), new Vector2(1.3f, 0.48f), new Vector2(1.28f, 0.4f), new Vector2(0f, 0.4f) }, 40);
            var bedGo = Piece("TreeBed", bed, Plinth, room, new Vector3(-3.2f, 0f, 2.2f), Quaternion.identity);
            // The bed stops bodies but not eyes (it is knee-high); the trunk stops both.
            Solid(bedGo.transform, new Vector3(0f, 0.24f, 0f), new Vector3(2.9f, 0.48f, 2.9f));
            Standing(bedGo.transform, Surface.Stone); // she can step up onto its stone rim
            var trunkCollider = new GameObject("TrunkCollider").AddComponent<CapsuleCollider>();
            trunkCollider.transform.SetParent(room, false);
            trunkCollider.transform.position = new Vector3(-3.2f, 1.9f, 2.2f);
            trunkCollider.radius = 0.22f; trunkCollider.height = 3f;
            trunkCollider.gameObject.layer = EnvironmentLayer;
            Piece("TreeSoil", Lathe("TreeSoil", new[] { new Vector2(1.29f, 0f), new Vector2(0f, 0.02f) }, 40, capBottom: false), _soot, room, new Vector3(-3.2f, 0.4f, 2.2f), Quaternion.identity);
            FrangipaniTree(room, new Vector3(-3.2f, 0.42f, 2.2f), 7);

            // The stone basin: a turned bowl of grey stone on a short foot, dark water, the rockery rising out of it.
            var basin = Lathe("StoneBasin", new[] { new Vector2(0.78f, 0f), new Vector2(0.8f, 0.08f), new Vector2(0.95f, 0.18f), new Vector2(1.06f, 0.42f),
                new Vector2(1.08f, 0.55f), new Vector2(0.98f, 0.56f), new Vector2(0.95f, 0.5f), new Vector2(0f, 0.5f) }, 40);
            var basinGo = Piece("Basin", basin, Plinth, room, new Vector3(4.6f, 0f, -2.4f), Quaternion.identity);
            var basinCollider = basinGo.AddComponent<CapsuleCollider>();
            basinCollider.radius = 1.06f; basinCollider.height = 1.2f; basinCollider.center = new Vector3(0f, 0.3f, 0f);
            basinGo.layer = EnvironmentLayer;
            Piece("BasinWater", Lathe("BasinWater", new[] { new Vector2(0.95f, 0f), new Vector2(0f, 0.001f) }, 40, capBottom: false), Mat("LTB_Water", new Color(0.05f, 0.07f, 0.07f), 0.95f), room, new Vector3(4.6f, 0.505f, -2.4f), Quaternion.identity);
            StoryArtKit.Rock(new Vector3(4.7f, 0.55f, -2.3f), new Vector3(0.7f, 1.2f, 0.6f), 20f, room, 0);
            StoryArtKit.Rock(new Vector3(4.3f, 0.55f, -2.6f), new Vector3(0.45f, 0.7f, 0.4f), 70f, room, 1);

            // Palms in glazed pots at the corners, trunks solid; rain jars under the eaves; stone benches.
            var glaze = Mat("LTB_GlazePot", new Color(0.16f, 0.24f, 0.26f), 0.7f);
            Planter(room, new Vector3(-8.2f, 0f, -5.2f), PropBanana, 0.22f, glaze);
            Planter(room, new Vector3(8.2f, 0f, 7.2f), PropBanana, 0.24f, _brownGlaze);
            Planter(room, new Vector3(8.2f, 0f, -5.2f), PropAreca, 0.2f, _celadon);
            Planter(room, new Vector3(-8.2f, 0f, 7.2f), PropAreca, 0.18f, glaze);
            LiteProp(PropJars, 30000, new Vector3(-8.25f, 0f, 4.4f), 90f, 0.62f, room, true, "RainJars");
            LiteProp(PropJars, 30000, new Vector3(8.25f, 0f, -2.2f), -90f, 0.62f, room, true, "RainJars");
            foreach (var (p, yaw) in new[] { (new Vector3(4.2f, 0f, 7.25f), 0f), (new Vector3(-5.2f, 0f, -5.45f), 0f) })
                StoneBench(room, p, yaw);

            // Open sky over the yard and the alley: there the moon (or the evening sun) lights her too, for the guards.
            foreach (var (centre, size) in new[] { (new Vector3(0f, 1.5f, 1f), new Vector3(18.4f, 3f, 14.4f)), (new Vector3(-22.9f, 1.5f, 0f), new Vector3(3.5f, 3f, 38.8f)) })
            {
                var sky = new GameObject("OpenSky").AddComponent<OpenSky>();
                sky.transform.SetParent(room, false);
                sky.transform.position = centre;
                Set(sky, "_size", size);
            }
            // Moss where the flagstones meet the kerbs and stay damp; a darker wet band where the eaves drip.
            var moss = StoryArtKit.Surface("LTB_Moss", "mossy_rock", new Color(0.26f, 0.32f, 0.2f), 0.8f, 0.08f, 0.6f, 0.5f, 0f, Folder);
            foreach (var (a, b) in new[] { (new Vector3(-8.95f, 0.004f, -5.9f), new Vector3(-8.65f, 0.012f, 7.9f)), (new Vector3(8.65f, 0.004f, -5.9f), new Vector3(8.95f, 0.012f, 7.9f)),
                         (new Vector3(-8.9f, 0.004f, 7.6f), new Vector3(8.9f, 0.012f, 7.95f)), (new Vector3(-8.9f, 0.004f, -5.95f), new Vector3(8.9f, 0.012f, -5.7f)) })
                Box("MossEdge", a, b, moss, room, false);
            // Where the upper eaves drip: the stone is wettest on the drip line and dries out across the yard and back towards
            // the kerb; the veranda boards are damp at their outer edge where the rain blows in. Soft bands, no rectangles.
            var wet = WetMaterial();
            foreach (var (a, b, toYard) in new[] { (new Vector3(-8.5f, 0.006f, -5.55f), new Vector3(-8.5f, 0.006f, 7.5f), Vector3.right), (new Vector3(8.5f, 0.006f, 7.5f), new Vector3(8.5f, 0.006f, -5.55f), Vector3.left),
                         (new Vector3(8.5f, 0.006f, 7.5f), new Vector3(-8.5f, 0.006f, 7.5f), Vector3.back), (new Vector3(-8.5f, 0.006f, -5.55f), new Vector3(8.5f, 0.006f, -5.55f), Vector3.forward) })
            {
                WetBand("DripLine", a, b, toYard, 1.1f, wet, room);
                WetBand("DripLine", a, b, -toYard, 0.4f, wet, room);
            }
            foreach (var (a, b, inward) in new[] { (new Vector3(-9.12f, Floor + 0.004f, -6.1f), new Vector3(-9.12f, Floor + 0.004f, 7.9f), Vector3.left),
                         (new Vector3(9.12f, Floor + 0.004f, 7.9f), new Vector3(9.12f, Floor + 0.004f, -6.1f), Vector3.right),
                         (new Vector3(-9.0f, Floor + 0.004f, -6.12f), new Vector3(9.0f, Floor + 0.004f, -6.12f), Vector3.back) })
                WetBand("DampBoards", a, b, inward, _night ? 0.75f : 0.4f, WetMaterial(true), room);
            foreach (var (p, s, yaw) in new[] { (new Vector3(-0.8f, 0.005f, -3.4f), new Vector2(1.6f, 1f), 20f), (new Vector3(1.6f, 0.005f, 4.6f), new Vector2(1.2f, 0.8f), -35f),
                         (new Vector3(6.2f, 0.005f, 2.0f), new Vector2(0.9f, 0.7f), 60f), (new Vector3(-22.9f, 0.005f, 6.0f), new Vector2(1.4f, 0.9f), 10f),
                         (new Vector3(-22.8f, 0.005f, -8.0f), new Vector2(1.1f, 0.8f), -20f), (new Vector3(-23.0f, 0.005f, 14.5f), new Vector2(1.2f, 0.8f), 40f) })
                Puddle(room, p, s, yaw);
            // Small tea tables in the wider veranda bays, against the railing: a girl and a guest sit here between rooms.
            // They stand clear of the walk along the veranda (the night round keeps to x = ±10.5).
            foreach (var (x, z) in new[] { (-9.62f, -1.6f), (-9.62f, 3.7f), (9.62f, -1.6f), (9.62f, 3.7f) })
            {
                var table = Table("VerandaTable", room, new Vector3(x, Floor, z), 0f, 0.56f, 0.56f, 0.7f, Lacquer, false, false);
                TeaSet(table, table.TransformPoint(new Vector3(0f, 0.7f, 0f)), z * 20f, _brownGlaze);
                foreach (float s in new[] { -1f, 1f }) StoryPropKit.Stool(new Vector3(x, Floor, z + s * 0.58f), s > 0f ? 180f : 0f, room);
            }
            // Curtains let down in some veranda bays and tied back to the pillars (gathered narrow: they frame the yard,
            // they do not pretend to hide anyone).
            var bayCurtain = Mat("LTB_SilkCurtain", new Color(0.36f, 0.05f, 0.045f), 0.38f);
            foreach (float s in new[] { -1f, 1f })
                // The end bays only, and on the west side not the one outside Kiều's door: none hangs in the line from her
                // door out to the yard.
                foreach (var (from, to) in s < 0f ? new[] { (-8.9f, -6.0f) } : new[] { (-8.9f, -6.0f), (5.1f, 7.9f) })
                {
                    float x = s * 9.3f;
                    PleatedCurtain("VerandaCurtain", new Vector3(x, GalleryBeamTop - 0.26f, from + 0.5f), 0.9f, 2.5f, -90f, bayCurtain, room, 0.6f, false);
                    PleatedCurtain("VerandaCurtain", new Vector3(x, GalleryBeamTop - 0.26f, to - 0.5f), 0.9f, 2.5f, 90f, bayCurtain, room, 0.6f, false);
                }
            // Silk lanterns under the veranda eaves: warm pools of light with darkness between them (six with shadows,
            // as before the house grew: shadowed lights are not multiplied with the floor area).
            foreach (var p in new[] { new Vector3(-9.6f, Floor + 2.15f, -4.5f), new Vector3(-9.6f, Floor + 2.15f, 3.7f), new Vector3(9.6f, Floor + 2.15f, -4.5f),
                         new Vector3(9.6f, Floor + 2.15f, 3.7f), new Vector3(-4.5f, Floor + 2.15f, -6.6f), new Vector3(4.5f, Floor + 2.15f, -6.6f) })
                _refs.YardLanterns.Add(HouseLantern("YardLantern", p, room, 6f, 3.4f, true));
            foreach (var p in new[] { new Vector3(-2.2f, Floor + 2.4f, -18.3f), new Vector3(2.2f, Floor + 2.4f, -18.3f) })
                _refs.YardLanterns.Add(HouseLantern("GateLantern", p, room, 5.5f, 3f));
            // Hung low over the tables, not under the ceiling: the light falls on chairs, tables and faces, and the ceiling
            // and back wall get only what spills up.
            foreach (var p in new[] { new Vector3(-7f, Floor + 2.0f, -14f), new Vector3(7f, Floor + 2.0f, -14f), new Vector3(-17.5f, Floor + 2.05f, -11.5f), new Vector3(17.5f, Floor + 2.05f, -11.5f) })
                _refs.YardLanterns.Add(HouseLantern("HallLantern", p, room, 6.5f, 3.6f));
            // Smaller lanterns over the tea tables by the front: the near layer of the hall seen from the yard.
            foreach (var p in new[] { new Vector3(-3.3f, Floor + 1.95f, -11.2f), new Vector3(3.3f, Floor + 1.95f, -11.2f) })
                _refs.YardLanterns.Add(HouseLantern("HallTeaLantern", p, room, 4.5f, 2.4f));
        }

        // ------------------------------------------------------------------ furniture builders

        /// <summary>Tall wardrobe, hollow inside, doors with a band of slats at eye height to peek through. Returns the door to rattle.</summary>
        private static Transform Wardrobe(string name, Vector3 floorCentre, float yaw, Transform parent)
        {
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.SetPositionAndRotation(floorCentre, Quaternion.Euler(0f, yaw, 0f));
            // Local +Z is the front. Inside: 1.0 wide, 0.66 deep, 2.05 tall.
            Local(root, "Back", new Vector3(0f, 1.05f, -0.36f), new Vector3(1.12f, 2.1f, 0.05f), Lacquer, true);
            Local(root, "SideL", new Vector3(-0.54f, 1.05f, 0f), new Vector3(0.05f, 2.1f, 0.74f), Lacquer, true);
            Local(root, "SideR", new Vector3(0.54f, 1.05f, 0f), new Vector3(0.05f, 2.1f, 0.74f), Lacquer, true);
            Local(root, "Top", new Vector3(0f, 2.12f, 0f), new Vector3(1.16f, 0.06f, 0.78f), Timber, true);
            Local(root, "Base", new Vector3(0f, 0.02f, 0f), new Vector3(1.12f, 0.04f, 0.74f), Timber, false);
            var door = new GameObject("Doors").transform;
            door.SetParent(root, false);
            door.localPosition = new Vector3(0f, 0f, 0.36f);
            Local(door, "Lower", new Vector3(0f, 0.68f, 0f), new Vector3(1.08f, 1.3f, 0.04f), Lacquer, true);
            Local(door, "Upper", new Vector3(0f, 1.97f, 0f), new Vector3(1.08f, 0.26f, 0.04f), Lacquer, true);
            for (float y = 1.36f; y < 1.83f; y += 0.06f) Local(door, "Louvre", new Vector3(0f, y, 0f), new Vector3(1.08f, 0.022f, 0.035f), Timber, false);
            Local(door, "Seam", new Vector3(0f, 1.05f, 0.022f), new Vector3(0.012f, 2.0f, 0.01f), Ridge, false);
            Local(door, "Pull", new Vector3(-0.06f, 1.0f, 0.03f), new Vector3(0.03f, 0.1f, 0.02f), EmissiveMat("LTB_Brass", new Color(0.72f, 0.52f, 0.24f), 0.15f), false);
            Local(door, "Pull", new Vector3(0.06f, 1.0f, 0.03f), new Vector3(0.03f, 0.1f, 0.02f), EmissiveMat("LTB_Brass", new Color(0.72f, 0.52f, 0.24f), 0.15f), false);
            // The louvre band stops bodies, not eyes.
            var band = new GameObject("LouvreBlock");
            band.transform.SetParent(door, false);
            band.layer = 2;
            band.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            band.AddComponent<BoxCollider>().size = new Vector3(1.08f, 0.5f, 0.04f);
            return door;
        }

        /// <summary>The cabinet the Mã–Tú Bà contract is locked in: lattice doors (papers glimpsed inside) and a brass lock.</summary>
        private static Transform Cabinet(Vector3 floorCentre, Transform parent)
        {
            var root = new GameObject("BrassCabinet").transform;
            root.SetParent(parent, false);
            root.SetPositionAndRotation(floorCentre, Quaternion.Euler(0f, -90f, 0f)); // front faces −X
            Local(root, "Body", new Vector3(0f, 0.95f, -0.05f), new Vector3(1.3f, 1.9f, 0.55f), Lacquer, true);
            var door = new GameObject("Door").transform;
            door.SetParent(root, false);
            door.localPosition = new Vector3(-0.62f, 0f, 0.24f);
            Local(door, "Panel", new Vector3(0.62f, 1.25f, 0f), new Vector3(1.24f, 1.05f, 0.03f), Timber, false);
            for (int i = 0; i < 7; i++) Local(door, "Lattice", new Vector3(0.15f + i * 0.16f, 1.25f, 0.02f), new Vector3(0.02f, 1.0f, 0.02f), Lacquer, false);
            Local(door, "Lock", new Vector3(1.16f, 1.05f, 0.04f), new Vector3(0.1f, 0.14f, 0.03f), EmissiveMat("LTB_Brass", new Color(0.72f, 0.52f, 0.24f), 0.15f), false);
            // Papers tied with red string inside, visible through the lattice.
            Local(root, "Papers", new Vector3(0.1f, 1.08f, 0f), new Vector3(0.36f, 0.08f, 0.28f), Mat("LTB_Letter", new Color(0.86f, 0.8f, 0.66f), 0.05f), false);
            Local(root, "RedString", new Vector3(0.1f, 1.125f, 0f), new Vector3(0.02f, 0.01f, 0.29f), Mat("LTB_RedString", new Color(0.7f, 0.06f, 0.05f), 0.3f), false);
            Local(root, "SilverBag", new Vector3(-0.3f, 1.12f, 0f), new Vector3(0.3f, 0.2f, 0.24f), Mat("LTB_Sackcloth", new Color(0.45f, 0.38f, 0.28f), 0.2f), false);
            return door;
        }

        /// <summary>A bronze mirror on a stand, covered with a cloth (the J1 mirror). The reflection is real but hazy.</summary>
        private static void MirrorStand(Vector3 floorCentre, float yaw, Transform parent)
        {
            var root = new GameObject("BronzeMirror").transform;
            root.SetParent(parent, false);
            root.SetPositionAndRotation(floorCentre, Quaternion.Euler(0f, yaw, 0f));
            Local(root, "Foot", new Vector3(0f, 0.05f, 0f), new Vector3(0.5f, 0.1f, 0.35f), Timber, true);
            Local(root, "Stem", new Vector3(0f, 0.75f, 0f), new Vector3(0.07f, 1.4f, 0.07f), Timber, false);
            // A round brass rim behind the disc, and a little crossbar the mirror hangs from.
            var rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rim.name = "Rim";
            Object.DestroyImmediate(rim.GetComponent<Collider>());
            rim.transform.SetParent(root, false);
            rim.transform.localPosition = new Vector3(0f, 1.45f, 0.025f);
            rim.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            rim.transform.localScale = new Vector3(0.67f, 0.02f, 0.67f);
            rim.GetComponent<Renderer>().sharedMaterial = EmissiveMat("LTB_Brass", new Color(0.72f, 0.52f, 0.24f), 0.15f);
            Local(root, "Crossbar", new Vector3(0f, 1.82f, 0f), new Vector3(0.5f, 0.05f, 0.05f), Timber, false);
            // A thin disc of polished bronze; the reflection plane faces into the room (+Z) like the vanity's.
            var face = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            face.name = "MirrorFace";
            Object.DestroyImmediate(face.GetComponent<Collider>());
            face.transform.SetParent(root, false);
            face.transform.localPosition = new Vector3(0f, 1.45f, 0.05f);
            face.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            face.transform.localScale = new Vector3(0.58f, 0.004f, 0.58f);
            // Its own bronze: the shared mirror's milky haze is a fixed colour that would glow in a dark room and drown the
            // reflection, so this one is barely hazed and the haze is dark.
            const string bronzePath = Folder + "/LTB_BronzeMirror.mat";
            // Updated in place (never recreated): the other chapter's scene already points at this asset.
            var bronze = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(bronzePath);
            if (bronze == null) { bronze = new Material(Shader.Find("ThuyKieu/StoryMirror")); UnityEditor.AssetDatabase.CreateAsset(bronze, bronzePath); }
            bronze.SetColor("_BaseColor", new Color(0.9f, 0.74f, 0.52f));
            bronze.SetColor("_HazeColor", new Color(0.16f, 0.12f, 0.07f));
            bronze.SetFloat("_Haze", 0.22f);
            UnityEditor.EditorUtility.SetDirty(bronze);
            face.GetComponent<Renderer>().sharedMaterial = bronze;
            var plane = new GameObject("MirrorPlane").transform;
            plane.SetParent(root, false);
            plane.localPosition = new Vector3(0f, 1.45f, 0.055f);
            plane.localRotation = Quaternion.identity;
            var mirror = plane.gameObject.AddComponent<PlanarMirror>();
            Set(mirror, "_surface", face.GetComponent<Renderer>());
            Set(mirror, "_resolutionScale", 0.5f);
            Set(mirror, "_maxDistance", 7f);
            _refs.MirrorFace = plane;
            // The cloth: hangs over the face; Chapter 4 slips it and lets it fall.
            var cloth = new GameObject("MirrorCloth").transform;
            cloth.SetParent(root, false);
            cloth.localPosition = new Vector3(0f, 1.5f, 0.07f);
            Local(cloth, "Cloth", new Vector3(0f, -0.1f, 0f), new Vector3(0.72f, 0.82f, 0.012f), Mat("LTB_ClothGrey", new Color(0.38f, 0.34f, 0.3f), 0.15f), false);
            Local(cloth, "Fold", new Vector3(0f, 0.31f, -0.03f), new Vector3(0.74f, 0.06f, 0.07f), Mat("LTB_ClothGrey", new Color(0.38f, 0.34f, 0.3f), 0.15f), false);
            _refs.MirrorCloth = cloth;
        }

        private static void PriceSlip(Vector3 position, float yaw, Transform parent)
        {
            var slip = Block("PriceSlip", position, new Vector3(0.09f, 0.004f, 0.2f), Mat("LTB_SlipRed", new Color(0.75f, 0.12f, 0.08f), 0.1f), parent, false, yaw);
            slip.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        private static LampFlame Lamp(string name, Vector3 position, Transform parent, float range, float intensity, float standHeight = 1.1f)
        {
            // Lamps carry the rooms at night: a warm pool that reaches the furniture around them, darkness beyond.
            // (Above 1.5 the guards' light reading is already saturated: the extra is for the eye only.)
            var stand = StoryPropKit.Candle(position, parent, range + 1f, intensity * (_night ? 2.4f : 1.5f), standHeight);
            stand.name = name;
            // The candle kit's parts stand in for the flame and light; what is seen is a turned lamp stand with the
            // project's oil lamp (lighter copy) on its tray.
            foreach (string part in new[] { "Base", "Stem", "Dish", "Candle" })
            {
                var old = stand.transform.Find(part);
                if (old != null) Object.DestroyImmediate(old.gameObject);
            }
            if (standHeight > 0.3f)
                Piece("Stand", Lathe("LampStand", new[] { new Vector2(0.14f, 0f), new Vector2(0.14f, 0.03f), new Vector2(0.06f, 0.07f), new Vector2(0.03f, 0.12f),
                    new Vector2(0.022f, 0.5f), new Vector2(0.03f, 0.55f), new Vector2(0.022f, 0.6f), new Vector2(0.025f, 0.97f), new Vector2(0.03f, 1f) }, 16), Lacquer,
                    stand.transform, position, Quaternion.identity, new Vector3(1f, standHeight - 0.03f, 1f));
            Piece("Tray", TrayMesh, _brass, stand.transform, position + Vector3.up * (standHeight - 0.03f), Quaternion.identity, new Vector3(0.6f, 1f, 0.6f));
            LiteProp(PropOilLamp, 8000, position + Vector3.up * (standHeight - 0.02f), 30f, 1f, stand.transform, false, "OilLamp");
            var light = stand.GetComponentInChildren<Light>();
            if (light != null) { light.shadows = LightShadows.None; light.transform.position = position + Vector3.up * (standHeight + 0.28f); }
            var flame = stand.transform.Find("Flame");
            if (flame != null) { flame.position = position + Vector3.up * (standHeight + 0.2f); flame.localScale = new Vector3(0.018f, 0.04f, 0.018f); }
            var lamp = stand.AddComponent<LampFlame>();
            Set(lamp, "_light", light);
            Set(lamp, "_flame", flame != null ? flame.gameObject : null);
            Set(lamp, "_audio", _audio);
            var box = stand.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.center = Vector3.up * (standHeight + 0.1f);
            box.size = new Vector3(0.35f, 0.4f, 0.35f);
            return lamp;
        }

        private static Light RoomLight(string name, Vector3 position, Color color, float range, float intensity, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.range = range;
            light.intensity = intensity;
            light.shadows = LightShadows.None;
            return light;
        }

        private static GameObject Local(Transform parent, string name, Vector3 local, Vector3 size, Material material, bool solid)
        {
            var go = Block(name, Vector3.zero, size, material, parent, solid);
            go.transform.localPosition = local;
            go.transform.localRotation = Quaternion.identity;
            go.isStatic = false;
            return go;
        }

        private static HideSpot WithCrouch(this HideSpot spot)
        {
            Set(spot, "_crouch", true);
            return spot;
        }

        /// <summary>What sort of place it is, so whoever finds her opens it the right way (see CatchScene).</summary>
        private static HideSpot WithKind(this HideSpot spot, HideKind kind)
        {
            var serialized = new UnityEditor.SerializedObject(spot);
            serialized.FindProperty("_kind").enumValueIndex = (int)kind;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return spot;
        }

        /// <summary>Something solid she can stand on that is not boards (a stone kerb or bed): its steps sound like it.</summary>
        private static void Standing(Transform solid, Surface surface)
        {
            var tag = solid.gameObject.AddComponent<SurfaceTag>();
            var serialized = new UnityEditor.SerializedObject(tag);
            serialized.FindProperty("_surface").enumValueIndex = (int)surface;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>A rug or mat lying on the boards: steps on it sound and carry like the covering (no collider needed).</summary>
        private static void Covering(Transform covering, Surface surface)
        {
            var tag = covering.gameObject.AddComponent<SurfaceTag>();
            var serialized = new UnityEditor.SerializedObject(tag);
            serialized.FindProperty("_surface").enumValueIndex = (int)surface;
            serialized.FindProperty("_patch").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
