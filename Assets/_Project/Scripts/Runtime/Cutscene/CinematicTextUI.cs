using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThuyKieu.Cutscene
{
    /// <summary>
    /// On-screen text for cinematics: game title, chapter title card, typewriter subtitles
    /// with a coloured speaker nameplate, location caption and letterbox bars.
    /// Builds its own overlay canvas; knows nothing about cameras or the story.
    /// </summary>
    public class CinematicTextUI : MonoBehaviour
    {
        [Header("Fonts (need Vietnamese glyphs)")]
        [SerializeField] private Font regularFont;
        [SerializeField] private Font boldFont;
        [SerializeField] private Font italicFont;

        [Header("Style")]
        [SerializeField] private Color gold = new Color(0.86f, 0.72f, 0.45f);
        [SerializeField] private Color paper = new Color(0.95f, 0.91f, 0.82f);
        [SerializeField] private Color cinnabar = new Color(0.72f, 0.18f, 0.14f);
        [Tooltip("Seconds per character for subtitles.")]
        [SerializeField] private float typewriterSpeed = 0.035f;
        [SerializeField] private int sortingOrder = 600;

        private TMP_FontAsset regular;
        private TMP_FontAsset bold;
        private TMP_FontAsset italic;
        private RectTransform root;
        private RectTransform barTop;
        private RectTransform barBottom;
        private Coroutine subtitleRoutine;
        private CanvasGroup subtitleGroup;
        private TextMeshProUGUI subtitleSpeaker;
        private TextMeshProUGUI subtitleLine;

        // ---------------------------------------------------------------- public API

        /// <summary>Studio line, then the game title with a slow tracking-in, then the tagline.</summary>
        public IEnumerator ShowGameTitle(string presenter, string title, string tagline, float hold)
        {
            EnsureBuilt();
            RectTransform group = Group("Title");
            CanvasGroup cg = group.gameObject.AddComponent<CanvasGroup>();

            TextMeshProUGUI pre = Label(group, presenter, regular, 26f, paper, new Vector2(0.5f, 0.5f), Vector2.zero);
            pre.characterSpacing = 18f;
            yield return FadeGroupIn(pre, 0.9f);
            yield return Wait(1.4f);
            yield return FadeGroupOut(pre, 0.7f);
            Destroy(pre.gameObject);

            TextMeshProUGUI main = Label(group, title, bold, 110f, gold, new Vector2(0.5f, 0.5f), new Vector2(0f, 30f));
            main.fontStyle = FontStyles.UpperCase;
            TextMeshProUGUI rule = Label(group, "———  ◆  ———", regular, 22f, cinnabar, new Vector2(0.5f, 0.5f), new Vector2(0f, -45f));
            TextMeshProUGUI sub = Label(group, tagline, italic, 34f, paper, new Vector2(0.5f, 0.5f), new Vector2(0f, -95f));

            cg.alpha = 0f;
            for (float t = 0f; t < hold; t += Time.deltaTime)
            {
                float k = t / hold;
                cg.alpha = Mathf.Min(Mathf.SmoothStep(0f, 1f, k * 3f), Mathf.SmoothStep(0f, 1f, (1f - k) * 4f));
                main.characterSpacing = Mathf.Lerp(40f, 14f, Mathf.SmoothStep(0f, 1f, k));
                sub.alpha = Mathf.SmoothStep(0f, 1f, (k - 0.25f) * 4f);
                rule.alpha = sub.alpha;
                yield return null;
            }

            Destroy(group.gameObject);
        }

        /// <summary>"CHƯƠNG I" small caps above the chapter name, with a red seal line.</summary>
        public IEnumerator ShowChapterCard(string chapterLabel, string chapterTitle, float hold)
        {
            EnsureBuilt();
            RectTransform group = Group("ChapterCard");
            CanvasGroup cg = group.gameObject.AddComponent<CanvasGroup>();

            TextMeshProUGUI label = Label(group, chapterLabel, regular, 28f, cinnabar, new Vector2(0.5f, 0.5f), new Vector2(0f, 55f));
            label.characterSpacing = 30f;
            label.fontStyle = FontStyles.UpperCase;
            TextMeshProUGUI title = Label(group, chapterTitle, bold, 72f, paper, new Vector2(0.5f, 0.5f), new Vector2(0f, -10f));
            Image line = Line(group, new Vector2(0f, -70f), gold);

            for (float t = 0f; t < hold; t += Time.deltaTime)
            {
                float k = t / hold;
                cg.alpha = Mathf.Min(Mathf.SmoothStep(0f, 1f, k * 4f), Mathf.SmoothStep(0f, 1f, (1f - k) * 4f));
                line.rectTransform.sizeDelta = new Vector2(Mathf.Lerp(0f, 420f, Mathf.SmoothStep(0f, 1f, k * 2.5f)), 2f);
                title.rectTransform.anchoredPosition = new Vector2(0f, Mathf.Lerp(-22f, -10f, Mathf.SmoothStep(0f, 1f, k * 3f)));
                yield return null;
            }

            Destroy(group.gameObject);
        }

        /// <summary>Starts a typewriter subtitle and returns immediately. A new line replaces the old one.</summary>
        public void Say(string speaker, Color speakerColor, string text, float holdAfter = 2.2f, bool innerThought = false)
        {
            EnsureBuilt();
            if (subtitleRoutine != null)
            {
                StopCoroutine(subtitleRoutine);
            }

            subtitleRoutine = StartCoroutine(SayRoutine(speaker, speakerColor, text, holdAfter, innerThought));
        }

        /// <summary>Bottom-left location caption: place name with a gold rule and a small detail line.</summary>
        public IEnumerator ShowLocation(string place, string detail, float hold)
        {
            EnsureBuilt();
            RectTransform group = Group("Location");
            CanvasGroup cg = group.gameObject.AddComponent<CanvasGroup>();

            TextMeshProUGUI name = Label(group, place, bold, 40f, paper, new Vector2(0f, 0f), new Vector2(90f, 205f));
            name.alignment = TextAlignmentOptions.BottomLeft;
            name.characterSpacing = 10f;
            name.fontStyle = FontStyles.UpperCase;
            Image line = Line(group, new Vector2(90f, 190f), gold);
            line.rectTransform.anchorMin = line.rectTransform.anchorMax = Vector2.zero;
            line.rectTransform.pivot = new Vector2(0f, 0.5f);
            TextMeshProUGUI info = Label(group, detail, italic, 24f, gold, new Vector2(0f, 0f), new Vector2(90f, 150f));
            info.alignment = TextAlignmentOptions.BottomLeft;

            for (float t = 0f; t < hold; t += Time.deltaTime)
            {
                float k = t / hold;
                cg.alpha = Mathf.Min(Mathf.SmoothStep(0f, 1f, k * 5f), Mathf.SmoothStep(0f, 1f, (1f - k) * 4f));
                line.rectTransform.sizeDelta = new Vector2(Mathf.Lerp(0f, 320f, Mathf.SmoothStep(0f, 1f, k * 3f)), 2f);
                name.rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(70f, 90f, Mathf.SmoothStep(0f, 1f, k * 3f)), 205f);
                yield return null;
            }

            Destroy(group.gameObject);
        }

        /// <summary>Cinematic black bars. amount 0 = none, 1 = 2.39:1 framing.</summary>
        public IEnumerator Letterbox(float to, float duration)
        {
            EnsureBuilt();
            float from = barTop.anchorMin.y < 1f ? (1f - barTop.anchorMin.y) / 0.12f : 0f;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                SetBars(Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / duration)));
                yield return null;
            }

            SetBars(to);
        }

        public void HideAll()
        {
            StopAllCoroutines();
            subtitleRoutine = null;
            if (root != null)
            {
                Destroy(root.gameObject);
                root = null;
            }
        }

        // ---------------------------------------------------------------- internals

        private IEnumerator SayRoutine(string speaker, Color speakerColor, string text, float holdAfter, bool innerThought)
        {
            subtitleSpeaker.text = speaker;
            subtitleSpeaker.color = speakerColor;
            subtitleSpeaker.gameObject.SetActive(!string.IsNullOrEmpty(speaker));
            subtitleLine.font = innerThought ? italic : regular;
            subtitleLine.color = innerThought ? new Color(paper.r, paper.g, paper.b, 0.85f) : paper;
            subtitleLine.text = text;
            subtitleLine.maxVisibleCharacters = 0;

            for (float t = 0f; t < 0.25f; t += Time.deltaTime)
            {
                subtitleGroup.alpha = Mathf.Max(subtitleGroup.alpha, t / 0.25f);
                yield return null;
            }

            subtitleGroup.alpha = 1f;
            subtitleLine.ForceMeshUpdate();
            int total = subtitleLine.textInfo.characterCount;
            for (float t = 0f; subtitleLine.maxVisibleCharacters < total; t += Time.deltaTime)
            {
                subtitleLine.maxVisibleCharacters = Mathf.Min(total, Mathf.FloorToInt(t / typewriterSpeed));
                yield return null;
            }

            yield return Wait(holdAfter);
            for (float t = 0f; t < 0.4f; t += Time.deltaTime)
            {
                subtitleGroup.alpha = 1f - t / 0.4f;
                yield return null;
            }

            subtitleGroup.alpha = 0f;
            subtitleRoutine = null;
        }

        private void EnsureBuilt()
        {
            if (root != null)
            {
                return;
            }

            regular = CreateFont(regularFont);
            bold = boldFont != null ? CreateFont(boldFont) : regular;
            italic = italicFont != null ? CreateFont(italicFont) : regular;

            GameObject canvasGo = new GameObject("CinematicText_Canvas", typeof(Canvas), typeof(CanvasScaler));
            canvasGo.transform.SetParent(transform, false);
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            root = (RectTransform)canvasGo.transform;

            barTop = Bar("LetterboxTop", true);
            barBottom = Bar("LetterboxBottom", false);
            SetBars(0f);

            RectTransform sub = Group("Subtitle");
            subtitleGroup = sub.gameObject.AddComponent<CanvasGroup>();
            subtitleGroup.alpha = 0f;
            subtitleSpeaker = Label(sub, "", bold, 30f, gold, new Vector2(0.5f, 0f), new Vector2(0f, 165f));
            subtitleSpeaker.characterSpacing = 6f;
            subtitleLine = Label(sub, "", regular, 36f, paper, new Vector2(0.5f, 0f), new Vector2(0f, 105f));
            subtitleLine.rectTransform.sizeDelta = new Vector2(1400f, 110f);
            subtitleLine.alignment = TextAlignmentOptions.Top;
            subtitleLine.textWrappingMode = TextWrappingModes.Normal;
        }

        private static TMP_FontAsset CreateFont(Font font)
        {
            TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(font, 90, 9, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024);
            asset.name = font.name + "_Runtime SDF";
            return asset;
        }

        private RectTransform Group(string objectName)
        {
            GameObject go = new GameObject(objectName, typeof(RectTransform));
            RectTransform rect = (RectTransform)go.transform;
            rect.SetParent(root, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        private TextMeshProUGUI Label(RectTransform parent, string text, TMP_FontAsset font, float size, Color color, Vector2 anchor, Vector2 position)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(anchor.x, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(1600f, size * 1.6f);

            TextMeshProUGUI label = go.GetComponent<TextMeshProUGUI>();
            label.font = font;
            label.fontSize = size;
            label.color = color;
            label.text = text;
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.raycastTarget = false;
            // Soft shadow so text stays readable over bright frames.
            label.fontMaterial.EnableKeyword("UNDERLAY_ON");
            label.fontMaterial.SetColor("_UnderlayColor", new Color(0f, 0f, 0f, 0.75f));
            label.fontMaterial.SetFloat("_UnderlaySoftness", 0.6f);
            label.fontMaterial.SetFloat("_UnderlayOffsetY", -0.6f);
            return label;
        }

        private Image Line(RectTransform parent, Vector2 position, Color color)
        {
            GameObject go = new GameObject("Rule", typeof(RectTransform), typeof(Image));
            RectTransform rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(0f, 2f);
            Image image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private RectTransform Bar(string objectName, bool top)
        {
            GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            RectTransform rect = (RectTransform)go.transform;
            rect.SetParent(root, false);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            Image image = go.GetComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = false;
            return rect;
        }

        private void SetBars(float amount)
        {
            float h = 0.12f * amount;
            barTop.anchorMin = new Vector2(0f, 1f - h);
            barTop.anchorMax = Vector2.one;
            barBottom.anchorMin = Vector2.zero;
            barBottom.anchorMax = new Vector2(1f, h);
        }

        private static IEnumerator FadeGroupIn(TextMeshProUGUI label, float duration)
        {
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                label.alpha = t / duration;
                yield return null;
            }

            label.alpha = 1f;
        }

        private static IEnumerator FadeGroupOut(TextMeshProUGUI label, float duration)
        {
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                label.alpha = 1f - t / duration;
                yield return null;
            }

            label.alpha = 0f;
        }

        private static IEnumerator Wait(float seconds)
        {
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                yield return null;
            }
        }
    }
}
