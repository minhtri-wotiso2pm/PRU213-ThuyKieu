using System.Collections;

namespace ThuyKieu.Core.Story
{
    /// <summary>
    /// A chapter world whose progress happens in the world rather than through explore anchors (first-person chapters:
    /// latches, keys, doors). The QA autopilot hands it each explore turn and it plays one step of the chapter.
    /// </summary>
    public interface IAutopilotDriver
    {
        /// <param name="thorough">QA asked for every branch and failure path, not one quick run.</param>
        IEnumerator AutopilotStep(bool thorough);
    }
}
