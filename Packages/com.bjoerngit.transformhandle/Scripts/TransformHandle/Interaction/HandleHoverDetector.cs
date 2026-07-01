using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Coordinates hover detection by delegating to specialized detectors
    /// </summary>
    public class HandleHoverDetector
    {
        private Camera mainCamera;
        private TranslationHoverDetector translationDetector;
        private RotationHoverDetector rotationDetector;
        private ScaleHoverDetector scaleDetector;

        public HandleHoverDetector(Camera camera)
        {
            mainCamera = camera;
            translationDetector = new TranslationHoverDetector(camera);
            rotationDetector = new RotationHoverDetector(camera);
            scaleDetector = new ScaleHoverDetector(camera);
        }

        /// <summary>
        /// Returns the index of the hovered axis, or -1 if none.
        /// </summary>
        public int GetHoveredAxis(Vector2 mousePos, Transform target, float handleScale, HandleType handleType, HandleSpace handleSpace)
        {
            if (!IsValidForDetection(target))
                return -1;

            switch (handleType)
            {
                case HandleType.Translation:
                    return translationDetector.GetHoveredAxis(mousePos, target, handleScale, handleSpace);
                case HandleType.Rotation:
                    return rotationDetector.GetHoveredAxis(mousePos, target, handleScale, handleSpace);
                case HandleType.Scale:
                    return scaleDetector.GetHoveredAxis(mousePos, target, handleScale, handleSpace);
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Returns the index of the hovered axis using a profile for mixed-space support.
        /// Also reports which space the hovered element belongs to.
        /// </summary>
        public int GetHoveredAxisWithProfile(Vector2 mousePos, Transform target, float handleScale, HandleType handleType, HandleProfile profile, out HandleSpace hoveredSpace)
        {
            hoveredSpace = HandleSpace.Local;

            if (!IsValidForDetection(target) || profile == null)
                return -1;

            switch (handleType)
            {
                case HandleType.Translation:
                    return translationDetector.GetHoveredAxisWithProfile(mousePos, target, handleScale, profile, out hoveredSpace);
                case HandleType.Rotation:
                    return rotationDetector.GetHoveredAxisWithProfile(mousePos, target, handleScale, profile, out hoveredSpace);
                case HandleType.Scale:
                    return scaleDetector.GetHoveredAxisWithProfile(mousePos, target, handleScale, profile, out hoveredSpace);
                default:
                    return -1;
            }
        }

        private bool IsValidForDetection(Transform target)
        {
            if (target == null || mainCamera == null)
                return false;

            Vector3 screenPos = mainCamera.WorldToScreenPoint(target.position);
            return screenPos.z > 0f; // Not behind camera
        }
    }
}