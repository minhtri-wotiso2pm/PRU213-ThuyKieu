using System.Collections.Generic;
using UnityEngine;

namespace ThuyKieu.Core
{
    /// <summary>
    /// Blocks player control while something else owns the screen (dialogue, mini-game, menu).
    /// Each owner pushes itself and pops itself, so overlapping systems never unlock each other.
    /// </summary>
    public static class InputLock
    {
        private static readonly HashSet<object> owners = new HashSet<object>();

        public static bool IsLocked => owners.Count > 0;

        public static void Push(object owner)
        {
            if (owner != null)
            {
                owners.Add(owner);
            }
        }

        public static void Pop(object owner)
        {
            if (owner != null)
            {
                owners.Remove(owner);
            }
        }

        /// <summary>The project disables domain reload on Play, so statics must be reset by hand.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            owners.Clear();
        }
    }
}
