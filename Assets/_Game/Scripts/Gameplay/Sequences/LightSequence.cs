using System.Collections;
using ThuyKieu.Core.Story;
using UnityEngine;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>"Rồi ánh sáng xuất hiện": the screen swells to white and stays there while the chapter closes.</summary>
    public class LightSequence : StorySequence
    {
        [SerializeField] private float _duration = 2.6f;

        public override IEnumerator Run(StoryDirector director)
        {
            if (director.Fx != null) director.Fx.SetMood("spirit");
            if (director.Audio != null) { director.Audio.PlaySfx("memory"); director.Audio.PlayMusic("spirit"); }
            director.Hud.SetFadeColor(new Color(1f, 0.97f, 0.9f));
            yield return director.Hud.Fade(1f, director.Autopilot ? 0f : _duration);
            yield return new WaitForSeconds(director.Autopilot ? 0f : 0.8f);
        }
    }
}
