using UnityEngine;

namespace ThuyKieu.Gameplay.Chase
{
    /// <summary>
    /// The chase score as built from its recording (by the Ch5Foley music build): the loops from calm to climax, their
    /// levels in the game, and the tempo they were cut to. Played by <see cref="ChaseMusic"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "ThuyKieu/Chase Score")]
    public class ChaseScore : ScriptableObject
    {
        [Tooltip("Seamless loops, calm to climax, same tempo, each a whole number of bars.")]
        public AudioClip[] Levels = new AudioClip[0];
        [Tooltip("Level of each loop in the game.")]
        public float[] Gains = new float[0];
        public float Bpm = 120f;
        public int BeatsPerBar = 4;
        [TextArea] public string Credit;
    }
}
