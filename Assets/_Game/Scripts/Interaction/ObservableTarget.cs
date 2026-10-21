using ThuyKieu.Core.Story;
using UnityEngine;

namespace ThuyKieu.Interaction
{
    /// <summary>
    /// Something worth studying in observation mode (hold right mouse). Holding focus on it long
    /// enough resolves its "#explore" choice.
    /// </summary>
    public class ObservableTarget : MonoBehaviour
    {
        [SerializeField] private StoryDirector _director;
        [SerializeField] private string _exploreId;
        [SerializeField] private string _label = "Quan sát";
        [SerializeField] private Transform _focusPoint;
        [SerializeField] private float _requiredSeconds = 2.6f;
        [SerializeField] private float _maxDistance = 13f;

        public string ExploreId => _exploreId;
        public string Label => _label;
        public float RequiredSeconds => _requiredSeconds;
        public float MaxDistance => _maxDistance;
        public Vector3 FocusPoint => _focusPoint != null ? _focusPoint.position : transform.position + Vector3.up * 1.4f;
        public bool IsAvailable => _director != null && _director.IsExploreAvailable(_exploreId);

        /// <summary>Configures targets created at runtime so existing scenes can gain an observation beat without YAML edits.</summary>
        public void Configure(StoryDirector director, string exploreId, string label, Transform focusPoint,
            float requiredSeconds, float maxDistance)
        {
            _director = director;
            _exploreId = exploreId;
            _label = label;
            _focusPoint = focusPoint;
            _requiredSeconds = requiredSeconds;
            _maxDistance = maxDistance;
        }

        public void Complete() { if (IsAvailable) _director.ChooseExplore(_exploreId, transform); }
    }
}
