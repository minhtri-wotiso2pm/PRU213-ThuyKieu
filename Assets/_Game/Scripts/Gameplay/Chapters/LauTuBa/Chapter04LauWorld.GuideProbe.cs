using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.House;
using ThuyKieu.Gameplay.Stealth;
using ThuyKieu.Presentation;
using UnityEngine;

namespace ThuyKieu.Gameplay.Chapters.LauTuBa
{
    public partial class Chapter04LauWorld
    {
        private bool _guideProbed, _guideJ1Checked, _guideAfterContract;
        private int _guideCues;
        private float _guideSensitivity, _guideGateSensitivity;

        /// <summary>
        /// "-autopilotGuide": the objective guidance through the real steps of the night. This part runs up to the office:
        /// nothing about later steps at the start; the office cues only once the keys are taken through the minigame and
        /// the close view has handed the camera back; the reminder after half a minute without headway; the cabinet's cue
        /// inside the office; checkpoints before and after the keys restored to their own cues. Then the autopilot plays on
        /// (its own choices through the cabinet) while <see cref="GuideWatchContract"/> watches the contract and J1, and
        /// <see cref="GuideAfterContract"/> restores the checkpoint after it. A broken rule is logged as an error.
        /// </summary>
        private IEnumerator GuideProbe()
        {
            var g = _guidance;
            const float F = 0.35f;
            if (g == null) { GuideReport("installed", false, "no Chapter04Guidance on the chapter"); yield break; }
            _guideCues = FindObjectsByType<GuideGlow>(FindObjectsInactive.Include).Length;

            // 1. The start of the night: her room, nothing about the keys, the contract or the kitchen.
            yield return GuideSettle(1.5f);
            string first = _director.Hud.Objective;
            bool future = new[] { "chìa", "giao kèo", "sổ sách", "bếp", "cửa sau" }.Any(w => first.ToLowerInvariant().Contains(w));
            GuideReport("start: only the room", g.Current == Chapter04Guidance.Step.Room && !future && GuideMax(g.All) < 0.01f, $"step {g.Current} · \"{first}\" · {GuideLevels()}");

            // 2. Out of the room: Tú Bà's room and her keys, still nothing lit.
            if (!_inventory.Has("tram")) _hairpin.Interact();
            AutoTeleport(_kieuDoorOutside.position + Vector3.left * 1.9f);
            _kieuDoor.SetLock(DoorLock.None);
            OnKieuDoor(_kieuDoor);
            _kieuDoor.SetOpen(1f);
            yield return GuideSettle(1.5f);
            string keysLine = _director.Hud.Objective;
            GuideReport("out of the room: the keys", g.Current == Chapter04Guidance.Step.Keys && keysLine.Contains("Tú Bà") && !keysLine.Contains("giao kèo") && GuideMax(g.All) < 0.01f,
                $"\"{keysLine}\" · {GuideLevels()}");
            // The checkpoint in the dark store, once she is asleep on her sập (as a player reaches it).
            yield return GuideUntil(() => _tuBa.IsLying, 30f, "Tú Bà in bed");
            AutoTeleport(_storeSlats.position);
            yield return GuideSettle(0.5f);
            yield return GuideRestore();
            GuideReport("checkpoint before the keys", g.Current == Chapter04Guidance.Step.Keys && GuideMax(g.All) < 0.01f && _director.Hud.Objective.Contains("Tú Bà"),
                $"step {g.Current} · \"{_director.Hud.Objective}\" · {GuideLevels()}");
            GuideDuplicates("(before the keys)");

            // 3. The keys through the real minigame. While the close view holds the camera nothing changes; once it has
            // handed the camera back, the office objective and its cues.
            AutoTeleport(_storeSlats.position);
            yield return new WaitForSeconds(0.4f);
            float earlyCue = 0f;
            bool earlyLine = false, keysWatch = true;
            IEnumerator WatchEarly()
            {
                while (keysWatch)
                {
                    if (_keyTheft.InMinigame || _director.CameraController.CloseWeight > 0.01f || !_inventory.Has("chia"))
                    {
                        earlyCue = Mathf.Max(earlyCue, GuideMax(g.OfficeCues));
                        earlyLine |= _director.Hud.Objective == Chapter04Guidance.OfficeObjective;
                    }
                    yield return null;
                }
            }
            var watch = StartCoroutine(WatchEarly());
            yield return AutoTakeKeys();
            yield return GuideUntil(() => !_keyTheft.InMinigame && _director.CameraController.CloseWeight <= 0.01f, 10f, "the close view to end");
            keysWatch = false;
            StopCoroutine(watch);
            GuideReport("nothing during the close view", earlyCue < 0.01f && !earlyLine, $"office cue at most {earlyCue:F2} · line early {earlyLine}");
            float back = Time.time;
            yield return GuideUntil(() => GuideMax(g.OfficeCues) >= 0.5f, 6f, "the office cues");
            GuideReport("keys taken: the office", g.Current == Chapter04Guidance.Step.Office && _director.Hud.Objective == Chapter04Guidance.OfficeObjective
                && GuideMax(g.OfficeCues) >= 0.5f && GuideMax(g.CabinetCues) < 0.01f && GuideMax(g.KitchenCues) < 0.01f,
                $"{Time.time - back:F1} s after the camera came back · office lamp lit {_officeLamp.Lit} · {GuideLevels()}");
            yield return GuideUntil(() => _director.IsExploreAvailable("c4_cabinet"), 5f, "the cabinet to be offered");
            GuideReport("the cabinet can be opened with the keys", _director.IsExploreAvailable("c4_cabinet"),
                "explore options: " + string.Join(", ", _director.ExploreOptions.Select(o => o.Id)));

            // 4. The way from where the keys hang, and from the dark store next door (the checkpoint after them): which
            // cues an eye there can see (walls and doors block; the store's slats do not).
            string Seen(Vector3 eye)
            {
                var seen = g.OfficeCues.Where(x => x.Shown > 0.3f && !Physics.Linecast(eye, x.transform.position, 1 << 8, QueryTriggerInteraction.Ignore)).Select(x => x.name).ToArray();
                return seen.Length + " (" + string.Join(", ", seen) + ")";
            }
            string fromKeys = Seen(_director.CameraController.transform.position);
            string fromStore = Seen(_spawnStore.position + Vector3.up * 1.56f);
            GuideReport("the way can be seen from the keys", !fromKeys.StartsWith("0"), "from the sập " + fromKeys + " · from the store " + fromStore);

            // 5. The guards' eyes: light readings and sight lines at control points, every cue lit, then every cue dark.
            var points = new[]
            {
                new Vector3(13.6f, F + 1.05f, 10.2f), new Vector3(7.4f, F + 1.05f, 15.6f), new Vector3(19.0f, F + 1.05f, 15.2f),
                new Vector3(-19.5f, 1.17f, 14.6f), new Vector3(-18.3f, F + 1.05f, 10.2f), new Vector3(6.0f, F + 1.05f, 10.0f), new Vector3(0f, F + 1.05f, 13.5f)
            };
            var eyes = new[] { new Vector3(2.6f, F + 1.62f, 9.6f), new Vector3(10.5f, F + 1.62f, 9.9f), new Vector3(-15.5f, F + 1.62f, 9.6f) };
            string Reading()
            {
                var sb = new System.Text.StringBuilder();
                LightExposure.Enable(true);
                foreach (var p in points)
                {
                    sb.Append(LightExposure.At(p).ToString("F4")).Append(' ');
                    foreach (var e in eyes) sb.Append(Physics.Linecast(e, p, ~0, QueryTriggerInteraction.Ignore) ? '1' : '0');
                    sb.Append(' ');
                }
                return sb.ToString().Trim();
            }
            var glows = FindObjectsByType<GuideGlow>(FindObjectsInactive.Include);
            var saved = glows.Select(x => x.Target).ToArray();
            foreach (var x in glows) { x.Target = 1f; x.Snap(); }
            Physics.SyncTransforms();
            string lit = Reading();
            foreach (var x in glows) { x.Target = 0f; x.Snap(); }
            Physics.SyncTransforms();
            string dark = Reading();
            for (int i = 0; i < glows.Length; i++) { glows[i].Target = saved[i]; glows[i].Snap(); }
            bool bare = glows.All(x => x.GetComponentInChildren<Light>(true) == null && x.GetComponentInChildren<Collider>(true) == null);
            GuideReport("guards' light and sight the same lit and dark", lit == dark && bare, $"no light or collider on a cue {bare} · {lit}" + (lit == dark ? "" : " ≠ " + dark));
            _guideSensitivity = _guard.Sensitivity;
            _guideGateSensitivity = _gateGuard.Sensitivity;

            // 6. No headway for half a minute (standing still, free, unhidden, no danger): the objective comes back once.
            int before = g.Reminders;
            for (float t = 0f; t < 34f; t += Time.deltaTime) yield return null;
            int once = g.Reminders - before;
            for (float t = 0f; t < 20f; t += Time.deltaTime) yield return null;
            GuideReport("reminder after no headway", once == 1 && g.Reminders - before == 1, $"after 34 s {once} · after 54 s {g.Reminders - before}");

            // 7. The checkpoint after the keys (the dark store): the office again, straight away, and the cabinet offered.
            yield return GuideRestore();
            GuideReport("checkpoint after the keys", g.Current == Chapter04Guidance.Step.Office && _director.Hud.Objective == Chapter04Guidance.OfficeObjective
                && GuideMax(g.OfficeCues) >= 0.5f && GuideMax(g.CabinetCues) < 0.01f, $"step {g.Current} · {GuideLevels()}");
            yield return GuideUntil(() => _director.IsExploreAvailable("c4_cabinet"), 5f, "the cabinet to be offered after the restore");
            GuideDuplicates("(after the keys)");

            // 8. Into the office: the door cues give way to the cabinet's lock.
            _innerDoor.SetLock(DoorLock.None);
            _innerDoor.SetOpen(1f);
            AutoTeleport(_officeInside.position);
            yield return GuideSettle(3.5f);
            GuideReport("in the office: the cabinet", g.Current == Chapter04Guidance.Step.Cabinet && GuideMax(g.OfficeCues) < 0.05f && GuideMax(g.CabinetCues) >= 0.5f, GuideLevels());

            // The autopilot now takes the contract with its own choices; the watcher follows it through J1.
            StartCoroutine(GuideWatchContract());
        }

        /// <summary>The contract: everything for the office goes; the kitchen waits while J1 plays and returns after it.</summary>
        private IEnumerator GuideWatchContract()
        {
            var g = _guidance;
            yield return GuideUntil(() => g.Current == Chapter04Guidance.Step.Kitchen && _director.InExplore, 120f, "the contract");
            yield return GuideSettle(2.5f);
            GuideReport("contract: the office cues gone", g.Current == Chapter04Guidance.Step.Kitchen && _director.Hud.Objective == Chapter04Guidance.KitchenObjective
                && GuideMax(g.OfficeCues) < 0.02f && GuideMax(g.CabinetCues) < 0.02f, $"J1 running {_j1Running} · {GuideLevels()}");
            float kitchenInJ1 = 0f, warnedAt = -1f;
            string warned = null;
            int reminders = g.Reminders;
            for (float wait = 120f; !_j1Done && (wait -= Time.deltaTime) > 0f;)
            {
                kitchenInJ1 = Mathf.Max(kitchenInJ1, GuideMax(g.KitchenCues));
                if (warned == null && !string.IsNullOrEmpty(_director.Hud.Situation))
                {
                    warned = _director.Hud.Situation;
                    warnedAt = Vector3.Distance(_guard.transform.position, _officeDoorOutside.position);
                }
                yield return null;
            }
            // A warning, if any, only once he stands at the door (the rattle), never while he is still on his way.
            bool heard = warned == null || warnedAt < 2.5f;
            GuideReport("J1: no kitchen cue, no reminder, warned only once heard", _j1Done && kitchenInJ1 < 0.01f && g.Reminders == reminders && heard,
                $"J1 done {_j1Done} · kitchen at most {kitchenInJ1:F2} · reminders {g.Reminders - reminders} · warning \"{warned ?? "none"}\" with the guard {warnedAt:F1} m from the door");
            yield return GuideSettle(4f);
            GuideReport("after J1: the kitchen", GuideMax(g.KitchenCues) >= 0.5f && GuideMax(g.OfficeCues) < 0.01f,
                $"danger {InDanger} · situation \"{_director.Hud.Situation}\" · {GuideLevels()}");
            _guideJ1Checked = true;
        }

        /// <summary>The checkpoint after the contract: the kitchen, and nothing of the office left; no cue doubled.</summary>
        private IEnumerator GuideAfterContract()
        {
            var g = _guidance;
            yield return GuideUntil(() => _guideJ1Checked, 30f, "the J1 checks");
            yield return GuideRestore();
            GuideReport("checkpoint after the contract", g.Current == Chapter04Guidance.Step.Kitchen && GuideMax(g.OfficeCues) < 0.01f && GuideMax(g.CabinetCues) < 0.01f
                && GuideMax(g.KitchenCues) >= 0.5f, $"stage {_stage} · {GuideLevels()}");
            GuideDuplicates("(after the contract)");
            GuideReport("guards' sensitivity untouched", Mathf.Approximately(_guideSensitivity, _guard.Sensitivity) && Mathf.Approximately(_guideGateSensitivity, _gateGuard.Sensitivity),
                $"guard {_guard.Sensitivity:F2} · gate {_gateGuard.Sensitivity:F2}");
            // Brightness is post exposure: it lifts the frame by b and the cue by b / √b, so the cue against the room is 1 / √b.
            StoryAutopilot.Note("GUIDE info brightness 50/100/200 %: cue against the room " + string.Join(" / ", new[] { 0.5f, 1f, 2f }.Select(b => (1f / Mathf.Sqrt(b)).ToString("F2"))));
        }

        // ------------------------------------------------------------------ probe helpers

        private void GuideReport(string test, bool ok, string detail)
        {
            StoryAutopilot.Note("GUIDE " + (ok ? "ok   " : "FAIL ") + test + " · " + detail);
            Debug.Log("[GuideProbe] " + (ok ? "ok " : "FAIL ") + test);
            if (!ok) StoryAutopilot.Problem("[GuideProbe] " + test + " · " + detail);
        }

        private static float GuideMax(IEnumerable<GuideGlow> glows) => glows == null ? 0f : glows.Select(x => x.Shown).DefaultIfEmpty(0f).Max();

        private string GuideLevels() => $"office {GuideMax(_guidance.OfficeCues):F2} · cabinet {GuideMax(_guidance.CabinetCues):F2} · kitchen {GuideMax(_guidance.KitchenCues):F2}";

        /// <summary>Every wait gives up after a while: a probe that hangs tells nothing.</summary>
        private IEnumerator GuideUntil(System.Func<bool> done, float seconds, string what)
        {
            float t = 0f;
            while (!done() && (t += Time.deltaTime) < seconds) yield return null;
            if (!done()) GuideReport("waited for " + what, false, $"still not after {seconds:F0} s · explore {_director.InExplore} · busy {_director.IsBusy} · stage {_stage}");
        }

        private IEnumerator GuideSettle(float seconds)
        {
            for (float t = 0f; t < seconds; t += Time.deltaTime) yield return null;
            yield return GuideUntil(() => _director.InExplore, 30f, "exploring");
        }

        private IEnumerator GuideRestore()
        {
            yield return GuideUntil(() => _director.InExplore && !_director.IsBusy, 30f, "exploring before the restore");
            _director.Fail("QA", "Guide probe: checkpoint restore");
            yield return GuideUntil(() => _director.IsBusy, 3f, "the failure to start");
            yield return GuideUntil(() => !_director.IsBusy && _director.InExplore, 30f, "the restore");
            yield return GuideSettle(0.6f);
        }

        private void GuideDuplicates(string when)
        {
            int now = FindObjectsByType<GuideGlow>(FindObjectsInactive.Include).Length;
            GuideReport("no cue duplicated " + when, now == _guideCues, $"{now} cues (at the start {_guideCues})");
        }
    }
}
