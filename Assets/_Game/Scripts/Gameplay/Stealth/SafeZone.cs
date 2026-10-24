using ThuyKieu.Player;
using UnityEngine;

namespace ThuyKieu.Gameplay.Stealth
{
    /// <summary>
    /// The one place at night that is hers (Chapter 4: Kiều's own room). One rule for every watcher, whatever he is doing:
    /// <list type="bullet">
    /// <item>Once she is all the way in (<see cref="PlayerSafe"/>), he neither sees nor catches her, and a chase ends there.</item>
    /// <item>Nothing he walks to is inside: a sound, a sighting, a search or a story walk that would take him in takes him to
    /// the <see cref="Porch"/> outside its door instead, and he looks from there.</item>
    /// <item>A hiding place inside is not his to search.</item>
    /// </list>
    /// The threshold is steady: she counts as inside once her whole body is past the door line, and stops counting only when
    /// she steps back onto it, so standing in the doorway does not flicker between the two. The room is the box and nothing
    /// more: the corridor outside is as dangerous as anywhere else. With no zone in the scene every query answers "not
    /// safe", so other chapters are unaffected.
    /// </summary>
    public class SafeZone : MonoBehaviour
    {
        [Tooltip("The room (world-aligned box; a trigger on Ignore Raycast).")]
        [SerializeField] private BoxCollider _volume;
        [Tooltip("Where a watcher stands to look in: outside the door, on the floor he can walk.")]
        [SerializeField] private Transform _porch;
        [SerializeField] private PlayerMovement _player;
        [Tooltip("How far past the box's edge her feet must be before she counts as inside (her body's radius and a little).")]
        [SerializeField] private float _enterDepth = 0.35f;

        private static SafeZone _current;
        private bool _safe;

        /// <summary>She is all the way inside the room (see the class summary for the threshold).</summary>
        public static bool PlayerSafe => _current != null && _current._safe;
        public static SafeZone Current => _current;
        public Vector3 PorchPosition => _porch != null ? _porch.position : transform.position;

        /// <summary>Whether a point (a place to walk to, a hiding place, a sound) is inside the room.</summary>
        public static bool Contains(Vector3 point) => _current != null && _current.Inside(point, 0f);

        /// <summary>The place to go instead of <paramref name="point"/>: the porch when the point is inside the room, else the point itself.</summary>
        public static Vector3 Porch(Vector3 point) => Contains(point) ? _current.PorchPosition : point;

        private void OnEnable() { _current = this; }

        private void OnDisable() { if (_current == this) _current = null; }

        private void Update() { Refresh(); }

        /// <summary>Re-reads where she is now (also after a checkpoint puts her somewhere else).</summary>
        public void Refresh()
        {
            if (_player == null || _volume == null) { _safe = false; return; }
            Vector3 feet = _player.transform.position;
            _safe = _safe ? Inside(feet, 0f) : Inside(feet, _enterDepth);
        }

        /// <summary>Inside the box shrunk by <paramref name="depth"/> on every side (height: the room's floor to its ceiling).</summary>
        private bool Inside(Vector3 point, float depth)
        {
            if (_volume == null) return false;
            var b = _volume.bounds;
            return point.x > b.min.x + depth && point.x < b.max.x - depth && point.z > b.min.z + depth && point.z < b.max.z - depth
                   && point.y > b.min.y - 0.6f && point.y < b.max.y;
        }
    }
}
