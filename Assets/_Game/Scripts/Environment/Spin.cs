using UnityEngine;

namespace ThuyKieu.Environment
{
    /// <summary>Turns steadily about its own up axis (a ceiling fan).</summary>
    public class Spin : MonoBehaviour
    {
        [SerializeField] private float _degreesPerSecond = 220f;

        private void Update() => transform.Rotate(0f, _degreesPerSecond * Time.deltaTime, 0f, Space.Self);
    }
}
