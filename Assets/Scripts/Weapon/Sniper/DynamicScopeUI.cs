using UnityEngine;
using UnityEngine.UI;

public class DynamicScopeUI : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public Transform scopeLens;
    public RawImage scopeOverlayUI;
    public CSGOScope csgoScope;

    [Header("UI Tweaks On Scope")]
    [Range(0f, 1f)] public float centerPull = 0.2f;
    public float uiScaleOnScope = 1.15f;
    public float uiLerpSpeed = 12f;

    [Header("Control")]
    [Tooltip("Nếu bật, script NÀY sẽ bật/tắt RawImage. Nếu tắt, để CSGOScope tự bật/tắt.")]
    public bool controlActiveHere = false;   // <<< mặc định tắt để tránh giẫm chân

    RectTransform uiRect;
    RectTransform canvasRoot;
    Vector2 currentAnchoredPos;
    Vector3 currentScale;

    public bool dontScale = true;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (scopeOverlayUI == null)
        {
            Debug.LogWarning("[DynamicScopeUI] scopeOverlayUI chưa được gán!");
            enabled = false; return;
        }
        uiRect = scopeOverlayUI.rectTransform;
        TryCacheCanvasRoot();

        // Chỉ tắt ở Start nếu chính script này quản lý bật/tắt
        if (controlActiveHere) scopeOverlayUI.gameObject.SetActive(false);

        currentAnchoredPos = uiRect.anchoredPosition;
        currentScale = uiRect.localScale;
    }

    void LateUpdate()
    {
        bool scoped = (csgoScope != null && csgoScope.IsScoped);

        if (!scoped)
        {
            if (controlActiveHere && scopeOverlayUI.gameObject.activeSelf)
                scopeOverlayUI.gameObject.SetActive(false);
            return;
        }

        if (scopeLens == null || mainCamera == null) return;

        Vector3 screenPos = mainCamera.WorldToScreenPoint(scopeLens.position);
        if (screenPos.z <= 0f)
        {
            if (controlActiveHere && scopeOverlayUI.gameObject.activeSelf)
                scopeOverlayUI.gameObject.SetActive(false);
            return;
        }

        if (controlActiveHere && !scopeOverlayUI.gameObject.activeSelf)
            scopeOverlayUI.gameObject.SetActive(true);

        if (canvasRoot == null) TryCacheCanvasRoot();

        if (canvasRoot == null)
        {
            uiRect.position = screenPos; // fallback
        }
        else
        {
            var canvas = scopeOverlayUI.canvas;
            Camera camForCanvas = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                                  ? canvas.worldCamera : null;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRoot, screenPos, camForCanvas, out var localPoint))
            {
                Vector2 targetAnchoredPos = Vector2.Lerp(localPoint, Vector2.zero, centerPull);
                currentAnchoredPos = Vector2.Lerp(currentAnchoredPos, targetAnchoredPos, Time.deltaTime * uiLerpSpeed);
                uiRect.anchoredPosition = currentAnchoredPos;
            }
            else
            {
                uiRect.position = screenPos; // fallback
            }
        }


        Vector3 targetScale = dontScale ? Vector3.one : (Vector3.one * uiScaleOnScope);
        currentScale = Vector3.Lerp(currentScale, targetScale, Time.deltaTime * uiLerpSpeed);
        uiRect.localScale = currentScale;
    }

    void TryCacheCanvasRoot()
    {
        var canvas = scopeOverlayUI.canvas;
        if (canvas && canvas.rootCanvas) canvasRoot = canvas.rootCanvas.GetComponent<RectTransform>();
        else canvasRoot = null;
    }
}
