using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Player Stats")]
    public PlayerStats playerStats;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats not assigned to PlayerMovement!");
            return;
        }

        // Ground check
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Input
        float moveX = KeyBindingManager.Instance.GetAxis("Horizontal");
        float moveZ = KeyBindingManager.Instance.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // Check running
        float currentSpeed = KeyBindingManager.Instance.GetKey("Run") ? playerStats.runSpeed : playerStats.walkSpeed;

        controller.Move(move * currentSpeed * Time.deltaTime);

        // Jump
        if (KeyBindingManager.Instance.GetKeyDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(playerStats.jumpHeight * -2f * playerStats.gravity);
        }

        // Gravity
        velocity.y += playerStats.gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
