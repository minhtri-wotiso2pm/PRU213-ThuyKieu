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

        [Header("Chase run (opt-in at runtime, Chapter 5)")]
        [Tooltip("Clearance of the feet at the top of a jump (metres).")]
        [SerializeField] private float _jumpHeight = 1.1f;
        [Tooltip("A jump still counts this long after running off an edge, and a press this long before landing is kept.")]
        [SerializeField] private float _coyoteTime = 0.12f, _jumpBuffer = 0.14f;
        [Tooltip("Running into something solid at body height (a culm, low culms): how long the stumble lasts and how much speed it leaves.")]
        [SerializeField] private float _stumbleSeconds = 0.6f, _stumbleSpeed = 0.25f;
        [Tooltip("Catching a foot on something low (a fallen culm): she trips and stops, and needs this long to get her stride back.")]
        [SerializeField] private float _tripSeconds = 1f;

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

        /// <summary>
        /// Chase run: the forward intent is held for the player at <see cref="AutoRunSpeed"/> along the way the body faces;
        /// A/D still strafe, W/S and Shift add nothing. It never steers: turns are the player's (the mouse in first person).
        /// </summary>
        public bool AutoRun { get; set; }
        public float AutoRunSpeed { get; set; } = 5f;
        /// <summary>Speed of a ducking run under something low while <see cref="AutoRun"/> is on.</summary>
        public float CrouchRunSpeed { get; set; } = 3f;
        /// <summary>Space jumps (off by default: no other chapter jumps).</summary>
        public bool JumpEnabled { get; set; }
        /// <summary>
        /// Ground steeper than the controller's slope limit is not ground (off by default): she slides down it and cannot
        /// jump from it, so a bank too steep to walk up cannot be climbed by jumping again and again from its face either.
        /// </summary>
        public bool SlideOffSteep { get; set; }
        public bool IsGrounded => controller != null && controller.isGrounded && !_onSteep;
        public float VerticalVelocity => verticalVelocity;
        /// <summary>0–1 of the stumble still to recover from (0 = full stride).</summary>
        public float Stumbling => _stumbleLeft > 0f ? _stumbleLeft / Mathf.Max(0.01f, _stumbleTotal) : 0f;
        public event System.Action Jumped;
        /// <summary>Feet back on the ground; the argument is the downward speed at touchdown (m/s).</summary>
        public event System.Action<float> Landed;
        /// <summary>Ran into something solid at speed (the hit point).</summary>
        public event System.Action<Vector3> Bumped;
        /// <summary>The head met something on the way up.</summary>
        public event System.Action HeadHit;

        private float _jumpPressedAt = -1f, _groundedAt = -1f, _stumbleLeft, _stumbleTotal, _bumpCooldown;
        private bool _wasGrounded = true, _onSteep;
        /// <summary>The surface under her feet (see <see cref="Steep"/>).</summary>
        private Vector3 _floorNormal;

        /// <summary>The last thing the capsule touched (its normal): which way to push her off a face with nothing under her.</summary>
        private Vector3 _contactNormal = Vector3.up;

        /// <summary>
        /// What carries her is steeper than she can stand on: a short sphere cast down from the bottom of the capsule, a
        /// little narrower than it, finds what is under her feet (not what she is pressed against at her side). The
        /// controller counts a face touched low on her side as ground; with nothing under her feet and that face steep,
        /// it is no footing.
        /// </summary>
        private bool Steep(out Vector3 normal)
        {
            float radius = controller.radius * 0.7f, level = Mathf.Cos((controller.slopeLimit + 3f) * Mathf.Deg2Rad);
            int mask = ~(1 << gameObject.layer);
            Vector3 bottom = transform.position + controller.center + Vector3.down * (controller.height * 0.5f - controller.radius);
            // Level ground under any part of her feet settles it (the sphere may graze a plank's edge between two planks,
            // or the foot of a bank beside her).
            for (int i = 0; i < 5; i++)
            {
                Vector3 from = bottom + (i == 0 ? Vector3.zero : new Vector3(i == 1 ? 1f : i == 2 ? -1f : 0f, 0f, i == 3 ? 1f : i == 4 ? -1f : 0f) * controller.radius * 0.6f);
                if (Physics.Raycast(from, Vector3.down, out var under, controller.radius + 0.3f, mask, QueryTriggerInteraction.Ignore) && under.normal.y >= level)
                {
                    normal = under.normal;
                    return false;
                }
            }
            if (!Physics.SphereCast(bottom + Vector3.up * 0.05f, radius, Vector3.down, out var hit, 0.3f, mask, QueryTriggerInteraction.Ignore))
            {
                // Nothing under her: standing on an edge (of a log, a lip) is footing, hanging on a face is not.
                normal = _contactNormal;
                return normal.y < level;
            }
            normal = hit.normal;
            if (normal.y >= level) return false;
            // The sphere met an edge (the end of a plank, the top of a log) rather than a face: there is level top just
            // beside the point it touched, on one side or the other.
            Vector3 across = Vector3.ProjectOnPlane(bottom - hit.point, Vector3.up).normalized * 0.05f;
            foreach (float side in new[] { 1f, -1f })
                if (Physics.Raycast(hit.point + across * side + Vector3.up * 0.1f, Vector3.down, out var top, 0.2f, mask, QueryTriggerInteraction.Ignore) && top.normal.y >= level)
                    return false;
            return true;
        }

        /// <summary>Loses the stride for a moment (a trip, a knock); speed comes back over <paramref name="seconds"/>.</summary>
        public void Stumble(float seconds, float speedLeft)
        {
            _stumbleTotal = Mathf.Max(seconds, 0.01f);
            _stumbleLeft = _stumbleTotal;
            _stumbleFloor = Mathf.Clamp01(speedLeft);
            _planarVelocity *= _stumbleFloor;
        }

        private float _stumbleFloor = 1f;

        /// <summary>First person on or off at runtime (a chapter that switches view for one sequence and back).</summary>
        public bool FirstPerson
        {
            get => _firstPerson;
            set
            {
                if (_firstPerson == value) return;
                _firstPerson = value;
                if (!value && controller != null)
                {
                    controller.height = _standHeight;
                    controller.center = new Vector3(controller.center.x, _standHeight * 0.5f, controller.center.z);
                }
                _eyeCurrent = _eyeHeight;
            }
        }

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
            bool autoRun = AutoRun && !ScriptedMove.HasValue && !ControlLocked && !HeldStill;
            Vector3 moveDirection = ScriptedMove.HasValue ? Flat(ScriptedMove.Value)
                : autoRun ? GetCameraRelativeDirection(new Vector2(InputReader.Move.x * 0.8f, 1f))
                : GetCameraRelativeDirection(ControlLocked || HeldStill ? Vector2.zero : InputReader.Move);

            // A scripted run ignores the crouch key; ForceCrouch (QA, story) still holds her down while it moves her.
            bool wantsCrouch = _allowCrouch && ((!ScriptedMove.HasValue && InputReader.IsCrouchHeld) || ForceCrouch) && !ControlLocked;
            // First person: someone crouched under a low beam stays down until there is room to stand.
            if (_firstPerson && IsCrouching && !wantsCrouch && !RoomToStand()) wantsCrouch = true;
            // Hidden, the place sets the posture: down behind something low (jars, cloth), upright in a wardrobe so her
            // eyes are level with the louvres, whatever posture she slipped in with.
            IsCrouching = _firstPerson && IsHidden ? ForceCrouch : wantsCrouch;
            bool wantsToRun = ScriptedMove.HasValue ? !IsCrouching && ScriptedSpeed > walkSpeed
                : autoRun ? !IsCrouching
                : !IsCrouching && CanRun && InputReader.IsRunHeld && moveDirection.sqrMagnitude > 0f;
            float speed = ScriptedMove.HasValue ? (IsCrouching ? Mathf.Min(ScriptedSpeed, _crouchSpeed) : ScriptedSpeed)
                : autoRun ? (IsCrouching ? CrouchRunSpeed : AutoRunSpeed) * StumbleFactor()
                : IsCrouching ? _crouchSpeed : wantsToRun ? runSpeed : walkSpeed * SpeedMultiplier;
            if (_hasCrouchParameter) _animator.SetBool(CrouchParameter, IsCrouching);

            if (_firstPerson && !ScriptedMove.HasValue) { FaceCamera(); FitCapsule(); }
            else RotateTowards(moveDirection);
            ApplyGravity();
            if (JumpEnabled) TryJump();

            _planarVelocity = Vector3.Lerp(_planarVelocity, moveDirection * speed, 1f - Mathf.Exp(-_moveSharpness * Time.deltaTime));
            Vector3 velocity = _planarVelocity;
            // Off a face too steep to stand on: pushed out from it as she slides down.
            if (_onSteep) velocity += Flat(_floorNormal) * 2.5f;
            velocity.y = verticalVelocity;
            Vector3 previousPosition = transform.position;
            float fallSpeed = -verticalVelocity;
            var flags = controller.Move(velocity * Time.deltaTime);
            // A jump that meets a low culm or a beam stops rising there instead of sliding along under it.
            if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f) { verticalVelocity = 0f; HeadHit?.Invoke(); }
            _onSteep = SlideOffSteep && controller.isGrounded && Steep(out _floorNormal);
            bool grounded = controller.isGrounded && !_onSteep;
            if (grounded) _groundedAt = Time.time;
            if (grounded && !_wasGrounded) Landed?.Invoke(fallSpeed);
            _wasGrounded = grounded;
            if (_bumpCooldown > 0f) _bumpCooldown -= Time.deltaTime;

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

        /// <summary>
        /// Space: a press is kept for a moment (pressed just before landing still jumps) and the ground for a moment after
        /// running off it (a late press at an edge). Never from a crouch, never in mid-air, never while the story holds her.
        /// </summary>
        private void TryJump()
        {
            if (ControlLocked || HeldStill || ScriptedMove.HasValue) { _jumpPressedAt = -1f; return; }
            if (InputReader.JumpPressedThisFrame) _jumpPressedAt = Time.time;
            if (_jumpPressedAt < 0f || Time.time - _jumpPressedAt > _jumpBuffer) return;
            if (IsCrouching || _groundedAt < 0f || Time.time - _groundedAt > _coyoteTime || verticalVelocity > 0.5f) return;
            verticalVelocity = Mathf.Sqrt(2f * -gravity * _jumpHeight);
            _jumpPressedAt = -1f;
            _groundedAt = -1f;
            Jumped?.Invoke();
        }

        private float StumbleFactor()
        {
            if (_stumbleLeft <= 0f) return 1f;
            _stumbleLeft -= Time.deltaTime;
            return Mathf.Lerp(1f, _stumbleFloor, Mathf.Clamp01(_stumbleLeft / _stumbleTotal));
        }

        /// <summary>
        /// Chase run only: running square into a culm, a log or a low arch knocks the stride out of her (she slows and picks
        /// up again); glancing along something only slides. Ground contacts are ignored.
        /// </summary>
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            _contactNormal = hit.normal;
            if (!AutoRun || _bumpCooldown > 0f || hit.normal.y > 0.5f) return;
            Vector3 normal = Flat(hit.normal);
            Vector3 heading = Flat(_planarVelocity);
            if (normal == Vector3.zero || _planarVelocity.magnitude < 2.4f || Vector3.Dot(heading, -normal) < 0.6f) return;
            _bumpCooldown = 0.6f;
            if (hit.point.y - transform.position.y < 0.6f) { Stumble(_tripSeconds, 0f); _planarVelocity = Vector3.zero; }
            else Stumble(_stumbleSeconds, _stumbleSpeed);
            Bumped?.Invoke(hit.point);
        }

        /// <summary>Moves the character instantly (checkpoints, cutscenes). CharacterController would otherwise override it.</summary>
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            controller.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            controller.enabled = true;
            verticalVelocity = 0f;
            _planarVelocity = Vector3.zero;
            _stumbleLeft = 0f;
            _jumpPressedAt = -1f;
            _wasGrounded = true;
            _onSteep = false;
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
            if (controller.isGrounded && !_onSteep && verticalVelocity < 0f)
            {
                verticalVelocity = groundedStickForce;
                return;
            }

            verticalVelocity += gravity * Time.deltaTime;
        }
    }
}
