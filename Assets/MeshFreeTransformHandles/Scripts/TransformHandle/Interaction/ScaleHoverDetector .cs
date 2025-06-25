using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Handles hover detection specifically for scale handles
    /// </summary>
    public class ScaleHoverDetector : BaseHoverDetector
    {
        private const float CENTER_HANDLE_MULTIPLIER = 0.09f; // 0.06f * 1.5f from original

        public ScaleHoverDetector(Camera camera) : base(camera) { }

        public override int GetHoveredAxis(Vector2 mousePos, Transform target, float handleScale, HandleSpace handleSpace)
        {
            float minDist = float.MaxValue;
            int axis = -1;

            // Check axis handles (0, 1, 2) - Scale typically uses local space
            for (int i = 0; i < 3; i++)
            {
                Vector3 dir = GetAxisDirection(target, i, HandleSpace.Local);
                float dist = GetDistanceToScaleHandle(mousePos, target.position, dir, handleScale);
                
                if (dist < minDist && dist < SCALE_THRESHOLD)
                {
                    minDist = dist;
                    axis = i;
                }
            }

            // Check center handle (index 3) for uniform scale
            float centerDist = GetDistanceToCenterHandle(mousePos, target.position, handleScale * CENTER_HANDLE_MULTIPLIER);
            if (centerDist < minDist && centerDist < SCALE_THRESHOLD)
            {
                axis = 3;
            }

            return axis;
        }

        public override int GetHoveredAxisWithProfile(Vector2 mousePos, Transform target, float handleScale, HandleProfile profile)
        {
            float minDist = float.MaxValue;
            int axis = -1;

            // Check axis handles (0, 1, 2)
            for (int i = 0; i < 3; i++)
            {
                // Check local space
                if (profile.IsAxisEnabled(HandleType.Scale, i, HandleSpace.Local))
                {
                    float dist = GetDistanceToScaleHandleInSpace(mousePos, target, i, handleScale, HandleSpace.Local);
                    if (dist < minDist && dist < SCALE_THRESHOLD)
                    {
                        minDist = dist;
                        axis = i;
                    }
                }

                // Check global space (less common for scale)
                if (profile.IsAxisEnabled(HandleType.Scale, i, HandleSpace.Global))
                {
                    float dist = GetDistanceToScaleHandleInSpace(mousePos, target, i, handleScale, HandleSpace.Global);
                    if (dist < minDist && dist < SCALE_THRESHOLD)
                    {
                        minDist = dist;
                        axis = i;
                    }
                }
            }

            // Check center handle (index 3) - uniform scale
            if (profile.IsAxisEnabled(HandleType.Scale, 3, HandleSpace.Local) || 
                profile.IsAxisEnabled(HandleType.Scale, 3, HandleSpace.Global))
            {
                float centerDist = GetDistanceToCenterHandle(mousePos, target.position, handleScale * CENTER_HANDLE_MULTIPLIER);
                if (centerDist < minDist && centerDist < SCALE_THRESHOLD)
                {
                    axis = 3;
                }
            }

            return axis;
        }

        private float GetDistanceToScaleHandleInSpace(Vector2 mousePos, Transform target, int axisIndex, float scale, HandleSpace space)
        {
            Vector3 dir = GetAxisDirection(target, axisIndex, space);
            return GetDistanceToScaleHandle(mousePos, target.position, dir, scale);
        }

        private float GetDistanceToScaleHandle(Vector2 mousePos, Vector3 origin, Vector3 direction, float scale)
        {
            // Scale handle has a box at the end of the axis
            Vector3 boxCenter = origin + direction * scale;
            
            if (IsPointBehindCamera(boxCenter))
                return float.MaxValue;
            
            Vector3 screenPos = mainCamera.WorldToScreenPoint(boxCenter);
            return Vector2.Distance(mousePos, new Vector2(screenPos.x, screenPos.y));
        }

        private float GetDistanceToCenterHandle(Vector2 mousePos, Vector3 center, float size)
        {
            if (IsPointBehindCamera(center))
                return float.MaxValue;
            
            Vector3 screenPos = mainCamera.WorldToScreenPoint(center);
            return Vector2.Distance(mousePos, new Vector2(screenPos.x, screenPos.y));
        }
    }
}