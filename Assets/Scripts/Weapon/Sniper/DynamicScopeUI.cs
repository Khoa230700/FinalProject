using UnityEngine;
using UnityEngine.UI;

public class DynamicScopeUI : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;        // camera chính
    public Transform scopeLens;         // marker trên model
    public RawImage scopeOverlayUI;    // RawImage chứa RenderTexture
    public CSGOScope csgoScope;         // để biết đang scoped hay không

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        scopeOverlayUI.gameObject.SetActive(false);
    }

    void Update()
    {
        // chỉ hiển thị khi đang scoped
        if (csgoScope != null && csgoScope.IsScoped)
        {
            // world→screen
            Vector3 screenPos = mainCamera.WorldToScreenPoint(scopeLens.position);

            // check nếu lens nằm trước camera mới show
            if (screenPos.z > 0)
            {
                scopeOverlayUI.gameObject.SetActive(true);
                // đặt UI vào đúng pixel
                scopeOverlayUI.rectTransform.position = screenPos;
            }
            else
            {
                // lens ra sau lưng camera → ẩn UI
                scopeOverlayUI.gameObject.SetActive(false);
            }
        }
        else
        {
            scopeOverlayUI.gameObject.SetActive(false);
        }
    }
}
