using UnityEngine;

namespace ThuyKieu.Core.Story
{
    /// <summary>Keeps an animated model's imported root curves from moving the whole actor.</summary>
    [DefaultExecutionOrder(10000)]
    public sealed class AnimatorRootAnchor : MonoBehaviour
    {
        private Vector3 _localPosition;
        private Quaternion _localRotation;
        private Vector3 _localScale;

        private void OnEnable()
        {
            _localPosition = transform.localPosition;
            _localRotation = transform.localRotation;
            _localScale = transform.localScale;
        }

        /// <summary>Moves the anchored pose on purpose (e.g. StoryActor's seat lift) instead of having it undone next frame.</summary>
        public void Shift(Vector3 localOffset)
        {
            _localPosition += localOffset;
            transform.localPosition += localOffset;
        }

        private void LateUpdate()
        {
            transform.localPosition = _localPosition;
            transform.localRotation = _localRotation;
            transform.localScale = _localScale;
        }
    }
}
