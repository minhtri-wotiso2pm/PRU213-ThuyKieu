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

        private Vignette _vignette;
        private ChromaticAberration _chroma;
        private ColorAdjustments _color;
        private FilmGrain _grain;
        private LensDistortion _distortion;
        private Bloom _bloom;
        private Mood _current, _target;
        private Image _flash;
        private float _flashAlpha, _shake, _shakeTime;
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
            _vignette.smoothness.Override(0.45f);
            _grain.type.Override(FilmGrainLookup.Medium3);
            _bloom.threshold.Override(0.9f);

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
            if (CurrentMood == "memory" || CurrentMood == "spirit") Flash(0.85f);
        }

        public void Flash(float strength) { _flashAlpha = Mathf.Max(_flashAlpha, strength); }

        public void Shake(float strength, float duration)
        {
            _shake = Mathf.Max(_shake, strength);
            _shakeTime = Mathf.Max(_shakeTime, duration);
        }

        private Mood MoodFor(string mood)
        {
            var m = new Mood { Vignette = _baseVignette, Saturation = _baseSaturation, Bloom = _baseBloom, Filter = _baseFilter, Contrast = 8f };
            switch (mood)
            {
                case "memory": m.Saturation = -70; m.Filter = new Color(1f, 0.86f, 0.62f); m.Vignette = 0.45f; m.Chroma = 0.55f; m.Bloom = 2.2f; m.Grain = 0.35f; break;
                case "imagine": m.Saturation = -85; m.Filter = new Color(0.72f, 0.82f, 1f); m.Vignette = 0.5f; m.Grain = 0.7f; m.Distortion = -0.15f; break;
                case "panic": m.Saturation = -35; m.Vignette = 0.5f; m.Chroma = 1f; m.Distortion = -0.25f; m.Contrast = 25f; m.Grain = 0.4f; break;
                case "spirit": m.Saturation = -55; m.Filter = new Color(1f, 0.88f, 0.55f); m.Vignette = 0.42f; m.Bloom = 3f; m.Exposure = -0.25f; m.Chroma = 0.3f; break;
                case "tension": m.Vignette = 0.38f; m.Saturation = -10; m.Contrast = 14f; break;
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
            _color.postExposure.Override(m.Exposure);
            _color.contrast.Override(m.Contrast);
            _color.colorFilter.Override(m.Filter);
            _grain.intensity.Override(m.Grain);
            _distortion.intensity.Override(m.Distortion);
            _bloom.intensity.Override(m.Bloom);
        }
    }
}
