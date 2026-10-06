using System.Collections;
using System.Collections.Generic;
using ThuyKieu.Core.Story;
using ThuyKieu.Interaction;
using ThuyKieu.Player;
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

        private int _waypoint;
        private float _glanceTimer = 10f, _glanceHold, _barkCooldown;
        private bool _waiting, _restCalled, _paperDropped;
        private float _suspicionCarry;

        public bool Glancing => _glanceHold > 0f;
        public int Waypoint => _waypoint;

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
            _director.Player.AllowCrouch = true;
            _director.Hud.SetControls("WASD: đi · Shift: chạy · Chuột phải: quan sát · Ctrl: khom · E: tương tác · J: sổ tay");
            ApplyLighting("Road_Departure");
        }

        private void Update()
        {
            if (_barkCooldown > 0f) _barkCooldown -= Time.deltaTime;
            if (!_director.InExplore)
            {
                if (!_director.IsBusy || _director.CurrentSequence == null) HoldEscort();
                return;
            }
            UpdateEscort();
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
                else if (!_ma.IsMoving) _ma.MoveTo(target, 1.45f);

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
            FollowServant(true);
        }

        private void FollowServant(bool follow)
        {
            Vector3 slot = _ma.transform.position - _ma.transform.forward * 1.7f + _ma.transform.right * 0.9f;
            if (!follow || Flat(slot - _servant.transform.position) < 0.8f) { if (_servant.IsMoving && !follow) _servant.Stop(); return; }
            _servant.MoveTo(slot, 1.7f);
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
        }

        // ------------------------------------------------------------------ suspicion

        private void UpdateSuspicion()
        {
            Vector3 player = _director.Player.transform.position;
            float distance = Flat(player - _ma.transform.position);
            float rate = 0f;
            if (distance > 14f) rate += 14f;
            else if (distance > 9f) rate += 6f;
            bool watched = Glancing || Vector3.Dot(_ma.transform.forward, (player - _ma.transform.position).normalized) > 0.6f;
            if (_observe != null && _observe.IsObserving && watched && distance < 11f)
            {
                rate += 24f;
                if (_barkCooldown <= 0f) Bark("MaGiamSinh", "Nàng nhìn gì ta chằm chằm thế?");
            }
            if (rate <= 0f && distance < 7f) rate = -3.5f;

            int value = _director.GetInt("nghi_ngo");
            _suspicionCarry += rate * Time.deltaTime;
            int step = (int)_suspicionCarry;
            if (step != 0)
            {
                _suspicionCarry -= step;
                value = Mathf.Clamp(value + step, 0, 100);
                _director.SetInt("nghi_ngo", value);
            }
            float normalized = value / 100f;
            _director.Hud.SetMeter(value >= 60 ? "MÃ GIÁM SINH ĐANG NGỜ VỰC" : "Nghi ngờ của Mã Giám Sinh",
                normalized, Color.Lerp(new Color(1f, 0.85f, 0.4f), new Color(0.9f, 0.2f, 0.15f), normalized));
            if (value >= 100)
                _director.Fail("Mã Giám Sinh đã sinh nghi", "Hắn nhìn bạn chằm chằm rồi ra lệnh cho tùy tùng kèm sát. Hãy kín đáo hơn: đi gần hắn, đừng quan sát khi hắn đang nhìn lại.");
        }

        private void OnRestored()
        {
            _director.SetInt("nghi_ngo", Mathf.Min(_director.GetInt("nghi_ngo"), 45));
            _suspicionCarry = 0f;
            _glanceHold = 0f;
            _waiting = false;
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
                    if (_director.Audio != null) _director.Audio.SetAmbience("night");
                }, 1.1f)); break;
            }
        }

        private IEnumerator Stage(System.Action move, float fade)
        {
            yield return _director.Hud.Fade(1f, fade * 0.5f);
            move();
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
            yield return (bool)story.variablesState["giu_mau_giay"] ? "Bạn đang giấu mẩu giấy giao hàng — một bằng chứng." : "Mẩu giấy đã trở về tay hắn, nhưng bạn nhớ từng chữ.";
        }
    }
}
