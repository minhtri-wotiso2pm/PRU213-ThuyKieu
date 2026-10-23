using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThuyKieu.Core.Story
{
    /// <summary>
    /// Where each character's mouth sits on its face, keyed by the face renderer's mesh name. The generated models
    /// have no jaw bone or mouth shapes (the lips are painted on), so <see cref="ActorMouth"/> opens a small mouth over
    /// the painted one at this spot. Written by the "Bake Mouth Spots" editor command, loaded from Resources.
    /// </summary>
    [CreateAssetMenu(menuName = "ThuyKieu/Mouth Spots")]
    public class MouthSpots : ScriptableObject
    {
        public const string ResourcePath = "MouthSpots";

        [Serializable]
        public struct Spot
        {
            [Tooltip("\"avatar/mesh\", see KeyFor.")]
            public string Key;
            [Tooltip("Relative to the humanoid head bone; otherwise to the renderer's own transform.")]
            public bool OnHeadBone;
            public Vector3 Position;
            [Tooltip("Local z points out of the face, local y up the face.")]
            public Quaternion Rotation;
            [Tooltip("Mouth width in the parent's local units.")]
            public float Width;
        }

        [SerializeField] private List<Spot> _spots = new List<Spot>();
        [Tooltip("Scenes whose actors open their mouths; the same models in other chapters stay as they were.")]
        [SerializeField] private List<string> _scenes = new List<string>();

        private static MouthSpots _loaded;
        private static bool _tried;

        public IReadOnlyList<Spot> Spots => _spots;

        /// <summary>
        /// Identifies a face as "avatar/mesh": the generated rigs all name their mesh "node_0", and some bodies share a
        /// generic avatar name ("seatAvatar"), so neither is unique alone. Null for a renderer without a rig or mesh.
        /// </summary>
        public static string KeyFor(Renderer face)
        {
            if (!(face is SkinnedMeshRenderer skinned) || skinned.sharedMesh == null) return null;
            var animator = face.GetComponentInParent<Animator>(true);
            if (animator == null || !animator.isHuman || animator.avatar == null) return null;
            return animator.avatar.name + "/" + skinned.sharedMesh.name;
        }

        /// <summary>True for the scenes the spots were baked for (Chapters 0–2).</summary>
        public static bool Covers(string scene)
        {
            Load();
            return _loaded != null && _loaded._scenes.Contains(scene);
        }

        public static bool TryGet(string key, out Spot spot)
        {
            Load();
            if (_loaded != null && key != null)
                foreach (var candidate in _loaded._spots)
                    if (candidate.Key == key) { spot = candidate; return true; }
            spot = default;
            return false;
        }

        private static void Load()
        {
            if (!_tried) { _tried = true; _loaded = Resources.Load<MouthSpots>(ResourcePath); }
        }

        public void AddScene(string scene)
        {
            if (!_scenes.Contains(scene)) _scenes.Add(scene);
        }

        public void Set(Spot spot)
        {
            _spots.RemoveAll(existing => existing.Key == spot.Key);
            _spots.Add(spot);
        }
    }
}
