using System;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Stealth;
using ThuyKieu.Presentation;
using ThuyKieu.UI;
using UnityEngine;

namespace ThuyKieu.Gameplay.Chapters.LauTuBa
{
    /// <summary>
    /// Tú Bà dozing on her sập in Chapter 4, and the only clock the player needs: her snoring.
    /// <list type="bullet">
    /// <item><b>Asleep</b>: snores on a steady breath (inhale … exhale). Sounds she hears add to a hidden disturbance; a loud
    /// sound close by rouses her at once.</item>
    /// <item><b>Stirring</b>: too much disturbance (or the guard rattling a door) stops the snore for a few seconds. She
    /// mumbles and settles again, unless she hears something more meanwhile: then she is roused.</item>
    /// <item><b>Roused</b>: lying awake, listening, no snore. She knows only where the sound came from, not who made it: the
    /// chapter has her call the guard to that place. Each sound she hears keeps her awake longer; after a quiet spell she
    /// mutters and sleeps again. Kiều is caught only at her bedside (see <see cref="HandCaught"/> and the chapter).</item>
    /// <item><b>Awake</b>: for good, from the moment she finds the keys gone (J2).</item>
    /// </list>
    /// None of it runs until the chapter has put her to bed (<see cref="Restore"/> with Asleep while she lies on the sập):
    /// at Kiều's door and on her way back she is up, not asleep. Silence is the warning. Checkpoints restore her asleep (or
    /// awake after J2), never halfway.
    /// <para>The breath is one clock, ticked before anything that reads it in a frame (the hand at her belt, the prompt),
    /// and its sound plays on her own source, restarted at each phase and stopped the moment the breathing stops.</para>
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class TuBaSleeper : MonoBehaviour
    {
        // New values at the end: the order is part of saved state.
        public enum Sleep { Asleep, Stirring, Awake, Roused }

        /// <summary>Why a hand at her belt was caught.</summary>
        public enum Grab { BreathingIn, Restless }

        [SerializeField] private StoryActor _actor;
        [SerializeField] private StoryAudio _audio;
        [SerializeField] private StoryHUD _hud;
        [SerializeField] private float _hearing = 4.5f;
        [SerializeField] private float _inhale = 1.7f;
        [SerializeField] private float _exhale = 2.6f;
        [Tooltip("Seconds of quiet before a roused Tú Bà falls asleep again.")]
        [SerializeField] private float _rousedSeconds = 14f;
        [SerializeField] private LayerMask _walls = 1 << 8;
        [Tooltip("How far her breathing carries (her room and the doorway, not the whole house).")]
        [SerializeField] private float _breathReach = 11f;
        [Tooltip("Degrees her chest rises on an in-breath (on the same clock as the snore), so the breath can be read close up.")]
        [SerializeField] private float _chestHeave = 1.6f;

        private Transform _chest;
        private float _phase, _length = 1.7f, _disturbance, _stir, _roused;
        private bool _inhaling = true, _inBed, _phaseStarted;
        private AudioSource _breath;
        private AudioClip _clip;
        private float _clipGain = 1f, _soundLength;
        private bool _sounding;

        public Sleep State { get; private set; } = Sleep.Asleep;
        /// <summary>Put to bed by the chapter and lying on the sập (not at Kiều's door, not on her way, not up after J2).</summary>
        public bool InBed => _inBed && (_actor == null || _actor.IsLying);
        /// <summary>The breath clock is running: asleep in bed, and no catch scene holding the world still.</summary>
        public bool Breathing => State == Sleep.Asleep && InBed && !CatchScene.Playing;
        /// <summary>The safe moment to move a hand near her: she is breathing out.</summary>
        public bool Exhaling => Breathing && !_inhaling;
        /// <summary>She is drawing breath (the snore): a hand at her belt now is felt.</summary>
        public bool Inhaling => Breathing && _inhaling;
        /// <summary>Seconds left in the current breath phase (0 when not breathing).</summary>
        public float PhaseLeft => Breathing ? Mathf.Max(0f, _length - _phase) : 0f;
        /// <summary>Length of the breath phase under way (an in-breath lasts as long as its recorded snore).</summary>
        public float PhaseLength => _length;
        public float ExhaleSeconds => _exhale;
        /// <summary>Of this frame's time, how much she spent breathing in / out (a slow frame can span a phase change).</summary>
        public float InhaledThisFrame { get; private set; }
        public float ExhaledThisFrame { get; private set; }
        /// <summary>
        /// The breath sound is under way on her source (started and not stopped, and its take not yet over). Kept as state
        /// rather than read back from the AudioSource, so QA can check it where no audio device plays (batch runs).
        /// </summary>
        public bool Snoring => _sounding && _phase < _soundLength;
        /// <summary>Which clip the breath source last started ("snore_deep" in, "snore_out" out).</summary>
        public string LastBreathId { get; private set; }
        public float Disturbance => _disturbance;
        /// <summary>Lying awake and listening (stirring or roused): a hand at her belt now is felt.</summary>
        public bool Restless => State == Sleep.Stirring || State == Sleep.Roused;
        /// <summary>Where the last sound that reached her came from.</summary>
        public Vector3 LastHeard { get; private set; }
        public event Action<TuBaSleeper> Woke;
        public event Action<TuBaSleeper> Stirred;
        /// <summary>Roused by a sound from this place (she did not see who made it).</summary>
        public event Action<TuBaSleeper, Vector3> Roused;
        /// <summary>Lying awake, she heard another sound from this place.</summary>
        public event Action<TuBaSleeper, Vector3> Heard;
        /// <summary>A quiet spell: she sleeps again.</summary>
        public event Action<TuBaSleeper> Settled;
        /// <summary>She felt a hand at her belt: while breathing in, or while not asleep.</summary>
        public event Action<TuBaSleeper, Grab> HandCaught;
        public Vector3 Head => _actor != null ? _actor.HeadPosition : transform.position;

        private void OnEnable() { Noise.Emitted += OnNoise; CatchScene.Started += OnCatch; }
        private void OnDisable() { Noise.Emitted -= OnNoise; CatchScene.Started -= OnCatch; StopBreath(); }

        /// <summary>A catch scene starts (anyone's): the breath stops in that same frame and its clock waits for the scene.</summary>
        private void OnCatch()
        {
            StopBreath();
            _phaseStarted = false;
        }

        private void OnNoise(Noise.Event noise)
        {
            if (State == Sleep.Awake) return;
            // Only while she lies on her sập (on her feet at Kiều's door she is not sleeping).
            if (!InBed) return;
            Vector3 ear = Head;
            float distance = Vector3.Distance(noise.Position, ear);
            if (distance > Mathf.Min(noise.Radius, _hearing + noise.Radius * 0.5f)) return;
            if (Blocked(ear, noise.Position + Vector3.up * 0.4f) && distance > noise.Radius * Noise.Muffled) return;
            LastHeard = noise.Position;
            switch (State)
            {
                case Sleep.Roused:
                    _roused = _rousedSeconds;
                    Heard?.Invoke(this, noise.Position);
                    return;
                case Sleep.Stirring:
                    Rouse(noise.Position);
                    return;
            }
            // A door flung open or slammed, or running, within her hearing brings her straight up; otherwise sounds add up:
            // a crouched step beside her adds a fifth, a walking step nearly half.
            if (noise.Radius >= 6f && distance <= _hearing + 1f) { Rouse(noise.Position); return; }
            Disturb(Mathf.Clamp01(noise.Radius / 6f) * 0.6f + 0.05f);
        }

        /// <summary>
        /// A wall or a shut door between her ear and the sound. Anything within arm's reach of her head is the sập she lies
        /// on (solid, on the walls layer), and anything right at the source is what made the sound (a door in its frame):
        /// neither is a wall between them.
        /// </summary>
        private bool Blocked(Vector3 ear, Vector3 source)
        {
            Vector3 direction = source - ear;
            float length = direction.magnitude;
            if (length < 0.01f) return false;
            foreach (var hit in Physics.RaycastAll(ear, direction / length, length, _walls, QueryTriggerInteraction.Ignore))
                if (hit.distance > 1.2f && hit.distance < length - 0.6f) return true;
            return false;
        }

        /// <summary>Adds to her unease; at 1 she stirs.</summary>
        public void Disturb(float amount)
        {
            if (State != Sleep.Asleep || !InBed) return;
            _disturbance += amount;
            if (_disturbance >= 1f) Stir(3.5f);
        }

        /// <summary>The snoring stops; she mumbles and turns over, then sleeps on (unless disturbed again meanwhile).</summary>
        public void Stir(float seconds)
        {
            if (State == Sleep.Awake || State == Sleep.Roused || !InBed) return;
            State = Sleep.Stirring;
            StopBreath();
            _stir = Mathf.Max(_stir, seconds);
            _disturbance = 0f;
            if (_audio != null) _audio.PlayAt("mumble", Head, 0.55f, 9f);
            if (_hud != null) _hud.ShowBark("TuBa", "…ừm… đứa nào… (ú ớ)", 2.2f);
            Stirred?.Invoke(this);
        }

        /// <summary>Awake in bed and listening, because of a sound from <paramref name="source"/>.</summary>
        public void Rouse(Vector3 source)
        {
            if (State == Sleep.Awake || !InBed) return;
            bool already = State == Sleep.Roused;
            State = Sleep.Roused;
            StopBreath();
            _roused = _rousedSeconds;
            _disturbance = 0f;
            LastHeard = source;
            if (already) { Heard?.Invoke(this, source); return; }
            if (_audio != null) { _audio.PlayAt("chair_creak", Head, 0.6f, 12f); _audio.PlayAt("mumble", Head, 0.75f, 12f); }
            Roused?.Invoke(this, source);
        }

        /// <summary>
        /// Kiều's hand at her belt at the wrong moment: while she breathes in, or while she lies awake. The breath stops
        /// at once and she is awake (no <see cref="Roused"/> call for the guard: she has the thief by the wrist); the
        /// chapter plays the catch.
        /// </summary>
        public void CatchHand(Grab why)
        {
            if (State == Sleep.Awake || !InBed) return;
            StopBreath();
            State = Sleep.Roused;
            _roused = _rousedSeconds;
            _disturbance = 0f;
            HandCaught?.Invoke(this, why);
        }

        /// <summary>Up for good (J2: the keys are gone).</summary>
        public void WakeUp()
        {
            if (State == Sleep.Awake) return;
            State = Sleep.Awake;
            StopBreath();
            Woke?.Invoke(this);
        }

        /// <summary>On her feet and out of bed (Kiều's door at the start): no breath clock, no hearing as a sleeper.</summary>
        public void GetUp()
        {
            _inBed = false;
            StopBreath();
        }

        /// <summary>
        /// Checkpoints and the chapter putting her to bed: asleep on the sập (the breath starts on an in-breath), or kept
        /// awake after J2, without any announcement.
        /// </summary>
        public void Restore(Sleep state)
        {
            State = state == Sleep.Awake ? Sleep.Awake : Sleep.Asleep;
            _inBed = State == Sleep.Asleep;
            _disturbance = 0f;
            _stir = 0f;
            _roused = 0f;
            RestartBreath();
        }

        private void RestartBreath()
        {
            _inhaling = true;
            StopBreath();
            BeginPhase();
        }

        /// <summary>
        /// A breath phase starts: its take is chosen now, and an in-breath lasts exactly as long as its recorded snore (the
        /// recordings are cut on their own rhythm, 1.0–1.6 s), so the snore never runs into the out-breath and the sound
        /// is the rule. Without recorded takes the set lengths are used. The out-breath always lasts the set 2.6 s.
        /// </summary>
        private void BeginPhase()
        {
            _phase = 0f;
            _phaseStarted = false;
            LastBreathId = _inhaling ? "snore_deep" : "snore_out";
            _clip = _audio != null ? _audio.Effect(LastBreathId, out _clipGain) : null;
            bool recorded = _audio != null && _audio.HasFoley(LastBreathId);
            _length = _inhaling ? (recorded && _clip != null ? Mathf.Clamp(_clip.length, 0.9f, 2f) : _inhale) : _exhale;
        }

        private void StopBreath()
        {
            _sounding = false;
            if (_breath != null && _breath.isPlaying) _breath.Stop();
        }

        /// <summary>The sound of the phase under way, on her own source, from where the clock is now.</summary>
        private void StartPhaseSound()
        {
            _phaseStarted = true;
            _sounding = false;
            if (_audio == null) return;
            if (_breath == null) _breath = _audio.OwnSource(_actor != null ? _actor.transform : transform, "Breath", _breathReach);
            _breath.Stop();
            if (_clip == null || _phase >= _clip.length) return; // a slow frame ran past this clip: wait for the next phase
            _breath.transform.position = Head;
            _breath.clip = _clip;
            _breath.volume = Mathf.Clamp01((_inhaling ? 0.8f : 0.5f) * _clipGain * _audio.EffectsLevel);
            _breath.time = _phase;
            _breath.Play();
            _sounding = true;
            _soundLength = _clip.length;
        }

        private void Update()
        {
            InhaledThisFrame = ExhaledThisFrame = 0f;
            _disturbance = Mathf.Max(0f, _disturbance - 0.12f * Time.deltaTime);
            if (!InBed) { StopBreath(); return; }
            if (State == Sleep.Stirring)
            {
                if ((_stir -= Time.deltaTime) <= 0f) { State = Sleep.Asleep; RestartBreath(); }
                return;
            }
            if (State == Sleep.Roused)
            {
                if ((_roused -= Time.deltaTime) > 0f) return;
                State = Sleep.Asleep;
                RestartBreath();
                if (_audio != null) _audio.PlayAt("mumble", Head, 0.4f, 8f);
                if (_hud != null) _hud.ShowBark("TuBa", "(lầu bầu) …lại con mèo… ngủ cũng không yên…", 2.6f);
                Settled?.Invoke(this);
                return;
            }
            // A catch scene holds the clock; the breath picks up where it stood if she is still asleep after it.
            if (!Breathing) { StopBreath(); _phaseStarted = false; return; }
            if (!_phaseStarted) StartPhaseSound();
            // One step of the clock, split at phase changes (a slow frame can hold the end of one breath and the start of the next).
            float left = Time.deltaTime;
            while (left > 0f)
            {
                float step = Mathf.Min(left, _length - _phase);
                if (_inhaling) InhaledThisFrame += step; else ExhaledThisFrame += step;
                _phase += step;
                left -= step;
                if (_phase < _length - 1e-5f) break;
                _inhaling = !_inhaling;
                BeginPhase();
            }
            if (!_phaseStarted) StartPhaseSound();
            if (_breath != null) _breath.transform.position = Head;
        }

        /// <summary>Her chest rises through the in-breath and settles through the out-breath (over the animator's pose).</summary>
        private void LateUpdate()
        {
            if (!Breathing || _chestHeave <= 0f || _actor == null) return;
            if (_chest == null)
            {
                var animator = _actor.GetComponentInChildren<Animator>();
                if (animator == null || !animator.isHuman) return;
                _chest = animator.GetBoneTransform(HumanBodyBones.Chest);
                if (_chest == null) _chest = animator.GetBoneTransform(HumanBodyBones.Spine);
                if (_chest == null) return;
            }
            float t = Mathf.Clamp01(_phase / Mathf.Max(0.01f, _length));
            float lift = _inhaling ? Mathf.SmoothStep(0f, 1f, t) : 1f - Mathf.SmoothStep(0f, 1f, t);
            _chest.rotation = Quaternion.AngleAxis(-_chestHeave * lift, _actor.transform.right) * _chest.rotation;
        }
    }
}
