using ThuyKieu.Core.Story;
using ThuyKieu.Dialogue;
using ThuyKieu.Player;
using ThuyKieu.Presentation;
using ThuyKieu.UI;
using UnityEngine;
using UnityEngine.AI;

namespace ThuyKieu.Gameplay.Chase
{
    /// <summary>
    /// Tú Bà's man with the torch, running Kiều down through the bamboo. He is a person: he runs the NavMesh path a person
    /// could run, goes over and under things through <see cref="ChaseTraverser"/>, and his speed stays in a narrow human band
    /// (a little slower than her clean stride when close, a little faster when he has dropped far back). He never teleports
    /// and never gains through a wall.
    /// The gap is wherever he really is: a clean run keeps him at bay, a stumble or a culm run into lets him close, several
    /// in a row let him reach her. He can only take her when he really can: within arm's reach, nothing solid between them,
    /// no ditch, ravine or culm between them on the ground, and their feet at about the same height.
    /// He is heard where he is: footfalls on the mud, his shouts from his own position, the torch flaring as he runs.
    /// </summary>
    [RequireComponent(typeof(StoryActor))]
    public class ChasePursuer : MonoBehaviour
    {
        [Header("Speed band (m/s)")]
        [SerializeField] private float _cruise = 4.95f;
        [Tooltip("Within Close metres of her (he is about to reach her): barely faster than cruising.")]
        [SerializeField] private float _closing = 4.95f;
        [Tooltip("Further than Far metres behind along the path: the most a running man has.")]
        [SerializeField] private float _catchUp = 5.35f;
        [SerializeField] private float _close = 3f, _far = 9.5f;

        [Header("Reach")]
        [Tooltip("Flat distance between his feet and hers at which his hands reach her: the two bodies (0.55 m), an arm (0.75 m) and a lean.")]
        [SerializeField] private float _reach = 1.5f;
        [SerializeField] private float _heightTolerance = 0.8f;
        [SerializeField] private LayerMask _walls = 1 << 8;

        [Header("Presence")]
        [SerializeField] private StoryAudio _audio;
        [SerializeField] private StoryHUD _hud;
        [SerializeField] private float _stride = 1.45f;

        /// <summary>Shouted at random while running (recorded barks; the voice manifest reads this list).</summary>
        public static readonly string[] Shouts = { "Đứng lại!", "Bắt lấy con ranh!", "Nó chạy đằng kia!", "Chặn đầu nó!" };

        private StoryActor _actor;
        private NavMeshAgent _agent;
        private ChaseTraverser _traverser;
        private AudioSource _voice;
        private PlayerMovement _target;
        private Vector3 _lastPosition;
        private float _repath, _travelled, _shoutIn;
        private NavMeshPath _path;

        public StoryActor Actor => _actor;
        public ChaseTraverser Traverser => _traverser;
        public bool Active { get; private set; }
        /// <summary>Metres he still has to run to reach her along the ground (his NavMesh path), or the flat distance when no path is known.</summary>
        public float PathDistance { get; private set; }
        public float FlatDistance => _target != null ? Flat(transform.position, _target.transform.position) : float.MaxValue;
        /// <summary>No full path to her: the ground between them is cut (the bridge is down).</summary>
        public bool CutOff { get; private set; }
        public float Speed => _agent != null && _agent.enabled ? _agent.speed : 0f;

        private void Awake()
        {
            _actor = GetComponent<StoryActor>();
            _agent = GetComponent<NavMeshAgent>();
            _traverser = GetComponent<ChaseTraverser>();
            if (_traverser == null) _traverser = gameObject.AddComponent<ChaseTraverser>();
            _path = new NavMeshPath();
        }

        /// <summary>Puts him at <paramref name="position"/> and sets him after <paramref name="target"/>.</summary>
        public void Begin(PlayerMovement target, Vector3 position, Quaternion rotation)
        {
            _target = target;
            gameObject.SetActive(true);
            _traverser.Cancel();
            // Onto the start first: switching navigation back on samples the NavMesh where the body stands (after a fall
            // into the ravine there is none near).
            _actor.DisableNavigation();
            transform.SetPositionAndRotation(position, rotation);
            _actor.EnableNavigation();
            _actor.Place(position, rotation);
            if (_agent != null) { _agent.acceleration = 14f; _agent.angularSpeed = 540f; _agent.autoBraking = false; _agent.stoppingDistance = 0.2f; }
            _lastPosition = transform.position;
            _travelled = 0f;
            _repath = 0f;
            _shoutIn = 1.2f;
            PathDistance = FlatDistance;
            CutOff = false;
            Active = true;
        }

        /// <summary>Stops him where he stands (a catch, a reset, the end of the chase).</summary>
        public void Halt()
        {
            Active = false;
            _traverser.Cancel();
            _actor.Stop();
            if (_voice != null) _voice.Stop();
        }

        private void Update()
        {
            if (!Active || _target == null || _agent == null || !_agent.enabled || !_agent.isOnNavMesh) return;
            if ((_repath -= Time.deltaTime) <= 0f && !_traverser.Traversing)
            {
                _repath = 0.2f;
                Measure();
                _actor.MoveTo(_target.transform.position, SpeedFor(PathDistance));
            }
            Steps();
            Shout();
        }

        /// <summary>His own path to her: its length is the real gap; a partial path means the ground between them is cut.</summary>
        private void Measure()
        {
            if (!NavMesh.SamplePosition(_target.transform.position, out var hit, 1.2f, _agent.areaMask)
                || !_agent.CalculatePath(hit.position, _path))
            {
                PathDistance = FlatDistance;
                return;
            }
            CutOff = _path.status != NavMeshPathStatus.PathComplete;
            float length = 0f;
            var corners = _path.corners;
            for (int i = 1; i < corners.Length; i++) length += Vector3.Distance(corners[i - 1], corners[i]);
            PathDistance = corners.Length > 1 ? length : FlatDistance;
        }

        /// <summary>A narrow human band: cruising, barely more when he is about to reach her, his best when far behind.</summary>
        private float SpeedFor(float gap)
        {
            if (gap <= _close) return _closing;
            if (gap >= _far) return _catchUp;
            return Mathf.Lerp(_cruise, _catchUp, Mathf.InverseLerp(_far - 3f, _far, gap));
        }

        /// <summary>
        /// Whether his hands reach her now: close enough, feet at about the same height, nothing solid between his chest and
        /// hers, and the ground between them unbroken (a ditch, the ravine or a fallen culm between them stops him).
        /// </summary>
        public bool CanCatch(PlayerMovement player)
        {
            if (!Active) { Refusal = "not running"; return false; }
            // Over a culm or a ditch he is off the ground and cannot grab; at the very start of it, or bent under low culms, he can.
            if (_traverser.Traversing && _traverser.CurrentKind != ChaseLink.Kind.Duck && _traverser.Progress > 0.15f) { Refusal = "crossing " + _traverser.CurrentKind; return false; }
            return CouldReach(player);
        }

        /// <summary>Why the last reach test said no (QA).</summary>
        public string Refusal { get; private set; }

        /// <summary>The reach test alone, wherever he stands and whatever he is doing (QA sets up the cases).</summary>
        public bool CouldReach(PlayerMovement player)
        {
            if (player == null || _agent == null || !_agent.enabled || !_agent.isOnNavMesh) { Refusal = "off the NavMesh"; return false; }
            Vector3 his = transform.position, hers = player.transform.position;
            if (Flat(his, hers) > _reach) { Refusal = "out of reach"; return false; }
            if (Mathf.Abs(his.y - hers.y) > _heightTolerance) { Refusal = $"height {his.y - hers.y:F2}"; return false; }
            if (Physics.Linecast(his + Vector3.up * 1.3f, hers + Vector3.up * 1.0f, out var block, _walls, QueryTriggerInteraction.Ignore)) { Refusal = "blocked by " + block.collider.name; return false; }
            // The ground between them, felt every 20 cm: a ditch or the ravine in between (a drop of more than 0.9 m) and he
            // cannot reach across it; a fallen culm or a root he can reach over.
            float floor = Mathf.Min(his.y, hers.y);
            // (Between their bodies only; the plank bridge, on the default layer, counts as ground.)
            for (float k = 0.25f; k <= 0.76f; k += 0.2f / Mathf.Max(0.4f, Flat(his, hers)))
            {
                Vector3 at = Vector3.Lerp(his, hers, k) + Vector3.up * 1f;
                if (!Physics.Raycast(at, Vector3.down, out var ground, 6f, _walls | 1, QueryTriggerInteraction.Ignore) || ground.point.y < floor - 0.9f)
                { Refusal = "ground broken between"; return false; }
            }
            Refusal = null;
            return true;
        }

        private void Steps()
        {
            Vector3 position = transform.position;
            _travelled += Flat(position, _lastPosition);
            _lastPosition = position;
            if (_travelled < _stride || _audio == null) return;
            _travelled = 0f;
            // Loud enough to carry 30 m through the culms: the player judges the gap by ear. A grown man's heavier tread,
            // pitched down and placed behind her, so it is never mistaken for her own feet; planks on the bridge.
            bool planks = _traverser.Traversing && _traverser.CurrentKind == ChaseLink.Kind.Run;
            _audio.PlayAt(planks ? "guard_step_wood" : "guard_step", position, 0.95f, 30f, Random.Range(0.84f, 0.92f));
        }

        private void Shout()
        {
            if ((_shoutIn -= Time.deltaTime) > 0f) return;
            _shoutIn = Random.Range(4f, 7f);
            float distance = FlatDistance;
            if (distance > 35f) return;
            string line = Shouts[Random.Range(0, Shouts.Length)];
            var clip = VoicePlayer.BarkClip("GiaNhan", line);
            if (clip != null && _audio != null)
            {
                if (_voice == null) _voice = _audio.OwnSource(transform, "PursuerVoice", 40f);
                _voice.transform.localPosition = Vector3.up * 1.6f;
                _voice.clip = clip;
                _voice.volume = VoicePlayer.BarkVolume;
                _voice.Play();
            }
            // The words on screen only when he is near enough to be understood.
            if (_hud != null && distance < 20f) _hud.ShowBark("GiaNhan", line, 1.6f, clip == null);
            StoryAutopilot.Note($"pursuer shouts at {distance:F1} m");
        }

        private static float Flat(Vector3 a, Vector3 b) { a.y = 0f; b.y = 0f; return Vector3.Distance(a, b); }
    }
}
