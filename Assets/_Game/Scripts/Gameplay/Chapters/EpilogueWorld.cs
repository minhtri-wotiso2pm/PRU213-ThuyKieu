using System.Collections;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Stealth;
using ThuyKieu.Player;
using Unity.AI.Navigation;
using UnityEngine;

namespace ThuyKieu.Gameplay.Chapters
{
    /// <summary>
    /// Epilogue — the same classroom fifteen minutes after the bell, at sunset. The student wakes at her desk with the
    /// book open in front of her; her deskmate has come back for a jacket. Ends on the words written on the page.
    /// </summary>
    public class EpilogueWorld : MonoBehaviour
    {
        [SerializeField] private StoryDirector _director;
        [SerializeField] private NavMeshSurface _navMesh;
        [SerializeField] private PlayerLook _look;
        [SerializeField] private Transform _seat;
        [SerializeField] private GameObject _thankYou;

        private bool _seated = true;

        private void OnEnable() => _director.ExploreChanged += OnExplore;
        private void OnDisable() => _director.ExploreChanged -= OnExplore;

        private IEnumerator Start()
        {
            LightExposure.Enable(false);
            if (_navMesh != null) _navMesh.BuildNavMesh();
            foreach (var actor in FindObjectsByType<StoryActor>()) actor.EnableNavigation();
            _director.Player.AllowCrouch = false;
            _director.Hud.SetControls("WASD: đi · E: tương tác · Space: tiếp lời thoại");
            _look.ShowStudent(true);
            if (_thankYou != null) _thankYou.SetActive(false);
            if (_director.Audio != null) _director.Audio.SetAmbience("classroom_evening");
            yield return null;
            _director.Player.Teleport(_seat.position, _seat.rotation);
            _director.Player.GetComponent<StoryActor>().Sit(true);
        }

        private void OnExplore(bool exploring)
        {
            if (!exploring || !_seated) return;
            _seated = false;
            StartCoroutine(StandUp());
        }

        private IEnumerator StandUp()
        {
            var player = _director.Player;
            player.ControlLocked = true;
            player.GetComponent<StoryActor>().Sit(false);
            yield return new WaitForSeconds(_director.Autopilot ? 0f : 1.6f);
            if (_director.InExplore) player.ControlLocked = false;
        }
    }
}
