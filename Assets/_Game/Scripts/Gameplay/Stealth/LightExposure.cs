using System.Collections.Generic;
using UnityEngine;

namespace ThuyKieu.Gameplay.Stealth
{
    /// <summary>
    /// How lit a point is by the scene's point and spot lights (0 = dark, 1 = in a lantern's glow).
    /// Night chapters enable it so guards notice the player much sooner in lamplight than in shadow.
    /// Walls block light (as they do on screen), so a lamp in the next room does not light the player.
    /// Lights can count for less (<see cref="ExposureWeight"/>, soft fills), and inside <see cref="OpenSky"/> areas the
    /// sun or moon adds its share unless a roof shades the point from it, so what looks lit on screen is what the guards treat as lit.
    /// </summary>
    public static class LightExposure
    {
        private const int Occluders = 1 << 8; // Environment layer
        /// <summary>How strongly a directional light of intensity 1 lights a point under open sky.</summary>
        private const float SkyShare = 0.35f;
        /// <summary>How far towards the sun or moon a roof can cast its shadow on an open yard.</summary>
        private const float SkyReach = 40f;
        private static readonly List<Light> Lights = new List<Light>();
        private static readonly List<float> Weights = new List<float>();
        private static readonly List<Light> Skylights = new List<Light>();
        private static readonly List<OpenSky> OpenAreas = new List<OpenSky>();

        public static bool Enabled { get; private set; }

        /// <summary>Collects the scene's local lights (call again if lights are added at runtime).</summary>
        public static void Enable(bool enabled)
        {
            Enabled = enabled;
            Lights.Clear();
            Weights.Clear();
            Skylights.Clear();
            OpenAreas.Clear();
            if (!enabled) return;
            foreach (var light in Object.FindObjectsByType<Light>())
            {
                if (light.type == LightType.Point || light.type == LightType.Spot)
                {
                    Lights.Add(light);
                    var weight = light.GetComponent<ExposureWeight>();
                    Weights.Add(weight != null ? weight.Weight : 1f);
                }
                else if (light.type == LightType.Directional) Skylights.Add(light);
            }
            OpenAreas.AddRange(Object.FindObjectsByType<OpenSky>());
        }

        public static float At(Vector3 point)
        {
            float exposure = 0.12f;
            for (int i = 0; i < Lights.Count; i++)
            {
                var light = Lights[i];
                if (light == null || !light.isActiveAndEnabled || light.range <= 0f) continue;
                float distance = Vector3.Distance(point, light.transform.position);
                if (distance >= light.range) continue;
                if (Physics.Linecast(light.transform.position, point, Occluders, QueryTriggerInteraction.Ignore)) continue;
                float falloff = 1f - distance / light.range;
                exposure += falloff * falloff * Mathf.Min(1.5f, light.intensity) * 0.9f * Weights[i];
            }
            if (OpenAreas.Count > 0 && UnderOpenSky(point))
                foreach (var sky in Skylights)
                    if (sky != null && sky.isActiveAndEnabled && !Physics.Raycast(point, -sky.transform.forward, SkyReach, Occluders, QueryTriggerInteraction.Ignore))
                        exposure += Mathf.Min(1.5f, sky.intensity) * SkyShare;
            return Mathf.Clamp01(exposure);
        }

        private static bool UnderOpenSky(Vector3 point)
        {
            foreach (var area in OpenAreas) if (area != null && area.Contains(point)) return true;
            return false;
        }
    }
}
