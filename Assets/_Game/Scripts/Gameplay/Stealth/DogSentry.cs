using ThuyKieu.Player;
using ThuyKieu.Presentation;
using UnityEngine;

namespace ThuyKieu.Gameplay.Stealth
{
    /// <summary>
    /// A chained dog that hears rather than sees: walking upright close by, or running anywhere near, sets it barking,
    /// which draws the guards' attention. Sneaking (crouched) past at a distance stays quiet.
    /// </summary>
    public class DogSentry : MonoBehaviour
    {
        [SerializeField] private PlayerMovement _player;
        [SerializeField] private StoryAudio _audio;
        [SerializeField] private float _walkHearing = 3.6f;
        [SerializeField] private float _runHearing = 8f;
        [SerializeField] private float _crouchHearing = 1.6f;
        [SerializeField] private Transform _head;

        private float _cooldown, _growl;

        public bool Active { get; set; } = true;
        public int Barks { get; private set; }
        public float Unease => _growl;

        public void ResetDog() { _cooldown = 0f; _growl = 0f; }
        public void ResetCount() { Barks = 0; }

        private void Update()
        {
            if (_cooldown > 0f) _cooldown -= Time.deltaTime;
            if (!Active || _player == null || _player.ControlLocked || _player.IsHidden) { _growl = Mathf.Max(0f, _growl - Time.deltaTime); return; }
            float distance = Vector3.Distance(transform.position, _player.transform.position);
            float hearing = _player.IsRunning ? _runHearing : _player.IsCrouching ? _crouchHearing : _walkHearing;
            bool hears = distance < hearing && _player.CurrentSpeed > 0.2f;
            _growl = hears ? _growl + Time.deltaTime * (_player.IsRunning ? 3f : 1.2f) : Mathf.Max(0f, _growl - Time.deltaTime * 0.6f);
            if (_head != null && hears)
            {
                Vector3 look = _player.transform.position - transform.position;
                look.y = 0f;
                if (look.sqrMagnitude > 0.01f) transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(look), 180f * Time.deltaTime);
            }
            if (_growl < 1f || _cooldown > 0f) return;
            Bark();
        }

        private void Bark()
        {
            _growl = 0f;
            _cooldown = 4f;
            Barks++;
            if (_audio != null) _audio.PlaySfx("dog");
            foreach (var guard in FindObjectsByType<GuardPatrol>())
            {
                guard.Investigate(transform.position + transform.forward * 1.5f, 4f);
                if (guard.Vision != null) guard.Vision.Alert(0.3f);
            }
        }
    }
}
