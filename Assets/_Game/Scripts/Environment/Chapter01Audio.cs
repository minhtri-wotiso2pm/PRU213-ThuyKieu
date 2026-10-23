using ThuyKieu.Player;
using ThuyKieu.Presentation;
using UnityEngine;

namespace ThuyKieu.Environment
{
    /// <summary>
    /// Chapter 1 scene sound: rain and wind that thin out under the roof, wood or stone footsteps, and the doors.
    /// Music and story effects come from the chapter's StoryAudio like every other chapter.
    /// Empty clip slots take the Chapter 1 clips from AudioBank.
    /// </summary>
    public sealed class Chapter01Audio : MonoBehaviour
    {
        [SerializeField] private PlayerMovement _player;
        [SerializeField] private AudioSource _rainSource, _windSource, _sfxSource, _footstepSource;
        [SerializeField] private AudioClip _doorOpen, _doorClose, _woodStep, _stoneStep;
        private float _stepDistance;
        private CharacterController _controller;

        private void Start()
        {
            if (_player != null) _controller = _player.GetComponent<CharacterController>();
            FillLoop(_rainSource, AudioBank.Get(AudioBank.Ambience, "rain"));
            FillLoop(_windSource, AudioBank.Get(AudioBank.Ambience, "wind"));
            if (_doorOpen == null) _doorOpen = AudioBank.Get(AudioBank.Sfx, "door_open");
            if (_doorClose == null) _doorClose = AudioBank.Get(AudioBank.Sfx, "door_close");
            if (_woodStep == null) _woodStep = AudioBank.Get(AudioBank.Sfx, "step_wood");
            if (_stoneStep == null) _stoneStep = AudioBank.Get(AudioBank.Sfx, "step_stone");
            StartLoop(_rainSource);
            StartLoop(_windSource);
        }

        private static void FillLoop(AudioSource source, AudioClip clip)
        {
            if (source == null || source.clip != null) return;
            source.clip = clip;
            source.spatialBlend = 0f;
        }

        private void OnDisable()
        {
            foreach (var source in new[] { _rainSource, _windSource }) if (source != null) source.Stop();
        }

        private static void StartLoop(AudioSource source)
        { if (source != null && source.clip != null) { source.loop = true; source.Play(); } }

        private void Update()
        {
            if (_player == null) return;
            bool inside = Mathf.Abs(_player.transform.position.x) < 6.3f && _player.transform.position.z > -3.2f;
            float blend = 1 - Mathf.Exp(-3 * Time.deltaTime);
            if (_rainSource != null) _rainSource.volume = Mathf.Lerp(_rainSource.volume, (inside ? .13f : .28f) * ThuyKieu.Core.GameSettings.Ambience, blend);
            if (_windSource != null) _windSource.volume = Mathf.Lerp(_windSource.volume, (inside ? .025f : .07f) * ThuyKieu.Core.GameSettings.Ambience, blend);
            if (_player.ControlLocked || _controller == null || !_controller.isGrounded || _player.CurrentSpeed < .1f)
            { _stepDistance = 0; return; }
            _stepDistance += _player.CurrentSpeed * Time.deltaTime;
            if (_stepDistance < (_player.IsRunning ? 1.6f : 1.05f)) return;
            _stepDistance = 0;
            AudioClip step = inside ? _woodStep : _stoneStep;
            if (step != null && _footstepSource != null)
            { _footstepSource.transform.position = _player.transform.position; _footstepSource.PlayOneShot(step, ThuyKieu.Core.GameSettings.Effects); }
        }

        public void PlayDoor(bool open, Vector3 position)
        {
            AudioClip clip = open ? _doorOpen : _doorClose;
            if (clip != null && _sfxSource != null) { _sfxSource.transform.position = position; _sfxSource.PlayOneShot(clip, ThuyKieu.Core.GameSettings.Effects); }
        }
    }
}
