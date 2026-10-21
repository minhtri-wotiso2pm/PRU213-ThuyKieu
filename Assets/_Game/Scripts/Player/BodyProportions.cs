using UnityEngine;

namespace ThuyKieu.Player
{
    /// <summary>
    /// Body fixes for the Tripo hanfu Kiều, applied on top of the Humanoid animation:
    /// - the sleeves are skinned tightly to the arms and flare at the elbows, which makes her look broad:
    ///   thin the upper arms (hands restored), narrow the SleeveDrape bones and tuck the elbows in;
    /// - the hanfu walk/run roll the hips about 13° and carry both feet to one side: while moving, the average sideways
    ///   tilt of the legs is rolled back out at the hips (the torso keeps its pose, each step keeps its own sway);
    /// - the hanfu run is a small dainty trot that reads as gliding at running speed: while running she leans into it,
    ///   gathers her skirt in her left hand at the thigh (the Kieu_Skirt_Lift_Left shape key lifts that side of the hem)
    ///   and swings the right arm lightly, in step with her feet;
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
        [SerializeField] private float _sleeveSwayDegrees = 10f;
        [Tooltip("Stiffness of the sleeve's swing towards where it hangs. Lower values feel silkier.")]
        [Range(2f, 20f)]
        [SerializeField] private float _sleeveFollowSpeed = 4.5f;
        [Tooltip("How much the swing overshoots and settles: lower is looser silk, 1 stops without swinging.")]
        [Range(0.2f, 1f)]
        [SerializeField] private float _sleeveDamping = 0.42f;
        [Tooltip("How strongly the sleeve hangs plumb instead of following the forearm (1 = like silk).")]
        [Range(0f, 1f)]
        [SerializeField] private float _sleeveGravity = 1f;
        [Tooltip("Tilt away from hanging plumb (plus flare) the sleeve may keep from the forearm's angle.")]
        [Range(0f, 60f)]
        [SerializeField] private float _sleeveFreeAngle = 8f;
        [Tooltip("Rotates the upper arms towards the body after animation. Any tuck makes the hands sink into the skirt while walking.")]
        [Range(-10f, 15f)]
        [SerializeField] private float _elbowTuckDegrees = 0f;
        [Tooltip("How much the model's silk-sleeve blend shapes flex as each arm changes direction.")]
        [Range(0f, 100f)]
        [SerializeField] private float _sleeveFlex = 42f;
        [Tooltip("Speed at which the soft folds catch up with the arm motion.")]
        [Range(1f, 15f)]
        [SerializeField] private float _sleeveFlexFollow = 4f;

        [Header("Legs")]
        [Tooltip("How much of the clips' average sideways leg tilt is rolled back out at the hips while moving.")]
        [Range(0f, 1f)]
        [SerializeField] private float _legAlign = 1f;

        [Header("Running posture")]
        [Tooltip("Forward lean of the spine at full run.")]
        [Range(0f, 25f)]
        [SerializeField] private float _runLeanDegrees = 8f;
        [Tooltip("How far the free (right) arm swings forward and back at full run.")]
        [Range(0f, 50f)]
        [SerializeField] private float _runArmSwingDegrees = 18f;
        [Tooltip("Elbow bend of the free arm while running.")]
        [Range(0f, 100f)]
        [SerializeField] private float _runElbowBendDegrees = 30f;
        [Tooltip("Left hand gathers the skirt at the front of the thigh while running, as in period dramas.")]
        [SerializeField] private bool _holdSkirt = true;
        [Tooltip("Where the left hand holds the skirt, in body space from the hips (right, up, forward).")]
        [SerializeField] private Vector3 _skirtGrip = new Vector3(-0.15f, -0.14f, 0.14f);
        [Tooltip("How far the held side of the hem lifts at full run (Kieu_Skirt_Lift_Left shape key, 0–100).")]
        [Range(0f, 100f)]
        [SerializeField] private float _skirtLift = 85f;

        private Animator _animator;
        private Transform _leftArm, _rightArm, _leftHand, _rightHand, _leftThigh, _rightThigh;
        private Transform _spine, _chest, _head, _hips, _leftForearm, _rightForearm, _leftFoot, _rightFoot;
        private float _run, _move, _legTilt;
        private SkinnedMeshRenderer _skirt;
        private int _skirtLiftShape = -1;
        private const string SkirtLiftShape = "Kieu_Skirt_Lift_Left";
        private const string LeftSleeveShape = "Kieu_Silk_Sleeve_A";
        private const string RightSleeveShape = "Kieu_Silk_Sleeve_B";
        private SkinnedMeshRenderer _leftSleeveMesh, _rightSleeveMesh;
        private int _leftSleeveShape = -1, _rightSleeveShape = -1;
        private float _leftSleeveFlex, _rightSleeveFlex;
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
            AlignLegs();
            RunPosture();
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
            FlexSleeves(moveAmount, phase);
        }

        private void OnDisable()
        {
            _drapeWorldValid = false;
            SetSleeveFlex(_leftSleeveMesh, _leftSleeveShape, 0f);
            SetSleeveFlex(_rightSleeveMesh, _rightSleeveShape, 0f);
        }

        /// <summary>
        /// The rig has one rigid drape bone per arm, while the meshes also contain a soft-fold shape for each sleeve.
        /// Alternating those shapes with the stride keeps the cloth beside the wrists from moving like cardboard.
        /// </summary>
        private void FlexSleeves(float moveAmount, float phase)
        {
            float leftTarget = Mathf.Max(0f, Mathf.Sin(phase)) * _sleeveFlex * moveAmount;
            float rightTarget = Mathf.Max(0f, -Mathf.Sin(phase)) * _sleeveFlex * moveAmount;
            float follow = 1f - Mathf.Exp(-_sleeveFlexFollow * Time.deltaTime);
            _leftSleeveFlex = Mathf.Lerp(_leftSleeveFlex, leftTarget, follow);
            _rightSleeveFlex = Mathf.Lerp(_rightSleeveFlex, rightTarget, follow);
            SetSleeveFlex(_leftSleeveMesh, _leftSleeveShape, _leftSleeveFlex);
            SetSleeveFlex(_rightSleeveMesh, _rightSleeveShape, _rightSleeveFlex);
        }

        private static void SetSleeveFlex(SkinnedMeshRenderer renderer, int shape, float weight)
        {
            if (renderer != null && shape >= 0) renderer.SetBlendShapeWeight(shape, weight);
        }

        /// <summary>
        /// Rolls the hips so the legs hang under them again: the clips keep both feet to one side all through the cycle.
        /// Only the running average is removed, and the spine turns back by the same amount so the torso is untouched.
        /// </summary>
        private void AlignLegs()
        {
            if (_hips == null || _leftFoot == null || _rightFoot == null || _spine == null) return;
            float target = Mathf.InverseLerp(0.05f, 0.3f, _animator.GetFloat(SpeedParameter));
            _move = Mathf.Lerp(_move, target, 1f - Mathf.Exp(-6f * Time.deltaTime));
            Vector3 forward = transform.forward;
            Vector3 legs = Vector3.ProjectOnPlane(_hips.position - (_leftFoot.position + _rightFoot.position) * 0.5f, forward);
            float tilt = Vector3.SignedAngle(Vector3.up, legs, forward);
            _legTilt = Mathf.Lerp(_legTilt, tilt, 1f - Mathf.Exp(-1.5f * Time.deltaTime));
            float roll = -_legTilt * _legAlign * _move;
            if (Mathf.Abs(roll) < 0.01f) return;
            _hips.rotation = Quaternion.AngleAxis(roll, forward) * _hips.rotation;
            _spine.rotation = Quaternion.AngleAxis(-roll, forward) * _spine.rotation;
        }

        /// <summary>
        /// Leans her into the run, gathers the skirt in her left hand and swings the right arm opposite its leg. The swing
        /// is read from the feet (which one is ahead), so it keeps time with whatever stride the clip and step sync produce.
        /// </summary>
        private void RunPosture()
        {
            // Speed blend: walk 0.5, run 1. Ease in so a jog only hints at it.
            float target = Mathf.InverseLerp(0.55f, 0.95f, _animator.GetFloat(SpeedParameter));
            _run = Mathf.Lerp(_run, target, 1f - Mathf.Exp(-6f * Time.deltaTime));
            LiftSkirt(0f);
            if (_run < 0.01f || _spine == null) return;
            Vector3 side = transform.right;
            float lean = _runLeanDegrees * _run;
            _spine.rotation = Quaternion.AngleAxis(lean * 0.6f, side) * _spine.rotation;
            if (_chest != null) _chest.rotation = Quaternion.AngleAxis(lean * 0.4f, side) * _chest.rotation;
            // Eyes stay on the road ahead.
            if (_head != null) _head.rotation = Quaternion.AngleAxis(-lean * 0.7f, side) * _head.rotation;
            if (_leftFoot == null || _rightFoot == null) return;
            // +1 when the left foot is forward, −1 when the right one is.
            float stride = Vector3.Dot(_leftFoot.position - _rightFoot.position, transform.forward);
            float swing = Mathf.Clamp(stride / 0.3f, -1f, 1f) * _runArmSwingDegrees * _run;
            // Positive about the side axis swings a hanging arm back: the left arm goes back as the left foot comes forward.
            Swing(_rightArm, _rightForearm, -swing, side);
            if (!_holdSkirt) { Swing(_leftArm, _leftForearm, swing, side); return; }
            if (_leftArm == null || _leftForearm == null || _leftHand == null || _hips == null) return;
            // The fist rides up a little with each step, as if lifting the hem clear of the feet.
            Vector3 grip = _hips.position + transform.rotation * (_skirtGrip + Vector3.up * (Mathf.Abs(swing) / Mathf.Max(1f, _runArmSwingDegrees) * 0.03f));
            Vector3 elbowOut = _leftArm.position - side * 0.3f - transform.forward * 0.15f - Vector3.up * 0.2f;
            LiftSkirt(_run * (0.85f + 0.15f * Mathf.Abs(swing) / Mathf.Max(1f, _runArmSwingDegrees)));
            Reach(_leftArm, _leftForearm, _leftHand, Vector3.Lerp(_leftHand.position, grip, _run), elbowOut);
        }

        /// <summary>Sets the held-hem shape key (0..1 of the configured lift); models without the key are left alone.</summary>
        private void LiftSkirt(float amount)
        {
            if (_skirt == null || _skirtLiftShape < 0) return;
            _skirt.SetBlendShapeWeight(_skirtLiftShape, _holdSkirt ? _skirtLift * Mathf.Clamp01(amount) : 0f);
        }

        /// <summary>Two-bone reach: bends the elbow (towards <paramref name="hint"/>) so the hand lands on the target.</summary>
        private static void Reach(Transform upper, Transform lower, Transform hand, Vector3 target, Vector3 hint)
        {
            Vector3 a = upper.position, b = lower.position, c = hand.position;
            float upperLength = (b - a).magnitude, lowerLength = (c - b).magnitude;
            float reach = Mathf.Clamp((target - a).magnitude, 0.01f, upperLength + lowerLength - 0.001f);
            float Angle(Vector3 x, Vector3 y) => Mathf.Acos(Mathf.Clamp(Vector3.Dot(x.normalized, y.normalized), -1f, 1f));
            float shoulderNow = Angle(c - a, b - a), elbowNow = Angle(a - b, c - b), aimNow = Angle(c - a, target - a);
            float shoulderWant = Mathf.Acos(Mathf.Clamp((lowerLength * lowerLength - upperLength * upperLength - reach * reach) / (-2f * upperLength * reach), -1f, 1f));
            float elbowWant = Mathf.Acos(Mathf.Clamp((reach * reach - upperLength * upperLength - lowerLength * lowerLength) / (-2f * upperLength * lowerLength), -1f, 1f));
            Vector3 bendAxis = Vector3.Cross(c - a, hint - a).normalized;
            Vector3 aimAxis = Vector3.Cross(c - a, target - a);
            lower.rotation = Quaternion.AngleAxis((elbowWant - elbowNow) * Mathf.Rad2Deg, bendAxis) * lower.rotation;
            upper.rotation = Quaternion.AngleAxis((shoulderWant - shoulderNow) * Mathf.Rad2Deg, bendAxis) * upper.rotation;
            if (aimAxis.sqrMagnitude > 1e-8f) upper.rotation = Quaternion.AngleAxis(aimNow * Mathf.Rad2Deg, aimAxis.normalized) * upper.rotation;
        }

        private void Swing(Transform upper, Transform forearm, float degrees, Vector3 side)
        {
            if (upper == null) return;
            upper.rotation = Quaternion.AngleAxis(degrees, side) * upper.rotation;
            if (forearm != null) forearm.rotation = Quaternion.AngleAxis(-_runElbowBendDegrees * _run, side) * forearm.rotation;
        }

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
            _spine = _animator.GetBoneTransform(HumanBodyBones.Spine);
            _hips = _animator.GetBoneTransform(HumanBodyBones.Hips);
            _skirt = null;
            _skirtLiftShape = -1;
            _leftSleeveMesh = _rightSleeveMesh = null;
            _leftSleeveShape = _rightSleeveShape = -1;
            foreach (var renderer in GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (renderer.sharedMesh == null) continue;
                int index = renderer.sharedMesh.GetBlendShapeIndex(SkirtLiftShape);
                if (index >= 0) { _skirt = renderer; _skirtLiftShape = index; }
                index = renderer.sharedMesh.GetBlendShapeIndex(LeftSleeveShape);
                if (index >= 0) { _leftSleeveMesh = renderer; _leftSleeveShape = index; }
                index = renderer.sharedMesh.GetBlendShapeIndex(RightSleeveShape);
                if (index >= 0) { _rightSleeveMesh = renderer; _rightSleeveShape = index; }
            }
            _chest = _animator.GetBoneTransform(HumanBodyBones.Chest);
            _head = _animator.GetBoneTransform(HumanBodyBones.Head);
            _leftForearm = _animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            _rightForearm = _animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            _leftFoot = _animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            _rightFoot = _animator.GetBoneTransform(HumanBodyBones.RightFoot);
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
