using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Handles the dragging logic for translation operations,
    /// supporting both single-axis and plane translations.
    /// </summary>
    public class TranslationDragHandler : IDragHandler
    {
        private Camera mainCamera;
        private Transform target;
        private int draggedAxis;

        // Single axis drag
        private Vector3 dragStartWorldPos;
        private Vector3 axisDirection;
        private float dragStartAxisOffset; // Grabbed point along the axis at drag start

        // Plane drag
        private bool isDraggingPlane;
        private Vector3 planeNormal;
        private Vector3 planeStartPosition;
        private Vector3 initialOffset;

        // Handle scale as computed by the manager (matches the rendered size)
        private float handleScale;

        public TranslationDragHandler(Camera camera)
        {
            mainCamera = camera;
        }

        public void StartDrag(Transform target, int axis, Vector2 mousePos, HandleSpace space, float handleScale)
        {
            this.target = target;
            this.draggedAxis = axis;
            this.handleScale = handleScale;

            dragStartWorldPos = target.position;
            isDraggingPlane = axis >= 4 && axis <= 6;

            if (isDraggingPlane)
            {
                StartPlaneDrag(mousePos, space);
            }
            else
            {
                StartAxisDrag(mousePos, space);
            }
        }

        private void StartAxisDrag(Vector2 mousePos, HandleSpace space)
        {
            // Get axis direction using utils (unit length, world space)
            axisDirection = TranslationHandleUtils.GetAxisDirection(target, draggedAxis, space);

            // Remember which point along the axis line the user grabbed.
            // Everything else works in world space, so the drag is immune to
            // camera distance, viewport rect / split-screen and view angle.
            Ray ray = mainCamera.ScreenPointToRay(mousePos);
            if (HandleMathUtils.TryGetRayLineOffset(ray, dragStartWorldPos, axisDirection, out float offset))
                dragStartAxisOffset = offset;
            else
                dragStartAxisOffset = 0f;
        }

        private void StartPlaneDrag(Vector2 mousePos, HandleSpace space)
        {
            // Get plane normal using utils
            planeNormal = TranslationHandleUtils.GetPlaneNormal(draggedAxis, target, space);
            
            // Calculate the actual plane position (accounting for camera-facing offset)
            Vector3 camForward = mainCamera.transform.forward;
            float planeSize = TranslationHandleRenderer.PLANE_SIZE_MULTIPLIER * handleScale;
            
            var (axis1, axis2) = TranslationHandleUtils.GetPlaneAxes(target, draggedAxis, space);
            Vector3 offset = TranslationHandleUtils.CalculatePlaneOffset(axis1, axis2, planeSize, camForward);
            
            planeStartPosition = target.position + offset;

            // Calculate initial offset from plane center to hit point
            Ray ray = mainCamera.ScreenPointToRay(mousePos);
            Plane dragPlane = new Plane(planeNormal, planeStartPosition);
            
            if (dragPlane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                initialOffset = target.position - hitPoint;
            }
        }

        public void UpdateDrag(Vector2 mousePos)
        {
            if (target == null) return;

            if (isDraggingPlane)
            {
                UpdatePlaneDrag(mousePos);
            }
            else
            {
                UpdateAxisDrag(mousePos);
            }
        }

        private void UpdateAxisDrag(Vector2 mousePos)
        {
            Ray ray = mainCamera.ScreenPointToRay(mousePos);

            // Find where the mouse ray is closest to the axis line. If the ray is
            // (almost) parallel to the axis - i.e. looking straight down the arrow -
            // the projection is undefined, so we keep the current position instead
            // of letting it jump.
            if (!HandleMathUtils.TryGetRayLineOffset(ray, dragStartWorldPos, axisDirection, out float offset))
                return;

            float worldMovement = offset - dragStartAxisOffset;
            target.position = dragStartWorldPos + axisDirection * worldMovement;
        }

        private void UpdatePlaneDrag(Vector2 mousePos)
        {
            Ray ray = mainCamera.ScreenPointToRay(mousePos);
            Plane dragPlane = new Plane(planeNormal, planeStartPosition);

            if (dragPlane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                
                // New position is hit point plus initial offset
                Vector3 newPosition = hitPoint + initialOffset;
                
                // The movement is already constrained to the plane by the ray-plane intersection
                target.position = newPosition;
            }
        }

        public void EndDrag()
        {
            target = null;
            draggedAxis = -1;
            isDraggingPlane = false;
        }
    }
}