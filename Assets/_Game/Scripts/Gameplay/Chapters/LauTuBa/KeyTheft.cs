using System;
using ThuyKieu.Core;
using ThuyKieu.Gameplay.Stealth;
using ThuyKieu.Interaction;
using ThuyKieu.Player;
using ThuyKieu.Presentation;
using UnityEngine;

namespace ThuyKieu.Gameplay.Chapters.LauTuBa
{
    /// <summary>
    /// Slipping the key ring off the sleeping Tú Bà's belt, in first person, close up (<see cref="KeyTheftView"/> draws it):
    /// <list type="bullet">
    /// <item>Looking at the keys, E leans Kiều in close. That press is not a grab, whatever the breath; once the view has
    /// settled, a held E has to be let go and pressed again to take hold. S backs out (the work done is kept).</item>
    /// <item>Holding E puts the hand on the keys. While she breathes out, drawing the mouse slowly the way shown works the
    /// ring loose in three stages (lift it, take it off the hook, draw it away), <see cref="Seconds"/> of such work in all.
    /// Only out-breath time spent drawing the right way counts: waiting, a still hand, the wrong way or a jerk do not.</item>
    /// <item>A hand on the keys as she breathes in (still holding as the breath turns, or taking hold during it), or while she
    /// stirs or lies awake, is felt: she grabs it (<see cref="TuBaSleeper.CatchHand"/>; the chapter plays her catch).
    /// Holding still is still holding.</item>
    /// <item>Letting go keeps the close view and the work; a checkpoint takes the work back.</item>
    /// </list>
    /// While the view is on, the keys are the interaction's pinned target, so turning the camera cannot lose them.
    /// </summary>
    public class KeyTheft : MonoBehaviour, IHoldInteractable, IInteractableDisplayName, IInteractionAvailability
    {
        private enum Session { None, Entering, Open }

        /// <summary>How the last frame's pull went (QA, the prompt).</summary>
        public enum Pull { None, Right, Still, Wrong, TooFast }

        [SerializeField] private TuBaSleeper _sleeper;
        [SerializeField] private KieuInventory _inventory;
        [Tooltip("The ring's root on her hip (it carries the hook, which stays on her).")]
        [SerializeField] private GameObject _keyRing;
        [Tooltip("What is taken: the ring with its keys.")]
        [SerializeField] private GameObject _bunch;
        [Tooltip("The point the trigger follows (the keys at rest); the key ring itself when empty.")]
        [SerializeField] private Transform _grip;
        [SerializeField] private KeyTheftView _view;
        [SerializeField] private PlayerMovement _player;
        [SerializeField] private StoryAudio _audio;
        [SerializeField] private string _item = "chia";
        [Tooltip("Seconds of work on her out-breaths, in all, to free the ring (three equal stages).")]
        [SerializeField] private float _seconds = 12f;
        [Tooltip("Drawing speed that counts, in screen heights per second (slow and steady; faster is a jerk).")]
        [SerializeField] private float _minPull = 0.02f, _maxPull = 0.45f;

        private Session _session;
        private float _held, _tick, _pull, _across;
        private bool _onKeys, _needRelease;
        private int _lastTick = -10, _lastStage;

        public event Action<KeyTheft> Taken;
        public float Seconds => _seconds;
        public float Progress => Mathf.Clamp01(_held / Mathf.Max(0.01f, _seconds));
        /// <summary>Seconds of valid work so far (drawing the right way on her out-breaths).</summary>
        public float HeldSeconds => _held;
        /// <summary>A hand is on the keys right now.</summary>
        public bool HandOnKeys => _onKeys;
        /// <summary>The ring with its keys (hidden once taken; the hook stays on her).</summary>
        public GameObject Ring => _bunch != null ? _bunch : _keyRing;
        /// <summary>Kiều is leaning in over the keys (the close view is on or coming).</summary>
        public bool InMinigame => _session != Session.None;
        /// <summary>The close view has settled: the keys can be taken hold of.</summary>
        public bool Ready => _session == Session.Open;
        /// <summary>Stage under way: 0 lift, 1 off the hook, 2 draw away.</summary>
        public int Stage => Mathf.Min(2, (int)(Progress * 3f));
        /// <summary>The way to draw the mouse now (screen, unit length).</summary>
        public Vector2 PullDirection => _view != null ? _view.ScreenDirection(Stage) : Vector2.up;
        public Pull LastPull { get; private set; }
        public KeyTheftView View => _view;
        public bool IsAvailable => _sleeper != null && _sleeper.InBed && _sleeper.State != TuBaSleeper.Sleep.Awake && _inventory != null && !_inventory.Has(_item);

        private static readonly string[] Steps = { "nâng chùm chìa", "gỡ vòng khỏi móc", "rút chùm chìa ra" };

        public string DisplayName
        {
            get
            {
                if (_sleeper == null) return "";
                if (_session == Session.None) return "Cúi sát lấy chùm chìa";
                if (_sleeper.State == TuBaSleeper.Sleep.Roused) return "<color=#E07A6A>Mụ đang thức, nằm nghe ngóng — đừng chạm vào!</color>";
                if (_sleeper.State == TuBaSleeper.Sleep.Stirring) return "<color=#E07A6A>Mụ đang cựa mình — buông tay, đứng im!</color>";
                string step = $"({Stage + 1}/3) {Steps[Stage]}";
                if (_session == Session.Entering) return step;
                if (_sleeper.Inhaling) return $"<color=#E07A6A>Hít vào — buông E</color>  <size=80%>{step}</size>";
                string way = Way(PullDirection);
                string colour = _sleeper.PhaseLeft < 0.5f ? "#E8B455" : "#BFD8B0";
                string doing = _needRelease ? "buông E rồi giữ lại" : !_onKeys ? $"giữ E, kéo {way}"
                    : LastPull == Pull.TooFast ? "chậm thôi…" : LastPull == Pull.Wrong ? $"kéo {way}" : LastPull == Pull.Still ? $"kéo chuột {way}" : $"kéo {way}";
                return $"<color={colour}>Thở ra — {doing}</color>  <size=80%>{step} · S: lùi ra</size>";
            }
        }

        private static string Way(Vector2 d)
        {
            string vertical = d.y > 0.38f ? "lên" : d.y < -0.38f ? "xuống" : null;
            string horizontal = d.x > 0.38f ? "sang phải" : d.x < -0.38f ? "sang trái" : null;
            return vertical != null && horizontal != null ? vertical + ", " + horizontal : vertical ?? horizontal ?? "lên";
        }

        public void Interact() { }

        public float Tick(bool pressed, bool held)
        {
            // A frame without a Tick means the hand had left the keys (controls taken, another target).
            if (_lastTick != Time.frameCount - 1) _onKeys = false;
            _lastTick = Time.frameCount;
            if (_session == Session.None)
            {
                if (!IsAvailable || CatchScene.Playing) return -1f;
                if (pressed) Begin();
                return Shown;
            }
            if (!IsAvailable || CatchScene.Playing) { End(CatchScene.Playing); return -1f; }
            LastPull = Pull.None;
            if (_session == Session.Entering)
            {
                // Leaning in: nothing is touched yet, whatever the breath.
                if (!held) _needRelease = false;
                if (_view == null || _view.Ready) _session = Session.Open;
                Pose();
                return Shown;
            }
            if (InputReader.Move.y < -0.6f)
            {
                // Backing out with the hand still on the keys as the breath turns is the same as holding on.
                if (_onKeys && held && _sleeper.InhaledThisFrame > 0f) { Grabbed(); return Progress; }
                End(false);
                return Shown;
            }
            if (!held) { _onKeys = false; _needRelease = false; Pose(); return Shown; }
            if (!_onKeys)
            {
                if (!pressed || _needRelease) { Pose(); return Shown; } // E carried over: let go and press again
                _onKeys = true;
                _pull = _across = 0f; // a new grip: the last one's drawing does not carry over
            }
            // The hand is on the keys. Wrong moment: she breathes in (or the breath turned while it stayed there this
            // frame), or she is not asleep. Nothing is added on that frame.
            bool turnedUnderHand = _sleeper.InhaledThisFrame > 0f && !pressed;
            if (!_sleeper.Exhaling || turnedUnderHand) { Grabbed(); return Progress; }
            if (Work())
            {
                _held = Mathf.Min(_seconds, _held + _sleeper.ExhaledThisFrame);
                if ((_tick -= Time.deltaTime) <= 0f) { _tick = 0.7f; if (_audio != null) _audio.PlayAt("keys_soft", transform.position, 0.08f, 3f); }
                if (Stage != _lastStage) { _lastStage = Stage; if (_audio != null) _audio.PlayAt("keys_soft", transform.position, 0.16f, 3f, 1.15f); }
            }
            Pose();
            if (_held < _seconds) return Progress;
            Complete();
            return 1f;
        }

        /// <summary>
        /// This frame's pull: the mouse drawn the way shown, steadily and gently (filtered over about a tenth of a second, so
        /// a tremor does not count and a flick does not finish anything; speed in screen heights, so the screen size and
        /// mouse do not matter much). True when it counts.
        /// </summary>
        private bool Work()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return false;
            Vector2 look = InputReader.Look;
            Vector2 way = PullDirection;
            float height = Mathf.Max(1f, Screen.height);
            float along = Vector2.Dot(look, way) / height / dt;
            float across = (look - way * Vector2.Dot(look, way)).magnitude / height / dt;
            float k = 1f - Mathf.Exp(-dt / 0.1f);
            _pull = Mathf.Lerp(_pull, along, k);
            _across = Mathf.Lerp(_across, across, k);
            if (_pull > _maxPull) { LastPull = Pull.TooFast; return false; }
            if (_pull < -_minPull || _across > Mathf.Max(_pull * 1.2f, 0.05f)) { LastPull = Pull.Wrong; return false; }
            if (_pull < _minPull) { LastPull = Pull.Still; return false; }
            LastPull = Pull.Right;
            return true;
        }

        private float Shown => _held > 0f || _session != Session.None ? Progress : -1f;

        private void Pose()
        {
            if (_view == null) return;
            _view.Pose(Progress, _onKeys, LastPull == Pull.TooFast ? 1f : 0f);
            _view.Hint(Stage, _session == Session.Open && !_sleeper.Inhaling && !_sleeper.Restless);
        }

        /// <summary>
        /// She felt the hand. The close view ends first, without moving the camera: her catch scene (or the failure, when it
        /// cannot be staged) then takes the camera and the controls from exactly where they are, with no free frame between.
        /// </summary>
        private void Grabbed()
        {
            _onKeys = false;
            End(true);
            _sleeper.CatchHand(_sleeper.Restless ? TuBaSleeper.Grab.Restless : TuBaSleeper.Grab.BreathingIn);
        }

        private void Complete()
        {
            _held = _seconds;
            _onKeys = false;
            _inventory.Add(_item);
            if (Ring != null) Ring.SetActive(false);
            End(false);
            Taken?.Invoke(this);
        }

        /// <summary>Leans in: the keys pinned as the target, Kiều held still, the camera to the close view.</summary>
        private void Begin()
        {
            _session = Session.Entering;
            _needRelease = true; // the press that opened it is still down
            _onKeys = false;
            _pull = _across = 0f;
            _lastStage = Stage;
            PlayerInteraction.Pinned = this;
            if (_player != null) _player.HeldStill = true;
            if (_view != null) _view.Begin();
        }

        /// <summary>
        /// Out of the close view: Kiều may walk again (the story's and menus' own control locks are theirs and untouched).
        /// <paramref name="instant"/> when a catch scene takes the camera from where it is.
        /// </summary>
        private void End(bool instant)
        {
            if (_session == Session.None) return;
            _session = Session.None;
            _onKeys = false;
            if (ReferenceEquals(PlayerInteraction.Pinned, this)) PlayerInteraction.Pinned = null;
            if (_player != null) _player.HeldStill = false;
            if (_view != null) _view.End(instant);
        }

        /// <summary>
        /// The keys stopped being the target: the controls were taken. If the breath turned in this very frame with E still
        /// down and a hand on the keys, it was still there when she breathed in. The pause menu (or the shell's overlay)
        /// only takes the hand off: the close view and the work wait for it. Anything else (a conversation, a catch) ends the
        /// close view.
        /// </summary>
        public void Lost()
        {
            bool wasOn = _onKeys;
            _onKeys = false;
            if (wasOn && IsAvailable && !CatchScene.Playing && _lastTick == Time.frameCount - 1 && InputReader.IsInteractHeld && _sleeper.InhaledThisFrame > 0f)
            {
                Grabbed();
                return;
            }
            if (GamePause.IsPaused && !CatchScene.Playing && IsAvailable) return;
            End(CatchScene.Playing);
        }

        /// <summary>Checkpoint restore: the ring back on its hook (or gone), no half-done work, out of the close view.</summary>
        public void Restore()
        {
            End(true);
            bool taken = _inventory != null && _inventory.Has(_item);
            _held = taken ? _seconds : 0f;
            _onKeys = false;
            if (_keyRing != null) _keyRing.SetActive(true);
            if (Ring != null) Ring.SetActive(!taken);
            if (_view != null) _view.ResetPose();
            Follow();
        }

        private void OnDisable() { End(true); }

        private void LateUpdate() { Follow(); }

        /// <summary>The trigger sits on the keys wherever her pose has put them (lying, she turns them with her hip).</summary>
        private void Follow()
        {
            var grip = _grip != null ? _grip : _keyRing != null ? _keyRing.transform : null;
            if (grip != null && grip.gameObject.activeInHierarchy) transform.position = grip.position;
        }
    }
}
