namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Chapter 5's own sound: edited one-shots from recordings (the forest steps, landings, the falling bamboo, the bridge)
    /// and the chase music, built from the sources under Audio/Ch5Foley/Source and Audio/Ch5Music/Source (licences and
    /// edits in Audio/Ch5Foley/CREDITS.md). Nothing here changes what Chapters 3–4 hear.
    /// </summary>
    public static partial class Ch5Foley
    {
        public const string Root = "Assets/_Game/Audio/Ch5Foley";
        public const string SourceRoot = Root + "/Source";
        public const string MusicRoot = "Assets/_Game/Audio/Ch5Music";
        private const string TreeSource = SourceRoot + "/kheetor_treefall/chop-tree-fall.ogg";
        private const string MusicSource = MusicRoot + "/Source/Juggernaut.mp3";
    }
}
