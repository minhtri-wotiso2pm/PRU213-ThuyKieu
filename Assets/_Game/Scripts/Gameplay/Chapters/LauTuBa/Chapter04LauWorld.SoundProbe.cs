using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThuyKieu.Core.Story;
using ThuyKieu.Environment;
using ThuyKieu.Gameplay.House;
using ThuyKieu.Gameplay.Stealth;
using UnityEngine;
using Gait = ThuyKieu.Gameplay.Stealth.FirstPersonFootsteps.Gait;

namespace ThuyKieu.Gameplay.Chapters.LauTuBa
{
    /// <summary>
    /// QA for sound in Chapter 4 ("-autopilotSound"), on the real scene with the real systems. Kiều is moved by the
    /// character controller (scripted input), so steps, floors and noise are the ones a player makes.
    /// <list type="number">
    /// <item>Every floor × crouch / walk / run: steps, the floor found, how far each step carries, after rain or music.</item>
    /// <item>Crossing floors; standing still, pushing into a wall, teleports and locked controls make no steps.</item>
    /// <item>The guard listening: gaits at the same distance, a door eased or flung, in the same room or through a wall.</item>
    /// <item>A noise, then she leaves: he checks where the noise was, not where she went, and goes back to his round.</item>
    /// <item>Two guards answer one sound without blocking each other and both go back.</item>
    /// <item>Tú Bà asleep, stirring, roused by a far door (no failure), roused with Kiều at her bedside (her catch),
    /// a hand on her belt while she stirs (her catch), a checkpoint, and the key ring taken the right way.</item>
    /// <item>The yard: every stop of the guard's round × crossing lines × gaits: seen, heard, detected.</item>
    /// </list>
    /// Each line is noted as ok/FAIL with what was measured; a FAIL is logged as an error, which fails the run.
    /// </summary>
    public partial class Chapter04LauWorld
    {
        private bool _soundProbed;

        private IEnumerator SoundProbe()
        {
            const float F = 0.35f;
            float timeScale = Time.timeScale, realStart = Time.realtimeSinceStartup;
            Time.timeScale = 1f;
            var steps = new List<FirstPersonFootsteps.StepEvent>();
            var noises = new List<Noise.Event>();
            void OnStep(FirstPersonFootsteps.StepEvent e) => steps.Add(e);
            void OnNoise(Noise.Event e) => noises.Add(e);
            _steps.Stepped += OnStep;
            Noise.Emitted += OnNoise;
            var player = _director.Player;

            void Report(string test, bool ok, string detail)
            {
                Info("SOUND " + (ok ? "ok   " : "FAIL ") + test + " · " + detail);
                if (!ok) StoryAutopilot.Problem("[SoundProbe] " + test + " · " + detail);
            }

            void Quiet()
            {
                foreach (var w in _watchers) { w.EndScript(false); w.Forget(); w.Perceiving = false; w.Pause(true); }
            }

            // Walks her from a to b (and back, `laps` times) in a gait, through the character controller.
            IEnumerator Walk(Vector3 a, Vector3 b, Gait gait, int laps = 1, float seconds = 12f)
            {
                player.ForceCrouch = gait == Gait.Crouch;
                player.Teleport(a + Vector3.up * 0.05f, Quaternion.LookRotation(Flat(b - a)));
                RestrictedZone.ResetPresence();
                for (int i = 0; i < 3; i++) yield return null;
                player.ScriptedSpeed = gait == Gait.Run ? 5.5f : gait == Gait.Walk ? player.WalkSpeed : 1.15f;
                float t = 0f;
                for (int lap = 0; lap < laps * 2 - 1 && t < seconds; lap++)
                {
                    Vector3 to = lap % 2 == 0 ? b : a;
                    while ((t += Time.deltaTime) < seconds && Flat(to - player.transform.position).magnitude > 0.25f)
                    {
                        player.ScriptedMove = to - player.transform.position;
                        yield return null;
                    }
                }
                player.ScriptedMove = null;
                player.ForceCrouch = false;
                yield return null;
            }

            // Out of everyone's way, without a step (her own room: the watchers ignore her there).
            void Away() { player.ScriptedMove = null; player.ForceCrouch = false; AutoTeleport(new Vector3(-16.5f, F + 0.05f, 4.5f)); }

            // ---------------------------------------------------------------- setup: out of the room, Tú Bà on her sập
            if (_stage == 0) { _kieuDoor.SetLock(DoorLock.None); OnKieuDoor(_kieuDoor); }
            for (float wait = 30f; !_tuBa.IsLying && (wait -= Time.deltaTime) > 0f;) yield return null;
            if (!_tuBa.IsLying) PutTuBaToBed();
            _probing = true;
            Quiet();

            // "-soundOnly tuba": the Tú Bà tests alone (quick to rerun).
            bool quick = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-soundOnly") >= 0;
            if (!quick)
            {
                // ---------------------------------------------------------------- 1. floors × gaits
                var floors = new (string name, Surface surface, Vector3 a, Vector3 b)[]
                {
                    ("wood · west veranda", Surface.Wood, new Vector3(-10.5f, F, -4f), new Vector3(-10.5f, F, 3f)),
                    ("stone · hall bricks", Surface.Stone, new Vector3(-17f, F, -12f), new Vector3(-12f, F, -12f)),
                    ("wet stone · yard (rain)", Surface.WetStone, new Vector3(-7f, 0f, 0f), new Vector3(-2f, 0f, 0f)),
                    ("mud · alley", Surface.Mud, new Vector3(-22.9f, 0f, -12f), new Vector3(-22.9f, 0f, -7f)),
                    ("earth · kitchen", Surface.Earth, new Vector3(-17.5f, 0.12f, 14.6f), new Vector3(-13.2f, 0.12f, 14.6f)),
                    ("mat · hall runner", Surface.Mat, new Vector3(0f, F, -17f), new Vector3(0f, F, -11f)),
                    ("mat · guests' room (music)", Surface.Mat, new Vector3(14.5f, F, -2.2f), new Vector3(18.5f, F, -2.2f)),
                };
                var radius = new Dictionary<(Surface, Gait), float>();
                foreach (var floor in floors)
                {
                    var line = new System.Text.StringBuilder();
                    float last = -1f;
                    bool ordered = true, right = true;
                    foreach (var gait in new[] { Gait.Crouch, Gait.Walk, Gait.Run })
                    {
                        steps.Clear();
                        noises.Clear();
                        float start = Time.time;
                        yield return Walk(floor.a, floor.b, gait);
                        float seconds = Time.time - start;
                        if (steps.Count == 0) { line.Append($"{gait}: NO STEPS · "); right = false; continue; }
                        var main = steps.GroupBy(s => s.Surface).OrderByDescending(g => g.Count()).First().Key;
                        float r = steps.Average(s => s.Radius);
                        float heard = noises.Where(n => n.Kind == NoiseKind.Footstep).Select(n => n.Radius).DefaultIfEmpty(0f).Average();
                        right &= main == floor.surface;
                        ordered &= r > last;
                        last = r;
                        if (floor.name.StartsWith(floor.surface.ToString().ToLower()) || !radius.ContainsKey((floor.surface, gait))) radius[(floor.surface, gait)] = r;
                        line.Append($"{gait}: {steps.Count} steps in {seconds:F1} s ({steps.Count / Mathf.Max(0.1f, seconds):F1}/s) on {main}{(steps.All(s => s.Tagged) ? "" : " (untagged!)")} · {steps[0].Sound} vol {steps.Average(s => s.Volume):F2} · carries {r:F1} m, after rain/music {heard:F1} m · ");
                    }
                    Report("floor " + floor.name, right && ordered, line.ToString().TrimEnd(' ', '·') + (ordered ? "" : " · NOT crouch < walk < run"));
                }
                bool matQuieter = new[] { Gait.Crouch, Gait.Walk, Gait.Run }.All(g => radius.TryGetValue((Surface.Mat, g), out var m) && radius.TryGetValue((Surface.Wood, g), out var w) && m < w);
                Report("mats are quieter than boards", matQuieter, string.Join(" · ", new[] { Gait.Crouch, Gait.Walk, Gait.Run }.Select(g => $"{g} mat {(radius.TryGetValue((Surface.Mat, g), out var m) ? m : -1):F1} / wood {(radius.TryGetValue((Surface.Wood, g), out var w) ? w : -1):F1} m")));

                // ---------------------------------------------------------------- 2. crossing floors, and no false steps
                steps.Clear();
                yield return Walk(new Vector3(-10.6f, F, 1f), new Vector3(-6.5f, 0f, 0f), Gait.Walk);
                string sequence = string.Join(" → ", steps.Select(s => s.Surface.ToString()).Aggregate(new List<string>(), (list, s) => { if (list.Count == 0 || list[list.Count - 1] != s) list.Add(s); return list; }));
                Report("veranda → steps → yard", steps.Count > 0 && steps[0].Surface == Surface.Wood && steps[steps.Count - 1].Surface == Surface.WetStone, sequence + $" ({steps.Count} steps)");
                steps.Clear();
                yield return Walk(new Vector3(-3f, F, -14f), new Vector3(3f, F, -14f), Gait.Walk);
                sequence = string.Join(" → ", steps.Select(s => s.Surface.ToString()).Aggregate(new List<string>(), (list, s) => { if (list.Count == 0 || list[list.Count - 1] != s) list.Add(s); return list; }));
                Report("hall bricks → runner → bricks", sequence == "Stone → Mat → Stone", sequence + $" ({steps.Count} steps)");

                steps.Clear();
                AutoTeleport(new Vector3(-10.5f, F + 0.05f, -2f));
                for (float t = 0f; t < 3f; t += Time.deltaTime) yield return null;
                int still = steps.Count;
                // Into the west wall of her room from half a metre away, for three seconds.
                steps.Clear();
                yield return Walk(new Vector3(-20.3f, F, 3f), new Vector3(-23f, F, 3f), Gait.Walk, 1, 3f);
                int wall = steps.Count;
                steps.Clear();
                for (int i = 0; i < 6; i++) { AutoTeleport(new Vector3(i % 2 == 0 ? -10.5f : 10.5f, F + 0.05f, -2f)); yield return null; yield return null; }
                int teleports = steps.Count;
                steps.Clear();
                player.ControlLocked = true;
                yield return Walk(new Vector3(-10.5f, F, -4f), new Vector3(-10.5f, F, 3f), Gait.Walk, 1, 3f);
                player.ControlLocked = false;
                int locked = steps.Count;
                Report("no false steps", still == 0 && wall <= 1 && teleports == 0 && locked == 0, $"standing 3 s: {still} · pushing into a wall 3 s: {wall} · 6 teleports of 21 m: {teleports} · controls locked: {locked}");

                // ---------------------------------------------------------------- 3. the guard listening
                // He stands on the west veranda looking south, she makes the sound 3.5 m behind him, then is gone (no sight).
                Vector3 post = new Vector3(-10.5f, F, -4.5f);
                IEnumerator Listen(string what, IEnumerator act)
                {
                    Quiet();
                    Stand(_guard, post, 180f);
                    yield return null;
                    int heardCount = 0;
                    Vector3 heardFrom = Vector3.zero;
                    var peak = WatchState.Patrol;
                    void Heard(Watchman w, Noise.Event e) { heardCount++; heardFrom = e.Position; }
                    void Changed(Watchman w, WatchState s) { if (Rank(s) > Rank(peak)) peak = s; }
                    _guard.Heard += Heard;
                    _guard.StateChanged += Changed;
                    _guard.Pause(false);
                    _guard.Perceiving = true;
                    noises.Clear();
                    yield return act;
                    Away();
                    float loudest = noises.Select(n => n.Radius).DefaultIfEmpty(0f).Max();
                    for (float t = 0f; t < 2.5f; t += Time.deltaTime) yield return null;
                    _guard.Heard -= Heard;
                    _guard.StateChanged -= Changed;
                    Info($"SOUND info guard hears {what}: {noises.Count} sounds up to {loudest:F1} m · heard {heardCount} · {(heardCount > 0 ? $"from {Vector3.Distance(heardFrom, _guard.transform.position):F1} m · " : "")}reaction {peak} · going to {(peak == WatchState.Investigate ? Vector3.Distance(_guard.Target, heardFrom).ToString("F1") + " m from the sound" : "-")}");
                    _listen[what] = (heardCount, peak);
                }
                _listen.Clear();
                Vector3 l1 = post + new Vector3(-1.1f, 0f, 3.5f), l2 = post + new Vector3(1.1f, 0f, 3.5f);
                yield return Listen("crouch 3.5 m", Walk(l1, l2, Gait.Crouch, 2));
                yield return Listen("walk 3.5 m", Walk(l1, l2, Gait.Walk, 2));
                yield return Listen("run 3.5 m", Walk(l1, l2, Gait.Run, 2));
                yield return Listen("walk 6 m", Walk(post + new Vector3(-1.1f, 0f, 6f), post + new Vector3(1.1f, 0f, 6f), Gait.Walk, 2));
                yield return Listen("run 6 m", Walk(post + new Vector3(-1.1f, 0f, 6f), post + new Vector3(1.1f, 0f, 6f), Gait.Run, 2));
                Report("gaits at 3.5 m", Got("crouch 3.5 m").heard == 0 && Got("walk 3.5 m").heard > 0 && Got("run 3.5 m").reaction == WatchState.Investigate,
                    $"crouch heard {Got("crouch 3.5 m").heard} ({Got("crouch 3.5 m").reaction}) · walk heard {Got("walk 3.5 m").heard} ({Got("walk 3.5 m").reaction}) · run heard {Got("run 3.5 m").heard} ({Got("run 3.5 m").reaction})");
                Report("gaits at 6 m", Got("walk 6 m").heard == 0 && Got("run 6 m").reaction == WatchState.Investigate,
                    $"walk heard {Got("walk 6 m").heard} · run heard {Got("run 6 m").heard} ({Got("run 6 m").reaction})");

                // Chị Lam's door, 4.3 m from him, eased or flung; then with a wall between (him in Kiều's room).
                _lamDoor.SetLock(DoorLock.None);
                Vector3 lamDoor = _lamDoor.transform.position;
                IEnumerator Door(bool fling)
                {
                    _lamDoor.SetOpen(0f);
                    AutoTeleport(new Vector3(-10.9f, F + 0.05f, -3f));
                    yield return null;
                    if (fling) { _lamDoor.Tick(true, false); yield return null; _lamDoor.Tick(false, false); for (float t = 0f; t < 0.6f; t += Time.deltaTime) yield return null; }
                    else
                    {
                        _lamDoor.Tick(true, true);
                        for (float t = 0f; t < 6f && _lamDoor.Openness < 0.999f; t += Time.deltaTime) { yield return null; _lamDoor.Tick(false, true); }
                        _lamDoor.Tick(false, false);
                    }
                }
                post = lamDoor + new Vector3(1.6f, 0f, -4f);
                post.y = F;
                yield return Listen("door eased, same side 4.3 m", Door(false));
                yield return Listen("door flung, same side 4.3 m", Door(true));
                Report("door eased vs flung", Got("door eased, same side 4.3 m").heard == 0 && Got("door flung, same side 4.3 m").reaction == WatchState.Investigate,
                    $"eased: heard {Got("door eased, same side 4.3 m").heard} · flung: heard {Got("door flung, same side 4.3 m").heard} → {Got("door flung, same side 4.3 m").reaction} · door at {lamDoor}");
                post = new Vector3(-15.5f, F, 1.5f);
                float through = Vector3.Distance(post, lamDoor);
                bool blocked = Physics.Linecast(lamDoor + Vector3.up * 0.4f, post + Vector3.up * 1.6f, 1 << 8, QueryTriggerInteraction.Ignore);
                yield return Listen("door flung, wall between", Door(true));
                Info($"SOUND info wall between: {through:F1} m, wall in the way {blocked} · a flung door (8 m) carries {8f * Noise.Muffled:F1} m through a wall");
                _lamDoor.SetOpen(0f);

                // ---------------------------------------------------------------- 4. a noise, then she leaves
                Quiet();
                post = new Vector3(-10.5f, F, -4.5f);
                Stand(_guard, post, 180f);
                _guard.Pause(false);
                _guard.Perceiving = true;
                yield return Walk(post + new Vector3(-1.1f, 0f, 3.5f), post + new Vector3(1.1f, 0f, 3.5f), Gait.Run, 1);
                Vector3 noiseAt = Noise.Last.Position;
                Away();
                Vector3 she = player.transform.position;
                float targetOff = Vector3.Distance(_guard.Target, noiseAt), closest = 99f, towardsHer = 99f;
                bool back = false;
                float tBack = 0f;
                for (float t = 0f; t < 45f; t += Time.deltaTime)
                {
                    closest = Mathf.Min(closest, Flat(_guard.transform.position - noiseAt).magnitude);
                    towardsHer = Mathf.Min(towardsHer, Flat(_guard.Target - she).magnitude);
                    if (t > 1f && (_guard.State == WatchState.Patrol || _guard.State == WatchState.Return)) { back = true; tBack = t; break; }
                    yield return null;
                }
                Report("noise then gone", targetOff < 1.2f && closest < 1.6f && towardsHer > 4f && back,
                    $"went to check {targetOff:F1} m from the noise · got within {closest:F1} m of it · never aimed nearer than {towardsHer:F1} m to where she went · back on his round after {tBack:F0} s");

                // ---------------------------------------------------------------- 5. two guards, one sound
                Quiet();
                Stand(_guard, new Vector3(-3f, F, -12.5f), 180f);
                _gateGuard.ResetAt(0);
                foreach (var w in new[] { _guard, _gateGuard }) { w.Pause(false); w.Perceiving = true; }
                Vector3 bead = new Vector3(0f, F, -14.5f);
                Noise.Emit(bead, 8f, NoiseKind.Thrown);
                var states = new[] { _guard.State, _gateGuard.State };
                float near0 = 99f, near1 = 99f, back0 = -1f, back1 = -1f;
                for (float t = 0f; t < 60f && (back0 < 0f || back1 < 0f); t += Time.deltaTime)
                {
                    near0 = Mathf.Min(near0, Flat(_guard.transform.position - bead).magnitude);
                    near1 = Mathf.Min(near1, Flat(_gateGuard.transform.position - bead).magnitude);
                    if (back0 < 0f && t > 1f && (_guard.State == WatchState.Patrol || _guard.State == WatchState.Return)) back0 = t;
                    if (back1 < 0f && t > 1f && (_gateGuard.State == WatchState.Patrol || _gateGuard.State == WatchState.Return)) back1 = t;
                    yield return null;
                }
                Report("two guards, one bead", states.All(s => s == WatchState.Investigate) && near0 < 1.8f && near1 < 1.8f && back0 > 0f && back1 > 0f,
                    $"reactions {states[0]} / {states[1]} · came within {near0:F1} / {near1:F1} m · back to their posts after {back0:F0} / {back1:F0} s");
            }

            // ---------------------------------------------------------------- 6. Tú Bà
            Quiet();
            Home();
            _guard.Pause(false);
            _guard.Perceiving = true;
            int caughtBefore = _director.GetInt("c4_bi_bat"), scenesBefore = CatchScene.Count;
            PutTuBaToBed(false);
            yield return Walk(new Vector3(-9f, F, 13f), new Vector3(-5f, F, 13f), Gait.Crouch, 2);
            Report("asleep, crouching next door", _sleeper.State == TuBaSleeper.Sleep.Asleep, $"{_sleeper.State} · disturbance {_sleeper.Disturbance:F2} · {Vector3.Distance(player.transform.position, _sleeper.Head):F1} m from her");
            Away();
            yield return Fling(_kieuDoor);
            Report("asleep, Kiều's door flung far away", _sleeper.State == TuBaSleeper.Sleep.Asleep, $"{_sleeper.State} · {Vector3.Distance(_kieuDoor.transform.position, _sleeper.Head):F1} m");

            // Her own paper door flung from outside: roused, the guard sent there, no failure, then sleep again.
            _tuBaDoor.SetLock(DoorLock.None);
            _tuBaDoor.SetOpen(0f);
            AutoTeleport(_tuBaDoorOutside.position + Vector3.up * 0.05f);
            yield return null;
            yield return Fling(_tuBaDoor);
            var roused = _sleeper.State;
            yield return null;
            var guardState = _guard.State;
            float sentOff = Vector3.Distance(_guard.Target, _tuBaDoor.transform.position);
            float settle = 0f;
            for (; settle < 25f && _sleeper.State != TuBaSleeper.Sleep.Asleep; settle += Time.deltaTime) yield return null;
            Report("her door flung: roused, no failure", roused != TuBaSleeper.Sleep.Asleep && _director.InExplore && _director.GetInt("c4_bi_bat") == caughtBefore && _sleeper.State == TuBaSleeper.Sleep.Asleep,
                $"{roused} · guard {guardState}, sent {sentOff:F1} m from the door · asleep again after {settle:F0} s · failures {_director.GetInt("c4_bi_bat") - caughtBefore}");
            _tuBaDoor.SetOpen(0f);

            // Stirring, Kiều about 3 m away on the rug round the sập (outside her reach from it, 2.4 m): walking there is
            // muffled by the rug (2.4 m); running is not (5.4 m) and rouses her, still without a failure.
            Quiet();
            Home();
            _guard.Pause(false);
            _guard.Perceiving = true;
            Vector3 head = _sleeper.Head;
            var (rugA, rugB) = ClearStretchNear(head, 3f);
            Info($"SOUND info rug spot {Vector3.Distance(Flat(rugA), Flat(head)):F1} m from her head, on a covering {SurfaceTag.PatchAt(rugA, out var under)} ({under})");
            _sleeper.Stir(3.5f);
            yield return Walk(rugA, rugB, Gait.Walk, 2, 3.5f);
            var afterWalk = _sleeper.State;
            Info($"SOUND info stirring, walking on her rug about 3 m away · {afterWalk} (a walking step on the rug carries 2.4 m)");
            for (float t = 0f; t < 6f && _sleeper.State != TuBaSleeper.Sleep.Asleep; t += Time.deltaTime) yield return null;
            _sleeper.Stir(3.5f);
            steps.Clear();
            yield return Walk(rugA, rugB, Gait.Run, 2, 2f);
            var afterRun = _sleeper.State;
            Info($"SOUND info running on her rug: {steps.Count} steps, carrying {steps.Select(e => e.Radius).DefaultIfEmpty(0f).Max():F1} m, {steps.Select(e => Vector3.Distance(e.Position, _sleeper.Head)).DefaultIfEmpty(0f).Min():F1} m from her ear · she last heard a sound from {Vector3.Distance(_sleeper.LastHeard, rugA):F1} m away from the rug spot");
            bool safe = _director.InExplore && !CatchScene.Playing;
            Away();
            for (float t = 0f; t < 25f && _sleeper.State != TuBaSleeper.Sleep.Asleep; t += Time.deltaTime) yield return null;
            Report("stirring, running on her rug about 3 m away", afterRun == TuBaSleeper.Sleep.Roused && safe && _sleeper.State == TuBaSleeper.Sleep.Asleep, $"{afterRun} · still free {safe} · then {_sleeper.State}");

            // Roused, and Kiều at her bedside: Tú Bà's own catch, then the checkpoint puts her back to sleep.
            _sleeper.Rouse(_tuBaDoor.transform.position);
            Vector3 bedside = head + Flat(_keyTheft.transform.position - head).normalized * 1.1f;
            bedside.y = F + 0.05f;
            player.Teleport(bedside, Quaternion.LookRotation(Flat(head - bedside)));
            _director.CameraController.SnapBehindTarget(Quaternion.LookRotation(Flat(head - bedside)).eulerAngles.y);
            RestrictedZone.ResetPresence();
            for (float t = 0f; t < 3f && !CatchScene.Playing; t += Time.deltaTime) yield return null;
            bool hers = CatchScene.Playing;
            yield return Settle();
            Report("roused, Kiều at the bedside", hers && _catch.LastKind == CatchScene.Kind.Front && CatchScene.Count == scenesBefore + 1 && _tuBa.IsLying && _sleeper.State == TuBaSleeper.Sleep.Asleep && !_tuBaWatch.enabled,
                $"catch played {hers} ({_catch.LastKind}) · after the checkpoint: lying {_tuBa.IsLying}, {_sleeper.State}, her eyes off {!_tuBaWatch.enabled}");

            // A hand at her belt while she stirs.
            // Leaning in over the keys, then taking hold as she stirs (through the real interaction, see the key probe).
            yield return null;
            _interaction = FindFirstObjectByType<ThuyKieu.Interaction.PlayerInteraction>();
            yield return EnterKeys(KeyStand(), true);
            _sleeper.Stir(3.5f);
            yield return null;
            ThuyKieu.Core.InputReader.SimulatedInteract = true;
            for (float t = 0f; t < 2f && !CatchScene.Playing && _director.InExplore; t += Time.deltaTime) yield return null;
            ThuyKieu.Core.InputReader.SimulatedInteract = false;
            player.ForceCrouch = false;
            bool grabbed = CatchScene.Playing;
            yield return Settle();
            Report("hand at her belt while she stirs", grabbed && CatchScene.Count == scenesBefore + 2, $"catch played {grabbed}");

            // After the checkpoint: nothing left over.
            Quiet();
            Home();
            _guard.Pause(false);
            _guard.Perceiving = true;
            int heardAfter = 0;
            void Count(Watchman w, Noise.Event e) => heardAfter++;
            _guard.Heard += Count;
            for (float t = 0f; t < 3f; t += Time.deltaTime) yield return null;
            _guard.Heard -= Count;
            Report("after the checkpoint", heardAfter == 0 && _sleeper.State == TuBaSleeper.Sleep.Asleep && _sleeper.Disturbance < 0.01f && _guard.State == WatchState.Patrol,
                $"sounds heard {heardAfter} · Tú Bà {_sleeper.State}, disturbance {_sleeper.Disturbance:F2} · guard {_guard.State}");

            // ---------------------------------------------------------------- 7. the yard
            Time.timeScale = timeScale;
            if (!quick) yield return YardSweep();
            Time.timeScale = 1f;

            // ---------------------------------------------------------------- 8. the key ring, the right way (the story goes on from here)
            Quiet();
            Home();
            for (float t = 0f; t < 25f && _sleeper.State != TuBaSleeper.Sleep.Asleep; t += Time.deltaTime) yield return null;
            // Through the real interaction: leaning in over the keys, E and the mouse on her out-breaths (see the key probe).
            _interaction = FindFirstObjectByType<ThuyKieu.Interaction.PlayerInteraction>();
            yield return TakeKeysTheRightWay(Report, _keyTheft, KeyStand());
            player.ForceCrouch = false;
            Report("key ring taken on her out-breaths", _inventory.Has("chia") && _stage == 2 && _sleeper.State != TuBaSleeper.Sleep.Awake, $"stage {_stage} · Tú Bà {_sleeper.State}");

            _steps.Stepped -= OnStep;
            Noise.Emitted -= OnNoise;
            _probing = false;
            foreach (var w in _watchers) { w.Sensitivity = 1f; w.Perceiving = false; w.Pause(false); }
            ArmGuard();
            _guard.Perceiving = _gateGuard.Perceiving = false;
            Time.timeScale = timeScale;
            Info("SOUND probe done");
            StoryAutopilot.ProbeTook(Time.realtimeSinceStartup - realStart);
        }

        private readonly Dictionary<string, (int heard, WatchState reaction)> _listen = new Dictionary<string, (int, WatchState)>();
        private (int heard, WatchState reaction) Got(string what) => _listen.TryGetValue(what, out var v) ? v : (-1, WatchState.Patrol);

        private static Vector3 Flat(Vector3 v) { v.y = 0f; return v; }

        /// <summary>Waits for a catch and its failure to play out and the world to be back (at most 30 s, noted if not).</summary>
        private IEnumerator Settle()
        {
            float t = 0f;
            for (; t < 30f && (CatchScene.Playing || !_director.InExplore); t += Time.unscaledDeltaTime) yield return null;
            if (t >= 30f) Info($"SOUND FAIL settle timed out · catch playing {CatchScene.Playing} · exploring {_director.InExplore} · Tú Bà {_sleeper.State}");
        }

        /// <summary>A line for the QA report, also in the log so a long run can be followed.</summary>
        private static void Info(string line)
        {
            StoryAutopilot.Note(line);
            Debug.Log("[Probe] " + line);
        }

        private Transform _probePost;

        /// <summary>
        /// A walkable stretch about <paramref name="distance"/> from a point: its start, and its end 1.5 m further on, going
        /// round the point (so the distance stays about the same) over clear floor: furniture round the sập must not stop
        /// the test walk, and a running stride is 1.35 m.
        /// </summary>
        private (Vector3 from, Vector3 to) ClearStretchNear(Vector3 point, float distance)
        {
            var filter = new UnityEngine.AI.NavMeshQueryFilter { agentTypeID = _navMesh.AgentTypeId, areaMask = UnityEngine.AI.NavMesh.AllAreas };
            for (int angle = 0; angle < 360; angle += 15)
            {
                Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                Vector3 along = Vector3.Cross(Vector3.up, direction);
                Vector3 candidate = new Vector3(point.x, LauTuBaFloor, point.z) + direction * distance;
                if (!_tuBaRoom.bounds.Contains(candidate + Vector3.up * 0.5f) || !_tuBaRoom.bounds.Contains(candidate + along * 1.5f + Vector3.up * 0.5f)) continue;
                if (!UnityEngine.AI.NavMesh.SamplePosition(candidate, out var hit, 0.15f, filter)) continue;
                if (UnityEngine.AI.NavMesh.Raycast(hit.position, hit.position + along * 1.6f, out _, filter)) continue;
                return (hit.position, hit.position + along * 1.5f);
            }
            return (new Vector3(point.x + distance, LauTuBaFloor, point.z), new Vector3(point.x + distance, LauTuBaFloor, point.z + 1.5f));
        }

        private const float LauTuBaFloor = 0.35f;

        /// <summary>A watcher keeps a post for the test: a round of one stop, standing there on his round (not scripted).</summary>
        private void Stand(Watchman watcher, Vector3 at, float yaw)
        {
            if (_probePost == null) _probePost = new GameObject("ProbePost").transform;
            _probePost.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            watcher.SetRoute(new[] { _probePost }, new[] { 999f });
            watcher.ResetAt(0, 999f);
        }

        /// <summary>The guard back on his own round, at its first stop.</summary>
        private void Home()
        {
            _guard.SetRoute(_guardPost.Points, _guardPost.Pauses);
            _guard.ResetAt(0);
        }

        /// <summary>How far a reaction went: on his round, turned his head, came to check, gave chase.</summary>
        private static int Rank(WatchState state)
        {
            switch (state)
            {
                case WatchState.Suspicious: return 1;
                case WatchState.Investigate:
                case WatchState.Search: return 2;
                case WatchState.Chase: return 3;
                default: return 0;
            }
        }

        private IEnumerator Fling(HouseDoor door)
        {
            door.Tick(true, false);
            yield return null;
            door.Tick(false, false);
            for (float t = 0f; t < 0.6f; t += Time.deltaTime) yield return null;
        }

        /// <summary>
        /// The yard, from every stop of the round that crosses it (the gate guard's after the alarm, walked by the guard): Kiều crosses from the west steps to the east steps on three lines,
        /// crouched, walking and running, with the guard perceiving at the chapter's own sensitivity. Noted per gait: how often
        /// he saw her, heard her, and came for her (investigate or chase).
        /// </summary>
        private IEnumerator YardSweep()
        {
            var player = _director.Player;
            _guard.Sensitivity = 0.85f + 0.15f * _director.GetInt("c4_canh_giac");
            int caught = 0;
            void Caught(Watchman _) => caught++;
            _guard.Caught += Caught;
            // Open paving between the tree bed, the basin, the jars and the benches (a straight line from step to step
            // would walk into the tree bed).
            var lines = new[] { 0f, -4.2f, 5.5f };
            var summary = new System.Text.StringBuilder();
            foreach (var gait in new[] { Gait.Crouch, Gait.Walk, Gait.Run })
            {
                int trials = 0, seen = 0, heard = 0, came = 0, chased = 0;
                var cameFrom = new List<int>();
                foreach (float z in lines)
                    for (int stop = 0; stop < _gateAlarm.Points.Length; stop++)
                    {
                        foreach (var w in _watchers) { w.EndScript(false); w.Forget(); w.Perceiving = false; w.Pause(true); }
                        _guard.SetRoute(_gateAlarm.Points, _gateAlarm.Pauses);
                        _guard.ResetAt(stop);
                        _guard.Pause(false);
                        _guard.Perceiving = true;
                        bool sawIt = false, heardIt = false, cameIt = false, chasedIt = false;
                        void Heard(Watchman w, Noise.Event e) { if (e.Kind == NoiseKind.Footstep) heardIt = true; }
                        _guard.Heard += Heard;
                        caught = 0;
                        player.ForceCrouch = gait == Gait.Crouch;
                        float edge = z > 5f ? 7.2f : 8.4f;
                        player.Teleport(new Vector3(-edge, 0.05f, z), Quaternion.Euler(0f, 90f, 0f));
                        RestrictedZone.ResetPresence();
                        yield return null;
                        player.ScriptedSpeed = gait == Gait.Run ? 5.5f : gait == Gait.Walk ? player.WalkSpeed : 1.15f;
                        Vector3 to = new Vector3(edge, 0f, z);
                        // Across, then a breath at the far steps: did he see her, hear her, come to check, give chase?
                        float after = 0f;
                        for (float t = 0f; t < 25f && after < 1.5f && !chasedIt; t += Time.deltaTime)
                        {
                            bool there = Flat(to - player.transform.position).magnitude <= 0.3f;
                            player.ScriptedMove = there ? (Vector3?)null : to - player.transform.position;
                            if (there) after += Time.deltaTime;
                            sawIt |= _guard.SeesPlayer;
                            cameIt |= _guard.State == WatchState.Investigate || _guard.State == WatchState.Search;
                            chasedIt |= _guard.State == WatchState.Chase || caught > 0;
                            yield return null;
                        }
                        player.ScriptedMove = null;
                        player.ForceCrouch = false;
                        _guard.Heard -= Heard;
                        trials++;
                        if (sawIt) seen++;
                        if (heardIt) heard++;
                        if (cameIt || chasedIt) { came++; cameFrom.Add(stop); }
                        if (chasedIt) chased++;
                    }
                summary.Append($"{gait}: seen {seen}/{trials} · heard {heard}/{trials} · came to check {came}/{trials} · chased {chased}/{trials}{(cameFrom.Count > 0 ? " (from stops " + string.Join(",", cameFrom.Distinct()) + ")" : "")} · ");
            }
            _guard.Caught -= Caught;
            Info("SOUND info yard crossings (lines z = 0, -4.2, 5.5; every stop of the round) · " + summary.ToString().TrimEnd(' ', '·'));
            foreach (var w in _watchers) { w.EndScript(false); w.Forget(); w.Perceiving = false; w.Pause(true); }
        }
    }
}
