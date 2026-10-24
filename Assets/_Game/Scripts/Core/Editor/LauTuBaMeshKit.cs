using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Meshes for Tú Bà's house that primitives cannot give: rounded and bevelled boxes (wood that catches light on its
    /// edges, cushions that look soft), lathe-turned shapes (ceramics, lantern bodies, turned legs, column bases), pleated
    /// cloth, and lighter copies of the project's own high-detail props (their 500k-triangle scans cut down with Unity's
    /// mesh LOD generator, textures untouched). Every mesh is saved as an asset and updated in place on a rebuild, so
    /// scenes keep their references.
    /// </summary>
    public static class LauTuBaMeshKit
    {
        public const string MeshFolder = "Assets/_Game/Art/Environment/LauTuBa/Meshes";
        private static readonly Dictionary<string, Mesh> Made = new Dictionary<string, Mesh>();

        /// <summary>Forget the meshes made during the last rebuild (they are rebuilt once per rebuild, then shared).</summary>
        public static void Begin() => Made.Clear();

        // ------------------------------------------------------------------ saving

        private static Mesh Save(string name, Mesh mesh)
        {
            System.IO.Directory.CreateDirectory(MeshFolder);
            string path = MeshFolder + "/" + name + ".asset";
            mesh.name = name;
            mesh.RecalculateBounds();
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, path);
                Made[name] = mesh;
                return mesh;
            }
            existing.Clear();
            existing.indexFormat = mesh.indexFormat;
            existing.SetVertices(mesh.vertices);
            existing.SetNormals(mesh.normals);
            if (mesh.tangents.Length == mesh.vertexCount) existing.SetTangents(mesh.tangents);
            existing.SetUVs(0, mesh.uv);
            var grain = new List<Vector3>();
            mesh.GetUVs(3, grain);
            if (grain.Count == mesh.vertexCount) existing.SetUVs(3, grain);
            existing.subMeshCount = mesh.subMeshCount;
            for (int i = 0; i < mesh.subMeshCount; i++) existing.SetTriangles(mesh.GetTriangles(i), i);
            existing.RecalculateBounds();
            EditorUtility.SetDirty(existing);
            Object.DestroyImmediate(mesh);
            Made[name] = existing;
            return existing;
        }

        private static bool Cached(string name, out Mesh mesh) => Made.TryGetValue(name, out mesh) && mesh != null;

        /// <summary>A mesh built elsewhere (e.g. by <see cref="LauTuBaJoinery"/>) already made this session.</summary>
        public static bool Has(string name, out Mesh mesh) => Cached(name, out mesh);

        /// <summary>Stores a mesh built elsewhere under <paramref name="name"/>, replacing an older one of that name.</summary>
        public static Mesh Keep(string name, Mesh mesh) => Save(name, mesh);

        // ------------------------------------------------------------------ rounded box

        /// <summary>
        /// A box of <paramref name="size"/> whose edges are rounded with <paramref name="radius"/> (a few millimetres reads
        /// as planed wood; a large radius and <paramref name="puff"/> read as a cushion). Centre at the origin.
        /// </summary>
        public static Mesh RoundedBox(Vector3 size, float radius, float puff = 0f)
        {
            string name = $"RBox_{size.x:F3}_{size.y:F3}_{size.z:F3}_{radius:F3}_{puff:F3}".Replace('.', 'p');
            if (Cached(name, out var cached)) return cached;
            Vector3 half = size * 0.5f;
            radius = Mathf.Min(radius, Mathf.Min(half.x, Mathf.Min(half.y, half.z)) * 0.98f);
            // Grid lines per axis: the faces stay flat, the bands near the edges get several rows to bend through.
            float[] Steps(float h)
            {
                var steps = new List<float> { -h };
                foreach (float s in new[] { 0.62f, 0.25f }) steps.Add(-h + radius * (1f - s));
                steps.Add(-h + radius);
                if (h - radius > -h + radius + 0.001f) steps.Add(h - radius);
                foreach (float s in new[] { 0.25f, 0.62f }) steps.Add(h - radius * (1f - s));
                steps.Add(h);
                return steps.ToArray();
            }
            float[] sx = Steps(half.x), sy = Steps(half.y), sz = Steps(half.z);
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            Vector3 inner = half - Vector3.one * radius;
            void Face(Vector3 normal, Vector3 u, Vector3 v, float[] su, float[] sv, float w)
            {
                int start = vertices.Count;
                for (int j = 0; j < sv.Length; j++)
                    for (int i = 0; i < su.Length; i++)
                    {
                        Vector3 p = normal * w + u * su[i] + v * sv[j];
                        Vector3 c = new Vector3(Mathf.Clamp(p.x, -inner.x, inner.x), Mathf.Clamp(p.y, -inner.y, inner.y), Mathf.Clamp(p.z, -inner.z, inner.z));
                        Vector3 d = p - c;
                        Vector3 n = d.sqrMagnitude > 1e-10f ? d.normalized : normal;
                        Vector3 q = c + n * radius;
                        if (puff > 0f && normal.y > 0.5f)
                            q.y += puff * (1f - Mathf.Pow(Mathf.Abs(q.x) / half.x, 2f)) * (1f - Mathf.Pow(Mathf.Abs(q.z) / half.z, 2f));
                        vertices.Add(q);
                        normals.Add(n);
                        uvs.Add(new Vector2(Vector3.Dot(q, u), Vector3.Dot(q, v)));
                    }
                for (int j = 0; j < sv.Length - 1; j++)
                    for (int i = 0; i < su.Length - 1; i++)
                    {
                        int a = start + j * su.Length + i, b = a + 1, c = a + su.Length, d = c + 1;
                        triangles.AddRange(new[] { a, c, b, b, c, d });
                    }
            }
            Face(Vector3.right, Vector3.forward, Vector3.up, sz, sy, half.x);
            Face(Vector3.left, Vector3.back, Vector3.up, sz, sy, half.x);
            Face(Vector3.up, Vector3.right, Vector3.forward, sx, sz, half.y);
            Face(Vector3.down, Vector3.right, Vector3.back, sx, sz, half.y);
            Face(Vector3.forward, Vector3.left, Vector3.up, sx, sy, half.z);
            Face(Vector3.back, Vector3.right, Vector3.up, sx, sy, half.z);
            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            // UV3: the board's length axis, so wood grain (WorldTriplanarLit _GRAIN_FOLLOWS_OBJECT) runs along it.
            Vector3 length = size.x >= size.y && size.x >= size.z ? Vector3.right : size.y >= size.z ? Vector3.up : Vector3.forward;
            mesh.SetUVs(3, Enumerable.Repeat(length, vertices.Count).ToList());
            mesh.SetTriangles(triangles, 0);
            if (puff > 0f) mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            return Save(name, mesh);
        }

        // ------------------------------------------------------------------ lathe

        /// <summary>
        /// A shape turned on a lathe around +Y: <paramref name="profile"/> lists (radius, height) from the bottom up.
        /// <paramref name="ribs"/> &gt; 0 pinches the surface into that many lobes (a silk lantern on its bamboo ribs).
        /// </summary>
        public static Mesh Lathe(string name, Vector2[] profile, int segments = 24, int ribs = 0, float ribDepth = 0f, bool capBottom = true, bool capTop = false)
        {
            if (Cached(name, out var cached)) return cached;
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            float length = 0f;
            var along = new float[profile.Length];
            for (int i = 1; i < profile.Length; i++) along[i] = length += Vector2.Distance(profile[i - 1], profile[i]);
            for (int i = 0; i < profile.Length; i++)
                for (int s = 0; s <= segments; s++)
                {
                    float angle = s * Mathf.PI * 2f / segments;
                    float r = profile[i].x;
                    if (ribs > 0) r *= 1f - ribDepth * Mathf.Pow(Mathf.Abs(Mathf.Cos(angle * ribs * 0.5f)), 6f);
                    vertices.Add(new Vector3(Mathf.Cos(angle) * r, profile[i].y, Mathf.Sin(angle) * r));
                    uvs.Add(new Vector2((float)s / segments, length > 0f ? along[i] / length : 0f));
                }
            int row = segments + 1;
            for (int i = 0; i < profile.Length - 1; i++)
                for (int s = 0; s < segments; s++)
                {
                    int a = i * row + s, b = a + 1, c = a + row, d = c + 1;
                    triangles.AddRange(new[] { a, c, b, b, c, d });
                }
            void Cap(int ring, bool up)
            {
                int centre = vertices.Count;
                vertices.Add(new Vector3(0f, profile[ring].y, 0f));
                uvs.Add(new Vector2(0.5f, 0.5f));
                int start = vertices.Count;
                for (int s = 0; s <= segments; s++) { vertices.Add(vertices[ring * row + s]); uvs.Add(new Vector2(0.5f, 0.5f)); }
                for (int s = 0; s < segments; s++)
                    triangles.AddRange(up ? new[] { centre, start + s + 1, start + s } : new[] { centre, start + s, start + s + 1 });
            }
            if (capBottom && profile[0].x > 0.0005f) Cap(0, false);
            if (capTop && profile[profile.Length - 1].x > 0.0005f) Cap(profile.Length - 1, true);
            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            // UV3: the turning axis is the grain of a turned part (WorldTriplanarLit _GRAIN_FOLLOWS_OBJECT).
            mesh.SetUVs(3, Enumerable.Repeat(Vector3.up, vertices.Count).ToList());
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            return Save(name, mesh);
        }

        /// <summary>A ring (torus section) lying in the XY plane: a handle, a lantern hoop, a bracelet.</summary>
        public static Mesh Ring(string name, float radius, float thickness, float arcDegrees = 360f, int segments = 20, int sides = 8)
        {
            if (Cached(name, out var cached)) return cached;
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            for (int i = 0; i <= segments; i++)
            {
                float a = Mathf.Deg2Rad * arcDegrees * i / segments;
                Vector3 centre = new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f);
                Vector3 outward = centre.normalized;
                for (int j = 0; j <= sides; j++)
                {
                    float b = Mathf.PI * 2f * j / sides;
                    vertices.Add(centre + (outward * Mathf.Cos(b) + Vector3.forward * Mathf.Sin(b)) * thickness);
                }
            }
            int row = sides + 1;
            for (int i = 0; i < segments; i++)
                for (int j = 0; j < sides; j++)
                {
                    int a = i * row + j, b = a + 1, c = a + row, d = c + 1;
                    triangles.AddRange(new[] { a, b, c, b, d, c });
                }
            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, new Vector2[vertices.Count]);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            return Save(name, mesh);
        }

        /// <summary>A flat patch with a wandering outline (a puddle, a stain), unit radius in XZ, facing up.</summary>
        public static Mesh Blob(int seed, int points = 28)
        {
            string name = "Blob_" + seed;
            if (Cached(name, out var cached)) return cached;
            var random = new System.Random(seed);
            float[] phase = { (float)random.NextDouble() * 6f, (float)random.NextDouble() * 6f, (float)random.NextDouble() * 6f };
            var vertices = new List<Vector3> { Vector3.zero };
            var triangles = new List<int>();
            for (int i = 0; i <= points; i++)
            {
                float a = Mathf.PI * 2f * i / points;
                float r = 1f + 0.18f * Mathf.Sin(a * 2f + phase[0]) + 0.1f * Mathf.Sin(a * 3f + phase[1]) + 0.06f * Mathf.Sin(a * 5f + phase[2]);
                vertices.Add(new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r));
                if (i > 0) triangles.AddRange(new[] { 0, i + 1, i });
            }
            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, vertices.ConvertAll(v => new Vector2(v.x, v.z)));
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            return Save(name, mesh);
        }

        // ------------------------------------------------------------------ the frangipani

        /// <summary>
        /// A frangipani (cây đại) as three meshes: bark, leaves and flowers. The trunk forks again and again into thick,
        /// upturned branches (the candelabra habit of the tree); each tip carries a rosette of long leaves and a cluster
        /// of five-petalled cream flowers. Origin at the foot of the trunk, about 4 m tall and 4 m across.
        /// </summary>
        public static (Mesh bark, Mesh leaves, Mesh flowers) Frangipani(int seed)
        {
            string name = "Frangipani_v4_" + seed;
            if (Cached(name + "_Bark", out var cachedBark) && Cached(name + "_Leaves", out var cachedLeaves) && Cached(name + "_Flowers", out var cachedFlowers))
                return (cachedBark, cachedLeaves, cachedFlowers);
            var random = new System.Random(seed);
            float Range(float a, float b) => a + (float)random.NextDouble() * (b - a);
            // Bark as swept limbs: each branch flows out of its parent (it starts inside it) and thins along a smooth curve.
            var bark = new LauTuBaJoinery();
            var leaves = (v: new List<Vector3>(), t: new List<int>());
            var flowers = (v: new List<Vector3>(), t: new List<int>());
            void Basis(Vector3 dir, out Vector3 side, out Vector3 up)
            {
                side = Vector3.Cross(dir, Mathf.Abs(dir.y) > 0.95f ? Vector3.right : Vector3.up).normalized;
                up = Vector3.Cross(side, dir).normalized;
            }
            void Leaf(Vector3 root, Vector3 dir, float length, float width)
            {
                Basis(dir, out var side, out _);
                int start = leaves.v.Count;
                const int segments = 6;
                for (int i = 0; i <= segments; i++)
                {
                    float t = (float)i / segments;
                    float w = Mathf.Sin(Mathf.PI * Mathf.Pow(t, 0.8f)) * width * 0.5f;
                    Vector3 centre = root + dir * (t * length) + Vector3.down * (t * t * length * 0.25f);
                    leaves.v.Add(centre - side * w);
                    leaves.v.Add(centre + side * w + Vector3.up * w * 0.3f);
                }
                for (int i = 0; i < segments; i++)
                {
                    int a = start + i * 2;
                    leaves.t.AddRange(new[] { a, a + 2, a + 1, a + 1, a + 2, a + 3 });
                }
            }
            void Flower(Vector3 centre, Vector3 facing)
            {
                Basis(facing, out var side, out var up);
                float spin = Range(0f, 72f);
                for (int p = 0; p < 5; p++)
                {
                    float angle = (spin + p * 72f) * Mathf.Deg2Rad;
                    Vector3 outward = side * Mathf.Cos(angle) + up * Mathf.Sin(angle);
                    Vector3 across = Vector3.Cross(facing, outward).normalized;
                    int start = flowers.v.Count;
                    flowers.v.Add(centre);
                    flowers.v.Add(centre + outward * 0.032f + across * 0.018f + facing * 0.006f);
                    flowers.v.Add(centre + outward * 0.062f + facing * 0.014f);
                    flowers.v.Add(centre + outward * 0.032f - across * 0.018f + facing * 0.006f);
                    flowers.t.AddRange(new[] { start, start + 1, start + 2, start, start + 2, start + 3 });
                }
            }
            void Branch(Vector3 start, Vector3 dir, float length, float radius, int depth)
            {
                // Two bent segments per branch.
                Vector3 bend = Quaternion.AngleAxis(Range(-12f, 12f), Vector3.Cross(dir, Vector3.up).normalized) * dir;
                Vector3 mid = start + dir * length * 0.5f;
                Vector3 end = mid + Vector3.Slerp(dir, bend, 0.5f).normalized * length * 0.5f;
                Vector3 inside = start - dir * radius * 1.2f;
                bool trunk = depth == 0;
                bark.Sweep(LauTuBaJoinery.Smooth(new[] { inside, start, mid, end }, 4), LauTuBaJoinery.Round(radius * 2f, radius * 2f, 10), Vector3.forward, 0,
                    t => trunk && t < 0.12f ? Mathf.Lerp(1.45f, 1f, t / 0.12f) : Mathf.Lerp(1f, 0.74f, t), false);
                Vector3 tipDir = (end - mid).normalized;
                if (depth >= 5)
                {
                    int count = 9 + random.Next(3);
                    float spin = Range(0f, 360f);
                    for (int i = 0; i < count; i++)
                    {
                        Vector3 around = Quaternion.AngleAxis(spin + i * 360f / count, tipDir) * Vector3.Cross(tipDir, Vector3.right).normalized;
                        Vector3 leafDir = Vector3.Slerp(around, Vector3.up, Range(0.25f, 0.45f)).normalized;
                        Leaf(end, leafDir, Range(0.36f, 0.52f), Range(0.11f, 0.15f));
                    }
                    if (random.NextDouble() < 0.9)
                        for (int f = 0; f < 8 + random.Next(5); f++)
                            Flower(end + Vector3.up * Range(0.05f, 0.12f) + new Vector3(Range(-0.11f, 0.11f), 0f, Range(-0.11f, 0.11f)),
                                (Vector3.up + new Vector3(Range(-0.5f, 0.5f), 0f, Range(-0.5f, 0.5f))).normalized);
                    return;
                }
                int children = depth == 0 ? 3 : depth >= 3 ? 2 : 2 + random.Next(2);
                float turn = Range(0f, 360f);
                for (int c = 0; c < children; c++)
                {
                    Vector3 axis = Quaternion.AngleAxis(turn + c * 360f / children, tipDir) * Vector3.Cross(tipDir, Vector3.forward).normalized;
                    Vector3 childDir = Quaternion.AngleAxis(Range(25f, 42f), axis) * tipDir;
                    childDir = Vector3.Slerp(childDir, Vector3.up, 0.2f).normalized;
                    Branch(end, childDir, length * Range(0.68f, 0.76f), radius * 0.68f, depth + 1);
                }
            }
            Branch(Vector3.zero, Vector3.Slerp(Vector3.up, new Vector3(Range(-0.3f, 0.3f), 0f, Range(-0.3f, 0.3f)), 0.25f).normalized, 1.5f, 0.2f, 0);
            Mesh Build((List<Vector3> v, List<int> t) part, string suffix)
            {
                var mesh = new Mesh { indexFormat = part.v.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
                mesh.SetVertices(part.v);
                mesh.SetUVs(0, part.v.ConvertAll(p => new Vector2(p.x + p.z, p.y)));
                mesh.SetTriangles(part.t, 0);
                mesh.RecalculateNormals();
                mesh.RecalculateTangents();
                return Save(name + suffix, mesh);
            }
            return (Save(name + "_Bark", bark.Build(name + "_Bark")), Build(leaves, "_Leaves"), Build(flowers, "_Flowers"));
        }

        /// <summary>A few fallen frangipani flowers (one mesh), scattered in a disc of <paramref name="radius"/>.</summary>
        public static Mesh FallenFlowers(int seed, float radius, int count)
        {
            string name = $"FallenFlowers_{seed}_{count}";
            if (Cached(name, out var cached)) return cached;
            var random = new System.Random(seed);
            var v = new List<Vector3>();
            var t = new List<int>();
            for (int f = 0; f < count; f++)
            {
                float a = (float)random.NextDouble() * Mathf.PI * 2f, r = Mathf.Sqrt((float)random.NextDouble()) * radius;
                Vector3 centre = new Vector3(Mathf.Cos(a) * r, 0.006f, Mathf.Sin(a) * r);
                float spin = (float)random.NextDouble() * 72f;
                for (int p = 0; p < 5; p++)
                {
                    float angle = (spin + p * 72f) * Mathf.Deg2Rad;
                    Vector3 outward = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                    Vector3 across = new Vector3(-outward.z, 0f, outward.x);
                    int start = v.Count;
                    v.Add(centre);
                    v.Add(centre + outward * 0.022f + across * 0.012f + Vector3.up * 0.003f);
                    v.Add(centre + outward * 0.042f + Vector3.up * 0.006f);
                    v.Add(centre + outward * 0.022f - across * 0.012f + Vector3.up * 0.003f);
                    t.AddRange(new[] { start, start + 2, start + 1, start, start + 3, start + 2 });
                }
            }
            var mesh = new Mesh();
            mesh.SetVertices(v);
            mesh.SetUVs(0, v.ConvertAll(p => new Vector2(p.x, p.z)));
            mesh.SetTriangles(t, 0);
            mesh.RecalculateNormals();
            return Save(name, mesh);
        }

        // ------------------------------------------------------------------ cloth

        /// <summary>
        /// Hanging cloth with soft vertical folds, hung from its top edge at the origin along +X and dropping down −Y.
        /// <paramref name="gather"/> pulls the bottom in (a curtain tied back) between 0 and 1. Two-sided.
        /// </summary>
        public static Mesh Pleats(string name, float width, float drop, int folds, float depth, float gather = 0f, float flare = 0.4f)
        {
            if (Cached(name, out var cached)) return cached;
            int cols = folds * 8, rows = 10;
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            for (int side = 0; side < 2; side++)
            {
                int start = vertices.Count;
                for (int j = 0; j <= rows; j++)
                    for (int i = 0; i <= cols; i++)
                    {
                        float u = (float)i / cols, v = (float)j / rows;
                        float x = u * width;
                        // Folds deepen towards the hem; a gathered curtain narrows towards one side.
                        float z = Mathf.Sin(u * folds * Mathf.PI * 2f) * depth * (1f - flare + flare * v);
                        x = Mathf.Lerp(x, x * (1f - gather) , v * v);
                        vertices.Add(new Vector3(x, -v * drop, z + (side == 0 ? 0.002f : -0.002f)));
                        uvs.Add(new Vector2(u * width, v * drop));
                    }
                for (int j = 0; j < rows; j++)
                    for (int i = 0; i < cols; i++)
                    {
                        int a = start + j * (cols + 1) + i, b = a + 1, c = a + cols + 1, d = c + 1;
                        triangles.AddRange(side == 0 ? new[] { a, b, c, b, d, c } : new[] { a, c, b, b, c, d });
                    }
            }
            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            return Save(name, mesh);
        }

        // ------------------------------------------------------------------ lighter copies of the project's props

        /// <summary>
        /// The mesh of a project prefab cut down to roughly <paramref name="targetTriangles"/> with Unity's mesh LOD
        /// generator (the level closest to the target is copied out on its own). The source asset is not touched.
        /// </summary>
        public static Mesh Lite(string prefabPath, int targetTriangles)
        {
            string name = "Lite_" + System.IO.Path.GetFileNameWithoutExtension(prefabPath) + "_" + targetTriangles / 1000 + "k";
            if (Cached(name, out var cached)) return cached;
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(MeshFolder + "/" + name + ".asset");
            if (existing != null) { Made[name] = existing; return existing; } // expensive: made once, kept across rebuilds
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var source = prefab.GetComponentInChildren<MeshFilter>().sharedMesh;
            var work = Object.Instantiate(source);
            MeshLodUtility.GenerateMeshLods(work, (MeshLodUtility.LodGenerationFlags)0, 12);
            var ranges = work.GetLods(0);
            int pick = 0;
            for (int i = 0; i < ranges.Length; i++)
                if (Mathf.Abs((int)ranges[i].indexCount / 3 - targetTriangles) < Mathf.Abs((int)ranges[pick].indexCount / 3 - targetTriangles)) pick = i;
            var range = ranges[pick];
            int[] indices;
            using (var data = Mesh.AcquireReadOnlyMeshData(work))
            {
                var all = data[0];
                indices = new int[(int)range.indexCount];
                if (all.indexFormat == IndexFormat.UInt32)
                {
                    var buffer = all.GetIndexData<uint>();
                    for (int i = 0; i < indices.Length; i++) indices[i] = (int)buffer[(int)range.indexStart + i];
                }
                else
                {
                    var buffer = all.GetIndexData<ushort>();
                    for (int i = 0; i < indices.Length; i++) indices[i] = buffer[(int)range.indexStart + i];
                }
            }
            // Keep only the vertices the chosen level uses.
            var remap = new Dictionary<int, int>();
            var positions = work.vertices; var normals = work.normals; var tangents = work.tangents; var uv = work.uv;
            var v = new List<Vector3>(); var n = new List<Vector3>(); var t = new List<Vector4>(); var u = new List<Vector2>();
            for (int i = 0; i < indices.Length; i++)
            {
                if (!remap.TryGetValue(indices[i], out int mapped))
                {
                    mapped = v.Count;
                    remap[indices[i]] = mapped;
                    v.Add(positions[indices[i]]);
                    if (normals.Length > 0) n.Add(normals[indices[i]]);
                    if (tangents.Length > 0) t.Add(tangents[indices[i]]);
                    if (uv.Length > 0) u.Add(uv[indices[i]]);
                }
                indices[i] = mapped;
            }
            var mesh = new Mesh { indexFormat = v.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
            mesh.SetVertices(v);
            if (n.Count == v.Count) mesh.SetNormals(n); else mesh.RecalculateNormals();
            if (u.Count == v.Count) mesh.SetUVs(0, u);
            if (t.Count == v.Count) mesh.SetTangents(t);
            mesh.SetTriangles(indices, 0);
            Object.DestroyImmediate(work);
            Debug.Log($"[LauTuBaMeshKit] {name}: {source.triangles.Length / 3:N0} → {indices.Length / 3:N0} triangles (level {pick})");
            return Save(name, mesh);
        }

        /// <summary>The material of a project prefab (to put on its lighter copy).</summary>
        public static Material[] MaterialsOf(string prefabPath) =>
            AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath).GetComponentInChildren<MeshRenderer>().sharedMaterials;
    }
}
