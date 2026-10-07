using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ThuyKieu.UI
{
    /// <summary>
    /// Main-menu backdrop drawn in code: a dusk sky over layered ink-wash mountains fading into mist,
    /// a full moon and slow lantern embers drifting upwards. No texture assets needed.
    /// </summary>
    public class MenuBackdrop : MonoBehaviour
    {
        private struct Ember { public RectTransform Rect; public Image Image; public float X, Speed, Phase, Size; }

        private readonly List<Ember> _embers = new List<Ember>();
        private RectTransform _area;

        public static MenuBackdrop Build(Transform parent)
        {
            var area = StoryUIBuilder.Rect(parent, "Backdrop", Vector2.zero, Vector2.one);
            var sky = area.gameObject.AddComponent<RawImage>();
            sky.texture = Landscape(640, 360);
            sky.raycastTarget = false;

            var moon = StoryUIBuilder.Rect(area, "Moon", new Vector2(0.73f, 0.66f), new Vector2(0.73f, 0.66f));
            moon.sizeDelta = new Vector2(170, 170);
            var moonImage = moon.gameObject.AddComponent<RawImage>();
            moonImage.texture = Disc(128, 0.42f, 0.5f);
            moonImage.color = new Color(1f, 0.93f, 0.78f, 0.95f);
            moonImage.raycastTarget = false;
            var glow = StoryUIBuilder.Rect(moon, "Glow", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            glow.sizeDelta = new Vector2(620, 620);
            var glowImage = glow.gameObject.AddComponent<RawImage>();
            glowImage.texture = Disc(128, 0f, 0.5f);
            glowImage.color = new Color(1f, 0.75f, 0.45f, 0.16f);
            glowImage.raycastTarget = false;
            glow.SetAsFirstSibling();

            // Darken the left third so the title and buttons read clearly.
            var shade = StoryUIBuilder.Rect(area, "Shade", Vector2.zero, new Vector2(0.62f, 1f));
            var shadeImage = shade.gameObject.AddComponent<RawImage>();
            shadeImage.texture = HorizontalFade();
            shadeImage.raycastTarget = false;

            var backdrop = area.gameObject.AddComponent<MenuBackdrop>();
            backdrop._area = area;
            var dot = Disc(32, 0f, 0.5f);
            var sprite = Sprite.Create(dot, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
            for (int i = 0; i < 26; i++)
            {
                var rect = StoryUIBuilder.Rect(area, "Ember" + i, Vector2.zero, Vector2.zero);
                var image = rect.gameObject.AddComponent<Image>();
                image.sprite = sprite;
                image.raycastTarget = false;
                backdrop._embers.Add(new Ember
                {
                    Rect = rect, Image = image,
                    X = Random.value, Speed = Random.Range(0.012f, 0.035f), Phase = Random.value * 10f, Size = Random.Range(5f, 13f)
                });
            }
            return backdrop;
        }

        private void Update()
        {
            float t = Time.unscaledTime;
            foreach (var ember in _embers)
            {
                float y = Mathf.Repeat(ember.Phase * 0.1f + t * ember.Speed, 1.15f) - 0.05f;
                float x = ember.X + Mathf.Sin(t * 0.4f + ember.Phase) * 0.02f;
                ember.Rect.anchorMin = ember.Rect.anchorMax = new Vector2(x, y);
                ember.Rect.sizeDelta = Vector2.one * ember.Size;
                float flicker = 0.55f + 0.45f * Mathf.Sin(t * 2.3f + ember.Phase * 3f);
                float fade = Mathf.Clamp01(y * 4f) * Mathf.Clamp01((1.05f - y) * 3f);
                ember.Image.color = new Color(1f, 0.62f + 0.2f * flicker, 0.3f, 0.75f * flicker * fade);
            }
        }

        // ---------------------------------------------------------------- textures

        private static Texture2D Landscape(int width, int height)
        {
            var pixels = new Color[width * height];
            Color top = new Color(0.025f, 0.025f, 0.06f), mid = new Color(0.12f, 0.07f, 0.13f), horizon = new Color(0.42f, 0.2f, 0.17f);
            Color mist = new Color(0.33f, 0.2f, 0.22f);
            // far → near: base height, amplitude, frequency, ink colour, how much the foot dissolves into mist
            var layers = new[]
            {
                (0.42f, 0.16f, 2.2f, new Color(0.2f, 0.12f, 0.16f), 0.85f),
                (0.31f, 0.14f, 3.4f, new Color(0.1f, 0.065f, 0.1f), 0.7f),
                (0.17f, 0.12f, 5.5f, new Color(0.035f, 0.028f, 0.045f), 0.35f),
            };
            for (int y = 0; y < height; y++)
            {
                float v = y / (height - 1f);
                Color sky = v < 0.45f ? Color.Lerp(horizon, mid, Smooth(v / 0.45f)) : Color.Lerp(mid, top, Smooth((v - 0.45f) / 0.55f));
                for (int x = 0; x < width; x++)
                {
                    float u = x / (width - 1f);
                    Color color = sky;
                    for (int i = 0; i < layers.Length; i++)
                    {
                        var (baseHeight, amplitude, frequency, ink, dissolve) = layers[i];
                        float ridge = baseHeight + amplitude * (Mathf.PerlinNoise(u * frequency + i * 7.3f, i * 3.1f) - 0.35f)
                                      + 0.025f * (Mathf.PerlinNoise(u * frequency * 6f, i * 5.7f) - 0.5f);
                        if (v > ridge) continue;
                        float depth = Mathf.Clamp01((ridge - v) / 0.2f);
                        color = Color.Lerp(ink, Color.Lerp(mist, sky, 0.4f), depth * dissolve);
                    }
                    // Low mist band hugging the bottom.
                    color = Color.Lerp(color, mist * 0.6f, Mathf.Clamp01(0.12f - v) * 2.2f);
                    pixels[y * width + x] = color;
                }
            }
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        /// <summary>White disc with a soft edge between inner and outer radius (as a fraction of the size).</summary>
        private static Texture2D Disc(int size, float inner, float outer)
        {
            var pixels = new Color[size * size];
            float centre = (size - 1) / 2f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Mathf.Sqrt((x - centre) * (x - centre) + (y - centre) * (y - centre)) / size;
                    float alpha = inner > 0f ? 1f - Smooth(Mathf.InverseLerp(inner, outer, d)) : Mathf.Pow(Mathf.Clamp01(1f - d / outer), 2.2f);
                    pixels[y * size + x] = new Color(1, 1, 1, alpha);
                }
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private static Texture2D HorizontalFade()
        {
            var texture = new Texture2D(64, 1, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int x = 0; x < 64; x++) texture.SetPixel(x, 0, new Color(0.01f, 0.01f, 0.02f, 0.78f * (1f - Smooth(x / 63f))));
            texture.Apply();
            return texture;
        }

        private static float Smooth(float t) { t = Mathf.Clamp01(t); return t * t * (3f - 2f * t); }
    }
}
