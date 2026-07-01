using UnityEngine;
#if ENABLE_INPUT_SYSTEM && TH_INPUT_SYSTEM_PACKAGE
using UnityEngine.InputSystem;
#endif

namespace MeshFreeHandles
{
    /// <summary>
    /// Input abstraction that works with both the legacy Input Manager and the
    /// new Input System package, depending on the project's active input handling.
    /// Uses the new Input System when it is installed and enabled, otherwise
    /// falls back to the legacy Input class.
    /// </summary>
    public static class HandleInput
    {
#if ENABLE_INPUT_SYSTEM && TH_INPUT_SYSTEM_PACKAGE
        /// <summary>Current mouse position in screen space.</summary>
        public static Vector2 MousePosition
        {
            get
            {
                var mouse = Mouse.current;
                return mouse != null ? mouse.position.ReadValue() : Vector2.zero;
            }
        }

        /// <summary>True in the frame the left mouse button was pressed.</summary>
        public static bool LeftMousePressedThisFrame
            => Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        /// <summary>True in the frame the left mouse button was released.</summary>
        public static bool LeftMouseReleasedThisFrame
            => Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;

        /// <summary>True while the right mouse button is held.</summary>
        public static bool RightMouseHeld
            => Mouse.current != null && Mouse.current.rightButton.isPressed;

        /// <summary>Scroll delta, normalized to roughly one unit per notch.</summary>
        public static Vector2 MouseScrollDelta
        {
            get
            {
                var mouse = Mouse.current;
                if (mouse == null) return Vector2.zero;

                Vector2 scroll = mouse.scroll.ReadValue();
                // The new Input System reports scroll in ~±120 steps per notch
                // on some platforms; normalize to match legacy mouseScrollDelta.
                if (Mathf.Abs(scroll.y) > 10f || Mathf.Abs(scroll.x) > 10f)
                    scroll /= 120f;
                return scroll;
            }
        }

        /// <summary>True in the frame the given key was pressed.</summary>
        public static bool GetKeyDown(KeyCode keyCode)
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;

            Key key = ToKey(keyCode);
            if (key == Key.None) return false;

            return keyboard[key].wasPressedThisFrame;
        }

        /// <summary>True while the given key is held.</summary>
        public static bool GetKey(KeyCode keyCode)
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;

            Key key = ToKey(keyCode);
            if (key == Key.None) return false;

            return keyboard[key].isPressed;
        }

        // Cached KeyCode -> Key mapping to avoid repeated enum parsing
        private static readonly System.Collections.Generic.Dictionary<KeyCode, Key> keyCache
            = new System.Collections.Generic.Dictionary<KeyCode, Key>();

        private static Key ToKey(KeyCode keyCode)
        {
            if (keyCache.TryGetValue(keyCode, out Key cached))
                return cached;

            Key key = MapKey(keyCode);
            keyCache[keyCode] = key;
            return key;
        }

        private static Key MapKey(KeyCode keyCode)
        {
            switch (keyCode)
            {
                // Names that differ between KeyCode and Key
                case KeyCode.Alpha0: return Key.Digit0;
                case KeyCode.Alpha1: return Key.Digit1;
                case KeyCode.Alpha2: return Key.Digit2;
                case KeyCode.Alpha3: return Key.Digit3;
                case KeyCode.Alpha4: return Key.Digit4;
                case KeyCode.Alpha5: return Key.Digit5;
                case KeyCode.Alpha6: return Key.Digit6;
                case KeyCode.Alpha7: return Key.Digit7;
                case KeyCode.Alpha8: return Key.Digit8;
                case KeyCode.Alpha9: return Key.Digit9;
                case KeyCode.Keypad0: return Key.Numpad0;
                case KeyCode.Keypad1: return Key.Numpad1;
                case KeyCode.Keypad2: return Key.Numpad2;
                case KeyCode.Keypad3: return Key.Numpad3;
                case KeyCode.Keypad4: return Key.Numpad4;
                case KeyCode.Keypad5: return Key.Numpad5;
                case KeyCode.Keypad6: return Key.Numpad6;
                case KeyCode.Keypad7: return Key.Numpad7;
                case KeyCode.Keypad8: return Key.Numpad8;
                case KeyCode.Keypad9: return Key.Numpad9;
                case KeyCode.Return: return Key.Enter;
                case KeyCode.KeypadEnter: return Key.NumpadEnter;
                case KeyCode.KeypadPlus: return Key.NumpadPlus;
                case KeyCode.KeypadMinus: return Key.NumpadMinus;
                case KeyCode.KeypadMultiply: return Key.NumpadMultiply;
                case KeyCode.KeypadDivide: return Key.NumpadDivide;
                case KeyCode.KeypadPeriod: return Key.NumpadPeriod;
                case KeyCode.CapsLock: return Key.CapsLock;
                case KeyCode.LeftControl: return Key.LeftCtrl;
                case KeyCode.RightControl: return Key.RightCtrl;
                default:
                    // Most names match 1:1 (letters, F-keys, arrows, Space, Tab,
                    // LeftShift, LeftAlt, Escape, ...)
                    return System.Enum.TryParse(keyCode.ToString(), out Key parsed)
                        ? parsed
                        : Key.None;
            }
        }
#else
        /// <summary>Current mouse position in screen space.</summary>
        public static Vector2 MousePosition => Input.mousePosition;

        /// <summary>True in the frame the left mouse button was pressed.</summary>
        public static bool LeftMousePressedThisFrame => Input.GetMouseButtonDown(0);

        /// <summary>True in the frame the left mouse button was released.</summary>
        public static bool LeftMouseReleasedThisFrame => Input.GetMouseButtonUp(0);

        /// <summary>True while the right mouse button is held.</summary>
        public static bool RightMouseHeld => Input.GetMouseButton(1);

        /// <summary>Scroll delta, roughly one unit per notch.</summary>
        public static Vector2 MouseScrollDelta => Input.mouseScrollDelta;

        /// <summary>True in the frame the given key was pressed.</summary>
        public static bool GetKeyDown(KeyCode keyCode) => Input.GetKeyDown(keyCode);

        /// <summary>True while the given key is held.</summary>
        public static bool GetKey(KeyCode keyCode) => Input.GetKey(keyCode);
#endif
    }
}
