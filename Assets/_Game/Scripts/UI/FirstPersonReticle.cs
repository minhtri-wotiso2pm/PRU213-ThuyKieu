using ThuyKieu.Gameplay.Stealth;
using ThuyKieu.Interaction;
using UnityEngine;
using UnityEngine.UI;

namespace ThuyKieu.UI
{
    /// <summary>
    /// First-person overlay: a small dot that brightens over something usable, a ring that fills while E is held, and a
    /// soft red glow on the screen edge towards anyone growing aware of Kiều (the stronger, the surer they are). No
    /// numbers or bars: the danger reads as a feeling at the edge of vision. Builds its own canvas.
    /// </summary>
    public class FirstPersonReticle : MonoBehaviour
    {
        [SerializeField] private PlayerInteraction _interaction;
        [SerializeField] private Camera _camera;

        private Image _dot, _ring;
        private Image[] _edges;
        private Watchman[] _watchers = new Watchman[0];
        private float _refresh;
        private static Sprite _ringSprite, _dotSprite, _glow;

        private void Awake()
        {
            var canvasObject = new GameObject("ReticleCanvas", typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            _ringSprite ??= MakeCircle(64, 1);
            _dotSprite ??= MakeCircle(32, 0);
            _glow ??= MakeCircle(128, 2);

            _edges = new Image[4];
            for (int i = 0; i < 4; i++)
            {
                var edge = Create(canvas.transform, "ThreatEdge" + i, _glow, new Color(0.75f, 0.08f, 0.05f, 0f));
                var rect = edge.rectTransform;
                // Left, right, top, bottom: a large soft disc mostly off screen, so only its rim shows.
                Vector2 anchor = i == 0 ? new Vector2(0f, 0.5f) : i == 1 ? new Vector2(1f, 0.5f) : i == 2 ? new Vector2(0.5f, 1f) : new Vector2(0.5f, 0f);
                rect.anchorMin = rect.anchorMax = anchor;
                rect.sizeDelta = i < 2 ? new Vector2(900, 1500) : new Vector2(2200, 700);
                _edges[i] = edge;
            }
            _ring = Create(canvas.transform, "HoldRing", _ringSprite, new Color(0.92f, 0.78f, 0.5f, 0.9f));
            _ring.type = Image.Type.Filled;
            _ring.fillMethod = Image.FillMethod.Radial360;
            _ring.fillOrigin = (int)Image.Origin360.Top;
            _ring.rectTransform.sizeDelta = new Vector2(46, 46);
            _dot = Create(canvas.transform, "Dot", _dotSprite, new Color(1f, 0.95f, 0.85f, 0.35f));
            _dot.rectTransform.sizeDelta = new Vector2(7, 7);
        }

        private static Image Create(Transform parent, string name, Sprite sprite, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private void Update()
        {
            bool target = _interaction != null && _interaction.CurrentTarget != null;
            float hold = _interaction != null ? _interaction.HoldProgress : -1f;
            _dot.color = new Color(1f, 0.92f, 0.78f, target ? 0.95f : 0.32f);
            _dot.rectTransform.sizeDelta = Vector2.one * (target ? 10f : 6f);
            _ring.enabled = hold >= 0f;
            if (hold >= 0f) _ring.fillAmount = Mathf.Clamp01(hold);

            if ((_refresh -= Time.unscaledDeltaTime) <= 0f) { _refresh = 1f; _watchers = FindObjectsByType<Watchman>(); }
            var strength = new float[4];
            if (_camera != null)
                foreach (var watcher in _watchers)
                {
                    if (watcher == null || !watcher.isActiveAndEnabled || watcher.Awareness < 0.08f) continue;
                    Vector3 local = _camera.transform.InverseTransformPoint(watcher.Eyes.position);
                    float amount = Mathf.InverseLerp(0.08f, 1f, watcher.Awareness);
                    // In front of the eyes the glow sits low and faint (you can see him); behind, it rides the side he is on.
                    if (local.z > 0f && Mathf.Abs(local.x) < local.z * 0.7f) strength[3] = Mathf.Max(strength[3], amount * 0.6f);
                    else if (local.x < 0f) strength[0] = Mathf.Max(strength[0], amount);
                    else strength[1] = Mathf.Max(strength[1], amount);
                    if (local.z < 0f) strength[3] = Mathf.Max(strength[3], amount * 0.5f);
                }
            for (int i = 0; i < 4; i++)
            {
                var color = _edges[i].color;
                color.a = Mathf.MoveTowards(color.a, strength[i] * 0.55f, Time.unscaledDeltaTime * 1.5f);
                _edges[i].color = color;
            }
        }

        /// <summary>0 = filled disc, 1 = thin ring, 2 = soft glow.</summary>
        private static Sprite MakeCircle(int size, int kind)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            float r = size * 0.5f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r)) / r;
                    float a = kind == 2 ? Mathf.Clamp01(1f - d) * Mathf.Clamp01(1f - d) : kind == 1 ? (d > 0.82f && d < 1f ? 1f : 0f) : Mathf.Clamp01((1f - d) * size * 0.5f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }
    }
}
