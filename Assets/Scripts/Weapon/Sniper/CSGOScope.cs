using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// CSGO-style Scope:
/// - Giữ chuột phải để ngắm, thả để bỏ.
/// - Zoom mượt theo FOV (lerp), cuộn chuột để chỉnh mức zoom khi đang ngắm.
/// - Tịnh tiến camera local về phía trước khi ngắm để tạo cảm giác "đưa ống ngắm sát mắt".
/// - Bật/tắt UI overlay (RawImage/Canvas) nếu cần.
///
/// Lưu ý an toàn:
/// - FOV luôn bị clamp cứng (1..179) để tránh tụt về ~0 khi prefab/instance có min/max lỗi.
/// - Nếu vũ khí không có scope, script vẫn ép trả FOV/offset về bình thường (không return sớm).
/// - Chống nhiều CSGOScope cùng điều khiển 1 Camera.
/// </summary>
public class CSGOScope : MonoBehaviour
{
    [Header("References")]
    [Tooltip("UI overlay (RawImage/panel) hiển thị khi đang ngắm. Có thể để trống nếu không dùng.")]
    public GameObject scopeOverlayUI;
    [Tooltip("Camera người chơi (nếu để trống sẽ dùng Camera.main).")]
    public Camera playerCamera;
    [Tooltip("Để đọc gunData.hasScope và scopeZoom (nếu có).")]
    public PlayerShoot playerShoot;

    [Header("Zoom Settings")]
    [Tooltip("FOV khi ngắm mặc định (dùng khi gunData.scopeZoom <= 0). Nhỏ hơn = zoom mạnh hơn.")]
    public float scopedFOV = 12f;
    [Tooltip("Tốc độ thu phóng (lerp).")]
    public float zoomSpeed = 10f;

    [Tooltip("Giới hạn FOV khi đang ngắm (cuộn chuột sẽ nằm trong khoảng này).")]
    public float minScopedFOV = 6f;    // zoom sâu nhất
    public float maxScopedFOV = 25f;   // zoom nông nhất

    [Tooltip("Độ nhạy cuộn chuột khi đang ngắm.")]
    public float scrollZoomSensitivity = 8f;

    [Header("Camera Offset On Scope")]
    [Tooltip("Tịnh tiến camera local khi ngắm (Z dương = tiến về trước).")]
    public Vector3 cameraLocalOffset = new Vector3(0f, -0.02f, 0.15f);
    [Tooltip("Tốc độ lerp vị trí camera.")]
    public float offsetLerpSpeed = 14f;

    /// <summary>Trạng thái scope cho script khác tham chiếu.</summary>
    public bool IsScoped { get; private set; } = false;

    // --- Internal state ---
    private float normalFOV;
    private float targetFOV;

    private Vector3 camLocalPosDefault;
    private Vector3 camLocalPosTarget;

    // Clamp cứng để chống FOV ~ 0
    private const float FOV_HARD_MIN = 34f;
    private const float FOV_HARD_MAX = 179f;

    // Chống nhiều scope điều khiển cùng camera
    private static readonly Dictionary<Camera, CSGOScope> Owners = new Dictionary<Camera, CSGOScope>();

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (playerCamera == null)
        {
            Debug.LogError("[CSGOScope] Không tìm thấy Camera. Gán playerCamera trong Inspector.");
            enabled = false;
            return;
        }

        if (playerCamera.orthographic)
        {
            Debug.LogWarning($"[CSGOScope] Camera '{playerCamera.name}' đang để Orthographic. Đổi sang Perspective để dùng FOV.");
            playerCamera.orthographic = false; // ép về Perspective cho chắc
        }

        // Sửa min/max nếu bị đảo/không hợp lệ & clamp cứng
        float minCfg = Mathf.Max(FOV_HARD_MIN, minScopedFOV);
        float maxCfg = Mathf.Max(FOV_HARD_MIN, maxScopedFOV);
        if (minCfg > maxCfg) { float t = minCfg; minCfg = maxCfg; maxCfg = t; }
        minScopedFOV = Mathf.Clamp(minCfg, FOV_HARD_MIN, FOV_HARD_MAX);
        maxScopedFOV = Mathf.Clamp(maxCfg, FOV_HARD_MIN, FOV_HARD_MAX);

        normalFOV = Mathf.Clamp(playerCamera.fieldOfView, FOV_HARD_MIN, FOV_HARD_MAX);
        targetFOV = normalFOV;

        camLocalPosDefault = playerCamera.transform.localPosition;
        camLocalPosTarget = camLocalPosDefault;

        if (scopeOverlayUI) scopeOverlayUI.SetActive(false);

        // Đăng ký owner duy nhất cho camera
        if (Owners.TryGetValue(playerCamera, out var other) && other != null && other != this)
        {
            Debug.LogWarning($"[CSGOScope] Camera '{playerCamera.name}' đã được '{other.name}' điều khiển. Vô hiệu hóa CSGOScope trên '{name}'.");
            enabled = false;
            return;
        }
        Owners[playerCamera] = this;
    }

    void OnDisable()
    {
        ForceScopeOut();

        if (playerCamera && Owners.TryGetValue(playerCamera, out var me) && me == this)
            Owners.Remove(playerCamera);
    }

    void OnDestroy()
    {
        // Phòng trường hợp OnDisable không chạy (tuỳ lifecycle)
        if (playerCamera && Owners.TryGetValue(playerCamera, out var me) && me == this)
            Owners.Remove(playerCamera);
    }

    void Update()
    {
        bool weaponHasScope = (playerShoot != null && playerShoot.gunData != null && playerShoot.gunData.hasScope);

        // Input scope (chuột phải)
        bool scopeDown = Input.GetButtonDown("Fire2") || Input.GetMouseButtonDown(1);
        bool scopeUp = Input.GetButtonUp("Fire2") || Input.GetMouseButtonUp(1);

        if (weaponHasScope)
        {
            if (scopeDown) ScopeIn();
            else if (scopeUp) ScopeOut();

            // Cuộn zoom khi đang scope
            if (IsScoped)
            {
                float scroll = Input.mouseScrollDelta.y;
                if (Mathf.Abs(scroll) > Mathf.Epsilon)
                {
                    targetFOV = Mathf.Clamp(
                        targetFOV - scroll * scrollZoomSensitivity,
                        minScopedFOV, maxScopedFOV
                    );
                }
            }
        }
        else
        {
            // Nếu súng hiện tại không có scope → đảm bảo trả về bình thường
            if (IsScoped) ScopeOut();
            targetFOV = normalFOV;
            camLocalPosTarget = camLocalPosDefault;
        }

        // Lerp FOV an toàn + Lerp vị trí camera
        playerCamera.fieldOfView = SafeLerpFov(playerCamera.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
        playerCamera.transform.localPosition = Vector3.Lerp(
            playerCamera.transform.localPosition,
            camLocalPosTarget,
            Mathf.Clamp01(Time.deltaTime * offsetLerpSpeed)
        );
    }

    public void ScopeIn()
    {
        if (IsScoped) return;
        IsScoped = true;

        if (scopeOverlayUI) scopeOverlayUI.SetActive(true);

        // Lấy FOV từ data nếu có, không thì dùng scopedFOV
        float baseScoped =
            (playerShoot && playerShoot.gunData && playerShoot.gunData.scopeZoom > 0f)
            ? playerShoot.gunData.scopeZoom
            : scopedFOV;

        // Clamp chặt trong min/max và trong hard range
        baseScoped = Mathf.Clamp(baseScoped, minScopedFOV, maxScopedFOV);
        targetFOV = Mathf.Clamp(baseScoped, FOV_HARD_MIN, FOV_HARD_MAX);

        // Tiến camera tới trước để tạo cảm giác đưa ống ngắm sát mắt
        camLocalPosTarget = camLocalPosDefault + cameraLocalOffset;
    }

    public void ScopeOut()
    {
        if (!IsScoped) return;
        IsScoped = false;

        if (scopeOverlayUI) scopeOverlayUI.SetActive(false);

        targetFOV = normalFOV;
        camLocalPosTarget = camLocalPosDefault; // trả camera về chỗ cũ
    }

    /// <summary>Ép hủy scope và trả camera về trạng thái bình thường ngay lập tức.</summary>
    public void ForceScopeOut()
    {
        IsScoped = false;
        if (scopeOverlayUI) scopeOverlayUI.SetActive(false);

        if (playerCamera)
        {
            playerCamera.fieldOfView = Mathf.Clamp(normalFOV, FOV_HARD_MIN, FOV_HARD_MAX);
            playerCamera.transform.localPosition = camLocalPosDefault;
        }
        targetFOV = Mathf.Clamp(normalFOV, FOV_HARD_MIN, FOV_HARD_MAX);
        camLocalPosTarget = camLocalPosDefault;
    }

    private float SafeLerpFov(float current, float target, float t)
    {
        t = Mathf.Clamp01(t);
        float f = Mathf.Lerp(current, target, t);
        return Mathf.Clamp(f, FOV_HARD_MIN, FOV_HARD_MAX);
    }
}
