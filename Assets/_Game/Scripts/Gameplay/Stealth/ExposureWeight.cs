using UnityEngine;

namespace ThuyKieu.Gameplay.Stealth
{
    /// <summary>
    /// How much a light counts for <see cref="LightExposure"/> (default 1). Soft fill lights that only stand in for
    /// bounced light or moonlight spilling into a room count for less than a lamp a guard would actually see her by.
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class ExposureWeight : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float _weight = 1f;
        public float Weight => _weight;
    }
}
