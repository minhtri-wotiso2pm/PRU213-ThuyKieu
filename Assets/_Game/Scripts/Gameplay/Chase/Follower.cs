using ThuyKieu.Core.Story;
using ThuyKieu.Player;
using UnityEngine;
using UnityEngine.AI;

namespace ThuyKieu.Gameplay.Chase
{
    /// <summary>
    /// A companion who keeps a step behind the player, sneaks when the player sneaks and is never left far behind
    /// (she catches up out of sight rather than getting stuck on a corner).
    /// </summary>
    [RequireComponent(typeof(StoryActor))]
    public class Follower : MonoBehaviour
    {
        [SerializeField] private PlayerMovement _player;
        [SerializeField] private float _distance = 1.6f;
        [SerializeField] private float _catchUp = 9f;

        private static readonly int CrouchParameter = Animator.StringToHash("Crouch");
        private StoryActor _actor;
        private Animator _animator;
        private bool _hasCrouch;
        private float _repath;

        public bool Following { get; set; } = true;
        /// <summary>Opt-in lateral spacing; chase companions retain their existing following behaviour.</summary>
        public float SideOffset { get; set; }

        private void Awake()
        {
            _actor = GetComponent<StoryActor>();
            _animator = GetComponentInChildren<Animator>();
            if (_animator != null && _animator.runtimeAnimatorController != null)
                foreach (var parameter in _animator.parameters) if (parameter.nameHash == CrouchParameter) _hasCrouch = true;
        }

        /// <summary>Puts her right behind the player (checkpoints, scene cuts).</summary>
        public void Snap()
        {
            Vector3 behind = FollowPoint();
            _actor.Place(behind, _player.transform.rotation);
        }

        private void Update()
        {
            if (!Following || _player == null || ThuyKieu.Core.GamePause.IsPaused) return;
            if (_hasCrouch) _animator.SetBool(CrouchParameter, _player.IsCrouching);
            float distance = Vector3.Distance(transform.position, _player.transform.position);
            if (distance > _catchUp && !Visible()) { Snap(); return; }
            if ((_repath -= Time.deltaTime) > 0f) return;
            _repath = 0.2f;
            Vector3 destination = Mathf.Abs(SideOffset) > 0.01f
                ? FollowPoint()
                : _player.transform.position - (_player.transform.position - transform.position).normalized * _distance;
            bool inPlace = Mathf.Abs(SideOffset) > 0.01f
                ? Vector3.ProjectOnPlane(destination - transform.position, Vector3.up).sqrMagnitude < 0.36f
                : distance < _distance + 0.3f;
            if (inPlace) { _actor.Stop(); _actor.FaceTowards(_player.transform.position); return; }
            float speed = _player.IsCrouching ? 1.2f : _player.IsRunning || _player.ScriptedMove.HasValue ? 5.6f : distance > 4f ? 4f : 2.6f;
            _actor.MoveTo(destination, speed);
        }

        private Vector3 FollowPoint()
        {
            Vector3 behind = _player.transform.position - _player.transform.forward * _distance;
            if (Mathf.Abs(SideOffset) < 0.01f || _actor.Agent == null) return behind;
            var filter = new NavMeshQueryFilter { agentTypeID = _actor.Agent.agentTypeID, areaMask = _actor.Agent.areaMask };
            Vector3 desired = behind + _player.transform.right * SideOffset;
            // In narrow lanes, stay behind rather than selecting a point through a wall or off the walkway.
            if (NavMesh.SamplePosition(_player.transform.position, out var origin, 1f, filter)
                && NavMesh.SamplePosition(desired, out var side, 0.5f, filter)
                && !NavMesh.Raycast(origin.position, side.position, out _, filter)) return side.position;
            return behind;
        }

        private bool Visible()
        {
            var camera = Camera.main;
            if (camera == null) return false;
            Vector3 viewport = camera.WorldToViewportPoint(transform.position + Vector3.up);
            return viewport.z > 0 && viewport.x > 0 && viewport.x < 1 && viewport.y > 0 && viewport.y < 1;
        }
    }
}
