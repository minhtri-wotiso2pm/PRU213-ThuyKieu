using System.Collections.Generic;
using System.Linq;
using ThuyKieu.Environment;
using ThuyKieu.Gameplay.Chase;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static ThuyKieu.Core.Editor.StorySceneKit;

namespace ThuyKieu.Core.Editor
{
    public static partial class Chapter05Forest
    {
        /// <summary>
        /// The course, fixed and learnable (metres along the moonlit way; at 5 m/s about two minutes from the start mark):
        /// 0–14 waste ground behind her, a 30 m run-up · 44 a fallen culm (jump) · 68 low culms (duck) · then sidesteps round
        /// rocks and broken culms, a culm cracking and falling across, a jump · 190 the fork: left the moonlit way with a
        /// sidestep, a ditch, a jump and a duck; right the dark thick way with a sidestep, two ducks and a jump; they meet
        /// again · then the squeeze, pairs of moves a stride or two apart, the second falling culm and ditch, the downhill
        /// switchback, the long straight sprint with a third falling culm, the plank bridge, the far stake at the corner, and
        /// the climb to the temple gate. Every mark is listed in <see cref="Make"/>.
        /// </summary>
        internal sealed class Layout
        {
            public struct Mark
            {
                public string Id;
                public ChaseCourse.ObstacleKind Kind;
                public int Way;      // 0 both, 1 moon only, 2 thick only
                public float S;      // along its own way (way 2 for thick-only marks, else the moon way)
                public float Length;
                /// <summary>Which side it takes (sidesteps, falling culms): +1 the right of the way, −1 the left.</summary>
                public int Side;
            }

            public readonly List<Vector3> Moon = new List<Vector3>();
            public readonly List<float> MoonHalf = new List<float>();
            public readonly List<Vector3> Thick = new List<Vector3>();
            public readonly List<float> ThickHalf = new List<float>();
            public readonly List<Mark> Marks = new List<Mark>();
            public float StartAt = 14f, ForkAt, MergeAt, ThickForkFrom, ThickForkTo, BridgeNear, BridgeFar, Corner, End;
            public Vector3 TempleGate;
            public Vector3 TempleForward;
            private float[] _moonS, _thickS;

            public static Layout Make()
            {
                var l = new Layout();
                Vector3 p = new Vector3(93f, 0f, 6f);
                float yaw = 90f;
                l.Moon.Add(p);
                l.MoonHalf.Add(1.7f);
                void Leg(float length, float turn, float endY, float half, bool both)
                {
                    float y0 = p.y, h0 = l.MoonHalf[l.MoonHalf.Count - 1];
                    int steps = Mathf.RoundToInt(length);
                    for (int i = 1; i <= steps; i++)
                    {
                        float k = i / (float)steps;
                        yaw += turn / steps;
                        p += new Vector3(Mathf.Sin(yaw * Mathf.Deg2Rad), 0f, Mathf.Cos(yaw * Mathf.Deg2Rad));
                        p.y = Mathf.Lerp(y0, endY, Mathf.SmoothStep(0f, 1f, k));
                        float h = Mathf.Lerp(h0, half, Mathf.Clamp01(k * 4f));
                        l.Moon.Add(p); l.MoonHalf.Add(h);
                        if (both) { l.Thick.Add(p); l.ThickHalf.Add(h); }
                    }
                }
                Leg(40f, 0f, -2.4f, 1.7f, false);
                Leg(30f, -25f, -2.7f, 1.5f, false);
                Leg(40f, 40f, -3.1f, 1.5f, false);
                Leg(50f, -50f, -3.0f, 1.5f, false);
                Leg(30f, 20f, -3.3f, 1.5f, false);
                l.Thick.AddRange(l.Moon);
                l.ThickHalf.AddRange(l.MoonHalf);
                l.ForkAt = Length(l.Moon);

                // The fork: both ways leave the same point and meet at the same point; the moonlit way bows left, wider and
                // brighter, the thick way bows right, narrow and dark.
                Vector3 f = p, df = Dir(yaw), m = f + Dir(yaw + 12f) * 80f, dm = Dir(yaw + 25f);
                m.y = -3.6f;
                const int n = 90;
                for (int i = 1; i <= n; i++)
                {
                    float t = i / (float)n;
                    Vector3 a = Hermite(f, m, df * 80f, dm * 80f, t), b = Hermite(f, m, df * 80f, dm * 80f, t - 0.001f);
                    Vector3 d = a - b;
                    d.y = 0f;
                    d.Normalize();
                    var right = new Vector3(d.z, 0f, -d.x);
                    float bulge = Mathf.Sin(Mathf.PI * t), y = Mathf.Lerp(f.y, m.y, t);
                    Vector3 moon = a - right * 9f * bulge, thick = a + right * 4.5f * bulge;
                    moon.y = thick.y = y;
                    l.Moon.Add(moon); l.MoonHalf.Add(Mathf.Lerp(1.5f, 1.8f, Mathf.Min(1f, bulge * 3f)));
                    l.Thick.Add(thick); l.ThickHalf.Add(Mathf.Lerp(1.5f, 1.1f, Mathf.Min(1f, bulge * 3f)));
                }
                l.MergeAt = Length(l.Moon);
                l.ThickForkFrom = l.ForkAt + 10f;
                l.ThickForkTo = Length(l.Thick) - 10f;
                p = m;
                yaw += 25f;

                Leg(35f, -20f, -3.6f, 1.5f, true);
                Leg(35f, 35f, -3.9f, 1.5f, true);
                Leg(40f, -30f, -4.1f, 1.5f, true);
                Leg(30f, 0f, -4.3f, 1.5f, true);
                Leg(20f, 0f, -4.4f, 1.5f, true);
                Leg(40f, 110f, -5.6f, 1.6f, true);
                Leg(60f, 0f, -5.8f, 1.6f, true);
                l.BridgeNear = Length(l.Moon);
                Leg(13f, 0f, -5.8f, 1.2f, true);
                l.BridgeFar = Length(l.Moon);
                Leg(4f, 0f, -5.8f, 1.3f, true);
                l.Corner = Length(l.Moon) + 2f;
                Leg(7f, -80f, -5.6f, 1.5f, true);
                Leg(36f, 8f, -2.6f, 1.6f, true);
                l.End = Length(l.Moon);
                Leg(9f, 0f, -2.6f, 3.6f, true); // the court before the temple gate
                l.TempleForward = Dir(yaw);
                l.Measure();
                l.TempleGate = l.MoonAt(l.End + 6f);

                float g1 = l.ForkAt + (l.MergeAt - l.ForkAt) * 0.5f;
                float thickFork = l.ThickLength(l.MergeAt) - l.ForkAt;
                // The order of a run (about 115 s at 5 m/s from the start mark at 14 m), each way 25 obstacles plus the choice
                // of way and the stake: taught one at a time, then sidesteps and a falling culm, then both ways each with their
                // own rhythm, then pairs a stride or two apart with short clear stretches between, then the sprint, and the
                // last 20 m before the bridge left clear to read the planks and the stake. Nothing is more than ~6 s from the
                // next decision except where the way itself asks to be read (the fork, the bridge).
                const ChaseCourse.ObstacleKind Jump = ChaseCourse.ObstacleKind.Jump, Duck = ChaseCourse.ObstacleKind.Duck, Gap = ChaseCourse.ObstacleKind.Gap,
                    Dodge = ChaseCourse.ObstacleKind.Dodge, Hazard = ChaseCourse.ObstacleKind.Hazard;
                // Teach: a jump alone, a duck alone (a 6 s run-up first).
                l.Add("J1", Jump, 0, 44f, 0.45f);
                l.Add("D1", Duck, 0, 68f, 2.4f);
                // Sidestep left of a rock, a culm cracking ahead, a jump, a sidestep the other way.
                l.Add("K1", Dodge, 0, 92f, 2.6f, -1);
                l.Add("H1", Hazard, 0, 118f, 0.5f, 1);
                l.Add("J6", Jump, 0, 142f, 0.45f);
                l.Add("K2", Dodge, 0, 162f, 2.6f, 1);
                // The moonlit way: a sidestep, the ditch, a jump, a low bough before the ways meet.
                float moonFork = l.MergeAt - l.ForkAt;
                l.Add("K3", Dodge, 1, l.ForkAt + 20f, 2.6f, -1);
                l.Add("G1", Gap, 1, g1, 1.6f);
                l.Add("J7", Jump, 1, l.ForkAt + moonFork * 0.7f, 0.45f);
                l.Add("D5", Duck, 1, l.ForkAt + moonFork * 0.86f, 2.4f);
                // The thick way: narrow, dark, low boughs and a log.
                l.Add("K4", Dodge, 2, l.ForkAt + thickFork * 0.15f, 2.6f, 1);
                l.Add("D2", Duck, 2, l.ForkAt + thickFork * 0.32f, 2.4f);
                l.Add("J2", Jump, 2, l.ForkAt + thickFork * 0.56f, 0.45f);
                l.Add("D3", Duck, 2, l.ForkAt + thickFork * 0.8f, 2.4f);
                // Together again: the squeeze, a jump, a duck, a sidestep straight into a jump, a falling culm, the second
                // ditch, a jump straight into a duck, a sidestep.
                l.Add("Q1", ChaseCourse.ObstacleKind.Squeeze, 0, l.MergeAt + 10f, 3f);
                l.Add("J3", Jump, 0, l.MergeAt + 26f, 0.45f);
                l.Add("D4", Duck, 0, l.MergeAt + 44f, 2.4f);
                l.Add("K5", Dodge, 0, l.MergeAt + 62f, 2.6f, -1);
                l.Add("J8", Jump, 0, l.MergeAt + 72f, 0.45f);
                l.Add("H2", Hazard, 0, l.MergeAt + 94f, 0.5f, -1);
                l.Add("G2", Gap, 0, l.MergeAt + 116f, 1.7f);
                l.Add("J4", Jump, 0, l.MergeAt + 132f, 0.45f);
                l.Add("D6", Duck, 0, l.MergeAt + 142f, 2.4f);
                l.Add("K6", Dodge, 0, l.MergeAt + 157f, 2.6f, 1);
                // The switchback and the long straight: the sprint, everything closer, then clear before the bridge.
                l.Add("J5", Jump, 0, l.MergeAt + 180f, 0.45f);
                l.Add("D7", Duck, 0, l.BridgeNear - 54f, 2.4f);
                l.Add("K7", Dodge, 0, l.BridgeNear - 44f, 2.6f, -1);
                l.Add("H3", Hazard, 0, l.BridgeNear - 30f, 0.5f, 1);
                l.Add("J9", Jump, 0, l.BridgeNear - 20f, 0.45f);
                l.Add("B1", ChaseCourse.ObstacleKind.Bridge, 0, (l.BridgeNear + l.BridgeFar) * 0.5f, l.BridgeFar - l.BridgeNear);
                l.Check();
                return l;
            }

            private void Add(string id, ChaseCourse.ObstacleKind kind, int way, float s, float length, int side = 1)
                => Marks.Add(new Mark { Id = id, Kind = kind, Way = way, S = s, Length = length, Side = side });

            public Mark Get(string id) => Marks.First(mark => mark.Id == id);

            /// <summary>Distance along the moonlit way of a mark (a thick-way mark is placed on the thick way).</summary>
            public float Of(string id) => Get(id).S;

            public Vector3 At(int way, float s) => Sample(way == 2 ? Thick : Moon, way == 2 ? _thickS : _moonS, s);
            public Vector3 MoonAt(float s) => At(1, s);
            public float HalfAt(int way, float s) => SampleHalf(way == 2 ? ThickHalf : MoonHalf, way == 2 ? _thickS : _moonS, s);

            public Vector3 DirAt(int way, float s)
            {
                Vector3 d = At(way, s + 0.8f) - At(way, s - 0.8f);
                d.y = 0f;
                return d.normalized;
            }

            public Vector3 RightAt(float s) { var d = DirAt(1, s); return new Vector3(d.z, 0f, -d.x); }
            public Vector3 RightAt(int way, float s) { var d = DirAt(way, s); return new Vector3(d.z, 0f, -d.x); }

            /// <summary>Thick-way distance of the point where the moon way is at <paramref name="moonS"/> (the shared parts).</summary>
            public float ThickLength(float moonS) => Project(2, MoonAt(moonS));

            private void Measure() { _moonS = Cumulative(Moon); _thickS = Cumulative(Thick); }

            public float Project(int way, Vector3 point)
            {
                var points = way == 2 ? Thick : Moon;
                var s = way == 2 ? _thickS : _moonS;
                float best = float.MaxValue, at = 0f;
                for (int i = 1; i < points.Count; i++)
                {
                    Vector3 a = points[i - 1], ab = points[i] - a;
                    ab.y = 0f;
                    Vector3 ap = point - a;
                    ap.y = 0f;
                    float k = Mathf.Clamp01(Vector3.Dot(ap, ab) / Mathf.Max(0.0001f, ab.sqrMagnitude));
                    float d = (ap - ab * k).sqrMagnitude;
                    if (d < best) { best = d; at = Mathf.Lerp(s[i - 1], s[i], k); }
                }
                return at;
            }

            /// <summary>No part of the way may come back near another (the banks and bamboo between them must fit).</summary>
            private void Check()
            {
                for (int i = 0; i < Moon.Count; i += 2)
                    for (int j = i; j < Moon.Count; j += 2)
                    {
                        if (_moonS[j] - _moonS[i] < 70f) continue;
                        if (Vector2.Distance(new Vector2(Moon[i].x, Moon[i].z), new Vector2(Moon[j].x, Moon[j].z)) < 30f)
                            throw new System.InvalidOperationException($"Chapter 5 course comes back on itself at {_moonS[i]:F0} m / {_moonS[j]:F0} m");
                    }
            }

            private static Vector3 Dir(float yaw) => new Vector3(Mathf.Sin(yaw * Mathf.Deg2Rad), 0f, Mathf.Cos(yaw * Mathf.Deg2Rad));

            private static Vector3 Hermite(Vector3 p0, Vector3 p1, Vector3 m0, Vector3 m1, float t)
            {
                float t2 = t * t, t3 = t2 * t;
                return (2f * t3 - 3f * t2 + 1f) * p0 + (t3 - 2f * t2 + t) * m0 + (-2f * t3 + 3f * t2) * p1 + (t3 - t2) * m1;
            }

            private static float Length(List<Vector3> points)
            {
                float length = 0f;
                for (int i = 1; i < points.Count; i++) length += Vector3.Distance(points[i - 1], points[i]);
                return length;
            }

            private static float[] Cumulative(List<Vector3> points)
            {
                var s = new float[points.Count];
                for (int i = 1; i < points.Count; i++) s[i] = s[i - 1] + Vector3.Distance(points[i - 1], points[i]);
                return s;
            }

            private static Vector3 Sample(List<Vector3> points, float[] s, float at)
            {
                if (at <= 0f) return points[0];
                for (int i = 1; i < points.Count; i++)
                    if (at <= s[i]) return Vector3.Lerp(points[i - 1], points[i], Mathf.InverseLerp(s[i - 1], s[i], at));
                return points[points.Count - 1];
            }

            private static float SampleHalf(List<float> halves, float[] s, float at)
            {
                for (int i = 1; i < halves.Count; i++)
                    if (at <= s[i]) return Mathf.Lerp(halves[i - 1], halves[i], Mathf.InverseLerp(s[i - 1], s[i], at));
                return halves[halves.Count - 1];
            }
        }

        // ------------------------------------------------------------------ ground

        private static ForestGround Ground(Transform root, Layout layout, NavMeshSurface nav)
        {
            var go = new GameObject("Ground");
            go.transform.SetParent(root, false);
            go.SetActive(false); // filled in before it first builds
            var ground = go.AddComponent<ForestGround>();
            var so = new SerializedObject(ground);
            var ways = so.FindProperty("_ways");
            ways.arraySize = 2;
            WriteWay(ways.GetArrayElementAtIndex(0), layout.Moon, layout.MoonHalf, 0f, 0f);
            // The thick way is its own corridor only from the fork to the merge (elsewhere it lies on the moonlit way).
            WriteWay(ways.GetArrayElementAtIndex(1), layout.Thick, layout.ThickHalf, layout.ThickForkFrom - 12f, layout.ThickForkTo + 12f);
            var cuts = new List<(Vector3 centre, Vector3 forward, float length, float across, float depth, float wall)>();
            foreach (var mark in layout.Marks.Where(m => m.Kind == ChaseCourse.ObstacleKind.Gap))
            {
                int way = mark.Way == 2 ? 2 : 1;
                cuts.Add((layout.At(way, mark.S), layout.DirAt(way, mark.S), mark.Length, 9f, 2.4f, 0.2f));
            }
            float mid = (layout.BridgeNear + layout.BridgeFar) * 0.5f;
            cuts.Add((layout.MoonAt(mid), layout.DirAt(1, mid), layout.BridgeFar - layout.BridgeNear - 2f, 60f, 9f, 1.6f));
            // What she could climb onto by the way (out to a jump from it), so the hedge holds her from up there too: logs, the sidestep rocks and
            // their broken culms, the squeeze, the fork's boulder, and the falling culms, which once down lie across the
            // way from bank to bank.
            var holds = new List<(Vector3 centre, float radius, float top)>();
            foreach (var mark in layout.Marks)
            {
                int way = mark.Way == 2 ? 2 : 1;
                float half = layout.HalfAt(way, mark.S);
                switch (mark.Kind)
                {
                    case ChaseCourse.ObstacleKind.Jump: holds.Add((layout.At(way, mark.S), half + 4.5f, 0.5f)); break;
                    case ChaseCourse.ObstacleKind.Dodge: holds.Add((layout.At(way, mark.S), mark.Length * 0.5f + half + 5f, 1.9f)); break;
                    case ChaseCourse.ObstacleKind.Squeeze: holds.Add((layout.MoonAt(mark.S), 7f, 1.4f)); break;
                    case ChaseCourse.ObstacleKind.Hazard: holds.Add((HazardCrossing(layout, mark), 8f, 3.5f)); break;
                }
            }
            holds.Add(((layout.MoonAt(layout.ForkAt + 14f) + layout.At(2, layout.ForkAt + 14f)) * 0.5f, 6f, 1f));
            var holdArray = so.FindProperty("_footholds");
            holdArray.arraySize = holds.Count;
            for (int i = 0; i < holds.Count; i++)
            {
                var h = holdArray.GetArrayElementAtIndex(i);
                h.FindPropertyRelative("Centre").vector3Value = holds[i].centre;
                h.FindPropertyRelative("Radius").floatValue = holds[i].radius;
                h.FindPropertyRelative("Top").floatValue = holds[i].top;
            }
            var cutArray = so.FindProperty("_cuts");
            cutArray.arraySize = cuts.Count;
            for (int i = 0; i < cuts.Count; i++)
            {
                var c = cutArray.GetArrayElementAtIndex(i);
                c.FindPropertyRelative("Centre").vector3Value = cuts[i].centre;
                c.FindPropertyRelative("Forward").vector3Value = cuts[i].forward;
                c.FindPropertyRelative("Length").floatValue = cuts[i].length;
                c.FindPropertyRelative("Across").floatValue = cuts[i].across;
                c.FindPropertyRelative("Depth").floatValue = cuts[i].depth;
                c.FindPropertyRelative("Wall").floatValue = cuts[i].wall;
            }
            so.FindProperty("_floor").objectReferenceValue = FloorMaterial();
            so.FindProperty("_navMesh").objectReferenceValue = nav;
            so.ApplyModifiedPropertiesWithoutUndo();
            go.SetActive(true);
            ground.Invalidate();
            ground.Build();
            Physics.SyncTransforms();
            return ground;
        }

        private static void WriteWay(SerializedProperty way, List<Vector3> points, List<float> half, float corridorFrom, float corridorTo)
        {
            var p = way.FindPropertyRelative("Points");
            var h = way.FindPropertyRelative("Half");
            p.arraySize = points.Count;
            h.arraySize = half.Count;
            for (int i = 0; i < points.Count; i++) { p.GetArrayElementAtIndex(i).vector3Value = points[i]; h.GetArrayElementAtIndex(i).floatValue = half[i]; }
            way.FindPropertyRelative("CorridorFrom").floatValue = corridorFrom;
            way.FindPropertyRelative("CorridorTo").floatValue = corridorTo;
        }

        // ------------------------------------------------------------------ the course

        private static ChaseCourse Course(Transform root, Layout layout, Refs refs)
        {
            var go = new GameObject("Course");
            go.transform.SetParent(root, false);
            var course = go.AddComponent<ChaseCourse>();
            var so = new SerializedObject(course);
            SetPoints(so.FindProperty("_moonWay"), layout.Moon);
            SetPoints(so.FindProperty("_thickWay"), layout.Thick);
            var obstacles = so.FindProperty("_obstacles");
            obstacles.arraySize = layout.Marks.Count;
            for (int i = 0; i < layout.Marks.Count; i++)
            {
                var mark = layout.Marks[i];
                int way = mark.Way == 2 ? 2 : 1;
                Vector3 at = mark.Kind == ChaseCourse.ObstacleKind.Hazard ? HazardCrossing(layout, mark)
                    : mark.Kind == ChaseCourse.ObstacleKind.Dodge ? DodgeCentre(layout, mark)
                    : layout.At(way, mark.S);
                var o = obstacles.GetArrayElementAtIndex(i);
                o.FindPropertyRelative("Kind").enumValueIndex = (int)mark.Kind;
                o.FindPropertyRelative("Position").vector3Value = at;
                o.FindPropertyRelative("Forward").vector3Value = layout.DirAt(way, mark.S);
                o.FindPropertyRelative("Length").floatValue = mark.Length;
                o.FindPropertyRelative("Way").intValue = mark.Way;
            }
            so.FindProperty("_start").objectReferenceValue = Waypoint("Start", layout, layout.StartAt, go.transform);
            so.FindProperty("_pursuerStart").objectReferenceValue = Waypoint("PursuerStart", layout, layout.StartAt - 12f, go.transform);
            so.FindProperty("_companionStart").objectReferenceValue = Waypoint("CompanionStart", layout, layout.StartAt + 4f, go.transform);
            so.FindProperty("_finish").objectReferenceValue = Zone("Finish", layout.MoonAt(layout.End - 6f), layout.DirAt(1, layout.End - 6f), new Vector3(4f, 3f, 10f), go.transform);
            var falls = new List<Object>();
            foreach (var mark in layout.Marks.Where(m => m.Kind == ChaseCourse.ObstacleKind.Gap))
            {
                int way = mark.Way == 2 ? 2 : 1;
                Vector3 at = layout.At(way, mark.S) + Vector3.down * 2.6f;
                falls.Add(Zone("Fall_" + mark.Id, at, layout.DirAt(way, mark.S), new Vector3(16f, 1.8f, mark.Length + 0.4f), go.transform));
            }
            float mid = (layout.BridgeNear + layout.BridgeFar) * 0.5f;
            falls.Add(Zone("Fall_Ravine", layout.MoonAt(mid) + Vector3.down * 11f, layout.DirAt(1, mid), new Vector3(60f, 9f, layout.BridgeFar - layout.BridgeNear - 1f), go.transform));
            var fallArray = so.FindProperty("_falls");
            fallArray.arraySize = falls.Count;
            for (int i = 0; i < falls.Count; i++) fallArray.GetArrayElementAtIndex(i).objectReferenceValue = falls[i];
            float thickMid = (layout.ThickForkFrom + layout.ThickForkTo) * 0.5f;
            so.FindProperty("_thickZone").objectReferenceValue = Zone("ThickWay", layout.At(2, thickMid), layout.DirAt(2, thickMid), new Vector3(3.5f, 3f, 24f), go.transform);
            so.FindProperty("_fallBelow").floatValue = -16f;
            so.ApplyModifiedPropertiesWithoutUndo();
            _ = refs;
            return course;
        }

        private static void SetPoints(SerializedProperty property, List<Vector3> points)
        {
            property.arraySize = points.Count;
            for (int i = 0; i < points.Count; i++) property.GetArrayElementAtIndex(i).vector3Value = points[i];
        }

        private static Transform Waypoint(string name, Layout layout, float s, Transform parent)
        {
            var point = Point(name, layout.MoonAt(s) + Vector3.up * 0.06f, 0f, parent);
            point.rotation = Quaternion.LookRotation(layout.DirAt(1, s));
            return point;
        }

        /// <summary>An oriented trigger box (size: across, up, along the way); on Ignore Raycast so nothing bakes or aims at it.</summary>
        private static BoxCollider Zone(string name, Vector3 centre, Vector3 forward, Vector3 size, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(centre + Vector3.up * size.y * 0.5f, Quaternion.LookRotation(forward));
            go.layer = 2;
            var box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = size;
            return box;
        }
    }
}
