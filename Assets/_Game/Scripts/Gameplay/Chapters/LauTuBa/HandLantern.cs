using ThuyKieu.Gameplay.Stealth;
using UnityEngine;

namespace ThuyKieu.Gameplay.Chapters.LauTuBa
{
    /// <summary>
    /// A watchman's own lantern (StorySceneKit.Lantern, "GuardLantern" under his root) carried in his right hand at night.
    /// The lantern hangs from a hook at its root: that hook follows the grip of the animated hand every frame, the lantern
    /// is shrunk to a hand lantern and kept hanging upright, leaning a little behind the hand's motion. It is not parented
    /// to the bone, whose FBX scale would blow it up again. Its light keeps its range and intensity (what LightExposure
    /// counts), only a softer colour. Without a humanoid right hand to hold it, it is hidden rather than left floating.
    /// The right fingers close a little around the handle, on top of whatever the animator posed; when the animator leaves
    /// the fingers alone (culled, no finger curves) the curl is undone before it is laid again, so it never adds up. His
    /// own catch takes the hand over (<see cref="GrabPose"/>) and gets it back open.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class HandLantern : MonoBehaviour
    {
        private const string LanternName = "GuardLantern";
        /// <summary>The kit lantern at this scale: body 0.21 m across, 0.27 m from base to cap, an 8 cm handle above it.</summary>
        private const float Size = 0.54f;
        /// <summary>The hook sits this far above the grip, so the handle runs through the fist.</summary>
        private const float HandleAbove = 0.03f;
        private static readonly Color Warm = new Color(1f, 0.72f, 0.5f);
        /// <summary>Degrees each finger joint (proximal, intermediate, distal) closes around the handle: a loose fist.</summary>
        private static readonly float[] Bend = { 45f, 60f, 40f };
        private static readonly HumanBodyBones[] FingerBones =
        {
            HumanBodyBones.RightIndexProximal, HumanBodyBones.RightIndexIntermediate, HumanBodyBones.RightIndexDistal,
            HumanBodyBones.RightMiddleProximal, HumanBodyBones.RightMiddleIntermediate, HumanBodyBones.RightMiddleDistal,
            HumanBodyBones.RightRingProximal, HumanBodyBones.RightRingIntermediate, HumanBodyBones.RightRingDistal,
            HumanBodyBones.RightLittleProximal, HumanBodyBones.RightLittleIntermediate, HumanBodyBones.RightLittleDistal
        };

        private Transform _actor, _hand, _knuckle, _thumb;
        private Vector3 _lastGrip, _lag;
        private Transform[] _fingers;
        /// <summary>The pose under the curl, and the curl as written: a finger still at <see cref="_curled"/> was not animated since.</summary>
        private Quaternion[] _open, _curled;
        private bool _gripping;

        /// <summary>Puts each of the watcher's own lanterns in his hand (safe to call again; adds nothing if he has none).</summary>
        public static void Attach(Watchman watcher)
        {
            if (watcher == null) return;
            foreach (Transform child in watcher.transform)
            {
                if (!child.name.StartsWith(LanternName) || child.GetComponentInChildren<Light>(true) == null) continue;
                var held = child.GetComponent<HandLantern>();
                if (held == null) held = child.gameObject.AddComponent<HandLantern>();
                held.Setup(watcher.transform);
            }
        }

        private void Setup(Transform actor)
        {
            Release();
            _actor = actor;
            var animator = actor.GetComponentInChildren<Animator>();
            _hand = animator != null && animator.isHuman ? animator.GetBoneTransform(HumanBodyBones.RightHand) : null;
            if (_hand == null)
            {
                Debug.LogWarning("[HandLantern] " + actor.name + " has no humanoid right hand: his lantern is hidden.", actor);
                enabled = false;
                gameObject.SetActive(false);
                return;
            }
            _knuckle = animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
            _thumb = animator.GetBoneTransform(HumanBodyBones.RightThumbProximal);
            _fingers = new Transform[FingerBones.Length];
            for (int i = 0; i < FingerBones.Length; i++) _fingers[i] = animator.GetBoneTransform(FingerBones[i]);
            _open = new Quaternion[FingerBones.Length];
            _curled = new Quaternion[FingerBones.Length];

            Vector3 parentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
            transform.localScale = new Vector3(Size / parentScale.x, Size / parentScale.y, Size / parentScale.z);
            foreach (Transform part in transform)
            {
                // No knot and tassel dangling below a hand lantern; the hanging cord becomes a handle you can see.
                if (part.name == "Knot" || part.name == "Tassel") part.gameObject.SetActive(false);
                else if (part.name == "Cord") part.localScale = new Vector3(0.05f, part.localScale.y, 0.05f);
            }
            foreach (var light in GetComponentsInChildren<Light>(true)) light.color = Warm;

            _lastGrip = Hook();
            _lag = Vector3.zero;
            Follow();
        }

        /// <summary>Between the wrist and the middle knuckle, where the fist closes; the wrist if the rig has no fingers.</summary>
        private Vector3 Hook()
        {
            Vector3 grip = _knuckle != null ? Vector3.Lerp(_hand.position, _knuckle.position, 0.8f) : _hand.position;
            return grip + Vector3.up * HandleAbove;
        }

        private void LateUpdate()
        {
            if (_hand == null) return;
            // A catch scene moves the lantern to his hip while his hands are on her; it puts it back when it ends.
            if (CatchScene.Playing)
            {
                // Only the catcher's hand is taken over; GrabPose runs later this frame and closes it from the open pose.
                var pose = _actor.GetComponentInChildren<GrabPose>();
                if (pose != null && pose.enabled) Release();
                else Grip();
                return;
            }
            Grip();
            Follow();
        }

        private void OnDisable() => Release();

        /// <summary>Lays the curl on this frame's finger pose: the animator's if it wrote one, otherwise the one under last frame's curl.</summary>
        private void Grip()
        {
            if (_fingers == null || _knuckle == null || _thumb == null) return;
            for (int i = 0; i < _fingers.Length; i++)
            {
                var bone = _fingers[i];
                if (bone == null) continue;
                if (_gripping && bone.localRotation == _curled[i]) bone.localRotation = _open[i];
                else _open[i] = bone.localRotation;
            }
            // Bend axis from the hand itself, as GrabPose does, not from the imported bones' axes: along × palm closes
            // a finger into the palm (right hand, palm = along × to-thumb).
            Vector3 along = (_knuckle.position - _hand.position).normalized;
            Vector3 palm = Vector3.Cross(along, (_thumb.position - _hand.position).normalized).normalized;
            Vector3 axis = Vector3.Cross(along, palm);
            for (int i = 0; i < _fingers.Length; i++)
            {
                var bone = _fingers[i];
                if (bone != null) bone.rotation = Quaternion.AngleAxis(Bend[i % 3], axis) * bone.rotation;
            }
            for (int i = 0; i < _fingers.Length; i++)
                if (_fingers[i] != null) _curled[i] = _fingers[i].localRotation;
            _gripping = true;
        }

        /// <summary>Opens the hand back to the pose under the curl, unless something has posed the fingers since.</summary>
        private void Release()
        {
            if (!_gripping) return;
            _gripping = false;
            for (int i = 0; i < _fingers.Length; i++)
            {
                var bone = _fingers[i];
                if (bone != null && bone.localRotation == _curled[i]) bone.localRotation = _open[i];
            }
        }

        private void Follow()
        {
            Vector3 hook = Hook();
            float dt = Time.deltaTime;
            if (dt > 0f)
            {
                Vector3 move = (hook - _lastGrip) / dt;
                move.y = 0f;
                if (move.magnitude > 8f) move = Vector3.zero; // placed somewhere else (a checkpoint), not a swing
                _lag = Vector3.Lerp(_lag, move, 1f - Mathf.Exp(-6f * dt));
            }
            _lastGrip = hook;
            Quaternion upright = Quaternion.Euler(0f, _actor.eulerAngles.y, 0f);
            // The body trails the hand: its bottom swings back against the motion, a few degrees at a walk.
            Quaternion lean = _lag.sqrMagnitude > 1e-4f
                ? Quaternion.AngleAxis(Mathf.Min(_lag.magnitude * 8f, 14f), Vector3.Cross(Vector3.up, _lag).normalized)
                : Quaternion.identity;
            transform.SetPositionAndRotation(hook, lean * upright);
        }
    }
}
