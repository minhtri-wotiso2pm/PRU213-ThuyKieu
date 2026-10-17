using UnityEngine;

namespace ThuyKieu.Player
{
    /// <summary>
    /// Body fixes for the Tripo hanfu Kiều, applied on top of the Humanoid animation:
    /// - the sleeves are skinned tightly to the arms and flare at the elbows, which makes her look broad:
    ///   thin the upper arms (hands restored), narrow the SleeveDrape bones and tuck the elbows in;
    /// The sleeve drapes follow the animated forearms with a small delayed sway so the cloth does not
    /// look rigid. Humanoid clips never write bone scale, so the arm scale holds.
    /// </summary>
    [ExecuteAlways]
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
        [Tooltip("How softly the sleeve catches up with the wrist motion. Lower values feel silkier.")]
        [Range(2f, 20f)]
        [SerializeField] private float _sleeveFollowSpeed = 7f;
        [Tooltip("Rotates the upper arms towards the body after animation. Any tuck makes the hands sink into the skirt while walking.")]
        [Range(-10f, 15f)]
        [SerializeField] private float _elbowTuckDegrees = 0f;

        private Animator _animator;
        private Transform _leftArm, _rightArm, _leftHand, _rightHand, _leftThigh, _rightThigh;
        private Transform[] _drapes = new Transform[0];
        private Quaternion[] _drapeRest = new Quaternion[0];
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
            // Drapes are children of the forearms, so this runs after the arm tuck. Build the target from
            // their rest rotation, then ease towards it; snapping to rest each frame made the cloth rigid.
            float moveAmount = Mathf.InverseLerp(0.05f, 4.8f, Mathf.Abs(_animator.GetFloat(SpeedParameter)));
            float phase = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime * Mathf.PI * 2f;
            float sway = Mathf.Sin(phase) * _sleeveSwayDegrees * moveAmount;
            float follow = 1f - Mathf.Exp(-_sleeveFollowSpeed * Time.deltaTime);
            for (int i = 0; i < _drapes.Length; i++)
            {
                var drape = _drapes[i];
                float sign = drape.name.Contains("Left") ? -1f : 1f;
                Quaternion restWorld = drape.parent.rotation * _drapeRest[i];
                Quaternion target = Quaternion.AngleAxis(sign * _sleeveFlareDegrees, forward)
                    * Quaternion.AngleAxis(sway, transform.right) * restWorld;
                drape.rotation = Quaternion.Slerp(drape.rotation, target, follow);
            }
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
