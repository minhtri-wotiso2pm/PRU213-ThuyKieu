using UnityEngine;
using UnityEngine.Rendering;

namespace ThuyKieu.Environment
{
    /// <summary>
    /// A wall the orbit camera may see through: while it sits between the camera and the player it renders
    /// shadows only (so lighting does not change), and the camera does not collide with it.
    /// </summary>
    public class CameraFadeable : MonoBehaviour
    {
        [Tooltip("Details mounted on this wall (windows, couplets) that must disappear with it.")]
        [SerializeField] private GameObject[] _linked;

        private Renderer[] _renderers;
        private bool _hidden;

        private void Awake()
        {
            var list = new System.Collections.Generic.List<Renderer>(GetComponentsInChildren<Renderer>());
            if (_linked != null) foreach (var go in _linked) if (go != null) list.AddRange(go.GetComponentsInChildren<Renderer>());
            _renderers = list.ToArray();
        }

        public void SetHidden(bool hidden)
        {
            if (hidden == _hidden || _renderers == null) return;
            _hidden = hidden;
            foreach (var renderer in _renderers)
                if (renderer != null) renderer.shadowCastingMode = hidden ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
        }
    }
}
