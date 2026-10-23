using UnityEngine;

namespace ThuyKieu.Core
{
    /// <summary>
    /// Global pause flag shared by the chapter pause menu (PauseMenuView) and the game shell
    /// (main menu, chapter select, dev console). Views that read raw keys check it before acting.
    /// </summary>
    public static class GamePause
    {
        /// <summary>Gameplay is frozen behind a menu.</summary>
        public static bool IsPaused { get; set; }

        /// <summary>The shell has an overlay open (chapter select, console); chapter menus must ignore keys.</summary>
        public static bool ShellOverlayOpen { get; set; }

        private static int _escapeFrame = -1;

        /// <summary>Marks this frame's Esc press as handled so a second menu does not react to it too.</summary>
        public static void ConsumeEscape() => _escapeFrame = Time.frameCount;

        public static bool EscapeConsumed => _escapeFrame == Time.frameCount;
    }
}
