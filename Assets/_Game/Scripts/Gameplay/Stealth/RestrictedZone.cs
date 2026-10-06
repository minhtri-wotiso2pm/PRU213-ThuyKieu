using UnityEngine;

namespace ThuyKieu.Gameplay.Stealth
{
    /// <summary>Trigger volume where being seen by a guard counts as trespassing.</summary>
    [RequireComponent(typeof(BoxCollider))]
    public class RestrictedZone : MonoBehaviour
    {
        [SerializeField] private string _label = "Khu vực cấm";
        private static int _inside;
        private static RestrictedZone _current;

        public static bool PlayerInside => _inside > 0;
        public static string CurrentLabel => _current != null ? _current._label : null;

        private void Awake() { GetComponent<BoxCollider>().isTrigger = true; }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _inside++;
            _current = this;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _inside = Mathf.Max(0, _inside - 1);
            if (_inside == 0) _current = null;
        }

        private void OnDisable() { if (_current == this) { _current = null; _inside = 0; } }

        /// <summary>Teleports skip trigger exits; call after moving the player.</summary>
        public static void ResetPresence() { _inside = 0; _current = null; }
    }
}
