using UnityEngine;
using UnityEngine.UI;

public class DynamicScopeUI : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;           // camera chính
    public Transform scopeLens;         // marker trên model
    public RawImage scopeOverlayUI;     // RawImage chứa RenderTexture
    public CSGOScope csgoScope;         // để biết đang scoped hay không

    [Header("UI Tweaks On Scope")]
    [Tooltip("Kéo vị trí UI về gần tâm màn hình bao nhiêu (0 = giữ nguyên, 1 = về đúng tâm)")]
    [Range(0f, 1f)] public float centerPull = 0.2f;

    [Tooltip("Scale UI khi scope để cảm giác 'tiến tới' + 'to' hơn")]
    public float uiScaleOnScope = 1.15f;

    [Tooltip("Tốc độ lerp vị trí/scale UI")]
    public float uiLerpSpeed = 12f;

    RectTransform uiRect;
    RectTransform canvasRoot;
    Vector2 currentAnchoredPos;
    Vector3 currentScale;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        uiRect = scopeOverlayUI.rectTransform;
        canvasRoot = scopeOverlayUI.canvas.rootCanvas.GetComponent<RectTransform>();

        scopeOverlayUI.gameObject.SetActive(false);
        currentAnchoredPos = uiRect.anchoredPosition;
        currentScale = uiRect.localScale;
    }

    void LateUpdate()
    {
        bool show = (csgoScope != null && csgoScope.IsScoped);
        if (!show)
        {
            if (scopeOverlayUI.gameObject.activeSelf) scopeOverlayUI.gameObject.SetActive(false);
            return;
        }

        if (scopeLens == null || mainCamera == null) return;

        // world -> screen
        Vector3 screenPos = mainCamera.WorldToScreenPoint(scopeLens.position);
        if (screenPos.z <= 0f)
        {
            if (scopeOverlayUI.gameObject.activeSelf) scopeOverlayUI.gameObject.SetActive(false);
            return;
        }

        if (!scopeOverlayUI.gameObject.activeSelf) scopeOverlayUI.gameObject.SetActive(true);

        // screen -> anchored (Canvas Overlay)
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRoot, screenPos, null, out var localPoint))
        {
            // kéo về gần tâm theo tỉ lệ centerPull
            Vector2 targetAnchoredPos = Vector2.Lerp(localPoint, Vector2.zero, centerPull);
            currentAnchoredPos = Vector2.Lerp(currentAnchoredPos, targetAnchoredPos, Time.deltaTime * uiLerpSpeed);
            uiRect.anchoredPosition = currentAnchoredPos;
        }

        // scale lên nhẹ khi ngắm (mượt)
        Vector3 targetScale = Vector3.one * uiScaleOnScope;
        currentScale = Vector3.Lerp(currentScale, targetScale, Time.deltaTime * uiLerpSpeed);
        uiRect.localScale = currentScale;
    }
}
