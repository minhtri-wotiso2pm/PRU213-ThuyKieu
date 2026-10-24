using ThuyKieu.Core.Story;
using ThuyKieu.Player;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThuyKieu.Presentation
{
    /// <summary>
    /// Builds dialogue shots from Ink camera ids and speakers.
    /// "CAM_&lt;id&gt;" scene anchors win; otherwise CU_&lt;actor&gt;, MCU_&lt;actor&gt;, OTS_&lt;from&gt;_&lt;to|Focus_to&gt;.
    /// Without a camera tag it alternates over-the-shoulder shots between Kiều and the speaker;
    /// inner thoughts after an authored CAM_ shot stay on it.
    /// </summary>
    public class DialogueCameraRig : MonoBehaviour
    {
        [SerializeField] private StoryDirector _director;
        [SerializeField] private ThirdPersonCameraController _camera;
        [SerializeField] private LayerMask _obstruction = 1 << 8;
        [Tooltip("Distance multiplier matching the dialogue lens: 1 for the gameplay FOV, ~1.5 for a 40° portrait lens.")]
        [SerializeField] private float _lens = 1f;
        [Tooltip("Soft key light riding with dialogue shots so faces read in dark scenes (0 = none).")]
        [SerializeField] private float _keyLight;
        [SerializeField] private Color _keyColor = new Color(1f, 0.82f, 0.66f);

        private Transform _shot;
        private Light _key;
        private StoryActor _lastOther;
        private bool _active;
        private bool _onAnchor;
        private readonly List<(Renderer renderer, ShadowCastingMode mode)> _hidden = new List<(Renderer, ShadowCastingMode)>();

        private void Awake()
        {
            _shot = new GameObject("DialogueShot").transform;
            _shot.SetParent(transform, false);
            if (_keyLight <= 0f) return;
            // Rides with the camera, a little above and to the side, like a bounce card on set.
            var key = new GameObject("DialogueKeyLight");
            key.transform.SetParent(_camera.transform, false);
            key.transform.localPosition = new Vector3(0.6f, 0.45f, 0f);
            key.transform.localRotation = Quaternion.Euler(6f, -8f, 0f);
            _key = key.AddComponent<Light>();
            _key.type = LightType.Spot;
            _key.spotAngle = 55f;
            _key.innerSpotAngle = 20f;
            _key.range = 5f;
            _key.intensity = _keyLight;
            _key.color = _keyColor;
            _key.shadows = LightShadows.None;
            _key.enabled = false;
        }

        public void Clear()
        {
            _active = false;
            _onAnchor = false;
            _lastOther = null;
            _camera.SetDialogueCue(null);
            Restore();
            if (_key != null) _key.enabled = false;
        }

        private string _lastShot;

        /// <summary>Takes the current line's shot again, after staging has moved the people in it (a fade-and-place).</summary>
        public void Reshoot()
        {
            if (!_director.Dialogue.IsDialogueActive) return;
            if (_lastShot != null) Shot(_lastShot);
            else Auto(_director.Dialogue.CurrentSpeakerId);
        }

        public void Shot(string id)
        {
            _lastShot = id;
            var anchor = GameObject.Find("CAM_" + id);
            _onAnchor = anchor != null;
            if (anchor != null) { Apply(anchor.transform.position, anchor.transform.rotation); return; }
            string[] parts = id.Split('_');
            if (parts.Length >= 2 && (parts[0] == "CU" || parts[0] == "MCU"))
            {
                var actor = _director.FindActor(parts[1]);
                if (actor != null && actor.isActiveAndEnabled) { CloseUp(actor, parts[0] == "CU" ? 1.15f : 1.7f); return; }
            }
            if (parts.Length >= 3 && parts[0] == "OTS")
            {
                var from = _director.FindActor(parts[1]);
                var toActor = _director.FindActor(parts[2]);
                var focus = toActor == null ? GameObject.Find("Focus_" + parts[2]) : null;
                if (from != null && (toActor != null || focus != null))
                {
                    if (toActor != null) _lastOther = toActor;
                    OverShoulder(from, toActor != null ? toActor.HeadPosition : focus.transform.position, toActor);
                    return;
                }
            }
            Auto(_director.Dialogue.CurrentSpeakerId);
        }

        public void Auto(string speaker)
        {
            _lastShot = null;
            var kieu = _director.FindActor("Kieu");
            if (kieu == null) return;
            // Kiều's thoughts about what she is looking at (the hồ cầm, the mirror): a close-up of her face
            // would swing round in front of her, often into the wall she faces.
            if (speaker == "NoiTam" && _onAnchor) return;
            _onAnchor = false;
            switch (speaker)
            {
                case "NoiTam": case "KieuThat": case "TrangSach":
                    CloseUp(kieu, 1.55f);
                    return;
                case "Narrator":
                    if (!_active) Wide(kieu);
                    return;
                case "Kieu": case "HocSinh":
                    if (_lastOther != null && _lastOther.isActiveAndEnabled && Vector3.Distance(_lastOther.transform.position, kieu.transform.position) < OverShoulderRange)
                        OverShoulder(_lastOther, kieu.HeadPosition, kieu);
                    else CloseUp(kieu, 1.35f);
                    return;
            }
            var actor = _director.FindActor(speaker);
            if (actor == null) return;
            // A voice from someone not in the scene, or calling from beyond a wall: hold on Kiều listening rather than
            // frame an empty spot or fly the camera out through the room.
            if (!actor.isActiveAndEnabled || WallBetween(kieu, actor)) { CloseUp(kieu, 1.45f); return; }
            _lastOther = actor;
            // Over the shoulder only at talking distance: across a room the partner shrinks to a speck, so cut in close.
            if (Vector3.Distance(actor.transform.position, kieu.transform.position) < OverShoulderRange) OverShoulder(kieu, actor.HeadPosition, actor);
            else CloseUp(actor, 1.5f);
        }

        private void CloseUp(StoryActor actor, float distance)
        {
            Vector3 head = actor.HeadPosition;
            // Frame from the side of whoever they are talking to: actors may still be turning when the shot is chosen.
            var kieu = _director.FindActor("Kieu");
            var partner = actor == kieu ? _lastOther : kieu;
            Vector3 forward = actor.transform.forward;
            // Seated people are framed from the front, across their desk; from the partner's side the lens sits
            // beside the bench and looks along their legs.
            if (partner != null && !actor.IsSitting && Vector3.Distance(partner.transform.position, actor.transform.position) < 7f
                && !WallBetween(actor, partner))
            {
                Vector3 toPartner = partner.transform.position - actor.transform.position;
                toPartner.y = 0;
                if (toPartner.sqrMagnitude > 0.01f) forward = toPartner.normalized;
            }
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            distance *= _lens;
            Vector3 position = head + forward * distance + right * 0.28f * _lens + Vector3.up * (0.04f + SeatedRise(actor));
            // Aim past the face towards the camera's side so the speaker sits on a third, looking into the open frame.
            Vector3 look = head - Vector3.up * (0.1f + 0.04f * distance) + right * 0.11f * distance;
            Compose(head, position, look, new[] { actor }, new[] { head, head - Vector3.up * 0.35f });
        }

        private void OverShoulder(StoryActor from, Vector3 target, StoryActor targetActor = null)
        {
            Vector3 head = from.HeadPosition;
            Vector3 direction = target - head;
            direction.y = 0;
            if (direction.sqrMagnitude < 0.01f) direction = from.transform.forward;
            direction.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, direction);
            float rise = Mathf.Max(SeatedRise(from), targetActor != null ? SeatedRise(targetActor) : 0f);
            Vector3 position = head - direction * 1.05f * _lens + right * 0.5f * Mathf.Sqrt(_lens) + Vector3.up * (0.12f + rise);
            // Turning around the person being looked at keeps them in frame if the shoulder shot is blocked.
            var subjects = targetActor != null ? new[] { from, targetActor } : new[] { from };
            Compose(target, position, target, subjects, new[] { target, target - Vector3.up * 0.35f }, head + Vector3.up * 0.1f);
        }

        /// <summary>
        /// Seated people are framed from above eye level, looking down across the desk: at a seated head's height the
        /// bottom of the frame falls on the laps of whoever sits in front of the lens.
        /// </summary>
        private static float SeatedRise(StoryActor actor) => actor != null && actor.IsSitting ? 0.45f : 0f;

        private void Wide(StoryActor kieu)
        {
            Vector3 position = kieu.transform.position - kieu.transform.forward * 4.5f + Vector3.up * 2.6f;
            Vector3 look = kieu.transform.position + Vector3.up * 1.2f;
            Compose(look, position, look, new[] { kieu }, new[] { kieu.HeadPosition, look });
        }

        // ------------------------------------------------------------------ shot composition

        private static readonly float[] Orbit = { 0f, 28f, -28f, 55f, -55f, 85f, -85f, 120f, -120f };
        private static readonly float[] NoOrbit = { 0f };
        private const float OverShoulderRange = 4.2f;

        /// <summary>
        /// Places the camera for a shot designed at <paramref name="position"/>: if a wall or a bystander stands between
        /// the lens and the people in it, the shot orbits around <paramref name="pivot"/> to the nearest clear angle.
        /// Whoever still blocks the best angle (or stands right in front of the lens) is hidden for the shot, the way
        /// cutscenes fade a passer-by out of frame.
        /// </summary>
        private void Compose(Vector3 pivot, Vector3 position, Vector3 look, StoryActor[] subjects, Vector3[] targets, Vector3? wallFrom = null)
        {
            Restore();
            var others = Bystanders(subjects);
            var screens = Screens(pivot, subjects);
            Vector3 bestPosition = position, bestLook = look;
            float bestScore = float.MaxValue;
            float desired = Vector3.Distance(pivot, position);
            // Orbiting only makes sense for a close framing; a long lens swung round a far pivot lands anywhere.
            foreach (float yaw in desired > 3f ? NoOrbit : Orbit)
            {
                var turn = Quaternion.AngleAxis(yaw, Vector3.up);
                Vector3 p = pivot + turn * (position - pivot);
                Vector3 l = pivot + turn * (look - pivot);
                Vector3 placed = Unblock(yaw == 0f && wallFrom.HasValue ? wallFrom.Value : pivot, p);
                float score = Mathf.Abs(yaw) / 120f;
                // Pulled in by a wall: tolerable a little, a lens jammed into a face is not.
                float kept = Vector3.Distance(pivot, placed) / Mathf.Max(0.01f, desired);
                if (kept < 0.75f) score += (0.75f - kept) * 8f;
                if (Physics.CheckSphere(placed, 0.1f, _obstruction, QueryTriggerInteraction.Ignore)) score += 20f;
                score += Blockers(placed, targets, others, null) * 6f;
                score += Screened(placed, targets, screens) * 10f;
                if (score < bestScore) { bestScore = score; bestPosition = placed; bestLook = l; }
                if (score < 0.05f) break;
            }
            var hide = new List<StoryActor>();
            Blockers(bestPosition, targets, others, hide);
            foreach (var actor in hide) if (CanHide(actor)) Hide(actor);
            Apply(bestPosition, Quaternion.LookRotation(bestLook - bestPosition), false);
        }

        /// <summary>
        /// Principal members of the Vương-family arrest must remain visible. Losing one of them between dialogue cuts
        /// reads as a continuity error, even when that actor happens to stand across the chosen sight line.
        /// </summary>
        private static bool CanHide(StoryActor actor)
        {
            if (actor == null) return false;
            switch (actor.SpeakerId)
            {
                case "VuongOng":
                case "VuongQuan":
                case "MeKieu":
                case "QuanSai":
                    return false;
                default:
                    return true;
            }
        }

        private List<StoryActor> Bystanders(StoryActor[] subjects)
        {
            var list = new List<StoryActor>();
            foreach (var actor in FindObjectsByType<StoryActor>())
                if (actor.isActiveAndEnabled && System.Array.IndexOf(subjects, actor) < 0) list.Add(actor);
            return list;
        }

        /// <summary>
        /// Visible scenery near the shot that has no collider (curtains, hangings, bed drapes): the wall checks cannot
        /// see it, so a lens placed behind a doorway curtain would film the cloth.
        /// </summary>
        private static List<Bounds> Screens(Vector3 pivot, StoryActor[] subjects)
        {
            var screens = new List<Bounds>();
            foreach (var renderer in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if (!renderer.enabled || renderer is ParticleSystemRenderer || renderer.GetComponentInParent<StoryActor>() != null) continue;
                if (renderer.GetComponent<Collider>() != null) continue;
                Bounds bounds = renderer.bounds;
                // Only tall cloth-like pieces screen a shot; books and bottles on a desk sit below the faces framed.
                if (bounds.size.y < 0.6f || bounds.size.magnitude > 6f || bounds.SqrDistance(pivot) > 25f) continue;
                screens.Add(bounds);
            }
            return screens;
        }

        /// <summary>Counts the sight lines from the lens to the targets that pass through one of the screens.</summary>
        private static int Screened(Vector3 lens, Vector3[] targets, List<Bounds> screens)
        {
            int count = 0;
            foreach (var target in targets)
            {
                Vector3 delta = target - lens;
                float length = delta.magnitude;
                if (length < 0.3f) continue;
                var ray = new Ray(lens, delta / length);
                foreach (var bounds in screens)
                {
                    // Stop short of the target, as for bystanders: props held or worn by the subject are in the picture.
                    if (bounds.Contains(target) || !bounds.IntersectRay(ray, out float hit) || hit > length - 0.3f) continue;
                    count++;
                    break;
                }
            }
            return count;
        }

        /// <summary>Counts the bystanders standing on the sight lines from the lens to the targets, at the lens or just in front of it.</summary>
        private static int Blockers(Vector3 lens, Vector3[] targets, List<StoryActor> others, List<StoryActor> found)
        {
            int count = 0;
            foreach (var actor in others)
            {
                Vector3 feet = actor.transform.position;
                Vector3 crown = actor.HeadPosition + Vector3.up * 0.12f;
                Vector3 hips = new Vector3(crown.x, Mathf.Min(feet.y + 0.35f, crown.y), crown.z);
                bool blocks = SegmentDistance(lens, lens, hips, crown) < 0.5f;
                // A head looming just in front of the lens fills the foreground even off the sight lines. It is only
                // hidden (found != null), never scored: scoring it swung seated shots round to worse angles.
                if (!blocks && found != null && targets.Length > 0 && Vector3.Distance(lens, crown) < 0.9f && Vector3.Dot(crown - lens, targets[0] - lens) > 0f)
                    blocks = true;
                foreach (var target in targets)
                {
                    if (blocks) break;
                    Vector3 delta = target - lens;
                    float length = delta.magnitude;
                    if (length < 0.3f) continue;
                    // Stop short of the target: someone standing right beside the subject is part of the picture.
                    Vector3 end = lens + delta * ((length - 0.3f) / length);
                    blocks = SegmentDistance(lens, end, hips, crown) < 0.26f;
                }
                if (!blocks) continue;
                count++;
                found?.Add(actor);
            }
            return count;
        }

        /// <summary>Closest distance between segments p1-q1 and p2-q2.</summary>
        private static float SegmentDistance(Vector3 p1, Vector3 q1, Vector3 p2, Vector3 q2)
        {
            Vector3 d1 = q1 - p1, d2 = q2 - p2, r = p1 - p2;
            float a = Vector3.Dot(d1, d1), e = Vector3.Dot(d2, d2), f = Vector3.Dot(d2, r);
            float s, t;
            if (a <= 1e-6f && e <= 1e-6f) return r.magnitude;
            if (a <= 1e-6f) { s = 0f; t = Mathf.Clamp01(f / e); }
            else
            {
                float c = Vector3.Dot(d1, r);
                if (e <= 1e-6f) { t = 0f; s = Mathf.Clamp01(-c / a); }
                else
                {
                    float b = Vector3.Dot(d1, d2), denom = a * e - b * b;
                    s = denom > 1e-6f ? Mathf.Clamp01((b * f - c * e) / denom) : 0f;
                    t = (b * s + f) / e;
                    if (t < 0f) { t = 0f; s = Mathf.Clamp01(-c / a); }
                    else if (t > 1f) { t = 1f; s = Mathf.Clamp01((b - c) / a); }
                }
            }
            return ((p1 + d1 * s) - (p2 + d2 * t)).magnitude;
        }

        /// <summary>Takes an actor out of the frame for one shot; its shadow stays so the scene still reads as occupied.</summary>
        public void Hide(StoryActor actor)
        {
            foreach (var renderer in actor.GetComponentsInChildren<Renderer>())
            {
                if (!renderer.enabled || renderer.shadowCastingMode == ShadowCastingMode.ShadowsOnly) continue;
                _hidden.Add((renderer, renderer.shadowCastingMode));
                renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }
        }

        private void Restore()
        {
            foreach (var (renderer, mode) in _hidden) if (renderer != null) renderer.shadowCastingMode = mode;
            _hidden.Clear();
        }

        private bool WallBetween(StoryActor a, StoryActor b) =>
            Physics.Linecast(a.HeadPosition, b.HeadPosition, _obstruction, QueryTriggerInteraction.Ignore);

        private Vector3 Unblock(Vector3 from, Vector3 to)
        {
            Vector3 delta = to - from;
            float length = delta.magnitude;
            if (length > 0.01f && Physics.SphereCast(from, 0.12f, delta / length, out RaycastHit hit, length, _obstruction, QueryTriggerInteraction.Ignore))
                // Keep enough room for the near clip plane; five centimetres still let wall faces fill the frame.
                return from + delta / length * Mathf.Max(0.05f, hit.distance - 0.18f);
            return to;
        }

        private void Apply(Vector3 position, Quaternion rotation, bool authored = true)
        {
            if (authored)
            {
                // Scene-authored anchors used to bypass Compose entirely, so an anchor on the wrong side of a wall
                // could look straight through it. Pull the lens onto the subject's side before using the shot.
                var speaker = _director.FindActor(_director.Dialogue.CurrentSpeakerId);
                var focusActor = speaker != null ? speaker : _director.FindActor("Kieu");
                Vector3 focus = focusActor != null ? focusActor.HeadPosition : Vector3.zero;
                // Only use a real person that the authored lens is actually facing. Guessing a point four metres ahead
                // can put that point beyond the very wall we are trying not to cross.
                bool hasFocus = focusActor != null && Vector3.Dot(focus - position, rotation * Vector3.forward) > 0f;
                Vector3 safePosition = hasFocus ? Unblock(focus, position) : position;
                if (hasFocus && (safePosition - position).sqrMagnitude > 0.0001f)
                {
                    position = safePosition;
                    Vector3 look = focus - position;
                    if (look.sqrMagnitude > 0.001f) rotation = Quaternion.LookRotation(look);
                }
                // Authored anchors: keep whoever is talking visible and nobody's back pressed against the lens.
                Restore();
                var others = Bystanders(speaker != null ? new[] { speaker } : new StoryActor[0]);
                var targets = speaker != null && Vector3.Dot(speaker.HeadPosition - position, rotation * Vector3.forward) > 0f
                    ? new[] { speaker.HeadPosition } : new Vector3[0];
                var hide = new List<StoryActor>();
                Blockers(position, targets, others, hide);
                foreach (var actor in hide) if (CanHide(actor)) Hide(actor);
            }
            _shot.SetPositionAndRotation(position, rotation);
            _camera.SetDialogueCue(_shot);
            _active = true;
            if (_key != null) _key.enabled = true;
        }
    }
}
