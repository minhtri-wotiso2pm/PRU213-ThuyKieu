using System.Collections;
using ThuyKieu.Core.Story;
using ThuyKieu.Player;
using ThuyKieu.Presentation;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace ThuyKieu.Gameplay.Chase
{
    /// <summary>
    /// A rotten culm, loosened by the night's wind and rain, that gives way across the path as Kiều comes: first it cracks
    /// and sways (heard from where it stands, seen leaning), then it falls about its foot and lies across the way as one
    /// more thing to jump or duck. Its collider is the culm itself, so nothing blocks the path before the culm is there.
    /// Once it lies still the pursuer's ground is cut by it (a carving obstacle) and a link lets him vault or duck it too.
    /// Nobody makes it fall: the warning zone is where her own weight on the roots sets it going.
    /// </summary>
    public class FallingBamboo : MonoBehaviour, IChaseReset
    {
        private enum State { Standing, Warning, Falling, Down }

        [SerializeField] private PlayerMovement _player;
        [SerializeField] private StoryAudio _audio;
        [SerializeField] private ScreenFx _fx;
        [Tooltip("Turns about the foot of the culm; the culm, its collider and its leaves hang under it.")]
        [SerializeField] private Transform _pivot;
        [SerializeField] private Vector3 _fallenEuler;
        [SerializeField] private float _length = 9f;
        [SerializeField] private float _radius = 0.13f;
        [Tooltip("Where her steps set it going.")]
        [SerializeField] private BoxCollider _zone;
        [SerializeField] private float _warnSeconds = 1.3f, _fallSeconds = 0.65f;
        [SerializeField] private NavMeshObstacle _cut;
        [SerializeField] private NavMeshLink[] _links;

        private State _state;
        private Quaternion _upright;
        private Coroutine _running;

        public bool Armed { get; set; }
        public bool IsDown => _state == State.Down;
        /// <summary>When the warning began and when the culm reached the path (Time.time), for QA.</summary>
        public float WarnedAt { get; private set; } = -1f;
        public float LandedAt { get; private set; } = -1f;
        /// <summary>It came down on her (she was under it as it landed).</summary>
        public bool HitHer { get; private set; }
        /// <summary>It has begun to give way (the chase music makes room for the crack).</summary>
        public event System.Action Cracked;

        private void Awake()
        {
            _upright = _pivot.localRotation;
            SetLinks(false);
        }

        public void ResetForChase()
        {
            if (_running != null) StopCoroutine(_running);
            _running = null;
            _state = State.Standing;
            _pivot.localRotation = _upright;
            WarnedAt = LandedAt = -1f;
            HitHer = false;
            SetLinks(false);
        }

        private void Update()
        {
            if (!Armed || _state != State.Standing || _player == null || _zone == null) return;
            if (ChaseCourse.Inside(_zone, _player.transform.position + Vector3.up * 0.8f)) _running = StartCoroutine(Fall());
        }

        private IEnumerator Fall()
        {
            _state = State.Warning;
            WarnedAt = Time.time;
            Vector3 crown = _pivot.position + _pivot.up * _length * 0.6f, splitting = _pivot.position + _pivot.up * 0.8f;
            StoryAutopilot.Note($"bamboo {name} cracks");
            // Three beats, each from where it happens: the culm splitting low down (twice, while it sways), the crown and its
            // leaves sweeping through the air as it goes, then one impact where it lands and the leaves settling.
            Cracked?.Invoke();
            if (_audio != null) _audio.PlayAt("bamboo_crack", splitting, 1f, 32f);
            Quaternion down = Quaternion.Euler(_fallenEuler);
            bool second = false;
            // Warning: two cracks and a sway towards where it will fall, growing; the leaves above shake.
            for (float t = 0f; t < _warnSeconds; t += Time.deltaTime)
            {
                float k = t / _warnSeconds;
                float sway = (Mathf.Sin(t * 9f) * 0.5f + 0.6f) * 5f * k;
                _pivot.localRotation = Quaternion.SlerpUnclamped(_upright, down, sway / Mathf.Max(1f, Quaternion.Angle(_upright, down)));
                if (!second && t >= _warnSeconds * 0.55f) { second = true; if (_audio != null) _audio.PlayAt("bamboo_crack", splitting, 0.85f, 32f, 0.94f); }
                yield return null;
            }
            _state = State.Falling;
            Quaternion from = _pivot.localRotation;
            if (_audio != null) _audio.PlayAt("bamboo_sweep", crown, 0.9f, 28f);
            for (float t = 0f; t < _fallSeconds; t += Time.deltaTime)
            {
                float k = t / _fallSeconds;
                _pivot.localRotation = Quaternion.Slerp(from, down, k * k);
                if (!HitHer && k > 0.6f && Touches()) { HitHer = true; _player.Stumble(1.1f, 0.05f); if (_fx != null) _fx.Shake(0.5f, 0.35f); }
                yield return null;
            }
            _pivot.localRotation = down;
            _state = State.Down;
            LandedAt = Time.time;
            Vector3 lying = _pivot.position + _pivot.up * _length * 0.5f;
            if (_audio != null)
            {
                _audio.PlayAt("bamboo_impact", lying, 1f, 34f);
                _audio.PlayAt("leaf_settle", _pivot.position + _pivot.up * _length * 0.8f, 0.7f, 22f);
            }
            if (_fx != null && _player != null && Vector3.Distance(_player.transform.position, lying) < 10f) _fx.Shake(0.2f, 0.3f);
            SetLinks(true);
            StoryAutopilot.Note($"bamboo {name} down {LandedAt - WarnedAt:F2} s after the crack");
            _running = null;
        }

        /// <summary>Her body is within the culm's reach (closest points of the two capsules).</summary>
        private bool Touches()
        {
            if (_player == null) return false;
            Vector3 feet = _player.transform.position;
            for (int i = 0; i <= 12; i++)
            {
                Vector3 p = _pivot.position + _pivot.up * _length * i / 12f;
                float y = Mathf.Clamp(p.y, feet.y + 0.25f, feet.y + 1.3f);
                if (Vector3.Distance(p, new Vector3(feet.x, y, feet.z)) < _radius + 0.28f) return true;
            }
            return false;
        }

        private void SetLinks(bool down)
        {
            if (_cut != null) _cut.enabled = down;
            if (_links != null) foreach (var link in _links) if (link != null) link.enabled = down;
        }
    }
}
