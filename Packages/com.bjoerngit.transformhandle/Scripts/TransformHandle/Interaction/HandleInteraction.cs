using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Manages user input and coordinates interaction components,
    /// supporting Local/Global handle space and mixed-space profiles.
    /// </summary>
    public class HandleInteraction
    {
        private Camera mainCamera;
        private Transform target;

        // Sub-components
        private HandleHoverDetector hoverDetector;
        private IDragHandler translationHandler;
        private IDragHandler rotationHandler;
        private IDragHandler scaleHandler;
        private IDragHandler currentDragHandler;

        // Interaction state
        public int HoveredAxis { get; private set; } = -1;
        public bool IsDragging { get; private set; }
        public int DraggedAxis { get; private set; } = -1;

        public HandleInteraction(Camera camera)
        {
            mainCamera = camera;
            hoverDetector = new HandleHoverDetector(camera);
            translationHandler = new TranslationDragHandler(camera);
            rotationHandler = new RotationDragHandler(camera);
            scaleHandler = new ScaleDragHandler(camera);
        }

        public void UpdateTarget(Transform newTarget)
        {
            target = newTarget;
        }

        /// <summary>
        /// Updates hover and drag state, taking into account handle type and space.
        /// </summary>
        public void Update(float handleScale, HandleType handleType, HandleSpace handleSpace)
        {
            if (target == null || mainCamera == null) return;

            Vector2 mousePos = HandleInput.MousePosition;
            bool mousePressed = HandleInput.LeftMousePressedThisFrame;
            bool mouseReleased = HandleInput.LeftMouseReleasedThisFrame;

            if (!IsDragging)
            {
                // Update hover state with space
                HoveredAxis = hoverDetector.GetHoveredAxis(mousePos, target, handleScale, handleType, handleSpace);

                // Start drag if mouse pressed on a handle
                if (mousePressed && HoveredAxis >= 0)
                    StartDrag(handleType, handleSpace, mousePos, handleScale);
            }
            else
            {
                // Continue drag
                currentDragHandler?.UpdateDrag(mousePos);

                // End drag if mouse released
                if (mouseReleased)
                    EndDrag();
            }
        }

        /// <summary>
        /// Updates hover and drag state using a HandleProfile for mixed-space support.
        /// </summary>
        public void UpdateWithProfile(float handleScale, HandleType handleType, HandleProfile profile)
        {
            if (target == null || mainCamera == null || profile == null) return;

            Vector2 mousePos = HandleInput.MousePosition;
            bool mousePressed = HandleInput.LeftMousePressedThisFrame;
            bool mouseReleased = HandleInput.LeftMouseReleasedThisFrame;

            if (!IsDragging)
            {
                // Update hover state with profile; the detector reports which
                // space the hovered element belongs to.
                HoveredAxis = hoverDetector.GetHoveredAxisWithProfile(
                    mousePos, target, handleScale, handleType, profile, out HandleSpace hoveredSpace);

                // Start drag if mouse pressed on a handle
                if (mousePressed && HoveredAxis >= 0)
                    StartDrag(handleType, hoveredSpace, mousePos, handleScale);
            }
            else
            {
                // Continue drag
                currentDragHandler?.UpdateDrag(mousePos);

                // End drag if mouse released
                if (mouseReleased)
                    EndDrag();
            }
        }

        private void StartDrag(HandleType handleType, HandleSpace handleSpace, Vector2 mousePos, float handleScale)
        {
            IsDragging = true;
            DraggedAxis = HoveredAxis;

            // Choose handler
            switch (handleType)
            {
                case HandleType.Translation:
                    currentDragHandler = translationHandler;
                    break;
                case HandleType.Rotation:
                    currentDragHandler = rotationHandler;
                    break;
                case HandleType.Scale:
                    currentDragHandler = scaleHandler;
                    break;
                default:
                    return;
            }

            // Pass space and handle scale into StartDrag
            currentDragHandler.StartDrag(target, DraggedAxis, mousePos, handleSpace, handleScale);
        }

        private void EndDrag()
        {
            currentDragHandler?.EndDrag();
            currentDragHandler = null;
            IsDragging = false;
            DraggedAxis = -1;
        }
    }
}
