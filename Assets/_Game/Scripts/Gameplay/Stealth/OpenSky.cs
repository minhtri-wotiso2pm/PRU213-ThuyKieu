using UnityEngine;

namespace ThuyKieu.Gameplay.Stealth
{
    /// <summary>
    /// A box of open sky (a courtyard, an alley): inside it the sun or the moon also lights the player for
    /// <see cref="LightExposure"/>, as it does on screen. Under roofs only lamps count.
    /// </summary>
    public class OpenSky : MonoBehaviour
    {
        [SerializeField] private Vector3 _size = new Vector3(10f, 6f, 10f);

        public bool Contains(Vector3 point) => new Bounds(transform.position, _size).Contains(point);
    }
}
