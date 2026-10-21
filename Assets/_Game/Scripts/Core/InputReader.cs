using UnityEngine;
using UnityEngine.InputSystem;

namespace ThuyKieu.Core
{
    /// <summary>
    /// Single entry point for raw device input.
    /// Gameplay scripts ask this class instead of touching Keyboard/Mouse directly,
    /// so we can later swap to an InputActionAsset without rewriting gameplay code.
    /// </summary>
    public static class InputReader
    {
        /// <summary>WASD as a normalized-ish vector: x = strafe, y = forward.</summary>
        public static Vector2 Move
        {
            get
            {
                if (GamePause.IsPaused) return default; // menus own the keyboard while paused
                Keyboard keyboard = Keyboard.current;
                if (keyboard == null)
                {
                    return Vector2.ClampMagnitude(SimulatedMove, 1f);
                }

                Vector2 move = Vector2.zero;
                if (keyboard.wKey.isPressed) move.y += 1f;
                if (keyboard.sKey.isPressed) move.y -= 1f;
                if (keyboard.dKey.isPressed) move.x += 1f;
                if (keyboard.aKey.isPressed) move.x -= 1f;

                return Vector2.ClampMagnitude(move + SimulatedMove, 1f);
            }
        }

        /// <summary>Mouse delta for this frame.</summary>
        public static Vector2 Look
        {
            get
            {
                if (GamePause.IsPaused) return default; // menus own the keyboard while paused
                Mouse mouse = Mouse.current;
                return (mouse == null ? Vector2.zero : mouse.delta.ReadValue()) + SimulatedLook;
            }
        }

        /// <summary>Left Shift held.</summary>
        public static bool IsRunHeld
        {
            get
            {
                if (GamePause.IsPaused) return default; // menus own the keyboard while paused
                Keyboard keyboard = Keyboard.current;
                return keyboard != null && keyboard.leftShiftKey.isPressed;
            }
        }

        /// <summary>E pressed this frame.</summary>
        public static bool InteractPressedThisFrame
        {
            get
            {
                if (GamePause.IsPaused) return default; // menus own the keyboard while paused
                Keyboard keyboard = Keyboard.current;
                return keyboard != null && keyboard.eKey.wasPressedThisFrame || SimulatedInteract && _simulatedSince == Time.frameCount;
            }
        }

        /// <summary>E held (hold-to-act interactions: pickpocketing, pushing a door).</summary>
        public static bool IsInteractHeld
        {
            get
            {
                if (GamePause.IsPaused) return default; // menus own the keyboard while paused
                Keyboard keyboard = Keyboard.current;
                return keyboard != null && keyboard.eKey.isPressed || SimulatedInteract;
            }
        }

        /// <summary>QA autopilot only: WASD held by a test (x strafe, y forward). Never set in normal play.</summary>
        public static Vector2 SimulatedMove { get; set; }

        /// <summary>QA autopilot only: mouse movement (pixels this frame) added by a test. Never set in normal play.</summary>
        public static Vector2 SimulatedLook { get; set; }

        private static bool _simulatedInteract;
        private static int _simulatedSince;

        /// <summary>
        /// QA autopilot only: E held by a test (the hold interactions are then played through the real interaction path).
        /// Set from a coroutine, it counts as pressed on the next frame and held until cleared. Never set in normal play.
        /// </summary>
        public static bool SimulatedInteract
        {
            get => _simulatedInteract;
            set
            {
                if (value && !_simulatedInteract) _simulatedSince = Time.frameCount + 1;
                _simulatedInteract = value;
            }
        }

        /// <summary>Q pressed this frame: throw a pebble.</summary>
        public static bool ThrowPressedThisFrame
        {
            get
            {
                if (GamePause.IsPaused) return default; // menus own the keyboard while paused
                Keyboard keyboard = Keyboard.current;
                return keyboard != null && keyboard.qKey.wasPressedThisFrame || _simulatedThrow == Time.frameCount;
            }
        }

        private static int _simulatedThrow = -1;

        /// <summary>QA autopilot only: Q pressed by a test, on the next frame (the throw then goes through the real input path).</summary>
        public static void SimulateThrow() { _simulatedThrow = Time.frameCount + 1; }

        /// <summary>Any key pressed this frame (quick-time prompts).</summary>
        public static bool KeyPressedThisFrame(Key key)
        {
            Keyboard keyboard = Keyboard.current;
            return keyboard != null && keyboard[key].wasPressedThisFrame;
        }

        /// <summary>Left Ctrl or C held: crouch / sneak.</summary>
        public static bool IsCrouchHeld
        {
            get
            {
                if (GamePause.IsPaused) return default; // menus own the keyboard while paused
                Keyboard keyboard = Keyboard.current;
                return keyboard != null && (keyboard.leftCtrlKey.isPressed || keyboard.cKey.isPressed);
            }
        }

        /// <summary>Right mouse button held: focused observation.</summary>
        public static bool IsObserveHeld
        {
            get
            {
                if (GamePause.IsPaused) return default; // menus own the keyboard while paused
                Mouse mouse = Mouse.current;
                return mouse != null && mouse.rightButton.isPressed;
            }
        }

        /// <summary>J pressed this frame: open or close the journal.</summary>
        public static bool JournalPressedThisFrame
        {
            get
            {
                if (GamePause.IsPaused) return default; // menus own the keyboard while paused
                Keyboard keyboard = Keyboard.current;
                return keyboard != null && keyboard.jKey.wasPressedThisFrame;
            }
        }

        /// <summary>B pressed this frame: open or close the Trang Sách book.</summary>
        public static bool BookPressedThisFrame
        {
            get
            {
                if (GamePause.IsPaused) return default;
                Keyboard keyboard = Keyboard.current;
                return keyboard != null && keyboard.bKey.wasPressedThisFrame;
            }
        }

        /// <summary>Escape pressed this frame (used to release the cursor in the editor).</summary>
        public static bool CancelPressedThisFrame
        {
            get
            {
                Keyboard keyboard = Keyboard.current;
                return keyboard != null && keyboard.escapeKey.wasPressedThisFrame;
            }
        }
    }
}
