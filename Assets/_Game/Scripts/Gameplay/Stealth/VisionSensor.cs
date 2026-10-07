using ThuyKieu.Player;
using UnityEngine;

namespace ThuyKieu.Gameplay.Stealth
{
    /// <summary>
    /// Cone of sight from an NPC's eyes towards the player. Builds up an awareness value (0–1)
    /// while the player is visible; crouching, distance and darkness slow it, losing sight drains it.
    /// Optionally only counts while the player stands in a <see cref="RestrictedZone"/>, or only while
    /// the player draws attention by running.
    /// </summary>
    public class VisionSensor : MonoBehaviour
    {
        [SerializeField] private Transform _eyes;
        [SerializeField] private PlayerMovement _player;
        [SerializeField] private float _range = 7.5f;
        [SerializeField, Range(10, 170)] private float _angle = 70f;
        [SerializeField] private float _secondsToSpot = 1.6f;
        [SerializeField] private float _crouchFactor = 0.45f;
        [SerializeField] private bool _onlyInRestrictedZones = true;
        [Tooltip("Crowded street: only a running player stands out.")]
        [SerializeField] private bool _onlyWhenConspicuous;
        [SerializeField] private LayerMask _occluders = 1 << 8;

        public float Awareness { get; private set; }
        public bool SeesPlayer { get; private set; }
        public bool Spotted => Awareness >= 1f;
        public float Range => _range;
        public float Angle => _angle;
        public Transform Eyes => _eyes != null ? _eyes : transform;
        public bool Active { get; set; } = true;
        public PlayerMovement Player => _player;
        /// <summary>Scales how fast suspicion grows (a drunk guard is slower, an alerted one faster).</summary>
        public float Sensitivity { get; set; } = 1f;

        public void ResetAwareness() { Awareness = 0f; SeesPlayer = false; }

        /// <summary>A noise or a glimpse from elsewhere raises suspicion without line of sight.</summary>
        public void Alert(float amount) { if (Active) Awareness = Mathf.Clamp(Awareness + amount, 0f, 0.95f); }

        private void Update()
        {
            SeesPlayer = Active && CanSee();
            if (SeesPlayer)
            {
                float distance = Vector3.Distance(Eyes.position, _player.transform.position);
                float closeness = Mathf.Lerp(2.2f, 0.6f, distance / _range);
                float stealth = _player.IsCrouching ? _crouchFactor : 1f;
                float light = LightExposure.Enabled ? Mathf.Lerp(0.45f, 1.3f, LightExposure.At(_player.transform.position + Vector3.up)) : 1f;
                Awareness = Mathf.Min(1f, Awareness + Time.deltaTime / _secondsToSpot * closeness * stealth * light * Sensitivity);
            }
            else Awareness = Mathf.Max(0f, Awareness - Time.deltaTime * 0.35f);
        }

        private bool CanSee()
        {
            if (_player == null || _player.ControlLocked || _player.IsHidden) return false;
            if (_onlyInRestrictedZones && !RestrictedZone.PlayerInside) return false;
            if (_onlyWhenConspicuous && !_player.IsRunning) return false;
            Vector3 eye = Eyes.position;
            Vector3 target = _player.transform.position + Vector3.up * (_player.IsCrouching ? 0.8f : 1.4f);
            Vector3 toTarget = target - eye;
            if (toTarget.magnitude > _range) return false;
            Vector3 flat = new Vector3(toTarget.x, 0, toTarget.z);
            Vector3 forward = new Vector3(Eyes.forward.x, 0, Eyes.forward.z);
            if (Vector3.Angle(forward, flat) > _angle * 0.5f) return false;
            return !Physics.Linecast(eye, target, _occluders, QueryTriggerInteraction.Ignore);
        }

        private void OnDrawGizmosSelected()
        {
            Transform eyes = Eyes;
            Gizmos.color = new Color(1, 0.3f, 0.2f, 0.6f);
            Vector3 left = Quaternion.Euler(0, -_angle * 0.5f, 0) * eyes.forward * _range;
            Vector3 right = Quaternion.Euler(0, _angle * 0.5f, 0) * eyes.forward * _range;
            Gizmos.DrawLine(eyes.position, eyes.position + left);
            Gizmos.DrawLine(eyes.position, eyes.position + right);
        }
    }
}
