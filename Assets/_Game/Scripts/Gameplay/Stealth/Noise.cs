using System;
using UnityEngine;

namespace ThuyKieu.Gameplay.Stealth
{
    /// <summary>What made a sound. Listeners react to loud sounds more than to soft ones, not to the kind itself.</summary>
    public enum NoiseKind { Footstep, Door, Lock, Object, Thrown, Voice, Silver }

    /// <summary>
    /// Sounds the player (or a thrown pebble) makes, broadcast to everyone listening: a radius in metres, already scaled by
    /// gait, surface and any <see cref="NoiseZone"/> masking (rain on the yard, music near the guests' room).
    /// Listeners decide for themselves whether a wall between them and the source muffles it.
    /// </summary>
    public static class Noise
    {
        public struct Event
        {
            public Vector3 Position;
            public float Radius;
            public NoiseKind Kind;
            public float Time;
        }

        /// <summary>Radius at which a sound counts as loud: listeners walk over to check instead of only turning their heads.</summary>
        public const float Loud = 6f;
        /// <summary>A wall or a closed door between source and listener shortens how far a sound carries.</summary>
        public const float Muffled = 0.6f;

        public static event Action<Event> Emitted;
        public static Event Last { get; private set; }

        public static void Emit(Vector3 position, float radius, NoiseKind kind)
        {
            radius *= NoiseZone.ScaleAt(position);
            if (radius <= 0.05f) return;
            var noise = new Event { Position = position, Radius = radius, Kind = kind, Time = UnityEngine.Time.time };
            Last = noise;
            Emitted?.Invoke(noise);
        }

        /// <summary>
        /// Whether a listener at <paramref name="ear"/> hears the event, with walls muffling it. <paramref name="attention"/>
        /// scales how far it carries for this listener (someone already searching listens harder than someone on his round).
        /// </summary>
        public static bool Hears(Event noise, Vector3 ear, LayerMask walls, float attention = 1f)
        {
            float radius = noise.Radius * attention;
            float distance = Vector3.Distance(noise.Position, ear);
            if (distance > radius) return false;
            Vector3 from = noise.Position + Vector3.up * 0.4f;
            bool blocked = Physics.Linecast(from, ear, walls, QueryTriggerInteraction.Ignore);
            return !blocked || distance <= radius * Muffled;
        }
    }
}
