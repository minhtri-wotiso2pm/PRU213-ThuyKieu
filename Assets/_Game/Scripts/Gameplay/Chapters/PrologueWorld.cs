using System.Collections;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Stealth;
using ThuyKieu.Player;
using Unity.AI.Navigation;
using UnityEngine;

namespace ThuyKieu.Gameplay.Chapters
{
    /// <summary>
    /// Prologue — Trang sách. The afternoon classroom (the modern student at her desk, the class leaving after the bell),
    /// then, through the light of the book, Kiều's room on the morning of the arrest, the Vương courtyard where the
    /// officials take the father and brother, and the rain that falls that afternoon (Chapter 1 begins in it).
    /// </summary>
    public class PrologueWorld : MonoBehaviour, IChapterSummary
    {
        [SerializeField] private StoryDirector _director;
        [SerializeField] private NavMeshSurface _navMesh;
        [SerializeField] private PlayerLook _look;

        [Header("Classroom")]
        [SerializeField] private Transform _seat;
        [SerializeField] private StoryActor _ngan;
        [SerializeField] private StoryActor _teacher;
        [SerializeField] private StoryActor[] _classmates;
        [SerializeField] private Transform _classDoor;
        [SerializeField] private GameObject _bookGlow;

        [Header("Vương house")]
        [SerializeField] private Transform _wake, _courtyard;
        [SerializeField] private StoryActor _father, _brother, _mother;
        [SerializeField] private Transform _fatherSpot, _brotherSpot, _motherSpot, _motherRainSpot;
        [SerializeField] private StoryActor[] _officials;
        [SerializeField] private Transform[] _officialSpots;
        [SerializeField] private Transform _gate;
        [SerializeField] private Light _sun;
        [SerializeField] private ParticleSystem _rain;
        [SerializeField] private GameObject _rainProps;

        private bool _seated = true, _classLeft, _nganLeft;

        private void OnEnable()
        {
            _director.SceneChanged += OnScene;
            _director.ExploreChanged += OnExplore;
            _director.TagReceived += OnTag;
        }

        private void OnDisable()
        {
            _director.SceneChanged -= OnScene;
            _director.ExploreChanged -= OnExplore;
            _director.TagReceived -= OnTag;
        }

        private IEnumerator Start()
        {
            LightExposure.Enable(false);
            if (_navMesh != null) _navMesh.BuildNavMesh();
            foreach (var actor in FindObjectsByType<StoryActor>()) actor.EnableNavigation();
            _director.Player.AllowCrouch = false;
            _director.Hud.SetControls("WASD: đi · E: tương tác · Chuột phải: quan sát · Space: tiếp lời thoại");
            _look.ShowStudent(true);
            foreach (var official in _officials) official.gameObject.SetActive(false);
            if (_rain != null) _rain.Stop();
            if (_rainProps != null) _rainProps.SetActive(false);
            if (_bookGlow != null) _bookGlow.SetActive(false);
            if (_director.Audio != null) _director.Audio.SetAmbience("classroom");
            // Everyone at their desks for the lesson: the animator needs one frame before the sit trigger takes.
            yield return null;
            _director.Player.Teleport(_seat.position, _seat.rotation);
            _director.Player.GetComponent<StoryActor>().Sit(true);
            _ngan.Sit(true);
            foreach (var classmate in _classmates) classmate.Sit(true);
        }

        private void OnExplore(bool exploring)
        {
            if (!exploring) return;
            if (_seated) StartCoroutine(StandUp());
            // The deskmate goes home once she has said goodbye.
            if (!_nganLeft && _director.Story.state.VisitCountAtPathString("pl_ngan") > 0) { _nganLeft = true; Leave(_ngan, _classDoor); }
        }

        /// <summary>After the bell: the student gets up from her desk, the class and the teacher file out.</summary>
        private IEnumerator StandUp()
        {
            _seated = false;
            var player = _director.Player;
            player.ControlLocked = true;
            player.GetComponent<StoryActor>().Sit(false);
            _ngan.Sit(false);
            if (!_classLeft)
            {
                _classLeft = true;
                for (int i = 0; i < _classmates.Length; i++) StartCoroutine(LeaveLater(_classmates[i], 0.6f + i * 0.45f));
                StartCoroutine(LeaveLater(_teacher, 1.2f));
            }
            yield return new WaitForSeconds(_director.Autopilot ? 0f : 1.6f);
            if (_director.InExplore) player.ControlLocked = false;
        }

        private IEnumerator LeaveLater(StoryActor actor, float delay)
        {
            actor.Sit(false);
            yield return new WaitForSeconds(_director.Autopilot ? 0f : delay + 1.3f);
            Leave(actor, _classDoor);
        }

        private void Leave(StoryActor actor, Transform door)
        {
            if (actor == null || !actor.isActiveAndEnabled) return;
            actor.Sit(false);
            StartCoroutine(actor.HasRig ? WalkOut(actor, door) : SlipAway(actor));
        }

        /// <summary>A posed (unrigged) classmate cannot walk, so it is gone the first moment the camera looks elsewhere.</summary>
        private IEnumerator SlipAway(StoryActor actor)
        {
            var renderers = actor.GetComponentsInChildren<Renderer>();
            for (float t = 0; t < 30f && actor.isActiveAndEnabled; t += Time.deltaTime)
            {
                bool seen = false;
                foreach (var r in renderers) seen |= r.isVisible;
                if (!seen || _director.Autopilot) break;
                yield return null;
            }
            actor.gameObject.SetActive(false);
        }

        private IEnumerator WalkOut(StoryActor actor, Transform door)
        {
            actor.MoveTo(door.position, 1.3f);
            for (float t = 0; t < 12f && actor.isActiveAndEnabled; t += Time.deltaTime)
            {
                if (t > 1f && !actor.IsMoving) break;
                yield return null;
            }
            actor.gameObject.SetActive(false);
        }

        private void OnTag(string key, string value)
        {
            if (key == "gameplay" && value == "book_glow" && _bookGlow != null) _bookGlow.SetActive(true);
            // Picking up the hồ cầm: a few notes ring out.
            if (key == "camera" && value == "Dan" && _director.Audio != null) _director.Audio.PlaySfx("dan");
            // The gong at the gate: the officials burst into the courtyard.
            if (key == "sfx" && value == "gong" && !_officials[0].gameObject.activeSelf) StartCoroutine(OfficialsArrive());
        }

        private IEnumerator OfficialsArrive()
        {
            for (int i = 0; i < _officials.Length; i++)
            {
                _officials[i].gameObject.SetActive(true);
                _officials[i].Place(_gate.position + _gate.right * (i - 0.5f) * 0.9f, _gate.rotation);
                _officials[i].EnableNavigation();
                _officials[i].MoveTo(_officialSpots[i].position, 2.6f);
            }
            yield return null;
        }

        private void OnScene(string scene)
        {
            switch (scene)
            {
                case "VuongGia_San": StartCoroutine(Cut(_courtyard, "courtyard")); break;
                case "VuongGia_Mua": StartCoroutine(Rain()); break;
            }
        }

        /// <summary>The light of the book: white, then Kiều's room in the morning, as Kiều (the "into_room" beat).</summary>
        public IEnumerator IntoKieu()
        {
            _director.Hud.SetFadeColor(new Color(1f, 0.97f, 0.9f));
            yield return _director.Hud.Fade(1f, _director.Autopilot ? 0f : 0.6f);
            if (_bookGlow != null) _bookGlow.SetActive(false);
            _look.ShowStudent(false);
            _director.Player.Teleport(_wake.position, _wake.rotation);
            _director.CameraController.SnapBehindTarget(_wake.eulerAngles.y);
            _father.Place(_fatherSpot.position, _fatherSpot.rotation);
            _brother.Place(_brotherSpot.position, _brotherSpot.rotation);
            _mother.Place(_motherSpot.position, _motherSpot.rotation);
            if (_director.Audio != null) _director.Audio.SetAmbience("house");
            yield return _director.Hud.Card("MỞ ĐẦU · II", "PHÒNG KIỀU", true, 2.2f);
            _director.Hud.SetFadeColor(Color.black);
            _director.Hud.SetChapterLabel("Mở đầu · Phòng Kiều");
        }

        private IEnumerator Cut(Transform spot, string ambience)
        {
            yield return _director.Hud.Fade(1f, 0.35f);
            _director.Player.Teleport(spot.position, spot.rotation);
            _director.CameraController.SnapBehindTarget(spot.eulerAngles.y);
            if (_director.Audio != null) _director.Audio.SetAmbience(ambience);
            yield return _director.Hud.Fade(0f, 0.6f);
        }

        /// <summary>That afternoon: rain on the empty courtyard, the mother on the steps.</summary>
        private IEnumerator Rain()
        {
            yield return _director.Hud.Fade(1f, 0.6f);
            foreach (var official in _officials) official.gameObject.SetActive(false);
            _father.gameObject.SetActive(false);
            _brother.gameObject.SetActive(false);
            _mother.Place(_motherRainSpot.position, _motherRainSpot.rotation);
            if (_sun != null) { _sun.intensity = 0.35f; _sun.color = new Color(0.62f, 0.68f, 0.8f); }
            RenderSettings.ambientLight = new Color(0.3f, 0.33f, 0.4f);
            RenderSettings.fogColor = new Color(0.42f, 0.45f, 0.5f);
            RenderSettings.fogDensity = 0.025f;
            if (_rain != null) _rain.Play();
            if (_rainProps != null) _rainProps.SetActive(true);
            if (_director.Fx != null) _director.Fx.SetMood("tension");
            if (_director.Audio != null) _director.Audio.SetAmbience("rain");
            _director.Player.Teleport(_courtyard.position, _courtyard.rotation);
            _director.CameraController.SnapBehindTarget(_courtyard.eulerAngles.y);
            yield return _director.Hud.Fade(0f, 0.9f);
        }

        public System.Collections.Generic.IEnumerable<string> SummaryLines(Ink.Runtime.Story story)
        {
            var v = story.variablesState;
            int opinion = (int)v["pl_che_kieu"];
            yield return "Bạn từng nghĩ về Kiều: " + (opinion == 1 ? "\"Dễ mà, mình sẽ làm tốt hơn.\"" : opinion == 2 ? "\"Phải có cách khác.\"" : "\"Kiều cam chịu quá.\"");
            yield return "Căn phòng của Kiều: đã xem " + (int)v["pl_vat"] + "/3 kỷ vật.";
            int act = (int)v["pl_hanh_dong"];
            yield return "Khi quan sai đến: " + (act == 1 ? "bạn lao ra chắn trước mặt cha." : act == 2 ? "bạn đòi xem đơn, hỏi tội danh." : "bạn đứng yên, ghi nhớ từng gương mặt.");
            yield return "<i>Chương 1 — Gia biến: đêm ấy, Mã Giám Sinh đến gõ cửa.</i>";
        }
    }
}
