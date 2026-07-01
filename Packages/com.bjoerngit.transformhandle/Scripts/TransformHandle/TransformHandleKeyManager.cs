using UnityEngine;
using UnityEngine.Events;

namespace MeshFreeHandles
{
    /// <summary>
    /// Dispatches key events for setting translation/rotation mode and toggling Local/Global space,
    /// using Editor-style shortcuts (W, E, R, X).
    /// Disabled when right mouse button is pressed (camera navigation mode).
    /// </summary>
    public class TransformHandleKeyManager : MonoBehaviour
    {
        [Header("Mode Keys")]
        [Tooltip("Key to set Translation mode (move tool).")]
        public KeyCode translationKey = KeyCode.W;
       
        [Tooltip("Invoked when translationKey is pressed.")]
        public UnityEvent onSetTranslation;
       
        [Tooltip("Key to set Rotation mode (rotate tool).")]
        public KeyCode rotationKey = KeyCode.E;
       
        [Tooltip("Invoked when rotationKey is pressed.")]
        public UnityEvent onSetRotation;
       
        [Tooltip("Key to set Scale mode (scale tool).")]
        public KeyCode scaleKey = KeyCode.R;
       
        [Tooltip("Invoked when scaleKey is pressed.")]
        public UnityEvent onSetScale;
       
        [Header("Space Key")]
        [Tooltip("Key to toggle between Local and Global axes.")]
        public KeyCode handleSpaceToggleKey = KeyCode.X;
       
        [Tooltip("Invoked when handleSpaceToggleKey is pressed.")]
        public UnityEvent onToggleHandleSpace;
       
        void Update()
        {
            // Skip if right mouse button is pressed (camera navigation mode)
            if (HandleInput.RightMouseHeld) return;

            // Check for key presses
            if (HandleInput.GetKeyDown(translationKey))
                onSetTranslation?.Invoke();

            if (HandleInput.GetKeyDown(rotationKey))
                onSetRotation?.Invoke();

            if (HandleInput.GetKeyDown(scaleKey))
                onSetScale?.Invoke();

            if (HandleInput.GetKeyDown(handleSpaceToggleKey))
                onToggleHandleSpace?.Invoke();
        }
    }
}