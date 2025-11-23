using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Handles hover detection specifically for translation handles
    /// </summary>
    public class TranslationHoverDetector : BaseHoverDetector
    {
        public TranslationHoverDetector(Camera camera) : base(camera) { }

        public override int GetHoveredAxis(Vector2 mousePos, Transform target, float handleScale, HandleSpace handleSpace)
        {
            float minDist = float.MaxValue;
            int axis = -1;

            // Check regular axes first (0-2)
            for (int i = 0; i < 3; i++)
            {
                Vector3 dir = TranslationHandleUtils.GetAxisDirection(target, i, handleSpace);
                float dist = GetDistanceToAxis(mousePos, target.position, dir, handleScale);
                
                if (dist < minDist && dist < TRANSLATION_THRESHOLD)
                {
                    minDist = dist;
                    axis = i;
                }
            }

            // Check plane handles (4-6)
            float planeScale = handleScale * TranslationHandleRenderer.PLANE_SIZE_MULTIPLIER;
            Vector3 camForward = mainCamera.transform.forward;

            for (int planeIndex = 4; planeIndex <= 6; planeIndex++)
            {
                var (axis1, axis2) = TranslationHandleUtils.GetPlaneAxes(target, planeIndex, handleSpace);
                Vector3 offset = TranslationHandleUtils.CalculatePlaneOffset(axis1, axis2, planeScale, camForward);
                
                float dist = GetDistanceToPlane(mousePos, target.position + offset, axis1, axis2, planeScale);
                if (dist < minDist && dist < TRANSLATION_THRESHOLD)
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

            // Check regular axes (0-2)
            for (int i = 0; i < 3; i++)
            {
                // Check both spaces
                foreach (HandleSpace space in System.Enum.GetValues(typeof(HandleSpace)))
                {
                    if (profile.IsAxisEnabled(HandleType.Translation, i, space))
                    {
                        Vector3 dir = TranslationHandleUtils.GetAxisDirection(target, i, space);
                        float dist = GetDistanceToAxis(mousePos, target.position, dir, handleScale);
                        
                        if (dist < minDist && dist < TRANSLATION_THRESHOLD)
                        {
                            minDist = dist;
                            axis = i;
                        }
                    }
                }
            }

            // Check plane handles (4-6)
            CheckPlanesWithProfile(mousePos, target, handleScale, profile, ref minDist, ref axis);

            return axis;
        }

        private void CheckPlanesWithProfile(Vector2 mousePos, Transform target, float handleScale, 
                                           HandleProfile profile, ref float minDist, ref int axis)
        {
            float planeSize = handleScale * TranslationHandleRenderer.PLANE_SIZE_MULTIPLIER;
            Camera cam = mainCamera;
            Vector3 camForward = cam.transform.forward;

            for (int planeIndex = 4; planeIndex <= 6; planeIndex++)
            {
                foreach (HandleSpace space in System.Enum.GetValues(typeof(HandleSpace)))
                {
                    if (profile.IsAxisEnabled(HandleType.Translation, planeIndex, space))
                    {
                        var (axis1, axis2) = TranslationHandleUtils.GetPlaneAxes(target, planeIndex, space);
                        Vector3 offset = TranslationHandleUtils.CalculatePlaneOffset(axis1, axis2, planeSize, camForward);
                        
                        float dist = GetDistanceToPlane(mousePos, target.position + offset, axis1, axis2, planeSize);
                        if (dist < minDist && dist < TRANSLATION_THRESHOLD)
                        {
                            minDist = dist;
                            axis = planeIndex;
                        }
                    }
                }
            }
        }

        private float GetDistanceToAxis(Vector2 mousePos, Vector3 origin, Vector3 direction, float scale)
        {
            Vector3 endPoint = origin + direction * scale;

            if (IsPointBehindCamera(origin) || IsPointBehindCamera(endPoint))
                return float.MaxValue;

            Vector3 originScreen = mainCamera.WorldToScreenPoint(origin);
            Vector3 endScreen = mainCamera.WorldToScreenPoint(endPoint);

            Vector2 originScreen2D = new Vector2(originScreen.x, originScreen.y);
            Vector2 endScreen2D = new Vector2(endScreen.x, endScreen.y);

            // 1. Calculate distance to the central line segment (shaft)
            float distToLine = DistancePointToLineSegment(
                mousePos,
                originScreen2D,
                endScreen2D
            );

            // 2. Check proximity to the arrow tip (cone)
            // The visual cone is wider than the line threshold, so we apply a larger hit radius at the tip.
            float distToTip = Vector2.Distance(mousePos, endScreen2D);
            const float CONE_HIT_RADIUS = 25f; // Visual radius approximation of the arrow head in pixels

            if (distToTip < CONE_HIT_RADIUS)
            {
                // Return 0 to ensure detection if the mouse is within the visual cone radius,
                // overriding the stricter line threshold.
                return 0f;
            }

            return distToLine;
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

            // Otherwise, distance to edges
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