using ThuyKieu.Core.Story;
using ThuyKieu.Player;
using UnityEngine;

namespace ThuyKieu.Presentation
{
    /// <summary>
    /// Builds dialogue shots from Ink camera ids and speakers.
    /// "CAM_&lt;id&gt;" scene anchors win; otherwise CU_&lt;actor&gt;, MCU_&lt;actor&gt;, OTS_&lt;from&gt;_&lt;to|Focus_to&gt;.
    /// Without a camera tag it alternates over-the-shoulder shots between Kiều and the speaker.
    /// </summary>
    public class DialogueCameraRig : MonoBehaviour
    {
        [SerializeField] private StoryDirector _director;
        [SerializeField] private ThirdPersonCameraController _camera;
        [SerializeField] private LayerMask _obstruction = 1 << 8;
        [Tooltip("Distance multiplier matching the dialogue lens: 1 for the gameplay FOV, ~1.5 for a 40° portrait lens.")]
        [SerializeField] private float _lens = 1f;

        private Transform _shot;
        private StoryActor _lastOther;
        private bool _active;

        private void Awake()
        {
            _shot = new GameObject("DialogueShot").transform;
            _shot.SetParent(transform, false);
        }

        public void Clear()
        {
            _active = false;
            _lastOther = null;
            _camera.SetDialogueCue(null);
        }

        public void Shot(string id)
        {
            var anchor = GameObject.Find("CAM_" + id);
            if (anchor != null) { Apply(anchor.transform.position, anchor.transform.rotation); return; }
            string[] parts = id.Split('_');
            if (parts.Length >= 2 && (parts[0] == "CU" || parts[0] == "MCU"))
            {
                var actor = _director.FindActor(parts[1]);
                if (actor != null) { CloseUp(actor, parts[0] == "CU" ? 1.15f : 1.7f); return; }
            }
            if (parts.Length >= 3 && parts[0] == "OTS")
            {
                var from = _director.FindActor(parts[1]);
                var toActor = _director.FindActor(parts[2]);
                var focus = toActor == null ? GameObject.Find("Focus_" + parts[2]) : null;
                if (from != null && (toActor != null || focus != null))
                {
                    if (toActor != null) _lastOther = toActor;
                    OverShoulder(from, toActor != null ? toActor.HeadPosition : focus.transform.position);
                    return;
                }
            }
            Auto(_director.Dialogue.CurrentSpeakerId);
        }

        public void Auto(string speaker)
        {
            var kieu = _director.FindActor("Kieu");
            if (kieu == null) return;
            switch (speaker)
            {
                case "NoiTam": case "KieuThat": case "TrangSach":
                    CloseUp(kieu, 1.55f);
                    return;
                case "Narrator":
                    if (!_active) Wide(kieu);
                    return;
                case "Kieu":
                    if (_lastOther != null && Vector3.Distance(_lastOther.transform.position, kieu.transform.position) < 7f)
                        OverShoulder(_lastOther, kieu.HeadPosition);
                    else CloseUp(kieu, 1.35f);
                    return;
            }
            var actor = _director.FindActor(speaker);
            if (actor == null) return;
            _lastOther = actor;
            if (Vector3.Distance(actor.transform.position, kieu.transform.position) < 7f) OverShoulder(kieu, actor.HeadPosition);
            else CloseUp(actor, 1.5f);
        }

        private void CloseUp(StoryActor actor, float distance)
        {
            Vector3 head = actor.HeadPosition;
            // Frame from the side of whoever they are talking to: actors may still be turning when the shot is chosen.
            var kieu = _director.FindActor("Kieu");
            var partner = actor == kieu ? _lastOther : kieu;
            Vector3 forward = actor.transform.forward;
            if (partner != null && Vector3.Distance(partner.transform.position, actor.transform.position) < 7f)
            {
                Vector3 toPartner = partner.transform.position - actor.transform.position;
                toPartner.y = 0;
                if (toPartner.sqrMagnitude > 0.01f) forward = toPartner.normalized;
            }
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            distance *= _lens;
            Vector3 position = head + forward * distance + right * 0.28f * _lens + Vector3.up * 0.04f;
            // Aim past the face towards the camera's side so the speaker sits on a third, looking into the open frame.
            Vector3 look = head - Vector3.up * (0.1f + 0.04f * distance) + right * 0.11f * distance;
            Apply(Unblock(look, position), Quaternion.LookRotation(look - position));
        }

        private void OverShoulder(StoryActor from, Vector3 target)
        {
            Vector3 head = from.HeadPosition;
            Vector3 direction = target - head;
            direction.y = 0;
            if (direction.sqrMagnitude < 0.01f) direction = from.transform.forward;
            direction.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, direction);
            Vector3 position = head - direction * 1.05f * _lens + right * 0.5f * Mathf.Sqrt(_lens) + Vector3.up * 0.12f;
            position = Unblock(head + Vector3.up * 0.1f, position);
            Apply(position, Quaternion.LookRotation(target - position));
        }

        private void Wide(StoryActor kieu)
        {
            Vector3 position = kieu.transform.position - kieu.transform.forward * 4.5f + Vector3.up * 2.6f;
            Vector3 look = kieu.transform.position + Vector3.up * 1.2f;
            position = Unblock(look, position);
            Apply(position, Quaternion.LookRotation(look - position));
        }

        private Vector3 Unblock(Vector3 from, Vector3 to)
        {
            Vector3 delta = to - from;
            float length = delta.magnitude;
            if (length > 0.01f && Physics.SphereCast(from, 0.12f, delta / length, out RaycastHit hit, length, _obstruction, QueryTriggerInteraction.Ignore))
                return from + delta / length * Mathf.Max(0.35f, hit.distance - 0.05f);
            return to;
        }

        private void Apply(Vector3 position, Quaternion rotation)
        {
            _shot.SetPositionAndRotation(position, rotation);
            _camera.SetDialogueCue(_shot);
            _active = true;
        }
    }
}
