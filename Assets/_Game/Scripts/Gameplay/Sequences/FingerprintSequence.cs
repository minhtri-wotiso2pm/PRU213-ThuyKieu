using System;
using System.Collections;
using ThuyKieu.Core;
using ThuyKieu.Core.Story;
using UnityEngine;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>
    /// Pressing a fingerprint onto a contract: the player holds the left mouse button (or E) until the thumb is down.
    /// Letting go lifts it again, so signing is a held decision rather than a click. The world animates the hand from
    /// <see cref="Progress"/> and lays the red mark when <see cref="Pressed"/> fires.
    /// </summary>
    public class FingerprintSequence : StorySequence
    {
        private const float HoldSeconds = 2.2f;
        private static readonly Color Cinnabar = new Color(0.78f, 0.12f, 0.1f);

        /// <summary>0 = hand hovering, 1 = thumb pressed down.</summary>
        public float Progress { get; private set; }
        public bool Done { get; private set; }

        /// <summary>The HUD controls line to put back afterwards.</summary>
        public string ControlsAfter { get; set; }

        public event Action Pressed;

        public static FingerprintSequence Create(GameObject host, string id)
        {
            var sequence = host.AddComponent<FingerprintSequence>();
            sequence.AssignId(id);
            return sequence;
        }

        public override IEnumerator Run(StoryDirector director)
        {
            Progress = 0f;
            Done = false;
            var hud = director.Hud;
            hud.SetControls("Giữ chuột trái (hoặc E) để điểm chỉ · buông ra để nhấc tay");
            if (director.Audio != null) director.Audio.PlaySfx("heartbeat", 0.6f);
            yield return new WaitForSeconds(director.Autopilot ? 0f : 0.6f);
            while (Progress < 1f)
            {
                if (director.Autopilot) Progress = 1f;
                else if (!GamePause.IsPaused)
                {
                    bool holding = InputReader.IsPrimaryHeld || InputReader.IsInteractHeld;
                    // Pressing is slow and deliberate; lifting off is quicker.
                    Progress = holding ? Mathf.Min(1f, Progress + Time.deltaTime / HoldSeconds)
                                       : Mathf.Max(0f, Progress - Time.deltaTime / (HoldSeconds * 0.5f));
                    hud.SetGauge(Progress <= 0f ? "Giữ chuột trái để điểm chỉ" : "Điểm chỉ...", Progress, Cinnabar);
                }
                yield return null;
            }
            Done = true;
            if (director.Audio != null) director.Audio.PlaySfx("stamp");
            if (director.Fx != null) director.Fx.Shake(0.06f, 0.15f);
            Pressed?.Invoke();
            yield return new WaitForSeconds(director.Autopilot ? 0f : 1.4f);
            if (!string.IsNullOrEmpty(ControlsAfter)) hud.SetControls(ControlsAfter);
        }
    }
}
