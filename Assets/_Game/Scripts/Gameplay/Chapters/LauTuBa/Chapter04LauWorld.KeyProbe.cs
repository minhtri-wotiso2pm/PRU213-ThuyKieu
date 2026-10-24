using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThuyKieu.Core;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Stealth;
using ThuyKieu.Interaction;
using UnityEngine;

namespace ThuyKieu.Gameplay.Chapters.LauTuBa
{
    /// <summary>
    /// QA for Tú Bà's sleep and the key theft ("-autopilotKeys"), on the real scene, through the real input path: Kiều
    /// stands at the sập, her eyes turn to the keys, E is pressed and held and the mouse drawn
    /// (<see cref="InputReader.SimulatedInteract"/>, <see cref="InputReader.SimulatedLook"/>, <see cref="InputReader.SimulatedMove"/>),
    /// so <see cref="PlayerInteraction"/> and the camera drive <see cref="KeyTheft"/> as they would for a player.
    /// <list type="number">
    /// <item>The breath watch (every autopilot run, from the chapter's start): no breath while she is up, one breath source.</item>
    /// <item>Where the ring hangs standing and lying, and that the keys are the target from beside the sập.</item>
    /// <item>Pause, stirring, roused: the breath stops and its clock holds.</item>
    /// <item>Outside the close view (another target, out of reach, E carried over): nothing opens, no catch.</item>
    /// <item>Leaning in (with E in her in-breath): the framing, the lens, the mouse not turning the head, the opening E.</item>
    /// <item>The work: one out-breath, a still hand, the wrong way, a jerk, re-gripping, the pause menu, backing out and in,
    /// the story taking the controls.</item>
    /// <item>Wrong moment (held still into the in-breath, pressed in it, while she stirs, at a low frame rate, a long frame,
    /// almost done, caught while leaning in): one catch straight from the close view, one failure, nothing gained on that
    /// frame, and after the checkpoint the work gone and her eyes, lens and controls back.</item>
    /// <item>The right way, 12 s of drawing on her out-breaths: the keys once, the time it took, the hook left on her, then
    /// a checkpoint after them.</item>
    /// </list>
    /// </summary>
    public partial class Chapter04LauWorld
    {
        private bool _keysProbed;
        private PlayerInteraction _interaction;

        // ------------------------------------------------------------------ breath watch (all autopilot runs)

        private int _breathFrames, _breathWrong, _breathSources;
        private float _firstBreathAfterBed = -1f, _bedTime = -1f;
        private string _breathWrongWhat;

        private bool _wasLying, _standingChecked;
        private string _standingRing;

        /// <summary>
        /// Every frame of an autopilot run from the chapter's start (called from Update: checkpoints stop coroutines): is
        /// the breath playing only while she is asleep in bed? Also notes once where the ring hangs while she stands.
        /// </summary>
        private void WatchBreath()
        {
            if (_tuBa.IsLying && !_wasLying) _bedTime = Time.time;
            _wasLying = _tuBa.IsLying;
            if (_sleeper.Snoring)
            {
                _breathFrames++;
                if (_firstBreathAfterBed < 0f && _bedTime >= 0f) _firstBreathAfterBed = Time.time - _bedTime;
                bool right = _sleeper.InBed && _sleeper.State == TuBaSleeper.Sleep.Asleep && !CatchScene.Playing;
                if (!right && _breathWrong++ == 0) _breathWrongWhat = $"stage {_stage}, {_sleeper.State}, lying {_tuBa.IsLying}, in bed {_sleeper.InBed}, catch {CatchScene.Playing}, scene running {!_director.InExplore}";
            }
            if (Time.frameCount % 30 == 0) _breathSources = Mathf.Max(_breathSources, _tuBa.GetComponentsInChildren<AudioSource>(true).Count(s => s.name == "Breath"));
            if (!_standingChecked && !_tuBa.IsLying && _keyTheft.Ring != null && _keyTheft.Ring.activeInHierarchy && Time.timeSinceLevelLoad > 1.5f)
            {
                _standingChecked = true;
                bool ok = RingAtHip(out string detail);
                _standingRing = (ok ? "ok" : "FAIL") + " · " + detail + $" · breathing {_sleeper.Breathing} · keys can be taken {_keyTheft.IsAvailable} · scene running {!_director.InExplore}";
                StoryAutopilot.Note("C4 KEYS standing (opening): " + _standingRing);
            }
        }

        /// <summary>The watch's findings so far, as a report line.</summary>
        private string BreathWatch() =>
            $"breath played on {_breathFrames} frames · while not asleep in bed {_breathWrong}{(_breathWrong > 0 ? " (first: " + _breathWrongWhat + ")" : "")} · breath sources on her {_breathSources}";

        // ------------------------------------------------------------------ the probe

        private IEnumerator KeyProbe()
        {
            float timeScale = Time.timeScale, realStart = Time.realtimeSinceStartup;
            Time.timeScale = 1f;
            _probing = true;
            _interaction = FindFirstObjectByType<PlayerInteraction>();
            var player = _director.Player;
            var camera = _director.CameraController.GetComponent<Camera>();
            foreach (var w in _watchers) { w.EndScript(false); w.Forget(); w.Perceiving = false; w.Pause(true); }

            void Report(string test, bool ok, string detail)
            {
                Info("KEYS " + (ok ? "ok   " : "FAIL ") + test + " · " + detail);
                if (!ok) StoryAutopilot.Problem("[KeyProbe] " + test + " · " + detail);
            }

            // ---------------------------------------------------------------- 1. the opening, standing
            Report("opening: no breath while she is up", _breathWrong == 0 && _breathSources <= 1, BreathWatch());
            var ring = _keyTheft;
            // Noted by the watch while she stood at Kiều's door in the opening.
            Report("standing (opening): ring at her hip, no breath, no theft",
                _standingRing != null && _standingRing.StartsWith("ok") && _standingRing.Contains("breathing False") && _standingRing.Contains("taken False"), _standingRing ?? "never seen standing");
            for (float t = 0f; t < 30f && !_tuBa.IsLying; t += Time.deltaTime) yield return null;
            PutTuBaToBed(false);
            for (float t = 0f; t < 0.5f; t += Time.deltaTime) yield return null;
            Report("in bed: breathing starts on an in-breath", _sleeper.Breathing && _sleeper.Snoring && _sleeper.LastBreathId == "snore_deep",
                $"breathing {_sleeper.Breathing} · sound {_sleeper.Snoring} ({_sleeper.LastBreathId}) · first breath {(_firstBreathAfterBed >= 0f ? _firstBreathAfterBed.ToString("F2") + " s after she lay down" : "before this probe")}");

            // ---------------------------------------------------------------- 2. the ring lying, the target on it
            Report("lying: ring on the mat beside her hip", RingLying(out string lying), lying);
            float follow = Vector3.Distance(_keyTheft.transform.position, RingRoot().Find("Grip").position);
            Report("lying: the hand's target is on the keys", follow < 0.01f, $"trigger {follow * 100f:F1} cm from the keys' middle");
            Vector3 stand = KeyStand();
            yield return AimAtKeys(stand, true);
            Report("crouched beside the sập, eyes on the keys: they are the target", ReferenceEquals(_interaction.CurrentTarget, _keyTheft),
                $"target {Describe(_interaction.CurrentTarget)} · {Flat(stand - _keyTheft.transform.position).magnitude:F2} m from them · eyes {_director.CameraController.transform.position.y - LauTuBaFloor:F2} m above the floor");
            if (StoryCapture.Directory != null) StoryCapture.Shot("KEYS_lying_crouched");
            yield return AimAtKeys(stand, false);
            Report("standing beside the sập, eyes on the keys: they are the target", ReferenceEquals(_interaction.CurrentTarget, _keyTheft), $"target {Describe(_interaction.CurrentTarget)}");
            if (StoryCapture.Directory != null) yield return KeyStills(stand);

            // ---------------------------------------------------------------- 3. the clock: pause, stir, rouse
            yield return UntilPhase(true, 0.5f);
            float leftBefore = _sleeper.PhaseLeft;
            Time.timeScale = 0f;
            AudioListener.pause = true;
            bool frozen = true;
            for (float r = Time.realtimeSinceStartup; Time.realtimeSinceStartup - r < 1f;) { yield return null; frozen &= _sleeper.InhaledThisFrame == 0f && _sleeper.ExhaledThisFrame == 0f; }
            float leftPaused = _sleeper.PhaseLeft;
            Time.timeScale = 1f;
            AudioListener.pause = false;
            Report("paused: the breath clock holds, its sound waits", frozen && Mathf.Abs(leftPaused - leftBefore) < 0.001f && _sleeper.Snoring,
                $"phase left {leftBefore:F3} → {leftPaused:F3} s after 1 s paused · sound still queued {_sleeper.Snoring}");
            yield return UntilPhase(true, 0.6f);
            _sleeper.Stir(3.5f);
            bool stopped = !_sleeper.Snoring && !_sleeper.Breathing;
            yield return null;
            Report("stirring: the snore stops at once", stopped && !_sleeper.Snoring, $"sound {_sleeper.Snoring} · {_sleeper.State}");
            for (float t = 0f; t < 6f && _sleeper.State != TuBaSleeper.Sleep.Asleep; t += Time.deltaTime) yield return null;
            yield return null;
            Report("settled again: breath restarts on an in-breath", _sleeper.Breathing && _sleeper.LastBreathId == "snore_deep" && _sleeper.Snoring, $"{_sleeper.State} · {_sleeper.LastBreathId}");
            // Roused with Kiều out of her room (at the bedside that is her catch, tested by the sound probe).
            AutoTeleport(_tuBaDoorOutside.position + Vector3.up * 0.05f);
            yield return null;
            _sleeper.Rouse(_tuBaDoor.transform.position);
            Report("roused: no breath", !_sleeper.Snoring && !_sleeper.Breathing && !CatchScene.Playing, $"sound {_sleeper.Snoring} · catch {CatchScene.Playing}");
            SleepAgain();
            for (int i = 0; i < 5; i++) yield return null;

            // ---------------------------------------------------------------- 4. outside the close view: no catch, no gain
            int failsBefore = _director.GetInt("c4_bi_bat"), scenesBefore = CatchScene.Count;
            yield return AimAt(stand, _sleeper.Head + Vector3.up * 1.3f + Vector3.left * 1.5f, true);
            var other = _interaction.CurrentTarget;
            InputReader.SimulatedInteract = true;
            yield return UntilPhase(false, 0.2f);
            yield return UntilPhase(true, 0.2f);
            yield return UntilPhase(false, 0.2f);
            InputReader.SimulatedInteract = false;
            Report("E held at something else through her in-breath", CatchScene.Count == scenesBefore && ring.HeldSeconds == 0f && !ring.InMinigame && _director.InExplore,
                $"target {Describe(other)} · catches {CatchScene.Count - scenesBefore} · held {ring.HeldSeconds:F2} s · leaning in {ring.InMinigame}");
            Vector3 far = _keyTheft.transform.position + Flat(stand - _keyTheft.transform.position).normalized * 3.2f;
            far.y = LauTuBaFloor + 0.05f;
            yield return AimAt(far, _keyTheft.transform.position, false);
            var farTarget = _interaction.CurrentTarget;
            InputReader.SimulatedInteract = true;
            yield return UntilPhase(true, 0.2f);
            yield return UntilPhase(false, 0.2f);
            InputReader.SimulatedInteract = false;
            Report("out of reach (3 m), E held through her in-breath", !ReferenceEquals(farTarget, _keyTheft) && CatchScene.Count == scenesBefore && !ring.InMinigame,
                $"target {Describe(farTarget)} · catches {CatchScene.Count - scenesBefore}");
            // E already down (held at the wall) when the eyes come to the keys: not a press, nothing opens.
            yield return AimAt(stand, _sleeper.Head + Vector3.up * 1.3f + Vector3.left * 1.5f, true);
            InputReader.SimulatedInteract = true;
            for (float t = 0f; t < 0.3f; t += Time.deltaTime) yield return null;
            yield return AimAtKeys(stand, true, keepE: true);
            for (float t = 0f; t < 1f; t += Time.deltaTime) yield return null;
            bool carriedOpened = ring.InMinigame;
            InputReader.SimulatedInteract = false;
            yield return null;
            Report("E carried over onto the keys: nothing opens", !carriedOpened && CatchScene.Count == scenesBefore, $"leaning in {carriedOpened}");

            // ---------------------------------------------------------------- 5. leaning in
            float ownFov = camera.fieldOfView;
            // Pressed during her in-breath, and kept down: opening is not taking hold.
            yield return UntilPhase(true, 0.2f);
            int restores = 0;
            void OnRestore() => restores++;
            _director.CheckpointRestored += OnRestore;
            yield return AimAtKeys(stand, true);
            InputReader.SimulatedInteract = true;
            float entered = 0f;
            for (; entered < 2f && !ring.Ready; entered += Time.deltaTime) yield return null;
            Report("E in her in-breath opens the close view, not a grab", ring.Ready && CatchScene.Count == scenesBefore && !ring.HandOnKeys && player.HeldStill,
                $"ready after {entered:F2} s · catches {CatchScene.Count - scenesBefore} · hand on {ring.HandOnKeys} · Kiều held still {player.HeldStill}");
            Report("close view: keys in front, her chest and face in the picture, nothing cut through, lens narrowed", ViewCheck(camera, ownFov, out string view), view);
            if (StoryCapture.Directory != null) StoryCapture.Shot("KEYS_close_view");
            // Still holding the E that opened it, through an out-breath, pulling the right way: nothing until pressed again.
            yield return UntilPhase(false, 0.3f);
            for (float t = 0f; t < 1f; t += Time.deltaTime) { PullFrame(0.12f); yield return null; }
            float carriedGain = ring.HeldSeconds;
            InputReader.SimulatedInteract = false;
            InputReader.SimulatedLook = Vector2.zero;
            yield return null;
            Report("the E that opened it, still down: no hold until let go and pressed again", carriedGain == 0f && CatchScene.Count == scenesBefore, $"gained {carriedGain:F2} s");
            // The mouse does not turn the head in the close view.
            Quaternion before = camera.transform.rotation;
            for (int i = 0; i < 5; i++) { InputReader.SimulatedLook = new Vector2(300f, 200f); yield return null; }
            InputReader.SimulatedLook = Vector2.zero;
            Report("mouse in the close view does not turn the head", Quaternion.Angle(before, camera.transform.rotation) < 0.5f, $"turned {Quaternion.Angle(before, camera.transform.rotation):F2}°");

            // ---------------------------------------------------------------- 6. the work
            yield return OneBreath(0.25f, 0.3f);
            float afterOne = ring.HeldSeconds;
            yield return UntilPhase(true, 0.2f);
            yield return UntilPhase(false, 0.05f);
            Report("one out-breath, drawing the right way, let go before the in-breath", afterOne > 1.4f && afterOne < 2.6f && Mathf.Abs(ring.HeldSeconds - afterOne) < 0.001f && ring.InMinigame && CatchScene.Count == scenesBefore,
                $"gained {afterOne:F2} s (out-breath {_sleeper.ExhaleSeconds:F1} s) · kept through the in-breath {ring.HeldSeconds:F2} s · still leaning in {ring.InMinigame}");
            float mark = ring.HeldSeconds;
            yield return OneBreath(0.25f, 0.3f, pull: 0f);
            Report("holding without drawing: nothing", Mathf.Abs(ring.HeldSeconds - mark) < 0.001f && CatchScene.Count == scenesBefore, $"gained {ring.HeldSeconds - mark:F2} s");
            mark = ring.HeldSeconds;
            yield return OneBreath(0.25f, 0.3f, pull: -0.12f);
            Report("drawing the wrong way: nothing", Mathf.Abs(ring.HeldSeconds - mark) < 0.001f && ring.LastPull != KeyTheft.Pull.Right, $"gained {ring.HeldSeconds - mark:F2} s · last pull {ring.LastPull}");
            mark = ring.HeldSeconds;
            yield return OneBreath(0.25f, 0.3f, pull: 2.5f);
            Report("a jerk: nothing (too fast)", ring.HeldSeconds - mark < 0.05f, $"gained {ring.HeldSeconds - mark:F2} s · last pull {ring.LastPull}");
            // Let go part way and take hold again in the same out-breath (re-gripping the mouse): the work goes on.
            mark = ring.HeldSeconds;
            yield return OneBreath(0.25f, 1.4f);
            yield return null;
            InputReader.SimulatedInteract = true;
            float regrip = 0f;
            while (_sleeper.Exhaling && _sleeper.PhaseLeft > 0.3f) { PullFrame(0.12f); regrip += Time.deltaTime; yield return null; }
            InputReader.SimulatedInteract = false;
            InputReader.SimulatedLook = Vector2.zero;
            yield return null;
            Report("let go and take hold again in one out-breath: both parts count", ring.HeldSeconds - mark > 1.2f && CatchScene.Count == scenesBefore, $"gained {ring.HeldSeconds - mark:F2} s");
            Report("progress shows on the keys: the stage moved on", ring.Stage >= 1, $"stage {ring.Stage + 1}/3 at {ring.HeldSeconds:F2} s");
            if (StoryCapture.Directory != null) StoryCapture.Shot("KEYS_close_stage" + (ring.Stage + 1));

            // Pause and resume from the menu: still leaning in, nothing lost, nothing held.
            var pause = FindFirstObjectByType<ThuyKieu.UI.PauseMenuView>();
            mark = ring.HeldSeconds;
            if (pause != null)
            {
                pause.Pause();
                for (float r = Time.realtimeSinceStartup; Time.realtimeSinceStartup - r < 0.6f;) yield return null;
                pause.Resume();
                for (int i = 0; i < 5; i++) yield return null;
            }
            Report("pause menu and back: still in close, work kept", pause != null && ring.InMinigame && Mathf.Abs(ring.HeldSeconds - mark) < 0.001f && !ring.HandOnKeys && CatchScene.Count == scenesBefore,
                $"menu found {pause != null} · leaning in {ring.InMinigame} · held {mark:F2} → {ring.HeldSeconds:F2} s");

            // Backing out (S): her eyes again, her own lens, free to move, the work kept; leaning in again picks it up.
            InputReader.SimulatedMove = new Vector2(0f, -1f);
            yield return null;
            yield return null;
            InputReader.SimulatedMove = Vector2.zero;
            for (float t = 0f; t < 1.6f; t += Time.deltaTime) yield return null;
            Report("backing out: the eyes and lens back, Kiều free, work kept", !ring.InMinigame && !player.ControlLocked && !player.HeldStill && Mathf.Abs(camera.fieldOfView - ownFov) < 1f
                && _director.CameraController.CloseWeight <= 0f && Mathf.Abs(ring.HeldSeconds - mark) < 0.001f && PlayerInteraction.Pinned == null,
                $"leaning in {ring.InMinigame} · Kiều locked {player.ControlLocked} · lens {camera.fieldOfView:F1}° (hers {ownFov:F1}°) · close weight {_director.CameraController.CloseWeight:F2} · held {ring.HeldSeconds:F2} s");
            yield return EnterKeys(stand, true);
            Report("leaning in again", ring.Ready && Mathf.Abs(ring.HeldSeconds - mark) < 0.001f, $"ready {ring.Ready} · held {ring.HeldSeconds:F2} s");

            // The story takes the controls with a hand on the keys (as a conversation would): out of the close view, no catch.
            yield return UntilPhase(false, 0.3f);
            InputReader.SimulatedInteract = true;
            for (float t = 0f; t < 0.5f; t += Time.deltaTime) { PullFrame(0.12f); yield return null; }
            InputReader.SimulatedLook = Vector2.zero;
            _interaction.ControlLocked = true;
            float lockedAt = ring.HeldSeconds;
            yield return UntilPhase(true, 0.6f);
            InputReader.SimulatedInteract = false;
            Report("controls taken mid-hold: out of the close view, no catch, no gain", CatchScene.Count == scenesBefore && Mathf.Abs(ring.HeldSeconds - lockedAt) < 0.001f && !ring.InMinigame && !ring.HandOnKeys,
                $"held {lockedAt:F2} → {ring.HeldSeconds:F2} s · leaning in {ring.InMinigame} · catches {CatchScene.Count - scenesBefore}");
            // The story gives them back.
            _interaction.ControlLocked = false;
            player.ControlLocked = false;
            yield return null;
            Report("no failure so far", restores == 0, $"checkpoint restores {restores}");
            _director.CheckpointRestored -= OnRestore;

            // ---------------------------------------------------------------- 7. the wrong moment
            yield return WrongMoment("held still from her out-breath into her in-breath", Report, stand, ring, ownFov, () => UntilPhase(true, 0f, keepE: true), pressInExhale: true);
            yield return WrongMoment("pressed during her in-breath", Report, stand, ring, ownFov, null, pressInExhale: false);
            yield return WrongMoment("pressed while she stirs (no snore)", Report, stand, ring, ownFov, null, pressInExhale: false, stir: true);
            yield return WrongMoment("held across the turn at ~8 FPS", Report, stand, ring, ownFov, () => UntilPhase(true, 0f, keepE: true), pressInExhale: true, slow: 0.12f);
            yield return WrongMoment("one 0.3 s frame across the turn", Report, stand, ring, ownFov, () => LongFrameAcross(), pressInExhale: true);
            yield return WrongMoment("almost done, held into her in-breath", Report, stand, ring, ownFov, () => UntilPhase(true, 0f, keepE: true), pressInExhale: true, nearlyDone: true);
            yield return CaughtWhileLeaningIn(Report, stand, ring, ownFov);

            // At ~8 FPS the right way: gain only for her out-breath time with the hand on and drawing.
            SleepAgain();
            yield return EnterKeys(stand, true);
            float slowStart = ring.HeldSeconds, outWhileHeld = 0f;
            int scenesSlow = CatchScene.Count;
            yield return UntilPhase(false, 0.2f, startWithin: 0.6f);
            InputReader.SimulatedInteract = true;
            yield return null;
            while (_sleeper.Exhaling && _sleeper.PhaseLeft > 0.35f)
            {
                PullFrame(0.12f);
                System.Threading.Thread.Sleep(110);
                yield return null;
                if (ring.HandOnKeys && ring.LastPull == KeyTheft.Pull.Right) outWhileHeld += _sleeper.ExhaledThisFrame;
            }
            InputReader.SimulatedInteract = false;
            InputReader.SimulatedLook = Vector2.zero;
            yield return null;
            float slowGain = ring.HeldSeconds - slowStart;
            Report("~8 FPS, one out-breath drawn: gain = her out-breath time while drawing right", CatchScene.Count == scenesSlow && Mathf.Abs(slowGain - outWhileHeld) < 0.02f && slowGain > 0.6f,
                $"gained {slowGain:F2} s · out-breath under a right pull {outWhileHeld:F2} s");

            // ---------------------------------------------------------------- 8. the right way, from the checkpoint
            SleepAgain();
            _keyTheft.Restore(); // from nothing, as after a checkpoint
            yield return TakeKeysTheRightWay(Report, ring, stand);
            yield return null;
            Report("done: back to her eyes, the hook still on her", !ring.InMinigame && !player.ControlLocked && !player.HeldStill && RingRoot().gameObject.activeInHierarchy && !ring.Ring.activeSelf,
                $"leaning in {ring.InMinigame} · Kiều locked {player.ControlLocked} · hook shown {RingRoot().gameObject.activeInHierarchy} · keys shown {ring.Ring.activeSelf}");
            for (float t = 0f; t < 1.6f; t += Time.deltaTime) yield return null;
            Report("done: her own lens back", Mathf.Abs(camera.fieldOfView - ownFov) < 1f, $"lens {camera.fieldOfView:F1}° (hers {ownFov:F1}°)");

            // E at the hook afterwards: nothing to take, nothing opens, no catch.
            int scenesAfter = CatchScene.Count;
            yield return AimAt(stand, RingRoot().position, true);
            InputReader.SimulatedInteract = true;
            yield return UntilPhase(true, 0.6f);
            InputReader.SimulatedInteract = false;
            Report("keys taken: E at the hook through her in-breath", CatchScene.Count == scenesAfter && !ring.IsAvailable && !ring.InMinigame, $"catches {CatchScene.Count - scenesAfter} · leaning in {ring.InMinigame}");

            // A checkpoint after the keys: they stay taken, the hook stays on her, she sleeps on with one breath source.
            _director.Fail("QA", "checkpoint after the keys");
            for (float t = 0f; t < 20f && !_director.InExplore; t += Time.unscaledDeltaTime) yield return null;
            for (int i = 0; i < 10; i++) yield return null;
            Report("checkpoint after the keys: still taken, the hook on her, she sleeps on", _inventory.Has("chia") && !ring.Ring.activeSelf && RingRoot().gameObject.activeInHierarchy && _stage == 2 && _sleeper.Breathing && _tuBa.IsLying,
                $"keys {_inventory.Has("chia")} · keys shown {ring.Ring.activeSelf} · hook shown {RingRoot().gameObject.activeInHierarchy} · stage {_stage} · breathing {_sleeper.Breathing}");
            Report("breath watch after all of it", _breathWrong == 0 && _breathSources == 1, BreathWatch());

            player.ForceCrouch = false;
            InputReader.SimulatedLook = InputReader.SimulatedMove = Vector2.zero;
            foreach (var w in _watchers) { w.Sensitivity = 1f; w.Perceiving = false; w.Pause(false); }
            ArmGuard();
            _guard.Perceiving = _gateGuard.Perceiving = false;
            _probing = false;
            Time.timeScale = timeScale;
            Info("KEYS probe done");
            StoryAutopilot.ProbeTook(Time.realtimeSinceStartup - realStart);
        }

        /// <summary>
        /// The close view as it is: the keys, her chest and her face inside the picture (keys nearest), the lens narrower
        /// than hers, the camera not inside anything.
        /// </summary>
        private bool ViewCheck(Camera camera, float ownFov, out string detail)
        {
            var hips = _tuBa.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.Hips).position;
            Vector3 head = _sleeper.Head, chest = Vector3.Lerp(hips, head, 0.55f), keys = _keyTheft.Ring.transform.position;
            Vector3 vk = camera.WorldToViewportPoint(keys), vc = camera.WorldToViewportPoint(chest), vh = camera.WorldToViewportPoint(head);
            bool In(Vector3 v) => v.z > 0.05f && v.x > 0.03f && v.x < 0.97f && v.y > 0.03f && v.y < 0.97f;
            var inside = Physics.OverlapSphere(camera.transform.position, 0.05f, ~(1 << 2), QueryTriggerInteraction.Ignore);
            detail = $"keys at ({vk.x:F2}, {vk.y:F2}) {vk.z:F2} m · chest ({vc.x:F2}, {vc.y:F2}) {vc.z:F2} m · face ({vh.x:F2}, {vh.y:F2}) {vh.z:F2} m · lens {camera.fieldOfView:F1}° (hers {ownFov:F1}°) · inside {(inside.Length == 0 ? "nothing" : string.Join(", ", inside.Select(c => c.name)))} · camera {Vector3.Distance(camera.transform.position, _director.Player.transform.position + Vector3.up * _director.Player.EyeHeight):F2} m from her eyes";
            return In(vk) && In(vc) && In(vh) && vk.z < vc.z && camera.fieldOfView < ownFov - 6f && camera.fieldOfView > 50f && inside.Length == 0;
        }

        /// <summary>
        /// The ordinary autopilot runs take the keys through the minigame too (no shortcut): beside the sập, leaning in, E and
        /// a slow draw of the mouse on her out-breaths. The time it took goes in the report.
        /// </summary>
        private IEnumerator AutoTakeKeys()
        {
            _interaction = FindFirstObjectByType<PlayerInteraction>();
            for (float t = 0f; t < 30f && !_sleeper.Breathing; t += Time.deltaTime) yield return null;
            // Lying down moves the ring; the hand's target follows it after the frame (LateUpdate).
            for (int i = 0; i < 5; i++) yield return null;
            float start = Time.time;
            int scenes = CatchScene.Count;
            yield return EnterKeys(KeyStand(), true);
            while (!_inventory.Has("chia") && Time.time - start < 90f && _director.InExplore && !CatchScene.Playing)
            {
                if (!_keyTheft.InMinigame) yield return EnterKeys(KeyStand(), true);
                yield return OneBreath(0.25f, 0.3f);
            }
            InputReader.SimulatedInteract = false;
            InputReader.SimulatedLook = Vector2.zero;
            _director.Player.ForceCrouch = false;
            bool ok = _inventory.Has("chia") && CatchScene.Count == scenes;
            StoryAutopilot.Note($"C4 keys taken through the minigame {(ok ? "ok" : "FAIL")} · {Time.time - start:F1} s game time · held {_keyTheft.HeldSeconds:F1}/{_keyTheft.Seconds:F0} s · catches {CatchScene.Count - scenes} · {BreathWatch()}");
            if (!ok) StoryAutopilot.Problem($"[Keys] the autopilot did not get the keys through the minigame: held {_keyTheft.HeldSeconds:F1} s, leaning in {_keyTheft.InMinigame}, target {Describe(_interaction.CurrentTarget)}, catches {CatchScene.Count - scenes}");
        }

        /// <summary>
        /// Leans in over the keys as a player does: eyes on them from <paramref name="stand"/>, a press of E (let go again),
        /// then waits for the view to settle.
        /// </summary>
        private IEnumerator EnterKeys(Vector3 stand, bool crouch)
        {
            InputReader.SimulatedInteract = false;
            yield return AimAtKeys(stand, crouch);
            InputReader.SimulatedInteract = true;
            yield return null;
            yield return null;
            InputReader.SimulatedInteract = false;
            for (float t = 0f; t < 2f && !_keyTheft.Ready && !CatchScene.Playing; t += Time.deltaTime) yield return null;
        }

        /// <summary>One frame of drawing the mouse the way shown, at <paramref name="speed"/> screen heights a second.</summary>
        private void PullFrame(float speed)
        {
            InputReader.SimulatedLook = _keyTheft.PullDirection * (speed * Mathf.Max(1, Screen.height) * Time.deltaTime);
        }

        /// <summary>A wrong-moment hold: one catch by her, one failure, nothing gained that frame, the checkpoint resets the work.</summary>
        private IEnumerator WrongMoment(string test, System.Action<string, bool, string> report, Vector3 stand, KeyTheft ring, float ownFov, System.Func<IEnumerator> then,
            bool pressInExhale, bool stir = false, float slow = 0f, bool nearlyDone = false)
        {
            SleepAgain();
            yield return EnterKeys(stand, true);
            // Some work first, so "nothing gained" and "taken back" mean something.
            while (ring.HeldSeconds < (nearlyDone ? 11.3f : 0.5f) && ring.InMinigame && !CatchScene.Playing)
                yield return OneBreath(0.25f, 0.3f, stopAt: nearlyDone ? 11.3f : 99f);
            int fails = _director.GetInt("c4_bi_bat"), scenes = CatchScene.Count, restores = 0, countedAtCatch = -1;
            float heldAtCatch = -1f, gainedOnCatch = -1f;
            bool snoreAtCatch = true, cameraTakenAtCatch = false;
            void OnGrab(TuBaSleeper s, TuBaSleeper.Grab why) { heldAtCatch = ring.HeldSeconds; gainedOnCatch = ring.HeldSeconds - _heldLastFrame; snoreAtCatch = s.Snoring; }
            void OnRestore() => restores++;
            _director.CheckpointRestored += OnRestore;
            var tracking = StartCoroutine(TrackHeld(ring));
            _sleeper.HandCaught += OnGrab;
            if (stir) { yield return UntilPhase(false, 0.5f); _sleeper.Stir(3.5f); yield return null; }
            else if (pressInExhale) { yield return UntilPhase(false, 0.3f, startWithin: 0.6f); InputReader.SimulatedInteract = true; for (float t = 0f; t < 0.4f; t += Time.deltaTime) { if (!nearlyDone) PullFrame(0.12f); yield return null; } InputReader.SimulatedLook = Vector2.zero; }
            else yield return UntilPhase(true, 0.3f, startWithin: 0.6f);
            if (slow > 0f)
            {
                for (float t = 0f; t < 5f && !CatchScene.Playing && _director.InExplore; t += Time.deltaTime)
                {
                    InputReader.SimulatedInteract = true;
                    System.Threading.Thread.Sleep((int)(slow * 1000f));
                    yield return null;
                }
            }
            else
            {
                if (!InputReader.SimulatedInteract) InputReader.SimulatedInteract = true;
                if (then != null) yield return then();
                for (float t = 0f; t < 3f && !CatchScene.Playing && _director.InExplore; t += Time.deltaTime) yield return null;
            }
            cameraTakenAtCatch = CatchScene.Playing && !_director.CameraController.enabled;
            InputReader.SimulatedInteract = false;
            InputReader.SimulatedLook = Vector2.zero;
            _sleeper.HandCaught -= OnGrab;
            if (tracking != null) StopCoroutine(tracking);
            bool caught = CatchScene.Playing || !_director.InExplore;
            bool hers = _catch.LastKind == CatchScene.Kind.Front && CatchScene.Count == scenes + 1;
            bool taken = _inventory.Has("chia");
            countedAtCatch = _director.GetInt("c4_bi_bat") - fails;
            for (float t = 0f; t < 1.1f && CatchScene.Playing; t += Time.deltaTime) yield return null;
            if (!_catchPoseReported && CatchScene.Playing)
            {
                _catchPoseReported = true;
                if (StoryCapture.Directory != null) StoryCapture.Shot("KEYS_catch");
                report("her catch: ring at her hip as she grabs", RingAtHip(out string hipDetail), hipDetail);
            }
            yield return Settle();
            SleepAgain();
            for (float t = 0f; t < 1.5f; t += Time.deltaTime) yield return null;
            _director.CheckpointRestored -= OnRestore;
            report(test + ": her catch, once, straight from the close view", caught && hers && cameraTakenAtCatch && countedAtCatch == 1 && restores == 1 && CatchScene.Count == scenes + 1 && !taken,
                $"catch {caught} ({_catch.LastKind}) · camera handed to the catch {cameraTakenAtCatch} · scenes {CatchScene.Count - scenes} · counted as caught {countedAtCatch} · failures {restores} · keys taken {taken}");
            report(test + ": nothing gained on that frame, taken back after", heldAtCatch >= 0f && gainedOnCatch < 0.0001f && !snoreAtCatch && ring.HeldSeconds == 0f && _sleeper.Breathing && _tuBa.IsLying && ring.Ring.activeSelf,
                $"held {heldAtCatch:F2} s at the catch (gained on that frame {gainedOnCatch:F3} s) · after the checkpoint {ring.HeldSeconds:F2} s · she sleeps {_sleeper.Breathing}, lying {_tuBa.IsLying}, keys shown {ring.Ring.activeSelf}");
            report(test + ": after the checkpoint, her eyes, lens and controls back", Restored(ownFov, out string back), back);
        }

        /// <summary>Caught while the camera is still leaning in (she wakes with Kiều at her bedside): the catch takes over.</summary>
        private IEnumerator CaughtWhileLeaningIn(System.Action<string, bool, string> report, Vector3 stand, KeyTheft ring, float ownFov)
        {
            SleepAgain();
            int scenes = CatchScene.Count;
            yield return AimAtKeys(stand, true);
            InputReader.SimulatedInteract = true;
            yield return null;
            yield return null;
            InputReader.SimulatedInteract = false;
            // A fifth of a second into the half-second lean.
            for (float t = 0f; t < 0.2f; t += Time.deltaTime) yield return null;
            float weight = _director.CameraController.CloseWeight;
            _sleeper.Rouse(_tuBaDoor.transform.position);
            for (float t = 0f; t < 2f && !CatchScene.Playing; t += Time.deltaTime) yield return null;
            bool played = CatchScene.Playing;
            // The close view lets go on its next tick.
            yield return null;
            yield return null;
            bool handed = played && CatchScene.Playing && !_director.CameraController.enabled && !ring.InMinigame;
            yield return Settle();
            SleepAgain();
            for (float t = 0f; t < 1.5f; t += Time.deltaTime) yield return null;
            bool restored = Restored(ownFov, out string back);
            report("caught half way into the close view: her catch takes the camera, then all back", handed && CatchScene.Count == scenes + 1 && weight > 0f && weight < 1f && restored,
                $"leaning in {weight:F2} when she woke · catch played {played} · it took the camera {handed} · afterwards: {back}");
        }

        /// <summary>After a checkpoint: out of the close view, her own lens, eyes and controls, nothing pinned.</summary>
        private bool Restored(float ownFov, out string detail)
        {
            var cam = _director.CameraController;
            float fov = cam.GetComponent<Camera>().fieldOfView;
            bool controls = !_director.Player.ControlLocked && !_director.Player.HeldStill && !_interaction.ControlLocked;
            detail = $"leaning in {_keyTheft.InMinigame} · close weight {cam.CloseWeight:F2} · lens {fov:F1}° (hers {ownFov:F1}°) · camera on {cam.enabled} · controls free {controls} · pinned {PlayerInteraction.Pinned != null}";
            return !_keyTheft.InMinigame && cam.CloseWeight <= 0f && Mathf.Abs(fov - ownFov) < 1.5f && cam.enabled && controls && PlayerInteraction.Pinned == null;
        }

        private int _pressNotes;
        private float _heldLastFrame;
        private bool _catchPoseReported;

        /// <summary>The work as it stood at the end of each frame (what a catch frame added is measured against it).</summary>
        private IEnumerator TrackHeld(KeyTheft ring)
        {
            while (true) { _heldLastFrame = ring.HeldSeconds; yield return null; }
        }

        /// <summary>One long frame (0.3 s) taken just before the out-breath ends, E down.</summary>
        private IEnumerator LongFrameAcross()
        {
            while (_sleeper.Exhaling && _sleeper.PhaseLeft > 0.15f) yield return null;
            System.Threading.Thread.Sleep(300);
            yield return null;
        }

        /// <summary>
        /// The keys the way a careful player takes them: leaning in, E pressed a quarter second into each out-breath and the
        /// mouse drawn slowly the way shown, let go 0.3 s before it ends, until the ring is free. Reports the work that
        /// counted against the time it took in all (leaning in included).
        /// </summary>
        private IEnumerator TakeKeysTheRightWay(System.Action<string, bool, string> report, KeyTheft ring, Vector3 stand)
        {
            int taken = 0, scenes = CatchScene.Count, before = _inventory.Items.Count(i => i == "chia");
            void OnTaken(KeyTheft k) => taken++;
            ring.Taken += OnTaken;
            float start = Time.time, heldTime = 0f, startHeld = ring.HeldSeconds;
            int breaths = 0;
            yield return EnterKeys(stand, true);
            int shot = 0;
            while (!_inventory.Has("chia") && Time.time - start < 90f && _director.InExplore && !CatchScene.Playing)
            {
                if (!ring.InMinigame) yield return EnterKeys(stand, true);
                yield return OneBreath(0.25f, 0.3f, () => heldTime += Time.deltaTime);
                breaths++;
                if (StoryCapture.Directory != null && ring.InMinigame && ring.Stage > shot) { shot = ring.Stage; StoryCapture.Shot("KEYS_close_stage" + (shot + 1) + "_rest"); }
            }
            float total = Time.time - start;
            ring.Taken -= OnTaken;
            report("12 s of drawing on her out-breaths frees the ring, once", taken == 1 && _inventory.Items.Count(i => i == "chia") == before + 1 && !ring.Ring.activeSelf && CatchScene.Count == scenes && _stage == 2,
                $"taken {taken}× · keys in hand {_inventory.Items.Count(i => i == "chia")} · keys shown {ring.Ring.activeSelf} · stage {_stage} · catches {CatchScene.Count - scenes}");
            report("time: counted work vs. time taken", Mathf.Abs(ring.Seconds - 12f) < 0.001f && ring.HeldSeconds - startHeld <= 12.001f && total >= 18f && total <= 34f,
                $"setting {ring.Seconds:F1} s · counted {ring.HeldSeconds - startHeld:F2} s of work (E down {heldTime:F2} s) over {breaths} out-breaths · {total:F1} s in all, leaning in and waiting included (aim 20–30 s)");
        }

        /// <summary>
        /// Waits for the next out-breath, presses E <paramref name="after"/> s into it and draws the mouse at
        /// <paramref name="pull"/> screen heights a second (negative: the wrong way) until <paramref name="margin"/> s before
        /// it ends (as a player who has the rhythm would), or until <paramref name="stopAt"/> seconds of work.
        /// </summary>
        private IEnumerator OneBreath(float after, float margin, System.Action whileHeld = null, float pull = 0.12f, float stopAt = 99f)
        {
            // The start of an out-breath (not one already half gone).
            while (!(_sleeper.Exhaling && _sleeper.PhaseLength - _sleeper.PhaseLeft >= after && _sleeper.PhaseLength - _sleeper.PhaseLeft <= after + 0.25f))
            {
                if (CatchScene.Playing || !_director.InExplore || _inventory.Has("chia")) yield break;
                yield return null;
            }
            InputReader.SimulatedInteract = true;
            yield return null;
            if (_pressNotes < 2)
            {
                _pressNotes++;
                Info($"KEYS info press: target {Describe(_interaction != null ? _interaction.CurrentTarget : null)} · leaning in {_keyTheft.InMinigame} (ready {_keyTheft.Ready}) · hand on {_keyTheft.HandOnKeys} · stage {_keyTheft.Stage + 1} · pull way ({_keyTheft.PullDirection.x:F2}, {_keyTheft.PullDirection.y:F2}) · screen {Screen.width}×{Screen.height}");
            }
            while (_sleeper.Exhaling && _sleeper.PhaseLeft > margin && !_inventory.Has("chia") && _keyTheft.HeldSeconds < stopAt)
            {
                InputReader.SimulatedLook = pull == 0f ? Vector2.zero : _keyTheft.PullDirection * (pull * Mathf.Max(1, Screen.height) * Time.deltaTime);
                whileHeld?.Invoke();
                yield return null;
            }
            InputReader.SimulatedInteract = false;
            InputReader.SimulatedLook = Vector2.zero;
            yield return null;
        }

        /// <summary>
        /// Waits until she breathes in (<paramref name="inhale"/>) or out, at least <paramref name="into"/> s into that
        /// phase. With <paramref name="startWithin"/> it waits for a fresh phase (no later than that far into it).
        /// </summary>
        private IEnumerator UntilPhase(bool inhale, float into, float startWithin = -1f, bool keepE = false)
        {
            for (float t = 0f, real = Time.realtimeSinceStartup; t < 15f; t += Time.deltaTime)
            {
                if (TimeStopped(real)) yield break;
                bool phase = inhale ? _sleeper.Inhaling : _sleeper.Exhaling;
                float elapsed = _sleeper.PhaseLength - _sleeper.PhaseLeft;
                if (phase && elapsed >= into && (startWithin < 0f || elapsed <= startWithin)) yield break;
                if (CatchScene.Playing || !_director.InExplore) yield break;
                if (keepE) InputReader.SimulatedInteract = true;
                yield return null;
            }
        }

        private bool _timeStopNoted;

        /// <summary>
        /// A wait counted in game time that has gone on for 3 s of real time with the clock stopped: something froze the
        /// game (a menu, an overlay). Noted once with who holds it, and the wait given up, so the probe never hangs on it.
        /// </summary>
        private bool TimeStopped(float realStart)
        {
            if (Time.timeScale > 0f || Time.realtimeSinceStartup - realStart < 3f) return false;
            if (!_timeStopNoted)
            {
                _timeStopNoted = true;
                StoryAutopilot.Problem($"[KeyProbe] game time stopped: time scale {Time.timeScale} · paused {GamePause.IsPaused} · overlay {GamePause.ShellOverlayOpen} · audio paused {AudioListener.pause} · exploring {_director.InExplore} · Kiều locked {_director.Player.ControlLocked} · catch {CatchScene.Playing}");
            }
            return true;
        }

        /// <summary>Back to sleep (as a checkpoint does) if she is not breathing in her sleep now; E and the mouse let go.</summary>
        private void SleepAgain()
        {
            InputReader.SimulatedInteract = false;
            InputReader.SimulatedLook = InputReader.SimulatedMove = Vector2.zero;
            // A checkpoint puts the guard back on his round; his steps near her room would rouse her with Kiều at the
            // bedside (her other, legitimate catch), which is not what these tests are about.
            foreach (var w in _watchers) { w.EndScript(false); w.Forget(); w.Perceiving = false; w.Pause(true); }
            if (!_sleeper.Breathing) _sleeper.Restore(TuBaSleeper.Sleep.Asleep);
        }

        /// <summary>Beside the sập, on the side the keys hang, about 0.65 m from them.</summary>
        private Vector3 KeyStand()
        {
            Vector3 keys = _keyTheft.transform.position, feet = _tuBa.transform.position, head = _sleeper.Head;
            Vector3 axis = Flat(head - feet).normalized;
            Vector3 along = feet + axis * Vector3.Dot(Flat(keys - feet), axis);
            Vector3 outward = Flat(keys - along).normalized;
            Vector3 stand = keys + outward * 0.65f;
            stand.y = LauTuBaFloor + 0.05f;
            return stand;
        }

        private IEnumerator AimAtKeys(Vector3 stand, bool crouch, bool keepE = false) => AimAt(stand, _keyTheft.transform.position, crouch, keepE);

        /// <summary>Kiều at <paramref name="stand"/> (crouched or not), her eyes on <paramref name="point"/>, settled for a few frames.</summary>
        private IEnumerator AimAt(Vector3 stand, Vector3 point, bool crouch, bool keepE = false)
        {
            var player = _director.Player;
            player.ScriptedMove = null;
            player.ForceCrouch = crouch;
            if (Flat(player.transform.position - stand).magnitude > 0.05f) player.Teleport(stand, Quaternion.LookRotation(Flat(point - stand)));
            RestrictedZone.ResetPresence();
            // Long enough for the crouch to settle the eyes at their real height (the view lowers over a few tenths of a second).
            for (float t = 0f, real = Time.realtimeSinceStartup; t < 0.7f; t += Time.deltaTime)
            {
                if (TimeStopped(real)) yield break;
                if (keepE) InputReader.SimulatedInteract = true;
                _director.CameraController.LookAtNow(point);
                yield return null;
            }
        }

        /// <summary>The ring's root on her hip (it carries the hook; the keys hang from it).</summary>
        private Transform RingRoot() => _keyTheft.Ring.transform.parent;

        private Bounds RingBounds()
        {
            var renderers = RingRoot().GetComponentsInChildren<Renderer>().Where(r => !(r is LineRenderer)).ToArray();
            var bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            return bounds;
        }

        /// <summary>Standing: the hook just outside her hip, the keys hanging down beside her thigh.</summary>
        private bool RingAtHip(out string detail)
        {
            var root = RingRoot();
            var hips = _tuBa.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.Hips);
            Vector3 hook = root.position;
            float side = Flat(hook - hips.position).magnitude;
            float drop = hips.position.y - hook.y;
            var bounds = RingBounds();
            float down = Vector3.Angle(root.GetComponent<HangingKeys>().Down, Vector3.down);
            detail = $"hook {side * 100f:F0} cm out from her hip bone, {drop * 100f:F0} cm below it · keys hang {down:F0}° from straight down · reach {bounds.min.y - _tuBa.transform.position.y:F2} m above her feet · size {bounds.size.y * 100f:F0} cm";
            return side > 0.1f && side < 0.3f && Mathf.Abs(drop) < 0.15f && down < 30f;
        }

        /// <summary>Lying: on her room-side hip, resting on (not sunk in, not floating over) the mat.</summary>
        private bool RingLying(out string detail)
        {
            var bounds = RingBounds();
            Vector3 grip = RingRoot().Find("Grip").position;
            float mat = -1f;
            foreach (var hit in Physics.RaycastAll(grip + Vector3.up * 0.5f, Vector3.down, 1.5f, ~0, QueryTriggerInteraction.Ignore).OrderBy(h => h.distance))
                if (!hit.collider.transform.IsChildOf(_tuBa.transform)) { mat = hit.point.y; break; }
            Vector3 feet = _tuBa.transform.position, head = _sleeper.Head;
            Vector3 axis = Flat(head - feet).normalized;
            Vector3 along = feet + axis * Vector3.Dot(Flat(grip - feet), axis);
            float outFromMiddle = Flat(grip - along).magnitude;
            float towardRoom = Vector3.Dot(Flat(grip - along).normalized, Flat(_tuBaDoorInside.position - along).normalized);
            detail = $"lowest point {(bounds.min.y - mat) * 100f:F1} cm above the mat · keys {outFromMiddle * 100f:F0} cm out from her middle line · toward the room {towardRoom:F2}";
            return mat > 0f && bounds.min.y - mat > -0.012f && bounds.min.y - mat < 0.08f && outFromMiddle > 0.15f && outFromMiddle < 0.45f && towardRoom > 0f;
        }

        /// <summary>
        /// QA stills of the ring (only with -autopilotCapture): close up lying, then Tú Bà standing beside her sập with the
        /// ring at her hip, from the side and from the front; then back to bed.
        /// </summary>
        private IEnumerator KeyStills(Vector3 stand)
        {
            Vector3 keys = _keyTheft.transform.position;
            Vector3 near = keys + Flat(stand - keys).normalized * 0.38f;
            near.y = LauTuBaFloor + 0.05f;
            yield return AimAt(near, keys, true);
            StoryCapture.Shot("KEYS_lying_close");
            Vector3 up = _sapFeet.position + Flat(stand - keys).normalized * 0.9f;
            _tuBa.StandUp(up, Quaternion.LookRotation(Flat(stand - keys)));
            BodyLying(false);
            for (int i = 0; i < 30; i++) yield return null;
            var hips = _tuBa.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.Hips);
            Vector3 side = _tuBa.transform.position - _tuBa.transform.right * 1.1f;
            side.y = LauTuBaFloor + 0.05f;
            yield return AimAt(side, hips.position, false);
            StoryCapture.Shot("KEYS_standing_side");
            Vector3 front = _tuBa.transform.position + _tuBa.transform.forward * 1.3f;
            front.y = LauTuBaFloor + 0.05f;
            yield return AimAt(front, hips.position, false);
            StoryCapture.Shot("KEYS_standing_front");
            PutTuBaToBed(false);
            for (int i = 0; i < 30; i++) yield return null;
            yield return AimAtKeys(stand, true);
        }

        private static string Describe(IInteractable target) => target is MonoBehaviour b && b != null ? b.name : "none";
    }
}
