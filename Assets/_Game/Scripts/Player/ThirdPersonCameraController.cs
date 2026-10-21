using System.Collections.Generic;
using ThuyKieu.Core;
using ThuyKieu.Environment;
using UnityEngine;

namespace ThuyKieu.Player
{
    /// <summary>
    /// Over-the-shoulder orbit camera: follows a target transform, mouse rotates around it.
    /// Framing (distance, shoulder offset, FOV, height) blends towards the active <see cref="CameraZone"/>.
    /// Collisions pull the camera in quickly and let it ease back out; <see cref="CameraFadeable"/> walls are
    /// hidden instead of pushing the camera into the player's back.
    /// Chapters that opt into <c>_firstPerson</c> get Kiều's own eyes instead: no orbit, the view rides at the body's
    /// eye height, dialogue turns the head towards the speaker, and hiding places can narrow how far she can look.
    /// Lives on the camera, not on the player, so movement and camera stay independent.
    /// </summary>
    public class ThirdPersonCameraController : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("Usually the CameraTarget child of the Player.")]
        [SerializeField] private Transform target;
        [Tooltip("Used when no target is assigned: finds the object with this tag, then this child.")]
        [SerializeField] private string fallbackTargetTag = "Player";
        [SerializeField] private string fallbackTargetChildName = "CameraTarget";

        [Header("Framing")]
        [SerializeField] private float distance = 4.5f;
        [SerializeField] private Vector3 targetOffset = Vector3.zero;
        [Tooltip("Sideways offset of the pivot (positive = right shoulder). 0 keeps the classic centred orbit.")]
        [SerializeField] private float _shoulder = 0f;
        [Tooltip("Gameplay field of view. 0 keeps whatever the Camera component has.")]
        [SerializeField] private float _fov = 0f;
        [Tooltip("Field of view used for dialogue shots. 0 leaves the FOV unchanged during dialogue.")]
        [SerializeField] private float _dialogueFov = 0f;
        [Tooltip("How quickly the camera catches up with the target. 0 = instant.")]
        [SerializeField] private float followSmoothTime = 0.05f;

        [Header("Rotation")]
        [SerializeField] private float mouseSensitivity = 0.15f;
        [SerializeField] private float minPitch = -30f;
        [SerializeField] private float maxPitch = 70f;
        [SerializeField] private bool invertY = false;

        [Header("Cursor")]
        [SerializeField] private bool lockCursorOnStart = true;

        [Header("First person (opt-in per chapter)")]
        [SerializeField] private bool _firstPerson;
        [SerializeField] private PlayerMovement _body;

        public bool IsFirstPerson => _firstPerson && _body != null;
        /// <summary>First person, while control is locked: the head turns towards this point (the person speaking).</summary>
        public Vector3? LookTarget { get; set; }
        private bool _limitLook;
        private float _limitCentre, _limitRange;

        /// <summary>First person: keeps the view within <paramref name="halfRange"/> degrees of a heading (peeking out of a hiding place).</summary>
        public void LimitLook(float centreYaw, float halfRange) { _limitLook = true; _limitCentre = centreYaw; _limitRange = halfRange; yaw = centreYaw; }
        public void ClearLookLimit() { _limitLook = false; }
        public float Yaw => yaw;
        public float Pitch => pitch;

        /// <summary>First person: turns the eyes to a point at once (checkpoints, QA stills).</summary>
        public void LookAtNow(Vector3 point)
        {
            if (!IsFirstPerson) return;
            var euler = Quaternion.LookRotation(point - EyePosition()).eulerAngles;
            yaw = euler.y;
            pitch = Mathf.Clamp(NormalizeAngle(euler.x), minPitch, maxPitch);
            transform.SetPositionAndRotation(EyePosition(), Quaternion.Euler(pitch, yaw, 0f));
        }

        private float yaw;
        private float pitch;
        private Vector3 followVelocity;
        [SerializeField] private LayerMask _collisionMask;
        public bool ControlLocked { get; set; }
        private Transform _dialogueCue, _lastCue;
        private Vector3 _lastCuePosition;
        private Quaternion _lastCueRotation;
        public void SetDialogueCue(Transform preset) { _dialogueCue = preset; }

        /// <summary>Hard cut to a shot (no blend): for shots on the far side of a wall, which a blend would get stuck on.</summary>
        public void CutTo(Transform preset)
        {
            _dialogueCue = preset;
            if (preset != null) transform.SetPositionAndRotation(preset.position, preset.rotation);
        }

        /// <summary>Observation zoom requested by ObserveMode (0 = none). Tightens framing over the shoulder.</summary>
        public float ObserveFov { get; set; }

        private Camera _camera;
        private float _baseFov, _currentDistance, _currentShoulder, _currentHeight, _collisionDistance = -1f;
        private readonly RaycastHit[] _hits = new RaycastHit[16];
        private readonly HashSet<CameraFadeable> _faded = new HashSet<CameraFadeable>();
        private readonly HashSet<CameraFadeable> _fadeNow = new HashSet<CameraFadeable>();

        // ---------------------------------------------------------------- close view (first person)

        private Transform _closeView, _closeLast;
        private float _closeFov, _closeWeight;
        [Tooltip("First person: seconds to lean into a close view and back out of it.")]
        [SerializeField] private float _closeBlendSeconds = 0.5f;

        /// <summary>
        /// First person: lean in to a close shot (taking the keys from Tú Bà's belt). The eyes blend to <paramref name="view"/>
        /// and the lens to <paramref name="fov"/>; the mouse no longer turns the head while it is set (the shot owns it).
        /// </summary>
        public void SetCloseView(Transform view, float fov)
        {
            _closeView = view;
            _closeLast = view;
            _closeFov = fov;
        }

        /// <summary>
        /// Back to her own eyes, turned towards <paramref name="lookAt"/> (where the close shot was), blending out unless
        /// <paramref name="instant"/> (a catch scene takes the camera over from where it is).
        /// </summary>
        public void ClearCloseView(Vector3 lookAt, bool instant)
        {
            _closeView = null;
            if (_body != null)
            {
                var euler = Quaternion.LookRotation(lookAt - EyePosition()).eulerAngles;
                yaw = euler.y;
                pitch = Mathf.Clamp(NormalizeAngle(euler.x), minPitch, maxPitch);
            }
            if (instant) _closeWeight = 0f;
        }

        /// <summary>0 at her eyes, 1 fully in the close view.</summary>
        public float CloseWeight => _closeWeight;
        public bool InCloseView => _closeView != null;

        /// <summary>Places the orbit camera behind the target at the given yaw without smoothing (after teleports).</summary>
        public void SnapBehindTarget(float yawDegrees, float pitchDegrees = 14f)
        {
            _closeView = null;
            _closeWeight = 0f;
            yaw = yawDegrees;
            pitch = Mathf.Clamp(pitchDegrees, minPitch, maxPitch);
            followVelocity = Vector3.zero;
            _collisionDistance = -1f;
            if (IsFirstPerson)
            {
                // Eyes look ahead after a teleport; the orbit's downward tilt would stare at the floor.
                pitch = 0f;
                transform.SetPositionAndRotation(EyePosition(), Quaternion.Euler(pitch, yaw, 0f));
                return;
            }
            if (target == null) return;
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            transform.SetPositionAndRotation(Pivot(rotation) - rotation * Vector3.forward * _currentDistance, rotation);
        }

        private void Awake()
        {
            if (target == null)
            {
                target = ResolveFallbackTarget();
            }

            Vector3 euler = transform.eulerAngles;
            yaw = euler.y;
            pitch = Mathf.Clamp(NormalizeAngle(euler.x), minPitch, maxPitch);
            _camera = GetComponent<Camera>();
            _baseFov = _camera != null ? _camera.fieldOfView : 60f;
            _currentDistance = distance;
            _currentShoulder = _shoulder;
        }

        private void Start()
        {
            if (lockCursorOnStart)
            {
                SetCursorLocked(true);
            }
            // Her own body stays out of the picture but still casts its shadow on the floor.
            if (IsFirstPerson)
                foreach (var renderer in _body.GetComponentsInChildren<Renderer>(true))
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }

        private void OnDisable() { RestoreFaded(); }

        private Vector3 EyePosition() => _body.transform.position + Vector3.up * _body.EyeHeight;

        private void FirstPersonView()
        {
            bool close = _closeView != null;
            _closeWeight = Mathf.MoveTowards(_closeWeight, close ? 1f : 0f, Time.deltaTime / Mathf.Max(0.01f, _closeBlendSeconds));
            if (!ControlLocked) HandleCursorToggle();
            if (!close && _closeWeight <= 0f && !ControlLocked && Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 look = InputReader.Look * mouseSensitivity * GameSettings.MouseSensitivity;
                yaw += look.x;
                pitch += invertY != GameSettings.InvertY ? look.y : -look.y;
            }
            else if (ControlLocked && LookTarget.HasValue)
            {
                // Turn the head towards whoever speaks, as a person would; never snap.
                Vector3 to = LookTarget.Value - EyePosition();
                if (to.sqrMagnitude > 0.01f)
                {
                    var wanted = Quaternion.LookRotation(to).eulerAngles;
                    float k = 1 - Mathf.Exp(-3.5f * Time.deltaTime);
                    yaw = Mathf.LerpAngle(yaw, wanted.y, k);
                    pitch = Mathf.LerpAngle(pitch, NormalizeAngle(wanted.x), k);
                }
            }
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            if (_limitLook) yaw = _limitCentre + Mathf.Clamp(Mathf.DeltaAngle(_limitCentre, yaw), -_limitRange, _limitRange);
            float ownFov = ObserveFov > 0f ? ObserveFov : _fov > 0f ? _fov : _baseFov;
            if (_closeWeight > 0f && _closeLast != null)
            {
                // Leaning in (or back out): from her eyes to the close shot, never through what lies between.
                float w = Mathf.SmoothStep(0f, 1f, _closeWeight);
                Vector3 eye = EyePosition();
                Vector3 shot = ResolveObstruction(eye, _closeLast.position);
                transform.SetPositionAndRotation(Vector3.Lerp(eye, shot, w), Quaternion.Slerp(Quaternion.Euler(pitch, yaw, 0f), _closeLast.rotation, w));
                if (_camera != null) _camera.fieldOfView = Mathf.Lerp(ownFov, _closeFov, w);
                return;
            }
            transform.SetPositionAndRotation(EyePosition(), Quaternion.Euler(pitch, yaw, 0f));
            BlendFov(ownFov, ObserveFov > 0f ? 8f : 3f);
        }

        private void LateUpdate()
        {
            if (IsFirstPerson && !(ControlLocked && _dialogueCue != null)) { _lastCue = null; FirstPersonView(); return; }
            if (ControlLocked && _dialogueCue != null)
            {
                RestoreFaded();
                Vector3 shotPosition = _dialogueCue.position;
                if (target != null && _dialogueCue.IsChildOf(target.root))
                    shotPosition = ResolveObstruction(target.position, shotPosition);
                // A new shot is a cut, as in film: blending across the room drags the lens through people and walls
                // (and a wall on the way leaves it stuck). Only small reframings, with nothing in between, glide.
                bool newShot = _dialogueCue != _lastCue || (shotPosition - _lastCuePosition).sqrMagnitude > 0.04f
                               || Quaternion.Angle(_dialogueCue.rotation, _lastCueRotation) > 4f;
                _lastCue = _dialogueCue;
                _lastCuePosition = shotPosition;
                _lastCueRotation = _dialogueCue.rotation;
                if (newShot && (Vector3.Distance(transform.position, shotPosition) > 1.2f
                                || Quaternion.Angle(transform.rotation, _dialogueCue.rotation) > 35f
                                || ResolveObstruction(transform.position, shotPosition) != shotPosition))
                {
                    transform.SetPositionAndRotation(shotPosition, _dialogueCue.rotation);
                    if (_dialogueFov > 0f) BlendFov(_dialogueFov, 1000f);
                    _collisionDistance = -1f;
                    return;
                }
                Vector3 nextPosition = Vector3.Lerp(transform.position, shotPosition, 1 - Mathf.Exp(-5 * Time.deltaTime));
                transform.position = ResolveObstruction(transform.position, nextPosition);
                transform.rotation = Quaternion.Slerp(transform.rotation, _dialogueCue.rotation, 1 - Mathf.Exp(-5 * Time.deltaTime));
                if (_dialogueFov > 0f) BlendFov(_dialogueFov, 5f);
                _collisionDistance = -1f;
                return;
            }
            _lastCue = null;
            if (target == null)
            {
                return;
            }

            if (!ControlLocked) HandleCursorToggle();

            if (!ControlLocked && Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 look = InputReader.Look * mouseSensitivity * GameSettings.MouseSensitivity;
                yaw += look.x;
                pitch += invertY != GameSettings.InvertY ? look.y : -look.y;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }

            // Framing: blend towards the zone the player stands in; observing pulls in over the shoulder.
            var zone = CameraZone.Find(target.position);
            float wantDistance = zone != null ? zone.Distance : distance;
            float wantShoulder = zone != null ? zone.Shoulder : _shoulder;
            float wantHeight = zone != null ? zone.Height : 0f;
            float wantFov = zone != null && zone.Fov > 0f ? zone.Fov : _fov > 0f ? _fov : _baseFov;
            if (ObserveFov > 0f)
            {
                wantDistance *= 0.8f;
                wantShoulder = Mathf.Max(wantShoulder * 1.3f, 0.5f);
                wantFov = ObserveFov;
            }
            float k = 1 - Mathf.Exp(-3f * Time.deltaTime);
            _currentDistance = Mathf.Lerp(_currentDistance, wantDistance, k);
            _currentShoulder = Mathf.Lerp(_currentShoulder, wantShoulder, k);
            _currentHeight = Mathf.Lerp(_currentHeight, wantHeight, k);
            BlendFov(wantFov, ObserveFov > 0f ? 8f : 3f);

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 pivot = Pivot(rotation);
            Vector3 back = -(rotation * Vector3.forward);
            float allowed = CastBack(pivot, back, _currentDistance);
            // Pull in fast (never see through a wall), ease back out slowly (no popping).
            if (_collisionDistance < 0f) _collisionDistance = allowed;
            _collisionDistance = allowed < _collisionDistance
                ? Mathf.Lerp(_collisionDistance, allowed, 1 - Mathf.Exp(-30f * Time.deltaTime))
                : Mathf.Lerp(_collisionDistance, allowed, 1 - Mathf.Exp(-4f * Time.deltaTime));
            Vector3 desiredPosition = pivot + back * _collisionDistance;

            transform.position = followSmoothTime > 0f
                ? Vector3.SmoothDamp(transform.position, desiredPosition, ref followVelocity, followSmoothTime)
                : desiredPosition;

            transform.rotation = rotation;
        }

        /// <summary>Target point plus the shoulder offset, pulled back towards the target if a wall is in the way.</summary>
        private Vector3 Pivot(Quaternion rotation)
        {
            Vector3 centre = target.position + targetOffset + Vector3.up * _currentHeight;
            if (Mathf.Abs(_currentShoulder) < 0.001f) return centre;
            Vector3 side = rotation * Vector3.right * _currentShoulder;
            if (_collisionMask.value != 0 && Physics.SphereCast(centre, 0.15f, side.normalized, out RaycastHit hit, side.magnitude, _collisionMask, QueryTriggerInteraction.Ignore)
                && hit.collider.GetComponentInParent<CameraFadeable>() == null)
                side = side.normalized * Mathf.Max(0f, hit.distance - 0.05f);
            return centre + side;
        }

        /// <summary>Distance the camera may sit behind the pivot. Fadeable walls in front of that point are hidden.</summary>
        private float CastBack(Vector3 pivot, Vector3 back, float length)
        {
            _fadeNow.Clear();
            float allowed = length;
            if (_collisionMask.value != 0)
            {
                int count = Physics.SphereCastNonAlloc(pivot, 0.22f, back, _hits, length, _collisionMask, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < count; i++)
                {
                    var hit = _hits[i];
                    if (hit.distance <= 0f && hit.point == Vector3.zero) continue; // started inside: ignore
                    if (hit.collider.GetComponentInParent<CameraFadeable>() != null) continue;
                    allowed = Mathf.Min(allowed, Mathf.Max(0.35f, hit.distance - 0.1f));
                }
                for (int i = 0; i < count; i++)
                {
                    var fade = _hits[i].collider.GetComponentInParent<CameraFadeable>();
                    if (fade != null && _hits[i].distance < allowed + 0.3f) _fadeNow.Add(fade);
                }
            }
            foreach (var fade in _faded) if (fade != null && !_fadeNow.Contains(fade)) fade.SetHidden(false);
            foreach (var fade in _fadeNow) fade.SetHidden(true);
            _faded.Clear();
            _faded.UnionWith(_fadeNow);
            return allowed;
        }

        private void RestoreFaded()
        {
            foreach (var fade in _faded) if (fade != null) fade.SetHidden(false);
            _faded.Clear();
        }

        private void BlendFov(float fov, float speed)
        {
            if (_camera == null || fov <= 0f) return;
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, fov, 1 - Mathf.Exp(-speed * Time.deltaTime));
        }

        private Vector3 ResolveObstruction(Vector3 origin, Vector3 destination)
        {
            Vector3 delta = destination - origin;
            float length = delta.magnitude;
            if (_collisionMask.value != 0 && length > .001f && Physics.SphereCast(origin, .15f,
                    delta / length, out RaycastHit hit, length, _collisionMask, QueryTriggerInteraction.Ignore))
                return origin + delta / length * Mathf.Max(0, hit.distance - .03f);
            return destination;
        }

        /// <summary>Lets other systems (dialogue, menus) release camera control later on.</summary>
        public void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private void HandleCursorToggle()
        {
            if (InputReader.CancelPressedThisFrame && !GamePause.EscapeConsumed) // the game shell turns Esc into its pause menu
            {
                SetCursorLocked(Cursor.lockState != CursorLockMode.Locked);
            }
        }

        private Transform ResolveFallbackTarget()
        {
            if (string.IsNullOrEmpty(fallbackTargetTag))
            {
                return null;
            }

            GameObject player = GameObject.FindGameObjectWithTag(fallbackTargetTag);
            if (player == null)
            {
                return null;
            }

            Transform child = player.transform.Find(fallbackTargetChildName);
            return child != null ? child : player.transform;
        }

        private static float NormalizeAngle(float angle)
        {
            return angle > 180f ? angle - 360f : angle;
        }
    }
}
