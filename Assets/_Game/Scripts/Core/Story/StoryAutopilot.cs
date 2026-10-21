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
        private readonly List<float> _frames = new List<float>();
        private int _explores;

        /// <summary>"-autopilotMidSave": let the director write its mid-chapter save even under the autopilot.</summary>
        public static bool MidSaveTest => Enabled && System.Environment.GetCommandLineArgs().Contains("-autopilotMidSave");

        /// <summary>"-autopilotStopAtExplore N": end the run when the Nth explore checkpoint is reached.</summary>
        private static int StopAtExplore
        {
            get
            {
                var args = System.Environment.GetCommandLineArgs();
                int index = Array.IndexOf(args, "-autopilotStopAtExplore");
                return index >= 0 && index + 1 < args.Length && int.TryParse(args[index + 1], out int n) ? n : 0;
            }
        }
        private float _sceneStart, _warmupWorst, _firstFrame;
        private int _warmupFrames;

        /// <summary>Frame times of this scene, skipping the first two seconds of loading and shader warm-up.</summary>
        private void Update()
        {
            if (_sceneStart <= 0f) _sceneStart = Time.realtimeSinceStartup;
            if (Time.realtimeSinceStartup - _sceneStart <= 2f)
            {
                // The first frame's delta spans the start-up and the scene load: it is reported as such, not as a hitch.
                if (_warmupFrames++ == 0) _firstFrame = Time.unscaledDeltaTime;
                else _warmupWorst = Mathf.Max(_warmupWorst, Time.unscaledDeltaTime);
                return;
            }
            _frames.Add(Time.unscaledDeltaTime);
            // A single slow frame is logged in order with the other notes, so the line before it names the cause.
            if (Time.unscaledDeltaTime > 0.15f) Note($"HITCH {Time.unscaledDeltaTime * 1000f:F0} ms");
        }

        /// <summary>
        /// Average fps, the 1% low (the slowest 1% of frames), the worst frame, median and 99th-percentile frame times and
        /// the share of frames slower than 60 fps, all after the first two seconds; then the scene's load (time since the
        /// process started when its first frame ran) and the worst frame of those first two seconds, kept apart.
        /// </summary>
        private string FrameStats()
        {
            if (_frames.Count == 0) return "no frames";
            var sorted = _frames.OrderByDescending(f => f).ToList();
            float average = _frames.Average();
            float low = sorted.Take(Mathf.Max(1, sorted.Count / 100)).Average();
            float median = sorted[sorted.Count / 2], p99 = sorted[Mathf.Min(sorted.Count - 1, sorted.Count / 100)];
            float slow = 100f * _frames.Count(f => f > 1f / 60f) / _frames.Count;
            return $"avg {1f / average:F0} fps · 1% low {1f / low:F0} fps · worst {sorted[0] * 1000f:F0} ms · {_frames.Count} frames · {Screen.width}x{Screen.height}"
                + $" · median {median * 1000f:F1} ms · p99 {p99 * 1000f:F1} ms · under 60 fps {slow:F1}% · load {_sceneStart:F1} s (first frame {_firstFrame * 1000f:F0} ms) · warm-up worst {_warmupWorst * 1000f:F0} ms";
        }

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

        /// <summary>
        /// "-autopilotEnding &lt;nghich_menh|thang|mat_minh|dinh_menh&gt;": steer the courtroom (bond, trust, evidence and how
        /// the trial and lục bát are played) so QA can reach each ending on purpose. Null when not given.
        /// </summary>
        public static string EndingTarget
        {
            get
            {
                var args = System.Environment.GetCommandLineArgs();
                int index = Array.IndexOf(args, "-autopilotEnding");
                return Enabled && index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
            }
        }

        /// <summary>Sets the story up for the requested ending when Chapter 7 starts.</summary>
        private void SteerEnding()
        {
            string target = EndingTarget;
            var d = _director;
            // The fate run also meets Sở Khanh in Chapter 5 (fate ≥ 50).
            if (target == "dinh_menh" && d.ChapterNumber == 5 && d.GetInt("dinh_menh") < 60) { d.SetInt("dinh_menh", 60); Log.Add($"[{Elapsed()}] STEER fate → 60"); }
            if (target == null || d.ChapterNumber != 7) return;
            switch (target)
            {
                case "nghich_menh":
                    d.SetInt("dong_dieu", 9); d.SetInt("kt_tin_tuong", 6); d.SetInt("c6_ho_so", 4); d.SetInt("ev_nhan_chung", 2);
                    d.SetBool("nc_ba_nuoc", true); d.SetBool("nc_thu_lai", true); d.SetBool("ev_giay_no", true); d.SetBool("c6_so_bac", true);
                    d.SetBool("ev_tien", true); d.SetInt("dinh_menh", 20);
                    break;
                case "thang": d.SetInt("dong_dieu", 5); d.SetInt("kt_tin_tuong", 4); break;
                case "mat_minh": d.SetInt("dong_dieu", 2); d.SetInt("kt_tin_tuong", 4); break;
                case "dinh_menh": d.SetInt("dong_dieu", 5); d.SetInt("kt_tin_tuong", 0); d.SetInt("dinh_menh", 90); break;
            }
            Log.Add($"[{Elapsed()}] STEER ending → {target}");
        }

        /// <summary>Player builds: "-autopilotScene &lt;name&gt;" jumps straight to a chapter for automated runs.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (!Enabled) return;
            var args = System.Environment.GetCommandLineArgs();
            // "-autopilotResume": start the chapter from its mid-chapter save, as "Continue" does.
            if (args.Contains("-autopilotResume")) InkStatePersistence.Ensure().ResumeMid = true;
            int index = Array.IndexOf(args, "-autopilotScene");
            if (index < 0 || index + 1 >= args.Length) return;
            string scene = args[index + 1];
            if (SceneManager.GetActiveScene().name != scene && Application.CanStreamedLevelBeLoaded(scene)) SceneManager.LoadScene(scene);
        }

        private void Start()
        {
            if (!Enabled) { enabled = false; return; }
            // QA runs are silent (editor playtests and the built game alike): sounds still play, nobody hears them.
            AudioListener.volume = 0f;
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
            while (_director.Story == null) yield return null; // the director creates the story in its own Start
            SteerEnding();
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
                if (Time.realtimeSinceStartup - sceneStart - _probeSeconds > 420f) { Finish("TIMEOUT in " + SceneManager.GetActiveScene().name); yield break; }
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
                        if (capture && StoryCapture.EveryLine)
                        {
                            // Camera review: every line, numbered, so each dialogue shot can be checked for framing.
                            yield return new WaitForSecondsRealtime(1.3f);
                            StoryCapture.Shot(prefix + "L" + lines.ToString("000") + "_" + dialogue.CurrentSpeakerId, 800, 450);
                            var speakerActor = _director.FindActor(dialogue.CurrentSpeakerId);
                            Log.Add($"  L{lines:000} {dialogue.CurrentSpeakerId} cam {Camera.main.transform.position:F2} head {(speakerActor != null ? speakerActor.HeadPosition.ToString("F2") : "-")} root {(speakerActor != null ? speakerActor.transform.position.ToString("F2") : "-")}");
                        }
                        else if (capture && styles.Add(SpeakerCatalog.Get(dialogue.CurrentSpeakerId).Style))
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
                    if (StopAtExplore > 0 && ++_explores >= StopAtExplore)
                    {
                        // Mid-save QA: the director has just written its checkpoint for this explore.
                        Log.Add($"[{Elapsed()}] STOP at explore {_explores} · player {_director.Player.transform.position:F2} · clues {_director.Clues.Count}");
                        Finish("PASS");
                        yield break;
                    }
                    if (capture && exploreShots < 2) { yield return new WaitForSecondsRealtime(1.5f); StoryCapture.Shot(prefix + "explore_" + ++exploreShots); }
                    if (capture && exploreShots == 2 && _director.Clues.Count > 0 && !_menusShot) yield return CaptureMenus(prefix);
                    yield return Explore(capture);
                }
                yield return null;
            }
            if (capture) { yield return new WaitForSecondsRealtime(1.2f); StoryCapture.Shot(prefix + "zz_complete"); }
            Log.Add($"[{Elapsed()}] CHAPTER {_director.ChapterNumber} COMPLETE · lines {lines} · choices {choices}");
            Log.Add($"[{Elapsed()}] FRAMES {SceneManager.GetActiveScene().name} · {FrameStats()}");
            string[] variables = _director.ChapterNumber == ChapterCatalog.Prologue ? new[] { "pl_che_kieu", "pl_vat", "pl_hanh_dong", "tinh_tao", "tu_trong" }
                : _director.ChapterNumber == 1 ? new[] { "c1_viec", "c1_manh_moi", "c1_nghi_ngo", "tinh_tao", "tu_trong", "doc_ky_khe", "c1_hoi_tien", "nghi_ngo", "ts_hieu" }
                : _director.ChapterNumber <= 3 ? new[] { "nghi_ngo", "manh_moi", "suy_luan", "gia_vo", "c3_viec", "c3_khe_uoc", "c3_bi_bat", "tra_loi_kieu_that" }
                : _director.ChapterNumber == 4 ? new[] { "c4_viec", "c4_canh_giac", "c4_tien", "c4_van_tu", "c4_so_no", "c4_ru_co_lam", "c4_bi_bat" }
                : _director.ChapterNumber == 5 ? new[] { "c5_lang_le", "c5_duong", "c5_xo_do", "c5_qte", "c5_co_lam_thoat", "c5_bi_bat" }
                : _director.ChapterNumber == 6 ? new[] { "kt_tin_tuong", "kt_biet_xuyen_khong", "c6_doi_chieu", "ev_nhan_chung", "ev_giay_no", "ev_tien", "c6_ho_so" }
                : _director.ChapterNumber == 7 ? new[] { "c7_viec", "c7_nhan_chung_vung", "c7_ke_hoach_kt", "c7_biet_hoi_lo", "c7_lui", "c7_r1", "c7_r2", "c7_r3", "c7_diem", "c7_long_tin", "c7_thua_vong", "c7_ket" }
                : new[] { "c7_ket", "pl_che_kieu", "c7_diem" };
            foreach (string variable in variables)
                Log.Add("   " + variable + " = " + _director.Story.variablesState[variable]);
            Log.Add("   clues = " + string.Join(", ", _director.Clues));

            bool chain = System.Environment.GetCommandLineArgs().Contains("-autopilotChain");
            // Every chapter hands over to the next one; the epilogue is the end of the game.
            string next = _director.ChapterNumber < ChapterCatalog.Epilogue ? _director.NextScene : null;
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
            var book = FindAnyObjectByType<TrangSachView>();
            if (book != null)
            {
                book.OpenBook();
                yield return new WaitForSecondsRealtime(0.6f);
                StoryCapture.Shot(prefix + "trang_sach");
                book.CloseBook();
            }
            var pause = FindAnyObjectByType<PauseMenuView>();
            if (pause == null) yield break;
            pause.Pause();
            yield return new WaitForSecondsRealtime(0.5f);
            StoryCapture.Shot(prefix + "pause");
            pause.Resume();
        }

        private static readonly string[] HubEnders = { "c1_cong", "c1_khe", "c1_ra_di", "pl_book", "pl_san", "c2_eavesdrop", "c3_makieu", "c4_leave", "c6_dossier", "c7_trong", "ep_book" };
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
            foreach (var behaviour in FindObjectsByType<MonoBehaviour>())
                if (behaviour is IAutopilotDriver driver)
                {
                    yield return driver.AutopilotStep(thorough);
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

        /// <summary>Lets gameplay sequences record events (retries inside a sequence) in the QA report.</summary>
        public static void Note(string text) { if (Enabled) Log.Add($"[{Elapsed()}] {text}"); }
        /// <summary>A QA probe found something wrong: fails the run like an error, without logging one (the Editor may pause play on errors).</summary>
        public static void Problem(string text) { if (!Enabled) return; Errors.Add("PROBE: " + text); Note("PROBLEM " + text); }
        /// <summary>A long QA probe ran inside a scene: its time does not count against the scene's own time limit.</summary>
        public static void ProbeTook(float seconds) { _probeSeconds += seconds; }
        private static float _probeSeconds;

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
