using System.Runtime.CompilerServices;

// Scene builders and validation tools keep access to runtime internals without shipping Editor code in the player.
[assembly: InternalsVisibleTo("ThuyKieu.Game.Editor")]
