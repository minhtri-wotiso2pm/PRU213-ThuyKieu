using UnityEngine;

namespace ThuyKieu.Gameplay.Chapters
{
    /// <summary>Brief two-bone arm pose used only for Kiều's first-person waking shot.</summary>
    public sealed class WakeFirstPersonGesture : MonoBehaviour
    {
        private Animator _animator;
        private Transform _upperArm;
        private Transform _lowerArm;
        private Transform _hand;
        private Transform _view;

        public float Weight { get; set; }

        public void Initialize(Animator animator, Transform view)
        {
            _animator = animator;
            _view = view;
            if (_animator == null || !_animator.isHuman) return;
            _upperArm = _animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            _lowerArm = _animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            _hand = _animator.GetBoneTransform(HumanBodyBones.RightHand);
        }

        private void LateUpdate()
        {
            float weight = Mathf.Clamp01(Weight);
            if (weight <= 0f || _view == null || _upperArm == null || _lowerArm == null || _hand == null) return;

            // Keep the palm just below and to the right of the lens, as a person shielding newly-opened eyes.
            Vector3 target = _view.position + _view.forward * 0.31f + _view.right * 0.1f + _view.up * 0.09f;
            for (int i = 0; i < 3; i++)
            {
                AimJoint(_lowerArm, target, weight);
                AimJoint(_upperArm, target, weight);
            }
        }

        private void AimJoint(Transform joint, Vector3 target, float weight)
        {
            Vector3 current = _hand.position - joint.position;
            Vector3 desired = target - joint.position;
            if (current.sqrMagnitude < 0.0001f || desired.sqrMagnitude < 0.0001f) return;
            Quaternion aimed = Quaternion.FromToRotation(current, desired) * joint.rotation;
            joint.rotation = Quaternion.Slerp(joint.rotation, aimed, weight);
        }
    }
}
