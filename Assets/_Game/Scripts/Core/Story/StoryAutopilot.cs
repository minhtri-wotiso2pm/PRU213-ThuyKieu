using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ThuyKieu.Dialogue;
using ThuyKieu.Player;
using ThuyKieu.Interaction;
using ThuyKieu.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThuyKieu.Core.Story
{
    /// <summary>
    /// QA bot. Started with the command-line flag "-storyAutopilot", it plays chapters end to end with
    /// random choices and random exploration order, records every beat and any logged error,
    /// then writes a report ("-autopilotReport &lt;path&gt;", default persistentDataPath).
    /// "-autopilotChain" continues into the next chapter scene instead of stopping.
    /// </summary>
    public class StoryAutopilot : MonoBehaviour
    {
        public const string Flag = "-storyAutopilot";
        private static readonly List<string> Log = new List<string>();
        private static readonly List<string> Errors = new List<string>();
        private static bool _hooked;
        private static float _startTime;

        [SerializeField] private StoryDirector _director;

        public static bool Enabled => System.Environment.GetCommandLineArgs().Contains(Flag);
        public static string ReportPath
        {
            get
            {
                var args = System.Environment.GetCommandLineArgs();
                int index = Array.IndexOf(args, "-autopilotReport");
                return index >= 0 && index + 1 < args.Length ? args[index + 1] : Path.Combine(Application.persistentDataPath, "AutopilotReport.txt");
            }
        }

        /// <summary>Player builds: "-autopilotScene &lt;name&gt;" jumps straight to a chapter for automated runs.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (!Enabled) return;
            var args = System.Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-autopilotScene");
            if (index < 0 || index + 1 >= args.Length) return;
            string scene = args[index + 1];
            if (SceneManager.GetActiveScene().name != scene && Application.CanStreamedLevelBeLoaded(scene)) SceneManager.LoadScene(scene);
        }

        private void Start()
        {
            if (!Enabled) { enabled = false; return; }
            if (!_hooked)
            {
                _hooked = true;
                _startTime = Time.realtimeSinceStartup;
                Application.logMessageReceived += (message, stack, type) =>
                {
                    if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                        Errors.Add(type + ": " + message + " @ " + stack.Split('\n').FirstOrDefault());
                };
                var args = System.Environment.GetCommandLineArgs();
                int seedIndex = Array.IndexOf(args, "-autopilotSeed");
                UnityEngine.Random.InitState(seedIndex >= 0 && seedIndex + 1 < args.Length ? int.Parse(args[seedIndex + 1]) : 213);
            }
            _director.Autopilot = true;
            _director.CheckpointRestored += () => Log.Add($"[{Elapsed()}] FAIL → checkpoint restored");
            Time.timeScale = StoryCapture.Directory != null ? 1f : 3f;
            foreach (var view in FindObjectsByType<StoryHUD>()) view.FastMode = true;
            foreach (var view in FindObjectsByType<DocumentView>()) view.FastMode = true;
            foreach (var view in FindObjectsByType<DeductionView>()) view.FastMode = true;
            Log.Add($"[{Elapsed()}] SCENE {SceneManager.GetActiveScene().name}");
            StartCoroutine(Drive());
        }

        private IEnumerator Drive()
        {
            var dialogue = _director.Dialogue;
            bool capture = StoryCapture.Directory != null;
            string prefix = "C" + _director.ChapterNumber + "_";
            var styles = new HashSet<SpeakerStyle>();
            int choiceShots = 0, exploreShots = 0;
            bool failShot = false;
            string lastSequence = null;
            int lines = 0, choices = 0;
            float sceneStart = Time.realtimeSinceStartup;
            if (capture) { yield return new WaitForSecondsRealtime(0.7f); StoryCapture.Shot(prefix + "00_card"); }
            while (!_director.ChapterComplete)
            {
                if (Time.realtimeSinceStartup - sceneStart > 420f) { Finish("TIMEOUT in " + SceneManager.GetActiveScene().name); yield break; }
                if (_director.CurrentSequence != lastSequence)
                {
                    lastSequence = _director.CurrentSequence;
                    if (lastSequence != null)
                    {
                        Log.Add($"[{Elapsed()}] SEQUENCE {lastSequence}");
                        if (capture) { yield return new WaitForSecondsRealtime(0.9f); StoryCapture.Shot(prefix + "seq_" + lastSequence); }
                    }
                }
                if (_director.IsBusy && _director.CurrentSequence == null && capture && !failShot)
                {
                    failShot = true;
                    yield return new WaitForSecondsRealtime(0.4f);
                    StoryCapture.Shot(prefix + "fail");
                }
                if (_director.Hud.IsTutorialVisible || (_director.IsBusy && _director.CurrentSequence == null)) { yield return null; continue; }
                if (dialogue.IsDialogueActive)
                {
                    if (dialogue.IsAtChoicePoint)
                    {
                        if (capture && choiceShots < 3) { yield return new WaitForSecondsRealtime(1.6f); StoryCapture.Shot(prefix + "choice_" + ++choiceShots); }
                        int count = dialogue.InkStory.currentChoices.Count;
                        int pick = UnityEngine.Random.Range(0, count);
                        Log.Add($"[{Elapsed()}] CHOICE {pick + 1}/{count}: {dialogue.InkStory.currentChoices[pick].text}");
                        dialogue.SelectChoice(pick);
                        choices++;
                    }
                    else
                    {
                        if (capture && styles.Add(SpeakerCatalog.Get(dialogue.CurrentSpeakerId).Style))
                        {
                            yield return new WaitForSecondsRealtime(1.8f);
                            StoryCapture.Shot(prefix + "line_" + SpeakerCatalog.Get(dialogue.CurrentSpeakerId).Style);
                        }
                        dialogue.Advance();
                        lines++;
                    }
                    yield return new WaitForSecondsRealtime(capture ? 0.05f : 0.02f);
                    continue;
                }
                if (_director.InExplore)
                {
                    if (capture && exploreShots < 2) { yield return new WaitForSecondsRealtime(1.5f); StoryCapture.Shot(prefix + "explore_" + ++exploreShots); }
                    if (capture && exploreShots == 2 && _director.Clues.Count > 0 && !_menusShot) yield return CaptureMenus(prefix);
                    yield return Explore(capture);
                }
                yield return null;
            }
            if (capture) { yield return new WaitForSecondsRealtime(1.2f); StoryCapture.Shot(prefix + "zz_complete"); }
            Log.Add($"[{Elapsed()}] CHAPTER {_director.ChapterNumber} COMPLETE · lines {lines} · choices {choices}");
            foreach (string variable in new[] { "nghi_ngo", "manh_moi", "suy_luan", "gia_vo", "c3_viec", "c3_khe_uoc", "c3_bi_bat", "tra_loi_kieu_that" })
                Log.Add("   " + variable + " = " + _director.Story.variablesState[variable]);
            Log.Add("   clues = " + string.Join(", ", _director.Clues));

            bool chain = System.Environment.GetCommandLineArgs().Contains("-autopilotChain");
            string next = _director.ChapterNumber == 2 ? "Chapter03_TuBaHouse" : null;
            if (chain && next != null && Application.CanStreamedLevelBeLoaded(next))
            {
                yield return new WaitForSecondsRealtime(0.3f);
                SceneManager.LoadScene(next);
                yield break;
            }
            Finish("PASS");
        }

        private bool _menusShot;

        private IEnumerator CaptureMenus(string prefix)
        {
            _menusShot = true;
            var journal = FindAnyObjectByType<JournalView>();
            if (journal != null && _director.InExplore)
            {
                journal.Open();
                yield return new WaitForSecondsRealtime(0.6f);
                StoryCapture.Shot(prefix + "journal");
                journal.Close();
            }
            var pause = FindAnyObjectByType<PauseMenuView>();
            if (pause == null) yield break;
            pause.Pause();
            yield return new WaitForSecondsRealtime(0.5f);
            StoryCapture.Shot(prefix + "pause");
            pause.Resume();
        }

        private static readonly string[] HubEnders = { "c2_eavesdrop", "c3_makieu" };
        private bool _failTested;

        private IEnumerator Explore(bool capture)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            if (!_director.InExplore) yield break;
            var args = System.Environment.GetCommandLineArgs();
            bool thorough = args.Contains("-autopilotThorough");
            if (thorough && !_failTested)
            {
                // Exercise the failure → checkpoint path once per chapter.
                _failTested = true;
                Log.Add($"[{Elapsed()}] TEST forced failure");
                if (_director.ChapterNumber == 2) _director.SetInt("nghi_ngo", 100);
                else _director.Fail("Kiểm thử", "Autopilot kiểm tra khôi phục checkpoint.");
                yield return new WaitForSecondsRealtime(1f);
                yield break;
            }
            var options = _director.ExploreOptions.ToList();
            var targets = FindObjectsByType<ExploreTarget>();
            var observables = FindObjectsByType<ObservableTarget>();
            var ready = options.Where(o => targets.Any(t => t.ExploreId == o.Id && t.IsRevealed) || observables.Any(t => t.ExploreId == o.Id)).ToList();
            if (thorough)
            {
                // Leave the hub-ending option until everything else has been seen.
                bool pending = options.Any(o => !HubEnders.Contains(o.Id));
                if (pending) ready = ready.Where(o => !HubEnders.Contains(o.Id)).ToList();
            }
            if (ready.Count == 0)
            {
                // Keep up with the escort so the world can progress (the note only drops on the road ahead).
                var ma = _director.FindActor("MaGiamSinh");
                if (ma != null && _director.ChapterNumber == 2)
                {
                    Vector3 behind = ma.transform.position - ma.transform.forward * 2.5f;
                    behind.y = _director.Player.transform.position.y;
                    _director.Player.Teleport(behind, ma.transform.rotation);
                }
                yield return new WaitForSecondsRealtime(0.5f);
                yield break;
            }
            var option = ready[UnityEngine.Random.Range(0, ready.Count)];
            Log.Add($"[{Elapsed()}] EXPLORE {option.Id}");
            var target = targets.FirstOrDefault(t => t.ExploreId == option.Id && t.IsRevealed);
            Transform anchor = target != null ? target.transform : observables.First(t => t.ExploreId == option.Id).transform;
            Vector3 toPlayer = _director.Player.transform.position - anchor.position;
            toPlayer.y = 0;
            Vector3 stand = anchor.position + (toPlayer.sqrMagnitude > 0.01f ? toPlayer.normalized : Vector3.back) * 1.3f;
            stand.y = _director.Player.transform.position.y;
            _director.Player.Teleport(stand, Quaternion.LookRotation(anchor.position - stand == Vector3.zero ? Vector3.forward : new Vector3(anchor.position.x - stand.x, 0, anchor.position.z - stand.z)));
            ThuyKieu.Gameplay.Stealth.RestrictedZone.ResetPresence();
            yield return new WaitForSecondsRealtime(0.1f);
            if (capture && target == null)
            {
                // Show observation mode on screen before completing it.
                _director.CameraController.SnapBehindTarget(_director.Player.transform.eulerAngles.y, 8f);
                var observe = FindAnyObjectByType<ThuyKieu.Player.ObserveMode>();
                if (observe != null) observe.ForceObserve = true;
                yield return new WaitForSecondsRealtime(1.4f);
                StoryCapture.Shot("C" + _director.ChapterNumber + "_observe");
                if (observe != null) observe.ForceObserve = false;
            }
            if (target != null) target.Interact();
            else observables.First(t => t.ExploreId == option.Id).Complete();
        }

        private static string Elapsed() => (Time.realtimeSinceStartup - _startTime).ToString("0.0") + "s";

        private void Finish(string result)
        {
            var report = new StringBuilder();
            bool pass = result == "PASS" && Errors.Count == 0;
            report.AppendLine(pass ? "AUTOPILOT PASS" : "AUTOPILOT FAIL: " + result + (Errors.Count > 0 ? " · " + Errors.Count + " errors" : ""));
            foreach (string line in Log) report.AppendLine(line);
            foreach (string error in Errors.Distinct().Take(40)) report.AppendLine("ERROR " + error);
            string path = ReportPath;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, report.ToString(), new UTF8Encoding(false));
            Debug.Log("[Autopilot] Report written to " + path);
            Time.timeScale = 1f;
            if (!Application.isEditor) Application.Quit(pass ? 0 : 1);
        }
    }
}
