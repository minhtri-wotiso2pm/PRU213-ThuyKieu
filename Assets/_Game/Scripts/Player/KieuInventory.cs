using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThuyKieu.Player
{
    /// <summary>
    /// The few things Kiều carries in the first-person chapters (a hairpin, the key ring, papers): plain ids, no inventory
    /// screen. The chapter world mirrors them into Ink variables so checkpoints and the next chapter agree.
    /// </summary>
    public class KieuInventory : MonoBehaviour
    {
        private readonly HashSet<string> _items = new HashSet<string>();

        public event Action<string> Added;
        public IReadOnlyCollection<string> Items => _items;

        public bool Has(string id) => !string.IsNullOrEmpty(id) && _items.Contains(id);

        public void Add(string id)
        {
            if (string.IsNullOrEmpty(id) || !_items.Add(id)) return;
            Added?.Invoke(id);
        }

        public void Remove(string id) { _items.Remove(id); }

        /// <summary>Replaces the contents (checkpoint restore) without announcing anything.</summary>
        public void Set(IEnumerable<string> items)
        {
            _items.Clear();
            foreach (string id in items) if (!string.IsNullOrEmpty(id)) _items.Add(id);
        }
    }
}
