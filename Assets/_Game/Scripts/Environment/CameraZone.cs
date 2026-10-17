using System.Collections.Generic;
using UnityEngine;

namespace ThuyKieu.Environment
{
    /// <summary>
    /// Box region that overrides the orbit camera's framing while the player stands in it:
    /// small rooms frame closer and lower, open roads pull back. Highest priority wins; ties go to the smaller box.
    /// </summary>
    public class CameraZone : MonoBehaviour
    {
        private static readonly List<CameraZone> Active = new List<CameraZone>();

        [SerializeField] private Vector3 _size = new Vector3(6, 4, 6);
        [SerializeField] private float _distance = 3.2f;
        [SerializeField] private float _shoulder = 0.4f;
        [SerializeField] private float _fov = 55f;
        [Tooltip("Extra pivot height above the camera target.")]
        [SerializeField] private float _height = 0f;
        [SerializeField] private int _priority;

        public float Distance => _distance;
        public float Shoulder => _shoulder;
        public float Fov => _fov;
        public float Height => _height;

        private void OnEnable() { Active.Add(this); }
        private void OnDisable() { Active.Remove(this); }

        public bool Contains(Vector3 point) => new Bounds(transform.position, _size).Contains(point);

        public static CameraZone Find(Vector3 point)
        {
            CameraZone best = null;
            foreach (var zone in Active)
            {
                if (!zone.Contains(point)) continue;
                if (best == null || zone._priority > best._priority ||
                    (zone._priority == best._priority && zone._size.sqrMagnitude < best._size.sqrMagnitude)) best = zone;
            }
            return best;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.6f);
            Gizmos.DrawWireCube(transform.position, _size);
        }
    }
}
