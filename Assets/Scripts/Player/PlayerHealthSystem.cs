using UnityEngine;

public class PlayerHealthSystem : BaseHealthSystem, IDamageable
{
    public enum PlayerClass
    {
        Sniper,
        Soldier,
        Tanker
    }

    [Header("Data")]
    [SerializeField] private PlayerStats stats; // Kéo SO PlayerStats vào đây để dùng chỉ số từ asset

    [Header("Player Class Settings (fallback khi không có stats)")]
    [SerializeField] private PlayerClass playerClass = PlayerClass.Soldier;

    [Header("Shield Settings (runtime)")]
    [SerializeField] private float maxShield = 0f;
    [SerializeField] private float currentShield = 0f;
    public float MaxShield => maxShield;
    public float CurrentShield => currentShield;

    // Hồi giáp (có thể lấy từ stats)
    [SerializeField] private float shieldRegenPerSecond = 0f; // 0 = tắt
    [SerializeField] private float shieldRegenDelay = 3f;
    private float _lastDamageTime = -999f;

    [Header("Movement Settings")]
    public float baseMoveSpeed = 5f;   // sẽ sync với stats.walkSpeed nếu có
    private float currentMoveSpeed;

    // UI: (current, max)
    public event System.Action<float, float> OnShieldChanged;

    // NEW — Kéo 2 thanh UI vào đây trong Inspector
    [Header("UI Binding")]
    [SerializeField] private BarUI healthBar;
    [SerializeField] private BarUI shieldBar;

    protected override void Start()
    {
        // đặt chỉ số từ Stats hoặc Class (giữ nguyên)
        if (stats != null) ApplyStatsFromSO(stats);
        else ApplyFallbackByClass(playerClass);

        currentShield = Mathf.Clamp(currentShield <= 0 ? maxShield : currentShield, 0f, maxShield);

        base.Start();          // Base sẽ init currentHealth = maxHealth + bắn OnHealthChanged
        BroadcastShield();     // bắn OnShieldChanged(currentShield, maxShield)

        // NEW — đăng ký sự kiện để đẩy giá trị sang BarUI
        OnHealthChanged += HandleHealthChanged;
        OnShieldChanged += HandleShieldChanged;

        // NEW — cập nhật UI lần đầu (phòng khi UI bật sau Start)
        HandleHealthChanged(currentHealth, maxHealth);
        HandleShieldChanged(currentShield, maxShield);
    }

    private void OnDisable()
    {
        // NEW — hủy đăng ký
        OnHealthChanged -= HandleHealthChanged;
        OnShieldChanged -= HandleShieldChanged;
    }

    // NEW — đẩy giá trị health sang BarUI
    private void HandleHealthChanged(float current, float max)
    {
        if (healthBar == null) return;
        healthBar.maxValue = Mathf.Max(1f, max);
        healthBar.SetValue(current);
    }

    // NEW — đẩy giá trị shield sang BarUI
    private void HandleShieldChanged(float current, float max)
    {
        if (shieldBar == null) return;
        shieldBar.maxValue = Mathf.Max(0f, max);
        shieldBar.SetValue(current);
    }

    private void Update()
    {
        // Hồi giáp nếu bật và đã qua delay
        if (shieldRegenPerSecond > 0f && Time.time >= _lastDamageTime + Mathf.Max(0f, shieldRegenDelay))
        {
            if (currentShield < maxShield)
            {
                currentShield = Mathf.Min(maxShield, currentShield + shieldRegenPerSecond * Time.deltaTime);
                BroadcastShield();
            }
        }
    }

    // ====== Stats loading ======
    private void ApplyStatsFromSO(PlayerStats s)
    {
        // Vitals
        maxHealth = Mathf.Max(1f, s.maxHealth);
        maxShield = Mathf.Max(0f, s.maxShield);
        shieldRegenPerSecond = Mathf.Max(0f, s.shieldRegenPerSecond);
        shieldRegenDelay = Mathf.Max(0f, s.shieldRegenDelay);

        // Movement
        if (s.walkSpeed > 0f) baseMoveSpeed = s.walkSpeed;
        // (Nếu muốn dùng runSpeed/jumpHeight… bạn có thể sync ở các hệ movement khác)
    }

    private void ApplyFallbackByClass(PlayerClass cls)
    {
        // Fallback mặc định (khi chưa gán PlayerStats)
        maxHealth = 100f;

        switch (cls)
        {
            case PlayerClass.Sniper:
                maxShield = 20f;
                baseMoveSpeed = 6f;
                break;
            case PlayerClass.Soldier:
                maxShield = 40f;
                baseMoveSpeed = 5f;
                break;
            case PlayerClass.Tanker:
                maxShield = 60f;
                baseMoveSpeed = 4f;
                break;
        }
        // Không set regen khi không có stats (giữ giá trị serialized hiện có)
    }

    // ====== API ======
    public float GetCurrentMoveSpeed(bool isHeavyWeapon)
    {
        return isHeavyWeapon ? baseMoveSpeed * 0.8f : baseMoveSpeed;
    }

    public override void TakeDamage(float damage)
    {
        if (damage <= 0f) return;
        _lastDamageTime = Time.time;

        float remainingDamage = damage;

        if (currentShield > 0f)
        {
            float shieldAbsorb = Mathf.Min(currentShield, remainingDamage);
            currentShield -= shieldAbsorb;
            remainingDamage -= shieldAbsorb;
            BroadcastShield(); // sẽ kích hoạt HandleShieldChanged -> cập nhật thanh giáp
        }

        if (remainingDamage > 0f)
        {
            base.TakeDamage(remainingDamage); // Base sẽ BroadcastHealth -> cập nhật thanh máu
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
        Debug.Log("Player Dead!");
        // EventBus.PlayerDied(); // nếu có
        GameManager.Instance.ChangeState(GameManager.GameState.GameOver);
        // Không gọi base.Die() vì base là abstract
    }


}
