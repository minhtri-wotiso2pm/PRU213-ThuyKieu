using System.Collections;
using ThuyKieu.Core.Story;
using UnityEngine;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>Contextual how-to-play card shown the first time a mechanic appears.</summary>
    public class TutorialSequence : StorySequence
    {
        [SerializeField, TextArea(4, 10)] private string _body;
        [SerializeField] private string _controlsHint;

        public override IEnumerator Run(StoryDirector director)
        {
            director.Hud.FastMode = director.Autopilot;
            if (!string.IsNullOrEmpty(_controlsHint)) director.Hud.SetControls(_controlsHint);
            yield return null; // after DialogueCursorController re-locks on dialogue end
            director.CameraController.SetCursorLocked(false);
            yield return director.Hud.TutorialCard(_body);
        }
    }
}
