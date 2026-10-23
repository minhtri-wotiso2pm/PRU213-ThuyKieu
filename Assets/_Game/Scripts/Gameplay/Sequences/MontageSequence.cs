using System.Collections;
using ThuyKieu.Core;
using ThuyKieu.Core.Story;
using ThuyKieu.Environment;
using ThuyKieu.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>
    /// A few seconds of stills in place of narration ("# montage:&lt;id&gt;" plays before its line). Each still is the live
    /// scene rendered from the panel's shot at the moment it cuts in, so staging that is still moving underneath (an
    /// arrival, a fade to rain) shows its newest state; painted art in Resources/StoryPanels wins when present.
    /// Registered at runtime by the director, so no scene needs to author it.
    /// </summary>
    public class MontageSequence : StorySequence
    {
        public const string Prefix = "montage_";

        public static MontageSequence Create(GameObject host, string id)
        {
            var sequence = host.AddComponent<MontageSequence>();
            sequence.AssignId(id);
            return sequence;
        }

        public override IEnumerator Run(StoryDirector director)
        {
            var montage = MontageCatalog.Get(Id);
            if (montage == null) yield break;
            var view = MontageView.Create();
            yield return FadeView(view, 0f, 1f, director.Autopilot ? 0f : 0.25f);
            if (!director.Autopilot) yield return new WaitForSeconds(montage.Settle);

            RenderTexture previous = null;
            for (int i = 0; i < montage.Panels.Length; i++)
            {
                var panel = montage.Panels[i];
                Texture art = Resources.Load<Texture2D>("StoryPanels/" + Id + "_" + (i + 1));
                RenderTexture still = art == null ? Render(director, panel.Shot) : null;
                view.Show(art != null ? art : still, panel.Caption, i);
                if (previous != null) Destroy(previous);
                previous = still;
                if (director.Audio != null) director.Audio.PlaySfx("whoosh", 0.3f);
                yield return Hold(director, view, panel.Seconds, i);
            }
            yield return FadeView(view, 1f, 0f, director.Autopilot ? 0f : 0.3f);
            if (previous != null) Destroy(previous);
            Destroy(view.gameObject);
        }

        private IEnumerator Hold(StoryDirector director, MontageView view, float seconds, int index)
        {
            if (director.Autopilot)
            {
                yield return new WaitForSecondsRealtime(StoryCapture.Hold);
                StoryCapture.Shot("MONTAGE_" + Id + "_" + (index + 1));
                yield break;
            }
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                view.Tick(t, t / seconds);
                if (t > 0.3f && !GamePause.IsPaused && SkipPressed()) break;
                yield return null;
            }
        }

        private static bool SkipPressed()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            return (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame))
                || (mouse != null && mouse.leftButton.wasPressedThisFrame);
        }

        private static IEnumerator FadeView(MontageView view, float from, float to, float duration)
        {
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                view.SetAlpha(Mathf.Lerp(from, to, t / duration));
                yield return null;
            }
            view.SetAlpha(to);
        }

        /// <summary>Renders the main camera from the shot into a screen-sized still (overlay UI is not drawn into it).</summary>
        private static RenderTexture Render(StoryDirector director, string shot)
        {
            var camera = Camera.main;
            if (camera == null) return null;
            Transform lens = camera.transform;
            Vector3 position = lens.position;
            Quaternion rotation = lens.rotation;
            if (TryShot(director, shot, out Vector3 at, out Quaternion look)) lens.SetPositionAndRotation(at, look);
            int width = Mathf.Max(640, Screen.width), height = Mathf.Max(360, Screen.height);
            PlanarMirror.RenderAllFor(camera, width, height);
            var still = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
            var target = camera.targetTexture;
            camera.targetTexture = still;
            camera.Render();
            camera.targetTexture = target;
            lens.SetPositionAndRotation(position, rotation);
            return still;
        }

        /// <summary>A CAM_ anchor, or a close-up (CU_) / medium close-up (MCU_) of a speaker from in front of their face.</summary>
        private static bool TryShot(StoryDirector director, string shot, out Vector3 position, out Quaternion rotation)
        {
            position = default;
            rotation = default;
            var anchor = GameObject.Find("CAM_" + shot);
            if (anchor != null)
            {
                position = anchor.transform.position;
                rotation = anchor.transform.rotation;
                return true;
            }
            string[] parts = shot.Split('_');
            if (parts.Length < 2 || (parts[0] != "CU" && parts[0] != "MCU")) return false;
            var actor = director.FindActor(parts[1]);
            if (actor == null || !actor.isActiveAndEnabled) return false;
            Vector3 head = actor.HeadPosition;
            Vector3 forward = actor.transform.forward;
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            float distance = parts[0] == "CU" ? 1.1f : 1.6f;
            // Slightly off-axis and a touch above the eyes reads as a portrait rather than a passport photo.
            position = head + forward * distance + right * (distance * 0.28f) + Vector3.up * 0.06f;
            rotation = Quaternion.LookRotation(head - Vector3.up * 0.05f - position, Vector3.up);
            return true;
        }
    }
}
