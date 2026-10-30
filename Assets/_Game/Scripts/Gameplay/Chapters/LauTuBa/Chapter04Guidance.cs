using System.Collections.Generic;
using ThuyKieu.Core;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.House;
using ThuyKieu.Player;
using ThuyKieu.Presentation;
using UnityEngine;
using UnityEngine.AI;

namespace ThuyKieu.Gameplay.Chapters.LauTuBa
{
    /// <summary>
    /// What Kiều has to do next in Chapter 4, read every moment from the night's real state (the stage and the contract in
    /// Ink, what she carries), never kept on its own: one objective line per step. After the keys, and only once the close
    /// view over them has handed the camera back, a few warm cues that belong to the house lead to the ledger office: its
    /// lamp on the paper window, the same light through the crack of door B, a gleam on the rings and the bolt of the doors
    /// into it; inside, a gleam on the cabinet's brass lock instead; with the contract all of that goes, and the kitchen's
    /// paper glows with the stove while the back door's ring catches it. The cues are <see cref="GuideGlow"/>s (additive,
    /// no Light, no collider), so the guards' light readings and sight lines are what they were.
    /// Danger happening now gets a short line under the objective, only after its real sign (the rattle at the door, Tú Bà's
    /// shout, a chase), and the kitchen cues step back until it is over. Tab shows the objective again; a player who makes
    /// no headway for half a minute, outside scares, chases, hiding, dialogue and the keys, is shown it once more.
    /// </summary>
    public class Chapter04Guidance : MonoBehaviour
    {
        public enum Step { Room, Keys, KeysInHand, Office, Cabinet, Kitchen }

        public const string OfficeObjective = "Đến phòng sổ sách — lấy giao kèo trong tủ khóa đồng.";
        public const string KitchenObjective = "Tìm đường tới bếp — thoát qua cửa sau.";
        private const float FirstReminder = 32f, LaterReminder = 60f, Headway = 2.5f;
        private const int MaxReminders = 3;

        [SerializeField] private StoryDirector _director;
        [SerializeField] private Chapter04LauWorld _world;
        [SerializeField] private KieuInventory _inventory;
        [SerializeField] private KeyTheft _keyTheft;
        [SerializeField] private ThuyKieu.Environment.NarrowNavMesh _navMesh;
        [SerializeField] private LampFlame _officeLamp;
        [SerializeField] private HouseDoor _innerDoor;
        [Tooltip("The ledger office and the archive behind its lattice (a world box): inside, the door cues give way to the cabinet's.")]
        [SerializeField] private Vector3 _officeMin, _officeMax;

        [Header("Cues")]
        [Tooltip("The office lamp seen from outside: its paper window. Only while that lamp burns.")]
        [SerializeField] private GuideGlow[] _officeLight;
        [Tooltip("The same lamp through the crack of door B, from Tú Bà's room. Only while the lamp burns and the door is shut.")]
        [SerializeField] private GuideGlow _doorCrack;
        [Tooltip("Gleams on the ways in: the ring and bolt of door B, the ring of the office door.")]
        [SerializeField] private GuideGlow[] _officeDoors;
        [SerializeField] private GuideGlow[] _cabinet;
        [Tooltip("The stove's glow on the kitchen paper, the back door's ring: only with the contract, never while danger is on.")]
        [SerializeField] private GuideGlow[] _kitchen;

        [Header("Where each step leads (for the no-headway reminder)")]
        [SerializeField] private Transform _hairpin;
        [SerializeField] private Transform _kieuDoor;
        [SerializeField] private Transform _tuBaBed;
        [SerializeField] private Transform _cabinetFront;
        [SerializeField] private Transform _backDoor;

        private Step _step;
        private bool _stepKnown, _viewBack, _snap = true;
        private string _warning;
        private float _warningUntil, _poll, _pathPoll, _stuck, _best;
        private NavMeshPath _path;

        public Step Current => _step;
        /// <summary>How many times the no-headway reminder has shown the objective in this step (QA).</summary>
        public int Reminders { get; private set; }
        public bool KieuInOffice => _director != null && InOffice(_director.Player.transform.position);

        internal IEnumerable<GuideGlow> All
        {
            get
            {
                foreach (var set in new[] { _officeLight, _officeDoors, _cabinet, _kitchen })
                    if (set != null) foreach (var glow in set) if (glow != null) yield return glow;
                if (_doorCrack != null) yield return _doorCrack;
            }
        }

        internal IEnumerable<GuideGlow> OfficeCues
        {
            get
            {
                if (_officeLight != null) foreach (var glow in _officeLight) yield return glow;
                if (_officeDoors != null) foreach (var glow in _officeDoors) yield return glow;
                if (_doorCrack != null) yield return _doorCrack;
            }
        }

        internal IEnumerable<GuideGlow> CabinetCues => _cabinet;
        internal IEnumerable<GuideGlow> KitchenCues => _kitchen;

        private void Awake()
        {
            _path = new NavMeshPath();
            ResetHeadway();
        }

        /// <summary>A danger line under the objective for a while (after its real sign: a sound, a word, a deed).</summary>
        public void Warn(string text, float seconds)
        {
            _warning = text;
            _warningUntil = Time.time + seconds;
        }

        public void ClearWarning() { _warning = null; }

        /// <summary>
        /// A checkpoint restored (or the night resumed): everything is worked out again from the restored state, the cues go
        /// straight to it (nothing left lit from before), the reminder starts over.
        /// </summary>
        public void Restore()
        {
            _warning = null;
            _viewBack = false;
            _stepKnown = false;
            _snap = true;
            _poll = 0f;
            Reminders = 0;
            ResetHeadway();
        }

        private void Update()
        {
            if (_director == null || _director.Player == null || _world == null) return;
            if (InputReader.ObjectivePressedThisFrame && _director.InExplore)
            {
                _director.Hud.RemindObjective();
                _stuck = 0f;
            }
            if ((_poll -= Time.deltaTime) <= 0f || _snap)
            {
                _poll = 0.25f;
                Think();
            }
            Cue();
            WatchHeadway();
        }

        // ------------------------------------------------------------------ the step and its line

        private void Think()
        {
            var step = Decide();
            if (!_stepKnown || step != _step)
            {
                _step = step;
                _stepKnown = true;
                Reminders = 0;
                ResetHeadway();
            }
            if (!_director.InExplore) return;
            string line = ObjectiveFor(step);
            if (line != null && line != _director.Hud.Objective) _director.Hud.SetObjective(line);
            _director.Hud.SetSituation(Situation());
        }

        private Step Decide()
        {
            if (_director.GetBool("c4_khe_uoc_mg")) return Step.Kitchen;
            if (_inventory.Has("chia"))
            {
                // The keys are hers, but the office is not shown until the close view over them has given the camera back.
                if (!_viewBack) _viewBack = ViewHandedBack();
                if (!_viewBack) return Step.KeysInHand;
                return KieuInOffice ? Step.Cabinet : Step.Office;
            }
            return _director.GetInt("c4_buoc") >= 1 ? Step.Keys : Step.Room;
        }

        private bool ViewHandedBack()
        {
            var camera = _director.CameraController;
            var player = _director.Player;
            return _director.InExplore && !_keyTheft.InMinigame && (camera == null || camera.CloseWeight < 0.01f)
                   && !player.ControlLocked && !player.HeldStill;
        }

        private string ObjectiveFor(Step step)
        {
            switch (step)
            {
                case Step.Room:
                    return _inventory.Has("tram") ? "Luồn trâm qua khe cửa — nâng then để ra ngoài." : "Tìm cây trâm, nâng then cửa phòng.";
                case Step.Keys:
                    return _director.GetBool("c3_biet_chia_khoa") ? "Vào phòng Tú Bà — lấy chùm chìa ở thắt lưng mụ." : "Tìm phòng Tú Bà — mụ đeo chìa khoá ở thắt lưng.";
                case Step.Office:
                case Step.Cabinet:
                    return OfficeObjective;
                case Step.Kitchen:
                    return KitchenObjective;
                default:
                    return null; // the keys' close view: the line waits with the cues
            }
        }

        private string Situation()
        {
            if (_world.Chased) return "Bị lộ — cắt tầm nhìn rồi hãy trốn.";
            return _warning != null && Time.time < _warningUntil ? _warning : null;
        }

        // ------------------------------------------------------------------ the cues

        private void Cue()
        {
            bool danger = _world.InDanger;
            bool lamp = _officeLamp == null || _officeLamp.Lit;
            // Danger dims the way to the office a little; the way to the kitchen goes out until it is over.
            float office = _step == Step.Office ? (danger ? 0.5f : 1f) : 0f;
            Set(_officeLight, lamp ? office : 0f);
            if (_doorCrack != null)
            {
                _doorCrack.Target = lamp ? office : 0f;
                _doorCrack.Gate = _innerDoor == null || _innerDoor.Openness < 0.04f ? 1f : 0f;
            }
            // With the lamp blown out the paper is dark: the gleams carry the way a little more.
            Set(_officeDoors, office * (lamp ? 1f : 1.3f));
            Set(_cabinet, _step == Step.Cabinet ? 1f : 0f);
            Set(_kitchen, _step == Step.Kitchen && !danger ? 1f : 0f);
            // Snapped once the restored state is settled in play (the step is only known for sure while exploring).
            if (!_snap || !_director.InExplore) return;
            _snap = false;
            foreach (var glow in All) glow.Snap();
        }

        private static void Set(GuideGlow[] glows, float target)
        {
            if (glows == null) return;
            foreach (var glow in glows) if (glow != null) glow.Target = target;
        }

        public bool InOffice(Vector3 position)
        {
            return position.x > _officeMin.x && position.x < _officeMax.x && position.z > _officeMin.z && position.z < _officeMax.z
                   && position.y > _officeMin.y && position.y < _officeMax.y;
        }

        // ------------------------------------------------------------------ no headway

        /// <summary>
        /// Counts the time she is free to act and gets no nearer her goal (by the walkable way, 2.5 m at a time); scares,
        /// chases, hiding, the keys, dialogue and pauses do not count. Past half a minute the objective comes back once, then
        /// at most once a minute, three times a step.
        /// </summary>
        private void WatchHeadway()
        {
            var player = _director.Player;
            bool free = _director.InExplore && !_director.IsBusy && !GamePause.IsPaused && !_keyTheft.InMinigame && !_world.InDanger
                        && !player.IsHidden && !player.ControlLocked && _step != Step.KeysInHand;
            if (!free) return;
            _stuck += Time.deltaTime;
            if ((_pathPoll -= Time.deltaTime) <= 0f)
            {
                _pathPoll = 1f;
                float distance = DistanceToGoal();
                if (distance < _best - Headway) { _best = distance; _stuck = 0f; }
            }
            if (Reminders >= MaxReminders || _stuck < (Reminders == 0 ? FirstReminder : LaterReminder)) return;
            _stuck = 0f;
            Reminders++;
            _director.Hud.RemindObjective();
        }

        private void ResetHeadway()
        {
            _stuck = 0f;
            _best = float.MaxValue;
            _pathPoll = 0f;
        }

        private Transform Goal()
        {
            switch (_step)
            {
                case Step.Room: return _inventory.Has("tram") ? _kieuDoor : _hairpin;
                case Step.Keys: return _tuBaBed;
                case Step.Office:
                case Step.Cabinet: return _cabinetFront;
                case Step.Kitchen: return _backDoor;
                default: return null;
            }
        }

        private float DistanceToGoal()
        {
            var goal = Goal();
            if (goal == null) return 0f;
            Vector3 from = _director.Player.transform.position;
            var filter = new NavMeshQueryFilter { areaMask = NavMesh.AllAreas, agentTypeID = _navMesh != null && _navMesh.AgentTypeId >= 0 ? _navMesh.AgentTypeId : 0 };
            if (NavMesh.SamplePosition(from, out var start, 1.5f, filter) && NavMesh.SamplePosition(goal.position, out var end, 2f, filter)
                && NavMesh.CalculatePath(start.position, end.position, filter, _path) && _path.status == NavMeshPathStatus.PathComplete)
            {
                var corners = _path.corners;
                float length = 0f;
                for (int i = 1; i < corners.Length; i++) length += Vector3.Distance(corners[i - 1], corners[i]);
                return length;
            }
            return Vector3.Distance(from, goal.position);
        }
    }
}
