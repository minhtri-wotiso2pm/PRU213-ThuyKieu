using System.Collections;
using ThuyKieu.Core.Story;
using UnityEngine;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>
    /// Emotional beat between lines: switches the screen mood, plays a sting and holds briefly.
    /// The mood persists until an Ink "# fx:none" tag.
    /// </summary>
    public class FxSequence : StorySequence
    {
        [SerializeField] private string _mood = "memory";
        [SerializeField] private string _sfx = "memory";
        [SerializeField] private string _music;
        [SerializeField] private float _hold = 1.6f;
        [SerializeField] private float _shake;
        [SerializeField] private int _repeatSfx = 1;

        public override IEnumerator Run(StoryDirector director)
        {
            if (director.Fx != null)
            {
                director.Fx.SetMood(_mood);
                if (_shake > 0f) director.Fx.Shake(_shake, _hold);
            }
            if (!string.IsNullOrEmpty(_music) && director.Audio != null) director.Audio.PlayMusic(_music);
            for (int i = 0; i < _repeatSfx; i++)
            {
                if (!string.IsNullOrEmpty(_sfx) && director.Audio != null) director.Audio.PlaySfx(_sfx);
                yield return new WaitForSeconds(director.Autopilot ? 0f : _hold / _repeatSfx);
            }
        }
    }
}
