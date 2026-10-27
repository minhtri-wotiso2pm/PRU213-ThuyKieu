using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThuyKieu.Environment
{
    /// <summary>
    /// Mesh building for bamboo: culms (tapering, bowed tubes whose nodes are drawn by the texture, one internode per
    /// repeat), twigs, leaf sprays (bent, folded cards hanging off the twigs), canopy clumps, low growth, roots and far
    /// silhouette cards. Many pieces go into one <see cref="Builder"/> so a grove is a few draw calls. Plain lists only:
    /// safe to fill on a worker thread; the <see cref="Mesh"/> is made on the main thread.
    /// A <see cref="Builder.Windy"/> builder writes how each vertex moves in the wind into its colour (see Ch5Bamboo.shader:
    /// r bend, g flutter, b phase); any other builder writes zeros, so what it makes (the obstacles) stays still.
    /// </summary>
    public static class BambooMesh
    {
        public sealed class Builder
        {
            public readonly List<Vector3> Vertices = new List<Vector3>();
            public readonly List<Vector3> Normals = new List<Vector3>();
            public readonly List<Vector4> Tangents = new List<Vector4>();
            public readonly List<Vector2> Uvs = new List<Vector2>();
            public readonly List<Color> Colors = new List<Color>();
            public readonly List<int>[] Triangles;
            /// <summary>What it makes moves in the wind.</summary>
            public bool Windy;

            public Builder(int submeshes)
            {
                Triangles = new List<int>[submeshes];
                for (int i = 0; i < submeshes; i++) Triangles[i] = new List<int>();
            }

            public bool Empty => Vertices.Count == 0;

            /// <summary>The wind weights of a vertex: how far it bends with its culm (0 at the foot), how much it flutters, its phase.</summary>
            public Color Weights(float bend, float flutter, float phase) =>
                Windy ? new Color(Mathf.Clamp01(bend), Mathf.Clamp01(flutter), phase - Mathf.Floor(phase), 0f) : new Color(0f, 0f, 0f, 0f);

            public Mesh ToMesh(string name)
            {
                var mesh = new Mesh { name = name, indexFormat = Vertices.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
                mesh.SetVertices(Vertices);
                mesh.SetNormals(Normals);
                mesh.SetTangents(Tangents);
                mesh.SetUVs(0, Uvs);
                mesh.SetColors(Colors);
                mesh.subMeshCount = Triangles.Length;
                for (int i = 0; i < Triangles.Length; i++) mesh.SetTriangles(Triangles[i], i, false);
                mesh.RecalculateBounds();
                return mesh;
            }

            /// <summary>A quad a–b–c–d (counter-clockwise seen from its front), lit as <paramref name="normal"/>.</summary>
            public void Quad(int submesh, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal, Vector2 uvMin, Vector2 uvMax, Color low = default, Color high = default)
            {
                int start = Vertices.Count;
                Vertices.Add(a); Vertices.Add(b); Vertices.Add(c); Vertices.Add(d);
                Vector3 t = (b - a).normalized;
                var tangent = new Vector4(t.x, t.y, t.z, 1f);
                for (int i = 0; i < 4; i++) { Normals.Add(normal); Tangents.Add(tangent); }
                Colors.Add(low); Colors.Add(low); Colors.Add(high); Colors.Add(high);
                Uvs.Add(new Vector2(uvMin.x, uvMin.y)); Uvs.Add(new Vector2(uvMax.x, uvMin.y));
                Uvs.Add(new Vector2(uvMax.x, uvMax.y)); Uvs.Add(new Vector2(uvMin.x, uvMax.y));
                var tris = Triangles[submesh];
                tris.Add(start); tris.Add(start + 2); tris.Add(start + 1);
                tris.Add(start); tris.Add(start + 3); tris.Add(start + 2);
            }
        }

        /// <summary>
        /// A culm from <paramref name="foot"/> along <paramref name="axis"/> (unit), bowing sideways by up to
        /// <paramref name="bow"/> metres towards <paramref name="bowDirection"/>. Rings every <paramref name="ringEvery"/>
        /// metres; the texture repeats every <paramref name="node"/> metres so each repeat is one internode. In the wind it
        /// bends from its foot, a tall culm more than a short one, by <paramref name="sway"/> (0: lying, or a root).
        /// </summary>
        public static void Culm(Builder b, int submesh, Vector3 foot, Vector3 axis, float length, float radius, float bow, Vector3 bowDirection,
            int sides, float ringEvery, float node, float uOffset = 0f, float taper = 0.45f, float sway = 1f)
        {
            int rings = Mathf.Max(2, Mathf.CeilToInt(length / ringEvery) + 1);
            Vector3 side = Vector3.Cross(axis, Mathf.Abs(axis.y) > 0.95f ? Vector3.right : Vector3.up).normalized;
            Vector3 other = Vector3.Cross(axis, side);
            int start = b.Vertices.Count;
            float reach = Mathf.Clamp01(length / 10f) * sway;
            for (int r = 0; r < rings; r++)
            {
                float t = r / (float)(rings - 1);
                Vector3 centre = foot + axis * length * t + bowDirection * bow * t * t;
                Vector3 tangent = (axis * length + bowDirection * bow * 2f * t).normalized;
                Vector3 s = Vector3.Cross(tangent, other).normalized, o = Vector3.Cross(s, tangent);
                float rad = radius * Mathf.Lerp(1f, taper, t * t);
                Color weights = b.Weights(t * reach, 0f, uOffset);
                for (int k = 0; k <= sides; k++)
                {
                    float angle = k / (float)sides * Mathf.PI * 2f;
                    float cos = Mathf.Cos(angle), sin = Mathf.Sin(angle);
                    Vector3 n = s * cos + o * sin;
                    b.Vertices.Add(centre + n * rad);
                    b.Normals.Add(n);
                    // Around the culm (+u); the bitangent n × t then runs down it, so w = −1 puts +v up the culm.
                    Vector3 around = o * cos - s * sin;
                    b.Tangents.Add(new Vector4(around.x, around.y, around.z, -1f));
                    b.Uvs.Add(new Vector2(k / (float)sides + uOffset, length * t / node));
                    b.Colors.Add(weights);
                }
            }
            var tris = b.Triangles[submesh];
            for (int r = 0; r + 1 < rings; r++)
                for (int k = 0; k < sides; k++)
                {
                    int a = start + r * (sides + 1) + k, c = a + sides + 1;
                    tris.Add(a); tris.Add(c); tris.Add(a + 1);
                    tris.Add(a + 1); tris.Add(c); tris.Add(c + 1);
                }
        }

        /// <summary>Where a culm built by <see cref="Culm"/> passes at <paramref name="t"/> (0 foot, 1 tip).</summary>
        public static Vector3 Along(Vector3 foot, Vector3 axis, float length, float bow, Vector3 bowDirection, float t)
            => foot + axis * length * t + bowDirection * bow * t * t;

        /// <summary>
        /// A branch from a node of a culm out to where its leaves hang: a thin three-sided tube, thinning towards its end,
        /// moving with the culm at <paramref name="bend"/> and a little more at its end.
        /// </summary>
        public static void Twig(Builder b, int submesh, Vector3 from, Vector3 to, float radius, float bend, float phase)
        {
            Vector3 axis = to - from;
            float length = axis.magnitude;
            if (length < 0.01f) return;
            axis /= length;
            Vector3 side = Vector3.Cross(axis, Mathf.Abs(axis.y) > 0.95f ? Vector3.right : Vector3.up).normalized;
            Vector3 other = Vector3.Cross(axis, side);
            int start = b.Vertices.Count;
            const int sides = 3;
            for (int r = 0; r < 2; r++)
            {
                Vector3 centre = r == 0 ? from : to;
                float rad = radius * (r == 0 ? 1f : 0.35f);
                Color weights = b.Weights(bend + r * 0.05f, r * 0.35f, phase);
                for (int k = 0; k <= sides; k++)
                {
                    float angle = k / (float)sides * Mathf.PI * 2f;
                    Vector3 n = side * Mathf.Cos(angle) + other * Mathf.Sin(angle);
                    b.Vertices.Add(centre + n * rad);
                    b.Normals.Add(n);
                    b.Tangents.Add(new Vector4(axis.x, axis.y, axis.z, 1f));
                    b.Uvs.Add(new Vector2(k / (float)sides, r * length / 0.4f));
                    b.Colors.Add(weights);
                }
            }
            var tris = b.Triangles[submesh];
            for (int k = 0; k < sides; k++)
            {
                int a = start + k, c = a + sides + 1;
                tris.Add(a); tris.Add(c); tris.Add(a + 1);
                tris.Add(a + 1); tris.Add(c); tris.Add(c + 1);
            }
        }

        /// <summary>
        /// A spray of leaves: a card hanging from <paramref name="at"/> out along <paramref name="outward"/>, curving down
        /// more towards its tip (<paramref name="droop"/>) and folded along its middle, so seen from the side it is a
        /// drooping fan of leaves rather than a flat plate. <paramref name="variant"/> picks one of the four sprays of the
        /// leaf atlas. The normals lean up so the leaves light like the canopy they belong to, never as black backs.
        /// Not <paramref name="folded"/> (the middle distance, where nobody sees it edge-on): one flat drooping card.
        /// </summary>
        public static void Spray(Builder b, int submesh, Vector3 at, Vector3 outward, float width, float height, float droop, bool flip,
            int variant = 0, float bend = 0.8f, float phase = 0f, bool folded = true)
        {
            if (!folded)
            {
                FlatSpray(b, submesh, at, outward, width, height, droop, flip, variant, bend, phase);
                return;
            }
            outward.y = 0f;
            outward = outward.sqrMagnitude > 0.0001f ? outward.normalized : Vector3.forward;
            Vector3 across = Vector3.Cross(Vector3.up, outward).normalized;
            across = (across + Vector3.down * 0.25f).normalized;
            Vector3 face = Vector3.Cross((outward + Vector3.down * droop * 0.5f).normalized, across).normalized;
            Vector3 normal = (face * 0.4f + Vector3.up).normalized;
            var uv0 = new Vector2((variant & 1) * 0.5f, ((variant >> 1) & 1) * 0.5f);
            int start = b.Vertices.Count;
            for (int i = 0; i < 3; i++)
            {
                float k = i * 0.5f;
                Vector3 spine = at + outward * width * k + Vector3.down * width * droop * (0.15f * k + 0.45f * k * k);
                Vector3 along = (outward + Vector3.down * droop * (0.15f + 0.9f * k)).normalized;
                var tangent = new Vector4(along.x, along.y, along.z, 1f);
                Color weights = b.Weights(bend + k * 0.08f, 0.35f + 0.65f * k, phase);
                for (int j = 0; j < 3; j++)
                {
                    float c = j - 1f; // −1, 0, 1 across
                    Vector3 p = spine + across * height * 0.5f * c * (1f - 0.15f * k);
                    if (j == 1) p += face * height * 0.12f * (1f - 0.5f * k); // the fold along the twig
                    b.Vertices.Add(p);
                    b.Normals.Add(normal);
                    b.Tangents.Add(tangent);
                    float v = flip ? 1f - j * 0.5f : j * 0.5f;
                    b.Uvs.Add(uv0 + new Vector2(k, v) * 0.5f);
                    b.Colors.Add(weights);
                }
            }
            var tris = b.Triangles[submesh];
            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                {
                    int a = start + i * 3 + j, c = a + 3;
                    tris.Add(a); tris.Add(a + 1); tris.Add(c + 1);
                    tris.Add(a); tris.Add(c + 1); tris.Add(c);
                }
        }

        private static void FlatSpray(Builder b, int submesh, Vector3 at, Vector3 outward, float width, float height, float droop, bool flip, int variant, float bend, float phase)
        {
            outward.y = 0f;
            outward = outward.sqrMagnitude > 0.0001f ? outward.normalized : Vector3.forward;
            Vector3 along = (outward + Vector3.down * droop).normalized;
            Vector3 across = (Vector3.Cross(Vector3.up, outward).normalized + Vector3.down * 0.25f).normalized;
            Vector3 normal = (Vector3.Cross(along, across).normalized * 0.4f + Vector3.up).normalized;
            var uv0 = new Vector2((variant & 1) * 0.5f, ((variant >> 1) & 1) * 0.5f);
            int start = b.Vertices.Count;
            Vector3 a = at - across * height * 0.5f, c = at + across * height * 0.5f;
            b.Vertices.Add(a); b.Vertices.Add(c); b.Vertices.Add(c + along * width); b.Vertices.Add(a + along * width);
            var tangent = new Vector4(along.x, along.y, along.z, 1f);
            Color root = b.Weights(bend, 0.35f, phase), tip = b.Weights(bend + 0.08f, 1f, phase);
            for (int i = 0; i < 4; i++) { b.Normals.Add(normal); b.Tangents.Add(tangent); }
            b.Colors.Add(root); b.Colors.Add(root); b.Colors.Add(tip); b.Colors.Add(tip);
            float v0 = flip ? 0.5f : 0f, v1 = flip ? 0f : 0.5f;
            b.Uvs.Add(uv0 + new Vector2(0f, v0)); b.Uvs.Add(uv0 + new Vector2(0f, v1)); b.Uvs.Add(uv0 + new Vector2(0.5f, v1)); b.Uvs.Add(uv0 + new Vector2(0.5f, v0));
            var tris = b.Triangles[submesh];
            tris.Add(start); tris.Add(start + 1); tris.Add(start + 2);
            tris.Add(start); tris.Add(start + 2); tris.Add(start + 3);
        }

        /// <summary>
        /// Three crossed cards (low leaves, a bush when <paramref name="height"/> is more than its width), each leaning out
        /// a little from the middle so the tuft opens like a plant and is not three flat boards; the tops flutter.
        /// </summary>
        public static void Tuft(Builder b, int submesh, Vector3 foot, float size, float yaw, float height = -1f)
        {
            float tall = height > 0f ? height : size;
            Color low = b.Weights(0f, 0f, yaw / 360f), high = b.Weights(0.05f, 0.8f, yaw / 360f);
            for (int q = 0; q < 3; q++)
            {
                float angle = (yaw + q * 60f) * Mathf.Deg2Rad;
                Vector3 side = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * size * 0.6f;
                Vector3 lean = new Vector3(-Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * tall * (q == 1 ? -0.18f : 0.18f);
                b.Quad(submesh, foot - side, foot + side, foot + side + Vector3.up * tall + lean, foot - side + Vector3.up * tall + lean, Vector3.up, Vector2.zero, Vector2.one, low, high);
            }
        }

        /// <summary>A flat card on the ground (fallen leaves), following its slope.</summary>
        public static void Litter(Builder b, int submesh, Vector3 centre, Vector3 normal, float size, float yaw)
        {
            float a = yaw * Mathf.Deg2Rad;
            Vector3 x = Vector3.Cross(normal, new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a))).normalized * size * 0.5f;
            Vector3 z = Vector3.Cross(x, normal).normalized * size * 0.5f;
            b.Quad(submesh, centre - x - z, centre + x - z, centre + x + z, centre - x + z, normal, Vector2.zero, Vector2.one);
        }

        /// <summary>
        /// A clump of the canopy seen from below: two leaf cards crossed at a slant around <paramref name="centre"/>, drooping
        /// at the rim, so the roof of the forest is many overlapping clumps with sky between them, never a flat lid.
        /// </summary>
        public static void Canopy(Builder b, int submesh, Vector3 centre, float size, float yaw, float tilt)
        {
            Color low = b.Weights(0.85f, 0.5f, yaw / 360f), high = b.Weights(1f, 1f, yaw / 360f);
            for (int q = 0; q < 2; q++)
            {
                // No Quaternion here: this runs on worker threads.
                float y = (yaw + q * 90f) * Mathf.Deg2Rad, t = (q == 0 ? tilt : -tilt * 0.7f) * Mathf.Deg2Rad;
                var right = new Vector3(Mathf.Cos(y), 0f, -Mathf.Sin(y));
                var forward = new Vector3(Mathf.Sin(y), 0f, Mathf.Cos(y)) * Mathf.Cos(t) + Vector3.up * Mathf.Sin(t);
                Vector3 x = right * size * 0.5f, z = forward * size * 0.5f;
                Vector3 droop = Vector3.down * size * 0.12f;
                b.Quad(submesh, centre - x - z + droop, centre + x - z + droop, centre + x + z, centre - x + z, Vector3.down * 0.3f + Vector3.up, Vector2.zero, Vector2.one, low, high);
            }
        }

        /// <summary>
        /// An upright card standing on the ground at <paramref name="foot"/>, facing <paramref name="facing"/> (flat): the far
        /// forest drawn as one picture, culms and crowns, a few metres wide and as tall as the bamboo; its top sways a little.
        /// </summary>
        public static void Silhouette(Builder b, int submesh, Vector3 foot, Vector3 facing, float width, float height, bool flip)
        {
            facing.y = 0f;
            facing = facing.sqrMagnitude > 0.0001f ? facing.normalized : Vector3.forward;
            Vector3 right = Vector3.Cross(Vector3.up, facing) * width * 0.5f;
            Vector2 uv0 = flip ? new Vector2(1f, 0f) : Vector2.zero, uv1 = flip ? new Vector2(0f, 1f) : Vector2.one;
            float phase = (foot.x + foot.z) * 0.07f;
            b.Quad(submesh, foot - right, foot + right, foot + right + Vector3.up * height, foot - right + Vector3.up * height, (facing + Vector3.up * 0.6f).normalized, uv0, uv1,
                b.Weights(0f, 0f, phase), b.Weights(0.45f, 0f, phase));
        }
    }
}
