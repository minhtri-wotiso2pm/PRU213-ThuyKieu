using UnityEngine;

namespace ThuyKieu.Player
{
    /// <summary>
    /// Body fixes for the Tripo hanfu Kiều, applied on top of the Humanoid animation:
    /// - the sleeves are skinned tightly to the arms and flare at the elbows, which makes her look broad:
    ///   thin the upper arms (hands restored), narrow the SleeveDrape bones and tuck the elbows in;
    /// The sleeve drapes hang plumb from the animated forearms and swing there on a soft spring,
    /// so the cloth falls like silk instead of jutting out or riding up with the wrist.
    /// Humanoid clips never write bone scale, so the arm scale holds.
    /// </summary>
    [ExecuteAlways]
    // After scripted arm poses (e.g. WakeFirstPersonGesture) so the drapes react to the final forearm.
    [DefaultExecutionOrder(500)]
    [RequireComponent(typeof(Animator))]
    public class BodyProportions : MonoBehaviour
    {
        [Header("Arms")]
        [Range(0.6f, 1f)]
        [SerializeField] private float _armThickness = 0.8f;
        [Tooltip("Width of the hanging sleeve cloth (SleeveDrape bones). Too narrow and it sinks into the skirt.")]
        [Range(0.4f, 1f)]
        [SerializeField] private float _sleeveWidth = 0.85f;
        [Tooltip("Swings the hanging sleeve cloth away from the body so it does not clip into the skirt.")]
        [Range(0f, 30f)]
        [SerializeField] private float _sleeveFlareDegrees = 15f;
        [Tooltip("Maximum secondary sway of the sleeve while moving.")]
        [Range(0f, 15f)]
        [SerializeField] private float _sleeveSwayDegrees = 7f;
        [Tooltip("Stiffness of the sleeve's swing towards where it hangs. Lower values feel silkier.")]
        [Range(2f, 20f)]
        [SerializeField] private float _sleeveFollowSpeed = 7f;
        [Tooltip("How much the swing overshoots and settles: lower is looser silk, 1 stops without swinging.")]
        [Range(0.2f, 1f)]
        [SerializeField] private float _sleeveDamping = 0.55f;
        [Tooltip("How strongly the sleeve hangs plumb instead of following the forearm (1 = like silk).")]
        [Range(0f, 1f)]
        [SerializeField] private float _sleeveGravity = 1f;
        [Tooltip("Tilt away from hanging plumb (plus flare) the sleeve may keep from the forearm's angle.")]
        [Range(0f, 60f)]
        [SerializeField] private float _sleeveFreeAngle = 8f;
        [Tooltip("Rotates the upper arms towards the body after animation. Any tuck makes the hands sink into the skirt while walking.")]
        [Range(-10f, 15f)]
        [SerializeField] private float _elbowTuckDegrees = 0f;

        private Animator _animator;
        private Transform _leftArm, _rightArm, _leftHand, _rightHand, _leftThigh, _rightThigh;
        private Transform[] _drapes = new Transform[0];
        private Quaternion[] _drapeRest = new Quaternion[0];
        private Quaternion[] _drapeWorld = new Quaternion[0];
        private Vector3[] _drapeSpin = new Vector3[0];
        private bool _drapeWorldValid;
        private static readonly int SpeedParameter = Animator.StringToHash("Speed");

        private void OnEnable()
        {
            Cache();
            ApplyArmScale();
            RestoreLegScale();
            RestoreLegScale();
        }

        // Arm/sleeve scale is saved in the prefab: set the sliders to 1 before removing this component.
        private void OnValidate()
        {
            Cache();
            ApplyArmScale();
        }

        private void LateUpdate()
        {
            if (_animator == null) return;
            // Clips exported from Blender may key the drape bones, so scale is re-applied after animation.
            ApplyArmScale();
            // Rotation is rewritten by the Animator every frame, so this only runs while it animates.
            if (!Application.isPlaying || !_animator.enabled) return;
            Vector3 forward = transform.forward;
            if (_elbowTuckDegrees != 0f)
            {
                if (_leftArm != null) _leftArm.rotation = Quaternion.AngleAxis(_elbowTuckDegrees, forward) * _leftArm.rotation;
                if (_rightArm != null) _rightArm.rotation = Quaternion.AngleAxis(-_elbowTuckDegrees, forward) * _rightArm.rotation;
            }
            // Drapes are children of the forearms, so this runs after the arm tuck. The sleeve is one rigid bone:
            // following the forearm's angle made it jut out like a stick whenever the arm came forward, so it
            // hangs plumb (flared off the skirt) and swings there on a soft spring from last frame's rotation.
            float moveAmount = Mathf.InverseLerp(0.05f, 4.8f, Mathf.Abs(_animator.GetFloat(SpeedParameter)));
            float phase = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime * Mathf.PI * 2f;
            float sway = Mathf.Sin(phase) * _sleeveSwayDegrees * moveAmount;
            float dt = Mathf.Min(Time.deltaTime, 1f / 30f);
            float stiffness = _sleeveFollowSpeed * _sleeveFollowSpeed;
            float damping = 2f * _sleeveDamping * _sleeveFollowSpeed;
            for (int i = 0; i < _drapes.Length; i++)
            {
                var drape = _drapes[i];
                float sign = drape.name.Contains("Left") ? -1f : 1f;
                Vector3 hang = Quaternion.AngleAxis(sign * _sleeveFlareDegrees, forward)
                    * Quaternion.AngleAxis(sway, transform.right) * Vector3.down;
                Quaternion target = Hang(drape.parent.rotation * _drapeRest[i], hang);
                if (!_drapeWorldValid) { _drapeWorld[i] = target; _drapeSpin[i] = Vector3.zero; }
                // Angular spring: accelerate towards the target, damped just under critical so silk sways and settles.
                (target * Quaternion.Inverse(_drapeWorld[i])).ToAngleAxis(out float angle, out Vector3 axis);
                if (angle > 180f) angle -= 360f;
                Vector3 error = float.IsInfinity(axis.x) ? Vector3.zero : axis * (angle * Mathf.Deg2Rad);
                _drapeSpin[i] += (error * stiffness - _drapeSpin[i] * damping) * dt;
                float step = _drapeSpin[i].magnitude * dt * Mathf.Rad2Deg;
                if (step > 0.0001f) _drapeWorld[i] = Quaternion.AngleAxis(step, _drapeSpin[i].normalized) * _drapeWorld[i];
                drape.rotation = _drapeWorld[i];
            }
            _drapeWorldValid = true;
        }

        private void OnDisable() => _drapeWorldValid = false;

        /// <summary>Swings a drape's length (local Y) from the forearm's angle to the hanging direction, keeping the free angle.</summary>
        private Quaternion Hang(Quaternion rotation, Vector3 hang)
        {
            Vector3 along = rotation * Vector3.up;
            float excess = Vector3.Angle(along, hang) - _sleeveFreeAngle;
            if (excess <= 0f || _sleeveGravity <= 0f) return rotation;
            Vector3 axis = Vector3.Cross(along, hang);
            // Pointing straight up: any horizontal axis will do; use the body's side axis.
            if (axis.sqrMagnitude < 0.0001f) axis = transform.right;
            return Quaternion.AngleAxis(excess * _sleeveGravity, axis.normalized) * rotation;
        }

        private void Cache()
        {
            _animator = GetComponent<Animator>();
            if (_animator == null || !_animator.isHuman) { _animator = null; return; }
            _leftArm = _animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            _rightArm = _animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            _leftHand = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
            _rightHand = _animator.GetBoneTransform(HumanBodyBones.RightHand);
            _leftThigh = _animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            _rightThigh = _animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            var drapes = new System.Collections.Generic.List<Transform>();
            foreach (var bone in GetComponentsInChildren<Transform>(true))
                if (bone.name.EndsWith("SleeveDrape")) drapes.Add(bone);
            _drapes = drapes.ToArray();
            if (_drapeWorld.Length != _drapes.Length || _drapeSpin.Length != _drapes.Length)
            {
                _drapeWorld = new Quaternion[_drapes.Length];
                _drapeSpin = new Vector3[_drapes.Length];
                _drapeWorldValid = false;
            }
            // Cache the rest pose once; OnValidate during Play must not capture an already flared rotation.
            if (_drapeRest.Length == _drapes.Length) return;
            _drapeRest = new Quaternion[_drapes.Length];
            for (int i = 0; i < _drapes.Length; i++) _drapeRest[i] = _drapes[i].localRotation;
        }

        private void RestoreLegScale()
        {
            SetScale(_leftThigh, Vector3.one);
            SetScale(_rightThigh, Vector3.one);
        }

        private void ApplyArmScale()
        {
            if (_animator == null) return;
            // Mixamo arm bones and the drape bones point along local Y, so X/Z are the thickness axes.
            var thin = new Vector3(_armThickness, 1f, _armThickness);
            var restore = new Vector3(1f / _armThickness, 1f, 1f / _armThickness);
            var drape = new Vector3(_sleeveWidth, 1f, _sleeveWidth);
            SetScale(_leftArm, thin);
            SetScale(_rightArm, thin);
            SetScale(_leftHand, restore);
            SetScale(_rightHand, restore);
            foreach (var bone in _drapes) SetScale(bone, drape);
        }

        private static void SetScale(Transform bone, Vector3 scale)
        {
            if (bone != null && bone.localScale != scale) bone.localScale = scale;
        }
    }
}
