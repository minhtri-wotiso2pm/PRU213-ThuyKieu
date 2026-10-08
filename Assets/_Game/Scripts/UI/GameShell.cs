using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThuyKieu.Core;
using ThuyKieu.Core.Story;
using ThuyKieu.Interaction;
using ThuyKieu.Player;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThuyKieu.UI
{
    /// <summary>
    /// Game-wide shell that lives across scenes (created automatically, no scene setup needed):
    /// main menu in the MainMenu scene, chapter select, settings, credits, an Esc pause menu for scenes without the
    /// story pause (Chapter 1), the ` test console, fades between scenes, and the "-chapter N" command-line jump.
    /// Runs before other scripts so it can claim Esc first.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class GameShell : MonoBehaviour
    {
        private struct LockState { public Behaviour Target; public bool Locked; }

        private sealed class ChapterRow
        {
            public ChapterCatalog.Entry Entry;
            public Button Button;
            public ShellButton Look;
            public TMP_Text Status;
        }

        private static GameShell _instance;

        // UI blends in linear space, where faint alphas read as grey: rows use opaque ink tones instead.
        private static readonly Color RowColor = new Color(0.1f, 0.075f, 0.085f, 1f);
        private static readonly Color RowHighlight = new Color(0.3f, 0.21f, 0.11f, 1f);
        private static readonly Color MenuHighlight = new Color(1f, 0.8f, 0.45f, 0.06f);

        private Transform _canvas;
        private CanvasGroup _fade;
        private GameObject _menu, _menuButtons, _menuControls, _pause, _pauseControls, _select;
        private DevConsole _console;
        private SettingsView _settings;
        private CreditsView _credits;
        private EndingGalleryView _gallery;
        private Button _continueButton, _pauseResume, _selectStart, _selectKeep, _selectBack;
        private TMP_Text _continueLabel, _pauseChapter, _detailNumber, _detailTitle, _detailBlurb, _detailMeta;
        private readonly List<ChapterRow> _rows = new List<ChapterRow>();
        private ChapterCatalog.Entry _focused;
        private Action _selectClosed;

        private readonly List<LockState> _locks = new List<LockState>();
        private int _freezeCount;
        private bool _frozeWasPaused, _cursorWasLocked, _cursorWasVisible, _storyPauseScene, _inMenuScene, _loading;
        private float _frozenTimeScale = 1f;
        private GameObject _selectedBeforeOverlay;

        // ---------------------------------------------------------------- boot & static API

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (_instance != null || Application.isBatchMode || StoryAutopilot.Enabled) return;
            var go = new GameObject("GameShell");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<GameShell>();
            _instance.OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
            _instance.ApplyCommandLine();
        }

        /// <summary>Opens chapter select on top of whatever is showing; onClose runs when it is dismissed without loading.</summary>
        public static void ShowChapterSelect(Action onClose)
        {
            if (_instance != null) _instance.OpenSelect(onClose);
        }

        /// <summary>Opens the options screen over the current view; onClose runs when it is dismissed.</summary>
        public static void ShowSettings(Action onClose)
        {
            if (_instance == null) { onClose?.Invoke(); return; }
            _instance.OpenSettings(onClose);
        }

        /// <summary>Rolls the credits (end of the game, or from the main menu); onClose runs when they are dismissed.</summary>
        public static void ShowCredits(Action onClose)
        {
            if (_instance == null) { onClose?.Invoke(); return; }
            _instance.OpenCredits(onClose);
        }

        public static void GoToMainMenu()
        {
            if (!ChapterLoader.LoadMainMenu()) Debug.LogWarning("[GameShell] Add the MainMenu scene (Thúy Kiều > Menu > Build Main Menu Scene).");
        }

        public static void QuitGame() => ChapterLoader.Quit();

        /// <summary>Time scale restored when the console or pause closes (console "ts" command).</summary>
        public static void SetResumeTimeScale(float scale)
        {
            if (_instance != null && _instance._freezeCount > 0) _instance._frozenTimeScale = scale;
            else Time.timeScale = scale;
        }

        /// <summary>"-chapter 3" (or ch3 / a scene name) jumps straight to a chapter; "-keepChoices" carries saved choices,
        /// "-continue" resumes the last chapter, "-unlockAll" unlocks chapter select.</summary>
        private void ApplyCommandLine()
        {
            var args = System.Environment.GetCommandLineArgs();
            if (args.Contains("-unlockAll")) GameProgress.UnlockAll = true;
            int preview = Array.IndexOf(args, "-shellPreview");
            if (preview >= 0 && preview + 1 < args.Length) { StartCoroutine(Preview(args[preview + 1])); return; }
            bool keep = args.Contains("-keepChoices");
            for (int i = 0; i < args.Length; i++)
            {
                string flag = args[i].ToLowerInvariant();
                if ((flag == "-chapter" || flag == "-chuong") && i + 1 < args.Length)
                {
                    var entry = ChapterCatalog.Parse(args[i + 1]);
                    if (entry == null) Debug.LogWarning("[GameShell] Unknown chapter: " + args[i + 1]);
                    else if (SceneManager.GetActiveScene().name != entry.Scene) ChapterLoader.LoadChapter(entry, keep);
                    return;
                }
                if (flag == "-continue") { ChapterLoader.Continue(); return; }
            }
        }

        /// <summary>QA: "-shellPreview &lt;folder&gt;" photographs the main menu, every settings tab, chapter select and the credits, then quits.</summary>
        private IEnumerator Preview(string folder)
        {
            System.IO.Directory.CreateDirectory(folder);
            yield return new WaitForSecondsRealtime(2.5f);
            yield return Photo(folder, "menu");
            OpenSettings(null);
            for (int tab = 0; tab < 4; tab++)
            {
                _settings.ShowTab(tab);
                yield return Photo(folder, "settings_" + tab);
            }
            _settings.Close();
            OpenSelect(null);
            yield return Photo(folder, "select");
            CloseSelect();
            OpenCredits(null);
            yield return new WaitForSecondsRealtime(9f);
            yield return Photo(folder, "credits");
            _credits.Close();
            ChapterLoader.Quit();
        }

        private static IEnumerator Photo(string folder, string name)
        {
            yield return new WaitForSecondsRealtime(0.8f);
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folder, name + ".png"));
            yield return new WaitForSecondsRealtime(0.4f);
        }

        // ---------------------------------------------------------------- lifecycle

        private void Awake()
        {
            BuildCanvas();
            ChapterLoader.SceneLoader = scene => StartCoroutine(FadeAndLoad(scene));
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (_instance == this) { _instance = null; ChapterLoader.SceneLoader = null; }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode != LoadSceneMode.Single) return;
            _loading = false;
            _locks.Clear();
            _freezeCount = 0;
            ChapterLoader.ClearPause();
            _pause.SetActive(false);
            _select.SetActive(false);
            _menuControls.SetActive(false);
            _settings.Close();
            _credits.Close();
            _gallery.Close();
            _console.Close();

            var entry = ChapterCatalog.FindByScene(scene.name);
            if (entry != null && !entry.Placeholder) GameProgress.MarkPlaying(entry.Number);
            _inMenuScene = scene.name == ChapterCatalog.MainMenuScene;
            _storyPauseScene = FindAnyObjectByType<PauseMenuView>() != null && FindAnyObjectByType<StoryDirector>() != null;
            _menu.SetActive(_inMenuScene);
            EnsureEventSystem();
            if (_inMenuScene) ShowMenu();
            StartCoroutine(FadeTo(0f, 0.6f));
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || _loading) return;

            if (keyboard.backquoteKey.wasPressedThisFrame || keyboard.f1Key.wasPressedThisFrame) { _console.Toggle(); return; }
            if (_settings.IsOpen || _credits.IsOpen || _gallery.IsOpen) return; // they handle their own keys, Esc included

            if (_select.activeSelf && !_console.IsOpen)
            {
                for (int i = 0; i < _rows.Count; i++)
                    if (keyboard[Key.Digit1 + i].wasPressedThisFrame || keyboard[Key.Numpad1 + i].wasPressedThisFrame) Select(_rows[i].Button.gameObject);
            }

            if (!keyboard.escapeKey.wasPressedThisFrame) return;
            if (_console.IsOpen) { GamePause.ConsumeEscape(); _console.Close(); return; }
            if (_select.activeSelf) { GamePause.ConsumeEscape(); CloseSelect(); return; }
            if (_menuControls.activeSelf) { GamePause.ConsumeEscape(); _menuControls.SetActive(false); _menuButtons.SetActive(true); Select(_continueButton.interactable ? _continueButton.gameObject : _menuButtons.transform.GetChild(1).gameObject); return; }
            if (_inMenuScene || _storyPauseScene) return; // the story chapters' PauseMenuView owns Esc there.
            GamePause.ConsumeEscape();
            if (_pause.activeSelf) ResumeGeneric();
            else PauseGeneric();
        }

        // ---------------------------------------------------------------- freeze (console / generic pause)

        /// <summary>Stops gameplay behind an overlay: time, audio, player and camera control. Nested calls are counted.</summary>
        private void Freeze()
        {
            if (_freezeCount++ > 0) return;
            _frozeWasPaused = GamePause.IsPaused;
            _frozenTimeScale = Time.timeScale;
            _cursorWasLocked = Cursor.lockState == CursorLockMode.Locked;
            _cursorWasVisible = Cursor.visible;
            _locks.Clear();
            foreach (var target in FindObjectsByType<PlayerMovement>()) { _locks.Add(new LockState { Target = target, Locked = target.ControlLocked }); target.ControlLocked = true; }
            foreach (var target in FindObjectsByType<PlayerInteraction>()) { _locks.Add(new LockState { Target = target, Locked = target.ControlLocked }); target.ControlLocked = true; }
            foreach (var target in FindObjectsByType<ThirdPersonCameraController>()) { _locks.Add(new LockState { Target = target, Locked = target.ControlLocked }); target.ControlLocked = true; }
            Time.timeScale = 0f;
            AudioListener.pause = true;
            GamePause.IsPaused = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Unfreeze()
        {
            if (_freezeCount == 0 || --_freezeCount > 0) return;
            foreach (var state in _locks)
            {
                if (state.Target == null) continue;
                switch (state.Target)
                {
                    case PlayerMovement movement: movement.ControlLocked = state.Locked; break;
                    case PlayerInteraction interaction: interaction.ControlLocked = state.Locked; break;
                    case ThirdPersonCameraController camera: camera.ControlLocked = state.Locked; break;
                }
            }
            _locks.Clear();
            GamePause.IsPaused = _frozeWasPaused;
            if (!_frozeWasPaused) { Time.timeScale = _frozenTimeScale; AudioListener.pause = false; }
            Cursor.lockState = _cursorWasLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = _cursorWasVisible;
        }

        private void SyncOverlayFlag() => GamePause.ShellOverlayOpen = _console.IsOpen || _select.activeSelf || _pause.activeSelf || _menuControls.activeSelf
                                                                    || _settings.IsOpen || _credits.IsOpen || _gallery.IsOpen;

        // ---------------------------------------------------------------- main menu

        private void ShowMenu()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            var last = GameProgress.HasSave ? ChapterCatalog.Get(GameProgress.Last) : null;
            bool canContinue = last != null && ChapterCatalog.InBuild(last);
            _continueButton.interactable = canContinue;
            _continueLabel.text = canContinue
                ? "Tiếp tục  <size=21><color=#FFFFFF88>" + ChapterCatalog.Label(last.Number) + " · " + last.Title +
                  (MidChapterSave.Load(last.Number) is MidChapterSave.Data mid ? " · giữa chương (" + mid.SavedAt + ")" : "") + "</color></size>"
                : "Tiếp tục  <size=21><color=#FFFFFF55>chưa có tiến trình</color></size>";
            _continueButton.GetComponent<ShellButton>().Refresh();
            _menuButtons.SetActive(true);
            Select(canContinue ? _continueButton.gameObject : _menuButtons.transform.GetChild(1).gameObject);
        }

        // ---------------------------------------------------------------- generic pause

        private void PauseGeneric()
        {
            var entry = ChapterCatalog.FindByScene(SceneManager.GetActiveScene().name);
            _pauseChapter.text = entry != null ? (ChapterCatalog.Label(entry.Number) + " · " + entry.Title).ToUpperInvariant() : SceneManager.GetActiveScene().name;
            Freeze();
            _pauseControls.SetActive(false);
            _pause.SetActive(true);
            SyncOverlayFlag();
            Select(_pauseResume.gameObject);
        }

        private void ResumeGeneric()
        {
            if (!_pause.activeSelf) return;
            _pause.SetActive(false);
            SyncOverlayFlag();
            Select(null);
            Unfreeze();
        }

        // ---------------------------------------------------------------- settings & credits

        private void OpenSettings(Action onClose)
        {
            if (_inMenuScene) _menuButtons.SetActive(false);
            bool fromPause = _pause.activeSelf;
            if (fromPause) _pause.SetActive(false);
            _settings.Open(() =>
            {
                if (fromPause) { _pause.SetActive(true); Select(_pauseResume.gameObject); }
                else if (_inMenuScene) ShowMenu();
                SyncOverlayFlag();
                onClose?.Invoke();
            });
            SyncOverlayFlag();
        }

        private void OpenGallery()
        {
            _menuButtons.SetActive(false);
            Select(null);
            _gallery.Open(() => { ShowMenu(); SyncOverlayFlag(); });
            SyncOverlayFlag();
        }

        private void OpenCredits(Action onClose)
        {
            if (_inMenuScene) _menuButtons.SetActive(false);
            Select(null);
            _credits.Open(() =>
            {
                if (_inMenuScene) ShowMenu();
                SyncOverlayFlag();
                onClose?.Invoke();
            });
            SyncOverlayFlag();
        }

        // ---------------------------------------------------------------- chapter select

        private void OpenSelect(Action onClose)
        {
            _selectClosed = onClose;
            if (_pause.activeSelf) { _pause.SetActive(false); _selectClosed = () => { _pause.SetActive(true); SyncOverlayFlag(); Select(_pauseResume.gameObject); }; }
            if (_inMenuScene) _menuButtons.SetActive(false);
            foreach (var row in _rows)
            {
                bool playable = GameProgress.IsUnlocked(row.Entry.Number) && ChapterCatalog.InBuild(row.Entry);
                row.Status.text = Status(row.Entry);
                row.Look.LabelColor = playable ? Color.white : new Color(1, 1, 1, 0.45f);
                row.Look.Refresh();
            }
            _select.SetActive(true);
            SyncOverlayFlag();
            var current = ChapterCatalog.FindByScene(SceneManager.GetActiveScene().name) ?? ChapterCatalog.Get(GameProgress.HasSave ? GameProgress.Last : ChapterCatalog.Prologue);
            var start = _rows.FirstOrDefault(r => r.Entry == current) ?? _rows[0];
            Select(start.Button.gameObject);
            Focus(start.Entry);
        }

        private void CloseSelect()
        {
            _select.SetActive(false);
            SyncOverlayFlag();
            var closed = _selectClosed;
            _selectClosed = null;
            if (_inMenuScene) ShowMenu();
            else Select(null);
            closed?.Invoke();
        }

        private void Focus(ChapterCatalog.Entry entry)
        {
            _focused = entry;
            bool unlocked = GameProgress.IsUnlocked(entry.Number);
            bool inBuild = ChapterCatalog.InBuild(entry);
            bool hasChoices = entry.Number > ChapterCatalog.Prologue && GameProgress.LoadInkStateFor(entry.Number) != null;
            _detailNumber.text = entry.Number == ChapterCatalog.Prologue ? "MỞ" : entry.Number == ChapterCatalog.Epilogue ? "KẾT" : entry.Number.ToString("00");
            _detailTitle.text = entry.Title;
            _detailBlurb.text = entry.Blurb;
            _detailMeta.text = "Scene: " + entry.Scene + "\n" + Status(entry) +
                               (entry.Number > ChapterCatalog.Prologue ? (hasChoices ? "\nCó lựa chọn đã lưu từ " + ChapterCatalog.Label(entry.Number - 1) + "." : "\nChưa có lựa chọn lưu từ phần trước — biến Ink dùng giá trị mặc định.") : "");
            _selectStart.interactable = unlocked && inBuild;
            _selectKeep.gameObject.SetActive(hasChoices);
            _selectKeep.interactable = unlocked && inBuild;
            _selectStart.GetComponent<ShellButton>().Refresh();
            _selectKeep.GetComponent<ShellButton>().Refresh();
        }

        private static string Status(ChapterCatalog.Entry entry)
        {
            if (!ChapterCatalog.InBuild(entry)) return "<color=#FF8A7A>Thiếu trong Build Settings</color>";
            if (!GameProgress.IsUnlocked(entry.Number)) return "<color=#FFFFFF66>Chưa mở khoá</color>";
            if (entry.Placeholder) return "<color=#FFFFFF77>Đang phát triển</color>";
            if (GameProgress.HasSave && entry.Number == GameProgress.Last) return "<color=#E8C27A>Đang chơi</color>";
            if (GameProgress.HasSave && entry.Number <= GameProgress.Reached) return "<color=#9FD49F>Đã tới</color>";
            return "<color=#FFFFFF88>Chưa chơi</color>";
        }

        private void StartFocused(bool keepChoices)
        {
            if (_focused == null || !GameProgress.IsUnlocked(_focused.Number)) return;
            ChapterLoader.LoadChapter(_focused, keepChoices);
        }

        // ---------------------------------------------------------------- loading

        private IEnumerator FadeAndLoad(string scene)
        {
            _loading = true;
            Select(null);
            _console.Close();
            yield return FadeTo(1f, 0.35f);
            SceneManager.LoadScene(scene);
        }

        private IEnumerator FadeTo(float alpha, float duration)
        {
            _fade.blocksRaycasts = alpha > 0.5f;
            float from = _fade.alpha;
            for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
            {
                _fade.alpha = Mathf.Lerp(from, alpha, t / duration);
                yield return null;
            }
            _fade.alpha = alpha;
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null || FindAnyObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            go.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        private static void Select(GameObject target)
        {
            var events = EventSystem.current;
            if (events != null) events.SetSelectedGameObject(target);
        }

        // ---------------------------------------------------------------- UI construction

        private void BuildCanvas()
        {
            var config = Resources.Load<GameShellConfig>(GameShellConfig.ResourcePath);
            TMP_FontAsset font = config != null && config.Font != null ? config.Font : null;
            if (font == null) { var any = FindAnyObjectByType<TextMeshProUGUI>(); font = any != null ? any.font : TMP_Settings.defaultFontAsset; }
            StoryUIBuilder.Font = font;

            var root = new GameObject("ShellCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            _canvas = root.transform;

            BuildMenu();
            BuildPause();
            BuildSelect();
            _settings = SettingsView.Build(_canvas, MakeButton);
            _credits = CreditsView.Build(_canvas);
            _gallery = EndingGalleryView.Build(_canvas);
            _console = DevConsole.Build(_canvas);
            _console.OpenChanged = open =>
            {
                if (open) { _selectedBeforeOverlay = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null; Select(null); Freeze(); }
                else { Unfreeze(); if (_selectedBeforeOverlay != null && _selectedBeforeOverlay.activeInHierarchy) Select(_selectedBeforeOverlay); }
                SyncOverlayFlag();
            };

            var fade = StoryUIBuilder.Image(StoryUIBuilder.Rect(_canvas, "Fade", Vector2.zero, Vector2.one), Color.black);
            fade.raycastTarget = true;
            _fade = fade.gameObject.AddComponent<CanvasGroup>();
            _fade.alpha = 0f;
            _fade.blocksRaycasts = false;
        }

        private void BuildMenu()
        {
            var menu = StoryUIBuilder.Rect(_canvas, "MainMenu", Vector2.zero, Vector2.one);
            StoryUIBuilder.Image(menu, Color.black).raycastTarget = true;
            MenuBackdrop.Build(menu);

            var title = StoryUIBuilder.Text(menu, "Title", "THÚY KIỀU", 124, new Vector2(0.065f, 0.64f), new Vector2(0.7f, 0.82f), StoryUIBuilder.Gold);
            title.fontStyle = FontStyles.Bold;
            title.characterSpacing = 8;
            title.outlineWidth = 0.08f;
            title.outlineColor = new Color32(40, 18, 8, 200);
            var subtitle = StoryUIBuilder.Text(menu, "Subtitle", "NGHỊCH MỆNH", 32, new Vector2(0.07f, 0.585f), new Vector2(0.6f, 0.64f), new Color(1, 1, 1, 0.78f));
            subtitle.characterSpacing = 30;
            StoryUIBuilder.Image(StoryUIBuilder.Rect(menu, "Rule", new Vector2(0.072f, 0.572f), new Vector2(0.2f, 0.574f)), StoryUIBuilder.Vermilion);

            var buttons = StoryUIBuilder.Rect(menu, "Buttons", new Vector2(0.065f, 0.15f), new Vector2(0.42f, 0.555f));
            _menuButtons = buttons.gameObject;
            string[] labels = { "Tiếp tục", "Chơi mới", "Chọn chương", "Các kết", "Cài đặt", "Điều khiển", "Ghi công", "Thoát game" };
            Action[] actions =
            {
                () => ChapterLoader.Continue(),
                () => ChapterLoader.LoadChapter(ChapterCatalog.Prologue, false),
                () => OpenSelect(null),
                OpenGallery,
                () => OpenSettings(null),
                () => { _menuButtons.SetActive(false); _menuControls.SetActive(true); SyncOverlayFlag(); },
                () => OpenCredits(null),
                QuitGame
            };
            float rowStep = 1f / labels.Length;
            for (int i = 0; i < labels.Length; i++)
            {
                float top = 1f - i * rowStep;
                var button = MakeButton(buttons, "Button" + i, labels[i], new Vector2(0f, top - rowStep * 0.86f), new Vector2(1f, top), actions[i], 34);
                if (i == 0) { _continueButton = button; _continueLabel = button.GetComponent<ShellButton>().Label; }
            }

            var verse = StoryUIBuilder.Text(menu, "Verse", "Trăm năm trong cõi người ta,\nChữ tài chữ mệnh khéo là ghét nhau.  <size=18>— Nguyễn Du</size>",
                22, new Vector2(0.07f, 0.06f), new Vector2(0.6f, 0.15f), new Color(1, 1, 1, 0.45f));
            verse.fontStyle = FontStyles.Italic;
            var footer = StoryUIBuilder.Text(menu, "Footer", "PRU213 · bản thử nghiệm   <color=#E8C27A>`</color> bảng lệnh",
                18, new Vector2(0.6f, 0.02f), new Vector2(0.985f, 0.06f), new Color(1, 1, 1, 0.4f));
            footer.alignment = TextAlignmentOptions.BottomRight;

            var controls = StoryUIBuilder.Panel(menu, "Controls", new Vector2(0.065f, 0.14f), new Vector2(0.42f, 0.56f), new Color(0.05f, 0.04f, 0.05f, 0.88f), 26);
            var text = StoryUIBuilder.Text(controls, "Text", StoryUIBuilder.ControlsHelp, 24, new Vector2(0.07f, 0.12f), new Vector2(0.93f, 0.95f), Color.white);
            text.alignment = TextAlignmentOptions.TopLeft;
            text.lineSpacing = 10;
            StoryUIBuilder.Text(controls, "Hint", "Esc: quay lại", 19, new Vector2(0.07f, 0.02f), new Vector2(0.93f, 0.1f), new Color(1, 1, 1, 0.5f)).alignment = TextAlignmentOptions.Right;
            _menuControls = controls.gameObject;
            _menuControls.SetActive(false);
            _menu = menu.gameObject;
            _menu.SetActive(false);
        }

        private void BuildPause()
        {
            var pause = StoryUIBuilder.Rect(_canvas, "Pause", Vector2.zero, Vector2.one);
            StoryUIBuilder.Image(pause, new Color(0.02f, 0.01f, 0.02f, 0.8f)).raycastTarget = true;
            var panel = StoryUIBuilder.Panel(pause, "Panel", new Vector2(0.36f, 0.16f), new Vector2(0.64f, 0.84f), new Color(0.07f, 0.05f, 0.06f, 0.97f), 26);
            var title = StoryUIBuilder.Text(panel, "Title", "TẠM DỪNG", 42, new Vector2(0.08f, 0.88f), new Vector2(0.92f, 0.97f), StoryUIBuilder.Gold);
            title.alignment = TextAlignmentOptions.Center; title.fontStyle = FontStyles.Bold;
            _pauseChapter = StoryUIBuilder.Text(panel, "Chapter", "", 20, new Vector2(0.08f, 0.82f), new Vector2(0.92f, 0.88f), new Color(1, 1, 1, 0.55f));
            _pauseChapter.alignment = TextAlignmentOptions.Center; _pauseChapter.characterSpacing = 4;

            string[] labels = { "Tiếp tục", "Chơi lại chương", "Chọn chương", "Cài đặt", "Điều khiển", "Về menu chính", "Thoát game" };
            Action[] actions =
            {
                ResumeGeneric,
                () => ChapterLoader.LoadScene(SceneManager.GetActiveScene().name),
                () => OpenSelect(null),
                () => OpenSettings(null),
                () => _pauseControls.SetActive(!_pauseControls.activeSelf),
                GoToMainMenu,
                QuitGame
            };
            for (int i = 0; i < labels.Length; i++)
            {
                float top = 0.8f - i * 0.108f;
                var button = MakeButton(panel, "Button" + i, labels[i], new Vector2(0.1f, top - 0.088f), new Vector2(0.9f, top), actions[i], 27, i == 0);
                if (i == 0) _pauseResume = button;
            }

            var controls = StoryUIBuilder.Panel(panel, "ControlsText", new Vector2(1.04f, 0.08f), new Vector2(1.9f, 0.92f), new Color(0.07f, 0.05f, 0.06f, 0.97f), 26);
            var text = StoryUIBuilder.Text(controls, "Text", StoryUIBuilder.ControlsHelp, 24, new Vector2(0.08f, 0.05f), new Vector2(0.92f, 0.95f), Color.white);
            text.alignment = TextAlignmentOptions.TopLeft; text.lineSpacing = 12;
            _pauseControls = controls.gameObject;
            _pauseControls.SetActive(false);
            _pause = pause.gameObject;
            _pause.SetActive(false);
        }

        private void BuildSelect()
        {
            var select = StoryUIBuilder.Rect(_canvas, "ChapterSelect", Vector2.zero, Vector2.one);
            StoryUIBuilder.Image(select, new Color(0.015f, 0.01f, 0.02f, 0.94f)).raycastTarget = true;
            var panel = StoryUIBuilder.Panel(select, "Panel", new Vector2(0.1f, 0.08f), new Vector2(0.9f, 0.92f), new Color(0.06f, 0.045f, 0.055f, 0.98f), 26);
            StoryUIBuilder.Text(panel, "Title", "CHỌN CHƯƠNG", 40, new Vector2(0.035f, 0.89f), new Vector2(0.5f, 0.98f), StoryUIBuilder.Gold).fontStyle = FontStyles.Bold;
            var hint = StoryUIBuilder.Text(panel, "Hint", "Chuột / mũi tên chọn · 1–9 nhảy nhanh · Enter chơi · Esc quay lại", 19,
                new Vector2(0.45f, 0.89f), new Vector2(0.97f, 0.98f), new Color(1, 1, 1, 0.5f));
            hint.alignment = TextAlignmentOptions.MidlineRight;
            StoryUIBuilder.Image(StoryUIBuilder.Rect(panel, "Rule", new Vector2(0.035f, 0.885f), new Vector2(0.965f, 0.887f)), new Color(1f, 0.8f, 0.45f, 0.3f));

            var list = StoryUIBuilder.Rect(panel, "List", new Vector2(0.03f, 0.04f), new Vector2(0.5f, 0.87f));
            float step = 1f / ChapterCatalog.All.Count;
            for (int i = 0; i < ChapterCatalog.All.Count; i++)
            {
                var entry = ChapterCatalog.All[i];
                float top = 1f - i * step;
                string number = entry.Number == ChapterCatalog.Prologue || entry.Number == ChapterCatalog.Epilogue ? ChapterCatalog.Label(entry.Number).ToUpperInvariant() : "CHƯƠNG " + entry.Number.ToString("00");
                var label = "<size=19><color=#E8C27A>" + number + "</color></size>\n" + entry.Title;
                var button = MakeButton(list, "Row" + entry.Number, label, new Vector2(0f, top - step * 0.9f), new Vector2(1f, top), () => StartFocused(false), 28);
                var look = button.GetComponent<ShellButton>();
                look.BackgroundColor = RowColor;
                look.BackgroundHighlight = RowHighlight;
                look.Label.lineSpacing = -8;
                look.Selected = () => Focus(entry);
                look.Setup();
                var status = StoryUIBuilder.Text(button.transform, "Status", "", 19, new Vector2(0.6f, 0f), new Vector2(0.97f, 1f), Color.white);
                status.alignment = TextAlignmentOptions.MidlineRight;
                _rows.Add(new ChapterRow { Entry = entry, Button = button, Look = look, Status = status });
            }

            var detail = StoryUIBuilder.Rect(panel, "Detail", new Vector2(0.54f, 0.04f), new Vector2(0.97f, 0.87f));
            _detailNumber = StoryUIBuilder.Text(detail, "Number", "", 150, new Vector2(0f, 0.68f), new Vector2(1f, 1f), new Color(1f, 0.8f, 0.45f, 0.18f));
            _detailNumber.fontStyle = FontStyles.Bold;
            _detailTitle = StoryUIBuilder.Text(detail, "Title", "", 50, new Vector2(0f, 0.6f), new Vector2(1f, 0.74f), Color.white);
            _detailTitle.fontStyle = FontStyles.Bold;
            _detailTitle.enableAutoSizing = true; _detailTitle.fontSizeMin = 30; _detailTitle.fontSizeMax = 50;
            _detailBlurb = StoryUIBuilder.Text(detail, "Blurb", "", 27, new Vector2(0f, 0.44f), new Vector2(1f, 0.6f), new Color(1, 1, 1, 0.82f));
            _detailBlurb.alignment = TextAlignmentOptions.TopLeft;
            _detailMeta = StoryUIBuilder.Text(detail, "Meta", "", 19, new Vector2(0f, 0.28f), new Vector2(1f, 0.44f), new Color(1, 1, 1, 0.5f));
            _detailMeta.alignment = TextAlignmentOptions.TopLeft;
            _selectStart = MakeButton(detail, "Start", "Chơi từ đầu chương", new Vector2(0f, 0.17f), new Vector2(1f, 0.26f), () => StartFocused(false), 28, true);
            _selectKeep = MakeButton(detail, "Keep", "Chơi với lựa chọn đã lưu", new Vector2(0f, 0.065f), new Vector2(0.62f, 0.155f), () => StartFocused(true), 24);
            _selectBack = MakeButton(detail, "Back", "Quay lại", new Vector2(0.65f, 0.065f), new Vector2(1f, 0.155f), CloseSelect, 24);
            foreach (var b in new[] { _selectKeep, _selectBack })
            {
                var look = b.GetComponent<ShellButton>();
                look.BackgroundColor = new Color(0.15f, 0.115f, 0.125f, 1f);
                look.BackgroundHighlight = RowHighlight;
                look.Setup();
            }
            _select = select.gameObject;
            _select.SetActive(false);
        }

        private static Button MakeButton(Transform parent, string name, string label, Vector2 min, Vector2 max, Action onClick, float fontSize, bool filled = false)
        {
            var rect = StoryUIBuilder.Panel(parent, name, min, max, Color.clear, 14);
            var background = rect.GetComponent<Image>();
            var button = rect.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = background;
            button.onClick.AddListener(() => onClick());

            var accent = StoryUIBuilder.Rect(rect, "Accent", new Vector2(0f, 0.2f), new Vector2(0f, 0.8f));
            accent.offsetMax = new Vector2(5f, 0f);
            StoryUIBuilder.Image(accent, filled ? Color.white : StoryUIBuilder.Gold).raycastTarget = false;
            var text = StoryUIBuilder.Text(rect, "Label", label, fontSize, Vector2.zero, Vector2.one, Color.white);
            text.rectTransform.offsetMin = new Vector2(24f, 0f);
            text.alignment = TextAlignmentOptions.Left; // font-metric centring: Vietnamese stacked marks would shift a geometry-centred label
            text.richText = true;

            var look = rect.gameObject.AddComponent<ShellButton>();
            look.Accent = accent;
            look.Label = text;
            look.Background = background;
            look.BackgroundHighlight = MenuHighlight;
            if (filled)
            {
                look.BackgroundColor = StoryUIBuilder.Vermilion;
                look.BackgroundHighlight = new Color(0.86f, 0.25f, 0.18f);
                look.HighlightColor = Color.white;
            }
            look.Setup();
            return button;
        }
    }
}
