using ThuyKieu.Core;
using ThuyKieu.Core.Story;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThuyKieu.UI
{
    /// <summary>Esc pause menu for story chapters: resume, journal, controls, restart chapter, chapter select, main menu, quit.</summary>
    public class PauseMenuView : MonoBehaviour
    {
        public static readonly string[] ButtonNames = { "Resume", "Journal", "Controls", "Restart", "ChapterSelect", "MainMenu", "Quit" };
        public static readonly string[] ButtonLabels = { "Tiếp tục", "Sổ tay manh mối", "Điều khiển", "Chơi lại chương", "Chọn chương", "Về menu chính", "Thoát game" };

        public static bool IsPaused => GamePause.IsPaused;

        private GameObject _root, _controls;
        private StoryDirector _director;
        private JournalView _journal;
        private bool _playerWasLocked, _paused;
        private float _timeScale = 1f;

        private void Awake()
        {
            _root = transform.Find("Pause").gameObject;
            _controls = transform.Find("Pause/Panel/ControlsText").gameObject;
            EnsureButtons(transform.Find("Pause/Panel"));
            Bind("Pause/Panel/Resume", Resume);
            Bind("Pause/Panel/Journal", OpenJournal);
            Bind("Pause/Panel/Controls", () => _controls.SetActive(!_controls.activeSelf));
            Bind("Pause/Panel/Restart", Restart);
            Bind("Pause/Panel/ChapterSelect", OpenChapterSelect);
            Bind("Pause/Panel/MainMenu", () => { Resume(); GameShell.GoToMainMenu(); });
            Bind("Pause/Panel/Quit", Quit);
            _root.SetActive(false);
            _controls.SetActive(false);
        }

        /// <summary>Scenes generated before chapter select existed only have five buttons: clone the missing ones.</summary>
        private static void EnsureButtons(Transform panel)
        {
            var template = panel.Find("Quit");
            if (template == null) return;
            for (int i = 0; i < ButtonNames.Length; i++)
            {
                if (panel.Find(ButtonNames[i]) != null) continue;
                var copy = Instantiate(template.gameObject, panel);
                copy.name = ButtonNames[i];
                var label = copy.GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = ButtonLabels[i];
            }
            LayoutButtons(panel);
        }

        /// <summary>Stacks the buttons under the title, in ButtonNames order.</summary>
        public static void LayoutButtons(Transform panel)
        {
            var rect = (RectTransform)panel;
            rect.anchorMin = new Vector2(0.36f, 0.12f);
            rect.anchorMax = new Vector2(0.64f, 0.88f);
            var title = (RectTransform)panel.Find("Title");
            if (title != null) { title.anchorMin = new Vector2(0.08f, 0.89f); title.anchorMax = new Vector2(0.92f, 0.98f); }
            const float step = 0.118f, height = 0.095f;
            for (int i = 0; i < ButtonNames.Length; i++)
            {
                var button = panel.Find(ButtonNames[i]) as RectTransform;
                if (button == null) continue;
                float top = 0.87f - i * step;
                button.anchorMin = new Vector2(0.12f, top - height);
                button.anchorMax = new Vector2(0.88f, top);
                button.offsetMin = button.offsetMax = Vector2.zero;
                button.SetSiblingIndex(i + 1);
            }
        }

        private void Start()
        {
            _director = FindAnyObjectByType<StoryDirector>();
            _journal = GetComponent<JournalView>();
        }

        private void OnDestroy() { if (_paused) { Time.timeScale = 1f; AudioListener.pause = false; GamePause.IsPaused = false; } }

        private void Bind(string path, UnityEngine.Events.UnityAction action)
        {
            var button = StoryUIBuilder.Find<Button>(transform, path);
            if (button != null) button.onClick.AddListener(action);
        }

        private void Update()
        {
            if (_director == null || _director.Autopilot || !InputReader.CancelPressedThisFrame) return;
            if (GamePause.ShellOverlayOpen || GamePause.EscapeConsumed) return; // chapter select / console handle their own Esc.
            if (_journal != null && (_journal.IsOpen || _journal.ClosedFrame == Time.frameCount)) return; // Esc closes the journal first.
            if (_director.IsBusy && _director.CurrentSequence == null) return; // failure card in progress
            GamePause.ConsumeEscape();
            if (_paused) Resume();
            else if (!_director.ChapterComplete) Pause();
        }

        public void Pause()
        {
            _paused = true;
            GamePause.IsPaused = true;
            _playerWasLocked = _director.Player.ControlLocked;
            _director.SetFreeControl(false);
            _director.CameraController.SetCursorLocked(false);
            _timeScale = Time.timeScale;
            Time.timeScale = 0f;
            AudioListener.pause = true;
            _controls.SetActive(false);
            _root.SetActive(true);
        }

        public void Resume()
        {
            if (!_paused) return;
            _root.SetActive(false);
            Time.timeScale = _timeScale;
            AudioListener.pause = false;
            _paused = false;
            GamePause.IsPaused = false;
            if (!_playerWasLocked) _director.SetFreeControl(true);
            else _director.CameraController.SetCursorLocked(false);
        }

        private void OpenJournal()
        {
            if (!_director.InExplore) return;
            Resume();
            _journal.Open();
        }

        private void OpenChapterSelect()
        {
            _root.SetActive(false);
            GameShell.ShowChapterSelect(() => { if (_paused) _root.SetActive(true); });
        }

        private void Restart()
        {
            Resume();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void Quit()
        {
            Resume();
            GameShell.QuitGame();
        }
    }
}
