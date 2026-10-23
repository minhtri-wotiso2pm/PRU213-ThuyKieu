using TMPro;
using UnityEngine;

namespace ThuyKieu.Gameplay.Stealth
{
    /// <summary>"?" over a guard's head while he is suspicious or searching, "!" when he is about to spot the player.</summary>
    public class AlertIcon : MonoBehaviour
    {
        [SerializeField] private VisionSensor _vision;
        [SerializeField] private GuardPatrol _patrol;
        [SerializeField] private TextMeshPro _text;

        private Camera _camera;

        private void Update()
        {
            if (_camera == null) _camera = Camera.main;
            float awareness = _vision != null && _vision.Active ? _vision.Awareness : 0f;
            bool searching = _patrol != null && _patrol.IsInvestigating;
            string symbol = awareness > 0.6f ? "!" : awareness > 0.2f || searching ? "?" : string.Empty;
            if (_text.text != symbol) _text.text = symbol;
            if (symbol.Length == 0) return;
            _text.color = symbol == "!" ? new Color(1f, 0.25f, 0.18f) : new Color(1f, 0.82f, 0.35f);
            float pulse = 1f + 0.12f * Mathf.Sin(Time.time * (symbol == "!" ? 14f : 6f));
            transform.localScale = Vector3.one * pulse;
            if (_camera != null) transform.rotation = Quaternion.LookRotation(transform.position - _camera.transform.position);
        }
    }
}
