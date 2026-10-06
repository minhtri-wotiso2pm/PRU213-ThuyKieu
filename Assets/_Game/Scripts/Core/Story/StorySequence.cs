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
        public abstract IEnumerator Run(StoryDirector director);
    }
}
