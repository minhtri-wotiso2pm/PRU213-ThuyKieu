using System.Collections.Generic;
using System.IO;
using ThuyKieu.Core.Story;
using TMPro;
using UnityEditor;
using UnityEngine;
using static ThuyKieu.Core.Editor.StorySceneKit;
using static ThuyKieu.Core.Editor.StoryArtKit;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// A Vietnamese high-school classroom of today, shared by the prologue (afternoon) and the epilogue (sunset):
    /// pale yellow walls with a green dado, a blackboard under the "Tiên học lễ — hậu học văn" banner, double desks
    /// with benches in four rows, open windows with shutters onto a corridor and the school yard, ceiling fans.
    /// Interior x −5…5, z −4.5…4.5 around <c>origin</c>; the board is on the north wall, the door at the back-west.
    /// </summary>
    public static class ClassroomKit
    {
        public sealed class Parts
        {
            public Transform Root;
            public Transform Seat;          // the student's place: third row, by the window
            public Transform NganSeat;      // her deskmate
            public Transform TeacherSpot;
            public Transform Door;          // where people walk out
            public Transform BookAnchor;    // the open Truyện Kiều on her desk
            public Transform BoardAnchor, WindowAnchor, WindowStand;
            public GameObject BookGlow;
            public TMP_Text ThankYou;
            public List<Transform> ClassmateSeats = new List<Transform>();
        }

        // Row centres of the desks (front to back) and the seat offset behind a desk (hips over the bench).
        private static readonly float[] Rows = { 2.3f, 0.85f, -0.6f, -2.05f };
        private static readonly float[] Columns = { -3.2f, 0f, 3.2f };
        public const float SeatBehindDesk = 0.55f;

        public static Parts Build(Transform world, Vector3 o, bool sunset)
        {
            var parts = new Parts();
            var root = new GameObject("Classroom").transform;
            root.SetParent(world, false);
            root.position = o;
            parts.Root = root;

            var wall = Surface("ClassWall", "plastered_wall_02", new Color(1f, 0.9f, 0.62f), 2.2f, 0.12f, 0.6f, 0.25f);
            var dado = Material("ClassDado", new Color(0.42f, 0.6f, 0.52f), 0.35f);
            var floor = TerrazzoFloor();
            var ceiling = Material("ClassCeiling", new Color(0.93f, 0.92f, 0.88f), 0.1f);
            var deskWood = Surface("ClassDesk", "planks_brown_10", new Color(0.9f, 0.72f, 0.52f), 0.9f, 0.35f, 0.5f, 0.2f);
            var metal = Material("ClassMetal", new Color(0.18f, 0.2f, 0.22f), 0.5f);
            var frameGreen = Material("ClassFrame", new Color(0.24f, 0.42f, 0.36f), 0.45f);

            Block("ClassFloor", o + new Vector3(0, -0.1f, 0), new Vector3(10.4f, 0.2f, 9.4f), floor, root);
            Block("ClassCeiling", o + new Vector3(0, 3.5f, 0), new Vector3(10.4f, 0.2f, 9.4f), ceiling, root);
            // North (board) and south (back) walls.
            Block("WallNorth", o + new Vector3(0, 1.7f, 4.6f), new Vector3(10.4f, 3.4f, 0.2f), wall, root);
            Block("WallSouth", o + new Vector3(0, 1.7f, -4.6f), new Vector3(10.4f, 3.4f, 0.2f), wall, root);
            // West wall with the door at the back.
            Block("WallWest", o + new Vector3(-5.1f, 1.7f, 0.7f), new Vector3(0.2f, 3.4f, 7.8f), wall, root);
            Block("WallWestCorner", o + new Vector3(-5.1f, 1.7f, -4.4f), new Vector3(0.2f, 3.4f, 0.2f), wall, root);
            Block("DoorLintel", o + new Vector3(-5.1f, 2.8f, -3.7f), new Vector3(0.2f, 1.2f, 1.2f), wall, root);
            Block("DoorFrame", o + new Vector3(-5.0f, 1.1f, -3.1f), new Vector3(0.08f, 2.2f, 0.08f), frameGreen, root, false);
            Block("DoorFrame", o + new Vector3(-5.0f, 1.1f, -4.3f), new Vector3(0.08f, 2.2f, 0.08f), frameGreen, root, false);
            var leaf = Block("DoorLeaf", o + new Vector3(-5.55f, 1.05f, -3.15f), new Vector3(0.05f, 2.1f, 1.1f), frameGreen, root, false, 70f);
            leaf.transform.position = o + new Vector3(-5.55f, 1.05f, -2.7f);
            // East wall: three tall windows between piers, open onto the corridor.
            Block("WallEastSill", o + new Vector3(5.1f, 0.45f, 0), new Vector3(0.2f, 0.9f, 9.4f), wall, root);
            Block("WallEastTop", o + new Vector3(5.1f, 2.9f, 0), new Vector3(0.2f, 1.0f, 9.4f), wall, root);
            foreach (var (z0, z1) in new[] { (-4.5f, -3.4f), (-1.8f, -0.8f), (0.8f, 1.8f), (3.4f, 4.5f) })
                Block("WallEastPier", o + new Vector3(5.1f, 1.65f, (z0 + z1) * 0.5f), new Vector3(0.2f, 1.5f, z1 - z0), wall, root);
            foreach (float z in new[] { -2.6f, 0f, 2.6f }) Window(root, o + new Vector3(5.1f, 1.65f, z), frameGreen);
            // Dado band on the inner walls.
            Block("DadoNorth", o + new Vector3(0, 0.55f, 4.49f), new Vector3(10f, 1.1f, 0.02f), dado, root, false);
            Block("DadoSouth", o + new Vector3(0, 0.55f, -4.49f), new Vector3(10f, 1.1f, 0.02f), dado, root, false);
            Block("DadoWest", o + new Vector3(-4.99f, 0.55f, 0.7f), new Vector3(0.02f, 1.1f, 7.6f), dado, root, false);
            Block("DadoEast", o + new Vector3(4.99f, 0.45f, 0f), new Vector3(0.02f, 0.9f, 9f), dado, root, false);

            Board(root, o, frameGreen, deskWood);
            Desks(root, o, deskWood, metal, parts);
            Dressing(root, o, metal, deskWood);
            Outside(root, o, sunset);
            Fans(root, o, metal);
            Lights(root, o, sunset);

            parts.TeacherSpot = Point("TeacherSpot", o + new Vector3(-1.2f, 0.18f, 3.55f), 180f, root);
            parts.Door = Point("ClassDoor", o + new Vector3(-6.4f, 0f, -3.7f), 270f, root);
            parts.BoardAnchor = Point("BoardSpot", o + new Vector3(0f, 0f, 3.1f), 0f, root);
            parts.WindowAnchor = Point("WindowSpot", o + new Vector3(4.4f, 0f, 0f), 90f, root);
            Point("Focus_Board", o + new Vector3(0f, 1.6f, 4.45f), 0f, root);
            Look("CAM_Wide_Classroom", o + new Vector3(-4.2f, 2.7f, -4f), o + new Vector3(1.6f, 0.9f, 1.4f), root);
            Look("CAM_Board", o + new Vector3(0.6f, 1.75f, 1.6f), o + new Vector3(-0.2f, 1.7f, 4.5f), root);
            parts.WindowStand = Point("WindowStand", o + new Vector3(4.42f, 0f, 0.25f), 90f, root);
            Look("CAM_Window", o + new Vector3(3.55f, 1.62f, -0.55f), o + new Vector3(7.5f, 1.6f, 1.2f), root);
            var book = parts.BookAnchor.position;
            Look("CAM_Book", book + new Vector3(0.48f, 0.9f, -0.78f), book + new Vector3(0, 0.02f, 0), root);
            Look("CAM_BookClose", book + new Vector3(0.05f, 0.42f, -0.22f), book + new Vector3(0.12f, 0.02f, 0.02f), root);
            Zone("Classroom", o + new Vector3(0, 1.5f, 0), new Vector3(10f, 4f, 9f), 2.4f, 0.32f, 58f, 0.25f, root);
            return parts;
        }

        /// <summary>Granito (terrazzo) floor like Vietnamese schools: a generated texture of chips in grey cement, 40 cm tiles.</summary>
        private static Material TerrazzoFloor()
        {
            string path = AssetFolder + "/ClassTerrazzo.png";
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null)
            {
                const int size = 512, tile = 256;
                var image = new Texture2D(size, size, TextureFormat.RGB24, false);
                var random = new System.Random(19);
                var pixels = new Color[size * size];
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        float n = (float)random.NextDouble() * 0.04f;
                        int t = (x / tile + y / tile) % 2;
                        var base0 = t == 0 ? new Color(0.8f, 0.77f, 0.71f) : new Color(0.78f, 0.75f, 0.7f);
                        pixels[y * size + x] = base0 + new Color(n, n, n);
                    }
                Color[] chips = { new Color(0.95f, 0.94f, 0.9f), new Color(0.35f, 0.34f, 0.33f), new Color(0.62f, 0.5f, 0.38f), new Color(0.12f, 0.12f, 0.12f), new Color(0.72f, 0.66f, 0.56f) };
                for (int i = 0; i < 2600; i++)
                {
                    int cx = random.Next(size), cy = random.Next(size), r = 1 + random.Next(4);
                    var c = chips[random.Next(chips.Length)];
                    for (int dy = -r; dy <= r; dy++)
                        for (int dx = -r; dx <= r; dx++)
                            if (dx * dx + dy * dy <= r * r + random.Next(2))
                                pixels[((cy + dy + size) % size) * size + (cx + dx + size) % size] = c;
                }
                // Grout seams between the tiles.
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                        if (x % tile < 2 || y % tile < 2) pixels[y * size + x] = new Color(0.58f, 0.55f, 0.5f);
                image.SetPixels(pixels);
                File.WriteAllBytes(path, image.EncodeToPNG());
                Object.DestroyImmediate(image);
                AssetDatabase.ImportAsset(path);
                texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            var material = Material("ClassTerrazzo", Color.white, 0.45f);
            material.SetTexture("_BaseMap", texture);
            material.SetTextureScale("_BaseMap", new Vector2(13f, 11.75f));
            EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>Flat five-pointed star facing −Z (the flag's star).</summary>
        private static void Star(Transform root, Vector3 centre, float radius, Material material)
        {
            var vertices = new List<Vector3> { Vector3.zero };
            for (int i = 0; i < 10; i++)
            {
                float a = Mathf.PI * 0.5f + i * Mathf.PI / 5f;
                float r = i % 2 == 0 ? radius : radius * 0.382f;
                vertices.Add(new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f));
            }
            var triangles = new List<int>();
            for (int i = 0; i < 10; i++) { triangles.Add(0); triangles.Add(1 + (i + 1) % 10); triangles.Add(1 + i); }
            var mesh = new Mesh { name = "FlagStar" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            string meshPath = AssetFolder + "/FlagStar.asset";
            AssetDatabase.DeleteAsset(meshPath);
            AssetDatabase.CreateAsset(mesh, meshPath);
            var go = new GameObject("FlagStar");
            go.transform.SetParent(root, false);
            go.transform.position = centre;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
        }

        public const string StudentModel = "Assets/_Game/Art/Characters/LauXanh1/Rigged/LauXanh1_Rigged.fbx";
        public const string StudentTextures = "Assets/_Game/Art/Characters/LauXanh1/Rigged";

        /// <summary>
        /// The modern student's body inside the player object, next to Kiều's (PlayerLook switches between them).
        /// Placeholder rig until her own model arrives: replace <see cref="StudentModel"/> and <see cref="StudentTextures"/>.
        /// </summary>
        public static ThuyKieu.Player.PlayerLook StudentBody(Systems s)
        {
            var player = s.Player.transform;
            Transform kieu = player.GetComponentInChildren<Animator>().transform;
            while (kieu.parent != player) kieu = kieu.parent;
            var student = Body(player, "StudentBody", StudentModel, StudentTextures,
                1.6f, new Color(0.96f, 0.98f, 1f), StoryAnimationSetup.KieuController());
            var look = player.gameObject.AddComponent<ThuyKieu.Player.PlayerLook>();
            Set(look, "_kieu", kieu.gameObject);
            Set(look, "_student", student);
            Set(look, "_movement", s.Player);
            Set(look, "_actor", player.GetComponent<StoryActor>());
            return look;
        }

        private static void Look(string name, Vector3 position, Vector3 target, Transform parent)
        {
            Point(name, position, 0, parent).rotation = Quaternion.LookRotation(target - position);
        }

        /// <summary>Window opening with a green frame, a cross mullion and two shutters folded back outside.</summary>
        private static void Window(Transform root, Vector3 c, Material frame)
        {
            Block("WinFrameTop", c + new Vector3(0, 0.76f, 0), new Vector3(0.24f, 0.06f, 1.62f), frame, root, false);
            Block("WinFrameSill", c + new Vector3(-0.06f, -0.76f, 0), new Vector3(0.34f, 0.05f, 1.66f), frame, root, false);
            Block("WinMullion", c, new Vector3(0.05f, 1.5f, 0.05f), frame, root, false);
            Block("WinTransom", c + new Vector3(0, 0.2f, 0), new Vector3(0.05f, 0.04f, 1.6f), frame, root, false);
            foreach (int side in new[] { -1, 1 })
            {
                Block("WinFrameSide", c + new Vector3(0, 0, side * 0.8f), new Vector3(0.24f, 1.5f, 0.06f), frame, root, false);
                var shutter = Block("Shutter", c + new Vector3(0.48f, 0, side * 1.1f), new Vector3(0.04f, 1.44f, 0.76f), frame, root, false, side * 62f);
                for (int i = 0; i < 6; i++)
                    Block("Louvre", shutter.transform.position + new Vector3(0, -0.6f + i * 0.24f, 0), new Vector3(0.06f, 0.03f, 0.7f), frame, root, false, side * 62f);
            }
        }

        private static void Board(Transform root, Vector3 o, Material frame, Material wood)
        {
            var green = Material("ClassBoard", new Color(0.1f, 0.2f, 0.16f), 0.12f);
            Block("BoardFrame", o + new Vector3(0, 1.62f, 4.47f), new Vector3(4.7f, 1.52f, 0.06f), wood, root, false);
            Block("Board", o + new Vector3(0, 1.62f, 4.43f), new Vector3(4.5f, 1.32f, 0.04f), green, root, false);
            Block("ChalkTray", o + new Vector3(0, 0.9f, 4.38f), new Vector3(4.5f, 0.04f, 0.12f), wood, root, false);
            for (int i = 0; i < 3; i++)
                Block("Chalk", o + new Vector3(-1.4f + i * 0.12f, 0.93f, 4.36f), new Vector3(0.08f, 0.02f, 0.02f), Material("ClassChalk", new Color(0.95f, 0.95f, 0.92f)), root, false, i * 20f);
            WorldText(root, "BoardText", "<size=70%>Thứ Năm, ngày 15 tháng 5</size>\n<b>TRUYỆN KIỀU</b> — Nguyễn Du\n<size=80%>Đoạn: Mua bán Kiều</size>\n\n<i>\"Trăm năm trong cõi người ta,\nchữ tài chữ mệnh khéo là ghét nhau.\"</i>",
                o + new Vector3(-0.2f, 1.62f, 4.405f), 0f, new Vector2(4.2f, 1.25f), 0.11f, new Color(0.93f, 0.95f, 0.9f, 0.92f));
            // Motto banner above the board.
            Block("Banner", o + new Vector3(0, 2.75f, 4.48f), new Vector3(4.2f, 0.36f, 0.03f), Material("ClassBanner", new Color(0.72f, 0.1f, 0.08f), 0.3f), root, false);
            WorldText(root, "BannerText", "<b>TIÊN HỌC LỄ — HẬU HỌC VĂN</b>", o + new Vector3(0, 2.75f, 4.46f), 0f, new Vector2(4.1f, 0.34f), 0.2f, new Color(1f, 0.85f, 0.35f));
            // The teacher's platform and desk.
            Block("Platform", o + new Vector3(0, 0.09f, 3.65f), new Vector3(9.8f, 0.18f, 1.5f), Surface("ClassPlatform", "old_wooden_floor_02", new Color(0.82f, 0.66f, 0.5f), 1.4f, 0.3f), root);
            Block("TeacherDesk", o + new Vector3(-3.1f, 0.6f, 3.35f), new Vector3(1.4f, 0.84f, 0.7f), wood, root);
            Block("TeacherDeskTop", o + new Vector3(-3.1f, 1.04f, 3.35f), new Vector3(1.46f, 0.04f, 0.76f), Material("ClassDeskTop", new Color(0.5f, 0.32f, 0.2f), 0.4f), root, false);
            Block("Register", o + new Vector3(-3.3f, 1.08f, 3.3f), new Vector3(0.32f, 0.03f, 0.24f), Material("ClassRegister", new Color(0.15f, 0.25f, 0.5f)), root, false, 12f);
            Prop("Assets/_Game/Prefabs/Props/CeramicJar.prefab", o + new Vector3(-2.65f, 1.06f, 3.4f), 0, 0.25f, root);
        }

        private static void Desks(Transform root, Vector3 o, Material wood, Material metal, Parts parts)
        {
            var random = new System.Random(15);
            var bag = new[] { Material("ClassBagA", new Color(0.18f, 0.24f, 0.45f)), Material("ClassBagB", new Color(0.55f, 0.12f, 0.15f)), Material("ClassBagC", new Color(0.2f, 0.2f, 0.2f)) };
            for (int r = 0; r < Rows.Length; r++)
                for (int c = 0; c < Columns.Length; c++)
                {
                    Vector3 d = o + new Vector3(Columns[c], 0, Rows[r]);
                    Block("DeskTop", d + new Vector3(0, 0.74f, 0), new Vector3(1.24f, 0.04f, 0.46f), wood, root);
                    Block("DeskFront", d + new Vector3(0, 0.5f, 0.21f), new Vector3(1.2f, 0.44f, 0.03f), wood, root, false);
                    Block("DeskShelf", d + new Vector3(0, 0.56f, 0.02f), new Vector3(1.16f, 0.02f, 0.4f), wood, root, false);
                    foreach (int side in new[] { -1, 1 })
                    {
                        Block("DeskLeg", d + new Vector3(side * 0.58f, 0.36f, 0), new Vector3(0.04f, 0.72f, 0.42f), metal, root, false);
                        Block("BenchLeg", d + new Vector3(side * 0.56f, 0.21f, -SeatBehindDesk - 0.3f), new Vector3(0.04f, 0.42f, 0.24f), metal, root, false);
                    }
                    Block("Bench", d + new Vector3(0, 0.44f, -SeatBehindDesk - 0.3f), new Vector3(1.2f, 0.04f, 0.3f), wood, root, false);
                    // A few things left on the desks.
                    if (random.NextDouble() < 0.6)
                        Block("Notebook", d + new Vector3((float)random.NextDouble() * 0.8f - 0.4f, 0.765f, 0), new Vector3(0.21f, 0.01f, 0.29f),
                            Material("ClassNotebook", new Color(0.92f, 0.9f, 0.82f)), root, false, (float)random.NextDouble() * 30f - 15f);
                    if (random.NextDouble() < 0.3)
                    {
                        var bottle = Cylinder("Bottle", d + new Vector3((float)random.NextDouble() * 0.9f - 0.45f, 0.76f, 0.12f), 0.035f, 0.22f, Material("ClassBottle", new Color(0.55f, 0.75f, 0.9f), 0.85f), root, false);
                        Cylinder("BottleCap", bottle.transform.position + Vector3.up * 0.22f, 0.02f, 0.03f, Material("ClassBottleCap", new Color(0.2f, 0.45f, 0.85f), 0.5f), root, false);
                    }
                    if (random.NextDouble() < 0.45)
                        Block("Bag", d + new Vector3((float)random.NextDouble() * 0.6f - 0.3f, 0.55f, -0.02f), new Vector3(0.32f, 0.12f, 0.24f), bag[random.Next(bag.Length)], root, false);
                    // Seats face the board (north). The student sits third row, right column, by the window.
                    for (int side = -1; side <= 1; side += 2)
                    {
                        var seat = Point("Seat_" + r + "_" + c + (side < 0 ? "L" : "R"), d + new Vector3(side * 0.3f, 0f, -SeatBehindDesk), 0f, root);
                        if (r == 2 && c == 2 && side < 0) parts.Seat = seat;
                        else if (r == 2 && c == 2) parts.NganSeat = seat;
                        else parts.ClassmateSeats.Add(seat);
                    }
                    if (r == 2 && c == 2) StudentBook(root, d + new Vector3(-0.3f, 0.76f, -0.02f), parts);
                }
        }

        /// <summary>Her open copy of Truyện Kiều, a pencil case, the glow for the prologue and the words for the epilogue.</summary>
        private static void StudentBook(Transform root, Vector3 top, Parts parts)
        {
            var anchor = Point("Book", top, 0f, root);
            parts.BookAnchor = anchor;
            var paper = Material("ClassPage", new Color(0.95f, 0.92f, 0.84f), 0.05f);
            var cover = Material("ClassCover", new Color(0.55f, 0.12f, 0.1f), 0.3f);
            Block("BookCover", top + new Vector3(0, 0.002f, 0), new Vector3(0.44f, 0.006f, 0.3f), cover, root, false);
            foreach (int side in new[] { -1, 1 })
            {
                var page = Block("BookPage", top + new Vector3(side * 0.105f, 0.014f, 0), new Vector3(0.2f, 0.014f, 0.28f), paper, root, false);
                page.transform.rotation = Quaternion.Euler(0, 0, side * -4f);
            }
            WorldText(root, "BookVerses", "<i>Duyên hội ngộ, đức cù lao,\nBên tình bên hiếu bên nào nặng hơn?\nĐể lời thệ hải minh sơn,\nLàm con trước phải đền ơn sinh thành.</i>",
                top + new Vector3(-0.105f, 0.024f, 0.01f), 0f, new Vector2(0.18f, 0.24f), 0.018f, new Color(0.2f, 0.14f, 0.1f), true);
            Block("PencilCase", top + new Vector3(0.36f, 0.02f, 0.05f), new Vector3(0.2f, 0.04f, 0.06f), Material("ClassPencilCase", new Color(0.25f, 0.45f, 0.75f)), root, false, 8f);
            // Prologue: the pages light up.
            var glow = new GameObject("BookGlow");
            glow.transform.SetParent(root, false);
            glow.transform.position = top + Vector3.up * 0.18f;
            var light = glow.AddComponent<Light>();
            light.type = LightType.Point; light.color = new Color(1f, 0.82f, 0.45f); light.range = 3.5f; light.intensity = 3.2f; light.shadows = LightShadows.None;
            var halo = Block("GlowPlane", top + new Vector3(0, 0.03f, 0), new Vector3(0.42f, 0.002f, 0.28f), Emissive("ClassBookGlow", new Color(1f, 0.85f, 0.5f), 2.2f), glow.transform, false);
            halo.transform.SetParent(glow.transform, true);
            Sphere("GlowMotes", top + Vector3.up * 0.25f, Vector3.one * 0.05f, Emissive("ClassBookGlow", new Color(1f, 0.85f, 0.5f), 2.2f), glow.transform);
            Dust("GlowDust", top + Vector3.up * 0.3f, new Vector3(0.4f, 0.4f, 0.3f), glow.transform, new Color(1f, 0.85f, 0.5f, 0.6f), 40);
            parts.BookGlow = glow;
            glow.SetActive(false);
            // Epilogue: "Cảm ơn." in small handwriting at the foot of the right page.
            parts.ThankYou = WorldText(root, "ThankYou", "<i>Cảm ơn.</i>", top + new Vector3(0.12f, 0.025f, -0.1f), 0f, new Vector2(0.16f, 0.04f), 0.02f, new Color(0.45f, 0.08f, 0.05f), true);
            parts.ThankYou.gameObject.SetActive(false); // written only in the epilogue (RevealSequence)
        }

        private static void Dressing(Transform root, Vector3 o, Material metal, Material wood)
        {
            // Back wall: a notice board, a cupboard and a broom corner; the clock above the door side.
            Block("NoticeBoard", o + new Vector3(1.5f, 1.6f, -4.47f), new Vector3(2.6f, 1.2f, 0.04f), Material("ClassCork", new Color(0.62f, 0.45f, 0.3f)), root, false);
            var random = new System.Random(5);
            for (int i = 0; i < 7; i++)
                Block("Notice", o + new Vector3(0.5f + (float)random.NextDouble() * 2f, 1.25f + (float)random.NextDouble() * 0.7f, -4.44f),
                    new Vector3(0.22f + (float)random.NextDouble() * 0.15f, 0.28f, 0.005f), Material(i % 3 == 0 ? "ClassNoticeA" : i % 3 == 1 ? "ClassNoticeB" : "ClassNoticeC",
                        i % 3 == 0 ? new Color(0.95f, 0.93f, 0.85f) : i % 3 == 1 ? new Color(0.98f, 0.88f, 0.55f) : new Color(0.7f, 0.85f, 0.95f)), root, false, (float)random.NextDouble() * 10f - 5f);
            WorldText(root, "ClassSign", "<b>LỚP 10A2</b>", o + new Vector3(1.5f, 2.4f, -4.45f), 180f, new Vector2(1.6f, 0.3f), 0.2f, new Color(0.25f, 0.2f, 0.15f));
            Block("Cupboard", o + new Vector3(-2.6f, 0.9f, -4.25f), new Vector3(1.6f, 1.8f, 0.45f), wood, root);
            Block("CupboardLine", o + new Vector3(-2.6f, 0.9f, -4.02f), new Vector3(0.02f, 1.7f, 0.01f), metal, root, false);
            Cylinder("Broom", o + new Vector3(-4.6f, 0f, -4.2f), 0.02f, 1.3f, Material("ClassBroom", new Color(0.7f, 0.6f, 0.35f)), root, false);
            Cylinder("Bin", o + new Vector3(-4.5f, 0f, -3.6f), 0.18f, 0.42f, Material("ClassBin", new Color(0.2f, 0.45f, 0.3f), 0.4f), root, false);
            // Clock on the west wall.
            var face = Cylinder("ClockFace", o + new Vector3(-4.98f, 2.55f, 1.5f), 0.2f, 0.03f, Material("ClassClock", new Color(0.95f, 0.95f, 0.92f), 0.6f), root, false);
            face.transform.rotation = Quaternion.Euler(0, 0, 90f);
            face.transform.position = o + new Vector3(-4.96f, 2.55f, 1.5f);
            Block("ClockHand", o + new Vector3(-4.94f, 2.6f, 1.5f), new Vector3(0.01f, 0.13f, 0.015f), metal, root, false);
            Block("ClockHand", o + new Vector3(-4.94f, 2.55f, 1.55f), new Vector3(0.01f, 0.015f, 0.1f), metal, root, false);
            // The national flag above the board's left side, a timetable by the door, the month's calendar, the teacher's
            // chair, a fire extinguisher.
            Block("Flag", o + new Vector3(-3.55f, 2.55f, 4.47f), new Vector3(0.9f, 0.6f, 0.02f), Material("ClassFlagRed", new Color(0.78f, 0.06f, 0.05f), 0.2f), root, false);
            Star(root, o + new Vector3(-3.55f, 2.55f, 4.455f), 0.17f, Material("ClassFlagStar", new Color(1f, 0.85f, 0.1f), 0.2f));
            Block("Timetable", o + new Vector3(-4.97f, 1.6f, -1.75f), new Vector3(0.01f, 0.75f, 0.6f), Material("ClassNoticeA", new Color(0.95f, 0.93f, 0.85f)), root, false);
            WorldText(root, "TimetableText", "<b>THỜI KHÓA BIỂU</b>\n<size=70%>Thứ 2: Văn · Toán · Anh\nThứ 3: Lý · Hóa · Sử\nThứ 4: Văn · Sinh · Địa\nThứ 5: Văn · Toán · GDCD\nThứ 6: Anh · Tin · Thể dục</size>",
                o + new Vector3(-4.96f, 1.6f, -1.75f), 270f, new Vector2(0.56f, 0.7f), 0.05f, new Color(0.15f, 0.15f, 0.2f));
            Block("Calendar", o + new Vector3(4.2f, 2.1f, 4.47f), new Vector3(0.42f, 0.56f, 0.01f), Material("ClassNoticeA", new Color(0.95f, 0.93f, 0.85f)), root, false);
            WorldText(root, "CalendarText", "<color=#B01010><b>15</b></color>\n<size=45%>THÁNG 5</size>", o + new Vector3(4.2f, 2.1f, 4.46f), 0f, new Vector2(0.4f, 0.5f), 0.16f, new Color(0.2f, 0.2f, 0.2f));
            Block("ChairSeat", o + new Vector3(-3.1f, 0.63f, 3.98f), new Vector3(0.44f, 0.04f, 0.42f), wood, root, false);
            Block("ChairBack", o + new Vector3(-3.1f, 0.92f, 4.18f), new Vector3(0.44f, 0.55f, 0.04f), wood, root, false);
            foreach (float x in new[] { -0.19f, 0.19f })
                foreach (float z in new[] { -0.18f, 0.18f })
                    Block("ChairLeg", o + new Vector3(-3.1f + x, 0.4f, 3.98f + z), new Vector3(0.03f, 0.44f, 0.03f), metal, root, false);
            var extinguisher = Cylinder("Extinguisher", o + new Vector3(-4.8f, 0f, -2.7f), 0.08f, 0.52f, Material("ClassExtinguisher", new Color(0.8f, 0.08f, 0.06f), 0.6f), root, false);
            Cylinder("ExtinguisherHead", extinguisher.transform.position + Vector3.up * 0.52f, 0.03f, 0.08f, metal, root, false);
            // A potted plant by the board and the class flag holder.
            PottedPlant(o + new Vector3(4.4f, 0.18f, 3.9f), 0.8f, root, 101);
            PottedPlant(o + new Vector3(-4.4f, 0.18f, 3.9f), 0.7f, root, 102);
        }

        /// <summary>The corridor outside the windows, its railing, trees in the yard and the opposite wing of the school.</summary>
        private static void Outside(Transform root, Vector3 o, bool sunset)
        {
            var tiles = Surface("ClassCorridor", "brick_floor", new Color(0.92f, 0.78f, 0.66f), 1.2f, 0.25f, 0.8f, 0.3f);
            var wall = Surface("ClassWall", "plastered_wall_02", new Color(1f, 0.9f, 0.62f), 2.2f, 0.12f, 0.6f, 0.25f);
            var green = Material("ClassFrame", new Color(0.24f, 0.42f, 0.36f), 0.45f);
            Block("Corridor", o + new Vector3(6.6f, -0.1f, 0f), new Vector3(2.8f, 0.2f, 13f), tiles, root);
            Block("CorridorWest", o + new Vector3(-6.5f, -0.1f, -1f), new Vector3(2.6f, 0.2f, 11f), tiles, root);
            Block("Railing", o + new Vector3(7.9f, 0.5f, 0f), new Vector3(0.12f, 1f, 13f), wall, root);
            Block("RailCap", o + new Vector3(7.9f, 1.03f, 0f), new Vector3(0.22f, 0.06f, 13f), green, root, false);
            Block("CorridorRoof", o + new Vector3(6.6f, 3.5f, 0f), new Vector3(3f, 0.2f, 13f), Material("ClassCeiling", new Color(0.93f, 0.92f, 0.88f), 0.1f), root, false);
            for (float z = -6f; z <= 6.1f; z += 3f) Block("CorridorPost", o + new Vector3(7.85f, 1.75f, z), new Vector3(0.22f, 3.5f, 0.22f), wall, root, false);
            var yard = Surface("ClassYard", "brick_floor", new Color(0.85f, 0.7f, 0.6f), 1.6f, 0.2f, 0.8f, 0.35f);
            Block("Yard", o + new Vector3(16f, -0.3f, 0f), new Vector3(16f, 0.2f, 24f), yard, root, false);
            Prop("Assets/_Game/Prefabs/Vegetation/BananaTree.prefab", o + new Vector3(11f, -0.2f, -3.5f), 30, 1.1f, root);
            Prop("Assets/_Game/Prefabs/Vegetation/ArecaPalm.prefab", o + new Vector3(12.5f, -0.2f, 3f), 0, 1.2f, root);
            Prop("Assets/_Game/Prefabs/Vegetation/ArecaPalm.prefab", o + new Vector3(10.2f, -0.2f, 6.5f), 80, 1f, root);
            // A big flame tree (phượng) in the middle of the yard: a trunk and a wide crown of red blossom.
            var trunk = Material("ClassTrunk", new Color(0.32f, 0.25f, 0.2f));
            Cylinder("FlameTreeTrunk", o + new Vector3(15f, -0.2f, 0f), 0.35f, 4f, trunk, root, false);
            var leaves = Material("ClassFlameLeaves", sunset ? new Color(0.3f, 0.4f, 0.2f) : new Color(0.35f, 0.55f, 0.25f), 0.1f);
            var blossom = Material("ClassFlameBlossom", new Color(0.85f, 0.18f, 0.1f), 0.1f);
            var random = new System.Random(21);
            for (int i = 0; i < 9; i++)
            {
                var p = o + new Vector3(15f + (float)random.NextDouble() * 5f - 2.5f, 4.2f + (float)random.NextDouble() * 1.2f, (float)random.NextDouble() * 5f - 2.5f);
                Sphere("Crown", p, new Vector3(2.4f, 1.1f, 2.4f) * (0.8f + (float)random.NextDouble() * 0.5f), i % 3 == 0 ? leaves : blossom, root);
            }
            // The opposite wing: a long yellow building with rows of green windows.
            Block("OppositeWing", o + new Vector3(25f, 3f, 0f), new Vector3(3f, 6.4f, 26f), wall, root, false);
            for (int floorIndex = 0; floorIndex < 2; floorIndex++)
                for (float z = -11f; z <= 11.1f; z += 2.6f)
                    Block("OppositeWindow", o + new Vector3(23.45f, 1.5f + floorIndex * 3f, z), new Vector3(0.05f, 1.4f, 1.3f), green, root, false);
            Block("OppositeRoof", o + new Vector3(25f, 6.35f, 0f), new Vector3(3.6f, 0.3f, 26.6f), Material("ClassRoofTile", new Color(0.62f, 0.3f, 0.22f)), root, false);
            if (sunset) Fireflies("EveningMotes", o + new Vector3(16f, 2f, 0f), new Vector3(8f, 3f, 14f), root, 25);
        }

        private static void Fans(Transform root, Vector3 o, Material metal)
        {
            var blade = Material("ClassFanBlade", new Color(0.9f, 0.9f, 0.86f), 0.3f);
            foreach (float x in new[] { -2.4f, 2.4f })
            {
                Cylinder("FanRod", o + new Vector3(x, 2.95f, 0.2f), 0.02f, 0.45f, metal, root, false);
                var hub = new GameObject("FanHub");
                hub.transform.SetParent(root, false);
                hub.transform.position = o + new Vector3(x, 2.95f, 0.2f);
                Sphere("Hub", hub.transform.position, new Vector3(0.18f, 0.1f, 0.18f), metal, hub.transform);
                for (int i = 0; i < 3; i++)
                {
                    var b = Block("Blade", hub.transform.position, new Vector3(0.62f, 0.012f, 0.12f), blade, hub.transform, false, i * 120f);
                    b.transform.position = hub.transform.position + Quaternion.Euler(0, i * 120f, 0) * Vector3.right * 0.38f;
                }
                hub.AddComponent<ThuyKieu.Environment.Spin>();
            }
        }

        private static void Lights(Transform root, Vector3 o, bool sunset)
        {
            var tube = Emissive(sunset ? "ClassTubeOff" : "ClassTube", sunset ? new Color(0.85f, 0.85f, 0.82f) : new Color(0.95f, 0.98f, 1f), sunset ? 0.05f : 1.4f);
            foreach (float x in new[] { -2.6f, 2.6f })
                foreach (float z in new[] { -2f, 2f })
                    Block("Tube", o + new Vector3(x, 3.37f, z), new Vector3(0.1f, 0.05f, 1.2f), tube, root, false);
            if (sunset) return;
            foreach (float z in new[] { -2f, 2f })
            {
                var go = new GameObject("RoomFill");
                go.transform.SetParent(root, false);
                go.transform.position = o + new Vector3(0, 3f, z);
                var light = go.AddComponent<Light>();
                light.type = LightType.Point; light.color = new Color(0.95f, 0.97f, 1f); light.range = 8f; light.intensity = 1.05f; light.shadows = LightShadows.None;
            }
        }

        /// <summary>
        /// Flat world-space text (chalk on the board, ink on a page, a sign) in the shared Vietnamese font.
        /// <paramref name="lineHeight"/> and <paramref name="size"/> are in metres; yaw 0 reads from the south (facing +Z),
        /// lying text faces up with its top towards +Z.
        /// </summary>
        public static TMP_Text WorldText(Transform parent, string name, string text, Vector3 position, float yaw, Vector2 size, float lineHeight, Color color, bool lying = false)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.rotation = lying ? Quaternion.Euler(90f, yaw, 0f) : Quaternion.Euler(0f, yaw, 0f);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            tmp.text = text;
            tmp.fontSize = lineHeight * 100f; // at a 0.1 scale, TMP font size 10 is about 0.1 m
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.richText = true;
            tmp.rectTransform.localScale = Vector3.one * 0.1f;
            ((RectTransform)go.transform).sizeDelta = size * 10f;
            return tmp;
        }
    }
}
