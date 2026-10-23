using ThuyKieu.Core;
using UnityEngine;

namespace ThuyKieu.Player
{
    /// <summary>
    /// Camera-relative character movement. This class owns movement only:
    /// no camera logic, no interaction, no animation state machine.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Speed")]
        [SerializeField] private float walkSpeed = 2.5f;
        [SerializeField] private float runSpeed = 5.5f;
        [Tooltip("How fast the character turns towards the movement direction (deg/s).")]
        [SerializeField] private float rotationSpeed = 720f;
        [Tooltip("How quickly speed eases towards the target; lower = softer starts and stops.")]
        [SerializeField] private float _moveSharpness = 9f;
        [Tooltip("How quickly the facing eases towards the movement direction.")]
        [SerializeField] private float _turnSharpness = 12f;

        [Header("Gravity")]
        [SerializeField] private float gravity = -20f;
        [Tooltip("Downward force kept while grounded so the controller stays glued to slopes.")]
        [SerializeField] private float groundedStickForce = -2f;

        [Header("References")]
        [Tooltip("Movement is relative to this transform. Leave empty to fall back to Camera.main.")]
        [SerializeField] private Transform cameraTransform;

        [Header("Animation")]
        [SerializeField] private Animator _animator;
        [SerializeField] private float _animationSmoothTime = 0.08f;
        [Tooltip("Use 0 / 0.5 / 1 for idle / walk / run. Leave off for controllers using metres per second.")]
        [SerializeField] private bool _normalizeAnimatorSpeed;
        [Tooltip("Fastest walk/run playback used to keep the steps under a body that moves faster than the clips "
                 + "(read from the clips' own ground speed). 1 turns the step sync off.")]
        [SerializeField] private float _maxStepRate = 2.2f;
        private static readonly int SpeedParameter = Animator.StringToHash("Speed");
        private static readonly int CrouchParameter = Animator.StringToHash("Crouch");

        [Header("Crouch (opt-in per chapter)")]
        [SerializeField] private bool _allowCrouch;
        [SerializeField] private float _crouchSpeed = 1.15f;

        [Header("First person (opt-in per chapter)")]
        [Tooltip("The body faces where the camera looks and strafes; crouching lowers the capsule and the eyes.")]
        [SerializeField] private bool _firstPerson;
        [SerializeField] private float _eyeHeight = 1.56f;
        [SerializeField] private float _crouchEyeHeight = 0.98f;
        [SerializeField] private float _crouchCapsuleHeight = 1.08f;

        private CharacterController controller;
        private float verticalVelocity;
        private Vector3 _planarVelocity;
        private bool _hasCrouchParameter, _parametersCached;
        private float _walkClipSpeed, _runClipSpeed;
        private float _standHeight, _eyeCurrent;
        private readonly Collider[] _ceiling = new Collider[4];

        /// <summary>True while sneaking. Stealth systems read this to lower visibility.</summary>
        public bool IsCrouching { get; private set; }
        public bool AllowCrouch { get => _allowCrouch; set => _allowCrouch = value; }
        /// <summary>Scales walking speed, e.g. while observing. 1 = normal.</summary>
        public float SpeedMultiplier { get; set; } = 1f;
        /// <summary>Crouch without input (QA autopilot).</summary>
        public bool ForceCrouch { get; set; }

        /// <summary>Current planar speed in units/second. Useful for the animation team.</summary>
        public float CurrentSpeed { get; private set; }

        /// <summary>True while the run input is held and the character is actually moving.</summary>
        public bool IsRunning { get; private set; }
        public bool ControlLocked { get; set; }
        /// <summary>
        /// Kept in place by an interaction (leaning over Tú Bà's keys): no walking, while crouch and everything else stay
        /// the player's. Separate from <see cref="ControlLocked"/>, which the story and menus own.
        /// </summary>
        public bool HeldStill { get; set; }
        /// <summary>Inside a hiding place: guards cannot see the player.</summary>
        public bool IsHidden { get; set; }
        /// <summary>Stamina or story beats can forbid sprinting.</summary>
        public bool CanRun { get; set; } = true;
        /// <summary>Story-driven movement (cinematic runs): while set, this direction and <see cref="ScriptedSpeed"/> replace the input.</summary>
        public Vector3? ScriptedMove { get; set; }
        public float ScriptedSpeed { get; set; } = 5.5f;

        public bool IsFirstPerson => _firstPerson;
        /// <summary>Height of the eyes above the feet, eased between standing and crouching (first person).</summary>
        public float EyeHeight => _eyeCurrent;
        public float WalkSpeed => walkSpeed;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            _standHeight = controller.height;
            _eyeCurrent = _eyeHeight;
            SetAnimator(_animator != null ? _animator : GetComponentInChildren<Animator>());

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        /// <summary>Drives another body (the classroom scenes swap Kiều for the modern student).</summary>
        public void SetAnimator(Animator animator)
        {
            if (_animator != null) _animator.speed = 1f;
            _animator = animator;
            _hasCrouchParameter = false;
            _parametersCached = false;
            _walkClipSpeed = _runClipSpeed = 0f;
            if (_animator == null) return;
            _animator.applyRootMotion = false;
            _walkClipSpeed = ClipGroundSpeed(_animator, "walk");
            _runClipSpeed = ClipGroundSpeed(_animator, "run");
        }

        private void Update()
        {
            CacheAnimatorParameters();
            Vector3 moveDirection = ScriptedMove.HasValue ? Flat(ScriptedMove.Value) : GetCameraRelativeDirection(ControlLocked || HeldStill ? Vector2.zero : InputReader.Move);

            // A scripted run ignores the crouch key; ForceCrouch (QA, story) still holds her down while it moves her.
            bool wantsCrouch = _allowCrouch && ((!ScriptedMove.HasValue && InputReader.IsCrouchHeld) || ForceCrouch) && !ControlLocked;
            // First person: someone crouched under a low beam stays down until there is room to stand.
            if (_firstPerson && IsCrouching && !wantsCrouch && !RoomToStand()) wantsCrouch = true;
            // Hidden, the place sets the posture: down behind something low (jars, cloth), upright in a wardrobe so her
            // eyes are level with the louvres, whatever posture she slipped in with.
            IsCrouching = _firstPerson && IsHidden ? ForceCrouch : wantsCrouch;
            bool wantsToRun = ScriptedMove.HasValue ? !IsCrouching && ScriptedSpeed > walkSpeed : !IsCrouching && CanRun && InputReader.IsRunHeld && moveDirection.sqrMagnitude > 0f;
            float speed = ScriptedMove.HasValue ? (IsCrouching ? Mathf.Min(ScriptedSpeed, _crouchSpeed) : ScriptedSpeed) : IsCrouching ? _crouchSpeed : wantsToRun ? runSpeed : walkSpeed * SpeedMultiplier;
            if (_hasCrouchParameter) _animator.SetBool(CrouchParameter, IsCrouching);

            if (_firstPerson && !ScriptedMove.HasValue) { FaceCamera(); FitCapsule(); }
            else RotateTowards(moveDirection);
            ApplyGravity();

            _planarVelocity = Vector3.Lerp(_planarVelocity, moveDirection * speed, 1f - Mathf.Exp(-_moveSharpness * Time.deltaTime));
            Vector3 velocity = _planarVelocity;
            velocity.y = verticalVelocity;
            Vector3 previousPosition = transform.position;
            controller.Move(velocity * Time.deltaTime);

            Vector3 displacement = transform.position - previousPosition;
            displacement.y = 0f;
            CurrentSpeed = Time.deltaTime > 0f ? displacement.magnitude / Time.deltaTime : 0f;
            IsRunning = wantsToRun && CurrentSpeed > walkSpeed + 0.01f;
            if (_animator != null && _animator.runtimeAnimatorController != null)
            {
                float animationSpeed = CurrentSpeed;
                if (_normalizeAnimatorSpeed)
                    animationSpeed = CurrentSpeed <= walkSpeed
                        ? Mathf.InverseLerp(0f, walkSpeed, CurrentSpeed) * 0.5f
                        : 0.5f + Mathf.InverseLerp(walkSpeed, runSpeed, CurrentSpeed) * 0.5f;
                _animator.SetFloat(SpeedParameter, animationSpeed, _animationSmoothTime, Time.deltaTime);
                SyncSteps();
            }
        }

        private void CacheAnimatorParameters()
        {
            if (_parametersCached || _animator == null || !_animator.isInitialized || _animator.runtimeAnimatorController == null) return;
            _parametersCached = true;
            foreach (var parameter in _animator.parameters)
                if (parameter.nameHash == CrouchParameter) _hasCrouchParameter = true;
        }

        /// <summary>
        /// The hanfu walk and run take small steps (about 0.5 and 0.9 m/s of ground), far slower than the movement, so
        /// the feet slid. Playing the clips faster in proportion keeps the steps under the body, up to a natural cadence.
        /// </summary>
        private void SyncSteps()
        {
            if (!_normalizeAnimatorSpeed || _maxStepRate <= 1f || _walkClipSpeed <= 0f) return;
            float blend = _animator.GetFloat(SpeedParameter);
            float clipSpeed = blend <= 0.5f
                ? Mathf.Lerp(0f, _walkClipSpeed, blend / 0.5f)
                : Mathf.Lerp(_walkClipSpeed, _runClipSpeed > 0f ? _runClipSpeed : _walkClipSpeed, (blend - 0.5f) / 0.5f);
            float rate = clipSpeed > 0.05f && CurrentSpeed > 0.1f ? Mathf.Clamp(CurrentSpeed / clipSpeed, 1f, _maxStepRate) : 1f;
            _animator.speed = Mathf.Lerp(_animator.speed, rate, 1f - Mathf.Exp(-8f * Time.deltaTime));
        }

        /// <summary>Ground speed of the controller's first clip whose name contains <paramref name="key"/> (0 for in-place clips).</summary>
        private static float ClipGroundSpeed(Animator animator, string key)
        {
            if (animator.runtimeAnimatorController == null) return 0f;
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip == null || clip.name.IndexOf(key, System.StringComparison.OrdinalIgnoreCase) < 0) continue;
                Vector3 speed = clip.averageSpeed;
                speed.y = 0f;
                if (speed.magnitude > 0.05f) return speed.magnitude;
            }
            return 0f;
        }

        /// <summary>Moves the character instantly (checkpoints, cutscenes). CharacterController would otherwise override it.</summary>
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            controller.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            controller.enabled = true;
            verticalVelocity = 0f;
            _planarVelocity = Vector3.zero;
        }

        private static Vector3 Flat(Vector3 direction)
        {
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.zero;
        }

        /// <summary>Projects the 2D input onto the horizontal plane of the camera.</summary>
        private Vector3 GetCameraRelativeDirection(Vector2 input)
        {
            if (input.sqrMagnitude < 0.0001f)
            {
                return Vector3.zero;
            }

            Vector3 forward = Vector3.forward;
            Vector3 right = Vector3.right;

            if (cameraTransform != null)
            {
                forward = cameraTransform.forward;
                right = cameraTransform.right;
            }

            forward.y = 0f;
            right.y = 0f;

            return (forward.normalized * input.y + right.normalized * input.x).normalized;
        }

        /// <summary>First person: the body turns with the view, so walking sideways is a strafe.</summary>
        private void FaceCamera()
        {
            if (cameraTransform == null || ControlLocked) return;
            Vector3 forward = cameraTransform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        }

        /// <summary>Lowers the capsule while crouched (so low gaps fit) and eases the eyes with it.</summary>
        private void FitCapsule()
        {
            float height = IsCrouching ? _crouchCapsuleHeight : _standHeight;
            if (!Mathf.Approximately(controller.height, height))
            {
                controller.height = height;
                controller.center = new Vector3(controller.center.x, height * 0.5f, controller.center.z);
            }
            float eye = IsCrouching ? _crouchEyeHeight : _eyeHeight;
            _eyeCurrent = Mathf.Lerp(_eyeCurrent, eye, 1 - Mathf.Exp(-9f * Time.deltaTime));
        }

        private bool RoomToStand()
        {
            float radius = controller.radius * 0.95f;
            Vector3 bottom = transform.position + Vector3.up * (_crouchCapsuleHeight - radius);
            Vector3 top = transform.position + Vector3.up * (_standHeight - radius);
            int hits = Physics.OverlapCapsuleNonAlloc(bottom, top, radius, _ceiling, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hits; i++) if (_ceiling[i].transform.root != transform.root) return false;
            return true;
        }

        private void RotateTowards(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Quaternion target = Quaternion.LookRotation(direction, Vector3.up);
            Quaternion eased = Quaternion.Slerp(transform.rotation, target, 1f - Mathf.Exp(-_turnSharpness * Time.deltaTime));
            transform.rotation = Quaternion.RotateTowards(transform.rotation, eased, rotationSpeed * Time.deltaTime);
        }

        private void ApplyGravity()
        {
            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = groundedStickForce;
                return;
            }

            verticalVelocity += gravity * Time.deltaTime;
        }
    }
}
