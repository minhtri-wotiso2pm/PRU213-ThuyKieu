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
    /// Chapter 5 — the escape. Sets up the back yard at night, brings Cô Lam along if the player invited her,
    /// raises the alarm when the story says so, and turns the night into rain once Kiều is clear of the forest.
    /// The gameplay itself lives in the sneak, chase and forest sequences.
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
        [SerializeField] private ParticleSystem _rain;
        [SerializeField] private Transform _rainSpot;
        [SerializeField] private Light _moon;
        [SerializeField] private Stamina _stamina;

        private MaterialPropertyBlock _coneBlock;
        private float _coneAlpha = 0.18f;

        private void OnEnable() { _director.SceneChanged += OnScene; }
        private void OnDisable() { _director.SceneChanged -= OnScene; }
        private void OnDestroy() { LightExposure.Enable(false); }

        private void Start()
        {
            if (_navMesh != null) _navMesh.BuildNavMesh();
            foreach (var actor in FindObjectsByType<StoryActor>()) actor.EnableNavigation();
            _director.Player.AllowCrouch = true;
            _director.Hud.SetControls("WASD: đi · Shift: chạy · Ctrl: khom · E: tương tác / đẩy cửa / xô đổ · J: sổ tay");
            LightExposure.Enable(true);
            _stamina.enabled = false;
            _tuBa.gameObject.SetActive(false);
            foreach (var pursuer in _pursuers) pursuer.gameObject.SetActive(false);
            _guard.Patrolling = false;
            if (_director.GetBool("c4_ru_co_lam")) _companion.Snap();
            else _companion.gameObject.SetActive(false);
            if (_director.Audio != null) _director.Audio.SetAmbience("night");
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
                case "Escape_Rain": StartCoroutine(Rain()); break;
            }
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

        private IEnumerator Rain()
        {
            yield return _director.Hud.Fade(1f, 0.4f);
            LightExposure.Enable(false);
            _director.Player.Teleport(_rainSpot.position, _rainSpot.rotation);
            _director.CameraController.SnapBehindTarget(_rainSpot.eulerAngles.y);
            foreach (var pursuer in _pursuers) pursuer.gameObject.SetActive(false);
            if (_director.GetBool("c5_co_lam_thoat")) { _companion.gameObject.SetActive(true); _companion.Following = true; _companion.Snap(); }
            else _companion.gameObject.SetActive(false);
            _rain.Play();
            if (_director.Audio != null) _director.Audio.SetAmbience("forest_rain");
            if (_moon != null) { _moon.intensity *= 0.6f; _moon.color = new Color(0.5f, 0.6f, 0.85f); }
            RenderSettings.fogDensity *= 1.8f;
            yield return _director.Hud.Fade(0f, 0.8f);
        }

        public IEnumerable<string> SummaryLines(Ink.Runtime.Story story)
        {
            var v = story.variablesState;
            int quiet = (int)v["c5_lang_le"], road = (int)v["c5_duong"], qte = (int)v["c5_qte"];
            yield return "Lẻn khỏi sân sau: " + (quiet >= 3 ? "êm như gió." : quiet >= 2 ? "khá êm." : quiet >= 1 ? "suýt bị lộ." : "ồn ào, may mà thoát.");
            yield return "Đường chạy: " + (road == 2 ? "lẫn vào chợ đêm." : "ngõ tắt tối om.") + " · xô đổ " + (int)v["c5_xo_do"] + " vật cản.";
            yield return "Rừng tre: vượt " + qte + "/3 thử thách khó · số lần bị đuổi kịp: " + (int)v["c5_bi_bat"];
            if ((bool)v["c4_ru_co_lam"])
                yield return (bool)v["c5_co_lam_thoat"] ? "Chị Lam đã thoát cùng bạn." : "Chị Lam bị bắt lại. Bạn đã hứa sẽ quay lại.";
            yield return "<i>Chương tiếp theo — Kim Trọng: một mái chùa trong mưa...</i>";
        }
    }
}
