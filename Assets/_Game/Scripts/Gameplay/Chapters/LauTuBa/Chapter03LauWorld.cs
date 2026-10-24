using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.House;
using ThuyKieu.Gameplay.Stealth;
using ThuyKieu.Interaction;
using ThuyKieu.Player;
using UnityEngine;

namespace ThuyKieu.Gameplay.Chapters.LauTuBa
{
    /// <summary>
    /// Chapter 3 in first person — arriving at Tú Bà's house (docs/codex/CHAPTER3_4_LAU_TU_BA_DESIGN.md §4).
    /// Afternoon after rain: the house can be walked freely except the rear range, where the guard turns anyone back.
    /// After two things learned, dusk falls and the ledger office stands ajar. Reading the bill of sale brings Tú Bà and
    /// Mã Giám Sinh through the inner door (B): hide (6 s of warning) and overhear them. They leave by the front and lock
    /// it; the only way out is back through door B, never bolted on this day, into Tú Bà's empty room. Back in her own
    /// room the door is latched from outside and the chapter closes.
    /// </summary>
    public class Chapter03LauWorld : MonoBehaviour, IChapterSummary, IAutopilotDriver
    {
        [Header("Story")]
        [SerializeField] private StoryDirector _director;
        [SerializeField] private ThuyKieu.Environment.NarrowNavMesh _navMesh;
        [SerializeField] private KieuInventory _inventory;

        [Header("People")]
        [SerializeField] private Watchman _guard;
        [SerializeField] private StoryActor _tuBa;
        [SerializeField] private StoryActor _ma;
        [SerializeField] private StoryActor _hong;
        [SerializeField] private StoryActor _lam;
        [SerializeField] private StoryActor _maKieu;
        [SerializeField] private Watchman _tuBaEyes;
        [Tooltip("Plays the grab when the guard really reaches her, before the failure screen.")]
        [SerializeField] private CatchScene _catch;

        [Header("House")]
        [SerializeField] private HouseDoor _gateLeft;
        [SerializeField] private HouseDoor _gateRight;
        [SerializeField] private HouseDoor _kieuDoor;
        [SerializeField] private HouseDoor _innerDoor;
        [SerializeField] private HouseDoor _officeDoor;
        [SerializeField] private HouseDoor _tuBaDoor;
        [SerializeField] private HouseDoor[] _allDoors;
        [SerializeField] private Transform _windowCurtain;
        [SerializeField] private Light _sun;
        [SerializeField] private Light[] _lanterns;
        [SerializeField] private Light[] _roomLamps;
        [Tooltip("Soft daylight bounce under the eaves and in the rooms; it fades with the sun at dusk.")]
        [SerializeField] private Light[] _dayFills;

        [Header("Places")]
        [SerializeField] private Transform _arrivalTuBa;
        [SerializeField] private Transform _arrivalMa;
        [SerializeField] private Transform _hallSpot;
        [SerializeField] private Transform _tuBaInsideB;
        [SerializeField] private Transform _maInsideB;
        [SerializeField] private Transform _officeDesk;
        [SerializeField] private Transform _officeDoorOutside;
        [SerializeField] private Transform _tuBaRoomSpot;
        [SerializeField] private Transform _kieuRoomSpawn;
        [SerializeField] private Transform _officeSpawn;
        [SerializeField] private Transform _tuBaRoomSpawn;
        [SerializeField] private Transform _kieuDoorOutside;
        [SerializeField] private BoxCollider _kieuRoom;
        [SerializeField] private BoxCollider _office;
        [SerializeField] private BoxCollider _tuBaRoom;
        [SerializeField] private Transform[] _guardRoute;
        [SerializeField] private ExploreTarget _boxTarget, _drawerTarget, _cabinetTarget;

        private bool _dusk, _voicesStarted, _overheard, _escapedOffice, _ended, _curtainOpen;
        private Coroutine _voices;
        private float _barkCooldown;

        private Transform Player => _director.Player.transform;

        private void OnEnable()
        {
            _director.ExploreChanged += OnExplore;
            _director.CheckpointRestored += OnRestored;
            _director.SceneChanged += OnScene;
            _director.TagReceived += OnTag;
            HideSpot.Entered += OnHide;
            HideSpot.Exited += OnUnhide;
        }

        private void OnDisable()
        {
            _director.ExploreChanged -= OnExplore;
            _director.CheckpointRestored -= OnRestored;
            _director.SceneChanged -= OnScene;
            _director.TagReceived -= OnTag;
            HideSpot.Entered -= OnHide;
            HideSpot.Exited -= OnUnhide;
        }

        private void OnDestroy() { LightExposure.Enable(false); }

        private void Start()
        {
            if (_navMesh != null) _navMesh.Build();
            foreach (var actor in FindObjectsByType<StoryActor>()) actor.EnableNavigation();
            _director.Player.AllowCrouch = true;
            _guard.Caught += OnCaught;
            _tuBaEyes.Caught += OnCaught;
            _tuBaEyes.enabled = false;
            foreach (var door in _allDoors) door.Refused += OnRefused;
            _innerDoor.SetLock(DoorLock.None);
            _officeDoor.SetLock(DoorLock.Locked);
            if (_director.Audio != null) _director.Audio.SetAmbience("after_rain");
            _director.Hud.SetControls("WASD: đi · Shift: chạy · Ctrl: khom · E: tương tác (giữ E: làm khẽ) · Chuột phải: nhìn kỹ · Esc: tạm dừng");
            foreach (var light in _lanterns) light.enabled = false;
            LightExposure.Enable(true);
            // Arrival: Tú Bà and Mã Giám Sinh by the gate; the gate open behind Kiều.
            _tuBa.Place(_arrivalTuBa.position, _arrivalTuBa.rotation);
            _ma.Place(_arrivalMa.position, _arrivalMa.rotation);
            _gateLeft.SetOpen(1f);
            _gateRight.SetOpen(1f);
            _guard.ResetAt(0);
            ApplyProgress();
        }

        /// <summary>Resume support: rebuilds the afternoon from what the story already knows.</summary>
        private void ApplyProgress()
        {
            bool explored = _director.GetInt("c3_viec") > 0 || _director.GetBool("c3l_phong");
            if (explored) { CloseGate(); _tuBa.Place(_hallSpot.position, _hallSpot.rotation); _ma.gameObject.SetActive(false); }
            if (_director.GetInt("c3_viec") >= 2) Dusk(true);
            if (_director.GetBool("c3_nghe_len")) { _overheard = true; _officeDoor.SetLock(DoorLock.Locked); _officeDoor.SetOpen(0f); }
        }

        // ------------------------------------------------------------------ story events

        private void OnScene(string scene)
        {
            switch (scene)
            {
                case "Lau_Explore":
                    CloseGate();
                    // Tú Bà settles in the hall; Mã Giám Sinh goes to rest (he reappears with her at dusk).
                    _tuBa.MoveTo(_hallSpot.position, 1.1f);
                    _ma.gameObject.SetActive(false);
                    break;
                case "Lau_Locked":
                    StartCoroutine(TheyLeave());
                    break;
                case "Lau_End":
                    _ended = true;
                    _guard.Pause(true);
                    _kieuDoor.SetOpen(0f, 0.3f, "door_slam");
                    _kieuDoor.SetLock(DoorLock.LatchedOutside);
                    _maKieu.Place(_kieuDoorOutside.position, _kieuDoorOutside.rotation * Quaternion.Euler(0f, 180f, 0f));
                    break;
            }
        }

        private void OnTag(string key, string value)
        {
            if (key != "speaker" || _director.CameraController == null) return;
            var actor = value == "NoiTam" || value == "KieuThat" || value == "TrangSach" || value == "Narrator" || value == "Kieu" ? null : _director.FindActor(value);
            if (actor != null && actor.isActiveAndEnabled) _director.CameraController.LookTarget = actor.HeadPosition;
        }

        private void OnExplore(bool exploring)
        {
            _guard.Pause(!exploring || _ended);
            if (!exploring) return;
            _director.CameraController.LookTarget = null;
            _director.Hud.SetExploreOptions(null);
            if (!_dusk && _director.GetInt("c3_viec") >= 2) Dusk(false);
            if (_director.GetBool("c3l_phong") && !_curtainOpen) OpenCurtain();
            if (_director.GetBool("c3_khe_uoc") && !_voicesStarted && !_overheard) _voices = StartCoroutine(VoicesApproach());
            if (_overheard && !_escapedOffice) _director.Hud.SetObjective("Thoát khỏi phòng sổ sách", false);
        }

        private void CloseGate()
        {
            _gateLeft.SetOpen(0f, 1.2f);
            _gateRight.SetOpen(0f, 1.2f);
        }

        private void OpenCurtain()
        {
            _curtainOpen = true;
            if (_windowCurtain != null) _windowCurtain.localScale = new Vector3(0.25f, 1f, 1f);
        }

        /// <summary>Dusk: the sky cools, the lanterns are lit, the office stands ajar (the clerk went to help with the guests).</summary>
        private void Dusk(bool instant)
        {
            _dusk = true;
            StartCoroutine(DuskFade(instant ? 0f : 6f));
            _officeDoor.SetLock(DoorLock.None);
            _officeDoor.SetOpen(0.25f, instant ? 0f : 1f);
            _director.Hud.SetObjective("Trời chạng vạng. Cửa phòng sổ sách đang hé.");
            if (!instant) _director.Hud.ShowBark("NoiTam", "Người giữ sổ ra sảnh lo tiệc. Cửa phòng sổ sách hé... Đó là khu cấm, người canh mà thấy là lôi về.", 4.2f);
        }

        private IEnumerator DuskFade(float seconds)
        {
            Color fromSun = _sun.color, toSun = new Color(0.95f, 0.5f, 0.32f);
            float fromIntensity = _sun.intensity, toIntensity = 0.55f;
            Color fromAmbient = RenderSettings.ambientLight, toAmbient = new Color(0.22f, 0.2f, 0.26f);
            Color fromSky = RenderSettings.ambientSkyColor, toSky = new Color(0.36f, 0.34f, 0.44f);
            Color fromEquator = RenderSettings.ambientEquatorColor, toEquator = new Color(0.3f, 0.24f, 0.24f);
            var fills = _dayFills ?? new Light[0];
            var fillFrom = fills.Select(l => l != null ? l.intensity : 0f).ToArray();
            foreach (var light in _lanterns) light.enabled = true;
            foreach (var light in _roomLamps) light.enabled = true;
            LightExposure.Enable(true);
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                float k = t / seconds;
                _sun.color = Color.Lerp(fromSun, toSun, k);
                _sun.intensity = Mathf.Lerp(fromIntensity, toIntensity, k);
                RenderSettings.ambientLight = Color.Lerp(fromAmbient, toAmbient, k);
                RenderSettings.ambientSkyColor = Color.Lerp(fromSky, toSky, k);
                RenderSettings.ambientEquatorColor = Color.Lerp(fromEquator, toEquator, k);
                for (int i = 0; i < fills.Length; i++) if (fills[i] != null) fills[i].intensity = Mathf.Lerp(fillFrom[i], fillFrom[i] * 0.45f, k);
                yield return null;
            }
            _sun.color = toSun;
            _sun.intensity = toIntensity;
            RenderSettings.ambientLight = toAmbient;
            RenderSettings.ambientSkyColor = toSky;
            RenderSettings.ambientEquatorColor = toEquator;
            for (int i = 0; i < fills.Length; i++) if (fills[i] != null) fills[i].intensity = fillFrom[i] * 0.45f;
        }

        // ------------------------------------------------------------------ the office

        /// <summary>
        /// Voices beyond the inner door, footsteps, the bolt-less door swinging open: about six seconds to get into the
        /// wardrobe or behind the screen. Caught in the open means back to the office entrance.
        /// </summary>
        private IEnumerator VoicesApproach()
        {
            _voicesStarted = true;
            yield return Wait(2.5f);
            _ma.gameObject.SetActive(true);
            _ma.EnableNavigation();
            _tuBa.Place(_tuBaRoomSpot.position, _tuBaRoomSpot.rotation);
            _ma.Place(_tuBaRoomSpot.position + Vector3.left * 0.8f, _tuBaRoomSpot.rotation);
            if (_director.Audio != null) _director.Audio.PlayAt("mumble", _innerDoor.transform.position + Vector3.up * 1.5f + Vector3.left, 0.8f, 12f);
            _director.Hud.ShowBark("TuBa", "(sau cửa thông) ...vào đây mà tính cho rõ...", 2.6f);
            _director.Hud.ShowBark("NoiTam", "Có người sau cửa thông! Trốn đi — tủ áo, bình phong!", 3f);
            yield return Wait(3.5f);
            _innerDoor.SetOpen(1f, 0.8f, "door_fast");
            _tuBa.MoveTo(_tuBaInsideB.position, 1.1f);
            _ma.MoveTo(_maInsideB.position, 1.1f);
            // They walk in and talk at the desk. Tú Bà catches Kiều only by seeing her (or seeing her slip into a hiding
            // place); unseen, the talk simply waits until Kiều is hidden somewhere in the office to listen.
            _tuBaEyes.Forget();
            _tuBaEyes.enabled = true;
            _tuBaEyes.Perceiving = true;
            bool settled = false, hinted = false;
            while (true)
            {
                if (_tuBaEyes.Awareness >= 1f) { FailSeenByTuBa(); yield break; }
                if (!settled && !_tuBa.IsMoving && !_ma.IsMoving)
                {
                    settled = true;
                    _tuBa.FaceInstant(_officeDesk.position);
                    _ma.FaceInstant(_officeDesk.position);
                }
                // Something caught her eye and she went to look: once she gives up she goes back to the desk.
                bool calm = _tuBaEyes.State == WatchState.Patrol;
                if (settled && calm && !_tuBa.IsMoving && Vector3.Distance(_tuBa.transform.position, _tuBaInsideB.position) > 0.6f)
                    _tuBa.MoveTo(_tuBaInsideB.position, 1.1f);
                bool inOffice = _office.bounds.Contains(Player.position + Vector3.up * 0.5f);
                if (settled && calm && !_tuBa.IsMoving && _director.Player.IsHidden && inOffice) break;
                if (settled && !hinted && !inOffice)
                {
                    hinted = true;
                    _director.Hud.ShowBark("NoiTam", "Họ đứng bên bàn sổ mà bàn chuyện... Phải lẻn vào nấp trong phòng mới nghe rõ.", 3.6f);
                }
                yield return Exploring();
            }
            _tuBaEyes.enabled = false;
            if (_director.IsExploreAvailable("c3_office_hide")) _director.ChooseExplore("c3_office_hide");
        }

        private void FailSeenByTuBa()
        {
            if (_ended) return;
            _tuBaEyes.enabled = false;
            _director.Fail("Tú Bà bắt gặp!", "\"Con ranh, vào đây làm gì?\" — Mụ chỉ bắt được khi thấy tận mắt. Nghe tiếng sau cửa thông thì chui vào tủ áo hoặc nấp sau bình phong, đừng để mụ thấy lúc chui vào.");
        }

        private IEnumerator TheyLeave()
        {
            _overheard = true;
            _tuBa.MoveTo(_officeDoorOutside.position, 1.1f);
            _ma.MoveTo(_officeDoorOutside.position + Vector3.right, 1.1f);
            _officeDoor.SetOpen(1f, 0.6f, "door_fast");
            yield return Wait(4f);
            _officeDoor.SetOpen(0f, 0.4f, "door_slam");
            _officeDoor.SetLock(DoorLock.Locked);
            if (_director.Audio != null) _director.Audio.PlayAt("key_turn", _officeDoor.transform.position + Vector3.up, 0.8f, 10f);
            _tuBa.Place(_hallSpot.position, _hallSpot.rotation);
            _ma.gameObject.SetActive(false);
            // The guard passes the paper door of Tú Bà's room soon after: a shadow to wait out.
            _guard.ResetAt(Mathf.Min(1, _guardRoute.Length - 1));
            _director.Hud.SetObjective("Thoát khỏi phòng sổ sách", false);
        }

        private void Update()
        {
            if (_barkCooldown > 0f) _barkCooldown -= Time.deltaTime;
            if (_ended || !_director.InExplore || CatchScene.Playing) return;
            // Out through Tú Bà's room: a checkpoint there, then home.
            if (_overheard && !_escapedOffice && _tuBaRoom.bounds.Contains(Player.position + Vector3.up * 0.5f))
            {
                _escapedOffice = true;
                _director.Hud.SetObjective("Về phòng mình ở hiên tây");
                _director.Hud.ShowBark("NoiTam", "Phòng mụ. Trống. Đợi người canh đi qua cửa giấy đã...", 3.2f);
                _director.SaveCheckpoint();
            }
            if (_escapedOffice && _kieuRoom.bounds.Contains(Player.position + Vector3.up * 0.5f) && _director.IsExploreAvailable("c3_end"))
                _director.ChooseExplore("c3_end");
        }

        private void OnRefused(HouseDoor door, string reason)
        {
            if (_barkCooldown > 0f) return;
            _barkCooldown = 4f;
            if (door == _officeDoor && _overheard)
                _director.Hud.ShowBark("NoiTam", "Khoá ngoài rồi. Họ vào bằng cửa thông bên phòng Tú Bà — lối đó còn mở.", 3.4f);
        }

        // ------------------------------------------------------------------ failure & hiding

        private void OnCaught(Watchman watcher)
        {
            if (_ended || CatchScene.Playing || !_director.InExplore) return; // one catch at a time, none while a failure plays out
            _director.SetInt("c3_bi_bat", _director.GetInt("c3_bi_bat") + 1);
            if (watcher == _tuBaEyes) { if (_voices != null) StopCoroutine(_voices); FailSeenByTuBa(); return; } // she found the hiding place she saw Kiều slip into
            const string headline = "Bị người canh bắt gặp";
            const string detail = "\"Cô em đi đâu đấy? Chỗ này không phải chỗ của cô.\" — Hắn lôi bạn về hiên. Dãy nhà sau là khu cấm: canh lúc hắn quay đi, khom người, đi trong bóng hiên.";
            // By day the guard does not hurt her: he takes her by the collar and hauls her back to the veranda.
            if (_catch == null || !_catch.TryPlay(watcher, "NguoiCanh", "Cô em đi đâu đấy? Chỗ này không phải chỗ của cô.", () => _director.Fail(headline, detail)))
                _director.Fail(headline, detail);
        }

        private void OnHide(HideSpot spot)
        {
            if (_director.CameraController != null) _director.CameraController.LimitLook(spot.Inside.eulerAngles.y, spot.PeekRange);
        }

        private void OnUnhide(HideSpot spot)
        {
            if (_director.CameraController != null) _director.CameraController.ClearLookLimit();
        }

        private void OnRestored()
        {
            _director.Hud.RemindObjective();
            StopAllCoroutines();
            HideSpot.ExitAll();
            RestrictedZone.ResetPresence();
            _tuBaEyes.enabled = false;
            _tuBaEyes.Forget();
            _guard.ResetAt(0);
            var spawn = _overheard ? (_escapedOffice ? _tuBaRoomSpawn : _officeSpawn) : _director.GetBool("c3_khe_uoc") ? _officeSpawn : _kieuRoomSpawn;
            if (_director.GetBool("c3_khe_uoc") && !_director.GetBool("c3_nghe_len"))
            {
                // Caught before hiding: the voices come again, with all their warnings.
                _voicesStarted = false;
                _innerDoor.SetOpen(0f);
                _tuBa.Place(_hallSpot.position, _hallSpot.rotation);
                _ma.gameObject.SetActive(false);
            }
            _director.Player.Teleport(spawn.position, spawn.rotation);
            _director.CameraController.SnapBehindTarget(spawn.eulerAngles.y);
        }

        // ------------------------------------------------------------------ summary

        public IEnumerable<string> SummaryLines(Ink.Runtime.Story story)
        {
            var v = story.variablesState;
            yield return "Việc đã tìm hiểu: " + (int)v["c3_viec"] + " · số lần bị bắt gặp: " + (int)v["c3_bi_bat"];
            if ((bool)v["c3_khe_uoc"]) yield return "Đã đọc văn tự bán thân: bốn trăm năm mươi lạng.";
            if ((bool)v["c3_nghe_len"]) yield return "Đã nghe Mã Giám Sinh và Tú Bà chia bạc: đám cưới là một vở tuồng.";
            if ((bool)v["c3_biet_chia_khoa"]) yield return "Chìa khoá: ở thắt lưng Tú Bà. Cửa sau: trong bếp.";
            yield return "<i>Chương tiếp theo — Không còn đường lui: đêm nay người canh uống rượu.</i>";
        }

        // ------------------------------------------------------------------ helpers

        private IEnumerator Wait(float seconds)
        {
            for (float t = 0f; t < seconds; t += Time.deltaTime) yield return Exploring();
        }

        private IEnumerator Exploring()
        {
            yield return null;
            while (!_director.InExplore && !_ended) yield return null;
        }

        // ------------------------------------------------------------------ QA autopilot

        private readonly string[] _daytime = { "c3_room", "c3_girl_hong", "c3_girl_lam", "c3_makieu_bep", "c3_gate" };
        private bool _failTested;
        private bool _catchProbed;

        /// <summary>"-autopilotCatches": one real catch by day (he sees her in the office, chases, takes hold, hauls her off).</summary>
        private IEnumerator CatchProbe()
        {
            const float F = 0.35f;
            HideSpot.ExitAll();
            _director.Player.Teleport(new Vector3(11.4f, F, 14.6f), Quaternion.Euler(0f, 180f, 0f));
            _director.CameraController.SnapBehindTarget(180f);
            RestrictedZone.ResetPresence();
            _guard.EndScript(false);
            _guard.Forget();
            _guard.Actor.Place(new Vector3(11.0f, F, 11.6f), Quaternion.Euler(0f, 0f, 0f));
            yield return new WaitForSeconds(0.4f);
            CatchScene.ClipName = "A_ch3_drag";
            _guard.Perceiving = true;
            _guard.Sensitivity = 4f;
            float t = 0f;
            while (!CatchScene.Playing && (t += Time.deltaTime) < 25f) yield return null;
            if (!CatchScene.Playing) StoryAutopilot.Note("CATCH A_ch3_drag · TIMEOUT (no catch in 25 s)");
            while (CatchScene.Playing) yield return null;
            while (!_director.InExplore) yield return null;
            _guard.Sensitivity = 1f;
            _guard.Perceiving = false;
        }

        public IEnumerator AutopilotStep(bool thorough)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            if (!_director.InExplore) yield break;
            // The bot checks the chapter's logic, not its stealth (failures are forced on purpose).
            _guard.Perceiving = false;
            if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-autopilotCatches") >= 0 && !_catchProbed) { _catchProbed = true; yield return CatchProbe(); }
            if (StoryCapture.Directory != null && !_shotDay)
            {
                _shotDay = true;
                yield return Still("C3_courtyard", new Vector3(-10.5f, 0.35f, -8.0f), new Vector3(8f, 1f, 6f));
                yield return Still("C3_kieu_room", new Vector3(-12.6f, 0.35f, 4.4f), new Vector3(-20f, 1.5f, 6.2f));
                if (ViewTour.Requested) yield return ViewTour.Shoot("C3day");
            }
            if (StoryCapture.Directory != null && _dusk && !_shotDusk)
            {
                _shotDusk = true;
                yield return new WaitForSecondsRealtime(2.5f);
                yield return Still("C3_dusk", new Vector3(-10.5f, 0.35f, 1.0f), new Vector3(8f, 1.5f, 0f));
                if (ViewTour.Requested) { yield return new WaitForSecondsRealtime(4.5f); yield return ViewTour.Shoot("C3dusk"); }
            }
            var open = _daytime.Where(_director.IsExploreAvailable).ToList();
            int done = _director.GetInt("c3_viec");
            if (open.Count > 0 && (done < 2 || thorough || Random.value < 0.4f))
            {
                string id = open[Random.Range(0, open.Count)];
                var target = FindObjectsByType<ExploreTarget>().FirstOrDefault(t => t.ExploreId == id);
                if (target != null) AutoTeleport(target.transform.position + (Player.position - target.transform.position).normalized * 1.2f);
                _director.ChooseExplore(id);
                yield break;
            }
            if (!_director.GetBool("c3_khe_uoc"))
            {
                if (thorough && !_failTested)
                {
                    _failTested = true;
                    StoryAutopilot.Note("C3 TEST guard catch in the office");
                    AutoTeleport(_officeSpawn.position);
                    OnCaught(_guard);
                    yield return new WaitForSecondsRealtime(1.5f);
                    yield break;
                }
                foreach (string id in new[] { "c3_drawer", "c3_cabinet" })
                    if (_director.IsExploreAvailable(id) && Random.value < 0.5f) { AutoTeleport(_officeSpawn.position); _director.ChooseExplore(id); yield break; }
                AutoTeleport(_officeSpawn.position);
                _director.ChooseExplore("c3_box");
                yield break;
            }
            if (!_overheard)
            {
                // Hide in the archive wardrobe and wait for them.
                var spot = FindObjectsByType<HideSpot>().Where(h => h.name.Contains("Wardrobe")).OrderBy(h => Vector3.Distance(h.transform.position, _officeDesk.position)).First();
                AutoTeleport(spot.transform.position + Vector3.back * 0.8f);
                if (HideSpot.Current == null) spot.Interact();
                if (StoryCapture.Directory != null)
                {
                    // Through the louvres while the voices come: the view a hider has.
                    yield return new WaitForSecondsRealtime(4f);
                    StoryCapture.Shot("C3_wardrobe");
                }
                float wait = 40f;
                while (!_overheard && !_director.DialogueActive && (wait -= Time.unscaledDeltaTime) > 0f) yield return null;
                StoryAutopilot.Note("C3 overhear " + (_overheard || _director.DialogueActive ? "started" : "TIMEOUT"));
                yield break;
            }
            if (HideSpot.Current != null) HideSpot.Current.Exit();
            AutoTeleport(_tuBaRoomSpawn.position);
            yield return new WaitForSecondsRealtime(0.4f);
            AutoTeleport(_kieuRoomSpawn.position);
        }

        private void AutoTeleport(Vector3 position)
        {
            _director.Player.Teleport(position, _director.Player.transform.rotation);
            RestrictedZone.ResetPresence();
        }

        private bool _shotDay, _shotDusk;

        /// <summary>QA still from Kiều's eyes at a place, looking at a point (only with -autopilotCapture).</summary>
        private IEnumerator Still(string name, Vector3 at, Vector3 lookAt)
        {
            AutoTeleport(at);
            for (float t = 0f; t < 0.8f; t += Time.unscaledDeltaTime) { _director.CameraController.LookAtNow(lookAt); yield return null; }
            StoryCapture.Shot(name);
        }
    }
}
