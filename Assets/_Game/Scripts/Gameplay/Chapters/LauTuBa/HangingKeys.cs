using ThuyKieu.Core.Story;
using UnityEngine;

namespace ThuyKieu.Gameplay.Chapters.LauTuBa
{
    /// <summary>
    /// Tú Bà's key ring hangs from a hook on her hip: it rides with the hip bone, but gravity, not the bone, decides which
    /// way the keys hang. Standing or walking they hang down beside her thigh (a little out, clear of her clothes); lying on
    /// her sập they fall out sideways and rest on the mat beside her hip (as steep as the hook's height over the mat
    /// allows, flat if it is low), instead of sticking out along her body or through the mat. The ring keeps its flat side
    /// against her, or on the mat. It swings into place rather than snapping (she turns, sits up, grabs).
    /// </summary>
    public class HangingKeys : MonoBehaviour
    {
        [SerializeField] private StoryActor _actor;
        [SerializeField] private Transform _hips;
        [Tooltip("Standing: how far out from straight down the keys hang (tan of the angle), to clear the cloth below the hip.")]
        [SerializeField] private float _standingTilt = 0.15f;

        private readonly RaycastHit[] _hits = new RaycastHit[8];
        private float _reach = -1f;
        private bool _wasLying;

        /// <summary>The direction the keys hang in now (world).</summary>
        public Vector3 Down => -transform.up;

        private void OnEnable() => Settle(1f);

        private void LateUpdate() => Settle(1f - Mathf.Exp(-10f * Time.deltaTime));

        private void Settle(float blend)
        {
            var anchor = transform.parent;
            if (anchor == null || _hips == null) return;
            Vector3 outward = Vector3.ProjectOnPlane(anchor.position - _hips.position, Vector3.up);
            if (outward.sqrMagnitude < 1e-6f) outward = anchor.right;
            outward.Normalize();
            Vector3 down;
            bool lying = _actor != null && _actor.IsLying;
            // Laid down: the keys are on the mat at once (a swing from standing would pass through it).
            if (lying && !_wasLying) blend = 1f;
            _wasLying = lying;
            if (lying)
            {
                // Lying: the keys reach down to the mat and no further (the sập's solid is under a 2 cm mattress).
                float drop = Mathf.Max(0f, HeightOverMat(anchor.position) - 0.028f);
                float cos = Mathf.Clamp01(drop / Mathf.Max(0.01f, Reach()));
                down = (Vector3.down * cos + outward * Mathf.Sqrt(1f - cos * cos)).normalized;
            }
            else down = (Vector3.down + outward * _standingTilt).normalized;
            Vector3 up = -down;
            Vector3 side = Vector3.ProjectOnPlane(outward, up);
            if (side.sqrMagnitude < 1e-4f) side = Vector3.ProjectOnPlane(Vector3.up, up);
            Vector3 forward = Vector3.Cross(side.normalized, up);
            if (forward.sqrMagnitude < 1e-6f) return;
            var target = Quaternion.LookRotation(forward, up);
            transform.localPosition = Vector3.zero;
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Mathf.Clamp01(blend));
        }

        /// <summary>From the hook to the tips of the keys (the hook and the bunch hanging in it, at rest).</summary>
        private float Reach()
        {
            if (_reach < 0f)
            {
                float lowest = 0f;
                foreach (var filter in GetComponentsInChildren<MeshFilter>(true))
                {
                    if (filter.sharedMesh == null) continue;
                    var b = filter.sharedMesh.bounds;
                    for (int i = 0; i < 8; i++)
                    {
                        var corner = new Vector3((i & 1) == 0 ? b.min.x : b.max.x, (i & 2) == 0 ? b.min.y : b.max.y, (i & 4) == 0 ? b.min.z : b.max.z);
                        // In the ring's own frame, as it hangs (its local down is the way the keys hang).
                        Vector3 local = filter.transform == transform ? corner : transform.InverseTransformPoint(filter.transform.TransformPoint(corner));
                        lowest = Mathf.Min(lowest, local.y);
                    }
                }
                _reach = lowest < -0.01f ? -lowest : 0.18f;
            }
            return _reach;
        }

        /// <summary>How high a point is over what lies under it (her sập), not counting her own body or triggers.</summary>
        private float HeightOverMat(Vector3 from)
        {
            int count = Physics.RaycastNonAlloc(from + Vector3.up * 0.05f, Vector3.down, _hits, 1.5f, ~0, QueryTriggerInteraction.Ignore);
            float best = 1.45f;
            for (int i = 0; i < count; i++)
            {
                if (_actor != null && _hits[i].collider.transform.IsChildOf(_actor.transform)) continue;
                best = Mathf.Min(best, _hits[i].distance - 0.05f);
            }
            return best;
        }
    }
}
