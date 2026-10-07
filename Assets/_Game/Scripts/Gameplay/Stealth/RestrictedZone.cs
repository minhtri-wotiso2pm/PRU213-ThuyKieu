using System.Collections.Generic;
using UnityEngine;

namespace ThuyKieu.Gameplay.Stealth
{
    /// <summary>Trigger volume where being seen by a guard counts as trespassing.</summary>
    [RequireComponent(typeof(BoxCollider))]
    public class RestrictedZone : MonoBehaviour
    {
        [SerializeField] private string _label = "Khu vực cấm";
        [Tooltip("Open ground the player crosses all the time (a yard at night): counts for guards, but no banner.")]
        [SerializeField] private bool _quiet;

        private static readonly List<RestrictedZone> Inside = new List<RestrictedZone>();

        public static bool PlayerInside => Inside.Count > 0;
        public static string CurrentLabel => Current != null ? Current._label : null;
        /// <summary>True when the zone the player is in should not announce itself.</summary>
        public static bool CurrentQuiet => Current != null && Current._quiet;

        /// <summary>The most specific zone: a room inside the yard wins over the yard itself.</summary>
        private static RestrictedZone Current
        {
            get
            {
                RestrictedZone best = null;
                foreach (var zone in Inside)
                    if (zone != null && (best == null || (best._quiet && !zone._quiet))) best = zone;
                return best;
            }
        }

        private void Awake() { GetComponent<BoxCollider>().isTrigger = true; }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !Inside.Contains(this)) Inside.Add(this);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) Inside.Remove(this);
        }

        private void OnDisable() { Inside.Remove(this); }

        /// <summary>Teleports skip trigger exits; call after moving the player.</summary>
        public static void ResetPresence() { Inside.Clear(); }
    }
}
