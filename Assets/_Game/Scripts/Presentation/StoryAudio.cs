using System;
using System.Collections.Generic;
using ThuyKieu.Core;
using ThuyKieu.Player;
using UnityEngine;

namespace ThuyKieu.Presentation
{
    /// <summary>
    /// Music crossfades, ambience beds, one-shot effects and footsteps for story chapters.
    /// Assigned clips override the procedural placeholders by id, so recorded audio can be dropped in later.
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

        private readonly Dictionary<string, AudioClip> _cache = new Dictionary<string, AudioClip>();
        private AudioSource _musicA, _musicB, _ambience, _sfx, _steps;
        private AudioSource _activeMusic;
        private string _musicId;
        private float _duck = 1f, _stepDistance;

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
            source.volume = 0f;
            return source;
        }

        private AudioClip Get(string kind, string id, Func<string, AudioClip> make)
        {
            if (_overrides != null) foreach (var entry in _overrides) if (entry.Id == id && entry.Clip != null) return entry.Clip;
            string key = kind + ":" + id;
            if (!_cache.TryGetValue(key, out AudioClip clip)) _cache[key] = clip = make(id);
            return clip;
        }

        public void PlayMusic(string id)
        {
            if (id == _musicId) return;
            _musicId = id;
            AudioClip clip = id == "none" ? null : Get("music", id, ProceduralAudio.Music);
            var next = _activeMusic == _musicA ? _musicB : _musicA;
            next.clip = clip;
            next.volume = 0f;
            if (clip != null) next.Play();
            _activeMusic = next;
        }

        public void SetAmbience(string id)
        {
            AudioClip clip = string.IsNullOrEmpty(id) ? null : Get("amb", id, ProceduralAudio.Ambience);
            if (_ambience.clip == clip) return;
            _ambience.clip = clip;
            if (clip != null) _ambience.Play(); else _ambience.Stop();
        }

        public void PlaySfx(string id, float volume = 1f)
        {
            var clip = Get("sfx", id, ProceduralAudio.Sfx);
            if (clip != null) _sfx.PlayOneShot(clip, volume * _sfxVolume * GameSettings.Effects);
        }

        /// <summary>Lowers music (e.g. while eavesdropping) without stopping it.</summary>
        public void Duck(float amount) { _duck = Mathf.Clamp01(amount); }

        private void Update()
        {
            float k = 1 - Mathf.Exp(-1.6f * Time.unscaledDeltaTime);
            var other = _activeMusic == _musicA ? _musicB : _musicA;
            _activeMusic.volume = Mathf.Lerp(_activeMusic.volume, _activeMusic.clip != null ? _musicVolume * _duck * GameSettings.Music : 0f, k);
            other.volume = Mathf.Lerp(other.volume, 0f, k * 1.5f);
            if (other.volume < 0.005f && other.isPlaying) other.Stop();
            _ambience.volume = Mathf.Lerp(_ambience.volume, _ambienceVolume * GameSettings.Ambience, k);
            Footsteps();
        }

        private void Footsteps()
        {
            if (_player == null || _player.ControlLocked || _player.CurrentSpeed < 0.1f) { _stepDistance = 0f; return; }
            _stepDistance += _player.CurrentSpeed * Time.deltaTime;
            float stride = _player.IsRunning ? 1.5f : _player.IsCrouching ? 0.75f : 0.95f;
            if (_stepDistance < stride) return;
            _stepDistance = 0f;
            var clip = Get("sfx", "step", ProceduralAudio.Sfx);
            _steps.pitch = UnityEngine.Random.Range(0.85f, 1.15f);
            _steps.PlayOneShot(clip, (_player.IsCrouching ? 0.12f : _player.IsRunning ? 0.45f : 0.28f) * _sfxVolume * GameSettings.Effects);
        }
    }
}
