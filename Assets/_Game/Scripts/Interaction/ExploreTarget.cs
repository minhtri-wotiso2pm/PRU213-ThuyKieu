using ThuyKieu.Core.Story;
using UnityEngine;

namespace ThuyKieu.Interaction
{
    /// <summary>
    /// World object that resolves an Ink "#explore:&lt;id&gt;" choice when the player presses E.
    /// Available only while that choice is on offer (and, optionally, once revealed by the world).
    /// </summary>
    public class ExploreTarget : MonoBehaviour, IInteractable, IInteractableDisplayName, IInteractionAvailability
    {
        [SerializeField] private StoryDirector _director;
        [SerializeField] private string _exploreId;
        [SerializeField] private string _label;
        [Tooltip("Hidden until the world calls Reveal() (e.g. a paper that has not fallen yet).")]
        [SerializeField] private bool _startHidden;
        [SerializeField] private GameObject _highlight;

        private bool _revealed;

        public string ExploreId => _exploreId;
        public string DisplayName => _label;
        public bool IsRevealed => _revealed;
        public bool IsAvailable => _revealed && _director != null && _director.IsExploreAvailable(_exploreId);

        private void Awake() { _revealed = !_startHidden; }

        public void Reveal(bool revealed = true) { _revealed = revealed; }

        public void Interact()
        {
            if (IsAvailable) _director.ChooseExplore(_exploreId, transform);
        }

        private void Update()
        {
            if (_highlight != null && _highlight.activeSelf != IsAvailable) _highlight.SetActive(IsAvailable);
        }
    }
}
