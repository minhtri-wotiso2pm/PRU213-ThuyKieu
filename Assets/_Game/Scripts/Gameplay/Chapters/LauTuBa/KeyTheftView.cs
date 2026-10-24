using ThuyKieu.Core.Story;
using ThuyKieu.Player;
using UnityEngine;

namespace ThuyKieu.Gameplay.Chapters.LauTuBa
{
    /// <summary>
    /// What the key theft looks like (<see cref="KeyTheft"/> owns the rules): the close shot Kiều leans into, the ring and
    /// its keys moved by the work done so far, and a small arrow by the ring showing which way to draw the mouse. No hand is
    /// shown: the keys tilt, rise, slide off the hook and come away.
    /// <list type="number">
    /// <item>Lift: the bunch rises off the mat and tips up about the bottom of the hook's J.</item>
    /// <item>Unhook: the ring's wire slides up and out over the tip of the J (which opens upward on the outer side).</item>
    /// <item>Draw off: the bunch, free of the hook, comes towards Kiều and down, out of the picture.</item>
    /// </list>
    /// Whenever E is let go the bunch settles on something real for the stage reached: back on the mat, hanging further
    /// along the hook, or lying free on the mat beside her; taking hold again lifts it back to where the work had got to.
    /// While the shot is on, this component owns the bunch's pose (the ring's gravity hang, <see cref="HangingKeys"/>, is
    /// paused) and gives both back when the shot ends.
    /// </summary>
    public class KeyTheftView : MonoBehaviour
    {
        [SerializeField] private ThirdPersonCameraController _camera;
        [SerializeField] private StoryActor _actor;
        [SerializeField] private TuBaSleeper _sleeper;
        [SerializeField] private Transform _ringRoot;
        [SerializeField] private Transform _bunch;
        [SerializeField] private Transform _grip;
        [SerializeField] private Material _hintMaterial;
        [Tooltip("Lens of the close shot (her own field of view comes back when it ends).")]
        [SerializeField] private float _fov = 60f;
        [SerializeField] private LayerMask _solid = ~(1 << 2);

        private Transform _shot;
        private HangingKeys _hanging;
        private LineRenderer _arrow;
        private Vector3 _bunchBase, _up = Vector3.up, _out = Vector3.right, _toward = Vector3.back, _keysAtRest;
        private Quaternion _bunchBaseRotation = Quaternion.identity;
        private float _grab;
        private bool _active;

        /// <summary>The camera has finished leaning in.</summary>
        public bool Ready => _active && _camera != null && _camera.CloseWeight >= 0.999f;
        public bool Active => _active;
        /// <summary>The close shot's place (QA).</summary>
        public Transform Shot => _shot;
        public Vector3 KeysAtRest => _keysAtRest;

        private void Awake()
        {
            if (_bunch != null) { _bunchBase = _bunch.localPosition; _bunchBaseRotation = _bunch.localRotation; }
            if (_ringRoot != null) _hanging = _ringRoot.GetComponent<HangingKeys>();
            var go = new GameObject("PullHint");
            go.transform.SetParent(transform, false);
            _arrow = go.AddComponent<LineRenderer>();
            _arrow.useWorldSpace = true;
            _arrow.positionCount = 5;
            _arrow.widthMultiplier = 0.0022f;
            _arrow.numCapVertices = 2;
            _arrow.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _arrow.receiveShadows = false;
            if (_hintMaterial != null) _arrow.sharedMaterial = _hintMaterial;
            _arrow.enabled = false;
        }

        private void OnDisable() { if (_active) End(true); }

        // ------------------------------------------------------------------ the shot

        /// <summary>Leans into the close shot: from beside her hip, towards her feet, looking along her to her face.</summary>
        public void Begin()
        {
            if (_active) return;
            _active = true;
            _keysAtRest = _grip != null ? _grip.position : _bunch.position;
            Vector3 feet = _actor != null ? _actor.transform.position : _keysAtRest;
            Vector3 head = _sleeper != null ? _sleeper.Head : _keysAtRest + Vector3.forward;
            Vector3 axis = Flat(head - feet).normalized;
            Vector3 along = feet + axis * Vector3.Dot(Flat(_keysAtRest - feet), axis);
            _out = Flat(_keysAtRest - along).normalized;
            if (_out.sqrMagnitude < 0.5f) _out = Vector3.Cross(Vector3.up, axis);
            // Her chest half way up from the belt to the head: in the picture behind the keys, so her breathing can be read.
            Vector3 chest = Vector3.Lerp(along + Vector3.up * (_keysAtRest.y - along.y), head, 0.55f);
            // Aimed nearer the keys than her chest: the keys sit above the prompt, her face still near the top.
            Vector3 look = Vector3.Lerp(_keysAtRest, chest, 0.3f);
            // About 0.45 m from the keys: close enough to read them, far enough to keep her in the picture.
            Vector3 place = _keysAtRest + _out * 0.3f - axis * 0.21f + Vector3.up * 0.27f;
            // Never inside anything (the sập, her body, a wall): closer to the keys along the same line until it is clear.
            for (float t = 1f; t >= 0.45f; t -= 0.05f)
            {
                Vector3 p = _keysAtRest + (place - _keysAtRest) * t;
                if (!Physics.CheckSphere(p, 0.06f, _solid, QueryTriggerInteraction.Ignore)) { place = p; break; }
            }
            if (_shot == null) _shot = new GameObject("KeyTheftShot").transform;
            _shot.SetPositionAndRotation(place, Quaternion.LookRotation(look - place));
            _toward = (place - _keysAtRest).normalized;
            if (_camera != null) _camera.SetCloseView(_shot, _fov);
            if (_hanging != null) _hanging.enabled = false;
            _grab = 0f;
        }

        /// <summary>
        /// Back to her eyes (looking at the keys). <paramref name="instant"/> when a catch scene takes the camera from where it
        /// is. The ring hangs by gravity again.
        /// </summary>
        public void End(bool instant)
        {
            if (!_active) return;
            _active = false;
            _arrow.enabled = false;
            if (_camera != null) _camera.ClearCloseView(_keysAtRest, instant);
            if (_hanging != null && _ringRoot != null && _ringRoot.gameObject.activeInHierarchy) _hanging.enabled = true;
        }

        // ------------------------------------------------------------------ the keys

        /// <summary>The way stage <paramref name="stage"/> (0–2) moves the bunch, in the world.</summary>
        public Vector3 StageMotion(int stage)
        {
            switch (stage)
            {
                case 0: return _up;
                case 1: return (_out * 0.85f + _up * 0.5f).normalized;
                default: return (_toward - _up * 0.6f).normalized;
            }
        }

        /// <summary>That way as the mouse must go on screen (unit length).</summary>
        public Vector2 ScreenDirection(int stage)
        {
            var camera = _camera != null ? _camera.GetComponent<Camera>() : Camera.main;
            if (camera == null) return Vector2.up;
            Vector3 from = _keysAtRest, to = from + StageMotion(stage) * 0.05f;
            Vector3 a = camera.WorldToScreenPoint(from), b = camera.WorldToScreenPoint(to);
            Vector2 d = new Vector2(b.x - a.x, b.y - a.y);
            return d.sqrMagnitude > 1e-4f ? d.normalized : Vector2.up;
        }

        /// <summary>
        /// Poses the bunch for <paramref name="progress"/> (0–1 of the whole job) and whether a hand is on it (eased), with a
        /// small shiver when it is pulled too hard.
        /// </summary>
        public void Pose(float progress, bool held, float shiver)
        {
            if (_bunch == null || _ringRoot == null) return;
            _grab = Mathf.MoveTowards(_grab, held ? 1f : 0f, Time.deltaTime / 0.25f);
            float g = Mathf.SmoothStep(0f, 1f, _grab);
            float s1 = Mathf.Clamp01(progress * 3f), s2 = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress * 3f - 1f)), s3 = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress * 3f - 2f));
            Vector3 unhook = _up * 0.012f * Mathf.Clamp01(s2 * 2f) + _out * 0.022f * Mathf.Clamp01((s2 - 0.3f) / 0.7f);
            // In the hand: lifted and tipped up, along the hook, then drawn off towards Kiều and down.
            Vector3 active = _up * 0.025f * s1 + unhook + (_toward * 0.22f - _up * 0.06f) * s3;
            float tilt = 30f * s1 * (1f - s3);
            // Let go: on the mat (lift undone), hanging in the hook where it got to, or, off the hook, lying on the mat.
            Vector3 rest = progress < 2f / 3f ? unhook : unhook + _toward * 0.04f * s3 - _up * 0.035f;
            Vector3 offset = Vector3.Lerp(rest, active, g);
            if (shiver > 0f) offset += _out * Mathf.Sin(Time.time * 70f) * 0.0015f * shiver;
            Vector3 swingAxis = Vector3.Cross(Vector3.down, _toward);
            if (swingAxis.sqrMagnitude < 1e-4f) swingAxis = _out;
            _bunch.localPosition = _bunchBase + _ringRoot.InverseTransformDirection(offset);
            _bunch.rotation = Quaternion.AngleAxis(tilt * g, swingAxis.normalized) * _ringRoot.rotation * _bunchBaseRotation;
        }

        /// <summary>The bunch back on its hook at rest (a checkpoint, a new night).</summary>
        public void ResetPose()
        {
            _grab = 0f;
            if (_bunch == null) return;
            _bunch.localPosition = _bunchBase;
            _bunch.localRotation = _bunchBaseRotation;
        }

        /// <summary>The arrow by the ring, along the way to draw for this stage (hidden when <paramref name="show"/> is false).</summary>
        public void Hint(int stage, bool show)
        {
            _arrow.enabled = show && _active;
            if (!_arrow.enabled) return;
            Vector3 motion = StageMotion(stage);
            Vector3 side = Vector3.Cross(motion, _toward).normalized;
            // Beside the ring, not on it.
            Vector3 tail = _bunch.position + side * 0.045f + _toward * 0.02f;
            Vector3 head = tail + motion * 0.045f;
            Vector3 barb = (-motion + side * 0.6f).normalized * 0.012f, barb2 = (-motion - side * 0.6f).normalized * 0.012f;
            _arrow.SetPosition(0, tail);
            _arrow.SetPosition(1, head);
            _arrow.SetPosition(2, head + barb);
            _arrow.SetPosition(3, head);
            _arrow.SetPosition(4, head + barb2);
        }

        private static Vector3 Flat(Vector3 v) { v.y = 0f; return v; }
    }
}
