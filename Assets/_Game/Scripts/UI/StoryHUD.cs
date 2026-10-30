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
        private RectTransform _barTop, _barBottom, _meterFill, _observeFill, _gaugeFill, _qteFill;
        private GameObject _header, _prompt, _meter, _observe, _toast, _tutorial, _fail, _complete, _chapterCard, _barkRoot, _gauge, _qte;
        private GameObject[] _overlays;
        private TMP_Text _chapterLabel, _objective, _exploreList, _controls, _promptText, _meterLabel, _observeLabel,
            _toastTitle, _toastBody, _bark, _tutorialBody, _failHeadline, _failDetail, _completeTitle, _completeLines,
            _cardNumber, _cardTitle, _gaugeLabel, _qteKey, _qteCaption;
        private Image _meterFillImage, _gaugeFillImage;
        private CanvasGroup _fade, _chapterGroup, _toastGroup;
        private Button _continue;
        private PlayerInteraction _interaction;
        private readonly Queue<(string, string)> _toasts = new Queue<(string, string)>();
        private float _barTarget, _barHeight, _barkTimer, _toastTimer;
        private bool _dialogueMode, _meterRequested, _gaugeRequested;
        private Action _onContinue;
        private float _completeShownAt;

        // Compact HUD (first-person chapters): the objective is shown once, where it fades out after a while; control
        // hints only after they change; subtitles get a soft shadow so they read over a bright scene.
        [SerializeField] private bool _compact;
        private float _headerTimer, _controlsTimer;
        // While an objective toast is on screen the header waits, so the same text is not shown twice at once.
        private float _objectiveToastHold;
        private CanvasGroup _headerGroup, _controlsGroup;
        private const float HeaderSeconds = 9f, ControlsSeconds = 14f, SituationSeconds = 7f;
        // The objective as set (the header line), and a danger happening now shown under it, above any options.
        private string _objectiveLine = string.Empty, _situation, _optionsText = string.Empty;
        private int _optionLines;

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
            _objectiveLine = _objective.text;
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
            _toastGroup = _toast.GetComponent<CanvasGroup>();
            if (_toastGroup == null) _toastGroup = _toast.AddComponent<CanvasGroup>();
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
            _gauge = t.Find("HUD/Gauge").gameObject;
            _gaugeLabel = StoryUIBuilder.Find<TMP_Text>(t, "HUD/Gauge/Label");
            _gaugeFill = StoryUIBuilder.Find<RectTransform>(t, "HUD/Gauge/Track/Fill");
            _gaugeFillImage = _gaugeFill.GetComponent<Image>();
            _qte = t.Find("HUD/Qte").gameObject;
            _qteKey = StoryUIBuilder.Find<TMP_Text>(t, "HUD/Qte/Key");
            _qteCaption = StoryUIBuilder.Find<TMP_Text>(t, "HUD/Qte/Caption");
            _qteFill = StoryUIBuilder.Find<RectTransform>(t, "HUD/Qte/Track/Fill");
            var overlays = new System.Collections.Generic.List<GameObject> { _tutorial, _fail, _complete, _chapterCard };
            foreach (string name in new[] { "Pause", "Journal", "Document", "Deduction", "Dossier", "Trial", "Recap" })
            {
                var overlay = t.Find(name);
                if (overlay != null) overlays.Add(overlay.gameObject);
            }
            _overlays = overlays.ToArray();
            _continue.onClick.AddListener(() => { var callback = _onContinue; _onContinue = null; callback?.Invoke(); });

            _prompt.SetActive(false);
            _meter.SetActive(false);
            _observe.SetActive(false);
            _toast.SetActive(false);
            _gauge.SetActive(false);
            _qte.SetActive(false);
            _bark.text = string.Empty;
            _fade.alpha = 1f;
            if (_compact) SetupCompact();
            _fade.blocksRaycasts = false;
        }

        private void Start() { _interaction = FindAnyObjectByType<PlayerInteraction>(); }

        // ------------------------------------------------------------------ persistent HUD

        public void SetChapterLabel(string text) => _chapterLabel.text = text;
        public void SetControls(string text)
        {
            _controls.text = text;
            _controlsTimer = ControlsSeconds;
        }

        public string Objective => _objectiveLine;

        public void SetObjective(string text, bool announce = true)
        {
            bool changed = _objectiveLine != text;
            _objectiveLine = text;
            _objective.text = text;
            _headerTimer = HeaderSeconds;
            // A new objective is announced by the toast; the header then repeats it, not both at once.
            // Re-setting the same objective (a sequence re-stating it) stays quiet instead of re-announcing.
            if (announce && !_compact && changed) { ShowToast("MỤC TIÊU", text); _objectiveToastHold = 3.4f; }
        }

        public void SetExploreOptions(IList<string> options)
        {
            // Keep the list short so it never covers the scene; the rest stays one line away.
            const int shown = 4;
            int count = options == null ? 0 : options.Count;
            _optionLines = count > shown ? shown + 1 : count;
            var builder = new StringBuilder();
            for (int i = 0; i < count && i < shown; i++) builder.Append("<color=#E8C27A>›</color> ").AppendLine(options[i]);
            if (count > shown) builder.Append("<color=#A89A86><i>+ ").Append(count - shown).AppendLine(" việc khác có thể làm…</i></color>");
            _optionsText = builder.ToString();
            LayoutHeader();
        }

        /// <summary>
        /// A short line under the objective about a danger happening now ("someone at the door"); null clears it. The
        /// objective stays as it is. In the compact HUD a new line brings the header back for a few seconds.
        /// </summary>
        public void SetSituation(string text)
        {
            if (string.IsNullOrEmpty(text)) text = null;
            if (_situation == text) return;
            _situation = text;
            LayoutHeader();
            if (text != null) _headerTimer = Mathf.Max(_headerTimer, SituationSeconds);
        }

        public string Situation => _situation;

        private void LayoutHeader()
        {
            int lines = _optionLines + (_situation != null ? 1 : 0);
            var header = (RectTransform)_header.transform;
            header.offsetMin = new Vector2(header.offsetMin.x, header.offsetMax.y - 122 - 25 * lines);
            _exploreList.text = _situation == null ? _optionsText : "<color=#E3A58C>" + _situation + "</color>\n" + _optionsText;
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

        /// <summary>Small bottom gauge (stamina, visibility). Call every frame while visible.</summary>
        public void SetGauge(string label, float value, Color color)
        {
            _gaugeRequested = true;
            _gaugeLabel.text = label;
            _gaugeFill.anchorMax = new Vector2(Mathf.Clamp01(value), 1f);
            _gaugeFillImage.color = color;
        }

        /// <summary>Quick-time prompt; <paramref name="remaining"/> is the share of the window left (0–1).</summary>
        public void ShowQte(string key, string caption, float remaining, Color tint)
        {
            if (!_qte.activeSelf) _qte.SetActive(true);
            _qteKey.text = key;
            _qteKey.color = tint;
            _qteCaption.text = caption;
            _qteFill.anchorMax = new Vector2(Mathf.Clamp01(remaining), 1f);
        }

        public void HideQte() { if (_qte.activeSelf) _qte.SetActive(false); }

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

        /// <param name="voiced">False when the caller plays the voice itself (from a place in the world).</param>
        public void ShowBark(string speakerId, string text, float duration = 3f, bool voiced = true)
        {
            var entry = SpeakerCatalog.Get(speakerId);
            _bark.text = "<color=#" + ColorUtility.ToHtmlStringRGB(entry.Color) + ">" + entry.DisplayName + ":</color> " + text;
            _barkTimer = duration;
            if (voiced) VoicePlayer.PlayBark(speakerId, text);
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
            if (_objectiveToastHold > 0f) _objectiveToastHold -= Time.unscaledDeltaTime;
            _header.SetActive(!_dialogueMode && !overlay && _objectiveToastHold <= 0f);
            _barkRoot.SetActive(!_dialogueMode && !overlay);
            _toastGroup.alpha = overlay ? 0f : 1f; // documents and boards cover the screen; toasts wait underneath
            _controls.gameObject.SetActive(!_dialogueMode);
            if (_compact) UpdateCompact();

            if (_meter.activeSelf != _meterRequested) _meter.SetActive(_meterRequested);
            _meterRequested = false;
            bool gauge = _gaugeRequested && !_dialogueMode && !overlay;
            if (_gauge.activeSelf != gauge) _gauge.SetActive(gauge);
            _gaugeRequested = false;

            if (_toastTimer > 0f) { _toastTimer -= Time.unscaledDeltaTime; if (_toastTimer <= 0f) NextToast(); }
            if (_barkTimer > 0f) { _barkTimer -= Time.unscaledDeltaTime; if (_barkTimer <= 0f) _bark.text = string.Empty; }
            if (_complete.activeSelf && _onContinue != null && Time.unscaledTime - _completeShownAt > 0.8f && ConfirmPressed()) { var callback = _onContinue; _onContinue = null; callback(); }
            if (_complete.activeSelf && Cursor.lockState == CursorLockMode.Locked) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }

            var target = _interaction != null && !_dialogueMode ? _interaction.CurrentTarget : null;
            bool showPrompt = target != null;
            if (_prompt.activeSelf != showPrompt) _prompt.SetActive(showPrompt);
            if (showPrompt) _promptText.text = "<color=#E8C27A>[E]</color>  " + ((target as IInteractableDisplayName)?.DisplayName ?? "Tương tác");
        }

        private void SetupCompact()
        {
            _headerGroup = _header.GetComponent<CanvasGroup>();
            if (_headerGroup == null) _headerGroup = _header.AddComponent<CanvasGroup>();
            _controlsGroup = _controls.GetComponent<CanvasGroup>();
            if (_controlsGroup == null) _controlsGroup = _controls.gameObject.AddComponent<CanvasGroup>();
            var subtitle = _bark.fontMaterial;
            subtitle.EnableKeyword(ShaderUtilities.Keyword_Underlay);
            subtitle.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0f, 0f, 0f, 0.85f));
            subtitle.SetFloat(ShaderUtilities.ID_UnderlayDilate, 0.4f);
            subtitle.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.6f);
            _headerTimer = HeaderSeconds;
            _controlsTimer = ControlsSeconds;
        }

        /// <summary>Compact mode: fade the objective and the control hints out once they have been read.</summary>
        private void UpdateCompact()
        {
            float step = Time.unscaledDeltaTime;
            if (_headerTimer > 0f) _headerTimer -= step;
            if (_controlsTimer > 0f) _controlsTimer -= step;
            _headerGroup.alpha = Mathf.MoveTowards(_headerGroup.alpha, _headerTimer > 0f ? 1f : 0f, step * 1.2f);
            _controlsGroup.alpha = Mathf.MoveTowards(_controlsGroup.alpha, _controlsTimer > 0f ? 1f : 0f, step * 1.2f);
        }

        /// <summary>Compact mode: bring the objective back for a moment (after a checkpoint, on request: Tab in Chapter 4).</summary>
        public void RemindObjective() => _headerTimer = HeaderSeconds;

        // ------------------------------------------------------------------ cards

        /// <summary>Colour of the full-screen fade (black by default; white for the light at the end of the story).</summary>
        /// <summary>Sets the black-out directly, frame by frame, for scenes that time their own fade (even in fast QA runs).</summary>
        public float FadeAlpha { get => _fade.alpha; set => _fade.alpha = value; }

        public void SetFadeColor(Color color)
        {
            var image = _fade.GetComponent<Image>();
            if (image != null) image.color = color;
        }

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
            SetChapterLabel(ThuyKieu.Core.ChapterCatalog.Label(number) + " · " + title);
            yield return Card(ThuyKieu.Core.ChapterCatalog.Label(number).ToUpperInvariant(), title.ToUpperInvariant(), true);
        }

        /// <summary>Full-screen title card on black: a small line above, a large title and optionally the game's name below.</summary>
        public IEnumerator Card(string top, string title, bool gameLine, float seconds = 2.6f)
        {
            _cardNumber.text = top;
            _cardTitle.text = title;
            var line = _chapterCard.transform.Find("Line");
            if (line != null) line.gameObject.SetActive(gameLine);
            _chapterCard.SetActive(true);
            _chapterGroup.alpha = 0f;
            _fade.alpha = 0f;
            float hold = FastMode ? Mathf.Max(0.05f, ThuyKieu.Core.Story.StoryCapture.Hold) : seconds;
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
            while (!FastMode && (PauseMenuView.IsPaused || (!ThuyKieu.Core.InputReader.InteractPressedThisFrame && !ConfirmPressed()))) yield return null;
            _tutorial.SetActive(false);
        }

        public IEnumerator FailCard(string headline, string detail, float hold)
        {
            _failHeadline.text = headline;
            _failDetail.text = detail;
            // A catch scene ends on black: the cards come above the fade while the failure reads, then go back.
            var cards = _fail.transform.parent;
            int order = cards.GetSiblingIndex();
            cards.SetAsLastSibling();
            _fail.SetActive(true);
            yield return new WaitForSecondsRealtime(FastMode ? Mathf.Max(0.05f, ThuyKieu.Core.Story.StoryCapture.Hold) : hold);
            yield return Fade(1f, 0.5f);
            _fail.SetActive(false);
            cards.SetSiblingIndex(order);
        }

        public void ShowChapterComplete(int number, string title, IList<string> lines, Action onContinue)
        {
            _completeTitle.text = ThuyKieu.Core.ChapterCatalog.Label(number).ToUpperInvariant() + " HOÀN THÀNH\n<size=26><color=#FFFFFF>" + title + "</color></size>";
            var builder = new StringBuilder();
            foreach (string line in lines) builder.Append("<color=#E8C27A>•</color> ").AppendLine(line);
            _completeLines.text = builder.ToString();
            _onContinue = onContinue;
            _completeShownAt = Time.unscaledTime;
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
