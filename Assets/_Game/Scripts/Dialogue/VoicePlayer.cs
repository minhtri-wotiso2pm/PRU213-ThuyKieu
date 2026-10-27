using UnityEngine;

namespace ThuyKieu.Dialogue
{
    /// <summary>
    /// Plays the recorded/synthesised voice for every dialogue line and bark. Clips are looked up in
    /// Resources/Voice by <see cref="VoiceLine.Key"/>; lines without a clip stay silent.
    /// Created automatically once per game and follows whichever <see cref="DialogueManager"/> is active.
    /// Inner thoughts get a close room reverb, the real Kiều's voice an echoing hall, so they read as "not out loud".
    /// </summary>
    public class VoicePlayer : MonoBehaviour
    {
        private const float Volume = 1f;

        private static VoicePlayer _instance;
        private AudioSource _dry, _inner, _spirit, _barkDry, _barkInner, _barkSpirit;
        private DialogueManager _bound;
        private string _lineSpeaker, _barkSpeaker, _silentSpeaker;
        private float _silentStart, _silentEnd;
        private readonly float[] _samples = new float[256];

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("VoicePlayer");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<VoicePlayer>();
        }

        /// <summary>Speaks a short in-world line (HUD bark) without cutting the dialogue voice.</summary>
        public static void PlayBark(string speakerId, string text)
        {
            if (_instance == null) return;
            _instance._barkSpeaker = speakerId;
            var clip = Core.GameSettings.VoiceOn ? Load(speakerId, text) : null;
            if (clip == null) { _instance.Mumble(speakerId, text, true); return; }
            var source = _instance.BarkSourceFor(SpeakerCatalog.Get(speakerId).Style);
            source.volume = Volume * Core.GameSettings.Voice;
            source.Stop();
            source.clip = clip;
            source.Play();
        }

        private void Awake()
        {
            _dry = Source("Dry");
            _inner = Source("Inner");
            var innerReverb = _inner.gameObject.AddComponent<AudioReverbFilter>();
            innerReverb.reverbPreset = AudioReverbPreset.Room;
            _spirit = Source("Spirit");
            var spiritReverb = _spirit.gameObject.AddComponent<AudioReverbFilter>();
            spiritReverb.reverbPreset = AudioReverbPreset.Concerthall;
            var echo = _spirit.gameObject.AddComponent<AudioEchoFilter>();
            echo.delay = 260f;
            echo.decayRatio = 0.3f;
            echo.wetMix = 0.45f;
            _barkDry = Source("BarkDry");
            _barkInner = Source("BarkInner");
            _barkInner.gameObject.AddComponent<AudioReverbFilter>().reverbPreset = AudioReverbPreset.Room;
            _barkSpirit = Source("BarkSpirit");
            _barkSpirit.gameObject.AddComponent<AudioReverbFilter>().reverbPreset = AudioReverbPreset.Concerthall;
        }

        private AudioSource Source(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = Volume;
            source.priority = 32;
            return source;
        }

        private void Update()
        {
            var manager = DialogueManager.Instance;
            if (manager == _bound) return;
            Unbind();
            _bound = manager;
            if (_bound == null) return;
            _bound.LineChanged += OnLine;
            _bound.DialogueEnded += StopDialogueVoice;
        }

        private void OnDestroy() { Unbind(); }

        private void Unbind()
        {
            if (_bound == null) return;
            _bound.LineChanged -= OnLine;
            _bound.DialogueEnded -= StopDialogueVoice;
            _bound = null;
        }

        private void OnLine(string speakerName, string text)
        {
            StopDialogueVoice();
            string speakerId = _bound != null ? _bound.CurrentSpeakerId : "Narrator";
            _lineSpeaker = speakerId;
            var clip = Core.GameSettings.VoiceOn ? Load(speakerId, text) : null;
            if (clip == null) { Mumble(speakerId, text, false); return; }
            var source = SourceFor(SpeakerCatalog.Get(speakerId).Style);
            source.volume = Volume * Core.GameSettings.Voice;
            source.clip = clip;
            source.Play();
        }

        /// <summary>
        /// How open <paramref name="speakerId"/>'s mouth should be right now (0–1): the loudness of their voice while it
        /// plays aloud, or a syllable rhythm for a spoken line that has no recording. Thoughts and the spirit's voice are
        /// not said out loud, so they never move a mouth.
        /// </summary>
        public static float MouthLevel(string speakerId)
        {
            if (_instance == null || string.IsNullOrEmpty(speakerId)) return 0f;
            return _instance.Level(speakerId);
        }

        private float Level(string speakerId)
        {
            if (_dry.isPlaying && _lineSpeaker == speakerId) return Loudness(_dry);
            if (_barkDry.isPlaying && _barkSpeaker == speakerId) return Loudness(_barkDry);
            if (_silentSpeaker == speakerId && Time.unscaledTime < _silentEnd)
            {
                // Roughly six syllables a second, each opening by a different amount.
                float t = (Time.unscaledTime - _silentStart) * 6.2f;
                float syllable = Mathf.Abs(Mathf.Sin(t * Mathf.PI));
                return syllable * Mathf.Lerp(0.45f, 1f, Mathf.PerlinNoise(Mathf.Floor(t) * 0.37f, 0.5f));
            }
            return 0f;
        }

        private float Loudness(AudioSource source)
        {
            source.GetOutputData(_samples, 0);
            float sum = 0f;
            foreach (float sample in _samples) sum += sample * sample;
            float rms = Mathf.Sqrt(sum / _samples.Length) / Mathf.Max(source.volume, 0.05f);
            return Mathf.Clamp01((rms - 0.012f) * 7f);
        }

        /// <summary>A spoken line without a voice clip still moves the speaker's mouth for about as long as it reads.</summary>
        private void Mumble(string speakerId, string text, bool bark)
        {
            if (!Core.GameSettings.VoiceOn || string.IsNullOrWhiteSpace(text)) return;
            var style = SpeakerCatalog.Get(speakerId).Style;
            // Keep unrecorded conversations audible, but quieter than authored barks so the placeholder cadence does
            // not compete with reading. Clips are cached and bounded by ProceduralAudio.
            var source = bark ? BarkSourceFor(style) : SourceFor(style);
            source.Stop();
            source.clip = Presentation.ProceduralAudio.Voice(speakerId, text, style == SpeakerStyle.Spirit, bark ? 6f : 1.8f);
            source.volume = (bark ? 0.5f : 0.22f) * Core.GameSettings.Voice;
            source.Play();
            _silentSpeaker = speakerId;
            _silentStart = Time.unscaledTime;
            _silentEnd = _silentStart + Mathf.Clamp(VoiceLine.Normalize(text).Length * 0.055f, 0.6f, 6f);
        }

        /// <summary>True while a dialogue line is still being spoken (auto-advance waits for it).</summary>
        public static bool IsSpeaking => _instance != null && (_instance._dry.isPlaying || _instance._inner.isPlaying || _instance._spirit.isPlaying);

        private AudioSource SourceFor(SpeakerStyle style)
        {
            switch (style)
            {
                case SpeakerStyle.Inner: return _inner;
                case SpeakerStyle.Spirit: return _spirit;
                default: return _dry;
            }
        }

        private AudioSource BarkSourceFor(SpeakerStyle style)
        {
            switch (style)
            {
                case SpeakerStyle.Inner: return _barkInner;
                case SpeakerStyle.Spirit: return _barkSpirit;
                default: return _barkDry;
            }
        }

        private void StopDialogueVoice()
        {
            _silentSpeaker = null;
            _dry.Stop();
            _inner.Stop();
            _spirit.Stop();
        }

        /// <summary>
        /// The recorded bark for a line, or null (voices off, not recorded), for callers that play it from a place in the
        /// world (a pursuer shouting behind her) instead of over the scene.
        /// </summary>
        public static AudioClip BarkClip(string speakerId, string text) => Core.GameSettings.VoiceOn ? Load(speakerId, text) : null;

        public static float BarkVolume => Volume * Core.GameSettings.Voice;

        private static AudioClip Load(string speakerId, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            return Resources.Load<AudioClip>(VoiceLine.ResourceFolder + "/" + VoiceLine.Key(speakerId, text));
        }
    }
}
