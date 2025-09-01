using UnityEngine;
using UnityEngine.UI;

public class CrosshairAimDot : MonoBehaviour
{
    [Header("Refs")]
    public Camera cam;                     // gán camera người chơi
    public RectTransform dot;              // Image/RectTransform của dot
    public LayerMask rayMask = ~0;         // layer nào raycast trúng (tuỳ map của bạn)
    public float maxDistance = 500f;

    [Header("Behaviour")]
    public bool hideWhenNoHit = false;     // không có hit thì ẩn dot
    public bool onlyWhenScoped = false;    // chỉ hiện khi đang ngắm (nếu bạn muốn)
    public CSGOScope scope;                // tham chiếu nếu dùng onlyWhenScoped

    RectTransform canvasRoot;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!dot) dot = GetComponent<RectTransform>();
        var canvas = dot.GetComponentInParent<Canvas>();
        if (canvas && canvas.rootCanvas) canvasRoot = canvas.rootCanvas.GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        if (!cam || !dot) return;

        if (onlyWhenScoped && scope && !scope.IsScoped)
        {
            if (dot.gameObject.activeSelf) dot.gameObject.SetActive(false);
            return;
        }

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        Vector3 worldPoint;
        bool gotHit = false;

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, rayMask, QueryTriggerInteraction.Ignore))
        {
            worldPoint = hit.point;
            gotHit = true;
        }
        else
        {
            worldPoint = cam.transform.position + cam.transform.forward * (maxDistance * 0.6f);
        }

        Vector3 screen = cam.WorldToScreenPoint(worldPoint);
        if (screen.z <= 0f)
        {
            if (hideWhenNoHit && dot.gameObject.activeSelf) dot.gameObject.SetActive(false);
            return;
        }

        if (!dot.gameObject.activeSelf) dot.gameObject.SetActive(true);

        var canvas = dot.GetComponentInParent<Canvas>();
        if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            dot.position = screen;
        }
        else
        {
            Camera uiCam = canvas.worldCamera;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRoot, screen, uiCam, out var lp))
                dot.anchoredPosition = lp;
            else
                dot.position = screen; // fallback
        }

        if (hideWhenNoHit) dot.gameObject.SetActive(gotHit);
    }
}
