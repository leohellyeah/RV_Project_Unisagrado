using UnityEngine;

namespace ProjetoRV.Player
{
    public class MouseLook360 : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Assign Player_Cam transform here.")]
        public Transform playerCam;

        [Tooltip("Optional. Assign Head transform (used only as organizational reference).")]
        public Transform head;

        [Header("Look Settings")]
        public float sensitivity = 2.0f;
        public bool invertY = false;
        public float minPitch = -80f;
        public float maxPitch = 80f;

        [Header("Cursor")]
        public bool lockCursor = true;

        private float pitch;

        void Start()
        {
            if (playerCam == null)
            {
                Camera cam = GetComponentInChildren<Camera>();
                if (cam) playerCam = cam.transform;
            }

            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        void Update()
        {
            if (playerCam == null) return;

            float mouseX = Input.GetAxis("Mouse X") * sensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

            // Yaw rotates the Player (horizontal)
            transform.Rotate(Vector3.up * mouseX);

            // Pitch rotates the camera (vertical)
            float y = invertY ? mouseY : -mouseY;
            pitch += y;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            playerCam.localRotation = Quaternion.Euler(pitch, 0f, 0f);

            // Optional: press ESC to release cursor (helpful for testing)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}