// PlayerMovement.cs
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Player Stats")]
    public PlayerStats playerStats;

    [Header("Aiming (for Sniper)")]
    public CSGOScope csgoScope; // Kéo thả trong Inspector nếu có scope

    [Header("Audio")]
    [Tooltip("AudioSource đặt trên Player (spatialBlend ~1.0 nếu muốn 3D).")]
    public AudioSource audioSource;
    [Tooltip("Các clip bước chân sẽ được random phát khi đang đi/chạy trên mặt đất.")]
    public AudioClip[] footstepClips;
    [Tooltip("Âm thanh khi nhảy.")]
    public AudioClip jumpSound;
    [Tooltip("Âm thanh khi tiếp đất.")]
    public AudioClip landSound;

    [Header("Footstep Volume")]
    [Range(0f, 3f)] public float footstepVolume = 1.0f;     // tăng/giảm to nhỏ bước chân
    [Range(0f, 3f)] public float runVolumeMultiplier = 1.25f; // chạy to hơn đi bộ
    [Range(0f, 3f)] public float jumpVolume = 1.0f;          // âm lượng nhảy
    [Range(0f, 3f)] public float landVolume = 1.0f;          // âm lượng tiếp đất

    [Header("Footstep Timing")]
    [Tooltip("Khoảng thời gian giữa 2 bước chân khi đi bộ.")]
    public float stepIntervalWalk = 0.6f;
    [Tooltip("Khoảng thời gian giữa 2 bước chân khi chạy.")]
    public float stepIntervalRun = 0.4f;
    [Tooltip("Scale theo tốc độ hiện tại để bước chân tự nhiên hơn.")]
    public float stepSpeedScale = 1.0f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool wasGrounded;
    private bool didMove = false;
    private bool didRun = false;
    private bool didJump = false;

    private float stepTimer = 0f;
    private int lastFootstepIndex = -1;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (playerStats == null || KeyBindingManager.Instance == null)
            return;

        if (isGrounded)
        {
            if (!didMove && IsMoving() && QuestManager.Instance.UpdateQuestProgress(QuestObjectiveType.Interact, "TutorialMove"))
            {
                didMove = true;
            }

            if (!didRun && IsRunning() && QuestManager.Instance.UpdateQuestProgress(QuestObjectiveType.Interact, "TutorialRun"))
            {
                didRun = true;
            }

            if (!didJump && KeyBindingManager.Instance.GetKeyDown("Jump") && QuestManager.Instance.UpdateQuestProgress(QuestObjectiveType.Interact, "TutorialJump"))
            {
                didJump = true;
            }
        }

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

        // Có đang di chuyển? (threshold tránh axis decay)
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

        // Di chuyển phẳng
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Phát footsteps khi đang di chuyển + chạm đất
        HandleFootsteps(isMoving, isAiming, currentSpeed, shiftHeld);

        // Nhảy
        if (KeyBindingManager.Instance.GetKeyDown("Jump") && isGrounded)
        {
            float g = Mathf.Abs(playerStats.gravity);
            velocity.y = Mathf.Sqrt(2f * g * playerStats.jumpHeight);

            // Âm thanh nhảy (dùng volume riêng)
            PlayOneShot(jumpSound, jumpVolume);
        }

        // Gravity
        velocity.y += playerStats.gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Land sound (vừa chạm đất)
        if (!wasGrounded && isGrounded)
        {
            // Tránh phát khi vừa spawn đã ở mặt đất: kiểm tra vận tốc rơi đủ lớn sẽ “đã” hơn
            if (Mathf.Abs(velocity.y) > 0.1f)
                PlayOneShot(landSound, landVolume);

            // Reset stepTimer để nhịp bước chân ko dồn ngay khi vừa tiếp đất
            stepTimer = 0f;
        }

        wasGrounded = isGrounded;
    }

    private void HandleFootsteps(bool isMoving, bool isAiming, float currentSpeed, bool shiftHeld)
    {
        if (!isGrounded || !isMoving) return;

        // Tính interval theo trạng thái đi/chạy
        bool isRunning = (!isAiming && shiftHeld && isMoving);
        float baseInterval = isRunning ? stepIntervalRun : stepIntervalWalk;

        // Scale theo tốc độ hiện tại để tự nhiên hơn (chạy nhanh => khoảng cách giữa các bước ngắn lại)
        float speedFactor = Mathf.Max(0.1f, currentSpeed / Mathf.Max(0.01f, playerStats.walkSpeed));
        float interval = baseInterval / (speedFactor * stepSpeedScale);

        stepTimer -= Time.deltaTime;
        if (stepTimer <= 0f)
        {
            // Tính volume: đi bộ = footstepVolume; chạy = footstepVolume * runVolumeMultiplier
            float vol = footstepVolume * (isRunning ? runVolumeMultiplier : 1f);
            PlayRandomFootstep(vol);
            stepTimer = interval;
        }
    }

    private void PlayRandomFootstep(float volume = 1f)
    {
        if (audioSource == null || footstepClips == null || footstepClips.Length == 0) return;

        int idx;
        // Random clip nhưng hạn chế lặp lại clip trước đó
        do { idx = Random.Range(0, footstepClips.Length); }
        while (idx == lastFootstepIndex && footstepClips.Length > 1);

        lastFootstepIndex = idx;
        audioSource.PlayOneShot(footstepClips[idx], volume); // <-- dùng volumeScale
    }

    private void PlayOneShot(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip, volumeScale); // <-- dùng volumeScale
    }

    // API cho Animator/controller dùng lại
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
