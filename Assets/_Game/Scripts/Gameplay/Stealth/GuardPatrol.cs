using ThuyKieu.Core.Story;
using UnityEngine;

namespace ThuyKieu.Gameplay.Stealth
{
    /// <summary>
    /// NavMesh patrol between waypoints with a pause and a look-around at each stop.
    /// Turns towards the player while growing suspicious.
    /// </summary>
    [RequireComponent(typeof(StoryActor))]
    public class GuardPatrol : MonoBehaviour
    {
        [SerializeField] private Transform[] _waypoints;
        [SerializeField] private float _speed = 1.25f;
        [SerializeField] private float _pause = 2.2f;
        [SerializeField] private VisionSensor _vision;

        private StoryActor _actor;
        private int _index;
        private float _wait;
        private float _scanYaw;
        private Vector3 _start;
        private Quaternion _startRotation;

        public bool Patrolling { get; set; }
        public VisionSensor Vision => _vision;

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
            if (_vision != null) _vision.ResetAwareness();
        }

        private void Update()
        {
            if (!Patrolling || _waypoints == null || _waypoints.Length == 0) return;
            if (_vision != null && _vision.Awareness > 0.25f && _vision.SeesPlayer)
            {
                // Suspicious: stop and stare at what caught the eye.
                _actor.Stop();
                Vector3 look = _vision.Player.transform.position;
                Vector3 direction = look - transform.position;
                direction.y = 0;
                if (direction.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), 200 * Time.deltaTime);
                return;
            }
            if (_wait > 0f)
            {
                _wait -= Time.deltaTime;
                _scanYaw += Time.deltaTime;
                transform.Rotate(0f, Mathf.Sin(_scanYaw * 1.6f) * 50f * Time.deltaTime, 0f);
                if (_wait <= 0f) _actor.MoveTo(_waypoints[_index].position, _speed);
                return;
            }
            if (!_actor.IsMoving)
            {
                _index = (_index + 1) % _waypoints.Length;
                _wait = _pause;
                _scanYaw = 0f;
            }
        }
    }
}
