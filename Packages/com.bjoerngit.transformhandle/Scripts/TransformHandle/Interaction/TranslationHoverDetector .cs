using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Handles hover detection specifically for translation handles (Axes and Planes).
    /// Uses shared logic from BaseHoverDetector for linear axes.
    /// </summary>
    public class TranslationHoverDetector : BaseHoverDetector
    {
        public TranslationHoverDetector(Camera camera) : base(camera) { }

        public override int GetHoveredAxis(Vector2 mousePos, Transform target, float handleScale, HandleSpace handleSpace)
        {
            float minDist = float.MaxValue;
            int axis = -1;

            // 1. Check regular axes (0-2) using unified linear logic
            for (int i = 0; i < 3; i++)
            {
                Vector3 dir = TranslationHandleUtils.GetAxisDirection(target, i, handleSpace);

                // Use a hit radius of 20f for the arrow tip to ensure good usability
                float dist = CalculateDistanceToLinearHandle(mousePos, target.position, dir, handleScale, tipHitRadius: 20f);

                if (dist < minDist && dist < AXIS_THRESHOLD)
                {
                    minDist = dist;
                    axis = i;
                }
            }

            // 2. Check plane handles (4-6)
            float planeScale = handleScale * TranslationHandleRenderer.PLANE_SIZE_MULTIPLIER;
            Vector3 camForward = mainCamera.transform.forward;

            for (int planeIndex = 4; planeIndex <= 6; planeIndex++)
            {
                var (axis1, axis2) = TranslationHandleUtils.GetPlaneAxes(target, planeIndex, handleSpace);
                Vector3 offset = TranslationHandleUtils.CalculatePlaneOffset(axis1, axis2, planeScale, camForward);

                float dist = GetDistanceToPlane(mousePos, target.position + offset, axis1, axis2, planeScale);

                // We use AXIS_THRESHOLD here as well for consistency across the tool
                if (dist < minDist && dist < AXIS_THRESHOLD)
                {
                    minDist = dist;
                    axis = planeIndex;
                }
            }

            return axis;
        }

        public override int GetHoveredAxisWithProfile(Vector2 mousePos, Transform target, float handleScale, HandleProfile profile)
        {
            float minDist = float.MaxValue;
            int axis = -1;

            // 1. Check regular axes (0-2)
            for (int i = 0; i < 3; i++)
            {
                // Check both spaces (Local/Global) as defined in profile
                foreach (HandleSpace space in System.Enum.GetValues(typeof(HandleSpace)))
                {
                    if (profile.IsAxisEnabled(HandleType.Translation, i, space))
                    {
                        Vector3 dir = TranslationHandleUtils.GetAxisDirection(target, i, space);
                        float dist = CalculateDistanceToLinearHandle(mousePos, target.position, dir, handleScale, tipHitRadius: 20f);

                        if (dist < minDist && dist < AXIS_THRESHOLD)
                        {
                            minDist = dist;
                            axis = i;
                        }
                    }
                }
            }

            // 2. Check plane handles (4-6)
            CheckPlanesWithProfile(mousePos, target, handleScale, profile, ref minDist, ref axis);

            return axis;
        }

        private void CheckPlanesWithProfile(Vector2 mousePos, Transform target, float handleScale,
                                           HandleProfile profile, ref float minDist, ref int axis)
        {
            float planeSize = handleScale * TranslationHandleRenderer.PLANE_SIZE_MULTIPLIER;
            Vector3 camForward = mainCamera.transform.forward;

            for (int planeIndex = 4; planeIndex <= 6; planeIndex++)
            {
                foreach (HandleSpace space in System.Enum.GetValues(typeof(HandleSpace)))
                {
                    if (profile.IsAxisEnabled(HandleType.Translation, planeIndex, space))
                    {
                        var (axis1, axis2) = TranslationHandleUtils.GetPlaneAxes(target, planeIndex, space);
                        Vector3 offset = TranslationHandleUtils.CalculatePlaneOffset(axis1, axis2, planeSize, camForward);

                        float dist = GetDistanceToPlane(mousePos, target.position + offset, axis1, axis2, planeSize);
                        if (dist < minDist && dist < AXIS_THRESHOLD)
                        {
                            minDist = dist;
                            axis = planeIndex;
                        }
                    }
                }
            }
        }

        private float GetDistanceToPlane(Vector2 mousePos, Vector3 center, Vector3 axis1, Vector3 axis2, float size)
        {
            // Calculate plane corners - matching the renderer's corner calculation
            Vector3[] corners = new Vector3[4];
            corners[0] = center;
            corners[1] = center - axis1 * size;
            corners[2] = center - axis1 * size - axis2 * size;
            corners[3] = center - axis2 * size;

            // Check if any corner is behind camera
            for (int i = 0; i < 4; i++)
            {
                if (IsPointBehindCamera(corners[i]))
                    return float.MaxValue;
            }

            // Convert to screen space
            Vector2[] screenCorners = new Vector2[4];
            for (int i = 0; i < 4; i++)
            {
                Vector3 screenPos = mainCamera.WorldToScreenPoint(corners[i]);
                screenCorners[i] = new Vector2(screenPos.x, screenPos.y);
            }

            // Point in polygon test
            if (IsPointInQuad(mousePos, screenCorners))
            {
                return 0f; // Inside the plane
            }

            // Otherwise, calculate distance to closest edge
            float minDist = float.MaxValue;
            for (int i = 0; i < 4; i++)
            {
                int next = (i + 1) % 4;
                float dist = DistancePointToLineSegment(mousePos, screenCorners[i], screenCorners[next]);
                minDist = Mathf.Min(minDist, dist);
            }

            return minDist;
        }

        private bool IsPointInQuad(Vector2 point, Vector2[] quad)
        {
            // Simple point-in-polygon test using cross products
            bool sign = false;
            for (int i = 0; i < 4; i++)
            {
                int next = (i + 1) % 4;
                Vector2 edge = quad[next] - quad[i];
                Vector2 toPoint = point - quad[i];
                float cross = edge.x * toPoint.y - edge.y * toPoint.x;

                if (i == 0)
                    sign = cross > 0;
                else if ((cross > 0) != sign)
                    return false;
            }
            return true;
        }
    }
}