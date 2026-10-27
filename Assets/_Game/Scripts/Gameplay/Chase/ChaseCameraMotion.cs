using ThuyKieu.Core;
using ThuyKieu.Gameplay.Stealth;
using ThuyKieu.Player;
using UnityEngine;

namespace ThuyKieu.Gameplay.Chase
{
    /// <summary>
    /// The weight of Kiều's run in her own eyes (Chapter 5 only), as one motion handed to the camera each frame
    /// (<see cref="ThirdPersonCameraController.MotionOffset"/>, <see cref="ThirdPersonCameraController.MotionTilt"/>):
    ///   stride   the eyes sink at each footfall and rise between, swaying a little towards the foot that lands; driven by
    ///            the footsteps' own stride (one phase for sound and sight, never drifting apart); less crouched, none in
    ///            the air or when she is held;
    ///   jump     a short drop as she pushes off; the rise and fall themselves are the real jump;
    ///   landing  a dip as deep as she really came down, recovered within about a quarter of a second;
    ///   sidestep a lean of the head of at most ~1.5° into an A/D step, eased back;
    ///   knock    a trip, a culm, the head: a short push away from what she hit, decaying at once.
    /// Everything is summed then limited (offset and tilt) so events meeting never throw the view off the way, and all of
    /// it is scaled by the "screen shake" setting (off: no motion at all; the eye height of a crouch or a jump stays).
    /// It never turns the heading the mouse steers and never touches collisions or timing.
    /// </summary>
    [DefaultExecutionOrder(110)] // after the footsteps have counted this frame's stride
    public class ChaseCameraMotion : MonoBehaviour
    {
        [SerializeField] private ThirdPersonCameraController _camera;
        [SerializeField] private PlayerMovement _player;
        [SerializeField] private FirstPersonFootsteps _steps;
        [Tooltip("Stride: how far the eyes sink at a footfall, and sway towards the landing foot (metres).")]
        [SerializeField] private float _bobHeight = 0.034f, _bobSway = 0.012f;
        [SerializeField] private float _maxLean = 1.5f;
        [Tooltip("Landing dip (metres) for a soft landing and for the hardest she can make.")]
        [SerializeField] private float _landSoft = 0.025f, _landHard = 0.085f;
        [Tooltip("The limits of the sum of everything.")]
        [SerializeField] private float _maxOffset = 0.11f, _maxTilt = 3f;

        private float _air = 1f, _lean, _dip, _dipTarget, _dipHold;
        private Vector3 _knockOffset, _knockTilt;
        private int _foot = 1, _lastCount;

        private void OnDisable()
        {
            if (_camera != null) { _camera.MotionOffset = Vector3.zero; _camera.MotionTilt = Vector3.zero; }
        }

        /// <summary>Back to rest at once (a new attempt, a teleport): nothing carried over from where she was.</summary>
        public void ResetMotion()
        {
            _lean = _dip = _dipTarget = _dipHold = 0f;
            _knockOffset = _knockTilt = Vector3.zero;
            _air = 1f;
            if (_steps != null) _lastCount = _steps.StepCount;
            if (_camera != null) { _camera.MotionOffset = Vector3.zero; _camera.MotionTilt = Vector3.zero; }
        }

        public void Jump() { _dipTarget = Mathf.Max(_dipTarget, 0.018f); _dipHold = 0.05f; }

        public void Land(float fallSpeed)
        {
            float force = Mathf.InverseLerp(2f, 8f, fallSpeed);
            _dipTarget = Mathf.Max(_dipTarget, Mathf.Lerp(_landSoft, _landHard, force));
            _dipHold = 0.06f;
        }

        /// <summary>A knock from <paramref name="point"/>: the eyes pushed away from it and tipped, <paramref name="strength"/> 0–1.</summary>
        public void Knock(Vector3 point, float strength)
        {
            if (_camera == null) return;
            Vector3 away = _camera.transform.position - point;
            Vector3 local = Quaternion.Inverse(_camera.transform.rotation) * away.normalized;
            strength = Mathf.Clamp01(strength);
            _knockOffset += new Vector3(local.x * 0.03f, -0.035f, local.z * 0.04f) * strength;
            _knockTilt += new Vector3(-local.y * 2.2f + 1.2f, local.x * 1.6f, -local.x * 2.4f) * strength;
        }

        private void Update()
        {
            if (_camera == null || _player == null) return;
            float dt = Time.deltaTime;
            float amount = GameSettings.ShakeAmount;
            if (dt <= 0f) return; // paused: the view holds where it is
            bool moving = !_player.ControlLocked && _player.IsGrounded;
            _air = Mathf.MoveTowards(_air, moving ? 1f : 0f, dt * (moving ? 6f : 10f));

            // Stride, in step with the footfalls the footsteps count: lowest as a foot lands, highest mid-stride.
            float phase = _steps != null ? _steps.StridePhase : 0f;
            if (_steps != null && _steps.StepCount != _lastCount) { _lastCount = _steps.StepCount; _foot = -_foot; }
            float pace = Mathf.Clamp01(_player.CurrentSpeed / 5f) * (_player.IsCrouching ? 0.5f : 1f) * _air;
            float sink = -Mathf.Cos(phase * Mathf.PI * 2f) * 0.5f * _bobHeight * pace;
            float sway = Mathf.Sin(phase * Mathf.PI) * _foot * _bobSway * pace;

            // Landing / push-off: down quickly, held a moment, back up within ~0.25 s.
            if (_dipHold > 0f) { _dipHold -= dt; _dip = Mathf.MoveTowards(_dip, _dipTarget, dt * 1.6f); }
            else { _dipTarget = 0f; _dip = Mathf.Lerp(_dip, 0f, 1f - Mathf.Exp(-14f * dt)); }

            // Sidestep lean (A/D), eased in and out.
            float side = _player.ControlLocked ? 0f : Mathf.Clamp(InputReader.Move.x, -1f, 1f);
            _lean = Mathf.Lerp(_lean, -side * _maxLean, 1f - Mathf.Exp(-7f * dt));

            // Knocks die away at once.
            float decay = 1f - Mathf.Exp(-11f * dt);
            _knockOffset = Vector3.Lerp(_knockOffset, Vector3.zero, decay);
            _knockTilt = Vector3.Lerp(_knockTilt, Vector3.zero, decay);

            Vector3 offset = new Vector3(sway, sink - _dip, 0f) + _knockOffset;
            Vector3 tilt = new Vector3(0f, 0f, _lean) + _knockTilt;
            _camera.MotionOffset = Vector3.ClampMagnitude(offset, _maxOffset) * amount;
            _camera.MotionTilt = Vector3.ClampMagnitude(tilt, _maxTilt) * amount;
        }
    }
}
