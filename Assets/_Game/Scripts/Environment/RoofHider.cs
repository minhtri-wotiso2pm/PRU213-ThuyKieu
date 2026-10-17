using UnityEngine;
using UnityEngine.Rendering;

namespace ThuyKieu.Environment
{
    /// <summary>Hides a room's roof (keeping its shadow) while the player is inside, so the orbit camera can see in.</summary>
    public class RoofHider : MonoBehaviour
    {
        [SerializeField] private Vector3 _centre;
        [SerializeField] private Vector3 _size = new Vector3(6, 4, 10);

        private Renderer[] _renderers;
        private Transform _player;
        private bool _hidden;

        private void Start()
        {
            _renderers = GetComponentsInChildren<Renderer>();
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) _player = player.transform;
        }

        private void Update()
        {
            if (_player == null) return;
            bool inside = new Bounds(_centre, _size).Contains(_player.position + Vector3.up);
            if (inside == _hidden) return;
            _hidden = inside;
            foreach (var renderer in _renderers)
                renderer.shadowCastingMode = inside ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
        }
    }
}
