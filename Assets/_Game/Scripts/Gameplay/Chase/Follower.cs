using ThuyKieu.Core.Story;
using ThuyKieu.Player;
using UnityEngine;

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
            Vector3 behind = _player.transform.position - _player.transform.forward * _distance;
            _actor.Place(behind, _player.transform.rotation);
        }

        private void Update()
        {
            if (!Following || _player == null) return;
            if (_hasCrouch) _animator.SetBool(CrouchParameter, _player.IsCrouching);
            float distance = Vector3.Distance(transform.position, _player.transform.position);
            if (distance > _catchUp && !Visible()) { Snap(); return; }
            if ((_repath -= Time.deltaTime) > 0f) return;
            _repath = 0.2f;
            if (distance < _distance + 0.3f) { _actor.Stop(); _actor.FaceTowards(_player.transform.position); return; }
            float speed = _player.IsCrouching ? 1.2f : _player.IsRunning || _player.ScriptedMove.HasValue ? 5.6f : distance > 4f ? 4f : 2.6f;
            _actor.MoveTo(_player.transform.position - (_player.transform.position - transform.position).normalized * _distance, speed);
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
