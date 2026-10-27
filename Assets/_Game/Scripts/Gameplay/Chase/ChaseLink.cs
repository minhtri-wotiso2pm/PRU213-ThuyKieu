using UnityEngine;

namespace ThuyKieu.Gameplay.Chase
{
    /// <summary>
    /// What a person does to get across one NavMeshLink in the bamboo chase, read by <see cref="ChaseTraverser"/>: vault a
    /// fallen culm, duck under a low one, leap a ditch, or run over a plank bridge. Sits next to the link component.
    /// </summary>
    public class ChaseLink : MonoBehaviour
    {
        public enum Kind { Vault, Duck, Leap, Run }

        [SerializeField] private Kind _kind = Kind.Vault;
        [Tooltip("Vault and leap: how high the body rises above the straight line between the ends (metres).")]
        [SerializeField] private float _height = 0.7f;
        [Tooltip("Duck: speed while bent under the culm (m/s).")]
        [SerializeField] private float _duckSpeed = 3f;

        public Kind Type => _kind;
        public float Height => _height;
        public float DuckSpeed => _duckSpeed;
    }
}
