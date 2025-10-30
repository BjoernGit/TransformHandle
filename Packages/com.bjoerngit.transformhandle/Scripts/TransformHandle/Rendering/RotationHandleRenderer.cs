using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Optimized Rotation Handle Renderer that uses batching
    /// </summary>
    public class RotationHandleRenderer : IProfileAwareRenderer
    {
        private readonly float axisAlpha = 0.8f;
        private readonly float selectedAlpha = 1f;
        private readonly int circleSegments = 64;
        private readonly float baseThickness = 6f;
        private readonly float hoverThickness = 12f;

        // Batching system
        private BatchedHandleRenderer batcher;

        // Constructors
        public RotationHandleRenderer(BatchedHandleRenderer sharedBatcher)
        {
            this.batcher = sharedBatcher;
        }

        public RotationHandleRenderer(Camera camera)
        {
            this.batcher = new BatchedHandleRenderer(camera);
        }

        public RotationHandleRenderer()
        {
            Debug.LogWarning("RotationHandleRenderer created without camera - won't render!");
            this.batcher = new BatchedHandleRenderer(null);
        }

        public void Render(Transform target, float scale, int hoveredAxis, HandleSpace handleSpace = HandleSpace.Local)
        {
            // Only clear if we own the batcher
            if (batcher != null && batcher.GetHashCode() == this.batcher.GetHashCode())
                batcher.Clear();
            
            Vector3 position = target.position;
            Camera camera = batcher?.GetCamera();
            if (camera == null) return; // no cam protection

            Vector3 dirX = (handleSpace == HandleSpace.Local) ? target.right   : Vector3.right;
            Vector3 dirY = (handleSpace == HandleSpace.Local) ? target.up      : Vector3.up;
            Vector3 dirZ = (handleSpace == HandleSpace.Local) ? target.forward : Vector3.forward;

            // Collect all circles
            CollectRotationCircle(position, dirX, TranslationHandleUtils.GetAxisColor(0), scale, 0, hoveredAxis, camera);
            CollectRotationCircle(position, dirY, TranslationHandleUtils.GetAxisColor(1), scale, 1, hoveredAxis, camera);
            CollectRotationCircle(position, dirZ, TranslationHandleUtils.GetAxisColor(2), scale, 2, hoveredAxis, camera);

            // Free rotation sphere (axis index 3)
            CollectCameraFacingCircle(position, scale * 1.2f, camera, hoveredAxis == 3);

            // Only render if we own the batcher
            if (batcher != null && batcher.GetHashCode() == this.batcher.GetHashCode())
                batcher.Render();
        }

        public void RenderWithProfile(Transform target, float scale, int hoveredAxis, HandleProfile profile)
        {
            // Only clear if we own the batcher
            if (batcher != null && batcher.GetHashCode() == this.batcher.GetHashCode())
                batcher.Clear();
            
            Vector3 position = target.position;
            Camera camera = batcher?.GetCamera();
            if (camera == null) return; //no cam protection

            // Collect circles based on profile
            for (int axis = 0; axis < 3; axis++)
            {
                Color color = TranslationHandleUtils.GetAxisColor(axis);

                foreach (HandleSpace space in System.Enum.GetValues(typeof(HandleSpace)))
                {
                    if (profile.IsAxisEnabled(HandleType.Rotation, axis, space))
                    {
                        Vector3 normal = TranslationHandleUtils.GetAxisDirection(target, axis, space);
                        CollectRotationCircle(position, normal, color, scale, axis, hoveredAxis, camera);
                    }
                }
            }

            // Free rotation sphere (axis index 3)
            if (profile.IsAxisEnabled(HandleType.Rotation, 3, HandleSpace.Local) ||
                profile.IsAxisEnabled(HandleType.Rotation, 3, HandleSpace.Global))
            {
                CollectCameraFacingCircle(position, scale * 1.2f, camera, hoveredAxis == 3);
            }

            // Only render if we own the batcher
            if (batcher != null && batcher.GetHashCode() == this.batcher.GetHashCode())
                batcher.Render();
        }

        private void CollectRotationCircle(Vector3 center, Vector3 normal, Color color, float radius,
                                         int axisIndex, int hoveredAxis, Camera camera)
        {
            if (camera == null) return; //no cam protection

            float alpha = (hoveredAxis == axisIndex) ? selectedAlpha : axisAlpha;
            Color baseColor = new Color(color.r, color.g, color.b, alpha);

            // Skip if circle is edge-on
            Vector3 toCamera = (camera.transform.position - center).normalized;
            float edgeDot = Mathf.Abs(Vector3.Dot(normal, toCamera));
            if (edgeDot > 0.98f)
                return;

            // Determine tangent basis
            Vector3 tangent1 = Vector3.Cross(normal, Vector3.up).normalized;
            if (tangent1.sqrMagnitude < 0.1f)
                tangent1 = Vector3.Cross(normal, Vector3.right).normalized;
            Vector3 tangent2 = Vector3.Cross(normal, tangent1).normalized;

            float thickness = (hoveredAxis == axisIndex) ? hoverThickness : baseThickness;

            // Collect all circle segments at once
            for (int i = 0; i < circleSegments; i++)
            {
                float angleA = (i / (float)circleSegments) * 2f * Mathf.PI;
                float angleB = ((i + 1) / (float)circleSegments) * 2f * Mathf.PI;

                Vector3 pA = center + (tangent1 * Mathf.Cos(angleA) + tangent2 * Mathf.Sin(angleA)) * radius;
                Vector3 pB = center + (tangent1 * Mathf.Cos(angleB) + tangent2 * Mathf.Sin(angleB)) * radius;

                // Visibility check
                Vector3 mid = (pA + pB) * 0.5f;
                float dotMid = Vector3.Dot((mid - center).normalized, toCamera);
                if (dotMid < -0.1f)
                    continue;

                // Fade based on angle to camera
                float fade = Mathf.Clamp01((dotMid + 0.1f) / 0.2f);
                Color segmentColor = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * fade);
                
                // Add to batch
                batcher.AddThickLine(pA, pB, segmentColor, thickness);
            }
        }

        private void CollectCameraFacingCircle(Vector3 center, float radius, Camera camera, bool isHovered)
        {
            Vector3 normal = (camera.transform.position - center).normalized;
            
            // Dünnerer Ring, aber dicker wenn gehovered
            float thickness = isHovered ? baseThickness : baseThickness * 0.5f;
            
            // Mehr opak wenn gehovered
            float alpha = isHovered ? 0.6f : 0.2f;
            Color color = new Color(1f, 1f, 1f, alpha);
            
            // Use the batched circle method
            batcher.AddCircle(center, normal, radius, color, circleSegments, thickness);
        }
    }
}