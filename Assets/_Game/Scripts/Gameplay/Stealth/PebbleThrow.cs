using System;
using System.Collections;
using ThuyKieu.Core;
using ThuyKieu.Environment;
using ThuyKieu.Player;
using ThuyKieu.Presentation;
using UnityEngine;

namespace ThuyKieu.Gameplay.Stealth
{
    /// <summary>
    /// Q throws a pebble where the camera points. It flies a real arc and stops at the first solid thing in its way (a door,
    /// a pillar, the eaves, a wall): it never passes through to land behind it. Where it first strikes, the house hears it
    /// once (<see cref="Noise"/>, muffled by walls and rain like any sound), loud on stone, less on boards, little on a mat
    /// or earth; guards walk over to that spot, never to the thrower. It then bounces and rolls, each contact a recorded
    /// tap at that place, quieter as it slows; the bounces are heard by the player only, never counted as more sounds.
    /// </summary>
    public class PebbleThrow : MonoBehaviour
    {
        /// <summary>What it struck first, for the chapter and QA.</summary>
        public struct Impact
        {
            public Vector3 Point, Aim;
            public Surface Surface;
            public float Radius, Speed, Time;
            public Collider Collider;
            public int Contacts;
            /// <summary>It struck something short of where it was aimed (a pillar, a door, the eaves).</summary>
            public bool Blocked;
        }

        private const float Gravity = 9.81f, Radius = 0.03f;

        [SerializeField] private PlayerMovement _player;
        [SerializeField] private Camera _camera;
        [SerializeField] private StoryAudio _audio;
        [SerializeField] private float _maxDistance = 14f;
        [Tooltip("How far a pebble striking stone hard carries (boards and soft ground carry less, a light toss less again).")]
        [SerializeField] private float _hearingRadius = 11f;
        [SerializeField] private LayerMask _surfaces = 1 << 8;
        [SerializeField] private Material _pebbleMaterial;
        [Tooltip("How many she can carry at once.")]
        [SerializeField] private int _capacity = 3;

        private int _count;
        private float _cooldown;

        public int Count => _count;
        public int Capacity => _capacity;
        public bool Full => _count >= _capacity;
        public event Action<int> CountChanged;
        /// <summary>A thrown pebble struck something for the first time (once per pebble).</summary>
        public event Action<Impact> Landed;
        public Impact LastImpact { get; private set; }
        public int Thrown { get; private set; }

        public void SetCount(int count) { _count = Mathf.Max(0, count); CountChanged?.Invoke(_count); }

        /// <summary>Picks up pebbles up to what she can carry; returns how many she took.</summary>
        public int Refill(int count = int.MaxValue)
        {
            int taken = Mathf.Clamp(_capacity - _count, 0, count);
            if (taken > 0) SetCount(_count + taken);
            return taken;
        }

        private void Update()
        {
            if (_cooldown > 0f) _cooldown -= Time.deltaTime;
            if (_count <= 0 || _cooldown > 0f || _player.ControlLocked || !InputReader.ThrowPressedThisFrame) return;
            var ray = new Ray(_camera.transform.position, _camera.transform.forward);
            Vector3 aim = Physics.Raycast(ray, out RaycastHit hit, _maxDistance + 4f, _surfaces, QueryTriggerInteraction.Ignore)
                ? hit.point : ray.GetPoint(_maxDistance);
            _count--;
            _cooldown = 0.8f;
            Thrown++;
            CountChanged?.Invoke(_count);
            // From the throwing hand, a little in front of her face (from the eye itself if a wall is that close).
            Vector3 eye = _camera.transform.position;
            Vector3 hand = eye + _camera.transform.forward * 0.3f + _camera.transform.right * 0.15f - Vector3.up * 0.15f;
            if (Physics.Linecast(eye, hand, _surfaces, QueryTriggerInteraction.Ignore)) hand = eye;
            StartCoroutine(Fly(hand, aim));
        }

        /// <summary>QA only: the same flight from a given hand position towards a given point, without using up a pebble.</summary>
        public void ThrowForTest(Vector3 hand, Vector3 aim) { StartCoroutine(Fly(hand, aim)); }

        private IEnumerator Fly(Vector3 hand, Vector3 aim)
        {
            if (Vector3.Distance(hand, aim) > _maxDistance) aim = hand + (aim - hand).normalized * _maxDistance;
            // An arc that would pass through the aim point in T seconds if nothing were in the way.
            float flight = Mathf.Clamp(Vector3.Distance(hand, aim) / 12f, 0.25f, 1f);
            Vector3 velocity = (aim - hand) / flight + Vector3.up * (0.5f * Gravity * flight);

            var pebble = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(pebble.GetComponent<Collider>());
            pebble.transform.localScale = Vector3.one * Radius * 2f;
            if (_pebbleMaterial != null) pebble.GetComponent<Renderer>().sharedMaterial = _pebbleMaterial;
            pebble.transform.position = hand;
            if (_audio != null) _audio.PlaySfx("whoosh", 0.25f);

            Vector3 position = hand;
            float firstSpeed = 0f, age = 0f;
            int contacts = 0;
            while (age < 4f && position.y > -10f)
            {
                float dt = Mathf.Min(Time.deltaTime, 0.04f);
                age += dt;
                // Two half steps a frame: a fast pebble does not skip a thin leaf or a pillar.
                for (int half = 0; half < 2; half++)
                {
                    float step = dt * 0.5f;
                    Vector3 move = velocity * step + Vector3.down * (0.5f * Gravity * step * step);
                    velocity += Vector3.down * Gravity * step;
                    float length = move.magnitude;
                    if (length < 1e-5f) continue;
                    if (!Physics.SphereCast(position, Radius, move / length, out RaycastHit hit, length, _surfaces, QueryTriggerInteraction.Ignore))
                    {
                        position += move;
                        continue;
                    }
                    position = hit.point + hit.normal * (Radius + 0.004f);
                    float into = -Vector3.Dot(velocity, hit.normal);
                    if (into <= 0.05f) { velocity = Vector3.ProjectOnPlane(velocity, hit.normal); continue; }
                    var surface = SurfaceAt(hit);
                    if (contacts == 0)
                    {
                        firstSpeed = into;
                        Strike(hit, surface, into, aim, hand);
                    }
                    else if (into > 0.6f) Tap(hit.point, surface, Mathf.Clamp01(into / Mathf.Max(0.5f, firstSpeed)) * Loudness(into));
                    contacts++;
                    // The bounce: the speed into the surface mostly lost (stone keeps the most), some along it.
                    float keep = Hard(surface) ? 0.42f : Soft(surface) ? 0.1f : 0.32f;
                    Vector3 along = Vector3.ProjectOnPlane(velocity, hit.normal) * (Soft(surface) ? 0.4f : 0.7f);
                    velocity = along + hit.normal * into * keep;
                    if (hit.normal.y > 0.6f && velocity.magnitude < 0.7f) { velocity = Vector3.zero; break; }
                }
                pebble.transform.position = position;
                if (velocity == Vector3.zero || contacts >= 6) break;
                yield return null;
            }
            var impact = LastImpact;
            impact.Contacts = contacts;
            LastImpact = impact;
            Destroy(pebble, 20f);
        }

        /// <summary>The first contact: the one sound the house hears, at that spot, as loud as that floor and that throw make it.</summary>
        private void Strike(RaycastHit hit, Surface surface, float speed, Vector3 aim, Vector3 hand)
        {
            float force = Loudness(speed);
            float carry = Hard(surface) ? 1f : Soft(surface) ? 0.38f : 0.8f;
            float radius = _hearingRadius * carry * Mathf.Lerp(0.6f, 1f, force);
            // Heard from the side it struck: the point is nudged off the surface, out of the wall.
            Vector3 point = hit.point + hit.normal * 0.06f;
            var impact = new Impact
            {
                Point = hit.point, Aim = aim, Surface = surface, Radius = radius, Speed = speed, Time = UnityEngine.Time.time, Collider = hit.collider,
                Blocked = Vector3.Distance(hit.point, aim) > 0.6f && Vector3.Distance(hand, hit.point) < Vector3.Distance(hand, aim) - 0.4f
            };
            LastImpact = impact;
            if (_audio == null || !_audio.HasFoley(SoundFor(surface))) { if (_audio != null) _audio.PlayAt("clatter", hit.point, 0.55f, 20f); }
            else Tap(hit.point, surface, force);
            // Older chapters' guards are called directly; the first-person chapters' watchers listen through the noise system.
            foreach (var guard in FindObjectsByType<GuardPatrol>())
                if (Vector3.Distance(guard.transform.position, point) < radius) guard.Investigate(point, 5f);
            Noise.Emit(point, radius, NoiseKind.Thrown);
            Landed?.Invoke(impact);
        }

        private void Tap(Vector3 point, Surface surface, float loudness)
        {
            if (_audio == null || loudness < 0.04f) return;
            string id = SoundFor(surface);
            if (!_audio.HasFoley(id)) return;
            _audio.PlayAt(id, point, Mathf.Lerp(0.25f, 1f, loudness), 24f, UnityEngine.Random.Range(0.94f, 1.06f));
        }

        /// <summary>0 for a pebble barely dropped … 1 for a hard throw (about 5.5 m/s into the surface).</summary>
        private static float Loudness(float speed) => Mathf.Clamp01(speed / 5.5f);

        public static string SoundFor(Surface surface) => Hard(surface) ? "pebble_hard" : Soft(surface) ? "pebble_soft" : "pebble_wood";
        public static bool Hard(Surface surface) => surface == Surface.Stone || surface == Surface.WetStone;
        public static bool Soft(Surface surface) => surface == Surface.Earth || surface == Surface.Mat || surface == Surface.Mud;

        /// <summary>What it struck: a covering (rug, mat) over the floor, else the struck thing's own tag; untagged is wood (this house is timber).</summary>
        private static Surface SurfaceAt(RaycastHit hit)
        {
            if (hit.normal.y > 0.6f && SurfaceTag.PatchAt(hit.point, out Surface patch)) return patch;
            var tag = hit.collider.GetComponentInParent<SurfaceTag>();
            return tag != null ? tag.Surface : Surface.Wood;
        }
    }
}
