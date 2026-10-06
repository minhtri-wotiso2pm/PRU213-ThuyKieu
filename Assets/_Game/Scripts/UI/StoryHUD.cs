using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using ThuyKieu.Dialogue;
using ThuyKieu.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThuyKieu.UI
{
    /// <summary>
    /// In-game overlay for story chapters: objective and options, interaction prompt, meters,
    /// toasts, barks, cinematic bars, chapter / failure / completion cards and fades.
    /// </summary>
    public class StoryHUD : MonoBehaviour
    {
        private RectTransform _barTop, _barBottom, _meterFill, _observeFill;
        private GameObject _header, _prompt, _meter, _observe, _toast, _tutorial, _fail, _complete, _chapterCard, _barkRoot;
        private GameObject[] _overlays;
        private TMP_Text _chapterLabel, _objective, _exploreList, _controls, _promptText, _meterLabel, _observeLabel,
            _toastTitle, _toastBody, _bark, _tutorialBody, _failHeadline, _failDetail, _completeTitle, _completeLines,
            _cardNumber, _cardTitle;
        private Image _meterFillImage;
        private CanvasGroup _fade, _chapterGroup;
        private Button _continue;
        private PlayerInteraction _interaction;
        private readonly Queue<(string, string)> _toasts = new Queue<(string, string)>();
        private float _barTarget, _barHeight, _barkTimer, _toastTimer;
        private bool _dialogueMode, _meterRequested;
        private Action _onContinue;

        /// <summary>QA autopilot shortens cards and waits.</summary>
        public bool FastMode { get; set; }

        private void Awake()
        {
            Transform t = transform;
            _barTop = StoryUIBuilder.Find<RectTransform>(t, "Letterbox/Top");
            _barBottom = StoryUIBuilder.Find<RectTransform>(t, "Letterbox/Bottom");
            _header = t.Find("HUD/Header").gameObject;
            _chapterLabel = StoryUIBuilder.Find<TMP_Text>(t, "HUD/Header/ChapterLabel");
            _objective = StoryUIBuilder.Find<TMP_Text>(t, "HUD/Header/Objective");
            _exploreList = StoryUIBuilder.Find<TMP_Text>(t, "HUD/Header/ExploreList");
            _controls = StoryUIBuilder.Find<TMP_Text>(t, "HUD/Controls");
            _prompt = t.Find("HUD/Prompt").gameObject;
            _promptText = StoryUIBuilder.Find<TMP_Text>(t, "HUD/Prompt/Text");
            _meter = t.Find("HUD/Meter").gameObject;
            _meterLabel = StoryUIBuilder.Find<TMP_Text>(t, "HUD/Meter/Label");
            _meterFill = StoryUIBuilder.Find<RectTransform>(t, "HUD/Meter/Track/Fill");
            _meterFillImage = _meterFill.GetComponent<Image>();
            _observe = t.Find("HUD/Observe").gameObject;
            _observeLabel = StoryUIBuilder.Find<TMP_Text>(t, "HUD/Observe/Label");
            _observeFill = StoryUIBuilder.Find<RectTransform>(t, "HUD/Observe/Track/Fill");
            _toast = t.Find("HUD/Toast").gameObject;
            _toastTitle = StoryUIBuilder.Find<TMP_Text>(t, "HUD/Toast/Title");
            _toastBody = StoryUIBuilder.Find<TMP_Text>(t, "HUD/Toast/Body");
            _bark = StoryUIBuilder.Find<TMP_Text>(t, "HUD/Bark/Text");
            _chapterCard = t.Find("Cards/Chapter").gameObject;
            _chapterGroup = _chapterCard.GetComponent<CanvasGroup>();
            _cardNumber = StoryUIBuilder.Find<TMP_Text>(t, "Cards/Chapter/Number");
            _cardTitle = StoryUIBuilder.Find<TMP_Text>(t, "Cards/Chapter/Title");
            _tutorial = t.Find("Cards/Tutorial").gameObject;
            _tutorialBody = StoryUIBuilder.Find<TMP_Text>(t, "Cards/Tutorial/Body");
            _fail = t.Find("Cards/Fail").gameObject;
            _failHeadline = StoryUIBuilder.Find<TMP_Text>(t, "Cards/Fail/Headline");
            _failDetail = StoryUIBuilder.Find<TMP_Text>(t, "Cards/Fail/Detail");
            _complete = t.Find("Cards/Complete").gameObject;
            _completeTitle = StoryUIBuilder.Find<TMP_Text>(t, "Cards/Complete/Panel/Title");
            _completeLines = StoryUIBuilder.Find<TMP_Text>(t, "Cards/Complete/Panel/Lines");
            _continue = StoryUIBuilder.Find<Button>(t, "Cards/Complete/Panel/Continue");
            _fade = StoryUIBuilder.Find<CanvasGroup>(t, "Fade");
            _barkRoot = t.Find("HUD/Bark").gameObject;
            _overlays = new[] { t.Find("Journal").gameObject, t.Find("Document").gameObject, t.Find("Deduction").gameObject, _tutorial, _fail, _complete, _chapterCard };
            _continue.onClick.AddListener(() => { var callback = _onContinue; _onContinue = null; callback?.Invoke(); });

            _prompt.SetActive(false);
            _meter.SetActive(false);
            _observe.SetActive(false);
            _toast.SetActive(false);
            _bark.text = string.Empty;
            _fade.alpha = 1f;
            _fade.blocksRaycasts = false;
        }

        private void Start() { _interaction = FindAnyObjectByType<PlayerInteraction>(); }

        // ------------------------------------------------------------------ persistent HUD

        public void SetChapterLabel(string text) => _chapterLabel.text = text;
        public void SetControls(string text) => _controls.text = text;

        public string Objective => _objective.text;

        public void SetObjective(string text, bool announce = true)
        {
            _objective.text = text;
            if (announce) ShowToast("MỤC TIÊU", text);
        }

        public void SetExploreOptions(IList<string> options)
        {
            int count = options == null ? 0 : options.Count;
            var header = (RectTransform)_header.transform;
            header.offsetMin = new Vector2(header.offsetMin.x, header.offsetMax.y - 122 - 25 * count);
            if (count == 0) { _exploreList.text = string.Empty; return; }
            var builder = new StringBuilder();
            foreach (string option in options) builder.Append("<color=#E8C27A>›</color> ").AppendLine(option);
            _exploreList.text = builder.ToString();
        }

        public void SetDialogueMode(bool active)
        {
            _dialogueMode = active;
            _barTarget = active ? 92f : 0f;
        }

        /// <summary>Generic top meter (suspicion, detection, listening). Call every frame while visible.</summary>
        public void SetMeter(string label, float value, Color color)
        {
            _meterRequested = true;
            _meterLabel.text = label;
            _meterFill.anchorMax = new Vector2(Mathf.Clamp01(value), 1f);
            _meterFillImage.color = color;
        }

        public void SetObserve(bool observing, string label, float progress)
        {
            if (_observe.activeSelf != observing) _observe.SetActive(observing);
            if (!observing) return;
            _observeLabel.text = label ?? "Đưa tâm nhìn vào điều đáng chú ý";
            _observeFill.anchorMax = new Vector2(Mathf.Clamp01(progress), 1f);
        }

        public void ShowToast(string title, string body)
        {
            _toasts.Enqueue((title, body));
            if (_toastTimer <= 0f) NextToast();
        }

        public void ClearBark() { _bark.text = string.Empty; _barkTimer = 0f; }

        public void ShowBark(string speakerId, string text, float duration = 3f)
        {
            var entry = SpeakerCatalog.Get(speakerId);
            _bark.text = "<color=#" + ColorUtility.ToHtmlStringRGB(entry.Color) + ">" + entry.DisplayName + ":</color> " + text;
            _barkTimer = duration;
        }

        private void NextToast()
        {
            if (_toasts.Count == 0) { _toast.SetActive(false); return; }
            var (title, body) = _toasts.Dequeue();
            _toastTitle.text = title;
            _toastBody.text = body;
            _toast.SetActive(true);
            _toastTimer = 3.2f;
        }

        private void LateUpdate()
        {
            _barHeight = Mathf.Lerp(_barHeight, _barTarget, 1 - Mathf.Exp(-6f * Time.unscaledDeltaTime));
            _barTop.sizeDelta = new Vector2(0, _barHeight);
            _barBottom.sizeDelta = new Vector2(0, _barHeight);
            bool overlay = System.Array.Exists(_overlays, o => o.activeSelf);
            _header.SetActive(!_dialogueMode && !overlay);
            _barkRoot.SetActive(!_dialogueMode && !overlay);
            _controls.gameObject.SetActive(!_dialogueMode);

            if (_meter.activeSelf != _meterRequested) _meter.SetActive(_meterRequested);
            _meterRequested = false;

            if (_toastTimer > 0f) { _toastTimer -= Time.unscaledDeltaTime; if (_toastTimer <= 0f) NextToast(); }
            if (_barkTimer > 0f) { _barkTimer -= Time.unscaledDeltaTime; if (_barkTimer <= 0f) _bark.text = string.Empty; }

            var target = _interaction != null && !_dialogueMode ? _interaction.CurrentTarget : null;
            bool showPrompt = target != null;
            if (_prompt.activeSelf != showPrompt) _prompt.SetActive(showPrompt);
            if (showPrompt) _promptText.text = "<color=#E8C27A>[E]</color>  " + ((target as IInteractableDisplayName)?.DisplayName ?? "Tương tác");
        }

        // ------------------------------------------------------------------ cards

        public IEnumerator Fade(float alpha, float duration)
        {
            float start = _fade.alpha;
            for (float t = 0; t < duration && !FastMode; t += Time.unscaledDeltaTime)
            {
                _fade.alpha = Mathf.Lerp(start, alpha, t / duration);
                yield return null;
            }
            _fade.alpha = alpha;
        }

        public IEnumerator ChapterCard(int number, string title)
        {
            SetChapterLabel("Chương " + number + " · " + title);
            _cardNumber.text = "CHƯƠNG " + number;
            _cardTitle.text = title.ToUpperInvariant();
            _chapterCard.SetActive(true);
            _chapterGroup.alpha = 0f;
            _fade.alpha = 0f;
            float hold = FastMode ? Mathf.Max(0.05f, ThuyKieu.Core.Story.StoryCapture.Hold) : 2.6f;
            for (float t = 0; t < 1f && !FastMode; t += Time.unscaledDeltaTime) { _chapterGroup.alpha = t; yield return null; }
            _chapterGroup.alpha = 1f;
            yield return new WaitForSecondsRealtime(hold);
            for (float t = 1; t > 0f && !FastMode; t -= Time.unscaledDeltaTime * 0.8f) { _chapterGroup.alpha = t; yield return null; }
            _chapterCard.SetActive(false);
        }

        public IEnumerator TutorialCard(string body)
        {
            _tutorialBody.text = body;
            _tutorial.SetActive(true);
            yield return new WaitForSecondsRealtime(FastMode ? ThuyKieu.Core.Story.StoryCapture.Hold : 0.6f);
            while (!FastMode && !ThuyKieu.Core.InputReader.InteractPressedThisFrame && !ConfirmPressed()) yield return null;
            _tutorial.SetActive(false);
        }

        public IEnumerator FailCard(string headline, string detail, float hold)
        {
            _failHeadline.text = headline;
            _failDetail.text = detail;
            _fail.SetActive(true);
            yield return new WaitForSecondsRealtime(FastMode ? Mathf.Max(0.05f, ThuyKieu.Core.Story.StoryCapture.Hold) : hold);
            yield return Fade(1f, 0.5f);
            _fail.SetActive(false);
        }

        public void ShowChapterComplete(int number, string title, IList<string> lines, Action onContinue)
        {
            _completeTitle.text = "CHƯƠNG " + number + " HOÀN THÀNH\n<size=26><color=#FFFFFF>" + title + "</color></size>";
            var builder = new StringBuilder();
            foreach (string line in lines) builder.Append("<color=#E8C27A>•</color> ").AppendLine(line);
            _completeLines.text = builder.ToString();
            _onContinue = onContinue;
            _complete.SetActive(true);
        }

        public bool IsCompleteVisible => _complete.activeSelf;
        public bool IsTutorialVisible => _tutorial.activeSelf;

        private static bool ConfirmPressed()
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            return keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame);
        }
    }
}
