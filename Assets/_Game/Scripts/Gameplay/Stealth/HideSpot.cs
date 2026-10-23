using ThuyKieu.Interaction;
using ThuyKieu.Player;
using UnityEngine;

namespace ThuyKieu.Gameplay.Stealth
{
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

        private Vector3 _exitPosition;
        private Quaternion _exitRotation;
        private Renderer[] _renderers;

        public static HideSpot Current { get; private set; }

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
            _player.Teleport(_inside.position, _inside.rotation);
            _player.ControlLocked = true;
            _player.IsHidden = true;
            foreach (var renderer in _renderers) renderer.enabled = false;
            Sound();
        }

        public void Exit()
        {
            if (Current != this) return;
            Current = null;
            _player.Teleport(_exitPosition, _exitRotation);
            _player.ControlLocked = false;
            _player.IsHidden = false;
            if (_renderers != null) foreach (var renderer in _renderers) if (renderer != null) renderer.enabled = true;
            Sound();
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
        }

        private void OnDisable() { if (Current == this) Exit(); }

        private void Sound()
        {
            var audio = FindAnyObjectByType<ThuyKieu.Presentation.StoryAudio>();
            if (audio != null && !string.IsNullOrEmpty(_sfx)) audio.PlaySfx(_sfx, 0.5f);
        }
    }
}
