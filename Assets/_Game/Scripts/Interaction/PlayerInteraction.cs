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
        [Tooltip("Radius of the sphere cast, so the player does not need pixel perfect aim.")]
        [SerializeField] private float interactionRadius = 0.4f;
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
        private readonly RaycastHit[] _rayHits = new RaycastHit[12];

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
            int count = Physics.RaycastNonAlloc(origin, detectionOrigin.forward, _rayHits, interactionDistance, interactionLayers, QueryTriggerInteraction.Collide);
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
            // Opt-in range detection also works when standing beside an NPC.
            IInteractable nearest = null;
            float bestDistance = interactionDistance;
            foreach (Collider collider in Physics.OverlapSphere(transform.position, interactionDistance, interactionLayers, triggerInteraction))
            {
                var availability = collider.GetComponentInParent<IInteractionAvailability>();
                var candidate = collider.GetComponentInParent<IInteractable>();
                if (availability == null || !availability.IsAvailable || candidate == null) continue;
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                if (distance < bestDistance) { nearest = candidate; bestDistance = distance; }
            }
            if (nearest != null) return nearest;
            Vector3 origin = detectionOrigin.position + Vector3.up * detectionHeight;
            Vector3 direction = detectionOrigin.forward;

            if (!Physics.SphereCast(origin, interactionRadius, direction, out RaycastHit hit,
                    interactionDistance, interactionLayers, triggerInteraction))
            {
                return null;
            }

            // GetComponentInParent so a collider on a child of an NPC still resolves to the NPC.
            var available = hit.collider.GetComponentInParent<IInteractionAvailability>();
            if (available != null && !available.IsAvailable) return null;
            return hit.collider.GetComponentInParent<IInteractable>();
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
