using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Unity Editor-style camera navigation with WASD+QE movement and right mouse button rotation
    /// </summary>
    public class SimpleCameraController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float shiftSpeedMultiplier = 3f;
        [SerializeField] private float acceleration = 20f;
        [SerializeField] private float deceleration = 30f;
        [SerializeField] private float maxSpeed = 50f;
        
        [Header("Rotation Settings")]
        [SerializeField] private float rotationSpeed = 3f;
        [SerializeField] private float rotationSmoothTime = 0.05f;
        [SerializeField] private bool invertY = false;
        
        [Header("Zoom Settings")]
        [SerializeField] private float scrollSpeed = 10f;
        [SerializeField] private float scrollAcceleration = 50f;
        
        private Camera cam;
        private Vector3 currentVelocity;
        private Vector3 targetVelocity;
        private Vector2 rotationVelocity;
        private Vector2 currentRotation;
        private float scrollVelocity;
        
        // For mouse delta calculation
        private Vector2 lastMousePosition;

        void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null)
                cam = Camera.main;

            // Initialize rotation from current transform
            currentRotation.x = transform.eulerAngles.x;
            currentRotation.y = transform.eulerAngles.y;

            lastMousePosition = HandleInput.MousePosition;
        }
        
        void Update()
        {
            HandleMovement();
            HandleRotation();
            HandleScroll();
        }
        
        private void HandleMovement()
        {
            // Only move when right mouse button is held
            bool isRightMousePressed = HandleInput.RightMouseHeld;

            Vector3 inputDirection = Vector3.zero;

            if (isRightMousePressed)
            {
                // Get input
                if (HandleInput.GetKey(KeyCode.W)) inputDirection += Vector3.forward;
                if (HandleInput.GetKey(KeyCode.S)) inputDirection -= Vector3.forward;
                if (HandleInput.GetKey(KeyCode.A)) inputDirection -= Vector3.right;
                if (HandleInput.GetKey(KeyCode.D)) inputDirection += Vector3.right;
                if (HandleInput.GetKey(KeyCode.Q)) inputDirection -= Vector3.up;
                if (HandleInput.GetKey(KeyCode.E)) inputDirection += Vector3.up;

                // Normalize input
                if (inputDirection.magnitude > 1f)
                    inputDirection.Normalize();
            }

            // Speed modifier
            float targetSpeed = moveSpeed;
            if (HandleInput.GetKey(KeyCode.LeftShift) || HandleInput.GetKey(KeyCode.RightShift))
                targetSpeed *= shiftSpeedMultiplier;
            
            // Calculate target velocity in world space
            if (inputDirection.magnitude > 0.01f)
            {
                targetVelocity = transform.TransformDirection(inputDirection) * targetSpeed;
                targetVelocity = Vector3.ClampMagnitude(targetVelocity, maxSpeed);
            }
            else
            {
                targetVelocity = Vector3.zero;
            }
            
            // Smooth acceleration/deceleration
            float smoothingFactor = inputDirection.magnitude > 0.01f ? acceleration : deceleration;
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, smoothingFactor * Time.deltaTime);
            
            // Apply movement
            if (currentVelocity.magnitude > 0.01f)
            {
                transform.position += currentVelocity * Time.deltaTime;
            }
        }
        
        private void HandleRotation()
        {
            if (!HandleInput.RightMouseHeld)
            {
                lastMousePosition = HandleInput.MousePosition;
                return;
            }

            // Calculate mouse delta manually
            Vector2 currentMousePosition = HandleInput.MousePosition;
            Vector2 mouseDelta = currentMousePosition - lastMousePosition;
            lastMousePosition = currentMousePosition;
            
            if (mouseDelta.magnitude > 0)
            {
                // Calculate target rotation
                float yaw = mouseDelta.x * rotationSpeed * 0.1f;  // 0.1f to scale down sensitivity
                float pitch = (invertY ? mouseDelta.y : -mouseDelta.y) * rotationSpeed * 0.1f;
                
                // Smooth rotation
                rotationVelocity = Vector2.Lerp(rotationVelocity, new Vector2(pitch, yaw), 
                    Time.deltaTime / rotationSmoothTime);
                
                currentRotation.x += rotationVelocity.x;
                currentRotation.y += rotationVelocity.y;
                
                // Clamp pitch to prevent flipping
                currentRotation.x = Mathf.Clamp(currentRotation.x, -89f, 89f);
                
                // Apply rotation
                transform.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0f);
            }
            else
            {
                // Dampen rotation velocity when not moving mouse
                rotationVelocity = Vector2.Lerp(rotationVelocity, Vector2.zero, 
                    Time.deltaTime / rotationSmoothTime);
            }
        }
        
        private void HandleScroll()
        {
            float scrollInput = HandleInput.MouseScrollDelta.y;
            
            if (Mathf.Abs(scrollInput) > 0.01f)
            {
                // Accelerate scroll velocity
                scrollVelocity += scrollInput * scrollSpeed;
                scrollVelocity = Mathf.Clamp(scrollVelocity, -maxSpeed, maxSpeed);
            }
            else
            {
                // Decelerate when not scrolling
                scrollVelocity = Mathf.Lerp(scrollVelocity, 0f, scrollAcceleration * Time.deltaTime);
            }
            
            // Apply scroll movement
            if (Mathf.Abs(scrollVelocity) > 0.01f)
            {
                transform.position += transform.forward * scrollVelocity * Time.deltaTime;
            }
        }
        
        // Public method to reset velocity (useful when teleporting)
        public void ResetVelocity()
        {
            currentVelocity = Vector3.zero;
            targetVelocity = Vector3.zero;
            scrollVelocity = 0f;
            rotationVelocity = Vector2.zero;
        }
        
        // Public method to set position without interpolation
        public void SetPosition(Vector3 newPosition)
        {
            transform.position = newPosition;
            ResetVelocity();
        }
    }
}