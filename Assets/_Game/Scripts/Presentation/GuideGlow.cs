using ThuyKieu.Core;
using UnityEngine;

namespace ThuyKieu.Presentation
{
    /// <summary>
    /// A soft warm glow over one surface, for objective guidance: lamplight on paper, light through the crack of a door, a
    /// gleam on a ring or a lock. It is an additive unlit quad (shader ThuyKieu/GuideGlow) with no collider and no Light, so
    /// nothing in the world sees it: not the guards' light readings, not their sight lines, not the rooms' lighting. The
    /// owner sets <see cref="Target"/> (0–1.5); the glow rises and falls to it slowly, never blinks, and stays readable at
    /// any brightness setting by following only half of the player's brightness change.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    public class GuideGlow : MonoBehaviour
    {
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        private static readonly int EdgeId = Shader.PropertyToID("_Edge");
        private static readonly int RoundId = Shader.PropertyToID("_Round");

        [SerializeField] private Color _color = new Color(1f, 0.74f, 0.45f);
        [Tooltip("Brightness at a target of 1.")]
        [SerializeField] private float _intensity = 0.5f;
        [Tooltip("Soft border across (x) and along (y) the quad, as a share of its half-size (panels only).")]
        [SerializeField] private Vector2 _edge = new Vector2(0.3f, 0.3f);
        [Tooltip("A round gleam fading out from its middle, instead of a soft-edged panel.")]
        [SerializeField] private bool _round;
        [Tooltip("Turns to face the eye (gleams): readable crouched, from below or from the side.")]
        [SerializeField] private bool _faceCamera;
        [SerializeField] private float _riseSeconds = 2.6f;
        [SerializeField] private float _fallSeconds = 1.6f;

        private MeshRenderer _renderer;
        private MaterialPropertyBlock _block;
        private float _level, _seed;

        /// <summary>Where the glow is heading, faded to slowly.</summary>
        public float Target { get; set; }
        /// <summary>Applied at once (a door opened over a crack of light): multiplies the faded level.</summary>
        public float Gate { get; set; } = 1f;
        /// <summary>The faded level now (0 = off and not drawn).</summary>
        public float Level => _level;
        /// <summary>The level as drawn: faded, gated.</summary>
        public float Shown => _level * Gate;

        private void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
            _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _renderer.receiveShadows = false;
            _block = new MaterialPropertyBlock();
            _seed = Random.value * 10f;
            _renderer.enabled = false;
        }

        /// <summary>Straight to the target, no fade (a checkpoint restored, the chapter starting).</summary>
        public void Snap()
        {
            _level = Mathf.Max(0f, Target);
            Apply();
        }

        private void LateUpdate()
        {
            float goal = Mathf.Max(0f, Target);
            float seconds = goal > _level ? _riseSeconds : _fallSeconds;
            _level = Mathf.MoveTowards(_level, goal, Time.deltaTime / Mathf.Max(0.05f, seconds));
            Apply();
        }

        private void Apply()
        {
            float shown = _level * Gate;
            bool on = shown > 0.002f;
            if (_renderer.enabled != on) _renderer.enabled = on;
            if (!on) return;
            if (_faceCamera)
            {
                var eye = Camera.main;
                if (eye != null) transform.rotation = Quaternion.LookRotation(transform.position - eye.transform.position, Vector3.up);
            }
            // A lamp's slow breathing (a few percent), never a flicker.
            float breath = 1f + 0.08f * (Mathf.PerlinNoise(_seed, Time.time * 0.3f) - 0.5f);
            // Brightness is post exposure on the whole frame: dividing by its square root leaves the glow half of the change,
            // so at 50 % it still reads against the darker room and at 200 % it does not turn into a lamp itself.
            float brightness = 1f / Mathf.Sqrt(GameSettings.Brightness);
            float eased = shown < 1f ? shown * shown * (3f - 2f * shown) : shown;
            _block.SetColor(ColorId, _color);
            _block.SetFloat(IntensityId, _intensity * eased * breath * brightness);
            _block.SetVector(EdgeId, new Vector4(_edge.x, _edge.y, 0f, 0f));
            _block.SetFloat(RoundId, _round ? 1f : 0f);
            _renderer.SetPropertyBlock(_block);
        }
    }
}
