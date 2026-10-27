using ThuyKieu.Core.Story;
using ThuyKieu.Player;
using UnityEngine;
using UnityEngine.AI;

namespace ThuyKieu.Gameplay.Chase
{
    /// <summary>
    /// Cô Lam in the bamboo chase. She said she knows the way across the stream, so she runs a few strides ahead of Kiều on
    /// the moonlit way, going over and under things like anyone (<see cref="ChaseTraverser"/>), keeping pace with Kiều rather
    /// than running off, and waiting with a look back when Kiều falls behind. She is never what catches the pursuer's eye
    /// and never blocks Kiều (no collider while she runs). If her path ever jams out of sight she is put back on the course
    /// ahead of Kiều, so the chase is never lost because a companion got stuck on a corner.
    /// </summary>
    [RequireComponent(typeof(StoryActor))]
    public class ChaseRunner : MonoBehaviour
    {
        [SerializeField] private float _lead = 5f;
        [SerializeField] private float _run = 5.7f, _ease = 4.4f;
        [Tooltip("How fast she runs when well behind her place ahead of Kiều.")]
        [SerializeField] private float _sprint = 7.2f;
        [Tooltip("Further ahead than lead + this, she stops and looks back for Kiều.")]
        [SerializeField] private float _wait = 5f;

        private StoryActor _actor;
        private NavMeshAgent _agent;
        private ChaseTraverser _traverser;
        private Follower _follower;
        private Collider[] _colliders;
        private ChaseCourse _course;
        private PlayerMovement _player;
        private float _repath, _stuck;
        private Vector3 _last;

        public bool Active { get; private set; }
        /// <summary>Her distance along the moonlit way.</summary>
        public float Progress => _course != null ? _course.Progress(transform.position) : 0f;
        public int Unstuck { get; private set; }

        private void Awake()
        {
            _actor = GetComponent<StoryActor>();
            _agent = GetComponent<NavMeshAgent>();
            _follower = GetComponent<Follower>();
            _traverser = GetComponent<ChaseTraverser>();
            if (_traverser == null) _traverser = gameObject.AddComponent<ChaseTraverser>();
            _colliders = GetComponentsInChildren<Collider>(true);
        }

        public void Begin(ChaseCourse course, PlayerMovement player, Vector3 position, Quaternion rotation)
        {
            _course = course;
            _player = player;
            gameObject.SetActive(true);
            if (_follower != null) _follower.Following = false;
            foreach (var collider in _colliders) if (collider != null) collider.enabled = false;
            _traverser.Cancel();
            // Onto the start first: switching navigation back on samples the NavMesh where the body stands (after a fall
            // into the ravine there is none near).
            _actor.DisableNavigation();
            transform.SetPositionAndRotation(position, rotation);
            _actor.EnableNavigation();
            _actor.Place(position, rotation);
            if (_agent != null) { _agent.acceleration = 14f; _agent.angularSpeed = 540f; _agent.autoBraking = false; }
            _last = transform.position;
            _stuck = 0f;
            _repath = 0f;
            Active = true;
        }

        /// <summary>Done running (across, or the run reset): her colliders come back; the follower is the chapter's to restart.</summary>
        public void End()
        {
            Active = false;
            _traverser.Cancel();
            _actor.Stop();
            foreach (var collider in _colliders) if (collider != null) collider.enabled = true;
        }

        private void Update()
        {
            if (!Active || _player == null || _agent == null || !_agent.enabled || !_agent.isOnNavMesh || _traverser.Traversing) return;
            float hers = Progress;
            float target = _course.Progress(_player.transform.position) + _lead;
            Unjam(hers < target - 1f);
            if ((_repath -= Time.deltaTime) > 0f) return;
            _repath = 0.2f;
            if (hers > target + _wait)
            {
                _actor.Stop();
                _actor.FaceTowards(_player.transform.position);
                return;
            }
            float goal = Mathf.Min(_course.WayLength(1), Mathf.Max(target, hers) + 3f);
            // Behind her place (each log and bough costs her time as it does Kiều), she runs harder to get it back: she must
            // be over the bridge ahead of Kiều, never be waited for at the stake with the torch on the planks.
            float speed = hers < target ? Mathf.Lerp(_run, _sprint, Mathf.InverseLerp(1f, 6f, target - hers)) : _ease;
            _actor.MoveTo(_course.PointAt(1, goal), speed);
        }

        /// <summary>Wanting to move but not moving for a few seconds, and nobody looking: back onto the way ahead of Kiều.</summary>
        private void Unjam(bool wantsToMove)
        {
            float moved = Vector3.Distance(transform.position, _last);
            _last = transform.position;
            _stuck = wantsToMove && moved < 0.5f * Time.deltaTime ? _stuck + Time.deltaTime : 0f;
            if (_stuck < 2.5f || Seen()) return;
            float at = _course.Progress(_player.transform.position) + _lead;
            if (!NavMesh.SamplePosition(_course.PointAt(1, at), out var hit, 2f, _agent.areaMask)) return;
            _actor.Place(hit.position, Quaternion.LookRotation(_course.DirectionAt(1, at)));
            _stuck = 0f;
            Unstuck++;
            StoryAutopilot.Note($"Lam was stuck at {transform.position:F1}: put back on the way ahead of Kiều");
        }

        private bool Seen()
        {
            var camera = Camera.main;
            if (camera == null) return false;
            Vector3 viewport = camera.WorldToViewportPoint(transform.position + Vector3.up);
            return viewport.z > 0f && viewport.x > -0.05f && viewport.x < 1.05f && viewport.y > -0.05f && viewport.y < 1.05f;
        }
    }
}
