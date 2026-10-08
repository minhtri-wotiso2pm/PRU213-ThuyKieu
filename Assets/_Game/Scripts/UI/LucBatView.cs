using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThuyKieu.UI
{
    /// <summary>
    /// The lục bát exchange screen, built in code (no prefab): the six-word line, the eight-word line being built (with
    /// bằng/trắc marks under positions 2-4-6-8), up to four answer buttons, a feedback line and an optional timer.
    /// It only shows things and reports clicks; LucBatSequence owns the rules.
    /// </summary>
    public class LucBatView : MonoBehaviour
    {
        private TMP_Text _title, _luc, _bat, _feedback;
        private RectTransform _options, _timerFill;
        private GameObject _timer;
        private readonly List<Button> _buttons = new List<Button>();

        /// <summary>Answer clicked (index), or -1.</summary>
        public int Clicked { get; set; } = -1;

        public static LucBatView Create()
        {
            var view = new GameObject("LucBat").AddComponent<LucBatView>();
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
            canvas.sortingOrder = 12;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            var dim = StoryUIBuilder.Image(StoryUIBuilder.Rect(transform, "Dim", Vector2.zero, Vector2.one), new Color(0, 0, 0, 0.45f));
            dim.raycastTarget = true;
            var panel = StoryUIBuilder.Panel(transform, "Panel", new Vector2(0.16f, 0.1f), new Vector2(0.84f, 0.9f), StoryUIBuilder.Paper, 18);
            var ink = new Color(0.18f, 0.1f, 0.05f);
            _title = StoryUIBuilder.Text(panel, "Title", "", 22, new Vector2(0.04f, 0.9f), new Vector2(0.96f, 0.97f), new Color(0.55f, 0.2f, 0.08f));
            _title.alignment = TextAlignmentOptions.Center;
            var rules = StoryUIBuilder.Text(panel, "Rules", "Lục bát: câu sáu chữ 2-4-6 là <b>B-T-B</b> · câu tám chữ 2-4-6-8 là <b>B-T-B-B</b> · " +
                "chữ thứ sáu câu tám <b>vần</b> với chữ cuối câu sáu.  (B: thanh ngang, huyền · T: sắc, hỏi, ngã, nặng)", 16,
                new Vector2(0.05f, 0.8f), new Vector2(0.95f, 0.9f), new Color(0.35f, 0.2f, 0.1f));
            rules.alignment = TextAlignmentOptions.Center;
            _luc = StoryUIBuilder.Text(panel, "Luc", "", 34, new Vector2(0.05f, 0.65f), new Vector2(0.95f, 0.79f), ink);
            _luc.alignment = TextAlignmentOptions.Center;
            _bat = StoryUIBuilder.Text(panel, "Bat", "", 34, new Vector2(0.05f, 0.5f), new Vector2(0.95f, 0.65f), ink);
            _bat.alignment = TextAlignmentOptions.Center;
            _options = StoryUIBuilder.Rect(panel, "Options", new Vector2(0.06f, 0.14f), new Vector2(0.94f, 0.48f));
            var layout = _options.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10;
            layout.childForceExpandHeight = false;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            _feedback = StoryUIBuilder.Text(panel, "Feedback", "", 20, new Vector2(0.05f, 0.04f), new Vector2(0.95f, 0.13f), new Color(0.5f, 0.15f, 0.08f));
            _feedback.alignment = TextAlignmentOptions.Center;
            var timer = StoryUIBuilder.Panel(panel, "Timer", new Vector2(0.05f, 0.975f), new Vector2(0.95f, 0.99f), new Color(0, 0, 0, 0.15f), 3);
            _timer = timer.gameObject;
            _timerFill = StoryUIBuilder.Rect(timer, "Fill", Vector2.zero, Vector2.one);
            StoryUIBuilder.Image(_timerFill, StoryUIBuilder.Vermilion);
            _timer.SetActive(false);
        }

        public void SetTitle(string text) => _title.text = text;
        public void SetLuc(string text) => _luc.text = text + "\n<size=55%><color=#8A5A2A>" + Marks(6) + "</color></size>";
        public void SetBat(string text) => _bat.text = text + "\n<size=55%><color=#8A5A2A>" + Marks(8) + "</color></size>";
        public void SetFeedback(string text) => _feedback.text = text;

        /// <summary>Remaining time as 0..1, or a negative value to hide the bar.</summary>
        public void SetTimer(float remaining)
        {
            _timer.SetActive(remaining >= 0f);
            if (remaining >= 0f) _timerFill.anchorMax = new Vector2(Mathf.Clamp01(remaining), 1f);
        }

        /// <summary>Replaces the answers; dimmed ones are greyed out and cannot be picked.</summary>
        public void ShowOptions(IList<string> labels, IList<bool> dimmed)
        {
            foreach (var button in _buttons) Destroy(button.gameObject);
            _buttons.Clear();
            Clicked = -1;
            for (int i = 0; i < labels.Count; i++)
            {
                int index = i;
                bool off = dimmed != null && i < dimmed.Count && dimmed[i];
                var rect = StoryUIBuilder.Panel(_options, "Option" + i, Vector2.zero, Vector2.one, off ? new Color(0, 0, 0, 0.06f) : new Color(0.36f, 0.22f, 0.1f, 0.9f), 10);
                rect.gameObject.AddComponent<LayoutElement>().preferredHeight = labels.Count > 3 ? 58 : 76;
                var label = StoryUIBuilder.Text(rect, "Label", "<color=#E8C27A>" + (i + 1) + "</color>   " + labels[i], 24, Vector2.zero, Vector2.one, off ? new Color(0.3f, 0.25f, 0.2f, 0.5f) : Color.white);
                label.margin = new Vector4(18, 4, 18, 4);
                var button = rect.gameObject.AddComponent<Button>();
                button.interactable = !off;
                button.onClick.AddListener(() => Clicked = index);
                _buttons.Add(button);
            }
        }

        public void Close() => Destroy(gameObject);

        /// <summary>The bằng/trắc pattern under a line: "· B · T · B" style marks on the even positions.</summary>
        private static string Marks(int words)
        {
            string[] pattern = words == 6 ? new[] { "B", "T", "B" } : new[] { "B", "T", "B", "B" };
            var parts = new List<string>();
            for (int i = 1; i <= words; i++) parts.Add(i % 2 == 0 ? pattern[i / 2 - 1] : "·");
            return string.Join("    ", parts);
        }
    }
}
