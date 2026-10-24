using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Builds one mesh out of joinery parts, the way a carpenter makes them: members swept along a (possibly curved)
    /// path with an eased-edge section, boards cut from an outline (aprons with arched or cloud-cut edges), turned parts.
    /// Every part is UV-mapped in metres with U along its grain, and its grain direction is stored in UV3 (object
    /// space), so wood textures follow each member whether drawn with URP Lit (UV0) or WorldTriplanarLit (UV3).
    /// Parts go to submeshes (0 frame wood, 1 panels/lacquer, 2 metal or cloth), one renderer per piece of furniture.
    /// </summary>
    public sealed class LauTuBaJoinery
    {
        private readonly List<Vector3> _vertices = new List<Vector3>();
        private readonly List<Vector3> _normals = new List<Vector3>();
        private readonly List<Vector2> _uvs = new List<Vector2>();
        private readonly List<Vector3> _grain = new List<Vector3>();
        private readonly List<List<int>> _triangles = new List<List<int>>();

        private List<int> Submesh(int index)
        {
            while (_triangles.Count <= index) _triangles.Add(new List<int>());
            return _triangles[index];
        }

        private int Vertex(Vector3 position, Vector3 normal, Vector2 uv, Vector3 grain)
        {
            _vertices.Add(position);
            _normals.Add(normal);
            _uvs.Add(uv);
            _grain.Add(grain);
            return _vertices.Count - 1;
        }

        // ------------------------------------------------------------------ sections

        /// <summary>A rectangle <paramref name="width"/> × <paramref name="height"/> with eased corners (closed, CCW).</summary>
        public static Vector2[] EasedRect(float width, float height, float ease, int steps = 3)
        {
            ease = Mathf.Min(ease, Mathf.Min(width, height) * 0.49f);
            var points = new List<Vector2>();
            var corners = new[] { new Vector2(1f, 1f), new Vector2(-1f, 1f), new Vector2(-1f, -1f), new Vector2(1f, -1f) };
            for (int c = 0; c < 4; c++)
            {
                var centre = new Vector2(corners[c].x * (width * 0.5f - ease), corners[c].y * (height * 0.5f - ease));
                float start = c * 90f;
                for (int s = 0; s <= steps; s++)
                {
                    float angle = (start + 90f * s / steps) * Mathf.Deg2Rad;
                    points.Add(centre + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * ease);
                }
            }
            return points.ToArray();
        }

        /// <summary>An ellipse-ish round section (rails, rods, crest ends).</summary>
        public static Vector2[] Round(float width, float height, int sides = 12)
        {
            var points = new Vector2[sides];
            for (int i = 0; i < sides; i++)
            {
                float angle = i * Mathf.PI * 2f / sides;
                points[i] = new Vector2(Mathf.Cos(angle) * width * 0.5f, Mathf.Sin(angle) * height * 0.5f);
            }
            return points;
        }

        // ------------------------------------------------------------------ paths

        /// <summary>A smooth path through <paramref name="points"/> (Catmull-Rom), <paramref name="perSpan"/> samples per span.</summary>
        public static List<Vector3> Smooth(IList<Vector3> points, int perSpan = 6)
        {
            var path = new List<Vector3>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector3 p0 = points[Mathf.Max(0, i - 1)], p1 = points[i], p2 = points[i + 1], p3 = points[Mathf.Min(points.Count - 1, i + 2)];
                for (int s = 0; s < perSpan; s++)
                {
                    float t = (float)s / perSpan, t2 = t * t, t3 = t2 * t;
                    path.Add(0.5f * (2f * p1 + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (-p0 + 3f * p1 - 3f * p2 + p3) * t3));
                }
            }
            path.Add(points[points.Count - 1]);
            return path;
        }

        // ------------------------------------------------------------------ parts

        /// <summary>
        /// A member swept along <paramref name="path"/> with <paramref name="section"/> (x across, y "up" relative to
        /// <paramref name="upHint"/>), scaled along the way by <paramref name="scale"/> (taper; null = 1). Capped ends.
        /// </summary>
        public LauTuBaJoinery Sweep(IList<Vector3> path, Vector2[] section, Vector3 upHint, int submesh = 0, System.Func<float, float> scale = null, bool caps = true)
        {
            int rings = path.Count, sides = section.Length;
            // Arc length along the path (UV u) and along the section outline (UV v).
            var along = new float[rings];
            for (int i = 1; i < rings; i++) along[i] = along[i - 1] + Vector3.Distance(path[i - 1], path[i]);
            float total = Mathf.Max(0.0001f, along[rings - 1]);
            var around = new float[sides + 1];
            for (int j = 1; j <= sides; j++) around[j] = around[j - 1] + Vector2.Distance(section[j - 1], section[j % sides]);
            // Smooth 2D normals of the section outline (CCW: outward is the right-hand normal of each edge).
            var sectionNormals = new Vector2[sides];
            for (int j = 0; j < sides; j++)
            {
                Vector2 prev = section[(j - 1 + sides) % sides], next = section[(j + 1) % sides];
                Vector2 edgeA = (section[j] - prev).normalized, edgeB = (next - section[j]).normalized;
                Vector2 n = new Vector2(edgeA.y, -edgeA.x) + new Vector2(edgeB.y, -edgeB.x);
                sectionNormals[j] = n.sqrMagnitude > 1e-8f ? n.normalized : new Vector2(edgeA.y, -edgeA.x);
            }
            // Rotation-minimising frames by parallel transport from the up hint.
            var tangents = new Vector3[rings];
            for (int i = 0; i < rings; i++)
                tangents[i] = (path[Mathf.Min(rings - 1, i + 1)] - path[Mathf.Max(0, i - 1)]).normalized;
            var ups = new Vector3[rings];
            Vector3 up = Vector3.ProjectOnPlane(upHint, tangents[0]);
            if (up.sqrMagnitude < 1e-6f) up = Vector3.ProjectOnPlane(Vector3.forward, tangents[0]);
            ups[0] = up.normalized;
            for (int i = 1; i < rings; i++) ups[i] = Quaternion.FromToRotation(tangents[i - 1], tangents[i]) * ups[i - 1];
            var tris = Submesh(submesh);
            int start = _vertices.Count;
            for (int i = 0; i < rings; i++)
            {
                Vector3 side = Vector3.Cross(ups[i], tangents[i]).normalized;
                float s = scale != null ? scale(along[i] / total) : 1f;
                for (int j = 0; j <= sides; j++)
                {
                    Vector2 p = section[j % sides] * s, n = sectionNormals[j % sides];
                    Vertex(path[i] + side * p.x + ups[i] * p.y, (side * n.x + ups[i] * n.y).normalized, new Vector2(along[i], around[j] * s), tangents[i]);
                }
            }
            for (int i = 0; i < rings - 1; i++)
                for (int j = 0; j < sides; j++)
                {
                    int a = start + i * (sides + 1) + j, b = a + 1, c = a + sides + 1, d = c + 1;
                    tris.AddRange(new[] { a, b, c, b, d, c });
                }
            if (caps)
            {
                Cap(path[0], -tangents[0], Vector3.Cross(ups[0], tangents[0]).normalized, ups[0], section, scale != null ? scale(0f) : 1f, tris, true, tangents[0]);
                Cap(path[rings - 1], tangents[rings - 1], Vector3.Cross(ups[rings - 1], tangents[rings - 1]).normalized, ups[rings - 1], section, scale != null ? scale(1f) : 1f, tris, false, tangents[rings - 1]);
            }
            return this;
        }

        private void Cap(Vector3 centre, Vector3 normal, Vector3 side, Vector3 up, Vector2[] section, float s, List<int> tris, bool flip, Vector3 grain)
        {
            int middle = Vertex(centre, normal, Vector2.zero, grain);
            int first = _vertices.Count;
            foreach (var p in section) Vertex(centre + side * p.x * s + up * p.y * s, normal, p * s, grain);
            for (int j = 0; j < section.Length; j++)
            {
                int a = first + j, b = first + (j + 1) % section.Length;
                if (flip) tris.AddRange(new[] { middle, b, a });
                else tris.AddRange(new[] { middle, a, b });
            }
        }

        /// <summary>A straight member from <paramref name="a"/> to <paramref name="b"/> (rails, stiles, legs).</summary>
        public LauTuBaJoinery Member(Vector3 a, Vector3 b, float width, float height, float ease, Vector3 upHint, int submesh = 0) =>
            Sweep(new[] { a, b }, EasedRect(width, height, ease), upHint, submesh);

        /// <summary>
        /// A board cut from <paramref name="outline"/> (closed, in its own X/Y plane, any simple polygon) and
        /// <paramref name="thickness"/> deep along local Z, placed by <paramref name="place"/>. Grain along local X.
        /// </summary>
        public LauTuBaJoinery Board(Vector2[] outline, float thickness, Matrix4x4 place, int submesh = 0)
        {
            var tris = Submesh(submesh);
            var points = new List<Vector2>(outline);
            if (SignedArea(points) < 0f) points.Reverse();
            Vector3 grain = place.MultiplyVector(Vector3.right).normalized;
            Vector3 front = place.MultiplyVector(Vector3.back).normalized, back = -front;
            var order = Triangulate(points);
            foreach (float z in new[] { -thickness * 0.5f, thickness * 0.5f })
            {
                bool isFront = z < 0f;
                int first = _vertices.Count;
                foreach (var p in points) Vertex(place.MultiplyPoint3x4(new Vector3(p.x, p.y, z)), isFront ? front : back, p, grain);
                for (int t = 0; t < order.Count; t += 3)
                {
                    if (isFront) tris.AddRange(new[] { first + order[t], first + order[t + 2], first + order[t + 1] });
                    else tris.AddRange(new[] { first + order[t], first + order[t + 1], first + order[t + 2] });
                }
            }
            // Edges: hard, each with its own normal; UV u along the outline, v across the thickness.
            float run = 0f;
            for (int j = 0; j < points.Count; j++)
            {
                Vector2 a = points[j], b = points[(j + 1) % points.Count];
                float length = Vector2.Distance(a, b);
                Vector2 n2 = new Vector2(b.y - a.y, a.x - b.x).normalized;
                Vector3 n = place.MultiplyVector(new Vector3(n2.x, n2.y, 0f)).normalized;
                int i0 = Vertex(place.MultiplyPoint3x4(new Vector3(a.x, a.y, -thickness * 0.5f)), n, new Vector2(run, 0f), grain);
                int i1 = Vertex(place.MultiplyPoint3x4(new Vector3(b.x, b.y, -thickness * 0.5f)), n, new Vector2(run + length, 0f), grain);
                int i2 = Vertex(place.MultiplyPoint3x4(new Vector3(a.x, a.y, thickness * 0.5f)), n, new Vector2(run, thickness), grain);
                int i3 = Vertex(place.MultiplyPoint3x4(new Vector3(b.x, b.y, thickness * 0.5f)), n, new Vector2(run + length, thickness), grain);
                tris.AddRange(new[] { i0, i1, i2, i1, i3, i2 });
                run += length;
            }
            return this;
        }

        /// <summary>A turned part around local +Y from <paramref name="foot"/>: profile (radius, height), UV u along the axis.</summary>
        public LauTuBaJoinery Turned(Vector3 foot, Vector2[] profile, int segments = 12, int submesh = 0, Quaternion? rotation = null)
        {
            var rot = rotation ?? Quaternion.identity;
            var tris = Submesh(submesh);
            int start = _vertices.Count;
            float along = 0f;
            Vector3 axis = rot * Vector3.up;
            for (int i = 0; i < profile.Length; i++)
            {
                if (i > 0) along += Vector2.Distance(profile[i - 1], profile[i]);
                Vector2 prev = profile[Mathf.Max(0, i - 1)], next = profile[Mathf.Min(profile.Length - 1, i + 1)];
                Vector2 slope = (next - prev).normalized; // (dr, dh)
                for (int s = 0; s <= segments; s++)
                {
                    float angle = s * Mathf.PI * 2f / segments;
                    var radial = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                    var local = radial * profile[i].x + Vector3.up * profile[i].y;
                    var normal = (radial * slope.y - Vector3.up * slope.x).normalized;
                    Vertex(foot + rot * local, rot * normal, new Vector2(along, angle * Mathf.Max(0.01f, profile[i].x)), axis);
                }
            }
            for (int i = 0; i < profile.Length - 1; i++)
                for (int s = 0; s < segments; s++)
                {
                    int a = start + i * (segments + 1) + s, b = a + 1, c = a + segments + 1, d = c + 1;
                    tris.AddRange(new[] { a, c, b, b, c, d });
                }
            return this;
        }

        /// <summary>
        /// A surface sampled on a (<paramref name="columns"/>+1) × (<paramref name="rows"/>+1) grid of (u, v) in 0..1, with
        /// its points, normals and UVs given by the callbacks (roof tiles with their relief). Faces up for a grid whose u
        /// runs along +X and v along +Z; <paramref name="flip"/> turns it the other way. Grain along u.
        /// </summary>
        public LauTuBaJoinery Grid(int columns, int rows, System.Func<float, float, Vector3> point, System.Func<float, float, Vector3> normal,
            System.Func<float, float, Vector2> uv, int submesh = 0, bool flip = false)
        {
            var tris = Submesh(submesh);
            int start = _vertices.Count;
            for (int j = 0; j <= rows; j++)
                for (int i = 0; i <= columns; i++)
                {
                    float u = (float)i / columns, v = (float)j / rows;
                    var p = point(u, v);
                    var along = point(Mathf.Min(1f, u + 1f / columns), v) - point(Mathf.Max(0f, u - 1f / columns), v);
                    Vertex(p, normal(u, v), uv(u, v), along.normalized);
                }
            for (int j = 0; j < rows; j++)
                for (int i = 0; i < columns; i++)
                {
                    int a = start + j * (columns + 1) + i, b = a + 1, c = a + columns + 1, d = c + 1;
                    if (flip) tris.AddRange(new[] { a, b, c, b, d, c });
                    else tris.AddRange(new[] { a, c, b, b, c, d });
                }
            return this;
        }

        /// <summary>Adds an existing mesh (e.g. a rounded box) at <paramref name="place"/>; its UV3 grain is carried over.</summary>
        public LauTuBaJoinery Add(Mesh mesh, Matrix4x4 place, int submesh = 0)
        {
            var tris = Submesh(submesh);
            int start = _vertices.Count;
            var grain = new List<Vector3>();
            mesh.GetUVs(3, grain);
            var vertices = mesh.vertices; var normals = mesh.normals; var uvs = mesh.uv;
            for (int i = 0; i < vertices.Length; i++)
                Vertex(place.MultiplyPoint3x4(vertices[i]), place.MultiplyVector(normals[i]).normalized, uvs.Length > i ? uvs[i] : Vector2.zero,
                    grain.Count > i ? place.MultiplyVector(grain[i]).normalized : place.MultiplyVector(Vector3.right).normalized);
            foreach (int index in mesh.triangles) tris.Add(start + index);
            return this;
        }

        public Mesh Build(string name)
        {
            var mesh = new Mesh { name = name, indexFormat = _vertices.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
            mesh.SetVertices(_vertices);
            mesh.SetNormals(_normals);
            mesh.SetUVs(0, _uvs);
            mesh.SetUVs(3, _grain);
            mesh.subMeshCount = _triangles.Count;
            for (int i = 0; i < _triangles.Count; i++) mesh.SetTriangles(_triangles[i], i);
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        public int TriangleCount { get { int n = 0; foreach (var t in _triangles) n += t.Count / 3; return n; } }

        // ------------------------------------------------------------------ polygon helpers

        private static float SignedArea(IList<Vector2> points)
        {
            float area = 0f;
            for (int i = 0; i < points.Count; i++) { var a = points[i]; var b = points[(i + 1) % points.Count]; area += a.x * b.y - b.x * a.y; }
            return area * 0.5f;
        }

        /// <summary>Ear clipping of a simple CCW polygon; returns index triples.</summary>
        private static List<int> Triangulate(IList<Vector2> points)
        {
            var result = new List<int>();
            var remaining = new List<int>();
            for (int i = 0; i < points.Count; i++) remaining.Add(i);
            int guard = points.Count * points.Count;
            while (remaining.Count > 3 && guard-- > 0)
            {
                bool clipped = false;
                for (int i = 0; i < remaining.Count; i++)
                {
                    int ia = remaining[(i - 1 + remaining.Count) % remaining.Count], ib = remaining[i], ic = remaining[(i + 1) % remaining.Count];
                    Vector2 a = points[ia], b = points[ib], c = points[ic];
                    if ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x) <= 1e-9f) continue; // reflex
                    bool inside = false;
                    foreach (int k in remaining)
                    {
                        if (k == ia || k == ib || k == ic) continue;
                        if (InTriangle(points[k], a, b, c)) { inside = true; break; }
                    }
                    if (inside) continue;
                    result.AddRange(new[] { ia, ib, ic });
                    remaining.RemoveAt(i);
                    clipped = true;
                    break;
                }
                if (!clipped) break;
            }
            if (remaining.Count == 3) result.AddRange(remaining);
            return result;
        }

        private static bool InTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = (p.x - b.x) * (a.y - b.y) - (a.x - b.x) * (p.y - b.y);
            float d2 = (p.x - c.x) * (b.y - c.y) - (b.x - c.x) * (p.y - c.y);
            float d3 = (p.x - a.x) * (c.y - a.y) - (c.x - a.x) * (p.y - a.y);
            bool neg = d1 < 0 || d2 < 0 || d3 < 0, pos = d1 > 0 || d2 > 0 || d3 > 0;
            return !(neg && pos);
        }
    }
}
