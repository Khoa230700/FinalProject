using UnityEngine;
using System.Collections;

public class BulletTracer : MonoBehaviour
{
    private LineRenderer lineRenderer;

    public Color tracerColor = Color.yellow;
    public float tracerDuration = 0.2f;
    public float maxDistance = 100f;
    public float shortDistance = 2f;

    public void Init(Vector3 start, Vector3 direction)
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        SetupLineRenderer();

        float tracerLength = 2.5f; // 🎯 Độ dài vệt đạn mong muốn
        Vector3 end = start + direction * tracerLength;

        if (Physics.Raycast(start, direction, out RaycastHit hit, maxDistance))
        {
            float hitDistance = Vector3.Distance(start, hit.point);
            if (hitDistance < tracerLength)
            {
                end = hit.point; // Nếu trúng gần hơn độ dài vệt đạn
            }
        }

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        StartCoroutine(FadeAndDestroy());
    }

    private void SetupLineRenderer()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended"));

        lineRenderer.startColor = tracerColor;
        lineRenderer.endColor = tracerColor;
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.005f;

        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
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
