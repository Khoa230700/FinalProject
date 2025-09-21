using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [Header("Owner (quái thường)")]
    public EnemyM ownerHealthSystem;              // để trống nếu là Boss

    public enum HitboxType { Body, Head }
    public HitboxType hitboxType = HitboxType.Body;

    [Header("Damage")]
    [Tooltip("Nhân sát thương cho headshot.")]
    public float headshotMultiplier = 2f;
    [Tooltip("Bật để Hitbox tự relay sát thương lên BossHealth/EnemyM.")]
    public bool relayDamage = true;

    [Header("Blood FX (Backward compatible)")]
    [Tooltip("Nếu bloodEffects trống, sẽ dùng prefab này.")]
    public GameObject bloodEffect;

    [Header("Blood FX (Random)")]
    [Tooltip("Danh sách prefab máu để random.")]
    public GameObject[] bloodEffects;

    [Tooltip("Âm thanh máu bắn (random 1 clip nếu có).")]
    public AudioClip[] bloodSfx;

    [Header("Randomize Settings")]
    public Vector2Int spawnCountRange = new Vector2Int(1, 2);
    public Vector2 scaleRange = new Vector2(0.85f, 1.25f);
    public Vector2 lifetimeRange = new Vector2(1.0f, 2.0f);
    public bool randomYawOnly = true;
    public bool alignToSurfaceNormal = true;
    public float surfaceOffset = 0.01f;

    [Header("Audio Settings")]
    [Range(0f, 2f)] public float bloodSfxVolume = 1.0f;

    // =================== API được gọi từ súng/đòn đánh ===================

    // Giữ tương thích code cũ:
    public void OnHit(float damage, Vector3 hitPoint)
    {
        RelayDamageIfNeeded(damage);
        SpawnBloodFX(hitPoint, Vector3.zero, hasNormal: false);
    }

    // Có normal để dán FX theo bề mặt:
    public void OnHit(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        RelayDamageIfNeeded(damage);
        SpawnBloodFX(hitPoint, hitNormal, hasNormal: true);
    }

    // =================== Relay Damage (KHÔNG sửa BossHealth) ===================

    void RelayDamageIfNeeded(float damage)
    {
        if (!relayDamage) return;

        float finalDamage = (hitboxType == HitboxType.Head) ? damage * headshotMultiplier : damage;

        // 1) Ưu tiên: BossHealth ở parent (boss)
        if (TryGetComponentInParents(out BossHealth boss))
        {
            boss.TakeDamage(finalDamage);     // GỌI TRỰC TIẾP, KHÔNG SỬA BossHealth
            return;
        }

        // 2) Quái thường: EnemyM nếu có
        if (ownerHealthSystem != null)
        {
            ownerHealthSystem.TakeDamage(finalDamage);
            return;
        }

        // 3) Fallback: Gửi message "TakeDamage(float)" nếu có component khác xử lý
        transform.root.SendMessage("TakeDamage", finalDamage, SendMessageOptions.DontRequireReceiver);
    }

    bool TryGetComponentInParents<T>(out T comp) where T : Component
    {
        comp = GetComponent<T>();
        if (comp) return true;
        var p = transform.parent;
        while (p)
        {
            comp = p.GetComponent<T>();
            if (comp) return true;
            p = p.parent;
        }
        return false;
    }

    // =================== Triển khai FX ===================

    void SpawnBloodFX(Vector3 point, Vector3 normal, bool hasNormal)
    {
        int spawnCount = Mathf.Max(spawnCountRange.x, Random.Range(spawnCountRange.x, spawnCountRange.y + 1));

        for (int i = 0; i < spawnCount; i++)
        {
            GameObject prefab = PickBloodPrefab();
            if (prefab == null) continue;

            Quaternion rot;
            if (alignToSurfaceNormal && hasNormal && normal != Vector3.zero)
                rot = Quaternion.LookRotation(normal);
            else
                rot = randomYawOnly ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) : Random.rotationUniform;

            Vector3 pos = (alignToSurfaceNormal && hasNormal && normal != Vector3.zero)
                        ? point + normal * surfaceOffset
                        : point;

            GameObject fx = Instantiate(prefab, pos, rot);

            float s = Random.Range(Mathf.Min(scaleRange.x, scaleRange.y), Mathf.Max(scaleRange.x, scaleRange.y));
            fx.transform.localScale *= s;

            PlayBloodSfx(pos);

            float life = Random.Range(Mathf.Min(lifetimeRange.x, lifetimeRange.y), Mathf.Max(lifetimeRange.x, lifetimeRange.y));
            Destroy(fx, life);
        }
    }

    GameObject PickBloodPrefab()
    {
        if (bloodEffects != null && bloodEffects.Length > 0)
        {
            int guard = 0;
            while (guard++ < 8)
            {
                int idx = Random.Range(0, bloodEffects.Length);
                if (bloodEffects[idx] != null) return bloodEffects[idx];
            }
        }
        return bloodEffect; // fallback
    }

    void PlayBloodSfx(Vector3 pos)
    {
        if (bloodSfx == null || bloodSfx.Length == 0) return;
        int idx = Random.Range(0, bloodSfx.Length);
        var clip = bloodSfx[idx];
        if (clip != null) AudioSource.PlayClipAtPoint(clip, pos, bloodSfxVolume);
    }
}
