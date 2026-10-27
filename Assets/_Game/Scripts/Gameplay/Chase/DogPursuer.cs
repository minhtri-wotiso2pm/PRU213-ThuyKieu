using ThuyKieu.Presentation;
using UnityEngine;
using UnityEngine.AI;

namespace ThuyKieu.Gameplay.Chase
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class DogPursuer : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private StoryAudio _audio;
        private NavMeshAgent _agent;
        private float _barkCooldown;
        private Vector3 _visualPosition;
        private Quaternion _visualRotation;
        private static readonly int Speed = Animator.StringToHash("Speed");

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.enabled = false;
            if (_animator != null)
            {
                // The Blender dog's forward axis is -X; NavMesh movement uses +Z.
                var correction = Quaternion.Euler(0f, 90f, 0f);
                _visualPosition = correction * _animator.transform.localPosition;
                _visualRotation = correction * _animator.transform.localRotation;
            }
        }

        public void StartAt(Vector3 position, Quaternion rotation)
        {
            Stop();
            gameObject.SetActive(true);
            if (!NavMesh.SamplePosition(position, out var hit, 3f, _agent.areaMask)) return;
            transform.SetPositionAndRotation(hit.position, rotation);
            _agent.enabled = true;
            _agent.Warp(hit.position);
            _agent.isStopped = false;
            _barkCooldown = 2f;
        }

        public void Pursue(Vector3 target, float speed)
        {
            if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh) return;
            _agent.speed = speed;
            _agent.isStopped = false;
            if (NavMesh.SamplePosition(target, out var hit, 1.5f, _agent.areaMask)) _agent.SetDestination(hit.position);
        }

        public void Stop()
        {
            if (_agent == null) _agent = GetComponent<NavMeshAgent>();
            if (_agent.enabled && _agent.isOnNavMesh) _agent.ResetPath();
            _agent.enabled = false;
            if (_animator != null) _animator.SetFloat(Speed, 0f);
        }

        private void Update()
        {
            bool moving = _agent != null && _agent.enabled && _agent.isOnNavMesh && !_agent.isStopped;
            if (_animator != null) _animator.SetFloat(Speed, moving ? _agent.velocity.magnitude : 0f, 0.12f, Time.deltaTime);
            if (!moving || _agent.velocity.sqrMagnitude < 0.1f) return;
            _barkCooldown -= Time.deltaTime;
            if (_barkCooldown > 0f) return;
            _barkCooldown = 4.5f;
            if (_audio != null) _audio.PlaySfx("dog");
        }

        private void OnDisable() { Stop(); }

        private void LateUpdate()
        {
            if (_animator == null) return;
            _animator.transform.localPosition = _visualPosition;
            _animator.transform.localRotation = _visualRotation;
        }
    }
}
