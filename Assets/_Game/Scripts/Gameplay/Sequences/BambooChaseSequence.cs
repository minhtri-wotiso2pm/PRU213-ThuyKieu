using System.Collections;
using System.Linq;
using ThuyKieu.Core;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Chase;
using ThuyKieu.Gameplay.Stealth;
using ThuyKieu.Player;
using ThuyKieu.UI;
using UnityEngine;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>
    /// Chapter 5: the run through the bamboo at night, in Kiều's own eyes, with Tú Bà's man and his torch behind her.
    ///
    /// She runs by herself (the forward intent is held for the player), and the player steers with the mouse, sidesteps
    /// with A/D, jumps (Space) and ducks (Ctrl): nothing is pressed for her and there are no on-screen key prompts. The
    /// course is fixed and learnable: a jump, a duck, a choice of two ways, then everything mixed with room to breathe, a
    /// ditch, culms that crack and fall across the path, and a plank bridge over the stream ravine whose far stake she can
    /// kick out (E) so nobody can follow.
    ///
    /// The pursuer is a real body on a real path (<see cref="ChasePursuer"/>): a stumble lets him close, a few in a row let
    /// him reach her, and only an honest reach catches her. Caught, or fallen into a ditch, she starts again from the
    /// edge of the forest: everything on the course is put back, the failure is counted once, and nothing before the
    /// chase (the story, Sở Khanh, rewards) is replayed or repeated.
    ///
    /// Everything this takes over (first person, auto-run, jump, crouch, camera lens and limits, cursor, controls, time
    /// scale, stamina, observe mode, simulated input) is handed back when it ends, fails out or the scene is torn down.
    /// Writes "c5_bi_bat" (+1 per failure), "c5_vap", "c5_loi", "c5_giay" and "c5_co_lam_thoat".
    /// </summary>
    public partial class BambooChaseSequence : StorySequence
    {
        private enum Outcome { None, Escaped, Caught, Fell }

        [SerializeField] private ChaseCourse _course;
        [SerializeField] private ThuyKieu.Environment.ForestGround _ground;
        [SerializeField] private ChasePursuer _pursuer;
        [Tooltip("The other servants from the alarm: not in the forest.")]
        [SerializeField] private StoryActor[] _others;
        [SerializeField] private ChaseRunner _companion;
        [SerializeField] private DropBridge _bridge;
        [SerializeField] private CatchScene _catch;
        [SerializeField] private float _runSpeed = 5f, _crouchRunSpeed = 3f;
        [Tooltip("How far behind her he starts, along the path; less when Sở Khanh shouted for him at the edge of the bamboo.")]
        [SerializeField] private float _startGap = 12f, _shoutedGap = 9f;
        [SerializeField] private float _fov = 74f;
        [Tooltip("Her body's weight in the view while she runs (bob in step with her feet, landings, leans, knocks).")]
        [SerializeField] private ChaseCameraMotion _motion;
        [Tooltip("The chase score in levels of pressure (falls back to the shared \"chase\" music without it).")]
        [SerializeField] private ChaseMusic _music;
        [Tooltip("Metres along the moonlit way where the score steps up: the combinations after the ways meet, the sprint.")]
        [SerializeField] private float[] _pressureAt = { 280f, 470f };

        private const string Controls = "Tự chạy · Chuột: chọn hướng · A/D: né sang bên · Space: nhảy · Ctrl (giữ): cúi · E: đạp cọc cầu";

        private struct Saved
        {
            public bool BodyFirstPerson, CameraFirstPerson, AutoRun, Jump, Slide, Crouch, CanRun;
            public Vector3? Scripted;
            public float RunSpeed, CrouchRunSpeed, Fov, Bob, Sensitivity, Near, LensFov, TimeScale;
            public Vector2 Pitch;
            public CursorLockMode Cursor;
            public bool CursorVisible, Stamina, Observe, Steps;
            public float StepOffset;
        }

        private StoryDirector _director;
        private Saved _saved;
        private float _awokeAt;
        private bool _owns, _withLam, _thick, _hinted, _calledOut, _heart;
        private int _bumps;
        private float _runStart, _minGap, _breathIn, _heartIn, _refusalNoteAt;
        private Outcome _outcome;
        private Stamina _stamina;
        private ObserveMode _observe;
        private FirstPersonFootsteps _steps;
        private CharacterController _body;

        /// <summary>Failures in this chase (each also counted once in c5_bi_bat).</summary>
        public int Failures { get; private set; }

        private void Awake() { _awokeAt = Time.realtimeSinceStartup; }

        public override IEnumerator Run(StoryDirector director)
        {
            _director = director;
            _withLam = _companion != null && director.GetBool("c4_ru_co_lam");
            // The chapter opens on this run, behind black: the forest NavMesh is still being built in the background, and
            // nothing (input, the pursuer, a catch) starts before it is there.
            float waited = Time.realtimeSinceStartup;
            while (_ground != null && !_ground.NavReady) yield return null;
            StoryAutopilot.Note($"FOREST ready · waited {Time.realtimeSinceStartup - waited:F2} s for the NavMesh · {Time.realtimeSinceStartup - _awokeAt:F2} s after the scene woke");
            // Into her own eyes behind a short black, never as a cut in the open (already black when the chapter opens here).
            if (director.Hud.FadeAlpha < 0.99f) yield return director.Hud.Fade(1f, 0.3f);
            Take();
            director.Hud.SetObjective("Chạy xuyên rừng tre, qua cầu khe suối", false);
            if (director.Audio != null)
            {
                director.Audio.SetAmbience(null); // a dry night in the bamboo: no rain under the run
                director.Audio.PlayMusic(_music != null ? "none" : "chase");
            }
            if (_music != null) _music.Begin(0);
            if (director.Autopilot && StoryAutopilot.Args.Contains("-autopilotLook")) yield return Look();
            if (director.Autopilot && StoryAutopilot.Args.Contains("-autopilotForest")) yield return Probe();

            var pilot = director.Autopilot ? AutopilotPlan() : null;
            while (true)
            {
                yield return Begin();
                yield return Attempt(pilot);
                if (_outcome == Outcome.Escaped) break;
                yield return Fail(_outcome);
                if (pilot != null) pilot = AutopilotPlan();
            }
            yield return Escape();
        }

        // ---------------------------------------------------------------- one attempt

        private float StartGap => _director.GetInt("c5_so_khanh") == 3 ? _shoutedGap : _startGap;

        /// <summary>Everyone and everything back at the edge of the forest; the screen comes up and she is running.</summary>
        private IEnumerator Begin()
        {
            var hud = _director.Hud;
            if (hud.FadeAlpha < 0.99f) yield return hud.Fade(1f, 0.25f);
            _director.SetFreeControl(false);
            ClearSimulated();
            _course.ResetAll(true);
            if (_motion != null) { _motion.enabled = _owns; _motion.ResetMotion(); }
            var start = _course.StartPoint;
            var player = _director.Player;
            player.Teleport(start.position, start.rotation);
            _director.CameraController.SnapBehindTarget(start.eulerAngles.y, 0f);
            float startAt = _course.Progress(start.position);
            Vector3 behind = _course.PointAt(1, startAt - StartGap);
            _pursuer.Begin(player, behind, Quaternion.LookRotation(_course.DirectionAt(1, startAt - StartGap)));
            foreach (var other in _others) if (other != null && other != _pursuer.Actor) other.gameObject.SetActive(false);
            if (_withLam)
            {
                Vector3 ahead = _course.PointAt(1, startAt + 4f);
                _companion.Begin(_course, player, ahead, Quaternion.LookRotation(_course.DirectionAt(1, startAt + 4f)));
            }
            else if (_companion != null) _companion.gameObject.SetActive(false);
            // Chị Lam must be off the planks before the stake goes (not at the stake: waiting for her there let him over).
            var crossing = _bridge.Crossing;
            float deckEnd = _course.Project(1, crossing.transform.TransformPoint(crossing.endPoint), out _);
            _bridge.Ready = () => !_withLam || _companion.Progress > deckEnd;
            _bumps = 0;
            _thick = false;
            _hinted = false;
            _minGap = float.MaxValue;
            _outcome = Outcome.None;
            _breathIn = _heartIn = 0f;
            _topLevel = 0;
            _director.Hud.SetControls(Controls);
            if (!_calledOut)
            {
                float now = Time.realtimeSinceStartup;
                StoryAutopilot.Note($"FOREST first frame up {now - _awokeAt:F2} s after the scene woke" +
                                    (StoryDirector.HandoverStartedAt >= 0f ? $", {now - StoryDirector.HandoverStartedAt:F2} s after Chapter 4's exit began" : ""));
            }
            // Free from the first visible frame: she is already running when the picture comes up.
            _director.SetFreeControl(true);
            _runStart = Time.time;
            if (!_calledOut) { _calledOut = true; StartCoroutine(CallsBehind()); }
            yield return hud.Fade(0f, 0.3f);
            StoryAutopilot.Note($"FOREST run starts · pursuer {StartGap:F0} m behind · Lam {(_withLam ? "with her" : "no")}");
        }

        /// <summary>
        /// The alarm from the house, heard on the run instead of watched (the chapter opens out of the back door): Tú Bà
        /// screaming behind her, chị Lam's word about the stream if she came. Never on a retry, never blocking.
        /// </summary>
        private IEnumerator CallsBehind()
        {
            var hud = _director.Hud;
            yield return new WaitForSeconds(0.6f);
            bool robbed = _director.GetInt("c4_tien") == 2 || _director.GetInt("c4_van_tu") > 0;
            if (_director.Audio != null) _director.Audio.PlaySfx("gong", 0.5f);
            hud.ShowBark("TuBa", robbed ? "Tủ của ta! Đứa nào mở tủ của ta?! Bắt nó lại!" : "Đứa nào lục đục dưới bếp đấy?! Bắt nó lại!", 3f);
            if (!_withLam || Failures > 0) yield break;
            yield return new WaitForSeconds(3.6f);
            if (Failures == 0 && _outcome == Outcome.None) hud.ShowBark("CoLam", "Qua rừng tre là tới khe suối. Chị biết đường, cứ chạy theo chị!", 3.2f);
        }

        private IEnumerator Attempt(Pilot pilot)
        {
            var player = _director.Player;
            while (_outcome == Outcome.None)
            {
                if (GamePause.IsPaused) { yield return null; continue; }
                Vector3 feet = player.transform.position;
                if (_course.InThickWay(feet)) _thick = true;
                _minGap = Mathf.Min(_minGap, _pursuer.PathDistance);
                Pressure(feet);
                Cues();
                if (!_hinted && _bridge.OnFarBank(feet) && !_bridge.Dropped)
                {
                    _hinted = true;
                    _director.Hud.ShowBark("NoiTam", "Cái cọc cầu mục rồi — đạp đổ nó!", 2.2f);
                }
                if (_bridge.Dropped && _course.InFinish(feet)) _outcome = Outcome.Escaped;
                else if (_course.Fallen(feet)) _outcome = Outcome.Fell;
                else if (_pursuer.CanCatch(player)) _outcome = Outcome.Caught;
                else if (_director.Autopilot && _pursuer.FlatDistance < 1.8f && Time.time >= _refusalNoteAt)
                {
                    _refusalNoteAt = Time.time + 0.5f;
                    StoryAutopilot.Note($"FOREST no catch at {_pursuer.FlatDistance:F2} m: {_pursuer.Refusal}");
                }
                else if (pilot != null) pilot.Step(this);
                // QA only: a pilot that has not got anywhere in four minutes of game time is stuck; report it, start over.
                if (pilot != null && _outcome == Outcome.None && Time.time - _runStart > 240f)
                {
                    StoryAutopilot.Problem($"forest pilot stuck at {player.transform.position:F1} (moon way {_course.Progress(player.transform.position):F0} m)");
                    _outcome = Outcome.Fell;
                }
                yield return null;
            }
            ClearSimulated();
            StoryAutopilot.Note($"FOREST {_outcome} after {Time.time - _runStart:F1} s · bumps {_bumps} · closest {_minGap:F1} m · way {(_thick ? "thick" : "moon")} · at {player.transform.position:F1}");
        }

        /// <summary>Caught (his hands on her, the Chapter 4 catch played by him) or fallen; counted once; then back to the start.</summary>
        private IEnumerator Fail(Outcome outcome)
        {
            Failures++;
            _director.SetInt("c5_bi_bat", _director.GetInt("c5_bi_bat") + 1);
            _director.SetFreeControl(false);
            // The catch takes the camera: no run motion under it (back on with the next attempt); the score drops under the
            // catch and goes back to its first level for the next try (at a bar line, never restarted).
            if (_music != null) { _music.Duck(0.35f, 3f); _music.Want(0); }
            if (_motion != null) _motion.enabled = false;
            var hud = _director.Hud;
            if (outcome == Outcome.Caught)
            {
                bool done = false;
                bool staged = _catch != null && _catch.TryPlay(_pursuer.Actor, "GiaNhan", "Bắt lấy con ranh!", () => done = true);
                _pursuer.Halt();
                if (staged) while (!done) yield return null;
                else if (_director.Audio != null) _director.Audio.PlaySfx("fail");
            }
            else
            {
                _pursuer.Halt();
                if (_director.Fx != null) _director.Fx.Shake(0.45f, 0.4f);
                if (_director.Audio != null) { _director.Audio.PlaySfx("stumble"); _director.Audio.PlaySfx("fail", 0.8f); }
                yield return hud.Fade(1f, 0.3f);
            }
            if (_companion != null && _withLam) _companion.End();
            hud.FadeAlpha = 1f;
            string headline = outcome == Outcome.Caught ? "Bị bắt lại!" : "Ngã xuống khe!";
            string detail = outcome == Outcome.Caught
                ? "Mỗi lần vấp hay va vào thân tre, hắn lại áp sát. Nhìn đường từ xa: nhảy (Space) qua cây đổ, giữ Ctrl để cúi dưới tre thấp, A/D để né."
                : "Nhảy (Space) sát mép khe, đừng nhảy quá sớm. Bờ bên kia dốc và trơn.";
            yield return hud.FailCard(headline, detail, _director.Autopilot ? 0f : 2.2f);
        }

        /// <summary>Over the bridge and up the temple path: she looks back at the torch stopped across the ravine.</summary>
        private IEnumerator Escape()
        {
            var player = _director.Player;
            int seconds = Mathf.RoundToInt(Time.time - _runStart);
            _director.SetFreeControl(false);
            _director.Hud.SetControls(string.Empty);
            _director.CameraController.LookTarget = _pursuer.transform.position + Vector3.up * 1.5f;
            // Clear of him: the score lets go as she looks back at the torch stopped across the ravine.
            if (_music != null) _music.Stop(4f);
            if (_director.Audio != null) _director.Audio.PlaySfx("breath_in", 1f);
            yield return new WaitForSeconds(1.4f);
            _pursuer.Halt();
            _pursuer.Actor.FaceInstant(player.transform.position);
            _director.CameraController.LookTarget = null;
            bool lamOver = _withLam && _companion.Progress > _course.Project(1, _bridge.Stake.position, out _);
            _director.SetInt("c5_vap", _bumps);
            _director.SetInt("c5_loi", _thick ? 2 : 1);
            _director.SetInt("c5_giay", seconds);
            _director.SetBool("c5_co_lam_thoat", lamOver);
            StoryAutopilot.Note($"FOREST escaped in {seconds} s (game time) · failures {Failures} · bumps {_bumps} · way {(_thick ? 2 : 1)} · Lam over {lamOver}");
            yield return _director.Hud.Fade(1f, 0.4f);
            if (_withLam) { _companion.End(); _companion.transform.SetPositionAndRotation(player.transform.position - player.transform.forward * 1.4f, player.transform.rotation); }
            GiveBack();
            _director.CameraController.SnapBehindTarget(player.transform.eulerAngles.y + 180f, 8f);
            yield return _director.Hud.Fade(0f, 0.5f);
        }

        // ---------------------------------------------------------------- the score

        /// <summary>How hard the score pushes here: calm-tense to the ways meeting, harder through the combinations, the top for the sprint and the bridge.</summary>
        private void Pressure(Vector3 feet)
        {
            if (_music == null) return;
            float s = _course.Progress(feet);
            int level = 0;
            foreach (float at in _pressureAt) if (s >= at) level++;
            _music.Want(level);
            _topLevel = Mathf.Max(_topLevel, _music.Level);
            _music.Pursuer(_pursuer.PathDistance);
        }

        private void OnCulmCracked() { if (_music != null) _music.Duck(0.55f, 1.6f); }

        // ---------------------------------------------------------------- cues by ear

        /// <summary>Her breath quickens and her heart is heard as he really gets closer (his own footfalls and shouts are his).</summary>
        private void Cues()
        {
            var audio = _director.Audio;
            if (audio == null) return;
            float gap = _pursuer.FlatDistance;
            float close = Mathf.Clamp01(1f - (gap - 2f) / 14f);
            if ((_breathIn -= Time.deltaTime) <= 0f)
            {
                _breathIn = Mathf.Lerp(1.3f, 0.55f, close);
                audio.PlaySfx("breath_in", Mathf.Lerp(0.25f, 0.6f, close));
            }
            // The heart comes in under 6.5 m and goes only past 8.5 m: no flicker as the gap breathes.
            if (gap < 6.5f) _heart = true;
            else if (gap > 8.5f) _heart = false;
            if (_heart && (_heartIn -= Time.deltaTime) <= 0f)
            {
                _heartIn = Mathf.Lerp(0.95f, 0.6f, close);
                audio.PlaySfx("heartbeat", Mathf.Lerp(0.3f, 0.75f, close));
            }
        }

        /// <summary>What her foot meets (the floor's tag under her): earth on the forest ground, wood on the planks, stone on rock.</summary>
        private string Ground()
        {
            var floor = _steps != null ? _steps.FloorAt(_director.Player.transform.position, out _) : ThuyKieu.Environment.Surface.Earth;
            return floor == ThuyKieu.Environment.Surface.Wood ? "wood"
                : floor == ThuyKieu.Environment.Surface.Stone || floor == ThuyKieu.Environment.Surface.WetStone ? "stone" : "earth";
        }

        private Vector3 Feet => _director.Player.transform.position + Vector3.up * 0.1f;

        // The push off the ground and the cloth moving; no whoosh, nothing for a clean jump but the jump.
        private void OnJumped()
        {
            if (_director.Audio != null)
            {
                _director.Audio.PlayAt("jump_" + Ground(), Feet, 0.8f, 6f, Random.Range(0.97f, 1.03f));
                _director.Audio.PlaySfx("cloth", 0.25f);
            }
            if (_motion != null) _motion.Jump();
            StoryAutopilot.Note($"FOREST jump at {_director.Player.transform.position:F1} · {_director.Player.CurrentSpeed:F1} m/s · t {Time.time - _runStart:F2} · gap {_pursuer.PathDistance:F1} (flat {_pursuer.FlatDistance:F1}) · his speed {_pursuer.Speed:F2}");
        }

        // The landing on what is there, as hard as she really came down (her own steps are quiet until the next stride, so
        // a landing is one sound, never a landing and two footfalls).
        private void OnLanded(float speed)
        {
            if (speed < 2f) return;
            float force = Mathf.InverseLerp(2f, 8f, speed);
            if (_director.Audio != null) _director.Audio.PlayAt("land_" + Ground(), Feet, Mathf.Lerp(0.35f, 1f, force), 8f, Random.Range(0.97f, 1.03f));
            if (_motion != null) _motion.Land(speed);
        }

        // A trip on a log is her foot scuffing and catching; running into a culm is the hollow knock of the culm.
        private void OnBumped(Vector3 point)
        {
            _bumps++;
            Vector3 feet = _director.Player.transform.position;
            bool trip = point.y - feet.y < 0.6f;
            if (_director.Audio != null)
            {
                if (trip) _director.Audio.PlayAt("trip_" + Ground(), Feet, 0.9f, 8f);
                else _director.Audio.PlayAt("culm_knock", point, 0.9f, 12f, Random.Range(0.95f, 1.05f));
                _director.Audio.PlaySfx("breath_startled", 0.5f);
            }
            if (_motion != null) _motion.Knock(point, trip ? 1f : 0.8f);
            StoryAutopilot.Note($"FOREST bump at {point:F1} · t {Time.time - _runStart:F2} · gap {_pursuer.PathDistance:F1} (flat {_pursuer.FlatDistance:F1}) · his speed {_pursuer.Speed:F2}");
        }

        private void OnHeadHit()
        {
            Vector3 head = _director.Player.transform.position + Vector3.up * 1.6f;
            if (_director.Audio != null) _director.Audio.PlayAt("culm_knock", head, 0.6f, 8f, 1.08f);
            if (_motion != null) _motion.Knock(head + Vector3.up, 0.5f);
            StoryAutopilot.Note("FOREST head hit on the way up");
        }

        /// <summary>
        /// He was on the planks when they went. The deck swings down from its hinge at the near lip, so he is thrown back
        /// down it against the near bank, catches the ropes there and hauls himself back up onto the near side: he ends where
        /// the story leaves him, his torch stopped across the ravine, without ever being moved by anything but the fall.
        /// </summary>
        private void OnBridgeFell()
        {
            if (_pursuer.Traverser.CurrentLink == null || _pursuer.Traverser.CurrentLink != _bridge.Crossing) return;
            StoryAutopilot.Note("FOREST pursuer was on the bridge: thrown back against the near bank");
            StartCoroutine(PursuerThrownBack());
        }

        private IEnumerator PursuerThrownBack()
        {
            _pursuer.Halt();
            _pursuer.Actor.DisableNavigation();
            Vector3 from = _pursuer.transform.position;
            Vector3 lip = _bridge.transform.position;
            Vector3 back = (lip - from);
            back.y = 0f;
            Vector3 hanging = lip + Vector3.down * 1.6f + back.normalized * 0.4f;
            Vector3 bank = lip + back.normalized * 1.6f;
            // Down the tipping deck to the hinge, 0.8 s.
            for (float t = 0f; t < 0.8f; t += Time.deltaTime)
            {
                float k = t / 0.8f;
                _pursuer.transform.position = Vector3.Lerp(from, hanging, k * k);
                yield return null;
            }
            if (_director.Audio != null) _director.Audio.PlayAt("guard_land", hanging, 1f, 30f, 0.88f);
            yield return new WaitForSeconds(0.8f);
            // Up the ropes onto the near bank, 1.6 s.
            for (float t = 0f; t < 1.6f; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / 1.6f);
                _pursuer.transform.position = Vector3.Lerp(hanging, bank, k);
                yield return null;
            }
            _pursuer.Actor.EnableNavigation();
            _pursuer.Actor.FaceTowards(_director.Player.transform.position);
        }

        // ---------------------------------------------------------------- take over and give back

        private void Take()
        {
            var player = _director.Player;
            var camera = _director.CameraController;
            var lens = camera.GetComponent<Camera>();
            _stamina = player.GetComponent<Stamina>();
            _observe = camera.GetComponent<ObserveMode>();
            _steps = player.GetComponent<FirstPersonFootsteps>();
            _body = player.GetComponent<CharacterController>();
            _saved = new Saved
            {
                BodyFirstPerson = player.FirstPerson, CameraFirstPerson = camera.IsFirstPerson, AutoRun = player.AutoRun, Jump = player.JumpEnabled, Slide = player.SlideOffSteep,
                Crouch = player.AllowCrouch, CanRun = player.CanRun, Scripted = player.ScriptedMove, RunSpeed = player.AutoRunSpeed,
                CrouchRunSpeed = player.CrouchRunSpeed, Fov = camera.FovOverride, Bob = camera.RunBob, Sensitivity = camera.Sensitivity,
                Pitch = camera.PitchLimits, Near = lens != null ? lens.nearClipPlane : 0.08f, LensFov = lens != null ? lens.fieldOfView : 58f,
                TimeScale = Time.timeScale, Cursor = UnityEngine.Cursor.lockState, CursorVisible = UnityEngine.Cursor.visible,
                Stamina = _stamina != null && _stamina.enabled, Observe = _observe != null && _observe.enabled,
                Steps = _steps != null && _steps.enabled, StepOffset = _body != null ? _body.stepOffset : 0.25f
            };
            _owns = true;
            player.ScriptedMove = null;
            player.FirstPerson = true;
            camera.SetFirstPerson(true, player);
            camera.SetDialogueCue(null);
            player.AutoRun = true;
            player.AutoRunSpeed = _runSpeed;
            player.CrouchRunSpeed = _crouchRunSpeed;
            player.JumpEnabled = true;
            player.SlideOffSteep = true; // the banks hold her: no climbing them by jumping again and again off their face
            player.AllowCrouch = true;
            player.CanRun = false;
            camera.FovOverride = _fov;
            camera.RunBob = 0f; // the run's own motion (in step with her feet) replaces the shared stride bob
            if (_motion != null) { _motion.enabled = true; _motion.ResetMotion(); }
            camera.Sensitivity = 0.1f;
            camera.PitchLimits = new Vector2(-70f, 75f);
            if (lens != null) lens.nearClipPlane = 0.04f;
            if (_stamina != null) _stamina.enabled = false;
            if (_observe != null) _observe.enabled = false;
            // Her own steps by surface (mud, planks), and a low step: a fallen culm is jumped, never walked up.
            if (_steps != null) _steps.enabled = true;
            if (_body != null) _body.stepOffset = 0.12f;
            player.Jumped += OnJumped;
            player.Landed += OnLanded;
            player.Bumped += OnBumped;
            player.HeadHit += OnHeadHit;
            _bridge.Fell += OnBridgeFell;
            foreach (var culm in _course.GetComponentsInChildren<FallingBamboo>(true)) culm.Cracked += OnCulmCracked;
        }

        private void GiveBack()
        {
            if (!_owns) return;
            _owns = false;
            ClearSimulated();
            var player = _director.Player;
            var camera = _director.CameraController;
            if (player != null)
            {
                player.Jumped -= OnJumped;
                player.Landed -= OnLanded;
                player.Bumped -= OnBumped;
                player.HeadHit -= OnHeadHit;
                player.AutoRun = _saved.AutoRun;
                player.AutoRunSpeed = _saved.RunSpeed;
                player.CrouchRunSpeed = _saved.CrouchRunSpeed;
                player.JumpEnabled = _saved.Jump;
                player.SlideOffSteep = _saved.Slide;
                player.AllowCrouch = _saved.Crouch;
                player.CanRun = _saved.CanRun;
                player.ScriptedMove = _saved.Scripted;
                player.FirstPerson = _saved.BodyFirstPerson;
            }
            if (_bridge != null) { _bridge.Fell -= OnBridgeFell; _bridge.Armed = false; }
            if (_course != null) foreach (var culm in _course.GetComponentsInChildren<FallingBamboo>(true)) culm.Cracked -= OnCulmCracked;
            if (_music != null && _music.Playing) _music.Stop(2f);
            if (camera != null)
            {
                camera.SetFirstPerson(_saved.CameraFirstPerson, player);
                camera.FovOverride = _saved.Fov;
                camera.RunBob = _saved.Bob;
                camera.Sensitivity = _saved.Sensitivity;
                camera.PitchLimits = _saved.Pitch;
                camera.LookTarget = null;
                var lens = camera.GetComponent<Camera>();
                if (lens != null) { lens.nearClipPlane = _saved.Near; lens.fieldOfView = _saved.LensFov; }
            }
            if (_stamina != null) _stamina.enabled = _saved.Stamina;
            if (_observe != null) _observe.enabled = _saved.Observe;
            if (_steps != null) _steps.enabled = _saved.Steps;
            if (_motion != null) _motion.enabled = false;
            if (_body != null) _body.stepOffset = _saved.StepOffset;
            if (!GamePause.IsPaused) Time.timeScale = _saved.TimeScale;
            UnityEngine.Cursor.lockState = _saved.Cursor;
            UnityEngine.Cursor.visible = _saved.CursorVisible;
            if (_pursuer != null) _pursuer.Halt();
            if (_companion != null && _companion.Active) _companion.End();
            if (_course != null) foreach (var reset in _course.GetComponentsInChildren<IChaseReset>(true)) reset.Armed = false;
        }

        private static void ClearSimulated()
        {
            InputReader.SimulatedMove = Vector2.zero;
            InputReader.SimulatedLook = Vector2.zero;
            InputReader.SimulatedCrouch = false;
            InputReader.SimulatedInteract = false;
        }

        // Torn down mid-chase (scene change, quitting to the menu): hand everything back.
        private void OnDisable() { if (_owns) GiveBack(); }

        // Alt-Tab or a lost window mid-run: the run must not go on without the player.
        private void OnApplicationFocus(bool focused)
        {
            if (focused || !_owns || _director == null || _director.Autopilot || GamePause.IsPaused) return;
            var pause = FindAnyObjectByType<PauseMenuView>();
            if (pause != null) pause.Pause();
        }
    }
}
