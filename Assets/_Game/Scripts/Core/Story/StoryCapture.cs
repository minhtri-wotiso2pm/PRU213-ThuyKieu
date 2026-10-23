using System.IO;
using UnityEngine;

namespace ThuyKieu.Core.Story
{
    /// <summary>
    /// QA screenshots that work in batch mode: renders the main camera (with the overlay UI
    /// temporarily switched to camera space) into a texture and saves a PNG.
    /// </summary>
    public static class StoryCapture
    {
        /// <summary>How long fast-mode views stay on screen (0 for plain QA runs, longer while capturing).</summary>
        public static float Hold => Directory != null ? 1.6f : 0f;

        public static string Directory
        {
            get
            {
                var args = StoryAutopilot.Args;
                int index = System.Array.IndexOf(args, "-autopilotCapture");
                return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
            }
        }

        /// <summary>-autopilotCaptureLines: shoot every dialogue line (camera review) instead of one per speaker style.</summary>
        public static bool EveryLine => System.Array.IndexOf(StoryAutopilot.Args, "-autopilotCaptureLines") >= 0;

        public static void Shot(string name, int width = 1600, int height = 900)
        {
            string folder = Directory;
            if (folder == null) return;
            System.IO.Directory.CreateDirectory(folder);
            ShotFile(Path.Combine(folder, name + ".png"), width, height);
        }

        /// <summary>The main camera and the HUD as the player sees them, saved to <paramref name="path"/> (QA frames).</summary>
        public static void ShotFile(string path, int width, int height)
        {
            var camera = Camera.main;
            if (camera == null) return;
            var canvases = Object.FindObjectsByType<Canvas>();
            var modes = new RenderMode[canvases.Length];
            for (int i = 0; i < canvases.Length; i++)
            {
                modes[i] = canvases[i].renderMode;
                if (modes[i] != RenderMode.ScreenSpaceOverlay || !canvases[i].isRootCanvas) continue;
                canvases[i].renderMode = RenderMode.ScreenSpaceCamera;
                canvases[i].worldCamera = camera;
                canvases[i].planeDistance = camera.nearClipPlane + 0.05f + 0.01f * canvases[i].sortingOrder / 100f;
            }
            Canvas.ForceUpdateCanvases();
            ThuyKieu.Environment.PlanarMirror.RenderAllFor(camera, width, height);
            var texture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var previous = camera.targetTexture;
            camera.targetTexture = texture;
            camera.Render();
            var active = RenderTexture.active;
            RenderTexture.active = texture;
            var image = new Texture2D(width, height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            image.Apply();
            RenderTexture.active = active;
            camera.targetTexture = previous;
            for (int i = 0; i < canvases.Length; i++) canvases[i].renderMode = modes[i];
            File.WriteAllBytes(path, image.EncodeToPNG());
            Object.Destroy(image);
            texture.Release();
        }
    }
}
