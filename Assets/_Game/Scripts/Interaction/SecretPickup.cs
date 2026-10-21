using System;
using UnityEngine;

namespace ThuyKieu.Interaction
{
    /// <summary>
    /// A hidden object the story never points at: E picks it up once, the world decides what finding it means.
    /// Nothing marks it beyond a faint glint, so only a player who looks around finds it.
    /// </summary>
    public class SecretPickup : MonoBehaviour, IInteractable, IInteractableDisplayName, IInteractionAvailability
    {
        [SerializeField] private string _label = "Nhặt lên";

        private Func<bool> _available;
        private Action _found;

        public string DisplayName => _label;
        public bool IsAvailable => _available == null || _available();

        public void Configure(string label, Func<bool> available, Action found)
        {
            _label = label;
            _available = available;
            _found = found;
        }

        public void Interact()
        {
            if (!IsAvailable) return;
            _found?.Invoke();
            Destroy(gameObject);
        }
    }
}
