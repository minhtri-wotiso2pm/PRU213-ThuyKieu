using ThuyKieu.Core;
using ThuyKieu.Core.Story;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThuyKieu.UI
{
    /// <summary>Esc pause menu for story chapters: resume, journal, controls, restart chapter, quit.</summary>
    public class PauseMenuView : MonoBehaviour
    {
        public static bool IsPaused { get; private set; }

        private GameObject _root, _controls;
        private StoryDirector _director;
        private JournalView _journal;
        private bool _playerWasLocked;
        private float _timeScale = 1f;

        private void Awake()
        {
            _root = transform.Find("Pause").gameObject;
            _controls = transform.Find("Pause/Panel/ControlsText").gameObject;
            Bind("Pause/Panel/Resume", Resume);
            Bind("Pause/Panel/Journal", OpenJournal);
            Bind("Pause/Panel/Controls", () => _controls.SetActive(!_controls.activeSelf));
            Bind("Pause/Panel/Restart", Restart);
            Bind("Pause/Panel/Quit", Quit);
            _root.SetActive(false);
            _controls.SetActive(false);
        }

        private void Start()
        {
            _director = FindAnyObjectByType<StoryDirector>();
            _journal = GetComponent<JournalView>();
        }

        private void OnDestroy() { if (IsPaused) { Time.timeScale = 1f; AudioListener.pause = false; IsPaused = false; } }

        private void Bind(string path, UnityEngine.Events.UnityAction action)
        {
            var button = StoryUIBuilder.Find<Button>(transform, path);
            if (button != null) button.onClick.AddListener(action);
        }

        private void Update()
        {
            if (_director == null || _director.Autopilot || !InputReader.CancelPressedThisFrame) return;
            if (_journal != null && (_journal.IsOpen || _journal.ClosedFrame == Time.frameCount)) return; // Esc closes the journal first.
            if (_director.IsBusy && _director.CurrentSequence == null) return; // failure card in progress
            if (IsPaused) Resume();
            else if (!_director.ChapterComplete) Pause();
        }

        public void Pause()
        {
            IsPaused = true;
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
            _root.SetActive(false);
            Time.timeScale = _timeScale;
            AudioListener.pause = false;
            IsPaused = false;
            if (!_playerWasLocked) _director.SetFreeControl(true);
            else _director.CameraController.SetCursorLocked(false);
        }

        private void OpenJournal()
        {
            if (!_director.InExplore) return;
            Resume();
            _journal.Open();
        }

        private void Restart()
        {
            Resume();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void Quit()
        {
            Resume();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
