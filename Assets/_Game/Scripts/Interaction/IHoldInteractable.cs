namespace ThuyKieu.Interaction
{
    /// <summary>
    /// An interaction that reacts to how E is used, not just to a press: a tap snaps a door open (loud), holding eases it
    /// open (quiet), holding lifts a latch or slips a key ring off a belt.
    /// </summary>
    public interface IHoldInteractable : IInteractable
    {
        /// <summary>
        /// Called every frame while the player aims at it. Returns the 0–1 progress to show on the reticle, or a negative
        /// value when there is nothing to show.
        /// </summary>
        float Tick(bool pressed, bool held);

        /// <summary>The player looked away or lost control mid-hold.</summary>
        void Lost();
    }
}
