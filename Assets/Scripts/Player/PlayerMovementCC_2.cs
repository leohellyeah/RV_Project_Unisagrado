using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementCC_2 : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 6f;
    public float sprintMultiplier = 1.6f;

    [Header("Pulo")]
    public float jumpHeight = 1.6f; // <-- DECLARADO AQUI
    public float gravity = -25f;

    private CharacterController controller;
    private Vector3 velocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        bool isGrounded = controller.isGrounded;

        // "cola" no chão para evitar tremedeira
        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        // WASD
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = (transform.right * x + transform.forward * z).normalized;

        float currentSpeed = speed;
        if (Input.GetKey(KeyCode.LeftShift))
            currentSpeed *= sprintMultiplier;

        controller.Move(move * currentSpeed * Time.deltaTime);

        // SPACE = pulo
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            // v = sqrt(h * -2g)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // gravidade
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}