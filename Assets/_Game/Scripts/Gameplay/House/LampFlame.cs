using System;
using ThuyKieu.Gameplay.Stealth;
using ThuyKieu.Interaction;
using ThuyKieu.Presentation;
using UnityEngine;

namespace ThuyKieu.Gameplay.House
{
    /// <summary>
    /// An oil lamp or candle Kiều can blow out: the room darkens (guards see less in the dark, see <see cref="LightExposure"/>),
    /// and anyone who knows the lamp was burning may notice. The flame flickers while lit.
    /// </summary>
    public class LampFlame : MonoBehaviour, IInteractable, IInteractableDisplayName, IInteractionAvailability
    {
        [SerializeField] private Light _light;
        [SerializeField] private GameObject _flame;
        [SerializeField] private StoryAudio _audio;
        [SerializeField] private bool _canBlowOut = true;
        [SerializeField] private float _flicker = 0.12f;

        private float _base, _phase;

        public event Action<LampFlame> Changed;
        public bool Lit { get; private set; } = true;
        public Light Light => _light;
        public string DisplayName => "Thổi tắt đèn";
        public bool IsAvailable => _canBlowOut && Lit;
        /// <summary>A draught leans the flame (a warning before something happens). 0 = still.</summary>
        public float Lean { get; set; }

        private void Awake()
        {
            if (_light != null) _base = _light.intensity;
            _phase = UnityEngine.Random.value * 10f;
        }

        public void Interact()
        {
            if (!IsAvailable) return;
            SetLit(false);
            if (_audio != null) _audio.PlayAt("blow", transform.position, 0.5f, 8f);
            Noise.Emit(transform.position, 1f, NoiseKind.Object);
        }

        public void SetLit(bool lit)
        {
            if (Lit == lit) return;
            Lit = lit;
            if (_light != null) _light.enabled = lit;
            if (_flame != null) _flame.SetActive(lit);
            Changed?.Invoke(this);
        }

        private void Update()
        {
            if (!Lit || _light == null) return;
            _phase += Time.deltaTime * (7f + Lean * 9f);
            float flicker = (Mathf.PerlinNoise(_phase, 0.37f) - 0.5f) * 2f * (_flicker + Lean * 0.3f);
            _light.intensity = _base * (1f + flicker - Lean * 0.25f);
            if (_flame != null) _flame.transform.localRotation = Quaternion.Euler(Lean * 28f + flicker * 6f, 0f, flicker * 4f);
        }
    }
}
