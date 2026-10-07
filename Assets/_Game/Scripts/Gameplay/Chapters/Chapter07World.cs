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
        }

        private void OnDisable()
        {
            _director.SceneChanged -= OnScene;
            _director.ExploreChanged -= OnExplore;
            _director.CheckpointRestored -= OnRestored;
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
                case "HuyenDuong_AnhSang": Stillness(); break;
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

        /// <summary>Time stops: everyone freezes mid-gesture and the real Kiều stands facing the player.</summary>
        private void Stillness()
        {
            foreach (var actor in FindObjectsByType<StoryActor>())
            {
                if (actor.gameObject == _director.Player.gameObject) continue;
                foreach (var animator in actor.GetComponentsInChildren<Animator>()) animator.speed = 0f;
                actor.Stop();
            }
            var player = _director.Player.transform;
            Vector3 facing = player.forward;
            facing.y = 0f;
            _apparition.transform.SetPositionAndRotation(player.position + facing.normalized * 2.2f, Quaternion.LookRotation(-facing));
            _apparition.SetActive(true);
            // A two-shot from the side: Kiều on the left, the real Kiều on the right.
            Vector3 middle = player.position + facing.normalized * 1.1f + Vector3.up * 1.45f;
            Vector3 side = Vector3.Cross(Vector3.up, facing.normalized);
            _spiritCamera.position = middle - side * 2.6f + Vector3.up * 0.1f - facing.normalized * 0.4f;
            _spiritCamera.rotation = Quaternion.LookRotation(middle - _spiritCamera.position);
            if (_director.Audio != null) _director.Audio.SetAmbience(null);
        }

        public IEnumerable<string> SummaryLines(Ink.Runtime.Story story)
        {
            var v = story.variablesState;
            int ending = (int)v["c7_ket"];
            yield return "Phán quyết: " + (ending == 3 ? "trọn vẹn — lầu xanh bị niêm phong, ba người con gái được tìm về." : ending == 2 ? "Mã Giám Sinh bị giam, Tú Bà bị phạt nặng." : "Kiều được tự do, nhưng Tú Bà chỉ nộp phạt.");
            yield return "Điểm đối chất: " + (int)v["c7_diem"] + " · lòng tin của quan: " + (int)v["c7_long_tin"] + "%.";
            if ((int)v["c7_that_bai"] > 0) yield return "Quan huyện mất kiên nhẫn " + (int)v["c7_that_bai"] + " lần.";
            yield return "Vương Ông và Vương Quan được trắng án.";
            yield return "<i>Kết — Trở lại lớp học.</i>";
        }
    }
}
