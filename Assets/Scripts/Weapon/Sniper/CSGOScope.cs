using UnityEngine;

/// <summary>
/// CSGO-style Scope: giữ chuột phải để ngắm, thả để bỏ.
/// - Zoom mượt theo FOV (lerp).
/// - Cho phép cuộn chuột để chỉnh mức zoom khi đang ngắm.
/// - Tịnh tiến camera local về phía trước khi ngắm để tạo cảm giác "đưa ống ngắm sát mắt".
/// - Bật/tắt UI overlay (RawImage/Canvas) nếu cần.
/// 
/// Gợi ý set-up:
/// - Gán playerShoot (để kiểm tra gunData.hasScope và lấy scopeZoom).
/// - Gán playerCamera (nếu null sẽ lấy Camera.main).
/// - Nếu dùng UI RawImage vòng scope, gán scopeOverlayUI.
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
    public float minScopedFOV = 6f;   // zoom sâu nhất
    public float maxScopedFOV = 25f;  // zoom nông nhất

    [Tooltip("Độ nhạy cuộn chuột khi đang ngắm.")]
    public float scrollZoomSensitivity = 8f;

    [Header("Camera Offset On Scope")]
    [Tooltip("Tịnh tiến camera local khi ngắm (Z dương = tiến về trước).")]
    public Vector3 cameraLocalOffset = new Vector3(0f, -0.02f, 0.15f);
    [Tooltip("Tốc độ lerp vị trí camera.")]
    public float offsetLerpSpeed = 14f;

    private float normalFOV;
    private float targetFOV;

    private Vector3 camLocalPosDefault;
    private Vector3 camLocalPosTarget;

    /// <summary>Trạng thái scope cho script khác tham chiếu.</summary>
    public bool IsScoped { get; private set; } = false;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (playerCamera == null)
        {
            Debug.LogError("[CSGOScope] Không tìm thấy Camera. Gán playerCamera trong Inspector.");
            enabled = false;
            return;
        }

        // Đảm bảo min/max FOV hợp lệ
        if (minScopedFOV > maxScopedFOV)
        {
            float t = minScopedFOV;
            minScopedFOV = maxScopedFOV;
            maxScopedFOV = t;
        }

        normalFOV = playerCamera.fieldOfView;
        targetFOV = normalFOV;

        camLocalPosDefault = playerCamera.transform.localPosition;
        camLocalPosTarget = camLocalPosDefault;

        if (scopeOverlayUI != null) scopeOverlayUI.SetActive(false);
    }

    void OnDisable()
    {
        // Reset sạch khi component bị tắt (đổi súng, disable object...)
        ForceScopeOut();
    }

    void Update()
    {
        // Không cho ngắm nếu vũ khí không có scope
        if (playerShoot == null || playerShoot.gunData == null || !playerShoot.gunData.hasScope)
            return;

        // Hỗ trợ cả Input Manager cũ và chuột phải trực tiếp
        bool scopeDown = Input.GetButtonDown("Fire2") || Input.GetMouseButtonDown(1);
        bool scopeUp = Input.GetButtonUp("Fire2") || Input.GetMouseButtonUp(1);

        if (scopeDown) ScopeIn();
        else if (scopeUp) ScopeOut();

        // Cuộn chuột để tinh chỉnh zoom khi đang ngắm
        if (IsScoped)
        {
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > Mathf.Epsilon)
            {
                targetFOV = Mathf.Clamp(
                    targetFOV - scroll * scrollZoomSensitivity,
                    minScopedFOV,
                    maxScopedFOV
                );
            }
        }

        // Lerp FOV
        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFOV,
            Time.deltaTime * zoomSpeed
        );

        // Lerp vị trí camera
        playerCamera.transform.localPosition = Vector3.Lerp(
            playerCamera.transform.localPosition,
            camLocalPosTarget,
            Time.deltaTime * offsetLerpSpeed
        );
    }

    public void ScopeIn()
    {
        if (IsScoped) return;
        IsScoped = true;

        if (scopeOverlayUI != null) scopeOverlayUI.SetActive(true);

        // Lấy FOV từ data nếu có, không thì dùng scopedFOV
        float baseScoped = (playerShoot != null && playerShoot.gunData != null && playerShoot.gunData.scopeZoom > 0f)
                           ? playerShoot.gunData.scopeZoom
                           : scopedFOV;

        targetFOV = Mathf.Clamp(baseScoped, minScopedFOV, maxScopedFOV);

        // Tiến camera tới trước để tạo cảm giác đưa ống ngắm sát mắt
        camLocalPosTarget = camLocalPosDefault + cameraLocalOffset;
    }

    public void ScopeOut()
    {
        if (!IsScoped) return;
        IsScoped = false;

        if (scopeOverlayUI != null) scopeOverlayUI.SetActive(false);

        targetFOV = normalFOV;
        camLocalPosTarget = camLocalPosDefault; // trả camera về chỗ cũ
    }

    /// <summary>
    /// Dùng khi cần ép hủy scope từ bên ngoài (đổi súng, pause, cutscene...).
    /// </summary>
    public void ForceScopeOut()
    {
        IsScoped = false;
        if (scopeOverlayUI != null) scopeOverlayUI.SetActive(false);

        if (playerCamera != null)
        {
            playerCamera.fieldOfView = normalFOV;
            playerCamera.transform.localPosition = camLocalPosDefault;
        }
        targetFOV = normalFOV;
        camLocalPosTarget = camLocalPosDefault;
    }
}
