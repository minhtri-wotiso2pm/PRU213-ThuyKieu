using ThuyKieu.Environment;
using UnityEditor;
using UnityEngine;
using static ThuyKieu.Core.Editor.StorySceneKit;
using Random = System.Random;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Furniture for the prologue, Chapter 7 and the epilogue: a dressing table with a working bronze mirror, the
    /// hồ cầm, an embroidery frame, a flower vase, the yamen's placards, stools and kneeling mat.
    /// </summary>
    public static partial class StoryPropKit
    {
        private static Material Silk(Color color) => Material("PropSilk_" + ColorUtility.ToHtmlStringRGB(color), color, 0.35f);

        /// <summary>The ThuyKieu/StoryMirror material (reflection filled in at runtime by <see cref="PlanarMirror"/>).</summary>
        public static Material MirrorMaterial(Color tint, float haze)
        {
            string path = AssetFolder + "/StoryMirror_" + ColorUtility.ToHtmlStringRGB(tint) + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find("ThuyKieu/StoryMirror"));
            material.SetColor("_BaseColor", tint);
            material.SetFloat("_Haze", haze);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        /// <summary>
        /// Dressing table with a round bronze mirror on a stand (faces local +Z, the room side). The mirror really
        /// reflects: its face carries a <see cref="PlanarMirror"/>. Returns the mirror's centre, on its face.
        /// </summary>
        public static Transform Vanity(Vector3 position, float yaw, Transform parent, int seed)
        {
            var random = new Random(seed);
            var root = Root("Vanity", position, yaw, parent);
            var wood = WoodDark;
            // Table: lacquered top, apron with a drawer, turned legs.
            Box(root, "Top", new Vector3(0, 0.74f, 0), new Vector3(0.95f, 0.04f, 0.44f), Lacquer);
            Box(root, "Apron", new Vector3(0, 0.66f, 0.2f), new Vector3(0.9f, 0.12f, 0.02f), wood);
            Box(root, "Drawer", new Vector3(0, 0.66f, 0.215f), new Vector3(0.36f, 0.09f, 0.01f), Lacquer);
            Part(root, PrimitiveType.Sphere, "Pull", new Vector3(0, 0.66f, 0.225f), Vector3.one * 0.022f, Brass);
            foreach (float x in new[] { -0.42f, 0.42f })
                foreach (float z in new[] { -0.17f, 0.17f })
                    Post(root, "Leg", new Vector3(x, 0, z), 0.025f, 0.72f, wood);
            // Mirror stand: two posts and a carved cross rail holding the disc in a bronze rim.
            float centre = 1.43f, radius = 0.22f;
            foreach (float x in new[] { -0.27f, 0.27f }) Post(root, "MirrorPost", new Vector3(x, 0.76f, -0.1f), 0.018f, centre - 0.76f + radius + 0.08f, wood);
            Box(root, "MirrorRail", new Vector3(0, centre + radius + 0.08f, -0.1f), new Vector3(0.64f, 0.035f, 0.035f), Lacquer);
            Box(root, "MirrorFoot", new Vector3(0, 0.78f, -0.1f), new Vector3(0.64f, 0.04f, 0.14f), wood);
            Rod(root, "MirrorArm", new Vector3(-0.27f, centre, -0.1f), new Vector3(0.27f, centre, -0.1f), 0.012f, Brass);
            Part(root, PrimitiveType.Cylinder, "MirrorRim", new Vector3(0, centre, -0.085f), new Vector3(radius * 2f + 0.06f, 0.012f, radius * 2f + 0.06f), Brass, new Vector3(90, 0, 0));
            Part(root, PrimitiveType.Cylinder, "MirrorBack", new Vector3(0, centre, -0.1f), new Vector3(radius * 2f + 0.02f, 0.015f, radius * 2f + 0.02f), Bronze, new Vector3(90, 0, 0));
            var face = Part(root, PrimitiveType.Cylinder, "MirrorFace", new Vector3(0, centre, -0.072f), new Vector3(radius * 2f, 0.004f, radius * 2f),
                MirrorMaterial(new Color(1f, 0.88f, 0.66f), 0.14f), new Vector3(90, 0, 0), false);
            var plane = new GameObject("MirrorPlane").transform;
            plane.SetParent(root, false);
            plane.localPosition = new Vector3(0, centre, -0.068f);
            plane.localRotation = Quaternion.identity; // forward = +Z, the reflecting side
            var mirror = plane.gameObject.AddComponent<PlanarMirror>();
            Set(mirror, "_surface", face.GetComponent<Renderer>());
            // What a girl of a scholar's house keeps on it: comb, powder jar, a lacquer box, hairpins, a little vase.
            Box(root, "Comb", new Vector3(-0.3f, 0.77f, 0.08f), new Vector3(0.11f, 0.012f, 0.04f), wood, 18f);
            Part(root, PrimitiveType.Cylinder, "PowderJar", new Vector3(0.26f, 0.79f, 0.08f), new Vector3(0.08f, 0.03f, 0.08f), Porcelain);
            Part(root, PrimitiveType.Sphere, "PowderLid", new Vector3(0.26f, 0.82f, 0.08f), new Vector3(0.08f, 0.03f, 0.08f), BlueWare);
            Box(root, "JewelBox", new Vector3(0.36f, 0.8f, -0.02f), new Vector3(0.16f, 0.08f, 0.12f), Lacquer, -10f);
            Box(root, "JewelLid", new Vector3(0.36f, 0.845f, -0.02f), new Vector3(0.17f, 0.012f, 0.13f), Brass, -10f);
            for (int i = 0; i < 2; i++)
                Rod(root, "Hairpin", new Vector3(-0.12f + i * 0.05f, 0.765f, 0.12f), new Vector3(-0.02f + i * 0.05f, 0.765f, 0.02f), 0.004f, Brass, false);
            FlowerVase(root.TransformPoint(new Vector3(-0.38f, 0.76f, -0.08f)), root, seed + 1, 0.55f);
            Solid(root, new Vector3(0, 0.4f, 0), new Vector3(0.95f, 0.8f, 0.44f));
            return plane;
        }

        /// <summary>
        /// The hồ cầm (a pear-bodied lute) leaning against a wall behind it (local −Z), on a small cloth mat.
        /// </summary>
        public static GameObject HoCam(Vector3 position, float yaw, Transform parent, float lean = 14f)
        {
            var root = Root("HoCam", position, yaw, parent);
            Box(root, "Mat", new Vector3(0, 0.005f, 0.05f), new Vector3(0.5f, 0.01f, 0.36f), Silk(new Color(0.55f, 0.16f, 0.14f)), 0f, false);
            var lute = new GameObject("Lute").transform;
            lute.SetParent(root, false);
            lute.localPosition = new Vector3(0, 0.02f, 0.08f);
            lute.localRotation = Quaternion.Euler(-lean, 0, 0); // top towards the wall
            var body = Lacquer;
            var board = Material("PropSoundboard", new Color(0.78f, 0.6f, 0.38f), 0.45f);
            var strings = Material("PropSilkString", new Color(0.95f, 0.9f, 0.75f), 0.6f);
            Part(lute, PrimitiveType.Sphere, "Body", new Vector3(0, 0.3f, 0), new Vector3(0.34f, 0.52f, 0.08f), body);
            Part(lute, PrimitiveType.Sphere, "Soundboard", new Vector3(0, 0.3f, 0.032f), new Vector3(0.31f, 0.48f, 0.02f), board);
            Part(lute, PrimitiveType.Cylinder, "Rosette", new Vector3(0, 0.36f, 0.044f), new Vector3(0.05f, 0.002f, 0.05f), Ink, new Vector3(90, 0, 0), false);
            Box(lute, "Neck", new Vector3(0, 0.74f, 0.012f), new Vector3(0.05f, 0.4f, 0.035f), WoodDark);
            Box(lute, "Fingerboard", new Vector3(0, 0.74f, 0.031f), new Vector3(0.045f, 0.4f, 0.006f), Material("PropEbony", new Color(0.08f, 0.06f, 0.05f), 0.5f));
            var head = Part(lute, PrimitiveType.Cube, "Pegbox", new Vector3(0, 0.99f, -0.01f), new Vector3(0.055f, 0.14f, 0.04f), body);
            head.transform.localRotation = Quaternion.Euler(-22f, 0, 0);
            for (int i = 0; i < 4; i++)
            {
                float y = 0.95f + (i / 2) * 0.06f;
                float side = i % 2 == 0 ? -1f : 1f;
                Rod(lute, "Peg", new Vector3(side * 0.02f, y, -0.01f - (y - 0.95f) * 0.4f), new Vector3(side * 0.09f, y + 0.01f, -0.01f - (y - 0.95f) * 0.4f), 0.008f, WoodDark);
            }
            for (int f = 0; f < 6; f++)
                Box(lute, "Fret", new Vector3(0, 0.6f + f * 0.045f, 0.036f), new Vector3(0.05f, 0.008f, 0.008f), Porcelain, 0f, false);
            Box(lute, "Bridge", new Vector3(0, 0.15f, 0.046f), new Vector3(0.12f, 0.018f, 0.014f), WoodDark);
            for (int s = 0; s < 4; s++)
            {
                float x = -0.018f + s * 0.012f;
                Rod(lute, "String", new Vector3(x * 2.2f, 0.15f, 0.054f), new Vector3(x, 0.93f, 0.036f), 0.0012f, strings, false);
            }
            return root.gameObject;
        }

        /// <summary>A standing embroidery frame with a half-finished lotus on the silk (faces local +Z).</summary>
        public static GameObject EmbroideryFrame(Vector3 position, float yaw, Transform parent, int seed)
        {
            var random = new Random(seed);
            var root = Root("EmbroideryFrame", position, yaw, parent);
            foreach (float x in new[] { -0.32f, 0.32f })
            {
                Post(root, "Leg", new Vector3(x, 0, 0), 0.02f, 0.72f, WoodDark);
                Box(root, "Foot", new Vector3(x, 0.02f, 0), new Vector3(0.05f, 0.04f, 0.4f), WoodDark);
            }
            var hoop = new GameObject("Hoop").transform;
            hoop.SetParent(root, false);
            hoop.localPosition = new Vector3(0, 0.78f, 0);
            hoop.localRotation = Quaternion.Euler(-35f, 0, 0);
            Part(hoop, PrimitiveType.Cylinder, "Ring", Vector3.zero, new Vector3(0.6f, 0.015f, 0.6f), Lacquer);
            Part(hoop, PrimitiveType.Cylinder, "Silk", Vector3.up * 0.012f, new Vector3(0.56f, 0.002f, 0.56f), Silk(new Color(0.95f, 0.93f, 0.86f)), default, false);
            var petal = Silk(new Color(0.92f, 0.5f, 0.58f));
            var leaf = Silk(new Color(0.3f, 0.5f, 0.32f));
            for (int i = 0; i < 7; i++)
            {
                float a = i * 0.9f;
                Part(hoop, PrimitiveType.Sphere, "Petal", new Vector3(Mathf.Cos(a) * 0.06f, 0.016f, Mathf.Sin(a) * 0.06f + 0.03f), new Vector3(0.05f, 0.004f, 0.09f), petal, new Vector3(0, a * Mathf.Rad2Deg, 0), false);
            }
            for (int i = 0; i < 3; i++)
                Part(hoop, PrimitiveType.Sphere, "Leaf", new Vector3(-0.1f + i * 0.1f, 0.016f, -0.12f), new Vector3(0.1f, 0.004f, 0.05f), leaf, new Vector3(0, (float)random.NextDouble() * 60f - 30f, 0), false);
            Rod(hoop, "Needle", new Vector3(0.14f, 0.02f, 0.05f), new Vector3(0.2f, 0.05f, 0.06f), 0.002f, Brass, false);
            Rod(hoop, "Thread", new Vector3(0.2f, 0.05f, 0.06f), new Vector3(0.3f, -0.25f, 0.12f), 0.0015f, petal, false);
            Solid(root, new Vector3(0, 0.45f, 0), new Vector3(0.7f, 0.9f, 0.45f));
            return root.gameObject;
        }

        /// <summary>A porcelain vase with a few flowering stems (peach blossom by default).</summary>
        public static GameObject FlowerVase(Vector3 position, Transform parent, int seed, float scale = 1f)
        {
            var random = new Random(seed);
            var root = Root("FlowerVase", position, (float)random.NextDouble() * 360f, parent);
            root.localScale = Vector3.one * scale;
            Part(root, PrimitiveType.Sphere, "Belly", new Vector3(0, 0.1f, 0), new Vector3(0.16f, 0.2f, 0.16f), BlueWare);
            Post(root, "Throat", new Vector3(0, 0.18f, 0), 0.035f, 0.1f, BlueWare);
            Part(root, PrimitiveType.Cylinder, "Lip", new Vector3(0, 0.285f, 0), new Vector3(0.09f, 0.01f, 0.09f), Porcelain);
            var stem = Material("PropStem", new Color(0.24f, 0.17f, 0.12f), 0.1f);
            var bloom = Material("PropBlossom", new Color(1f, 0.72f, 0.78f), 0.2f);
            for (int i = 0; i < 4; i++)
            {
                float a = (float)random.NextDouble() * Mathf.PI * 2f;
                Vector3 tip = new Vector3(Mathf.Cos(a) * 0.14f, 0.55f + (float)random.NextDouble() * 0.2f, Mathf.Sin(a) * 0.14f);
                Rod(root, "Branch", new Vector3(0, 0.25f, 0), tip, 0.006f, stem, false);
                for (int b = 0; b < 5; b++)
                {
                    Vector3 p = Vector3.Lerp(new Vector3(0, 0.3f, 0), tip, 0.35f + b * 0.14f) + new Vector3((float)random.NextDouble() - 0.5f, 0, (float)random.NextDouble() - 0.5f) * 0.04f;
                    Part(root, PrimitiveType.Sphere, "Blossom", p, Vector3.one * (0.022f + (float)random.NextDouble() * 0.012f), bloom, default, false);
                }
            }
            return root.gameObject;
        }

        /// <summary>A yamen placard on a pole ("TÚC TĨNH", "HỒI TỴ"): red board, gold characters, faces local +Z.</summary>
        public static GameObject Placard(Vector3 position, float yaw, Transform parent, string text)
        {
            var root = Root("Placard", position, yaw, parent);
            Post(root, "Pole", Vector3.zero, 0.03f, 2.3f, WoodDark);
            Box(root, "Stand", new Vector3(0, 0.05f, 0), new Vector3(0.36f, 0.1f, 0.36f), Stone);
            Box(root, "Board", new Vector3(0, 1.75f, 0.035f), new Vector3(0.42f, 0.95f, 0.04f), Lacquer);
            Box(root, "Edge", new Vector3(0, 1.75f, 0.03f), new Vector3(0.48f, 1.01f, 0.03f), Brass);
            var label = ClassroomKit.WorldText(root, "PlacardText", "<b>" + string.Join("\n", text.Split(' ')) + "</b>", Vector3.zero, 0f, new Vector2(0.4f, 0.9f), 0.15f, new Color(1f, 0.82f, 0.36f));
            label.transform.SetParent(root, false);
            label.transform.localPosition = new Vector3(0, 1.75f, 0.058f);
            label.transform.localRotation = Quaternion.Euler(0, 180f, 0);
            return root.gameObject;
        }

        /// <summary>Low wooden stool.</summary>
        public static GameObject Stool(Vector3 position, float yaw, Transform parent)
        {
            var root = Root("Stool", position, yaw, parent);
            Box(root, "Seat", new Vector3(0, 0.42f, 0), new Vector3(0.42f, 0.05f, 0.3f), WoodDark);
            foreach (float x in new[] { -0.17f, 0.17f })
                foreach (float z in new[] { -0.11f, 0.11f })
                    Post(root, "Leg", new Vector3(x, 0, z), 0.018f, 0.4f, WoodDark);
            return root.gameObject;
        }
    }
}
