using UnityEngine;

public class DamagedUI : MonoBehaviour
{
    [Header("Blood Screen")]
    [SerializeField] private DamagedFader bloodScreenFader;

    [Header("Indicator")]
    [SerializeField] private RectTransform damageIndicator;
    [SerializeField] private DamagedFader damageIndicatorFader;
    [SerializeField] private float indicatorDistance = 128f;

    [Header("Fallback (nếu không đọc được từ hệ thống HP)")]
    [SerializeField] private float fallbackMaxHealth = 100f;
    [SerializeField] private float fallbackMaxShield = 0f;

    private Transform player;
    private Vector3 lastHitPoint;

    // Hệ mới
    private BaseHealthSystem baseHealth;
    private PlayerHealthSystem playerHealthSystem;

    // cache từ event
    private float cachedMaxShield = -1f;
    private float cachedMaxHealth = -1f;

    private void Start()
    {
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO == null)
        {
            Debug.LogWarning("[DamagedUI] Không tìm thấy Player theo tag 'Player'.");
            return;
        }

        player = playerGO.transform;
        baseHealth = player.GetComponent<BaseHealthSystem>();
        playerHealthSystem = player.GetComponent<PlayerHealthSystem>();

        // Nghe sát thương (UnityEvent<float, Vector3>)
        if (baseHealth != null && baseHealth.OnTakeDamage != null)
            baseHealth.OnTakeDamage.AddListener(OnTakeDamage);
        else
            Debug.LogWarning("[DamagedUI] Không thấy BaseHealthSystem.OnTakeDamage");

        // Nghe thay đổi HP để lấy maxHealth
        if (baseHealth != null)
            baseHealth.OnHealthChanged += OnHealthChanged;

        // Nghe thay đổi Shield để lấy maxShield
        if (playerHealthSystem != null)
            playerHealthSystem.OnShieldChanged += OnShieldChanged;
    }

    private void OnDestroy()
    {
        if (baseHealth != null)
        {
            if (baseHealth.OnTakeDamage != null)
                baseHealth.OnTakeDamage.RemoveListener(OnTakeDamage);
            baseHealth.OnHealthChanged -= OnHealthChanged;
        }

        if (playerHealthSystem != null)
            playerHealthSystem.OnShieldChanged -= OnShieldChanged;
    }

    private void OnHealthChanged(float current, float max)
    {
        cachedMaxHealth = max;
    }

    private void OnShieldChanged(float current, float max)
    {
        cachedMaxShield = max;
    }

    /// <summary>
    /// delta âm khi nhận damage; hitPoint có thể là Vector3.zero nếu không biết.
    /// </summary>
    private void OnTakeDamage(float delta, Vector3 hitPoint)
    {
        if (delta >= 0f) return; // chỉ xử lý khi nhận sát thương

        float maxH = (cachedMaxHealth > 0f) ? cachedMaxHealth : fallbackMaxHealth;
        float maxS = (cachedMaxShield >= 0f) ? cachedMaxShield : fallbackMaxShield;

        float maxEffectiveHealth = Mathf.Max(1f, maxH + maxS);
        float normalizedDamage = Mathf.Clamp01(Mathf.Abs(delta) / maxEffectiveHealth);

        if (bloodScreenFader != null)
            bloodScreenFader.DoFadeCycle(this, normalizedDamage);

        if (hitPoint != Vector3.zero && damageIndicatorFader != null)
        {
            lastHitPoint = hitPoint;
            damageIndicatorFader.DoFadeCycle(this, 1f); // hiện indicator
        }
    }

    private void Update()
    {
        if (player == null || damageIndicatorFader == null || !damageIndicatorFader.Fading) return;

        Vector3 lookDir = Vector3.ProjectOnPlane(player.forward, Vector3.up).normalized;
        Vector3 dirToHit = Vector3.ProjectOnPlane(lastHitPoint - player.position, Vector3.up).normalized;
        Vector3 right = Vector3.Cross(lookDir, Vector3.up);
        float angle = Vector3.Angle(lookDir, dirToHit) * Mathf.Sign(Vector3.Dot(right, dirToHit));

        if (damageIndicator != null)
        {
            damageIndicator.localEulerAngles = Vector3.forward * angle;
            damageIndicator.localPosition = Quaternion.Euler(0f, 0f, angle) * Vector3.up * indicatorDistance;
        }
    }
}
