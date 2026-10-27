using System;
using ThuyKieu.Core;
using ThuyKieu.Player;
using UnityEngine;

namespace ThuyKieu.Presentation
{
    /// <summary>
    /// Music crossfades, ambience beds, one-shot effects and footsteps for story chapters.
    /// Clips come from AudioBank by id (recorded, then baked); assigned overrides win, so recorded audio can be dropped in per scene.
    /// </summary>
    public class StoryAudio : MonoBehaviour
    {
        [Serializable] private struct Override { public string Id; public AudioClip Clip; }

        [SerializeField] private PlayerMovement _player;
        [SerializeField] private string _startAmbience = "road";
        [SerializeField, Range(0, 1)] private float _musicVolume = 0.32f;
        [SerializeField, Range(0, 1)] private float _ambienceVolume = 0.35f;
        [SerializeField, Range(0, 1)] private float _sfxVolume = 0.8f;
        [SerializeField] private Override[] _overrides;
        [Tooltip("Recorded Foley for this scene (several takes per id); its ids win over the shared bank.")]
        [SerializeField] private FoleyBank _foley;

        private AudioSource _musicA, _musicB, _ambience, _sfx, _steps;
        private AudioSource _activeMusic;
        private string _musicId;
        private float _duck = 1f, _ambienceDuck = 1f, _stepDistance;

        private void Awake()
        {
            _musicA = Source("MusicA", true);
            _musicB = Source("MusicB", true);
            _ambience = Source("Ambience", true);
            _sfx = Source("Sfx", false);
            _steps = Source("Footsteps", false);
            _activeMusic = _musicA;
        }

        private void Start() { SetAmbience(_startAmbience); }

        private AudioSource Source(string name, bool loop)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var source = go.AddComponent<AudioSource>();
            source.loop = loop;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            // Loops fade in from silence in Update; one-shots set their level per PlayOneShot, which this volume scales.
            source.volume = loop ? 0f : 1f;
            return source;
        }

        private AudioClip Get(string kind, string id) => Get(kind, id, out _);

        /// <summary>The clip for an id; <paramref name="gain"/> is the scene Foley group's level when the clip comes from it.</summary>
        private AudioClip Get(string kind, string id, out float gain)
        {
            gain = 1f;
            if (_overrides != null) foreach (var entry in _overrides) if (entry.Id == id && entry.Clip != null) return entry.Clip;
            if (kind == AudioBank.Sfx && _foley != null)
            {
                var take = _foley.Pick(id, out float level);
                if (take != null) { gain = level; return take; }
            }
            return AudioBank.Get(kind, id);
        }

        /// <summary>Whether this scene has recorded takes for an effect id (callers may ask for a finer id first).</summary>
        public bool HasFoley(string id) => _foley != null && _foley.Has(id);

        /// <summary>The music id last asked for, so a passing moment can hand the score back afterwards.</summary>
        public string CurrentMusic => _musicId;

        public void PlayMusic(string id)
        {
            if (id == _musicId) return;
            _musicId = id;
            AudioClip clip = Get(AudioBank.Music, id);
            var next = _activeMusic == _musicA ? _musicB : _musicA;
            next.clip = clip;
            next.volume = 0f;
            if (clip != null) next.Play();
            _activeMusic = next;
        }

        public void SetAmbience(string id)
        {
            AudioClip clip = Get(AudioBank.Ambience, id);
            if (_ambience.clip == clip) return;
            _ambience.clip = clip;
            if (clip != null) _ambience.Play(); else _ambience.Stop();
        }

        public void PlaySfx(string id, float volume = 1f)
        {
            var clip = Get(AudioBank.Sfx, id, out float gain);
            if (clip == null) return;
            _sfx.PlayOneShot(clip, volume * gain * _sfxVolume * GameSettings.Effects);
            EffectPlayed?.Invoke(id);
        }

        private readonly System.Collections.Generic.List<AudioSource> _spatial = new System.Collections.Generic.List<AudioSource>();

        /// <summary>
        /// An effect heard from a place in the world (a guard's step behind a wall, a door down the corridor): panned and
        /// fading with distance, so the player can tell where danger is by ear.
        /// </summary>
        public AudioSource PlayAt(string id, Vector3 position, float volume = 1f, float maxDistance = 18f, float pitch = 1f)
        {
            var clip = Get(AudioBank.Sfx, id, out float gain);
            if (clip == null) return null;
            volume *= gain;
            AudioSource source = null;
            foreach (var candidate in _spatial) if (!candidate.isPlaying) { source = candidate; break; }
            if (source == null)
            {
                if (_spatial.Count >= 24) source = _spatial[0];
                else
                {
                    source = Source("Spatial", false);
                    Spatialise(source);
                    _spatial.Add(source);
                }
            }
            source.transform.position = position;
            source.maxDistance = maxDistance;
            source.minDistance = 1f;
            source.pitch = pitch;
            source.clip = clip;
            source.volume = Mathf.Clamp01(volume * _sfxVolume * GameSettings.Effects);
            source.Play();
            EffectPlayed?.Invoke(id);
            return source;
        }

        /// <summary>QA: the id of every effect that started (one-shots and placed ones), in any chapter.</summary>
        public static event Action<string> EffectPlayed;

        private static void Spatialise(AudioSource source)
        {
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Custom;
            source.dopplerLevel = 0f;
            source.spread = 40f;
            // Soft near, gentle fall-off: footsteps across the yard should still be readable.
            source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, new AnimationCurve(
                new Keyframe(0f, 1f), new Keyframe(0.15f, 0.85f), new Keyframe(0.5f, 0.32f), new Keyframe(1f, 0f)));
        }

        /// <summary>
        /// A spatial source owned by the caller under <paramref name="parent"/> (a sleeper's breath, which must stop the
        /// moment she wakes rather than ring on in the shared pool), with the same fall-off as <see cref="PlayAt"/>.
        /// </summary>
        public AudioSource OwnSource(Transform parent, string name, float maxDistance)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            Spatialise(source);
            source.minDistance = 1f;
            source.maxDistance = maxDistance;
            return source;
        }

        /// <summary>The effect clip for an id (scene Foley first) and its group level, for callers with their own source.</summary>
        public AudioClip Effect(string id, out float gain) => Get(AudioBank.Sfx, id, out gain);

        /// <summary>The effects level (scene mix × player setting) that <see cref="PlayAt"/> applies.</summary>
        public float EffectsLevel => _sfxVolume * GameSettings.Effects;

        /// <summary>Lowers music (e.g. while eavesdropping) without stopping it.</summary>
        public void Duck(float amount) { _duck = Mathf.Clamp01(amount); }

        /// <summary>Lowers the ambience bed (the rain going quiet before something happens). 1 = normal.</summary>
        public void DuckAmbience(float amount) { _ambienceDuck = Mathf.Clamp01(amount); }

        private void Update()
        {
            float k = 1 - Mathf.Exp(-1.6f * Time.unscaledDeltaTime);
            var other = _activeMusic == _musicA ? _musicB : _musicA;
            _activeMusic.volume = Mathf.Lerp(_activeMusic.volume, _activeMusic.clip != null ? _musicVolume * _duck * GameSettings.Music : 0f, k);
            other.volume = Mathf.Lerp(other.volume, 0f, k * 1.5f);
            if (other.volume < 0.005f && other.isPlaying) other.Stop();
            _ambience.volume = Mathf.Lerp(_ambience.volume, _ambienceVolume * _ambienceDuck * GameSettings.Ambience, k);
            Footsteps();
        }

        private void Footsteps()
        {
            // First-person chapters play their own steps, by surface, with the noise they make.
            if (_player == null || _player.IsFirstPerson || _player.ControlLocked || _player.CurrentSpeed < 0.1f) { _stepDistance = 0f; return; }
            _stepDistance += _player.CurrentSpeed * Time.deltaTime;
            float stride = _player.IsRunning ? 1.5f : _player.IsCrouching ? 0.75f : 0.95f;
            if (_stepDistance < stride) return;
            _stepDistance = 0f;
            var clip = Get(AudioBank.Sfx, "step");
            _steps.pitch = UnityEngine.Random.Range(0.85f, 1.15f);
            _steps.PlayOneShot(clip, (_player.IsCrouching ? 0.12f : _player.IsRunning ? 0.45f : 0.28f) * _sfxVolume * GameSettings.Effects);
        }
    }
}
