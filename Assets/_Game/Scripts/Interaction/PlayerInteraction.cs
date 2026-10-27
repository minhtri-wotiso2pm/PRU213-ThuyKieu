using ThuyKieu.Core;

using UnityEngine;

namespace ThuyKieu.Interaction
{
    /// <summary>
    /// Finds the best IInteractable in front of the player and triggers it.
    /// This class knows nothing about what an interactable does (dialogue, pickup, door...).
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private float interactionDistance = 2.5f;
        [Tooltip("Within this radius, a target may be used without an exact facing check.")]
        [SerializeField] private float interactionRadius = 0.4f;
        [Tooltip("How far to either side an object may be and still show its E prompt.")]
        [Range(-1f, 1f)] [SerializeField] private float minimumFacingDot = 0.1f;
        [Tooltip("Which layers can be interacted with. Default: everything.")]
        [SerializeField] private LayerMask interactionLayers = ~0;
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;

        [Header("References")]
        [Tooltip("Origin and direction of the detection cast. Leave empty to use this transform.")]
        [SerializeField] private Transform detectionOrigin;
        [Tooltip("Vertical offset from the origin, so the cast leaves the feet of the character.")]
        [SerializeField] private float detectionHeight = 1f;
        [Tooltip("First person: only what the eyes aim at counts (a ray from the camera), never whatever stands nearest.")]
        [SerializeField] private bool _lookOnly;

        /// <summary>The interactable currently targeted, or null. UI can poll this for a prompt.</summary>
        public IInteractable CurrentTarget { get; private set; }
        public bool ControlLocked { get; set; }
        /// <summary>0–1 progress of a hold interaction under the reticle, negative when none.</summary>
        public float HoldProgress { get; private set; } = -1f;

        /// <summary>
        /// Wins over whatever is aimed at while set: inside a wardrobe the eyes are within its volume, so a ray cannot
        /// find it, yet E must still lead out.
        /// </summary>
        public static IInteractable Pinned { get; set; }

        private IHoldInteractable _holding;
        private RaycastHit[] _rayHits = new RaycastHit[16];
        private Collider[] _nearby = new Collider[32];

        private void Awake()
        {
            if (detectionOrigin == null)
            {
                detectionOrigin = transform;
            }
        }

        private void Update()
        {
            var target = ControlLocked ? null : Pinned is MonoBehaviour pinned && pinned != null ? Pinned : _lookOnly ? AimedInteractable() : FindInteractable();
            if (_holding != null && !ReferenceEquals(_holding, target)) { _holding.Lost(); _holding = null; }
            CurrentTarget = target;
            HoldProgress = -1f;
            if (target is IHoldInteractable hold)
            {
                _holding = hold;
                HoldProgress = hold.Tick(InputReader.InteractPressedThisFrame, InputReader.IsInteractHeld);
                return;
            }

            if (CurrentTarget != null && InputReader.InteractPressedThisFrame)
            {
                CurrentTarget.Interact();
            }
        }

        /// <summary>
        /// The first available interactable along the line of sight. Big trigger volumes (zones, explore anchors) are passed
        /// through; a solid surface without an interaction stops the search, so nothing is used through a wall.
        /// </summary>
        private IInteractable AimedInteractable()
        {
            Vector3 origin = detectionOrigin.position + Vector3.up * detectionHeight;
            int count = Raycast(origin, detectionOrigin.forward, interactionDistance, interactionLayers, QueryTriggerInteraction.Collide);
            System.Array.Sort(_rayHits, 0, count, HitDistance.Instance);
            for (int i = 0; i < count; i++)
            {
                var collider = _rayHits[i].collider;
                // Her own body: crouched and looking down, the eyes sit just above her own capsule (taking the keys from a
                // sleeper's belt, something on the floor) and the ray would stop on it.
                if (collider.transform.IsChildOf(transform)) continue;
                var candidate = collider.GetComponentInParent<IInteractable>();
                var availability = collider.GetComponentInParent<IInteractionAvailability>();
                bool available = candidate != null && (availability == null || availability.IsAvailable);
                if (available) return candidate;
                if (!collider.isTrigger) return null;
            }
            return null;
        }

        private sealed class HitDistance : System.Collections.Generic.IComparer<RaycastHit>
        {
            public static readonly HitDistance Instance = new HitDistance();
            public int Compare(RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance);
        }

        private IInteractable FindInteractable()
        {
            Vector3 origin = detectionOrigin.position + Vector3.up * detectionHeight;
            Vector3 forward = Vector3.ProjectOnPlane(detectionOrigin.forward, Vector3.up).normalized;
            IInteractable nearest = null;
            float bestDistance = float.MaxValue;
            // Wide enough to catch a target whose stand point is in reach while its body is a little farther away.
            float searchRadius = Mathf.Max(interactionDistance, ExploreTarget.StandPointRange) + 1.5f;
            int count;
            while ((count = Physics.OverlapSphereNonAlloc(transform.position, searchRadius, _nearby, interactionLayers, triggerInteraction)) == _nearby.Length)
                System.Array.Resize(ref _nearby, _nearby.Length * 2);
            for (int i = 0; i < count; i++)
            {
                Collider collider = _nearby[i];
                var availability = collider.GetComponentInParent<IInteractionAvailability>();
                var candidate = collider.GetComponentInParent<IInteractable>();
                if (candidate == null || (availability != null && !availability.IsAvailable)) continue;

                var explore = collider.GetComponentInParent<ExploreTarget>();
                float distance;
                if (explore != null && explore.HasStandPoint)
                {
                    distance = Vector3.ProjectOnPlane(explore.InteractionPosition - transform.position, Vector3.up).magnitude;
                    // Close to the stand point it always answers (her own desk: she is seated across it). Farther out
                    // it needs a clear way there, so it never moves her through a wall or a row of desks.
                    if (distance > ExploreTarget.StandPointRange
                        || distance > ExploreTarget.StandPointNearRange && !PathClear(explore.InteractionPosition, collider)) continue;
                }
                else
                {
                    // Measure to the object's edge, not its pivot, so large props answer before Kiều is pressed against them.
                    Vector3 edge = collider.bounds.ClosestPoint(transform.position);
                    distance = Vector3.ProjectOnPlane(edge - transform.position, Vector3.up).magnitude;
                    if (distance > interactionDistance) continue;
                }
                if (!Faces(collider.bounds.center, origin, forward)) continue;
                if (!HasLineOfSight(origin, collider, candidate)) continue;
                if (distance < bestDistance) { nearest = candidate; bestDistance = distance; }
            }
            return nearest;
        }

        private bool Faces(Vector3 point, Vector3 origin, Vector3 forward)
        {
            Vector3 direction = Vector3.ProjectOnPlane(point - origin, Vector3.up);
            return direction.sqrMagnitude < interactionRadius * interactionRadius
                   || Vector3.Dot(forward, direction.normalized) >= minimumFacingDot;
        }

        /// <summary>
        /// Kiều is moved to a target's stand point when she uses it, so nothing solid (a desk, a wall) may lie between her
        /// and that point at hip height; otherwise the prompt would teleport her through it.
        /// </summary>
        private bool PathClear(Vector3 standPoint, Collider target)
        {
            Vector3 from = transform.position + Vector3.up * 0.5f;
            Vector3 delta = standPoint + Vector3.up * 0.5f - from;
            float distance = delta.magnitude;
            if (distance < 0.05f) return true;
            int count = Raycast(from, delta / distance, distance, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = _rayHits[i];
                if (hit.collider.transform.IsChildOf(transform) || hit.collider == target) continue;
                return false;
            }
            return true;
        }

        /// <summary>The first solid thing between Kiều and a target must be the target itself, never a wall.</summary>
        private bool HasLineOfSight(Vector3 origin, Collider targetCollider, IInteractable target)
        {
            // Aim at the upper half of the target volume (chest height for a person): the centre of someone seated
            // behind a table lies below its top, so the table read as a wall between Kiều and them.
            Bounds bounds = targetCollider.bounds;
            // Standing inside the target's own volume: a ray cast from inside a collider never hits it, so this would
            // otherwise hide the prompt exactly when she is right at the thing.
            if (bounds.Contains(origin)) return true;
            Vector3 destination = bounds.center + Vector3.up * bounds.extents.y * 0.5f;
            Vector3 delta = destination - origin;
            float distance = delta.magnitude;
            if (distance < 0.05f) return true;

            int count = Raycast(origin, delta / distance, distance + 0.05f,
                interactionLayers, triggerInteraction);
            float targetDistance = float.MaxValue;
            float blockerDistance = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = _rayHits[i];
                if (hit.collider.transform.IsChildOf(transform)) continue;
                var hitTarget = hit.collider.GetComponentInParent<IInteractable>();
                if (ReferenceEquals(hitTarget, target)) targetDistance = Mathf.Min(targetDistance, hit.distance);
                else if (!hit.collider.isTrigger) blockerDistance = Mathf.Min(blockerDistance, hit.distance);
            }
            // Trigger volumes are valid targets but never blockers. A solid wall closer than the target rejects it.
            return targetDistance < float.MaxValue && targetDistance <= blockerDistance + 0.02f;
        }

        // Grow on saturation so a busy scene cannot silently drop a nearby target or an occluding wall.
        // After warming up, the physics queries reuse their buffers instead of allocating every frame.
        private int Raycast(Vector3 origin, Vector3 direction, float distance, int layers, QueryTriggerInteraction triggers)
        {
            int count;
            while ((count = Physics.RaycastNonAlloc(origin, direction, _rayHits, distance, layers, triggers)) == _rayHits.Length)
                System.Array.Resize(ref _rayHits, _rayHits.Length * 2);
            return count;
        }

        private void OnDrawGizmosSelected()
        {
            Transform origin = detectionOrigin != null ? detectionOrigin : transform;
            Vector3 start = origin.position + Vector3.up * detectionHeight;
            Vector3 end = start + origin.forward * interactionDistance;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawWireSphere(end, interactionRadius);
        }
    }
}
