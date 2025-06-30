using UnityEngine;

public class CSGOScope : MonoBehaviour
{
    [Header("References")]
    public GameObject scopeOverlayUI;
    public Camera playerCamera;
    public PlayerShoot playerShoot;

    [Header("Settings")]
    public float scopedFOV = 10f;
    public float zoomSpeed = 8f; // tốc độ thu phóng

    private float normalFOV;
    private float targetFOV;
    private bool isScoped = false;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        normalFOV = playerCamera.fieldOfView;
        targetFOV = normalFOV;

        if (scopeOverlayUI != null)
            scopeOverlayUI.SetActive(false);
    }

    void Update()
    {
        if (playerShoot == null || !playerShoot.gunData.hasScope)
            return;

        if (Input.GetButtonDown("Fire2"))
        {
            ScopeIn();
        }
        else if (Input.GetButtonUp("Fire2"))
        {
            ScopeOut();
        }

        // Mượt FOV về giá trị targetFOV
        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFOV,
            Time.deltaTime * zoomSpeed
        );
    }

    void ScopeIn()
    {
        isScoped = true;

        if (scopeOverlayUI != null)
            scopeOverlayUI.SetActive(true);

        targetFOV = playerShoot.gunData.scopeZoom > 0 ? playerShoot.gunData.scopeZoom : scopedFOV;
    }

    void ScopeOut()
    {
        isScoped = false;

        if (scopeOverlayUI != null)
            scopeOverlayUI.SetActive(false);

        targetFOV = normalFOV;
    }
}
