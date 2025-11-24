using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Handles the dragging logic for scale operations
    /// </summary>
    public class ScaleDragHandler : IDragHandler
    {
        private Camera mainCamera;
        private Transform target;
        private int draggedAxis;
        private HandleSpace handleSpace; // Not used for scale, but required by interface

        private Vector3 scaleStartValue;
        private Vector2 dragStartMousePos;
        private Vector3 dragAxisDirection;

        // Settings
        private readonly float scaleSpeed = 0.01f;

        public ScaleDragHandler(Camera camera)
        {
            mainCamera = camera;
        }

        public void StartDrag(Transform target, int axis, Vector2 mousePos, HandleSpace space)
        {
            this.target = target;
            this.draggedAxis = axis;
            this.handleSpace = space; // Ignored for scale

            scaleStartValue = target.localScale;
            dragStartMousePos = mousePos;

            // Store axis direction for axis-constrained scaling
            dragAxisDirection = GetScaleAxisMask(axis);
        }

        public void UpdateDrag(Vector2 mousePos)
        {
            if (target == null) return;

            // Mouse delta in screen space
            Vector2 mouseDelta = mousePos - dragStartMousePos;

            // Uniform scale (center handle, axis 3)
            if (draggedAxis == 3)
            {
                // Use vertical mouse movement for scaling
                float projectedDelta = mouseDelta.y;

                // Convert to scale factor
                float scaleFactor = 1f + (projectedDelta * scaleSpeed);

                //prevent negative or zero scale
                scaleFactor = Mathf.Max(scaleFactor, 0.01f);

                target.localScale = scaleStartValue * scaleFactor;
                return;
            }

            // Axis-constrained scale (axes 0–2)

            // Project mouse movement onto screen-space axis for better control
            Vector3 handleScreenPos = mainCamera.WorldToScreenPoint(target.position);

            // Get screen direction of the handle
            Vector3 worldDir = GetWorldAxisDirection(draggedAxis);
            Vector3 screenEndPos = mainCamera.WorldToScreenPoint(target.position + worldDir);
            Vector2 screenDir = new Vector2(
                screenEndPos.x - handleScreenPos.x,
                screenEndPos.y - handleScreenPos.y
            ).normalized;

            // Project mouse delta onto axis direction
            float axisProjectedDelta = Vector2.Dot(mouseDelta, screenDir);

            // Convert to scale factor
            float axisScaleFactor = 1f + (axisProjectedDelta * scaleSpeed);
            axisScaleFactor = Mathf.Max(axisScaleFactor, 0.01f);

            // Apply scale based on axis mask
            Vector3 newScale = scaleStartValue;
            newScale.x *= Mathf.Lerp(1f, axisScaleFactor, dragAxisDirection.x);
            newScale.y *= Mathf.Lerp(1f, axisScaleFactor, dragAxisDirection.y);
            newScale.z *= Mathf.Lerp(1f, axisScaleFactor, dragAxisDirection.z);

            target.localScale = newScale;
        }


        public void EndDrag()
        {
            target = null;
            draggedAxis = -1;
        }

        private Vector3 GetWorldAxisDirection(int axis)
        {
            switch (axis)
            {
                case 0: return target.right;
                case 1: return target.up;
                case 2: return target.forward;
                default: return Vector3.zero;
            }
        }

        private Vector3 GetScaleAxisMask(int axis)
        {
            switch (axis)
            {
                case 0: return Vector3.right;    // Scale only X
                case 1: return Vector3.up;       // Scale only Y
                case 2: return Vector3.forward;  // Scale only Z
                case 3: return Vector3.one;      // Scale all (uniform)
                default: return Vector3.one;
            }
        }
    }
}