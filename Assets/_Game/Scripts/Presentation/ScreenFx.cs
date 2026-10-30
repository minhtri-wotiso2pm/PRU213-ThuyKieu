using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace ThuyKieu.Presentation
{
    /// <summary>
    /// Emotional screen states driven by "# fx:" tags and story sequences:
    /// memory (sepia flash), imagine (cold, grainy), panic (pulsing vignette, distortion),
    /// spirit (golden, bloomy). Uses a runtime URP Volume plus a white flash overlay and camera shake.
    /// </summary>
    public class ScreenFx : MonoBehaviour
    {
        private struct Mood
        {
            public float Vignette, Chroma, Saturation, Exposure, Contrast, Grain, Distortion, Bloom;
            public Color Filter;
        }

        [SerializeField] private Camera _camera;
        [SerializeField] private Color _baseFilter = Color.white;
        [SerializeField] private float _baseVignette = 0.26f;
        [SerializeField] private float _baseSaturation = 6f;
        [SerializeField] private float _baseBloom = 0.6f;
        [Header("Chapter grade")]
        [SerializeField] private float _baseContrast = 8f;
        [SerializeField] private float _baseExposure = 0f;
        [Tooltip("Filmic tonemapping keeps bright lanterns and skies coloured instead of clipping to white.")]
        [SerializeField] private bool _filmic;
        [SerializeField] private float _temperature;
        [SerializeField] private Color _shadowTint = Color.grey;
        [SerializeField] private Color _highlightTint = Color.grey;

        private Vignette _vignette;
        private ChromaticAberration _chroma;
        private ColorAdjustments _color;
        private FilmGrain _grain;
        private LensDistortion _distortion;
        private Bloom _bloom;
        private DepthOfField _depthOfField;
        private Mood _current, _target;
        private Image _flash;
        private float _flashAlpha, _shake, _shakeTime;
        private float _wakeBlur, _wakeBlurTarget;
        private Quaternion _lastShake = Quaternion.identity;

        public string CurrentMood { get; private set; } = "none";

        private void Awake()
        {
            var volumeObject = new GameObject("StoryVolume");
            volumeObject.transform.SetParent(transform, false);
            var volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 50;
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            volume.profile = profile;
            _vignette = profile.Add<Vignette>(true);
            _chroma = profile.Add<ChromaticAberration>(true);
            _color = profile.Add<ColorAdjustments>(true);
            _grain = profile.Add<FilmGrain>(true);
            _distortion = profile.Add<LensDistortion>(true);
            _bloom = profile.Add<Bloom>(true);
            _depthOfField = profile.Add<DepthOfField>(true);
            _vignette.smoothness.Override(0.45f);
            _grain.type.Override(FilmGrainLookup.Medium3);
            _bloom.threshold.Override(0.9f);
            if (_filmic)
            {
                profile.Add<Tonemapping>(true).mode.Override(TonemappingMode.ACES);
                var balance = profile.Add<WhiteBalance>(true);
                balance.temperature.Override(_temperature);
                var split = profile.Add<SplitToning>(true);
                split.shadows.Override(_shadowTint);
                split.highlights.Override(_highlightTint);
                split.balance.Override(-15f);
            }

            var canvasObject = new GameObject("FlashCanvas", typeof(Canvas));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 400;
            var flashObject = new GameObject("Flash", typeof(RectTransform), typeof(Image));
            flashObject.transform.SetParent(canvasObject.transform, false);
            var rect = flashObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            _flash = flashObject.GetComponent<Image>();
            _flash.color = new Color(1, 1, 1, 0);
            _flash.raycastTarget = false;

            _target = _current = MoodFor("none");
            Push(_current);
        }

        public void SetMood(string mood)
        {
            CurrentMood = string.IsNullOrEmpty(mood) ? "none" : mood;
            _target = MoodFor(CurrentMood);
            if (CurrentMood == "memory") Flash(0.55f); else if (CurrentMood == "spirit") Flash(0.32f);
            else if (CurrentMood == "page") Flash(0.45f);
        }

        public void Flash(float strength) { _flashAlpha = Mathf.Max(_flashAlpha, strength); }

        /// <summary>Soft, short-lived far blur for the instant Kiều opens her eyes.</summary>
        public void SetWakeBlur(float strength) { _wakeBlurTarget = Mathf.Clamp01(strength); }

        public void Shake(float strength, float duration)
        {
            strength *= ThuyKieu.Core.GameSettings.ShakeAmount;
            if (strength <= 0f) return;
            _shake = Mathf.Max(_shake, strength);
            _shakeTime = Mathf.Max(_shakeTime, duration);
        }

        private Mood MoodFor(string mood)
        {
            var m = new Mood { Vignette = _baseVignette, Saturation = _baseSaturation, Bloom = _baseBloom, Filter = _baseFilter, Contrast = _baseContrast, Exposure = _baseExposure };
            switch (mood)
            {
                case "memory": m.Saturation = -70; m.Filter = new Color(1f, 0.86f, 0.62f); m.Vignette = 0.45f; m.Chroma = 0.55f; m.Bloom = 1.35f; m.Grain = 0.35f; break;
                case "imagine": m.Saturation = -85; m.Filter = new Color(0.72f, 0.82f, 1f); m.Vignette = 0.5f; m.Grain = 0.7f; m.Distortion = -0.15f; break;
                case "panic": m.Saturation = -35; m.Vignette = 0.5f; m.Chroma = 1f; m.Distortion = -0.25f; m.Contrast = _baseContrast + 17f; m.Grain = 0.4f; break;
                case "spirit": m.Saturation = -45; m.Filter = new Color(1f, 0.9f, 0.68f); m.Vignette = 0.38f; m.Bloom = 1.15f; m.Exposure = _baseExposure - 0.3f; m.Chroma = 0.25f; break;
                case "tension": m.Vignette = 0.38f; m.Saturation = -10; m.Contrast = _baseContrast + 6f; break;
                // Warm lamplight of an old page: full colour, no grain or distortion (the story world, not a haunting).
                case "page": m.Filter = new Color(1f, 0.93f, 0.8f); m.Vignette = 0.3f; m.Bloom = 1.05f; m.Exposure = _baseExposure + 0.04f; break;
            }
            return m;
        }

        private void LateUpdate()
        {
            float k = 1 - Mathf.Exp(-3f * Time.unscaledDeltaTime);
            _current.Vignette = Mathf.Lerp(_current.Vignette, _target.Vignette, k);
            _current.Chroma = Mathf.Lerp(_current.Chroma, _target.Chroma, k);
            _current.Saturation = Mathf.Lerp(_current.Saturation, _target.Saturation, k);
            _current.Exposure = Mathf.Lerp(_current.Exposure, _target.Exposure, k);
            _current.Contrast = Mathf.Lerp(_current.Contrast, _target.Contrast, k);
            _current.Grain = Mathf.Lerp(_current.Grain, _target.Grain, k);
            _current.Distortion = Mathf.Lerp(_current.Distortion, _target.Distortion, k);
            _current.Bloom = Mathf.Lerp(_current.Bloom, _target.Bloom, k);
            _current.Filter = Color.Lerp(_current.Filter, _target.Filter, k);
            Mood shown = _current;
            if (CurrentMood == "panic")
            {
                // Heartbeat rhythm: two pulses then a rest.
                float beat = Mathf.Repeat(Time.time * 1.25f, 1f);
                float pulse = Mathf.Exp(-beat * 14f) + 0.6f * Mathf.Exp(-Mathf.Abs(beat - 0.22f) * 14f);
                shown.Vignette += pulse * 0.18f;
                shown.Distortion -= pulse * 0.12f;
            }
            Push(shown);

            _wakeBlur = Mathf.MoveTowards(_wakeBlur, _wakeBlurTarget, Time.unscaledDeltaTime * 0.7f);
            _depthOfField.mode.Override(_wakeBlur > 0.01f ? DepthOfFieldMode.Gaussian : DepthOfFieldMode.Off);
            _depthOfField.gaussianStart.Override(Mathf.Lerp(10f, 0.35f, _wakeBlur));
            _depthOfField.gaussianEnd.Override(Mathf.Lerp(20f, 1.45f, _wakeBlur));
            _depthOfField.gaussianMaxRadius.Override(Mathf.Lerp(0f, 1.35f, _wakeBlur));

            _flashAlpha = Mathf.MoveTowards(_flashAlpha, 0f, Time.unscaledDeltaTime * 1.1f);
            _flash.color = new Color(1, 1, 1, _flashAlpha);

            if (_shakeTime > 0f) _shakeTime -= Time.deltaTime;
        }

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += ApplyShake;
            RenderPipelineManager.endCameraRendering += RemoveShake;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= ApplyShake;
            RenderPipelineManager.endCameraRendering -= RemoveShake;
        }

        // Shake only exists while the frame renders, so camera controllers never see or accumulate it.
        private void ApplyShake(ScriptableRenderContext context, Camera camera)
        {
            if (camera != _camera || _shakeTime <= 0f) return;
            float amount = _shake * Mathf.Clamp01(_shakeTime * 2f) * 3f;
            float t = Time.time * 23f;
            _lastShake = Quaternion.Euler((Mathf.PerlinNoise(t, 0.3f) - 0.5f) * amount, (Mathf.PerlinNoise(0.7f, t) - 0.5f) * amount, 0f);
            camera.transform.rotation *= _lastShake;
        }

        private void RemoveShake(ScriptableRenderContext context, Camera camera)
        {
            if (camera != _camera || _lastShake == Quaternion.identity) return;
            camera.transform.rotation *= Quaternion.Inverse(_lastShake);
            _lastShake = Quaternion.identity;
        }

        private void Push(Mood m)
        {
            _vignette.intensity.Override(m.Vignette);
            _chroma.intensity.Override(m.Chroma);
            _color.saturation.Override(m.Saturation);
            // Apply the player's brightness after mood blending so it never accumulates or changes world lighting.
            _color.postExposure.Override(m.Exposure + Mathf.Log(ThuyKieu.Core.GameSettings.Brightness, 2f));
            _color.contrast.Override(m.Contrast);
            _color.colorFilter.Override(m.Filter);
            _grain.intensity.Override(m.Grain);
            _distortion.intensity.Override(m.Distortion);
            _bloom.intensity.Override(m.Bloom);
        }
    }
}
