using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Optimized Scale Handle Renderer that uses batching
    /// </summary>
    public class ScaleHandleRenderer : IProfileAwareRenderer
    {
        private readonly Color centerColor = Color.white;
        private readonly float axisAlpha = 0.8f;
        private readonly float selectedAlpha = 1f;
        private readonly float baseThickness = 6f;
        private readonly float hoverThickness = 12f;
        private readonly float boxSize = 0.09f;

        // Batching system
        private BatchedHandleRenderer batcher;

        // Constructors
        public ScaleHandleRenderer(BatchedHandleRenderer sharedBatcher)
        {
            this.batcher = sharedBatcher;
        }

        public ScaleHandleRenderer(Camera camera)
        {
            this.batcher = new BatchedHandleRenderer(camera);
        }

        public ScaleHandleRenderer()
        {
            Debug.LogWarning("ScaleHandleRenderer created without camera - won't render!");
            this.batcher = new BatchedHandleRenderer(null);
        }

        public void Render(Transform target, float scale, int hoveredAxis, HandleSpace handleSpace = HandleSpace.Local)
        {
            // Only clear if we own the batcher
            if (batcher != null && batcher.GetHashCode() == this.batcher.GetHashCode())
                batcher.Clear();
            
            Vector3 position = target.position;

            // For scale, we always use local axes regardless of handle space setting
            Vector3 dirX = target.right;
            Vector3 dirY = target.up;
            Vector3 dirZ = target.forward;

            // Collect axis lines and boxes
            CollectScaleAxis(position, dirX, TranslationHandleUtils.GetAxisColor(0), scale, 0, hoveredAxis);
            CollectScaleAxis(position, dirY, TranslationHandleUtils.GetAxisColor(1), scale, 1, hoveredAxis);
            CollectScaleAxis(position, dirZ, TranslationHandleUtils.GetAxisColor(2), scale, 2, hoveredAxis);

            // Collect center handle for uniform scaling (axis index 3)
            CollectCenterHandle(position, scale * boxSize * 1.5f, hoveredAxis == 3);

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

            // Collect each axis based on profile settings
            for (int axis = 0; axis < 3; axis++)
            {
                Color color = TranslationHandleUtils.GetAxisColor(axis);

                foreach (HandleSpace space in System.Enum.GetValues(typeof(HandleSpace)))
                {
                    if (profile.IsAxisEnabled(HandleType.Scale, axis, space))
                    {
                        Vector3 direction = TranslationHandleUtils.GetAxisDirection(target, axis, space);
                        CollectScaleAxis(position, direction, color, scale, axis, hoveredAxis);
                    }
                }
            }

            // Uniform scale handle (axis index 3)
            if (profile.IsAxisEnabled(HandleType.Scale, 3, HandleSpace.Local) || 
                profile.IsAxisEnabled(HandleType.Scale, 3, HandleSpace.Global))
            {
                CollectCenterHandle(position, scale * boxSize * 1.5f, hoveredAxis == 3);
            }

            // Only render if we own the batcher
            if (batcher != null && batcher.GetHashCode() == this.batcher.GetHashCode())
                batcher.Render();
        }

        private void CollectScaleAxis(Vector3 origin, Vector3 direction, Color color, float length,
                                    int axisIndex, int hoveredAxis)
        {
            float alpha = (hoveredAxis == axisIndex) ? selectedAlpha : axisAlpha;
            Color finalColor = new Color(color.r, color.g, color.b, alpha);

            Vector3 endPoint = origin + direction * length;

            // Collect line
            float thickness = (hoveredAxis == axisIndex) ? hoverThickness : baseThickness;
            batcher.AddThickLine(origin, endPoint, finalColor, thickness);

            // Collect box at end
            float currentBoxSize = length * boxSize;
            if (hoveredAxis == axisIndex) currentBoxSize *= 1.3f; // Bigger when hovered
            
            batcher.AddBox(endPoint, direction, currentBoxSize, finalColor);
        }

        private void CollectCenterHandle(Vector3 center, float size, bool isHovered)
        {
            Color color = new Color(centerColor.r, centerColor.g, centerColor.b, 
                                   isHovered ? selectedAlpha : axisAlpha);

            // Collect a special center box that's always camera-facing
            Camera cam = batcher?.GetCamera();
            if (cam == null) return; //no cam protection
            Vector3 forward = (cam.transform.position - center).normalized;

            batcher.AddBox(center, forward, size, color);
            
            // Collect connecting lines to make it clear it's the center
            float lineSize = size * 2f;
            Color lineColor = new Color(color.r, color.g, color.b, color.a * 0.5f);
            
            batcher.AddLine(center - Vector3.right * lineSize, center + Vector3.right * lineSize, lineColor);
            batcher.AddLine(center - Vector3.up * lineSize, center + Vector3.up * lineSize, lineColor);
            batcher.AddLine(center - Vector3.forward * lineSize, center + Vector3.forward * lineSize, lineColor);
        }
    }
}