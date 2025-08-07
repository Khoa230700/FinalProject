// BulletRaycast.cs
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BulletRaycast : MonoBehaviour
{
    private int damage;
    private float range;
    private float tracerDuration;
    private Color tracerColor;
    private LineRenderer lineRenderer;

    public void Init(GunData gunData, Vector3 origin, Vector3 direction)
    {
        damage = gunData.damage;
        range = gunData.range;
        tracerDuration = gunData.reloadTime * 0 + 0.05f; // hoặc expose trong GunData nếu cần
        tracerColor = Color.yellow;

        lineRenderer = GetComponent<LineRenderer>();
        SetupLineRenderer();

        RaycastHit hit;
        Vector3 end = origin + direction * range;
        if (Physics.Raycast(origin, direction, out hit, range))
        {
            end = hit.point;
            var target = hit.collider.GetComponent<IDamageable>();
            if (target != null) target.TakeDamage(damage);
        }

        lineRenderer.SetPosition(0, origin);
        lineRenderer.SetPosition(1, end);

        Destroy(gameObject, tracerDuration);
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
}
