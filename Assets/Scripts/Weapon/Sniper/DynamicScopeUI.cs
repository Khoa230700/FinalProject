using UnityEngine;
using UnityEngine.UI;

public class DynamicScopeUI : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;            // camera người chơi
    public Transform scopeLens;          // marker trên model
    public RawImage scopeOverlayUI;      // RawImage chứa RenderTexture
    public CSGOScope csgoScope;          // để biết đang scoped hay không

    [Header("UI Tweaks On Scope")]
    [Range(0f, 1f)] public float centerPull = 0.2f; // kéo về tâm
    public float uiScaleOnScope = 1.15f;            // phóng to nhẹ
    public float uiLerpSpeed = 12f;

    private RectTransform uiRect;
    private RectTransform canvasRoot;
    private Vector2 currentAnchoredPos;
    private Vector3 currentScale;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        if (scopeOverlayUI == null)
        {
            Debug.LogWarning("[DynamicScopeUI] scopeOverlayUI chưa được gán!");
            enabled = false; return;
        }

        uiRect = scopeOverlayUI.rectTransform;
        TryCacheCanvasRoot(); // cố lấy canvasRoot nếu có

        scopeOverlayUI.gameObject.SetActive(false);
        currentAnchoredPos = uiRect.anchoredPosition;
        currentScale = uiRect.localScale;
    }

    void LateUpdate()
    {
        // chỉ hiển thị khi đang scope
        if (csgoScope == null || !csgoScope.IsScoped)
        {
            if (scopeOverlayUI.gameObject.activeSelf) scopeOverlayUI.gameObject.SetActive(false);
            return;
        }

        if (scopeLens == null || mainCamera == null) return;

        // world → screen
        Vector3 screenPos = mainCamera.WorldToScreenPoint(scopeLens.position);
        if (screenPos.z <= 0f)
        {
            if (scopeOverlayUI.gameObject.activeSelf) scopeOverlayUI.gameObject.SetActive(false);
            return;
        }

        if (!scopeOverlayUI.gameObject.activeSelf) scopeOverlayUI.gameObject.SetActive(true);

        // bảo đảm có canvasRoot; nếu chưa có thì thử lấy lại (trường hợp UI được bật runtime)
        if (canvasRoot == null) TryCacheCanvasRoot();

        // Nếu vẫn chưa có canvasRoot → fallback đặt theo screenPos (ít chuẩn hơn nhưng không crash)
        if (canvasRoot == null)
        {
            uiRect.position = screenPos;
        }
        else
        {
            // Chọn camera phù hợp theo render mode
            var canvas = scopeOverlayUI.canvas;
            Camera camForCanvas = null;
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                camForCanvas = canvas.worldCamera; // bắt buộc có khi dùng Screen Space - Camera / World Space

            // screen → anchored
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRoot, screenPos, camForCanvas, out var localPoint))
            {
                Vector2 targetAnchoredPos = Vector2.Lerp(localPoint, Vector2.zero, centerPull);
                currentAnchoredPos = Vector2.Lerp(currentAnchoredPos, targetAnchoredPos, Time.deltaTime * uiLerpSpeed);
                uiRect.anchoredPosition = currentAnchoredPos;
            }
            else
            {
                // nếu convert thất bại (camera null sai mode, v.v.) thì fallback
                uiRect.position = screenPos;
            }
        }

        // scale mượt khi scope
        Vector3 targetScale = Vector3.one * uiScaleOnScope;
        currentScale = Vector3.Lerp(currentScale, targetScale, Time.deltaTime * uiLerpSpeed);
        uiRect.localScale = currentScale;
    }

    private void TryCacheCanvasRoot()
    {
        var canvas = scopeOverlayUI.canvas;
        if (canvas != null && canvas.rootCanvas != null)
            canvasRoot = canvas.rootCanvas.GetComponent<RectTransform>();
        else
            canvasRoot = null;
    }
}
