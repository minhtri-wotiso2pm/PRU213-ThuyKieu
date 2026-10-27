namespace ThuyKieu.Gameplay.Chapters.LauTuBa
{
    /// <summary>
    /// When the second scare (Tú Bà bursting out of her room) may start: only after the first one is fully over and the
    /// player has had a minute to breathe, and only where the player can see it and has somewhere to go. No timer forces it;
    /// every way back to the kitchen crosses the corridor; a player who reaches the kitchen before the rest is over hears it
    /// from there (Chapter04LauWorld treats the kitchen as in the zone). The back door is never held shut to wait for it.
    /// </summary>
    public static class ScareRules
    {
        public const float RestAfterFirst = 60f;
        public const float MinDistance = 3.5f;
        public const float MaxDistance = 16f;

        public static bool SecondAllowed(bool firstDone, float secondsSinceFirst, bool hidden, bool chased, bool inZone, float distanceToDoor, bool inHerRoom)
        {
            if (!firstDone || secondsSinceFirst < RestAfterFirst) return false;
            if (hidden || chased || !inZone || inHerRoom) return false;
            return distanceToDoor >= MinDistance && distanceToDoor <= MaxDistance;
        }
    }
}
