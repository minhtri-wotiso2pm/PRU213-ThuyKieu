using System.Collections;
using System.Collections.Generic;
using ThuyKieu.Core.Story;
using UnityEngine;
using UnityEngine.AI;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>
    /// Guards seize their captives and march them off as one visible escort: the two captives stay in the middle and a
    /// guard holds each outside arm. The formation is kept rigid through the gate so nobody merely follows behind or
    /// overlaps another actor. Whoever stands in the lane (the player) is pushed aside.
    /// </summary>
    public class EscortSequence : StorySequence
    {
        [SerializeField] private StoryActor[] _guards;
        [SerializeField] private StoryActor[] _captives;
        [Tooltip("Where each pair ends up (outside the gate).")]
        [SerializeField] private Transform[] _exits;
        [Tooltip("The opening each pair passes through (the gate), one per pair so the lanes stay apart.")]
        [SerializeField] private Transform[] _gateways;
        [SerializeField] private float _speed = 1.3f;
        [SerializeField] private string _sfx = "stumble";
        [SerializeField] private float _shake = 0.15f;
        [SerializeField] private LayerMask _ground = 1 << 8;

        private const float CaptiveSpacing = 0.42f;
        private const float GuardSpacing = 1.05f;
        private const float GuardStepBack = -0.12f;

        public override IEnumerator Run(StoryDirector director)
        {
            bool fast = director.Autopilot;
            int count = Mathf.Min(_guards.Length, _captives.Length);
            if (count < 2 || _exits.Length == 0 || _gateways.Length == 0) yield break;

            Vector3 start = (_captives[0].transform.position + _captives[1].transform.position) * 0.5f;
            Vector3 gateway = Average(_gateways);
            Vector3 exit = Average(_exits);
            var lane = Lane(start, gateway, exit);
            Quaternion facing = Facing(lane[0], lane[1]);

            // Vương Ông's previous line uses the authored collapse clip. It is a one-shot gesture and can still be
            // playing when the player advances immediately, which used to freeze his feet while this sequence slid
            // his root along the floor. Both captives enter locomotion before anybody is moved.
            for (int i = 0; i < count; i++) _captives[i].ResetToLocomotion();
            if (!fast) yield return new WaitForSeconds(0.14f);

            // 1. The guards close in from the outside instead of following the prisoners from behind. This is
            // scripted rather than NavMesh-driven because the veranda's low table is also a baked walkable surface.
            _captives[0].Place(_captives[0].transform.position, facing);
            _captives[1].Place(_captives[1].transform.position, facing);
            Vector3[] guardFrom = { _guards[0].transform.position, _guards[1].transform.position };
            Vector3[] guardTo = { FormationPoint(start, facing, -GuardSpacing, GuardStepBack),
                FormationPoint(start, facing, GuardSpacing, GuardStepBack) };
            Quaternion[] approachFacing = { Facing(guardFrom[0], guardTo[0]), Facing(guardFrom[1], guardTo[1]) };
            float approachDuration = fast ? 0f : Mathf.Max(Vector3.Distance(guardFrom[0], guardTo[0]),
                Vector3.Distance(guardFrom[1], guardTo[1])) / 2f;
            for (int i = 0; i < 2; i++) { Hold(_guards[i], true); _guards[i].SetScriptedSpeed(2f); }
            for (float t = 0f; t < approachDuration; t += Time.deltaTime)
            {
                float blend = Mathf.SmoothStep(0f, 1f, t / approachDuration);
                for (int i = 0; i < 2; i++)
                    Set(_guards[i], Vector3.Lerp(guardFrom[i], guardTo[i], blend), approachFacing[i]);
                yield return null;
            }
            // Once beside the captives, the guards turn towards the gate before the formation starts moving.
            float turnDuration = fast ? 0f : 0.32f;
            for (float t = 0f; t < turnDuration; t += Time.deltaTime)
            {
                float blend = Mathf.SmoothStep(0f, 1f, t / turnDuration);
                for (int i = 0; i < 2; i++)
                    Set(_guards[i], guardTo[i], Quaternion.Slerp(approachFacing[i], facing, blend));
                yield return null;
            }
            for (int i = 0; i < 2; i++) Set(_guards[i], guardTo[i], facing);
            if (director.Audio != null && !string.IsNullOrEmpty(_sfx)) director.Audio.PlaySfx(_sfx);
            if (director.Fx != null && _shake > 0f) director.Fx.Shake(_shake, 0.5f);

            // 2. Kiều stands in a lane: pushed out of the way before they set off.
            yield return ShoveAside(director, lane, fast);

            // 3. Pull the family into the four-person escort, then keep that formation locked all the way out.
            for (int i = 0; i < count; i++)
                foreach (var actor in new[] { _guards[i], _captives[i] }) { Hold(actor, true); actor.SetScriptedSpeed(_speed); }

            Vector3[] from = { _guards[0].transform.position, _captives[0].transform.position,
                _captives[1].transform.position, _guards[1].transform.position };
            Vector3[] offsets = { new Vector3(-GuardSpacing, 0f, GuardStepBack), new Vector3(-CaptiveSpacing, 0f, 0f),
                new Vector3(CaptiveSpacing, 0f, 0f), new Vector3(GuardSpacing, 0f, GuardStepBack) };
            StoryActor[] formation = { _guards[0], _captives[0], _captives[1], _guards[1] };
            float assembleDuration = fast ? 0f : 0.55f;
            for (float t = 0f; t < assembleDuration; t += Time.deltaTime)
            {
                float blend = Mathf.SmoothStep(0f, 1f, t / assembleDuration);
                for (int i = 0; i < formation.Length; i++)
                    Set(formation[i], Vector3.Lerp(from[i], FormationPoint(start, facing, offsets[i].x, offsets[i].z), blend), facing);
                yield return null;
            }

            float walked = 0f;
            float length = Length(lane);
            while (walked < length)
            {
                walked = fast ? length : Mathf.Min(length, walked + _speed * Time.deltaTime);
                Vector3 at = PointAt(lane, walked, out Vector3 heading);
                facing = Quaternion.LookRotation(heading);
                for (int i = 0; i < formation.Length; i++)
                    Set(formation[i], FormationPoint(at, facing, offsets[i].x, offsets[i].z), facing);
                if (walked < length) yield return null;
            }
            for (int i = 0; i < count; i++)
                foreach (var actor in new[] { _guards[i], _captives[i] }) { actor.SetScriptedSpeed(null); Hold(actor, false); }
        }

        private static Vector3 Average(Transform[] points)
        {
            Vector3 total = Vector3.zero;
            foreach (var point in points) total += point.position;
            return total / points.Length;
        }

        private static Vector3 FormationPoint(Vector3 centre, Quaternion facing, float side, float forward)
            => centre + facing * new Vector3(side, 0f, forward);

        /// <summary>Off the veranda straight towards the yard, through the gateway, out to the exit.</summary>
        private static List<Vector3> Lane(Vector3 start, Vector3 gateway, Vector3 exit)
        {
            Vector3 down = new Vector3(Mathf.Lerp(start.x, gateway.x, 0.4f), start.y, start.z - 1.6f);
            return new List<Vector3> { start, down, gateway, exit };
        }

        private static Quaternion Facing(Vector3 from, Vector3 to)
        {
            Vector3 d = to - from;
            d.y = 0f;
            return Quaternion.LookRotation(d.sqrMagnitude > 0.001f ? d : Vector3.back);
        }

        private static float Length(List<Vector3> lane)
        {
            float length = 0f;
            for (int i = 1; i < lane.Count; i++) length += Flat(lane[i] - lane[i - 1]).magnitude;
            return length;
        }

        private static Vector3 PointAt(List<Vector3> lane, float distance, out Vector3 heading)
        {
            for (int i = 1; i < lane.Count; i++)
            {
                Vector3 segment = Flat(lane[i] - lane[i - 1]);
                float length = segment.magnitude;
                heading = segment / Mathf.Max(length, 0.001f);
                if (distance <= length || i == lane.Count - 1) return lane[i - 1] + heading * Mathf.Min(distance, length);
                distance -= length;
            }
            heading = Vector3.back;
            return lane[lane.Count - 1];
        }

        private static Vector3 Flat(Vector3 v) { v.y = 0f; return v; }

        /// <summary>Puts the actor on the ground under <paramref name="at"/> (veranda or yard).</summary>
        private void Set(StoryActor actor, Vector3 at, Quaternion facing)
        {
            if (Physics.Raycast(at + Vector3.up * 1.2f, Vector3.down, out RaycastHit hit, 3f, _ground, QueryTriggerInteraction.Ignore))
                at.y = hit.point.y;
            actor.transform.SetPositionAndRotation(at, facing);
        }

        /// <summary>While held, the agent is off so pathfinding and avoidance cannot pull the pair apart.</summary>
        private static void Hold(StoryActor actor, bool held)
        {
            var agent = actor.GetComponent<NavMeshAgent>();
            if (agent == null) return;
            if (held) agent.enabled = false;
            else actor.EnableNavigation();
        }

        private IEnumerator ShoveAside(StoryDirector director, List<Vector3> lane, bool fast)
        {
            var player = director.Player;
            Vector3 p = Flat(player.transform.position);
            for (int s = 1; s < lane.Count; s++)
            {
                Vector3 a = Flat(lane[s - 1]), b = Flat(lane[s]);
                Vector3 ab = b - a;
                float t = Mathf.Clamp01(Vector3.Dot(p - a, ab) / Mathf.Max(ab.sqrMagnitude, 0.001f));
                Vector3 side = p - (a + ab * t);
                if (side.magnitude > 1.55f) continue;
                Vector3 push = new Vector3(p.x >= a.x ? 1f : -1f, 0f, 0f);
                if (director.Audio != null) director.Audio.PlaySfx("stumble");
                if (director.Fx != null) director.Fx.Shake(0.25f, 0.4f);
                player.ScriptedSpeed = 3.2f;
                player.ScriptedMove = push;
                yield return new WaitForSeconds(fast ? 0f : 0.5f);
                player.ScriptedMove = null;
                yield break;
            }
        }

    }
}
