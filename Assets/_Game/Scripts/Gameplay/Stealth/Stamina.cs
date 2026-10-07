using ThuyKieu.Player;
using UnityEngine;

namespace ThuyKieu.Gameplay.Stealth
{
    /// <summary>Sprinting drains breath; when it runs out Kiều can only walk until she recovers a little.</summary>
    public class Stamina : MonoBehaviour
    {
        [SerializeField] private PlayerMovement _player;
        [SerializeField] private float _seconds = 6f;
        [SerializeField] private float _recovery = 0.22f;
        [Tooltip("Carrying the whole bag of silver makes running harder.")]
        [SerializeField] private float _load = 1f;

        public float Value { get; private set; } = 1f;
        public bool Exhausted { get; private set; }
        public float Load { get => _load; set => _load = Mathf.Max(0.5f, value); }

        public void Refill() { Value = 1f; Exhausted = false; _player.CanRun = true; }

        private void Update()
        {
            if (_player.IsRunning) Value = Mathf.Max(0f, Value - Time.deltaTime / _seconds * _load);
            else Value = Mathf.Min(1f, Value + Time.deltaTime * _recovery * (_player.CurrentSpeed < 0.1f ? 1.6f : 1f));
            if (Value <= 0f) Exhausted = true;
            else if (Exhausted && Value > 0.3f) Exhausted = false;
            _player.CanRun = !Exhausted;
        }

        private void OnDisable() { if (_player != null) _player.CanRun = true; }
    }
}
