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

        if (KeyBindingManager.Instance == null)
        {
            Debug.LogWarning("KeyBindingManager not initialized!");
            return;
        }

        // Ground check
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // reset trọng lực khi chạm đất
        }

        // Input
        float moveX = KeyBindingManager.Instance.GetAxis("Horizontal");
        float moveZ = KeyBindingManager.Instance.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // Tính tốc độ chạy/walk
        float currentSpeed = KeyBindingManager.Instance.GetKey("Run") ? playerStats.runSpeed : playerStats.walkSpeed;

        // Nếu trên không, giảm khả năng điều khiển
        if (!isGrounded)
        {
            currentSpeed *= playerStats.airControlMultiplier; // ví dụ 0.5f
        }

        // Di chuyển ngang
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Nhảy
        if (KeyBindingManager.Instance.GetKeyDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(playerStats.jumpHeight * -2f * playerStats.gravity);
        }

        // Trọng lực
        velocity.y += playerStats.gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    public bool IsMoving()
    {
        if (KeyBindingManager.Instance == null) return false;
        return KeyBindingManager.Instance.GetAxis("Horizontal") != 0f || KeyBindingManager.Instance.GetAxis("Vertical") != 0f;
    }

    public bool IsRunning()
    {
        if (KeyBindingManager.Instance == null) return false;
        return KeyBindingManager.Instance.GetKey("Run") && IsMoving();
    }

    public bool IsGrounded() => isGrounded;
}
