using System.Collections;
using System.Linq;
using ThuyKieu.Core.Story;
using ThuyKieu.Player;
using UnityEngine;

namespace ThuyKieu.Gameplay.Chapters.LauTuBa
{
    /// <summary>
    /// QA: in Play Mode, shoots the scene from every LOOK_* viewpoint at a fixed size and field of view, with the HUD
    /// hidden, so before/after pictures of the environment line up exactly. Runs only with "-autopilotViews"
    /// (and "-autopilotCapture &lt;folder&gt;" for where the pictures go).
    /// </summary>
    public static class ViewTour
    {
        public const int Width = 1920, Height = 1080;
        public const float FieldOfView = 72f;

        public static bool Requested => System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-autopilotViews") >= 0 && StoryCapture.Directory != null;

        /// <summary>Shoots every LOOK_* point as "VIEW_&lt;prefix&gt;_&lt;look&gt;", then gives the camera back.</summary>
        public static IEnumerator Shoot(string prefix)
        {
            var camera = Camera.main;
            if (camera == null) yield break;
            var rig = camera.GetComponent<ThirdPersonCameraController>();
            var looks = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include).Where(t => t.name.StartsWith("LOOK_")).OrderBy(t => t.name).ToArray();
            var canvases = Object.FindObjectsByType<Canvas>().Where(c => c.isRootCanvas && c.enabled).ToArray();
            float fov = camera.fieldOfView;
            if (rig != null) rig.enabled = false;
            foreach (var canvas in canvases) canvas.enabled = false;
            foreach (var look in looks)
            {
                camera.transform.SetPositionAndRotation(look.position, look.rotation);
                camera.fieldOfView = FieldOfView;
                // A few frames so temporal effects, probes and the rain settle at the new place.
                for (int i = 0; i < 8; i++) yield return null;
                StoryCapture.Shot("VIEW_" + prefix + "_" + look.name.Substring(5), Width, Height);
            }
            camera.fieldOfView = fov;
            foreach (var canvas in canvases) if (canvas != null) canvas.enabled = true;
            if (rig != null) rig.enabled = true;
            ExposureMap("EXPOSURE_" + prefix);
        }

        /// <summary>
        /// Top-down map of what the guards treat as lit (<see cref="Stealth.LightExposure"/> at chest height over every
        /// floor), to set beside the screenshots: dark blue = shadow, yellow = lamplight, white = fully lit, grey = walls.
        /// </summary>
        private static void ExposureMap(string name)
        {
            if (!Stealth.LightExposure.Enabled) return;
            const float step = 0.12f, x0 = -24.6f, x1 = 21.4f, z0 = -19.6f, z1 = 19.6f;
            int w = Mathf.CeilToInt((x1 - x0) / step), h = Mathf.CeilToInt((z1 - z0) / step);
            var image = new Texture2D(w, h, TextureFormat.RGB24, false);
            var histogram = new int[5];
            for (int j = 0; j < h; j++)
                for (int i = 0; i < w; i++)
                {
                    // From under the ceilings (no colliders up there) down to the floor; walls and furniture at chest height show grey.
                    var top = new Vector3(x0 + (i + 0.5f) * step, 2.7f, z0 + (j + 0.5f) * step);
                    Color c = Color.black;
                    if (Physics.Raycast(top, Vector3.down, out var hit, 4f, 1 << 8, QueryTriggerInteraction.Ignore))
                    {
                        if (hit.point.y > 0.9f || Physics.CheckSphere(hit.point + Vector3.up * 1.05f, 0.08f, 1 << 8, QueryTriggerInteraction.Ignore)) c = new Color(0.3f, 0.3f, 0.3f);
                        else
                        {
                            float e = Stealth.LightExposure.At(hit.point + Vector3.up * 1.05f);
                            histogram[Mathf.Min(4, (int)(e * 5f))]++;
                            c = e < 0.5f ? Color.Lerp(new Color(0.02f, 0.03f, 0.18f), new Color(0.85f, 0.55f, 0.1f), e * 2f) : Color.Lerp(new Color(0.85f, 0.55f, 0.1f), Color.white, (e - 0.5f) * 2f);
                        }
                    }
                    image.SetPixel(i, j, c);
                }
            image.Apply();
            System.IO.File.WriteAllBytes(System.IO.Path.Combine(StoryCapture.Directory, name + ".png"), image.EncodeToPNG());
            Object.Destroy(image);
            StoryAutopilot.Note(name + " · floor share by exposure 0–0.2/0.2–0.4/0.4–0.6/0.6–0.8/0.8–1: " + string.Join(" / ", histogram));
        }
    }
}
