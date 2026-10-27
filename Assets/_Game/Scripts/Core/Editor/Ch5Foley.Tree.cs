using System.Linq;
using System.Text;
using UnityEngine;

namespace ThuyKieu.Core.Editor
{
    public static partial class Ch5Foley
    {
        /// <summary>
        /// The falling bamboo from kheetor's "chop-tree-fall" (CC0): the recording is an axe chopping, the trunk creaking and
        /// the tree hitting the ground. The chops are left out entirely (nobody chops the culm). Cut by the envelope: the
        /// impact is the loudest hit of the second half; the chops are the sharp hits before the creak; the creak is what lies
        /// between the last chop and the fall; the sweep is the rush just before the impact; the settle is the leaves after it.
        /// A culm is lighter and hollower than a tree: the impact is raised ×1.12; the creak keeps its pitch.
        /// </summary>
        private static void Tree(StringBuilder report)
        {
            var x = Ch4Foley.Samples(TreeSource, out int rate, out _);
            x = Ch4Foley.Resample(x, rate, Rate);
            float length = x.Length / (float)Rate;
            var env = Ch4Foley.Envelope(x, Rate / 100);
            int half = env.Length / 2;
            // The first strong hit of the second half (the culm meeting the ground; the hit after it is its bounce, kept in the impact).
            float fallPeak = env.Skip(half).Max();
            int impact = half;
            while (impact < env.Length - 1 && env[impact] < fallPeak * 0.6f) impact++;
            while (impact > half && env[impact - 1] > env[impact] * 0.5f && env[impact - 1] > fallPeak * 0.2f) impact--;
            float peak = env.Max();
            // Sharp hits: the envelope jumping through 30 % of the peak within 10 ms, at least 150 ms apart, before the fall.
            var hits = new System.Collections.Generic.List<int>();
            for (int i = Rate / 100; i < impact - Rate / 2; i++)
                if (env[i] >= peak * 0.3f && env[i - Rate / 100] < peak * 0.12f && (hits.Count == 0 || i - hits[hits.Count - 1] > Rate * 15 / 100)) hits.Add(i);
            int lastChop = hits.Count > 0 ? hits[hits.Count - 1] : 0;
            // Each chop rings for up to 0.35 s; the creak starts after the last ring.
            int creakFrom = hits.Count > 0 ? Mathf.Min(impact - Rate, lastChop + Rate * 35 / 100) : Rate / 10;
            // The rush is only the last ~0.3 s before the impact; the crackling before it is the creak.
            int sweepFrom = Mathf.Max(creakFrom + Rate / 4, impact - Rate * 30 / 100);
            int creakTo = sweepFrom;
            report.AppendLine($"tree: {length:F2} s · chops at {string.Join(", ", hits.Select(h => (h / (float)Rate).ToString("F2")))} s (left out) · creak {creakFrom / (float)Rate:F2}–{creakTo / (float)Rate:F2} s · sweep {sweepFrom / (float)Rate:F2}–{impact / (float)Rate:F2} s · impact {impact / (float)Rate:F2} s");

            float[] Span(int from, int to) => x.Skip(Mathf.Max(0, from)).Take(Mathf.Max(1, Mathf.Min(x.Length, to) - Mathf.Max(0, from))).ToArray();
            // Two cracks from the creak (its first and second half), so a culm's two warnings differ.
            int creakMid = (creakFrom + creakTo) / 2;
            Render(report, "bamboo_crack", -26f, new[]
            {
                (Fade(Span(creakFrom, creakMid), 0.01f, 0.08f), $"chop-tree-fall {creakFrom / (float)Rate:F2}–{creakMid / (float)Rate:F2} s (creak)"),
                (Fade(Span(creakMid, creakTo), 0.01f, 0.08f), $"chop-tree-fall {creakMid / (float)Rate:F2}–{creakTo / (float)Rate:F2} s (creak)")
            }, sustained: true);
            Render(report, "bamboo_sweep", -26f, new[] { (Fade(Span(sweepFrom, impact - Rate / 100), 0.2f, 0.03f), $"chop-tree-fall {sweepFrom / (float)Rate:F2}–{impact / (float)Rate:F2} s (rush before the impact)") }, sustained: true);
            int ring = Mathf.Min(x.Length, impact + Rate * 7 / 10);
            Render(report, "bamboo_impact", -13f, new[] { (Pitch(Fade(Span(impact - Rate / 50, ring), 0.002f, 0.25f), 1.12f), $"chop-tree-fall {impact / (float)Rate:F2}–{ring / (float)Rate:F2} s (impact) ×1.12") });
            int settleFrom = impact + Rate / 4, settleTo = Mathf.Min(x.Length, impact + Rate * 18 / 10);
            if (settleTo - settleFrom > Rate / 4)
                Render(report, "leaf_settle", -32f, new[] { (Fade(Span(settleFrom, settleTo), 0.1f, 0.4f), $"chop-tree-fall {settleFrom / (float)Rate:F2}–{settleTo / (float)Rate:F2} s (after the impact)") }, sustained: true);
            // The rope tearing off the stake: the sharpest crack inside the creak, short and raised (fibres, not an axe).
            int crack = creakFrom;
            for (int i = creakFrom + Rate / 100; i < creakTo; i++) if (env[i] - env[i - Rate / 100] > env[crack] - env[Mathf.Max(0, crack - Rate / 100)]) crack = i;
            Render(report, "rope_snap", -18f, new[] { (Pitch(Fade(Span(crack - Rate / 100, crack + Rate / 4), 0.002f, 0.08f), 1.3f), $"chop-tree-fall {crack / (float)Rate:F2} s (sharpest crack of the creak) ×1.3") });
        }
    }
}
