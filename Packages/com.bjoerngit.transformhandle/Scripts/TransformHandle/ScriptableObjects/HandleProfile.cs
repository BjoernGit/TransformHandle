using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// ScriptableObject that defines handle configuration for transform manipulation
    /// </summary>
    [CreateAssetMenu(fileName = "HandleProfile", menuName = "MeshFreeHandles/Handle Profile")]
    public class HandleProfile : ScriptableObject
    {
        [Header("Translation Handles")]
        [SerializeField] private bool showTranslationLocalX = true;
        [SerializeField] private bool showTranslationLocalY = true;
        [SerializeField] private bool showTranslationLocalZ = true;
        [SerializeField] private bool showTranslationGlobalX = false;
        [SerializeField] private bool showTranslationGlobalY = false;
        [SerializeField] private bool showTranslationGlobalZ = false;

        [Header("Translation Plane Handles")]
        [SerializeField] private bool showTranslationLocalXY = false;
        [SerializeField] private bool showTranslationLocalXZ = false;
        [SerializeField] private bool showTranslationLocalYZ = false;
        [SerializeField] private bool showTranslationGlobalXY = false;
        [SerializeField] private bool showTranslationGlobalXZ = false;
        [SerializeField] private bool showTranslationGlobalYZ = false;

        [Header("Rotation Handles")]
        [SerializeField] private bool showRotationLocalX = true;
        [SerializeField] private bool showRotationLocalY = false;
        [SerializeField] private bool showRotationLocalZ = true;
        [SerializeField] private bool showRotationGlobalX = false;
        [SerializeField] private bool showRotationGlobalY = true;
        [SerializeField] private bool showRotationGlobalZ = false;
        [SerializeField] private bool showFreeRotation = false;      // Roll ring facing the camera
        [SerializeField] private bool showTrackballRotation = false; // Free drag area inside the ring

        [Tooltip("Draws rotation circles as complete ellipses instead of hiding the half that faces away " +
                 "from the camera. The hidden half also becomes interactive. Useful when only a single " +
                 "rotation axis is enabled.")]
        [SerializeField] private bool showFullRotationCircles = false;

        /// <summary>
        /// When enabled, rotation circles are drawn and hit-tested in full,
        /// including the part facing away from the camera.
        /// </summary>
        public bool ShowFullRotationCircles => showFullRotationCircles;

        [Header("Scale Handles")]
        [SerializeField] private bool showScaleLocalX = true;
        [SerializeField] private bool showScaleLocalY = true;
        [SerializeField] private bool showScaleLocalZ = true;
        [SerializeField] private bool showScaleGlobalX = false;
        [SerializeField] private bool showScaleGlobalY = false;
        [SerializeField] private bool showScaleGlobalZ = false;
        [SerializeField] private bool showUniformScale = true;

        /// <summary>
        /// Checks if a specific axis should be shown for the given handle type and space
        /// </summary>
        public bool IsAxisEnabled(HandleType handleType, int axis, HandleSpace space)
        {
            switch (handleType)
            {
                case HandleType.Translation:
                    if (space == HandleSpace.Local)
                    {
                        switch (axis)
                        {
                            case 0: return showTranslationLocalX;
                            case 1: return showTranslationLocalY;
                            case 2: return showTranslationLocalZ;
                            case 4: return showTranslationLocalXY;  // XY Plane
                            case 5: return showTranslationLocalXZ;  // XZ Plane
                            case 6: return showTranslationLocalYZ;  // YZ Plane
                        }
                    }
                    else // Global
                    {
                        switch (axis)
                        {
                            case 0: return showTranslationGlobalX;
                            case 1: return showTranslationGlobalY;
                            case 2: return showTranslationGlobalZ;
                            case 4: return showTranslationGlobalXY;  // XY Plane
                            case 5: return showTranslationGlobalXZ;  // XZ Plane
                            case 6: return showTranslationGlobalYZ;  // YZ Plane
                        }
                    }
                    break;

                case HandleType.Rotation:
                    // Roll ring and trackball are camera-space operations and therefore space-agnostic
                    if (axis == 3) return showFreeRotation;
                    if (axis == 7) return showTrackballRotation;

                    if (space == HandleSpace.Local)
                    {
                        switch (axis)
                        {
                            case 0: return showRotationLocalX;
                            case 1: return showRotationLocalY;
                            case 2: return showRotationLocalZ;
                        }
                    }
                    else // Global
                    {
                        switch (axis)
                        {
                            case 0: return showRotationGlobalX;
                            case 1: return showRotationGlobalY;
                            case 2: return showRotationGlobalZ;
                        }
                    }
                    break;

                case HandleType.Scale:
                    if (space == HandleSpace.Local)
                    {
                        switch (axis)
                        {
                            case 0: return showScaleLocalX;
                            case 1: return showScaleLocalY;
                            case 2: return showScaleLocalZ;
                            case 3: return showUniformScale;
                        }
                    }
                    else // Global
                    {
                        switch (axis)
                        {
                            case 0: return showScaleGlobalX;
                            case 1: return showScaleGlobalY;
                            case 2: return showScaleGlobalZ;
                            case 3: return showUniformScale;
                        }
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// Gets if any axis is enabled for a given handle type in any space
        /// </summary>
        public bool HasAnyAxisEnabled(HandleType handleType)
        {
            switch (handleType)
            {
                case HandleType.Translation:
                    return showTranslationLocalX || showTranslationLocalY || showTranslationLocalZ ||
                           showTranslationGlobalX || showTranslationGlobalY || showTranslationGlobalZ ||
                           showTranslationLocalXY || showTranslationLocalXZ || showTranslationLocalYZ ||
                           showTranslationGlobalXY || showTranslationGlobalXZ || showTranslationGlobalYZ;

                case HandleType.Rotation:
                    return showRotationLocalX || showRotationLocalY || showRotationLocalZ ||
                           showRotationGlobalX || showRotationGlobalY || showRotationGlobalZ ||
                           showFreeRotation || showTrackballRotation;

                case HandleType.Scale:
                    return showScaleLocalX || showScaleLocalY || showScaleLocalZ ||
                           showScaleGlobalX || showScaleGlobalY || showScaleGlobalZ || showUniformScale;
            }
            return false;
        }

        /// <summary>
        /// Creates a default profile with standard local handles enabled
        /// </summary>
        public static HandleProfile CreateDefault()
        {
            var profile = CreateInstance<HandleProfile>();
            profile.name = "Default Handle Profile";
            return profile;
        }
    }
}