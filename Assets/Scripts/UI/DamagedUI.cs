using UnityEngine;

public class DamagedUI : MonoBehaviour
{
    [Header("Blood Screen")]
    [SerializeField] private DamagedFader bloodScreenFader;

    [Header("Indicator")]
    [SerializeField] private RectTransform damageIndicator;
    [SerializeField] private DamagedFader damageIndicatorFader;
    [SerializeField] private float indicatorDistance = 128f;

    private Transform player;
    private Vector3 lastHitPoint;
    private PlayerHealth playerHealth;
    private PlayerShield playerShield;

    private void Start()
    {
        player ??= GameObject.FindGameObjectWithTag("Player").transform;
        playerHealth = player.GetComponent<PlayerHealth>();
        playerShield = player.GetComponent<PlayerShield>();
        playerHealth?.OnTakeDamage.AddListener(OnTakeDamage);
    }

    private void OnDestroy()
    {
        playerHealth?.OnTakeDamage.RemoveListener(OnTakeDamage);
    }

    //* Goi khi nhân vật bị thương
    private void OnTakeDamage(float delta, Vector3 hitPoint)
    {
        if (delta >= 0f) return;  //* Chỉ xử lý khi nhận sát thương

        float maxEffectiveHealth = playerHealth.maxHealth;
        if (playerShield != null)
        {
            maxEffectiveHealth += playerShield.maxShield;
        }

        float normalizedDamage = Mathf.Abs(delta) / maxEffectiveHealth;
        bloodScreenFader.DoFadeCycle(this, normalizedDamage);

        if (hitPoint != Vector3.zero)
        {
            lastHitPoint = hitPoint;
            damageIndicatorFader.DoFadeCycle(this, 1f); //* Hiện thị chỉ báo sát thương với alpha = 1
        }
    }

    private void Update()
    {
        if (!damageIndicatorFader.Fading) return;

        //* Hướng nhìn của người chơi
        Vector3 lookDir = Vector3.ProjectOnPlane(player.forward, Vector3.up).normalized; 

        //* Hướng từ nhân vật đến điểm va chạm
        Vector3 dirToHit = Vector3.ProjectOnPlane(lastHitPoint - player.position, Vector3.up).normalized;

        //* Phương ngang của nhân vật, (bên phải)
        Vector3 right = Vector3.Cross(lookDir, Vector3.up); 

        //* Góc giữa hướng nhìn và hướng đến điểm va chạm, với dấu hiệu để xác định bên trái hay phải
        float angle = Vector3.Angle(lookDir, dirToHit) * Mathf.Sign(Vector3.Dot(right, dirToHit)); 

        //* Xoay và đặt vị trí
        damageIndicator.localEulerAngles = Vector3.forward * angle;
        damageIndicator.localPosition = Quaternion.Euler(0f, 0f, angle) * Vector3.up * indicatorDistance;

    }    
}
