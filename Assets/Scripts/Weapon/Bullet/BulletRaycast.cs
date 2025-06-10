using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BulletRaycast : MonoBehaviour
{
    private int damage;
    private float range;
    private float penetrationPower;

    [Header("Tracer Settings")]
    public float tracerDuration = 0.05f;      // Bao lâu thì ray biến mất
    public Color tracerColor = Color.yellow;  // Màu vệt đạn

    private LineRenderer lineRenderer;

    public void InitFromGunData(GunData gunData, Transform shootPoint)
    {
        damage = gunData.damage;
        range = gunData.range;
        penetrationPower = gunData.penetrationPower;

        lineRenderer = GetComponent<LineRenderer>();
        SetupLineRenderer();

        RaycastHit hit;
        Vector3 endPoint;

        if (Physics.Raycast(shootPoint.position, shootPoint.forward, out hit, range))
        {
            endPoint = hit.point;

            // Gây sát thương nếu có
            IDamageable target = hit.collider.GetComponent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }
        }
        else
        {
            endPoint = shootPoint.position + shootPoint.forward * range;
        }

        // Vẽ vệt đạn
        lineRenderer.SetPosition(0, shootPoint.position);
        lineRenderer.SetPosition(1, endPoint);

        // Hủy sau thời gian ngắn
        Destroy(gameObject, tracerDuration);
    }

    private void SetupLineRenderer()
    {
        if (lineRenderer == null) return;

        lineRenderer.positionCount = 2;
        lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended"));
        lineRenderer.startColor = tracerColor;
        lineRenderer.endColor = tracerColor;
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.005f;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
    }
}
