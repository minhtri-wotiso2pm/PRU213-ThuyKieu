using System.Collections;
using System.Collections.Generic;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Stealth;
using Unity.AI.Navigation;
using UnityEngine;

namespace ThuyKieu.Gameplay.Chapters
{
    /// <summary>
    /// Chapter 3 — Tú Bà's house. Stages the welcome, sends Tú Bà and Mã Giám Sinh to her room,
    /// runs the guard patrol and trespass detection, and makes Mã Kiều beckon when she is ready.
    /// </summary>
    public class Chapter03World : MonoBehaviour, IChapterSummary
    {
        [SerializeField] private StoryDirector _director;
        [SerializeField] private NavMeshSurface _navMesh;
        [SerializeField] private StoryActor _tuBa, _ma, _maKieu;
        [SerializeField] private GuardPatrol _guard;
        [SerializeField] private Transform _tuBaRoom, _maRoom, _playerRoom;
        [SerializeField] private GameObject _maKieuMarker;
        [SerializeField] private Light _sun;
        [SerializeField] private Color _duskSun = new Color(1f, 0.42f, 0.25f);
        [SerializeField] private Color _duskAmbient = new Color(0.32f, 0.16f, 0.18f);

        private bool _movedInside, _beckoned;
        private float _barkCooldown;

        private void OnEnable()
        {
            _director.ExploreChanged += OnExplore;
            _director.SceneChanged += OnScene;
            _director.CheckpointRestored += OnRestored;
        }

        private void OnDisable()
        {
            _director.ExploreChanged -= OnExplore;
            _director.SceneChanged -= OnScene;
            _director.CheckpointRestored -= OnRestored;
        }

        private void Start()
        {
            if (_navMesh != null) _navMesh.BuildNavMesh();
            foreach (var actor in FindObjectsByType<StoryActor>()) actor.EnableNavigation();
            _director.Player.AllowCrouch = true;
            _director.Hud.SetControls("WASD: đi · Shift: chạy · Ctrl: khom (khó bị thấy) · E: tương tác · J: sổ tay");
            if (_maKieuMarker != null) _maKieuMarker.SetActive(false);
            if (_director.Audio != null) _director.Audio.SetAmbience("house");
        }

        private void OnExplore(bool exploring)
        {
            _guard.Patrolling = exploring || _director.CurrentSequence == "eavesdrop_c3";
            if (exploring && !_movedInside)
            {
                // After the welcome, the two of them withdraw to talk business.
                _movedInside = true;
                _tuBa.MoveTo(_tuBaRoom.position, 1.1f);
                _ma.MoveTo(_maRoom.position, 1.3f);
            }
        }

        private void Update()
        {
            if (_barkCooldown > 0f) _barkCooldown -= Time.deltaTime;
            if (_director.CurrentSequence == "eavesdrop_c3") _guard.Patrolling = true;
            if (!_director.InExplore) return;

            var vision = _guard.Vision;
            if (vision.Awareness > 0.04f)
            {
                _director.Hud.SetMeter(vision.Awareness > 0.6f ? "NGƯỜI CANH ĐANG TIẾN LẠI!" : "Người canh nghi ngờ...", vision.Awareness, new Color(0.9f, 0.25f, 0.2f));
                if (vision.Awareness > 0.5f && _barkCooldown <= 0f) { _barkCooldown = 5f; _director.Hud.ShowBark("NguoiCanh", "Ai đấy? Đứng lại!"); }
            }
            else if (RestrictedZone.PlayerInside)
                _director.Hud.SetMeter("Khu vực cấm: " + RestrictedZone.CurrentLabel, 0f, new Color(1f, 0.8f, 0.45f));

            if (vision.Spotted)
                _director.Fail("Bị người canh bắt gặp!", "\"Cô em đi đâu đấy? Về phòng!\" — Hắn lôi bạn về phòng. Hãy quan sát đường tuần của hắn và khom người khi lẻn đi.");

            bool ready = _director.IsExploreAvailable("c3_makieu");
            if (_maKieuMarker != null && _maKieuMarker.activeSelf != ready) _maKieuMarker.SetActive(ready);
            if (ready && !_beckoned)
            {
                _beckoned = true;
                _director.Hud.ShowBark("MaKieu", "Này... cô gái mới. Lại đây, ta có chuyện muốn nói.", 4f);
                _director.Hud.SetObjective("Một cô gái lạ ở góc sân đang vẫy gọi bạn");
            }
        }

        private void OnRestored()
        {
            _director.Player.Teleport(_playerRoom.position, _playerRoom.rotation);
            _director.CameraController.SnapBehindTarget(_playerRoom.eulerAngles.y);
            RestrictedZone.ResetPresence();
            _guard.ResetPatrol();
            _director.SetInt("c3_bi_bat", _director.GetInt("c3_bi_bat") + 1);
        }

        private void OnScene(string scene)
        {
            if (scene == "TuBa_MaKieu") StartCoroutine(Dusk());
        }

        private IEnumerator Dusk()
        {
            Color sun = _sun.color, ambient = RenderSettings.ambientLight;
            float intensity = _sun.intensity;
            for (float t = 0; t < 1f; t += Time.deltaTime / 3f)
            {
                _sun.color = Color.Lerp(sun, _duskSun, t);
                _sun.intensity = Mathf.Lerp(intensity, intensity * 0.55f, t);
                RenderSettings.ambientLight = Color.Lerp(ambient, _duskAmbient, t);
                yield return null;
            }
        }

        public IEnumerable<string> SummaryLines(Ink.Runtime.Story story)
        {
            int tasks = (int)story.variablesState["c3_viec"];
            int caught = (int)story.variablesState["c3_bi_bat"];
            int answer = (int)story.variablesState["tra_loi_kieu_that"];
            yield return "Việc đã làm trong nhà Tú Bà: " + tasks;
            yield return (bool)story.variablesState["c3_khe_uoc"] ? "Đã tận mắt đọc văn tự bán thân." : "Chưa tự tìm thấy văn tự — Mã Kiều đã chỉ chỗ.";
            if ((bool)story.variablesState["c3_biet_chia_khoa"]) yield return "Biết chìa khóa tủ nằm ở thắt lưng Tú Bà.";
            yield return "Số lần bị người canh bắt gặp: " + caught;
            yield return "Bạn đáp lời Kiều thật: " + (answer == 1 ? "nhận mình đã sai." : answer == 2 ? "không chịu buông xuôi." : "bằng im lặng.");
            yield return "<i>Chương tiếp theo — Không còn đường lui: đêm nay người canh sẽ uống rượu...</i>";
        }
    }
}
