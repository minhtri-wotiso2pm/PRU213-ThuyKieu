using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Chase;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ThuyKieu.Gameplay.Sequences
{
    // QA for how the forest looks from her eyes, in Play Mode with the chapter's own exposure, grade and fog
    // ("-autopilotLook"). Nothing is saved as a picture: each view is rendered off screen and only numbers are reported.
    //   open      share of the frame where nothing at all is drawn (sky through the canopy, or the end of the world)
    //   horizon   the same in the band around the horizon (a level view): what would show an empty map edge
    //   luma      display brightness after grading (0–1): mean, 5th / 50th / 95th percentile, share crushed below 0.03
    //   obstacle  pixels it covers and how much it changes them, from 18 m and 10 m (how early it reads in the dark)
    public partial class BambooChaseSequence
    {
        private const int LookWidth = 640, LookHeight = 360;

        private struct Shot
        {
            public float Open, Horizon, Mean, P5, P50, P95, Dark, Raw, Way;
            public Color32[] Pixels;
        }

        private IEnumerator Look()
        {
            var main = _director.CameraController.GetComponent<Camera>();
            var probe = new GameObject("LookProbe").AddComponent<Camera>();
            probe.CopyFrom(main);
            probe.enabled = false;
            probe.fieldOfView = _fov;
            var mainData = main.GetUniversalAdditionalCameraData();
            var data = probe.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = mainData.renderPostProcessing;
            data.renderShadows = mainData.renderShadows;
            data.volumeLayerMask = mainData.volumeLayerMask;
            data.antialiasing = AntialiasingMode.None;
            var target = new RenderTexture(LookWidth, LookHeight, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var read = new Texture2D(LookWidth, LookHeight, TextureFormat.RGBA32, false);
            for (int i = 0; i < 5; i++) yield return null;

            // The player's own camera (her eyes at the start), read back in memory only: the check that the off-screen views
            // below are graded the same way.
            var seen = Grab(main, target, read).Select(Luma).OrderBy(l => l).ToArray();
            StoryAutopilot.Note($"LOOK player camera luma {seen.Average():F3} (p5 {seen[seen.Length * 5 / 100]:F3} p50 {seen[seen.Length / 2]:F3} p95 {seen[seen.Length * 95 / 100]:F3}) · crushed {seen.Count(l => l < 0.03f) / (float)seen.Length:P0}");
            probe.transform.SetPositionAndRotation(main.transform.position, main.transform.rotation);
            var same = Render(probe, data, target, read, false);
            StoryAutopilot.Note($"LOOK probe from the same eyes luma {same.Mean:F3} (p5 {same.P5:F3} p50 {same.P50:F3} p95 {same.P95:F3}) · crushed {same.Dark:P0}");
            // What each part of the grade does to that view (each switched off alone).
            foreach (var volume in FindObjectsByType<UnityEngine.Rendering.Volume>())
            {
                var profile = volume.HasInstantiatedProfile() ? volume.profile : volume.sharedProfile;
                if (profile == null) continue;
                foreach (var component in profile.components)
                {
                    if (!component.active) continue;
                    component.active = false;
                    var without = Render(probe, data, target, read, false);
                    component.active = true;
                    StoryAutopilot.Note($"LOOK grade without {volume.name}/{component.GetType().Name}: luma {without.Mean:F3} (p50 {without.P50:F3} p95 {without.P95:F3})");
                }
            }

            float start = _course.Progress(_course.StartPoint.position);
            var stations = new List<(string name, int way, float s)> { ("start", 1, start + 2f) };
            var obstacles = _course.Obstacles;
            for (int i = 0; i < obstacles.Length; i++)
            {
                var o = obstacles[i];
                int way = o.Way == 2 ? 2 : 1;
                float s = _course.Project(way, o.Position, out _);
                if (o.Kind == ChaseCourse.ObstacleKind.Bridge) { stations.Add(("bridge-30", 1, s - 30f)); stations.Add(("bridge-6", 1, s - o.Length * 0.5f - 6f)); }
                else stations.Add((o.Kind + "#" + i + (way == 2 ? " thick" : "") + "-14", way, s - 14f));
            }
            var poses = new (string name, float eye, float yaw, float pitch)[]
            {
                ("stand", 1.56f, 0f, 0f), ("stand L", 1.56f, -90f, 0f), ("stand R", 1.56f, 90f, 0f), ("stand back", 1.56f, 180f, 0f),
                ("crouch", 0.98f, 0f, 0f), ("apex", 2.66f, 0f, 0f), ("apex L", 2.66f, -90f, 0f), ("apex R", 2.66f, 90f, 0f), ("apex up", 2.66f, 0f, -55f)
            };
            float worstHorizon = 0f, skyUp = 0f;
            string worstAt = "";
            int ups = 0;
            foreach (var (name, way, s) in stations)
            {
                Vector3 at = _course.PointAt(way, s), dir = _course.DirectionAt(way, s);
                foreach (var pose in poses)
                {
                    var rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(pose.pitch, pose.yaw, 0f);
                    probe.transform.SetPositionAndRotation(at + Vector3.up * pose.eye, rotation);
                    var shot = Render(probe, data, target, read, true);
                    StoryAutopilot.Note($"LOOK {name,-22} {pose.name,-10} open {shot.Open:P1} · horizon {shot.Horizon:P1} · luma {shot.Mean:F3} (p5 {shot.P5:F3} p50 {shot.P50:F3} p95 {shot.P95:F3}) · crushed {shot.Dark:P0} · way ahead {shot.Way:F3} · ungraded {shot.Raw:F3}");
                    if (pose.pitch == 0f && shot.Horizon > worstHorizon) { worstHorizon = shot.Horizon; worstAt = name + " " + pose.name; }
                    if (pose.name == "apex up") { skyUp += shot.Open; ups++; }
                }
                yield return null;
            }

            // Each obstacle from 18 m and 10 m, standing, with and without it: how much of the frame it is and how strongly it shows.
            var parts = _course.transform.Cast<Transform>().Where(t => t.name.StartsWith("Log_") || t.name.StartsWith("Low_") || t.name.StartsWith("Squeeze_")).ToList();
            for (int i = 0; i < obstacles.Length; i++)
            {
                var o = obstacles[i];
                if (o.Kind != ChaseCourse.ObstacleKind.Jump && o.Kind != ChaseCourse.ObstacleKind.Duck && o.Kind != ChaseCourse.ObstacleKind.Squeeze) continue;
                var part = parts.OrderBy(t => (Centre(t) - o.Position).sqrMagnitude).FirstOrDefault();
                if (part == null) continue;
                var renderers = part.GetComponentsInChildren<Renderer>();
                int way = o.Way == 2 ? 2 : 1;
                float s = _course.Project(way, o.Position, out _);
                foreach (float before in new[] { 18f, 10f })
                {
                    Vector3 eye = _course.PointAt(way, s - before) + Vector3.up * 1.56f;
                    probe.transform.SetPositionAndRotation(eye, Quaternion.LookRotation(o.Position + Vector3.up * 0.5f - eye));
                    var with = Render(probe, data, target, read, false);
                    foreach (var r in renderers) r.enabled = false;
                    yield return null; // the GPU-resident renderers take a frame to drop it
                    var without = Render(probe, data, target, read, false);
                    foreach (var r in renderers) r.enabled = true;
                    yield return null;
                    int covered = 0;
                    float change = 0f, signed = 0f, largest = 0f;
                    for (int p = 0; p < with.Pixels.Length; p++)
                    {
                        float d = Luma(with.Pixels[p]) - Luma(without.Pixels[p]);
                        largest = Mathf.Max(largest, Mathf.Abs(d));
                        if (Mathf.Abs(d) < 3f / 255f) continue;
                        covered++;
                        change += Mathf.Abs(d);
                        signed += d;
                    }
                    // Brightness where it stands on screen (its bounds), with and without it.
                    var bounds = renderers.Length > 0 ? renderers[0].bounds : new Bounds(o.Position, Vector3.one);
                    foreach (var r in renderers) bounds.Encapsulate(r.bounds);
                    var rect = ScreenRect(probe, bounds);
                    StoryAutopilot.Note($"LOOK obstacle {part.name,-12} from {before:F0} m · covers {covered} px ({covered / (float)with.Pixels.Length:P2}) · mean change {(covered > 0 ? change / covered : 0f):F3} ({(signed < 0f ? "darker" : "lighter")} than behind it) · largest {largest:F3} · in its box {MeanIn(with.Pixels, rect):F3} vs {MeanIn(without.Pixels, rect):F3} without · box {rect.width:F0}x{rect.height:F0} px · {renderers.Length} renderers");
                }
                yield return null;
            }
            StoryAutopilot.Note($"LOOK summary · worst level-view horizon open {worstHorizon:P1} at {worstAt} · sky seen looking up from the top of a jump {(ups > 0 ? skyUp / ups : 0f):P1} on average · stations {stations.Count}");
            target.Release();
            Destroy(target);
            Destroy(read);
            Destroy(probe.gameObject);
        }

        private static float Luma(Color32 c) => (0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b) / 255f;

        /// <summary>Where an obstacle object is (its renderers' middle: the squeeze's root stays at the origin).</summary>
        private static Vector3 Centre(Transform part)
        {
            var renderers = part.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return part.position;
            var bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            return bounds.center;
        }

        /// <summary>The pixel rectangle the box covers in the probe's view (clamped to the frame).</summary>
        private static Rect ScreenRect(Camera camera, Bounds b)
        {
            float x0 = LookWidth, y0 = LookHeight, x1 = 0f, y1 = 0f;
            for (int i = 0; i < 8; i++)
            {
                var corner = new Vector3((i & 1) == 0 ? b.min.x : b.max.x, (i & 2) == 0 ? b.min.y : b.max.y, (i & 4) == 0 ? b.min.z : b.max.z);
                Vector3 v = camera.WorldToViewportPoint(corner);
                if (v.z <= 0f) continue;
                x0 = Mathf.Min(x0, v.x * LookWidth); x1 = Mathf.Max(x1, v.x * LookWidth);
                y0 = Mathf.Min(y0, v.y * LookHeight); y1 = Mathf.Max(y1, v.y * LookHeight);
            }
            x0 = Mathf.Clamp(x0, 0f, LookWidth); x1 = Mathf.Clamp(x1, 0f, LookWidth);
            y0 = Mathf.Clamp(y0, 0f, LookHeight); y1 = Mathf.Clamp(y1, 0f, LookHeight);
            return x1 > x0 && y1 > y0 ? Rect.MinMaxRect(x0, y0, x1, y1) : new Rect(0f, 0f, 0f, 0f);
        }

        private static float MeanIn(Color32[] pixels, Rect rect)
        {
            float sum = 0f;
            int count = 0;
            for (int y = (int)rect.yMin; y < (int)rect.yMax; y++)
                for (int x = (int)rect.xMin; x < (int)rect.xMax; x++) { sum += Luma(pixels[y * LookWidth + x]); count++; }
            return count > 0 ? sum / count : 0f;
        }

        /// <summary>One view: graded as in play, then again with no sky, no fog and no grading on magenta to find where nothing is drawn.</summary>
        private static Shot Render(Camera probe, UniversalAdditionalCameraData data, RenderTexture target, Texture2D read, bool emptyPass)
        {
            var shot = new Shot { Pixels = Grab(probe, target, read) };
            var lumas = shot.Pixels.Select(Luma).OrderBy(l => l).ToArray();
            shot.Mean = lumas.Average();
            shot.P5 = lumas[lumas.Length * 5 / 100];
            shot.P50 = lumas[lumas.Length / 2];
            shot.P95 = lumas[lumas.Length * 95 / 100];
            shot.Dark = lumas.Count(l => l < 0.03f) / (float)lumas.Length;
            // The way ahead: the middle of the lower part of a level view.
            shot.Way = MeanIn(shot.Pixels, Rect.MinMaxRect(LookWidth * 0.35f, LookHeight * 0.1f, LookWidth * 0.65f, LookHeight * 0.4f));
            if (!emptyPass) return shot;
            var clear = probe.clearFlags;
            var background = probe.backgroundColor;
            bool post = data.renderPostProcessing, fog = RenderSettings.fog;
            probe.clearFlags = CameraClearFlags.SolidColor;
            probe.backgroundColor = Color.magenta;
            data.renderPostProcessing = false;
            RenderSettings.fog = false;
            var empty = Grab(probe, target, read);
            probe.clearFlags = clear;
            probe.backgroundColor = background;
            data.renderPostProcessing = post;
            RenderSettings.fog = fog;
            int open = 0, band = 0, bandOpen = 0;
            float raw = 0f;
            for (int p = 0; p < empty.Length; p++)
            {
                bool none = empty[p].r > 235 && empty[p].g < 20 && empty[p].b > 235;
                if (none) open++;
                else raw += Luma(empty[p]);
                int row = p / LookWidth;
                if (row < LookHeight * 0.45f || row > LookHeight * 0.55f) continue;
                band++;
                if (none) bandOpen++;
            }
            shot.Open = open / (float)empty.Length;
            shot.Horizon = band > 0 ? bandOpen / (float)band : 0f;
            shot.Raw = open < empty.Length ? raw / (empty.Length - open) : 0f;
            return shot;
        }

        private static Color32[] Grab(Camera probe, RenderTexture target, Texture2D read)
        {
            probe.targetTexture = target;
            probe.Render();
            probe.targetTexture = null;
            var previous = RenderTexture.active;
            RenderTexture.active = target;
            read.ReadPixels(new Rect(0, 0, LookWidth, LookHeight), 0, 0, false);
            read.Apply(false);
            RenderTexture.active = previous;
            return read.GetPixels32();
        }
    }
}
