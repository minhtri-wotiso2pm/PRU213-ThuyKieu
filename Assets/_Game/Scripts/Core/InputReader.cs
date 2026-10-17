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
                    return Vector2.zero;
                }

                Vector2 move = Vector2.zero;
                if (keyboard.wKey.isPressed) move.y += 1f;
                if (keyboard.sKey.isPressed) move.y -= 1f;
                if (keyboard.dKey.isPressed) move.x += 1f;
                if (keyboard.aKey.isPressed) move.x -= 1f;

                return Vector2.ClampMagnitude(move, 1f);
            }
        }

        /// <summary>Mouse delta for this frame.</summary>
        public static Vector2 Look
        {
            get
            {
                if (GamePause.IsPaused) return default; // menus own the keyboard while paused
                Mouse mouse = Mouse.current;
                return mouse == null ? Vector2.zero : mouse.delta.ReadValue();
            }
        }

        /// <summary>Mouse-wheel movement, normalized so one physical notch is approximately one step.</summary>
        public static float Zoom
        {
            get
            {
                if (GamePause.IsPaused) return 0f;
                Mouse mouse = Mouse.current;
                if (mouse == null) return 0f;
                return Mathf.Clamp(mouse.scroll.ReadValue().y / 120f, -1f, 1f);
            }
        }

        /// <summary>V toggles the close front-facing character portrait camera.</summary>
        public static bool PortraitPressedThisFrame
        {
            get
            {
                if (GamePause.IsPaused) return false;
                Keyboard keyboard = Keyboard.current;
                return keyboard != null && keyboard.vKey.wasPressedThisFrame;
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
                return keyboard != null && keyboard.eKey.wasPressedThisFrame;
            }
        }

        /// <summary>E held (hold-to-act interactions: pickpocketing, pushing a door).</summary>
        public static bool IsInteractHeld
        {
            get
            {
                if (GamePause.IsPaused) return default; // menus own the keyboard while paused
                Keyboard keyboard = Keyboard.current;
                return keyboard != null && keyboard.eKey.isPressed;
            }
        }

        /// <summary>Q pressed this frame: throw a pebble.</summary>
        public static bool ThrowPressedThisFrame
        {
            get
            {
                if (GamePause.IsPaused) return default; // menus own the keyboard while paused
                Keyboard keyboard = Keyboard.current;
                return keyboard != null && keyboard.qKey.wasPressedThisFrame;
            }
        }

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
