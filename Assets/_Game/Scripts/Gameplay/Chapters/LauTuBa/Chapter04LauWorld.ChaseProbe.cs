using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.House;
using ThuyKieu.Gameplay.Stealth;
using UnityEngine;
using UnityEngine.AI;

namespace ThuyKieu.Gameplay.Chapters.LauTuBa
{
    /// <summary>
    /// QA for chasing, searching and giving up ("-autopilotChase", best with "-traceWatch"), on the real Chapter 4 scene
    /// with the guard's senses on. Kiều is moved by the character controller. Each case notes what the guard did
    /// (states in order, where he went, how long he stood stuck, the spots he searched and whether he could walk there, how
    /// far from his round he was when he went back to it) and an ok/FAIL against what a player should be able to read.
    /// </summary>
    public partial class Chapter04LauWorld
    {
        private bool _chaseProbed;

        /// <summary>What the guard did while a case ran.</summary>
        private sealed class Watch
        {
            public readonly List<string> States = new List<string>();
            public readonly List<(Vector3 point, NavMeshPathStatus status, bool walled)> SearchSpots = new List<(Vector3, NavMeshPathStatus, bool)>();
            public float Stuck, StuckLongest, PatrolOffRound = -1f, ReachedLastKnown = 99f;
            public int Caught;
            public bool Patrolling;
            public string Line => string.Join(" → ", States);
        }

        private IEnumerator ChaseProbe()
        {
            const float F = 0.35f;
            float timeScale = Time.timeScale, realStart = Time.realtimeSinceStartup;
            Time.timeScale = 1f;
            _probing = true;
            var player = _director.Player;
            if (_stage == 0) { _kieuDoor.SetLock(DoorLock.None); OnKieuDoor(_kieuDoor); }
            for (float wait = 30f; !_tuBa.IsLying && (wait -= Time.deltaTime) > 0f;) yield return null;
            if (!_tuBa.IsLying) PutTuBaToBed();

            void Report(string test, bool ok, string detail)
            {
                Info("CHASE " + (ok ? "ok   " : "FAIL ") + test + " · " + detail);
                if (!ok) StoryAutopilot.Problem("[ChaseProbe] " + test + " · " + detail);
            }

            void Quiet()
            {
                player.ScriptedMove = null;
                player.ForceCrouch = false;
                HideSpot.ExitAll();
                foreach (var w in _watchers) { w.EndScript(false); w.Forget(); w.Perceiving = false; w.Pause(true); w.Sensitivity = 1f; }
            }

            // Kiều runs (or walks) through the points with the character controller.
            IEnumerator Go(float speed, params Vector3[] points)
            {
                player.ScriptedSpeed = speed;
                foreach (var p in points)
                    for (float t = 0f; t < 8f && Flat(p - player.transform.position).magnitude > 0.3f; t += Time.deltaTime)
                    {
                        player.ScriptedMove = p - player.transform.position;
                        yield return null;
                    }
                player.ScriptedMove = null;
            }

            // Watches the guard for up to `seconds`, until `done` says so.
            IEnumerator Observe(Watch w, float seconds, Func<bool> done)
            {
                Vector3 lastSpot = Vector3.positiveInfinity;
                void Changed(Watchman g, WatchState s) => w.States.Add($"{s}@{Time.time - _caseStart:F1}");
                void Caught(Watchman g) => w.Caught++;
                _guard.StateChanged += Changed;
                _guard.Caught += Caught;
                for (float t = 0f; t < seconds && !done(); t += Time.deltaTime)
                {
                    var actor = _guard.Actor;
                    if (actor.IsMoving && actor.Velocity < 0.05f) { w.Stuck += Time.deltaTime; w.StuckLongest = Mathf.Max(w.StuckLongest, w.Stuck); }
                    else w.Stuck = 0f;
                    if (_guard.State == WatchState.Search && actor.IsMoving && !actor.PathPending && Vector3.Distance(actor.Destination, lastSpot) > 0.5f)
                    {
                        lastSpot = actor.Destination;
                        bool walled = Physics.Linecast(_guard.Target + Vector3.up * 1.2f, lastSpot + Vector3.up * 1.2f, 1 << 8, QueryTriggerInteraction.Ignore);
                        w.SearchSpots.Add((lastSpot, actor.PathStatus, walled));
                    }
                    w.ReachedLastKnown = Mathf.Min(w.ReachedLastKnown, Flat(_guard.transform.position - _guard.LastKnown).magnitude);
                    if (_guard.State == WatchState.Patrol && !w.Patrolling && w.States.Count > 1)
                    {
                        w.Patrolling = true;
                        w.PatrolOffRound = _guard.Waypoints.Min(p => Flat(p.position - _guard.transform.position).magnitude);
                    }
                    yield return null;
                }
                _guard.StateChanged -= Changed;
                _guard.Caught -= Caught;
            }

            // "across something": a straight line from where he lost her to the spot hits a wall, railing or post (information; the
            // rule is a complete, short walk).
            string Spots(Watch w) => w.SearchSpots.Count == 0 ? "none" : string.Join(", ", w.SearchSpots.Select(s => $"{Flat(s.point)} {s.status}{(s.walled ? " (across something)" : "")}"));

            // A guard on the south end of the west veranda, looking north, who sees Kiều ahead and gives chase.
            IEnumerator SeenAndChased(Watch w)
            {
                Quiet();
                _lamDoor.SetLock(DoorLock.None);
                _lamDoor.SetOpen(1f);
                player.Teleport(new Vector3(-10.5f, F + 0.05f, -5.0f), Quaternion.identity);
                RestrictedZone.ResetPresence();
                Home();
                Stand(_guard, new Vector3(-10.5f, F, -9.0f), 0f);
                _guard.Sensitivity = 3f;
                _guard.Pause(false);
                _guard.Perceiving = true;
                _caseStart = Time.time;
                yield return Observe(w, 15f, () => _guard.State == WatchState.Chase);
            }

            // ------------------------------------------------------------ A. lost round a corner (Lam's room), then...
            foreach (string variant in new[] { "stays still", "makes noise there", "slips away" })
            {
                var w = new Watch();
                yield return SeenAndChased(w);
                bool chased = _guard.State == WatchState.Chase;
                yield return Go(5.5f, new Vector3(-11.3f, F, -3.6f), new Vector3(-13.2f, F, -3.6f), new Vector3(-16.5f, F, -1.3f));
                if (variant == "slips away") Away();
                Vector3 hidden = player.transform.position;
                if (variant == "makes noise there")
                {
                    float until = Time.time + 25f;
                    yield return Observe(w, 40f, () =>
                    {
                        // A few soft steps on the spot every couple of seconds (heard within 4 m).
                        if (Time.time < until && Mathf.Repeat(Time.time, 2.5f) < Time.deltaTime * 1.5f) Noise.Emit(hidden, 4f, NoiseKind.Footstep);
                        return w.Patrolling || w.Caught > 0;
                    });
                }
                else yield return Observe(w, 45f, () => w.Patrolling || w.Caught > 0);
                bool sane = chased && w.StuckLongest < 2f && w.SearchSpots.All(s => s.status == NavMeshPathStatus.PathComplete) && (w.Patrolling ? w.PatrolOffRound < 1.5f : w.Caught > 0);
                Report($"lost round a corner, {variant}", sane,
                    $"chased {chased} · {w.Line} · came within {w.ReachedLastKnown:F1} m of where he last knew · stuck up to {w.StuckLongest:F1} s · searched {Spots(w)} · caught {w.Caught} · back on his round {(w.Patrolling ? w.PatrolOffRound.ToString("F1") + " m from it" : "no")}");
            }

            // ------------------------------------------------------------ B. sounds moving away while he checks
            {
                Quiet();
                Home();
                Stand(_guard, new Vector3(-10.5f, F, -6.5f), 180f);
                _guard.Pause(false);
                _guard.Perceiving = true;
                _caseStart = Time.time;
                var w = new Watch();
                var sounds = new[] { new Vector3(-11.4f, F, -3.6f), new Vector3(-10.5f, F, 1.5f), new Vector3(-10.5f, F, 6.5f) };
                var after = new List<float>();
                foreach (var s in sounds)
                {
                    Noise.Emit(s, 9f, NoiseKind.Door);
                    yield return Observe(w, 1.5f, () => false);
                    after.Add(Flat(_guard.Target - s).magnitude);
                }
                yield return Observe(w, 40f, () => w.Patrolling);
                Report("three sounds, each further away", after.Last() < 1f && w.Patrolling,
                    $"after each sound he was heading {string.Join(" / ", after.Select(a => a.ToString("F1")))} m from it · {w.Line} · back on his round {(w.Patrolling ? w.PatrolOffRound.ToString("F1") + " m from it" : "no")}");
            }

            // ------------------------------------------------------------ C. the door shut and bolted behind her
            {
                var w = new Watch();
                yield return SeenAndChased(w);
                bool chased = _guard.State == WatchState.Chase;
                // Through the doorway, the door shut and bolted behind her at once (he is still outside), then on into the room.
                yield return Go(5.5f, new Vector3(-11.3f, F, -3.6f), new Vector3(-13.4f, F, -3.6f));
                _lamDoor.SetOpen(0f);
                _lamDoor.SetLock(DoorLock.Locked);
                float outside = Flat(_guard.transform.position - new Vector3(-13.4f, F, -3.6f)).magnitude;
                yield return Go(5.5f, new Vector3(-16.5f, F, -1.3f));
                yield return Observe(w, 45f, () => w.Patrolling || w.Caught > 0);
                bool inside(Vector3 p) => p.x < -12.1f && p.z < -0.5f;
                bool sane = chased && w.StuckLongest < 2f && w.Caught == 0 && w.SearchSpots.All(s => s.status == NavMeshPathStatus.PathComplete && !inside(s.point)) && w.Patrolling && w.PatrolOffRound < 1.5f;
                Report("door bolted behind her", sane,
                    $"chased {chased} · he was {outside:F1} m behind her when it shut · {w.Line} · stuck up to {w.StuckLongest:F1} s · searched {Spots(w)} · caught {w.Caught} · back on his round {(w.Patrolling ? w.PatrolOffRound.ToString("F1") + " m from it" : "no")}");
                _lamDoor.SetLock(DoorLock.None);
                _lamDoor.SetOpen(1f);
            }

            // ------------------------------------------------------------ D. a long frame just as he gives up
            {
                Quiet();
                Away();
                Home();
                _guard.Pause(false);
                _guard.Perceiving = true;
                _caseStart = Time.time;
                var w = new Watch();
                bool slow = false;
                void OnState(Watchman g, WatchState s) { if (s == WatchState.Return) slow = true; }
                _guard.StateChanged += OnState;
                _guard.Alarm(new Vector3(-3f, 0f, 2f)); // a sound in the yard, away from his round
                // The frame after he turns back is a slow one (150 ms of work on the main thread).
                yield return Observe(w, 40f, () =>
                {
                    if (slow) { slow = false; System.Threading.Thread.Sleep(150); }
                    return w.Patrolling;
                });
                _guard.StateChanged -= OnState;
                Report("a 150 ms frame as he gives up", w.Patrolling && w.PatrolOffRound < 1.5f, $"{w.Line} · back on his round {(w.Patrolling ? w.PatrolOffRound.ToString("F1") + " m from it" : "no")}");
            }

            // ------------------------------------------------------------ E. paused in the middle of a search
            {
                var w = new Watch();
                yield return SeenAndChased(w);
                yield return Go(5.5f, new Vector3(-11.3f, F, -3.6f), new Vector3(-13.2f, F, -3.6f));
                Away();
                yield return Observe(w, 20f, () => _guard.State == WatchState.Search && _guard.Actor.IsMoving);
                bool searching = _guard.State == WatchState.Search;
                Vector3 spot = _guard.Actor.Destination;
                float before = Flat(spot - _guard.transform.position).magnitude;
                _guard.Pause(true);
                for (float t = 0f; t < 2f; t += Time.deltaTime) yield return null;
                _guard.Pause(false);
                for (float t = 0f; t < 1.2f; t += Time.deltaTime) yield return null;
                float after = Flat(spot - _guard.transform.position).magnitude;
                Report("paused 2 s while walking to a search spot", searching && (after < before - 0.6f || after < 0.8f),
                    $"searching {searching} · {before:F1} m from the spot before the pause, {after:F1} m 1.2 s after · {_guard.State}");
            }

            // ------------------------------------------------------------ F. saw her hide, but cannot get there
            var lamScreen = FindObjectsByType<HideSpot>().First(h => h.name == "Hide_LamScreen");
            {
                Quiet();
                _lamDoor.SetLock(DoorLock.None);
                _lamDoor.SetOpen(1f);
                Home();
                Stand(_guard, new Vector3(-15.5f, F, -3.5f), 222f);
                _guard.Sensitivity = 2f;
                _guard.Pause(false);
                _guard.Perceiving = true;
                player.Teleport(new Vector3(-19.0f, F + 0.05f, -7.4f), Quaternion.identity);
                RestrictedZone.ResetPresence();
                _caseStart = Time.time;
                for (float t = 0f; t < 10f && _guard.Awareness < Watchman.InvestigateAt; t += Time.deltaTime) yield return null;
                lamScreen.Interact();
                bool knew = _guard.KnownHide == lamScreen;
                // Now he is outside, and the door is bolted: he cannot reach the screen.
                _guard.Actor.Place(new Vector3(-10.5f, F, -3.0f), Quaternion.Euler(0f, 270f, 0f));
                _lamDoor.SetOpen(0f);
                _lamDoor.SetLock(DoorLock.Locked);
                var w = new Watch();
                float nearest = 99f;
                yield return Observe(w, 30f, () => { nearest = Mathf.Min(nearest, Flat(_guard.transform.position - lamScreen.ExitPosition).magnitude); return w.Caught > 0 || w.Patrolling; });
                Report("saw her hide, the way there bolted", knew && w.Caught == 0,
                    $"knew the place {knew} · {w.Line} · came within {nearest:F1} m of the screen · caught {w.Caught} (a catch from outside the bolted room would be wrong)");
                HideSpot.ExitAll();
                _lamDoor.SetLock(DoorLock.None);
                _lamDoor.SetOpen(1f);
            }

            // ------------------------------------------------------------ G. saw her hide, she slips out before he comes
            {
                Quiet();
                Home();
                Stand(_guard, new Vector3(-15.5f, F, -3.5f), 222f);
                _guard.Sensitivity = 2f;
                _guard.Pause(false);
                _guard.Perceiving = true;
                player.Teleport(new Vector3(-19.0f, F + 0.05f, -7.4f), Quaternion.identity);
                RestrictedZone.ResetPresence();
                _caseStart = Time.time;
                for (float t = 0f; t < 10f && _guard.Awareness < Watchman.InvestigateAt; t += Time.deltaTime) yield return null;
                lamScreen.Interact();
                bool knew = _guard.KnownHide == lamScreen;
                lamScreen.Interact(); // out again at once
                Away();
                bool forgot = _guard.KnownHide == null;
                var w = new Watch();
                yield return Observe(w, 30f, () => w.Caught > 0 || w.Patrolling);
                Report("saw her hide, she left before he came", knew && w.Caught == 0 && forgot,
                    $"knew the place {knew} · forgot it when she left {forgot} · {w.Line} · caught {w.Caught}");
            }

            // ------------------------------------------------------------ H. the story sends him somewhere mid-chase
            {
                var w = new Watch();
                yield return SeenAndChased(w);
                bool chased = _guard.State == WatchState.Chase;
                _guard.Script(_officeDoorOutside.position, 1.25f, null);
                var state = _guard.State;
                Report("story walk ordered mid-chase", chased && state == WatchState.Chase, $"chasing {chased} · after the order: {state}");
                Quiet();
            }

            // ------------------------------------------------------------ I. a story walk to a place he cannot reach
            {
                Quiet();
                Home();
                _guard.Pause(false);
                _officeDoor.SetOpen(0f);
                _officeDoor.SetLock(DoorLock.Locked);
                bool arrived = false;
                _caseStart = Time.time;
                _guard.Script(_officeInside.position, 1.25f, () => arrived = true);
                var w = new Watch();
                yield return Observe(w, 40f, () => arrived || _guard.State != WatchState.Scripted);
                float off = Flat(_guard.transform.position - _officeInside.position).magnitude;
                Report("story walk into a locked room", !arrived || off < 1.5f,
                    $"told he arrived {arrived} · {off:F1} m from the place · {w.Line} · {_guard.State}");
            }

            Quiet();
            Away();
            _probing = false;
            foreach (var w in _watchers) { w.Sensitivity = 1f; w.Perceiving = false; w.Pause(false); }
            ArmGuard();
            Time.timeScale = timeScale;
            Info("CHASE probe done");
            StoryAutopilot.ProbeTook(Time.realtimeSinceStartup - realStart);
        }

        private float _caseStart;

        private void Away() { _director.Player.ScriptedMove = null; _director.Player.ForceCrouch = false; AutoTeleport(new Vector3(-16.5f, 0.4f, 4.5f)); }
    }
}
