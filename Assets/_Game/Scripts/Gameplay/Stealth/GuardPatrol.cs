using ThuyKieu.Core.Story;
using UnityEngine;

namespace ThuyKieu.Gameplay.Stealth
{
    /// <summary>
    /// NavMesh patrol between waypoints with a pause and a look-around at each stop.
    /// Turns towards the player while growing suspicious, walks over when nearly sure,
    /// and goes to investigate noises (thrown pebbles, a creaking door) before resuming the round.
    /// A drunk guard walks slower, lingers longer and sways.
    /// </summary>
    [RequireComponent(typeof(StoryActor))]
    public class GuardPatrol : MonoBehaviour
    {
        [SerializeField] private Transform[] _waypoints;
        [SerializeField] private float _speed = 1.25f;
        [SerializeField] private float _pause = 2.2f;
        [SerializeField] private VisionSensor _vision;
        [SerializeField] private bool _drunk;

        private StoryActor _actor;
        private int _index;
        private float _wait;
        private float _scanYaw;
        private Vector3 _start;
        private Quaternion _startRotation;
        private Vector3? _investigate;
        private float _investigateTime;

        public bool Patrolling { get; set; }
        public VisionSensor Vision => _vision;
        public bool IsInvestigating => _investigate.HasValue;
        public float SpeedScale { get; set; } = 1f;

        private float Speed => _speed * SpeedScale * (_drunk ? 0.85f : 1f);

        private void Awake()
        {
            _actor = GetComponent<StoryActor>();
            _start = transform.position;
            _startRotation = transform.rotation;
        }

        public void ResetPatrol()
        {
            _actor.Place(_start, _startRotation);
            _index = 0;
            _wait = 0.5f;
            _investigate = null;
            if (_vision != null) _vision.ResetAwareness();
        }

        /// <summary>Walks to a noise and searches around it for a few seconds.</summary>
        public void Investigate(Vector3 point, float seconds)
        {
            if (!Patrolling) return;
            _investigate = point;
            _investigateTime = seconds;
            _actor.MoveTo(point, Speed * 1.3f);
        }

        private void Update()
        {
            if (!Patrolling || _waypoints == null || _waypoints.Length == 0) return;
            if (_vision != null && _vision.Awareness > 0.25f && _vision.SeesPlayer)
            {
                Vector3 look = _vision.Player.transform.position;
                if (_vision.Awareness > 0.6f)
                {
                    // Nearly sure: walk over to check. Losing sight leaves him searching where he last saw the player.
                    _actor.MoveTo(look, Speed * 1.5f);
                    _wait = 0f;
                    return;
                }
                // Suspicious: stop and stare at what caught the eye.
                _actor.Stop();
                Vector3 direction = look - transform.position;
                direction.y = 0;
                if (direction.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), 200 * Time.deltaTime);
                return;
            }
            if (_investigate.HasValue)
            {
                if (_actor.IsMoving) return;
                _investigateTime -= Time.deltaTime;
                _scanYaw += Time.deltaTime;
                transform.Rotate(0f, Mathf.Sin(_scanYaw * 2.2f) * 90f * Time.deltaTime, 0f);
                if (_investigateTime <= 0f) { _investigate = null; _wait = 0.3f; }
                return;
            }
            if (_wait > 0f)
            {
                _wait -= Time.deltaTime;
                _scanYaw += Time.deltaTime;
                transform.Rotate(0f, Mathf.Sin(_scanYaw * 1.6f) * 50f * Time.deltaTime, 0f);
                if (_wait <= 0f) _actor.MoveTo(_waypoints[_index].position, Speed);
                return;
            }
            if (!_actor.IsMoving)
            {
                _index = (_index + 1) % _waypoints.Length;
                _wait = _pause + (_drunk ? Random.Range(0.5f, 2.5f) : 0f);
                _scanYaw = 0f;
            }
        }
    }
}
