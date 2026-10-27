using System.Collections;
using ThuyKieu.Interaction;
using ThuyKieu.Player;
using ThuyKieu.Presentation;
using UnityEngine;

namespace ThuyKieu.Gameplay.Chase
{
    /// <summary>
    /// A cart, a stack of baskets or a rack of pots beside the path: E while running past knocks it over,
    /// and pursuers wading through the mess slow right down.
    /// </summary>
    public class ChaseObstacle : MonoBehaviour, IInteractable, IInteractableDisplayName, IInteractionAvailability
    {
        [SerializeField] private PlayerMovement _player;
        [SerializeField] private StoryAudio _audio;
        [SerializeField] private Transform _visual;
        [Tooltip("Rotation (degrees) the visual falls by.")]
        [SerializeField] private Vector3 _fall = new Vector3(0, 0, 85);
        [SerializeField] private Vector3 _fallOffset = new Vector3(0, -0.3f, 0);
        [SerializeField] private float _slowRadius = 2.4f;
        [SerializeField] private string _label = "Xô đổ";
        [SerializeField] private GameObject _highlight;

        private Quaternion _upright;
        private Vector3 _uprightPosition;
        private Renderer[] _renderers;
        private float _groundHeight;

        public bool Toppled { get; private set; }
        public bool Armed { get; set; }
        public float SlowRadius => _slowRadius;
        public Vector3 SlowCenter => Toppled ? VisualBounds().center : transform.position;
        public string DisplayName => _label;
        public bool IsAvailable => Armed && !Toppled && _player != null && !_player.ControlLocked;

        private void Awake()
        {
            if (_visual == null) _visual = transform;
            _upright = _visual.localRotation;
            _uprightPosition = _visual.localPosition;
            _renderers = _visual.GetComponentsInChildren<Renderer>();
            _groundHeight = transform.position.y + 0.03f;
        }

        private void Update()
        {
            if (_highlight == null) return;
            bool visible = IsAvailable && Vector3.SqrMagnitude(_player.transform.position - transform.position) < 100f;
            if (_highlight.activeSelf != visible) _highlight.SetActive(visible);
        }

        public void Interact() { if (IsAvailable) Topple(); }

        public void Topple()
        {
            if (Toppled) return;
            Toppled = true;
            if (_highlight != null) _highlight.SetActive(false);
            if (_audio != null) _audio.PlaySfx("clatter");
            StartCoroutine(Fall());
        }

        public void ResetObstacle()
        {
            StopAllCoroutines();
            Toppled = false;
            _visual.localRotation = _upright;
            _visual.localPosition = _uprightPosition;
        }

        private IEnumerator Fall()
        {
            Quaternion end = _upright * Quaternion.Euler(_fall);
            Vector3 endPosition = FallenPosition(end);
            _visual.localRotation = _upright;
            _visual.localPosition = _uprightPosition;
            for (float t = 0; t < 1f; t += Time.deltaTime / 0.55f)
            {
                float k = t * t;
                _visual.localRotation = Quaternion.Slerp(_upright, end, k);
                _visual.localPosition = Vector3.Lerp(_uprightPosition, endPosition, k);
                KeepAboveGround();
                yield return null;
            }
            _visual.localRotation = end;
            _visual.localPosition = endPosition;
        }

        private Vector3 FallenPosition(Quaternion rotation)
        {
            _visual.localRotation = rotation;
            _visual.localPosition = _uprightPosition + _fallOffset;
            KeepAboveGround();
            return _visual.localPosition;
        }

        private void KeepAboveGround()
        {
            float lift = _groundHeight - VisualBounds().min.y;
            if (lift > 0f) _visual.position += Vector3.up * lift;
        }

        private Bounds VisualBounds()
        {
            if (_renderers == null || _renderers.Length == 0) return new Bounds(_visual.position, Vector3.zero);
            var bounds = _renderers[0].bounds;
            for (int i = 1; i < _renderers.Length; i++) bounds.Encapsulate(_renderers[i].bounds);
            return bounds;
        }
    }
}
