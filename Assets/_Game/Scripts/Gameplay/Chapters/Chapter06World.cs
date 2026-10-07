using System.Collections;
using System.Collections.Generic;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Chase;
using ThuyKieu.Gameplay.Stealth;
using Unity.AI.Navigation;
using UnityEngine;

namespace ThuyKieu.Gameplay.Chapters
{
    /// <summary>
    /// Chapter 6 — Kim Trọng. Moves the story between the temple at dawn, the study and the street with fades,
    /// has Kim Trọng walk beside Kiều on the street, and keeps one of Tú Bà's men searching the crowd
    /// (he only notices someone running).
    /// </summary>
    public class Chapter06World : MonoBehaviour, IChapterSummary
    {
        [SerializeField] private StoryDirector _director;
        [SerializeField] private NavMeshSurface _navMesh;
        [SerializeField] private StoryActor _kimTrong;
        [SerializeField] private Follower _kimFollower;
        [SerializeField] private Transform _kimArrival, _kimTempleSpot;
        [SerializeField] private StoryActor _coLam;
        [SerializeField] private StoryActor _messenger;
        [SerializeField] private GuardPatrol _searcher;
        [SerializeField] private Transform _studyPlayer, _studyKim, _streetPlayer, _nightPlayer, _nightKim;
        [SerializeField] private Light _sun;
        [SerializeField] private GameObject _studyLamp;

        private bool _onStreet;
        private float _barkCooldown;

        private void OnEnable()
        {
            _director.SceneChanged += OnScene;
            _director.CheckpointRestored += OnRestored;
            _director.ExploreChanged += OnExplore;
        }

        private void OnDisable()
        {
            _director.SceneChanged -= OnScene;
            _director.CheckpointRestored -= OnRestored;
            _director.ExploreChanged -= OnExplore;
        }

        private void Start()
        {
            LightExposure.Enable(false);
            if (_navMesh != null) _navMesh.BuildNavMesh();
            foreach (var actor in FindObjectsByType<StoryActor>()) actor.EnableNavigation();
            _director.Player.AllowCrouch = false;
            _director.Hud.SetControls("WASD: đi · E: nói chuyện / tương tác · Chuột phải: quan sát · J: sổ tay · Đừng chạy trên phố");
            _kimFollower.Following = false;
            _messenger.gameObject.SetActive(false);
            _searcher.Patrolling = false;
            _studyLamp.SetActive(false);
            bool lam = _director.GetBool("c4_ru_co_lam") && _director.GetBool("c5_co_lam_thoat");
            _coLam.gameObject.SetActive(lam);
            if (_director.Audio != null) _director.Audio.SetAmbience("temple");
            // Kim Trọng hurries in through the temple gate while Kiều wakes.
            _kimTrong.Place(_kimArrival.position, _kimArrival.rotation);
            _kimTrong.MoveTo(_kimTempleSpot.position, 2.4f);
        }

        private void OnExplore(bool exploring)
        {
            _searcher.Patrolling = exploring && _onStreet;
            _kimFollower.Following = exploring && _onStreet;
        }

        private void Update()
        {
            if (_barkCooldown > 0f) _barkCooldown -= Time.deltaTime;
            if (!_director.InExplore || !_onStreet) return;
            var vision = _searcher.Vision;
            if (vision.Awareness > 0.04f)
            {
                _director.Hud.SetMeter(vision.Awareness > 0.6f ? "NGƯỜI CỦA TÚ BÀ ĐANG NHÌN VỀ PHÍA BẠN!" : "Có người đang để ý bạn...", vision.Awareness, new Color(0.9f, 0.25f, 0.2f));
                if (vision.Awareness > 0.5f && _barkCooldown <= 0f) { _barkCooldown = 6f; _director.Hud.ShowBark("NguoiCanh", "Khoan... cô gái đội nón kia, đứng lại xem nào!"); }
            }
            if (vision.Spotted)
                _director.Fail("Người của Tú Bà nhận ra bạn!", "Trên phố đông người, chạy là bị chú ý. Đi thong thả, cúi đầu, để Kim Trọng đi cạnh.");
        }

        private void OnScene(string scene)
        {
            switch (scene)
            {
                case "KimTrong_Study": StartCoroutine(Cut(_studyPlayer, _studyKim, "house", false)); break;
                case "LamTri_Street": StartCoroutine(Cut(_streetPlayer, null, "street", true)); break;
                case "KimTrong_Night": StartCoroutine(Night()); break;
            }
        }

        private IEnumerator Cut(Transform player, Transform kim, string ambience, bool street)
        {
            yield return _director.Hud.Fade(1f, 0.35f);
            _onStreet = street;
            _director.Player.Teleport(player.position, player.rotation);
            _director.CameraController.SnapBehindTarget(player.eulerAngles.y);
            if (kim != null) _kimTrong.Place(kim.position, kim.rotation);
            else _kimFollower.Snap();
            if (_director.Audio != null) _director.Audio.SetAmbience(ambience);
            yield return _director.Hud.Fade(0f, 0.6f);
        }

        private IEnumerator Night()
        {
            yield return _director.Hud.Fade(1f, 0.5f);
            _onStreet = false;
            _searcher.Patrolling = false;
            _searcher.gameObject.SetActive(false);
            _kimFollower.Following = false;
            _director.Player.Teleport(_nightPlayer.position, _nightPlayer.rotation);
            _director.CameraController.SnapBehindTarget(_nightPlayer.eulerAngles.y);
            _kimTrong.Place(_nightKim.position, _nightKim.rotation);
            _messenger.gameObject.SetActive(true);
            _studyLamp.SetActive(true);
            if (_sun != null) { _sun.intensity = 0.4f; _sun.color = new Color(0.55f, 0.65f, 0.95f); }
            RenderSettings.ambientLight = new Color(0.13f, 0.14f, 0.22f);
            if (_director.Fx != null) _director.Fx.SetMood("tension");
            if (_director.Audio != null) _director.Audio.SetAmbience("night");
            yield return _director.Hud.Fade(0f, 0.8f);
        }

        private void OnRestored()
        {
            _director.Player.Teleport(_streetPlayer.position, _streetPlayer.rotation);
            _director.CameraController.SnapBehindTarget(_streetPlayer.eulerAngles.y);
            _searcher.ResetPatrol();
            _kimFollower.Snap();
        }

        public IEnumerable<string> SummaryLines(Ink.Runtime.Story story)
        {
            var v = story.variablesState;
            int trust = (int)v["kt_tin_tuong"], dossier = (int)v["c6_ho_so"], witnesses = (int)v["ev_nhan_chung"];
            yield return "Kim Trọng tin bạn: " + (trust >= 5 ? "trọn vẹn." : trust >= 3 ? "nhiều." : "còn dè dặt.") + ((bool)v["kt_biet_xuyen_khong"] ? " Chàng biết bạn đến từ nơi rất xa." : string.Empty);
            yield return "Đối chiếu giấy tờ: tìm ra " + (int)v["c6_doi_chieu"] + "/3 chỗ mâu thuẫn.";
            yield return "Nhân chứng: " + witnesses + " người.";
            yield return "Hồ sơ cho huyện đường: " + dossier + "/4 loại bằng chứng.";
            yield return "<i>Chương tiếp theo — Nghịch Mệnh: sáng mai, huyện đường.</i>";
        }
    }
}
