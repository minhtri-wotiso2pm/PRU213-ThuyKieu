using System;
using ThuyKieu.Interaction;
using ThuyKieu.Player;
using ThuyKieu.Presentation;
using UnityEngine;

namespace ThuyKieu.Gameplay.House
{
    /// <summary>A small thing Kiều takes with her (a hairpin, a torn page): E puts it in her sleeve and hides the model.</summary>
    public class PickupItem : MonoBehaviour, IInteractable, IInteractableDisplayName, IInteractionAvailability
    {
        [SerializeField] private string _item;
        [SerializeField] private string _label = "Lấy";
        [SerializeField] private GameObject _visual;
        [SerializeField] private KieuInventory _inventory;
        [SerializeField] private StoryAudio _audio;
        [SerializeField] private string _sfx = "cloth";

        public event Action<PickupItem> Taken;
        public string Item => _item;
        public string DisplayName => _label;
        public bool IsAvailable => _inventory != null && !_inventory.Has(_item) && Enabled;
        /// <summary>The chapter can keep an item out of reach until its moment.</summary>
        public bool Enabled { get; set; } = true;

        public void Interact()
        {
            if (!IsAvailable) return;
            _inventory.Add(_item);
            if (_audio != null) _audio.PlaySfx(_sfx, 0.5f);
            Refresh();
            Taken?.Invoke(this);
        }

        /// <summary>Shows or hides the model to match the inventory (checkpoint restore).</summary>
        public void Refresh()
        {
            if (_visual != null) _visual.SetActive(_inventory == null || !_inventory.Has(_item));
        }
    }
}
