using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThuyKieu.UI
{
    /// <summary>
    /// The free-play strip under the hồ cầm, built in code: one key per string with its ngũ cung name, which lights up
    /// when plucked, a hint line and a feedback line. It only shows things; LuteSequence owns the playing.
    /// </summary>
    public class LuteView : MonoBehaviour
    {
        private static readonly Color Idle = new Color(0.08f, 0.06f, 0.05f, 0.72f);
        private static readonly Color Lit = new Color(0.86f, 0.62f, 0.3f, 0.95f);

        private readonly List<Image> _strings = new List<Image>();
        private readonly List<float> _glow = new List<float>();
        private TMP_Text _feedback;

        public static LuteView Create(IList<string> keys, IList<string> names, string hint)
        {
            var view = new GameObject("LuteFreePlay").AddComponent<LuteView>();
            view.Build(keys, names, hint);
            return view;
        }

        private void Build(IList<string> keys, IList<string> names, string hint)
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

            var title = StoryUIBuilder.Text(transform, "Title", "TỰ DO GẢY ĐÀN", 22, new Vector2(0.3f, 0.255f), new Vector2(0.7f, 0.3f), StoryUIBuilder.Gold);
            title.alignment = TextAlignmentOptions.Center;
            float width = 0.5f / keys.Count;
            for (int i = 0; i < keys.Count; i++)
            {
                var key = StoryUIBuilder.Panel(transform, "String" + i, new Vector2(0.25f + i * width + 0.004f, 0.13f),
                    new Vector2(0.25f + (i + 1) * width - 0.004f, 0.25f), Idle, 10);
                _strings.Add(key.GetComponent<Image>());
                _glow.Add(0f);
                var label = StoryUIBuilder.Text(key, "Label", "<size=150%><b>" + keys[i] + "</b></size>\n" + names[i], 22, Vector2.zero, Vector2.one, Color.white);
                label.alignment = TextAlignmentOptions.Center;
            }
            var hintText = StoryUIBuilder.Text(transform, "Hint", hint, 18, new Vector2(0.2f, 0.085f), new Vector2(0.8f, 0.125f), new Color(1f, 1f, 1f, 0.75f));
            hintText.alignment = TextAlignmentOptions.Center;
            _feedback = StoryUIBuilder.Text(transform, "Feedback", "", 22, new Vector2(0.2f, 0.3f), new Vector2(0.8f, 0.35f), new Color(1f, 0.92f, 0.78f));
            _feedback.alignment = TextAlignmentOptions.Center;
        }

        public void Pluck(int index) { if (index >= 0 && index < _glow.Count) _glow[index] = 1f; }

        public void SetFeedback(string text) => _feedback.text = text;

        private void Update()
        {
            for (int i = 0; i < _strings.Count; i++)
            {
                _glow[i] = Mathf.Max(0f, _glow[i] - Time.unscaledDeltaTime * 2.5f);
                _strings[i].color = Color.Lerp(Idle, Lit, _glow[i]);
                _strings[i].rectTransform.localScale = Vector3.one * (1f + 0.06f * _glow[i]);
            }
        }
    }
}
