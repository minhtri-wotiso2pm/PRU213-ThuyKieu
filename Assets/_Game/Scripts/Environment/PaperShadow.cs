using System.Collections;
using UnityEngine;

namespace ThuyKieu.Environment
{
    /// <summary>
    /// The shadow of someone passing behind a lit paper window or door: a soft dark shape that fades in, slides across
    /// and fades out just behind the paper. Real people walking past do not cast it (paper does not take shadows here),
    /// so the chapter plays it at the moments that matter: a guest crossing the room, Tú Bà getting up behind her door.
    /// </summary>
    public class PaperShadow : MonoBehaviour
    {
        [SerializeField] private Renderer _shape;
        [SerializeField] private Vector3 _from;
        [SerializeField] private Vector3 _to;
        [SerializeField] private float _seconds = 2.2f;
        [SerializeField, Range(0f, 1f)] private float _darkness = 0.7f;

        private MaterialPropertyBlock _block;
        private Coroutine _playing;

        public bool Playing => _playing != null;

        private void Awake()
        {
            _block = new MaterialPropertyBlock();
            SetAlpha(0f);
        }

        public void Play(float seconds = -1f)
        {
            if (_playing != null) StopCoroutine(_playing);
            _playing = StartCoroutine(Pass(seconds > 0f ? seconds : _seconds));
        }

        public void Stop()
        {
            if (_playing != null) StopCoroutine(_playing);
            _playing = null;
            SetAlpha(0f);
        }

        private IEnumerator Pass(float seconds)
        {
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                float k = t / seconds;
                _shape.transform.localPosition = Vector3.Lerp(_from, _to, Mathf.SmoothStep(0f, 1f, k));
                // A slight bob, the gait of someone walking.
                _shape.transform.localPosition += Vector3.up * Mathf.Abs(Mathf.Sin(t * 7f)) * 0.02f;
                SetAlpha(_darkness * Mathf.Clamp01(Mathf.Min(k, 1f - k) * 5f));
                yield return null;
            }
            SetAlpha(0f);
            _playing = null;
        }

        private void SetAlpha(float alpha)
        {
            if (_shape == null) return;
            _shape.GetPropertyBlock(_block);
            _block.SetColor("_BaseColor", new Color(0.02f, 0.015f, 0.01f, alpha));
            _shape.SetPropertyBlock(_block);
            _shape.enabled = alpha > 0.001f;
        }
    }
}
