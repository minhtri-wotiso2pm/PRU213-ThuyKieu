using System;
using ThuyKieu.Environment;
using ThuyKieu.Player;
using ThuyKieu.Presentation;
using UnityEngine;

namespace ThuyKieu.Gameplay.Stealth
{
    /// <summary>
    /// Kiều's steps in the first-person chapters: one sound per stride, chosen by the floor under her (a rug or mat laid on
    /// it wins over the boards), as loud as the noise it sends to the guards: crouch 1.5 m, walk 4 m, run 9 m, times the
    /// floor's loudness. The player hears exactly how far each step carries.
    /// A stride is counted from how far she really moved on the ground this frame, so standing still, pushing against a
    /// wall, falling, a teleport (checkpoint), locked controls, a hiding place and a catch scene make no steps.
    /// </summary>
    [DefaultExecutionOrder(100)] // after PlayerMovement has moved her this frame
    [RequireComponent(typeof(PlayerMovement))]
    public class FirstPersonFootsteps : MonoBehaviour
    {
        public enum Gait { Crouch, Walk, Run }

        /// <summary>One step, as heard by the player and the house (QA reads these).</summary>
        public struct StepEvent
        {
            public Vector3 Position;
            public Surface Surface;
            public bool Tagged;
            public Gait Gait;
            public float Radius;
            public string Sound;
            public float Volume;
        }

        [SerializeField] private StoryAudio _audio;
        [SerializeField] private float _crouchRadius = 1.5f;
        [SerializeField] private float _walkRadius = 4f;
        [SerializeField] private float _runRadius = 9f;
        [Tooltip("Metres between footfalls: short and slow when crouched, long and quick when running.")]
        [SerializeField] private float _crouchStride = 0.55f, _walkStride = 0.8f, _runStride = 1.35f;
        [SerializeField] private LayerMask _floor = 1 << 8;

        private PlayerMovement _player;
        private CharacterController _controller;
        private Vector3 _last;
        private float _travelled;
        private int _variant;

        /// <summary>Extra metres a running step carries (a purse of silver jingling).</summary>
        public float RunExtra { get; set; }
        public Surface Underfoot { get; private set; } = Surface.Wood;
        public event Action<StepEvent> Stepped;

        private void Awake()
        {
            _player = GetComponent<PlayerMovement>();
            _controller = GetComponent<CharacterController>();
            _last = transform.position;
        }

        private void OnEnable() { _last = transform.position; _travelled = 0f; }

        private void Update()
        {
            Vector3 now = transform.position;
            Vector3 moved = now - _last;
            _last = now;
            moved.y = 0f;
            float distance = moved.magnitude;
            // Faster than anyone runs (8 m/s), and more than a stride: she was placed there (checkpoint, story), not walking.
            bool teleported = distance > Mathf.Max(1f, 8f * Time.deltaTime);
            bool grounded = _controller == null || _controller.isGrounded;
            if (teleported || !grounded || _player.ControlLocked || _player.IsHidden || CatchScene.Playing || Time.deltaTime <= 0f || distance < 0.0005f)
            {
                // A pause between steps is not forgotten entirely: the next footfall comes half a stride after she sets off.
                _travelled = Mathf.Min(_travelled, Stride() * 0.5f);
                if (teleported || !grounded) _travelled = 0f;
                return;
            }
            _travelled += distance;
            if (_travelled < Stride()) return;
            _travelled = 0f;
            Step();
        }

        private Gait Current => _player.IsRunning ? Gait.Run : _player.IsCrouching ? Gait.Crouch : Gait.Walk;

        private float Stride()
        {
            switch (Current)
            {
                case Gait.Run: return _runStride;
                case Gait.Crouch: return _crouchStride;
                default: return _walkStride;
            }
        }

        /// <summary>How far a step carries on this floor in this gait, before any rain or music covers it.</summary>
        public float RadiusFor(Gait gait, Surface surface) => GaitRadius(gait) * SurfaceTag.Loudness(surface);

        private float GaitRadius(Gait gait) => gait == Gait.Run ? _runRadius + RunExtra : gait == Gait.Crouch ? _crouchRadius : _walkRadius;

        /// <summary>The floor under her feet: a rug or mat first, then the tagged floor she stands on (untagged = wood).</summary>
        public Surface FloorAt(Vector3 feet, out bool tagged) => FloorAt(feet, out tagged, out _);

        /// <summary>As above, with how far a step there carries (the tag's <see cref="SurfaceTag.Carry"/>).</summary>
        public Surface FloorAt(Vector3 feet, out bool tagged, out float carry)
        {
            tagged = true;
            if (SurfaceTag.PatchAt(feet, out var patch, out carry)) return patch;
            if (Physics.Raycast(feet + Vector3.up * 0.3f, Vector3.down, out RaycastHit hit, 0.8f, _floor, QueryTriggerInteraction.Ignore))
            {
                var tag = hit.collider.GetComponentInParent<SurfaceTag>();
                if (tag != null) { carry = tag.Carry; return tag.Surface; }
            }
            tagged = false;
            carry = 1f;
            return Surface.Wood;
        }

        private void Step()
        {
            var gait = Current;
            Underfoot = FloorAt(transform.position, out bool tagged, out float carry);
            float radius = GaitRadius(gait) * carry;
            bool silver = RunExtra > 0f && gait == Gait.Run;
            Noise.Emit(transform.position, radius, silver ? NoiseKind.Silver : NoiseKind.Footstep);
            // Heard as loud as this floor carries (the same as the noise unless the scene keeps its watchers' hearing apart,
            // see SurfaceTag.Carry); a little life in each step (pitch ±3 %, level ±5 %) without hiding the risk.
            float sounded = RadiusFor(gait, Underfoot);
            float volume = Mathf.Lerp(0.08f, 0.5f, Mathf.InverseLerp(_crouchRadius, _runRadius * 1.5f, sounded)) * UnityEngine.Random.Range(0.95f, 1.05f);
            // Recorded takes for this floor and gait when the scene has them (lighter takes crouched, heavier running);
            // those keep their own pitch (±2 %). Without them, the shared sound with a pitch per gait.
            string floorId = SurfaceTag.StepSound(Underfoot);
            string gaitId = floorId + "." + gait.ToString().ToLowerInvariant();
            bool recorded = _audio != null && (_audio.HasFoley(gaitId) || _audio.HasFoley(floorId));
            string sound = !recorded ? SurfaceTag.StepSound(Underfoot, _variant++) : _audio.HasFoley(gaitId) ? gaitId : floorId;
            float pitch = recorded ? UnityEngine.Random.Range(0.98f, 1.02f)
                : (gait == Gait.Run ? 0.94f : gait == Gait.Crouch ? 1.04f : 1f) * UnityEngine.Random.Range(0.97f, 1.03f);
            if (_audio != null)
            {
                // Her own feet, just below the camera: centred and close (×1.3 makes up for the 1.5 m to the ear).
                _audio.PlayAt(sound, transform.position + Vector3.up * 0.1f, volume * 1.3f, 6f, pitch);
                if (silver) _audio.PlaySfx("coins", 0.25f);
            }
            Stepped?.Invoke(new StepEvent { Position = transform.position, Surface = Underfoot, Tagged = tagged, Gait = gait, Radius = radius, Sound = sound, Volume = volume });
        }
    }
}
