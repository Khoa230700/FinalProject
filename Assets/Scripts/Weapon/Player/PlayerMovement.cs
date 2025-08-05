// PlayerMovement.cs
using UnityEngine;
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Player Stats")]
    public PlayerStats playerStats;

    [Header("Aiming (for Sniper)")]
    public CSGOScope csgoScope;    // Kéo thả trong Inspector nếu có scope

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (playerStats == null) return;
        if (KeyBindingManager.Instance == null) return;

        // Ground check
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Input
        float moveX = KeyBindingManager.Instance.GetAxis("Horizontal");
        float moveZ = KeyBindingManager.Instance.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // --- CHẮC CHỈ ĐI BỘ KHI ĐANG SCOPE ---
        bool isAiming = (csgoScope != null && csgoScope.IsScoped);
        float currentSpeed;
        if (isAiming)
        {
            // khi scoped chỉ dùng walkSpeed
            currentSpeed = playerStats.walkSpeed;
        }
        else
        {
            // bình thường: giữ Run để chạy nhanh
            currentSpeed = KeyBindingManager.Instance.GetKey("Run")
                           ? playerStats.runSpeed
                           : playerStats.walkSpeed;
        }
        // :contentReference[oaicite:2]{index=2}

        // Move horizontally
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Jump
        if (KeyBindingManager.Instance.GetKeyDown("Jump") && isGrounded)
        {
            float g = Mathf.Abs(playerStats.gravity);
            velocity.y = Mathf.Sqrt(2f * g * playerStats.jumpHeight);
        }

        // Gravity
        velocity.y += playerStats.gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    public bool IsMoving()
    {
        if (KeyBindingManager.Instance == null) return false;
        return KeyBindingManager.Instance.GetAxis("Horizontal") != 0f ||
               KeyBindingManager.Instance.GetAxis("Vertical") != 0f;
    }

    public bool IsRunning()
    {
        if (KeyBindingManager.Instance == null) return false;
        return KeyBindingManager.Instance.GetKey("Run") && IsMoving();
    }

    public bool IsGrounded() => isGrounded;
}
