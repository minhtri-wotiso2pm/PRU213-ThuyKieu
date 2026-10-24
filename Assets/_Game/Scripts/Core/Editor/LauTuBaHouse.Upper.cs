using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static ThuyKieu.Core.Editor.LauTuBaKit;
using static ThuyKieu.Core.Editor.LauTuBaMeshKit;
using static ThuyKieu.Core.Editor.StorySceneKit;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// The upper storey round the yard and the tiled roofs over it. The upper storey is background architecture: a
    /// gallery over the verandas and the corridor (floor, posts, railing, lanterns), the upper rooms' walls with paper
    /// windows, the outer walls carried up to the eaves and closed by gables. There is no stair and no floor to stand on:
    /// nobody goes up there. The roofs are real tile roofs: a slab with tile relief, a row of round tile ends along the
    /// eaves, fascia and barge boards, a heavy ridge with curled ends, lifted eave corners, rafters and boards beneath,
    /// invisible slope colliders for sight and light.
    /// </summary>
    public static partial class LauTuBaHouse
    {
        /// <summary>Top of the veranda pillars' beams = underside of the gallery floor.</summary>
        internal const float GalleryBeamTop = Floor + 2.9f;
        /// <summary>The gallery's floor level.</summary>
        internal const float GalleryFloor = Floor + 3.05f;
        /// <summary>Top of the upper eave beam along the yard, on every side.</summary>
        internal const float UpperEave = 5.95f;

        private static readonly float[] WingPosts = { -8.9f, -6.0f, -3.0f, -0.3f, 2.3f, 5.1f, 7.9f };
        private static readonly float[] SouthPosts = { -6.0f, -3.0f, -1.15f, 1.15f, 3.0f, 6.0f };
        private static readonly float[] NorthPosts = { -6.2f, -3.4f, -0.75f, 0.75f, 3.4f, 6.2f };

        private static void UpperStorey(Transform root)
        {
            var upper = Group(root, "UpperStorey");
            const float front = 9.12f;
            // Gallery floors (their undersides are the verandas' ceilings), the beams on the veranda pillars under them.
            GalleryFloorSlab("GalleryW", -Plan.WingFace, Plan.HallFront, -front + 0.25f, Plan.RearFront, upper);
            GalleryFloorSlab("GalleryE", front - 0.25f, Plan.HallFront, Plan.WingFace, Plan.RearFront, upper);
            GalleryFloorSlab("GalleryN", -front, Plan.Screen - 0.25f, front, Plan.RearFront, upper);
            GalleryFloorSlab("GalleryS", -front, Plan.HallFront, front, -6.12f + 0.25f, upper);
            float beamY = GalleryBeamTop - 0.12f;
            Beam("BeamW", new Vector3(-front, beamY, Plan.HallFront - 0.3f), new Vector3(-front, beamY, Plan.RearFront), upper);
            Beam("BeamE", new Vector3(front, beamY, Plan.HallFront - 0.3f), new Vector3(front, beamY, Plan.RearFront), upper);
            Beam("BeamN", new Vector3(-front, beamY, Plan.Screen), new Vector3(front, beamY, Plan.Screen), upper);
            Beam("BeamS", new Vector3(-front, beamY, -6.12f), new Vector3(front, beamY, -6.12f), upper);
            // The hall front carries the south gallery's wall on its taller pillars.
            Beam("BeamHall", new Vector3(-front - 0.3f, Floor + 2.93f, -9.1f), new Vector3(front + 0.3f, Floor + 2.93f, -9.1f), upper);

            // Gallery railings (open at the corners where one gallery runs into the next), upper posts and eave beams.
            Railing("UpperRailW", new Vector3(-front, GalleryFloor, -6.12f), new Vector3(-front, GalleryFloor, Plan.Screen), upper, 0.78f);
            Railing("UpperRailE", new Vector3(front, GalleryFloor, -6.12f), new Vector3(front, GalleryFloor, Plan.Screen), upper, 0.78f);
            Railing("UpperRailN", new Vector3(-front, GalleryFloor, Plan.Screen), new Vector3(front, GalleryFloor, Plan.Screen), upper, 0.78f);
            Railing("UpperRailS", new Vector3(-front, GalleryFloor, -6.12f), new Vector3(front, GalleryFloor, -6.12f), upper, 0.78f);
            float postHeight = UpperEave - GalleryFloor;
            foreach (float z in WingPosts)
            {
                UpperPost("UpperPostW", new Vector3(-front, GalleryFloor, z), postHeight, Vector3.forward, upper);
                UpperPost("UpperPostE", new Vector3(front, GalleryFloor, z), postHeight, Vector3.forward, upper);
            }
            foreach (float x in SouthPosts) UpperPost("UpperPostS", new Vector3(x, GalleryFloor, -6.12f), postHeight, Vector3.right, upper);
            foreach (float x in NorthPosts) UpperPost("UpperPostN", new Vector3(x, GalleryFloor, Plan.Screen), postHeight, Vector3.right, upper);
            float eaveBeamY = UpperEave - 0.12f;
            Beam("EaveBeamW", new Vector3(-front, eaveBeamY, Plan.HallFront - 0.3f), new Vector3(-front, eaveBeamY, Plan.RearFront), upper);
            Beam("EaveBeamE", new Vector3(front, eaveBeamY, Plan.HallFront - 0.3f), new Vector3(front, eaveBeamY, Plan.RearFront), upper);
            Beam("EaveBeamN", new Vector3(-front, eaveBeamY, Plan.Screen), new Vector3(front, eaveBeamY, Plan.Screen), upper);
            Beam("EaveBeamS", new Vector3(-front, eaveBeamY, -6.12f), new Vector3(front, eaveBeamY, -6.12f), upper);

            // The upper rooms' walls facing the galleries, a paper window to each bay, some lit.
            float wallHeight = UpperEave + 0.2f - GalleryFloor;
            UpperWall("UpperWallW", new Vector2(-Plan.WingFace, Plan.HallFront), new Vector2(-Plan.WingFace, Plan.RearFront), wallHeight, 1, upper);
            UpperWall("UpperWallE", new Vector2(Plan.WingFace, Plan.RearFront), new Vector2(Plan.WingFace, Plan.HallFront), wallHeight, 2, upper);
            UpperWall("UpperWallN", new Vector2(Plan.WingFace, Plan.RearFront), new Vector2(-Plan.WingFace, Plan.RearFront), wallHeight, 3, upper);
            UpperWall("UpperWallS", new Vector2(-Plan.WingFace, Plan.HallFront), new Vector2(Plan.WingFace, Plan.HallFront), wallHeight, 4, upper);

            // Lanterns along the galleries: small, unshadowed, high above the yard (they light the gallery, not the ground).
            foreach (var p in new[] { new Vector3(-front + 0.4f, UpperEave - 0.75f, -4.5f), new Vector3(-front + 0.4f, UpperEave - 0.75f, 3.7f),
                         new Vector3(front - 0.4f, UpperEave - 0.75f, -4.5f), new Vector3(front - 0.4f, UpperEave - 0.75f, 3.7f),
                         new Vector3(-4.6f, UpperEave - 0.75f, -6.5f), new Vector3(4.6f, UpperEave - 0.75f, -6.5f),
                         new Vector3(-4.8f, UpperEave - 0.75f, Plan.Screen + 0.4f), new Vector3(4.8f, UpperEave - 0.75f, Plan.Screen + 0.4f) })
                _refs.YardLanterns.Add(HouseLantern("GalleryLantern", p, upper, 3.2f, 1.4f));

            // Outer walls carried up from the ground-floor walls to the eaves.
            float outerBase = Floor + 3.0f;
            Wall("UpperOuterWest", new Vector2(Plan.West, Plan.South), new Vector2(Plan.West, Plan.North), outerBase, 6.0f - outerBase, Outer, PlasterDirty, upper);
            Wall("UpperOuterEast", new Vector2(Plan.East, Plan.North), new Vector2(Plan.East, Plan.South), outerBase, 6.0f - outerBase, Outer, PlasterDirty, upper);
            Wall("UpperOuterNorth", new Vector2(Plan.West, Plan.North), new Vector2(Plan.East, Plan.North), outerBase, 6.35f - outerBase, Outer, PlasterDirty, upper);
            Wall("UpperOuterSouth", new Vector2(Plan.East, Plan.South), new Vector2(Plan.West, Plan.South), outerBase, 5.85f - outerBase, Outer, PlasterDirty, upper);

            // The gallery's railings, posts and window lattices are fine work high up: they cast no sun shadow (each shadow
            // map would draw thousands of balusters for stripes nobody sees from below). Floors, walls, beams and roofs do.
            foreach (var renderer in upper.GetComponentsInChildren<Renderer>(true))
                for (var t = renderer.transform; t != upper; t = t.parent)
                    if (t.name.StartsWith("UpperRail") || t.name.StartsWith("UpperPost") || t.name.EndsWith("Window"))
                    {
                        renderer.shadowCastingMode = ShadowCastingMode.Off;
                        break;
                    }
        }

        /// <summary>A gallery floor: boards on joists seen from the veranda below, the walking boards on top, a fascia.</summary>
        private static void GalleryFloorSlab(string name, float x0, float z0, float x1, float z1, Transform parent)
        {
            CeilingPanel(name + "Ceiling", x0, z0, x1, z1, GalleryBeamTop, parent, Mathf.Abs(x1 - x0) < Mathf.Abs(z1 - z0));
            Box(name + "Boards", new Vector3(x0, GalleryBeamTop + 0.06f, z0), new Vector3(x1, GalleryFloor, z1), FloorBoards, parent, false);
        }

        /// <summary>A beam (section 0.22 × 0.24) between two points, with its ends cut back in a curve where they pass the post.</summary>
        private static void Beam(string name, Vector3 a, Vector3 b, Transform parent)
        {
            string key = $"Joinery_Beam_{Vector3.Distance(a, b):F2}_v1".Replace('.', 'p');
            if (!LauTuBaMeshKit.Has(key, out var mesh))
            {
                var j = new LauTuBaJoinery();
                float length = Vector3.Distance(a, b);
                j.Member(Vector3.zero, Vector3.forward * length, 0.22f, 0.24f, 0.02f, Vector3.up);
                // Beam ends (đầu xà): the lower edge cut away in a quarter curve past each end post.
                foreach (float z in new[] { 0f, length })
                {
                    var outline = new List<Vector2> { new Vector2(-0.18f, 0.12f), new Vector2(0.18f, 0.12f), new Vector2(0.18f, 0.02f) };
                    for (int i = 1; i <= 6; i++) { float t = i / 6f; outline.Add(new Vector2(0.18f - 0.36f * t, 0.02f - 0.12f * Mathf.Sin(t * Mathf.PI * 0.5f))); }
                    var place = Matrix4x4.TRS(new Vector3(0f, 0f, z), Quaternion.Euler(0f, -90f, 0f), Vector3.one);
                    j.Board(outline.ToArray(), 0.2f, place);
                }
                mesh = LauTuBaMeshKit.Keep(key, j.Build(key));
            }
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(a, Quaternion.LookRotation(b - a, Vector3.up));
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = Timber;
            go.isStatic = true;
        }

        /// <summary>A gallery post: lacquered shaft on a small block, with the capital and bracket arms of the pillars below.</summary>
        private static void UpperPost(string name, Vector3 foot, float height, Vector3 beamAlong, Transform parent)
        {
            var root = Frame(name, parent, foot, 0f);
            const float radius = 0.11f;
            Piece("Block", RoundedBox(new Vector3(0.26f, 0.06f, 0.26f), 0.01f), Timber, root, foot + Vector3.up * 0.03f, Quaternion.identity);
            var shaft = Lathe("ColumnShaft", new[] { new Vector2(1f, 0f), new Vector2(0.97f, 0.5f), new Vector2(0.92f, 1f) }, 20, capBottom: false);
            Piece("Shaft", shaft, PillarWood, root, foot + Vector3.up * 0.06f, Quaternion.identity, new Vector3(radius, height - 0.22f, radius));
            Piece("Capital", CapitalMesh(radius, height, beamAlong, Vector3.zero), PillarWood, root, foot, Quaternion.identity);
        }

        /// <summary>An upper room wall from <paramref name="a"/> to <paramref name="b"/>: plaster in timber bays, a paper window
        /// in most bays (every third one lit from within).</summary>
        private static void UpperWall(string name, Vector2 a, Vector2 b, float height, int seed, Transform parent)
        {
            float length = Vector2.Distance(a, b);
            int bays = Mathf.Max(1, Mathf.RoundToInt(length / 3f));
            float bay = length / bays;
            var openings = new List<Opening>();
            var windows = new List<(float at, bool lit)>();
            for (int i = 0; i < bays; i++)
            {
                if ((i + seed) % 4 == 3) continue; // a blank bay now and then, where a partition meets the wall
                float mid = bay * (i + 0.5f);
                openings.Add(Opening.Window(mid - 0.8f, mid + 0.8f, 0.75f, 1.95f));
                windows.Add((mid, (i + seed) % 3 == 0));
            }
            var wall = Wall(name, a, b, GalleryFloor, height, 0.2f, Plaster, parent, Timber, openings.ToArray());
            Vector2 dir = (b - a) / length;
            float yaw = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg + 90f;
            foreach (var (at, lit) in windows)
            {
                Vector2 p = a + dir * at;
                PaperWindow(name + "Window", new Vector3(p.x, GalleryFloor + 1.35f, p.y), 1.6f, 1.2f, yaw, lit, wall);
            }
            // Timber posts between the bays.
            for (int i = 1; i < bays; i++)
            {
                Vector2 p = a + dir * (bay * i);
                Post(wall, p, GalleryFloor, height, 0.28f, yaw - 90f);
            }
        }

        // ------------------------------------------------------------------ tile roofs

        /// <summary>
        /// A tile roof with its ridge at <paramref name="ridge"/> (top of the tiles at the ridge line), <paramref name="length"/>
        /// along the ridge, <paramref name="half"/> horizontally to each eave, at <paramref name="pitch"/> degrees.
        /// <paramref name="gables"/>: close the ends with gable walls down to <paramref name="gableBase"/>.
        /// </summary>
        private static void TileRoof(string name, Vector3 ridge, float length, float half, float pitch, bool alongX, Transform parent, bool gables = false, float gableBase = 0f)
        {
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.SetPositionAndRotation(ridge, alongX ? Quaternion.identity : Quaternion.Euler(0f, 90f, 0f));
            var visual = new GameObject("Tiles");
            visual.transform.SetParent(root, false);
            visual.AddComponent<MeshFilter>().sharedMesh = TileRoofMesh(length, half, pitch);
            visual.AddComponent<MeshRenderer>().sharedMaterials = new[] { UvTwin(Tiles), UvTwin(Ceiling), UvTwin(Timber), UvTwin(_terracotta), Ridge };
            visual.isStatic = true;
            // Invisible slopes for the light and sight checks (what the old box roofs did).
            float rad = pitch * Mathf.Deg2Rad, slab = half / Mathf.Cos(rad);
            for (int side = -1; side <= 1; side += 2)
            {
                var slope = new GameObject("Slope");
                slope.transform.SetParent(root, false);
                slope.layer = EnvironmentLayer;
                slope.isStatic = true;
                slope.transform.localPosition = new Vector3(0f, -Mathf.Sin(rad) * slab * 0.5f - 0.1f, side * half * 0.5f);
                slope.transform.localRotation = Quaternion.Euler(side * pitch, 0f, 0f);
                slope.AddComponent<BoxCollider>().size = new Vector3(length, 0.16f, slab);
            }
            if (!gables) return;
            // Gable walls under the roof's ends, set in under the barge boards.
            foreach (float end in new[] { -1f, 1f })
            {
                float baseY = gableBase - ridge.y;
                float edge = -(half - 0.35f) * Mathf.Tan(rad) - 0.25f;
                var outline = new[] { new Vector2(-half + 0.35f, baseY), new Vector2(half - 0.35f, baseY), new Vector2(half - 0.35f, edge), new Vector2(0f, -0.25f), new Vector2(-half + 0.35f, edge) };
                var gable = new GameObject("Gable");
                gable.transform.SetParent(root, false);
                gable.transform.localPosition = new Vector3(end * (length * 0.5f - 0.4f), 0f, 0f);
                gable.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                var j = new LauTuBaJoinery();
                j.Board(outline, 0.25f, Matrix4x4.identity);
                gable.AddComponent<MeshFilter>().sharedMesh = LauTuBaMeshKit.Keep($"Joinery_Gable_{half:F2}_{pitch:F0}_{baseY:F2}_v1".Replace('.', 'p'), j.Build("Gable"));
                gable.AddComponent<MeshRenderer>().sharedMaterial = UvTwin(PlasterDirty);
                gable.isStatic = true;
            }
        }

        private static Mesh TileRoofMesh(float length, float half, float pitch)
        {
            string key = $"Joinery_TileRoof_{length:F2}_{half:F2}_{pitch:F0}_v1".Replace('.', 'p');
            if (LauTuBaMeshKit.Has(key, out var cached)) return cached;
            var j = new LauTuBaJoinery();
            float rad = pitch * Mathf.Deg2Rad, tan = Mathf.Tan(rad), drop = half * tan, slant = half / Mathf.Cos(rad);
            const float channel = 0.175f, relief = 0.024f, thickness = 0.2f;
            float hl = length * 0.5f;
            int columns = Mathf.CeilToInt(length / (channel / 4f));
            for (int s = -1; s <= 1; s += 2)
            {
                int side = s;
                // Tiles: cover tiles over pans, a ridge of relief every 17.5 cm along the eave, UVs in metres.
                j.Grid(columns, 1, (u, v) =>
                    {
                        float x = -hl + u * length;
                        return new Vector3(x, -v * drop + relief * Mathf.Cos(x / channel * Mathf.PI * 2f), side * v * half);
                    },
                    (u, v) =>
                    {
                        float x = -hl + u * length;
                        float slope = -relief * Mathf.PI * 2f / channel * Mathf.Sin(x / channel * Mathf.PI * 2f);
                        return new Vector3(-slope, 1f, side * tan).normalized;
                    },
                    (u, v) => new Vector2(-hl + u * length, v * slant), 0, side < 0);
                // Boards under the rafters.
                j.Grid(1, 1, (u, v) => new Vector3(-hl + u * length, -v * drop - thickness, side * v * half), (u, v) => new Vector3(0f, -1f, -side * tan).normalized,
                    (u, v) => new Vector2(-hl + u * length, v * slant), 1, side > 0);
                // Rafters every 60 cm.
                for (float x = -hl + 0.3f; x < hl; x += 0.6f)
                    j.Member(new Vector3(x, -thickness - 0.05f, 0f), new Vector3(x, -drop - thickness - 0.05f, side * half), 0.07f, 0.09f, 0.01f, Vector3.up, 2);
                // Fascia board and the row of round tile ends along the eave, the eave corners lifting (đầu đao).
                j.Member(new Vector3(-hl, -drop - thickness * 0.5f, side * half), new Vector3(hl, -drop - thickness * 0.5f, side * half), 0.05f, thickness + 0.06f, 0.01f, Vector3.up, 2);
                var outward = new Vector3(0f, -Mathf.Sin(rad), side * Mathf.Cos(rad));
                var face = Quaternion.FromToRotation(Vector3.up, outward);
                var disc = new[] { new Vector2(0f, 0f), new Vector2(0.07f, 0f), new Vector2(0.075f, 0.02f), new Vector2(0.075f, 0.05f), new Vector2(0.06f, 0.06f), new Vector2(0f, 0.06f) };
                for (float x = channel * Mathf.Ceil(-hl / channel); x <= hl; x += channel)
                    j.Turned(new Vector3(x, -drop + relief, side * half) - outward * 0.04f, disc, 8, 3, face);
                foreach (float e in new[] { -1f, 1f })
                    j.Sweep(LauTuBaJoinery.Smooth(new[] { new Vector3(e * (hl - 0.9f), -drop + 0.02f, side * half), new Vector3(e * (hl - 0.2f), -drop + 0.08f, side * (half + 0.05f)),
                        new Vector3(e * (hl + 0.15f), -drop + 0.3f, side * (half + 0.18f)), new Vector3(e * (hl + 0.25f), -drop + 0.55f, side * (half + 0.24f)) }, 6),
                        LauTuBaJoinery.Round(0.13f, 0.1f, 10), Vector3.up, 4, t => Mathf.Lerp(1f, 0.55f, t));
                // Barge boards down both gable edges.
                foreach (float e in new[] { -1f, 1f })
                    j.Member(new Vector3(e * hl, 0.04f, 0f), new Vector3(e * hl, -drop - 0.02f, side * half), 0.06f, 0.26f, 0.01f, Vector3.up, 2);
            }
            // Ridge: a heavy rounded ridge with a bead, its ends curling up and back.
            j.Sweep(new[] { new Vector3(-hl - 0.1f, 0.08f, 0f), new Vector3(hl + 0.1f, 0.08f, 0f) }, LauTuBaJoinery.EasedRect(0.34f, 0.26f, 0.09f, 3), Vector3.up, 4);
            j.Sweep(new[] { new Vector3(-hl, 0.23f, 0f), new Vector3(hl, 0.23f, 0f) }, LauTuBaJoinery.Round(0.1f, 0.08f, 10), Vector3.up, 4);
            foreach (float e in new[] { -1f, 1f })
                j.Sweep(LauTuBaJoinery.Smooth(new[] { new Vector3(e * (hl + 0.05f), 0.12f, 0f), new Vector3(e * (hl + 0.3f), 0.2f, 0f), new Vector3(e * (hl + 0.5f), 0.48f, 0f),
                    new Vector3(e * (hl + 0.44f), 0.74f, 0f), new Vector3(e * (hl + 0.28f), 0.82f, 0f) }, 6), LauTuBaJoinery.Round(0.3f, 0.22f, 12), Vector3.forward, 4, t => Mathf.Lerp(1f, 0.45f, t));
            return LauTuBaMeshKit.Keep(key, j.Build(key));
        }
    }
}
