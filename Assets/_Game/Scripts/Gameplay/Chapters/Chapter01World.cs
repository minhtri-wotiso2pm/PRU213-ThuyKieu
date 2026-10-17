using System.Collections;
using System.Collections.Generic;
using ThuyKieu.Core.Story;
using ThuyKieu.Player;
using UnityEngine;

namespace ThuyKieu.Gameplay.Chapters
{
    /// <summary>
    /// Chapter 1 — the rainy night at the Vương house. Stages the family and Mã Giám Sinh for each Ink scene,
    /// knocks at the gate once Kiều has learned enough, and runs his suspicion while she sizes him up.
    /// </summary>
    public class Chapter01World : MonoBehaviour, IChapterSummary
    {
        private const string Negotiation = "VuongGia_DamPhan";

        [SerializeField] private StoryDirector _director;
        [SerializeField] private StoryActor _mother;
        [SerializeField] private StoryActor _ma;
        [SerializeField] private StoryActor _servant;
        [SerializeField] private ObserveMode _observe;
        [Tooltip("The bag of silver on the contract table, shown once Mã Giám Sinh has put it down.")]
        [SerializeField] private GameObject _silver;
        [Tooltip("The written contract and its seal, laid on the table when he brings it out.")]
        [SerializeField] private GameObject _contract;
        [SerializeField] private Transform _kieuMeet, _motherMeet, _maMeet, _servantPorch;
        [SerializeField] private Transform _motherTable, _maTable, _tableFocus;
        [SerializeField] private Transform _kieuDoor, _motherDoor, _maGate, _servantGate;

        private string _scene = string.Empty;
        private float _glanceTimer = 6f, _glanceHold, _barkCooldown, _suspicionCarry;
        private bool _knocked;

        public bool Glancing => _glanceHold > 0f;

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
            _director.Player.AllowCrouch = false;
            _director.Hud.SetControls("WASD: đi · Shift: chạy · Chuột phải: quan sát · E: tương tác · J: sổ tay");
            // Mã Giám Sinh only arrives later; a resumed save replays its scene tag and stages him again.
            if (_scene.Length == 0) { SetGuests(false); _contract.SetActive(false); }
        }

        private void Update()
        {
            if (_barkCooldown > 0f) _barkCooldown -= Time.deltaTime;
            if (!_knocked && _director.IsExploreAvailable("c1_cong"))
            {
                _knocked = true;
                if (_director.Audio != null) _director.Audio.PlaySfx("knock");
                Bark("NoiTam", "Có tiếng gõ cổng. Giữa đêm mưa thế này... là hắn.");
            }
            if (_scene == Negotiation && _director.InExplore) UpdateSuspicion();
        }

        // ------------------------------------------------------------------ suspicion

        private void UpdateSuspicion()
        {
            Vector3 player = _director.Player.transform.position;
            float distance = Flat(player - _ma.transform.position);
            if (_glanceHold > 0f)
            {
                _glanceHold -= Time.deltaTime;
                _ma.FaceTowards(player);
            }
            else
            {
                _ma.FaceTowards(_tableFocus.position);
                if ((_glanceTimer -= Time.deltaTime) <= 0f)
                {
                    _glanceTimer = Random.Range(6f, 10f);
                    _glanceHold = 2f;
                }
            }

            bool watched = Glancing || Vector3.Dot(_ma.transform.forward, (player - _ma.transform.position).normalized) > 0.6f;
            bool observing = _observe != null && _observe.IsObserving;
            float rate = -3f;
            if (observing && watched && distance < 11f)
            {
                rate = 24f;
                if (_barkCooldown <= 0f) Bark("MaGiamSinh", "Cô nương nhìn gì ta chằm chằm thế?");
            }
            else if (observing) rate = 0f;

            int value = _director.GetInt("c1_nghi_ngo");
            _suspicionCarry += rate * Time.deltaTime;
            int step = (int)_suspicionCarry;
            if (step != 0)
            {
                _suspicionCarry -= step;
                value = Mathf.Clamp(value + step, 0, 100);
                _director.SetInt("c1_nghi_ngo", value);
            }
            float normalized = value / 100f;
            _director.Hud.SetMeter(value >= 60 ? "MÃ GIÁM SINH ĐANG NGỜ VỰC" : "Nghi ngờ của Mã Giám Sinh",
                normalized, Color.Lerp(new Color(1f, 0.85f, 0.4f), new Color(0.9f, 0.2f, 0.15f), normalized));
            if (value >= 100)
                _director.Fail("Mã Giám Sinh đã sinh nghi", "Hắn bảo Mẹ Kiều rằng con gái bà \"lắm điều\" rồi đứng dậy toan bỏ đi. " +
                    "Hãy kín đáo hơn: đừng quan sát khi hắn đang nhìn lại, đừng hỏi quá thẳng.");
        }

        private void OnRestored()
        {
            _director.SetInt("c1_nghi_ngo", Mathf.Min(_director.GetInt("c1_nghi_ngo"), 45));
            _suspicionCarry = 0f;
            _glanceHold = 0f;
            _glanceTimer = 6f;
        }

        // ------------------------------------------------------------------ staging

        private void OnScene(string scene)
        {
            _scene = scene;
            _contract.SetActive(scene == "VuongGia_BanKyKhe");
            switch (scene)
            {
                case "VuongGia_DemMua": SetGuests(false); break;
                case "VuongGia_MaDen": StartCoroutine(Stage(() =>
                {
                    SetGuests(true);
                    _silver.SetActive(false);
                    Teleport(_kieuMeet);
                    Place(_mother, _motherMeet);
                    Place(_ma, _maMeet);
                    Place(_servant, _servantPorch);
                    _ma.FaceTowards(_kieuMeet.position);
                }, 0.5f)); break;
                case Negotiation:
                    SetGuests(true);
                    _silver.SetActive(true);
                    Place(_mother, _motherTable);
                    Place(_ma, _maTable);
                    Place(_servant, _servantPorch);
                    break;
                case "VuongGia_RaDi": StartCoroutine(Stage(() =>
                {
                    SetGuests(true);
                    _silver.SetActive(false);
                    Teleport(_kieuDoor);
                    Place(_mother, _motherDoor);
                    Place(_ma, _maGate);
                    Place(_servant, _servantGate);
                }, 1.1f)); break;
            }
        }

        private void SetGuests(bool present)
        {
            _ma.gameObject.SetActive(present);
            _servant.gameObject.SetActive(present);
            _silver.SetActive(present && _scene == Negotiation);
        }

        private void Teleport(Transform point)
        {
            _director.Player.Teleport(point.position, point.rotation);
            _director.CameraController.SnapBehindTarget(point.eulerAngles.y);
        }

        private static void Place(StoryActor actor, Transform point) => actor.Place(point.position, point.rotation);

        private IEnumerator Stage(System.Action move, float fade)
        {
            yield return _director.Hud.Fade(1f, fade * 0.5f);
            move();
            yield return new WaitForSeconds(_director.Autopilot ? 0f : fade * 0.3f);
            yield return _director.Hud.Fade(0f, fade);
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
            var v = story.variablesState;
            int clues = (int)v["c1_manh_moi"];
            int suspicion = Mathf.Clamp((int)v["c1_nghi_ngo"], 0, 100);
            yield return "Manh mối về Mã Giám Sinh: " + clues + " / 6";
            yield return "Tỉnh táo: " + (int)v["tinh_tao"];
            yield return (bool)v["doc_ky_khe"] ? "Tờ khế: bạn đã đọc kỹ từng chữ trước khi điểm chỉ." : "Tờ khế: bạn đã ký vội để cứu cha.";
            if ((bool)v["c1_bac_dau"]) yield return "Bạc dấu Phúc Ký — ai đó đã đưa tiền cho hắn đi \"hỏi vợ\".";
            yield return "Nghi ngờ của Mã Giám Sinh: " + suspicion + "%" + (suspicion >= 60 ? " — hắn sẽ để mắt tới bạn trên đường đi" : " — hắn tin bạn là cô gái ngoan");
        }
    }
}
