using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

namespace ThuyKieu.Environment
{
    /// <summary>
    /// The ground of the Chapter 5 bamboo forest, made from the course itself when the scene loads (the meshes are too big to
    /// keep as text assets): a running corridor along each way cut into a wooded hillside, so that beside it the ground is
    /// in turn a tall crumbling cut bank, a low lip with the floor running level or falling away behind it, and every shape
    /// between, differently on each side (<see cref="Bank"/>); the forest floor swelling into low hills further out, ditches
    /// and the stream ravine cut across. A person can neither climb nor jump out: where a bank is too low to hold her by its
    /// steepness, a bamboo hedge on its lip does (<see cref="Hedges"/>). The trodden line, damp hollows, bare faces and
    /// fallen leaves are in the vertex colours for the ground's material (a look only: underfoot it is the same earth).
    /// Near the ways the ground is fine and solid (what is seen is what is stood on); further out it is seen only, and out
    /// to the skyline a coarse floor carries the far bamboo so no edge of the world shows, even from the top of a jump.
    /// Only the corridor and the banks go into the forest's NavMesh, built in the background (<see cref="NavReady"/>).
    /// The shape is worked out on worker threads and the colliders cooked there too; the same <see cref="Height"/> places
    /// everything the scene setup puts on the ground (logs, rocks, the bridge).
    /// Runs in the Editor too, as hidden unsaved objects, so the forest can be seen without playing.
    /// </summary>
    [ExecuteAlways]
    public class ForestGround : MonoBehaviour
    {
        [Serializable]
        public struct Way
        {
            public Vector3[] Points;
            [Tooltip("Half-width of the running corridor at each point (metres).")]
            public float[] Half;
            [Tooltip("Only this part of the way is a corridor of its own (the rest lies on another way); 0 and 0: all of it.")]
            public float CorridorFrom, CorridorTo;
        }

        [Serializable]
        public struct Cut
        {
            [Tooltip("Middle of the cut at path height.")]
            public Vector3 Centre;
            public Vector3 Forward;
            [Tooltip("Width along the way, and how far it runs to each side.")]
            public float Length, Across, Depth;
            [Tooltip("Width of each wall from the lip to the bottom.")]
            public float Wall;
        }

        /// <summary>Something on the course she could stand on (a log, a rock, a fallen culm), and how high its top is above the way.</summary>
        [Serializable]
        public struct Foothold
        {
            public Vector3 Centre;
            public float Radius, Top;
        }

        [SerializeField] private Way[] _ways;
        [SerializeField] private Cut[] _cuts;
        [Tooltip("Within these, she may already stand this high when she tries the bank (the hedge holds her from there too).")]
        [SerializeField] private Foothold[] _footholds;
        [SerializeField] private Material _floor;
        [Tooltip("Fine ground out to this far past a corridor's edge.")]
        [SerializeField] private float _band = 16f, _step = 0.4f, _tile = 32f;
        [Tooltip("Of the fine ground, solid out to here (well past the bank tops she can never reach).")]
        [SerializeField] private float _solidBand = 6f;
        [Tooltip("Of the solid ground, in the NavMesh build out to here: the corridor and the bank slopes.")]
        [SerializeField] private float _navBand = 3f;
        [Tooltip("The coarse far floor under the far bamboo, out to here.")]
        [SerializeField] private float _farBand = 74f, _farStep = 2f, _farTile = 64f;
        [Tooltip("A cut bank is too steep to stand on up to at least this height above the way (m): from the top of a jump " +
                 "(1.1 m) her capsule finds no footing below it. Lower banks are held by a bamboo hedge on their lip.")]
        [SerializeField] private float _hold = 1.9f;
        [Tooltip("The hillside the ways are cut into: how far it rises above or falls below them (m), the size of its folds " +
                 "(1/m) and how much higher than the way it lies on average (m).")]
        [SerializeField] private float _relief = 6.5f, _reliefScale = 0.017f, _reliefBias = 0.8f;
        [Tooltip("Height of the low hills the forest floor rises into away from the ways.")]
        [SerializeField] private float _hills = 6f;
        [Tooltip("Banks fade out before this x (the open waste ground she comes from).")]
        [SerializeField] private float _openUntilX = 97f;
        [SerializeField] private int _seed = 5;
        [SerializeField] private NavMeshSurface _navMesh;
        [Tooltip("The forest's NavMesh baked by the scene setup from this same ground, used instead of building it as the " +
                 "chapter opens (it is built in the background only when this is missing).")]
        [SerializeField] private NavMeshData _baked;

        private const int Ground = 8;
        private const float Cell = 8f;
        private const int MaxRing = 12;
        private readonly List<GameObject> _made = new List<GameObject>();
        private int[][] _cells;
        private int _gridX, _gridZ, _gridWidth, _gridDepth;
        private float _maxHalf;
        private NavMeshData _data;
        private Transform _outer;

        public bool NavReady { get; private set; }
        public bool HasWays => _ways != null && _ways.Length > 0 && _ways[0].Points != null && _ways[0].Points.Length > 1;
        public float NavBuildSeconds { get; private set; }
        public float BuildSeconds { get; private set; }
        /// <summary>Where the last build spent its time (QA).</summary>
        public string Timing { get; private set; }
        public int VertexCount { get; private set; }

        private void OnEnable()
        {
            Build();
            if (!Application.isPlaying) return;
            ThuyKieu.Core.Story.StoryAutopilot.Note($"forest ground built in {BuildSeconds:F2} s ({Timing}) · {VertexCount} vertices");
            StartCoroutine(BuildNavMesh());
        }

        private void OnDisable()
        {
            Clear();
            if (_navMesh != null && Application.isPlaying) _navMesh.RemoveData();
        }

        // ---------------------------------------------------------------- the shape of the ground

        /// <summary>Nearest corridor: how far past its edge a point is (negative inside), and the way's height there.</summary>
        public float Corridor(float x, float z, out float pathY)
        {
            float best = Nearest(x, z, out int segment, out float k);
            pathY = segment >= 0 ? _segments[segment].A.y + _segments[segment].Rise * k : 0f;
            return best;
        }

        /// <summary>
        /// The nearest corridor edge, searched ring by ring of the 8 m grid until nothing further out can be nearer (exact at
        /// any distance; a point by the path looks at nine cells). Safe from worker threads once <see cref="Index"/> ran.
        /// </summary>
        private float Nearest(float x, float z, out int segment, out float along)
        {
            Index();
            float best = float.MaxValue;
            segment = -1;
            along = 0f;
            int cx = Mathf.FloorToInt(x / Cell) - _gridX, cz = Mathf.FloorToInt(z / Cell) - _gridZ;
            for (int r = 0; r <= MaxRing; r++)
            {
                if (r == 0) Scan(cx, cz, x, z, ref best, ref segment, ref along);
                else
                    for (int d = -r; d <= r; d++)
                    {
                        Scan(cx + d, cz - r, x, z, ref best, ref segment, ref along);
                        Scan(cx + d, cz + r, x, z, ref best, ref segment, ref along);
                        if (d == -r || d == r) continue;
                        Scan(cx - r, cz + d, x, z, ref best, ref segment, ref along);
                        Scan(cx + r, cz + d, x, z, ref best, ref segment, ref along);
                    }
                // Every segment not yet seen lies wholly in cells at least r cells away: r·8 m from the point.
                if (best <= r * Cell - _maxHalf) break;
            }
            return best == float.MaxValue ? 1000f : best;
        }

        /// <summary>The segments listed in one cell of the dense grid (none outside it).</summary>
        private void Scan(int gx, int gz, float x, float z, ref float best, ref int segment, ref float along)
        {
            if (gx < 0 || gz < 0 || gx >= _gridWidth || gz >= _gridDepth) return;
            var list = _cells[gz * _gridWidth + gx];
            if (list == null) return;
            for (int i = 0; i < list.Length; i++)
            {
                ref readonly var seg = ref _segments[list[i]];
                float px = x - seg.A.x, pz = z - seg.A.z;
                float k = Mathf.Clamp01((px * seg.AB.x + pz * seg.AB.y) * seg.InvLength2);
                float ex = px - seg.AB.x * k, ez = pz - seg.AB.y * k;
                float d = Mathf.Sqrt(ex * ex + ez * ez) - (seg.HalfA + (seg.HalfB - seg.HalfA) * k);
                if (d < best) { best = d; segment = list[i]; along = k; }
            }
        }

        /// <summary>Ground height at a point: corridor floor, banks, rising forest floor and hills, and the cuts.</summary>
        public float Height(float x, float z) => Shape(x, z, out _);

        /// <summary>Height, and how far past the nearest corridor's edge the point is (one corridor search for both).</summary>
        private float Shape(float x, float z, out float edge)
        {
            edge = Corridor(x, z, out float pathY);
            float h = pathY + Bank(edge, x, z);
            foreach (var cut in _cuts) h = Carve(cut, x, z, h);
            return h;
        }

        /// <summary>
        /// Height, distance past the nearest corridor's edge and whether a cut is here, from one corridor search (what the
        /// bamboo asks for every few hand-spans of the forest floor).
        /// </summary>
        public float Height(float x, float z, out float edge, out bool cut)
        {
            float h = Shape(x, z, out edge);
            cut = InCut(x, z);
            return h;
        }

        /// <summary>
        /// The ground beside a way, as the way was cut into a wooded hillside: where the hill stands above the way, a cut bank
        /// rises steep (crumbling, its foot heaped with what slid down), then follows the hill up; where the hill is level with
        /// the way or falls away below it, a low earth lip, and behind it the floor runs level or down the slope, a traverse
        /// with the forest below. Which it is changes along the way with the hill's folds, side by side differently. The
        /// lips are held by the hedge (<see cref="Hedges"/>), the cut banks by their steepness.
        /// </summary>
        private float Bank(float edge, float x, float z)
        {
            float micro = (ForestNoise.Perlin(x * 0.9f + _seed, z * 0.9f) - 0.5f) * 0.05f;
            if (edge <= 0f) return micro;
            float open = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(_openUntilX - 4f, _openUntilX + 4f, x));
            // The hillside: large folds and small swells, relative to the way.
            float hill = _reliefBias + _relief * (ForestNoise.Perlin(x * _reliefScale + 211f, z * _reliefScale + _seed) - 0.5f) * 2f
                       + (ForestNoise.Perlin(x * 0.08f + 17f, z * 0.08f) - 0.5f) * 0.9f;
            // Away from the ways the floor swells into low hills that close the view (never near the banks).
            hill += ForestNoise.Perlin(x * 0.017f + 41f, z * 0.017f + _seed) * _hills * Mathf.SmoothStep(0f, 1f, (edge - 10f) / 30f);
            float h = Mathf.Max(Mathf.Min(hill, CutFace(edge, x, z)), Lip(edge, x, z));
            // Under a tall cut, a heap of what slid down from it, soft at its foot.
            float slump = Mathf.Clamp01((ForestNoise.Perlin(x * 0.11f + 63f, z * 0.11f + _seed) - 0.58f) * 6f) * Mathf.Clamp01(hill - 2.6f);
            if (slump > 0f) h = Mathf.Max(h, slump * 0.32f * Mathf.Sin(Mathf.PI * Mathf.Clamp01(edge / 1.1f)));
            return micro + h * open;
        }

        /// <summary>
        /// A cut bank: steep from its foot to well over <see cref="_hold"/> (its width and height change along the way, the
        /// earth crumbles a little), then climbing on at a slant until it meets the hillside.
        /// </summary>
        private float CutFace(float edge, float x, float z)
        {
            float width = Mathf.Lerp(0.75f, 1.2f, ForestNoise.Perlin(x * 0.06f + 5f, z * 0.06f + _seed));
            float top = _hold + 0.2f + 0.6f * ForestNoise.Perlin(x * 0.05f + 71f, z * 0.05f);
            float crumble = (ForestNoise.Perlin(x * 0.85f + 9f, z * 0.85f + edge * 0.85f) - 0.5f) * 0.1f * Mathf.Clamp01(edge * 3f);
            if (edge < width) return top * Mathf.Pow(edge / width, 1.5f) + crumble;
            return top + crumble + (edge - width) * Mathf.Lerp(0.8f, 1.5f, ForestNoise.Perlin(x * 0.04f + 13f, z * 0.04f + _seed));
        }

        /// <summary>A low earth lip at the side of the way (where the hill does not stand above it): steep, a narrow top, then falling off.</summary>
        private float Lip(float edge, float x, float z)
        {
            float height = Mathf.Lerp(0.5f, 0.95f, ForestNoise.Perlin(x * 0.07f + 29f, z * 0.07f + _seed)), width = height * 0.72f;
            if (edge < width) return height * Mathf.Pow(edge / width, 1.5f);
            float top = Mathf.Lerp(0.4f, 1.1f, ForestNoise.Perlin(x * 0.09f, z * 0.09f + 3f));
            if (edge < width + top) return height - (edge - width) * 0.08f;
            return height - top * 0.08f - (edge - width - top) * 0.75f;
        }

        private static float Carve(Cut cut, float x, float z, float h)
        {
            Vector3 forward = cut.Forward;
            forward.y = 0f;
            forward.Normalize();
            var right = new Vector3(forward.z, 0f, -forward.x);
            var offset = new Vector3(x - cut.Centre.x, 0f, z - cut.Centre.z);
            float along = Mathf.Abs(Vector3.Dot(offset, forward)), across = Mathf.Abs(Vector3.Dot(offset, right));
            float half = cut.Length * 0.5f;
            if (along >= half || across >= cut.Across) return h;
            float bottom = cut.Centre.y - cut.Depth + (ForestNoise.Perlin(x * 0.7f, z * 0.7f) - 0.5f) * 0.3f;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((half - along) / Mathf.Max(0.01f, cut.Wall)));
            // The ends of a ditch run out into the forest floor instead of stopping at a wall.
            float ends = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((cut.Across - across) / 3f));
            return Mathf.Min(h, Mathf.Lerp(h, bottom, k * ends));
        }

        private struct Segment
        {
            public Vector3 A;
            public Vector2 AB;
            public float InvLength2, Rise, HalfA, HalfB;
        }

        private Segment[] _segments;

        /// <summary>
        /// The corridor search runs for every vertex of the ground and every culm, so the ways are searched as 3 m chords
        /// (within 6 cm of the 1 m centreline on the tightest bend) in an 8 m grid; a way adds only its own part
        /// (the thick way only between the fork and the merge).
        /// </summary>
        private void Index()
        {
            if (_cells != null) return;
            var grid = new Dictionary<Vector2Int, List<int>>();
            var segments = new List<Segment>();
            float maxHalf = 0f;
            const int chord = 3;
            for (int w = 0; w < _ways.Length; w++)
            {
                var way = _ways[w];
                var points = way.Points;
                bool part = way.CorridorTo > way.CorridorFrom;
                float s = 0f;
                for (int i = 0; i + 1 < points.Length;)
                {
                    int j = Mathf.Min(i + chord, points.Length - 1);
                    float length = 0f;
                    for (int k = i; k < j; k++) length += Vector3.Distance(points[k], points[k + 1]);
                    bool inside = !part || (s + length >= way.CorridorFrom && s <= way.CorridorTo);
                    s += length;
                    if (inside)
                    {
                        Vector3 a = points[i], b = points[j];
                        var ab = new Vector2(b.x - a.x, b.z - a.z);
                        int id = segments.Count;
                        segments.Add(new Segment { A = a, AB = ab, InvLength2 = 1f / Mathf.Max(0.0001f, ab.sqrMagnitude), Rise = b.y - a.y, HalfA = way.Half[i], HalfB = way.Half[j] });
                        maxHalf = Mathf.Max(maxHalf, Mathf.Max(way.Half[i], way.Half[j]));
                        int x0 = Mathf.FloorToInt(Mathf.Min(a.x, b.x) / Cell), x1 = Mathf.FloorToInt(Mathf.Max(a.x, b.x) / Cell);
                        int z0 = Mathf.FloorToInt(Mathf.Min(a.z, b.z) / Cell), z1 = Mathf.FloorToInt(Mathf.Max(a.z, b.z) / Cell);
                        for (int x = x0; x <= x1; x++)
                        for (int z = z0; z <= z1; z++)
                        {
                            var key = new Vector2Int(x, z);
                            if (!grid.TryGetValue(key, out var list)) grid[key] = list = new List<int>();
                            list.Add(id);
                        }
                    }
                    i = j;
                }
            }
            _segments = segments.ToArray();
            _maxHalf = maxHalf;
            // Into a dense array (looked up millions of times as the forest is made, from many threads at once).
            int minX = grid.Keys.Min(k => k.x), maxX = grid.Keys.Max(k => k.x), minZ = grid.Keys.Min(k => k.y), maxZ = grid.Keys.Max(k => k.y);
            _gridX = minX;
            _gridZ = minZ;
            _gridWidth = maxX - minX + 1;
            _gridDepth = maxZ - minZ + 1;
            var cells = new int[_gridWidth * _gridDepth][];
            foreach (var pair in grid) cells[(pair.Key.y - minZ) * _gridWidth + pair.Key.x - minX] = pair.Value.ToArray();
            _cells = cells;
        }

        /// <summary>Inside a ditch or the ravine (nothing grows or stands there).</summary>
        public bool InCutAt(float x, float z) => InCut(x, z);

        /// <summary>Flat unit direction from a point towards the nearest corridor.</summary>
        public Vector3 TowardsCorridor(float x, float z)
        {
            Nearest(x, z, out int segment, out float k);
            if (segment < 0) return Vector3.forward;
            var seg = _segments[segment];
            var to = new Vector3(seg.A.x + seg.AB.x * k - x, 0f, seg.A.z + seg.AB.y * k - z);
            return to.sqrMagnitude > 0.0001f ? to.normalized : Vector3.forward;
        }

        /// <summary>Bounds of every corridor, grown by <paramref name="margin"/> metres.</summary>
        public Bounds CorridorBounds(float margin)
        {
            var bounds = new Bounds(_ways[0].Points[0], Vector3.zero);
            foreach (var way in _ways) foreach (var p in way.Points) bounds.Encapsulate(p);
            bounds.Expand(new Vector3(margin * 2f, 0f, margin * 2f));
            return bounds;
        }

        /// <summary>Drops the cached shape (the setup changed the ways).</summary>
        public void Invalidate() { _cells = null; _hedges = null; }

        /// <summary>Makes the corridor search and the hedge lines ready for worker threads (they are built lazily otherwise).</summary>
        public void Prepare() { Index(); FindHedges(); }

        // ---------------------------------------------------------------- meshes

        /// <summary>One tile's meshes as plain lists, worked out off the main thread.</summary>
        private sealed class TileMesh
        {
            public string Name;
            public readonly List<Vector3> Vertices = new List<Vector3>();
            public readonly List<Vector3> Normals = new List<Vector3>();
            public readonly List<Vector2> Uvs = new List<Vector2>();
            /// <summary>What the ground is like at each vertex, for its material: trodden, damp, bare face, leaf-covered (r, g, b, a).</summary>
            public readonly List<Color> Colors = new List<Color>();
            public readonly List<int> Triangles = new List<int>();
            public bool Solid, Nav;
        }

        public void Build()
        {
            Clear();
            if (!HasWays) return;
            float started = Time.realtimeSinceStartup;
            Invalidate();
            Index();
            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            foreach (var way in _ways)
                foreach (var p in way.Points) { min = Vector2.Min(min, new Vector2(p.x, p.z)); max = Vector2.Max(max, new Vector2(p.x, p.z)); }

            var near = Tiles(min - Vector2.one * (_band + 2f), max + Vector2.one * (_band + 2f), _tile);
            var far = Tiles(min - Vector2.one * (_farBand + 2f), max + Vector2.one * (_farBand + 2f), _farTile);
            var nearMeshes = new List<TileMesh>[near.Count];
            var farMeshes = new TileMesh[far.Count];
            Parallel.For(0, near.Count, i => nearMeshes[i] = NearTile(near[i].x, near[i].y));
            Parallel.For(0, far.Count, i => farMeshes[i] = FarTile(far[i].x, far[i].y));
            FindHedges();
            float shaped = Time.realtimeSinceStartup;

            // Out of the NavMesh build: the floor past the banks and the far floor (and the bamboo, see BambooGrove).
            _outer = new GameObject("Outer").transform;
            if (!Application.isPlaying) _outer.gameObject.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
            _outer.SetParent(transform, false);
            var modifier = _outer.gameObject.AddComponent<NavMeshModifier>();
            modifier.ignoreFromBuild = true;
            _made.Add(_outer.gameObject);

            var solid = new List<Mesh>();
            var solidObjects = new List<GameObject>();
            VertexCount = 0;
            foreach (var list in nearMeshes)
                foreach (var tile in list)
                {
                    var go = Make(tile, _floor, tile.Nav ? transform : _outer, ShadowCastingMode.Off);
                    // Packed forest earth with grit in it (what the eye reads on the way), not mud: the rain does not make it a bog.
                    SurfaceTag.Add(go, Surface.Earth);
                    if (tile.Solid) { solid.Add(go.GetComponent<MeshFilter>().sharedMesh); solidObjects.Add(go); }
                }
            foreach (var tile in farMeshes) if (tile != null) Make(tile, _floor, _outer, ShadowCastingMode.Off);
            HedgeColliders();

            // Cook the colliders on worker threads, then hand them over (the same cooking options as a MeshCollider's own).
            var ids = new EntityId[solid.Count];
            for (int i = 0; i < ids.Length; i++) ids[i] = solid[i].GetEntityId();
            float made = Time.realtimeSinceStartup;
            Parallel.For(0, ids.Length, i => Physics.BakeMesh(ids[i], false));
            float cooked = Time.realtimeSinceStartup;
            for (int i = 0; i < solidObjects.Count; i++) solidObjects[i].AddComponent<MeshCollider>().sharedMesh = solid[i];
            BuildSeconds = Time.realtimeSinceStartup - started;
            Timing = $"shape {shaped - started:F2} s on {System.Environment.ProcessorCount} threads · meshes {made - shaped:F2} · cook {cooked - made:F2} · colliders {Time.realtimeSinceStartup - cooked:F2} · hedges {_hedges.Count}, {_hedges.Sum(h => (h.Points.Length - 1) * HedgeStep):F0} m";
        }

        private static List<Vector2> Tiles(Vector2 min, Vector2 max, float size)
        {
            var tiles = new List<Vector2>();
            for (float tx = Mathf.Floor(min.x / size) * size; tx < max.x; tx += size)
                for (float tz = Mathf.Floor(min.y / size) * size; tz < max.y; tz += size)
                    tiles.Add(new Vector2(tx, tz));
            return tiles;
        }

        /// <summary>
        /// A fine tile in three parts, quad by quad on its farthest corner: solid and in the NavMesh (corridor, bank slopes),
        /// solid only (bank tops and just beyond), seen only (the forest floor out to the fine band). The parts share their
        /// edge vertices, so they meet without a crack.
        /// </summary>
        private List<TileMesh> NearTile(float x0, float z0)
        {
            var result = new List<TileMesh>();
            // One ring of samples beyond the tile, so the normals at its edge need no extra searches.
            int n = Mathf.RoundToInt(_tile / _step) + 1, m = n + 2;
            var keep = new bool[m * m];
            var heights = new float[m * m];
            var edges = new float[m * m];
            int kept = 0;
            for (int j = 0; j < m; j++)
                for (int i = 0; i < m; i++)
                {
                    float x = x0 + (i - 1) * _step, z = z0 + (j - 1) * _step;
                    int id = j * m + i;
                    heights[id] = Shape(x, z, out float edge);
                    edges[id] = edge;
                    keep[id] = edge < _band || InCut(x, z);
                    if (keep[id] && i > 0 && j > 0 && i <= n && j <= n) kept++;
                }
            if (kept < 4) return result;
            var parts = new[]
            {
                new TileMesh { Name = $"ForestGround_{x0}_{z0}", Solid = true, Nav = true },
                new TileMesh { Name = $"ForestShelf_{x0}_{z0}", Solid = true },
                new TileMesh { Name = $"ForestFloor_{x0}_{z0}" }
            };
            var maps = new int[3][];
            for (int p = 0; p < 3; p++) { maps[p] = new int[m * m]; for (int q = 0; q < maps[p].Length; q++) maps[p][q] = -1; }
            int Vertex(int part, int id)
            {
                var map = maps[part];
                if (map[id] >= 0) return map[id];
                var tile = parts[part];
                int i = id % m, j = id / m;
                float x = x0 + (i - 1) * _step, z = z0 + (j - 1) * _step;
                map[id] = tile.Vertices.Count;
                tile.Vertices.Add(new Vector3(x, heights[id], z));
                float hx = heights[id + 1] - heights[id - 1], hz = heights[id + m] - heights[id - m];
                var normal = new Vector3(-hx, 2f * _step, -hz).normalized;
                tile.Normals.Add(normal);
                tile.Uvs.Add(new Vector2(x, z));
                tile.Colors.Add(Look(x, z, edges[id], normal));
                return map[id];
            }
            for (int j = 1; j < n; j++)
                for (int i = 1; i < n; i++)
                {
                    int a = j * m + i, b = a + 1, c = a + m, d = c + 1;
                    if (!keep[a] || !keep[b] || !keep[c] || !keep[d]) continue;
                    float far = Mathf.Max(Mathf.Max(edges[a], edges[b]), Mathf.Max(edges[c], edges[d]));
                    int part = far < _navBand ? 0 : far < _solidBand ? 1 : 2;
                    var tris = parts[part].Triangles;
                    int va = Vertex(part, a), vb = Vertex(part, b), vc = Vertex(part, c), vd = Vertex(part, d);
                    tris.Add(va); tris.Add(vc); tris.Add(vb);
                    tris.Add(vb); tris.Add(vc); tris.Add(vd);
                }
            foreach (var part in parts) if (part.Triangles.Count > 0) result.Add(part);
            return result;
        }

        /// <summary>
        /// A coarse tile of the far floor, from just inside the fine band (laid a hand lower, under it) out to the skyline;
        /// left out where a cut runs deep (the fine ground shows those) and over the town she ran from.
        /// </summary>
        private TileMesh FarTile(float x0, float z0)
        {
            int n = Mathf.RoundToInt(_farTile / _farStep) + 1, m = n + 2;
            var keep = new bool[m * m];
            var heights = new float[m * m];
            int kept = 0;
            for (int j = 0; j < m; j++)
                for (int i = 0; i < m; i++)
                {
                    float x = x0 + (i - 1) * _farStep, z = z0 + (j - 1) * _farStep;
                    int id = j * m + i;
                    float edge = Corridor(x, z, out float pathY);
                    float h = pathY + Bank(edge, x, z);
                    float carved = h;
                    foreach (var cut in _cuts) carved = Carve(cut, x, z, carved);
                    heights[id] = carved - 0.15f;
                    keep[id] = edge >= _band - 3f && edge < _farBand && carved > h - 0.5f && x > _openUntilX - 12f;
                    if (keep[id] && i > 0 && j > 0 && i <= n && j <= n) kept++;
                }
            if (kept < 4) return null;
            var tile = new TileMesh { Name = $"ForestFar_{x0}_{z0}" };
            var map = new int[m * m];
            for (int q = 0; q < map.Length; q++) map[q] = -1;
            for (int j = 1; j <= n; j++)
                for (int i = 1; i <= n; i++)
                {
                    int id = j * m + i;
                    if (!keep[id]) continue;
                    float x = x0 + (i - 1) * _farStep, z = z0 + (j - 1) * _farStep;
                    map[id] = tile.Vertices.Count;
                    tile.Vertices.Add(new Vector3(x, heights[id], z));
                    float hx = heights[id + 1] - heights[id - 1], hz = heights[id + m] - heights[id - m];
                    var normal = new Vector3(-hx, 2f * _farStep, -hz).normalized;
                    tile.Normals.Add(normal);
                    tile.Uvs.Add(new Vector2(x, z));
                    tile.Colors.Add(Look(x, z, 99f, normal));
                }
            for (int j = 1; j < n; j++)
                for (int i = 1; i < n; i++)
                {
                    int a = map[j * m + i], b = map[j * m + i + 1], c = map[(j + 1) * m + i], d = map[(j + 1) * m + i + 1];
                    if (a < 0 || b < 0 || c < 0 || d < 0) continue;
                    tile.Triangles.Add(a); tile.Triangles.Add(c); tile.Triangles.Add(b);
                    tile.Triangles.Add(b); tile.Triangles.Add(c); tile.Triangles.Add(d);
                }
            return tile.Triangles.Count > 0 ? tile : null;
        }

        private bool InCut(float x, float z)
        {
            foreach (var cut in _cuts) if (Carve(cut, x, z, 1000f) < 1000f) return true;
            return false;
        }

        private GameObject Make(TileMesh tile, Material material, Transform parent, ShadowCastingMode shadows)
        {
            var mesh = new Mesh { name = tile.Name, indexFormat = tile.Vertices.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
            mesh.SetVertices(tile.Vertices);
            mesh.SetNormals(tile.Normals);
            mesh.SetUVs(0, tile.Uvs);
            if (tile.Colors.Count == tile.Vertices.Count) mesh.SetColors(tile.Colors);
            mesh.SetTriangles(tile.Triangles, 0);
            mesh.RecalculateBounds();
            VertexCount += tile.Vertices.Count;
            var go = new GameObject(tile.Name);
            if (!Application.isPlaying) go.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
            mesh.hideFlags = go.hideFlags;
            go.transform.SetParent(parent, false);
            go.layer = tile.Solid ? Ground : 0;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = shadows;
            _made.Add(go);
            return go;
        }

        private void Clear()
        {
            foreach (var go in _made)
            {
                if (go == null) continue;
                var filter = go.GetComponent<MeshFilter>();
                if (filter != null) DestroyNow(filter.sharedMesh);
                DestroyNow(go);
            }
            _made.Clear();
            _outer = null;
            // Anything a previous Editor session left behind (hidden, unsaved): never two grounds on top of each other.
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i).gameObject;
                if ((child.hideFlags & HideFlags.DontSave) == 0) continue;
                foreach (var filter in child.GetComponentsInChildren<MeshFilter>(true)) DestroyNow(filter.sharedMesh);
                DestroyNow(child);
            }
        }

        private static void DestroyNow(UnityEngine.Object thing)
        {
            if (thing == null) return;
            if (Application.isPlaying) Destroy(thing);
            else DestroyImmediate(thing);
        }

        // ---------------------------------------------------------------- the look of the ground

        /// <summary>
        /// The ground's vertex colour, read by its material: r trodden (the line feet keep to, darker and packed), g damp
        /// (hollows where the rain stands, the bottoms of cuts), b bare earth (faces too steep for leaves to lie on),
        /// a fallen leaves (the forest floor, a few blown onto the edges of the way).
        /// </summary>
        private Color Look(float x, float z, float edge, Vector3 normal)
        {
            float open = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(_openUntilX - 4f, _openUntilX + 8f, x));
            float wander = ForestNoise.Perlin(x * 0.21f + 5f, z * 0.21f);
            float trodden = (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-1.1f + wander * 0.6f, 0.1f, edge))) * Mathf.Lerp(0.5f, 1f, open);
            float damp = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.48f, 0.8f, ForestNoise.Perlin(x * 0.13f + 40f, z * 0.13f + _seed)));
            damp *= 1f - 0.65f * Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.2f, 1.5f, edge));
            if (InCut(x, z)) damp = Mathf.Max(damp, 0.8f);
            float bare = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.9f, 0.6f, normal.y));
            float leaves = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.15f, 1.6f, edge)) * (1f - bare * 0.85f)
                         * Mathf.Lerp(0.5f, 1f, ForestNoise.Perlin(x * 0.19f, z * 0.19f + 77f));
            leaves = Mathf.Max(leaves, (1f - trodden) * 0.35f * ForestNoise.Perlin(x * 0.5f + 3f, z * 0.5f));
            return new Color(trodden, damp, bare, leaves);
        }

        // ---------------------------------------------------------------- the hedges

        /// <summary>A line of bamboo hedge on the lip of a low bank: points on the ground and, at each, the way out of the corridor.</summary>
        public sealed class Hedge
        {
            public Vector3[] Points, Outward;
        }

        private const float HedgeStep = 0.5f;
        private List<Hedge> _hedges;

        /// <summary>
        /// Where the bank alone does not hold her (she could find footing past it from the top of a jump, perhaps from a
        /// first footing on its lip), a thick hedge of bamboo grows along its lip: <see cref="BambooGrove"/> grows it, the
        /// ground gives it its colliders. Found by walking out from the way's edge every half metre on both sides.
        /// </summary>
        public IReadOnlyList<Hedge> Hedges { get { Prepare(); return _hedges; } }

        private void FindHedges()
        {
            if (_hedges != null) return;
            var found = new List<Hedge>[_ways.Length * 2];
            Parallel.For(0, found.Length, j => found[j] = HedgesAlong(j / 2, j % 2 == 0 ? -1 : 1));
            var hedges = found.SelectMany(list => list).ToList();
            hedges.AddRange(DitchHedges());
            _hedges = hedges;
        }

        /// <summary>
        /// A ditch runs out sideways into the forest, under the banks: from inside it (jumped into at its lip, beside the
        /// way) the bank behind a hedge could be reached. So the hedge also lines both lips of each ditch, from the way's
        /// edge three metres out, closing the corner.
        /// </summary>
        private IEnumerable<Hedge> DitchHedges()
        {
            foreach (var cut in _cuts)
            {
                if (cut.Length > 5f) continue; // the ravine: the bridge is the only way over it, and the drop is a fall
                Vector3 forward = cut.Forward;
                forward.y = 0f;
                forward.Normalize();
                var right = new Vector3(forward.z, 0f, -forward.x);
                float half = Mathf.Max(0.5f, -Corridor(cut.Centre.x, cut.Centre.z, out _));
                foreach (int side in new[] { -1, 1 })
                    foreach (int lip in new[] { -1, 1 })
                    {
                        var points = new List<Vector3>();
                        var outward = new List<Vector3>();
                        for (int k = 0; k <= 6; k++)
                        {
                            Vector3 p = cut.Centre + forward * lip * (cut.Length * 0.5f + 0.35f) + right * side * (half + 0.4f + k * HedgeStep);
                            if (InCut(p.x, p.z)) continue;
                            p.y = Height(p.x, p.z);
                            points.Add(p);
                            outward.Add(forward * lip);
                        }
                        if (points.Count >= 3) yield return new Hedge { Points = points.ToArray(), Outward = outward.ToArray() };
                    }
            }
        }

        /// <summary>One look out from the way's edge: where, which way, the way's height, the ground's heights going out (null: not to be hedged).</summary>
        private struct Section
        {
            public Vector3 Edge, Outward;
            public float[] Heights;
            /// <summary>How high she may already stand here on something by the way (a log, a rock).</summary>
            public float Hold;
        }

        private const float SectionStep = 0.06f;
        private const int SectionCount = 46;
        /// <summary>How far along the way a footing still counts: a running jump carries her about this far.</summary>
        private const int JumpSections = 9;

        private List<Hedge> HedgesAlong(int w, int side)
        {
            var way = _ways[w];
            bool part = way.CorridorTo > way.CorridorFrom;
            var sections = new List<Section>();
            float s = 0f, offset = 0f;
            for (int i = 0; i + 1 < way.Points.Length; i++)
            {
                Vector3 a = way.Points[i], b = way.Points[i + 1];
                float length = Vector3.Distance(a, b);
                var dir = new Vector3(b.x - a.x, 0f, b.z - a.z).normalized;
                var outward = new Vector3(dir.z, 0f, -dir.x) * side;
                for (; offset < length; offset += HedgeStep)
                {
                    float k = offset / length;
                    Vector3 p = Vector3.Lerp(a, b, k);
                    var section = new Section { Edge = p + outward * Mathf.Lerp(way.Half[i], way.Half[i + 1], k), Outward = outward };
                    bool inside = !part || (s + offset >= way.CorridorFrom && s + offset <= way.CorridorTo);
                    if (inside && section.Edge.x > _openUntilX + 6f) section.Heights = Profile(section.Edge, outward, p.y);
                    if (_footholds != null)
                        foreach (var hold in _footholds)
                        {
                            float dx = section.Edge.x - hold.Centre.x, dz = section.Edge.z - hold.Centre.z;
                            if (dx * dx + dz * dz < hold.Radius * hold.Radius) section.Hold = Mathf.Max(section.Hold, hold.Top);
                        }
                    sections.Add(section);
                }
                offset -= length;
                s += length;
            }
            // Where she may stand depends on where she stood a jump before (on a lip held by a hedge, say, she is higher
            // when she reaches the next bank): carried along the way until nothing changes.
            int n = sections.Count;
            var found = new float[n];
            var inner = new float[n];
            var from = new float[n];
            for (int k = 0; k < n; k++) from[k] = sections[k].Hold;
            for (int pass = 0; pass < 6; pass++)
            {
                for (int k = 0; k < n; k++)
                    found[k] = sections[k].Heights != null ? HedgeAt(sections[k].Heights, from[k], out inner[k]) : -1f;
                bool changed = false;
                for (int k = 0; k < n; k++)
                {
                    float most = sections[k].Hold;
                    for (int j = Mathf.Max(0, k - JumpSections); j <= Mathf.Min(n - 1, k + JumpSections); j++)
                        if (sections[j].Heights != null) most = Mathf.Max(most, inner[j]);
                    if (most > from[k] + 0.01f) { from[k] = most; changed = true; }
                }
                if (!changed) break;
            }
            for (int k = 0; k < n; k++)
            {
                // Another way nearer than this one there (the ridge between the two ways): not this way's hedge.
                Vector3 at = sections[k].Edge + sections[k].Outward * found[k];
                if (found[k] >= 0f && Corridor(at.x, at.z, out _) < found[k] - 0.15f) found[k] = -1f;
            }
            // Short breaks closed, each run carried on a metre past both ends (never into a cut), then split into lines.
            var want = new float[n];
            for (int k = 0; k < n; k++)
            {
                want[k] = found[k];
                if (want[k] >= 0f || InCut(sections[k].Edge.x, sections[k].Edge.z)) continue;
                int back = k - 1, ahead = k + 1;
                while (back >= 0 && back > k - 4 && found[back] < 0f) back--;
                while (ahead < n && ahead < k + 4 && found[ahead] < 0f) ahead++;
                bool before = back >= 0 && found[back] >= 0f, after = ahead < n && found[ahead] >= 0f;
                if ((before && after) || (before && k - back <= 2)) want[k] = found[back];
                else if (after && ahead - k <= 2) want[k] = found[ahead];
            }
            var result = new List<Hedge>();
            var points = new List<Vector3>();
            var outward2 = new List<Vector3>();
            for (int k = 0; k <= n; k++)
            {
                if (k < n && want[k] >= 0f)
                {
                    Vector3 p = sections[k].Edge + sections[k].Outward * want[k];
                    p.y = Height(p.x, p.z);
                    points.Add(p);
                    outward2.Add(sections[k].Outward);
                    continue;
                }
                if (points.Count >= 3) result.Add(new Hedge { Points = points.ToArray(), Outward = outward2.ToArray() });
                points.Clear();
                outward2.Clear();
            }
            return result;
        }

        /// <summary>The ground's height above the way going out from its edge, every 6 cm to 2.8 m (null where a cut is: nothing is hedged there).</summary>
        private float[] Profile(Vector3 edge, Vector3 outward, float pathY)
        {
            var heights = new float[SectionCount + 2];
            for (int i = 0; i < heights.Length; i++)
            {
                float x = edge.x + outward.x * i * SectionStep, z = edge.z + outward.z * i * SectionStep;
                if (InCut(x, z)) return null;
                heights[i] = Height(x, z) - pathY;
            }
            return heights;
        }

        /// <summary>
        /// How far past the way's edge the hedge must stand here (on the first footing past its foot), or −1 when the bank
        /// holds her by itself: walking out from the edge, a footing she can reach (no higher than a jump from the highest (1.1 m, and her capsule finding a slope 0.5 m above that)
        /// footing she already has, <paramref name="from"/> among them) more than 0.7 m out means she could leave; a face
        /// she cannot get over ends the search. <paramref name="inner"/>: the highest she can stand while still on the way
        /// (within 0.7 m of its edge: a lip, a heap at the foot of a bank).
        /// </summary>
        private static float HedgeAt(float[] heights, float from, out float inner)
        {
            float reach = Mathf.Max(0.05f, Mathf.Max(heights[0], from)), first = -1f;
            inner = 0f;
            for (int i = 1; i <= SectionCount; i++)
            {
                float e = i * SectionStep, h = heights[i];
                bool footing = (heights[i + 1] - heights[i - 1]) / (2f * SectionStep) < 1.15f; // under about 49°
                if (!footing) { if (h > reach + 1.85f) return -1f; continue; }
                if (h > reach + 1.6f) return -1f;
                reach = Mathf.Max(reach, h);
                if (e <= 0.7f) inner = Mathf.Max(inner, h);
                if (e > 0.3f && first < 0f) first = e;
                if (e > 0.7f) return Mathf.Clamp(first + 0.08f, 0.4f, 0.85f);
            }
            return -1f;
        }

        /// <summary>The hedges' colliders: a tall thin box every metre along each, in the ground's NavMesh build (he keeps off them too).</summary>
        private void HedgeColliders()
        {
            var root = new GameObject("Hedges");
            if (!Application.isPlaying) root.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
            root.transform.SetParent(transform, false);
            root.layer = Ground;
            _made.Add(root);
            foreach (var hedge in Hedges)
                for (int i = 0; i + 1 < hedge.Points.Length; i += 2)
                {
                    Vector3 a = hedge.Points[i], b = hedge.Points[Mathf.Min(i + 2, hedge.Points.Length - 1)];
                    var flat = new Vector3(b.x - a.x, 0f, b.z - a.z);
                    if (flat.sqrMagnitude < 0.01f) continue;
                    float bottom = Mathf.Min(a.y, b.y) - 0.5f, top = Mathf.Max(a.y, b.y) + 3f;
                    var go = new GameObject("Hedge");
                    go.hideFlags = root.hideFlags;
                    go.layer = Ground;
                    go.transform.SetParent(root.transform, false);
                    go.transform.SetPositionAndRotation(new Vector3((a.x + b.x) * 0.5f, (bottom + top) * 0.5f, (a.z + b.z) * 0.5f), Quaternion.LookRotation(flat));
                    go.AddComponent<BoxCollider>().size = new Vector3(0.3f, top - bottom, flat.magnitude + 0.15f);
                }
        }

        // ---------------------------------------------------------------- navigation

        /// <summary>The forest's NavMesh, from the corridor and bank colliders and everything solid under the course, built off the main thread.</summary>
        private IEnumerator BuildNavMesh()
        {
            if (_navMesh == null) { NavReady = true; yield break; }
            yield return null; // obstacles and links placed under this object finish their own Awake / OnEnable first
            float started = Time.realtimeSinceStartup;
            if (_baked != null)
            {
                if (_navMesh.navMeshData != _baked) { _navMesh.RemoveData(); _navMesh.navMeshData = _baked; _navMesh.AddData(); }
                foreach (var link in _navMesh.GetComponentsInChildren<NavMeshLink>()) if (link.isActiveAndEnabled) link.UpdateLink();
                NavBuildSeconds = Time.realtimeSinceStartup - started;
                NavReady = true;
                ThuyKieu.Core.Story.StoryAutopilot.Note($"forest NavMesh baked by the setup, added in {NavBuildSeconds:F3} s");
                yield break;
            }
            _navMesh.RemoveData();
            _data = new NavMeshData(_navMesh.agentTypeID) { name = "ForestNavMesh" };
            _navMesh.navMeshData = _data;
            _navMesh.AddData();
            var operation = _navMesh.UpdateNavMesh(_data);
            while (!operation.isDone) yield return null;
            // Links registered before there was a NavMesh under their ends are joined to it now.
            foreach (var link in _navMesh.GetComponentsInChildren<NavMeshLink>()) if (link.isActiveAndEnabled) link.UpdateLink();
            NavBuildSeconds = Time.realtimeSinceStartup - started;
            NavReady = true;
            ThuyKieu.Core.Story.StoryAutopilot.Note($"forest NavMesh built in {NavBuildSeconds:F2} s");
        }
    }
}
