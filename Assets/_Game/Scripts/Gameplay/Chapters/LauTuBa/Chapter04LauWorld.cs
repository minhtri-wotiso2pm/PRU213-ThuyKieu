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
    /// Chapter 4 in first person — the night in Tú Bà's house (docs/codex/CHAPTER3_4_LAU_TU_BA_DESIGN.md §5–9).
    /// Progress is a stage kept in Ink (<c>c4_buoc</c>): 0 locked in, 1 out of the room, 2 holding the keys, 3 holding the
    /// contract (J1 running), 4 J1 over, 5 J2 over. Every checkpoint saves the whole night (doors, locks, lamps, Tú Bà,
    /// the guard, chị Lam, what Kiều carries) together with the Ink state, and restores it with Kiều at a safe spot for the
    /// stage. Whether a scare has been seen is kept apart from progress, so a retry keeps the warnings but not the jolt.
    /// </summary>
    public partial class Chapter04LauWorld : MonoBehaviour, IChapterSummary, IAutopilotDriver
    {
        /// <summary>A guard's round: its stops and how long he stands at each (one stop with a long pause is a post).</summary>
        [System.Serializable]
        private sealed class Round
        {
            public Transform[] Points;
            public float[] Pauses;
        }

        private sealed class Snapshot
        {
            public int Stage;
            public string[] Items;
            public HouseDoor.Snapshot[] Doors;
            public bool[] Lamps;
            public TuBaSleeper.Sleep Sleep;
            public int TuBaPhase;
            public bool J1Done, J2Done, CorridorLit, LamFollowing;
            public int LamNode;
        }

        [Header("Story")]
        [SerializeField] private StoryDirector _director;
        [SerializeField] private ThuyKieu.Environment.NarrowNavMesh _navMesh;
        [SerializeField] private KieuInventory _inventory;
        [SerializeField] private FirstPersonFootsteps _steps;
        [Tooltip("Pebbles (Q) thrown to draw the guard at Tú Bà's door away to where they land.")]
        [SerializeField] private PebbleThrow _pebbles;
        [Tooltip("The gravel heap outside Kiều's room: she can always fill her hand again.")]
        [SerializeField] private PebblePile _pebblePile;
        [Tooltip("Kiều's own room: the one place at night no watcher enters, sees into, or catches her in.")]
        [SerializeField] private SafeZone _safeZone;

        [Header("People")]
        [SerializeField] private Watchman _guard;
        [SerializeField] private Watchman _gateGuard;
        [SerializeField] private Watchman _tuBaWatch;
        [Tooltip("Plays the grab when a watcher really reaches her, before the failure screen.")]
        [SerializeField] private CatchScene _catch;
        [SerializeField] private StoryActor _tuBa;
        [SerializeField] private TuBaSleeper _sleeper;
        [SerializeField] private KeyTheft _keyTheft;
        [SerializeField] private StoryActor _lam;
        [SerializeField] private LamCompanion _lamCompanion;
        [SerializeField] private StoryActor _maKieu;
        [Tooltip("Older scenes only: the lantern once hung beside Tú Bà. Hidden for the whole night (see HideTuBaLamp).")]
        [SerializeField] private Light _tuBaLamp;
        [SerializeField] private GameObject _mirrorFigure;

        [Header("House")]
        [SerializeField] private HouseDoor _kieuDoor;
        [SerializeField] private HouseDoor _lamDoor;
        [SerializeField] private HouseDoor _hongDoor;
        [SerializeField] private HouseDoor _tuBaDoor;
        [SerializeField] private HouseDoor _innerDoor;
        [SerializeField] private HouseDoor _officeDoor;
        [SerializeField] private HouseDoor _backDoor;
        [SerializeField] private HouseDoor[] _allDoors;
        [SerializeField] private PickupItem _hairpin;
        [SerializeField] private LampFlame[] _lamps;
        [SerializeField] private LampFlame _officeLamp;
        [SerializeField] private LampFlame _archiveLamp;
        [SerializeField] private Light[] _corridorLanterns;
        [SerializeField] private Transform _mirrorFace;
        [SerializeField] private Transform _mirrorCloth;
        [SerializeField] private Transform _crossBar;
        [SerializeField] private ExploreTarget _lamTalk;
        [Tooltip("A guest crossing behind the lit paper of the guests' room (the first look at the yard, and now and then after).")]
        [SerializeField] private ThuyKieu.Environment.PaperShadow _guestShadow;
        [Tooltip("Tú Bà getting up behind her paper door (J2 warning).")]
        [SerializeField] private ThuyKieu.Environment.PaperShadow _doorShadow;

        [Header("Places")]
        [SerializeField] private Transform _spawnRoom;
        [SerializeField] private Transform _spawnStore;
        [SerializeField] private Transform _spawnArchive;
        [SerializeField] private Transform _sapFeet;
        [SerializeField] private Transform _tuBaDoorInside;
        [SerializeField] private Transform _tuBaDoorOutside;
        [SerializeField] private Transform _kieuDoorOutside;
        [SerializeField] private Transform _hallSpot;
        [SerializeField] private Transform _officeDoorOutside;
        [SerializeField] private Transform _officeInside;
        [SerializeField] private Transform _storeSlats;
        [SerializeField] private Transform _cabinetFront;
        [SerializeField] private Transform _kitchen;
        [SerializeField] private Transform _lamRoom;
        [SerializeField] private BoxCollider _j2Zone;
        [SerializeField] private BoxCollider _tuBaRoom;
        [SerializeField] private BoxCollider _storeRoom;
        [Tooltip("Before the alarm: the guard posted aslant Tú Bà's door, looking down the way anyone would come.")]
        [SerializeField] private Round _guardPost;
        [Tooltip("After the alarm: the guard's round of the corridor past Tú Bà's room, the store and the office.")]
        [SerializeField] private Round _guardAlarm;
        [Tooltip("Before the alarm: the gate guard at the gate.")]
        [SerializeField] private Round _gatePost;
        [Tooltip("After the alarm: the gate guard's round from the gate through the hall and the yard towards the kitchen.")]
        [SerializeField] private Round _gateAlarm;

        private int _stage;
        private Snapshot _saved;
        private bool _mirrorSeen, _slamSeen, _j1Done, _j2Done, _j1Running, _j2Running, _storeCheckpoint, _corridorLit, _hongSpoke, _ended, _pebblesShown;
        private float _j1EndTime = -999f, _barkCooldown;
        private int _tuBaPhase;
        private Coroutine _scare;
        private readonly List<Watchman> _watchers = new List<Watchman>();

        private Transform Player => _director.Player.transform;

        // ------------------------------------------------------------------ setup

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
            _watchers.AddRange(new[] { _guard, _gateGuard, _tuBaWatch }.Where(w => w != null));
            foreach (var watcher in _watchers) watcher.Caught += OnCaught;
            _sleeper.Roused += OnTuBaRoused;
            _sleeper.Heard += OnTuBaHeard;
            _sleeper.HandCaught += (_, why) => TuBaCatches(why == TuBaSleeper.Grab.BreathingIn
                ? "Bạn vẫn giữ E khi mụ hít vào: tiếng ngáy kéo vào là lúc mụ cảm thấy bàn tay ở thắt lưng. Mụ chộp lấy cổ tay bạn."
                : "Mụ chưa ngủ lại mà bạn đã đặt tay lên chùm chìa. Mụ chộp lấy cổ tay bạn.",
                "Chỉ giữ E khi mụ thở ra, buông trước khi mụ hít vào (chữ chuyển vàng là sắp hết hơi thở ra). Tiếng ngáy ngừng là mụ đang cựa mình: buông tay, đứng im đến khi mụ ngáy lại. Phần chìa đã gỡ được giữ giữa các hơi thở.");
            _keyTheft.Taken += _ => TookKeys();
            _hairpin.Taken += _ => _director.Hud.ShowBark("NoiTam", "Cây trâm. Luồn qua khe cửa... có lẽ nâng được cái then.", 3.2f);
            _kieuDoor.Changed += OnKieuDoor;
            if (_pebblePile != null) _pebblePile.Taken += OnPebblesTaken;
            _lamDoor.Changed += OnLamDoor;
            _backDoor.Changed += OnBackDoor;
            _hongDoor.Refused += (_, __) => HongRefuses();
            foreach (var door in _allDoors) door.Refused += OnRefused;
            _backDoor.Gate = BackDoorGate;
            _tuBaWatch.enabled = false;
            HideTuBaLamp();
            HandLantern.Attach(_guard);
            HandLantern.Attach(_gateGuard);
            if (_mirrorFigure != null) _mirrorFigure.SetActive(false);
            // Mã Kiều asleep on her mat by the stove (she only opens her eyes when Kiều passes).
            if (_maKieu != null && !_maKieu.IsLying) _maKieu.LieDown(_maKieu.transform.position, _maKieu.transform.rotation, 0.06f);
            if (_director.Audio != null)
            {
                _director.Audio.SetAmbience("rain_house");
                StoryAutopilot.Note("C4 Foley " + (_director.Audio.HasFoley("step_wood.walk") && _director.Audio.HasFoley("door_slam") ? "recorded set loaded" : "MISSING: shared sounds"));
            }
            if (_pebbles != null) { _pebbles.CountChanged += _ => UpdateControls(); _pebbles.SetCount(_pebbles.Capacity); }
            UpdateControls();
            foreach (var light in _corridorLanterns) light.enabled = false;
            LightExposure.Enable(true);

            // A fresh night or a resumed save: the stage in Ink decides how the house looks.
            _stage = _director.GetInt("c4_buoc");
            _j1Done = _stage >= 4;
            _j2Done = _stage >= 5;
            SyncInventoryFromStory();
            _kieuDoor.SetLock(_stage >= 1 ? DoorLock.None : DoorLock.LatchedOutside);
            _lamDoor.SetLock(_director.GetBool("c4_gap_co_lam") ? DoorLock.None : DoorLock.Locked);
            _hongDoor.SetLock(DoorLock.Bolted);
            _innerDoor.SetLock(DoorLock.Bolted);
            _officeDoor.SetLock(DoorLock.Locked);
            _backDoor.SetLock(DoorLock.Locked);
            if (_stage == 0) PlaceTuBaAtKieuDoor();
            else PutTuBaToBed();
            if (_j2Done) { _tuBaPhase = 2; WakeTuBaAt(_hallSpot, false); LightCorridor(); }
            _keyTheft.Restore();
            _hairpin.Refresh();
            ArmGuard();
        }

        private void UpdateControls()
        {
            int pebbles = _pebbles != null ? _pebbles.Count : 0;
            _director.Hud.SetControls("WASD: đi · Shift: chạy · Ctrl: khom · E: tương tác (giữ E: làm khẽ) · Q: ném sỏi để dụ lính (" + pebbles + ") · Chuột phải: nhìn kỹ");
        }

        /// <summary>Both guards in their places for the stage (a fresh night, a resumed save, a probe), calm.</summary>
        private void ArmGuard()
        {
            int wariness = _director.GetInt("c4_canh_giac");
            _guard.Sensitivity = 0.85f + 0.15f * wariness;
            SetRoles(_j2Done, false);
            _guard.ResetAt(_j2Done ? FarthestWaypoint(Player.position) : 0);
            _gateGuard.ResetAt(0);
        }

        /// <summary>
        /// The guards' two arrangements. Before the alarm: the guard posted at Tú Bà's door (a pebble draws him off, he comes
        /// back), the gate guard at the gate, neither calling the other. After it: both on rounds, each calling the other to
        /// where he saw her (the other must find her himself). <paramref name="onFoot"/>: they walk to their new rounds from
        /// where they stand (the alarm itself); otherwise the round is only set (a checkpoint places them).
        /// </summary>
        private void SetRoles(bool alarm, bool onFoot)
        {
            var guard = alarm ? _guardAlarm : _guardPost;
            var gate = alarm ? _gateAlarm : _gatePost;
            if (onFoot) { _guard.Rejoin(guard.Points, guard.Pauses); _gateGuard.Rejoin(gate.Points, gate.Pauses); }
            else { _guard.SetRoute(guard.Points, guard.Pauses); _gateGuard.SetRoute(gate.Points, gate.Pauses); }
            _guard.Ally = alarm ? _gateGuard : null;
            _gateGuard.Ally = alarm ? _guard : null;
            _guard.CallsAlly = _gateGuard.CallsAlly = alarm;
        }

        /// <summary>The first handful from the heap: what pebbles are for, once.</summary>
        private void OnPebblesTaken(PebblePile pile, int taken)
        {
            if (_pebblesShown) return;
            _pebblesShown = true;
            _director.Hud.ShowBark("NoiTam", "Sỏi. Ném ra chỗ nền đá, tiếng lách cách sẽ kéo hắn khỏi cửa phòng mụ... nhưng ném ngay trước mặt hắn thì hắn thấy mình.", 5f);
        }

        private void SyncInventoryFromStory()
        {
            var items = new List<string>();
            if (_stage >= 1) items.Add("tram");
            if (_director.GetBool("c4_chia_khoa")) items.Add("chia");
            if (_director.GetBool("c4_khe_uoc_mg")) items.Add("giao_keo");
            if (_director.GetBool("c4_so_no")) items.Add("trang_no");
            _inventory.Set(items);
            if (_steps != null) _steps.RunExtra = _director.GetInt("c4_tien") == 2 ? 3f : 0f;
        }

        // ------------------------------------------------------------------ story events

        private void OnScene(string scene)
        {
            if (scene == "Dem_Night") StartCoroutine(TuBaLeavesKieuDoor());
            if (scene == "Dem_BackDoor") { _ended = true; foreach (var watcher in _watchers) watcher.Pause(true); }
        }

        private void OnTag(string key, string value)
        {
            if (key != "speaker" || _director.CameraController == null) return;
            var actor = value == "NoiTam" || value == "KieuThat" || value == "TrangSach" || value == "Narrator" || value == "Kieu" ? null : _director.FindActor(value);
            if (actor != null && actor.isActiveAndEnabled) _director.CameraController.LookTarget = actor.HeadPosition;
        }

        private void OnExplore(bool exploring)
        {
            foreach (var watcher in _watchers) watcher.Pause(!exploring || _ended);
            if (!exploring) return;
            _director.CameraController.LookTarget = null;
            _director.Hud.SetExploreOptions(null);
            SyncInventoryFromStory();
            // A conversation just ended: act on what it decided before saving.
            if (_director.GetBool("c4_gap_co_lam") && _director.GetBool("c4_ru_co_lam") && !_lamCompanion.Following) _lamCompanion.Begin();
            if (_director.GetBool("c4_khe_uoc_mg") && _stage < 3) SetStage(3);
            TakeSnapshot();
            if (_stage == 3 && !_j1Done && !_j1Running) _scare = StartCoroutine(FirstScare());
        }

        private void SetStage(int stage)
        {
            _stage = stage;
            _director.SetInt("c4_buoc", stage);
        }

        /// <summary>A checkpoint from the world (not from a story beat): Ink and the whole house are saved together.</summary>
        private void Checkpoint()
        {
            _director.SaveCheckpoint();
            TakeSnapshot();
        }

        private void OnKieuDoor(HouseDoor door)
        {
            if (_stage != 0 || door.Lock != DoorLock.None || door.MovedByNpc) return;
            SetStage(1);
            _director.Hud.SetObjective(_director.GetBool("c3_biet_chia_khoa") ? "Lấy chùm chìa ở thắt lưng Tú Bà" : "Tìm chìa khoá — Tú Bà đeo ở thắt lưng");
            _director.Hud.ShowBark("NoiTam", "Then nhấc rồi. Đi khẽ thôi... phòng mụ ở dãy nhà sau.", 3.4f);
            Checkpoint();
        }

        private void TookKeys()
        {
            _director.SetBool("c4_chia_khoa", true);
            _director.SetBool("c4_quan_sat", true);
            SetStage(2);
            _director.Hud.SetObjective("Lấy giao kèo trong tủ khoá — phòng sổ sách");
            _director.Hud.ShowBark("NoiTam", "Năm chiếc chìa. Chiếc khắc hình con cá... giống ổ khoá trên cái tủ đồng.", 3.6f);
            // Checkpoint in the dark store next door, not beside her.
            Checkpoint();
        }

        private void OnLamDoor(HouseDoor door)
        {
            if (door.MovedByNpc) return;
            if (door.Lock == DoorLock.None && _lamTalk != null) _lamTalk.Reveal(true);
        }

        private void OnBackDoor(HouseDoor door)
        {
            if (_ended || !door.IsOpen || door.MovedByNpc || !_director.GetBool("c4_khe_uoc_mg")) return;
            if (_director.IsExploreAvailable("c4_leave")) _director.ChooseExplore("c4_leave");
        }

        /// <summary>The back door's story condition, separate from its lock (E8): say plainly why it will not open yet.</summary>
        private string BackDoorGate()
        {
            if (!_director.GetBool("c4_khe_uoc_mg")) return "Chưa đi được — giao kèo còn trong tủ phòng sổ sách";
            if (_lamCompanion.Following && !_lamCompanion.Arrived) return "Đợi chị Lam tới bếp";
            return null;
        }

        private void OnRefused(HouseDoor door, string reason)
        {
            if (_barkCooldown > 0f) return;
            _barkCooldown = 4f;
            if (door == _backDoor && door.Lock == DoorLock.None && !_director.GetBool("c4_khe_uoc_mg"))
                _director.Hud.ShowBark("NoiTam", "Đi tay không thì sáng mai mình chỉ là đứa ở trốn. Phải có giao kèo trong tay.", 3.8f);
            else if (door == _kieuDoor && !_inventory.Has("tram"))
                _director.Hud.ShowBark("NoiTam", "Then ở phía ngoài. Cần một thứ gì mảnh để luồn qua khe...", 3.2f);
        }

        private void HongRefuses()
        {
            if (_hongSpoke) return;
            _hongSpoke = true;
            if (_director.Audio != null) _director.Audio.PlayAt("mumble", _hongDoor.transform.position + Vector3.up * 1.4f, 0.4f, 6f);
            _director.Hud.ShowBark("CoHong", "(thì thầm qua cửa) Đừng mở. Tao không đi đâu được. Đi đi... đừng để nó nghe.", 4.2f);
        }

        // ------------------------------------------------------------------ Tú Bà

        /// <summary>
        /// Scenes built before the fix hang a lantern on her root that floated by her arm and beside her on the sập: the
        /// whole lantern (geometry and light) is switched off once, and nothing switches it back on.
        /// </summary>
        private void HideTuBaLamp()
        {
            if (_tuBaLamp == null) return;
            _tuBaLamp.enabled = false;
            var lantern = _tuBaLamp.transform;
            if (_tuBa != null)
                while (lantern.parent != null && lantern.parent != _tuBa.transform) lantern = lantern.parent;
            // Only the lantern under her root goes; a light found anywhere else is switched off on its own.
            (_tuBa != null && lantern.parent == _tuBa.transform ? lantern : _tuBaLamp.transform).gameObject.SetActive(false);
        }

        private void PlaceTuBaAtKieuDoor()
        {
            _sleeper.GetUp();
            _tuBa.Place(_kieuDoorOutside.position, _kieuDoorOutside.rotation * Quaternion.Euler(0f, 180f, 0f));
        }

        private IEnumerator TuBaLeavesKieuDoor()
        {
            if (_stage > 0 || _tuBa.IsLying) yield break;
            _tuBa.MoveTo(_tuBaDoorOutside.position, 0.9f);
            float timeout = 25f;
            while ((_tuBa.IsMoving || timeout > 22f) && (timeout -= Time.deltaTime) > 0f) yield return null;
            PutTuBaToBed();
        }

        private void PutTuBaToBed(bool closeDoor = true)
        {
            _tuBaWatch.enabled = false;
            if (closeDoor) _tuBaDoor.SetOpen(0f);
            if (!_tuBa.IsLying) LayOnSap();
            BodyLying(true);
            _sleeper.Restore(TuBaSleeper.Sleep.Asleep);
        }

        private float _bedHeight = -1f;

        /// <summary>
        /// Lays her on the sập resting on its mat. The rig tipped onto its back hangs its body some way off its pivot, so a
        /// fixed lift left her floating above the mat (and her key ring in the air): the first time, her lowest point over
        /// the sập is measured from the posed body and the lift corrected; later times reuse it.
        /// </summary>
        private void LayOnSap()
        {
            const float first = 0.45f;
            _tuBa.LieDown(_sapFeet.position, _sapFeet.rotation, _bedHeight >= 0f ? _bedHeight : first);
            if (_bedHeight >= 0f) return;
            _bedHeight = first;
            if (!LowestOverSap(out float lowest, out float mat)) return;
            float lift = Mathf.Clamp(first - (lowest - mat), 0.05f, 0.8f);
            if (Mathf.Abs(lift - first) < 0.01f) return;
            _bedHeight = lift;
            _tuBa.StandUp(_sapFeet.position, _sapFeet.rotation);
            _tuBa.LieDown(_sapFeet.position, _sapFeet.rotation, _bedHeight);
            StoryAutopilot.Note($"C4 Tú Bà on her sập: body {(lowest - mat) * 100f:F0} cm above the mat with the old lift, lift now {_bedHeight:F2} m");
        }

        /// <summary>
        /// The lowest point of her posed body above the sập (a low percentile of the skinned vertices over it, so a stray
        /// vertex does not decide), and the top of the mat (the sập's solid plus its mattress, 2 cm, less 1 cm of give).
        /// </summary>
        private bool LowestOverSap(out float lowest, out float mat)
        {
            lowest = mat = 0f;
            Vector3 over = _sapFeet.position + _sapFeet.forward * 0.9f + Vector3.up * 1.5f;
            Collider sap = null;
            foreach (var hit in Physics.RaycastAll(over, Vector3.down, 2f, ~0, QueryTriggerInteraction.Ignore).OrderBy(h => h.distance))
                if (!hit.collider.transform.IsChildOf(_tuBa.transform)) { sap = hit.collider; mat = hit.point.y + 0.01f; break; }
            if (sap == null) return false;
            var bounds = sap.bounds;
            var heights = new List<float>();
            var baked = new Mesh();
            foreach (var skin in _tuBa.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                skin.BakeMesh(baked, true);
                var toWorld = Matrix4x4.TRS(skin.transform.position, skin.transform.rotation, Vector3.one);
                foreach (var v in baked.vertices)
                {
                    Vector3 p = toWorld.MultiplyPoint3x4(v);
                    if (p.x > bounds.min.x && p.x < bounds.max.x && p.z > bounds.min.z && p.z < bounds.max.z) heights.Add(p.y);
                }
            }
            Destroy(baked);
            if (heights.Count < 50) return false;
            heights.Sort();
            lowest = heights[(int)(heights.Count * 0.02f)];
            return true;
        }

        private void WakeTuBaAt(Transform spot, bool hurry)
        {
            _sleeper.Restore(TuBaSleeper.Sleep.Awake);
            _tuBa.StandUp(spot.position, spot.rotation);
            BodyLying(false);
            _tuBa.EnableNavigation();
            _tuBaWatch.enabled = true;
            _tuBaWatch.Perceiving = true;
            LightExposure.Enable(true);
            if (hurry) _tuBaWatch.MakeAlert(60f);
        }

        // A sound roused her. She did not see who made it: she calls the guard to where it came from and lies listening.
        // Kiều loses only if she is right at the bedside (TuBaAtBedside), never because of a door somewhere else.
        private void OnTuBaRoused(TuBaSleeper sleeper, Vector3 source)
        {
            if (_j2Running || _ended || !_director.InExplore) return;
            if (KieuAtBedside()) { TuBaCatches("Mụ mở mắt — bạn đứng ngay cạnh sập."); return; }
            _director.Hud.ShowBark("TuBa", "(quát vọng ra) Thằng Tư! Tiếng gì ngoài kia? Ra xem!", 3f);
            SendGuardTo(source);
        }

        private void OnTuBaHeard(TuBaSleeper sleeper, Vector3 source)
        {
            if (_j2Running || _ended || !_director.InExplore) return;
            if (_tuBaCallCooldown > 0f) return;
            _director.Hud.ShowBark("TuBa", "(gắt) Lại nữa! Thằng Tư đâu?!", 2.4f);
            SendGuardTo(source);
        }

        /// <summary>The guard is sent to check the place a sound came from (not where Kiều is now), unless he is chasing.</summary>
        private void SendGuardTo(Vector3 source)
        {
            _tuBaCallCooldown = 6f;
            if (_guard.State == WatchState.Chase || !_guard.Perceiving) return;
            _guard.Alarm(source);
            StoryAutopilot.Note($"C4 Tú Bà calls the guard to {source} · guard {_guard.State} → {_guard.Target}");
        }

        /// <summary>Kiều within reach of the sập in Tú Bà's room, not hidden.</summary>
        private bool KieuAtBedside()
        {
            if (_director.Player.IsHidden || !_tuBaRoom.bounds.Contains(Player.position + Vector3.up * 0.5f)) return false;
            Vector3 head = _sleeper.Head;
            Vector3 flat = Player.position - head;
            flat.y = 0f;
            return flat.magnitude < 2.4f;
        }

        /// <summary>Tú Bà catches Kiều at her bedside: she sits up and grabs her (the catch scene), then the failure.</summary>
        private void TuBaCatches(string how, string advice = null)
        {
            // Once per failure: she stays roused, with Kiều beside her, until the checkpoint is restored.
            if (_ended || _tuBaCaught || CatchScene.Playing || !_director.InExplore || _j2Running) return;
            _tuBaCaught = true;
            _director.SetInt("c4_bi_bat", _director.GetInt("c4_bi_bat") + 1);
            const string headline = "Tú Bà tỉnh giấc!";
            string detail = how + " " + (advice ?? "Khi tiếng ngáy ngừng là mụ đang cựa mình: buông tay, đứng im cho đến khi mụ ngáy lại. Chỉ nhích tay khi mụ thở ra.");
            // She comes up off the sập on Kiều's side, facing her.
            Vector3 toKieu = Player.position - _sleeper.Head;
            toKieu.y = 0f;
            Vector3 stand = new Vector3(_sleeper.Head.x, Player.position.y, _sleeper.Head.z) + toKieu.normalized * Mathf.Min(0.7f, toKieu.magnitude * 0.5f);
            _tuBa.StandUp(stand, Quaternion.LookRotation(toKieu.sqrMagnitude > 0.01f ? toKieu : Vector3.forward));
            BodyLying(false);
            _tuBa.EnableNavigation();
            _tuBaWatch.enabled = true;
            _tuBaWatch.Perceiving = false;
            if (_catch == null || !_catch.TryPlay(_tuBaWatch, "TuBa", "Con ranh! Mày mò vào đây làm gì?!", () => _director.Fail(headline, detail)))
                _director.Fail(headline, detail);
        }

        private float _tuBaCallCooldown;
        private bool _tuBaCaught;

        /// <summary>
        /// Her body collider follows her pose: upright when she stands, along the sập when she lies (an upright capsule at
        /// the foot of the bed would be an invisible post in the way of the hand reaching for the keys).
        /// </summary>
        private void BodyLying(bool lying)
        {
            var body = _tuBa.GetComponent<CapsuleCollider>();
            if (body == null) return;
            if (_bodyHeight <= 0f) { _bodyHeight = body.height; _bodyRadius = body.radius; }
            body.direction = lying ? 2 : 1;
            body.radius = lying ? 0.22f : _bodyRadius;
            body.height = _bodyHeight;
            // Lying, the root is at her feet and faces away from her head (see StoryActor.LieDown).
            body.center = lying ? new Vector3(0f, 0.45f + 0.18f, -_bodyHeight * 0.5f) : Vector3.up * _bodyHeight * 0.5f;
        }

        private float _bodyHeight, _bodyRadius;

        // ------------------------------------------------------------------ the guard

        private void OnCaught(Watchman watcher)
        {
            if (_ended || CatchScene.Playing || !_director.InExplore) return; // one catch at a time, none while a failure plays out
            if (_probing) return; // the stealth probe records catches itself
            _director.SetInt("c4_bi_bat", _director.GetInt("c4_bi_bat") + 1);
            string headline = watcher == _gateGuard ? "Gác cổng bắt gặp!" : watcher == _tuBaWatch ? "Tú Bà bắt được!" : "Bị bắt gặp!";
            string detail = "\"Đi đâu đấy, con?\" — Một bàn tay túm vai, ánh đèn soi thẳng vào mặt. Cắt tầm nhìn (góc tường, cột, rèm) rồi mới trốn: ai thấy bạn chui vào chỗ nấp sẽ tìm ra bạn.";
            // Night: the one who reaches her takes hold of her and forces her down; then the failure and the checkpoint.
            string line = watcher == _tuBaWatch ? "Con ranh! Trốn đâu cho thoát?" : watcher == _gateGuard ? "Đứng lại! Đi đâu đấy?" : "Định trốn đi đâu?";
            string speaker = watcher == _tuBaWatch ? "TuBa" : "NguoiCanh";
            if (_catch == null || !_catch.TryPlay(watcher, speaker, line, () => _director.Fail(headline, detail)))
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

        private int FarthestWaypoint(Vector3 from)
        {
            var route = (_j2Done ? _guardAlarm : _guardPost).Points;
            int best = 0;
            float bestDistance = -1f;
            for (int i = 0; i < route.Length; i++)
            {
                float distance = Vector3.Distance(route[i].position, from);
                if (distance > bestDistance) { bestDistance = distance; best = i; }
            }
            return best;
        }

        // ------------------------------------------------------------------ frame

        private void Update()
        {
            if (_barkCooldown > 0f) _barkCooldown -= Time.deltaTime;
            if (_tuBaCallCooldown > 0f) _tuBaCallCooldown -= Time.deltaTime;
            if (StoryAutopilot.Enabled) WatchBreath();
            if (_ended || !_director.InExplore || CatchScene.Playing) return;

            // Lying awake and listening, she sees whoever stands at her bedside.
            if (_sleeper.State == TuBaSleeper.Sleep.Roused && !_j2Running && KieuAtBedside()) TuBaCatches("Mụ nằm thức, nghe ngóng — và thấy bạn ngay cạnh sập.");

            // Watching her from the dark store, through the slats.
            if (!_director.GetBool("c4_quan_sat") && _stage >= 1 && Vector3.Distance(Player.position, _storeSlats.position) < 1.6f
                && Vector3.Angle(_director.CameraController.transform.forward, _sapFeet.position - _director.CameraController.transform.position) < 35f)
            {
                _director.SetBool("c4_quan_sat", true);
                _director.Hud.ShowBark("NoiTam", "Mụ ngủ trên sập, miệng còn lẩm bẩm tính tiền. Chùm chìa lủng lẳng bên hông. Mụ ngáy khi hít vào — lúc ấy phải buông tay; chỉ gỡ chìa khi mụ thở ra. Ngáy ngừng là mụ sắp tỉnh.", 6f);
            }
            if (!_storeCheckpoint && _stage == 1 && _storeRoom.bounds.Contains(Player.position))
            {
                _storeCheckpoint = true;
                Checkpoint();
            }
            if (_stage == 4 && !_j2Done && !_j2Running && SecondScareReady()) _scare = StartCoroutine(SecondScare());

            // Mã Kiều wakes when Kiều crosses the kitchen, looks at her, and turns to the wall: she has seen nothing.
            if (!_maKieuTurned && _stage >= 2 && Vector3.Distance(Player.position, _kitchen.position) < 3.2f)
            {
                _maKieuTurned = true;
                if (_director.Audio != null) _director.Audio.PlayAt("cloth", _maKieu.HeadPosition, 0.5f, 6f);
                _director.Hud.ShowBark("Narrator", "Mã Kiều mở mắt. Bà nhìn Kiều một lúc lâu... rồi quay mặt vào vách.", 4f);
            }

            // Life behind the paper: the first look at the yard catches a guest passing; then now and then.
            if (_guestShadow != null && _stage >= 1 && !_j2Done)
            {
                bool first = !_heroShadow && Vector3.Distance(Player.position, _kieuDoorOutside.position) < 2.4f;
                if (first || (_heroShadow && (_shadowTimer -= Time.deltaTime) <= 0f))
                {
                    _heroShadow = true;
                    _shadowTimer = Random.Range(30f, 55f);
                    _guestShadow.Play(first ? 2.4f : 2f);
                }
            }
        }

        private bool _heroShadow, _maKieuTurned;
        private float _shadowTimer;

        private bool SecondScareReady()
        {
            bool chased = _watchers.Any(w => w.isActiveAndEnabled && w.State == WatchState.Chase);
            // In the larger house a quick player can be in the kitchen before the minute of rest after J1 is up. Then the
            // house wakes while she is there (she hears it, the corridor lights up behind her): the back door is never held
            // shut to make her wait for it.
            bool inKitchen = Vector3.Distance(Player.position, _kitchen.position) < 6f;
            bool inZone = inKitchen || _j2Zone.bounds.Contains(Player.position + Vector3.up * 0.5f);
            float distance = inKitchen ? ScareRules.MinDistance : Vector3.Distance(Player.position, _tuBaDoorOutside.position);
            return ScareRules.SecondAllowed(_j1Done, Time.time - _j1EndTime, _director.Player.IsHidden, chased, inZone, distance,
                _tuBaRoom.bounds.Contains(Player.position + Vector3.up * 0.5f));
        }

        // ------------------------------------------------------------------ J1: the bronze mirror, then the guard at the door

        /// <summary>The J1 draught: both flames in the office and archive lean together.</summary>
        private void LeanFlames(float amount)
        {
            _officeLamp.Lean = amount;
            if (_archiveLamp != null) _archiveLamp.Lean = amount;
        }

        private IEnumerator FirstScare()
        {
            _j1Running = true;
            _director.Hud.SetObjective("Mang giao kèo ra cửa sau trong bếp");
            // Warning: the flame leans, the cloth on the mirror slips, the rain goes quiet.
            if (_director.Audio != null) _director.Audio.DuckAmbience(0.55f);
            for (float t = 0f; t < 2f; t += Time.deltaTime) { LeanFlames(t / 2f); yield return Exploring(); }
            if (!_mirrorSeen)
            {
                if (_director.Audio != null) _director.Audio.PlayAt("cloth", _mirrorFace.position, 0.6f, 6f);
                Vector3 clothStart = _mirrorCloth.localPosition;
                _mirrorCloth.localPosition = clothStart + Vector3.down * 0.02f;
                // Only if the mirror comes into view within ten seconds; otherwise go straight to the guard.
                float wait = 10f;
                while (wait > 0f && !MirrorInView()) { wait -= Time.deltaTime; yield return Exploring(); }
                if (wait > 0f)
                {
                    _mirrorSeen = true;
                    yield return MirrorScare(clothStart);
                }
            }
            LeanFlames(0.25f);
            yield return new WaitForSeconds(1f);
            // The real danger: the guard comes off his round to the office door.
            if (_director.Audio != null) _director.Audio.DuckAmbience(1f);
            bool arrived = false;
            _guard.Script(_officeDoorOutside.position, 1.25f, () => arrived = true);
            float timeout = 45f;
            while (!arrived && _guard.State == WatchState.Scripted && (timeout -= Time.deltaTime) > 0f) yield return Exploring();
            if (arrived)
            {
                if (_director.Audio != null) _director.Audio.PlayAt("lock_rattle", _officeDoor.transform.position + Vector3.up, 0.9f, 14f);
                _sleeper.Stir(4f); // the rattle carries next door: she stirs, she does not wake
                yield return new WaitForSeconds(1.2f);
                if (_officeDoor.Lock != DoorLock.None)
                {
                    _director.Hud.ShowBark("NguoiCanh", _officeLamp.Lit ? "(lẩm bẩm) Bà lại quên tắt đèn... Kệ." : "(lẩm bẩm) Ngủ cả rồi.", 2.8f);
                    yield return new WaitForSeconds(2f);
                    _guard.EndScript();
                }
                else
                {
                    _officeDoor.SetOpen(1f, 0.6f, "door_fast");
                    _director.Hud.ShowBark("NguoiCanh", "Ai ở trong đấy?", 2f);
                    yield return new WaitForSeconds(0.6f);
                    if (_guard.State != WatchState.Chase) _guard.Investigate(_officeInside.position, 6f);
                }
            }
            // Over once he is back on his round and away from the door.
            while (!(_guard.State == WatchState.Patrol || _guard.State == WatchState.Return) || Vector3.Distance(_guard.transform.position, _officeDoorOutside.position) < 6f)
                yield return Exploring();
            LeanFlames(0f);
            _j1Running = false;
            _j1Done = true;
            _j1EndTime = Time.time;
            SetStage(4);
            Checkpoint();
        }

        private IEnumerator MirrorScare(Vector3 clothStart)
        {
            // The cloth falls; in the hazy bronze a figure in Kiều's clothes stands right behind her, looking at the door.
            // Stand it on the ray the bronze reflects through its centre, a little further than Kiều: from where she looks,
            // it fills the middle of the disc, just over her shoulder. Kept clear of walls.
            Vector3 eye = _director.CameraController.transform.position;
            Vector3 centre = _mirrorFace.position;
            Vector3 reflected = Vector3.Reflect((centre - eye).normalized, _mirrorFace.forward);
            reflected.y = 0f;
            reflected.Normalize();
            float reach = Vector3.Distance(eye, centre) + 0.4f;
            Vector3 floorCentre = new Vector3(centre.x, Player.position.y + 1f, centre.z);
            if (Physics.Raycast(floorCentre, reflected, out var wall, reach + 0.3f, 1 << 8, QueryTriggerInteraction.Ignore)) reach = Mathf.Max(0.6f, wall.distance - 0.3f);
            Vector3 behind = floorCentre + reflected * reach;
            behind.y = Player.position.y;
            _mirrorFigure.transform.SetPositionAndRotation(behind, Quaternion.LookRotation(_officeDoorOutside.position - behind));
            _mirrorFigure.SetActive(true);
            if (_director.Audio != null) _director.Audio.PlaySfx("string_snap", 1f);
            if (_director.Fx != null) _director.Fx.Shake(0.12f, 0.2f);
            for (float t = 0f; t < 0.3f; t += Time.deltaTime)
            {
                _mirrorCloth.localPosition = clothStart + Vector3.down * (0.02f + t * 2.6f);
                yield return null;
            }
            _mirrorCloth.gameObject.SetActive(false);
            // The bare bronze holds the figure for a breath, then only Kiều is there.
            yield return new WaitForSeconds(0.6f);
            _mirrorFigure.SetActive(false);
            _director.Hud.ShowBark("KieuThat", "Có người.", 2.4f);
        }

        private bool MirrorInView()
        {
            var camera = _director.CameraController.transform;
            Vector3 to = _mirrorFace.position - camera.position;
            if (to.magnitude > 5f) return false;
            return Vector3.Angle(camera.forward, to) < 40f && Vector3.Dot(_mirrorFace.forward, -to) > 0f;
        }

        // ------------------------------------------------------------------ J2: Tú Bà finds the keys gone

        private IEnumerator SecondScare()
        {
            _j2Running = true;
            bool firstTime = !_slamSeen;
            // Warnings: the gate guard calls her from the hall, the snoring stops, the bed creaks, she mutters, light behind the paper.
            _director.Hud.ShowBark("NguoiCanh", "(vọng từ sảnh) Bà ơi! Khách quan về!", 2.8f);
            if (_director.Audio != null) _director.Audio.PlayAt("knock", _hallSpot.position + Vector3.up, 0.8f, 30f);
            yield return Wait(1.6f);
            _sleeper.WakeUp(); // silence: the snoring is gone
            yield return Wait(1.3f);
            if (_director.Audio != null) { _director.Audio.PlayAt("chair_creak", _sapFeet.position + Vector3.up * 0.6f, 0.8f, 14f); _director.Audio.PlayAt("bracelet", _sapFeet.position + Vector3.up, 0.7f, 10f); }
            _director.Hud.ShowBark("TuBa", "...Chìa... Chìa của ta đâu?", 2.6f);
            yield return Wait(1.2f);
            // Light behind the paper door, then her shadow crossing it.
            _tuBa.StandUp(_tuBaDoorInside.position, _tuBaDoorInside.rotation);
            BodyLying(false);
            if (_doorShadow != null) _doorShadow.Play(1.2f);
            yield return Wait(1.1f);
            // The slam (only the first time; a retry finds her already standing in the doorway, still shouting).
            if (firstTime)
            {
                _slamSeen = true;
                _tuBaDoor.SetOpen(1f, 0.12f, "slide_slam");
                if (_director.Audio != null) _director.Audio.PlaySfx("sting_low", 0.9f);
                if (_director.Fx != null) _director.Fx.Shake(0.18f, 0.25f);
            }
            else _tuBaDoor.SetOpen(1f, 0.6f, "slide_fast");
            _tuBa.Place(_tuBaDoorOutside.position, _tuBaDoorOutside.rotation);
            _director.Hud.ShowBark("TuBa", "Thằng Tư! Chìa của tao đâu?! Đứa nào vào phòng tao?!", 3.4f);
            WakeTuBaAt(_tuBaDoorOutside, true);
            // A moment of startle before she can make anyone out.
            _tuBaWatch.Perceiving = false;
            yield return Wait(1.5f);
            _tuBaWatch.Perceiving = true;
            // Only now, with the alarm really raised, do both guards leave their posts for their rounds, on foot from where
            // they stand. A chase in progress goes on.
            SetRoles(true, true);
            _guard.MakeAlert(60f);
            _gateGuard.MakeAlert(60f);
            if (_guard.State != WatchState.Chase) _guard.Investigate(_tuBaDoorOutside.position + Vector3.back * 1.5f, 4f);
            LightCorridor();
            // She goes to check the new girl's room first.
            _tuBaPhase = 1;
            bool arrived = false;
            _tuBaWatch.Script(_kieuDoorOutside.position, 1.0f, () => arrived = true);
            float timeout = 40f;
            while (!arrived && (timeout -= Time.deltaTime) > 0f) yield return Exploring();
            _j2Done = true;
            _j2Running = false;
            SetStage(5);
            _director.Hud.SetObjective("Về bếp — cửa sau");
            Checkpoint();
            yield return TuBaAtKieuDoor();
        }

        private IEnumerator TuBaAtKieuDoor()
        {
            _tuBaWatch.EndScript(false);
            _tuBa.FaceInstant(_kieuDoorOutside.position + Vector3.left);
            _director.Hud.ShowBark("TuBa", "(gào) Con Kiều! Con ranh đâu rồi?! Người đâu!", 3.4f);
            // She shouts at the door from outside (the room is Kiều's: she does not go in) and soon goes off to the hall: a
            // warning, not a door held shut.
            yield return Wait(12f);
            _tuBaPhase = 2;
            _tuBaWatch.Script(_hallSpot.position, 1.0f, null);
        }

        private void LightCorridor()
        {
            _corridorLit = true;
            foreach (var light in _corridorLanterns) light.enabled = true;
            LightExposure.Enable(true);
        }

        private IEnumerator Wait(float seconds)
        {
            for (float t = 0f; t < seconds; t += Time.deltaTime) yield return Exploring();
        }

        /// <summary>Waits a frame, and keeps waiting while a conversation or a menu holds the world still.</summary>
        private IEnumerator Exploring()
        {
            yield return null;
            while (!_director.InExplore && !_ended) yield return null;
        }

        // ------------------------------------------------------------------ checkpoints

        private void TakeSnapshot()
        {
            _saved = new Snapshot
            {
                Stage = _stage,
                Items = _inventory.Items.ToArray(),
                Doors = _allDoors.Select(d => d.Save()).ToArray(),
                Lamps = _lamps.Select(l => l.Lit).ToArray(),
                Sleep = _sleeper.State == TuBaSleeper.Sleep.Awake ? TuBaSleeper.Sleep.Awake : TuBaSleeper.Sleep.Asleep,
                TuBaPhase = _tuBaPhase,
                J1Done = _j1Done,
                J2Done = _j2Done,
                CorridorLit = _corridorLit,
                LamFollowing = _lamCompanion.Following,
                LamNode = _lamCompanion.Node
            };
        }

        private void OnRestored()
        {
            _director.Hud.RemindObjective();
            if (_scare != null) StopCoroutine(_scare);
            StopAllCoroutines();
            _j1Running = _j2Running = false;
            _tuBaCallCooldown = 0f;
            _tuBaCaught = false;
            HideSpot.ExitAll();
            RestrictedZone.ResetPresence();
            var s = _saved;
            if (s == null) return;
            _stage = s.Stage;
            _director.SetInt("c4_buoc", _stage);
            _inventory.Set(s.Items);
            _hairpin.Refresh();
            _keyTheft.Restore();
            if (_pebbles != null && _pebbles.Count < 2) _pebbles.SetCount(2);
            for (int i = 0; i < _allDoors.Length; i++) _allDoors[i].Load(s.Doors[i]);
            for (int i = 0; i < _lamps.Length; i++) _lamps[i].SetLit(s.Lamps[i]);
            LeanFlames(0f);
            if (_mirrorFigure != null) _mirrorFigure.SetActive(false);
            if (_mirrorSeen) _mirrorCloth.gameObject.SetActive(false);
            _j1Done = s.J1Done;
            _j2Done = s.J2Done;
            // A fresh minute of rest after a retry, rather than a scare the moment she stands up.
            if (_j1Done) _j1EndTime = Time.time;
            _corridorLit = s.CorridorLit;
            foreach (var light in _corridorLanterns) light.enabled = _corridorLit;
            _tuBaPhase = s.TuBaPhase;
            _tuBaWatch.EndScript(false);
            if (s.Sleep == TuBaSleeper.Sleep.Awake)
            {
                WakeTuBaAt(_tuBaPhase >= 2 ? _hallSpot : _kieuDoorOutside, false);
                if (_tuBaPhase == 1) StartCoroutine(TuBaAtKieuDoor());
            }
            else PutTuBaToBed(false);
            if (_lamTalk != null) _lamTalk.Reveal(_lamDoor.Lock == DoorLock.None && !_director.GetBool("c4_gap_co_lam"));
            _lamCompanion.Restore(s.LamFollowing, s.LamNode);
            if (!s.LamFollowing) _lam.Place(_lamRoom.position, _lamRoom.rotation);
            var spawn = SpawnFor(_stage);
            _director.Player.Teleport(spawn.position, spawn.rotation);
            _director.CameraController.SnapBehindTarget(spawn.eulerAngles.y);
            if (_safeZone != null) _safeZone.Refresh();
            // Back to the stage's arrangement: the post and the gate before the alarm, the two rounds after it (placed, calm).
            SetRoles(_j2Done, false);
            _guard.ResetAt(_j2Done ? FarthestWaypoint(spawn.position) : 0);
            _gateGuard.ResetAt(0);
            SyncInventoryFromStory();
            LightExposure.Enable(true);
            // J1 interrupted: it plays again (without the mirror if it was seen), warnings and all.
            if (_stage == 3 && !_j1Done) _scare = StartCoroutine(FirstScareAfterRestore());
        }

        private IEnumerator FirstScareAfterRestore()
        {
            while (!_director.InExplore) yield return null;
            yield return FirstScare();
        }

        private Transform SpawnFor(int stage)
        {
            if (stage <= 1) return _spawnRoom;
            if (stage == 2 || stage >= 5) return _spawnStore;
            return _spawnArchive;
        }

        // ------------------------------------------------------------------ summary

        public IEnumerable<string> SummaryLines(Ink.Runtime.Story story)
        {
            var v = story.variablesState;
            int money = (int)v["c4_tien"], paper = (int)v["c4_van_tu"];
            yield return "Số lần bị bắt gặp: " + (int)v["c4_bi_bat"];
            yield return "Mang theo giao kèo Mã Giám Sinh – Tú Bà: bằng chứng buôn người.";
            yield return paper == 1 ? "Văn tự bán thân: cất trong áo làm bằng chứng." : paper == 2 ? "Văn tự bán thân: đã đốt thành tro." : "Văn tự bán thân: để lại chỗ cũ để Tú Bà chậm phát hiện.";
            yield return money == 2 ? "Lấy cả túi bạc có dấu \"T.\" — bằng chứng, nhưng nặng và kêu khi chạy." : money == 1 ? "Chỉ lấy vài lạng làm lộ phí." : "Không lấy một đồng nào của Tú Bà.";
            if ((bool)v["c4_so_no"]) yield return "Đã xé trang sổ nợ của mình.";
            yield return (bool)v["c4_ru_co_lam"] ? "Chị Lam đi cùng bạn." : "Bạn đi một mình. Chị Lam ở lại.";
            yield return "<i>Chương tiếp theo — Đào thoát: ngõ sau, chó, và đèn lồng...</i>";
        }

        // ------------------------------------------------------------------ QA autopilot

        private int _autoFailures;

        /// <summary>
        /// Plays the night in order: latch, keys, contract (hidden through J1), J2, chị Lam (on the thorough run or at
        /// random), the back door. It exercises the chapter's logic and checkpoints, not the player's stealth.
        /// </summary>
        public IEnumerator AutopilotStep(bool thorough)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            if (!_director.InExplore || _watchingAlarm && _j2Done) yield break;
            // The bot checks the chapter's logic, not its stealth: the guards do not see it (failures are forced on purpose).
            _guard.Perceiving = false;
            _gateGuard.Perceiving = false;
            if (ViewTour.Requested && !_shotViews) { _shotViews = true; yield return ViewTour.Shoot("C4night"); }
            if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-autopilotStealth") >= 0 && !_probed) { _probed = true; yield return StealthProbe(); }
            if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-autopilotCatches") >= 0 && !_catchesProbed) { _catchesProbed = true; yield return CatchProbe(); }
            if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-autopilotSound") >= 0 && !_soundProbed) { _soundProbed = true; yield return SoundProbe(); }
            if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-autopilotChase") >= 0 && !_chaseProbed) { _chaseProbed = true; yield return ChaseProbe(); }
            if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-autopilotGuards") >= 0 && !_guardsProbed) { _guardsProbed = true; yield return GuardProbe(); }
            switch (_stage)
            {
                case 0:
                    if (!_inventory.Has("tram")) _hairpin.Interact();
                    AutoTeleport(_kieuDoorOutside.position + Vector3.left * 1.9f);
                    _kieuDoor.SetLock(DoorLock.None);
                    OnKieuDoor(_kieuDoor);
                    _kieuDoor.SetOpen(1f);
                    break;
                case 1:
                    if (StoryCapture.Directory != null && !_shotHero)
                    {
                        _shotHero = true;
                        yield return Still("C4_hero", _kieuDoorOutside.position + Vector3.left * 0.75f, new Vector3(8f, 1.2f, -3f));
                        // She is still on her way to bed right after the latch: frame her once she lies on the sập.
                        for (float bedWait = 30f; !_tuBa.IsLying && (bedWait -= Time.unscaledDeltaTime) > 0f;) yield return null;
                        yield return Still("C4_slats", _storeSlats.position + Vector3.left * 0.5f, _sapFeet.position + Vector3.forward * 0.8f + Vector3.up * 0.5f);
                    }
                    AutoTeleport(_storeSlats.position);
                    yield return new WaitForSecondsRealtime(0.4f);
                    if (thorough && _autoFailures == 0)
                    {
                        _autoFailures++;
                        StoryAutopilot.Note("C4 TEST guard catch at stage 1");
                        OnCaught(_guard);
                        yield return new WaitForSecondsRealtime(1.5f);
                        break;
                    }
                    _innerDoor.SetLock(DoorLock.None);
                    // The keys through the real minigame (the probe tests every way of getting it wrong first).
                    if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-autopilotKeys") >= 0 && !_keysProbed) { _keysProbed = true; yield return KeyProbe(); break; }
                    yield return AutoTakeKeys();
                    break;
                case 2:
                    _innerDoor.SetLock(DoorLock.None);
                    _innerDoor.SetOpen(1f);
                    if (!_director.GetBool("c4_so_no") && Random.value < 0.5f && _director.IsExploreAvailable("c4_ledger"))
                    {
                        AutoTeleport(_officeInside.position);
                        _director.ChooseExplore("c4_ledger");
                        break;
                    }
                    AutoTeleport(_cabinetFront.position);
                    _director.ChooseExplore("c4_cabinet");
                    break;
                case 3:
                    if (StoryCapture.Directory != null && !_mirrorSeen)
                    {
                        // Look at the mirror so the scare fires, and keep the frame.
                        AutoTeleport(_cabinetFront.position);
                        _director.CameraController.LookAtNow(_mirrorFace.position);
                        float look = 14f;
                        while (!(_mirrorFigure.activeSelf && !_mirrorCloth.gameObject.activeSelf) && (look -= Time.unscaledDeltaTime) > 0f) { _director.CameraController.LookAtNow(_mirrorFace.position); yield return null; }
                        StoryCapture.Shot("C4_J1_mirror");
                    }
                    // Hide in the archive wardrobe through J1.
                    var wardrobe = FindObjectsByType<HideSpot>().OrderBy(h => Vector3.Distance(h.transform.position, _cabinetFront.position)).First();
                    if (HideSpot.Current == null) wardrobe.Interact();
                    float wait = 90f;
                    while (!_j1Done && (wait -= Time.unscaledDeltaTime) > 0f) yield return null;
                    if (HideSpot.Current != null) HideSpot.Current.Exit();
                    StoryAutopilot.Note("C4 J1 " + (_j1Done ? "done" : "TIMEOUT") + " · mirror seen " + _mirrorSeen);
                    break;
                case 4:
                    _j1EndTime = Mathf.Min(_j1EndTime, Time.time - ScareRules.RestAfterFirst);
                    AutoTeleport(_tuBaDoorOutside.position + Vector3.left * 5f);
                    float wait2 = 60f;
                    bool shot = StoryCapture.Directory == null;
                    while (!_j2Done && (wait2 -= Time.unscaledDeltaTime) > 0f)
                    {
                        if (!shot) _director.CameraController.LookAtNow(_tuBaDoorOutside.position + Vector3.up * 1.4f);
                        if (!shot && _slamSeen) { shot = true; yield return new WaitForSecondsRealtime(0.15f); StoryCapture.Shot("C4_J2_slam"); }
                        yield return null;
                    }
                    StoryAutopilot.Note("C4 J2 " + (_j2Done ? "done" : "TIMEOUT") + " · Tú Bà phase " + _tuBaPhase + $" · after J2: {_sleeper.State}, breathing {_sleeper.Breathing}, snore {_sleeper.Snoring} · {BreathWatch()}");
                    if (_j2Done && (_sleeper.Breathing || _sleeper.Snoring || _breathWrong > 0)) StoryAutopilot.Problem("[Keys] breath after J2 or while not asleep in bed: " + BreathWatch());
                    break;
                default:
                    bool freeLam = !_director.GetBool("c4_gap_co_lam") && (thorough || Random.value < 0.5f);
                    if (freeLam)
                    {
                        _lamDoor.SetLock(DoorLock.None);
                        OnLamDoor(_lamDoor);
                        _lamDoor.SetOpen(1f);
                        AutoTeleport(_lamRoom.position + Vector3.right * 1.2f);
                        _director.ChooseExplore("c4_colam");
                        break;
                    }
                    if (_lamCompanion.Following && !_lamCompanion.Arrived)
                    {
                        AutoTeleport(_kitchen.position);
                        float wait3 = 70f;
                        while (!_lamCompanion.Arrived && (wait3 -= Time.unscaledDeltaTime) > 0f) yield return null;
                        StoryAutopilot.Note("C4 Lam " + (_lamCompanion.Arrived ? "reached the kitchen" : "TIMEOUT"));
                    }
                    AutoTeleport(_kitchen.position);
                    _backDoor.SetLock(DoorLock.None);
                    _backDoor.SetOpen(1f);
                    break;
            }
            yield return new WaitForSecondsRealtime(0.3f);
        }

        // ------------------------------------------------------------------ QA: stealth with the guard's eyes open

        private bool _probing, _probed, _probeCaught, _catchesProbed;

        /// <summary>
        /// "-autopilotCatches": real catches, one after another, through the real chain (the guard perceives, sees her,
        /// chases, reaches her, the catch plays, the chapter fails and restores the checkpoint): from the front standing
        /// and crouched, from behind, found behind Lam's screen and in the archive wardrobe, two guards at once, and close
        /// to a wall. Each is noted (kind, catcher, slowest frame); with -autopilotCatchClips each is recorded.
        /// </summary>
        private IEnumerator CatchProbe()
        {
            float F = 0.35f;
            var lamScreen = FindObjectsByType<HideSpot>().First(h => h.name == "Hide_LamScreen");
            var wardrobe = FindObjectsByType<HideSpot>().First(h => h.name == "Hide_ArchiveWardrobe");
            int catchesBefore = CatchScene.Count;

            IEnumerator Case(string name, Vector3 her, float herYaw, bool crouch, Vector3 him, float hisYaw, Watchman second = null, Vector3 secondAt = default)
            {
                while (!_director.InExplore) yield return null;
                HideSpot.ExitAll();
                _director.Player.ForceCrouch = crouch;
                _director.Player.Teleport(her, Quaternion.Euler(0f, herYaw, 0f));
                _director.CameraController.SnapBehindTarget(herYaw);
                RestrictedZone.ResetPresence();
                foreach (var watcher in new[] { _guard, second })
                {
                    if (watcher == null) continue;
                    watcher.EndScript(false);
                    watcher.Forget();
                    watcher.Actor.Place(watcher == _guard ? him : secondAt, Quaternion.Euler(0f, hisYaw, 0f));
                    watcher.Pause(true);
                }
                yield return new WaitForSeconds(0.4f);
                CatchScene.ClipName = name;
                foreach (var watcher in new[] { _guard, second })
                {
                    if (watcher == null) continue;
                    watcher.Perceiving = true;
                    watcher.Sensitivity = 4f;
                    watcher.Pause(false);
                }
                float t = 0f;
                while (!CatchScene.Playing && (t += Time.deltaTime) < 25f) yield return null;
                if (!CatchScene.Playing) StoryAutopilot.Note($"CATCH {name} · TIMEOUT (no catch in 25 s)");
                while (CatchScene.Playing) yield return null;
                while (!_director.InExplore) yield return null;
                foreach (var watcher in new[] { _guard, second }) if (watcher != null) { watcher.Sensitivity = 1f; watcher.Perceiving = false; }
                _director.Player.ForceCrouch = false;
                yield return new WaitForSeconds(0.3f);
            }

            IEnumerator HideCase(string name, HideSpot spot, Vector3 her, float herYaw, Vector3 him, float hisYaw)
            {
                while (!_director.InExplore) yield return null;
                HideSpot.ExitAll();
                RestrictedZone.ResetPresence();
                _guard.EndScript(false);
                _guard.Forget();
                _guard.Actor.Place(him, Quaternion.Euler(0f, hisYaw, 0f));
                _guard.Perceiving = true;
                _guard.Sensitivity = 2f;
                // As in the stealth probe: he walks to where he stands looking in, and only then does she step into view.
                // He must see her slip in to know where she is.
                bool there = false;
                _guard.Script(him, 1.4f, () => there = true);
                float t = 0f;
                while (!there && (t += Time.deltaTime) < 10f) yield return null;
                _director.Player.Teleport(her, Quaternion.Euler(0f, herYaw, 0f));
                _director.CameraController.SnapBehindTarget(herYaw);
                t = 0f;
                float peak = 0f;
                while (_guard.Awareness < Watchman.InvestigateAt && (t += Time.deltaTime) < 12f)
                {
                    if (_guard.State == WatchState.Scripted) _guard.transform.rotation = Quaternion.Euler(0f, hisYaw, 0f);
                    peak = Mathf.Max(peak, _guard.Awareness);
                    yield return null;
                }
                float seen = _guard.Awareness;
                bool saw = _guard.SeesPlayer;
                CatchScene.ClipName = name;
                if (!CatchScene.Playing) spot.Interact();
                bool knew = _guard.KnownHide == spot;
                t = 0f;
                while (!CatchScene.Playing && (t += Time.deltaTime) < 25f) yield return null;
                if (!CatchScene.Playing)
                {
                    CatchScene.ClipName = null;
                    StoryAutopilot.Note($"CATCH {name} · TIMEOUT · reached his spot {there} · awareness when she hid {seen:F2} (peak {peak:F2}) · saw her {saw} · knew the place {knew} · now {_guard.State} at {_guard.transform.position}");
                }
                while (CatchScene.Playing) yield return null;
                while (!_director.InExplore) yield return null;
                _guard.Sensitivity = 1f;
                _guard.Perceiving = false;
                yield return new WaitForSeconds(0.3f);
            }

            // East veranda by the lantern: he comes up the veranda from the north.
            yield return Case("A_front_standing", new Vector3(10.2f, F, -6.0f), 0f, false, new Vector3(10.4f, F, -2.6f), 180f);
            yield return Case("A_front_crouched", new Vector3(10.2f, F, -6.0f), 0f, true, new Vector3(10.4f, F, -2.6f), 180f);
            yield return Case("B_rear", new Vector3(10.2f, F, -6.0f), 180f, false, new Vector3(10.4f, F, -2.6f), 180f);
            yield return Case("B_rear_crouched", new Vector3(10.2f, F, -6.0f), 180f, true, new Vector3(10.4f, F, -2.6f), 180f);
            // Along the corridor, the wall at her shoulder.
            yield return Case("A_front_wall", new Vector3(-6.5f, F, 10.75f), 90f, false, new Vector3(-3.2f, F, 10.6f), 270f);
            // Two guards from two sides: one catch plays, one failure.
            int before = CatchScene.Count;
            yield return Case("A_two_guards", new Vector3(10.2f, F, -6.0f), 0f, false, new Vector3(10.4f, F, -2.6f), 180f, _gateGuard, new Vector3(7.2f, F, -7.8f));
            StoryAutopilot.Note($"CATCH two guards · catch scenes played {CatchScene.Count - before} (one expected)");
            // Found: behind chị Lam's screen (screen shoved aside), in the archive wardrobe (doors pulled open).
            _lamDoor.SetLock(DoorLock.None);
            _lamDoor.SetOpen(1f);
            // She stands at the screen when she slips behind it (where he will come to pull her out), as play requires.
            yield return HideCase("C_screen", lamScreen, new Vector3(-19.0f, F, -7.4f), 270f, new Vector3(-15.5f, F, -3.5f), 222f);
            yield return HideCase("C_wardrobe", wardrobe, new Vector3(16.1f, F, 17.55f), 0f, new Vector3(17.6f, F, 13.6f), 330f);
            // Low cover: reached over (the jars on the south-west veranda, the cloth heap in the store); the archive screen
            // shoved aside. Each from where she stood when she hid.
            var jars = FindObjectsByType<HideSpot>().First(h => h.name == "Hide_Jars");
            var heap = FindObjectsByType<HideSpot>().First(h => h.name == "Hide_ClothHeap");
            var archiveScreen = FindObjectsByType<HideSpot>().First(h => h.name == "Hide_ArchiveScreen");
            yield return HideCase("C_jars", jars, new Vector3(-10.6f, F, -8.0f), 225f, new Vector3(-10.5f, F, -3.5f), 180f);
            yield return HideCase("C_cloth", heap, new Vector3(-8.2f, F, 16.0f), 270f, new Vector3(-5.0f, F, 16.0f), 270f);
            yield return HideCase("C_archive_screen", archiveScreen, new Vector3(18.45f, F, 11.95f), 270f, new Vector3(14.5f, F, 12.0f), 90f);
            // At the foot of the yard steps, and with her back to her own door.
            yield return Case("A_front_steps", new Vector3(-8.4f, 0f, 1f), 90f, false, new Vector3(-5.0f, 0f, 1f), 270f);
            Vector3 door = _kieuDoorOutside.position;
            yield return Case("A_front_door", door, 180f, false, door + new Vector3(0f, 0f, -3.4f), 0f);

            // Torn down mid-catch: everything handed back at once, without the failure.
            while (!_director.InExplore) yield return null;
            var camera = _director.CameraController.GetComponent<Camera>();
            float fov = camera.fieldOfView, scale = Time.timeScale;
            HideSpot.ExitAll();
            _director.Player.Teleport(new Vector3(10.2f, F, -6.0f), Quaternion.identity);
            _director.CameraController.SnapBehindTarget(0f);
            RestrictedZone.ResetPresence();
            _guard.EndScript(false);
            _guard.Forget();
            _guard.Actor.Place(new Vector3(10.4f, F, -2.6f), Quaternion.Euler(0f, 180f, 0f));
            _guard.Perceiving = true;
            _guard.Sensitivity = 4f;
            for (float t = 0f; !CatchScene.Playing && t < 25f; t += Time.deltaTime) yield return null;
            bool started = CatchScene.Playing;
            for (float t = 0f; t < 0.5f; t += Time.unscaledDeltaTime) yield return null;
            _catch.Abort();
            _guard.Perceiving = false;
            _guard.Forget();
            var grab = _guard.Actor.GetComponent<GrabPose>();
            bool clean = !CatchScene.Playing && _director.CameraController.enabled && Mathf.Approximately(camera.fieldOfView, fov) && Mathf.Approximately(Time.timeScale, scale)
                         && !_director.Player.ControlLocked && _director.Hud.FadeAlpha < 0.01f && (grab == null || !grab.enabled) && _director.InExplore;
            StoryAutopilot.Note($"CATCH abort mid-catch · {(started && clean ? "ok" : "FAIL")} · started {started} · playing {CatchScene.Playing} · camera {_director.CameraController.enabled} · FOV {camera.fieldOfView:F0}/{fov:F0} · time {Time.timeScale:F1}/{scale:F1} · controls free {!_director.Player.ControlLocked} · fade {_director.Hud.FadeAlpha:F2} · pose off {grab == null || !grab.enabled}");
            if (!(started && clean)) StoryAutopilot.Problem("[CatchProbe] abort did not hand everything back");
            _guard.Sensitivity = 1f;
            StoryAutopilot.Note($"CATCH probe done · {CatchScene.Count - catchesBefore} catch scenes");
        }

        /// <summary>
        /// "-autopilotStealth": live checks of the stealth rules with the night guard perceiving, using his real state
        /// machine, lights, walls and hiding places. Being caught is recorded, not played out. A broken rule is logged as
        /// an error, which fails the autopilot run.
        /// </summary>
        private IEnumerator StealthProbe()
        {
            _probing = true;
            void Caught(Watchman _) => _probeCaught = true;
            _guard.Caught += Caught;
            _guard.Perceiving = true;
            var lamScreen = FindObjectsByType<HideSpot>().First(h => h.name == "Hide_LamScreen");
            float F = 0.35f;
            IEnumerator Reset()
            {
                HideSpot.ExitAll();
                _director.Player.ForceCrouch = false;
                _guard.EndScript(false);
                _guard.ResetAt(0);
                _guard.Forget();
                _probeCaught = false;
                yield return new WaitForSeconds(0.5f);
            }
            void Report(string test, bool ok, string detail)
            {
                StoryAutopilot.Note("STEALTH " + (ok ? "ok   " : "FAIL ") + test + " · " + detail);
                if (!ok) StoryAutopilot.Problem("[StealthProbe] " + test + " · " + detail);
            }
            IEnumerator Stand(Vector3 feet, float yaw, bool crouch)
            {
                _director.Player.Teleport(feet, Quaternion.Euler(0f, yaw, 0f));
                _director.Player.ForceCrouch = crouch;
                RestrictedZone.ResetPresence();
                yield return new WaitForSeconds(0.3f);
            }

            // Cases 1–2 need a guard who walks a round: before the alarm he only keeps his post (§19), so they use the round he
            // walks after it (the corridor past Tú Bà's door, the office, down the east veranda).
            _guard.SetRoute(_guardAlarm.Points, _guardAlarm.Pauses);

            // 1. Standing in lantern light beside his round: he must see her and catch her within one pass.
            yield return Reset();
            yield return Stand(new Vector3(10.0f, F, 7.0f), 0f, false);
            float light = LightExposure.At(_director.Player.transform.position + Vector3.up * 1.05f);
            float t = 0f;
            while (!_probeCaught && (t += Time.deltaTime) < 90f) yield return null;
            Report("lit, standing, on his round", _probeCaught, $"exposure {light:F2} · caught after {t:F1} s");

            // 2. Crouched in the dark corridor, 6 m beyond the west end of his round (where he stands looking west): one full
            // round, unseen.
            yield return Reset();
            yield return Stand(new Vector3(-10.2f, F, 10.3f), 90f, true);
            light = LightExposure.At(_director.Player.transform.position + Vector3.up * 0.6f);
            float peak = 0f;
            t = 0f;
            while (!_probeCaught && (t += Time.deltaTime) < 100f) { peak = Mathf.Max(peak, _guard.Awareness); yield return null; }
            Report("dark, crouched, 6 m off his round", !_probeCaught && peak < Watchman.InvestigateAt, $"exposure {light:F2} · peak awareness {peak:F2} over {t:F0} s");

            // 3. A wall between them: he stands 1.8 m away on the corridor side of the kitchen wall, she stands in the kitchen.
            yield return Reset();
            // She waits out of his sight (in the back alley) while he walks to his post, then steps into the kitchen.
            yield return Stand(new Vector3(-22.9f, 0f, -12f), 0f, true);
            bool there = false;
            _guard.Script(new Vector3(-15.2f, F, 10.2f), 1.4f, () => there = true);
            t = 0f;
            while (!there && (t += Time.deltaTime) < 30f) yield return null;
            _guard.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            yield return Stand(new Vector3(-15.2f, 0.12f, 12.0f), 180f, false);
            peak = 0f;
            bool everSaw = false;
            for (t = 0f; t < 6f; t += Time.deltaTime) { _guard.transform.rotation = Quaternion.identity; peak = Mathf.Max(peak, _guard.Awareness); everSaw |= _guard.SeesPlayer; yield return null; }
            Report("wall between them", there && !everSaw && peak < 0.05f, $"reached post {there} · saw {everSaw} · peak awareness {peak:F2}");

            // 4. Hidden before he looks: he stands in chị Lam's open doorway looking in, she is behind the screen. Not found.
            yield return Reset();
            _lamDoor.SetLock(DoorLock.None);
            _lamDoor.SetOpen(1f);
            yield return Stand(new Vector3(-19.0f, F, -7.4f), 270f, false);
            lamScreen.Interact();
            there = false;
            _guard.Script(new Vector3(-10.9f, F, -3.0f), 1.4f, () => there = true);
            t = 0f;
            while (!there && (t += Time.deltaTime) < 40f) yield return null;
            for (t = 0f; t < 8f && !_probeCaught; t += Time.deltaTime) { _guard.transform.rotation = Quaternion.Euler(0f, 270f, 0f); yield return null; }
            Report("hidden before he looked", there && !_probeCaught && _guard.KnownHide == null, $"reached door {there} · caught {_probeCaught} · knows the place {_guard.KnownHide != null}");

            // 5. Seen slipping behind the screen: he knows the place, goes to it and finds her (E2).
            yield return Reset();
            _lamDoor.SetOpen(1f);
            there = false;
            _guard.Script(new Vector3(-15.5f, F, -3.5f), 1.4f, () => there = true);
            t = 0f;
            while (!there && (t += Time.deltaTime) < 40f) yield return null;
            // At the screen, as in play (the place she hides from is where he comes to find her).
            yield return Stand(new Vector3(-19.0f, F, -7.4f), 270f, false);
            for (t = 0f; t < 10f && _guard.Awareness < Watchman.InvestigateAt; t += Time.deltaTime) { if (_guard.State == WatchState.Scripted) _guard.transform.rotation = Quaternion.Euler(0f, 222f, 0f); yield return null; }
            float seen = _guard.Awareness;
            lamScreen.Interact();
            bool knows = _guard.KnownHide == lamScreen;
            for (t = 0f; t < 25f && !_probeCaught; t += Time.deltaTime) yield return null;
            // He must find her at the screen, within arm's reach, not from where he stood.
            float reach = Vector3.Distance(_guard.transform.position, lamScreen.ExitPosition);
            Report("seen slipping into hiding", knows && _probeCaught && reach < 1.5f, $"awareness when she hid {seen:F2} · knew the place {knows} · found her {_probeCaught} after {t:F1} s · {reach:F1} m from the screen");

            // 6. The yard crossing (information, not pass/fail): how lit the way across is at chest height, every metre from
            // the west steps to the east steps, in the roofs' moon-shadow and out of it.
            var crossing = new System.Text.StringBuilder();
            int shaded = 0, samples = 0;
            float darkest = 1f, brightest = 0f, sum = 0f;
            for (float x = -8.5f; x <= 8.51f; x += 1f, samples++)
            {
                float e = LightExposure.At(new Vector3(x, 1.0f, 1f));
                sum += e; darkest = Mathf.Min(darkest, e); brightest = Mathf.Max(brightest, e);
                if (e < 0.2f) shaded++;
                crossing.Append(e.ToString("F2")).Append(' ');
            }
            StoryAutopilot.Note($"STEALTH info yard crossing exposure min {darkest:F2} · avg {sum / samples:F2} · max {brightest:F2} · {shaded}/{samples} points under 0.20 · [{crossing.ToString().Trim()}]");

            // 7. Standing, then crouching, in the open middle of the yard while he walks his round (information): does the
            // moonlit yard still give her away, and how fast.
            foreach (bool crouch in new[] { false, true })
            {
                yield return Reset();
                yield return Stand(new Vector3(2.5f, 0f, 0.5f), 90f, crouch);
                float yardLight = LightExposure.At(_director.Player.transform.position + Vector3.up * (crouch ? 0.6f : 1.05f));
                peak = 0f;
                for (t = 0f; t < 70f && !_probeCaught; t += Time.deltaTime) { peak = Mathf.Max(peak, _guard.Awareness); yield return null; }
                StoryAutopilot.Note($"STEALTH info open yard, {(crouch ? "crouched" : "standing")} · exposure {yardLight:F2} · caught {_probeCaught} after {t:F1} s · peak awareness {peak:F2}");
            }

            yield return Reset();
            _lamDoor.SetOpen(0f);
            _lamDoor.SetLock(_director.GetBool("c4_gap_co_lam") ? DoorLock.None : DoorLock.Locked);
            _guard.Perceiving = false;
            _guard.Caught -= Caught;
            _probing = false;
            ArmGuard();
            AutoTeleport(SpawnFor(_stage).position);
        }

        private void AutoTeleport(Vector3 position)
        {
            _director.Player.Teleport(position, _director.Player.transform.rotation);
            RestrictedZone.ResetPresence();
        }

        private bool _shotHero, _shotViews;

        /// <summary>QA still from Kiều's eyes at a place, looking at a point (only with -autopilotCapture).</summary>
        private IEnumerator Still(string name, Vector3 at, Vector3 lookAt)
        {
            AutoTeleport(at);
            for (float t = 0f; t < 0.8f; t += Time.unscaledDeltaTime) { _director.CameraController.LookAtNow(lookAt); yield return null; }
            StoryCapture.Shot(name);
        }
    }
}
