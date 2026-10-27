using ThuyKieu.Core;
using ThuyKieu.Core.Story;
using UnityEngine;

namespace ThuyKieu.Gameplay.Chase
{
    /// <summary>
    /// The Chapter 5 chase score in levels of pressure, all cut from one recording at one tempo (no stems exist, and none
    /// are faked by stacking the mix): each level is a seamless loop a whole number of bars long. A change of level waits
    /// for the next bar line of the loop playing and crossfades over one beat into the start of the next loop, so the beat
    /// never stumbles; the music is never restarted by an obstacle, a retry or a catch. It makes room for what matters: a
    /// short duck under a culm cracking or a catch, and the level steps up while the pursuer is really close (with
    /// hysteresis, so it does not flicker). It follows the Music setting and the pause (a switch waiting across a pause is
    /// planned again after it).
    /// </summary>
    public class ChaseMusic : MonoBehaviour
    {
        [SerializeField] private ChaseScore _score;

        [SerializeField, Range(0f, 1f)] private float _volume = 0.32f;

        private AudioSource _a, _b, _current, _pending;
        private int _level = -1, _wanted, _pendingLevel = -1;
        private double _switchAt;
        private float _fade = 1f, _duck = 1f, _duckUntil, _master, _stopSeconds = 1f, _oldGain;
        private bool _stopping, _close;
        private float _closeFor, _farFor;

        public int Level => _level;
        public bool Playing => _current != null && _current.isPlaying;
        private AudioClip[] Loops => _score != null ? _score.Levels : System.Array.Empty<AudioClip>();
        private float[] LoopGains => _score != null ? _score.Gains : System.Array.Empty<float>();
        private int BarBeats => _score != null ? Mathf.Max(1, _score.BeatsPerBar) : 4;
        private float Tempo => _score != null ? _score.Bpm : 120f;
        private float Beat => 60f / Mathf.Max(30f, Tempo);

        private void Awake()
        {
            _a = Source("ChaseMusicA");
            _b = Source("ChaseMusicB");
        }

        private AudioSource Source(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.volume = 0f;
            source.priority = 0;
            return source;
        }

        /// <summary>Starts the score at a level (it keeps playing across attempts after this).</summary>
        public void Begin(int level)
        {
            if (Loops.Length == 0) return;
            _stopping = false;
            _master = 1f;
            _wanted = Mathf.Clamp(level, 0, Loops.Length - 1);
            if (Playing) return;
            _level = _wanted;
            _current = _a;
            _current.clip = Loops[_level];
            _current.volume = 0f;
            _current.Play();
            _fade = 0f;
            StoryAutopilot.Note($"MUSIC chase starts at level {_level} ({_current.clip.name}, {Tempo:F1} BPM)");
        }

        /// <summary>The pressure the run asks for now (0 calm … highest); taken at the next bar line.</summary>
        public void Want(int level) { _wanted = Mathf.Clamp(level, 0, Loops.Length - 1); }

        /// <summary>Lowers the score for a moment so a sound can be heard over it (a crack, a catch).</summary>
        public void Duck(float amount, float seconds)
        {
            _duck = Mathf.Min(_duck, Mathf.Clamp01(amount));
            _duckUntil = Mathf.Max(_duckUntil, Time.unscaledTime + seconds);
        }

        /// <summary>How close the pursuer is (metres along the way): very close for a while lifts the pressure one level.</summary>
        public void Pursuer(float gap)
        {
            if (gap < 5f) { _closeFor += Time.deltaTime; _farFor = 0f; } else if (gap > 8f) { _farFor += Time.deltaTime; _closeFor = 0f; }
            if (!_close && _closeFor > 0.6f) _close = true;
            else if (_close && _farFor > 2f) _close = false;
        }

        /// <summary>Fades the score out (she is clear); it stops when silent.</summary>
        public void Stop(float seconds)
        {
            if (_stopping) return;
            _stopping = true;
            _stopSeconds = _master = Mathf.Max(0.1f, seconds);
        }

        private int Target => Mathf.Min(Loops.Length - 1, _wanted + (_close ? 1 : 0));

        private void Update()
        {
            if (_current == null) return;
            if (AudioListener.pause)
            {
                // Paused: a switch waiting for its bar line would start out of time; plan it again afterwards.
                if (_pending != null) { _pending.Stop(); _pending = null; _pendingLevel = -1; }
                return;
            }
            float dt = Time.unscaledDeltaTime;
            if (Time.unscaledTime > _duckUntil) _duck = Mathf.MoveTowards(_duck, 1f, dt * 0.9f);

            if (_pending == null && Target != _level && _current.isPlaying && !_stopping) Plan(Target);
            if (_pending != null && AudioSettings.dspTime >= _switchAt)
            {
                // Over the bar line: the new loop is playing; the old one fades out over a beat.
                var old = _current;
                _oldGain = GainOf(_level);
                _current = _pending;
                _level = _pendingLevel;
                _pending = null;
                _pendingLevel = -1;
                old.SetScheduledEndTime(AudioSettings.dspTime + Beat);
                _fade = 0f;
                StoryAutopilot.Note($"MUSIC level {_level} ({_current.clip.name})");
            }

            float level = _volume * GameSettings.Music * _duck;
            if (_stopping)
            {
                _master = Mathf.Max(0f, _master - dt);
                level *= Mathf.Clamp01(_master / _stopSeconds);
                if (_master <= 0f) { _a.Stop(); _b.Stop(); _current = null; return; }
            }
            _fade = Mathf.MoveTowards(_fade, 1f, dt / Beat);
            // Equal-power over the beat after the bar line: the new loop up, the old one down.
            _current.volume = level * GainOf(_level) * Mathf.Sin(_fade * Mathf.PI * 0.5f);
            var other = _current == _a ? _b : _a;
            if (other != _pending && other.isPlaying) other.volume = level * _oldGain * Mathf.Cos(_fade * Mathf.PI * 0.5f);
        }

        private float GainOf(int level) => level >= 0 && LoopGains != null && level < LoopGains.Length ? LoopGains[level] : 1f;

        /// <summary>The next bar line of the loop playing, and the other source set to start its loop there.</summary>
        private void Plan(int level)
        {
            int rate = _current.clip.frequency;
            double bar = Beat * BarBeats;
            double position = _current.timeSamples / (double)rate;
            double untilBar = bar - position % bar;
            if (untilBar < 0.05) untilBar += bar; // too close to schedule reliably: the bar after
            _switchAt = AudioSettings.dspTime + untilBar;
            _pending = _current == _a ? _b : _a;
            _pending.Stop();
            _pending.clip = Loops[level];
            _pending.timeSamples = 0;
            _pending.volume = 0f;
            _pending.PlayScheduled(_switchAt);
            _pendingLevel = level;
        }

        private void OnDisable()
        {
            if (_a != null) _a.Stop();
            if (_b != null) _b.Stop();
            _current = _pending = null;
            _level = -1;
        }
    }
}
