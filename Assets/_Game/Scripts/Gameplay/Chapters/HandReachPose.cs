using UnityEngine;

namespace ThuyKieu.Gameplay.Chapters
{
    /// <summary>
    /// Two-bone hand reaches and a head aim layered over the animation, for Kiều handling things in her room
    /// (her cheek in the mirror, the hồ cầm, the letter). Targets are wrist positions, set every frame.
    /// </summary>
    public sealed class HandReachPose : MonoBehaviour
    {
        private Transform _spine, _leftUpper, _leftLower, _leftHand, _rightUpper, _rightLower, _rightHand, _head;
        private Vector3? _left, _right, _look;
        private Vector3? _leftFingers, _leftPalm, _rightFingers, _rightPalm;
        private Vector3? _leftElbow, _rightElbow;
        private Transform[] _leftDigits = new Transform[0], _rightDigits = new Transform[0];

        public float Weight { get; set; }

        /// <summary>Forward bend at the waist in degrees, so she can reach things low or out of arm's length.</summary>
        public float Lean { get; set; }

        /// <summary>How much of the weight also turns the hands to their finger and palm directions.</summary>
        public float HandAim { get; set; } = 1f;

        /// <summary>Extra bend of each finger joint towards the palm, in degrees (the thumb bends half as much).</summary>
        public float LeftCurl { get; set; }
        public float RightCurl { get; set; }

        public Transform LeftHand => _leftHand;
        public Transform RightHand => _rightHand;

        /// <summary>Runs after the reach each frame, e.g. to keep a carried prop in the posed hand.</summary>
        public System.Action AfterPose { get; set; }

        public void Initialize(Animator animator)
        {
            if (animator == null || !animator.isHuman) return;
            _spine = animator.GetBoneTransform(HumanBodyBones.Spine);
            _leftUpper = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            _leftLower = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            _leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            _rightUpper = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            _rightLower = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            _rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            _head = animator.GetBoneTransform(HumanBodyBones.Head);
            _leftDigits = Digits(animator, HumanBodyBones.LeftThumbProximal);
            _rightDigits = Digits(animator, HumanBodyBones.RightThumbProximal);
        }

        /// <summary>The fifteen finger bones of one hand, thumb first, each finger root to tip (they are consecutive in the enum).</summary>
        private static Transform[] Digits(Animator animator, HumanBodyBones first)
        {
            var bones = new Transform[15];
            for (int i = 0; i < bones.Length; i++) bones[i] = animator.GetBoneTransform(first + i);
            return bones;
        }

        /// <summary>Null leaves that hand (or the head) to the animation.</summary>
        public void SetTargets(Vector3? left, Vector3? right, Vector3? look)
        {
            _left = left;
            _right = right;
            _look = look;
        }

        /// <summary>World directions the fingers point and the palm faces; null leaves that hand's turn to the animation.</summary>
        public void SetHandAim(Vector3? leftFingers, Vector3? leftPalm, Vector3? rightFingers, Vector3? rightPalm)
        {
            _leftFingers = leftFingers;
            _leftPalm = leftPalm;
            _rightFingers = rightFingers;
            _rightPalm = rightPalm;
        }

        /// <summary>World points each elbow swings towards (about the shoulder–wrist line, so the wrist stays put), e.g. to keep a forearm out of a held prop.</summary>
        public void SetElbowHints(Vector3? left, Vector3? right)
        {
            _leftElbow = left;
            _rightElbow = right;
        }

        private void LateUpdate()
        {
            float weight = Mathf.Clamp01(Weight);
            if (weight <= 0f) { AfterPose?.Invoke(); return; }
            if (_spine != null && Lean != 0f) _spine.rotation = Quaternion.AngleAxis(Lean * weight, transform.right) * _spine.rotation;
            if (_left.HasValue) Reach(_leftUpper, _leftLower, _leftHand, _left.Value, weight);
            if (_right.HasValue) Reach(_rightUpper, _rightLower, _rightHand, _right.Value, weight);
            if (_leftElbow.HasValue) SwingElbow(_leftUpper, _leftLower, _leftHand, _leftElbow.Value, weight);
            if (_rightElbow.HasValue) SwingElbow(_rightUpper, _rightLower, _rightHand, _rightElbow.Value, weight);
            // Mixamo hands on this rig: fingers along local +Y, the palm facing local +Z on both hands.
            if (_leftFingers.HasValue && _leftPalm.HasValue) Orient(_leftHand, _leftFingers.Value, _leftPalm.Value, weight * HandAim);
            if (_rightFingers.HasValue && _rightPalm.HasValue) Orient(_rightHand, _rightFingers.Value, _rightPalm.Value, weight * HandAim);
            Curl(_leftHand, _leftDigits, LeftCurl * weight);
            Curl(_rightHand, _rightDigits, RightCurl * weight);
            if (_look.HasValue && _head != null)
            {
                // The clips keep the face along the body's forward, so turn the head by that much towards the target.
                Quaternion turn = Quaternion.FromToRotation(transform.forward, _look.Value - _head.position);
                _head.rotation = Quaternion.Slerp(Quaternion.identity, turn, weight * 0.7f) * _head.rotation;
            }
            AfterPose?.Invoke();
        }

        private static void Reach(Transform upper, Transform lower, Transform hand, Vector3 target, float weight)
        {
            if (upper == null || lower == null || hand == null) return;
            for (int i = 0; i < 3; i++)
            {
                Aim(lower, hand, target, weight);
                Aim(upper, hand, target, weight);
            }
        }

        private static void SwingElbow(Transform upper, Transform lower, Transform hand, Vector3 hint, float weight)
        {
            if (upper == null || lower == null || hand == null) return;
            Vector3 axis = hand.position - upper.position;
            if (axis.sqrMagnitude < 0.0001f) return;
            Vector3 elbow = Vector3.ProjectOnPlane(lower.position - upper.position, axis);
            Vector3 wanted = Vector3.ProjectOnPlane(hint - upper.position, axis);
            if (elbow.sqrMagnitude < 0.000001f || wanted.sqrMagnitude < 0.000001f) return;
            float angle = Vector3.SignedAngle(elbow, wanted, axis);
            upper.rotation = Quaternion.AngleAxis(angle * weight, axis.normalized) * upper.rotation;
        }

        private static void Orient(Transform hand, Vector3 fingers, Vector3 palm, float weight)
        {
            if (hand == null || weight <= 0f) return;
            Vector3 facing = Vector3.ProjectOnPlane(palm, fingers);
            if (facing.sqrMagnitude < 0.0001f || fingers.sqrMagnitude < 0.0001f) return;
            hand.rotation = Quaternion.Slerp(hand.rotation, Quaternion.LookRotation(facing, fingers), weight);
        }

        /// <summary>Bends every finger joint towards the palm, root first so each joint carries the ones beyond it.</summary>
        private static void Curl(Transform hand, Transform[] digits, float degrees)
        {
            if (hand == null || Mathf.Approximately(degrees, 0f)) return;
            for (int i = 0; i < digits.Length; i++)
            {
                var bone = digits[i];
                if (bone == null || bone.childCount == 0) continue;
                Vector3 along = bone.GetChild(0).position - bone.position;
                Vector3 axis = Vector3.Cross(along, hand.rotation * Vector3.forward);
                if (axis.sqrMagnitude < 0.000001f) continue;
                bone.rotation = Quaternion.AngleAxis(i < 3 ? degrees * 0.5f : degrees, axis) * bone.rotation;
            }
        }

        private static void Aim(Transform joint, Transform hand, Vector3 target, float weight)
        {
            Vector3 current = hand.position - joint.position;
            Vector3 desired = target - joint.position;
            if (current.sqrMagnitude < 0.0001f || desired.sqrMagnitude < 0.0001f) return;
            Quaternion aimed = Quaternion.FromToRotation(current, desired) * joint.rotation;
            joint.rotation = Quaternion.Slerp(joint.rotation, aimed, weight);
        }
    }
}
