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
        if (playerStats == null || KeyBindingManager.Instance == null)
            return;

        // Ground check
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        // Đọc input
        float moveX = KeyBindingManager.Instance.GetAxis("Horizontal");
        float moveZ = KeyBindingManager.Instance.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // Aiming chỉ đi bộ
        bool isAiming = (csgoScope != null && csgoScope.IsScoped);

        // Tính có đang di chuyển hay không (threshold tránh axis decay)
        float threshold = 0.1f;
        bool isMoving = Mathf.Abs(moveX) > threshold || Mathf.Abs(moveZ) > threshold;

        // Shift để chạy
        bool shiftHeld = KeyBindingManager.Instance.GetKey("Run");

        // Chọn tốc độ
        float currentSpeed;
        if (isAiming)
        {
            currentSpeed = playerStats.walkSpeed;
        }
        else
        {
            currentSpeed = (shiftHeld && isMoving)
                           ? playerStats.runSpeed
                           : playerStats.walkSpeed;
        }

        // Di chuyển
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Nhảy
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
        float h = KeyBindingManager.Instance.GetAxis("Horizontal");
        float v = KeyBindingManager.Instance.GetAxis("Vertical");
        return Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;
    }

    public bool IsRunning()
    {
        if (KeyBindingManager.Instance == null) return false;
        bool shiftHeld = KeyBindingManager.Instance.GetKey("Run");
        return shiftHeld && IsMoving();
    }

    public bool IsGrounded() => isGrounded;
}
