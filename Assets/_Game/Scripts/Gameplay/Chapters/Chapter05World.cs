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
    /// Chapter 5 — the escape. The chapter opens on the run: Kiều is already at the edge of the bamboo (behind black until
    /// the forest is ready), Cô Lam with her if the player invited her, and once she is clear of the forest she stops by the
    /// temple path under a dimmer moon (a dry night: no rain anywhere in the chapter). The run itself is <see cref="ThuyKieu.Gameplay.Sequences.BambooChaseSequence"/>. The yard and the town
    /// stay in the scene as they were; the old sneak, alarm and Sở Khanh beats are no longer called from the story.
    /// </summary>
    public class Chapter05World : MonoBehaviour, IChapterSummary
    {
        [SerializeField] private StoryDirector _director;
        [SerializeField] private NavMeshSurface _navMesh;
        [SerializeField] private GuardPatrol _guard;
        [SerializeField] private Renderer _visionCone;
        [SerializeField] private StoryActor _tuBa;
        [SerializeField] private Follower _companion;
        [SerializeField] private StoryActor[] _pursuers;
        [SerializeField] private GameObject _gateBlocker;
        [SerializeField] private Transform _rainSpot;
        [SerializeField] private Light _moon;
        [SerializeField] private Stamina _stamina;
        [SerializeField] private StoryActor _soKhanh;
        [Tooltip("Where Kiều stands at the edge of the bamboo once she is out of the alley (the chase starts from here).")]
        [SerializeField] private Transform _forestEdge;

        private MaterialPropertyBlock _coneBlock;
        private float _coneAlpha = 0.18f;

        private void OnEnable() { _director.SceneChanged += OnScene; }
        private void OnDisable() { _director.SceneChanged -= OnScene; }
        private void OnDestroy() { LightExposure.Enable(false); }

        private void Start()
        {
            // Black from the first frame: nothing of the yard is shown, the run comes up once the forest is ready.
            _director.Hud.FadeAlpha = 1f;
            _director.Hud.SetControls(string.Empty);
            _director.Player.AllowCrouch = true;
            _stamina.enabled = false;
            _tuBa.gameObject.SetActive(false);
            _guard.Patrolling = false;
            _guard.gameObject.SetActive(false);
            foreach (var pursuer in _pursuers) pursuer.gameObject.SetActive(false);
            // Cô Lam is placed and led by the run itself (on the forest's NavMesh, which is still being built now).
            _companion.Following = false;
            if (!_director.GetBool("c4_ru_co_lam")) _companion.gameObject.SetActive(false);
            PlaceAtForestEdge();
        }

        private void Update()
        {
            bool sneaking = _director.CurrentSequence == "sneak_out";
            if (_visionCone == null) return;
            _coneBlock ??= new MaterialPropertyBlock();
            _coneAlpha = Mathf.MoveTowards(_coneAlpha, sneaking ? (_guard.Vision.Awareness > 0.04f ? 0.34f : 0.18f) : 0f, Time.deltaTime * 0.6f);
            // Lamplight while he is calm, red as he grows suspicious.
            var calm = new Color(1f, 0.62f, 0.3f, _coneAlpha);
            _coneBlock.SetColor("_BaseColor", Color.Lerp(calm, new Color(1f, 0.18f, 0.1f, _coneAlpha), Mathf.Clamp01(_guard.Vision.Awareness * 2f)));
            _visionCone.SetPropertyBlock(_coneBlock);
        }

        private void OnScene(string scene)
        {
            switch (scene)
            {
                case "Escape_Alarm": Alarm(); break;
                case "Escape_Forest": PlaceAtForestEdge(); break;
                case "Escape_SoKhanh": MeetSoKhanh(); break;
                case "Escape_Rain": if (_soKhanh != null) _soKhanh.gameObject.SetActive(false); StartCoroutine(ClearOfTheForest()); break;
            }
        }

        /// <summary>
        /// Định Mệnh pushes back (fate ≥ 50): Sở Khanh waits at the edge of the bamboo with a horse, as in the poem. He is a
        /// placeholder recast from one of the pursuers' rigs (the Mã Giám Sinh model) in a scholar's pale blue, without torch.
        /// </summary>
        private void MeetSoKhanh()
        {
            if (_soKhanh == null && _pursuers.Length > 0)
            {
                var copy = Instantiate(_pursuers[0].gameObject, transform);
                copy.name = "SoKhanh";
                var torch = copy.transform.Find("Torch");
                if (torch != null) Destroy(torch.gameObject);
                var block = new MaterialPropertyBlock();
                foreach (var renderer in copy.GetComponentsInChildren<Renderer>())
                {
                    renderer.GetPropertyBlock(block);
                    block.SetColor("_BaseColor", new Color(0.72f, 0.8f, 0.92f));
                    renderer.SetPropertyBlock(block);
                }
                _soKhanh = copy.GetComponent<StoryActor>();
                _soKhanh.Recast("SoKhanh");
            }
            if (_soKhanh == null) return;
            // The town's NavMesh is only built for this beat now (the chapter no longer plays in the yard and streets).
            if (_navMesh != null && _navMesh.navMeshData == null) _navMesh.BuildNavMesh();
            _director.RegisterActor(_soKhanh);
            var player = _director.Player.transform;
            Vector3 spot = player.position + player.forward * 2.4f;
            _soKhanh.gameObject.SetActive(true);
            _soKhanh.Place(spot, Quaternion.LookRotation(player.position - spot));
            _soKhanh.FaceTowards(player.position);
        }

        /// <summary>Kiều at the edge of the bamboo where the run starts, facing into it; the pursuers are placed by the run itself.</summary>
        private void PlaceAtForestEdge()
        {
            if (_forestEdge == null) return;
            _director.Player.Teleport(_forestEdge.position, _forestEdge.rotation);
            _director.CameraController.SnapBehindTarget(_forestEdge.eulerAngles.y);
            if (_director.Audio != null) _director.Audio.SetAmbience(null); // the forest is dry: no rain while she runs
        }

        /// <summary>The gate is open, a window bangs upstairs and the whole house wakes.</summary>
        private void Alarm()
        {
            if (_gateBlocker != null) _gateBlocker.SetActive(false);
            _tuBa.gameObject.SetActive(true);
            _guard.Patrolling = false;
            _guard.Vision.Active = false;
            _guard.GetComponent<StoryActor>().FaceTowards(_director.Player.transform.position);
            if (_director.Fx != null) _director.Fx.Shake(0.2f, 0.6f);
            if (_director.Audio != null) _director.Audio.PlaySfx("dog");
        }

        /// <summary>Out of the forest, at the temple path: the pursuit is gone, the moon goes behind thin cloud.</summary>
        private IEnumerator ClearOfTheForest()
        {
            yield return _director.Hud.Fade(1f, 0.4f);
            LightExposure.Enable(false);
            _director.Player.Teleport(_rainSpot.position, _rainSpot.rotation);
            _director.CameraController.SnapBehindTarget(_rainSpot.eulerAngles.y);
            foreach (var pursuer in _pursuers) pursuer.gameObject.SetActive(false);
            if (_director.GetBool("c5_co_lam_thoat")) { _companion.gameObject.SetActive(true); _companion.Following = true; _companion.Snap(); }
            else _companion.gameObject.SetActive(false);
            if (_moon != null) { _moon.intensity *= 0.6f; _moon.color = new Color(0.5f, 0.6f, 0.85f); }
            yield return _director.Hud.Fade(0f, 0.8f);
        }

        public IEnumerable<string> SummaryLines(Ink.Runtime.Story story)
        {
            var v = story.variablesState;
            int caught = (int)v["c5_bi_bat"], seconds = (int)v["c5_giay"];
            yield return "Rừng tre: " + (caught == 0 ? "thoát ngay lần đầu." : "thoát sau " + caught + " lần bị đuổi kịp hoặc ngã xuống khe.");
            yield return "Lượt chạy thoát: " + seconds / 60 + " phút " + seconds % 60 + " giây · vấp " + (int)v["c5_vap"] + " lần · "
                         + ((int)v["c5_loi"] == 2 ? "lối tre rậm." : "lối khe trăng.");
            yield return "Cầu tre qua khe suối đã đổ: lính của mụ Tú không qua được.";
            if ((bool)v["c4_ru_co_lam"])
                yield return (bool)v["c5_co_lam_thoat"] ? "Chị Lam đã thoát cùng bạn." : "Chị Lam bị bắt lại. Bạn đã hứa sẽ quay lại.";
            yield return "<i>Chương tiếp theo — Kim Trọng: một mái chùa trong sương sớm...</i>";
        }
    }
}
