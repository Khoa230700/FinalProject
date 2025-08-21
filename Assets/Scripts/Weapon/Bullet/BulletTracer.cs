using UnityEngine;
using System.Collections;

public class BulletTracer : MonoBehaviour
{
    private LineRenderer lineRenderer;

    [Header("Visual")]
    public Color tracerColor = Color.yellow;
    public float tracerDuration = 0.2f;
    [Tooltip("Độ dài vệt đạn hiển thị (chỉ là hiệu ứng, không phải tầm bắn).")]
    public float tracerLength = 2.5f;
    public float startWidth = 0.01f;
    public float endWidth = 0.005f;

    [Header("Ballistics / Raycast")]
    [Tooltip("Tầm ray ngắm từ camera (tâm màn hình).")]
    public float aimMaxDistance = 1000f;
    [Tooltip("Tầm kiểm tra va chạm đường đạn từ nòng súng.")]
    public float maxDistance = 1000f;
    [Tooltip("Layer bị bắn trúng (nên bỏ Player).")]
    public LayerMask hitMask = ~0; // mặc định mọi layer

    /// <summary>
    /// Gọi hàm này khi bắn.
    /// - muzzle: Transform của nòng súng (điểm xuất phát vệt đạn).
    /// - cam: Camera dùng để ray từ tâm màn hình (mặc định Camera.main).
    /// </summary>
    public void Init(Transform muzzle, Camera cam = null)
    {
        if (cam == null) cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[BulletTracer] Không tìm thấy Camera để ngắm.");
            return;
        }

        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null) lineRenderer = gameObject.AddComponent<LineRenderer>();
        SetupLineRenderer();

        // 1) Lấy điểm ngắm từ tâm màn hình (crosshair)
        Ray centerRay = cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));
        Vector3 aimPoint;
        if (Physics.Raycast(centerRay, out RaycastHit aimHit, aimMaxDistance, hitMask, QueryTriggerInteraction.Ignore))
        {
            aimPoint = aimHit.point;
        }
        else
        {
            aimPoint = centerRay.origin + centerRay.direction * aimMaxDistance;
        }

        // 2) Hướng bắn từ NÒNG SÚNG tới điểm ngắm
        Vector3 start = muzzle.position;
        Vector3 dir = (aimPoint - start).normalized;

        // 3) Kiểm tra va chạm thực tế dọc đường đạn từ nòng súng
        Vector3 pathEnd = start + dir * maxDistance;
        if (Physics.Raycast(start, dir, out RaycastHit shotHit, maxDistance, hitMask, QueryTriggerInteraction.Ignore))
        {
            pathEnd = shotHit.point;
        }

        // 4) Hiển thị VỆT ĐẠN (độ dài ngắn để tạo cảm giác tốc độ)
        float showLen = tracerLength;
        float distToImpact = Vector3.Distance(start, pathEnd);
        if (distToImpact < showLen) showLen = distToImpact;

        Vector3 visualEnd = start + dir * showLen;

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, visualEnd);

        // (Tùy chọn) Nếu bạn muốn vệt đạn kéo dài dần tới điểm trúng, có thể animate từ start->visualEnd tới pathEnd.
        // Ở đây mình giữ hiệu ứng "vệt ngắn" tiêu chuẩn.

        StartCoroutine(FadeAndDestroy());
    }

    private void SetupLineRenderer()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended"));
        lineRenderer.startColor = tracerColor;
        lineRenderer.endColor = tracerColor;
        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = endWidth;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
        lineRenderer.alignment = LineAlignment.View; // đẹp hơn khi nhìn từ nhiều góc
    }

    private IEnumerator FadeAndDestroy()
    {
        float elapsed = 0f;
        Color startColor = tracerColor;

        while (elapsed < tracerDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / tracerDuration);
            Color faded = new Color(startColor.r, startColor.g, startColor.b, alpha);
            lineRenderer.startColor = faded;
            lineRenderer.endColor = faded;

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
