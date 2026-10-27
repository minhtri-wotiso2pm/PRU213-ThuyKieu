using UnityEngine;

namespace ThuyKieu.Environment
{
    /// <summary>
    /// The night wind through the Chapter 5 bamboo: sets the global the bamboo material reads (_C5Wind: direction in xy,
    /// strength in z, gusts per second in w), its strength rising and falling in slow gusts. Cleared when disabled, so
    /// nothing moves anywhere else.
    /// </summary>
    [ExecuteAlways]
    public class ForestWind : MonoBehaviour
    {
        private static readonly int WindId = Shader.PropertyToID("_C5Wind");

        [Tooltip("Where the wind blows towards (degrees, 0 = +z).")]
        [SerializeField] private float _heading = 60f;
        [SerializeField, Range(0f, 1f)] private float _strength = 0.55f;
        [Tooltip("How much the strength rises and falls in gusts (0..1 of it).")]
        [SerializeField, Range(0f, 1f)] private float _gustiness = 0.45f;
        [SerializeField] private float _gustsPerSecond = 0.35f;

        private void OnDisable() => Shader.SetGlobalVector(WindId, Vector4.zero);

        private void Update()
        {
            float t = Application.isPlaying ? Time.time : (float)(Time.realtimeSinceStartupAsDouble % 1000.0);
            float gust = Mathf.PerlinNoise(t * 0.11f, 3.7f);
            float strength = _strength * (1f - _gustiness + _gustiness * 2f * gust);
            float heading = (_heading + (Mathf.PerlinNoise(t * 0.03f, 9.1f) - 0.5f) * 30f) * Mathf.Deg2Rad;
            Shader.SetGlobalVector(WindId, new Vector4(Mathf.Sin(heading), Mathf.Cos(heading), Mathf.Clamp01(strength), _gustsPerSecond));
        }
    }
}
