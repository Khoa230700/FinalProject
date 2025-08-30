using UnityEngine;

/// <summary>
/// FPS movement dùng CharacterController, chỉ Move() **một lần** mỗi frame
/// để cc.velocity phản ánh đúng vận tốc (giúp FootstepAudio bắt bước chân).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Speeds")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;

    [Header("Jump & Gravity")]
    public float jumpHeight = 1.8f;
    public float gravity = -9.81f;

    [Header("Acceleration")]
    public float accelGround = 18f;
    public float accelAir = 6f;
    [Range(0f, 1f)] public float airControl = 0.5f; // điều khiển ngang trên không

    [Header("Input")]
    public string horizontalAxis = "Horizontal";
    public string verticalAxis = "Vertical";
    public KeyCode runKey = KeyCode.LeftShift;

    // state
    CharacterController controller;
    Vector3 velocity;          // vận tốc tổng (x,z,y)
    float currentPlanarSpeed;  // tốc độ ngang hiện tại
    bool isRunning;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // ---- INPUT ----
        float ix = Input.GetAxisRaw(horizontalAxis);
        float iz = Input.GetAxisRaw(verticalAxis);
        Vector3 inputDir = new Vector3(ix, 0f, iz);
        inputDir = inputDir.sqrMagnitude > 1f ? inputDir.normalized : inputDir;

        isRunning = Input.GetKey(runKey);

        // ---- GROUND CHECK ----
        bool grounded = controller.isGrounded;

        // ---- HƯỚNG DI CHUYỂN (LOCAL -> WORLD) ----
        Vector3 moveDir = transform.TransformDirection(inputDir);

        // ---- TỐC ĐỘ MỤC TIÊU & GIA TỐC ----
        float targetSpeed = (isRunning ? runSpeed : walkSpeed) * inputDir.magnitude;
        float accel = grounded ? accelGround : accelAir;
        currentPlanarSpeed = Mathf.MoveTowards(currentPlanarSpeed, targetSpeed, accel * Time.deltaTime);

        // giảm điều khiển ngang khi trên không
        float airCtrl = grounded ? 1f : airControl;

        // ---- GHÉP VẬN TỐC NGANG ----
        Vector3 planarVel = moveDir * (currentPlanarSpeed * airCtrl);
        velocity.x = planarVel.x;
        velocity.z = planarVel.z;

        // ---- NHẢY & TRỌNG LỰC ----
        if (grounded && velocity.y < 0f) velocity.y = -2f; // bám đất ổn định

        if (grounded && Input.GetButtonDown("Jump"))
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;

        // ---- MOVE 1 LẦN DUY NHẤT ----
        controller.Move(velocity * Time.deltaTime);
    }

    // ===== API cho các script khác (Footstep/Anim …) =====
    public bool IsGrounded() => controller.isGrounded;
    public bool IsRunning() => isRunning;

    public bool IsMoving(float threshold = 0.1f)
    {
        Vector3 v = controller.velocity; v.y = 0f;
        return v.sqrMagnitude > threshold * threshold;
    }

    public float GetPlanarSpeed()
    {
        Vector3 v = controller.velocity; v.y = 0f;
        return v.magnitude;
    }
}
