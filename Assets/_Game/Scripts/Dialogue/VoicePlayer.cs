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
        private AudioSource _dry, _inner, _spirit, _barkDry, _barkInner;
        private DialogueManager _bound;

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
            if (_instance == null || !Core.GameSettings.VoiceOn) return;
            var clip = Load(speakerId, text);
            if (clip == null) return;
            var source = SpeakerCatalog.Get(speakerId).Style == SpeakerStyle.Inner ? _instance._barkInner : _instance._barkDry;
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
            if (!Core.GameSettings.VoiceOn) return;
            string speakerId = _bound != null ? _bound.CurrentSpeakerId : "Narrator";
            var clip = Load(speakerId, text);
            if (clip == null) return;
            var source = SourceFor(SpeakerCatalog.Get(speakerId).Style);
            source.volume = Volume * Core.GameSettings.Voice;
            source.clip = clip;
            source.Play();
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

        private void StopDialogueVoice()
        {
            _dry.Stop();
            _inner.Stop();
            _spirit.Stop();
        }

        private static AudioClip Load(string speakerId, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            return Resources.Load<AudioClip>(VoiceLine.ResourceFolder + "/" + VoiceLine.Key(speakerId, text));
        }
    }
}
