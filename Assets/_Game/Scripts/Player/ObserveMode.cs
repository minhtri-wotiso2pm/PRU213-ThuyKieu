using ThuyKieu.Core;
using ThuyKieu.Interaction;
using ThuyKieu.UI;
using UnityEngine;

namespace ThuyKieu.Player
{
    /// <summary>
    /// Hold right mouse to observe: the view narrows, Kiều slows down and a target near the
    /// screen centre fills a focus ring. A full ring resolves the target's story choice.
    /// </summary>
    public class ObserveMode : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private PlayerMovement _player;
        [SerializeField] private StoryHUD _hud;
        [SerializeField] private float _normalFov = 58f;
        [SerializeField] private float _observeFov = 34f;
        [SerializeField] private float _centreTolerance = 0.22f;
        [SerializeField] private LayerMask _occluders = 1 << 8;

        private ObservableTarget[] _targets;
        private ObservableTarget _focus;
        private float _progress;

        public bool IsObserving { get; private set; }
        public ObservableTarget Focus => _focus;
        /// <summary>QA autopilot can hold observation without a mouse.</summary>
        public bool ForceObserve { get; set; }

        private ThirdPersonCameraController _orbit;

        private void Start()
        {
            // Include targets on characters who only arrive later in the chapter; FindTarget skips inactive ones.
            _targets = FindObjectsByType<ObservableTarget>(FindObjectsInactive.Include);
            _orbit = _camera.GetComponent<ThirdPersonCameraController>();
        }

        private void Update()
        {
            IsObserving = !_player.ControlLocked && (InputReader.IsObserveHeld || ForceObserve);
            _player.SpeedMultiplier = IsObserving ? 0.45f : 1f;
            // The orbit camera owns the field of view (zones, dialogue); it only needs to know we are zooming.
            if (_orbit != null) _orbit.ObserveFov = IsObserving ? _observeFov : 0f;
            else
            {
                float fov = IsObserving ? _observeFov : _normalFov;
                _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, _player.ControlLocked ? _normalFov : fov, 1 - Mathf.Exp(-8 * Time.deltaTime));
            }

            ObservableTarget best = IsObserving ? FindTarget() : null;
            if (best != _focus) { _focus = best; _progress = 0f; }
            if (_focus != null)
            {
                _progress += Time.deltaTime / _focus.RequiredSeconds;
                if (_progress >= 1f) { var done = _focus; _focus = null; _progress = 0f; done.Complete(); }
            }
            if (_hud != null) _hud.SetObserve(IsObserving, _focus != null ? _focus.Label : null, _progress);
        }

        private ObservableTarget FindTarget()
        {
            ObservableTarget best = null;
            float bestScore = _centreTolerance;
            foreach (var target in _targets)
            {
                if (target == null || !target.isActiveAndEnabled || !target.IsAvailable) continue;
                Vector3 point = target.FocusPoint;
                if (Vector3.Distance(point, _player.transform.position) > target.MaxDistance) continue;
                Vector3 viewport = _camera.WorldToViewportPoint(point);
                if (viewport.z <= 0) continue;
                float score = Vector2.Distance(new Vector2(viewport.x, viewport.y), new Vector2(0.5f, 0.5f));
                if (score > bestScore) continue;
                if (Physics.Linecast(_camera.transform.position, point, _occluders, QueryTriggerInteraction.Ignore)) continue;
                best = target;
                bestScore = score;
            }
            return best;
        }
    }
}
