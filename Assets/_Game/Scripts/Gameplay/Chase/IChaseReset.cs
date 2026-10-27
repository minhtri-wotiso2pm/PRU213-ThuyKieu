namespace ThuyKieu.Gameplay.Chase
{
    /// <summary>Something on the bamboo chase course that changes during a run (a culm that falls, the bridge) and is put back for the next attempt.</summary>
    public interface IChaseReset
    {
        /// <summary>Back to how it stood before the run, armed for it.</summary>
        void ResetForChase();
        /// <summary>Whether it may react to Kiều (false outside the chase).</summary>
        bool Armed { get; set; }
    }
}
