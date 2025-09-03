using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class CrosshairRayFollower : MonoBehaviour
{
    [Header("World & Weapon")]
    public Camera worldCamera;          // Main Camera
    public Transform shootPoint;        // nòng súng
    public bool useWeaponRay = true;    // BẬT để bám đúng ray từ nòng súng
    public float maxDistance = 1000f;
    public LayerMask rayMask = ~0;      // layer bắn

    [Header("UI")]
    public RectTransform crosshairRect; // CrosshairRoot
    public Canvas targetCanvas;         // Canvas chứa crosshair
    public Vector2 screenOffset = Vector2.zero;

    [Header("Behaviour")]
    public bool hideIfBehindOrOffscreen = false;
    [Range(0f, 60f)] public float followSmoothing = 20f;
    public bool showWhenNoHit = true;
    public bool rayFromScreenCenter = false; // dùng camera.ScreenPointToRay(center) nếu muốn

    Camera _uiCamera;
    RectTransform _canvasRect;
    bool _isOverlay = true;
    bool _inited;

    void Awake() => InitCanvasRefs();
    void Reset() { if (!worldCamera) worldCamera = Camera.main; }

    void InitCanvasRefs()
    {
        if (!worldCamera) worldCamera = Camera.main;
        if (!targetCanvas)
            targetCanvas = crosshairRect ? crosshairRect.GetComponentInParent<Canvas>() : FindObjectOfType<Canvas>();

        if (!targetCanvas)
        {
            _isOverlay = true; _uiCamera = null; _canvasRect = null; _inited = true; return;
        }

        _isOverlay = (targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay);
        _uiCamera = _isOverlay ? null : targetCanvas.worldCamera;
        _canvasRect = targetCanvas.transform as RectTransform;
        _inited = true;
    }

    void Update()
    {
        if (!_inited) InitCanvasRefs();
        if (!crosshairRect || !worldCamera) return;

        Ray ray = BuildRay();
        bool hit = Physics.Raycast(ray, out RaycastHit info, maxDistance, rayMask, QueryTriggerInteraction.Ignore);
        Vector3 worldPoint = hit ? info.point : ray.origin + ray.direction * maxDistance;

        Vector3 sp = worldCamera.WorldToScreenPoint(worldPoint);
        bool behind = sp.z < 0f;

        if (hideIfBehindOrOffscreen)
        {
            bool off = sp.x < 0 || sp.x > Screen.width || sp.y < 0 || sp.y > Screen.height;
            crosshairRect.gameObject.SetActive(!behind && !off);
        }
        else
        {
            if (!hit && !showWhenNoHit) crosshairRect.gameObject.SetActive(false);
            else crosshairRect.gameObject.SetActive(true);
        }

        Vector3 targetScreen = sp + new Vector3(screenOffset.x, screenOffset.y, 0f);
        MoveUIToScreen(targetScreen);
    }

    Ray BuildRay()
    {
        if (useWeaponRay && shootPoint)
            return new Ray(shootPoint.position, shootPoint.forward);

        if (rayFromScreenCenter)
        {
            Vector3 c = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
            return worldCamera.ScreenPointToRay(c);
        }

        return new Ray(worldCamera.transform.position, worldCamera.transform.forward);
    }

    void MoveUIToScreen(Vector3 screenPos)
    {
        if (followSmoothing > 0f)
        {
            if (_isOverlay)
                crosshairRect.position = Vector3.Lerp(crosshairRect.position, screenPos, Time.deltaTime * followSmoothing);
            else if (_canvasRect)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screenPos, _uiCamera, out var lp);
                crosshairRect.anchoredPosition = Vector2.Lerp(crosshairRect.anchoredPosition, lp, Time.deltaTime * followSmoothing);
            }
            else crosshairRect.position = Vector3.Lerp(crosshairRect.position, screenPos, Time.deltaTime * followSmoothing);
        }
        else
        {
            if (_isOverlay) crosshairRect.position = screenPos;
            else if (_canvasRect)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screenPos, _uiCamera, out var lp);
                crosshairRect.anchoredPosition = lp;
            }
            else crosshairRect.position = screenPos;
        }
    }
}
