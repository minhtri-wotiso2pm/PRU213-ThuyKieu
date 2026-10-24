using System;
using ThuyKieu.Interaction;
using ThuyKieu.Presentation;
using UnityEngine;

namespace ThuyKieu.Gameplay.Stealth
{
    /// <summary>
    /// A heap of gravel she can always come back to (Chapter 4: by the steps outside her room): E fills her hand up to what
    /// she can carry (<see cref="PebbleThrow.Capacity"/>), so throwing them all never leaves the night unwinnable.
    /// </summary>
    public class PebblePile : MonoBehaviour, IInteractable, IInteractableDisplayName, IInteractionAvailability
    {
        [SerializeField] private PebbleThrow _pebbles;
        [SerializeField] private StoryAudio _audio;
        [SerializeField] private string _name = "sỏi";

        public event Action<PebblePile, int> Taken;

        public bool IsAvailable => _pebbles != null && isActiveAndEnabled;

        public string DisplayName => _pebbles == null ? "" : _pebbles.Full
            ? "<color=#A89A86>Đã cầm đủ " + _pebbles.Capacity + " viên " + _name + "</color>"
            : "Nhặt " + _name + "  <size=80%><color=#A89A86>(" + _pebbles.Count + "/" + _pebbles.Capacity + " · Q: ném để dụ lính)</color></size>";

        public void Interact()
        {
            if (!IsAvailable) return;
            int taken = _pebbles.Refill();
            if (taken <= 0) return;
            // A hand in the gravel: a few stones knocking together, close by and soft (no noise for the watchers).
            if (_audio != null) _audio.PlayAt("pebble_hard", transform.position + Vector3.up * 0.1f, 0.22f, 6f, 1.1f);
            Taken?.Invoke(this, taken);
        }
    }
}
