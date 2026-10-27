using System.Collections;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Stealth;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace ThuyKieu.Gameplay.Chase
{
    /// <summary>
    /// Takes a person (the pursuer, Cô Lam) across the links of the bamboo chase the way a body would, instead of the
    /// agent's instant hop: a vault over a fallen culm costs time and rises over it, a duck under a low culm is slow and bent
    /// over, a leap over a ditch is an arc at running pace, a plank bridge is run across. Nobody slides through a culm or
    /// floats over a gap: the NavMesh has a hole there and only a link crosses it.
    /// The bend for ducking is laid over the run clip by <see cref="GrabPose"/> (there is no crouch clip for these rigs).
    /// </summary>
    [RequireComponent(typeof(StoryActor))]
    public class ChaseTraverser : MonoBehaviour
    {
        [Tooltip("Vault: the run slows to this share of the agent's speed while going over.")]
        [SerializeField] private float _vaultPace = 0.85f;
        [SerializeField] private float _minVaultSeconds = 0.45f;

        private NavMeshAgent _agent;
        private StoryActor _actor;
        private GrabPose _pose;
        private Transform _model;
        private Vector3 _modelBase;
        private Coroutine _crossing;

        public bool Traversing => _crossing != null;
        /// <summary>0 at the start of the link being crossed, 1 at its end.</summary>
        public float Progress { get; private set; }
        public ChaseLink.Kind CurrentKind { get; private set; }
        /// <summary>The link being crossed (the bridge asks whether he is on it).</summary>
        public NavMeshLink CurrentLink { get; private set; }
        /// <summary>Links crossed so far, by kind (Vault, Duck, Leap, Run), for QA.</summary>
        public int[] Crossed { get; } = new int[4];

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _actor = GetComponent<StoryActor>();
        }

        private void OnEnable() { if (_agent != null) _agent.autoTraverseOffMeshLink = false; }

        private void OnDisable() { Cancel(); }

        private void Update()
        {
            if (_crossing != null || _agent == null || !_agent.enabled || !_agent.isOnNavMesh || !_agent.isOnOffMeshLink) return;
            _crossing = StartCoroutine(Cross(_agent.currentOffMeshLinkData));
        }

        /// <summary>Stops a crossing where it is (a reset, a catch); the caller places the body again.</summary>
        public void Cancel()
        {
            if (_crossing != null) { StopCoroutine(_crossing); _crossing = null; }
            EndPose();
            CurrentLink = null;
            if (_actor != null) _actor.SetScriptedSpeed(null);
            // Never CompleteOffMeshLink here: that puts him at the far end of the link. He stays where his body is (the NavMesh
            // takes the nearer side if that is over a gap).
            if (_agent != null && _agent.enabled && _agent.isOnNavMesh && _agent.isOnOffMeshLink) _agent.Warp(transform.position);
        }

        private IEnumerator Cross(OffMeshLinkData data)
        {
            var linkComponent = data.owner as NavMeshLink;
            CurrentLink = linkComponent;
            float arrived = Time.time;
            float before = _agent.velocity.magnitude;
            var link = linkComponent != null ? linkComponent.GetComponent<ChaseLink>() : null;
            CurrentKind = link != null ? link.Type : ChaseLink.Kind.Vault;
            Progress = 0f;
            Vector3 start = transform.position;
            Vector3 end = data.endPos + Vector3.up * _agent.baseOffset;
            Vector3 flat = end - start;
            flat.y = 0f;
            float length = flat.magnitude;
            float pace = Mathf.Max(1f, _agent.speed);
            float height = link != null ? link.Height : 0.6f;
            float seconds;
            switch (CurrentKind)
            {
                case ChaseLink.Kind.Duck: seconds = length / (link != null ? link.DuckSpeed : 3f); break;
                case ChaseLink.Kind.Leap: seconds = length / pace + 0.08f; break;
                case ChaseLink.Kind.Run: seconds = length / pace; break;
                default: seconds = Mathf.Max(_minVaultSeconds, length / (pace * _vaultPace)); break;
            }
            if (flat.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(flat);
            _actor.SetScriptedSpeed(length / Mathf.Max(0.05f, seconds));
            if (CurrentKind == ChaseLink.Kind.Duck) BeginDuck();

            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                float k = t / seconds;
                Progress = k;
                Vector3 at = Vector3.Lerp(start, end, k);
                if (CurrentKind == ChaseLink.Kind.Vault || CurrentKind == ChaseLink.Kind.Leap) at += Vector3.up * height * 4f * k * (1f - k);
                else if (CurrentKind == ChaseLink.Kind.Duck) Duck(Mathf.Min(1f, Mathf.Min(k, 1f - k) * 6f));
                transform.position = at;
                yield return null;
            }
            transform.position = end;
            Crossed[(int)CurrentKind]++;
            StoryAutopilot.Note($"{name} crossed {CurrentKind} {length:F1} m in {Time.time - arrived:F2} s (came in at {before:F1} m/s)");
            EndPose();
            _actor.SetScriptedSpeed(null);
            CurrentLink = null;
            if (_agent.enabled && _agent.isOnNavMesh && _agent.isOnOffMeshLink) _agent.CompleteOffMeshLink();
            _crossing = null;
        }

        // ---------------------------------------------------------------- ducking pose

        private void BeginDuck()
        {
            if (_pose == null) _pose = GetComponent<GrabPose>();
            if (_pose == null) _pose = gameObject.AddComponent<GrabPose>();
            var animator = GetComponentInChildren<Animator>();
            _model = animator != null ? animator.transform : null;
            if (_model != null) _modelBase = _model.localPosition;
            if (_pose != null && _pose.isActiveAndEnabled == false) _pose.enabled = true;
            if (_pose != null) { _pose.Look = _pose.Right = _pose.Left = _pose.Grip = 0f; _pose.LeanDegrees = 70f; _pose.LookTarget = transform.position + transform.forward * 3f; }
        }

        /// <summary>Bent over at the hips, knees giving a little (the body sinks a hand's breadth): 0 upright, 1 fully down.</summary>
        private void Duck(float amount)
        {
            if (_pose != null) _pose.Lean = amount;
            if (_model != null) _model.localPosition = _modelBase + Vector3.down * 0.1f * amount;
        }

        private void EndPose()
        {
            if (_pose != null && CurrentKind == ChaseLink.Kind.Duck) { _pose.Lean = 0f; _pose.LeanDegrees = 22f; _pose.enabled = false; }
            if (_model != null) { _model.localPosition = _modelBase; _model = null; }
        }
    }
}
