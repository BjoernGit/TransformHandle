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

        public TranslationDragHandler(Camera camera)
        {
            mainCamera = camera;
        }

        public void StartDrag(Transform target, int axis, Vector2 mousePos, HandleSpace space)
        {
            this.target = target;
            this.draggedAxis = axis;
            
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
            if (TryGetAxisOffset(ray, out float offset))
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
            float planeSize = TranslationHandleRenderer.PLANE_SIZE_MULTIPLIER * GetHandleScale();
            
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
            if (!TryGetAxisOffset(ray, out float offset))
                return;

            float worldMovement = offset - dragStartAxisOffset;
            target.position = dragStartWorldPos + axisDirection * worldMovement;
        }

        /// <summary>
        /// Projects the closest point between the mouse ray and the axis line
        /// (through the drag start position, along <see cref="axisDirection"/>)
        /// onto the axis and returns its signed distance from the start position.
        /// Returns false when the ray is nearly parallel to the axis.
        /// </summary>
        private bool TryGetAxisOffset(Ray ray, out float offset)
        {
            offset = 0f;

            Vector3 d = axisDirection;      // axis line direction (unit length)
            Vector3 r = ray.direction;      // mouse ray direction (unit length)
            Vector3 w0 = dragStartWorldPos - ray.origin;

            float b = Vector3.Dot(d, r);
            float denom = 1f - b * b;       // = (d·d)(r·r) - (d·r)^2, with unit vectors

            if (denom < 1e-6f)
                return false;               // ray parallel to axis -> undefined

            float dW0 = Vector3.Dot(d, w0);
            float rW0 = Vector3.Dot(r, w0);

            // Scalar along the axis for the closest point to the ray.
            offset = (b * rW0 - dW0) / denom;
            return true;
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

        private float GetHandleScale()
        {
            // Estimate handle scale based on camera distance
            float distance = Vector3.Distance(mainCamera.transform.position, target.position);
            return distance * 0.1f; // Adjust multiplier as needed
        }
    }
}