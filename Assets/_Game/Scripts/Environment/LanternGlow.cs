using UnityEngine;

namespace ThuyKieu.Environment
{
    /// <summary>
    /// Keeps a lantern's silk honest about its light: glowing while its candle burns, plain red silk when it is out
    /// (by day, or the corridor before the house wakes).
    /// </summary>
    [ExecuteAlways]
    public class LanternGlow : MonoBehaviour
    {
        [SerializeField] private Light _light;
        [SerializeField] private Renderer _body;
        [SerializeField] private Material _lit;
        [SerializeField] private Material _unlit;

        private bool _shown;

        private void OnEnable() { Apply(); }

        private void LateUpdate()
        {
            if (_light != null && _light.enabled != _shown) Apply();
        }

        private void Apply()
        {
            if (_light == null || _body == null) return;
            _shown = _light.enabled;
            _body.sharedMaterial = _shown ? _lit : _unlit;
        }
    }
}
