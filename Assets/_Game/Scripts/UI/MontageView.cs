using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThuyKieu.UI
{
    /// <summary>
    /// Full-screen still-image montage, built in code: letterboxed stills that cut in on a white flash and drift slowly
    /// closer, with one caption line in the lower bar. It only shows things; MontageSequence owns the timing.
    /// </summary>
    public class MontageView : MonoBehaviour
    {
        private const float Push = 0.06f;

        private CanvasGroup _group;
        private RawImage _still;
        private AspectRatioFitter _fit;
        private Image _flash;
        private TMP_Text _caption;
        private Vector2 _drift;

        public static MontageView Create()
        {
            var view = new GameObject("Montage").AddComponent<MontageView>();
            view.Build();
            return view;
        }

        private void Build()
        {
            if (StoryUIBuilder.Font == null)
            {
                var anyText = FindAnyObjectByType<TMP_Text>();
                if (anyText != null) StoryUIBuilder.Font = anyText.font;
            }
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // Over the story HUD and its fades, under the screen effects and the pause menu.
            canvas.sortingOrder = 20;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            _group = gameObject.AddComponent<CanvasGroup>();
            _group.alpha = 0f;
            _group.blocksRaycasts = true;

            StoryUIBuilder.Image(StoryUIBuilder.Rect(transform, "Black", Vector2.zero, Vector2.one), Color.black);
            var frame = StoryUIBuilder.Rect(transform, "Frame", Vector2.zero, Vector2.one);
            frame.gameObject.AddComponent<RectMask2D>();
            var still = StoryUIBuilder.Rect(frame, "Still", Vector2.zero, Vector2.one);
            _still = still.gameObject.AddComponent<RawImage>();
            _still.raycastTarget = false;
            _still.enabled = false;
            _fit = still.gameObject.AddComponent<AspectRatioFitter>();
            _fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;

            var bar = new Color(0.02f, 0.015f, 0.02f, 1f);
            StoryUIBuilder.Image(StoryUIBuilder.Rect(transform, "BarTop", new Vector2(0f, 0.9f), Vector2.one), bar);
            StoryUIBuilder.Image(StoryUIBuilder.Rect(transform, "BarBottom", Vector2.zero, new Vector2(1f, 0.13f)), bar);
            _caption = StoryUIBuilder.Text(transform, "Caption", "", 34, new Vector2(0.08f, 0.015f), new Vector2(0.92f, 0.115f), new Color(0.97f, 0.9f, 0.76f));
            _caption.alignment = TextAlignmentOptions.Center;
            var hint = StoryUIBuilder.Text(transform, "Hint", "Space ›", 16, new Vector2(0.88f, 0.905f), new Vector2(0.99f, 0.995f), new Color(1f, 1f, 1f, 0.35f));
            hint.alignment = TextAlignmentOptions.MidlineRight;
            _flash = StoryUIBuilder.Image(StoryUIBuilder.Rect(transform, "Flash", Vector2.zero, Vector2.one), new Color(1f, 0.98f, 0.92f, 0f));
        }

        public void SetAlpha(float alpha) => _group.alpha = alpha;

        /// <summary>Cuts to the next still on a flash; each one drifts a different way.</summary>
        public void Show(Texture still, string caption, int index)
        {
            _still.texture = still;
            _still.enabled = still != null;
            if (still != null) _fit.aspectRatio = still.width / (float)Mathf.Max(1, still.height);
            _caption.text = caption;
            _caption.alpha = 0f;
            _drift = new Vector2(index % 2 == 0 ? -1f : 1f, index % 3 == 1 ? 0.6f : -0.4f) * 18f;
            _flash.color = new Color(_flash.color.r, _flash.color.g, _flash.color.b, 0.75f);
            Tick(0f, 0f);
        }

        /// <summary>Progress through the current still: seconds since it appeared and 0..1 of its hold.</summary>
        public void Tick(float seconds, float progress)
        {
            float k = Mathf.SmoothStep(0f, 1f, progress);
            var rect = _still.rectTransform;
            rect.localScale = Vector3.one * (1.02f + Push * k);
            rect.anchoredPosition = _drift * k;
            _flash.color = new Color(_flash.color.r, _flash.color.g, _flash.color.b, 0.75f * (1f - Mathf.Clamp01(seconds / 0.18f)));
            _caption.alpha = Mathf.Clamp01((seconds - 0.12f) / 0.3f);
        }
    }
}
