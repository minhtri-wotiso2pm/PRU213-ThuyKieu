using System.Collections;
using System.Linq;
using ThuyKieu.Core;
using ThuyKieu.Core.Story;
using UnityEngine;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>
    /// Lifting the key ring from a dozing Tú Bà: hold E to inch the hand forward, but only while she breathes out.
    /// Moving while she breathes in makes her stir; if she stirs fully Kiều pulls back and loses part of the progress.
    /// </summary>
    public class PickpocketSequence : StorySequence
    {
        [SerializeField] private StoryActor _sleeper;
        [SerializeField] private Transform _shot;
        [Tooltip("Where Kiều crouches beside the sleeper so the shot frames them both.")]
        [SerializeField] private Transform _stand;
        [SerializeField] private float _seconds = 3.2f;
        [SerializeField] private Vector2 _breath = new Vector2(2.6f, 3.8f);
        [Tooltip("Share of each breath spent breathing out (the safe window).")]
        [SerializeField, Range(0.2f, 0.8f)] private float _exhaleShare = 0.55f;
        [SerializeField] private string _variable = "c4_bi_bat";

        public override IEnumerator Run(StoryDirector director)
        {
            var hud = director.Hud;
            var kieu = director.FindActor("Kieu");
            director.SetFreeControl(false);
            if (_stand != null) director.Player.Teleport(_stand.position, _stand.rotation);
            if (_shot != null) director.CameraController.CutTo(_shot);
            if (director.Audio != null) { director.Audio.PlayMusic("stealth"); director.Audio.Duck(0.35f); }
            if (kieu != null && _sleeper != null) kieu.FaceInstant(_sleeper.transform.position);
            if (kieu != null) kieu.SetTalking(true, 6);

            float progress = 0f, stir = 0f, phase = 0f, period = Random.Range(_breath.x, _breath.y);
            bool exhaling = false;
            bool thorough = director.Autopilot && StoryAutopilot.Args.Contains("-autopilotThorough"), forced = false;
            while (progress < 1f)
            {
                phase += Time.deltaTime / period;
                if (phase >= 1f) { phase -= 1f; period = Random.Range(_breath.x, _breath.y); }
                bool nowExhaling = phase < _exhaleShare;
                if (nowExhaling && !exhaling && director.Audio != null) director.Audio.PlaySfx("snore", 0.35f);
                exhaling = nowExhaling;

                // QA: only reach while she breathes out, and not in the last sliver of it.
                bool held = director.Autopilot ? exhaling && phase < _exhaleShare - 0.1f : InputReader.IsInteractHeld;
                if (held && exhaling) progress += Time.deltaTime / _seconds;
                else if (held) stir += Time.deltaTime * 1.6f;
                else stir = Mathf.Max(0f, stir - Time.deltaTime * 0.5f);

                hud.SetMeter(exhaling ? "Mụ THỞ RA — giữ E để nhích tay" : "Mụ HÍT VÀO — buông E, đứng yên!", progress,
                    exhaling ? new Color(1f, 0.8f, 0.45f) : new Color(0.9f, 0.3f, 0.25f));
                hud.SetGauge("Tú Bà cựa mình", stir, new Color(0.9f, 0.3f, 0.25f));

                if (thorough && !forced && progress > 0.3f) { forced = true; stir = 1f; } // QA: exercise the stir → retry path
                if (stir >= 1f)
                {
                    StoryAutopilot.Note("STIRRED in pickpocket → retry");
                    if (director.Audio != null) director.Audio.PlaySfx("detect");
                    if (director.Fx != null) director.Fx.Shake(0.2f, 0.3f);
                    hud.ShowBark("TuBa", "Ư... ai... (lẩm bẩm rồi ngủ tiếp)", 2f);
                    yield return hud.FailCard("Tú Bà cựa mình!", "Kiều vội rụt tay lại. Chỉ nhích tay khi mụ thở ra — mụ hít vào thì đứng yên.", director.Autopilot ? 0f : 1.8f);
                    director.SetInt(_variable, director.GetInt(_variable) + 1);
                    progress *= 0.4f;
                    stir = 0f;
                    yield return hud.Fade(0f, 0.4f);
                }
                yield return null;
            }

            if (director.Audio != null) { director.Audio.PlaySfx("lock_rattle", 0.4f); director.Audio.Duck(1f); director.Audio.PlayMusic("tension_low"); }
            if (kieu != null) kieu.SetTalking(false, 0);
            director.CameraController.SetDialogueCue(null);
        }
    }
}
