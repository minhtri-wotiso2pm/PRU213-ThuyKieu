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
    /// Chapter 7 — Nghịch Mệnh. The yamen gate in the morning (Kim Trọng at Kiều's side, the witnesses under the eaves,
    /// Tú Bà and Mã Giám Sinh in a corner), the courtroom for the cross-examination, the family reunion at the gate
    /// three days later, and the still moment where the real Kiều appears for the last time.
    /// Witnesses who back down under Tú Bà's threat walk out of the hall.
    /// </summary>
    public class Chapter07World : MonoBehaviour, IChapterSummary
    {
        [SerializeField] private StoryDirector _director;
        [SerializeField] private NavMeshSurface _navMesh;
        [SerializeField] private StoryActor _kim;
        [SerializeField] private Follower _kimFollower;
        [SerializeField] private StoryActor _baNuoc, _thuLai, _chiLam, _tuBa, _ma;
        [SerializeField] private StoryActor _father, _brother, _mother;
        [SerializeField] private GameObject _apparition;
        [Tooltip("The CAM_Spirit anchor, framed at runtime on Kiều and the apparition.")]
        [SerializeField] private Transform _spiritCamera;
        [Tooltip("Where the real Kiều appears: open ground in front of the gate, clear of the family and Kim Trọng.")]
        [SerializeField] private Transform _spiritSpot;
        [SerializeField] private StoryActor _magistrate;
        [SerializeField] private Light _sun;
        [SerializeField] private Transform _gatePlayer, _exit;
        [Header("Courtroom marks")]
        [SerializeField] private Transform _hallPlayer, _hallKim, _hallTuBa, _hallMa;
        [SerializeField] private Transform[] _hallWitnesses;
        [Header("Reunion marks")]
        [SerializeField] private Transform _reunionPlayer, _reunionKim, _reunionFather, _reunionBrother, _reunionMother;

        private bool _inHall;

        private void OnEnable()
        {
            _director.SceneChanged += OnScene;
            _director.ExploreChanged += OnExplore;
            _director.CheckpointRestored += OnRestored;
            _director.TagReceived += OnTag;
        }

        private void OnDisable()
        {
            _director.SceneChanged -= OnScene;
            _director.ExploreChanged -= OnExplore;
            _director.CheckpointRestored -= OnRestored;
            _director.TagReceived -= OnTag;
        }

        /// <summary>
        /// A witness who was not called still speaks up "from the crowd" at the end of the trial: bring her into the
        /// hall, at the back of the plaintiffs' side, so the line has a face.
        /// </summary>
        private void OnTag(string key, string value)
        {
            if (key != "speaker" || !_inHall) return;
            var witness = value == "BaHangNuoc" ? _baNuoc : value == "ThuLai" ? _thuLai : value == "ChiLam" ? _chiLam : null;
            if (witness == null || witness.gameObject.activeSelf || _hallWitnesses.Length == 0) return;
            var spot = _hallWitnesses[_hallWitnesses.Length - 1];
            witness.gameObject.SetActive(true);
            witness.EnableNavigation();
            witness.Place(spot.position + spot.right * 0.9f - spot.forward * 0.6f, spot.rotation);
        }

        private IEnumerator Start()
        {
            LightExposure.Enable(false);
            if (_navMesh != null) _navMesh.BuildNavMesh();
            foreach (var actor in FindObjectsByType<StoryActor>()) actor.EnableNavigation();
            _director.Player.AllowCrouch = false;
            _director.Hud.SetControls("WASD: đi · E: tương tác · Chuột phải: quan sát · J: sổ tay");
            while (_director.Story == null) yield return null; // the director creates the story in its own Start
            _baNuoc.gameObject.SetActive(_director.GetBool("nc_ba_nuoc"));
            _thuLai.gameObject.SetActive(_director.GetBool("nc_thu_lai"));
            _chiLam.gameObject.SetActive(_director.GetBool("nc_co_lam"));
            _kim.gameObject.SetActive(true);
            foreach (var member in new[] { _father, _brother, _mother }) member.gameObject.SetActive(false);
            _apparition.SetActive(false);
            _kimFollower.Following = false;
            _magistrate.Sit(true);
            if (_director.Audio != null) _director.Audio.SetAmbience("yamen");
            _director.Story.ObserveVariable("c7_lui", (name, value) => StartCoroutine(Retreat((int)value)));
        }

        private void OnExplore(bool exploring) => _kimFollower.Following = exploring && !_inHall;

        private void OnRestored()
        {
            if (_inHall) return;
            _director.Player.Teleport(_gatePlayer.position, _gatePlayer.rotation);
            _director.CameraController.SnapBehindTarget(_gatePlayer.eulerAngles.y);
            _kimFollower.Snap();
        }

        private void OnScene(string scene)
        {
            switch (scene)
            {
                case "HuyenDuong_CongDuong": StartCoroutine(IntoHall()); break;
                case "HuyenDuong_DoanTu": StartCoroutine(Reunion()); break;
                case "HuyenDuong_AnhSang": Stillness(false); break;
                case "HuyenDuong_BongToi": Stillness(true); break;
            }
        }

        private IEnumerator IntoHall()
        {
            yield return _director.Hud.Fade(1f, 0.4f);
            _inHall = true;
            _kimFollower.Following = false;
            _director.Player.Teleport(_hallPlayer.position, _hallPlayer.rotation);
            _director.CameraController.SnapBehindTarget(_hallPlayer.eulerAngles.y);
            _kim.Place(_hallKim.position, _hallKim.rotation);
            _tuBa.Place(_hallTuBa.position, _hallTuBa.rotation);
            _ma.Place(_hallMa.position, _hallMa.rotation);
            int slot = 0;
            foreach (var witness in new[] { _baNuoc, _thuLai, _chiLam })
                if (witness.gameObject.activeSelf && slot < _hallWitnesses.Length) { witness.Place(_hallWitnesses[slot].position, _hallWitnesses[slot].rotation); slot++; }
            if (_director.Audio != null) _director.Audio.SetAmbience("yamen_hall");
            yield return _director.Hud.Fade(0f, 0.7f);
        }

        /// <summary>A witness frightened by Tú Bà's shout leaves the hall.</summary>
        private IEnumerator Retreat(int who)
        {
            var witness = who == 1 ? _chiLam : who == 2 ? _baNuoc : who == 3 ? _thuLai : null;
            if (witness == null || !witness.gameObject.activeSelf) yield break;
            witness.MoveTo(_exit.position, 1.5f);
            for (float t = 0; t < 8f; t += Time.deltaTime)
            {
                if (t > 1f && !witness.IsMoving) break;
                yield return null;
            }
            witness.gameObject.SetActive(false);
        }

        private IEnumerator Reunion()
        {
            yield return _director.Hud.Fade(1f, 0.6f);
            _inHall = false;
            _tuBa.gameObject.SetActive(false);
            _ma.gameObject.SetActive(false);
            _director.Player.Teleport(_reunionPlayer.position, _reunionPlayer.rotation);
            _director.CameraController.SnapBehindTarget(_reunionPlayer.eulerAngles.y);
            _kim.Place(_reunionKim.position, _reunionKim.rotation);
            foreach (var (member, mark) in new[] { (_father, _reunionFather), (_brother, _reunionBrother), (_mother, _reunionMother) })
            {
                member.gameObject.SetActive(true);
                member.EnableNavigation();
                member.Place(mark.position, mark.rotation);
            }
            if (_sun != null) { _sun.intensity = 1.25f; _sun.color = new Color(1f, 0.86f, 0.66f); }
            if (_director.Audio != null) _director.Audio.SetAmbience("courtyard");
            yield return _director.Hud.Fade(0f, 0.9f);
        }

        /// <summary>
        /// Time stops: everyone freezes mid-gesture, Kiều turns towards the open court and the real Kiều stands there
        /// facing her. The two-shot is taken from whichever side has nobody standing in it (Kim Trọng and the family
        /// stay behind Kiều, out of the line of sight); anyone still in the way steps out of the picture.
        /// </summary>
        private void Stillness(bool lost)
        {
            var frozen = new List<StoryActor>();
            foreach (var actor in FindObjectsByType<StoryActor>())
            {
                if (actor.gameObject == _director.Player.gameObject) continue;
                foreach (var animator in actor.GetComponentsInChildren<Animator>()) animator.speed = 0f;
                actor.Stop();
                frozen.Add(actor);
            }
            var player = _director.Player.transform;
            Vector3 spot = _spiritSpot != null ? _spiritSpot.position : player.position + player.forward * 2.2f;
            Vector3 facing = spot - player.position;
            facing.y = 0f;
            facing = facing.sqrMagnitude > 0.01f ? facing.normalized : player.forward;
            _director.Player.Teleport(player.position, Quaternion.LookRotation(facing));
            // Kim Trọng steps back a pace behind her shoulder (with the family) so he is neither between the two Kiều
            // nor standing in the background of their shot.
            Vector3 aside = Vector3.Cross(Vector3.up, facing);
            Vector3 behind = player.position - facing * 1.4f - aside * 0.9f;
            _kim.Place(behind, Quaternion.LookRotation(Flat(player.position - behind)));
            Vector3 apparitionAt = player.position + facing * Mathf.Clamp(Vector3.Distance(Flat(spot), Flat(player.position)), 1.8f, 2.4f);
            _apparition.transform.SetPositionAndRotation(apparitionAt, Quaternion.LookRotation(-facing));
            _apparition.SetActive(true);

            // A two-shot in profile: test both sides and keep the one with nobody between the lens and the pair.
            Vector3 middle = (player.position + apparitionAt) * 0.5f + Vector3.up * 1.45f;
            Vector3 side = Vector3.Cross(Vector3.up, facing);
            // Far enough back that both fit inside the narrow dialogue lens with air around them (about ±15° each side).
            float reach = Mathf.Max(3.2f, (Vector3.Distance(Flat(player.position), Flat(apparitionAt)) * 0.5f + 0.35f) / Mathf.Tan(15f * Mathf.Deg2Rad));
            Vector3 best = Vector3.zero;
            int fewest = int.MaxValue;
            foreach (float sign in new[] { -1f, 1f })
            {
                Vector3 lens = middle + side * sign * reach + Vector3.up * 0.05f - facing * 0.9f;
                int blocking = 0;
                // In front of the pair counts most; behind them (the background of the shot) counts too.
                foreach (var actor in frozen)
                {
                    if (Between(actor, lens, middle, 1.6f)) blocking += 3;
                    else if (Between(actor, middle, middle + (middle - lens) * 2.5f, 3.2f)) blocking++;
                }
                if (Physics.Linecast(middle, lens, 1 << 8, QueryTriggerInteraction.Ignore)) blocking += 10;
                if (blocking < fewest) { fewest = blocking; best = lens; }
            }
            _spiritCamera.position = best;
            _spiritCamera.rotation = Quaternion.LookRotation(middle - best);
            foreach (var actor in frozen)
                if (Between(actor, best, middle, 1.6f) || Vector3.Distance(Flat(actor.transform.position), Flat(apparitionAt)) < 0.8f
                    || Vector3.Distance(Flat(actor.transform.position), Flat(best)) < 2.5f)
                    foreach (var renderer in actor.GetComponentsInChildren<Renderer>())
                        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

            // The moment outside time: warm, hazy light instead of the noon glare — or, when the case is lost, a cold dusk.
            if (_sun != null) { _sun.intensity = lost ? 0.45f : 1.1f; _sun.color = lost ? new Color(0.55f, 0.62f, 0.78f) : new Color(1f, 0.84f, 0.62f); }
            RenderSettings.fogColor = lost ? new Color(0.32f, 0.35f, 0.42f) : new Color(0.95f, 0.86f, 0.7f);
            RenderSettings.fogDensity = lost ? 0.03f : 0.012f;
            if (_director.Audio != null) _director.Audio.SetAmbience(null);
        }

        /// <summary>Whether an actor stands in the corridor from the lens to the subjects (within <paramref name="width"/>).</summary>
        private static bool Between(StoryActor actor, Vector3 lens, Vector3 target, float width)
        {
            Vector3 a = Flat(lens), b = Flat(target), p = Flat(actor.transform.position);
            Vector3 ab = b - a;
            float t = Mathf.Clamp01(Vector3.Dot(p - a, ab) / Mathf.Max(0.0001f, ab.sqrMagnitude));
            return t > 0.02f && t < 0.98f && Vector3.Distance(p, a + ab * t) < width * 0.5f;
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

        public IEnumerable<string> SummaryLines(Ink.Runtime.Story story)
        {
            var v = story.variablesState;
            int ending = (int)v["c7_ket"];
            yield return "Phán quyết: " + (ending == 3 ? "trọn vẹn — lầu xanh bị niêm phong, ba người con gái được tìm về." : ending == 2 ? "Mã Giám Sinh bị giam, Tú Bà bị phạt nặng." : ending == 1 ? "Kiều được tự do, nhưng Tú Bà chỉ nộp phạt." : "thua kiện — Kiều bị trả về lầu xanh. Câu chuyện đi đúng con đường của nó.");
            yield return "Điểm đối chất: " + (int)v["c7_diem"] + " · lòng tin của quan: " + (int)v["c7_long_tin"] + "%.";
            if ((int)v["c7_thua_vong"] > 0) yield return "Quan huyện phải tự xét giấy tờ " + (int)v["c7_thua_vong"] + " vòng.";
            if (ending > 0) yield return "Vương Ông và Vương Quan được trắng án.";
            yield return "Điểm phán quyết: " + (int)v["c7_diem_pq"] + " · Định Mệnh " + (int)v["dinh_menh"] + "/100.";
            yield return "<i>Kết — Trở lại lớp học.</i>";
        }
    }
}
