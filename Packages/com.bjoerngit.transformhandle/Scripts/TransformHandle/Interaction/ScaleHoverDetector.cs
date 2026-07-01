using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Handles hover detection specifically for scale handles (axes and uniform center).
    /// </summary>
    public class ScaleHoverDetector : BaseHoverDetector
    {
        private const float CENTER_HANDLE_MULTIPLIER = 0.09f;

        public ScaleHoverDetector(Camera camera) : base(camera) { }

        public override int GetHoveredAxis(Vector2 mousePos, Transform target, float handleScale, HandleSpace handleSpace)
        {
            // 1. Uniform center: always prioritized if close enough
            float centerSize = handleScale * CENTER_HANDLE_MULTIPLIER;
            float centerDist = GetDistanceToCenterHandle(mousePos, target.position, centerSize);

            if (centerDist < CENTER_THRESHOLD)
                return 3; // uniform scale

            // 2. Linear scale axes (0–2)
            float minDist = float.MaxValue;
            int axis = -1;

            for (int i = 0; i < 3; i++)
            {
                float dist = GetDistanceToScaleHandleInSpace(mousePos, target, i, handleScale, handleSpace);

                if (dist < minDist && dist < AXIS_THRESHOLD)
                {
                    minDist = dist;
                    axis = i;
                }
            }

            return axis;
        }

        public override int GetHoveredAxisWithProfile(Vector2 mousePos, Transform target, float handleScale, HandleProfile profile)
        {
            // 1. Uniform center: prioritized if close enough
            float centerSize = handleScale * CENTER_HANDLE_MULTIPLIER;
            float centerDist = GetDistanceToCenterHandle(mousePos, target.position, centerSize);

            if (centerDist < CENTER_THRESHOLD)
                return 3;

            // 2. Linear axes with profile check
            float minDist = float.MaxValue;
            int axis = -1;

            foreach (HandleSpace space in System.Enum.GetValues(typeof(HandleSpace)))
            {
                for (int i = 0; i < 3; i++)
                {
                    if (!profile.IsAxisEnabled(HandleType.Scale, i, space))
                        continue;

                    float dist = GetDistanceToScaleHandleInSpace(mousePos, target, i, handleScale, space);

                    if (dist < minDist && dist < AXIS_THRESHOLD)
                    {
                        minDist = dist;
                        axis = i;
                    }
                }
            }

            return axis;
        }

        private float GetDistanceToScaleHandleInSpace(Vector2 mousePos, Transform target, int axisIndex, float scale, HandleSpace space)
        {
            Vector3 dir = GetAxisDirection(target, axisIndex, space);
            return CalculateDistanceToLinearHandle(mousePos, target.position, dir, scale, tipHitRadius: 20f);
        }

        private float GetDistanceToCenterHandle(Vector2 mousePos, Vector3 center, float size)
        {
            if (IsPointBehindCamera(center))
                return float.MaxValue;

            Vector3 screenPos = mainCamera.WorldToScreenPoint(center);
            Vector2 center2D = new Vector2(screenPos.x, screenPos.y);

            return Vector2.Distance(mousePos, center2D);
        }
    }
}
