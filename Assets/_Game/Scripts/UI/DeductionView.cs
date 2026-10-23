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
    /// Deduction board: Kiều settles what she believes, one question at a time, from the clues she gathered. It never
    /// says whether an answer is right: a wrong but reasonable conclusion goes with her to Lâm Tri, as it would.
    /// </summary>
    public class DeductionView : MonoBehaviour
    {
        public struct Question
        {
            public string Prompt;
            public string[] Options;
            public int Correct;
        }

        private GameObject _root;
        private TMP_Text _question, _progress, _verdict;
        private RectTransform _options;
        private Button _template;
        private readonly List<Button> _buttons = new List<Button>();
        private int _picked = -1;

        /// <summary>QA autopilot answers automatically (correctly when true, randomly otherwise).</summary>
        public bool FastMode { get; set; }

        private void Awake()
        {
            _root = transform.Find("Deduction").gameObject;
            _question = StoryUIBuilder.Find<TMP_Text>(transform, "Deduction/Panel/Question");
            _progress = StoryUIBuilder.Find<TMP_Text>(transform, "Deduction/Panel/Progress");
            _verdict = StoryUIBuilder.Find<TMP_Text>(transform, "Deduction/Panel/Verdict");
            _options = StoryUIBuilder.Find<RectTransform>(transform, "Deduction/Panel/Options");
            _template = StoryUIBuilder.Find<Button>(transform, "Deduction/Panel/Options/Template");
        }

        /// <summary>
        /// Asks every question, then shows the conclusion built from what she picked. <paramref name="onPicks"/> gets the
        /// chosen option of each question, in order.
        /// </summary>
        public IEnumerator Run(IList<Question> questions, Func<int[], string> conclusion, Action<int[]> onPicks)
        {
            _root.SetActive(true);
            _verdict.text = string.Empty;
            var picks = new int[questions.Count];
            for (int q = 0; q < questions.Count; q++)
            {
                var question = questions[q];
                _progress.text = "Mảnh " + (q + 1) + " / " + questions.Count;
                _question.text = question.Prompt;
                Build(question.Options);
                _picked = -1;
                float shown = Time.realtimeSinceStartup;
                while (_picked < 0)
                {
                    if (FastMode && Time.realtimeSinceStartup - shown >= ThuyKieu.Core.Story.StoryCapture.Hold) _picked = UnityEngine.Random.Range(0, question.Options.Length);
                    var keyboard = Keyboard.current;
                    for (int i = 0; i < _buttons.Count && keyboard != null && !PauseMenuView.IsPaused; i++)
                        if (keyboard[Key.Digit1 + i].wasPressedThisFrame) _picked = i;
                    yield return null;
                }
                picks[q] = _picked;
                Mark(_picked);
                _question.text = question.Prompt + "\n<size=24><color=#E8C27A>Kiều ghi nhớ điều ấy.</color></size>";
                yield return new WaitForSecondsRealtime(FastMode ? ThuyKieu.Core.Story.StoryCapture.Hold * 0.5f : 1.2f);
            }
            Build(new string[0]);
            _question.text = string.Empty;
            _progress.text = "Điều Kiều tin";
            _verdict.text = conclusion(picks);
            yield return new WaitForSecondsRealtime(FastMode ? ThuyKieu.Core.Story.StoryCapture.Hold : 2.8f);
            _root.SetActive(false);
            onPicks(picks);
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

        /// <summary>Highlights her choice in the same neutral gold whatever it is.</summary>
        private void Mark(int picked)
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                if (i == picked) _buttons[i].GetComponent<Image>().color = new Color(0.55f, 0.42f, 0.18f, 0.9f);
                _buttons[i].interactable = false;
            }
        }
    }
}
