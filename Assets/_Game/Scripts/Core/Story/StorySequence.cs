using System.Collections;
using UnityEngine;

namespace ThuyKieu.Core.Story
{
    /// <summary>
    /// A blocking gameplay beat triggered by "# gameplay:&lt;id&gt;" in Ink. The director closes the
    /// dialogue, runs the coroutine to completion, then continues the story.
    /// </summary>
    public abstract class StorySequence : MonoBehaviour
    {
        [SerializeField] private string _id;
        public string Id => _id;
        /// <summary>Names a sequence added at runtime (scene-authored ones keep their serialized id).</summary>
        public void AssignId(string id) { if (string.IsNullOrEmpty(_id)) _id = id; }
        public abstract IEnumerator Run(StoryDirector director);
    }
}
