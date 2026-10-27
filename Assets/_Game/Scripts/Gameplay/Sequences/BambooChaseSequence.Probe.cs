using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThuyKieu.Core;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Chase;
using ThuyKieu.UI;
using UnityEngine;

namespace ThuyKieu.Gameplay.Sequences
{
    // QA for the bamboo chase. The pilot plays through the same input the player has (mouse look, Space, Ctrl, E, all
    // through InputReader): it never moves Kiều itself, never lengthens a jump or the catch reach, and only knows the
    // course the way a player who has run it before knows it.
    public partial class BambooChaseSequence
    {
        private sealed class Pilot
        {
            public int Way = 1;
            /// <summary>
            /// Obstacle indices it reacts to late, on purpose: it runs into the log or the low culms and only then jumps or
            /// ducks: about 0.9 s after tripping on a log, 0.55 s after running into low culms (a ditch missed is a fall). The
            /// mistake a player makes who sees it too late.
            /// </summary>
            public readonly HashSet<int> Miss = new HashSet<int>();
            /// <summary>Obstacle index of a low culm it jumps at instead of ducking (head against the culm).</summary>
            public int JumpUnder = -1;
            /// <summary>Obstacle index of a low culm under which it lets go of Ctrl halfway.</summary>
            public int LetGoUnder = -1;
            public bool Kick = true;
            /// <summary>Steers into the bank on this side (−1 left, +1 right) and jumps whenever it can, away from obstacles.</summary>
            public int Hug;
            /// <summary>Until then (Time.time) a hugging pilot runs the way normally: it got stuck against the bank and backs off.</summary>
            public float HugRestUntil;
            public int Jumps, HeldUp;
            public bool StoodUnder;
            private readonly HashSet<int> _done = new HashSet<int>();
            private bool _pressed;
            private float _traceAt;

            public void Step(BambooChaseSequence s)
            {
                var course = s._course;
                var player = s._director.Player;
                var camera = s._director.CameraController;
                Vector3 feet = player.transform.position;
                float at = course.Project(Way, feet, out float wide);
                if (Time.time >= _traceAt)
                {
                    _traceAt = Time.time + 4f;
                    StoryAutopilot.Note($"pilot way {Way} at {at:F0} m · off the line {wide:F1} m · above it {feet.y - course.PointAt(Way, at).y:F2} m · gap {s._pursuer.PathDistance:F1} m");
                }
                bool kick = Kick && s._bridge.OnFarBank(feet) && !s._bridge.Dropped;
                Steer(camera, feet, kick ? s._bridge.Stake.position : Aim(course, at));

                bool crouch = false, clear = true;
                var obstacles = course.Obstacles;
                for (int i = 0; i < obstacles.Length; i++)
                {
                    var o = obstacles[i];
                    if (o.Way != 0 && o.Way != Way) continue;
                    float d = course.Project(Way, o.Position, out float off) - at;
                    if (off > 4f) continue;
                    if (d > -2f && d < o.Length * 0.5f + 6f) clear = false;
                    float half = o.Length * 0.5f;
                    if (Miss.Contains(i))
                    {
                        if (o.Kind == ChaseCourse.ObstacleKind.Jump || o.Kind == ChaseCourse.ObstacleKind.Hazard)
                        {
                            if (d - half < 0.4f && d > -0.5f && player.Stumbling > 0f && player.Stumbling < 0.12f && _done.Add(i)) { InputReader.SimulateJump(); Jumps++; }
                        }
                        else if (o.Kind == ChaseCourse.ObstacleKind.Duck && ((player.Stumbling > 0f && player.Stumbling < 0.1f) || player.IsCrouching)) crouch |= d - half < 0.4f && d + half > -0.6f;
                        continue;
                    }
                    switch (o.Kind)
                    {
                        case ChaseCourse.ObstacleKind.Jump:
                        case ChaseCourse.ObstacleKind.Hazard:
                            if (d - half > 0.6f && d - half < 1.5f && _done.Add(i)) { InputReader.SimulateJump(); Jumps++; }
                            break;
                        case ChaseCourse.ObstacleKind.Gap:
                            if (d - half > 0.1f && d - half < 0.8f && _done.Add(i)) { InputReader.SimulateJump(); Jumps++; }
                            break;
                        case ChaseCourse.ObstacleKind.Duck:
                            if (i == JumpUnder) { if (d - half < 1.4f && d - half > 0.4f && _done.Add(i)) InputReader.SimulateJump(); break; }
                            bool under = d - half < 1.3f && d + half > -0.6f;
                            if (i == LetGoUnder && Mathf.Abs(d) < half * 0.6f)
                            {
                                // Ctrl released under the culm: she must stay down until there is room to stand.
                                HeldUp++;
                                if (!player.IsCrouching) StoodUnder = true;
                                break;
                            }
                            crouch |= under;
                            break;
                    }
                }
                InputReader.SimulatedCrouch = crouch;
                if (Hug != 0 && Time.time >= HugRestUntil && clear && player.IsGrounded && !crouch) { InputReader.SimulateJump(); Jumps++; }
                if (_pressed) { InputReader.SimulatedInteract = false; _pressed = false; }
                else if (kick && s._bridge.IsAvailable) { InputReader.SimulatedInteract = true; _pressed = true; }
            }

            /// <summary>
            /// Where to run: the way 3.5 m ahead, moved to the open side from 9 m before a sidestep until just past it (the
            /// side a player who has run the course takes; the rock is seen from much further).
            /// </summary>
            private Vector3 Aim(ChaseCourse course, float at)
            {
                Vector3 aim = course.PointAt(Way, at + 3.5f);
                // A sidestep's rock fills one side of the way: past it the bank is tried again.
                bool rock = course.Obstacles.Any(o => o.Kind == ChaseCourse.ObstacleKind.Dodge && (o.Way == 0 || o.Way == Way)
                                                       && Mathf.Abs(course.Project(Way, o.Position, out _) - at - 3f) < 7f);
                if (Hug != 0 && Time.time >= HugRestUntil && !rock)
                {
                    // Into the bank at a slant, as someone trying to get out of the way would.
                    Vector3 ahead = course.DirectionAt(Way, at + 3.5f);
                    aim += new Vector3(ahead.z, 0f, -ahead.x) * Hug * 3.2f;
                    return aim;
                }
                foreach (var o in course.Obstacles)
                {
                    if (o.Kind != ChaseCourse.ObstacleKind.Dodge || (o.Way != 0 && o.Way != Way)) continue;
                    float so = course.Project(Way, o.Position, out _);
                    float d = so - at;
                    if (d < -1.5f || d > 9f) continue;
                    Vector3 dir = course.DirectionAt(Way, so);
                    var right = new Vector3(dir.z, 0f, -dir.x);
                    float blocked = Mathf.Sign(Vector3.Dot(o.Position - course.PointAt(Way, so), right));
                    aim -= right * blocked * 0.8f;
                }
                return aim;
            }

            /// <summary>Mouse movement that turns the view towards <paramref name="aim"/> at a human pace, eyes level.</summary>
            private static void Steer(ThuyKieu.Player.ThirdPersonCameraController camera, Vector3 feet, Vector3 aim)
            {
                Vector3 to = aim - feet;
                to.y = 0f;
                if (to.sqrMagnitude < 0.01f) { InputReader.SimulatedLook = Vector2.zero; return; }
                float want = Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg;
                float turn = Mathf.Clamp(Mathf.DeltaAngle(camera.Yaw, want), -320f * Time.deltaTime, 320f * Time.deltaTime);
                float perPixel = Mathf.Max(0.0001f, camera.Sensitivity * GameSettings.MouseSensitivity);
                float pitch = (camera.Pitch - 4f) / perPixel * (GameSettings.InvertY ? -1f : 1f);
                InputReader.SimulatedLook = new Vector2(turn / perPixel, Mathf.Clamp(pitch, -40f, 40f));
            }
        }

        /// <summary>
        /// The plan of a normal autopilot run: a random way, cleanly. A thorough run first makes mistake after mistake
        /// until he catches her, so the catch, the failure and the restart are played every time.
        /// </summary>
        private Pilot AutopilotPlan()
        {
            var pilot = new Pilot { Way = Random.Range(1, 3) };
            if (StoryAutopilot.Args.Contains("-autopilotThorough") && Failures == 0)
                foreach (int i in Indices(ChaseCourse.ObstacleKind.Jump, 1).Concat(Indices(ChaseCourse.ObstacleKind.Duck, 1)).Take(6)) pilot.Miss.Add(i);
            return pilot;
        }

        private IEnumerable<int> Indices(ChaseCourse.ObstacleKind kind, int way)
        {
            var obstacles = _course.Obstacles;
            return Enumerable.Range(0, obstacles.Length)
                .Where(i => obstacles[i].Kind == kind && (obstacles[i].Way == 0 || obstacles[i].Way == way))
                .OrderBy(i => _course.Project(way, obstacles[i].Position, out _));
        }

        // ---------------------------------------------------------------- "-autopilotForest"

        private int _fellEvents;

        /// <summary>
        /// Every case the brief lists, played on the real course: clean runs on both ways (time, closest gap, culm warnings),
        /// one mistake (recovered), many mistakes (caught for real), a ditch fall, a jump into a low culm, letting go of Ctrl
        /// under one, reach across a ditch, a pause mid-run, Cô Lam along, restarts. Story variables are put back after.
        /// </summary>
        private IEnumerator Probe()
        {
            float began = Time.realtimeSinceStartup;
            StoryAutopilot.ProbeRunning = true;
            int caughtBefore = _director.GetInt("c5_bi_bat");
            bool lamBefore = _withLam;
            _bridge.Fell += CountFell;
            var jumps = Indices(ChaseCourse.ObstacleKind.Jump, 1).ToList();
            var ducks = Indices(ChaseCourse.ObstacleKind.Duck, 1).ToList();
            var gaps = Indices(ChaseCourse.ObstacleKind.Gap, 1).ToList();
            var culms = _course.GetComponentsInChildren<FallingBamboo>(true);

            // 0. The course itself: how many decisions each way asks for, and the longest run between two of them.
            if (Want("layout"))
                foreach (int way in new[] { 1, 2 })
                {
                    float start = _course.Progress(_course.StartPoint.position);
                    var marks = Enumerable.Range(0, _course.Obstacles.Length).Where(i => _course.Obstacles[i].Way == 0 || _course.Obstacles[i].Way == way)
                        .Select(i => (kind: _course.Obstacles[i].Kind, s: _course.Project(way, _course.Obstacles[i].Position, out _))).OrderBy(m => m.s).ToList();
                    float fork = _course.Project(way, _course.Obstacles.First(o => o.Way == way).Position, out _) - 15f;
                    var decisions = marks.Select(m => m.s).Append(fork).OrderBy(s => s).ToList();
                    float longest = 0f;
                    string where = "";
                    float last = start;
                    foreach (float s in decisions)
                    {
                        if ((s - last) / _runSpeed > longest) { longest = (s - last) / _runSpeed; where = $"{last:F0}→{s:F0} m"; }
                        last = s;
                    }
                    StoryAutopilot.Note($"FOREST layout way {way}: {decisions.Count} decisions ({string.Join(" ", marks.GroupBy(m => m.kind).Select(g => g.Key + " " + g.Count()))} · the fork) · longest clear run {longest:F1} s at {where}");
                    Check(decisions.Count >= 22 && decisions.Count <= 28, $"layout way {way}: {decisions.Count} decisions (22–26 wanted, plus fork and stake)");
                    Check(longest <= 8f, $"layout way {way}: no clear run longer than 8 s ({longest:F1} s)");
                }

            // 1–2. Clean runs, both ways.
            foreach (int way in new[] { 1, 2 }.Where(w => Want("clean" + w)))
            {
                _fellEvents = 0;
                var clean = new Pilot { Way = way };
                var sounds = new SoundLog();
                sounds.Attach(_director.Player);
                sounds.Where = p => { float s = _course.Project(way, p, out float off); var o = _course.Obstacles.OrderBy(x => Vector3.Distance(x.Position, p)).First(); return $"({s:F0} m, {off:F1} off · {o.Kind} {Vector3.Distance(o.Position, p):F1} m away)"; };
                float from = _course.Progress(_course.StartPoint.position);
                yield return Trial(clean);
                sounds.Detach();
                float seconds = Time.time - _runStart;
                CheckSound(sounds, clean, culms, _course.Progress(_director.Player.transform.position) - from, way);
                Check(_outcome == Outcome.Escaped, $"clean run, way {way}: {_outcome} in {seconds:F1} s game time · bumps {_bumps} · jumps {clean.Jumps} · closest {_minGap:F1} m");
                Check(_bumps == 0, $"clean run, way {way}: no bump ({_bumps})");
                Check(_minGap > 3f, $"clean run, way {way}: he never got close ({_minGap:F1} m)");
                Check(_fellEvents == 1, $"clean run, way {way}: the bridge fell once ({_fellEvents})");
                foreach (var culm in culms.Where(c => c.WarnedAt > 0f))
                    Check(culm.LandedAt - culm.WarnedAt >= 1.8f && !culm.HitHer, $"culm {culm.name}: {culm.LandedAt - culm.WarnedAt:F2} s from crack to landing · hit her {culm.HitHer}");
                var crossed = _pursuer.Traverser.Crossed;
                StoryAutopilot.Note($"FOREST pursuer crossed: vault {crossed[0]} · duck {crossed[1]} · leap {crossed[2]} · bridge {crossed[3]}");
            }
            if (Want("clean1") || Want("clean2"))
                Check(_pursuer.Traverser.Crossed[0] > 0 && _pursuer.Traverser.Crossed[1] > 0 && _pursuer.Traverser.Crossed[2] > 0,
                    "pursuer went over, under and across (vault, duck, leap links)");

            // 3. One mistake: runs into the first log, stumbles, keeps ahead.
            if (Want("one"))
            {
                var once = new Pilot();
                once.Miss.Add(jumps[0]);
                yield return Trial(once);
                Check(_outcome == Outcome.Escaped && _bumps >= 1, $"one mistake recovers: {_outcome} · bumps {_bumps} · closest {_minGap:F1} m");
            }

            // 4. Mistake after mistake in one stretch (after the ways meet: a log, the low culms, the falling culm, a log, all
            //    seen too late): he reaches her and the catch plays.
            if (Want("many"))
            {
                var many = new Pilot();
                float merge = _course.Project(1, _course.Obstacles[ducks[ducks.Count - 1]].Position, out _) - 30f;
                foreach (int i in Enumerable.Range(0, _course.Obstacles.Length)
                    .Where(i => _course.Obstacles[i].Way == 0 && (_course.Obstacles[i].Kind == ChaseCourse.ObstacleKind.Jump || _course.Obstacles[i].Kind == ChaseCourse.ObstacleKind.Duck || _course.Obstacles[i].Kind == ChaseCourse.ObstacleKind.Hazard))
                    .Where(i => _course.Project(1, _course.Obstacles[i].Position, out _) > merge).OrderBy(i => _course.Project(1, _course.Obstacles[i].Position, out _)).Take(4))
                    many.Miss.Add(i);
                int before = _director.GetInt("c5_bi_bat");
                yield return Trial(many);
                Check(_outcome == Outcome.Caught, $"many mistakes are caught: {_outcome} · bumps {_bumps} · closest {_minGap:F1} m");
                if (_outcome != Outcome.Escaped) yield return Fail(_outcome);
                Check(_director.GetInt("c5_bi_bat") == before + 1, $"a failure counts once ({before} → {_director.GetInt("c5_bi_bat")})");
            }

            // 5. The ditch: no jump, she falls in; the restart puts everything back.
            if (Want("ditch"))
            {
                var ditch = new Pilot();
                ditch.Miss.Add(gaps[0]);
                yield return Trial(ditch);
                Check(_outcome == Outcome.Fell, $"no jump at the ditch falls in: {_outcome}");
                if (_outcome != Outcome.Escaped) yield return Fail(_outcome);
                bool downBefore = culms.Any(c => c.IsDown) || _bridge.Dropped;
                yield return Begin();
                _director.SetFreeControl(false);
                Check(culms.All(c => !c.IsDown) && !_bridge.Dropped && _director.Player.transform.position.y > -6f,
                    $"a restart puts the culms and the bridge back (something was down before it: {downBefore})");
            }

            // 6. Jumping into a low culm: the head meets it, she does not get through standing.
            if (Want("jumpunder"))
            {
                int heads = 0;
                System.Action head = () => heads++;
                _director.Player.HeadHit += head;
                var jumpUnder = new Pilot { JumpUnder = ducks[0] };
                yield return Trial(jumpUnder, stopAfter: _course.Project(1, _course.Obstacles[ducks[0]].Position, out _) + 6f);
                _director.Player.HeadHit -= head;
                Check(heads + _bumps > 0, $"jumping at a low culm hits it (head {heads}, bumps {_bumps})");
            }

            // 7. Letting go of Ctrl under a low culm: she stays down until there is room.
            if (Want("letgo"))
            {
                var letGo = new Pilot { LetGoUnder = ducks[0] };
                yield return Trial(letGo, stopAfter: _course.Project(1, _course.Obstacles[ducks[0]].Position, out _) + 6f);
                Check(letGo.HeldUp > 0 && !letGo.StoodUnder, $"no standing up under a low culm ({letGo.HeldUp} frames checked, stood {letGo.StoodUnder})");
            }

            // 8. Reach: across the ditch nothing, on the same side yes.
            if (Want("reach"))
            {
                yield return Begin();
                _director.SetFreeControl(false);
                var gap = _course.Obstacles[gaps[0]];
                Vector3 side = Vector3.Cross(Vector3.up, gap.Forward).normalized * 0.2f;
                Vector3 near = gap.Position - gap.Forward * (gap.Length * 0.5f + 1.2f) + side;
                Vector3 far = gap.Position + gap.Forward * (gap.Length * 0.5f + 0.2f) + side;
                _pursuer.Begin(_director.Player, near, Quaternion.LookRotation(gap.Forward));
                _pursuer.Halt();
                _director.Player.Teleport(Ground(far), Quaternion.LookRotation(gap.Forward));
                yield return new WaitForSeconds(0.1f);
                Check(!_pursuer.CouldReach(_director.Player), $"no reach across the ditch ({Flat(_pursuer.transform.position, _director.Player.transform.position):F2} m flat)");
                // Right behind where the NavMesh put him, on the same side of the ditch.
                _director.Player.Teleport(Ground(_pursuer.transform.position - gap.Forward * 1f), Quaternion.LookRotation(gap.Forward));
                yield return new WaitForSeconds(0.1f);
                Check(_pursuer.CouldReach(_director.Player), $"reach on the same side ({Flat(_pursuer.transform.position, _director.Player.transform.position):F2} m flat · {_pursuer.Refusal ?? "reached"} · him {_pursuer.transform.position:F1} · her {_director.Player.transform.position:F1} · lip {near:F1})");
            }

            // 8b. The banks hold her: running into either bank the whole way, jumping whenever she can, she never gets up
            //     out of the way (the pursuer is held back so the run lasts the whole course).
            if (Want("walls"))
            {
                var ground = FindAnyObjectByType<ThuyKieu.Environment.ForestGround>();
                foreach (int way in new[] { 1, 2 })
                    foreach (int side in new[] { -1, 1 })
                    {
                        var hug = new Pilot { Way = way, Hug = side, Kick = false };
                        yield return Begin();
                        _pursuer.Halt();
                        string under = "";
                        var trace = new Queue<string>();
                        float traceAt = 0f;
                        bool traced = false;
                        float worst = -9f, high = 0f, worstAt = 0f, end = _course.Project(way, _bridge.Stake.position, out _) - 8f;
                        float started = Time.time, best = 0f, bestAt = Time.time;
                        int rests = 0;
                        while (Time.time - started < 170f)
                        {
                            Vector3 feet = _director.Player.transform.position;
                            float at = _course.Project(way, feet, out _);
                            if (at > end || _course.Fallen(feet)) break;
                            // Stuck against the bank or the hedge: back to the way for a moment, then into the bank again.
                            if (at > best + 1f) { best = at; bestAt = Time.time; }
                            else if (Time.time - bestAt > 1.5f) { hug.HugRestUntil = Time.time + 1.6f; bestAt = Time.time + 1.6f; rests++; }
                            float edge = ground.Corridor(feet.x, feet.z, out float pathY);
                            // How she got out: the last two seconds, the first time she is clearly past the edge.
                            if (Time.time >= traceAt)
                            {
                                traceAt = Time.time + 0.1f;
                                string on = Physics.Raycast(feet + Vector3.up * 0.4f, Vector3.down, out var below, 0.8f, ~(1 << _director.Player.gameObject.layer), QueryTriggerInteraction.Ignore)
                                    ? $"{below.collider.name}/{below.collider.transform.parent?.name} n{below.normal.y:F2}" : "air";
                                trace.Enqueue($"{at:F1} m e{edge:F2} up{feet.y - pathY:F2} {(_director.Player.IsGrounded ? "G" : "-")} {on}");
                                if (trace.Count > 20) trace.Dequeue();
                            }
                            if (edge > 0.8f && !traced)
                            {
                                traced = true;
                                float nearest = float.MaxValue;
                                foreach (var line in ground.Hedges) foreach (var p in line.Points) nearest = Mathf.Min(nearest, Flat(p, feet));
                                StoryAutopilot.Note($"FOREST walls trace (nearest hedge {nearest:F1} m, at {feet:F1}): " + string.Join(" | ", trace));
                            }
                            if (edge > worst && feet.y - pathY > 0.3f && !ground.InCutAt(feet.x, feet.z)) // (into a ditch is a fall, not a way out)
                            {
                                worst = edge; worstAt = at; high = feet.y - pathY;
                                under = Physics.Raycast(feet + Vector3.up * 0.4f, Vector3.down, out var hit, 1f, ~(1 << _director.Player.gameObject.layer), QueryTriggerInteraction.Ignore)
                                    ? $"{hit.collider.name} under {hit.collider.transform.parent?.name} (normal y {hit.normal.y:F2}) at {feet:F1}" : $"nothing at {feet:F1}";
                            }
                            hug.Step(this);
                            yield return null;
                        }
                        ClearSimulated();
                        _outcome = Outcome.None;
                        float reached = _course.Project(way, _director.Player.transform.position, out _);
                        Check(worst < 0.9f, $"walls way {way} {(side < 0 ? "left" : "right")}: never out of the way (furthest {worst:F2} m past its edge at {worstAt:F0} m, {high:F2} m up · {hug.Jumps} jumps · {rests} times backed off · reached {reached:F0} of {end:F0} m · furthest on {under})");
                    }
            }

            // 9. Pause mid-run: nothing moves, nobody is caught, the run goes on after.
            if (Want("pause"))
            {
                // Paused the moment the first culm cracks, in the middle of its warning: nothing moves, nothing sounds,
                // and after the pause the culm still falls and lands once.
                var paused = new Pilot();
                yield return Begin();
                var first = culms.OrderBy(c => _course.Progress(c.transform.position)).First();
                for (float t = 0f; t < 40f && first.WarnedAt < 0f; t += Time.deltaTime) { paused.Step(this); yield return null; }
                var menu = FindAnyObjectByType<PauseMenuView>();
                Vector3 her = _director.Player.transform.position, his = _pursuer.transform.position;
                if (menu != null)
                {
                    ClearSimulated();
                    var quiet = new SoundLog();
                    menu.Pause();
                    yield return new WaitForSecondsRealtime(0.2f);
                    quiet.Attach(_director.Player);
                    var lean = first.transform.GetComponentsInChildren<Transform>().Select(t => t.rotation).ToArray();
                    yield return new WaitForSecondsRealtime(1.3f);
                    bool still = Vector3.Distance(her, _director.Player.transform.position) < 0.6f && Vector3.Distance(his, _pursuer.transform.position) < 0.6f;
                    Vector3 frozen = _director.Player.transform.position;
                    yield return new WaitForSecondsRealtime(1f);
                    still &= Vector3.Distance(frozen, _director.Player.transform.position) < 0.01f;
                    bool culmStill = first.transform.GetComponentsInChildren<Transform>().Select(t => t.rotation).SequenceEqual(lean);
                    quiet.Detach();
                    menu.Resume();
                    Check(still, "pause freezes Kiều and the pursuer");
                    Check(culmStill && !first.IsDown && quiet.Counts.Count == 0,
                        $"pause in a culm's warning: it holds still ({culmStill}), not down ({!first.IsDown}), no sound while paused ({string.Join(", ", quiet.Counts.Select(p => p.Key + " " + p.Value))})");
                }
                var after = new SoundLog();
                after.Attach(_director.Player);
                yield return Attempt(paused);
                after.Detach();
                Check(first.IsDown && first.LandedAt > first.WarnedAt, $"after the pause the culm came down ({first.LandedAt - first.WarnedAt:F1} s after its crack, pause included) · impacts this attempt {after["bamboo_impact"]}");
                Check(_outcome == Outcome.Escaped, $"the run goes on after a pause: {_outcome}");
            }

            // 10. Cô Lam along: she runs ahead, is over before the bridge goes, never stuck.
            if (_companion != null && Want("lam"))
            {
                _withLam = true;
                yield return Trial(new Pilot());
                bool over = _companion.Progress > _course.Project(1, _bridge.Stake.position, out _);
                Check(_outcome == Outcome.Escaped && over, $"with Cô Lam: {_outcome} · she is over {over} · put back {_companion.Unstuck} times");
                _companion.End();
                _companion.gameObject.SetActive(false);
                _withLam = lamBefore;
            }

            _bridge.Fell -= CountFell;
            _director.SetInt("c5_bi_bat", caughtBefore);
            Failures = 0;
            _director.Hud.FadeAlpha = 1f;
            float took = Time.realtimeSinceStartup - began;
            StoryAutopilot.ProbeTook(took);
            StoryAutopilot.Note($"FOREST probe done in {took:F0} s real time");
        }

        /// <summary>"-forestOnly many,lam": run only those cases (all of them without the flag).</summary>
        private static bool Want(string key)
        {
            var args = StoryAutopilot.Args;
            int index = System.Array.IndexOf(args, "-forestOnly");
            return index < 0 || index + 1 >= args.Length || args[index + 1].Split(',').Contains(key);
        }

        private void CountFell() => _fellEvents++;

        /// <summary>One attempt with a pilot; <paramref name="stopAfter"/> ends it early once she is that far along (metres on the moonlit way).</summary>
        private IEnumerator Trial(Pilot pilot, float stopAfter = -1f)
        {
            yield return Begin();
            if (stopAfter < 0f) { yield return Attempt(pilot); yield break; }
            while (_course.Progress(_director.Player.transform.position) < stopAfter && Time.time - _runStart < 60f)
            {
                if (_course.Fallen(_director.Player.transform.position) || _pursuer.CanCatch(_director.Player)) break;
                pilot.Step(this);
                yield return null;
            }
            ClearSimulated();
            _outcome = Outcome.None;
        }

        private static Vector3 Ground(Vector3 point)
        {
            return Physics.Raycast(point + Vector3.up * 3f, Vector3.down, out var hit, 8f, 1 << 8, QueryTriggerInteraction.Ignore) ? hit.point + Vector3.up * 0.05f : point;
        }

        private static float Flat(Vector3 a, Vector3 b) { a.y = 0f; b.y = 0f; return Vector3.Distance(a, b); }

        private static void Check(bool ok, string what)
        {
            StoryAutopilot.Note("FOREST " + (ok ? "ok   " : "FAIL ") + what);
            if (!ok) StoryAutopilot.Problem("forest: " + what);
        }
    }
}
