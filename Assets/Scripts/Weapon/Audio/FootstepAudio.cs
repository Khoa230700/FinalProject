using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Phát tiếng bước chân/nhảy/tiếp đất. Đo tốc độ ngang theo cả cc.velocity và delta vị trí
/// (lấy max) để luôn bắt được “đang di chuyển”. Dùng cho CharacterController.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FootstepAudio : MonoBehaviour
{
    [Header("Refs")]
    public PlayerMovement movement;   // kéo PlayerMovement (tùy chọn, nhưng nên có)
    public AudioSource audioSource;   // AudioSource 3D trên Player

    [Header("Footstep")]
    public List<AudioClip> footstepClips = new List<AudioClip>();
    public float stepIntervalWalk = 0.5f;
    public float stepIntervalRun = 0.35f;
    [Range(0f, 1f)] public float footstepVolume = 0.8f;
    [Range(0.5f, 1.5f)] public float pitchMin = 0.95f, pitchMax = 1.05f;

    [Header("Jump/Land")]
    public AudioClip jumpClip;
    public AudioClip landClip;
    [Range(0f, 1f)] public float miscVolume = 1f;

    [Header("Thresholds")]
    [Tooltip("Ngưỡng tốc độ ngang để tính là đang di chuyển (m/s).")]
    public float minMoveSpeed = 0.08f;

    [Header("Debug")]
    public bool logSpeed = false;

    CharacterController cc;
    float stepTimer;
    bool wasGrounded;
    Vector3 lastPos;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (!movement) movement = GetComponent<PlayerMovement>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        lastPos = transform.position;

        // thiết lập AudioSource gợi ý nếu để trống
        if (audioSource)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 1f; // 3D
        }
    }

    void Update()
    {
        if (!audioSource) return;

        bool grounded = movement ? movement.IsGrounded() : cc.isGrounded;
        bool running = movement ? movement.IsRunning() : false;

        // Nhảy
        if (grounded && Input.GetButtonDown("Jump"))
            PlayOneShot(jumpClip, miscVolume);

        // Tiếp đất
        if (grounded && !wasGrounded)
            PlayOneShot(landClip, miscVolume);

        // --- Tốc độ ngang ---
        // 1) từ cc.velocity
        Vector3 v = cc.velocity; v.y = 0f;
        float speedByCC = v.magnitude;

        // 2) fallback theo delta vị trí
        Vector3 delta = transform.position - lastPos; delta.y = 0f;
        float speedByDelta = delta.magnitude / Mathf.Max(Time.deltaTime, 1e-4f);

        // lấy max cho an toàn
        float horizontalSpeed = Mathf.Max(speedByCC, speedByDelta);
        if (logSpeed) Debug.Log($"[Footstep] speedCC={speedByCC:F3}, delta={speedByDelta:F3}, use={horizontalSpeed:F3}");

        bool movingEnough = horizontalSpeed > minMoveSpeed;

        // --- Phát bước chân ---
        if (grounded && movingEnough && footstepClips.Count > 0)
        {
            float interval = running ? stepIntervalRun : stepIntervalWalk;
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                stepTimer = interval;
                var clip = footstepClips[Random.Range(0, footstepClips.Count)];
                audioSource.pitch = Random.Range(pitchMin, pitchMax);
                audioSource.PlayOneShot(clip, footstepVolume);
            }
        }
        else
        {
            stepTimer = 0f; // reset nhịp khi dừng
        }

        wasGrounded = grounded;
        lastPos = transform.position;
    }

    void PlayOneShot(AudioClip c, float vol)
    {
        if (c) audioSource.PlayOneShot(c, vol);
    }
}
