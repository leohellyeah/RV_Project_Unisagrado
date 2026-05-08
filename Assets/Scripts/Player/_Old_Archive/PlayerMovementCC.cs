using UnityEngine;

namespace ProjetoRV.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovementCC : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Assign Player_Cam here (Camera Transform).")]
        public Transform playerCam;

        [Header("Movement")]
        public float walkSpeed = 5.0f;
        public float sprintSpeed = 7.5f;
        public bool allowSprint = true;

        [Header("Jump & Gravity")]
        public float jumpHeight = 1.2f;
        public float gravity = -19.62f; // stronger than default feels better
        public float groundedStickForce = -2f; // keeps you grounded

        [Header("Ground Check")]
        public bool useBuiltInGrounded = true;

        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;

        void Awake()
        {
            controller = GetComponent<CharacterController>();

            if (playerCam == null)
            {
                // Tries to find Player_Cam automatically if user forgot to assign
                Camera cam = GetComponentInChildren<Camera>();
                if (cam) playerCam = cam.transform;
            }
        }

        void Update()
        {
            HandleGround();
            HandleMove();
            HandleJumpAndGravity();
        }

        void HandleGround()
        {
            if (useBuiltInGrounded)
                isGrounded = controller.isGrounded;

            if (isGrounded && velocity.y < 0f)
                velocity.y = groundedStickForce;
        }

        void HandleMove()
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            // Move relative to player orientation (yaw)
            Vector3 move = transform.right * x + transform.forward * z;

            float speed = walkSpeed;
            if (allowSprint && Input.GetKey(KeyCode.LeftShift))
                speed = sprintSpeed;

            controller.Move(move * speed * Time.deltaTime);
        }

        void HandleJumpAndGravity()
        {
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                // v = sqrt(h * -2g)
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}