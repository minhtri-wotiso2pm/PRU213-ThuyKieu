using UnityEngine;

namespace ThuyKieu.Environment
{
    /// <summary>
    /// 2D gradient noise in 0–1 for the Chapter 5 forest, in plain C# so the ground and the bamboo can be worked out on
    /// worker threads (and give the same shape in the Editor build and at runtime).
    /// </summary>
    public static class ForestNoise
    {
        private static readonly int[] Permutation = Make();

        private static int[] Make()
        {
            var random = new System.Random(1031);
            var p = new int[512];
            for (int i = 0; i < 256; i++) p[i] = i;
            for (int i = 255; i > 0; i--) { int j = random.Next(i + 1); (p[i], p[j]) = (p[j], p[i]); }
            for (int i = 0; i < 256; i++) p[256 + i] = p[i];
            return p;
        }

        /// <summary>Smooth noise of period 256, 0–1 (about 0.5 on average).</summary>
        public static float Perlin(float x, float y)
        {
            int xi = Mathf.FloorToInt(x), yi = Mathf.FloorToInt(y);
            float xf = x - xi, yf = y - yi;
            xi &= 255; yi &= 255;
            float u = Fade(xf), v = Fade(yf);
            int aa = Permutation[Permutation[xi] + yi], ab = Permutation[Permutation[xi] + yi + 1];
            int ba = Permutation[Permutation[xi + 1] + yi], bb = Permutation[Permutation[xi + 1] + yi + 1];
            float x1 = Lerp(Grad(aa, xf, yf), Grad(ba, xf - 1f, yf), u);
            float x2 = Lerp(Grad(ab, xf, yf - 1f), Grad(bb, xf - 1f, yf - 1f), u);
            float n = Lerp(x1, x2, v); // about −0.7…0.7
            float k = n * 0.72f + 0.5f;
            return k < 0f ? 0f : k > 1f ? 1f : k;
        }

        private static float Fade(float t) => t * t * t * (t * (t * 6f - 15f) + 10f);
        private static float Lerp(float a, float b, float t) => a + (b - a) * t;

        private static float Grad(int hash, float x, float y)
        {
            switch (hash & 7)
            {
                case 0: return x + y;
                case 1: return -x + y;
                case 2: return x - y;
                case 3: return -x - y;
                case 4: return x;
                case 5: return -x;
                case 6: return y;
                default: return -y;
            }
        }
    }
}
