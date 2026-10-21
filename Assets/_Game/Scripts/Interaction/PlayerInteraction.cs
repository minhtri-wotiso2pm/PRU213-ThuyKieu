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

        /// <summary>The interactable currently targeted, or null. UI can poll this for a prompt.</summary>
        public IInteractable CurrentTarget { get; private set; }
        public bool ControlLocked { get; set; }

        private void Awake()
        {
            if (detectionOrigin == null)
            {
                detectionOrigin = transform;
            }
        }

        private void Update()
        {
            if (ControlLocked) { CurrentTarget = null; return; }
            CurrentTarget = FindInteractable();

            if (CurrentTarget != null && InputReader.InteractPressedThisFrame)
            {
                CurrentTarget.Interact();
            }
        }

        private IInteractable FindInteractable()
        {
            Vector3 origin = detectionOrigin.position + Vector3.up * detectionHeight;
            Vector3 forward = Vector3.ProjectOnPlane(detectionOrigin.forward, Vector3.up).normalized;
            IInteractable nearest = null;
            float bestDistance = interactionDistance;
            foreach (Collider collider in Physics.OverlapSphere(transform.position, interactionDistance, interactionLayers, triggerInteraction))
            {
                var availability = collider.GetComponentInParent<IInteractionAvailability>();
                var candidate = collider.GetComponentInParent<IInteractable>();
                if (candidate == null || (availability != null && !availability.IsAvailable)) continue;

                var explore = collider.GetComponentInParent<ExploreTarget>();
                Vector3 nearestPoint = explore != null ? explore.InteractionPosition : collider.transform.position;
                float distance = Vector3.ProjectOnPlane(nearestPoint - transform.position, Vector3.up).magnitude;
                if (explore != null && explore.HasStandPoint && distance > ExploreTarget.StandPointRange) continue;
                if (distance > interactionDistance || !Faces(collider.bounds.center, origin, forward)) continue;
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

        /// <summary>The first solid thing between Kiều and a target must be the target itself, never a wall.</summary>
        private bool HasLineOfSight(Vector3 origin, Collider targetCollider, IInteractable target)
        {
            // Aim at the upper half of the target volume (chest height for a person): the centre of someone seated
            // behind a table lies below its top, so the table read as a wall between Kiều and them.
            Bounds bounds = targetCollider.bounds;
            Vector3 destination = bounds.center + Vector3.up * bounds.extents.y * 0.5f;
            Vector3 delta = destination - origin;
            float distance = delta.magnitude;
            if (distance < 0.05f) return true;

            RaycastHit[] hits = Physics.RaycastAll(origin, delta / distance, distance + 0.05f,
                interactionLayers, triggerInteraction);
            float targetDistance = float.MaxValue;
            float blockerDistance = float.MaxValue;
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.transform.IsChildOf(transform)) continue;
                var hitTarget = hit.collider.GetComponentInParent<IInteractable>();
                if (ReferenceEquals(hitTarget, target)) targetDistance = Mathf.Min(targetDistance, hit.distance);
                else if (!hit.collider.isTrigger) blockerDistance = Mathf.Min(blockerDistance, hit.distance);
            }
            // Trigger volumes are valid targets but never blockers. A solid wall closer than the target rejects it.
            return targetDistance < float.MaxValue && targetDistance <= blockerDistance + 0.02f;
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
