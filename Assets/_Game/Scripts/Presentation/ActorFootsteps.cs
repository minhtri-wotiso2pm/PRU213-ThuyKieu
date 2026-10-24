using ThuyKieu.Environment;
using UnityEngine;
using UnityEngine.AI;

namespace ThuyKieu.Presentation
{
    /// <summary>
    /// Steps of someone walking about the house, heard from where they are: the first warning that a guard is coming
    /// round the corner, long before he is in view.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class ActorFootsteps : MonoBehaviour
    {
        [SerializeField] private StoryAudio _audio;
        [SerializeField] private float _stride = 0.75f;
        [SerializeField] private float _volume = 0.7f;
        [SerializeField] private float _hearing = 16f;
        [SerializeField] private float _pitch = 0.9f;
        [SerializeField] private LayerMask _floor = 1 << 8;

        private NavMeshAgent _agent;
        private Vector3 _last;
        private float _travelled;

        private void Awake() { _agent = GetComponent<NavMeshAgent>(); _last = transform.position; }

        private void Update()
        {
            Vector3 position = transform.position;
            Vector3 step = position - _last;
            step.y = 0f;
            _last = position;
            // Teleports (checkpoints) are not steps.
            if (step.sqrMagnitude > 1f || _audio == null) return;
            _travelled += step.magnitude;
            if (_travelled < _stride) return;
            _travelled = 0f;
            var surface = Surface.Wood;
            if (Physics.Raycast(position + Vector3.up * 0.3f, Vector3.down, out RaycastHit hit, 0.8f, _floor, QueryTriggerInteraction.Ignore))
            {
                var tag = hit.collider.GetComponentInParent<SurfaceTag>();
                if (tag != null) surface = tag.Surface;
            }
            float speed = _agent != null && _agent.enabled ? _agent.velocity.magnitude : 1f;
            float loud = Mathf.Clamp(speed / 1.4f, 0.6f, 1.6f) * SurfaceTag.Loudness(surface);
            _audio.PlayAt(SurfaceTag.StepSound(surface), position, _volume * loud, _hearing, _pitch * Random.Range(0.92f, 1.06f));
        }
    }
}
