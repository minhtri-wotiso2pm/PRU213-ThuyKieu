using UnityEngine;

namespace ThuyKieu.Environment
{
    /// <summary>Hanging lantern: swings gently in the breeze and its candle light flickers.</summary>
    public class LanternSway : MonoBehaviour
    {
        [SerializeField] private float _swing = 3.5f;
        [SerializeField] private float _speed = 0.8f;
        [SerializeField] private float _flicker = 0.12f;

        private Light _light;
        private float _baseIntensity, _seed;
        private Quaternion _rest;

        private void Start()
        {
            _light = GetComponentInChildren<Light>();
            if (_light != null) _baseIntensity = _light.intensity;
            _rest = transform.localRotation;
            _seed = (transform.position.x * 3.17f + transform.position.z * 1.71f) % 100f;
        }

        private void Update()
        {
            float t = Time.time * _speed + _seed;
            transform.localRotation = _rest * Quaternion.Euler(Mathf.Sin(t) * _swing, 0f, Mathf.Sin(t * 0.73f + 1.3f) * _swing * 0.6f);
            if (_light != null)
                _light.intensity = _baseIntensity * (1f - _flicker + _flicker * Mathf.PerlinNoise(Time.time * 6f, _seed));
        }
    }
}
