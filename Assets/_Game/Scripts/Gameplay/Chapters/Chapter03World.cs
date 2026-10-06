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
        [Tooltip("Ground cone showing where the guard looks: faint in the open yard, strong in forbidden areas or when he is suspicious.")]
        [SerializeField] private Renderer _visionCone;
        [Tooltip("Door of Tú Bà's room: shut (and barred) once she and Mã Giám Sinh are inside, so the side window is the only way to listen.")]
        [SerializeField] private GameObject _tuBaDoor;
        [Tooltip("Box of Tú Bà's room: the player must be outside it before the door shuts.")]
        [SerializeField] private Bounds _tuBaRoomBounds = new Bounds(new Vector3(10.5f, 1.5f, 7.8f), new Vector3(6f, 4f, 9.4f));
        [Tooltip("Sight of the people inside the room; only watches during the eavesdrop.")]
        [SerializeField] private VisionSensor[] _roomSensors;

        private MaterialPropertyBlock _coneBlock;
        private float _coneAlpha = 0.06f;

        private bool _movedInside, _beckoned, _doorShut, _doorHinted, _shooed;
        private float _barkCooldown;
        private string _lastSequence;

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
            if (_tuBaDoor != null) _tuBaDoor.SetActive(false);
            if (_roomSensors != null) foreach (var sensor in _roomSensors) if (sensor != null) sensor.Active = false;
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
            string sequence = _director.CurrentSequence;
            if (sequence == "eavesdrop_c3" && _lastSequence != sequence) BeginEavesdrop();
            _lastSequence = sequence;
            if (sequence == "eavesdrop_c3") _guard.Patrolling = true;
            if (_movedInside && !_doorShut) TryShutDoor();
            if (!_director.InExplore) return;
            HintAtDoor();

            var vision = _guard.Vision;
            if (vision.Awareness > 0.04f)
            {
                _director.Hud.SetMeter(vision.Awareness > 0.6f ? "NGƯỜI CANH ĐANG TIẾN LẠI!" : "Người canh nghi ngờ...", vision.Awareness, new Color(0.9f, 0.25f, 0.2f));
                if (vision.Awareness > 0.5f && _barkCooldown <= 0f) { _barkCooldown = 5f; _director.Hud.ShowBark("NguoiCanh", "Ai đấy? Đứng lại!"); }
            }
            else if (RestrictedZone.PlayerInside)
                _director.Hud.SetMeter("Khu vực cấm: " + RestrictedZone.CurrentLabel, 0f, new Color(1f, 0.8f, 0.45f));

            UpdateCone(RestrictedZone.PlayerInside || vision.Awareness > 0.04f || _director.CurrentSequence == "eavesdrop_c3");

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

        private bool Arrived(StoryActor actor, Transform spot)
        {
            Vector3 offset = actor.transform.position - spot.position;
            offset.y = 0f;
            return offset.sqrMagnitude < 0.6f * 0.6f || !actor.IsMoving;
        }

        /// <summary>Once both are in the room the door is barred, but only after Tú Bà has shooed the player out.</summary>
        private void TryShutDoor()
        {
            if (_tuBaDoor == null || !Arrived(_tuBa, _tuBaRoom) || !Arrived(_ma, _maRoom)) return;
            if (_tuBaRoomBounds.Contains(_director.Player.transform.position + Vector3.up))
            {
                if (!_shooed && _director.InExplore)
                {
                    _shooed = true;
                    _director.Hud.ShowBark("TuBa", "Con ra ngoài sân chơi đi. Người lớn đang bàn việc.", 3.5f);
                }
                return;
            }
            _doorShut = true;
            _tuBaDoor.SetActive(true);
            _tuBa.FaceTowards(_ma.transform.position);
            _ma.FaceTowards(_tuBa.transform.position);
            if (_director.Audio != null) _director.Audio.PlaySfx("lock_rattle");
        }

        private void HintAtDoor()
        {
            if (!_doorShut || _doorHinted || !_director.IsExploreAvailable("c3_eavesdrop")) return;
            if (Vector3.Distance(_director.Player.transform.position, _tuBaDoor.transform.position) > 2.2f) return;
            _doorHinted = true;
            _director.Hud.ShowBark("NoiTam", "Cửa cài then. Trong kia có tiếng đếm bạc... Cửa sổ bên hông thì sao?", 4f);
        }

        /// <summary>The conversation needs both of them in the room: if the player got there first, they are already inside.</summary>
        private void BeginEavesdrop()
        {
            _movedInside = true;
            if (Vector3.Distance(_tuBa.transform.position, _tuBaRoom.position) > 0.6f) _tuBa.Place(_tuBaRoom.position, _tuBaRoom.rotation);
            if (Vector3.Distance(_ma.transform.position, _maRoom.position) > 0.6f) _ma.Place(_maRoom.position, _maRoom.rotation);
            _shooed = true;
            if (!_doorShut) TryShutDoor();
            _tuBa.FaceTowards(_ma.transform.position);
            _ma.FaceTowards(_tuBa.transform.position);
        }

        private void UpdateCone(bool alert)
        {
            if (_visionCone == null) return;
            _coneBlock ??= new MaterialPropertyBlock();
            _coneAlpha = Mathf.MoveTowards(_coneAlpha, alert ? 0.2f : 0.06f, Time.deltaTime * 0.5f);
            _coneBlock.SetColor("_BaseColor", new Color(1f, 0.2f, 0.12f, _coneAlpha));
            _visionCone.SetPropertyBlock(_coneBlock);
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
            // Darken a runtime copy of the sky so the shared asset is never modified.
            Material sky = RenderSettings.skybox != null && RenderSettings.skybox.HasProperty("_Exposure") ? new Material(RenderSettings.skybox) : null;
            if (sky != null) RenderSettings.skybox = sky;
            float exposure = sky != null ? sky.GetFloat("_Exposure") : 1f;
            for (float t = 0; t < 1f; t += Time.deltaTime / 3f)
            {
                _sun.color = Color.Lerp(sun, _duskSun, t);
                _sun.intensity = Mathf.Lerp(intensity, intensity * 0.55f, t);
                RenderSettings.ambientLight = Color.Lerp(ambient, _duskAmbient, t);
                if (sky != null) sky.SetFloat("_Exposure", Mathf.Lerp(exposure, exposure * 0.55f, t));
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
