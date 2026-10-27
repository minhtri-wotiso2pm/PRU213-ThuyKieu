using UnityEngine;

namespace ThuyKieu.Gameplay.Stealth
{
    /// <summary>
    /// How fast a watcher who can see Kiều becomes sure of her, per second (1 = certain).
    /// Every modifier is a starting value for tuning, combined by one rule so they can never stack into invisibility:
    ///   rate = distance factor × clamp(posture × light × motion × focus, 0.25, 1.6) / <see cref="SecondsWhenClose"/>
    /// Standing close in front of someone (within <see cref="CloseRange"/>, in the central cone) is never safe:
    /// the combined factor is raised to at least <see cref="CloseFloor"/>, so she is noticed in about a second. A watcher may
    /// widen that to his side vision close by (<see cref="Input.NearRange"/>, <see cref="Input.NearFloor"/>): then someone
    /// standing beside him in plain view is not invisible either. The peripheral cone's width and reach can be set per
    /// watcher too; left at zero, every value is the shared default.
    /// Light only changes speed and range; line of sight is always tested separately by the caller.
    /// </summary>
    public static class SightModel
    {
        public const float SecondsWhenClose = 1f;
        public const float CloseRange = 2.2f;
        public const float CloseFloor = 0.9f;
        public const float MinCombined = 0.25f;
        public const float MaxCombined = 1.6f;
        /// <summary>In darkness a watcher only makes out shapes this close.</summary>
        public const float DarkRange = 5f;
        public const float PeripheralHalfAngle = 70f;
        public const float PeripheralRange = 4f;
        /// <summary>How far past the side cone the near field reaches (degrees), so "right beside him" is never a blind seam.</summary>
        public const float NearSideMargin = 10f;

        public struct Input
        {
            public float Distance;
            /// <summary>Angle between the watcher's facing and the line to her, in degrees.</summary>
            public float Angle;
            /// <summary>Half of the central cone, in degrees.</summary>
            public float HalfAngle;
            public float Range;
            /// <summary>0 = dark … 1 = in a lantern's glow (<see cref="LightExposure"/>).</summary>
            public float Light;
            public bool Crouched;
            public float Speed;
            public bool Running;
            /// <summary>Half of the side-vision cone, degrees (0: <see cref="PeripheralHalfAngle"/>).</summary>
            public float PeripheralHalf;
            /// <summary>How far side vision reaches, metres (0: <see cref="PeripheralRange"/>).</summary>
            public float PeripheralReach;
            /// <summary>Within this distance the floor holds in side vision too (0: only in the central cone, within <see cref="CloseRange"/>).</summary>
            public float NearRange;
            /// <summary>The floor's value (0: <see cref="CloseFloor"/>).</summary>
            public float NearFloor;
            /// <summary>Distance along the floor (eye height and crouching aside), for <see cref="NearRange"/>; 0: <see cref="Distance"/>.</summary>
            public float FlatDistance;
        }

        /// <summary>How far the watcher can make her out at this light level.</summary>
        public static float EffectiveRange(float range, float light) =>
            Mathf.Lerp(Mathf.Min(range, DarkRange), range, Mathf.InverseLerp(0.2f, 0.55f, light));

        /// <summary>Awareness gained per second while she is in sight (0 when outside the range or the cones).</summary>
        public static float Rate(Input input)
        {
            float range = EffectiveRange(input.Range, input.Light);
            if (input.Distance > range) return 0f;
            bool central = input.Angle <= input.HalfAngle;
            float sideHalf = input.PeripheralHalf > 0f ? input.PeripheralHalf : PeripheralHalfAngle;
            float sideReach = input.PeripheralReach > 0f ? input.PeripheralReach : PeripheralRange;
            float flat = input.FlatDistance > 0f ? input.FlatDistance : input.Distance;
            // Close by, someone right beside him (on his shoulder line, a little past it) is in plain view however his eyes
            // sit in his head: the near field reaches NearSideMargin degrees past the side cone. Further behind he is blind.
            bool near = input.NearRange > 0f && flat <= input.NearRange;
            bool peripheral = !central && (input.Angle <= sideHalf || near && input.Angle <= sideHalf + NearSideMargin) && input.Distance <= sideReach;
            if (!central && !peripheral) return 0f;

            float distanceFactor = input.Distance <= 2f ? 1f : Mathf.Lerp(1f, 0.35f, (input.Distance - 2f) / Mathf.Max(0.01f, range - 2f));
            float posture = input.Crouched ? 0.55f : 1f;
            float light = Mathf.Lerp(0.5f, 1.25f, Mathf.Clamp01(input.Light));
            float motion = input.Speed < 0.1f ? 0.6f : input.Running ? 1.4f : 1f;
            float focus = central ? 1f : 0.5f;
            float combined = Mathf.Clamp(posture * light * motion * focus, MinCombined, MaxCombined);
            float floor = input.NearFloor > 0f ? input.NearFloor : CloseFloor;
            if (central && input.Distance <= CloseRange) combined = Mathf.Max(combined, floor);
            else if (near) combined = Mathf.Max(combined, floor);
            return distanceFactor * combined / SecondsWhenClose;
        }
    }
}
