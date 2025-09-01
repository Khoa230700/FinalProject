using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Graphic))]
public class ScopeCutoutBinder : MonoBehaviour
{
    [Header("Bind")]
    public RectTransform rect;      // nếu để trống, tự lấy RectTransform của chính RawImage
    public Camera uiCamera;         // Canvas ScreenSpace-Camera/WorldSpace -> gán camera của Canvas; Overlay -> null
    public Camera worldCamera;      // camera người chơi (để theo dõi scopeLens)
    public Transform scopeLens;     // điểm ống ngắm trên khẩu súng (optional)
    public bool followLens = false; // bật nếu muốn vòng chạy theo lens

    [Header("Cutout Params")]
    [Range(0f, 0.6f)] public float radius = 0.35f;
    [Range(0f, 0.2f)] public float feather = 0.02f;
    public Color overlayColor = new Color(0, 0, 0, 1);

    private Material matInstance;
    private Graphic g;

    static readonly int ID_Color = Shader.PropertyToID("_Color");
    static readonly int ID_Radius = Shader.PropertyToID("_Radius");
    static readonly int ID_Feather = Shader.PropertyToID("_Feather");
    static readonly int ID_Aspect = Shader.PropertyToID("_RectAspect");
    static readonly int ID_Center = Shader.PropertyToID("_Center");

    void Awake()
    {
        g = GetComponent<Graphic>();
        if (!rect) rect = transform as RectTransform;

        EnsureMaterialInstance();

        // auto fill uiCamera nếu có Canvas camera
        var canvas = g.canvas;
        if (!uiCamera && canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = canvas.worldCamera;
    }

    void OnEnable()
    {
        EnsureMaterialInstance();
    }

    void OnDisable()
    {
        // giữ material khi tạm disable trong Editor/Play—không destroy ở đây
    }

    void OnDestroy()
    {
        if (matInstance != null)
        {
            if (Application.isPlaying) Destroy(matInstance);
            else DestroyImmediate(matInstance);
            matInstance = null;
        }
    }

    void EnsureMaterialInstance()
    {
        if (g == null) g = GetComponent<Graphic>();
        if (g == null) return;

        if (matInstance == null)
        {
            var baseMat = g.material != null ? g.material : Graphic.defaultGraphicMaterial;
            matInstance = new Material(baseMat);
            g.material = matInstance;
        }
    }

    void LateUpdate()
    {
        if (!matInstance || !rect) return;

        // Aspect của chính Rect (không phải màn hình)
        float aspect = Mathf.Max(0.0001f, rect.rect.width / Mathf.Max(1e-4f, rect.rect.height));

        // Center mặc định = tâm Rect
        Vector2 center01 = new Vector2(0.5f, 0.5f);

        // Nếu theo lens, convert world->screen->local->uv(0..1)
        if (followLens && worldCamera && scopeLens)
        {
            Vector3 screen = worldCamera.WorldToScreenPoint(scopeLens.position);
            if (screen.z > 0f)
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screen, uiCamera, out var local))
                {
                    // local = (-w/2..w/2, -h/2..h/2) => uv 0..1
                    center01.x = (local.x / rect.rect.width) + 0.5f;
                    center01.y = (local.y / rect.rect.height) + 0.5f;
                }
            }
        }

        // Set params
        matInstance.SetColor(ID_Color, overlayColor);
        matInstance.SetFloat(ID_Radius, radius);
        matInstance.SetFloat(ID_Feather, feather);
        matInstance.SetFloat(ID_Aspect, aspect);
        matInstance.SetVector(ID_Center, new Vector4(center01.x, center01.y, 0, 0));
    }
}
