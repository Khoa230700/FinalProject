using UnityEngine;

public class CSGOScope : MonoBehaviour
{
    [Header("References")]
    public GameObject scopeOverlayUI;
    public Camera playerCamera;
    public PlayerShoot playerShoot;

    [Header("Zoom Settings")]
    [Tooltip("FOV khi ngắm mặc định (nếu gunData.scopeZoom <= 0)")]
    public float scopedFOV = 12f;            // nhỏ hơn trước để 'zoom to hơn'
    [Tooltip("Tốc độ thu phóng (lerp)")]
    public float zoomSpeed = 10f;

    [Tooltip("Giới hạn FOV khi đang ngắm (cuộn chuột sẽ nằm trong khoảng này)")]
    public float minScopedFOV = 6f;          // zoom sâu nhất
    public float maxScopedFOV = 25f;         // zoom nông nhất

    [Tooltip("Độ nhạy cuộn chuột điều chỉnh targetFOV khi đang ngắm")]
    public float scrollZoomSensitivity = 8f;

    [Header("Camera Offset On Scope")]
    [Tooltip("Tịnh tiến camera local khi ngắm (Z dương = tiến tới trước)")]
    public Vector3 cameraLocalOffset = new Vector3(0f, -0.02f, 0.15f);  // tiến tới + hạ nhẹ
    [Tooltip("Tốc độ lerp vị trí camera")]
    public float offsetLerpSpeed = 14f;

    private float normalFOV;
    private float targetFOV;

    private Vector3 camLocalPosDefault;
    private Vector3 camLocalPosTarget;

    // Trạng thái scope để chỗ khác tham chiếu
    public bool IsScoped { get; private set; } = false;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;

        normalFOV = playerCamera.fieldOfView;
        targetFOV = normalFOV;

        camLocalPosDefault = playerCamera.transform.localPosition;
        camLocalPosTarget = camLocalPosDefault;

        if (scopeOverlayUI != null)
            scopeOverlayUI.SetActive(false);
    }

    void Update()
    {
        if (playerShoot == null || playerShoot.gunData == null || !playerShoot.gunData.hasScope)
            return;

        if (Input.GetButtonDown("Fire2")) ScopeIn();
        else if (Input.GetButtonUp("Fire2")) ScopeOut();

        // Cuộn chuột để tinh chỉnh độ zoom khi đang ngắm
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

        // Lerp camera local position
        playerCamera.transform.localPosition = Vector3.Lerp(
            playerCamera.transform.localPosition,
            camLocalPosTarget,
            Time.deltaTime * offsetLerpSpeed
        );
    }

    void ScopeIn()
    {
        IsScoped = true;
        if (scopeOverlayUI != null) scopeOverlayUI.SetActive(true);

        // Lấy FOV từ data nếu có, không thì dùng scopedFOV (nhỏ hơn để 'zoom to' hơn)
        float baseScoped = (playerShoot.gunData.scopeZoom > 0f) ? playerShoot.gunData.scopeZoom : scopedFOV;
        // Kẹp trong biên để cuộn chuột hoạt động hợp lý
        targetFOV = Mathf.Clamp(baseScoped, minScopedFOV, maxScopedFOV);

        // Tiến camera tới trước (local) để cảm giác 'đưa ống ngắm sát mắt'
        camLocalPosTarget = camLocalPosDefault + cameraLocalOffset;
    }

    void ScopeOut()
    {
        IsScoped = false;
        if (scopeOverlayUI != null) scopeOverlayUI.SetActive(false);

        targetFOV = normalFOV;
        camLocalPosTarget = camLocalPosDefault; // trả camera về chỗ cũ
    }
}
