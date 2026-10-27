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
        private static bool _isPaused;
        private static double _pausedAt, _pausedSeconds;

        public static bool IsPaused
        {
            get => _isPaused;
            set
            {
                if (_isPaused == value) return;
                if (value) _pausedAt = Time.unscaledTimeAsDouble;
                else _pausedSeconds += Time.unscaledTimeAsDouble - _pausedAt;
                _isPaused = value;
            }
        }

        /// <summary>Real gameplay time: unaffected by slow motion, frozen while menus pause the game.</summary>
        public static double UnpausedTime => (_isPaused ? _pausedAt : Time.unscaledTimeAsDouble) - _pausedSeconds;

        /// <summary>The shell has an overlay open (chapter select, console); chapter menus must ignore keys.</summary>
        public static bool ShellOverlayOpen { get; set; }

        private static int _escapeFrame = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession()
        {
            _isPaused = false;
            _pausedAt = _pausedSeconds = 0;
            ShellOverlayOpen = false;
            _escapeFrame = -1;
        }

        /// <summary>Marks this frame's Esc press as handled so a second menu does not react to it too.</summary>
        public static void ConsumeEscape() => _escapeFrame = Time.frameCount;

        public static bool EscapeConsumed => _escapeFrame == Time.frameCount;
    }
}
