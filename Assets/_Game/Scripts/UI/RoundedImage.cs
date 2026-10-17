using UnityEngine;
using UnityEngine.UI;

namespace ThuyKieu.UI
{
    /// <summary>Gives a plain uGUI Image a generated rounded, 9-sliced sprite at runtime (no texture asset needed).</summary>
    [RequireComponent(typeof(Image))]
    public class RoundedImage : MonoBehaviour
    {
        [SerializeField] private int _radius = 14;
        private static Sprite _small, _large;

        private void Awake()
        {
            var image = GetComponent<Image>();
            image.sprite = _radius > 20 ? (_large != null ? _large : _large = Build(26)) : (_small != null ? _small : _small = Build(14));
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 1f;
        }

        public void SetRadius(int radius) { _radius = radius; }

        private static Sprite Build(int radius)
        {
            int size = radius * 2 + 4;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var pixels = new Color32[size * size];
            float centre = (size - 1) / 2f;
            float inner = centre - radius;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Max(0, Mathf.Abs(x - centre) - inner);
                    float dy = Mathf.Max(0, Mathf.Abs(y - centre) - inner);
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    byte alpha = (byte)(Mathf.Clamp01(radius - distance + 0.5f) * 255);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            texture.SetPixels32(pixels);
            texture.Apply();
            float border = radius + 1;
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
        }
    }
}
