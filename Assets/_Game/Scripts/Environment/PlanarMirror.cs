using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ThuyKieu.Environment
{
    /// <summary>
    /// A real mirror: a second camera, mirrored across this object's plane (its forward is the mirror normal, pointing
    /// into the room), renders what the surface reflects into a texture that the ThuyKieu/StoryMirror material reads in
    /// screen space. An oblique near plane clips everything behind the glass. It only renders while the surface is on
    /// screen and the main camera is close, so a mirror on a dressing table costs nothing from across the house.
    /// </summary>
    [DefaultExecutionOrder(1000)] // after the camera controllers have placed the main camera for this frame
    public class PlanarMirror : MonoBehaviour
    {
        [SerializeField] private Renderer _surface;
        [SerializeField] private float _maxDistance = 9f;
        [SerializeField, Range(0.25f, 1f)] private float _resolutionScale = 0.6f;
        [SerializeField] private LayerMask _layers = ~(1 << 5); // everything but UI
        [SerializeField] private float _clipOffset = 0.02f;

        private static readonly int ReflectionTex = Shader.PropertyToID("_ReflectionTex");
        private Camera _reflection;
        private RenderTexture _texture;
        private MaterialPropertyBlock _block;

        private void OnEnable()
        {
            if (_surface == null) _surface = GetComponent<Renderer>();
            var go = new GameObject("MirrorCamera") { hideFlags = HideFlags.DontSave };
            go.transform.SetParent(transform, false);
            _reflection = go.AddComponent<Camera>();
            _reflection.enabled = false;
            _reflection.depth = -50f;
            _reflection.cullingMask = _layers;
            _reflection.clearFlags = CameraClearFlags.Skybox;
            var data = _reflection.GetUniversalAdditionalCameraData();
            data.renderShadows = true;
            data.renderPostProcessing = false;
            data.requiresDepthTexture = false;
            data.requiresColorTexture = false;
            _block = new MaterialPropertyBlock();
        }

        private void OnDisable()
        {
            if (_reflection != null) Destroy(_reflection.gameObject);
            if (_texture != null) { _texture.Release(); Destroy(_texture); }
            _texture = null;
        }

        private void LateUpdate()
        {
            var main = Camera.main;
            bool show = main != null && _surface != null && _surface.isVisible
                        && (main.transform.position - transform.position).sqrMagnitude < _maxDistance * _maxDistance
                        && Vector3.Dot(main.transform.position - transform.position, transform.forward) > 0f;
            _reflection.enabled = show;
            if (show) Place(main, main.pixelWidth, main.pixelHeight, main.aspect);
        }

        /// <summary>QA stills render the main camera off-screen at their own size: re-render every mirror to match.</summary>
        public static void RenderAllFor(Camera main, int width, int height)
        {
            foreach (var mirror in FindObjectsByType<PlanarMirror>())
            {
                if (mirror._reflection == null) continue;
                mirror.Place(main, width, height, width / (float)height);
                mirror._reflection.Render();
            }
        }

        private void Place(Camera main, int screenWidth, int screenHeight, float aspect)
        {
            int width = Mathf.Max(64, Mathf.RoundToInt(screenWidth * _resolutionScale));
            int height = Mathf.Max(64, Mathf.RoundToInt(screenHeight * _resolutionScale));
            if (_texture == null || _texture.width != width || _texture.height != height)
            {
                if (_texture != null) { _texture.Release(); Destroy(_texture); }
                _texture = new RenderTexture(width, height, 24, RenderTextureFormat.DefaultHDR) { name = "MirrorReflection" };
                _reflection.targetTexture = _texture;
                _surface.GetPropertyBlock(_block);
                _block.SetTexture(ReflectionTex, _texture);
                _surface.SetPropertyBlock(_block);
            }

            // Mirror the main camera across the plane. Building a normal (right-handed) camera from the mirrored forward
            // and up gives the reflection flipped left-right; the shader samples it with u flipped back.
            Vector3 normal = transform.forward, point = transform.position;
            Vector3 eye = main.transform.position;
            Vector3 mirroredEye = eye - 2f * Vector3.Dot(eye - point, normal) * normal;
            Vector3 forward = Vector3.Reflect(main.transform.forward, normal);
            Vector3 up = Vector3.Reflect(main.transform.up, normal);
            _reflection.transform.SetPositionAndRotation(mirroredEye, Quaternion.LookRotation(forward, up));
            _reflection.fieldOfView = main.fieldOfView;
            _reflection.aspect = aspect;
            _reflection.nearClipPlane = main.nearClipPlane;
            _reflection.farClipPlane = Mathf.Min(main.farClipPlane, 60f);
            _reflection.ResetProjectionMatrix();

            // Oblique near plane on the mirror surface: nothing behind the glass (the wall, the stand) gets in.
            var view = _reflection.worldToCameraMatrix;
            Vector3 planePoint = view.MultiplyPoint(point + normal * _clipOffset);
            Vector3 planeNormal = view.MultiplyVector(normal).normalized;
            var plane = new Vector4(planeNormal.x, planeNormal.y, planeNormal.z, -Vector3.Dot(planePoint, planeNormal));
            _reflection.projectionMatrix = _reflection.CalculateObliqueMatrix(plane);
        }
    }
}
