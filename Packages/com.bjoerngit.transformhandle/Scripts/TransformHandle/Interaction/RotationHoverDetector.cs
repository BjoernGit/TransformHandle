using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Handles hover detection specifically for rotation handles
    /// </summary>
    public class RotationHoverDetector : BaseHoverDetector
    {
        private const int CIRCLE_SEGMENTS = 64;
        private const float FREE_ROTATION_SCALE = 1.2f; // Same as in renderer

        public RotationHoverDetector(Camera camera) : base(camera) { }

        public override int GetHoveredAxis(Vector2 mousePos, Transform target, float handleScale, HandleSpace handleSpace)
        {
            float minDist = float.MaxValue;
            int axis = -1;

            // 1. Check normal rotation axes (0-2)
            for (int i = 0; i < 3; i++)
            {
                Vector3 normal = GetAxisDirection(target, i, handleSpace);
                float dist = GetDistanceToCircle(mousePos, target.position, normal, handleScale);

                // Track the closest axis hit that is within the ROTATION_THRESHOLD
                if (dist < minDist && dist < ROTATION_THRESHOLD)
                {
                    minDist = dist;
                    axis = i;
                }
            }

            // 2. Check the Roll Rotation Ring (Axis 3) - Higher priority than Trackball Area
            float freeRotationDist = GetDistanceToFreeRotationCircle(mousePos, target.position, handleScale * FREE_ROTATION_SCALE);
            if (freeRotationDist < minDist && freeRotationDist < ROTATION_THRESHOLD)
            {
                minDist = freeRotationDist;
                axis = 3; // Roll Rotation Ring gewinnt über Achsen 0-2, wenn näher
            }

            // 3. FALLBACK LOGIC FOR TRACKBALL (Axis 7): Check if mouse is inside the bounding area
            //    Wähle Axis 7, wenn:
            //    a) Bisher keine Achse getroffen wurde (axis == -1)
            //    b) ODER die am nächsten getroffene Achse weiter entfernt ist als der ROTATION_THRESHOLD (d.h. kein präziser Treffer)
            if (axis == -1 || minDist > ROTATION_THRESHOLD)
            {
                float freeRotationWorldRadius = handleScale * FREE_ROTATION_SCALE;
                float freeRotationScreenRadius = GetHandleScreenRadius(target.position, freeRotationWorldRadius);

                float distToCenter = GetDistanceToHandleCenter(mousePos, target.position);

                // If the mouse is visually inside the handle area
                if (distToCenter < freeRotationScreenRadius)
                {
                    // Setzt Achse 7, da kein präziser Treffer auf 0, 1, 2 oder 3 vorlag.
                    axis = 7;
                }
            }

            return axis;
        }

        public override int GetHoveredAxisWithProfile(Vector2 mousePos, Transform target, float handleScale, HandleProfile profile)
        {
            float minDist = float.MaxValue;
            int axis = -1;

            // 1. Check normal rotation axes (0-2)
            for (int i = 0; i < 3; i++)
            {
                // Check local space
                if (profile.IsAxisEnabled(HandleType.Rotation, i, HandleSpace.Local))
                {
                    float dist = GetDistanceToCircleInSpace(mousePos, target, i, handleScale, HandleSpace.Local);
                    if (dist < minDist && dist < ROTATION_THRESHOLD)
                    {
                        minDist = dist;
                        axis = i;
                    }
                }

                // Check global space
                if (profile.IsAxisEnabled(HandleType.Rotation, i, HandleSpace.Global))
                {
                    float dist = GetDistanceToCircleInSpace(mousePos, target, i, handleScale, HandleSpace.Global);
                    if (dist < minDist && dist < ROTATION_THRESHOLD)
                    {
                        minDist = dist;
                        axis = i;
                    }
                }
            }

            // 2. FALLBACK LOGIC FOR FREE ROTATION (Axis 3) WITH PROFILE CHECK

            // Check if free rotation is enabled in the profile
            if (profile.IsAxisEnabled(HandleType.Rotation, 3, HandleSpace.Local) ||
                profile.IsAxisEnabled(HandleType.Rotation, 3, HandleSpace.Global))
            {
                float freeRotationWorldRadius = handleScale * FREE_ROTATION_SCALE;
                float freeRotationScreenRadius = GetHandleScreenRadius(target.position, freeRotationWorldRadius);

                float distToCenter = GetDistanceToHandleCenter(mousePos, target.position);

                // If the mouse is visually inside the handle area
                if (distToCenter < freeRotationScreenRadius)
                {
                    // If no direct axis hit was found, use axis 3 as fallback
                    if (axis == -1 || minDist > ROTATION_THRESHOLD)
                    {
                        axis = 3;
                    }
                }
            }

            return axis;
        }

        private float GetDistanceToCircleInSpace(Vector2 mousePos, Transform target, int axisIndex, float radius, HandleSpace space)
        {
            Vector3 normal = GetAxisDirection(target, axisIndex, space);
            return GetDistanceToCircle(mousePos, target.position, normal, radius);
        }

        private float GetDistanceToCircle(Vector2 mousePos, Vector3 center, Vector3 normal, float radius)
        {
            // Create tangent vectors for the circle
            Vector3 tangent1 = GetPerpendicularVector(normal);
            Vector3 tangent2 = Vector3.Cross(normal, tangent1).normalized;

            float minDist = float.MaxValue;
            Vector3 camForward = mainCamera.transform.forward;

            // Sample points on the circle
            for (int i = 0; i < CIRCLE_SEGMENTS; i++)
            {
                float angle = i / (float)CIRCLE_SEGMENTS * Mathf.PI * 2f;
                Vector3 direction = tangent1 * Mathf.Cos(angle) + tangent2 * Mathf.Sin(angle);

                // Only check front-facing segments
                if (Vector3.Dot(direction, camForward) < 0f)
                {
                    Vector3 worldPoint = center + direction * radius;
                    if (!IsPointBehindCamera(worldPoint))
                    {
                        Vector3 screenPoint = mainCamera.WorldToScreenPoint(worldPoint);
                        float dist = Vector2.Distance(mousePos, new Vector2(screenPoint.x, screenPoint.y));
                        minDist = Mathf.Min(minDist, dist);
                    }
                }
            }

            return minDist;
        }

        /// <summary>
        /// Calculates the screen-space distance from the mouse position to the handle center.
        /// Used for checking if the mouse is within the bounding area.
        /// </summary>
        private float GetDistanceToHandleCenter(Vector2 mousePos, Vector3 center)
        {
            if (IsPointBehindCamera(center))
                return float.MaxValue;

            Vector3 screenPos = mainCamera.WorldToScreenPoint(center);
            return Vector2.Distance(mousePos, new Vector2(screenPos.x, screenPos.y));
        }

        /// <summary>
        /// Estimates the screen-space radius of a world-space circle for hover bounding check.
        /// </summary>
        private float GetHandleScreenRadius(Vector3 worldCenter, float radius)
        {
            // Use the camera-facing normal for approximation (similar to free rotation)
            Vector3 normal = (mainCamera.transform.position - worldCenter).normalized;
            Vector3 tangent = GetPerpendicularVector(normal);

            Vector3 worldPoint = worldCenter + tangent * radius;

            Vector3 screenCenter = mainCamera.WorldToScreenPoint(worldCenter);
            Vector3 screenPoint = mainCamera.WorldToScreenPoint(worldPoint);

            // If the center or point is behind the camera, assume max distance
            if (screenCenter.z < 0f || screenPoint.z < 0f) return float.MaxValue;

            return Vector2.Distance(new Vector2(screenCenter.x, screenCenter.y), new Vector2(screenPoint.x, screenPoint.y));
        }

        private Vector3 GetPerpendicularVector(Vector3 normal)
        {
            Vector3 perpendicular = Vector3.Cross(normal, Vector3.up).normalized;
            if (perpendicular.sqrMagnitude < 0.1f)
            {
                perpendicular = Vector3.Cross(normal, Vector3.right).normalized;
            }
            return perpendicular;
        }

        private float GetDistanceToFreeRotationCircle(Vector2 mousePos, Vector3 center, float radius)
        {
            // Free rotation circle is always camera-facing
            Vector3 normal = (mainCamera.transform.position - center).normalized;

            // For camera-facing circles, we need a different approach
            // since all points are equidistant from the camera
            Vector3 tangent1 = GetPerpendicularVector(normal);
            Vector3 tangent2 = Vector3.Cross(normal, tangent1).normalized;

            float minDist = float.MaxValue;

            // Sample all points on the circle without visibility culling
            for (int i = 0; i < CIRCLE_SEGMENTS; i++)
            {
                float angle = i / (float)CIRCLE_SEGMENTS * Mathf.PI * 2f;
                Vector3 direction = tangent1 * Mathf.Cos(angle) + tangent2 * Mathf.Sin(angle);
                Vector3 worldPoint = center + direction * radius;

                if (!IsPointBehindCamera(worldPoint))
                {
                    Vector3 screenPoint = mainCamera.WorldToScreenPoint(worldPoint);
                    float dist = Vector2.Distance(mousePos, new Vector2(screenPoint.x, screenPoint.y));
                    minDist = Mathf.Min(minDist, dist);
                }
            }

            return minDist;
        }
    }
}