using UnityEngine;

public class BotHealth : BaseHealthSystem, IDamageable
{
    [Header("Bot Settings")]
    [SerializeField] private float maxShield = 0f;
    [SerializeField] private float currentShield = 0f;
    public float MaxShield => maxShield;
    public float CurrentShield => currentShield;

    [SerializeField] private float shieldRegenPerSecond = 0f;
    [SerializeField] private float shieldRegenDelay = 3f;
    private float _lastDamageTime = -999f;

    [Header("Movement Settings")]
    [SerializeField] private float baseMoveSpeed = 4f;
    public float BaseMoveSpeed => baseMoveSpeed;

    [Header("UI Binding")]
    [SerializeField] private BarUI healthBar;
    [SerializeField] private BarUI shieldBar;

    public event System.Action<float, float> OnShieldChanged;

    protected override void Start()
    {
        // khởi tạo máu, giáp
        currentShield = Mathf.Clamp(currentShield <= 0 ? maxShield : currentShield, 0f, maxShield);

        base.Start();       // Base init máu
        BroadcastShield();

        // đăng ký sự kiện đẩy UI
        OnHealthChanged += HandleHealthChanged;
        OnShieldChanged += HandleShieldChanged;

        // cập nhật UI lần đầu
        HandleHealthChanged(currentHealth, maxHealth);
        HandleShieldChanged(currentShield, maxShield);
    }

    private void OnDisable()
    {
        OnHealthChanged -= HandleHealthChanged;
        OnShieldChanged -= HandleShieldChanged;
    }

    private void Update()
    {
        // regen giáp
        if (shieldRegenPerSecond > 0f && Time.time >= _lastDamageTime + Mathf.Max(0f, shieldRegenDelay))
        {
            if (currentShield < maxShield)
            {
                currentShield = Mathf.Min(maxShield, currentShield + shieldRegenPerSecond * Time.deltaTime);
                BroadcastShield();
            }
        }
    }

    // ===== UI handlers =====
    private void HandleHealthChanged(float current, float max)
    {
        if (healthBar == null) return;
        healthBar.maxValue = Mathf.Max(1f, max);
        healthBar.SetValue(current);
    }

    private void HandleShieldChanged(float current, float max)
    {
        if (shieldBar == null) return;
        shieldBar.maxValue = Mathf.Max(0f, max);
        shieldBar.SetValue(current);
    }

    // ===== API =====
    public override void TakeDamage(float damage)
    {
        if (damage <= 0f) return;
        _lastDamageTime = Time.time;

        float remainingDamage = damage;

        // giáp absorb trước
        if (currentShield > 0f)
        {
            float absorb = Mathf.Min(currentShield, remainingDamage);
            currentShield -= absorb;
            remainingDamage -= absorb;
            BroadcastShield();
        }

        // trừ máu
        if (remainingDamage > 0f)
        {
            base.TakeDamage(remainingDamage);
        }
    }

    public void AddShield(float amount)
    {
        if (amount <= 0f) return;
        currentShield = Mathf.Clamp(currentShield + amount, 0f, maxShield);
        BroadcastShield();
    }

    private void BroadcastShield()
    {
        OnShieldChanged?.Invoke(currentShield, maxShield);
    }

    protected override void Die()
    {
        Debug.Log($"{gameObject.name} (Bot) Dead!");
        // có thể gọi BotManager.Instance.OnBotDead(this) nếu cần
        //Destroy(gameObject, 1.5f); // bot chết thì destroy
    }
}