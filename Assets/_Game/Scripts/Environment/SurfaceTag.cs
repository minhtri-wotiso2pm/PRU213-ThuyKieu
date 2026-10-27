using System.Collections.Generic;
using UnityEngine;

namespace ThuyKieu.Environment
{
    /// <summary>Floor materials. New values go at the end: scenes store the index.</summary>
    public enum Surface { Wood, Stone, WetStone, Earth, Mat, Mud }

    /// <summary>
    /// What a floor is made of, for footstep sounds and how far they carry. Untagged floors count as wood.
    /// A <b>patch</b> is a thin covering with no collider of its own (a rug, a reed mat): it covers the floor under its
    /// renderers' footprint, so a step on it sounds and carries like the covering, not the boards beneath.
    /// </summary>
    public class SurfaceTag : MonoBehaviour
    {
        [SerializeField] private Surface _surface = Surface.Wood;
        [Tooltip("A covering laid on another floor (rug, mat): found by footprint instead of by collider.")]
        [SerializeField] private bool _patch;
        [Tooltip("How far a step here carries, as a multiple of a step on boards; negative: as this floor carries. Lets a scene " +
                 "sound a floor right while keeping what its watchers hear as it was (Chapter 3).")]
        [SerializeField] private float _carry = -1f;

        private static readonly List<SurfaceTag> Patches = new List<SurfaceTag>();
        private Bounds _footprint;

        public Surface Surface => _surface;

        /// <summary>Tags a floor made at runtime (the Chapter 5 forest ground is built when the scene loads).</summary>
        public static SurfaceTag Add(GameObject floor, Surface surface)
        {
            var tag = floor.AddComponent<SurfaceTag>();
            tag._surface = surface;
            return tag;
        }
        /// <summary>How much further a step here carries than on boards (this floor's own <see cref="Loudness"/> unless set).</summary>
        public float Carry => _carry >= 0f ? _carry : Loudness(_surface);

        private void OnEnable()
        {
            if (!_patch) return;
            var renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            _footprint = renderers[0].bounds;
            foreach (var renderer in renderers) _footprint.Encapsulate(renderer.bounds);
            Patches.Add(this);
        }

        private void OnDisable() { Patches.Remove(this); }

        /// <summary>The covering under <paramref name="feet"/>, if she stands on one (within a hand's breadth of its top).</summary>
        public static bool PatchAt(Vector3 feet, out Surface surface) => PatchAt(feet, out surface, out _);

        /// <summary>As above, with how far a step on the covering carries (<see cref="Carry"/>).</summary>
        public static bool PatchAt(Vector3 feet, out Surface surface, out float carry)
        {
            foreach (var patch in Patches)
            {
                var b = patch._footprint;
                if (feet.x < b.min.x || feet.x > b.max.x || feet.z < b.min.z || feet.z > b.max.z) continue;
                if (Mathf.Abs(feet.y - b.max.y) > 0.12f) continue;
                surface = patch._surface;
                carry = patch.Carry;
                return true;
            }
            surface = Surface.Wood;
            carry = 1f;
            return false;
        }

        /// <summary>How much further a step on this floor carries than on boards (wet flagstones slap, mats muffle).</summary>
        public static float Loudness(Surface surface)
        {
            switch (surface)
            {
                case Surface.WetStone: return 1.5f;
                case Surface.Stone: return 1.1f;
                case Surface.Mud: return 0.9f;
                case Surface.Earth: return 0.8f;
                case Surface.Mat: return 0.6f;
                default: return 1f;
            }
        }

        /// <summary>The step sound for this floor; <paramref name="variant"/> alternates takes where there are two.</summary>
        public static string StepSound(Surface surface, int variant = 0)
        {
            bool second = (variant & 1) == 1;
            switch (surface)
            {
                case Surface.WetStone: return second ? "step_wet_2" : "step_wet";
                case Surface.Stone: return "step_stone";
                case Surface.Earth: return second ? "step_earth_2" : "step_earth";
                case Surface.Mat: return second ? "step_mat_2" : "step_mat";
                case Surface.Mud: return second ? "step_mud_2" : "step_mud";
                default: return "step_wood";
            }
        }
    }
}
