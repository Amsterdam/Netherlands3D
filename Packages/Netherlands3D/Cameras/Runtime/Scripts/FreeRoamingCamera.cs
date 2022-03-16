using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Cameras
{
    /// <summary>
    /// A camera that can be controlled to freely roam through a scene
    /// </summary>
    /// <remarks>
    /// Controls:
    /// WASD: movement -
    /// Q: Ascend -
    /// E: Descend -
    /// R: Ascend locally -
    /// F: Descend locally -
    /// Shift: Move faster -
    /// Control: Move slower
    /// </remarks>
    [AddComponentMenu("Netherlands3D/Cameras/Free Roaming Camera")]
    [RequireComponent(typeof(Camera))]
    public class FreeRoamingCamera : MonoBehaviour
    {
        /// <summary>
        /// Is the free roaming camera enabled?
        /// </summary>
        public bool IsEnabled 
        {
            get { return isEnabled; }
            set
            {
                isEnabled = value;
                if(isEnabled)
                {
                    previousCursorLockMode = Cursor.lockState;
                    Cursor.lockState = CursorLockMode.Locked;
                    if(camera != null) camera.enabled = true;
                }
                else
                {
                    Cursor.lockState = previousCursorLockMode;
                    if(camera != null) camera.enabled = false;
                }
            }
        }

        /// <summary>
        /// Is the free roaming camera enabled?
        /// </summary>
        [SerializeField] private bool isEnabled;

        [Header("Values")]
        [Tooltip("The sensitivity of the camera")]
        public float sensitivity = 900;
        [Tooltip("How fast the camera moves normally")]
        public float normalSpeed = 20;
        [Tooltip("Speed that gets multiplied to normal speed to move slow")]
        public float slowSpeedMultiplier = 0.25f;
        [Tooltip("Speed that gets multiplied to normal speed to move fast")]
        public float fastSpeedMultiplier = 3;

        /// <summary>
        /// The input of the player
        /// </summary>
        private Vector2 input;
        /// <summary>
        /// The current speed of the camera
        /// </summary>
        private float currentSpeed;
        /// <summary>
        /// The lockmode before FreeRoamingCamera was enabled to reset when FRC is d
        /// </summary>
        private CursorLockMode previousCursorLockMode;
        /// <summary>
        /// The camera attached to this gameobject
        /// </summary>
        private new Camera camera;

        private void Awake()
        {
            camera = GetComponent<Camera>();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateMovement();
        }

        /// <summary>
        /// Update the movement of the camera by player input
        /// </summary>
        private void UpdateMovement()
        {
            if(!IsEnabled) return;

            // Rotation
            input += sensitivity * Time.deltaTime * new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            input.y = Mathf.Clamp(input.y, -90, 90); // Dont allow x-axis 360 degree rotation
            transform.localRotation = Quaternion.AngleAxis(input.x, Vector3.up) * Quaternion.AngleAxis(input.y, Vector3.left);

            // Calculate currentSpeed
            if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                currentSpeed = normalSpeed * fastSpeedMultiplier;
            }
            else if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                currentSpeed = normalSpeed * slowSpeedMultiplier;
            }
            else currentSpeed = normalSpeed;

            // Position
            transform.position += currentSpeed * Input.GetAxis("Vertical") * Time.deltaTime * transform.forward;
            transform.position += currentSpeed * Input.GetAxis("Horizontal") * Time.deltaTime * transform.right;

            // Ascend / Descend locally
            if(Input.GetKey(KeyCode.R)) transform.position += currentSpeed * Time.deltaTime * transform.up;
            else if(Input.GetKey(KeyCode.F)) transform.position -= currentSpeed * Time.deltaTime * transform.up;

            // Ascend / Descend worldspace
            if(Input.GetKey(KeyCode.E)) transform.position += currentSpeed * Time.deltaTime * Vector3.up;
            else if(Input.GetKey(KeyCode.Q)) transform.position -= currentSpeed * Time.deltaTime * Vector3.up;
        }

        private void OnValidate()
        {
            IsEnabled = isEnabled;
        }
    }
}
