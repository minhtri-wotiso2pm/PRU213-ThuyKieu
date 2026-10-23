using System.Collections;
using System.Collections.Generic;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Chapters;
using UnityEngine;
using UnityEngine.AI;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>
    /// Guards seize their captives and march them off as one visible escort: the two captives stay in the middle and a
    /// guard grips each outside arm. The captives do not go quietly: the first balks and looks back at the house until
    /// he is yanked on, the second wrenches from side to side. Whoever is left behind (the mother) runs after them,
    /// reaching for her husband, until a guard shoves her off near the gate and she sinks there weeping. Whoever stands
    /// in the lane (the player) is pushed aside.
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
        [Tooltip("Walk the captives use while marched along: their own (an old man's shuffle) cannot keep the guards' pace.")]
        [SerializeField] private AnimationClip _draggedWalk;
        [Tooltip("Family member left behind, who runs after the escort (optional).")]
        [SerializeField] private StoryActor _mourner;

        /// <summary>The family member who runs after the escort; the chapter can name her at runtime.</summary>
        public StoryActor Mourner { get => _mourner; set => _mourner = value; }
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
                // A speed that sits exactly on the blend tree's walk (0.5 × 3.2): no idle mixed into the stride.
                foreach (var actor in new[] { _guards[i], _captives[i] }) { Hold(actor, true); actor.SetScriptedSpeed(WalkBlendSpeed); }

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

            var ownControllers = new RuntimeAnimatorController[count];
            for (int i = 0; i < count; i++) ownControllers[i] = UseWalk(_captives[i], _draggedWalk);
            var cadence = new Cadence[formation.Length];
            for (int i = 0; i < formation.Length; i++) cadence[i] = new Cadence(formation[i]);
            var grips = new Grip[2];
            for (int i = 0; i < 2; i++) grips[i] = new Grip(_guards[i], _captives[i], i == 0 ? -1f : 1f);
            var mourner = fast || _mourner == null || !_mourner.isActiveAndEnabled ? null : new Chaser(_mourner, _captives[0]);

            float walked = 0f, clock = 0f;
            float length = Length(lane);
            Vector3 home = start;
            bool yanked = false, thrashed = false, shoved = false;
            while (walked < length)
            {
                clock += Time.deltaTime;
                float progress = walked / length;
                // The pace: steady, stopped dead while the father balks, a jerk forward when he is yanked, a crawl
                // while the boy fights them.
                float pace = _speed;
                float balk = fast ? 0f : Window(progress, clock, ref _balkStart, 0.2f, BalkDuration);
                float yank = fast ? 0f : Window(progress, clock, ref _yankStart, 0.2f, BalkDuration, YankDuration);
                float thrash = fast ? 0f : Window(progress, clock, ref _thrashStart, 0.5f, ThrashDuration);
                if (balk > 0f) pace = 0f;
                else if (yank > 0f) pace = _speed * 2.1f;
                else if (thrash > 0f) pace = _speed * 0.3f;
                if (yank > 0f && !yanked)
                {
                    yanked = true;
                    if (director.Audio != null) director.Audio.PlaySfx("stumble");
                    if (director.Fx != null) director.Fx.Shake(0.22f, 0.4f);
                }
                if (thrash > 0f && !thrashed)
                {
                    thrashed = true;
                    if (director.Audio != null) director.Audio.PlaySfx("stumble", 0.7f);
                    if (director.Fx != null) director.Fx.Shake(0.12f, 0.5f);
                }

                walked = fast ? length : Mathf.Min(length, walked + pace * Time.deltaTime);
                Vector3 at = PointAt(lane, walked, out Vector3 heading);
                facing = Quaternion.LookRotation(heading);
                Vector3 side = facing * Vector3.right;

                // The boy wrenches from side to side, the guard on his arm swaying with him.
                float wrench = thrash > 0f ? Mathf.Sin(clock * 9f) * 0.16f * Envelope(thrash) : 0f;
                Quaternion twist = Quaternion.Euler(0f, wrench * 220f, 0f);
                // The father turns to look back at his house while he balks.
                Quaternion lookBack = Quaternion.Slerp(Quaternion.identity, Quaternion.LookRotation(Flat(home - at).normalized + side * 0.4f)
                    * Quaternion.Inverse(facing), 0.75f * Envelope(balk));

                for (int i = 0; i < formation.Length; i++)
                {
                    Vector3 point = FormationPoint(at, facing, offsets[i].x, offsets[i].z);
                    Quaternion turn = facing;
                    if (formation[i] == _captives[1] || formation[i] == _guards[1]) point += side * wrench;
                    if (formation[i] == _captives[1]) turn = twist * facing;
                    if (formation[i] == _captives[0]) turn = lookBack * facing;
                    Set(formation[i], point, turn);
                    bool still = pace < 0.05f;
                    formation[i].SetScriptedSpeed(still ? 0f : WalkBlendSpeed);
                    cadence[i].Match(still ? 0f : pace);
                }
                // Gestures play only while they stand: the father pleads, the boy rages.
                _captives[0].SetTalking(balk > 0f, StoryDirector.Gesture("defensive"));
                _captives[1].SetTalking(false, 0);

                grips[0].Update(Envelope(balk) * 12f - 6f + Envelope(yank) * 22f, home);
                grips[1].Update(thrash > 0f ? -14f : -6f, home);

                if (mourner != null)
                {
                    if (!shoved && progress > 0.72f)
                    {
                        shoved = true;
                        StartCoroutine(mourner.Shoved(director, -(facing * Vector3.forward), _guards[0]));
                    }
                    if (!shoved) mourner.Follow(at - facing * Vector3.forward * 1.5f + side * 0.35f, _captives[0]);
                }
                if (walked < length) yield return null;
            }
            foreach (var c in cadence) c.Restore();
            foreach (var g in grips) g.Release();
            _captives[0].SetTalking(false, 0);
            for (int i = 0; i < count; i++)
            {
                var animator = _captives[i].GetComponentInChildren<Animator>();
                if (animator != null && ownControllers[i] != null) animator.runtimeAnimatorController = ownControllers[i];
            }
            for (int i = 0; i < count; i++)
                foreach (var actor in new[] { _guards[i], _captives[i] }) { actor.SetScriptedSpeed(null); Hold(actor, false); }
            _balkStart = _yankStart = _thrashStart = -1f;
        }

        private const float BalkDuration = 1.3f, YankDuration = 0.45f, ThrashDuration = 1.6f;
        private float _balkStart = -1f, _yankStart = -1f, _thrashStart = -1f;

        /// <summary>
        /// 0–1 progress through a beat that starts when the escort reaches <paramref name="at"/> of its way (after
        /// <paramref name="delay"/> more seconds) and lasts <paramref name="duration"/>; 0 outside it.
        /// </summary>
        private static float Window(float progress, float clock, ref float started, float at, float duration, float delay = 0f)
        {
            if (started < 0f && progress >= at) started = clock;
            if (started < 0f) return 0f;
            float t = (clock - started - delay) / duration;
            return t > 0f && t < 1f ? t : 0f;
        }

        /// <summary>Eases a beat in and out: 0 at its ends, 1 through its middle.</summary>
        private static float Envelope(float t) => t <= 0f ? 0f : Mathf.SmoothStep(0f, 1f, Mathf.Min(t / 0.25f, (1f - t) / 0.25f));

        /// <summary>
        /// A guard's hand closed on his captive's outside arm; the captive leans back against the pull (negative lean) or
        /// is jerked forward (positive). The head is left to the walk (forcing it round to the house tipped it skywards).
        /// </summary>
        private sealed class Grip
        {
            private readonly HandReachPose _guard, _captive;
            private readonly Transform _captiveElbow;
            private readonly bool _captiveLeft;

            public Grip(StoryActor guard, StoryActor captive, float side)
            {
                _guard = Pose(guard);
                _captive = Pose(captive);
                _captiveLeft = side < 0f;
                var animator = captive.GetComponentInChildren<Animator>();
                _captiveElbow = animator != null && animator.isHuman
                    ? animator.GetBoneTransform(_captiveLeft ? HumanBodyBones.LeftLowerArm : HumanBodyBones.RightLowerArm) : null;
            }

            private static HandReachPose Pose(StoryActor actor)
            {
                var animator = actor.GetComponentInChildren<Animator>();
                if (animator == null || !animator.isHuman) return null;
                var pose = actor.gameObject.AddComponent<HandReachPose>();
                pose.Initialize(animator);
                return pose;
            }

            public void Update(float lean, Vector3 home)
            {
                if (_guard == null || _captive == null || _captiveElbow == null) return;
                Vector3 elbow = _captiveElbow.position;
                // The guard's inner hand on the captive's forearm; the captive's own hand hangs a little behind it.
                Vector3 hold = elbow + (_captive.transform.forward * -0.05f);
                if (_captiveLeft) _guard.SetTargets(null, hold, null); else _guard.SetTargets(hold, null, null);
                _guard.Weight = Mathf.MoveTowards(_guard.Weight, 0.9f, Time.deltaTime * 3f);
                _captive.Lean = lean;
                _captive.Weight = Mathf.MoveTowards(_captive.Weight, 1f, Time.deltaTime * 3f);
            }

            public void Release()
            {
                if (_guard != null) Object.Destroy(_guard);
                if (_captive != null) Object.Destroy(_captive);
            }
        }

        /// <summary>The one left behind: runs after the escort reaching for her husband, until she is shoved away.</summary>
        private sealed class Chaser
        {
            private readonly StoryActor _actor;
            private readonly HandReachPose _reach;
            private Vector3 _last;

            public Chaser(StoryActor actor, StoryActor reachFor)
            {
                _actor = actor;
                Hold(actor, true);
                _actor.ResetToLocomotion();
                _last = actor.transform.position;
                var animator = actor.GetComponentInChildren<Animator>();
                if (animator != null && animator.isHuman)
                {
                    _reach = actor.gameObject.AddComponent<HandReachPose>();
                    _reach.Initialize(animator);
                }
            }

            public void Follow(Vector3 behind, StoryActor reachFor)
            {
                Vector3 at = _actor.transform.position;
                Vector3 to = Flat(behind - at);
                float run = Mathf.Min(to.magnitude * 3f, 3.4f);
                Vector3 next = at + (to.sqrMagnitude > 0.0001f ? to.normalized : Vector3.zero) * run * Time.deltaTime;
                Vector3 look = Flat(reachFor.transform.position - next);
                Quaternion facing = look.sqrMagnitude > 0.001f ? Quaternion.LookRotation(look) : _actor.transform.rotation;
                if (Physics.Raycast(next + Vector3.up * 1.2f, Vector3.down, out RaycastHit hit, 3f, 1 << 8, QueryTriggerInteraction.Ignore))
                    next.y = hit.point.y;
                _actor.transform.SetPositionAndRotation(next, facing);
                float speed = Flat(next - _last).magnitude / Mathf.Max(Time.deltaTime, 0.001f);
                _last = next;
                _actor.SetScriptedSpeed(speed);
                if (_reach == null) return;
                // Close enough, her right hand goes out to his back.
                Vector3 back = reachFor.transform.position + Vector3.up * 1.15f;
                float near = 1f - Mathf.InverseLerp(1.4f, 2.4f, Vector3.Distance(next, reachFor.transform.position));
                _reach.SetTargets(null, back, back + Vector3.up * 0.3f);
                _reach.Lean = 10f * near;
                _reach.Weight = Mathf.MoveTowards(_reach.Weight, near, Time.deltaTime * 2.5f);
            }

            /// <summary>A guard turns and shoves her back; she staggers and stays there weeping.</summary>
            public IEnumerator Shoved(StoryDirector director, Vector3 away, StoryActor guard)
            {
                if (director.Audio != null) director.Audio.PlaySfx("stumble");
                if (director.Fx != null) director.Fx.Shake(0.2f, 0.35f);
                Vector3 from = _actor.transform.position;
                Vector3 to = from + Flat(away).normalized * 1.1f;
                Quaternion facing = _actor.transform.rotation;
                _actor.SetScriptedSpeed(0f);
                for (float t = 0f; t < 0.4f; t += Time.deltaTime)
                {
                    float k = 1f - (1f - t / 0.4f) * (1f - t / 0.4f);
                    _actor.transform.SetPositionAndRotation(Vector3.Lerp(from, to, k), facing);
                    if (_reach != null) { _reach.Lean = Mathf.Lerp(10f, -14f, k); _reach.Weight = 1f - k; }
                    yield return null;
                }
                if (_reach != null) Object.Destroy(_reach);
                _actor.SetTalking(true, StoryDirector.Gesture("crying"));
                Hold(_actor, false);
                _actor.SetScriptedSpeed(null);
            }
        }

        private const float WalkBlendSpeed = 1.6f;

        /// <summary>Plays <paramref name="walk"/> as the actor's walk; returns the controller to put back afterwards.</summary>
        private static RuntimeAnimatorController UseWalk(StoryActor actor, AnimationClip walk)
        {
            var animator = actor.GetComponentInChildren<Animator>();
            if (walk == null || animator == null || animator.runtimeAnimatorController == null) return null;
            var own = animator.runtimeAnimatorController;
            var swapped = new AnimatorOverrideController(own);
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            swapped.GetOverrides(overrides);
            for (int i = 0; i < overrides.Count; i++)
                if (overrides[i].Key.name == "Walking") overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, walk);
            swapped.ApplyOverrides(overrides);
            animator.runtimeAnimatorController = swapped;
            return own;
        }

        /// <summary>
        /// Plays an actor's walk clip at the pace of the formation. The clips walk on the spot at their own cadence (an
        /// old man's shuffle takes three seconds a cycle), so at the formation's speed the planted feet slid along the
        /// yard. The clip's speed is its two steps per cycle, each as long as the widest the feet have opened so far.
        /// </summary>
        private sealed class Cadence
        {
            private readonly Animator _animator;
            private readonly Transform _left, _right;
            private float _stride;

            public Cadence(StoryActor actor)
            {
                _animator = actor.GetComponentInChildren<Animator>();
                if (_animator == null || !_animator.isHuman) { _animator = null; return; }
                _left = _animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                _right = _animator.GetBoneTransform(HumanBodyBones.RightFoot);
                Transform hips = _animator.GetBoneTransform(HumanBodyBones.Hips);
                // Until the feet have opened once, a step is taken to be about half the leg.
                _stride = hips != null && _left != null ? Mathf.Abs(hips.position.y - _left.position.y) * 0.5f : 0.4f;
            }

            public void Match(float speed)
            {
                if (_animator == null || _left == null || _right == null) return;
                // Measured along the way they walk: the feet also sit a hand apart sideways, which is not a step.
                _stride = Mathf.Max(_stride, Mathf.Abs(Vector3.Dot(_left.position - _right.position, _animator.transform.forward)));
                float cycle = 0f, weight = 0f;
                foreach (var info in _animator.GetCurrentAnimatorClipInfo(0))
                    if (info.weight > weight) { weight = info.weight; cycle = info.clip.length; }
                if (cycle < 0.1f) return;
                float natural = 2f * _stride / cycle;
                _animator.speed = Mathf.Clamp(speed / Mathf.Max(natural, 0.05f), 0.6f, 2.8f);
            }

            public void Restore()
            {
                if (_animator != null) _animator.speed = 1f;
            }
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
