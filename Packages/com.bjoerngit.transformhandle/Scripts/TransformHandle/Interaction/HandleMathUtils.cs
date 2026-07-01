using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Shared math utilities for handle interaction.
    /// </summary>
    public static class HandleMathUtils
    {
        /// <summary>
        /// Projects the closest point between a ray and a line onto the line and
        /// returns its signed distance from <paramref name="lineOrigin"/> along
        /// <paramref name="lineDirection"/>.
        /// Works entirely in world space, so it is immune to camera distance,
        /// viewport rect / split-screen setups and view angle.
        /// Returns false when the ray is nearly parallel to the line.
        /// </summary>
        /// <param name="ray">Ray (e.g. mouse ray from ScreenPointToRay).</param>
        /// <param name="lineOrigin">A point on the line.</param>
        /// <param name="lineDirection">Line direction (unit length).</param>
        /// <param name="offset">Signed distance of the closest point along the line.</param>
        public static bool TryGetRayLineOffset(Ray ray, Vector3 lineOrigin, Vector3 lineDirection, out float offset)
        {
            offset = 0f;

            Vector3 d = lineDirection;      // line direction (unit length)
            Vector3 r = ray.direction;      // ray direction (unit length)
            Vector3 w0 = lineOrigin - ray.origin;

            float b = Vector3.Dot(d, r);
            float denom = 1f - b * b;       // = (d·d)(r·r) - (d·r)^2, with unit vectors

            if (denom < 1e-6f)
                return false;               // ray parallel to line -> undefined

            float dW0 = Vector3.Dot(d, w0);
            float rW0 = Vector3.Dot(r, w0);

            // Scalar along the line for the closest point to the ray.
            offset = (b * rW0 - dW0) / denom;
            return true;
        }
    }
}
