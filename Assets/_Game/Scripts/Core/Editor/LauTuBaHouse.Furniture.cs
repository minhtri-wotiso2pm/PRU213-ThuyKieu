using System.Collections.Generic;
using UnityEngine;
using static ThuyKieu.Core.Editor.LauTuBaKit;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// The house's main furniture as real joinery (see <see cref="LauTuBaJoinery"/>): one mesh per piece, UV-mapped in
    /// metres along the grain and drawn with URP Lit twins of the house woods. Shapes follow late-19th-century Vietnamese
    /// and southern Chinese pieces: the official's-hat armchair (yoke crest rail, S-curved splat, arched aprons), waisted
    /// tables with horse-hoof feet, the heavy sập with cloud-cut aprons and curled feet.
    /// </summary>
    public static partial class LauTuBaHouse
    {
        private static readonly Vector2[] ChairLeg = LauTuBaJoinery.EasedRect(0.04f, 0.04f, 0.008f, 2);

        /// <summary>Places a joinery mesh at the frame with the wood (0), panel (1) and metal/accent (2) materials.</summary>
        private static GameObject Furniture(string name, Mesh mesh, Transform frame, Material wood, Material panel = null, Material accent = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(frame, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var materials = new List<Material> { UvTwin(wood) };
            if (mesh.subMeshCount > 1) materials.Add(UvTwin(panel ?? wood));
            if (mesh.subMeshCount > 2) materials.Add(accent ?? _brass);
            go.AddComponent<MeshRenderer>().sharedMaterials = materials.ToArray();
            go.isStatic = true;
            return go;
        }

        /// <summary>An apron board's outline: straight top, ends dropping to full depth, the lower edge arched between them;
        /// <paramref name="clouds"/> adds the scalloped cloud lobes of carved pieces.</summary>
        private static Vector2[] ApronOutline(float length, float height, float rise, int clouds = 0)
        {
            float h = length * 0.5f;
            var points = new List<Vector2> { new Vector2(-h, 0f), new Vector2(h, 0f), new Vector2(h, -height) };
            const int steps = 28;
            for (int i = 1; i < steps; i++)
            {
                float x = Mathf.Lerp(h, -h, i / (float)steps), u = Mathf.Abs(x) / h;
                float y = -height + rise * Mathf.Pow(Mathf.Max(0f, 1f - u * u), 1.5f);
                if (clouds > 0) y += rise * 0.35f * Mathf.Abs(Mathf.Sin(u * Mathf.PI * clouds)) * (1f - u);
                points.Add(new Vector2(x, y));
            }
            points.Add(new Vector2(-h, -height));
            return points.ToArray();
        }

        /// <summary>A board's outline as a plain rectangle centred on the origin.</summary>
        private static Vector2[] BoardRect(float width, float height) =>
            new[] { new Vector2(-width * 0.5f, -height * 0.5f), new Vector2(width * 0.5f, -height * 0.5f), new Vector2(width * 0.5f, height * 0.5f), new Vector2(-width * 0.5f, height * 0.5f) };

        private static Matrix4x4 Flat(Vector3 centre) => Matrix4x4.TRS(centre, Quaternion.Euler(90f, 0f, 0f), Vector3.one);
        private static Matrix4x4 AlongX(Vector3 centre) => Matrix4x4.TRS(centre, Quaternion.identity, Vector3.one);
        private static Matrix4x4 AlongZ(Vector3 centre) => Matrix4x4.TRS(centre, Quaternion.Euler(0f, -90f, 0f), Vector3.one);

        // ------------------------------------------------------------------ armchair

        private static Mesh ArmchairMesh()
        {
            const string key = "Joinery_Armchair_v1";
            if (LauTuBaMeshKit.Has(key, out var cached)) return cached;
            const float seat = 0.47f, x0 = 0.265f, z0 = 0.215f, crest = 1.02f;
            var j = new LauTuBaJoinery();
            foreach (float s in new[] { -1f, 1f })
            {
                // Back leg rising through the seat into the back post, raked and bowed backwards, thinning upwards.
                j.Sweep(LauTuBaJoinery.Smooth(new[] { new Vector3(s * x0, 0f, -z0), new Vector3(s * x0, seat, -z0), new Vector3(s * x0, seat + 0.28f, -z0 - 0.035f),
                    new Vector3(s * (x0 - 0.012f), crest - 0.015f, -z0 - 0.05f) }), ChairLeg, Vector3.forward, 0, t => Mathf.Lerp(1f, 0.8f, t));
                j.Sweep(new[] { new Vector3(s * x0, 0f, z0), new Vector3(s * x0, seat - 0.01f, z0) }, ChairLeg, Vector3.forward);
                // Goose-neck arm post from the seat rail up to the arm.
                j.Sweep(LauTuBaJoinery.Smooth(new[] { new Vector3(s * x0, seat - 0.01f, z0 - 0.012f), new Vector3(s * (x0 + 0.006f), seat + 0.1f, z0 - 0.04f),
                    new Vector3(s * (x0 + 0.012f), seat + 0.205f, z0 - 0.025f) }), LauTuBaJoinery.Round(0.026f, 0.026f, 10), Vector3.forward);
                // Arm: from the back post forward, dipping, ending in a small downturned hook past the post.
                j.Sweep(LauTuBaJoinery.Smooth(new[] { new Vector3(s * (x0 - 0.004f), seat + 0.25f, -z0 - 0.025f), new Vector3(s * (x0 + 0.008f), seat + 0.236f, -0.05f),
                    new Vector3(s * (x0 + 0.012f), seat + 0.214f, z0 - 0.03f), new Vector3(s * (x0 + 0.018f), seat + 0.19f, z0 + 0.035f) }, 8), LauTuBaJoinery.Round(0.034f, 0.028f, 12), Vector3.up);
                j.Member(new Vector3(s * x0, seat - 0.02f, -z0), new Vector3(s * x0, seat - 0.02f, z0), 0.03f, 0.04f, 0.006f, Vector3.up);
                j.Board(ApronOutline(2f * z0 - 0.04f, 0.075f, 0.03f), 0.016f, AlongZ(new Vector3(s * x0, seat - 0.04f, 0f)));
                j.Member(new Vector3(s * x0, 0.13f, -z0), new Vector3(s * x0, 0.13f, z0), 0.022f, 0.022f, 0.005f, Vector3.up);
            }
            // Yoke crest rail: overhanging ends lifting forward, a slight bow in the middle.
            j.Sweep(LauTuBaJoinery.Smooth(new[] { new Vector3(-x0 - 0.075f, crest + 0.035f, -z0 - 0.02f), new Vector3(-x0 + 0.01f, crest, -z0 - 0.05f), new Vector3(0f, crest + 0.012f, -z0 - 0.075f),
                new Vector3(x0 - 0.01f, crest, -z0 - 0.05f), new Vector3(x0 + 0.075f, crest + 0.035f, -z0 - 0.02f) }, 8), LauTuBaJoinery.Round(0.05f, 0.04f, 14), Vector3.up, 0,
                t => 1f - 0.28f * Mathf.Pow(Mathf.Abs(t * 2f - 1f), 3f));
            // S-curved back splat in lacquer: leans back from the seat, swells forward for the lower back, back again to the crest.
            j.Sweep(LauTuBaJoinery.Smooth(new[] { new Vector3(0f, seat + 0.015f, -z0 + 0.005f), new Vector3(0f, seat + 0.2f, -z0 - 0.03f), new Vector3(0f, seat + 0.42f, -z0 - 0.022f),
                new Vector3(0f, crest - 0.02f, -z0 - 0.065f) }), LauTuBaJoinery.EasedRect(0.17f, 0.014f, 0.004f, 2), Vector3.forward, 1, t => 1f - 0.12f * t);
            // Seat: frame-and-panel with the front and back rails, an arched front apron, a plain back apron.
            j.Member(new Vector3(-x0, seat - 0.02f, z0), new Vector3(x0, seat - 0.02f, z0), 0.03f, 0.04f, 0.006f, Vector3.up);
            j.Member(new Vector3(-x0, seat - 0.02f, -z0), new Vector3(x0, seat - 0.02f, -z0), 0.03f, 0.04f, 0.006f, Vector3.up);
            j.Board(BoardRect(2f * x0 - 0.02f, 2f * z0 - 0.02f), 0.014f, Flat(new Vector3(0f, seat - 0.012f, 0f)), 1);
            j.Board(ApronOutline(2f * x0 - 0.04f, 0.09f, 0.038f), 0.016f, AlongX(new Vector3(0f, seat - 0.04f, z0)));
            j.Board(BoardRect(2f * x0 - 0.04f, 0.05f), 0.016f, AlongX(new Vector3(0f, seat - 0.065f, -z0)));
            // Stepped stretchers: the wide footrest in front, higher at the sides, highest at the back.
            j.Member(new Vector3(-x0, 0.075f, z0), new Vector3(x0, 0.075f, z0), 0.045f, 0.02f, 0.005f, Vector3.up);
            j.Member(new Vector3(-x0, 0.16f, -z0), new Vector3(x0, 0.16f, -z0), 0.022f, 0.022f, 0.005f, Vector3.up);
            return LauTuBaMeshKit.Keep(key, j.Build(key));
        }

        private static Transform Armchair(string name, Transform parent, Vector3 floor, float yaw, Material wood, Material cushion)
        {
            var frame = Frame(name, parent, floor, yaw);
            Furniture("Joinery", ArmchairMesh(), frame, wood, Lacquer);
            Cushion("Cushion", frame, new Vector3(0f, 0.485f, 0.005f), new Vector3(0.46f, 0.035f, 0.38f), cushion, 0.01f);
            Solid(frame, new Vector3(0f, 0.45f, 0f), new Vector3(0.6f, 0.9f, 0.5f));
            return frame;
        }

        // ------------------------------------------------------------------ tables

        private static Mesh TableMesh(float width, float depth, float height, bool turned, bool carved)
        {
            string key = $"Joinery_Table_{width:F2}_{depth:F2}_{height:F2}_{(turned ? 1 : 0)}{(carved ? 1 : 0)}_v1".Replace('.', 'p');
            if (LauTuBaMeshKit.Has(key, out var cached)) return cached;
            var j = new LauTuBaJoinery();
            float top = Mathf.Clamp(height * 0.045f, 0.022f, 0.036f);
            float leg = Mathf.Clamp(Mathf.Min(width, depth) * 0.08f, 0.032f, 0.058f);
            float inset = leg * 0.5f + 0.018f;
            float legTop = height - top - 0.018f;
            // Top board with a bead run round its lower edge, a recessed waist under it.
            j.Board(BoardRect(width, depth), top, Flat(new Vector3(0f, height - top * 0.5f, 0f)));
            const float r = 0.012f;
            var bead = new List<Vector3>();
            float hx = width * 0.5f - r, hz = depth * 0.5f - r;
            foreach (var (cx, cz, a0) in new[] { (hx, hz, 0f), (-hx, hz, 90f), (-hx, -hz, 180f), (hx, -hz, 270f) })
                for (int s = 0; s <= 3; s++)
                {
                    float angle = (a0 + 30f * s) * Mathf.Deg2Rad;
                    bead.Add(new Vector3(cx + Mathf.Cos(angle) * r, height - top + 0.004f, cz + Mathf.Sin(angle) * r));
                }
            bead.Add(bead[0]);
            j.Sweep(bead, LauTuBaJoinery.Round(0.01f, 0.01f, 8), Vector3.up, 0, null, false);
            j.Board(BoardRect(width - 2f * inset + leg * 0.6f, depth - 2f * inset + leg * 0.6f), 0.018f, Flat(new Vector3(0f, height - top - 0.009f, 0f)), 1);
            // Legs: square, tapering, the foot turning in as a horse hoof; or turned like a baluster.
            foreach (float sx in new[] { -1f, 1f })
                foreach (float sz in new[] { -1f, 1f })
                {
                    var at = new Vector3(sx * (width * 0.5f - inset), 0f, sz * (depth * 0.5f - inset));
                    if (turned)
                    {
                        float k = legTop;
                        j.Turned(at, new[] { new Vector2(0.024f, 0f), new Vector2(0.03f, 0.015f), new Vector2(0.026f, 0.035f), new Vector2(0.018f, k * 0.22f), new Vector2(0.027f, k * 0.38f),
                            new Vector2(0.03f, k * 0.46f), new Vector2(0.019f, k * 0.62f), new Vector2(0.017f, k * 0.86f), new Vector2(0.028f, k * 0.9f), new Vector2(0.028f, k) }, 12);
                    }
                    else
                    {
                        var inward = new Vector3(-sx, 0f, -sz).normalized;
                        var section = LauTuBaJoinery.EasedRect(leg, leg, leg * 0.18f, 2);
                        j.Sweep(LauTuBaJoinery.Smooth(new[] { at + Vector3.up * legTop, at + Vector3.up * 0.16f, at + Vector3.up * 0.05f + inward * 0.006f, at + inward * 0.02f }, 5), section, Vector3.forward, 0,
                            t => t < 0.75f ? Mathf.Lerp(1f, 0.82f, t / 0.75f) : Mathf.Lerp(0.82f, 1.05f, (t - 0.75f) / 0.25f));
                    }
                }
            // Aprons flush with the legs' outer faces, their lower edge arched (scalloped with clouds when carved).
            float apron = Mathf.Clamp(height * 0.11f, 0.045f, 0.09f);
            float faceX = width * 0.5f - inset + leg * 0.5f - 0.01f, faceZ = depth * 0.5f - inset + leg * 0.5f - 0.01f;
            foreach (float s in new[] { -1f, 1f })
            {
                j.Board(ApronOutline(width - 2f * inset, apron, apron * 0.42f, carved ? 3 : 0), 0.016f, AlongX(new Vector3(0f, legTop, s * faceZ)));
                j.Board(ApronOutline(depth - 2f * inset, apron, apron * 0.42f, carved ? 2 : 0), 0.016f, AlongZ(new Vector3(s * faceX, legTop, 0f)));
            }
            // Stretchers on taller tables: humped along the long sides, straight across the ends.
            if (height > 0.5f)
            {
                float y = 0.13f, hump = Mathf.Min(0.06f, width * 0.04f);
                float lx = width * 0.5f - inset, lz = depth * 0.5f - inset;
                foreach (float s in new[] { -1f, 1f })
                {
                    j.Sweep(LauTuBaJoinery.Smooth(new[] { new Vector3(-lx, y, s * lz), new Vector3(-lx * 0.45f, y, s * lz), new Vector3(0f, y + hump, s * lz), new Vector3(lx * 0.45f, y, s * lz), new Vector3(lx, y, s * lz) }, 5),
                        LauTuBaJoinery.EasedRect(0.022f, 0.026f, 0.005f, 2), Vector3.up);
                    j.Member(new Vector3(s * lx, y, -lz), new Vector3(s * lx, y, lz), 0.022f, 0.026f, 0.005f, Vector3.up);
                }
            }
            return LauTuBaMeshKit.Keep(key, j.Build(key));
        }

        private static Transform Table(string name, Transform parent, Vector3 floor, float yaw, float width, float depth, float height, Material wood, bool turned, bool carved, bool solid = true)
        {
            var frame = Frame(name, parent, floor, yaw);
            Furniture("Joinery", TableMesh(width, depth, height, turned, carved), frame, wood, wood == Lacquer ? Lacquer : wood);
            if (solid) Solid(frame, new Vector3(0f, height * 0.5f, 0f), new Vector3(width, height, depth));
            return frame;
        }

        // ------------------------------------------------------------------ sập

        private static Mesh SapMesh(float length, float width, bool rich)
        {
            string key = $"Joinery_Sap_{length:F2}_{width:F2}_{(rich ? 1 : 0)}_v1".Replace('.', 'p');
            if (LauTuBaMeshKit.Has(key, out var cached)) return cached;
            var j = new LauTuBaJoinery();
            const float surface = 0.41f, rail = 0.085f, railH = 0.055f, leg = 0.085f;
            float hx = width * 0.5f - rail * 0.5f, hz = length * 0.5f - rail * 0.5f;
            // Heavy frame round a recessed panel of three boards.
            foreach (float s in new[] { -1f, 1f })
            {
                j.Member(new Vector3(-width * 0.5f, surface - railH * 0.5f, s * hz), new Vector3(width * 0.5f, surface - railH * 0.5f, s * hz), rail, railH, 0.012f, Vector3.up);
                j.Member(new Vector3(s * hx, surface - railH * 0.5f, -hz), new Vector3(s * hx, surface - railH * 0.5f, hz), rail, railH, 0.012f, Vector3.up);
            }
            float panelW = width - 2f * rail + 0.01f, panelL = length - 2f * rail + 0.01f;
            for (int b = 0; b < 3; b++)
            {
                float bw = panelW / 3f;
                j.Board(BoardRect(panelL, bw - 0.004f), 0.025f, Flat(new Vector3(-panelW * 0.5f + bw * (b + 0.5f), surface - 0.018f, 0f)) * Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, 90f)), 1);
            }
            // Waist and deep cloud-cut aprons.
            j.Board(BoardRect(width - 0.05f, length - 0.05f), 0.03f, Flat(new Vector3(0f, surface - railH - 0.015f, 0f)));
            float apronTop = surface - railH - 0.03f, inset = leg * 0.5f + 0.01f;
            foreach (float s in new[] { -1f, 1f })
            {
                j.Board(ApronOutline(width - 2f * inset, 0.13f, 0.06f, rich ? 2 : 1), 0.022f, AlongX(new Vector3(0f, apronTop, s * (length * 0.5f - 0.02f))));
                j.Board(ApronOutline(length - 2f * inset, 0.13f, 0.06f, rich ? 4 : 2), 0.022f, AlongZ(new Vector3(s * (width * 0.5f - 0.02f), apronTop, 0f)));
                if (rich)
                {
                    // A gilt line along the top of the long aprons.
                    j.Member(new Vector3(s * (width * 0.5f - 0.008f), apronTop - 0.012f, -length * 0.5f + inset), new Vector3(s * (width * 0.5f - 0.008f), apronTop - 0.012f, length * 0.5f - inset), 0.006f, 0.008f, 0.002f, Vector3.up, 2);
                    j.Member(new Vector3(-width * 0.5f + inset, apronTop - 0.012f, s * (length * 0.5f - 0.008f)), new Vector3(width * 0.5f - inset, apronTop - 0.012f, s * (length * 0.5f - 0.008f)), 0.006f, 0.008f, 0.002f, Vector3.up, 2);
                }
            }
            // Legs: heavy, square, curling inward at the foot.
            foreach (float sx in new[] { -1f, 1f })
                foreach (float sz in new[] { -1f, 1f })
                {
                    var at = new Vector3(sx * (width * 0.5f - inset), 0f, sz * (length * 0.5f - inset));
                    var inward = new Vector3(-sx, 0f, -sz).normalized;
                    j.Sweep(LauTuBaJoinery.Smooth(new[] { at + Vector3.up * apronTop, at + Vector3.up * 0.13f, at + Vector3.up * 0.045f + inward * 0.01f, at + inward * 0.035f }, 5),
                        LauTuBaJoinery.EasedRect(leg, leg, 0.014f, 2), Vector3.forward, 0, t => t < 0.7f ? Mathf.Lerp(1f, 0.78f, t / 0.7f) : Mathf.Lerp(0.78f, 0.95f, (t - 0.7f) / 0.3f));
                }
            return LauTuBaMeshKit.Keep(key, j.Build(key));
        }

        private static Transform Sap(string name, Transform parent, Vector3 floor, float yaw, float length, float width, bool rich)
        {
            var frame = Frame(name, parent, floor, yaw);
            var wood = rich ? Lacquer : Timber;
            Furniture("Joinery", SapMesh(length, width, rich), frame, wood, wood, _silkGold);
            float mattress = rich ? 0.05f : 0.035f;
            Cushion("Mattress", frame, new Vector3(0f, 0.41f + mattress * 0.5f, 0.02f), new Vector3(width - 0.16f, mattress, length - 0.18f), rich ? _silkRed : _cotton, 0.008f);
            if (rich)
            {
                Bolster(frame, new Vector3(0f, 0.41f + mattress + 0.07f, length * 0.5f - 0.18f), width - 0.3f, 0.07f, _silkDeep);
                Cushion("Pillow", frame, new Vector3(-0.15f, 0.41f + mattress + 0.04f, length * 0.5f - 0.38f), new Vector3(0.36f, 0.08f, 0.24f), _silkGold, 0.015f);
            }
            Solid(frame, new Vector3(0f, 0.22f, 0f), new Vector3(width, 0.44f, length));
            return frame;
        }
    }
}
