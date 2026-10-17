using ThuyKieu.Environment;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static ThuyKieu.Core.Editor.StorySceneKit;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Furniture and lived-in clutter for the Chapter 4–6 sets, built from primitives and the CC0 surfaces:
    /// timber-framed walls, a sập daybed, tea sets, chests, bookshelves, hanging scrolls, kitchen strings,
    /// laundry lines, haystacks, stone lanterns, torches, an incense burner and street facades.
    /// Each prop is one root object (position + yaw) whose parts are laid out in its local space.
    /// </summary>
    public static partial class StoryPropKit
    {
        // ------------------------------------------------------------------ materials

        public static Material WoodDark => StoryArtKit.Surface("PropWoodDark", "dark_wood", new Color(0.62f, 0.5f, 0.4f), 1f, 0.3f, 1f, 0.25f);
        public static Material Lacquer => StoryArtKit.Surface("PropLacquer", "lacquered_cherry_wood", new Color(0.85f, 0.32f, 0.26f), 0.8f, 0.55f, 0.8f, 0.2f);
        public static Material Planks => StoryArtKit.Surface("PropPlanks", "planks_brown_10", new Color(0.72f, 0.56f, 0.42f), 1.2f, 0.25f, 1.2f, 0.3f);
        public static Material Straw => StoryArtKit.Surface("PropStraw", "leafy_grass", new Color(1f, 0.84f, 0.5f), 0.6f, 0.05f, 1.4f, 0.4f);
        public static Material Stone => StoryArtKit.Surface("PropStone", "japanese_stone_wall", new Color(0.72f, 0.72f, 0.7f), 0.8f, 0.15f, 1f, 0.4f);
        public static Material Clay => StoryArtKit.Surface("PropClay", "brown_mud_02", new Color(0.82f, 0.5f, 0.34f), 0.6f, 0.15f, 0.8f, 0.3f);
        private static Material Brass => Material("PropBrass", new Color(0.72f, 0.52f, 0.22f), 0.75f);
        private static Material Bronze => Material("PropBronze", new Color(0.33f, 0.25f, 0.15f), 0.6f);
        private static Material Paper => Material("PropPaper", new Color(0.9f, 0.84f, 0.7f), 0.1f);
        private static Material Ink => Material("PropInk", new Color(0.05f, 0.05f, 0.06f), 0.6f);
        private static Material Porcelain => Material("PropPorcelain", new Color(0.88f, 0.88f, 0.84f), 0.75f);
        private static Material BlueWare => Material("PropBlueWare", new Color(0.22f, 0.3f, 0.52f), 0.75f);
        private static Material Flame => Emissive("PropFlame", new Color(1f, 0.62f, 0.25f), 3.2f);
        private static Material Ember => Emissive("PropEmber", new Color(1f, 0.32f, 0.1f), 2f);
        private static Material Cloth(Color color) => Material("PropCloth_" + ColorUtility.ToHtmlStringRGB(color), color, 0.1f);

        // ------------------------------------------------------------------ building blocks

        private static Transform Root(string name, Vector3 position, float yaw, Transform parent)
        {
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.SetPositionAndRotation(position, Quaternion.Euler(0, yaw, 0));
            return root;
        }

        /// <summary>A collider-free primitive placed in the root's local space.</summary>
        private static GameObject Part(Transform root, PrimitiveType type, string name, Vector3 local, Vector3 scale, Material material, Vector3 euler = default, bool shadows = true)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(root, false);
            go.transform.localPosition = local;
            go.transform.localRotation = Quaternion.Euler(euler);
            go.transform.localScale = scale;
            var renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            if (!shadows) renderer.shadowCastingMode = ShadowCastingMode.Off;
            return go;
        }

        private static GameObject Box(Transform root, string name, Vector3 local, Vector3 scale, Material material, float yaw = 0f, bool shadows = true) =>
            Part(root, PrimitiveType.Cube, name, local, scale, material, new Vector3(0, yaw, 0), shadows);

        /// <summary>Vertical cylinder standing on <paramref name="foot"/> (local space).</summary>
        private static GameObject Post(Transform root, string name, Vector3 foot, float radius, float height, Material material, bool shadows = true) =>
            Part(root, PrimitiveType.Cylinder, name, foot + Vector3.up * height * 0.5f, new Vector3(radius * 2f, height * 0.5f, radius * 2f), material, default, shadows);

        /// <summary>Cylinder lying between two local points (logs, rods, culms).</summary>
        private static GameObject Rod(Transform root, string name, Vector3 a, Vector3 b, float radius, Material material, bool shadows = true)
        {
            var go = Part(root, PrimitiveType.Cylinder, name, (a + b) * 0.5f, new Vector3(radius * 2f, Vector3.Distance(a, b) * 0.5f, radius * 2f), material, default, shadows);
            go.transform.localRotation = Quaternion.FromToRotation(Vector3.up, (b - a).normalized);
            return go;
        }

        /// <summary>One box collider on the environment layer so the player walks around the prop and guards cannot see through it.</summary>
        private static void Solid(Transform root, Vector3 centre, Vector3 size)
        {
            var go = new GameObject("Collider") { layer = EnvironmentLayer };
            go.transform.SetParent(root, false);
            go.transform.localPosition = centre;
            var box = go.AddComponent<BoxCollider>();
            box.size = size;
            go.isStatic = true;
        }

        /// <summary>Adds objects to a fadeable wall's linked list so they hide with it.</summary>
        public static void LinkFade(GameObject wall, GameObject extra)
        {
            var fade = wall != null ? wall.GetComponent<CameraFadeable>() : null;
            if (fade == null || extra == null) return;
            var serialized = new SerializedObject(fade);
            var linked = serialized.FindProperty("_linked");
            linked.arraySize++;
            linked.GetArrayElementAtIndex(linked.arraySize - 1).objectReferenceValue = extra;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        // ------------------------------------------------------------------ architecture

        /// <summary>
        /// Exposed timber frame on a plain wall block: posts, a sill, a mid rail and a tie beam, showing on both faces
        /// (the wooden-house look instead of a bare plaster slab). Fades with the wall.
        /// </summary>
        public static GameObject TimberFrame(GameObject wall, Material wood = null, float spacing = 1.5f)
        {
            if (wall == null) return null;
            wood ??= WoodDark;
            Vector3 size = wall.transform.lossyScale;
            bool alongX = size.x >= size.z;
            float length = alongX ? size.x : size.z, thick = (alongX ? size.z : size.x) + 0.05f, height = size.y;
            Vector3 centre = wall.transform.position;
            Vector3 foot = centre - Vector3.up * height * 0.5f;
            var root = Root(wall.name + "_Frame", foot, alongX ? 0f : 90f, wall.transform.parent);
            int posts = Mathf.Max(1, Mathf.RoundToInt(length / spacing));
            for (int i = 0; i <= posts; i++)
            {
                float x = -length * 0.5f + length * i / posts;
                x = Mathf.Clamp(x, -length * 0.5f + 0.08f, length * 0.5f - 0.08f);
                Box(root, "Post", new Vector3(x, height * 0.5f, 0), new Vector3(0.15f, height, thick), wood);
            }
            Box(root, "TieBeam", new Vector3(0, height - 0.12f, 0), new Vector3(length, 0.18f, thick + 0.01f), wood);
            Box(root, "Sill", new Vector3(0, 0.07f, 0), new Vector3(length, 0.14f, thick + 0.01f), wood);
            if (height > 2.4f) Box(root, "Rail", new Vector3(0, 0.95f, 0), new Vector3(length, 0.07f, thick - 0.01f), wood);
            LinkFade(wall, root.gameObject);
            return root.gameObject;
        }

        /// <summary>
        /// Timber frame on one face of a long wall (the inside of an outer wall, along one room): posts, sill, rail
        /// and tie beam standing just proud of the face. <paramref name="inward"/> points into the room.
        /// </summary>
        public static GameObject WallPanels(Vector3 from, Vector3 to, float height, Vector3 inward, Transform parent, float spacing = 1.5f)
        {
            var wood = WoodDark;
            float length = Vector3.Distance(from, to);
            var root = Root("WallPanels", (from + to) * 0.5f + inward.normalized * 0.04f, Quaternion.LookRotation(inward).eulerAngles.y, parent);
            int posts = Mathf.Max(1, Mathf.RoundToInt(length / spacing));
            for (int i = 0; i <= posts; i++)
            {
                float x = Mathf.Clamp(-length * 0.5f + length * i / posts, -length * 0.5f + 0.08f, length * 0.5f - 0.08f);
                Box(root, "Post", new Vector3(x, height * 0.5f, 0), new Vector3(0.15f, height, 0.08f), wood);
            }
            Box(root, "TieBeam", new Vector3(0, height - 0.12f, 0.01f), new Vector3(length, 0.18f, 0.1f), wood);
            Box(root, "Sill", new Vector3(0, 0.07f, 0.01f), new Vector3(length, 0.14f, 0.1f), wood);
            Box(root, "Rail", new Vector3(0, 0.95f, 0), new Vector3(length, 0.07f, 0.06f), wood);
            return root.gameObject;
        }

        /// <summary>
        /// Street-facing front of a town house: corner posts, an eave beam, a stone footing, a framed plank door,
        /// a lattice or shuttered window and some doorstep clutter. <paramref name="face"/> is the facade centre at ground level.
        /// </summary>
        public static void Facade(Transform parent, Vector3 face, Vector3 front, float width, float height, Random random, bool lit, Material stone, bool clutter = true)
        {
            var root = Root("Facade", face, Quaternion.LookRotation(front).eulerAngles.y, parent);
            var wood = WoodDark;
            float half = width * 0.5f;
            foreach (float x in new[] { -half + 0.1f, half - 0.1f })
                Box(root, "CornerPost", new Vector3(x, height * 0.5f, 0.06f), new Vector3(0.2f, height, 0.2f), wood);
            Box(root, "EaveBeam", new Vector3(0, height - 0.12f, 0.1f), new Vector3(width, 0.2f, 0.22f), wood);
            Box(root, "Footing", new Vector3(0, 0.18f, 0.04f), new Vector3(width, 0.36f, 0.1f), stone);

            // Door: jambs, lintel and two plank leaves, one left ajar on some houses.
            float doorX = width > 5.4f ? -width * 0.18f : 0f;
            foreach (float dx in new[] { -0.62f, 0.62f }) Box(root, "Jamb", new Vector3(doorX + dx, 1.12f, 0.06f), new Vector3(0.12f, 2.24f, 0.14f), wood);
            Box(root, "Lintel", new Vector3(doorX, 2.3f, 0.07f), new Vector3(1.5f, 0.16f, 0.16f), wood);
            Box(root, "Threshold", new Vector3(doorX, 0.06f, 0.1f), new Vector3(1.3f, 0.12f, 0.2f), stone);
            bool ajar = random.NextDouble() < 0.3;
            Box(root, "DoorDark", new Vector3(doorX, 1.1f, 0.01f), new Vector3(1.12f, 2.16f, 0.02f), Material("PropDoorway", new Color(0.04f, 0.03f, 0.03f)));
            var left = Box(root, "DoorLeaf", new Vector3(doorX - 0.28f, 1.1f, 0.04f), new Vector3(0.56f, 2.14f, 0.05f), Planks);
            var right = Box(root, "DoorLeaf", new Vector3(doorX + 0.28f, 1.1f, 0.04f), new Vector3(0.56f, 2.14f, 0.05f), Planks);
            if (ajar)
            {
                right.transform.localPosition = new Vector3(doorX + 0.52f, 1.1f, 0.3f);
                right.transform.localRotation = Quaternion.Euler(0, -70f, 0);
                if (lit) Box(root, "DoorGlow", new Vector3(doorX + 0.15f, 1.05f, 0.015f), new Vector3(0.5f, 2f, 0.01f), Emissive("PropWindowGlow", new Color(1f, 0.62f, 0.32f), 0.9f), 0f, false);
            }
            else
                foreach (float dx in new[] { -0.28f, 0.28f }) Box(root, "Batten", new Vector3(doorX + dx, 1.5f, 0.07f), new Vector3(0.5f, 0.08f, 0.02f), wood);
            _ = left;

            // Window on the other side of the door.
            float windowX = doorX + (doorX <= 0f ? 1f : -1f) * Mathf.Min(1.6f, half - 0.75f);
            if (half > 1.6f)
            {
                var paper = lit ? Emissive("PropWindowGlow", new Color(1f, 0.62f, 0.32f), 0.9f) : Material("PropWindowDark", new Color(0.16f, 0.13f, 0.1f));
                var window = StoryArtKit.Lattice("Window", Vector3.zero, 0.9f, 0.7f, 0f, Material("PropLatticeFrame", new Color(0.18f, 0.08f, 0.05f), 0.4f), paper, root);
                window.transform.localPosition = new Vector3(windowX, 1.65f, 0.05f);
                window.transform.localRotation = Quaternion.Euler(0, 180f, 0);
                if (random.NextDouble() < 0.45) StoryArtKit.Blind("Blind", root.TransformPoint(new Vector3(windowX, 2.1f, 0.12f)), 1.05f, 0.45f, root.eulerAngles.y, root);
            }

            // Signboard, an unlit lantern under the eave, and things left at the door.
            if (random.NextDouble() < 0.4)
            {
                var sign = Box(root, "Sign", new Vector3(doorX - 0.85f, 1.6f, 0.1f), new Vector3(0.26f, 1.1f, 0.04f), Material("CoupletRed", new Color(0.45f, 0.05f, 0.04f), 0.55f));
                for (int i = 0; i < 4; i++) Box(root, "Glyph", new Vector3(doorX - 0.85f, 1.98f - i * 0.25f, 0.125f), new Vector3(0.13f, 0.12f, 0.01f), Emissive("CoupletGold", new Color(0.85f, 0.62f, 0.22f), 0.25f), 0f, false);
                _ = sign;
            }
            if (random.NextDouble() < 0.5)
            {
                var lantern = Lantern("EaveLantern", root.TransformPoint(new Vector3(doorX + 0.95f, height - 0.35f, 0.35f)), new Color(1f, 0.25f, 0.15f), root, 0f, 0f);
                lantern.transform.localScale = Vector3.one * 0.8f;
            }
            double kind = random.NextDouble();
            Vector3 step = root.TransformPoint(new Vector3(windowX, 0f, 0.55f));
            if (!clutter) return;
            if (kind < 0.3) StoryArtKit.Jars(step, 2, parent, random.Next());
            else if (kind < 0.55) Baskets(step, Quaternion.LookRotation(front).eulerAngles.y, parent, random.Next());
            else if (kind < 0.7) Woodpile(step, Quaternion.LookRotation(front).eulerAngles.y + 90f, 1.2f, parent, random.Next());
        }

        // ------------------------------------------------------------------ furniture

        /// <summary>Sập: the low lacquered daybed of a well-off house, with a reed mat, a pillow and a folded quilt. Long axis = local Z.</summary>
        public static GameObject Sap(Vector3 position, float yaw, Transform parent, float length = 2f, float width = 1.2f)
        {
            var root = Root("Sap", position, yaw, parent);
            var wood = Lacquer;
            Box(root, "Top", new Vector3(0, 0.4f, 0), new Vector3(width, 0.08f, length), wood);
            Box(root, "Apron", new Vector3(0, 0.29f, 0), new Vector3(width - 0.08f, 0.16f, length - 0.08f), WoodDark);
            foreach (float x in new[] { -1f, 1f })
                foreach (float z in new[] { -1f, 1f })
                {
                    Box(root, "Leg", new Vector3(x * (width * 0.5f - 0.08f), 0.13f, z * (length * 0.5f - 0.08f)), new Vector3(0.14f, 0.26f, 0.14f), WoodDark);
                    Part(root, PrimitiveType.Sphere, "Foot", new Vector3(x * (width * 0.5f - 0.08f), 0.03f, z * (length * 0.5f - 0.08f)), new Vector3(0.16f, 0.06f, 0.16f), Brass);
                }
            Box(root, "Mat", new Vector3(0, 0.445f, 0), new Vector3(width - 0.1f, 0.012f, length - 0.1f), Straw);
            Part(root, PrimitiveType.Sphere, "Pillow", new Vector3(0, 0.5f, length * 0.5f - 0.25f), new Vector3(0.5f, 0.12f, 0.24f), Cloth(new Color(0.55f, 0.12f, 0.1f)));
            Box(root, "Quilt", new Vector3(0, 0.49f, -length * 0.5f + 0.35f), new Vector3(width - 0.2f, 0.08f, 0.5f), Cloth(new Color(0.42f, 0.3f, 0.5f)));
            Solid(root, new Vector3(0, 0.25f, 0), new Vector3(width, 0.5f, length));
            return root.gameObject;
        }

        /// <summary>Low table (sitting on mats) with an optional tea set.</summary>
        public static GameObject LowTable(Vector3 position, float yaw, Transform parent, bool tea = true, int seed = 0)
        {
            var root = Root("LowTable", position, yaw, parent);
            Box(root, "Top", new Vector3(0, 0.36f, 0), new Vector3(1f, 0.05f, 0.62f), Lacquer);
            foreach (float x in new[] { -0.44f, 0.44f })
                foreach (float z in new[] { -0.25f, 0.25f })
                    Box(root, "Leg", new Vector3(x, 0.17f, z), new Vector3(0.06f, 0.34f, 0.06f), WoodDark);
            foreach (float z in new[] { -0.55f, 0.55f })
                Box(root, "Cushion", new Vector3(0, 0.05f, z), new Vector3(0.55f, 0.1f, 0.42f), Cloth(new Color(0.5f, 0.18f, 0.14f)));
            if (tea) TeaSet(root, new Vector3(0, 0.385f, 0), seed);
            Solid(root, new Vector3(0, 0.2f, 0), new Vector3(1f, 0.4f, 0.62f));
            return root.gameObject;
        }

        /// <summary>Teapot and cups on a round tray, in the parent's local space.</summary>
        private static void TeaSet(Transform root, Vector3 top, int seed)
        {
            var random = new Random(seed);
            Part(root, PrimitiveType.Cylinder, "Tray", top + Vector3.up * 0.008f, new Vector3(0.42f, 0.008f, 0.42f), WoodDark);
            Part(root, PrimitiveType.Sphere, "Teapot", top + new Vector3(0, 0.08f, 0), new Vector3(0.15f, 0.13f, 0.15f), BlueWare);
            Part(root, PrimitiveType.Cylinder, "Lid", top + new Vector3(0, 0.15f, 0), new Vector3(0.06f, 0.01f, 0.06f), BlueWare);
            Rod(root, "Spout", top + new Vector3(0.06f, 0.07f, 0), top + new Vector3(0.13f, 0.12f, 0), 0.012f, BlueWare);
            for (int i = 0; i < 4; i++)
            {
                float a = i * Mathf.PI * 0.5f + (float)random.NextDouble() * 0.3f;
                Part(root, PrimitiveType.Cylinder, "Cup", top + new Vector3(Mathf.Cos(a) * 0.15f, 0.03f, Mathf.Sin(a) * 0.15f), new Vector3(0.05f, 0.022f, 0.05f), Porcelain);
            }
        }

        /// <summary>Lacquered chest (rương) with brass corners and a lock.</summary>
        public static GameObject Chest(Vector3 position, float yaw, Transform parent, float scale = 1f)
        {
            var root = Root("Chest", position, yaw, parent);
            root.localScale = Vector3.one * scale;
            Box(root, "Body", new Vector3(0, 0.27f, 0), new Vector3(0.9f, 0.5f, 0.55f), Lacquer);
            Box(root, "Lid", new Vector3(0, 0.54f, 0), new Vector3(0.94f, 0.07f, 0.59f), WoodDark);
            foreach (float x in new[] { -0.45f, 0.45f })
                foreach (float z in new[] { -0.27f, 0.27f })
                    Box(root, "Corner", new Vector3(x, 0.3f, z), new Vector3(0.05f, 0.56f, 0.05f), Brass);
            Box(root, "Lock", new Vector3(0, 0.42f, 0.285f), new Vector3(0.12f, 0.12f, 0.02f), Brass);
            Box(root, "Plinth", new Vector3(0, 0.02f, 0), new Vector3(0.96f, 0.04f, 0.6f), WoodDark);
            Solid(root, new Vector3(0, 0.29f, 0), new Vector3(0.94f, 0.58f, 0.59f));
            return root.gameObject;
        }

        /// <summary>Open bookshelf filled with stitched books, scroll rolls and boxes. Faces local +Z.</summary>
        public static GameObject Bookshelf(Vector3 position, float yaw, Transform parent, int seed, float width = 1.6f, float height = 2.1f)
        {
            var random = new Random(seed);
            var root = Root("Bookshelf", position, yaw, parent);
            var wood = WoodDark;
            Box(root, "Back", new Vector3(0, height * 0.5f, -0.18f), new Vector3(width, height, 0.03f), wood);
            foreach (float x in new[] { -width * 0.5f, width * 0.5f }) Box(root, "Side", new Vector3(x, height * 0.5f, 0), new Vector3(0.05f, height, 0.4f), wood);
            int shelves = Mathf.RoundToInt(height / 0.42f);
            var covers = new[] { Cloth(new Color(0.55f, 0.42f, 0.3f)), Cloth(new Color(0.3f, 0.24f, 0.18f)), Cloth(new Color(0.24f, 0.28f, 0.36f)), Cloth(new Color(0.62f, 0.55f, 0.4f)), Cloth(new Color(0.42f, 0.16f, 0.12f)) };
            for (int s = 0; s <= shelves; s++)
            {
                float y = 0.04f + s * (height - 0.08f) / shelves;
                Box(root, "Shelf", new Vector3(0, y, 0), new Vector3(width, 0.04f, 0.4f), wood);
                if (s == shelves) break;
                float x = -width * 0.5f + 0.06f;
                while (x < width * 0.5f - 0.12f)
                {
                    double kind = random.NextDouble();
                    if (kind < 0.62)
                    {
                        // A stack of stitched books lying flat, the way Vietnamese and Chinese books were kept.
                        int count = 2 + random.Next(5);
                        float w = 0.2f + (float)random.NextDouble() * 0.08f;
                        for (int b = 0; b < count; b++)
                            Box(root, "Book", new Vector3(x + w * 0.5f, y + 0.035f + b * 0.032f, 0.02f), new Vector3(w, 0.028f, 0.3f), covers[random.Next(covers.Length)], (float)(random.NextDouble() - 0.5) * 8f);
                        x += w + 0.04f;
                    }
                    else if (kind < 0.85)
                    {
                        int count = 2 + random.Next(3);
                        for (int r = 0; r < count; r++)
                            Rod(root, "ScrollRoll", new Vector3(x + 0.04f + r * 0.06f, y + 0.05f, -0.15f), new Vector3(x + 0.04f + r * 0.06f, y + 0.05f, 0.15f), 0.028f, Paper);
                        x += count * 0.06f + 0.06f;
                    }
                    else
                    {
                        Box(root, "Box", new Vector3(x + 0.12f, y + 0.08f, 0.02f), new Vector3(0.22f, 0.14f, 0.26f), Lacquer);
                        x += 0.28f;
                    }
                }
            }
            Solid(root, new Vector3(0, height * 0.5f, 0), new Vector3(width, height, 0.4f));
            return root.gameObject;
        }

        /// <summary>Hanging scroll painting: rods, silk border and a misty landscape in a few strokes. Faces local +Z.</summary>
        public static GameObject Scroll(Vector3 position, float yaw, Transform parent, int seed, float width = 0.55f, float height = 1.3f)
        {
            var random = new Random(seed);
            var root = Root("HangingScroll", position, yaw, parent);
            Box(root, "Silk", Vector3.zero, new Vector3(width, height, 0.01f), Cloth(new Color(0.42f, 0.3f, 0.2f)), 0f, false);
            Box(root, "Picture", new Vector3(0, 0.04f, 0.007f), new Vector3(width - 0.1f, height - 0.3f, 0.005f), Paper, 0f, false);
            Rod(root, "TopRod", new Vector3(-width * 0.5f - 0.03f, height * 0.5f, 0), new Vector3(width * 0.5f + 0.03f, height * 0.5f, 0), 0.015f, WoodDark, false);
            Rod(root, "BottomRod", new Vector3(-width * 0.5f - 0.05f, -height * 0.5f, 0), new Vector3(width * 0.5f + 0.05f, -height * 0.5f, 0), 0.022f, WoodDark, false);
            var inkWash = Material("PropInkWash", new Color(0.32f, 0.33f, 0.32f), 0.05f);
            for (int i = 0; i < 4; i++)
            {
                float h = 0.12f + (float)random.NextDouble() * 0.3f;
                Box(root, "Peak", new Vector3(((float)random.NextDouble() - 0.5f) * (width - 0.2f), -height * 0.2f + h * 0.5f + i * 0.04f, 0.011f),
                    new Vector3(0.12f + (float)random.NextDouble() * 0.1f, h, 0.002f), inkWash, (float)random.NextDouble() * 20f - 10f, false).transform.localRotation = Quaternion.Euler(0, 0, 45f);
            }
            Box(root, "Seal", new Vector3(width * 0.3f, -height * 0.3f, 0.011f), new Vector3(0.04f, 0.04f, 0.002f), Material("CoupletRed", new Color(0.45f, 0.05f, 0.04f), 0.55f), 0f, false);
            for (int i = 0; i < 5; i++)
                Box(root, "Verse", new Vector3(-width * 0.32f, height * 0.3f - i * 0.07f, 0.011f), new Vector3(0.035f, 0.045f, 0.002f), Ink, 0f, false);
            return root.gameObject;
        }

        /// <summary>Hanging cloth curtain gathered in folds. Faces local +Z.</summary>
        public static GameObject Curtain(Vector3 top, float width, float drop, float yaw, Color color, Transform parent)
        {
            var root = Root("Curtain", top, yaw, parent);
            var silk = Cloth(color);
            Rod(root, "Rail", new Vector3(-width * 0.5f - 0.05f, 0, 0), new Vector3(width * 0.5f + 0.05f, 0, 0), 0.02f, WoodDark);
            int folds = Mathf.Max(4, Mathf.RoundToInt(width / 0.14f));
            for (int i = 0; i < folds; i++)
            {
                float x = -width * 0.5f + (i + 0.5f) * width / folds;
                float sway = (i % 2 == 0 ? 1f : -1f) * 22f;
                Box(root, "Fold", new Vector3(x, -drop * 0.5f, (i % 2) * 0.03f), new Vector3(width / folds * 1.15f, drop, 0.012f), silk, sway);
            }
            Box(root, "Tie", new Vector3(width * 0.32f, -drop * 0.55f, 0.04f), new Vector3(0.06f, 0.05f, 0.05f), Brass);
            return root.gameObject;
        }

        /// <summary>Candle on a turned stand; the flame glows and, with a range, lights the room.</summary>
        public static GameObject Candle(Vector3 position, Transform parent, float lightRange = 0f, float intensity = 0.9f, float standHeight = 1.1f)
        {
            var root = Root("CandleStand", position, 0f, parent);
            Post(root, "Base", Vector3.zero, 0.13f, 0.05f, WoodDark);
            Post(root, "Stem", Vector3.up * 0.05f, 0.025f, standHeight - 0.1f, WoodDark);
            Post(root, "Dish", Vector3.up * (standHeight - 0.06f), 0.08f, 0.03f, Brass);
            Post(root, "Candle", Vector3.up * (standHeight - 0.03f), 0.025f, 0.14f, Material("PropWax", new Color(0.92f, 0.88f, 0.78f), 0.3f));
            Part(root, PrimitiveType.Sphere, "Flame", Vector3.up * (standHeight + 0.15f), new Vector3(0.025f, 0.05f, 0.025f), Flame, default, false);
            if (lightRange > 0f) AddLight(root, Vector3.up * (standHeight + 0.2f), new Color(1f, 0.66f, 0.38f), lightRange, intensity, true);
            return root.gameObject;
        }

        private static Light AddLight(Transform root, Vector3 local, Color color, float range, float intensity, bool flicker)
        {
            var go = new GameObject("Light");
            go.transform.SetParent(root, false);
            go.transform.localPosition = local;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.range = range;
            light.intensity = intensity;
            light.shadows = LightShadows.None;
            if (flicker)
            {
                var sway = root.gameObject.AddComponent<LanternSway>();
                StorySceneKit.Set(sway, "_swing", 0f);
                StorySceneKit.Set(sway, "_flicker", 0.18f);
            }
            return light;
        }

        /// <summary>Wall shelf with jars, bowls and bundles. Faces local +Z.</summary>
        public static GameObject WallShelf(Vector3 position, float yaw, float length, Transform parent, int seed)
        {
            var random = new Random(seed);
            var root = Root("WallShelf", position, yaw, parent);
            Box(root, "Board", Vector3.zero, new Vector3(length, 0.04f, 0.32f), WoodDark);
            foreach (float x in new[] { -length * 0.4f, length * 0.4f }) Box(root, "Bracket", new Vector3(x, -0.12f, -0.08f), new Vector3(0.05f, 0.22f, 0.16f), WoodDark);
            float cursor = -length * 0.5f + 0.1f;
            while (cursor < length * 0.5f - 0.1f)
            {
                double kind = random.NextDouble();
                float size = 0.12f + (float)random.NextDouble() * 0.1f;
                if (kind < 0.45) Part(root, PrimitiveType.Sphere, "Jar", new Vector3(cursor + size * 0.5f, 0.02f + size * 0.6f, 0), new Vector3(size, size * 1.25f, size), kind < 0.2 ? BlueWare : Clay);
                else if (kind < 0.75)
                {
                    int stack = 2 + random.Next(4);
                    for (int i = 0; i < stack; i++) Part(root, PrimitiveType.Cylinder, "Bowl", new Vector3(cursor + 0.09f, 0.035f + i * 0.03f, 0), new Vector3(0.18f, 0.015f, 0.18f), Porcelain);
                    size = 0.18f;
                }
                else Box(root, "Bundle", new Vector3(cursor + size * 0.5f, 0.08f, 0), new Vector3(size, 0.12f, 0.22f), Straw, (float)random.NextDouble() * 20f);
                cursor += size + 0.06f;
            }
            return root.gameObject;
        }

        /// <summary>Strings of garlic and dried chilli hanging from a beam (local space: hangs down from <paramref name="top"/>).</summary>
        public static GameObject Strings(Vector3 top, Transform parent, int seed)
        {
            var random = new Random(seed);
            var root = Root("HangingStrings", top, (float)random.NextDouble() * 360f, parent);
            var garlic = Material("PropGarlic", new Color(0.88f, 0.84f, 0.74f), 0.2f);
            var chilli = Material("PropChilli", new Color(0.62f, 0.08f, 0.05f), 0.45f);
            for (int s = 0; s < 3; s++)
            {
                float x = (s - 1) * 0.16f;
                float drop = 0.5f + (float)random.NextDouble() * 0.4f;
                Rod(root, "Cord", new Vector3(x, 0, 0), new Vector3(x, -drop, 0), 0.005f, Material("Rope", new Color(0.18f, 0.12f, 0.08f)), false);
                bool isChilli = s == 1;
                for (float y = 0.08f; y < drop; y += isChilli ? 0.05f : 0.08f)
                    Part(root, PrimitiveType.Sphere, isChilli ? "Chilli" : "Garlic", new Vector3(x + ((float)random.NextDouble() - 0.5f) * 0.05f, -y, ((float)random.NextDouble() - 0.5f) * 0.05f),
                        isChilli ? new Vector3(0.025f, 0.06f, 0.025f) : Vector3.one * 0.065f, isChilli ? chilli : garlic, default, false);
            }
            return root.gameObject;
        }

        public static void Sacks(Vector3 position, int count, Transform parent, int seed)
        {
            var random = new Random(seed);
            var burlap = StoryArtKit.Surface("PropBurlap", "leafy_grass", new Color(0.75f, 0.62f, 0.45f), 0.4f, 0.05f, 0.8f, 0.3f);
            var root = Root("Sacks", position, (float)random.NextDouble() * 360f, parent);
            for (int i = 0; i < count; i++)
            {
                Vector3 p = new Vector3((i % 3) * 0.48f - 0.48f, 0.26f + (i / 3) * 0.4f, ((float)random.NextDouble() - 0.5f) * 0.2f);
                Part(root, PrimitiveType.Sphere, "Sack", p, new Vector3(0.46f, 0.52f, 0.4f), burlap, new Vector3(0, random.Next(360), (float)(random.NextDouble() - 0.5) * 12f));
                Part(root, PrimitiveType.Cylinder, "Neck", p + Vector3.up * 0.26f, new Vector3(0.1f, 0.04f, 0.1f), burlap);
            }
            Solid(root, new Vector3(0, 0.35f, 0), new Vector3(1.4f, 0.7f, 0.5f));
        }

        public static void Baskets(Vector3 position, float yaw, Transform parent, int seed)
        {
            var random = new Random(seed);
            var weave = Straw;
            var root = Root("Baskets", position, yaw, parent);
            int count = 1 + random.Next(3);
            for (int i = 0; i < count; i++)
            {
                float r = 0.2f + (float)random.NextDouble() * 0.12f;
                Vector3 foot = new Vector3(i * 0.5f - 0.4f, 0, ((float)random.NextDouble() - 0.5f) * 0.3f);
                Post(root, "Basket", foot, r, r * 1.1f, weave);
                Post(root, "Rim", foot + Vector3.up * r * 1.1f, r + 0.015f, 0.03f, WoodDark);
                if (random.NextDouble() < 0.5)
                    for (int k = 0; k < 4; k++)
                        Part(root, PrimitiveType.Sphere, "Produce", foot + new Vector3(((float)random.NextDouble() - 0.5f) * r, r * 1.1f, ((float)random.NextDouble() - 0.5f) * r), Vector3.one * 0.1f,
                            Material(k % 2 == 0 ? "PropFruit" : "PropGreens", k % 2 == 0 ? new Color(0.78f, 0.42f, 0.12f) : new Color(0.25f, 0.42f, 0.16f), 0.35f));
            }
        }

        /// <summary>Shallow puddle: a dark, glossy disc that catches lamps and the moon.</summary>
        public static GameObject Puddle(Vector3 position, Vector2 size, float yaw, Transform parent)
        {
            var root = Root("Puddle", position, yaw, parent);
            Part(root, PrimitiveType.Cylinder, "Water", new Vector3(0, 0.006f, 0), new Vector3(size.x, 0.002f, size.y), Material("PropPuddleDark", new Color(0.02f, 0.025f, 0.03f), 0.82f), default, false);
            Part(root, PrimitiveType.Cylinder, "Water", new Vector3(size.x * 0.3f, 0.005f, size.y * 0.2f), new Vector3(size.x * 0.6f, 0.002f, size.y * 0.7f), Material("PropPuddleDark", new Color(0.02f, 0.025f, 0.03f), 0.82f), default, false);
            return root.gameObject;
        }

        // ------------------------------------------------------------------ yard

        /// <summary>Rounded haystack: tapering layers of straw and a capped top.</summary>
        public static GameObject Haystack(Vector3 position, float radius, float height, Transform parent)
        {
            var root = Root("Haystack", position, 0f, parent);
            for (int i = 0; i < 4; i++)
            {
                float t = i / 4f;
                Post(root, "Layer", Vector3.up * height * t * 0.8f, radius * (1f - t * 0.35f), height * 0.28f, Straw);
            }
            Part(root, PrimitiveType.Sphere, "Crown", Vector3.up * height * 0.85f, new Vector3(radius * 1.2f, height * 0.4f, radius * 1.2f), Straw);
            Post(root, "Pole", Vector3.zero, 0.04f, height + 0.4f, WoodDark);
            for (int i = 0; i < 10; i++)
            {
                float a = i * 0.63f;
                Rod(root, "Loose", new Vector3(Mathf.Cos(a) * radius * 1.05f, 0.03f, Mathf.Sin(a) * radius * 1.05f),
                    new Vector3(Mathf.Cos(a + 0.2f) * radius * 1.35f, 0.01f, Mathf.Sin(a + 0.2f) * radius * 1.35f), 0.02f, Straw, false);
            }
            Solid(root, new Vector3(0, height * 0.5f, 0), new Vector3(radius * 1.8f, height, radius * 1.8f));
            return root.gameObject;
        }

        /// <summary>Stacked firewood with visible log ends. Long axis = local X.</summary>
        public static GameObject Woodpile(Vector3 position, float yaw, float length, Transform parent, int seed, int rows = 3)
        {
            var random = new Random(seed);
            var root = Root("Woodpile", position, yaw, parent);
            var bark = WoodDark;
            var cut = Material("PropLogEnd", new Color(0.7f, 0.52f, 0.34f), 0.15f);
            for (int row = 0; row < rows; row++)
            {
                int perRow = 5 - row;
                for (int i = 0; i < perRow; i++)
                {
                    float z = (i - (perRow - 1) * 0.5f) * 0.17f;
                    float y = 0.08f + row * 0.14f;
                    float jitter = ((float)random.NextDouble() - 0.5f) * 0.12f;
                    Rod(root, "Log", new Vector3(-length * 0.5f + jitter, y, z), new Vector3(length * 0.5f + jitter, y, z), 0.075f, bark);
                    Part(root, PrimitiveType.Cylinder, "LogEnd", new Vector3(length * 0.5f + jitter + 0.002f, y, z), new Vector3(0.14f, 0.004f, 0.14f), cut, new Vector3(0, 0, 90), false);
                }
            }
            Solid(root, new Vector3(0, rows * 0.07f + 0.05f, 0), new Vector3(length, rows * 0.15f + 0.05f, 0.85f));
            return root.gameObject;
        }

        /// <summary>
        /// Washing line between two posts: a sagging rope with cloths of mixed sizes and colours, each hung slightly askew.
        /// The cloths block a guard's sight. <paramref name="skip"/> leaves a gap to slip through.
        /// </summary>
        public static GameObject ClothLine(Vector3 a, Vector3 b, Transform parent, int seed, int skip = -1)
        {
            var random = new Random(seed);
            var root = new GameObject("ClothLine").transform;
            root.SetParent(parent, false);
            const float top = 2.25f;
            foreach (var p in new[] { a, b })
            {
                var pole = Cylinder("LaundryPole", p, 0.05f, top + 0.1f, WoodDark, root);
                pole.isStatic = true;
            }
            StoryArtKit.Rope(a + Vector3.up * top, b + Vector3.up * top, 0.12f, root);
            Color[] colours = { new Color(0.78f, 0.76f, 0.7f), new Color(0.22f, 0.26f, 0.42f), new Color(0.55f, 0.42f, 0.3f), new Color(0.62f, 0.68f, 0.72f), new Color(0.45f, 0.18f, 0.16f) };
            float length = Vector3.Distance(a, b);
            Vector3 dir = (b - a).normalized;
            float yaw = Quaternion.LookRotation(Vector3.Cross(Vector3.up, dir)).eulerAngles.y;
            int index = 0;
            for (float s = 0.5f; s < length - 0.6f; index++)
            {
                float w = 0.9f + (float)random.NextDouble() * 0.8f;
                float drop = 0.9f + (float)random.NextDouble() * 0.7f;
                if (index == skip) { s += w + 0.3f; continue; }
                float t = (s + w * 0.5f) / length;
                float sag = 0.12f * 4f * t * (1 - t);
                Vector3 centre = a + dir * (s + w * 0.5f) + Vector3.up * (top - sag - drop * 0.5f);
                var cloth = Block("Cloth", centre, new Vector3(w, drop, 0.03f), Cloth(colours[random.Next(colours.Length)]), root, true, yaw + ((float)random.NextDouble() - 0.5f) * 8f);
                cloth.transform.rotation *= Quaternion.Euler(((float)random.NextDouble() - 0.5f) * 6f, 0, ((float)random.NextDouble() - 0.5f) * 4f);
                var pegs = new GameObject("Pegs").transform;
                pegs.SetParent(root, false);
                foreach (float k in new[] { -0.4f, 0.4f })
                    Block("Peg", centre + dir * w * k + Vector3.up * (drop * 0.5f + 0.02f), new Vector3(0.025f, 0.07f, 0.04f), WoodDark, pegs, false, yaw);
                s += w + 0.15f + (float)random.NextDouble() * 0.2f;
            }
            return root.gameObject;
        }

        /// <summary>Stone lantern (đèn đá): base, shaft, a lamp box with glowing openings and a little roof.</summary>
        public static GameObject StoneLantern(Vector3 position, float yaw, Transform parent, float lightRange = 0f, float intensity = 1f)
        {
            var root = Root("StoneLantern", position, yaw, parent);
            var stone = Stone;
            Box(root, "Base", new Vector3(0, 0.1f, 0), new Vector3(0.6f, 0.2f, 0.6f), stone);
            Post(root, "Shaft", Vector3.up * 0.2f, 0.12f, 0.7f, stone);
            Box(root, "Saucer", new Vector3(0, 0.95f, 0), new Vector3(0.5f, 0.1f, 0.5f), stone);
            Box(root, "LampBox", new Vector3(0, 1.17f, 0), new Vector3(0.36f, 0.34f, 0.36f), stone);
            var glow = Emissive("PropStoneLanternGlow", new Color(1f, 0.6f, 0.3f), 1.6f);
            Box(root, "Opening", new Vector3(0, 1.17f, 0.181f), new Vector3(0.16f, 0.2f, 0.01f), glow, 0f, false);
            Box(root, "Opening", new Vector3(0, 1.17f, -0.181f), new Vector3(0.16f, 0.2f, 0.01f), glow, 0f, false);
            Part(root, PrimitiveType.Cube, "Roof", new Vector3(0, 1.4f, 0), new Vector3(0.62f, 0.12f, 0.62f), stone, new Vector3(0, 45, 0));
            Part(root, PrimitiveType.Sphere, "Finial", new Vector3(0, 1.52f, 0), new Vector3(0.12f, 0.14f, 0.12f), stone);
            if (lightRange > 0f) AddLight(root, new Vector3(0, 1.17f, 0), new Color(1f, 0.62f, 0.35f), lightRange, intensity, true);
            Solid(root, new Vector3(0, 0.75f, 0), new Vector3(0.6f, 1.5f, 0.6f));
            return root.gameObject;
        }

        /// <summary>Burning torch held by a pursuer: a pole, a pitch-soaked head, a flickering flame and its light.</summary>
        public static GameObject Torch(Transform holder, Vector3 local)
        {
            var root = new GameObject("Torch").transform;
            root.SetParent(holder, false);
            root.localPosition = local;
            root.localRotation = Quaternion.Euler(-12f, 0, 8f);
            Post(root, "Pole", new Vector3(0, -0.6f, 0), 0.025f, 0.7f, WoodDark, false);
            Post(root, "Head", new Vector3(0, 0.08f, 0), 0.045f, 0.12f, Material("PropPitch", new Color(0.08f, 0.06f, 0.05f)), false);
            Part(root, PrimitiveType.Sphere, "Flame", new Vector3(0, 0.3f, 0), new Vector3(0.14f, 0.26f, 0.14f), Flame, default, false);
            Part(root, PrimitiveType.Sphere, "FlameCore", new Vector3(0, 0.25f, 0), new Vector3(0.09f, 0.15f, 0.09f), Ember, default, false);
            var light = AddLight(root, new Vector3(0, 0.35f, 0), new Color(1f, 0.55f, 0.25f), 7f, 1.8f, false);
            var sway = root.gameObject.AddComponent<LanternSway>();
            StorySceneKit.Set(sway, "_swing", 2f);
            StorySceneKit.Set(sway, "_speed", 2.6f);
            StorySceneKit.Set(sway, "_flicker", 0.3f);
            _ = light;
            return root.gameObject;
        }

        /// <summary>Bronze tripod incense burner (đỉnh) with handles, a lid and smouldering joss sticks.</summary>
        public static GameObject IncenseBurner(Vector3 position, Transform parent)
        {
            var root = Root("IncenseBurner", position, 0f, parent);
            var bronze = Bronze;
            Box(root, "Plinth", new Vector3(0, 0.2f, 0), new Vector3(1f, 0.4f, 1f), Stone);
            Box(root, "PlinthCap", new Vector3(0, 0.42f, 0), new Vector3(1.08f, 0.05f, 1.08f), Stone);
            for (int i = 0; i < 3; i++)
            {
                float a = i * Mathf.PI * 2f / 3f;
                Rod(root, "Leg", new Vector3(Mathf.Cos(a) * 0.28f, 0.45f, Mathf.Sin(a) * 0.28f), new Vector3(Mathf.Cos(a) * 0.2f, 0.72f, Mathf.Sin(a) * 0.2f), 0.04f, bronze);
            }
            Part(root, PrimitiveType.Sphere, "Belly", new Vector3(0, 0.88f, 0), new Vector3(0.62f, 0.42f, 0.62f), bronze);
            Post(root, "Rim", new Vector3(0, 1.02f, 0), 0.27f, 0.06f, bronze);
            foreach (float x in new[] { -0.36f, 0.36f })
            {
                Box(root, "Handle", new Vector3(x, 1.12f, 0), new Vector3(0.05f, 0.2f, 0.16f), bronze);
                Box(root, "HandleTop", new Vector3(x * 0.92f, 1.22f, 0), new Vector3(0.12f, 0.04f, 0.16f), bronze);
            }
            Post(root, "Ash", new Vector3(0, 1.06f, 0), 0.22f, 0.02f, Material("PropAsh", new Color(0.5f, 0.48f, 0.45f)));
            var stick = Material("PropJoss", new Color(0.55f, 0.12f, 0.08f));
            for (int i = 0; i < 9; i++)
            {
                float a = i * 0.7f;
                Vector3 foot = new Vector3(Mathf.Cos(a) * 0.12f * (i % 3) / 2f, 1.08f, Mathf.Sin(a) * 0.12f * (i % 3) / 2f);
                Vector3 tip = foot + new Vector3(Mathf.Cos(a) * 0.05f, 0.34f, Mathf.Sin(a) * 0.05f);
                Rod(root, "Joss", foot, tip, 0.006f, stick, false);
                Part(root, PrimitiveType.Sphere, "Ember", tip, Vector3.one * 0.016f, Ember, default, false);
            }
            Solid(root, new Vector3(0, 0.6f, 0), new Vector3(1f, 1.2f, 1f));
            return root.gameObject;
        }

        /// <summary>Temple drum or bell on a wooden frame.</summary>
        public static GameObject DrumStand(Vector3 position, float yaw, Transform parent, bool bell)
        {
            var root = Root(bell ? "BellFrame" : "DrumStand", position, yaw, parent);
            var wood = Lacquer;
            foreach (float x in new[] { -0.7f, 0.7f })
            {
                Box(root, "Upright", new Vector3(x, 1.1f, 0), new Vector3(0.12f, 2.2f, 0.12f), wood);
                Box(root, "Foot", new Vector3(x, 0.05f, 0), new Vector3(0.16f, 0.1f, 0.8f), WoodDark);
            }
            Box(root, "Beam", new Vector3(0, 2.15f, 0), new Vector3(1.7f, 0.14f, 0.16f), wood);
            if (bell)
            {
                Rod(root, "Hook", new Vector3(0, 2.08f, 0), new Vector3(0, 1.85f, 0), 0.02f, Bronze);
                Part(root, PrimitiveType.Sphere, "BellTop", new Vector3(0, 1.72f, 0), new Vector3(0.5f, 0.32f, 0.5f), Bronze);
                Post(root, "Bell", new Vector3(0, 0.95f, 0), 0.27f, 0.78f, Bronze);
                Post(root, "Lip", new Vector3(0, 0.92f, 0), 0.3f, 0.06f, Bronze);
            }
            else
            {
                var drum = Rod(root, "Drum", new Vector3(0, 1.25f, -0.35f), new Vector3(0, 1.25f, 0.35f), 0.42f, wood);
                _ = drum;
                foreach (float z in new[] { -0.355f, 0.355f })
                    Part(root, PrimitiveType.Cylinder, "Skin", new Vector3(0, 1.25f, z), new Vector3(0.78f, 0.004f, 0.78f), Material("PropDrumSkin", new Color(0.78f, 0.68f, 0.5f), 0.2f), new Vector3(90, 0, 0));
                Rod(root, "Rest", new Vector3(-0.7f, 0.85f, 0), new Vector3(0.7f, 0.85f, 0), 0.04f, WoodDark);
            }
            Solid(root, new Vector3(0, 1.1f, 0), new Vector3(1.6f, 2.2f, 0.8f));
            return root.gameObject;
        }

        /// <summary>Carved stone stele on a tortoise-ish base.</summary>
        public static GameObject Stele(Vector3 position, float yaw, Transform parent)
        {
            var root = Root("Stele", position, yaw, parent);
            Part(root, PrimitiveType.Sphere, "Base", new Vector3(0, 0.22f, 0), new Vector3(1.1f, 0.44f, 1.5f), Stone);
            Box(root, "Slab", new Vector3(0, 1.3f, 0), new Vector3(0.8f, 1.8f, 0.2f), Stone);
            Part(root, PrimitiveType.Sphere, "Crown", new Vector3(0, 2.2f, 0), new Vector3(0.82f, 0.4f, 0.22f), Stone);
            for (int c = 0; c < 4; c++)
                for (int r = 0; r < 7; r++)
                    Box(root, "Carving", new Vector3(-0.24f + c * 0.16f, 1.95f - r * 0.2f, 0.101f), new Vector3(0.09f, 0.1f, 0.004f), Material("PropCarving", new Color(0.3f, 0.3f, 0.29f)), 0f, false);
            Solid(root, new Vector3(0, 1.1f, 0), new Vector3(0.9f, 2.2f, 1.4f));
            return root.gameObject;
        }

        /// <summary>Silver ingots (nén bạc): boat-shaped lumps.</summary>
        public static void Ingots(Transform root, Vector3 local, int count)
        {
            var silver = Material("PropSilver", new Color(0.78f, 0.78f, 0.8f), 0.85f);
            for (int i = 0; i < count; i++)
            {
                Vector3 p = local + new Vector3((i % 3) * 0.16f, (i / 3) * 0.06f, ((i / 3) % 2) * 0.05f);
                Part(root, PrimitiveType.Sphere, "Ingot", p + Vector3.up * 0.03f, new Vector3(0.14f, 0.06f, 0.08f), silver);
                Part(root, PrimitiveType.Sphere, "IngotBoss", p + Vector3.up * 0.055f, new Vector3(0.06f, 0.04f, 0.05f), silver);
            }
        }

        /// <summary>Abacus lying on a counter.</summary>
        public static void Abacus(Transform root, Vector3 local, float yaw)
        {
            var frame = Box(root, "AbacusFrame", local + Vector3.up * 0.015f, new Vector3(0.36f, 0.03f, 0.2f), WoodDark, yaw);
            _ = frame;
            var bead = Material("PropBead", new Color(0.2f, 0.1f, 0.06f), 0.5f);
            var rotation = Quaternion.Euler(0, yaw, 0);
            for (int rod = 0; rod < 7; rod++)
                for (int b = 0; b < 4; b++)
                {
                    float x = -0.15f + rod * 0.05f;
                    float z = -0.07f + b * 0.035f + (b > 2 ? 0.02f : 0f);
                    Part(root, PrimitiveType.Sphere, "Bead", local + rotation * new Vector3(x, 0.04f, z), new Vector3(0.035f, 0.02f, 0.028f), bead, default, false);
                }
        }

        /// <summary>Scale of a silver shop: post, beam, two brass pans.</summary>
        public static void Balance(Transform root, Vector3 local)
        {
            var brass = Brass;
            Post(root, "ScalePost", local, 0.015f, 0.4f, WoodDark);
            Rod(root, "ScaleBeam", local + new Vector3(-0.2f, 0.4f, 0), local + new Vector3(0.2f, 0.4f, 0), 0.008f, brass);
            foreach (float x in new[] { -0.19f, 0.19f })
            {
                Rod(root, "ScaleCord", local + new Vector3(x, 0.4f, 0), local + new Vector3(x, 0.2f, 0), 0.003f, brass, false);
                Part(root, PrimitiveType.Cylinder, "Pan", local + new Vector3(x, 0.2f, 0), new Vector3(0.14f, 0.006f, 0.14f), brass);
            }
        }

        /// <summary>Notice board on two posts with pasted proclamations.</summary>
        public static GameObject NoticeBoard(Vector3 position, float yaw, Transform parent, int seed)
        {
            var random = new Random(seed);
            var root = Root("NoticeBoard", position, yaw, parent);
            foreach (float x in new[] { -0.8f, 0.8f }) Post(root, "Post", new Vector3(x, 0, 0), 0.06f, 2.2f, WoodDark);
            Box(root, "Board", new Vector3(0, 1.45f, 0), new Vector3(1.7f, 0.9f, 0.05f), Planks);
            Box(root, "Roof", new Vector3(0, 2.25f, 0), new Vector3(2f, 0.06f, 0.4f), WoodDark);
            for (int i = 0; i < 4; i++)
            {
                var sheet = Box(root, "Proclamation", new Vector3(-0.6f + i * 0.4f, 1.45f + ((float)random.NextDouble() - 0.5f) * 0.2f, 0.03f), new Vector3(0.32f, 0.5f, 0.005f), Paper, 0f, false);
                sheet.transform.localRotation = Quaternion.Euler(0, 0, ((float)random.NextDouble() - 0.5f) * 6f);
                for (int l = 0; l < 4; l++)
                    Box(root, "Line", sheet.transform.localPosition + new Vector3(-0.1f + l * 0.07f, 0, 0.004f), new Vector3(0.025f, 0.38f, 0.002f), Ink, 0f, false);
            }
            Solid(root, new Vector3(0, 1.1f, 0), new Vector3(1.8f, 2.2f, 0.2f));
            return root.gameObject;
        }

        /// <summary>Key ring on a cord, hung at a character's belt (child of the actor's visual so it follows her pose).</summary>
        public static GameObject KeyRing(Transform visual, Vector3 local)
        {
            var root = new GameObject("KeyRing").transform;
            root.SetParent(visual, false);
            root.localPosition = local;
            var brass = Emissive("PropKeyBrass", new Color(0.85f, 0.62f, 0.25f), 0.35f);
            Part(root, PrimitiveType.Cylinder, "Ring", Vector3.zero, new Vector3(0.09f, 0.006f, 0.09f), brass, new Vector3(0, 0, 90), false);
            for (int i = 0; i < 4; i++)
                Part(root, PrimitiveType.Cube, "Key", new Vector3(0, -0.06f - (i % 2) * 0.01f, (i - 1.5f) * 0.018f), new Vector3(0.008f, 0.09f, 0.014f), brass, new Vector3((i - 1.5f) * 10f, 0, 0), false);
            return root.gameObject;
        }
    }
}
