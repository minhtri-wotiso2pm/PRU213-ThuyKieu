using System.Collections;
using ThuyKieu.Core;
using ThuyKieu.Player;
using ThuyKieu.Presentation;
using UnityEngine;

namespace ThuyKieu.Gameplay.Stealth
{
    /// <summary>
    /// Q throws a pebble where the camera points. Where it lands, nearby guards hear it and walk over to look,
    /// pulling them away from the player's path.
    /// </summary>
    public class PebbleThrow : MonoBehaviour
    {
        [SerializeField] private PlayerMovement _player;
        [SerializeField] private Camera _camera;
        [SerializeField] private StoryAudio _audio;
        [SerializeField] private float _maxDistance = 14f;
        [SerializeField] private float _hearingRadius = 11f;
        [SerializeField] private LayerMask _surfaces = 1 << 8;
        [SerializeField] private Material _pebbleMaterial;

        private int _count;
        private float _cooldown;

        public int Count => _count;
        public event System.Action<int> CountChanged;

        public void SetCount(int count) { _count = count; CountChanged?.Invoke(_count); }

        private void Update()
        {
            if (_cooldown > 0f) _cooldown -= Time.deltaTime;
            if (_count <= 0 || _cooldown > 0f || _player.ControlLocked || !InputReader.ThrowPressedThisFrame) return;
            var ray = new Ray(_camera.transform.position, _camera.transform.forward);
            Vector3 target = Physics.Raycast(ray, out RaycastHit hit, _maxDistance + 4f, _surfaces, QueryTriggerInteraction.Ignore)
                ? hit.point : ray.GetPoint(_maxDistance);
            Vector3 hand = _player.transform.position + Vector3.up * 1.4f;
            if (Vector3.Distance(hand, target) > _maxDistance) target = hand + (target - hand).normalized * _maxDistance;
            _count--;
            _cooldown = 0.8f;
            CountChanged?.Invoke(_count);
            StartCoroutine(Fly(hand, target));
        }

        private IEnumerator Fly(Vector3 from, Vector3 to)
        {
            var pebble = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(pebble.GetComponent<Collider>());
            pebble.transform.localScale = Vector3.one * 0.07f;
            if (_pebbleMaterial != null) pebble.GetComponent<Renderer>().sharedMaterial = _pebbleMaterial;
            if (_audio != null) _audio.PlaySfx("whoosh", 0.4f);
            float duration = Mathf.Clamp(Vector3.Distance(from, to) / 14f, 0.25f, 0.9f);
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float k = t / duration;
                pebble.transform.position = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 1.2f;
                yield return null;
            }
            pebble.transform.position = to;
            if (_audio != null) _audio.PlaySfx("clatter", 0.55f);
            foreach (var guard in FindObjectsByType<GuardPatrol>())
                if (Vector3.Distance(guard.transform.position, to) < _hearingRadius) guard.Investigate(to, 5f);
            Destroy(pebble, 4f);
        }
    }
}
