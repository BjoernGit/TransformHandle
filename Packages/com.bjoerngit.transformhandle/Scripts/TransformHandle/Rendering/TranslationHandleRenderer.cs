using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Optimized Translation Handle Renderer that uses batching
    /// </summary>
    public class TranslationHandleRenderer : IProfileAwareRenderer
    {
        // Public constants
        public const float PLANE_SIZE_MULTIPLIER = 0.3f;

        // Alpha values
        private readonly float planeAlpha = 0.1f;
        private readonly float planeHoverAlpha = 0.3f;
        private readonly float axisAlpha = 0.8f;
        private readonly float selectedAlpha = 1f;
        
        // Sizes
        private readonly float baseThickness = 6f;
        private readonly float hoverThickness = 12f;

        // Batching system
        private BatchedHandleRenderer batcher;
        private readonly bool ownsBatcher;

        // Constructors
        public TranslationHandleRenderer(BatchedHandleRenderer sharedBatcher)
        {
            this.batcher = sharedBatcher;
            this.ownsBatcher = false; // owner clears and renders the batch
        }

        public TranslationHandleRenderer(Camera camera)
        {
            this.batcher = new BatchedHandleRenderer(camera);
            this.ownsBatcher = true;
        }

        public TranslationHandleRenderer()
        {
            Debug.LogWarning("TranslationHandleRenderer created without camera - won't render!");
            this.batcher = new BatchedHandleRenderer(null);
            this.ownsBatcher = true;
        }

        public void Render(Transform target, float scale, int hoveredAxis, HandleSpace handleSpace = HandleSpace.Local)
        {
            // Only clear if we own the batcher
            if (ownsBatcher)
                batcher.Clear();

            Vector3 position = target.position;

            // Collect all geometry first
            CollectPlanes(position, target, scale, hoveredAxis, handleSpace);
            CollectAxesInternal(position, target, scale, hoveredAxis, (axis, checkSpace) => checkSpace == handleSpace);
            CollectCenterPoint(position, scale * 0.1f);

            // Only render if we own the batcher
            if (ownsBatcher)
                batcher.Render();
        }

        public void RenderWithProfile(Transform target, float scale, int hoveredAxis, HandleProfile profile)
        {
            // Only clear if we own the batcher
            if (ownsBatcher)
                batcher.Clear();

            Vector3 position = target.position;

            // Collect all geometry
            CollectPlanesWithProfile(target, position, scale, hoveredAxis, profile);
            CollectAxesInternal(position, target, scale, hoveredAxis,
                (axis, space) => profile.IsAxisEnabled(HandleType.Translation, axis, space));
            CollectCenterPoint(position, scale * 0.1f);

            // Only render if we own the batcher
            if (ownsBatcher)
                batcher.Render();
        }

        private void CollectAxesInternal(Vector3 position, Transform target, float scale, int hoveredAxis,
                                       System.Func<int, HandleSpace, bool> shouldRenderAxis)
        {
            for (int axis = 0; axis < 3; axis++)
            {
                foreach (HandleSpace space in System.Enum.GetValues(typeof(HandleSpace)))
                {
                    if (shouldRenderAxis(axis, space))
                    {
                        Vector3 direction = TranslationHandleUtils.GetAxisDirection(target, axis, space);
                        Color color = TranslationHandleUtils.GetAxisColor(axis);
                        CollectAxis(position, direction, color, scale, axis, hoveredAxis);
                    }
                }
            }
        }

        private void CollectPlanes(Vector3 position, Transform target, float scale, int hoveredAxis, HandleSpace space)
        {
            CollectPlanesInternal(position, target, scale, hoveredAxis, (planeIndex, checkSpace) => checkSpace == space);
        }

        private void CollectPlanesWithProfile(Transform target, Vector3 position, float scale,
                                           int hoveredAxis, HandleProfile profile)
        {
            CollectPlanesInternal(position, target, scale, hoveredAxis, 
                (planeIndex, space) => profile.IsAxisEnabled(HandleType.Translation, planeIndex, space));
        }

        private void CollectPlanesInternal(Vector3 position, Transform target, float scale, int hoveredAxis, 
                                         System.Func<int, HandleSpace, bool> shouldRenderPlane)
        {
            float size = scale * PLANE_SIZE_MULTIPLIER;
            
            // Get camera from batcher's render camera!
            if (!batcher.HasValidCamera()) return;
            
            Vector3 camForward = batcher.GetCameraForward();

            for (int planeIndex = 4; planeIndex <= 6; planeIndex++)
            {
                foreach (HandleSpace space in System.Enum.GetValues(typeof(HandleSpace)))
                {
                    if (shouldRenderPlane(planeIndex, space))
                    {
                        var (axis1, axis2) = TranslationHandleUtils.GetPlaneAxes(target, planeIndex, space);
                        Vector3 offset = TranslationHandleUtils.CalculatePlaneOffset(axis1, axis2, size, camForward);
                        Color color = TranslationHandleUtils.GetAxisColor(planeIndex);
                        
                        CollectPlane(position + offset, axis1, axis2, color, size, planeIndex, hoveredAxis);
                    }
                }
            }
        }

        private void CollectAxis(Vector3 origin, Vector3 direction, Color color, float length,
                              int axisIndex, int hoveredAxis)
        {
            float alpha = (hoveredAxis == axisIndex) ? selectedAlpha : axisAlpha;
            Color finalColor = new Color(color.r, color.g, color.b, alpha);

            Vector3 endPoint = origin + direction * length;

            float thickness = (hoveredAxis == axisIndex) ? hoverThickness : baseThickness;
            
            // Add thick line to batch
            batcher.AddThickLine(origin, endPoint, finalColor, thickness);

            // Add arrow head
            batcher.AddArrowHead(endPoint, direction, finalColor, length * 0.2f);
        }

        private void CollectPlane(Vector3 center, Vector3 axis1, Vector3 axis2, Color color, 
                             float size, int planeIndex, int hoveredAxis)
        {
            bool isHovered = (hoveredAxis == planeIndex);
            float alpha = isHovered ? planeHoverAlpha : planeAlpha;

            Color fillColor = new Color(color.r, color.g, color.b, alpha);
            Color outlineColor = new Color(color.r, color.g, color.b, alpha * 2f);

            // Calculate corners
            Vector3[] corners = new Vector3[4];
            corners[0] = center;
            corners[1] = center - axis1 * size;
            corners[2] = center - axis1 * size - axis2 * size;
            corners[3] = center - axis2 * size;

            // Add filled quad
            batcher.AddQuad(corners[0], corners[1], corners[2], corners[3], fillColor);

            // Add outline
            batcher.AddLine(corners[0], corners[1], outlineColor);
            batcher.AddLine(corners[1], corners[2], outlineColor);
            batcher.AddLine(corners[2], corners[3], outlineColor);
            batcher.AddLine(corners[3], corners[0], outlineColor);
        }

        private void CollectCenterPoint(Vector3 center, float size)
        {
            Color color = new Color(1f, 1f, 1f, 0.5f);
            batcher.AddLine(center + Vector3.right * size, center - Vector3.right * size, color);
            batcher.AddLine(center + Vector3.up * size, center - Vector3.up * size, color);
            batcher.AddLine(center + Vector3.forward * size, center - Vector3.forward * size, color);
        }
    }
}