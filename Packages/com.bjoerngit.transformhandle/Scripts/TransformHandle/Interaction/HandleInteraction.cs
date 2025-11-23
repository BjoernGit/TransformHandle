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

        // Profile state
        private HandleProfile currentProfile;
        private HandleSpace draggedAxisSpace; // Remember which space the dragged axis uses

        public HandleInteraction(Camera camera)
        {
            mainCamera = camera;
            hoverDetector = new HandleHoverDetector(camera);
            translationHandler = new TranslationDragHandler(camera);
            rotationHandler = new RotationDragHandler(camera);
            scaleHandler = new ScaleDragHandler(camera);
        }

        public void SetCamera(Camera camera)
        {
            mainCamera = camera;

            // Update all sub-components with new camera
            if (hoverDetector != null)
                hoverDetector = new HandleHoverDetector(camera);
            if (translationHandler != null)
                translationHandler = new TranslationDragHandler(camera);
            if (rotationHandler != null)
                rotationHandler = new RotationDragHandler(camera);
            if (scaleHandler != null)
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

            Vector2 mousePos = Input.mousePosition;
            bool mousePressed = Input.GetMouseButtonDown(0);
            bool mouseReleased = Input.GetMouseButtonUp(0);

            if (!IsDragging)
            {
                // Update hover state with space
                HoveredAxis = hoverDetector.GetHoveredAxis(mousePos, target, handleScale, handleType, handleSpace);

                // Start drag if mouse pressed on a handle
                if (mousePressed && HoveredAxis >= 0)
                    StartDrag(handleType, handleSpace, mousePos);
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

            currentProfile = profile;

            Vector2 mousePos = Input.mousePosition;
            bool mousePressed = Input.GetMouseButtonDown(0);
            bool mouseReleased = Input.GetMouseButtonUp(0);

            if (!IsDragging)
            {
                // Update hover state with profile
                HoveredAxis = hoverDetector.GetHoveredAxisWithProfile(mousePos, target, handleScale, handleType, profile);

                // Start drag if mouse pressed on a handle
                if (mousePressed && HoveredAxis >= 0)
                {
                    // Determine which space this axis uses from profile
                    HandleSpace axisSpace = GetAxisSpaceFromProfile(profile, handleType, HoveredAxis);
                    StartDragWithSpace(handleType, axisSpace, mousePos);
                }
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

        private void StartDrag(HandleType handleType, HandleSpace handleSpace, Vector2 mousePos)
        {
            IsDragging = true;
            DraggedAxis = HoveredAxis;
            draggedAxisSpace = handleSpace;

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

            // Pass space into StartDrag
            currentDragHandler.StartDrag(target, DraggedAxis, mousePos, handleSpace);
        }

        private void StartDragWithSpace(HandleType handleType, HandleSpace axisSpace, Vector2 mousePos)
        {
            IsDragging = true;
            DraggedAxis = HoveredAxis;
            draggedAxisSpace = axisSpace;

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

            // Pass the determined space into StartDrag
            currentDragHandler.StartDrag(target, DraggedAxis, mousePos, axisSpace);
        }

        private void EndDrag()
        {
            currentDragHandler?.EndDrag();
            currentDragHandler = null;
            IsDragging = false;
            DraggedAxis = -1;
        }

        /// <summary>
        /// Determines which space an axis should use based on the profile configuration.
        /// </summary>
        private HandleSpace GetAxisSpaceFromProfile(HandleProfile profile, HandleType handleType, int axis)
        {
            // Check if this axis is enabled in local space
            bool hasLocal = profile.IsAxisEnabled(handleType, axis, HandleSpace.Local);
            // Check if this axis is enabled in global space  
            bool hasGlobal = profile.IsAxisEnabled(handleType, axis, HandleSpace.Global);

            // Handle Rotation modes (Axis 3: Roll, Axis 7: Trackball)
            if (handleType == HandleType.Rotation && (axis == 3 || axis == 7))
            {
                // These are camera-space operations, but must return an enabled space.
                if (hasGlobal)
                    return HandleSpace.Global;
                if (hasLocal)
                    return HandleSpace.Local;

                return HandleSpace.Global;
            }

            // If both are enabled, we need a priority system.
            // For planes (axis 4-6), we prefer the space that has components enabled.
            if (axis >= 4 && axis <= 6)
            {
                if (hasLocal) return HandleSpace.Local;
                if (hasGlobal) return HandleSpace.Global;
            }
            else // Regular axes (0, 1, 2)
            {
                // For regular axes, prefer local space if both are enabled.
                if (hasLocal) return HandleSpace.Local;
                if (hasGlobal) return HandleSpace.Global;
            }

            return HandleSpace.Local; // Final Fallback
        }
    }
}