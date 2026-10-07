using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ThuyKieu.UI
{
    /// <summary>
    /// Kim Trọng's desk. Compare mode lays two documents side by side and asks where they contradict;
    /// summary mode shows the dossier's four kinds of evidence, filled or missing.
    /// </summary>
    public class DossierView : MonoBehaviour
    {
        public struct Comparison
        {
            public string LeftTitle, LeftBody, RightTitle, RightBody;
            public string Prompt;
            public string[] Options;
            public int Correct;
            /// <summary>Shown after a wrong pick: what the player should have noticed.</summary>
            public string Hint;
            /// <summary>Shown after a right pick: what it proves.</summary>
            public string Insight;
        }

        public struct Slot
        {
            public string Title;
            public bool Filled;
            public string Body;
        }

        private GameObject _root, _slots;
        private TMP_Text _title, _progress, _question, _verdict, _leftTitle, _leftBody, _rightTitle, _rightBody;
        private GameObject _left, _right;
        private RectTransform _options;
        private Button _template;
        private readonly List<Button> _buttons = new List<Button>();
        private int _picked = -1;

        /// <summary>QA autopilot answers on its own.</summary>
        public bool FastMode { get; set; }
        public bool IsOpen => _root != null && _root.activeSelf;

        private void Awake()
        {
            _root = transform.Find("Dossier").gameObject;
            _title = StoryUIBuilder.Find<TMP_Text>(transform, "Dossier/Panel/Title");
            _progress = StoryUIBuilder.Find<TMP_Text>(transform, "Dossier/Panel/Progress");
            _question = StoryUIBuilder.Find<TMP_Text>(transform, "Dossier/Panel/Question");
            _verdict = StoryUIBuilder.Find<TMP_Text>(transform, "Dossier/Panel/Verdict");
            _left = transform.Find("Dossier/Panel/Left").gameObject;
            _right = transform.Find("Dossier/Panel/Right").gameObject;
            _leftTitle = StoryUIBuilder.Find<TMP_Text>(transform, "Dossier/Panel/Left/Title");
            _leftBody = StoryUIBuilder.Find<TMP_Text>(transform, "Dossier/Panel/Left/Body");
            _rightTitle = StoryUIBuilder.Find<TMP_Text>(transform, "Dossier/Panel/Right/Title");
            _rightBody = StoryUIBuilder.Find<TMP_Text>(transform, "Dossier/Panel/Right/Body");
            _options = StoryUIBuilder.Find<RectTransform>(transform, "Dossier/Panel/Options");
            _template = StoryUIBuilder.Find<Button>(transform, "Dossier/Panel/Options/Template");
            _slots = transform.Find("Dossier/Panel/Slots").gameObject;
        }

        public IEnumerator Compare(string title, IList<Comparison> comparisons, Action<int> onScore)
        {
            _root.SetActive(true);
            _slots.SetActive(false);
            _left.SetActive(true);
            _right.SetActive(true);
            _title.text = title;
            _verdict.text = string.Empty;
            int score = 0;
            for (int c = 0; c < comparisons.Count; c++)
            {
                var item = comparisons[c];
                _progress.text = "Đối chiếu " + (c + 1) + " / " + comparisons.Count;
                _leftTitle.text = item.LeftTitle;
                _leftBody.text = item.LeftBody;
                _rightTitle.text = item.RightTitle;
                _rightBody.text = item.RightBody;
                _question.text = item.Prompt;
                Build(item.Options);
                yield return Pick(item.Options.Length, item.Correct);
                bool right = _picked == item.Correct;
                if (right) score++;
                Mark(item.Correct, _picked);
                _question.text = right ? "<color=#9BE39B>Khớp. </color>" + item.Insight : "<color=#F08080>Chưa đúng. </color>" + item.Hint;
                yield return new WaitForSecondsRealtime(FastMode ? ThuyKieu.Core.Story.StoryCapture.Hold * 0.5f : right ? 2.2f : 3f);
            }
            Build(new string[0]);
            _question.text = string.Empty;
            _progress.text = score + " / " + comparisons.Count + " chỗ mâu thuẫn tìm ra";
            _verdict.text = score == comparisons.Count ? "GIẤY TỜ TỰ TỐ CÁO CHÚNG." : score > 0 ? "ĐỦ THẤY CHÚNG GIAN — NHƯNG CẦN NGƯỜI LÀM CHỨNG." : "GIẤY TỜ RỐI QUÁ. PHẢI TÌM NGƯỜI SỐNG NÓI THAY.";
            yield return new WaitForSecondsRealtime(FastMode ? ThuyKieu.Core.Story.StoryCapture.Hold : 2.4f);
            _root.SetActive(false);
            onScore(score);
        }

        public IEnumerator Summary(string title, IList<Slot> slots, string verdict)
        {
            _root.SetActive(true);
            _left.SetActive(false);
            _right.SetActive(false);
            Build(new string[0]);
            _question.text = string.Empty;
            _slots.SetActive(true);
            _title.text = title;
            int filled = 0;
            for (int i = 0; i < 4; i++)
            {
                var slot = _slots.transform.Find("Slot" + i);
                bool show = i < slots.Count;
                slot.gameObject.SetActive(show);
                if (!show) continue;
                if (slots[i].Filled) filled++;
                StoryUIBuilder.Find<TMP_Text>(slot, "Title").text = slots[i].Title;
                var mark = StoryUIBuilder.Find<TMP_Text>(slot, "Mark");
                mark.text = slots[i].Filled ? "<size=50>ĐỦ</size>" : "<size=50>THIẾU</size>";
                mark.color = slots[i].Filled ? new Color(0.6f, 0.9f, 0.6f) : new Color(1, 1, 1, 0.3f);
                StoryUIBuilder.Find<TMP_Text>(slot, "Body").text = slots[i].Body;
                slot.GetComponent<Image>().color = slots[i].Filled ? new Color(0.25f, 0.4f, 0.22f, 0.35f) : new Color(1, 1, 1, 0.05f);
            }
            _progress.text = filled + " / " + slots.Count + " loại bằng chứng";
            _verdict.text = verdict;
            yield return new WaitForSecondsRealtime(FastMode ? ThuyKieu.Core.Story.StoryCapture.Hold : 1f);
            while (!FastMode && !Confirm()) yield return null;
            _slots.SetActive(false);
            _root.SetActive(false);
        }

        private IEnumerator Pick(int count, int correct)
        {
            _picked = -1;
            float shown = Time.realtimeSinceStartup;
            while (_picked < 0)
            {
                if (FastMode && Time.realtimeSinceStartup - shown >= ThuyKieu.Core.Story.StoryCapture.Hold)
                    _picked = UnityEngine.Random.value < 0.7f ? correct : UnityEngine.Random.Range(0, count);
                var keyboard = Keyboard.current;
                for (int i = 0; i < _buttons.Count && keyboard != null && !PauseMenuView.IsPaused; i++)
                    if (keyboard[Key.Digit1 + i].wasPressedThisFrame) _picked = i;
                yield return null;
            }
        }

        private void Build(string[] options)
        {
            foreach (var button in _buttons) Destroy(button.gameObject);
            _buttons.Clear();
            for (int i = 0; i < options.Length; i++)
            {
                int index = i;
                var button = Instantiate(_template, _options);
                button.gameObject.SetActive(true);
                button.GetComponentInChildren<TMP_Text>().text = "<color=#E8C27A>" + (i + 1) + "</color>   " + options[i];
                button.onClick.AddListener(() => { if (_picked < 0) _picked = index; });
                _buttons.Add(button);
            }
        }

        private void Mark(int correct, int picked)
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                var image = _buttons[i].GetComponent<Image>();
                if (i == correct) image.color = new Color(0.2f, 0.45f, 0.22f, 0.9f);
                else if (i == picked) image.color = new Color(0.5f, 0.12f, 0.1f, 0.9f);
                _buttons[i].interactable = false;
            }
        }

        private static bool Confirm()
        {
            var keyboard = Keyboard.current;
            return !PauseMenuView.IsPaused && keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.eKey.wasPressedThisFrame);
        }
    }
}
