using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Base class for handle hover detection providing shared math utilities.
    /// </summary>
    public abstract class BaseHoverDetector
    {
        protected Camera mainCamera;

        // Unified thresholds for consistent user experience
        protected const float AXIS_THRESHOLD = 10;    // For Lines (Translation/Scale)
        protected const float ROTATION_THRESHOLD = 15f; // For Circles
        protected const float CENTER_THRESHOLD = 20f;   // For Center handles

        public BaseHoverDetector(Camera camera)
        {
            mainCamera = camera;
        }

        public abstract int GetHoveredAxis(Vector2 mousePos, Transform target, float handleScale, HandleSpace handleSpace);
        public abstract int GetHoveredAxisWithProfile(Vector2 mousePos, Transform target, float handleScale, HandleProfile profile);

        /// <summary>
        /// Calculates the screen-space distance to a linear handle (shaft + end cap).
        /// Used by both Translation and Scale handles.
        /// </summary>
        /// <param name="tipHitRadius">Radius in pixels for the end cap (cone/cube) to facilitate grabbing.</param>
        protected float CalculateDistanceToLinearHandle(Vector2 mousePos, Vector3 origin, Vector3 direction, float scale, float tipHitRadius)
        {
            Vector3 endPoint = origin + direction * scale;

            if (IsPointBehindCamera(origin) || IsPointBehindCamera(endPoint))
                return float.MaxValue;

            Vector3 originScreen = mainCamera.WorldToScreenPoint(origin);
            Vector3 endScreen = mainCamera.WorldToScreenPoint(endPoint);

            Vector2 originScreen2D = new Vector2(originScreen.x, originScreen.y);
            Vector2 endScreen2D = new Vector2(endScreen.x, endScreen.y);

            float distToLine = DistancePointToLineSegment(mousePos, originScreen2D, endScreen2D);
            float distToTip = Vector2.Distance(mousePos, endScreen2D);

            // Tip is treated the same as the shaft
            return Mathf.Min(distToLine, distToTip);
        }



        protected Vector3 GetAxisDirection(Transform target, int axisIndex, HandleSpace space)
        {
            switch (axisIndex)
            {
                case 0: return space == HandleSpace.Local ? target.right : Vector3.right;
                case 1: return space == HandleSpace.Local ? target.up : Vector3.up;
                case 2: return space == HandleSpace.Local ? target.forward : Vector3.forward;
                default: return Vector3.zero;
            }
        }

        protected float DistancePointToLineSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 v = b - a;
            float len = v.magnitude;
            if (len < 0.001f) return Vector2.Distance(p, a);
            float t = Mathf.Clamp01(Vector2.Dot(p - a, v) / (len * len));
            Vector2 proj = a + v * t;
            return Vector2.Distance(p, proj);
        }

        protected bool IsPointBehindCamera(Vector3 worldPoint)
        {
            Vector3 screenPoint = mainCamera.WorldToScreenPoint(worldPoint);
            return screenPoint.z < 0f;
        }
    }
}