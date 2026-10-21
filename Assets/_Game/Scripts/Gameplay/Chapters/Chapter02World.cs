using System.Collections;
using System.Collections.Generic;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Sequences;
using ThuyKieu.Interaction;
using ThuyKieu.Player;
using ThuyKieu.Presentation;
using Unity.AI.Navigation;
using UnityEngine;

namespace ThuyKieu.Gameplay.Chapters
{
    /// <summary>
    /// Chapter 2 — the road with Mã Giám Sinh. Owns the escort walk, his suspicion of Kiều,
    /// the dropped note, the tea-house staging and the dusk-to-night lighting.
    /// </summary>
    public class Chapter02World : MonoBehaviour, IChapterSummary
    {
        [System.Serializable]
        private struct LightingPreset { public string Scene; public Color Sun; public float Intensity; public Vector3 Angles; public Color Ambient; public Color Fog; public float FogDensity; public Material Sky; }

        [SerializeField] private StoryDirector _director;
        [SerializeField] private NavMeshSurface _navMesh;
        [SerializeField] private StoryActor _ma;
        [SerializeField] private StoryActor _servant;
        [SerializeField] private ObserveMode _observe;
        [SerializeField] private Transform[] _path;
        [SerializeField] private int _paperDropWaypoint = 4;
        [SerializeField] private ExploreTarget _paper;
        [SerializeField] private Transform _bench;
        [SerializeField] private Transform _maBehindWall, _servantBehindWall;
        [SerializeField] private Transform _gatePlayer, _gateMa, _gateServant;
        [SerializeField] private Light _sun;
        [SerializeField] private GameObject[] _nightLights;
        [SerializeField] private LightingPreset[] _lighting;

        private const string Controls = "WASD: đi · Shift: chạy · Chuột phải: quan sát · Ctrl: khom · E: tương tác · J: sổ tay";
        private const string RunAwayBeat = "c2_flee";
        private const string PageDeductionBeat = "c2_page_deduction";
        // Running from the tea house: how long before they notice, and when running has gone on long or far enough
        // for her to see what it costs.
        private const float NoticeAfter = 2.5f, RunLongest = 20f, RunFarthest = 60f;

        // Mã Giám Sinh's walking pace; he stops and turns when she comes up to talk.
        private const float EscortSpeed = 1.45f;
        // Straying this far from him sends the servant after her; coming back within GiveUpDistance calls him off.
        // Generous enough that stopping to look at the scenery or a NavMesh detour never reads as running away.
        private const float StrayDistance = 16f, GiveUpDistance = 6f, ChaseSpeed = 4.2f;
        private const float StrayGraceTime = 3.5f;
        // The road is a choice of distance: within EarshotDistance she overhears what Mã Giám Sinh and his servant
        // mutter, but walking at his heels makes him faintly uneasy; leaving the fallen note too long lets the servant
        // visibly walk back and recover it.
        private const float EarshotDistance = 3.2f;
        private static readonly (string speaker, string line)[] Mutterings =
        {
            ("TuyTung", "Bên Lâm Tri nhắn: phải giao người trước rằm."),
            ("MaGiamSinh", "Khẽ thôi. Con bé đi ngay sau lưng."),
            ("TuyTung", "Chuyến này lãi được năm mươi lạng, chủ nhỉ?")
        };

        // Each rise in suspicion leaves this share behind for good, and the floor never climbs past MaxFloor.
        private const float FloorShare = 1f / 3f, MaxFloor = 60f;

        private Transform _profileShot;
        private int _waypoint;
        private float _glanceTimer = 10f, _glanceHold, _barkCooldown, _strayTimer;
        private bool _waiting, _restCalled, _paperDropped, _retrievingPaper, _chasing, _shouted;
        private GameObject _nightCharacterFill;
        private float _suspicionCarry;
        // How low suspicion can fall back: where Chapter 1 left it, raised by every suspicious thing she does here.
        private float _suspicionFloor = -1f;
        private int _overheard;
        private float _earshotTime;

        public bool Glancing => _glanceHold > 0f;
        public int Waypoint => _waypoint;

        private void Awake()
        {
            _director.RegisterSequence(WorldSequence.Create(gameObject, RunAwayBeat, RunAway));
            _director.RegisterSequence(WorldSequence.Create(gameObject, PageDeductionBeat, ResolvePageDeduction));
        }

        private IEnumerator ResolvePageDeduction(StoryDirector director)
        {
            int suspicion = director.GetInt("nghi_ngo");
            int finalSuspicion = Mathf.Max(0, suspicion - 5);
            director.SetInt("nghi_ngo", finalSuspicion);
            director.SetBool("c2_sai_noi_den", false);
            director.SetBool("c2_sai_bac", false);
            director.SetBool("c2_sai_nguoi_nhan", false);
            string change = finalSuspicion == suspicion ? "Nghi ngờ không đổi" : "Nghi ngờ -" + (suspicion - finalSuspicion);
            director.Hud.ShowToast("TRANG SÁCH SOI RÕ SỰ THẬT", "Ba mảnh ghép đã được xác nhận · " + change);
            if (director.Audio != null) director.Audio.PlaySfx("clue");
            yield return null;
        }

        private void OnEnable()
        {
            _director.SceneChanged += OnScene;
            _director.CheckpointRestored += OnRestored;
        }

        private void OnDisable()
        {
            _director.SceneChanged -= OnScene;
            _director.CheckpointRestored -= OnRestored;
        }

        private void Start()
        {
            // Runtime bake: the road is generated geometry, and agents join once the mesh exists.
            if (_navMesh != null) _navMesh.BuildNavMesh();
            foreach (var actor in FindObjectsByType<StoryActor>()) actor.EnableNavigation();
            _ma.CalmGesture = StoryDirector.Gesture("satisfied");
            // Sized against Kiều by the head bone (whole-model bounds count hats and hairpins).
            float kieu = _director.Player.GetComponent<StoryActor>().HeadHeight;
            if (kieu > 0.5f)
            {
                _ma.FitHeadHeight(kieu * 1.07f);
                _servant.FitHeadHeight(kieu * 1.05f);
            }
            _director.Player.AllowCrouch = true;
            _director.Hud.SetControls(Controls);
            RefreshRoadTutorial();
            PrepareNightCharacterFill();
            PreloadChapterPresentation();
            ApplyLighting("Road_Departure");
        }

        private void RefreshRoadTutorial()
        {
            foreach (var tutorial in FindObjectsByType<TutorialSequence>(FindObjectsInactive.Include))
                if (tutorial.Id == "tutorial_road")
                    tutorial.Configure(
                        "<color=#E8C27A>Mục tiêu:</color> theo đoàn đến Lâm Tri và tìm hiểu Mã Giám Sinh.\n\n" +
                        "<color=#E8C27A>Chuột phải:</color> quan sát · <color=#E8C27A>E:</color> tương tác · <color=#E8C27A>Ctrl/C:</color> khom người.\n" +
                        "Đi vừa tầm đoàn. Nhìn chằm chằm khi hắn quay lại hoặc tụt quá xa sẽ làm tăng <color=#F08080>Nghi ngờ</color>.",
                        Controls);
        }

        private void PrepareNightCharacterFill()
        {
            _nightCharacterFill = new GameObject("Ch2NightCharacterFill");
            Vector3 focus = _gatePlayer != null ? _gatePlayer.position + Vector3.up * 1.1f : Vector3.up;
            CreateGateLight("WarmKey", focus + new Vector3(-3.8f, 3.8f, -3.5f), focus,
                new Color(1f, 0.66f, 0.38f), 3.1f, 12f, 62f);
            CreateGateLight("MoonRim", focus + new Vector3(3.5f, 3.2f, 2.8f), focus,
                new Color(0.45f, 0.62f, 1f), 2.35f, 11f, 58f);
            var lanternFill = new GameObject("LanternFill");
            lanternFill.transform.SetParent(_nightCharacterFill.transform, true);
            lanternFill.transform.position = focus + new Vector3(-1.8f, 1.6f, 0.8f);
            var point = lanternFill.AddComponent<Light>();
            point.type = LightType.Point;
            point.color = new Color(1f, 0.42f, 0.2f);
            point.intensity = 1.15f;
            point.range = 5.5f;
            point.shadows = LightShadows.None;
            var moonFill = new GameObject("MoonFill");
            moonFill.transform.SetParent(_nightCharacterFill.transform, true);
            moonFill.transform.position = focus + Vector3.up * 1.8f;
            var moonPoint = moonFill.AddComponent<Light>();
            moonPoint.type = LightType.Point;
            moonPoint.color = new Color(0.38f, 0.5f, 0.82f);
            moonPoint.intensity = 0.65f;
            moonPoint.range = 7.5f;
            moonPoint.shadows = LightShadows.None;
            _nightCharacterFill.SetActive(false);
        }

        private void CreateGateLight(string name, Vector3 position, Vector3 focus, Color color, float intensity, float range, float angle)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_nightCharacterFill.transform, true);
            go.transform.SetPositionAndRotation(position, Quaternion.LookRotation(focus - position));
            var light = go.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.innerSpotAngle = angle * 0.55f;
            light.spotAngle = angle;
            light.shadows = LightShadows.Soft;
        }

        private static void PreloadChapterPresentation()
        {
            string[] panels = { "montage_c2_mo_1", "montage_c2_mo_2", "montage_c2_lam_tri_1", "montage_c2_lam_tri_2" };
            foreach (string panel in panels) Resources.Load<Texture2D>("StoryPanels/" + panel);
            AudioBank.Get(AudioBank.Sfx, "whoosh");
        }

        private void Update()
        {
            KeepProfileShot();
            if (_barkCooldown > 0f) _barkCooldown -= Time.deltaTime;
            if (!_director.InExplore)
            {
                if (!_director.IsBusy || _director.CurrentSequence == null) HoldEscort();
                return;
            }
            UpdateChase();
            if (!_chasing) UpdateEscort();
            UpdateSuspicion();
            if (!_restCalled && _director.IsExploreAvailable("c2_eavesdrop"))
            {
                _restCalled = true;
                Bark("MaGiamSinh", "Trời sắp tối rồi. Ghé quán nước phía trước nghỉ chân đã.");
            }
        }

        // ------------------------------------------------------------------ escort

        private void HoldEscort()
        {
            _strayTimer = 0f;
            if (_ma.IsMoving) _ma.Stop();
            if (_servant.IsMoving) _servant.Stop();
        }

        private void UpdateEscort()
        {
            Vector3 player = _director.Player.transform.position;
            float distance = Flat(player - _ma.transform.position);

            if (_glanceHold > 0f)
            {
                _glanceHold -= Time.deltaTime;
                _ma.FaceTowards(player);
                return;
            }

            // She has come up to talk: he stops and turns to her so the conversation can start.
            if (distance < 3f && _director.IsExploreAvailable("c2_ask"))
            {
                _ma.Stop();
                _ma.FaceTowards(player);
                FollowServant(false);
                return;
            }

            bool atEnd = _waypoint >= _path.Length - 1;
            if (distance > 10f && !atEnd)
            {
                // Wait for her, turned back, impatient.
                if (!_waiting) { _waiting = true; Bark("MaGiamSinh", "Nương tử, nhanh chân lên. Đừng để ta phải chờ."); }
                _ma.Stop();
                _ma.FaceTowards(player);
                FollowServant(false);
                return;
            }
            if (_waiting && distance < 6f) _waiting = false;
            if (_waiting) return;

            if (!atEnd)
            {
                Vector3 target = _path[_waypoint].position;
                if (Flat(target - _ma.transform.position) < 1.2f)
                {
                    _waypoint++;
                    if (_waypoint == _paperDropWaypoint && !_paperDropped) StartCoroutine(DropPaper());
                }
                else if (!_ma.IsMoving) _ma.MoveTo(target, EscortSpeed);

                if ((_glanceTimer -= Time.deltaTime) <= 0f)
                {
                    _glanceTimer = Random.Range(9f, 14f);
                    _glanceHold = 1.8f;
                    _ma.Stop();
                }
            }
            else if (!_ma.IsMoving)
            {
                _ma.FaceTowards(_servant.transform.position);
            }
            if (!_retrievingPaper) FollowServant(true);
        }

        private void FollowServant(bool follow)
        {
            Vector3 slot = _ma.transform.position - _ma.transform.forward * 1.7f + _ma.transform.right * 0.9f;
            if (!follow || Flat(slot - _servant.transform.position) < 0.8f) { if (_servant.IsMoving && !follow) _servant.Stop(); return; }
            // He is an attendant keeping formation, not a guard charging at Kiều.
            _servant.MoveTo(slot, 1.35f);
        }

        /// <summary>
        /// "Kieu_Profile": side-on at her eye line, along the woven screen she listens through, so the moment she
        /// understands is on her face (from the far side the poles cover it, from behind there is no face).
        /// </summary>
        private void KeepProfileShot()
        {
            if (_profileShot == null)
            {
                _profileShot = new GameObject("CAM_Kieu_Profile").transform;
                _profileShot.SetParent(transform, false);
            }
            var kieu = _director.Player.GetComponent<StoryActor>();
            Vector3 head = kieu.HeadPosition;
            Vector3 facing = Vector3.ProjectOnPlane(kieu.transform.forward, Vector3.up).normalized;
            Vector3 side = Vector3.Cross(Vector3.up, facing);
            _profileShot.position = head - side * 1.25f + facing * 0.35f + Vector3.up * 0.05f;
            _profileShot.rotation = Quaternion.LookRotation(head - Vector3.up * 0.08f - _profileShot.position);
        }

        /// <summary>Kiều wanders off: Mã Giám Sinh shouts and his servant runs her down; catching her restores the checkpoint.</summary>
        private void UpdateChase()
        {
            Vector3 player = _director.Player.transform.position;
            float distance = Flat(player - _ma.transform.position);
            if (!_chasing)
            {
                // At the end of the road he waits at the tea house: walking up to it is not running away.
                bool resting = _waypoint >= _path.Length - 1 || (_bench != null && Flat(player - _bench.position) < 9f);
                if (distance < StrayDistance || resting)
                {
                    _strayTimer = 0f;
                    return;
                }
                // Scene entry, a corner in the NavMesh, or stopping to inspect the dropped note can briefly put Kiều
                // over the distance threshold. Only a sustained separation counts as trying to flee.
                _strayTimer += Time.deltaTime;
                if (_strayTimer < StrayGraceTime) return;
                _strayTimer = 0f;
                _chasing = true;
                _shouted = false;
                _ma.Stop();
                _ma.FaceTowards(player);
                Bark("MaGiamSinh", "Đứng lại! Bắt nó về đây!");
                _director.Hud.ShowToast("BỊ ĐUỔI THEO", "Tùy tùng của Mã Giám Sinh đang chạy tới. Quay lại gần hắn!");
                if (_director.Audio != null) { _director.Audio.PlaySfx("detect"); _director.Audio.PlaySfx("heartbeat"); }
                if (_director.Fx != null) _director.Fx.Shake(0.2f, 0.35f);
                return;
            }
            if (distance < GiveUpDistance)
            {
                _chasing = false;
                _strayTimer = 0f;
                _servant.Stop();
                Bark("TuyTung", "Nương tử đi lạc sao? Đừng để chủ ta nổi giận.");
                return;
            }
            _servant.MoveTo(player, ChaseSpeed);
            float gap = Flat(player - _servant.transform.position);
            _director.Hud.SetMeter("TÙY TÙNG ĐANG ĐUỔI THEO", Mathf.Clamp01(1f - gap / StrayDistance), new Color(0.9f, 0.2f, 0.15f));
            if (!_shouted && gap < 6f) { _shouted = true; Bark("TuyTung", "Đứng lại đó! Chạy đâu cho thoát!"); }
            if (gap < 1.3f)
            {
                _chasing = false;
                _servant.Stop();
                _director.Fail("Bị bắt lại", "Tên tùy tùng của Mã Giám Sinh đuổi kịp và lôi bạn về. Trên đường đi, đừng rời xa hắn quá.");
            }
        }

        private IEnumerator DropPaper()
        {
            _paperDropped = true;
            Transform paper = _paper.transform;
            Vector3 landing = _ma.transform.position - _ma.transform.forward * 0.6f;
            landing.y = 0.03f;
            Vector3 start = landing + Vector3.up * 1.1f;
            paper.gameObject.SetActive(true);
            for (float t = 0; t < 1f; t += Time.deltaTime * 0.9f)
            {
                paper.position = Vector3.Lerp(start, landing, t * t) + new Vector3(Mathf.Sin(t * 9f) * 0.15f, 0, Mathf.Cos(t * 7f) * 0.1f) * (1 - t);
                paper.rotation = Quaternion.Euler(Mathf.Sin(t * 11f) * 40f * (1 - t), t * 220f, Mathf.Cos(t * 9f) * 30f * (1 - t));
                yield return null;
            }
            paper.SetPositionAndRotation(landing, Quaternion.Euler(0, 35f, 0));
            _paper.Reveal();
            Bark("NoiTam", "Có thứ gì vừa rơi khỏi tay áo hắn...");

            // If Kiều leaves it on the road, the servant visibly walks back for it. This runs regardless of where she
            // stood when it landed; otherwise being close for one frame left the clue on the road forever.
            yield return new WaitForSeconds(_director.Autopilot ? 0f : 4f);
            if (!_director.IsExploreAvailable("c2_paper") || !paper.gameObject.activeSelf) yield break;
            _barkCooldown = 0f;
            Bark("TuyTung", "Công tử... hình như ban nãy có thứ rơi trên đường.");
            yield return new WaitForSeconds(_director.Autopilot ? 0f : 4f);
            if (!_director.IsExploreAvailable("c2_paper") || !paper.gameObject.activeSelf) yield break;
            _retrievingPaper = true;
            _servant.MoveTo(landing, 1.8f);
            for (float t = 0f; t < 3f && _director.IsExploreAvailable("c2_paper")
                 && Flat(_servant.transform.position - landing) > 0.75f; t += Time.deltaTime)
                yield return null;
            if (!_director.IsExploreAvailable("c2_paper") || !paper.gameObject.activeSelf)
            {
                _retrievingPaper = false;
                yield break;
            }
            _paper.Reveal(false);
            paper.gameObject.SetActive(false);
            _director.SetBool("c2_giay_mat", true);
            _retrievingPaper = false;
            _barkCooldown = 0f;
            Bark("NoiTam", "Tên tùy tùng đã nhặt mẩu giấy, nhét lại vào tay áo chủ. Mình tụt lại xa quá.");
        }

        // ------------------------------------------------------------------ suspicion

        private void UpdateSuspicion()
        {
            Vector3 player = _director.Player.transform.position;
            float distance = Flat(player - _ma.transform.position);
            float rate = 0f;
            if (_chasing) rate += 8f;
            else if (distance > 9f) rate += 6f;
            bool watched = Glancing || Vector3.Dot(_ma.transform.forward, (player - _ma.transform.position).normalized) > 0.6f;
            if (_observe != null && _observe.IsObserving && watched && distance < 11f)
            {
                rate += 24f;
                if (_barkCooldown <= 0f) Bark("MaGiamSinh", "Nàng nhìn gì ta chằm chằm thế?");
            }
            if (rate <= 0f && distance < EarshotDistance && !_chasing) rate = Overhear();
            else if (rate <= 0f && distance < 7f) rate = -3.5f;

            int value = _director.GetInt("nghi_ngo");
            if (_suspicionFloor < 0f) _suspicionFloor = value;
            _suspicionCarry += rate * Time.deltaTime;
            int step = (int)_suspicionCarry;
            if (step != 0)
            {
                _suspicionCarry -= step;
                if (step > 0)
                {
                    _suspicionFloor = Mathf.Min(MaxFloor, _suspicionFloor + step * FloorShare);
                    value = Mathf.Min(100, value + step);
                }
                // Standing close and behaving calms him, but only down to what he has already seen.
                else value = Mathf.Max(value + step, Mathf.Min(value, Mathf.FloorToInt(_suspicionFloor)));
                _director.SetInt("nghi_ngo", value);
            }
            float normalized = value / 100f;
            if (_chasing) return; // the chase owns the meter while it lasts
            _director.Hud.SetMeter(value >= 60 ? "MÃ GIÁM SINH ĐANG NGỜ VỰC" : "Nghi ngờ của Mã Giám Sinh",
                normalized, Color.Lerp(new Color(1f, 0.85f, 0.4f), new Color(0.9f, 0.2f, 0.15f), normalized));
            if (value >= 100)
                _director.Fail("Mã Giám Sinh đã sinh nghi", "Hắn nhìn bạn chằm chằm rồi ra lệnh cho tùy tùng kèm sát. Hãy kín đáo hơn: đi gần hắn, đừng quan sát khi hắn đang nhìn lại.");
        }

        /// <summary>
        /// Walking at his heels: every few seconds she catches another muttered line between the two men (each counted
        /// in "c2_nghe_duong"), but he feels her close behind and grows a little wary. Returns the suspicion rate.
        /// </summary>
        private float Overhear()
        {
            _earshotTime += Time.deltaTime;
            // Only while the two of them walk on ahead talking, not when he has stopped and turned to her.
            if (_overheard < Mutterings.Length && _earshotTime >= 4f + 6f * _overheard && !Glancing && _ma.IsMoving)
            {
                var (speaker, line) = Mutterings[_overheard++];
                _barkCooldown = 0f;
                Bark(speaker, line);
                _director.SetInt("c2_nghe_duong", _overheard);
            }
            // Staying close is the intended way to hear them; it should make him only faintly uneasy, not guarantee a
            // failure merely because the player followed the escort correctly for a minute.
            return 0.15f;
        }

        private void OnRestored()
        {
            _director.SetInt("nghi_ngo", Mathf.Min(_director.GetInt("nghi_ngo"), 45));
            _suspicionFloor = Mathf.Min(_suspicionFloor, 45f);
            _suspicionCarry = 0f;
            _glanceHold = 0f;
            _waiting = false;
            _retrievingPaper = false;
            _chasing = false;
            _strayTimer = 0f;
            // Bring the escort back beside Kiều so the restored state is readable.
            Vector3 player = _director.Player.transform.position;
            Vector3 forward = _director.Player.transform.forward;
            _ma.Place(player + forward * 3f, Quaternion.LookRotation(forward));
            _servant.Place(player + forward * 1.6f + Vector3.Cross(Vector3.up, forward) * 1.2f, Quaternion.LookRotation(forward));
            _waypoint = ClosestWaypoint(_ma.transform.position);
        }

        private int ClosestWaypoint(Vector3 position)
        {
            int best = _waypoint;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < _path.Length; i++)
            {
                float d = Flat(_path[i].position - position);
                if (d < bestDistance) { bestDistance = d; best = i; }
            }
            return Mathf.Min(best + 1, _path.Length - 1);
        }

        // ------------------------------------------------------------------ running away

        /// <summary>
        /// "Bỏ chạy" is a real attempt: she gets the road to herself, and after a moment behind the wall they notice and
        /// the servant runs her down. Caught, she is dragged back; outrun him and the dark road fills with what running
        /// costs (her father taken away again); stop and come back on her own and she simply walks back to the bench.
        /// The story then goes on as the mask either way. Writes c2_bo_chay: 1 caught · 2 the vision · 3 came back.
        /// </summary>
        private IEnumerator RunAway(StoryDirector director)
        {
            Transform kieu = director.Player.transform;
            Vector3 start = kieu.position;
            int outcome = 0;
            director.Hud.SetObjective("Chạy đi, khi họ còn ở sau vách");
            director.Hud.SetControls("WASD: chạy · Shift: chạy nhanh");
            director.SetFreeControl(true);
            _servant.EnableNavigation();
            float farthest = 0f;
            for (float t = 0f; outcome == 0; t += Time.deltaTime)
            {
                if (director.Autopilot) { outcome = 2; break; }
                float away = Flat(kieu.position - start);
                farthest = Mathf.Max(farthest, away);
                if (t >= NoticeAfter)
                {
                    if (!_chasing)
                    {
                        _chasing = true;
                        _shouted = false;
                        _barkCooldown = 0f;
                        Bark("MaGiamSinh", "Con bé đâu rồi? Đuổi theo!");
                        if (director.Audio != null) { director.Audio.PlaySfx("detect"); director.Audio.PlaySfx("heartbeat"); }
                        if (director.Fx != null) director.Fx.Shake(0.2f, 0.35f);
                    }
                    _servant.MoveTo(kieu.position, ChaseSpeed);
                    float gap = Flat(kieu.position - _servant.transform.position);
                    director.Hud.SetMeter("TÙY TÙNG ĐANG ĐUỔI THEO", Mathf.Clamp01(1f - gap / StrayDistance), new Color(0.9f, 0.2f, 0.15f));
                    if (!_shouted && gap < 6f) { _shouted = true; _barkCooldown = 0f; Bark("TuyTung", "Đứng lại đó! Chạy đâu cho thoát!"); }
                    if (gap < 1.3f) outcome = 1;
                }
                if (outcome == 0 && (t > RunLongest || away > RunFarthest)) outcome = 2;
                else if (outcome == 0 && farthest > 6f && away < 2.5f) outcome = 3;
                yield return null;
            }
            _chasing = false;
            director.SetFreeControl(false);
            _servant.Stop();
            if (outcome == 2) yield return CostOfRunning(director);

            yield return director.Hud.Fade(1f, director.Autopilot ? 0f : 0.5f);
            director.Player.Teleport(_bench.position, _bench.rotation);
            director.CameraController.SnapBehindTarget(_bench.eulerAngles.y);
            if (outcome == 1)
            {
                // Dragged back: he is waiting for her at the bench, his servant at her elbow.
                _ma.Place(_bench.position + _bench.forward * 1.6f, Quaternion.LookRotation(-_bench.forward));
                _servant.Place(_bench.position + _bench.right * 0.9f, Quaternion.LookRotation(-_bench.right));
            }
            else
            {
                _ma.Place(_maBehindWall.position, _maBehindWall.rotation);
                _servant.Place(_servantBehindWall.position, _servantBehindWall.rotation);
            }
            director.SetInt("c2_bo_chay", outcome);
            director.SetInt("nghi_ngo", Mathf.Min(100, director.GetInt("nghi_ngo") + (outcome == 1 ? 20 : outcome == 2 ? 10 : 5)));
            if (outcome == 1) director.SetInt("gia_vo", Mathf.Max(0, director.GetInt("gia_vo") - 1));
            else director.SetInt("tinh_tao", director.GetInt("tinh_tao") + 1);
            director.Hud.SetControls(Controls);
            yield return null;
            if (director.Shots != null) director.Shots.Reshoot();
            yield return director.Hud.Fade(0f, director.Autopilot ? 0f : 0.6f);
        }

        /// <summary>Far enough down the dark road, what running costs catches up with her: her father taken away again.</summary>
        private IEnumerator CostOfRunning(StoryDirector director)
        {
            string mood = director.Fx != null ? director.Fx.CurrentMood : null;
            if (director.Fx != null) director.Fx.SetMood("imagine");
            if (director.Audio != null) { director.Audio.Duck(0.3f); director.Audio.PlaySfx("heartbeat"); }
            bool fast = director.Autopilot;
            director.Hud.ShowBark("QuanSai", "Con gái lão Vương bỏ trốn! Khế ước còn đây, bạc phải hoàn. Giải lão lên huyện!", 3.5f);
            yield return new WaitForSeconds(fast ? 0f : 3.7f);
            director.Hud.ShowBark("VuongOng", "Kiều ơi... con đi đâu... Cha không trách con đâu...", 3.5f);
            yield return new WaitForSeconds(fast ? 0f : 3.7f);
            if (director.Fx != null) { director.Fx.SetMood(mood); director.Fx.Flash(0.35f); }
            if (director.Audio != null) director.Audio.Duck(1f);
        }

        // ------------------------------------------------------------------ staging

        private void OnScene(string scene)
        {
            ApplyLighting(scene);
            switch (scene)
            {
                case "Road_RestStop": StartCoroutine(Stage(() =>
                {
                    _director.Player.Teleport(_bench.position, _bench.rotation);
                    _director.CameraController.SnapBehindTarget(_bench.eulerAngles.y);
                    _ma.Place(_maBehindWall.position, _maBehindWall.rotation);
                    _servant.Place(_servantBehindWall.position, _servantBehindWall.rotation);
                    _ma.FaceTowards(_servant.transform.position);
                    _servant.FaceTowards(_ma.transform.position);
                }, 0.35f)); break;
                case "Road_MaskTest": StartCoroutine(Stage(() =>
                {
                    Transform kieu = _director.Player.transform;
                    Vector3 front = kieu.position + kieu.forward * 1.8f;
                    _ma.Place(front, Quaternion.LookRotation(-kieu.forward));
                    _servant.Place(front + kieu.forward * 1.5f + kieu.right * 1.1f, Quaternion.LookRotation(-kieu.forward));
                }, 0.3f)); break;
                case "Road_LamTriGate": StartCoroutine(Stage(() =>
                {
                    _director.Player.Teleport(_gatePlayer.position, _gatePlayer.rotation);
                    _director.CameraController.SnapBehindTarget(_gatePlayer.eulerAngles.y);
                    _ma.Place(_gateMa.position, _gateMa.rotation);
                    _servant.Place(_gateServant.position, _gateServant.rotation);
                    if (_director.Audio != null)
                    {
                        _director.Audio.SetAmbience("night");
                        _director.Audio.PlaySfx("door_close", 0.75f);
                        _director.Audio.PlaySfx("heartbeat", 0.3f);
                    }
                    if (_director.Fx != null) _director.Fx.Shake(0.14f, 0.45f);
                }, 1.1f)); break;
            }
        }

        private IEnumerator Stage(System.Action move, float fade)
        {
            yield return _director.Hud.Fade(1f, fade * 0.5f);
            move();
            yield return null; // let the moved rigs pose before framing them
            // The line's shot was taken before the people moved: frame them where they now stand.
            if (_director.Shots != null) _director.Shots.Reshoot();
            yield return new WaitForSeconds(_director.Autopilot ? 0f : fade * 0.3f);
            yield return _director.Hud.Fade(0f, fade);
        }

        private void ApplyLighting(string scene)
        {
            foreach (var preset in _lighting)
            {
                if (preset.Scene != scene) continue;
                _sun.color = preset.Sun;
                _sun.intensity = preset.Intensity;
                _sun.transform.rotation = Quaternion.Euler(preset.Angles);
                RenderSettings.ambientLight = preset.Ambient;
                RenderSettings.fogColor = preset.Fog;
                RenderSettings.fogDensity = preset.FogDensity;
                if (Camera.main != null) Camera.main.backgroundColor = preset.Fog;
                if (preset.Sky != null) RenderSettings.skybox = preset.Sky;
                bool night = scene == "Road_LamTriGate";
                foreach (var light in _nightLights) if (light != null) light.SetActive(night);
                if (_nightCharacterFill != null) _nightCharacterFill.SetActive(night);
                return;
            }
        }

        // ------------------------------------------------------------------ helpers

        private void Bark(string speaker, string text)
        {
            if (_barkCooldown > 0f) return;
            _barkCooldown = 4f;
            _director.Hud.ShowBark(speaker, text);
        }

        private static float Flat(Vector3 v) { v.y = 0; return v.magnitude; }

        public IEnumerable<string> SummaryLines(Ink.Runtime.Story story)
        {
            int clues = (int)story.variablesState["manh_moi"];
            int deduction = (int)story.variablesState["suy_luan"];
            int suspicion = (int)story.variablesState["nghi_ngo"];
            int acting = (int)story.variablesState["gia_vo"];
            yield return "Manh mối thu thập: " + clues + " / 4";
            yield return "Bảng suy luận: " + deduction + " / 3 mảnh khớp";
            yield return "Diễn vai ngây thơ: " + acting + " / 3 câu trả lời trót lọt";
            yield return "Nghi ngờ của Mã Giám Sinh: " + Mathf.Clamp(suspicion, 0, 100) + "%" + (suspicion >= 60 ? " — hắn sẽ để mắt tới bạn" : " — hắn tin bạn là con cừu");
            int overheard = (int)story.variablesState["c2_nghe_duong"];
            if (overheard > 0) yield return "Nghe lỏm trên đường: " + overheard + " / " + Mutterings.Length + " câu thì thầm giữa Mã và tùy tùng.";
            if ((bool)story.variablesState["c2_giay_mat"]) yield return "Mẩu giấy giao hàng: bạn tụt lại quá xa, tùy tùng đã nhặt mất.";
            switch ((int)story.variablesState["c2_bo_chay"])
            {
                case 1: yield return "Bỏ chạy: tên tùy tùng đuổi kịp và lôi bạn về."; break;
                case 2: yield return "Bỏ chạy: giữa đường tối, bạn thấy cha bị giải đi lần nữa, và tự dừng lại."; break;
                case 3: yield return "Bỏ chạy: bạn tự quay về trước khi ai kịp đuổi."; break;
            }
            yield return (bool)story.variablesState["giu_mau_giay"] ? "Bạn đang giấu mẩu giấy giao hàng — một bằng chứng." : "Mẩu giấy đã trở về tay hắn, nhưng bạn nhớ từng chữ.";
        }
    }
}
