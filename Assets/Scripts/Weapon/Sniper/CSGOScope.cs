using UnityEngine;

/// <summary>
/// CSGO-style Scope: giữ chuột phải để ngắm, thả để bỏ.
/// - Zoom mượt theo FOV (lerp) + cuộn chuột chỉnh zoom.
/// - Tịnh tiến camera local khi ngắm (đưa ống ngắm sát mắt).
/// - Tùy chọn: component này quản lý bật/tắt overlay (RawImage/panel).
/// - Có "safety clamp" để FOV không bao giờ tụt xuống quá nhỏ.
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
    public float scopedFOV = 24f;
    [Tooltip("Tốc độ Lerp chuyển FOV.")]
    public float zoomSpeed = 12f;

    [Tooltip("Giới hạn FOV khi ngắm (cuộn chuột sẽ bị kẹp trong khoảng này).")]
    public float minScopedFOV = 18f;     // zoom sâu nhất
    public float maxScopedFOV = 45f;     // zoom nông nhất

    [Tooltip("Độ nhạy cuộn chuột khi đang ngắm. Đặt 0 để tắt tính năng cuộn chỉnh zoom.")]
    public float scrollZoomSensitivity = 4f;

    [Header("Camera Offset On Scope")]
    [Tooltip("Tịnh tiến camera local khi ngắm (Z dương = tiến về trước).")]
    public Vector3 cameraLocalOffset = new Vector3(0f, -0.02f, 0.15f);
    [Tooltip("Tốc độ Lerp vị trí camera.")]
    public float offsetLerpSpeed = 16f;

    [Header("Overlay Control")]
    [Tooltip("Nếu bật: CSGOScope sẽ bật/tắt scopeOverlayUI. Nếu tắt: để script khác quản (vd DynamicScopeUI).")]
    public bool manageOverlayHere = true;

    [Header("Safety Clamp")]
    [Tooltip("FOV sẽ luôn nằm trong [safeMinFOV, safeMaxFOV] bất kể dữ liệu đầu vào.")]
    public float safeMinFOV = 15f;
    public float safeMaxFOV = 90f;

    // OUTPUT
    public bool IsScoped { get; private set; } = false;
    public System.Action<bool> OnScopeStateChanged; // true=in, false=out

    // internals
    private float normalFOV;
    private float targetFOV;
    private Vector3 camLocalPosDefault;
    private Vector3 camLocalPosTarget;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (playerCamera == null)
        {
            Debug.LogError("[CSGOScope] Không tìm thấy Camera. Gán playerCamera trong Inspector.");
            enabled = false;
            return;
        }

        // Chuẩn hóa và nhớ FOV ban đầu
        playerCamera.fieldOfView = Mathf.Clamp(playerCamera.fieldOfView, safeMinFOV, safeMaxFOV);
        normalFOV = playerCamera.fieldOfView;
        targetFOV = normalFOV;

        // Biên kẹp hợp lệ
        if (minScopedFOV > maxScopedFOV) { float t = minScopedFOV; minScopedFOV = maxScopedFOV; maxScopedFOV = t; }
        minScopedFOV = Mathf.Clamp(minScopedFOV, safeMinFOV, safeMaxFOV - 1f);
        maxScopedFOV = Mathf.Clamp(maxScopedFOV, minScopedFOV + 1f, safeMaxFOV);

        camLocalPosDefault = playerCamera.transform.localPosition;
        camLocalPosTarget = camLocalPosDefault;

        if (manageOverlayHere && scopeOverlayUI != null)
            scopeOverlayUI.SetActive(false);
    }

    void OnDisable()
    {
        // Reset sạch khi component bị tắt (đổi súng, disable object...)
        ForceScopeOut();
    }

    void Update()
    {
        // Không cho ngắm nếu vũ khí không có scope
        bool canScope = !(playerShoot == null || playerShoot.gunData == null || !playerShoot.gunData.hasScope);
        if (!canScope)
        {
            if (IsScoped) ForceScopeOut();
            return;
        }

        // Input: hỗ trợ cả Input Manager cũ và chuột phải trực tiếp
        bool scopeDown = Input.GetButtonDown("Fire2") || Input.GetMouseButtonDown(1);
        bool scopeUp = Input.GetButtonUp("Fire2") || Input.GetMouseButtonUp(1);

        if (scopeDown) ScopeIn();
        else if (scopeUp) ScopeOut();

        // Cuộn chuột tinh chỉnh FOV khi đang ngắm
        if (IsScoped && scrollZoomSensitivity > 0f)
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

        // Clamp cứng trước khi áp
        if (float.IsNaN(targetFOV)) targetFOV = normalFOV;
        targetFOV = Mathf.Clamp(targetFOV, safeMinFOV, safeMaxFOV);

        // Lerp FOV + Lerp vị trí
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
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
        OnScopeStateChanged?.Invoke(true);

        if (manageOverlayHere && scopeOverlayUI != null)
            scopeOverlayUI.SetActive(true);

        // Lấy FOV cơ sở từ GunData nếu có, không thì dùng scopedFOV
        float baseScoped =
            (playerShoot != null && playerShoot.gunData != null && playerShoot.gunData.scopeZoom > 0f)
            ? playerShoot.gunData.scopeZoom
            : scopedFOV;

        // Kẹp vào biên an toàn & biên min/max scoped
        baseScoped = Mathf.Clamp(baseScoped, safeMinFOV, safeMaxFOV);
        targetFOV = Mathf.Clamp(baseScoped, minScopedFOV, maxScopedFOV);

        // Tiến camera tới trước để tạo cảm giác đưa ống ngắm sát mắt
        camLocalPosTarget = camLocalPosDefault + cameraLocalOffset;
    }

    public void ScopeOut()
    {
        if (!IsScoped) return;
        IsScoped = false;
        OnScopeStateChanged?.Invoke(false);

        if (manageOverlayHere && scopeOverlayUI != null)
            scopeOverlayUI.SetActive(false);

        targetFOV = Mathf.Clamp(normalFOV, safeMinFOV, safeMaxFOV);
        camLocalPosTarget = camLocalPosDefault; // trả camera về chỗ cũ
    }

    /// <summary>Ép thoát scope từ bên ngoài (đổi súng, pause, cutscene…).</summary>
    public void ForceScopeOut()
    {
        if (IsScoped) OnScopeStateChanged?.Invoke(false);
        IsScoped = false;

        if (manageOverlayHere && scopeOverlayUI != null)
            scopeOverlayUI.SetActive(false);

        if (playerCamera != null)
        {
            playerCamera.fieldOfView = Mathf.Clamp(normalFOV, safeMinFOV, safeMaxFOV);
            playerCamera.transform.localPosition = camLocalPosDefault;
        }
        targetFOV = Mathf.Clamp(normalFOV, safeMinFOV, safeMaxFOV);
        camLocalPosTarget = camLocalPosDefault;
    }
}
