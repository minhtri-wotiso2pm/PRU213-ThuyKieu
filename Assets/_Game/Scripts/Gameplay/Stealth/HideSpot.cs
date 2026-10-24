using ThuyKieu.Interaction;
using ThuyKieu.Player;
using UnityEngine;

namespace ThuyKieu.Gameplay.Stealth
{
    public enum HideKind { Auto, Wardrobe, Screen, Curtain, Low }

    /// <summary>
    /// A wardrobe, a curtain or the space under a bed: E to slip inside (guards cannot see the player),
    /// E again to come out where the player stood.
    /// </summary>
    public class HideSpot : MonoBehaviour, IInteractable, IInteractableDisplayName, IInteractionAvailability
    {
        [SerializeField] private PlayerMovement _player;
        [SerializeField] private Transform _inside;
        [SerializeField] private string _label = "Trốn vào tủ";
        [SerializeField] private string _sfx = "door";
        [Tooltip("First person: how far (degrees either side) she can look while hidden, peeking through the slats.")]
        [SerializeField] private float _peekRange = 55f;
        [Tooltip("Optional: the door or curtain a searcher rattles when he checks this place.")]
        [SerializeField] private Transform _shakeVisual;
        [Tooltip("Low cover (jars, a screen): she crouches while hidden here.")]
        [SerializeField] private bool _crouch;
        [Tooltip("What sort of place it is, so whoever finds her opens it the right way (Auto: guessed from the other settings).")]
        [SerializeField] private HideKind _kind = HideKind.Auto;

        private bool _wasForcedCrouch;

        private Vector3 _exitPosition;
        private Quaternion _exitRotation;
        private Renderer[] _renderers;
        private float _shake;
        private Vector3 _shakeBase;

        public static HideSpot Current { get; private set; }
        /// <summary>Someone slipped inside: watchers who saw it happen remember this place.</summary>
        public static event System.Action<HideSpot> Entered;
        public static event System.Action<HideSpot> Exited;
        public Transform Inside => _inside;
        public float PeekRange => _peekRange;
        public Vector3 ExitPosition => _exitPosition;
        /// <summary>The door or screen that moves when this place is searched (null for a curtain or open cover).</summary>
        public Transform ShakeVisual => _shakeVisual;
        /// <summary>Low cover (jars, a screen, a heap): she crouches behind it rather than standing inside.</summary>
        public bool IsLowCover => _crouch;
        /// <summary>The sound of this place being disturbed (cloth, paper, a door).</summary>
        public string SoundId => _sfx;

        /// <summary>Wardrobe (doors pulled open), screen (shoved aside), curtain (swept back) or low cover (reached over).</summary>
        public HideKind Kind
        {
            get
            {
                if (_kind != HideKind.Auto) return _kind;
                if (_shakeVisual != null) return _shakeVisual.name == "Doors" ? HideKind.Wardrobe : HideKind.Screen;
                return _crouch ? HideKind.Low : HideKind.Curtain;
            }
        }

        public string DisplayName => Current == this ? "Ra khỏi chỗ trốn" : _label;
        public bool IsAvailable => Current == this || (Current == null && !_player.ControlLocked);

        public void Interact()
        {
            if (Current == this) Exit();
            else if (Current == null) Enter();
        }

        private void Enter()
        {
            _exitPosition = _player.transform.position;
            _exitRotation = _player.transform.rotation;
            _renderers = _player.GetComponentsInChildren<Renderer>();
            Current = this;
            // Watchers judge whether they saw her go in before she vanishes from sight.
            Entered?.Invoke(this);
            _player.Teleport(_inside.position, _inside.rotation);
            _player.ControlLocked = true;
            _player.IsHidden = true;
            foreach (var renderer in _renderers) renderer.enabled = false;
            if (_player.IsFirstPerson) PlayerInteraction.Pinned = this;
            _wasForcedCrouch = _player.ForceCrouch;
            if (_crouch) _player.ForceCrouch = true;
            Sound();
        }

        public void Exit()
        {
            if (Current != this) return;
            Current = null;
            if (ReferenceEquals(PlayerInteraction.Pinned, this)) PlayerInteraction.Pinned = null;
            if (_crouch) _player.ForceCrouch = _wasForcedCrouch;
            _player.Teleport(_exitPosition, _exitRotation);
            _player.ControlLocked = false;
            _player.IsHidden = false;
            if (_renderers != null) foreach (var renderer in _renderers) if (renderer != null) renderer.enabled = true;
            Sound();
            Exited?.Invoke(this);
        }

        /// <summary>A searcher yanks at the door or curtain (visual only; the watcher decides what he finds).</summary>
        public void Rattle(float seconds)
        {
            if (_shakeVisual == null) return;
            if (_shake <= 0f) _shakeBase = _shakeVisual.localPosition;
            _shake = seconds;
        }

        /// <summary>Checkpoints and cutscenes pull the player out of any hiding place.</summary>
        public static void ExitAll()
        {
            if (Current == null) return;
            var spot = Current;
            spot.Exit();
        }

        private void Update()
        {
            // Story beats unlock movement; while hidden the player stays put until they press E.
            if (Current == this && !_player.ControlLocked && _player.IsHidden) _player.ControlLocked = true;
            if (_shake > 0f && _shakeVisual != null)
            {
                _shake -= Time.deltaTime;
                _shakeVisual.localPosition = _shake > 0f ? _shakeBase + new Vector3(Mathf.Sin(Time.time * 47f), 0f, Mathf.Sin(Time.time * 31f)) * 0.012f : _shakeBase;
            }
        }

        private void OnDisable() { if (Current == this) Exit(); }

        private void Sound()
        {
            var audio = FindAnyObjectByType<ThuyKieu.Presentation.StoryAudio>();
            if (audio != null && !string.IsNullOrEmpty(_sfx)) audio.PlaySfx(_sfx, 0.5f);
        }
    }
}
