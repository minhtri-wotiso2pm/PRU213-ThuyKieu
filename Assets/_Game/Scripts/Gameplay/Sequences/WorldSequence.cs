using System;
using System.Collections;
using ThuyKieu.Core.Story;
using UnityEngine;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>
    /// A "# gameplay:" beat whose body lives in a chapter's world script, because it needs that chapter's actors and
    /// staging (e.g. Kiều running from the tea house in Chapter 2).
    /// </summary>
    public class WorldSequence : StorySequence
    {
        private Func<StoryDirector, IEnumerator> _body;

        public static WorldSequence Create(GameObject host, string id, Func<StoryDirector, IEnumerator> body)
        {
            var sequence = host.AddComponent<WorldSequence>();
            sequence.AssignId(id);
            sequence._body = body;
            return sequence;
        }

        public override IEnumerator Run(StoryDirector director) => _body(director);
    }
}
