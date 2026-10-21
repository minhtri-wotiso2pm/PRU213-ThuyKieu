using System.Collections.Generic;
using System.Linq;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Chapters.LauTuBa;
using UnityEditor;
using UnityEngine;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Tú Bà's key ring for Chapter 4 (§16), seen at arm's length when Kiều takes it: an S-hook at her belt, a round
    /// wire ring with a hole, five bronze keys of different lengths (bow with a hole, collar, shaft, toothed bit; one with
    /// an oval "fish" bow, the cabinet key). One mesh, metres, real normals and UVs; worn metal (no glow): bright and
    /// smoother on the edges and where the keys rub the ring, dark patina elsewhere. Built for this chapter only (the
    /// shared <see cref="StoryPropKit.KeyRing"/> stays as it is for the other chapters).
    /// </summary>
    public static class LauTuBaKeyRing
    {
        private const float Wire = 0.0026f;      // ring wire radius
        private const float RingRadius = 0.03f;  // ring centre-line radius

        /// <summary>
        /// Hangs the ring from her belt on the side that faces the room when she lies on her sập. Returns the ring root (it
        /// carries the hook); <paramref name="bunch"/> is the ring with its keys, <paramref name="grip"/> the point the KeyTheft
        /// trigger follows (the middle of the keys at rest).
        /// </summary>
        public static GameObject AtBelt(StoryActor tuBa, bool leftHip, out Transform grip, out Transform bunch)
        {
            var animator = tuBa.GetComponentInChildren<Animator>();
            var hips = animator != null && animator.isHuman ? animator.GetBoneTransform(HumanBodyBones.Hips) : null;
            var holder = hips != null ? hips : tuBa.transform;
            // Her own width at the belt (the baked body, rest pose): the hook sits just outside it, not floating off the hip
            // and not inside the cloth.
            float side = leftHip ? -1f : 1f;
            float belt = 0.56f * tuBa.GetComponent<CapsuleCollider>().height;
            float hipOut = Extent(tuBa, side, belt - 0.04f, belt + 0.04f, 0.19f);
            float skirtOut = Extent(tuBa, side, belt - 0.24f, belt - 0.06f, hipOut);
            // An anchor on the hip bone (rides with her pose), cancelling the rig's scale.
            var anchor = new GameObject("KeyAnchor").transform;
            anchor.SetParent(holder, false);
            anchor.position = tuBa.transform.TransformPoint(new Vector3(side * (hipOut + 0.012f), belt, 0.02f));
            anchor.rotation = tuBa.transform.rotation;
            Vector3 scale = holder.lossyScale;
            anchor.localScale = new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z);

            var material = Material();
            var ring = new GameObject("KeyRing");
            ring.transform.SetParent(anchor, false);
            ring.AddComponent<MeshFilter>().sharedMesh = HookMesh();
            ring.AddComponent<MeshRenderer>().sharedMaterial = material;
            // What is taken: the ring and keys, a child turning about the bottom of the hook's J (the hook stays on her).
            bunch = new GameObject("Bunch").transform;
            bunch.SetParent(ring.transform, false);
            bunch.localPosition = new Vector3(0f, -BunchPivot, 0f);
            bunch.gameObject.AddComponent<MeshFilter>().sharedMesh = BunchMesh();
            bunch.gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
            grip = new GameObject("Grip").transform;
            grip.SetParent(ring.transform, false);
            grip.localPosition = new Vector3(0f, -RingTop - RingRadius - 0.035f, 0f);
            var hang = ring.AddComponent<HangingKeys>();
            var so = new SerializedObject(hang);
            so.FindProperty("_actor").objectReferenceValue = tuBa;
            so.FindProperty("_hips").objectReferenceValue = holder;
            // Standing, the keys swing out just enough to clear the cloth below the hip.
            so.FindProperty("_standingTilt").floatValue = Mathf.Clamp((skirtOut + 0.012f - hipOut) / 0.12f, 0f, 0.45f);
            so.ApplyModifiedPropertiesWithoutUndo();
            return ring;
        }

        /// <summary>
        /// How far her body reaches on one side (root-local metres) between two heights, from the baked rest pose, near
        /// her front-back middle (hands and sleeves further out are left aside).
        /// </summary>
        private static float Extent(StoryActor tuBa, float side, float from, float to, float fallback)
        {
            var root = tuBa.transform;
            var xs = new List<float>();
            foreach (var skin in tuBa.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var baked = new Mesh();
                skin.BakeMesh(baked, true);
                var toWorld = Matrix4x4.TRS(skin.transform.position, skin.transform.rotation, Vector3.one);
                foreach (var v in baked.vertices)
                {
                    Vector3 p = root.InverseTransformPoint(toWorld.MultiplyPoint3x4(v));
                    if (p.y < from || p.y > to || Mathf.Abs(p.z) > 0.14f) continue;
                    float x = p.x * side;
                    if (x > 0f && x < 0.3f) xs.Add(x);
                }
                Object.DestroyImmediate(baked);
            }
            if (xs.Count < 8) return fallback;
            xs.Sort();
            return xs[(int)(xs.Count * 0.97f)];
        }

        // ------------------------------------------------------------------ material

        private static Material Material()
        {
            const string path = LauTuBaKit.Folder + "/LTB_KeyBronze.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool create = material == null;
            if (create) material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            var (albedo, metal) = LauTuBaTextures.KeyBronze();
            material.SetTexture("_BaseMap", albedo);
            material.SetColor("_BaseColor", Color.white);
            material.SetTexture("_MetallicGlossMap", metal);
            material.SetFloat("_Metallic", 1f);
            material.SetFloat("_Smoothness", 1f); // scales the map's alpha
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.DisableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", Color.black);
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            if (create) AssetDatabase.CreateAsset(material, path);
            else EditorUtility.SetDirty(material);
            return material;
        }

        // ------------------------------------------------------------------ mesh

        private const float RingTop = 0.0446f; // ring centre-line top, below the anchor (through the hook's lower eye)

        private sealed class Builder
        {
            public readonly List<Vector3> V = new List<Vector3>();
            public readonly List<Vector3> N = new List<Vector3>();
            public readonly List<Vector2> Uv = new List<Vector2>();
            public readonly List<int> T = new List<int>();
            private readonly System.Random _random = new System.Random(41);
            private Vector2 _jitter;
            private Vector3 _centre, _a, _b;

            /// <summary>A new part: its own spot on the texture, mapped in its own axes.</summary>
            public void Part(Vector3 centre, Vector3 a, Vector3 b)
            {
                _centre = centre; _a = a.normalized; _b = b.normalized;
                _jitter = new Vector2((float)_random.NextDouble() * 0.24f - 0.12f, (float)_random.NextDouble() * 0.06f - 0.03f);
            }

            /// <summary>A vertex; <paramref name="wear"/> 0 = patina, 1 = rubbed bright (edges, contact).</summary>
            public int Add(Vector3 p, Vector3 n, float wear)
            {
                Vector3 d = p - _centre;
                float u = 0.5f + _jitter.x + Vector3.Dot(d, _a) * 8f;
                float v = Mathf.Lerp(0.76f, 0.24f, Mathf.Clamp01(wear)) + _jitter.y + Vector3.Dot(d, _b) * 4f;
                V.Add(p); N.Add(n.normalized); Uv.Add(new Vector2(Mathf.Clamp(u, 0.02f, 0.98f), Mathf.Clamp(v, 0.02f, 0.98f)));
                return V.Count - 1;
            }

            /// <summary>A triangle wound so its front faces along its vertex normals.</summary>
            public void Tri(int a, int b, int c)
            {
                Vector3 face = Vector3.Cross(V[b] - V[a], V[c] - V[a]);
                if (Vector3.Dot(face, N[a] + N[b] + N[c]) < 0f) { int t = b; b = c; c = t; }
                T.Add(a); T.Add(b); T.Add(c);
            }

            public void Quad(int a, int b, int c, int d) { Tri(a, b, c); Tri(b, d, c); }
        }

        /// <summary>A torus (or arc) around <paramref name="axis"/>; <paramref name="wearInside"/> rubs its inner side bright.</summary>
        private static void Torus(Builder m, Vector3 centre, Vector3 axis, Vector3 start, float radius, float tube, float wearInside,
            float arcDegrees = 360f, int segments = 32, int sides = 12, float stretch = 1f)
        {
            axis.Normalize();
            start = Vector3.ProjectOnPlane(start, axis).normalized;
            Vector3 other = Vector3.Cross(axis, start);
            m.Part(centre, start, other);
            int row = sides + 1;
            int first = m.V.Count;
            for (int i = 0; i <= segments; i++)
            {
                float a = Mathf.Deg2Rad * arcDegrees * i / segments;
                Vector3 outward = start * Mathf.Cos(a) + other * Mathf.Sin(a);
                Vector3 c = centre + start * Mathf.Cos(a) * radius * stretch + other * Mathf.Sin(a) * radius;
                for (int j = 0; j <= sides; j++)
                {
                    float b = Mathf.PI * 2f * j / sides;
                    Vector3 n = outward * Mathf.Cos(b) + axis * Mathf.Sin(b);
                    // Inner side (facing the centre): rubbed by what hangs on it.
                    float inside = Mathf.Clamp01(-Mathf.Cos(b) * 1.4f - 0.2f);
                    m.Add(c + n * tube, n, inside * wearInside);
                }
            }
            for (int i = 0; i < segments; i++)
                for (int j = 0; j < sides; j++)
                {
                    int a = first + i * row + j;
                    m.Quad(a, a + 1, a + row, a + row + 1);
                }
            // An open arc gets rounded ends.
            if (arcDegrees < 359f)
                foreach (int i in new[] { 0, segments })
                {
                    float a = Mathf.Deg2Rad * arcDegrees * i / segments;
                    Vector3 c = centre + start * Mathf.Cos(a) * radius * stretch + other * Mathf.Sin(a) * radius;
                    Vector3 along = (-start * Mathf.Sin(a) + other * Mathf.Cos(a)) * (i == 0 ? -1f : 1f);
                    Cap(m, c, along, tube);
                }
        }

        /// <summary>A small half-sphere closing a wire end.</summary>
        private static void Cap(Builder m, Vector3 centre, Vector3 dir, float radius)
        {
            dir.Normalize();
            Vector3 a = Vector3.Cross(dir, Mathf.Abs(dir.y) < 0.9f ? Vector3.up : Vector3.right).normalized, b = Vector3.Cross(dir, a);
            const int rings = 3, sides = 10;
            int first = m.V.Count;
            for (int r = 0; r <= rings; r++)
            {
                float t = Mathf.PI * 0.5f * r / rings;
                for (int j = 0; j <= sides; j++)
                {
                    float s = Mathf.PI * 2f * j / sides;
                    Vector3 n = dir * Mathf.Sin(t) + (a * Mathf.Cos(s) + b * Mathf.Sin(s)) * Mathf.Cos(t);
                    m.Add(centre + n * radius, n, 1f);
                }
            }
            for (int r = 0; r < rings; r++)
                for (int j = 0; j < sides; j++)
                {
                    int i = first + r * (sides + 1) + j;
                    m.Quad(i, i + 1, i + sides + 1, i + sides + 2);
                }
        }

        /// <summary>A rod from <paramref name="from"/> to <paramref name="to"/> with chamfered ends (the chamfers worn bright).</summary>
        private static void Rod(Builder m, Vector3 from, Vector3 to, float radius, float chamfer, int sides = 12)
        {
            Vector3 axis = (to - from).normalized;
            float length = Vector3.Distance(from, to);
            Vector3 a = Vector3.Cross(axis, Mathf.Abs(axis.y) < 0.9f ? Vector3.up : Vector3.right).normalized, b = Vector3.Cross(axis, a);
            m.Part((from + to) * 0.5f, a, axis);
            chamfer = Mathf.Min(chamfer, radius * 0.6f, length * 0.3f);
            // Profile: (distance along, radius, normal along the axis, wear).
            var profile = new (float at, float r, float na, float wear)[]
            {
                (0f, radius - chamfer, -0.7f, 1f), (chamfer, radius, 0f, 0.6f), (length - chamfer, radius, 0f, 0.6f), (length, radius - chamfer, 0.7f, 1f)
            };
            int row = sides + 1;
            int first = m.V.Count;
            foreach (var (at, r, na, wear) in profile)
                for (int j = 0; j <= sides; j++)
                {
                    float s = Mathf.PI * 2f * j / sides;
                    Vector3 radial = a * Mathf.Cos(s) + b * Mathf.Sin(s);
                    // The long faces are patina in the middle, the chamfer bands bright.
                    float w = na != 0f ? wear : 0f;
                    m.Add(from + axis * at + radial * r, radial + axis * na, w);
                }
            for (int k = 0; k < profile.Length - 1; k++)
                for (int j = 0; j < sides; j++)
                {
                    int i = first + k * row + j;
                    m.Quad(i, i + 1, i + row, i + row + 1);
                }
            foreach (var (end, sign, ring) in new[] { (from, -1f, 0), (to, 1f, profile.Length - 1) })
            {
                int centre = m.Add(end, axis * sign, 1f);
                for (int j = 0; j < sides; j++)
                {
                    float s0 = Mathf.PI * 2f * j / sides, s1 = Mathf.PI * 2f * (j + 1) / sides;
                    float r = radius - chamfer;
                    int p = m.Add(end + (a * Mathf.Cos(s0) + b * Mathf.Sin(s0)) * r, axis * sign, 1f);
                    int q = m.Add(end + (a * Mathf.Cos(s1) + b * Mathf.Sin(s1)) * r, axis * sign, 1f);
                    m.Tri(centre, p, q);
                }
            }
        }

        /// <summary>A small block with chamfered edges (a key tooth), in the frame (<paramref name="x"/>, <paramref name="y"/>, <paramref name="z"/>).</summary>
        private static void Block(Builder m, Vector3 centre, Vector3 x, Vector3 y, Vector3 z, Vector3 size, float bevel)
        {
            m.Part(centre, x, y);
            Vector3 half = size * 0.5f;
            bevel = Mathf.Min(bevel, Mathf.Min(half.x, Mathf.Min(half.y, half.z)) * 0.9f);
            Vector3 inner = half - Vector3.one * bevel;
            float[] Steps(float h) => new[] { -h, -h + bevel, h - bevel, h };
            void Face(Vector3 n, Vector3 u, Vector3 v, float[] su, float[] sv, float w)
            {
                int first = m.V.Count;
                for (int j = 0; j < sv.Length; j++)
                    for (int i = 0; i < su.Length; i++)
                    {
                        Vector3 p = n * w + u * su[i] + v * sv[j];
                        Vector3 c = new Vector3(Mathf.Clamp(p.x, -inner.x, inner.x), Mathf.Clamp(p.y, -inner.y, inner.y), Mathf.Clamp(p.z, -inner.z, inner.z));
                        Vector3 d = p - c;
                        int axes = (Mathf.Abs(d.x) > 1e-6f ? 1 : 0) + (Mathf.Abs(d.y) > 1e-6f ? 1 : 0) + (Mathf.Abs(d.z) > 1e-6f ? 1 : 0);
                        Vector3 dn = d.sqrMagnitude > 1e-12f ? d.normalized : n;
                        Vector3 q = c + dn * bevel;
                        Vector3 world = centre + x * q.x + y * q.y + z * q.z;
                        Vector3 normal = x * dn.x + y * dn.y + z * dn.z;
                        m.Add(world, normal, axes >= 2 ? 1f : 0f);
                    }
                for (int j = 0; j < sv.Length - 1; j++)
                    for (int i = 0; i < su.Length - 1; i++)
                    {
                        int a = first + j * su.Length + i;
                        m.Quad(a, a + 1, a + su.Length, a + su.Length + 1);
                    }
            }
            float[] sx = Steps(half.x), sy = Steps(half.y), sz = Steps(half.z);
            Face(Vector3.right, Vector3.forward, Vector3.up, sz, sy, half.x);
            Face(Vector3.left, Vector3.forward, Vector3.up, sz, sy, half.x);
            Face(Vector3.up, Vector3.right, Vector3.forward, sx, sz, half.y);
            Face(Vector3.down, Vector3.right, Vector3.forward, sx, sz, half.y);
            Face(Vector3.forward, Vector3.right, Vector3.up, sx, sy, half.z);
            Face(Vector3.back, Vector3.right, Vector3.up, sx, sy, half.z);
        }

        /// <summary>
        /// The hook that stays on her belt, in the ring's frame (anchor at the origin, +X away from her side): an eye round the
        /// belt cord (which runs front to back, along Z), a short neck, and an open J below it. The ring's wire rests in the
        /// bottom of the J; the J opens upwards on the outer side, which is the way the ring comes off.
        /// </summary>
        public static Mesh HookMesh()
        {
            var m = new Builder();
            Torus(m, new Vector3(0f, -0.002f, 0f), Vector3.forward, Vector3.up, 0.0075f, 0.0021f, 0.6f, 300f, 20, 10);
            Rod(m, new Vector3(0f, -0.0095f, 0f), new Vector3(0f, -0.031f, 0f), 0.0021f, 0.0006f, 10);
            // From the neck round the inner side and the bottom, up the outer side to the tip: 250° (the arc turns from +Y
            // towards -X first), leaving the upper outer quarter open.
            Torus(m, new Vector3(0f, -0.0375f, 0f), Vector3.forward, Vector3.up, 0.0068f, 0.0021f, 1f, 250f, 20, 10);
            return Finish(m, "LTB_KeyHook");
        }

        /// <summary>Where the ring's wire rests in the hook's J, below the anchor: the bunch turns about this point.</summary>
        public const float BunchPivot = RingTop;

        /// <summary>
        /// The ring and its keys (what is taken), in the bunch's frame: the origin is the top of the ring's wire where it rests
        /// in the hook, the ring plane facing ±X, the keys hanging down.
        /// </summary>
        public static Mesh BunchMesh()
        {
            var m = new Builder();
            Vector3 ringCentre = new Vector3(0f, -RingRadius, 0f);
            Torus(m, ringCentre, Vector3.right, Vector3.up, RingRadius, Wire, 1f, 360f, 48, 12);
            // Keys: where on the ring (degrees from the bottom), shaft length, bow radius, teeth, twist about their own axis.
            var keys = new (float at, float shaft, float bow, float[] teeth, float twist, float oval)[]
            {
                (-52f, 0.040f, 0.0095f, new[] { 0.006f, 0.004f }, 28f, 1f),
                (-24f, 0.058f, 0.0105f, new[] { 0.008f, 0.005f, 0.007f }, -34f, 1.45f), // the "fish" key: oval bow, the cabinet's
                (2f, 0.047f, 0.0090f, new[] { 0.005f, 0.0075f }, 12f, 1f),
                (27f, 0.034f, 0.0085f, new[] { 0.0055f }, -18f, 1f),
                (50f, 0.052f, 0.0100f, new[] { 0.007f, 0.004f, 0.006f }, 40f, 1f),
            };
            foreach (var key in keys)
            {
                float a = key.at * Mathf.Deg2Rad;
                // On the ring (plane YZ): the bottom is at -Y.
                Vector3 radial = new Vector3(0f, -Mathf.Cos(a), Mathf.Sin(a));
                Vector3 onWire = ringCentre + radial * RingRadius;
                Vector3 tangent = new Vector3(0f, Mathf.Sin(a), Mathf.Cos(a));
                // A key hangs mostly under gravity, fanned out a little by the ring.
                Vector3 down = (Vector3.down + radial * 0.45f).normalized;
                Vector3 face = Vector3.ProjectOnPlane(tangent, down).normalized; // the wire runs through the bow
                Quaternion twist = Quaternion.AngleAxis(key.twist, down);
                face = twist * face;
                Vector3 across = Vector3.Cross(down, face).normalized; // the bit sticks out this way
                // The wire rests at the top of the bow's hole.
                Vector3 bowCentre = onWire + down * (key.bow - 0.0022f + Wire * 0.4f);
                Torus(m, bowCentre, face, -down, key.bow, 0.0024f, 1f, 360f, 24, 10, key.oval);
                Vector3 neck = bowCentre + down * (key.bow * key.oval + 0.0018f);
                Rod(m, neck, neck + down * 0.006f, 0.0036f, 0.0009f, 12);
                Vector3 shaftStart = neck + down * 0.006f;
                Vector3 tip = shaftStart + down * key.shaft;
                Rod(m, shaftStart, tip, 0.0021f, 0.0006f, 10);
                // The bit: teeth of different reach near the tip, on one side.
                for (int t = 0; t < key.teeth.Length; t++)
                {
                    float reach = key.teeth[t];
                    Vector3 centre = tip - down * (0.0028f + t * 0.0046f) + across * (0.0019f + reach * 0.5f);
                    Block(m, centre, across, -down, face, new Vector3(reach, 0.0034f, 0.0024f), 0.0006f);
                }
            }
            return Finish(m, "LTB_KeyBunch");
        }

        private static Mesh Finish(Builder m, string name)
        {
            var mesh = new Mesh { name = name };
            mesh.SetVertices(m.V);
            mesh.SetNormals(m.N);
            mesh.SetUVs(0, m.Uv);
            mesh.SetTriangles(m.T, 0);
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return LauTuBaMeshKit.Keep(name, mesh);
        }

        /// <summary>For the QA report: vertex and triangle counts, size, and whether every face looks along its normals.</summary>
        public static string Describe() => Describe(HookMesh()) + " | " + Describe(BunchMesh());

        private static string Describe(Mesh mesh)
        {
            var v = mesh.vertices; var n = mesh.normals; var t = mesh.triangles;
            int flipped = 0;
            for (int i = 0; i < t.Length; i += 3)
            {
                Vector3 face = Vector3.Cross(v[t[i + 1]] - v[t[i]], v[t[i + 2]] - v[t[i]]);
                if (face.sqrMagnitude > 1e-14f && Vector3.Dot(face, n[t[i]] + n[t[i + 1]] + n[t[i + 2]]) < 0f) flipped++;
            }
            return $"{mesh.name}: {mesh.vertexCount} vertices · {t.Length / 3} triangles · {mesh.bounds.size.x * 100f:F1}×{mesh.bounds.size.y * 100f:F1}×{mesh.bounds.size.z * 100f:F1} cm · faces against their normals {flipped} · uv in [0,1] {mesh.uv.All(uv => uv.x >= 0f && uv.x <= 1f && uv.y >= 0f && uv.y <= 1f)}";
        }
    }
}
