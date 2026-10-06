using UnityEngine;
using UnityEngine.AI;

namespace ThuyKieu.Core.Story
{
    /// <summary>
    /// A character the story can address by its Ink speaker id: talking animation, facing,
    /// NavMesh walking and, for models without a rig yet, a procedural walk/breath motion.
    /// </summary>
    public class StoryActor : MonoBehaviour
    {
        [SerializeField] private string _speakerId;
        [SerializeField] private Animator _animator;
        [Tooltip("Visual child moved by the procedural motion when the model has no rig.")]
        [SerializeField] private Transform _staticVisual;
        [SerializeField] private float _headHeight = 1.55f;
        [SerializeField] private float _turnSpeed = 240f;

        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Talking = Animator.StringToHash("DialogueTalking");
        private static readonly int Emotion = Animator.StringToHash("DialogueEmotion");
        private NavMeshAgent _agent;
        private Vector3? _faceTarget;
        private bool _hasEmotion, _hasTalking, _hasSpeed;
        private Vector3 _visualBase;
        private Quaternion _visualRotation;
        private float _phase;

        public string SpeakerId => _speakerId;
        public Vector3 HeadPosition => transform.position + Vector3.up * _headHeight;
        public NavMeshAgent Agent => _agent;
        public bool IsMoving => _agent != null && _agent.enabled && _agent.isOnNavMesh && !_agent.isStopped
                                && (_agent.pathPending || _agent.remainingDistance > _agent.stoppingDistance + 0.05f);

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            if (_animator == null) _animator = GetComponentInChildren<Animator>();
            if (_animator != null && _animator.runtimeAnimatorController != null)
                foreach (var parameter in _animator.parameters)
                {
                    if (parameter.nameHash == Emotion) _hasEmotion = true;
                    if (parameter.nameHash == Talking) _hasTalking = true;
                    if (parameter.nameHash == Speed) _hasSpeed = true;
                }
            if (_staticVisual != null) { _visualBase = _staticVisual.localPosition; _visualRotation = _staticVisual.localRotation; }
            _phase = Random.value * 10f;
        }

        public void SetTalking(bool talking, int emotion)
        {
            if (_animator == null || !_animator.isActiveAndEnabled) return;
            if (_hasTalking) _animator.SetBool(Talking, talking);
            if (_hasEmotion) _animator.SetInteger(Emotion, talking ? emotion : 0);
        }

        /// <summary>Smoothly turns to look at a world point (only while not walking).</summary>
        public void FaceTowards(Vector3? point) => _faceTarget = point;

        public void FaceInstant(Vector3 point)
        {
            Vector3 direction = point - transform.position;
            direction.y = 0;
            if (direction.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(direction);
            _faceTarget = null;
        }

        public bool MoveTo(Vector3 destination, float speed = 1.6f)
        {
            if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh) return false;
            _faceTarget = null;
            _agent.speed = speed;
            _agent.isStopped = false;
            return _agent.SetDestination(destination);
        }

        public void Stop()
        {
            if (_agent != null && _agent.enabled && _agent.isOnNavMesh) { _agent.isStopped = true; _agent.ResetPath(); }
        }

        /// <summary>Instant placement that keeps the NavMeshAgent consistent.</summary>
        public void Place(Vector3 position, Quaternion rotation)
        {
            if (_agent != null && _agent.enabled && _agent.isOnNavMesh) { _agent.ResetPath(); _agent.Warp(position); }
            else transform.position = position;
            transform.rotation = rotation;
            _faceTarget = null;
        }

        /// <summary>Called once the runtime NavMesh exists.</summary>
        public void EnableNavigation()
        {
            if (_agent == null) return;
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                _agent.enabled = true;
                _agent.Warp(hit.position);
            }
        }

        private void Update()
        {
            float speed = _agent != null && _agent.enabled ? _agent.velocity.magnitude : 0f;
            if (_hasSpeed) _animator.SetFloat(Speed, Mathf.Clamp01(speed / 3.2f), 0.12f, Time.deltaTime);
            if (_faceTarget.HasValue && speed < 0.1f)
            {
                Vector3 direction = _faceTarget.Value - transform.position;
                direction.y = 0;
                if (direction.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), _turnSpeed * Time.deltaTime);
            }
            AnimateStaticVisual(speed);
        }

        // Placeholder life for unrigged models: a stepping bob and sway while walking, breathing when idle.
        private void AnimateStaticVisual(float speed)
        {
            if (_staticVisual == null) return;
            _phase += Time.deltaTime * (speed > 0.1f ? 7.5f * Mathf.Clamp(speed / 1.6f, 0.6f, 1.6f) : 1.4f);
            float walk = Mathf.Clamp01(speed / 1.2f);
            float bob = Mathf.Abs(Mathf.Sin(_phase)) * 0.045f * walk + Mathf.Sin(_phase) * 0.006f * (1 - walk);
            float sway = Mathf.Sin(_phase) * 3.2f * walk;
            float lean = 4f * walk;
            _staticVisual.localPosition = _visualBase + Vector3.up * bob;
            _staticVisual.localRotation = _visualRotation * Quaternion.Euler(lean, 0f, sway);
        }
    }
}
