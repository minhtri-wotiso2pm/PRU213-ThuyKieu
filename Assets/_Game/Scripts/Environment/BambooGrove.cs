using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThuyKieu.Environment
{
    /// <summary>
    /// The bamboo of the Chapter 5 forest, grown from the ground's corridors when the scene loads (too much to keep as text
    /// assets), in layers seen from the path:
    ///   near (by the path)   clumps: a few old culms in the middle, olive or dark with moss, younger ones round them and
    ///                        fresh shoots at the rim, fanning out like a vase and bowed over the way, a dead or snapped one
    ///                        here and there; twigs from the nodes carry the leaf sprays; the only culms with colliders
    ///                        (their lower part, true to size) and the only ones that cast the moon's shadows; ferns (the
    ///                        scanned ones), roots down the bank faces, litter;
    ///   hedge                along the lip of each low bank (see <see cref="ForestGround.Hedges"/>) the clumps stand close
    ///                        with a thorny tangle of twigs between their feet, the way village bamboo hedges grow: what
    ///                        holds her to the way there, its colliders the ground's;
    ///   middle               the same clumps thinner, culms crossing, leaves overlapping, bushes and fallen culms between;
    ///   far                  plain culms with a crown each, and a roof of canopy clumps with small gaps of sky;
    ///   skyline              upright cards of far forest on the rising floor, so neither the end of the ground nor the sky
    ///                        at the horizon shows, even from the top of a jump.
    /// Stands change along the way (large-scale noise picks young, old or dying bamboo, open or close) so no stretch repeats
    /// the last. Everything that grows moves in the night wind (<see cref="ForestWind"/>): culms bend from the foot, leaves
    /// flutter. Leaves and bushes are soft: they catch nobody and stay off ditch lips and the obstacles' clearings.
    /// Worked out per 40 m square on worker threads (each square with its own seed), then merged into a few meshes per
    /// square; the ferns are separate objects (one scanned mesh, many times: the renderer draws them instanced).
    /// </summary>
    [ExecuteAlways]
    public class BambooGrove : MonoBehaviour
    {
        [Serializable]
        public struct Clearing
        {
            public Vector3 Centre;
            public float Radius;
        }

        [SerializeField] private ForestGround _ground;
        [Tooltip("Culm materials: fresh green, old olive, yellowed and dead, dark and mossy.")]
        [SerializeField] private Material[] _culms = new Material[4];
        [SerializeField] private Material _leaves, _canopy, _ferns, _litter, _shrubs, _roots, _skyline, _thicket;
        [Tooltip("The leaves of the middle and far culms (the same, without the moonlight through them).")]
        [SerializeField] private Material _farLeaves;
        [Tooltip("Scanned fern clumps grown by the way (the drawn fern cards stay for the middle distance).")]
        [SerializeField] private Mesh[] _fernMeshes;
        [SerializeField] private Material _fernMaterial;
        [Tooltip("No culms here (obstacles, the bridge, the temple path), so nothing grows through them.")]
        [SerializeField] private Clearing[] _clear;
        [Tooltip("The canopy opens here (moonlit gaps over the way, the ravine).")]
        [SerializeField] private Clearing[] _skyHoles;
        [Tooltip("Distance past the corridor's edge where each layer ends: near (colliders, shadows), middle, far culms.")]
        [SerializeField] private float _near = 4.5f, _mid = 18f, _far = 40f;
        [Tooltip("The skyline cards stand between these distances.")]
        [SerializeField] private float _skylineFrom = 30f, _skylineTo = 70f;
        [SerializeField] private float _chunk = 40f;
        [SerializeField] private int _seed = 77;
        [SerializeField, Range(0f, 1f)] private float _density = 1f;

        private readonly List<GameObject> _made = new List<GameObject>();
        private readonly SortedDictionary<string, int> _layerVertices = new SortedDictionary<string, int>();

        public int CulmCount { get; private set; }
        public int ColliderCount { get; private set; }
        public int VertexCount { get; private set; }
        public int FernCount { get; private set; }
        public float BuildSeconds { get; private set; }
        /// <summary>Where the last build spent its time (QA).</summary>
        public string Timing { get; private set; }

        private void OnEnable()
        {
            Build();
            if (Application.isPlaying) ThuyKieu.Core.Story.StoryAutopilot.Note($"bamboo grown in {BuildSeconds:F2} s ({Timing}): {CulmCount} culms, {ColliderCount} colliders, {VertexCount} vertices, {FernCount} ferns");
        }
        private void OnDisable() { Clear(); }

        private sealed class Chunk
        {
            public Vector2Int Key;
            public readonly BambooMesh.Builder Near = new BambooMesh.Builder(4) { Windy = true };       // culms ×4, casting the moon's shadows
            public readonly BambooMesh.Builder Side = new BambooMesh.Builder(4) { Windy = true };       // the near culms further back: colliders, no shadows
            public readonly BambooMesh.Builder NearLeaves = new BambooMesh.Builder(3) { Windy = true }; // their leaves, the hedge tangle, the twigs (no shadows: alpha-tested shadows cost the most)
            public readonly BambooMesh.Builder Back = new BambooMesh.Builder(5) { Windy = true };       // middle and far culms ×4, leaves
            public readonly BambooMesh.Builder Roof = new BambooMesh.Builder(1) { Windy = true };       // canopy clumps
            public readonly BambooMesh.Builder Low = new BambooMesh.Builder(4) { Windy = true };        // ferns, litter, bushes, roots
            public readonly BambooMesh.Builder Skyline = new BambooMesh.Builder(1) { Windy = true };
            public readonly List<(Vector3 foot, Vector3 top, float radius)> Solid = new List<(Vector3, Vector3, float)>();
            public readonly List<(Vector3 at, float yaw, float scale, int kind)> Ferns = new List<(Vector3, float, float, int)>();
            public int Culms;
            public System.Random Random;
            public float R() => (float)Random.NextDouble();
        }

        /// <summary>What a stand is like here: how close, how old, how much dies back (large-scale, so it changes slowly along the way).</summary>
        private struct Stand
        {
            public float Close, Old, Dying;
        }

        private Stand StandAt(float x, float z) => new Stand
        {
            Close = ForestNoise.Perlin(x * 0.011f + _seed, z * 0.011f + 7f),
            Old = ForestNoise.Perlin(x * 0.009f + 19f, z * 0.009f + _seed),
            Dying = Mathf.Clamp01((ForestNoise.Perlin(x * 0.013f + 53f, z * 0.013f + _seed) - 0.55f) * 3f)
        };

        public void Build()
        {
            Clear();
            if (_ground == null || !_ground.HasWays || _culms == null || _culms.Length < 4) return;
            float started = Time.realtimeSinceStartup;
            _ground.Prepare();
            Bounds area = _ground.CorridorBounds(_skylineTo + 2f);
            var keys = new List<Vector2Int>();
            for (int x = Mathf.FloorToInt(area.min.x / _chunk); x * _chunk < area.max.x; x++)
                for (int z = Mathf.FloorToInt(area.min.z / _chunk); z * _chunk < area.max.z; z++)
                    keys.Add(new Vector2Int(x, z));
            // The hedges, cut into the squares their points fall in, so each square grows its own part.
            var hedges = new Dictionary<Vector2Int, List<(Vector3 at, Vector3 outward, Vector3 along)>>();
            foreach (var hedge in _ground.Hedges)
                for (int i = 0; i < hedge.Points.Length; i++)
                {
                    Vector3 p = hedge.Points[i];
                    Vector3 along = hedge.Points[Mathf.Min(i + 1, hedge.Points.Length - 1)] - hedge.Points[Mathf.Max(i - 1, 0)];
                    along.y = 0f;
                    var key = new Vector2Int(Mathf.FloorToInt(p.x / _chunk), Mathf.FloorToInt(p.z / _chunk));
                    if (!hedges.TryGetValue(key, out var list)) hedges[key] = list = new List<(Vector3, Vector3, Vector3)>();
                    list.Add((p, hedge.Outward[i], along.sqrMagnitude > 0.0001f ? along.normalized : Vector3.forward));
                }
            var chunks = new Chunk[keys.Count];
            Parallel.For(0, keys.Count, i => chunks[i] = Grow(keys[i], hedges.TryGetValue(keys[i], out var line) ? line : null));
            float grown = Time.realtimeSinceStartup;

            CulmCount = 0;
            VertexCount = 0;
            _layerVertices.Clear();
            ColliderCount = 0;
            FernCount = 0;
            var culmsAndLeaves = new[] { _culms[0], _culms[1], _culms[2], _culms[3], _farLeaves != null ? _farLeaves : _leaves };
            var culms = new[] { _culms[0], _culms[1], _culms[2], _culms[3] };
            foreach (var chunk in chunks)
            {
                if (chunk == null) continue;
                CulmCount += chunk.Culms;
                string key = chunk.Key.x + "_" + chunk.Key.y;
                var near = Emit("BambooNear_" + key, chunk.Near, culms, true);
                var side = Emit("BambooSide_" + key, chunk.Side, culms, false);
                Emit("BambooLeaves_" + key, chunk.NearLeaves, new[] { _leaves, _thicket != null ? _thicket : _shrubs, _culms[1] }, false);
                Emit("BambooBack_" + key, chunk.Back, culmsAndLeaves, false);
                Emit("BambooRoof_" + key, chunk.Roof, new[] { _canopy }, false);
                Emit("BambooLow_" + key, chunk.Low, new[] { _ferns, _litter, _shrubs, _roots }, false);
                Emit("BambooSkyline_" + key, chunk.Skyline, new[] { _skyline }, false);
                Ferns(chunk);
                var holder = near != null ? near : side;
                if (holder == null) continue;
                foreach (var (foot, top, radius) in chunk.Solid) Solid(holder.transform, foot, top, radius);
            }
            BuildSeconds = Time.realtimeSinceStartup - started;
            Timing = $"grow {grown - started:F2} s · meshes and colliders {Time.realtimeSinceStartup - grown:F2} · vertices {string.Join(" ", _layerVertices.Select(p => p.Key + " " + p.Value))}";
        }

        // ---------------------------------------------------------------- one square (worker thread)

        private Chunk Grow(Vector2Int key, List<(Vector3 at, Vector3 outward, Vector3 along)> hedge)
        {
            float x0 = key.x * _chunk, z0 = key.y * _chunk;
            // Nothing of the forest is this far out: skip the square without growing it.
            if (_ground.Corridor(x0 + _chunk * 0.5f, z0 + _chunk * 0.5f, out _) - _chunk * 0.71f > _skylineTo) return null;
            var chunk = new Chunk { Key = key, Random = new System.Random(_seed * 7919 + key.x * 104729 + key.y * 15485863) };

            // Clumps two metres apart or so, kept by distance, clumping noise and the stand; each clump a handful of culms
            // or many, by how close the stand is here.
            const float cell = 2f;
            for (float x = x0; x < x0 + _chunk; x += cell)
                for (float z = z0; z < z0 + _chunk; z += cell)
                {
                    float px = x + chunk.R() * cell, pz = z + chunk.R() * cell;
                    float edge = _ground.Corridor(px, pz, out _);
                    if (edge < 0.7f || edge > _far) continue;
                    var stand = StandAt(px, pz);
                    float clumping = ForestNoise.Perlin(px * 0.16f + _seed, pz * 0.16f);
                    float chance = (edge < _near ? 0.24f : edge < _mid ? 0.25f : 0.17f) * Mathf.Lerp(0.25f, 1.75f, clumping) * Mathf.Lerp(0.6f, 1.35f, stand.Close) * _density;
                    if (chunk.R() > chance || Blocked(px, pz)) continue;
                    int count = edge < _near ? 3 + chunk.Random.Next(10) : edge < _mid ? 2 + chunk.Random.Next(7) : 1 + chunk.Random.Next(4);
                    Clump(chunk, new Vector3(px, 0f, pz), count, stand, 0f);
                }
            if (hedge != null) Hedge(chunk, hedge);

            Roof(chunk, x0, z0);
            Undergrowth(chunk, x0, z0);
            Skyline(chunk, x0, z0);
            return chunk;
        }

        private bool Blocked(float x, float z) => Cleared(_clear, x, z) || _ground.InCutAt(x, z);

        /// <summary>
        /// A clump: the oldest culms at its heart, younger round them and fresh shoots at the rim, all fanning out from the
        /// middle (more the further out they stand), bowed outward and, by the way, over it. <paramref name="cleared"/>:
        /// how much smaller the obstacles' clearings count here (a hedge stands closer to them than other bamboo may).
        /// </summary>
        private void Clump(Chunk chunk, Vector3 centre, int count, Stand stand, float cleared)
        {
            float vigour = chunk.R();
            float radius = 0.18f + 0.06f * count * (0.7f + chunk.R() * 0.6f);
            float clumpAge = Mathf.Clamp01(stand.Old * 0.7f + vigour * 0.5f - 0.1f);
            for (int i = 0; i < count; i++)
            {
                float a = chunk.R() * Mathf.PI * 2f, r = radius * Mathf.Sqrt((i + chunk.R()) / count);
                float px = centre.x + Mathf.Cos(a) * r, pz = centre.z + Mathf.Sin(a) * r;
                float edge = _ground.Corridor(px, pz, out _);
                if (edge < 0.4f || _ground.InCutAt(px, pz) || Cleared(_clear, px, pz, cleared)) continue;
                float rim = radius > 0.01f ? r / radius : 0f;
                var outward = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                Culm(chunk, px, pz, edge, stand, Mathf.Clamp01(clumpAge + (0.5f - rim) * 0.5f + (chunk.R() - 0.5f) * 0.3f), rim, outward);
            }
        }

        /// <summary>
        /// One culm of a clump: <paramref name="age"/> 0 a fresh shoot, 1 an old culm; <paramref name="rim"/> how far out in
        /// its clump it stands (the rim leans out most), <paramref name="outward"/> away from the clump's heart.
        /// </summary>
        private void Culm(Chunk chunk, float px, float pz, float edge, Stand stand, float age, float rim, Vector3 outward)
        {
            var foot = new Vector3(px, _ground.Height(px, pz) - 0.1f, pz);
            bool near = edge < _near, mid = edge < _mid;
            chunk.Culms++;
            // Old culms in a dying stand die back, yellowed, some snapped off and bare; the shoots are fresh green.
            bool dead = age > 0.35f && chunk.R() < 0.04f + stand.Dying * 0.35f + age * 0.06f;
            bool snapped = dead && chunk.R() < 0.4f;
            int material = dead ? 2 : age < 0.35f ? 0 : age < 0.75f ? (chunk.R() < 0.7f ? 1 : 0) : (chunk.R() < 0.55f ? 3 : 1);
            float radius = Mathf.Lerp(0.024f, 0.085f, age * age) * (0.85f + chunk.R() * 0.3f) * (mid ? 1f : 1.15f);
            float length = snapped ? 1.8f + chunk.R() * 3.2f : Mathf.Lerp(6.5f, 15.5f, age) * (0.85f + chunk.R() * 0.3f);
            // The clump fans out; by the path its culms also lean in and bow over the way; some bow far.
            Vector3 toPath = _ground.TowardsCorridor(px, pz);
            var wander = new Vector3(chunk.R() - 0.5f, 0f, chunk.R() - 0.5f);
            Vector3 lean = outward * (0.04f + 0.24f * rim) + wander * 0.06f + (near ? toPath * (0.05f + chunk.R() * 0.14f) : Vector3.zero);
            Vector3 axis = (Vector3.up + lean).normalized;
            Vector3 bowDirection = (outward * (0.4f + rim) + (near ? toPath * 0.9f : Vector3.zero) + wander * 0.3f);
            bowDirection = bowDirection.sqrMagnitude > 0.001f ? bowDirection.normalized : Vector3.forward;
            float bow = snapped ? 0.05f : (near ? 0.5f : 0.25f) + chunk.R() * (near ? 1.5f : 0.7f) * (0.4f + rim) + (chunk.R() < 0.07f ? 1.8f : 0f);
            float u = chunk.R();
            if (!mid)
            {
                // Far: a plain culm and a crown, enough to fill the depth between the trunks nearer.
                BambooMesh.Culm(chunk.Back, material, foot, axis, length, radius, bow, bowDirection, 3, length, 0.42f, u);
                if (snapped) return;
                for (int i = 0; i < 2; i++)
                {
                    float t = 0.62f + i * 0.13f;
                    Vector3 at = BambooMesh.Along(foot, axis, length, bow, bowDirection, t);
                    float a = chunk.R() * Mathf.PI * 2f, size = 2.6f + chunk.R() * 1.4f;
                    BambooMesh.Spray(chunk.Back, 4, at, new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)), size, size * 0.7f, 0.4f + chunk.R() * 0.4f, chunk.R() < 0.5f,
                        chunk.Random.Next(4), t * Mathf.Clamp01(length / 10f), u, false);
                }
                return;
            }
            // Only the culms right by the way cast the moon's shadows across it (the rest cost as much and show little).
            var builder = near ? (edge < 2.2f ? chunk.Near : chunk.Side) : chunk.Back;
            BambooMesh.Culm(builder, material, foot, axis, length, radius, bow, bowDirection, near ? 7 : 4, near ? 1.3f : 2.6f, 0.36f + chunk.R() * 0.12f, u, snapped ? 0.9f : 0.45f);
            if (near)
            {
                // Collider for the part a person can run into, true to the culm's size.
                Vector3 top = BambooMesh.Along(foot, axis, length, bow, bowDirection, Mathf.Min(1f, 3.2f / length));
                chunk.Solid.Add((foot + Vector3.up * 0.1f, top, radius));
            }
            if (snapped) return;
            // Branches from the nodes from a third of the way up, thickest towards the crown, so the canopy closes over the
            // way; by the path each spray hangs from a twig of its own.
            int sprays = near ? 7 + chunk.Random.Next(4) : 5 + chunk.Random.Next(3);
            float from = dead ? 0.6f : 0.33f, reach = Mathf.Clamp01(length / 10f);
            for (int i = 0; i < sprays; i++)
            {
                float t = from + (1f - from) * Mathf.Sqrt((i + chunk.R() * 0.8f) / sprays);
                Vector3 at = BambooMesh.Along(foot, axis, length, bow, bowDirection, t);
                float angle = chunk.R() * Mathf.PI * 2f;
                Vector3 side = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                Vector3 spread = side + bowDirection * 0.6f;
                float size = (near ? 1.4f : 1.6f) * (0.7f + chunk.R() * 0.6f) * Mathf.Lerp(1.1f, 0.75f, t) * (dead ? 0.6f : 1f);
                int variant = dead ? (chunk.R() < 0.5f ? 1 : 3) : chunk.Random.Next(4);
                if (near)
                {
                    // Twigs where they can be seen (the lower branches); the crown's sprays hang straight off the culm.
                    Vector3 end = at;
                    if (t < 0.7f)
                    {
                        float twig = 0.25f + chunk.R() * 0.45f;
                        end = at + (spread.normalized * 0.85f + Vector3.up * (0.25f + chunk.R() * 0.3f)).normalized * twig;
                        BambooMesh.Twig(chunk.NearLeaves, 2, at + side * radius * 0.6f, end, Mathf.Max(0.006f, radius * 0.18f), t * reach, u);
                    }
                    BambooMesh.Spray(chunk.NearLeaves, 0, end, spread, size, size * 0.65f, 0.3f + chunk.R() * 0.5f, chunk.R() < 0.5f, variant, t * reach, u, t < 0.8f);
                }
                else BambooMesh.Spray(chunk.Back, 4, at, spread, size, size * 0.65f, 0.3f + chunk.R() * 0.5f, chunk.R() < 0.5f, variant, t * reach, u, false);
            }
            // Old culms by the way: roots out of the foot of the clump and down the bank face.
            if (near && age > 0.5f && edge < 2.6f && chunk.R() < 0.5f) Roots(chunk, foot + Vector3.up * 0.1f);
        }

        /// <summary>
        /// The hedge on the lip of a low bank: clumps close together a little behind the lip (their colliders the ground's
        /// boxes and their own culms), and between their feet a tangle of thorny twigs as tall as a person, in two layers,
        /// so nothing along it looks like a way through.
        /// </summary>
        private void Hedge(Chunk chunk, List<(Vector3 at, Vector3 outward, Vector3 along)> line)
        {
            for (int i = 0; i < line.Count; i++)
            {
                var (at, outward, along) = line[i];
                if (_ground.InCutAt(at.x, at.z)) continue;
                var stand = StandAt(at.x, at.z);
                // A clump every two and a half metres or so, a little way behind the lip.
                if (i % 5 == 0 && chunk.R() < 0.75f)
                    Clump(chunk, at + outward * (0.25f + chunk.R() * 0.35f) + along * (chunk.R() - 0.5f) * 0.6f, 2 + chunk.Random.Next(4), stand, 1.4f);
                // The tangle: cards along the line (each half-metre piece), one just behind the lip, one further back.
                Vector3 next = i + 1 < line.Count ? line[i + 1].at : at + along * 0.5f;
                if ((next - at).sqrMagnitude > 1.2f) continue;
                for (int layer = 0; layer < 1; layer++)
                {
                    Vector3 back = outward * (0.05f + layer * 0.35f);
                    float tall = 1.35f + chunk.R() * 0.45f;
                    Vector3 a = at + back - along * 0.12f + Vector3.down * 0.15f, b = next + back + along * 0.12f + Vector3.down * 0.15f;
                    Vector3 lean = -outward * 0.15f * (layer == 0 ? 1f : 0.3f);
                    float u0 = (i * 0.37f + layer * 0.5f) % 1f;
                    chunk.NearLeaves.Quad(1, a, b, b + Vector3.up * tall + lean, a + Vector3.up * tall + lean, (-outward + Vector3.up * 0.8f).normalized,
                        new Vector2(u0, 0f), new Vector2(u0 + 0.25f, 1f), chunk.NearLeaves.Weights(0f, 0f, u0), chunk.NearLeaves.Weights(0.05f, 0.4f, u0));
                }
            }
        }

        /// <summary>
        /// The roof: canopy clumps every couple of metres at crown height (over the way too, a little higher), with gaps where
        /// the clumping noise is thin and wide openings at the sky holes; never over the far skyline cards, which carry their own.
        /// </summary>
        private void Roof(Chunk chunk, float x0, float z0)
        {
            for (float x = x0; x < x0 + _chunk; x += 2.6f)
                for (float z = z0; z < z0 + _chunk; z += 2.6f)
                {
                    float px = x + chunk.R() * 2.6f, pz = z + chunk.R() * 2.6f;
                    float edge = _ground.Corridor(px, pz, out float pathY);
                    if (edge > _skylineFrom || Cleared(_skyHoles, px, pz)) continue;
                    var stand = StandAt(px, pz);
                    float gaps = ForestNoise.Perlin(px * 0.21f + 3f, pz * 0.21f + _seed);
                    if (gaps > Mathf.Lerp(0.6f, 0.78f, stand.Close) - stand.Dying * 0.12f) continue;
                    // Over the way the crowns arch from the banks; further out they stand on the floor, high or low as it is.
                    float ground = edge < 1.5f ? pathY : Mathf.Max(pathY, _ground.Height(px, pz));
                    float high = edge < 0f ? 8.6f + chunk.R() * 2.6f : 7.4f + chunk.R() * 4.6f;
                    BambooMesh.Canopy(chunk.Roof, 0, new Vector3(px, ground + high, pz), 2.6f + chunk.R() * 2.2f, chunk.R() * 360f, 10f + chunk.R() * 28f);
                }
        }

        /// <summary>
        /// Low growth: ferns crowd the bank feet and lips (the scanned ones close to the way, drawn tufts further off), bushes
        /// stand between the trunks, fallen leaves lie everywhere but the running line, and a few dead culms lie where they fell.
        /// </summary>
        private void Undergrowth(Chunk chunk, float x0, float z0)
        {
            for (float x = x0; x < x0 + _chunk; x += 0.9f)
                for (float z = z0; z < z0 + _chunk; z += 0.9f)
                {
                    float px = x + chunk.R() * 0.9f, pz = z + chunk.R() * 0.9f;
                    float edge = _ground.Corridor(px, pz, out _);
                    if (edge > 14f) continue;
                    float h = _ground.Height(px, pz, out _, out bool cut);
                    if (cut) continue;
                    var p = new Vector3(px, h, pz);
                    // The floor of the way stays clear to run; ferns start at the bank foot, in patches.
                    float patch = ForestNoise.Perlin(px * 0.35f + 11f, pz * 0.35f);
                    if (edge > 0.05f && edge < 3.2f && _fernMeshes != null && _fernMeshes.Length > 0)
                    {
                        if (chunk.R() < 0.2f * patch * _density && !Cleared(_clear, px, pz, 2f))
                            chunk.Ferns.Add((p - Vector3.up * 0.04f, chunk.R() * 360f, 0.75f + chunk.R() * 0.6f, chunk.Random.Next(_fernMeshes.Length)));
                    }
                    else if (edge > 3.2f && edge < 7.5f && chunk.R() < 0.3f * patch * _density)
                        BambooMesh.Tuft(chunk.Low, 0, p - Vector3.up * 0.05f, 0.5f + chunk.R() * 0.6f, chunk.R() * 360f);
                    if (edge > 1.6f && chunk.R() < 0.06f * _density && !Cleared(_clear, px, pz))
                    {
                        float size = 1.1f + chunk.R() * 1.3f;
                        BambooMesh.Tuft(chunk.Low, 2, p - Vector3.up * 0.1f, size, chunk.R() * 360f, size * (0.75f + chunk.R() * 0.5f));
                    }
                    if (edge > -0.6f && edge < 8f && chunk.R() < (edge < 0f ? 0.1f : 0.16f) * _density)
                        BambooMesh.Litter(chunk.Low, 1, p + Vector3.up * 0.03f, edge < 0f ? Vector3.up : Normal(px, pz, h), 0.45f + chunk.R() * 0.4f, chunk.R() * 360f);
                }
            // Fallen culms off the way, lying across one another (no colliders: nobody runs there).
            int fallen = chunk.Random.Next(5);
            for (int i = 0; i < fallen; i++)
            {
                float px = x0 + chunk.R() * _chunk, pz = z0 + chunk.R() * _chunk;
                float edge = _ground.Corridor(px, pz, out _);
                if (edge < 3f || edge > _mid || Blocked(px, pz)) continue;
                float yaw = chunk.R() * Mathf.PI * 2f, length = 4f + chunk.R() * 7f;
                var dir = new Vector3(Mathf.Cos(yaw), -0.04f, Mathf.Sin(yaw)).normalized;
                var foot = new Vector3(px, _ground.Height(px, pz) + 0.04f, pz);
                Vector3 end = foot + dir * length;
                float drop = _ground.Height(end.x, end.z) + 0.04f - end.y;
                dir = (dir * length + Vector3.up * drop).normalized;
                BambooMesh.Culm(chunk.Back, 2, foot, dir, length, 0.035f + chunk.R() * 0.03f, 0.1f, Vector3.up, 5, 2.5f, 0.4f, chunk.R(), 0.45f, 0f);
            }
        }

        /// <summary>
        /// Roots from the foot of an old clump: a few dark runners down towards the way, each laid on the ground in short
        /// pieces so it follows the bank's face, half sunk into the earth, thinning as it goes.
        /// </summary>
        private void Roots(Chunk chunk, Vector3 foot)
        {
            Vector3 down = _ground.TowardsCorridor(foot.x, foot.z);
            for (int i = 0; i < 2 + chunk.Random.Next(3); i++)
            {
                var wander = new Vector3(chunk.R() - 0.5f, 0f, chunk.R() - 0.5f);
                Vector3 dir = (down + wander * 1.2f).normalized;
                float radius = 0.01f + chunk.R() * 0.012f;
                Vector3 at = foot + new Vector3(chunk.R() - 0.5f, 0f, chunk.R() - 0.5f) * 0.15f;
                at.y = _ground.Height(at.x, at.z) + radius * 0.4f;
                int pieces = 3 + chunk.Random.Next(3);
                for (int k = 0; k < pieces; k++)
                {
                    Vector3 next = at + dir * (0.22f + chunk.R() * 0.18f);
                    next.y = _ground.Height(next.x, next.z) + radius * 0.4f;
                    if (_ground.Corridor(next.x, next.z, out _) < 0.05f) break; // never out onto the way
                    BambooMesh.Culm(chunk.Low, 3, at, (next - at).normalized, Vector3.Distance(at, next), radius, 0f, Vector3.up, 4, 1f, 0.25f, chunk.R(), 0.85f, 0f);
                    at = next;
                    radius *= 0.82f;
                    dir = (dir + new Vector3(chunk.R() - 0.5f, 0f, chunk.R() - 0.5f) * 0.5f).normalized;
                }
            }
        }

        /// <summary>Upright pictures of far forest on the rising floor, turned to the way, in rows that overlap.</summary>
        private void Skyline(Chunk chunk, float x0, float z0)
        {
            for (float x = x0; x < x0 + _chunk; x += 4.2f)
                for (float z = z0; z < z0 + _chunk; z += 4.2f)
                {
                    float px = x + chunk.R() * 4.2f, pz = z + chunk.R() * 4.2f;
                    float edge = _ground.Corridor(px, pz, out _);
                    if (edge < _skylineFrom || edge > _skylineTo || _ground.InCutAt(px, pz)) continue;
                    if (chunk.R() > 0.62f) continue;
                    var foot = new Vector3(px, _ground.Height(px, pz) - 1f, pz);
                    Vector3 facing = _ground.TowardsCorridor(px, pz);
                    float width = 6f + chunk.R() * 4f, height = 14f + chunk.R() * 10f + (edge - _skylineFrom) * 0.12f;
                    BambooMesh.Silhouette(chunk.Skyline, 0, foot, facing, width, height, chunk.R() < 0.5f);
                }
        }

        /// <summary>The slope of the floor at a point whose height is known (two more samples).</summary>
        private Vector3 Normal(float x, float z, float h)
        {
            const float e = 0.3f;
            float hx = _ground.Height(x + e, z) - h, hz = _ground.Height(x, z + e) - h;
            return new Vector3(-hx, e, -hz).normalized;
        }

        private static bool Cleared(Clearing[] clearings, float x, float z, float smaller = 0f)
        {
            if (clearings == null) return false;
            foreach (var c in clearings)
            {
                float dx = x - c.Centre.x, dz = z - c.Centre.z, r = c.Radius - smaller;
                if (r > 0f && dx * dx + dz * dz < r * r) return true;
            }
            return false;
        }

        // ---------------------------------------------------------------- objects (main thread)

        private GameObject Emit(string name, BambooMesh.Builder builder, Material[] materials, bool shadows)
        {
            if (builder.Empty) return null;
            var mesh = builder.ToMesh(name);
            VertexCount += mesh.vertexCount;
            string layer = name.Substring(6, name.IndexOf('_') - 6).ToLowerInvariant();
            _layerVertices[layer] = (_layerVertices.TryGetValue(layer, out int sum) ? sum : 0) + mesh.vertexCount;
            var go = new GameObject(name);
            if (!Application.isPlaying) go.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
            mesh.hideFlags = go.hideFlags;
            go.transform.SetParent(transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = materials;
            renderer.shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            _made.Add(go);
            return go;
        }

        /// <summary>The square's scanned ferns, each its own small object (the same mesh and material: drawn instanced), culled when small on screen.</summary>
        private void Ferns(Chunk chunk)
        {
            if (chunk.Ferns.Count == 0 || _fernMaterial == null) return;
            var holder = new GameObject("Ferns_" + chunk.Key.x + "_" + chunk.Key.y);
            if (!Application.isPlaying) holder.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
            holder.transform.SetParent(transform, false);
            _made.Add(holder);
            foreach (var (at, yaw, scale, kind) in chunk.Ferns)
            {
                var mesh = _fernMeshes[kind];
                if (mesh == null) continue;
                var go = new GameObject("Fern");
                go.hideFlags = holder.hideFlags;
                go.transform.SetParent(holder.transform, false);
                go.transform.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f) * Upright(mesh));
                go.transform.localScale = Vector3.one * scale;
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = go.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = _fernMaterial;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                var lod = go.AddComponent<LODGroup>();
                lod.SetLODs(new[] { new LOD(0.012f, new Renderer[] { renderer }) });
                lod.RecalculateBounds();
                FernCount++;
            }
        }

        /// <summary>
        /// A scanned plant or rock stood up: the scans keep their up along the mesh's z (the turn is in their file's object,
        /// not in the mesh); a clump standing on the ground has its bounds above z = 0, which tells which way z points.
        /// </summary>
        public static Quaternion Upright(Mesh mesh) => Quaternion.FromToRotation(mesh.bounds.center.z >= 0f ? Vector3.forward : Vector3.back, Vector3.up);

        private void Solid(Transform parent, Vector3 foot, Vector3 top, float radius)
        {
            var go = new GameObject("Culm");
            if (!Application.isPlaying) go.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
            go.layer = 8;
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation((foot + top) * 0.5f, Quaternion.FromToRotation(Vector3.up, (top - foot).normalized));
            var capsule = go.AddComponent<CapsuleCollider>();
            capsule.radius = Mathf.Max(radius, 0.04f);
            capsule.height = Vector3.Distance(foot, top) + capsule.radius * 2f;
            ColliderCount++;
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
            // Anything a previous Editor session left behind (hidden, unsaved): never two groves on top of each other.
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i).gameObject;
                if ((child.hideFlags & HideFlags.DontSave) == 0) continue;
                var filter = child.GetComponent<MeshFilter>();
                if (filter != null) DestroyNow(filter.sharedMesh);
                DestroyNow(child);
            }
        }

        private static void DestroyNow(UnityEngine.Object thing)
        {
            if (thing == null) return;
            if (Application.isPlaying) Destroy(thing);
            else DestroyImmediate(thing);
        }
    }
}
