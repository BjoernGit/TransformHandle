using UnityEngine;
using System;

namespace MeshFreeHandles
{
    /// <summary>
    /// Singleton manager that controls transform handles for any selected object.
    /// </summary>
    public class TransformHandleManager : MonoBehaviour
    {
        private static TransformHandleManager instance;
        private int lastHoveredAxis = -1;

        // Effective camera currently used by interaction/renderer
        private Camera activeCamera;

        // Cached target state for change detection (OnTransformModified)
        private Vector3 lastTargetPosition;
        private Quaternion lastTargetRotation;
        private Vector3 lastTargetScale;

        // Components
        private HandleInteraction interaction;
        private HandleRenderer handleRenderer;
        private HandleProfile activeProfile;

        [Header("Target")]
        [Tooltip("The transform to manipulate with the handles.")]
        [SerializeField] private Transform targetTransform;

        /// <summary>
        /// Global instance of the Transform Handle Manager
        /// </summary>
        public static TransformHandleManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<TransformHandleManager>();

                    if (instance == null)
                    {
                        GameObject go = new GameObject("Transform Handle Manager");
                        instance = go.AddComponent<TransformHandleManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        [Header("Handle Settings")]
        [Tooltip("Maintain a constant on-screen size regardless of distance.")]
        public bool maintainConstantScreenSize = true;
        [Tooltip("Base size of the handles. Scales with distance if Maintain Constant Screen Size is enabled.")]
        public float handleSize = 1f;
        [Tooltip("Multiplier applied when maintaining constant screen size.")]
        public float screenSizeMultiplier = 0.1f;

        [Header("State")]
        [SerializeField] private HandleType handleType = HandleType.Translation;
        [SerializeField] private HandleSpace handleSpace = HandleSpace.Local;



        // Events
        public event Action<Transform> OnTargetChanged;
        public event Action<HandleType> OnHandleTypeChanged;
        public event Action<HandleSpace> OnHandleSpaceChanged;
        public event Action<Transform> OnTransformModified;
        public event Action<int> OnHoverEnter;
        public event Action<int> OnHoverExit;

        // Properties

        [Header("Camera Settings")]
        [Tooltip("The camera used for handle rendering and interaction. If null, Camera.main is used.")]
        [SerializeField] private Camera handleCamera;
        /// <summary>
        /// Gets or sets the camera used for handles. Falls back to Camera.main if null.
        /// </summary>
        public Camera HandleCamera
        {
            get
            {
                if (handleCamera != null && !handleCamera)
                    handleCamera = null;

                return handleCamera ?? Camera.main;
            }
            set
            {
                if (handleCamera != value)
                {
                    handleCamera = value;
                    RefreshCameraReferences();
                }
            }
        }

        public bool HandlesEnabled { get; private set; } = true;

        /// <summary>Allow or block handle interacition</summary>
        public void SetHandlesEnabled(bool enabled)
        {
            if (HandlesEnabled == enabled) return;
            HandlesEnabled = enabled;

            if (!enabled && lastHoveredAxis != -1)
            {
                OnHoverExit?.Invoke(lastHoveredAxis);
                lastHoveredAxis = -1;
            }
        }

        public Transform CurrentTarget => targetTransform;
        public HandleType CurrentHandleType => handleType;
        public HandleSpace CurrentHandleSpace => handleSpace;
        public bool IsActive => targetTransform != null;
        public bool IsDragging => interaction?.IsDragging ?? false;
        public bool IsHovering => interaction?.HoveredAxis != -1;
        public int HoveredAxis => interaction?.HoveredAxis ?? -1;

        void Awake()
        {
            // Singleton enforcement
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize components with the effective camera
            RefreshCameraReferences();
        }

        void Update()
        {
            // Keep interaction/renderer in sync with the effective camera
            // (explicit camera, Camera.main fallback, or destroyed camera).
            RefreshCameraReferences();

            if (!HandlesEnabled) return;

            if (targetTransform == null || activeCamera == null) return;

            // Update interaction target every frame
            interaction.UpdateTarget(targetTransform);

            // Compute scale
            float scale = GetHandleScale();

            // Pass profile to interaction if available
            if (activeProfile != null)
            {
                // Profile mode: Pass the profile for mixed space handling
                interaction.UpdateWithProfile(scale, handleType, activeProfile);
            }
            else
            {
                // Default mode: Use manager's handleType and handleSpace
                interaction.Update(scale, handleType, handleSpace);
            }

            int currentHovered = HoveredAxis;
            if (currentHovered != lastHoveredAxis)
            {
                if (lastHoveredAxis != -1)
                    OnHoverExit?.Invoke(lastHoveredAxis);

                if (currentHovered != -1)
                    OnHoverEnter?.Invoke(currentHovered);

                lastHoveredAxis = currentHovered;
            }

            // Fire modification event only when the transform actually changed
            if (interaction.IsDragging && HasTargetChanged())
            {
                OnTransformModified?.Invoke(targetTransform);
            }
            CacheTargetState();
        }

        /// <summary>
        /// Recreates interaction/renderer when the effective camera changed
        /// (explicit camera assigned, Camera.main fallback switched, or camera destroyed).
        /// </summary>
        private void RefreshCameraReferences()
        {
            Camera cam = HandleCamera;
            if (cam == activeCamera && interaction != null) return;

            activeCamera = cam;
            interaction = new HandleInteraction(cam);
            handleRenderer?.Cleanup();
            handleRenderer = new HandleRenderer(cam);
        }

        private bool HasTargetChanged()
        {
            return targetTransform.position != lastTargetPosition
                || targetTransform.rotation != lastTargetRotation
                || targetTransform.localScale != lastTargetScale;
        }

        private void CacheTargetState()
        {
            lastTargetPosition = targetTransform.position;
            lastTargetRotation = targetTransform.rotation;
            lastTargetScale = targetTransform.localScale;
        }

        /// <summary>
        /// Sets the target transform to manipulate
        /// </summary>
        public void SetTarget(Transform target)
        {
            if (targetTransform == target) return;

            if (lastHoveredAxis != -1)
                OnHoverExit?.Invoke(lastHoveredAxis);
            lastHoveredAxis = -1;

            targetTransform = target;

            // Check for custom profile on target
            if (target != null)
            {
                var profileHolder = target.GetComponent<HandleProfileHolder>();
                activeProfile = (profileHolder != null && profileHolder.HasProfile)
                    ? profileHolder.Profile
                    : null;
            }
            else
            {
                activeProfile = null;
            }

            OnTargetChanged?.Invoke(target);
        }

        /// <summary>
        /// Clears the current target
        /// </summary>
        public void ClearTarget()
        {
            SetTarget(null);
        }

        /// <summary>
        /// Refreshes the profile from the current target
        /// </summary>
        public void RefreshProfile()
        {
            if (targetTransform != null)
            {
                var profileHolder = targetTransform.GetComponent<HandleProfileHolder>();
                activeProfile = (profileHolder != null && profileHolder.HasProfile)
                    ? profileHolder.Profile
                    : null;
            }
        }

        /// <summary>
        /// Sets the handle mode to Translation.
        /// </summary>
        public void SetTranslationMode()
        {
            if (handleType != HandleType.Translation)
            {
                handleType = HandleType.Translation;
                OnHandleTypeChanged?.Invoke(handleType);
            }
        }

        /// <summary>
        /// Sets the handle mode to Rotation.
        /// </summary>
        public void SetRotationMode()
        {
            if (handleType != HandleType.Rotation)
            {
                handleType = HandleType.Rotation;
                OnHandleTypeChanged?.Invoke(handleType);
            }
        }

        /// <summary>
        /// Sets the handle mode to Scale.
        /// </summary>
        public void SetScaleMode()
        {
            if (handleType != HandleType.Scale)
            {
                handleType = HandleType.Scale;
                OnHandleTypeChanged?.Invoke(handleType);
            }
        }

        /// <summary>
        /// Toggles between Local and Global axis space.
        /// Only works when no profile is active on the current target.
        /// </summary>
        public void ToggleHandleSpace()
        {
            // Don't allow space toggle when using a profile
            if (activeProfile != null) return;

            handleSpace = (handleSpace == HandleSpace.Local)
                ? HandleSpace.Global
                : HandleSpace.Local;

            OnHandleSpaceChanged?.Invoke(handleSpace);
        }

        /// <summary>
        /// Sets the handle space directly.
        /// Only works when no profile is active on the current target.
        /// </summary>
        public void SetHandleSpace(HandleSpace space)
        {
            // Don't allow space change when using a profile
            if (activeProfile != null) return;

            if (handleSpace != space)
            {
                handleSpace = space;
                OnHandleSpaceChanged?.Invoke(handleSpace);
            }
        }

        void OnRenderObject()
        {
            if (targetTransform == null || HandleCamera == null || handleRenderer == null) return;

#if UNITY_EDITOR
            if (UnityEditor.SceneView.currentDrawingSceneView != null) return;
#endif


            Vector3 vp = HandleCamera.WorldToViewportPoint(targetTransform.position);
            if (vp.z < 0f) return;

            float scale = GetHandleScale();

            if (activeProfile != null)
            {
                // Profile mode: Render based on profile settings
                // This would need a new method in HandleRenderer that can handle mixed spaces
                handleRenderer.RenderWithProfile(
                    targetTransform,
                    scale,
                    interaction.HoveredAxis,
                    handleType,
                    activeProfile);
            }
            else
            {
                // Default mode: Use manager settings
                handleRenderer.Render(
                    targetTransform,
                    scale,
                    interaction.HoveredAxis,
                    handleType,
                    handleSpace);
            }
        }

        private float GetHandleScale()
        {
            if (!maintainConstantScreenSize || HandleCamera == null || targetTransform == null)
                return handleSize;

            float distance = Vector3.Distance(HandleCamera.transform.position, targetTransform.position);
            return distance * screenSizeMultiplier * handleSize;
        }

        void OnDestroy()
        {
            handleRenderer?.Cleanup();

            if (instance == this)
            {
                instance = null;
            }

            OnHoverEnter = null;
            OnHoverExit = null;
        }

        // Static convenience methods for easy access
        public static void ShowHandles(Transform target)
        {
            Instance.SetTarget(target);
        }

        public static void HideHandles()
        {
            Instance.ClearTarget();
        }

        public static void SetSpace(HandleSpace space)
        {
            Instance.SetHandleSpace(space);
        }

    }

    // Extension methods for intuitive API
    public static class TransformHandleExtensions
    {
        /// <summary>
        /// Shows transform handles for this transform
        /// </summary>
        public static void ShowHandles(this Transform transform)
        {
            TransformHandleManager.ShowHandles(transform);
        }

        /// <summary>
        /// Hides transform handles if this transform is currently selected
        /// </summary>
        public static void HideHandles(this Transform transform)
        {
            if (TransformHandleManager.Instance.CurrentTarget == transform)
            {
                TransformHandleManager.HideHandles();
            }
        }

        /// <summary>
        /// Checks if this transform currently has handles
        /// </summary>
        public static bool HasHandles(this Transform transform)
        {
            return TransformHandleManager.Instance.CurrentTarget == transform;
        }
    }


}