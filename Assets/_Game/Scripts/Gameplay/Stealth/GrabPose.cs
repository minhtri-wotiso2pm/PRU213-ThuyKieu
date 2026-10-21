using UnityEngine;

namespace ThuyKieu.Gameplay.Stealth
{
    /// <summary>
    /// A grab layered on top of whatever a humanoid's animator plays (the project has no grab clip): after the animator
    /// has posed the body each frame, the spine leans towards the one being caught, the head turns to her face, each arm
    /// reaches its target through a two-bone solve with the elbow bent down and out, and the fingers close as the hand
    /// takes hold. <see cref="CatchScene"/> drives the weights and targets; removing the component gives the animation
    /// back untouched.
    /// </summary>
    [DefaultExecutionOrder(1000)]
    public class GrabPose : MonoBehaviour
    {
        public Vector3 LookTarget;
        public Vector3 RightTarget, LeftTarget;
        public float Look, Lean, Right, Left, Grip;
        /// <summary>Degrees of forward bend at full <see cref="Lean"/> (more when she is crouched or low in cover).</summary>
        public float LeanDegrees = 22f;

        private Animator _animator;
        private Transform _spine, _chest, _neck, _head;
        private Transform _rUpper, _rLower, _rHand, _lUpper, _lLower, _lHand;
        private Transform[][] _rFingers, _lFingers;
        private Transform _rThumb, _lThumb, _rMiddle, _lMiddle;

        public bool Ready => _rUpper != null && _rLower != null && _rHand != null;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            if (_animator == null || !_animator.isHuman) { enabled = false; return; }
            Transform Bone(HumanBodyBones bone) => _animator.GetBoneTransform(bone);
            _spine = Bone(HumanBodyBones.Spine);
            _chest = Bone(HumanBodyBones.UpperChest) ?? Bone(HumanBodyBones.Chest);
            _neck = Bone(HumanBodyBones.Neck);
            _head = Bone(HumanBodyBones.Head);
            _rUpper = Bone(HumanBodyBones.RightUpperArm); _rLower = Bone(HumanBodyBones.RightLowerArm); _rHand = Bone(HumanBodyBones.RightHand);
            _lUpper = Bone(HumanBodyBones.LeftUpperArm); _lLower = Bone(HumanBodyBones.LeftLowerArm); _lHand = Bone(HumanBodyBones.LeftHand);
            _rThumb = Bone(HumanBodyBones.RightThumbProximal); _lThumb = Bone(HumanBodyBones.LeftThumbProximal);
            _rMiddle = Bone(HumanBodyBones.RightMiddleProximal); _lMiddle = Bone(HumanBodyBones.LeftMiddleProximal);
            _rFingers = Fingers(true);
            _lFingers = Fingers(false);
        }

        private Transform[][] Fingers(bool right)
        {
            var names = right
                ? new[] { HumanBodyBones.RightIndexProximal, HumanBodyBones.RightIndexIntermediate, HumanBodyBones.RightIndexDistal,
                          HumanBodyBones.RightMiddleProximal, HumanBodyBones.RightMiddleIntermediate, HumanBodyBones.RightMiddleDistal,
                          HumanBodyBones.RightRingProximal, HumanBodyBones.RightRingIntermediate, HumanBodyBones.RightRingDistal,
                          HumanBodyBones.RightLittleProximal, HumanBodyBones.RightLittleIntermediate, HumanBodyBones.RightLittleDistal }
                : new[] { HumanBodyBones.LeftIndexProximal, HumanBodyBones.LeftIndexIntermediate, HumanBodyBones.LeftIndexDistal,
                          HumanBodyBones.LeftMiddleProximal, HumanBodyBones.LeftMiddleIntermediate, HumanBodyBones.LeftMiddleDistal,
                          HumanBodyBones.LeftRingProximal, HumanBodyBones.LeftRingIntermediate, HumanBodyBones.LeftRingDistal,
                          HumanBodyBones.LeftLittleProximal, HumanBodyBones.LeftLittleIntermediate, HumanBodyBones.LeftLittleDistal };
            var fingers = new Transform[4][];
            for (int f = 0; f < 4; f++)
            {
                fingers[f] = new Transform[3];
                for (int j = 0; j < 3; j++) fingers[f][j] = _animator.GetBoneTransform(names[f * 3 + j]);
            }
            return fingers;
        }

        private void LateUpdate()
        {
            Vector3 toTarget = LookTarget - transform.position;
            toTarget.y = 0f;
            // Lean: bend the spine and chest forward (about the body's right axis) towards her.
            if (Lean > 0f)
            {
                var bend = Quaternion.AngleAxis(LeanDegrees * Lean * 0.5f, transform.right);
                if (_spine != null) _spine.rotation = bend * _spine.rotation;
                if (_chest != null) _chest.rotation = bend * _chest.rotation;
            }
            // Arms before the head, so the shoulders the arms hang from already include the lean.
            if (Right > 0f) Reach(_rUpper, _rLower, _rHand, RightTarget, Right, true);
            if (Left > 0f) Reach(_lUpper, _lLower, _lHand, LeftTarget, Left, false);
            if (Grip > 0f)
            {
                Curl(_rFingers, _rHand, _rMiddle, _rThumb, Grip * Mathf.Clamp01(Right * 1.5f), true);
                Curl(_lFingers, _lHand, _lMiddle, _lThumb, Grip * Mathf.Clamp01(Left * 1.5f), false);
            }
            if (Look > 0f && _head != null)
            {
                // Turn the face from where the body faces to her eyes: some of it in the neck, the rest in the head.
                Vector3 want = LookTarget - _head.position;
                var turn = Quaternion.FromToRotation(transform.forward, want.normalized);
                turn = Quaternion.Slerp(Quaternion.identity, turn, Look);
                if (_neck != null) _neck.rotation = Quaternion.Slerp(Quaternion.identity, turn, 0.4f) * _neck.rotation;
                _head.rotation = Quaternion.Slerp(Quaternion.identity, turn, _neck != null ? 0.6f : 1f) * _head.rotation;
            }
        }

        /// <summary>Two-bone reach: the elbow bends down and out to the side the arm belongs to.</summary>
        private void Reach(Transform upper, Transform lower, Transform hand, Vector3 target, float weight, bool right)
        {
            if (upper == null || lower == null || hand == null) return;
            Vector3 a = upper.position, b = lower.position, c = hand.position;
            float la = Vector3.Distance(a, b), lb = Vector3.Distance(b, c);
            Vector3 goal = Vector3.Lerp(c, target, weight);
            Vector3 toGoal = goal - a;
            float d = Mathf.Clamp(toGoal.magnitude, Mathf.Abs(la - lb) + 0.01f, la + lb - 0.005f);
            Vector3 dir = toGoal.normalized;
            Vector3 pole = Vector3.down * 0.6f + transform.right * (right ? 0.5f : -0.5f) - transform.forward * 0.2f;
            Vector3 bendDir = Vector3.ProjectOnPlane(pole, dir);
            if (bendDir.sqrMagnitude < 1e-6f) bendDir = Vector3.ProjectOnPlane(Vector3.down, dir);
            bendDir.Normalize();
            float cosA = Mathf.Clamp((la * la + d * d - lb * lb) / (2f * la * d), -1f, 1f);
            float angle = Mathf.Acos(cosA);
            Vector3 elbow = a + (dir * Mathf.Cos(angle) + bendDir * Mathf.Sin(angle)) * la;
            upper.rotation = Quaternion.FromToRotation(b - a, elbow - a) * upper.rotation;
            b = lower.position;
            c = hand.position;
            lower.rotation = Quaternion.FromToRotation(c - b, a + dir * d - b) * lower.rotation;
            // The palm turns to what it grabs: rolled about the forearm until it faces her chest (knuckles to the camera).
            Transform middle = right ? _rMiddle : _lMiddle, thumb = right ? _rThumb : _lThumb;
            if (middle != null && thumb != null)
            {
                Vector3 forearm = (hand.position - lower.position).normalized;
                Vector3 palm = PalmNormal(hand, middle, thumb, right);
                Vector3 want = Vector3.ProjectOnPlane(transform.forward + Vector3.down * 0.35f, forearm);
                Vector3 have = Vector3.ProjectOnPlane(palm, forearm);
                if (want.sqrMagnitude > 1e-4f && have.sqrMagnitude > 1e-4f)
                    hand.rotation = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(have, want), weight) * hand.rotation;
            }
        }

        /// <summary>The way the palm faces, from the thumb and the middle finger (right hand: along × to-thumb).</summary>
        private static Vector3 PalmNormal(Transform hand, Transform middle, Transform thumb, bool right)
        {
            Vector3 along = (middle.position - hand.position).normalized;
            Vector3 toThumb = (thumb.position - hand.position).normalized;
            return Vector3.Cross(along, toThumb).normalized * (right ? 1f : -1f);
        }

        /// <summary>Closes the fingers into the palm, as on a fistful of cloth.</summary>
        private static void Curl(Transform[][] fingers, Transform hand, Transform middle, Transform thumb, float amount, bool right)
        {
            if (hand == null || middle == null || thumb == null || amount <= 0f) return;
            Vector3 along = (middle.position - hand.position).normalized;
            Vector3 palm = PalmNormal(hand, middle, thumb, right);
            float[] bend = { 55f, 70f, 45f };
            foreach (var finger in fingers)
                for (int j = 0; j < 3; j++)
                {
                    var bone = finger[j];
                    if (bone == null) continue;
                    // Unity turns left-handed: about along × palm a positive angle brings the finger down into the palm.
                    Vector3 axis = Vector3.Cross(along, palm);
                    bone.rotation = Quaternion.AngleAxis(bend[j] * amount, axis) * bone.rotation;
                }
        }
    }
}
