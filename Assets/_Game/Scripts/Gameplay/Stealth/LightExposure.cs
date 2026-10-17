using System.Collections.Generic;
using UnityEngine;

namespace ThuyKieu.Gameplay.Stealth
{
    /// <summary>
    /// How lit a point is by the scene's point and spot lights (0 = dark, 1 = in a lantern's glow).
    /// Night chapters enable it so guards notice the player much sooner in lamplight than in shadow.
    /// Walls block light (as they do on screen), so a lamp in the next room does not light the player.
    /// </summary>
    public static class LightExposure
    {
        private const int Occluders = 1 << 8; // Environment layer
        private static readonly List<Light> Lights = new List<Light>();

        public static bool Enabled { get; private set; }

        /// <summary>Collects the scene's local lights (call again if lights are added at runtime).</summary>
        public static void Enable(bool enabled)
        {
            Enabled = enabled;
            Lights.Clear();
            if (!enabled) return;
            foreach (var light in Object.FindObjectsByType<Light>())
                if (light.type == LightType.Point || light.type == LightType.Spot) Lights.Add(light);
        }

        public static float At(Vector3 point)
        {
            float exposure = 0.12f;
            foreach (var light in Lights)
            {
                if (light == null || !light.isActiveAndEnabled || light.range <= 0f) continue;
                float distance = Vector3.Distance(point, light.transform.position);
                if (distance >= light.range) continue;
                if (Physics.Linecast(light.transform.position, point, Occluders, QueryTriggerInteraction.Ignore)) continue;
                float falloff = 1f - distance / light.range;
                exposure += falloff * falloff * Mathf.Min(1.5f, light.intensity) * 0.9f;
            }
            return Mathf.Clamp01(exposure);
        }
    }
}
