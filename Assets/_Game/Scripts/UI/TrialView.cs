using System;
using System.Collections;
using System.Collections.Generic;
using ThuyKieu.Dialogue;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThuyKieu.UI
{
    /// <summary>
    /// Courtroom screen for Chapter 7 (built by StoryUIBuilder.BuildTrial). It only shows things and reports clicks;
    /// the rules (what can be pressed, which evidence fits which statement, trust) live in TrialSequence.
    /// </summary>
    public class TrialView : MonoBehaviour
    {
        public struct Row
        {
            public string Speaker;
            public string Text;
            /// <summary>Already shown to be false (struck through, cannot be used again).</summary>
            public bool Disproved;
            /// <summary>Pressing changed what the witness says.</summary>
            public bool Revised;
        }

        public struct Card
        {
            public string Title;
            public string Body;
        }

        private GameObject _root, _stamp, _press, _present, _evidence;
        private TMP_Text _title, _subtitle, _actionHeader, _help, _lineName, _lineText, _stampText;
        private RectTransform _trustFill, _rows;
        private Image _trustImage;
        private Button _rowTemplate, _cardTemplate;
        private readonly List<Button> _rowButtons = new List<Button>();
        private readonly List<Button> _cardButtons = new List<Button>();

        /// <summary>Statement clicked (index).</summary>
        public int ClickedRow { get; set; } = -1;
        /// <summary>Evidence card clicked (index).</summary>
        public int ClickedCard { get; set; } = -1;
        public bool PressClicked { get; set; }
        public bool PresentClicked { get; set; }
        public bool IsOpen => _root != null && _root.activeSelf;
        public bool EvidenceOpen => _evidence != null && _evidence.activeSelf;

        private void Awake()
        {
            var trial = transform.Find("Trial");
            if (trial == null) return;
            _root = trial.gameObject;
            _title = StoryUIBuilder.Find<TMP_Text>(trial, "Top/Title");
            _subtitle = StoryUIBuilder.Find<TMP_Text>(trial, "Top/Subtitle");
            _trustFill = StoryUIBuilder.Find<RectTransform>(trial, "Top/Track/Fill");
            _trustImage = _trustFill.GetComponent<Image>();
            _rows = StoryUIBuilder.Find<RectTransform>(trial, "Testimony/Rows");
            _rowTemplate = StoryUIBuilder.Find<Button>(trial, "Testimony/Rows/Template");
            _actionHeader = StoryUIBuilder.Find<TMP_Text>(trial, "Actions/Header");
            _help = StoryUIBuilder.Find<TMP_Text>(trial, "Actions/Help");
            _press = trial.Find("Actions/Press").gameObject;
            _present = trial.Find("Actions/Present").gameObject;
            _evidence = trial.Find("Actions/Evidence").gameObject;
            _cardTemplate = StoryUIBuilder.Find<Button>(trial, "Actions/Evidence/Template");
            _lineName = StoryUIBuilder.Find<TMP_Text>(trial, "Line/Name");
            _lineText = StoryUIBuilder.Find<TMP_Text>(trial, "Line/Text");
            _stamp = trial.Find("Stamp").gameObject;
            _stampText = _stamp.GetComponent<TMP_Text>();
            _press.GetComponent<Button>().onClick.AddListener(() => PressClicked = true);
            _present.GetComponent<Button>().onClick.AddListener(() => PresentClicked = true);
        }

        public void Open(string title, string subtitle)
        {
            _root.SetActive(true);
            _stamp.SetActive(false);
            _title.text = title;
            _subtitle.text = subtitle;
            ShowLine(null, string.Empty);
            ShowActions(-1, false);
        }

        public void Close() { if (_root != null) _root.SetActive(false); }

        public void SetTrust(float value)
        {
            _trustFill.anchorMax = new Vector2(Mathf.Clamp01(value), 1f);
            _trustImage.color = value > 0.5f ? new Color(0.62f, 0.86f, 0.58f) : value > 0.25f ? StoryUIBuilder.Gold : new Color(0.9f, 0.3f, 0.25f);
        }

        public void SetSubtitle(string text) => _subtitle.text = text;

        /// <summary>Rebuilds the testimony list with the selected row highlighted.</summary>
        public void ShowRows(IList<Row> rows, int selected)
        {
            foreach (var button in _rowButtons) Destroy(button.gameObject);
            _rowButtons.Clear();
            for (int i = 0; i < rows.Count; i++)
            {
                int index = i;
                var button = Instantiate(_rowTemplate, _rows);
                button.gameObject.SetActive(true);
                var entry = SpeakerCatalog.Get(rows[i].Speaker);
                string text = rows[i].Disproved ? "<s><color=#FFFFFF66>" + rows[i].Text + "</color></s>  <size=80%><color=#9BE39B>— đã bác bỏ</color></size>" : rows[i].Text;
                if (rows[i].Revised && !rows[i].Disproved) text += "  <size=80%><color=#E8C27A>(đã sửa lời)</color></size>";
                button.GetComponentInChildren<TMP_Text>().text = "<color=#E8C27A>" + (i + 1) + "</color>  <size=80%><color=#" + ColorUtility.ToHtmlStringRGB(entry.Color) + ">" +
                                                                 entry.DisplayName + "</color></size>\n" + text;
                button.GetComponent<Image>().color = i == selected ? new Color(0.36f, 0.22f, 0.1f, 0.95f) : new Color(1, 1, 1, 0.05f);
                button.onClick.AddListener(() => ClickedRow = index);
                _rowButtons.Add(button);
            }
        }

        /// <summary>Right panel in action mode, describing the selected statement.</summary>
        public void ShowActions(int selected, bool disproved)
        {
            _evidence.SetActive(false);
            _press.SetActive(selected >= 0 && !disproved);
            _present.SetActive(selected >= 0 && !disproved);
            _actionHeader.text = selected < 0 ? "CHỌN MỘT LỜI KHAI" : "LỜI KHAI SỐ " + (selected + 1);
            _help.text = selected < 0
                ? "Đọc kỹ từng câu. Câu nào nghe có lý nhưng trái với giấy tờ hay nhân chứng của bạn?\n\n<color=#E8C27A>1–4 / ↑↓</color> chọn lời khai"
                : disproved ? "Câu này đã bị bác bỏ. Hãy xét những câu khác."
                : "<color=#E8C27A>Hỏi vặn</color> để người khai nói thêm — đôi khi họ tự lộ ra sơ hở.\n<color=#E8C27A>Đưa bằng chứng</color> khi bạn chắc chắn câu này sai. Đưa sai, quan huyện sẽ mất kiên nhẫn.";
        }

        /// <summary>Right panel in evidence mode: one card per item the player can present.</summary>
        public void ShowEvidence(IList<Card> cards)
        {
            _press.SetActive(false);
            _present.SetActive(false);
            _evidence.SetActive(true);
            _actionHeader.text = "ĐƯA BẰNG CHỨNG NÀO?";
            _help.text = string.Empty;
            foreach (var button in _cardButtons) Destroy(button.gameObject);
            _cardButtons.Clear();
            for (int i = 0; i < cards.Count; i++)
            {
                int index = i;
                var button = Instantiate(_cardTemplate, _evidence.transform);
                button.gameObject.SetActive(true);
                StoryUIBuilder.Find<TMP_Text>(button.transform, "Title").text = (i < 9 ? "<color=#8A3A1A>" + (i + 1) + "</color>  " : "") + cards[i].Title;
                StoryUIBuilder.Find<TMP_Text>(button.transform, "Body").text = cards[i].Body;
                button.onClick.AddListener(() => ClickedCard = index);
                _cardButtons.Add(button);
            }
        }

        public void ShowLine(string speakerId, string text)
        {
            if (string.IsNullOrEmpty(speakerId)) { _lineName.text = string.Empty; _lineText.text = text; return; }
            var entry = SpeakerCatalog.Get(speakerId);
            _lineName.text = entry.Style == SpeakerStyle.Inner ? string.Empty : entry.DisplayName;
            _lineName.color = entry.Color;
            _lineText.text = entry.Style == SpeakerStyle.Inner ? "<i><color=#D2E6FF>" + text + "</color></i>" : text;
        }

        /// <summary>Big slanted word over the hall ("MÂU THUẪN!") that punches in and fades.</summary>
        public IEnumerator Stamp(string text, Color color, float seconds)
        {
            _stampText.text = text;
            _stampText.color = color;
            _stamp.SetActive(true);
            var rect = (RectTransform)_stamp.transform;
            for (float t = 0; t < seconds; t += Time.unscaledDeltaTime)
            {
                float k = t / seconds;
                rect.localScale = Vector3.one * Mathf.Lerp(1.6f, 1f, Mathf.Clamp01(k * 6f));
                var c = color;
                c.a = k < 0.75f ? 1f : 1f - (k - 0.75f) / 0.25f;
                _stampText.color = c;
                yield return null;
            }
            _stamp.SetActive(false);
        }
    }
}
