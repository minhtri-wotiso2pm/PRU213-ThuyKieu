using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ThuyKieu.UI
{
    /// <summary>Deduction board: a series of questions answered from the clues the player gathered.</summary>
    public class DeductionView : MonoBehaviour
    {
        public struct Question
        {
            public string Prompt;
            public string[] Options;
            public int Correct;
            /// <summary>Shown under a wrong answer so the player learns which clue they missed.</summary>
            public string Hint;
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

        public IEnumerator Run(IList<Question> questions, string conclusion, Action<int> onScore)
        {
            _root.SetActive(true);
            _verdict.text = string.Empty;
            int score = 0;
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
                bool right = _picked == question.Correct;
                if (right) score++;
                Mark(question.Correct, _picked);
                _question.text = question.Prompt + "\n<size=24><color=" + (right ? "#9BE39B>Khớp." : "#F08080>Chưa khớp. " + question.Hint) + "</color></size>";
                yield return new WaitForSecondsRealtime(FastMode ? ThuyKieu.Core.Story.StoryCapture.Hold * 0.5f : right ? 1.1f : 2.4f);
            }
            Build(new string[0]);
            _question.text = string.Empty;
            _progress.text = score + " / " + questions.Count + " mảnh khớp";
            _verdict.text = conclusion;
            yield return new WaitForSecondsRealtime(FastMode ? ThuyKieu.Core.Story.StoryCapture.Hold : 2.6f);
            _root.SetActive(false);
            onScore(score);
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
    }
}
