using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [Header("Owner")]
    public EnemyM ownerHealthSystem;

    public enum HitboxType { Body, Head }
    public HitboxType hitboxType;

    [Header("Blood FX (Backward compatible)")]
    [Tooltip("Nếu bloodEffects trống, sẽ dùng prefab này.")]
    public GameObject bloodEffect;

    [Header("Blood FX (Random)")]
    [Tooltip("Danh sách prefab máu để random.")]
    public GameObject[] bloodEffects;

    [Tooltip("Âm thanh máu bắn (random 1 clip nếu có).")]
    public AudioClip[] bloodSfx;

    [Header("Randomize Settings")]
    [Tooltip("Số lượng effect spawn (min..max) mỗi lần trúng.")]
    public Vector2Int spawnCountRange = new Vector2Int(1, 2);

    [Tooltip("Khoảng scale ngẫu nhiên áp dụng lên prefab.")]
    public Vector2 scaleRange = new Vector2(0.85f, 1.25f);

    [Tooltip("Thời gian sống (Destroy) ngẫu nhiên của prefab (nếu prefab không tự hủy).")]
    public Vector2 lifetimeRange = new Vector2(1.0f, 2.0f);

    [Tooltip("Xoay ngẫu nhiên quanh trục Y (đủ dùng cho particle billboard).")]
    public bool randomYawOnly = true;

    [Tooltip("Nếu true, sẽ cố gắng xoay theo normal (nếu nhận được). Với method OnHit có normal.")]
    public bool alignToSurfaceNormal = true;

    [Tooltip("Đẩy nhẹ ra khỏi bề mặt để tránh z-fighting (nếu có normal).")]
    public float surfaceOffset = 0.01f;

    [Header("Audio Settings")]
    [Range(0f, 2f)] public float bloodSfxVolume = 1.0f;

    // =================== API được gọi từ súng/đòn đánh ===================

    // Giữ tương thích với code cũ (PlayerShoot/MeleeWeapon đang gọi hàm này):
    public void OnHit(float damage, Vector3 hitPoint)
    {
        // Không có normal → chỉ spawn tại vị trí, hướng ngẫu nhiên (hoặc yaw-only)
        SpawnBloodFX(hitPoint, Vector3.zero, hasNormal: false);
    }

    // Bản mở rộng: nếu bạn muốn xoay dính sát bề mặt, gọi hàm này (sửa call-site để truyền hit.normal).
    public void OnHit(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        SpawnBloodFX(hitPoint, hitNormal, hasNormal: true);
    }

    // =================== Triển khai FX ===================

    void SpawnBloodFX(Vector3 point, Vector3 normal, bool hasNormal)
    {
        int spawnCount = Mathf.Max(spawnCountRange.x, Random.Range(spawnCountRange.x, spawnCountRange.y + 1));

        for (int i = 0; i < spawnCount; i++)
        {
            // 1) Chọn prefab
            GameObject prefab = PickBloodPrefab();
            if (prefab == null) continue;

            // 2) Tính rotation
            Quaternion rot;
            if (alignToSurfaceNormal && hasNormal && normal != Vector3.zero)
            {
                // quay mặt (forward) theo normal để bám bề mặt
                rot = Quaternion.LookRotation(normal);
            }
            else
            {
                if (randomYawOnly)
                    rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                else
                    rot = Random.rotationUniform;
            }

            // 3) Tính vị trí (đẩy nhẹ theo normal nếu có)
            Vector3 pos = (alignToSurfaceNormal && hasNormal && normal != Vector3.zero)
                        ? point + normal * surfaceOffset
                        : point;

            // 4) Tạo effect
            GameObject fx = Instantiate(prefab, pos, rot);

            // 5) Random scale
            float s = Random.Range(Mathf.Min(scaleRange.x, scaleRange.y),
                                   Mathf.Max(scaleRange.x, scaleRange.y));
            fx.transform.localScale *= s;

            // 6) Phát âm thanh (nếu có)
            PlayBloodSfx(pos);

            // 7) Hủy sau lifetime (nếu prefab không tự hủy)
            float life = Random.Range(Mathf.Min(lifetimeRange.x, lifetimeRange.y),
                                      Mathf.Max(lifetimeRange.x, lifetimeRange.y));
            Destroy(fx, life);
        }
    }

    GameObject PickBloodPrefab()
    {
        if (bloodEffects != null && bloodEffects.Length > 0)
        {
            // lọc phần tử null (nếu lỡ để slot trống)
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
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, pos, bloodSfxVolume);
    }
}
