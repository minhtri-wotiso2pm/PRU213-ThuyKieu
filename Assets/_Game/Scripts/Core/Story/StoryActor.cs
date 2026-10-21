using UnityEngine;
using UnityEngine.AI;

namespace ThuyKieu.Core.Story
{
    /// <summary>
    /// A character the story can address by its Ink speaker id: talking animation, facing,
    /// NavMesh walking and, for models without a rig yet, a procedural walk/breath motion.
    /// </summary>
    public class StoryActor : MonoBehaviour
    {
        [SerializeField] private string _speakerId;
        [SerializeField] private Animator _animator;
        [Tooltip("Visual child moved by the procedural motion when the model has no rig.")]
        [SerializeField] private Transform _staticVisual;
        [SerializeField] private float _headHeight = 1.55f;
        [SerializeField] private float _turnSpeed = 240f;
        [Tooltip("Keep a staged actor facing its authored direction (useful for seated classroom extras).")]
        [SerializeField] private bool _lockFacing;
        [Tooltip("Raises the rigged body while seated, for sit clips authored for a lower seat than the bench it sits on.")]
        [SerializeField] private float _seatLift;

        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Talking = Animator.StringToHash("DialogueTalking");
        private static readonly int Emotion = Animator.StringToHash("DialogueEmotion");
        private NavMeshAgent _agent;
        private Vector3? _faceTarget;
        // Facing held before dialogue lines turned the actor; cleared when the actor is placed or walks somewhere new.
        private Quaternion? _restBeforeLines;
        private float? _scriptedSpeed;
        private bool _hasEmotion, _hasTalking, _hasSpeed;
        private bool _talkingRequested;
        private int _talkingEmotion;
        // Controllers with an arm-masked "RaiseHand" layer: its weight fades in and out on RaiseHand().
        private int _raiseHandLayer = -1;
        private float _raiseHandTarget;
        private Vector3 _visualBase;
        private Quaternion _visualRotation;
        private float _phase;
        private bool _lying;
        private float _lyingHeight;
        private float _currentLift;

        public string SpeakerId => _speakerId;
        /// <summary>Recasts a copy of an actor as another speaker (runtime placeholders reuse an existing rig).</summary>
        public void Recast(string speakerId) => _speakerId = speakerId;
        /// <summary>
        /// Eye level of the visible body: the rig's head bone when there is one (so seated, kneeling or lying actors are
        /// framed where their face really is), otherwise the standing head height above the root.
        /// </summary>
        public Vector3 HeadPosition
        {
            get
            {
                if (_animator != null && _animator.isActiveAndEnabled && _animator.isHuman)
                {
                    var head = _animator.GetBoneTransform(HumanBodyBones.Head);
                    if (head != null) return head.position + Vector3.up * 0.07f;
                }
                return transform.position + Vector3.up * _headHeight;
            }
        }

        /// <summary>Height of the visible model (its renderers' bounds), as the camera sees it.</summary>
        public float VisualHeight
        {
            get
            {
                var model = _animator != null ? _animator.transform : transform;
                var renderers = model.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length == 0) return 0f;
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
                return bounds.size.y;
            }
        }

        /// <summary>Scales the model so it stands <paramref name="height"/> metres tall; characters imported at mismatched sizes then stand together believably.</summary>
        public void FitVisualHeight(float height)
        {
            float current = VisualHeight;
            if (_animator == null || current < 0.1f) return;
            _animator.transform.localScale *= height / current;
        }

        /// <summary>Feet-to-crown capsule axis used by cameras to keep bystanders out of a shot.</summary>
        public float StandingHeight => _headHeight + 0.15f;
        public NavMeshAgent Agent => _agent;
        /// <summary>False for a static posed mesh (e.g. a seated classmate), which must not be walked around.</summary>
        public bool HasRig => _animator != null;
        public bool IsMoving => _agent != null && _agent.enabled && _agent.isOnNavMesh && !_agent.isStopped
                                && (_agent.pathPending || _agent.remainingDistance > _agent.stoppingDistance + 0.05f);

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            SetAnimator(_animator != null ? _animator : GetComponentInChildren<Animator>());
            if (_staticVisual != null) { _visualBase = _staticVisual.localPosition; _visualRotation = _staticVisual.localRotation; }
            _phase = Random.value * 10f;
        }

        /// <summary>Switches to another body's animator (see PlayerLook).</summary>
        public void SetAnimator(Animator animator)
        {
            ApplySeatLift(0f);
            _animator = animator;
            _hasEmotion = _hasTalking = _hasSpeed = false;
            _raiseHandLayer = -1;
            if (_animator == null || _animator.runtimeAnimatorController == null) return;
            _raiseHandLayer = _animator.GetLayerIndex("RaiseHand");
            foreach (var parameter in _animator.parameters)
            {
                if (parameter.nameHash == Emotion) _hasEmotion = true;
                if (parameter.nameHash == Talking) _hasTalking = true;
                if (parameter.nameHash == Speed) _hasSpeed = true;
            }
        }

        public void SetTalking(bool talking, int emotion)
        {
            _talkingRequested = talking;
            _talkingEmotion = talking ? emotion : 0;
            if (_animator == null || !_animator.isActiveAndEnabled) return;
            if (_hasTalking) _animator.SetBool(Talking, talking && !IsMoving);
            if (_hasEmotion) _animator.SetInteger(Emotion, _talkingEmotion);
        }

        /// <summary>Smoothly turns to look at a world point (only while not walking).</summary>
        public void FaceTowards(Vector3? point)
        {
            // Seated people do not swivel towards each speaker: turning about the root slides them off the bench.
            if ((_lockFacing || IsSitting) && point.HasValue) return;
            _faceTarget = point;
        }

        /// <summary>Turns to a speaker for a dialogue line; <see cref="EndLineTurn"/> turns back to how the actor stood before.</summary>
        public void TurnForLine(Vector3 point)
        {
            if (_lockFacing || IsSitting) return;
            if (!_restBeforeLines.HasValue) _restBeforeLines = transform.rotation;
            _faceTarget = point;
        }

        /// <summary>The conversation is over: face the way the actor stood before the first line turned them.</summary>
        public void EndLineTurn()
        {
            _faceTarget = _restBeforeLines.HasValue ? transform.position + _restBeforeLines.Value * Vector3.forward : (Vector3?)null;
            _restBeforeLines = null;
        }

        /// <summary>Walking speed (m/s) for the animator while a sequence moves the actor by hand; null hands it back to the agent.</summary>
        public void SetScriptedSpeed(float? metresPerSecond) => _scriptedSpeed = metresPerSecond;

        /// <summary>
        /// Ends a one-shot dialogue gesture immediately before an authored movement sequence takes ownership.
        /// Without this, a fall animation can keep the legs frozen while the sequence moves the actor's root.
        /// </summary>
        public void ResetToLocomotion(float blend = 0.12f)
        {
            _talkingRequested = false;
            _talkingEmotion = 0;
            if (_animator == null || !_animator.isActiveAndEnabled) return;
            if (_hasTalking) _animator.SetBool(Talking, false);
            if (_hasEmotion) _animator.SetInteger(Emotion, 0);
            _animator.CrossFade("Idle", blend, 0, 0f);
        }

        public void SetFacingLocked(bool locked)
        {
            _lockFacing = locked;
            if (locked) _faceTarget = null;
        }

        public void FaceInstant(Vector3 point)
        {
            if (_lockFacing || IsSitting) return;
            Vector3 direction = point - transform.position;
            direction.y = 0;
            if (direction.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(direction);
            _faceTarget = null;
        }

        public bool MoveTo(Vector3 destination, float speed = 1.6f)
        {
            if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh) return false;
            _faceTarget = null;
            _restBeforeLines = null;
            _agent.speed = speed;
            _agent.isStopped = false;
            return _agent.SetDestination(destination);
        }

        public void Stop()
        {
            if (_agent != null && _agent.enabled && _agent.isOnNavMesh) { _agent.isStopped = true; _agent.ResetPath(); }
        }

        /// <summary>Instant placement that keeps the NavMeshAgent consistent.</summary>
        public void Place(Vector3 position, Quaternion rotation)
        {
            if (_agent != null && _agent.enabled && _agent.isOnNavMesh) { _agent.ResetPath(); _agent.Warp(position); }
            else transform.position = position;
            transform.rotation = rotation;
            _faceTarget = null;
            _restBeforeLines = null;
        }

        /// <summary>
        /// Lays an unrigged model down on its back (asleep on a bed): navigation off, root placed at the feet with the
        /// head towards the root's forward, the visual tipped over and lifted onto the bed surface.
        /// </summary>
        public void LieDown(Vector3 feet, Quaternion facing, float bedHeight)
        {
            if (_agent != null) _agent.enabled = false;
            transform.SetPositionAndRotation(feet, facing * Quaternion.Euler(0f, 180f, 0f));
            _faceTarget = null;
            _lyingHeight = bedHeight;
            _lying = _staticVisual != null;
            if (_lying || _animator == null) return;
            // Rigged model: tip the whole model onto its back once; its idle slowed down reads as sleeping breath.
            var model = _animator.transform;
            model.localPosition += Vector3.up * bedHeight;
            model.localRotation = Quaternion.Euler(-90f, 0f, 0f) * model.localRotation;
            _animator.speed = 0.35f;
            SetTalking(false, 0);
        }

        /// <summary>Puts a hand up (answering in class) or down; does nothing on bodies without a RaiseHand layer.</summary>
        public void RaiseHand(bool raised) => _raiseHandTarget = raised ? 1f : 0f;

        /// <summary>Sits down on the spot (a chair must stand behind the actor), or stands up again.</summary>
        public void Sit(bool sitting)
        {
            if (_animator == null || !_animator.isActiveAndEnabled) return;
            if (sitting)
            {
                Stop();
                _animator.SetBool(IsSittingId, true);
                _animator.ResetTrigger(StandId);
                _animator.SetTrigger(SitId);
            }
            else if (_animator.GetBool(IsSittingId))
            {
                _animator.ResetTrigger(SitId);
                _animator.SetTrigger(StandId); // the stand-up state clears IsSitting when it finishes
            }
        }

        public bool IsSitting => _animator != null && _animator.isActiveAndEnabled && _animator.GetBool(IsSittingId);

        private static readonly int IsSittingId = Animator.StringToHash("IsSitting");
        private static readonly int SitId = Animator.StringToHash("Sit");
        private static readonly int StandId = Animator.StringToHash("Stand");

        /// <summary>Called once the runtime NavMesh exists.</summary>
        public void EnableNavigation()
        {
            if (_agent == null) return;
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                _agent.enabled = true;
                _agent.Warp(hit.position);
            }
        }

        /// <summary>Leaves a staged actor under sequence control instead of letting NavMesh snap it onto furniture.</summary>
        public void DisableNavigation()
        {
            if (_agent == null) return;
            if (_agent.enabled && _agent.isOnNavMesh) _agent.ResetPath();
            _agent.enabled = false;
        }

        private void Update()
        {
            float speed = _scriptedSpeed ?? (_agent != null && _agent.enabled ? _agent.velocity.magnitude : 0f);
            // A speaker may deliver its line while entering the shot. Locomotion must win during that movement;
            // otherwise the Talking state freezes the legs while the root keeps sliding across the ground.
            if (_hasTalking && _animator != null && _animator.isActiveAndEnabled)
                _animator.SetBool(Talking, _talkingRequested && speed < 0.1f);
            // PlayerMovement owns the Speed parameter on the playable actor. Writing zero
            // here as well made the classroom student's idle/walk states flicker each frame.
            if (_hasSpeed && (_agent != null || _scriptedSpeed.HasValue))
                _animator.SetFloat(Speed, Mathf.Clamp01(speed / 3.2f), 0.12f, Time.deltaTime);
            if (_faceTarget.HasValue && speed < 0.1f && !IsSitting)
            {
                Vector3 direction = _faceTarget.Value - transform.position;
                direction.y = 0;
                if (direction.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), _turnSpeed * Time.deltaTime);
            }
            if (_raiseHandLayer >= 0 && _animator.isActiveAndEnabled)
                _animator.SetLayerWeight(_raiseHandLayer, Mathf.MoveTowards(_animator.GetLayerWeight(_raiseHandLayer), _raiseHandTarget, Time.deltaTime * 3f));
            if (_seatLift != 0f) ApplySeatLift(Mathf.MoveTowards(_currentLift, IsSitting ? _seatLift : 0f, Time.deltaTime * 0.6f));
            AnimateStaticVisual(speed);
        }

        private void ApplySeatLift(float lift)
        {
            if (_animator == null || Mathf.Approximately(lift, _currentLift)) return;
            Vector3 offset = Vector3.up * (lift - _currentLift);
            if (_animator.TryGetComponent(out AnimatorRootAnchor anchor)) anchor.Shift(offset);
            else _animator.transform.localPosition += offset;
            _currentLift = lift;
        }

        // Placeholder life for unrigged models: a stepping bob and sway while walking, breathing when idle.
        private void AnimateStaticVisual(float speed)
        {
            if (_staticVisual == null) return;
            if (_lying)
            {
                // Slow sleeping breath: the chest rises a few millimetres.
                _phase += Time.deltaTime * 1.1f;
                _staticVisual.localPosition = _visualBase + Vector3.up * (_lyingHeight + Mathf.Max(0f, Mathf.Sin(_phase)) * 0.012f);
                _staticVisual.localRotation = Quaternion.Euler(-90f, 0f, 0f) * _visualRotation;
                return;
            }
            _phase += Time.deltaTime * (speed > 0.1f ? 7.5f * Mathf.Clamp(speed / 1.6f, 0.6f, 1.6f) : 1.4f);
            float walk = Mathf.Clamp01(speed / 1.2f);
            float bob = Mathf.Abs(Mathf.Sin(_phase)) * 0.045f * walk + Mathf.Sin(_phase) * 0.006f * (1 - walk);
            float sway = Mathf.Sin(_phase) * 3.2f * walk;
            float lean = 4f * walk;
            _staticVisual.localPosition = _visualBase + Vector3.up * bob;
            _staticVisual.localRotation = _visualRotation * Quaternion.Euler(lean, 0f, sway);
        }
    }
}
