using System.Collections.Generic;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Stealth;
using ThuyKieu.Interaction;
using Unity.AI.Navigation;
using UnityEngine;

namespace ThuyKieu.Gameplay.Chapters
{
    /// <summary>
    /// Chapter 4 — the house at night. Tú Bà shuttles between the guests in the hall and her room, sitting longer
    /// each time until, once the player has studied her routine, she dozes off with the keys on her belt.
    /// A drunk guard patrols the courtyard; lamplight and restricted areas decide when the player is seen.
    /// </summary>
    public class Chapter04World : MonoBehaviour, IChapterSummary
    {
        private enum TuBaState { Opening, ToHall, Serving, ToRoom, Sitting, Dozing }

        [SerializeField] private StoryDirector _director;
        [SerializeField] private NavMeshSurface _navMesh;
        [SerializeField] private StoryActor _tuBa;
        [SerializeField] private VisionSensor _tuBaSight;
        [SerializeField] private GuardPatrol _guard;
        [SerializeField] private Renderer _visionCone;
        [Tooltip("Posted at Kiều's door only if she defied Tú Bà; nods off now and then.")]
        [SerializeField] private StoryActor _doorGuard;
        [SerializeField] private VisionSensor _doorGuardSight;
        [SerializeField] private Transform _hallSpot, _roomSpot, _playerRoom;
        [SerializeField] private ExploreTarget _keyTarget;
        [SerializeField] private StoryActor[] _guests;
        [SerializeField] private PebbleThrow _pebbles;
        [SerializeField] private float _serveSeconds = 10f;
        [SerializeField] private float _sitSeconds = 6f;

        private TuBaState _state = TuBaState.Opening;
        private float _timer, _barkCooldown, _doorGuardTimer, _snoreTimer;
        private int _visits;
        private bool _dozeAnnounced, _doorGuardAwake = true, _ended;
        private MaterialPropertyBlock _coneBlock;
        private float _coneAlpha = 0.06f;
        private string _lastSequence;

        private void OnEnable()
        {
            _director.ExploreChanged += OnExplore;
            _director.CheckpointRestored += OnRestored;
            _director.SceneChanged += OnScene;
        }

        private void OnDisable()
        {
            _director.ExploreChanged -= OnExplore;
            _director.CheckpointRestored -= OnRestored;
            _director.SceneChanged -= OnScene;
        }

        private void OnDestroy() { LightExposure.Enable(false); }

        private void Start()
        {
            if (_navMesh != null) _navMesh.BuildNavMesh();
            foreach (var actor in FindObjectsByType<StoryActor>()) actor.EnableNavigation();
            _director.Player.AllowCrouch = true;
            LightExposure.Enable(true);
            if (_pebbles != null) { _pebbles.CountChanged += _ => UpdateControls(); _pebbles.SetCount(3); }
            UpdateControls();
            if (_keyTarget != null) _keyTarget.Reveal(false);
            if (_doorGuard != null) _doorGuard.gameObject.SetActive(false);
            _tuBaSight.Active = false;
            if (_director.Audio != null) _director.Audio.SetAmbience("night_house");
        }

        private void UpdateControls()
        {
            int pebbles = _pebbles != null ? _pebbles.Count : 0;
            _director.Hud.SetControls("WASD: đi · Ctrl: khom · E: tương tác / trốn · Q: ném sỏi (" + pebbles + ") · Chuột phải: quan sát · J: sổ tay");
        }

        private void OnExplore(bool exploring)
        {
            _guard.Patrolling = exploring;
            if (!exploring) return;
            if (_state == TuBaState.Opening)
            {
                // The lesson is over: Tú Bà goes back to her guests and the guard to his round. Defiance earns a door guard.
                int wariness = _director.GetInt("c4_canh_giac");
                _guard.Vision.Sensitivity = 0.75f + 0.2f * wariness;
                _tuBaSight.Sensitivity = 0.85f + 0.15f * wariness;
                if (_doorGuard != null && wariness >= 2) _doorGuard.gameObject.SetActive(true);
                GoHall();
            }
            if (_guests != null) foreach (var guest in _guests) if (guest != null) guest.SetTalking(true, 3);
        }

        private void Update()
        {
            if (_barkCooldown > 0f) _barkCooldown -= Time.deltaTime;
            UpdateTuBa();
            UpdateDoorGuard();
            string sequence = _director.CurrentSequence;
            if (sequence == "pickpocket" && _lastSequence != sequence) _tuBaSight.Active = false;
            _lastSequence = sequence;
            if (!_director.InExplore || _ended) { UpdateCone(false); return; }

            float awareness = Mathf.Max(_guard.Vision.Awareness, _tuBaSight.Awareness);
            if (_doorGuardSight != null && _doorGuard != null && _doorGuard.gameObject.activeSelf) awareness = Mathf.Max(awareness, _doorGuardSight.Awareness);
            if (awareness > 0.04f)
            {
                _director.Hud.SetMeter(awareness > 0.6f ? "CÓ NGƯỜI ĐANG TIẾN LẠI!" : "Có người nghi ngờ...", awareness, new Color(0.9f, 0.25f, 0.2f));
                if (awareness > 0.5f && _barkCooldown <= 0f)
                {
                    _barkCooldown = 5f;
                    bool tuBa = _tuBaSight.Awareness >= _guard.Vision.Awareness;
                    _director.Hud.ShowBark(tuBa ? "TuBa" : "NguoiCanh", tuBa ? "Hửm? Đứa nào lảng vảng ngoài kia?" : "Ai... ai đấy? (nấc) Đứng lại!");
                }
            }
            else if (RestrictedZone.PlayerInside && !_director.Player.IsHidden)
                _director.Hud.SetMeter("Khu vực cấm: " + RestrictedZone.CurrentLabel, 0f, new Color(1f, 0.8f, 0.45f));

            if (RestrictedZone.PlayerInside && !_director.Player.IsHidden)
            {
                float light = LightExposure.At(_director.Player.transform.position + Vector3.up);
                _director.Hud.SetGauge(light > 0.55f ? "Bạn đang đứng trong ánh đèn!" : light > 0.3f ? "Ánh sáng mờ" : "Bóng tối che bạn", light,
                    Color.Lerp(new Color(0.45f, 0.55f, 0.85f), new Color(1f, 0.6f, 0.3f), light));
            }
            if (_director.Player.IsHidden) _director.Hud.SetGauge("Đang trốn — E để ra", 0f, Color.white);

            UpdateCone(RestrictedZone.PlayerInside || _guard.Vision.Awareness > 0.04f);

            if (_guard.Vision.Spotted || _tuBaSight.Spotted || (_doorGuardSight != null && _doorGuardSight.Spotted))
            {
                bool tuBa = _tuBaSight.Spotted;
                _director.Fail(tuBa ? "Tú Bà bắt gặp!" : "Bị bắt gặp!",
                    tuBa ? "\"Con ranh, nửa đêm đi đâu?\" — Mụ lôi bạn về phòng. Đợi mụ quay lưng, đi trong bóng tối, nấp sau chum vại hoặc mành tre."
                         : "\"Về phòng! Mai mẹ Tú biết thì chết!\" — Đi trong bóng tối, khom người, ném sỏi (Q) để dụ hắn đi chỗ khác.");
            }
        }

        // ------------------------------------------------------------------ Tú Bà's round

        private void GoHall() { _state = TuBaState.ToHall; _tuBa.MoveTo(_hallSpot.position, 1.15f); _tuBaSight.Active = true; }
        private void GoRoom() { _state = TuBaState.ToRoom; _tuBa.MoveTo(_roomSpot.position, 1.05f); _tuBaSight.Active = true; }

        private void UpdateTuBa()
        {
            bool explore = _director.InExplore;
            switch (_state)
            {
                case TuBaState.ToHall:
                    if (Arrived(_hallSpot)) { _state = TuBaState.Serving; _timer = _serveSeconds; _tuBa.FaceTowards(_hallSpot.position + _hallSpot.forward); }
                    break;
                case TuBaState.Serving:
                    if (explore && (_timer -= Time.deltaTime) <= 0f) GoRoom();
                    break;
                case TuBaState.ToRoom:
                    if (Arrived(_roomSpot)) { _state = TuBaState.Sitting; _visits++; _timer = _sitSeconds + 3f * _visits; _tuBa.Place(_roomSpot.position, _roomSpot.rotation); }
                    break;
                case TuBaState.Sitting:
                    if (_director.GetBool("c4_quan_sat")) { Doze(); break; }
                    if (explore && (_timer -= Time.deltaTime) <= 0f) GoHall();
                    break;
                case TuBaState.Dozing:
                    if (explore && !_director.GetBool("c4_chia_khoa") && (_snoreTimer -= Time.deltaTime) <= 0f)
                    {
                        _snoreTimer = 4.5f;
                        if (Vector3.Distance(_director.Player.transform.position, _tuBa.transform.position) < 9f && _director.Audio != null) _director.Audio.PlaySfx("snore", 0.3f);
                    }
                    break;
            }
            // Once the routine is known, she heads for her chair at the end of whatever she is doing.
            if (_state == TuBaState.Serving && _director.GetBool("c4_quan_sat") && _timer > 2f) _timer = 2f;
        }

        private void Doze()
        {
            _state = TuBaState.Dozing;
            _tuBaSight.Active = false;
            _tuBaSight.ResetAwareness();
            _tuBa.Place(_roomSpot.position, _roomSpot.rotation);
            if (_keyTarget != null) _keyTarget.Reveal(true);
            if (_dozeAnnounced) return;
            _dozeAnnounced = true;
            _director.Hud.ShowBark("NoiTam", "Mụ ngủ gật rồi. Chùm chìa vẫn lủng lẳng bên hông...", 4f);
        }

        private bool Arrived(Transform spot)
        {
            Vector3 offset = _tuBa.transform.position - spot.position;
            offset.y = 0f;
            return offset.sqrMagnitude < 0.5f * 0.5f || !_tuBa.IsMoving;
        }

        // ------------------------------------------------------------------ door guard

        private void UpdateDoorGuard()
        {
            if (_doorGuard == null || !_doorGuard.gameObject.activeSelf || _doorGuardSight == null) return;
            if (!_director.InExplore) { _doorGuardSight.Active = false; return; }
            if ((_doorGuardTimer -= Time.deltaTime) > 0f) { _doorGuardSight.Active = _doorGuardAwake; return; }
            _doorGuardAwake = !_doorGuardAwake;
            _doorGuardTimer = _doorGuardAwake ? Random.Range(4f, 6f) : Random.Range(6f, 8f);
            _doorGuardSight.Active = _doorGuardAwake;
            if (!_doorGuardAwake) { _doorGuardSight.ResetAwareness(); _director.Hud.ShowBark("NguoiCanh", "(gà gật) Khò... khò...", 2.5f); }
            else _director.Hud.ShowBark("NguoiCanh", "(giật mình) Hử? ...Ai?", 1.8f);
        }

        private void UpdateCone(bool alert)
        {
            if (_visionCone == null) return;
            _coneBlock ??= new MaterialPropertyBlock();
            _coneAlpha = Mathf.MoveTowards(_coneAlpha, alert ? 0.22f : 0.07f, Time.deltaTime * 0.5f);
            _coneBlock.SetColor("_BaseColor", new Color(1f, 0.2f, 0.12f, _coneAlpha));
            _visionCone.SetPropertyBlock(_coneBlock);
        }

        // ------------------------------------------------------------------ checkpoints & scenes

        private void OnRestored()
        {
            HideSpot.ExitAll();
            _director.Player.Teleport(_playerRoom.position, _playerRoom.rotation);
            _director.CameraController.SnapBehindTarget(_playerRoom.eulerAngles.y);
            RestrictedZone.ResetPresence();
            _guard.ResetPatrol();
            _tuBaSight.ResetAwareness();
            if (_doorGuardSight != null) _doorGuardSight.ResetAwareness();
            if (_state != TuBaState.Dozing && _state != TuBaState.Opening) { _tuBa.Place(_hallSpot.position, _hallSpot.rotation); _state = TuBaState.Serving; _timer = _serveSeconds; }
            if (_pebbles != null && _pebbles.Count < 2) _pebbles.SetCount(2);
            _director.SetInt("c4_bi_bat", _director.GetInt("c4_bi_bat") + 1);
        }

        private void OnScene(string scene)
        {
            if (scene != "TuBa_BackDoor") return;
            _ended = true;
            _guard.Patrolling = false;
            _guard.Vision.Active = false;
            _tuBaSight.Active = false;
            if (_doorGuardSight != null) _doorGuardSight.Active = false;
        }

        public IEnumerable<string> SummaryLines(Ink.Runtime.Story story)
        {
            var v = story.variablesState;
            int money = (int)v["c4_tien"], paper = (int)v["c4_van_tu"];
            yield return "Việc đã làm trong đêm: " + (int)v["c4_viec"] + " · số lần bị bắt gặp: " + (int)v["c4_bi_bat"];
            yield return "Đã mang theo giao kèo Mã Giám Sinh – Tú Bà: bằng chứng buôn người.";
            yield return paper == 1 ? "Văn tự bán thân: cất trong áo làm bằng chứng." : paper == 2 ? "Văn tự bán thân: đã đốt thành tro." : "Văn tự bán thân: để lại chỗ cũ để Tú Bà chậm phát hiện.";
            yield return money == 2 ? "Lấy cả túi bạc có dấu \"T.\" — bằng chứng, nhưng nặng và dễ mang tiếng trộm." : money == 1 ? "Chỉ lấy vài lạng làm lộ phí." : "Không lấy một đồng nào của Tú Bà.";
            if ((bool)v["c4_so_no"]) yield return "Đã xé trang sổ nợ của mình.";
            yield return (bool)v["c4_ru_co_lam"] ? "Cô gái áo lam sẽ trốn cùng bạn." : "Bạn đi một mình. Cô gái áo lam ở lại.";
            yield return "<i>Chương tiếp theo — Đào thoát: sân sau có chó, có đèn lồng...</i>";
        }
    }
}
