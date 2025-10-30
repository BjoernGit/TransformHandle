using System.Collections.Generic;
using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Batched rendering system that collects all geometry and renders in minimal draw calls
    /// </summary>
    public class BatchedHandleRenderer
    {
        // Constants
        public const float LINE_THICKNESS_MULTIPLIER = 0.2f; // Controls visual thickness of lines (0.2 = 20% of calculated thickness)
        
        private Camera renderCamera;
        
        // Geometry buffers per primitive type
        private List<Vector3> lineVertices = new List<Vector3>();
        private List<Color> lineColors = new List<Color>();
        
        private List<Vector3> triangleVertices = new List<Vector3>();
        private List<Color> triangleColors = new List<Color>();
        
        private List<Vector3> quadVertices = new List<Vector3>();
        private List<Color> quadColors = new List<Color>();

        // Constructor with Camera (required)
        public BatchedHandleRenderer(Camera camera)
        {
            if (camera == null)
                Debug.LogWarning("BatchedHandleRenderer created without camera - won't render!");
            
            this.renderCamera = camera;
        }
        
        // SetCamera method
        public void SetCamera(Camera camera)
        {
            this.renderCamera = camera;
        }

        // GetCamera method
        public Camera GetCamera()
        {
            return renderCamera;
        }

        // Check if camera is valid
        public bool HasValidCamera()
        {
            return renderCamera != null && renderCamera;
        }
        
        // Get camera forward direction
        public Vector3 GetCameraForward()
        {
            if (renderCamera != null)
                return renderCamera.transform.forward;
            
            Debug.LogWarning("No camera set in BatchedHandleRenderer!");
            return Vector3.forward; // Fallback direction
        }

        /// <summary>
        /// Clear all buffers for new frame
        /// </summary>
        public void Clear()
        {
            lineVertices.Clear();
            lineColors.Clear();
            triangleVertices.Clear();
            triangleColors.Clear();
            quadVertices.Clear();
            quadColors.Clear();
        }

        /// <summary>
        /// Add a line to the batch
        /// </summary>
        public void AddLine(Vector3 start, Vector3 end, Color color)
        {
            lineVertices.Add(start);
            lineVertices.Add(end);
            lineColors.Add(color);
            lineColors.Add(color);
        }

        /// <summary>
        /// Add a thick line by adding multiple parallel lines
        /// </summary>
        public void AddThickLine(Vector3 start, Vector3 end, Color color, float thickness = 3f)
        {
            // No camera? Don't render!
            if (renderCamera == null) return;
            
            Vector3 direction = (end - start).normalized;
            Vector3 perpendicular = Vector3.Cross(direction, renderCamera.transform.forward).normalized;
            
            if (perpendicular.magnitude < 0.1f)
            {
                perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
            }
            
            // Calculate screen-space thickness
            Vector3 midPoint = (start + end) * 0.5f;
            float distanceToCamera = Vector3.Distance(renderCamera.transform.position, midPoint);
            
            // Convert thickness from pixels to world units based on camera distance
            float screenToWorldFactor = distanceToCamera * Mathf.Tan(renderCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * 2f / Screen.height * LINE_THICKNESS_MULTIPLIER;
            
            int lineCount = Mathf.Max(1, Mathf.RoundToInt(thickness * 0.3f)); // Reduziert von 1:1 auf 30%
            float step = thickness / lineCount;
            
            for (int i = 0; i < lineCount; i++)
            {
                float offset = (i - (lineCount - 1) * 0.5f) * step * screenToWorldFactor;
                Vector3 offsetVector = perpendicular * offset;
                
                AddLine(start + offsetVector, end + offsetVector, color);
            }
        }

        /// <summary>
        /// Add a triangle to the batch
        /// </summary>
        public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Color color)
        {
            triangleVertices.Add(v1);
            triangleVertices.Add(v2);
            triangleVertices.Add(v3);
            triangleColors.Add(color);
            triangleColors.Add(color);
            triangleColors.Add(color);
        }

        /// <summary>
        /// Add a quad to the batch
        /// </summary>
        public void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color color)
        {
            quadVertices.Add(v1);
            quadVertices.Add(v2);
            quadVertices.Add(v3);
            quadVertices.Add(v4);
            for (int i = 0; i < 4; i++)
            {
                quadColors.Add(color);
            }
        }

        /// <summary>
        /// Add a circle to the batch
        /// </summary>
        public void AddCircle(Vector3 center, Vector3 normal, float radius, Color color, 
                             int segments = 64, float thickness = 3f)
        {
            // No camera? Don't render!
            if (renderCamera == null) return;
            
            Vector3 tangent1 = Vector3.Cross(normal, Vector3.up).normalized;
            if (tangent1.magnitude < 0.1f)
            {
                tangent1 = Vector3.Cross(normal, Vector3.right).normalized;
            }
            Vector3 tangent2 = Vector3.Cross(normal, tangent1).normalized;
            
            // Calculate screen-space thickness for circles
            float distanceToCamera = Vector3.Distance(renderCamera.transform.position, center);
            float screenToWorldFactor = distanceToCamera * Mathf.Tan(renderCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * 2f / Screen.height * LINE_THICKNESS_MULTIPLIER;
            
            int lineCount = Mathf.Max(1, Mathf.RoundToInt(thickness * 0.3f)); // Auch hier reduziert
            float radiusStep = thickness * screenToWorldFactor / lineCount;
            
            for (int t = 0; t < lineCount; t++)
            {
                float currentRadius = radius + (t - (lineCount - 1) * 0.5f) * radiusStep;
                
                for (int i = 0; i < segments; i++)
                {
                    float angle1 = (i / (float)segments) * 2 * Mathf.PI;
                    float angle2 = ((i + 1) / (float)segments) * 2 * Mathf.PI;
                    
                    Vector3 point1 = center + (tangent1 * Mathf.Cos(angle1) + tangent2 * Mathf.Sin(angle1)) * currentRadius;
                    Vector3 point2 = center + (tangent1 * Mathf.Cos(angle2) + tangent2 * Mathf.Sin(angle2)) * currentRadius;
                    
                    AddLine(point1, point2, color);
                }
            }
        }

        /// <summary>
        /// Add an arrow head to the batch
        /// </summary>
        public void AddArrowHead(Vector3 tip, Vector3 direction, Color color, float size)
        {
            Vector3 perpendicular1 = Vector3.Cross(direction, Vector3.up).normalized;
            if (perpendicular1.sqrMagnitude < 0.1f)
                perpendicular1 = Vector3.Cross(direction, Vector3.right).normalized;
            Vector3 perpendicular2 = Vector3.Cross(direction, perpendicular1).normalized;

            Vector3 arrowBase = tip - direction * size;
            float baseSize = size * 0.4f;

            Vector3[] basePoints = new Vector3[]
            {
                arrowBase + perpendicular1 * baseSize,
                arrowBase - perpendicular1 * baseSize,
                arrowBase + perpendicular2 * baseSize,
                arrowBase - perpendicular2 * baseSize
            };

            // Add triangles for arrow head
            for (int i = 0; i < 4; i++)
            {
                int i1 = i;
                int i2 = (i + 1) % 4;
                AddTriangle(tip, basePoints[i1], basePoints[i2], color);
            }
            
            // Base quad
            AddTriangle(basePoints[0], basePoints[1], basePoints[2], color);
            AddTriangle(basePoints[1], basePoints[3], basePoints[2], color);

            // Outline
            Color outlineColor = color * 0.8f;
            for (int i = 0; i < 4; i++)
            {
                AddLine(tip, basePoints[i], outlineColor);
                AddLine(basePoints[i], basePoints[(i + 1) % 4], outlineColor);
            }
        }

        /// <summary>
        /// Add a box to the batch
        /// </summary>
        public void AddBox(Vector3 center, Vector3 forward, float size, Color color)
        {
            Vector3 up = Mathf.Abs(Vector3.Dot(forward, Vector3.up)) > 0.9f ? Vector3.forward : Vector3.up;
            Vector3 right = Vector3.Cross(forward, up).normalized;
            up = Vector3.Cross(right, forward).normalized;

            // Box corners
            Vector3[] corners = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                float x = (i & 1) == 0 ? -1 : 1;
                float y = (i & 2) == 0 ? -1 : 1;
                float z = (i & 4) == 0 ? -1 : 1;
                corners[i] = center + (right * x + up * y + forward * z) * size * 0.5f;
            }

            // Add faces as triangles
            // Front face
            AddTriangle(corners[0], corners[1], corners[2], color);
            AddTriangle(corners[1], corners[3], corners[2], color);
            
            // Back face
            AddTriangle(corners[5], corners[4], corners[6], color);
            AddTriangle(corners[5], corners[6], corners[7], color);
            
            // Other faces...
            AddTriangle(corners[4], corners[0], corners[2], color);
            AddTriangle(corners[4], corners[2], corners[6], color);
            
            AddTriangle(corners[1], corners[5], corners[7], color);
            AddTriangle(corners[1], corners[7], corners[3], color);
            
            AddTriangle(corners[4], corners[5], corners[1], color);
            AddTriangle(corners[4], corners[1], corners[0], color);
            
            AddTriangle(corners[2], corners[3], corners[7], color);
            AddTriangle(corners[2], corners[7], corners[6], color);

            // Add edges
            Color edgeColor = color * 0.8f;
            // Bottom face
            AddLine(corners[0], corners[1], edgeColor);
            AddLine(corners[1], corners[3], edgeColor);
            AddLine(corners[3], corners[2], edgeColor);
            AddLine(corners[2], corners[0], edgeColor);
            
            // Top face
            AddLine(corners[4], corners[5], edgeColor);
            AddLine(corners[5], corners[7], edgeColor);
            AddLine(corners[7], corners[6], edgeColor);
            AddLine(corners[6], corners[4], edgeColor);
            
            // Vertical edges
            AddLine(corners[0], corners[4], edgeColor);
            AddLine(corners[1], corners[5], edgeColor);
            AddLine(corners[2], corners[6], edgeColor);
            AddLine(corners[3], corners[7], edgeColor);
        }

        /// <summary>
        /// Render all batched geometry in minimal draw calls
        /// </summary>
        public void Render()
        {
            // Render all triangles in one batch
            if (triangleVertices.Count > 0)
            {
                GL.Begin(GL.TRIANGLES);
                for (int i = 0; i < triangleVertices.Count; i++)
                {
                    GL.Color(triangleColors[i]);
                    GL.Vertex(triangleVertices[i]);
                }
                GL.End();
            }

            // Render all quads in one batch
            if (quadVertices.Count > 0)
            {
                GL.Begin(GL.QUADS);
                for (int i = 0; i < quadVertices.Count; i++)
                {
                    GL.Color(quadColors[i]);
                    GL.Vertex(quadVertices[i]);
                }
                GL.End();
            }

            // Render all lines in one batch
            if (lineVertices.Count > 0)
            {
                GL.Begin(GL.LINES);
                for (int i = 0; i < lineVertices.Count; i++)
                {
                    GL.Color(lineColors[i]);
                    GL.Vertex(lineVertices[i]);
                }
                GL.End();
            }
        }
    }
}