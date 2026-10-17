using UnityEngine;

namespace ThuyKieu.Interaction
{
    /// <summary>Slow spin and bob for the floating interaction marker.</summary>
    public class MarkerSpin : MonoBehaviour
    {
        private Vector3 _base;
        private void Awake() { _base = transform.localPosition; }
        private void Update()
        {
            transform.localPosition = _base + Vector3.up * Mathf.Sin(Time.time * 2.4f) * 0.06f;
            transform.Rotate(0f, 90f * Time.deltaTime, 0f, Space.World);
        }
    }
}
