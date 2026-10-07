using ThuyKieu.Core;
using ThuyKieu.Dialogue;
using ThuyKieu.Player;
using ThuyKieu.Presentation;
using UnityEngine;

namespace ThuyKieu.Environment
{
    // Scene-local presentation. Empty clip slots fall back to ProceduralAudio until real assets are assigned.
    public sealed class Chapter01Audio : MonoBehaviour
    {
        [SerializeField] private Chapter01Director _director;
        [SerializeField] private DialogueUI _dialogueUI;
        [SerializeField] private PlayerMovement _player;
        [SerializeField] private AudioSource _musicSource, _rainSource, _windSource, _sfxSource, _uiSource, _footstepSource;
        [SerializeField] private AudioClip _paper, _doorOpen, _doorClose, _uiConfirm, _woodStep, _stoneStep;
        private float _stepDistance;
        private float _musicVolume = .18f;
        private CharacterController _controller;

        private void OnEnable()
        {
            if (_director != null) _director.CueReceived += OnCue;
            if (_dialogueUI != null) _dialogueUI.InteractionConfirmed += PlayUI;
        }
        private void Start()
        {
            if (_player != null) _controller = _player.GetComponent<CharacterController>();
            FillMissingClips();
            StartLoop(_musicSource); StartLoop(_rainSource); StartLoop(_windSource);
        }

        /// <summary>Slots without recorded audio use the synthesised placeholders shared with Chapters 2–3.</summary>
        private void FillMissingClips()
        {
            FillLoop(_musicSource, ProceduralAudio.Music("sad_strings"));
            FillLoop(_rainSource, ProceduralAudio.Ambience("rain"));
            FillLoop(_windSource, ProceduralAudio.Ambience("wind"));
            if (_paper == null) _paper = ProceduralAudio.Sfx("paper");
            if (_doorOpen == null) _doorOpen = ProceduralAudio.Sfx("door");
            if (_doorClose == null) _doorClose = ProceduralAudio.Sfx("door");
            if (_uiConfirm == null) _uiConfirm = ProceduralAudio.Sfx("ui");
            if (_woodStep == null) _woodStep = ProceduralAudio.Sfx("step");
            if (_stoneStep == null) _stoneStep = ProceduralAudio.Sfx("step");
        }

        private static void FillLoop(AudioSource source, AudioClip clip)
        {
            if (source == null || source.clip != null) return;
            source.clip = clip;
            source.spatialBlend = 0f;
        }
        private void OnDisable()
        {
            if (_director != null) _director.CueReceived -= OnCue;
            if (_dialogueUI != null) _dialogueUI.InteractionConfirmed -= PlayUI;
            foreach (var source in new[]{_musicSource, _rainSource, _windSource}) if (source != null) source.Stop();
        }
        private static void StartLoop(AudioSource source)
        { if (source != null && source.clip != null) { source.loop = true; source.Play(); } }
        private void OnCue(string key, string value)
        {
            if (key == "music" && value == "sad_strings") { _musicVolume = .18f; StartLoop(_musicSource); }
            if (key != "camera") return;
            if (value == "Reveal_MaGiamSinh") _musicVolume = .21f;
            if (value == "Table_Contract") { _musicVolume = .23f; PlayAt(_paper, new Vector3(0, .85f, 3.1f)); }
            if (value == "Hero_Kieu_Rain") _musicVolume = .16f;
        }
        private void Update()
        {
            if (_player == null) return;
            bool inside = Mathf.Abs(_player.transform.position.x) < 6.3f && _player.transform.position.z > -3.2f;
            float blend = 1 - Mathf.Exp(-3 * Time.deltaTime);
            if (_musicSource != null) _musicSource.volume = Mathf.Lerp(_musicSource.volume, _musicVolume * ThuyKieu.Core.GameSettings.Music, blend);
            if (_rainSource != null) _rainSource.volume = Mathf.Lerp(_rainSource.volume, (inside ? .13f : .28f) * ThuyKieu.Core.GameSettings.Ambience, blend);
            if (_windSource != null) _windSource.volume = Mathf.Lerp(_windSource.volume, (inside ? .025f : .07f) * ThuyKieu.Core.GameSettings.Ambience, blend);
            if (_player.ControlLocked || _controller == null || !_controller.isGrounded || _player.CurrentSpeed < .1f)
            { _stepDistance = 0; return; }
            _stepDistance += _player.CurrentSpeed * Time.deltaTime;
            if (_stepDistance < (_player.IsRunning ? 1.6f : 1.05f)) return;
            _stepDistance = 0;
            AudioClip step = inside ? _woodStep : _stoneStep;
            if (step != null && _footstepSource != null)
            { _footstepSource.transform.position = _player.transform.position; _footstepSource.PlayOneShot(step); }
        }
        public void PlayDoor(bool open, Vector3 position) { PlayAt(open ? _doorOpen : _doorClose, position); }
        private void PlayAt(AudioClip clip, Vector3 position)
        { if (clip != null && _sfxSource != null) { _sfxSource.transform.position = position; _sfxSource.PlayOneShot(clip); } }
        private void PlayUI() { if (_uiConfirm != null && _uiSource != null) _uiSource.PlayOneShot(_uiConfirm); }
    }
}
