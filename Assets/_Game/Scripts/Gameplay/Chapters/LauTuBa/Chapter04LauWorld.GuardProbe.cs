using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThuyKieu.Core;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.House;
using ThuyKieu.Gameplay.Stealth;
using ThuyKieu.Interaction;
using UnityEngine;

namespace ThuyKieu.Gameplay.Chapters.LauTuBa
{
    /// <summary>
    /// QA for the guards' revision ("-autopilotGuards", docs/concept/CLAUDE_CH4_GUARDS_DISTRACTION_REVISION.md §7), on the
    /// real Chapter 4 scene with the guards' senses on at the chapter's own sensitivity (never raised here).
    /// <list type="bullet">
    /// <item>Sight: front / left / right / back × 0.5, 1, 2, 4 m × his lantern lit or out × standing or crouched, still;
    /// the old shared values first (the bug reproduced), then this chapter's, for both guards; a shut door between them.
    /// Kiều and the guard are placed for these (each sense alone); the time to "come to check" and to "sure, chase" is noted.</item>
    /// <item>Pebbles: pick up / limit / refill through the real input, an arc stopped by a door and by a pillar, one noise
    /// per pebble at the spot it struck (stone, boards, a mat), a chase a pebble does not hijack, the lure limit.</item>
    /// <item>The lure out and back from Tú Bà's door, timed, and the way in, the keys (the real minigame) and out again
    /// with Kiều walked by the character controller while he is away.</item>
    /// <item>Kiều's room: chased in through the open door, a shut door with a sound inside, the threshold, a pebble thrown
    /// out of it, the checkpoint.</item>
    /// <item>Other doors: an unlocked one opened on the way to a check, a locked one left shut, two guards at one door, pulled
    /// to behind him on a round (never on her), no noise and no story step from a guard's hand.</item>
    /// <item>J2: both guards walk to their rounds from where they stand, call each other, and a checkpoint restores it.</item>
    /// </list>
    /// </summary>
    public partial class Chapter04LauWorld
    {
        private bool _guardsProbed;
        private readonly Dictionary<Watchman, Transform> _posts = new Dictionary<Watchman, Transform>();

        private sealed class SightResult
        {
            public float FirstSeen = -1f, Check = -1f, Sure = -1f, Light;
            public bool Felt;
            public string Cell(float limit) => (Felt ? "felt " : "") + (Sure >= 0f ? Sure.ToString("F2") : Check >= 0f ? "chk " + Check.ToString("F2") : FirstSeen >= 0f ? "seen" : ">" + limit.ToString("F0"))
                + (FirstSeen >= 0f && Check >= 0f && FirstSeen <= Check ? "" : FirstSeen >= 0f ? $" (seen {FirstSeen:F2})" : "");
        }

        private IEnumerator GuardProbe()
        {
            const float F = LauTuBaFloor;
            float timeScale = Time.timeScale, realStart = Time.realtimeSinceStartup;
            Time.timeScale = 1f;
            _probing = true;
            var player = _director.Player;
            _interaction = FindFirstObjectByType<PlayerInteraction>();
            if (_stage == 0) { _hairpin.Interact(); _kieuDoor.SetLock(DoorLock.None); OnKieuDoor(_kieuDoor); }
            for (float wait = 30f; !_tuBa.IsLying && (wait -= Time.deltaTime) > 0f;) yield return null;
            if (!_tuBa.IsLying) PutTuBaToBed();
            var storeDoor = _allDoors.First(d => d.name == "StoreDoor");
            var roomDoor = _tuBaDoor;
            var doors = new Dictionary<HouseDoor, HouseDoor.Snapshot>();
            foreach (var d in _allDoors) doors[d] = d.Save();

            void Report(string test, bool ok, string detail)
            {
                Info("GUARDS " + (ok ? "ok   " : "FAIL ") + test + " · " + detail);
                if (!ok) StoryAutopilot.Problem("[GuardProbe] " + test + " · " + detail);
            }

            // Everyone still and blind, far from the test, Kiều unhidden, doors as the night began.
            void Quiet()
            {
                player.ScriptedMove = null;
                player.ForceCrouch = false;
                InputReader.SimulatedInteract = false;
                HideSpot.ExitAll();
                foreach (var w in _watchers) { w.EndScript(false); w.Forget(); w.Perceiving = false; w.Pause(true); }
                foreach (var pair in doors) pair.Key.Load(pair.Value);
                foreach (var w in new[] { _guard, _gateGuard }) Lantern(w, true);
                player.Teleport(new Vector3(-16.5f, F + 0.05f, 4.5f), Quaternion.identity);
                Park(_guard, new Vector3(16f, F, 9.6f));
                Park(_gateGuard, new Vector3(2.4f, F, -17.9f));
            }

            IEnumerator Go(float speed, bool crouch, params Vector3[] points)
            {
                player.ForceCrouch = crouch;
                player.ScriptedSpeed = speed;
                foreach (var p in points)
                    for (float t = 0f; t < 12f && Flat(p - player.transform.position).magnitude > 0.3f; t += Time.deltaTime)
                    {
                        player.ScriptedMove = p - player.transform.position;
                        yield return null;
                    }
                player.ScriptedMove = null;
            }

            IEnumerator Frames(int n) { for (int i = 0; i < n; i++) yield return null; }

            // Whether the guard could make her out at a spot (his cone and a little more for his turning head, how far he
            // sees in that light, nothing in between): what a player reads from his lantern and his facing.
            bool Visible(Vector3 at)
            {
                Vector3 eye = _guard.Eyes.position, head = at + Vector3.up * 0.95f;
                Vector3 to = head - eye;
                if (Vector3.Angle(Flat(_guard.transform.forward), Flat(to)) > 110f) return false;
                if (to.magnitude > SightModel.EffectiveRange(_guard.Range, LightExposure.At(at + Vector3.up * 0.6f)) + 0.5f) return false;
                return !Physics.Linecast(eye, head, 1 << 8, QueryTriggerInteraction.Ignore);
            }

            // Crouched along the walkable way, a step at a time; she holds still while her next step would be in his sight.
            IEnumerator Creep(params Vector3[] points)
            {
                player.ForceCrouch = true;
                player.ScriptedSpeed = 1.15f;
                var way = new UnityEngine.AI.NavMeshPath();
                foreach (var p in points)
                {
                    Vector3[] corners = { p };
                    if (UnityEngine.AI.NavMesh.SamplePosition(player.transform.position, out var from, 0.8f, UnityEngine.AI.NavMesh.AllAreas)
                        && UnityEngine.AI.NavMesh.CalculatePath(from.position, p, UnityEngine.AI.NavMesh.AllAreas, way) && way.corners.Length > 1)
                        corners = way.corners.Skip(1).ToArray();
                    foreach (var c in corners)
                    {
                        float waited = 0f, t = 0f;
                        for (; t < 40f && Flat(c - player.transform.position).magnitude > 0.3f; t += Time.deltaTime)
                        {
                            Vector3 here = player.transform.position;
                            Vector3 next = here + Flat(c - here).normalized * 0.6f;
                            bool hold = Visible(next) && !Visible(here);
                            if (hold) waited += Time.deltaTime;
                            player.ScriptedMove = hold ? (Vector3?)null : c - here;
                            yield return null;
                        }
                        if (t >= 40f) Info($"GUARDS (creep did not reach {Flat(c)}: at {Flat(player.transform.position)}, held still {waited:F1} s of 40 for the guard at {Flat(_guard.transform.position)})");
                    }
                }
                player.ScriptedMove = null;
            }

            // Holds E on a door's leaf (eyes on it as it moves) until done says so.
            IEnumerator WorkDoor(HouseDoor door, System.Func<bool> done, float seconds)
            {
                var leafCollider = door.GetComponentInChildren<Collider>();
                for (float t = 0f; t < seconds && !done(); t += Time.deltaTime)
                {
                    _director.CameraController.LookAtNow(leafCollider != null ? leafCollider.bounds.center : door.Centre + Vector3.up);
                    InputReader.SimulatedInteract = t > 0.15f;
                    yield return null;
                }
                InputReader.SimulatedInteract = false;
                yield return Frames(3);
            }

            // ------------------------------------------------------------------ rig: eyes, facing, his own colliders
            foreach (var w in new[] { _guard, _gateGuard })
            {
                var animator = w.GetComponentInChildren<Animator>();
                var head = animator != null && animator.isHuman ? animator.GetBoneTransform(HumanBodyBones.Head) : null;
                float eyeVsHead = Vector3.Distance(w.Eyes.position, w.Actor.HeadPosition);
                float eyesTurn = Vector3.Angle(Flat(w.Eyes.forward), Flat(w.transform.forward));
                string solids = string.Join(", ", w.GetComponentsInChildren<Collider>(true).Where(c => !c.isTrigger && (1 << c.gameObject.layer & 1 << 8) != 0).Select(c => c.name));
                Report($"{w.name}: eyes at his head, facing his way, nothing of his own blocking his sight", eyeVsHead < 0.3f && eyesTurn < 5f && solids.Length == 0,
                    $"eyes {eyeVsHead:F2} m from the head{(head != null ? $" (head bone faces {Vector3.Angle(Flat(head.forward), Flat(w.transform.forward)):F0}° off his root)" : "")} · eyes turned {eyesTurn:F0}° · " +
                    $"his solid colliders on the sight layer: {(solids.Length == 0 ? "none" : solids)} · halfAngle {w.HalfAngle:F0}, side {w.PeripheralHalfAngle:F0}°/{w.PeripheralRange:F1} m, near {w.NearRange:F1} m ×{w.NearFloor:F1}, touch {w.TouchSense}, sensitivity {w.Sensitivity:F2}");
            }

            // ------------------------------------------------------------------ sight matrix
            Quiet();
            var centre = SightCentre(out string centreWhy);
            Info("GUARDS sight test ground: " + centreWhy);
            var directions = new[] { ("front", 0f), ("left", -90f), ("right", 90f), ("back", 180f) };
            var distances = new[] { 0.5f, 1f, 2f, 4f };
            const float limit = 3f;
            float yaw = 90f;
            var configs = new List<(string name, Watchman w, System.Action apply)>
            {
                ("guard, OLD shared sight (40°, side 70°/4 m at half focus, close floor in front only, no touch)", _guard, () => _guard.SetSight(40f, 0f, 0f, 0f, 0f, false)),
                ("guard, Chapter 4 sight", _guard, () => _guard.SetSight(60f, 90f, 4f, 2f, 1.4f, true)),
                ("gate guard, Chapter 4 sight", _gateGuard, null)
            };
            foreach (var (name, w, apply) in configs)
            {
                apply?.Invoke();
                var table = new System.Text.StringBuilder();
                int failures = 0;
                float worstNear = 0f;
                foreach (var (dirName, angle) in directions)
                    foreach (bool lit in new[] { true, false })
                        foreach (bool crouch in new[] { false, true })
                        {
                            var cells = new List<string>();
                            float rowLight = 0f;
                            foreach (float d in distances)
                            {
                                var r = new SightResult();
                                yield return SightCase(w, centre, yaw, angle, d, lit, crouch, limit, r);
                                cells.Add(r.Cell(limit));
                                if (d <= 1f) rowLight = Mathf.Max(rowLight, r.Light);
                                bool near = d <= 2f && angle != 180f;
                                bool chapter = !name.Contains("OLD");
                                // Front or side within 2 m, in plain view: sure within a second, whatever the light or posture.
                                if (chapter && near) { worstNear = Mathf.Max(worstNear, r.Sure < 0f ? 99f : r.Sure); if (r.Sure < 0f || r.Sure > 1.0f) failures++; }
                                // Behind: at 0.5 m the bodies touch (her capsule is pushed off his): he reacts, turns and finds her within
                                // a second; further back, nothing at all.
                                if (chapter && angle == 180f && (d <= 0.5f ? r.Check < 0f || r.Check > 1f : r.FirstSeen >= 0f || r.Check >= 0f)) failures++;
                            }
                            table.AppendLine($"      {dirName,-5} {(lit ? "lit " : "dark")} {(crouch ? "crouch" : "stand ")} (light at 1 m {rowLight:F2}) · " + string.Join(" · ", cells.Select((c, i) => $"{distances[i]:0.#} m {c}")));
                        }
                bool old = name.Contains("OLD");
                Report($"sight, {name}", old || failures == 0, old ? "the bug as it was (reproduced on the same rig and ground)" : $"{failures} cells off the rule · slowest front/side within 2 m {(worstNear >= 99f ? "never" : worstNear.ToString("F2") + " s")}");
                Info("GUARDS sight table (s to sure; chk = came to check only; felt = bumped from behind; seen = in sight but not sure; >3 = nothing):\n" + table);
            }
            // Walking into his back: unseen until she touches him, then he turns.
            {
                Quiet();
                PostAt(_guard, centre, yaw);
                Vector3 behind = centre + Quaternion.Euler(0f, yaw + 180f, 0f) * Vector3.forward * 1.6f;
                player.Teleport(behind + Vector3.up * 0.05f, Quaternion.Euler(0f, yaw, 0f));
                player.ForceCrouch = true;
                yield return Frames(4);
                _guard.Forget();
                Hold(_guard, yaw);
                _guard.Perceiving = true;
                _guard.Pause(false);
                float seenBefore = -1f, reacted = -1f, gap = 99f;
                for (float t = 0f; t < 4f && reacted < 0f; t += Time.deltaTime)
                {
                    if (_guard.State == WatchState.Scripted) Hold(_guard, yaw);
                    player.ScriptedSpeed = 1.15f;
                    player.ScriptedMove = Flat(_guard.transform.position - player.transform.position);
                    gap = Flat(_guard.transform.position - player.transform.position).magnitude;
                    if (_guard.SeesPlayer && seenBefore < 0f) seenBefore = t;
                    if (_guard.Awareness >= Watchman.InvestigateAt || _guard.State == WatchState.Investigate) reacted = t;
                    yield return null;
                }
                player.ScriptedMove = null;
                Report("walking into his back: not seen on the way, felt on touching, he turns to look",
                    reacted >= 0f && (seenBefore < 0f || seenBefore >= reacted), $"reacted after {(reacted < 0f ? "never" : reacted.ToString("F2") + " s")} at {gap:F2} m (bodies touch within {_guard.TouchReach:F2} m) · seen before touching {(seenBefore >= 0f && seenBefore < reacted)} · {_guard.State}");
            }

            // A shut door between them: nothing; opened, he sees her.
            {
                Quiet();
                PostAt(_guard, new Vector3(-6.0f, F, 9.9f), 0f);
                player.Teleport(new Vector3(-6.0f, F + 0.05f, 12.4f), Quaternion.Euler(0f, 0f, 0f));
                yield return Frames(3);
                _guard.Pause(false);
                _guard.Perceiving = true;
                Hold(_guard, 0f);
                float seenShut = 0f;
                for (float t = 0f; t < 3f; t += Time.deltaTime) { Hold(_guard, 0f); if (_guard.SeesPlayer) seenShut += Time.deltaTime; yield return null; }
                storeDoor.SetOpen(1f);
                float seen = -1f, sure = -1f;
                for (float t = 0f; t < 6f && sure < 0f; t += Time.deltaTime)
                {
                    if (_guard.State == WatchState.Scripted) Hold(_guard, 0f);
                    if (_guard.SeesPlayer && seen < 0f) seen = t;
                    if (_guard.Awareness >= 1f) sure = t;
                    yield return null;
                }
                Report("sight through a shut door (none) and through the same door open", seenShut == 0f && seen >= 0f,
                    $"in sight {seenShut:F1} s while shut · once open: in sight after {(seen < 0f ? "never" : seen.ToString("F2") + " s")}, sure after {(sure < 0f ? "more than 6 s" : sure.ToString("F2") + " s")} (2.5 m, light {LightExposure.At(player.transform.position + Vector3.up):F2})");
            }

            // ------------------------------------------------------------------ pebbles: pick up, limit, refill (real input)
            Quiet();
            {
                var pile = _pebblePile;
                Vector3 stand = pile.transform.position + Vector3.right * 0.95f;
                player.Teleport(stand + Vector3.up * 0.05f, Quaternion.LookRotation(Flat(pile.transform.position - stand)));
                _pebbles.SetCount(0);
                yield return Frames(2);
                for (int i = 0; i < 20; i++) { _director.CameraController.LookAtNow(pile.transform.position + Vector3.up * 0.1f); yield return null; }
                bool target = ReferenceEquals(_interaction.CurrentTarget, pile);
                InputReader.SimulatedInteract = true; yield return Frames(2); InputReader.SimulatedInteract = false; yield return Frames(2);
                int afterFirst = _pebbles.Count;
                InputReader.SimulatedInteract = true; yield return Frames(2); InputReader.SimulatedInteract = false; yield return Frames(2);
                int afterSecond = _pebbles.Count;
                string full = pile.DisplayName;
                int thrownBefore = _pebbles.Thrown;
                _director.CameraController.LookAtNow(new Vector3(-6f, 0f, 5f));
                for (int i = 0; i < 4; i++) { InputReader.SimulateThrow(); for (float t = 0f; t < 1f; t += Time.deltaTime) { _director.CameraController.LookAtNow(new Vector3(-6f, 0f, 5f)); yield return null; } }
                int thrown = _pebbles.Thrown - thrownBefore;
                int left = _pebbles.Count;
                for (int i = 0; i < 20; i++) { _director.CameraController.LookAtNow(pile.transform.position + Vector3.up * 0.1f); yield return null; }
                InputReader.SimulatedInteract = true; yield return Frames(2); InputReader.SimulatedInteract = false; yield return Frames(2);
                Report("pebbles: E at the heap fills her hand to 3, no more; Q throws them; the 4th Q does nothing; the heap fills it again",
                    target && afterFirst == 3 && afterSecond == 3 && thrown == 3 && left == 0 && _pebbles.Count == 3,
                    $"heap was the target {target} · after E {afterFirst}, again {afterSecond} (\"{StripTags(full)}\") · 4 presses of Q threw {thrown}, left {left} · after the heap {_pebbles.Count}");
            }

            // ------------------------------------------------------------------ pebbles: an arc stopped early, one noise where it struck
            {
                var heard = new List<Noise.Event>();
                void OnEmit(Noise.Event e) { if (e.Kind == NoiseKind.Thrown) heard.Add(e); }
                Noise.Emitted += OnEmit;
                IEnumerator Toss(Vector3 hand, Vector3 aim)
                {
                    heard.Clear();
                    float at = Time.time;
                    _pebbles.ThrowForTest(hand, aim);
                    for (float t = 0f; t < 4.5f; t += Time.deltaTime) yield return null;
                    if (_pebbles.LastImpact.Time < at) heard.Add(new Noise.Event { Kind = NoiseKind.Object }); // nothing struck: noted as a failure below
                }
                Quiet();
                // The store door shut between the hand and a point inside.
                Vector3 doorHand = new Vector3(-6.4f, F + 1.35f, 9.4f), doorAim = new Vector3(-6.2f, F, 13.6f);
                yield return Toss(doorHand, doorAim);
                var hit = _pebbles.LastImpact;
                Report("pebble: thrown at a point behind the shut store door, it strikes the door and is heard on her side, once",
                    hit.Collider != null && hit.Collider.GetComponentInParent<HouseDoor>() == storeDoor && hit.Blocked && heard.Count == 1 && heard[0].Position.z < 11.02f,
                    $"struck {Describe(hit.Collider)} at {Flat(hit.Point)} ({hit.Surface}, {hit.Contacts} contacts) · blocked {hit.Blocked} · noises {heard.Count} at {string.Join(", ", heard.Select(e => $"{Flat(e.Position)} r {e.Radius:F1}"))}");
                // A veranda pillar between the hand and the yard.
                var pillar = Physics.OverlapSphere(new Vector3(-9f, F + 1.5f, 1f), 3f, 1 << 8, QueryTriggerInteraction.Ignore)
                    .Where(c => c.bounds.size.y > 2f && c.bounds.size.x < 0.6f && c.bounds.size.z < 0.6f).OrderBy(c => Flat(c.bounds.center - new Vector3(-9f, 0f, 1f)).magnitude).FirstOrDefault();
                if (pillar == null) Report("pebble: a pillar on the arc", false, "no pillar found near the west yard edge");
                else
                {
                    Vector3 c = pillar.bounds.center;
                    Vector3 pillarHand = new Vector3(c.x - 2.2f, F + 1.4f, c.z), pillarAim = new Vector3(c.x + 3.5f, 0f, c.z);
                    yield return Toss(pillarHand, pillarAim);
                    hit = _pebbles.LastImpact;
                    Report("pebble: thrown past a pillar into the yard, it strikes the pillar first (once), not the yard behind it",
                        hit.Collider == pillar && hit.Blocked && heard.Count == 1, $"pillar {pillar.name} at {Flat(c)} · struck {Describe(hit.Collider)} at {Flat(hit.Point)} · blocked {hit.Blocked} · noises {heard.Count}");
                }
                // Surfaces: the wet yard (stone), the west veranda (boards), the reed mat in Kiều's room (soft).
                var cases = new[]
                {
                    ("the wet yard", new Vector3(-10.4f, F + 1.5f, 1.2f), new Vector3(-3.5f, 0f, 1.2f), true),
                    ("the veranda boards", new Vector3(-10.5f, F + 1.5f, -1.5f), new Vector3(-10.4f, F, -6.2f), true),
                    ("the reed mat in her room", new Vector3(-13.6f, F + 1.4f, 4.4f), new Vector3(-14.75f, F, 3.45f), true)
                };
                var lines = new List<string>();
                bool allOnce = true;
                float hardRadius = 0f, woodRadius = 0f, softRadius = 0f;
                foreach (var (label, hand, aim, _) in cases)
                {
                    yield return Toss(hand, aim);
                    hit = _pebbles.LastImpact;
                    bool once = heard.Count == 1 && Flat(heard[0].Position - hit.Point).magnitude < 0.2f;
                    allOnce &= once;
                    if (PebbleThrow.Hard(hit.Surface)) hardRadius = heard.Count > 0 ? heard[0].Radius : 0f;
                    else if (PebbleThrow.Soft(hit.Surface)) softRadius = heard.Count > 0 ? heard[0].Radius : 0f;
                    else woodRadius = heard.Count > 0 ? heard[0].Radius : 0f;
                    lines.Add($"{label}: {hit.Surface} ({PebbleThrow.SoundFor(hit.Surface)}) at {Flat(hit.Point)} {hit.Speed:F1} m/s in, {hit.Contacts} contacts, noises {heard.Count} r {(heard.Count > 0 ? heard[0].Radius : 0f):F1} m");
                }
                Report("pebble: one noise per pebble at the spot it first struck, however many bounces; stone carries furthest, a mat least",
                    allOnce && hardRadius >= Noise.Loud && softRadius > 0f && softRadius < Noise.Loud && hardRadius > softRadius, string.Join(" · ", lines));
                Noise.Emitted -= OnEmit;
            }

            // ------------------------------------------------------------------ Kiều's room
            {
                // Chased into her room through the open door: the chase ends at the threshold, he looks outside, then leaves.
                Quiet();
                _kieuDoor.SetOpen(1f);
                int caught = 0;
                void Caught(Watchman _) => caught++;
                _guard.Caught += Caught;
                player.Teleport(new Vector3(-10.6f, F + 0.05f, 6.6f), Quaternion.Euler(0f, 180f, 0f));
                PostAt(_guard, new Vector3(-10.5f, F, 1.6f), 0f);
                _guard.Pause(false);
                _guard.Perceiving = true;
                float t0 = Time.time;
                while (_guard.State != WatchState.Chase && Time.time - t0 < 8f) yield return null;
                bool chased = _guard.State == WatchState.Chase;
                yield return Go(5.5f, false, new Vector3(-11.4f, F, 5.6f), new Vector3(-14.2f, F, 5.6f));
                float safeAt = Time.time - t0;
                bool safe = SafeZone.PlayerSafe;
                bool entered = false;
                var states = new List<string>();
                void Changed(Watchman w, WatchState s) => states.Add($"{s}@{Time.time - t0:F1}");
                _guard.StateChanged += Changed;
                float nearest = 99f;
                for (float t = 0f; t < 45f && !(_guard.State == WatchState.Patrol && t > 3f); t += Time.deltaTime)
                {
                    entered |= SafeZone.Contains(_guard.transform.position);
                    nearest = Mathf.Min(nearest, Flat(_guard.transform.position - _kieuDoorOutside.position).magnitude);
                    yield return null;
                }
                _guard.StateChanged -= Changed;
                Report("safe room: chased in through the open door, no catch, he never steps in, searches outside and goes",
                    chased && safe && caught == 0 && !entered && _guard.State == WatchState.Patrol,
                    $"chased {chased} · inside at {safeAt:F1} s (safe {safe}) · caught {caught} · stepped in {entered} · nearest the porch {nearest:F1} m · {string.Join(" → ", states)} · now {_guard.State}");
                _guard.Caught -= Caught;

                // Door shut, a loud sound inside: he comes to the porch, does not open her door or go in.
                Quiet();
                player.Teleport(new Vector3(-14.5f, F + 0.05f, 4.6f), Quaternion.identity);
                PostAt(_guard, new Vector3(-10.5f, F, 3.6f), 0f);
                _guard.Pause(false);
                _guard.Perceiving = true;
                yield return Frames(3);
                Noise.Emit(new Vector3(-13.2f, F, 4.8f), 7f, NoiseKind.Footstep);
                yield return null;
                var target = _guard.Target;
                bool opened = false;
                entered = false;
                for (float t = 0f; t < 20f; t += Time.deltaTime) { opened |= _kieuDoor.Openness > 0.05f; entered |= SafeZone.Contains(_guard.transform.position); yield return null; }
                Report("safe room: a sound inside with the door shut brings him to the porch, the door stays shut and he stays out",
                    !opened && !entered && Flat(target - _kieuDoorOutside.position).magnitude < 0.5f,
                    $"he went to {Flat(target)} (porch {Flat(_kieuDoorOutside.position)}) · door opened {opened} · stepped in {entered} · {_guard.State}");

                // The threshold, steady both ways.
                Quiet();
                _kieuDoor.SetOpen(1f);
                var steps = new List<string>();
                bool thresholdOk = true;
                foreach (var (x, expect) in new[] { (-11.95f, false), (-12.25f, false), (-12.6f, true), (-12.2f, true), (-11.95f, false) })
                {
                    player.Teleport(new Vector3(x, F + 0.05f, 5.6f), Quaternion.Euler(0f, 270f, 0f));
                    yield return Frames(3);
                    thresholdOk &= SafeZone.PlayerSafe == expect;
                    steps.Add($"x {x:F2} (stood at {player.transform.position.x:F2}): {(SafeZone.PlayerSafe ? "safe" : "out")}");
                }
                Report("safe room: the threshold (inside once her body is past the door line, out only when she steps back onto it)", thresholdOk, string.Join(" · ", steps));

                // A pebble thrown out of the room: heard where it lands, outside.
                Quiet();
                _kieuDoor.SetOpen(1f);
                PostAt(_guard, new Vector3(-10.4f, F, 9.3f), 180f);
                _guard.Pause(false);
                _guard.Perceiving = true;
                player.Teleport(new Vector3(-13.4f, F + 0.05f, 5.6f), Quaternion.Euler(0f, 90f, 0f));
                yield return Frames(3);
                _pebbles.ThrowForTest(new Vector3(-13.1f, F + 1.4f, 5.6f), new Vector3(-9.8f, F, 5.6f));
                for (float t = 0f; t < 2f; t += Time.deltaTime) yield return null;
                var land = _pebbles.LastImpact.Point;
                Report("safe room: a pebble thrown out of her room draws him to where it fell, outside",
                    !SafeZone.Contains(land) && _guard.State == WatchState.Investigate && Flat(_guard.Target - land).magnitude < 1f,
                    $"struck {Describe(_pebbles.LastImpact.Collider)} at {Flat(land)} · he {_guard.State} → {Flat(_guard.Target)}");
            }

            // ------------------------------------------------------------------ other doors
            {
                int doorNoises = 0;
                void OnDoorNoise(Noise.Event e) { if (e.Kind == NoiseKind.Door || e.Kind == NoiseKind.Lock) doorNoises++; }
                Noise.Emitted += OnDoorNoise;
                var hands = _guard.GetComponent<DoorUser>();
                var gateHands = _gateGuard.GetComponent<DoorUser>();
                int stage = _stage;

                // An unlocked door on the way to a check: he opens it (no noise), goes in. Tú Bà's room has no other way in.
                Quiet();
                PostAt(_guard, new Vector3(-3.5f, F, 9.8f), 90f);
                _guard.Pause(false);
                int opened = hands.Opened;
                Vector3 inside = new Vector3(-1.5f, F, 12.7f);
                _guard.Investigate(inside, 2f);
                float reached = -1f;
                for (float t = 0f; t < 25f && reached < 0f; t += Time.deltaTime) { if (Flat(_guard.transform.position - inside).magnitude < 1.2f) reached = t; yield return null; }
                Report("doors: a check inside Tú Bà's room (paper door shut, unlocked): he slides it open by hand and goes in, no noise for anyone",
                    hands.Opened == opened + 1 && roomDoor.IsOpen && roomDoor.MovedByNpc && reached >= 0f && doorNoises == 0,
                    $"opened {hands.Opened - opened} · door {roomDoor.Openness:F2} · got in after {(reached < 0f ? "never" : reached.ToString("F1") + " s")} · door noises {doorNoises}");

                // A locked door: left shut, he checks as near as he can get.
                Quiet();
                PostAt(_guard, new Vector3(8.0f, F, 9.8f), 90f);
                _guard.Pause(false);
                opened = hands.Opened;
                _guard.Investigate(_officeInside.position, 2f);
                for (float t = 0f; t < 15f; t += Time.deltaTime) yield return null;
                Report("doors: a check behind the locked office door: the door stays shut, he stops outside it",
                    hands.Opened == opened && _officeDoor.Openness < 0.05f && hands.LastRefused == _officeDoor,
                    $"opened {hands.Opened - opened} · office door {_officeDoor.Openness:F2} ({_officeDoor.Lock}) · refused {(hands.LastRefused != null ? hands.LastRefused.name : "none")} · he is {Flat(_guard.transform.position - _officeDoorOutside.position).magnitude:F1} m from its porch, {_guard.State}");

                // Two guards to the same shut door: one hand on it, the other waits and goes through.
                Quiet();
                PostAt(_guard, new Vector3(-3.5f, F, 9.8f), 90f);
                PostAt(_gateGuard, new Vector3(3.5f, F, 9.8f), 270f);
                _guard.Pause(false);
                _gateGuard.Pause(false);
                opened = hands.Opened + gateHands.Opened;
                Vector3 inside2 = inside + Vector3.right * 2.4f;
                _guard.Investigate(inside, 2f);
                _gateGuard.Investigate(inside2, 2f);
                bool bothIn = false;
                for (float t = 0f; t < 25f && !bothIn; t += Time.deltaTime)
                {
                    bothIn = Flat(_guard.transform.position - inside).magnitude < 1.5f && Flat(_gateGuard.transform.position - inside2).magnitude < 1.5f;
                    yield return null;
                }
                Report("doors: two guards at one shut door: it is opened once, both get through",
                    hands.Opened + gateHands.Opened - opened == 1 && bothIn, $"opened {hands.Opened + gateHands.Opened - opened} times · both inside {bothIn}");

                // A round through the store: he opens it, pulls it to behind him; not while Kiều stands in its swing.
                Quiet();
                var roundOut = Point("ProbeRoundOut", new Vector3(-0.6f, F, 9.5f), 0f);
                var roundIn = Point("ProbeRoundIn", new Vector3(-1.5f, F, 13.0f), 180f);
                _guard.SetRoute(new[] { roundOut, roundIn }, new[] { 2f, 2f });
                _guard.ResetAt(0, 0.5f);
                _guard.Pause(false);
                int closed = hands.Closed;
                opened = hands.Opened;
                for (float t = 0f; t < 30f && hands.Closed == closed; t += Time.deltaTime) yield return null;
                bool closedBehind = hands.Closed > closed && Flat(_guard.transform.position - roundIn.position).magnitude < 3f;
                // Out again; Kiều steps into the doorway as he goes: he must not shut it on her.
                for (float t = 0f; t < 25f && !(roomDoor.IsOpen && Flat(_guard.transform.position - roomDoor.Centre).magnitude > 0.9f && _guard.transform.position.z < 10.9f); t += Time.deltaTime) yield return null;
                player.Teleport(roomDoor.Centre + Vector3.back * 0.3f + Vector3.up * 0.05f, Quaternion.identity);
                int closedBefore = hands.Closed;
                for (float t = 0f; t < 6f; t += Time.deltaTime) yield return null;
                bool spared = hands.Closed == closedBefore || roomDoor.IsOpen;
                Report("doors: on his round he pulls the door to behind him, and not on Kiều standing in it",
                    closedBehind && spared && doorNoises == 0, $"opened {hands.Opened - opened} · closed behind him {closedBehind} · with Kiều in the doorway the door is {(roomDoor.IsOpen ? "open" : "shut")} · door noises {doorNoises}");
                Report("doors: a guard's hand on doors moves no story step", _stage == stage, $"stage {stage} → {_stage}");
                Noise.Emitted -= OnDoorNoise;
                Destroy(roundOut.gameObject);
                Destroy(roundIn.gameObject);
            }

            // ------------------------------------------------------------------ a chase a pebble does not hijack
            {
                Quiet();
                PostAt(_guard, centre, yaw);
                Vector3 ahead = centre + Quaternion.Euler(0f, yaw, 0f) * Vector3.forward * 3f;
                player.Teleport(ahead + Vector3.up * 0.05f, Quaternion.Euler(0f, yaw, 0f));
                _guard.Pause(false);
                _guard.Perceiving = true;
                float t0 = Time.time;
                while (_guard.State != WatchState.Chase && Time.time - t0 < 6f) yield return null;
                bool chasing = _guard.State == WatchState.Chase;
                Vector3 side = centre + Quaternion.Euler(0f, yaw - 90f, 0f) * Vector3.forward * 5f;
                _pebbles.ThrowForTest(_guard.transform.position + Vector3.up * 1.5f + Quaternion.Euler(0f, yaw - 90f, 0f) * Vector3.forward, side);
                float stillChasing = 0f, nearestToPebble = 99f;
                for (float t = 0f; t < 2.5f; t += Time.deltaTime)
                {
                    player.ScriptedSpeed = 3f;
                    player.ScriptedMove = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
                    if (_guard.State == WatchState.Chase) stillChasing += Time.deltaTime;
                    nearestToPebble = Mathf.Min(nearestToPebble, Flat(_guard.LastKnown - _pebbles.LastImpact.Point).magnitude);
                    yield return null;
                }
                player.ScriptedMove = null;
                Report("pebble during a chase: he keeps after her, the pebble does not become where she is",
                    chasing && stillChasing > 2f && nearestToPebble > 2f, $"chasing {chasing} · in chase {stillChasing:F1} of 2.5 s after the pebble · his last-known nearest the pebble {nearestToPebble:F1} m");
            }

            // ------------------------------------------------------------------ calling the other guard (after the alarm)
            {
                Quiet();
                PostAt(_guard, new Vector3(-3.0f, F, 9.7f), 270f);
                PostAt(_gateGuard, new Vector3(-10.5f, F, -4.0f), 0f);
                // The calls of the alarm's arrangement, each guard on a post of his own for the test.
                _guard.Ally = _gateGuard;
                _gateGuard.Ally = _guard;
                _guard.CallsAlly = _gateGuard.CallsAlly = true;
                player.Teleport(new Vector3(-5.6f, F + 0.05f, 9.7f), Quaternion.Euler(0f, 270f, 0f));
                Lantern(_guard, true);
                _guard.Pause(false);
                _gateGuard.Pause(false);
                _guard.Perceiving = true;
                _gateGuard.Perceiving = true;
                float t0 = Time.time;
                while (_guard.State != WatchState.Chase && Time.time - t0 < 6f) yield return null;
                yield return null;
                Vector3 told = _gateGuard.Target;
                Vector3 seenAt = _guard.LastKnown;
                bool called = _gateGuard.State == WatchState.Investigate && Flat(told - seenAt).magnitude < 0.6f;
                // She slips away west and north; the called guard keeps to the place he was told unless he sees her himself.
                float drift = 0f;
                bool sawHer = false;
                player.ForceCrouch = false;
                for (float t = 0f; t < 3f; t += Time.deltaTime)
                {
                    player.ScriptedSpeed = 3.5f;
                    player.ScriptedMove = new Vector3(-1f, 0f, 0.2f);
                    sawHer |= _gateGuard.SeesPlayer;
                    if (!sawHer && _gateGuard.State == WatchState.Investigate) drift = Mathf.Max(drift, Flat(_gateGuard.Target - told).magnitude);
                    yield return null;
                }
                player.ScriptedMove = null;
                Report("after the alarm: the guard who sees her calls the other to where she was seen, not to where she goes",
                    called && (sawHer || drift < 0.6f), $"called {called} (told {Flat(told)}, seen at {Flat(seenAt)}) · his target moved {drift:F1} m while he had not seen her · saw her himself {sawHer}");
                SetRoles(_j2Done, false);
            }

            // ------------------------------------------------------------------ the lure out and back, and the way in, the keys, the way out
            Quiet();
            SetRoles(false, false);
            _guard.ResetAt(0);
            _gateGuard.ResetAt(0);
            {
                var post = _guardPost.Points[0].position;
                // Crouched in the corridor, at the nearest spot west of the door he cannot make out from his post (the dark
                // shortens his sight; a lamp lengthens it).
                Vector3 hide = new Vector3(-9f, F, 9.3f);
                Vector3 eyePost = post + Vector3.up * 1.62f;
                for (float x = -3.5f; x >= -9f; x -= 0.25f)
                {
                    Vector3 p = new Vector3(x, F, 9.3f);
                    float lightHere = LightExposure.At(p + Vector3.up * 0.6f);
                    if (Vector3.Distance(eyePost, p + Vector3.up * 0.95f) > SightModel.EffectiveRange(_guard.Range, lightHere) + 0.5f) { hide = p; break; }
                }
                Info($"GUARDS (hiding at {Flat(hide)}, light {LightExposure.At(hide + Vector3.up * 0.6f):F2})");
                player.Teleport(hide + Vector3.up * 0.05f, Quaternion.Euler(0f, 110f, 0f));
                player.ForceCrouch = true;
                _guard.Pause(false);
                _gateGuard.Pause(false);
                _guard.Perceiving = true;
                _gateGuard.Perceiving = true;
                foreach (var w in _watchers) w.Sensitivity = 1f;
                _guard.Sensitivity = 0.85f + 0.15f * _director.GetInt("c4_canh_giac");
                int scenes = CatchScene.Count;
                int caught = 0;
                float lastCatch = -9f;
                void Caught(Watchman _) { if (Time.time - lastCatch > 2f) caught++; lastCatch = Time.time; }
                _guard.Caught += Caught;
                bool everSaw = false;
                float peak = 0f;
                var states = new List<string>();
                float t0 = Time.time;
                void Changed(Watchman w, WatchState s) => states.Add($"{s}@{Time.time - t0:F1}");
                _guard.StateChanged += Changed;
                for (float t = 0f; t < 2f; t += Time.deltaTime) { everSaw |= _guard.SeesPlayer; yield return null; }
                Vector3 aim = new Vector3(-0.5f, 0f, 4.2f);
                for (int i = 0; i < 15; i++) { _director.CameraController.LookAtNow(aim); yield return null; }
                _pebbles.Refill();
                InputReader.SimulateThrow();
                for (int i = 0; i < 3; i++) { _director.CameraController.LookAtNow(aim); yield return null; }
                float thrownAt = Time.time;
                float left = -1f, back = -1f, keysAt = -1f, outAt = -1f, nearestToHer = 99f;
                // Wait for him to leave his post.
                for (float t = 0f; t < 8f && left < 0f; t += Time.deltaTime)
                {
                    if (Flat(_guard.transform.position - post).magnitude > 1.5f) left = Time.time;
                    everSaw |= _guard.SeesPlayer;
                    yield return null;
                }
                var impact = _pebbles.LastImpact;
                Report("lure: a pebble on the yard draws him from his post to where it fell (not to her)",
                    left > 0f && Flat(_guard.Target - impact.Point).magnitude < 2.5f, $"struck {impact.Surface} at {Flat(impact.Point)} r {impact.Radius:F1} m · he left {(left < 0f ? "never" : (left - thrownAt).ToString("F1") + " s after")} · going to {Flat(_guard.Target)} · {string.Join(" → ", states)}");
                if (left > 0f)
                {
                    // The way in while he is away: along the corridor, the paper door eased open, the keys, and out the same way.
                    bool sneaking = true;
                    IEnumerator Watch()
                    {
                        while (sneaking)
                        {
                            everSaw |= _guard.SeesPlayer;
                            peak = Mathf.Max(peak, _guard.Awareness);
                            nearestToHer = Mathf.Min(nearestToHer, Flat(_guard.transform.position - player.transform.position).magnitude);
                            if (back < 0f && _guard.State == WatchState.Patrol && Flat(_guard.transform.position - post).magnitude < 1.2f) back = Time.time;
                            yield return null;
                        }
                    }
                    StartCoroutine(Watch());
                    yield return Creep(new Vector3(-3.0f, F, 9.9f), _tuBaDoorOutside.position);
                    float atDoor = Time.time;
                    player.ForceCrouch = true;
                    // Eased open far enough to slip through (a little over half of the paper panel), in, and eased shut behind
                    // her so the lamp-lit room is not seen from the yard.
                    yield return WorkDoor(_tuBaDoor, () => _tuBaDoor.Openness >= 0.5f, 10f);
                    float doorOpen = _tuBaDoor.Openness;
                    for (float t = 0f; t < 0.8f; t += Time.deltaTime) yield return null; // the floor clears where the panel stood
                    float doorDone = Time.time;
                    yield return Creep(_tuBaDoorInside.position + Vector3.forward * 0.5f);
                    yield return WorkDoor(_tuBaDoor, () => _tuBaDoor.Openness <= 0.05f, 8f);
                    float shutBehind = _tuBaDoor.Openness;
                    yield return Creep(KeyStand());
                    float atKeys = Time.time;
                    yield return AutoTakeKeys();
                    keysAt = Time.time;
                    // Out the way the night goes on: the bolt of the inner door drawn slowly, the door eased, into the office.
                    Vector3 across = _innerDoor.transform.right * Mathf.Sign(Vector3.Dot(player.transform.position - _innerDoor.Centre, _innerDoor.transform.right));
                    // Round the tea table and its cushions (on the straight line from the sập to the inner door).
                    yield return Creep(new Vector3(1.0f, F, 14.2f), new Vector3(6.2f, F, 14.4f), _innerDoor.Centre + across * 0.8f);
                    yield return WorkDoor(_innerDoor, () => _innerDoor.Lock == DoorLock.None, 8f);
                    yield return WorkDoor(_innerDoor, () => _innerDoor.Openness >= 0.6f, 8f);
                    for (float t = 0f; t < 0.8f; t += Time.deltaTime) yield return null;
                    yield return Creep(_innerDoor.Centre - across * 1.5f);
                    outAt = Time.time;
                    Info($"GUARDS (paper door eased to {doorOpen:F2}, shut behind her to {shutBehind:F2}; inner door: lock {_innerDoor.Lock}, open {_innerDoor.Openness:F2}; she is at {Flat(player.transform.position)})");
                    Info($"GUARDS (inner door: lock {_innerDoor.Lock}, open {_innerDoor.Openness:F2}; she is at {Flat(player.transform.position)})");
                    Info($"GUARDS (the way in, s after he left: at the door {atDoor - left:F1}, door open {doorDone - left:F1}, at the keys {atKeys - left:F1}, keys {Time.time - left - (outAt - keysAt):F1}, in the office {outAt - left:F1})");
                    for (float t = 0f; t < 70f && back < 0f; t += Time.deltaTime) yield return null;
                    sneaking = false;
                    bool keys = _inventory.Has("chia");
                    Report("lure: the way in (door eased open), the keys through the minigame and out through the inner door to the office, unseen",
                        keys && _innerDoor.Openness > 0.4f && player.transform.position.x > _innerDoor.Centre.x && peak < Watchman.SuspiciousAt && caught == 0 && CatchScene.Count == scenes,
                        $"door opened to {doorOpen:F2} · keys {keys} {(keysAt - left):F1} s after he left · in the office {(outAt - left):F1} s after · in his sight at some moment {everSaw} (awareness peak {peak:F2}, under the {Watchman.SuspiciousAt:F2} that turns his head; nearest {nearestToHer:F1} m) · catches {caught + CatchScene.Count - scenes}");
                    Report("lure: he checks, looks round and walks back to his post (45–60 s from leaving it to standing at it again)",
                        back > 0f && back - left >= 40f && back - left <= 70f, $"away {(back < 0f ? "and not back in time" : (back - left).ToString("F1") + " s")} · {string.Join(" → ", states)}");
                }
                _guard.StateChanged -= Changed;
                _guard.Caught -= Caught;
            }

            // ------------------------------------------------------------------ the lure limit: the fourth pebble in a while only turns his head
            {
                Quiet();
                SetRoles(false, false);
                _guard.ResetAt(0);
                _guard.Pause(false);
                _guard.Perceiving = true;
                Time.timeScale = 3f;
                var post = _guardPost.Points[0].position;
                var results = new List<string>();
                int drawn = 0;
                string last = "";
                for (int i = 0; i < 7 && drawn < 3; i++)
                {
                    _pebbles.ThrowForTest(new Vector3(-1.0f, F + 1.6f, 7.4f), new Vector3(-1.0f + (i % 3) * 0.6f, 0f, 3.0f));
                    for (float t = 0f; t < 2.5f; t += Time.deltaTime) yield return null;
                    var hit = _pebbles.LastImpact;
                    results.Add($"{_guard.State} ({hit.Surface} at {Flat(hit.Point)} r {hit.Radius:F1})");
                    if (_guard.State == WatchState.Investigate) drawn++;
                    for (float t = 0f; t < 120f && !(_guard.State == WatchState.Patrol && Flat(_guard.transform.position - post).magnitude < 1.2f); t += Time.deltaTime) yield return null;
                }
                _pebbles.ThrowForTest(new Vector3(-1.0f, F + 1.6f, 7.4f), new Vector3(-0.4f, 0f, 3.0f));
                for (float t = 0f; t < 2.5f; t += Time.deltaTime) yield return null;
                last = _guard.State.ToString();
                Time.timeScale = 1f;
                Report("lure limit: three pebbles in a while draw him, the next only turns his head",
                    drawn == 3 && last == "Suspicious", string.Join(", ", results) + $" · then {last} · lures remembered {_guard.RecentLures}");
            }

            // ------------------------------------------------------------------ J2: the two rounds, from where they stand; checkpoints
            {
                Quiet();
                _director.Fail("QA", "probe: restore before the alarm");
                for (float t = 0f; t < 10f && !_director.InExplore; t += Time.unscaledDeltaTime) yield return null;
                yield return Frames(3);
                bool postBack = _guard.Waypoints == _guardPost.Points && _gateGuard.Waypoints == _gatePost.Points && !_guard.CallsAlly
                                && Flat(_guard.transform.position - _guardPost.Points[0].position).magnitude < 1f;
                Report("checkpoint before the alarm: the guard at his post, the gate guard at the gate, no calls; Kiều safe in her room if she starts there",
                    postBack && (SpawnFor(_stage) != _spawnRoom || SafeZone.PlayerSafe), $"stage {_stage} · post {postBack} · safe {SafeZone.PlayerSafe}");

                // J2 itself is left to the chapter: the autopilot plays the contract, J1 and J2 for real after this probe, and
                // a watcher kept on the director (it outlives the chapter's coroutines across a checkpoint) notes how the
                // guards switch to their rounds.
                _director.StartCoroutine(WatchAlarm());
            }

            Quiet();
            foreach (var pair in doors) pair.Key.Load(pair.Value);
            foreach (var w in _watchers) { w.Sensitivity = 1f; w.Perceiving = false; w.Pause(false); }
            _probing = false;
            ArmGuard();
            Time.timeScale = timeScale;
            Info($"GUARDS probe done · {Time.realtimeSinceStartup - realStart:F0} s real time, {Time.realtimeSinceStartup:F0} s since start");
            StoryAutopilot.ProbeTook(Time.realtimeSinceStartup - realStart);
        }

        /// <summary>
        /// J2 as the chapter plays it after the probe: the guards keep their posts until Tú Bà raises the alarm, then walk to
        /// their rounds from where they stand (no jump), call each other; Tú Bà leaves Kiều's door; a checkpoint restores it.
        /// </summary>
        private IEnumerator WatchAlarm()
        {
            _watchingAlarm = true;
            try { yield return WatchAlarmSteps(); }
            finally { _watchingAlarm = false; }
        }

        /// <summary>QA: holds the autopilot's next step while the alarm is being watched (so it does not leave by the back door first).</summary>
        private bool _watchingAlarm;

        private IEnumerator WatchAlarmSteps()
        {
            void Report(string test, bool ok, string detail)
            {
                Info("GUARDS " + (ok ? "ok   " : "FAIL ") + test + " · " + detail);
                if (!ok) StoryAutopilot.Problem("[GuardProbe] " + test + " · " + detail);
            }
            while (!_j2Running && !_ended) yield return null;
            if (_ended) yield break;
            bool earlySwitch = _guard.Waypoints != _guardPost.Points || _gateGuard.Waypoints != _gatePost.Points;
            Vector3 g0 = _guard.transform.position, k0 = _gateGuard.transform.position;
            float jump = 0f, t0 = Time.time, switchedAt = -1f;
            void Track()
            {
                jump = Mathf.Max(jump, (_guard.transform.position - g0).magnitude, (_gateGuard.transform.position - k0).magnitude);
                g0 = _guard.transform.position;
                k0 = _gateGuard.transform.position;
                if (switchedAt < 0f && _guard.Waypoints == _guardAlarm.Points) switchedAt = Time.time - t0;
            }
            while (!_j2Done && Time.time - t0 < 90f && !_ended) { Track(); yield return null; }
            bool roles = _guard.Waypoints == _guardAlarm.Points && _gateGuard.Waypoints == _gateAlarm.Points && _guard.CallsAlly && _gateGuard.CallsAlly
                         && _guard.Ally == _gateGuard && _gateGuard.Ally == _guard;
            for (float t = 0f; t < 25f && !_ended; t += Time.deltaTime) { Track(); yield return null; }
            float gateMoved = Flat(_gateGuard.transform.position - _gatePost.Points[0].position).magnitude;
            Report("J2: only when Tú Bà raises the alarm do both guards take their rounds, walking from where they stand",
                !earlySwitch && switchedAt >= 0f && roles && jump < 0.5f && gateMoved > 2f,
                $"posts kept until J2 {!earlySwitch} · switched {switchedAt:F1} s into J2 · roles {roles} · largest step in one frame {jump:F2} m · gate guard {gateMoved:F1} m from the gate 25 s later");
            for (float t = 0f; t < 20f && _tuBaPhase < 2 && !_ended; t += Time.deltaTime) yield return null;
            Report("J2: Tú Bà shouts at Kiều's door from outside and leaves within 15 s", _tuBaPhase >= 2 && !SafeZone.Contains(_tuBa.transform.position),
                $"phase {_tuBaPhase} · she is at {Flat(_tuBa.transform.position)}");
            if (_ended || !_director.InExplore) yield break;
            _director.Fail("QA", "probe: restore after the alarm");
            for (float t = 0f; t < 10f && !_director.InExplore; t += Time.unscaledDeltaTime) yield return null;
            for (int i = 0; i < 3; i++) yield return null;
            roles = _guard.Waypoints == _guardAlarm.Points && _gateGuard.Waypoints == _gateAlarm.Points && _guard.CallsAlly && _gateGuard.CallsAlly;
            Report("checkpoint after the alarm: both on their rounds, calling each other", roles && _j2Done, $"roles {roles} · J2 done {_j2Done} · stage {_stage}");
        }

        // ------------------------------------------------------------------ helpers

        /// <summary>One cell of the sight table: the guard held facing <paramref name="yaw"/>, Kiều still at the offset, timed.</summary>
        private IEnumerator SightCase(Watchman w, Vector3 centre, float yaw, float angle, float distance, bool lit, bool crouch, float limit, SightResult r)
        {
            var player = _director.Player;
            foreach (var other in _watchers) { other.Perceiving = false; other.Pause(true); }
            Vector3 at = centre + Quaternion.Euler(0f, yaw + angle, 0f) * Vector3.forward * distance;
            player.ScriptedMove = null;
            player.ForceCrouch = crouch;
            player.Teleport(at + Vector3.up * 0.05f, Quaternion.Euler(0f, yaw + angle, 0f));
            Lantern(w, lit);
            PostAt(w, centre, yaw);
            for (int i = 0; i < 4; i++) yield return null;
            r.Light = LightExposure.At(player.transform.position + Vector3.up * (crouch ? 0.6f : 1.05f));
            w.Forget();
            Hold(w, yaw);
            w.Perceiving = true;
            w.Pause(false);
            for (float t = 0f; t < limit; t += Time.deltaTime)
            {
                if (w.State == WatchState.Scripted) Hold(w, yaw);
                if (w.SeesPlayer && r.FirstSeen < 0f) r.FirstSeen = t;
                if (w.Awareness >= Watchman.InvestigateAt && r.Check < 0f) { r.Check = t; r.Felt = !w.SeesPlayer && r.FirstSeen < 0f; }
                if (w.Awareness >= 1f) { r.Sure = t; break; }
                yield return null;
            }
            w.Perceiving = false;
            w.Pause(true);
            Lantern(w, true);
        }

        /// <summary>Keeps him where he stands, facing one way (a story walk to his own feet, so his round does not turn him).</summary>
        private void Hold(Watchman w, float yaw)
        {
            if (w.State != WatchState.Scripted) w.Script(w.transform.position, 1f, null);
            w.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        /// <summary>A post of his own for the test (one stop, a long pause), placed there calm.</summary>
        private void PostAt(Watchman w, Vector3 at, float yaw)
        {
            if (!_posts.TryGetValue(w, out var post) || post == null) _posts[w] = post = new GameObject("ProbePost_" + w.name).transform;
            post.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            w.SetRoute(new[] { post }, new[] { 999f });
            w.ResetAt(0, 999f);
        }

        private void Park(Watchman w, Vector3 at) { PostAt(w, at, 0f); w.Pause(true); }

        private static void Lantern(Watchman w, bool lit) { foreach (var light in w.GetComponentsInChildren<Light>(true)) light.enabled = lit; }

        private Transform Point(string name, Vector3 at, float yaw)
        {
            var point = new GameObject(name).transform;
            point.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            return point;
        }

        /// <summary>
        /// Open, dark floor for the sight table: of a few candidates in the hall, the one with 4.5 m clear around it at chest
        /// height and the least light with the lanterns out.
        /// </summary>
        private Vector3 SightCentre(out string why)
        {
            foreach (var w in new[] { _guard, _gateGuard }) Lantern(w, false);
            var best = Vector3.zero;
            float bestLight = 9f;
            int candidates = 0;
            why = "none clear";
            for (float x = -20f; x <= 20f; x += 1f)
                for (float z = -18f; z <= 18f; z += 1f)
                {
                    if (!UnityEngine.AI.NavMesh.SamplePosition(new Vector3(x, 0.5f, z), out var hit, 0.6f, UnityEngine.AI.NavMesh.AllAreas)) continue;
                    Vector3 c = hit.position;
                    if (SafeZone.Contains(c)) continue;
                    bool clear = true;
                    for (int a = 0; a < 360 && clear; a += 45)
                    {
                        Vector3 dir = Quaternion.Euler(0f, a, 0f) * Vector3.forward;
                        foreach (float h in new[] { 0.5f, 1.0f, 1.6f })
                            clear &= !Physics.SphereCast(c + Vector3.up * h, 0.25f, dir, out _, 4.5f, 1 << 8, QueryTriggerInteraction.Ignore);
                        // The floor where she would stand is there, at the same height.
                        clear &= UnityEngine.AI.NavMesh.SamplePosition(c + dir * 4f, out var far, 0.3f, UnityEngine.AI.NavMesh.AllAreas) && Mathf.Abs(far.position.y - c.y) < 0.1f;
                    }
                    if (!clear) continue;
                    candidates++;
                    float light = 0f;
                    for (int a = 0; a < 360; a += 45) light = Mathf.Max(light, LightExposure.At(c + Quaternion.Euler(0f, a, 0f) * Vector3.forward * 2f + Vector3.up * 0.8f));
                    if (light < bestLight) { bestLight = light; best = c; why = $"{Flat(c)} (y {c.y:F2}), clear 4.5 m all round at three heights, light with the lanterns out ≤ {light:F2} ({candidates} open spots found)"; }
                }
            foreach (var w in new[] { _guard, _gateGuard }) Lantern(w, true);
            return best;
        }

        private static string StripTags(string text) => System.Text.RegularExpressions.Regex.Replace(text ?? "", "<[^>]+>", "");

        private static string Describe(Collider c) => c == null ? "nothing" : c.transform.parent != null ? c.transform.parent.name + "/" + c.name : c.name;
    }
}
