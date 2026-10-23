using System.Collections;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Chapters;
using UnityEngine;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>The prologue's crossing from the classroom into Kiều's room, run as a blocking beat so no line plays under the title card.</summary>
    public class WorldBeatSequence : StorySequence
    {
        [SerializeField] private PrologueWorld _world;

        public override IEnumerator Run(StoryDirector director)
        {
            yield return _world.IntoKieu();
        }
    }
}
