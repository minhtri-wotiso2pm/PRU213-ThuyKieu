using System.Collections.Generic;
using ThuyKieu.Dialogue;
using ThuyKieu.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ThuyKieu.UI
{
    /// <summary>
    /// Cinematic dialogue view: typewriter text, one presentation per speaker style
    /// (spoken panel, inner thought, the real Kiều's voice, remembered verse), numbered choices
    /// and Telltale-style timed choices driven by the "# timer:" tag.
    /// </summary>
    public class StoryDialogueView : MonoBehaviour
    {
        [SerializeField] private float _charactersPerSecond = 48f;

        private DialogueManager _manager;
        private StoryAudio _audio;
        private GameObject _panel, _innerObject, _spiritObject, _verse, _hint, _timer;
        private TMP_Text _name, _body, _inner, _spirit, _verseText, _active;
        private RectTransform _choices, _timerFill;
        private Button _template;
        private readonly List<Button> _buttons = new List<Button>();
        private DialogueChoice[] _currentChoices = new DialogueChoice[0];
        private float _visible, _timerLeft, _timerTotal;
        private int _length;

        public bool IsTyping => _active != null && _visible < _length;
        public float TimerRemaining => _timerLeft;

        private void Awake()
        {
            Transform t = transform;
            _panel = t.Find("Dialogue/Panel").gameObject;
            _name = StoryUIBuilder.Find<TMP_Text>(t, "Dialogue/Panel/Name");
            _body = StoryUIBuilder.Find<TMP_Text>(t, "Dialogue/Panel/Body");
            _inner = StoryUIBuilder.Find<TMP_Text>(t, "Dialogue/Inner");
            _spirit = StoryUIBuilder.Find<TMP_Text>(t, "Dialogue/Spirit");
            _verse = t.Find("Dialogue/Verse").gameObject;
            _verseText = StoryUIBuilder.Find<TMP_Text>(t, "Dialogue/Verse/Text");
            _hint = t.Find("Dialogue/Hint").gameObject;
            _choices = StoryUIBuilder.Find<RectTransform>(t, "Dialogue/Choices");
            _template = StoryUIBuilder.Find<Button>(t, "Dialogue/Choices/Template");
            _timer = t.Find("Dialogue/Choices/Timer").gameObject;
            _timerFill = StoryUIBuilder.Find<RectTransform>(t, "Dialogue/Choices/Timer/Fill");
            _innerObject = _inner.gameObject;
            _spiritObject = _spirit.gameObject;
            HideAll();
        }

        private void Start()
        {
            _manager = DialogueManager.Instance;
            _audio = FindAnyObjectByType<StoryAudio>();
            if (_manager == null) return;
            _manager.DialogueStarted += OnStarted;
            _manager.LineChanged += OnLine;
            _manager.ChoicesChanged += OnChoices;
            _manager.DialogueEnded += HideAll;
        }

        private void OnDestroy()
        {
            if (_manager == null) return;
            _manager.DialogueStarted -= OnStarted;
            _manager.LineChanged -= OnLine;
            _manager.ChoicesChanged -= OnChoices;
            _manager.DialogueEnded -= HideAll;
        }

        private void OnStarted(DialogueData _) { }

        private void HideAll()
        {
            _panel.SetActive(false);
            _innerObject.SetActive(false);
            _spiritObject.SetActive(false);
            _verse.SetActive(false);
            _hint.SetActive(false);
            _timer.SetActive(false);
            ClearChoices();
            _active = null;
        }

        private void OnLine(string speakerName, string text)
        {
            var entry = SpeakerCatalog.Get(_manager.CurrentSpeakerId);
            _panel.SetActive(entry.Style == SpeakerStyle.Spoken || entry.Style == SpeakerStyle.Narration);
            _innerObject.SetActive(entry.Style == SpeakerStyle.Inner);
            _spiritObject.SetActive(entry.Style == SpeakerStyle.Spirit);
            _verse.SetActive(entry.Style == SpeakerStyle.Verse);
            switch (entry.Style)
            {
                case SpeakerStyle.Spoken:
                    _name.text = entry.DisplayName;
                    _name.color = entry.Color;
                    _body.fontStyle = FontStyles.Normal;
                    _body.color = Color.white;
                    _active = _body;
                    text = StripSpeakerPrefix(text, entry.DisplayName, speakerName);
                    break;
                case SpeakerStyle.Narration:
                    _name.text = string.Empty;
                    _body.fontStyle = FontStyles.Italic;
                    _body.color = new Color(0.85f, 0.83f, 0.78f);
                    _active = _body;
                    break;
                case SpeakerStyle.Inner: _active = _inner; break;
                case SpeakerStyle.Spirit: _active = _spirit; break;
                default: _active = _verseText; break;
            }
            _active.text = text;
            _active.maxVisibleCharacters = 0;
            _active.ForceMeshUpdate();
            _length = _active.textInfo.characterCount;
            _visible = 0f;
            _hint.SetActive(true);
        }

        // Ink lines carry "Mã Giám Sinh: ..." for readability in Inky; the name plate already shows it.
        private static string StripSpeakerPrefix(string text, string displayName, string managerName)
        {
            int colon = text.IndexOf(':');
            if (colon > 0 && colon < 22)
            {
                string prefix = text.Substring(0, colon).Trim();
                if (prefix == displayName || prefix == managerName || prefix == "Kiều" || prefix.StartsWith("Cô gái") || prefix == "Tùy tùng"
                    || prefix == "Người canh" || prefix == "Tú Bà" || prefix == "Mã Kiều" || prefix == "Mẹ Kiều" || prefix == "Mã Giám Sinh")
                    return text.Substring(colon + 1).TrimStart();
            }
            return text;
        }

        private void OnChoices(DialogueChoice[] choices)
        {
            ClearChoices();
            _currentChoices = choices ?? new DialogueChoice[0];
            _timer.SetActive(false);
            if (_currentChoices.Length == 0) return;
            for (int i = 0; i < _currentChoices.Length; i++)
            {
                int index = i;
                var button = Instantiate(_template, _choices);
                button.gameObject.SetActive(true);
                button.GetComponentInChildren<TMP_Text>().text = "<color=#E8C27A>" + (i + 1) + "</color>   " + _currentChoices[i].ChoiceText;
                button.onClick.AddListener(() => Choose(index));
                _buttons.Add(button);
            }
            _timerTotal = _timerLeft = 0f;
            foreach (string tag in _manager.CurrentInkTags)
                if (tag.Trim().StartsWith("timer:") && float.TryParse(tag.Trim().Substring(6), out float seconds))
                    _timerTotal = _timerLeft = seconds;
            _timer.SetActive(_timerTotal > 0f);
        }

        private void ClearChoices()
        {
            foreach (var button in _buttons) if (button != null) Destroy(button.gameObject);
            _buttons.Clear();
            _currentChoices = new DialogueChoice[0];
        }

        private void Choose(int index)
        {
            if (_manager == null || !_manager.IsAtChoicePoint || index >= _currentChoices.Length) return;
            if (_audio != null) _audio.PlaySfx("ui", 0.6f);
            _manager.SelectChoice(index);
        }

        private void Update()
        {
            if (_manager == null || !_manager.IsDialogueActive || _active == null || PauseMenuView.IsPaused) return;
            if (_visible < _length)
            {
                _visible += Time.unscaledDeltaTime * _charactersPerSecond;
                _active.maxVisibleCharacters = (int)_visible;
            }
            // Choices appear once the line has finished typing.
            bool choicesReady = !IsTyping;
            _choices.gameObject.SetActive(choicesReady);

            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            bool confirm = (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame))
                           || (mouse != null && mouse.leftButton.wasPressedThisFrame && _buttons.Count == 0);
            if (_manager.IsAtChoicePoint && _buttons.Count > 0)
            {
                if (confirm && IsTyping) SkipTyping();
                if (!choicesReady) return;
                for (int i = 0; i < _buttons.Count && i < 9; i++)
                    if (keyboard != null && keyboard[Key.Digit1 + i].wasPressedThisFrame) { Choose(i); return; }
                if (_timerTotal > 0f)
                {
                    _timerLeft -= Time.unscaledDeltaTime;
                    _timerFill.anchorMax = new Vector2(Mathf.Clamp01(_timerLeft / _timerTotal), 1f);
                    if (_timerLeft <= 0f) Choose(TimeoutIndex());
                }
                return;
            }
            if (!confirm) return;
            if (IsTyping) { SkipTyping(); return; }
            if (_audio != null) _audio.PlaySfx("ui", 0.35f);
            _manager.Advance();
        }

        private void SkipTyping()
        {
            _visible = _length;
            _active.maxVisibleCharacters = _length;
        }

        private int TimeoutIndex()
        {
            for (int i = 0; i < _currentChoices.Length; i++) if (_currentChoices[i].HasTag("timeout")) return i;
            return 0;
        }
    }
}
