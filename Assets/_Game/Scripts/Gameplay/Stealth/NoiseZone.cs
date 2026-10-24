using System.Collections.Generic;
using UnityEngine;

namespace ThuyKieu.Gameplay.Stealth
{
    /// <summary>
    /// Box where sounds carry differently: rain drumming on the yard covers soft steps (below 1), the guests' music
    /// covers them better still, a wet floor makes them louder (above 1, applied by the footstep system instead).
    /// The smallest scale of all overlapping zones wins.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class NoiseZone : MonoBehaviour
    {
        [SerializeField, Range(0.1f, 1f)] private float _scale = 0.8f;

        private static readonly List<NoiseZone> Zones = new List<NoiseZone>();
        private BoxCollider _box;

        public float Scale { get => _scale; set => _scale = value; }

        private void Awake() { _box = GetComponent<BoxCollider>(); }
        private void OnEnable() { Zones.Add(this); }
        private void OnDisable() { Zones.Remove(this); }

        public static float ScaleAt(Vector3 point)
        {
            float scale = 1f;
            foreach (var zone in Zones)
                if (zone != null && zone._box != null && zone._box.bounds.Contains(point)) scale = Mathf.Min(scale, zone._scale);
            return scale;
        }
    }
}
