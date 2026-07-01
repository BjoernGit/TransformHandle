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

        private Vector3 scaleStartValue;
        private Vector2 dragStartMousePos;
        private Vector3 dragAxisMask;

        // Axis-constrained drag (ray-to-axis projection, world space)
        private Vector3 axisDirection;
        private float dragStartAxisOffset;

        // Handle scale as computed by the manager (matches the rendered size).
        // Dragging the box one handle-length outward doubles the scale.
        private float handleScale;

        // Settings
        private readonly float uniformScaleSpeed = 0.01f;

        public ScaleDragHandler(Camera camera)
        {
            mainCamera = camera;
        }

        public void StartDrag(Transform target, int axis, Vector2 mousePos, HandleSpace space, float handleScale)
        {
            this.target = target;
            this.draggedAxis = axis;
            this.handleScale = Mathf.Max(handleScale, 1e-5f);

            scaleStartValue = target.localScale;
            dragStartMousePos = mousePos;

            // Store axis mask for axis-constrained scaling
            dragAxisMask = GetScaleAxisMask(axis);

            if (axis >= 0 && axis <= 2)
            {
                // Use the same space-aware direction the handle was rendered with,
                // and remember which point along the axis line the user grabbed.
                axisDirection = TranslationHandleUtils.GetAxisDirection(target, axis, space);

                Ray ray = mainCamera.ScreenPointToRay(mousePos);
                if (!HandleMathUtils.TryGetRayLineOffset(ray, target.position, axisDirection, out dragStartAxisOffset))
                    dragStartAxisOffset = 0f;
            }
        }

        public void UpdateDrag(Vector2 mousePos)
        {
            if (target == null) return;

            // Uniform scale (center handle, axis 3)
            if (draggedAxis == 3)
            {
                // Use vertical mouse movement for scaling
                Vector2 mouseDelta = mousePos - dragStartMousePos;
                float scaleFactor = 1f + (mouseDelta.y * uniformScaleSpeed);

                // Prevent negative or zero scale
                scaleFactor = Mathf.Max(scaleFactor, 0.01f);

                target.localScale = scaleStartValue * scaleFactor;
                return;
            }

            // Axis-constrained scale (axes 0-2):
            // Project the mouse ray onto the axis line in world space - immune to
            // camera distance, viewport rect / split-screen and view angle.
            Ray ray = mainCamera.ScreenPointToRay(mousePos);
            if (!HandleMathUtils.TryGetRayLineOffset(ray, target.position, axisDirection, out float offset))
                return;

            float worldDelta = offset - dragStartAxisOffset;

            // Handle length is the reference: dragging one handle-length outward
            // doubles the scale, dragging inward shrinks it.
            float axisScaleFactor = 1f + worldDelta / handleScale;
            axisScaleFactor = Mathf.Max(axisScaleFactor, 0.01f);

            // Apply scale based on axis mask
            Vector3 newScale = scaleStartValue;
            newScale.x *= Mathf.Lerp(1f, axisScaleFactor, dragAxisMask.x);
            newScale.y *= Mathf.Lerp(1f, axisScaleFactor, dragAxisMask.y);
            newScale.z *= Mathf.Lerp(1f, axisScaleFactor, dragAxisMask.z);

            target.localScale = newScale;
        }

        public void EndDrag()
        {
            target = null;
            draggedAxis = -1;
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
