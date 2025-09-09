using UnityEngine;
using System.Collections;

public class PlayerHealthSystem : BaseHealthSystem, IDamageable
{
    public enum PlayerClass { Sniper, Soldier, Tanker }

    [Header("Data")]
    [SerializeField] private PlayerStats stats;       // SO chỉ số
    [SerializeField] private PlayerClass playerClass = PlayerClass.Soldier;

    [Header("Shield Settings")]
    [SerializeField] private float maxShield = 0f;
    [SerializeField] private float currentShield = 0f;
    public float MaxShield => maxShield;
    public float CurrentShield => currentShield;

    [Tooltip("Hồi giáp mỗi giây (0 = tắt).")]
    [SerializeField] private float shieldRegenPerSecond = 0f;
    [Tooltip("Trễ hồi giáp sau khi nhận sát thương.")]
    [SerializeField] private float shieldRegenDelay = 3f;

    [Header("Movement Settings")]
    public float baseMoveSpeed = 5f;   // sync với stats.walkSpeed nếu có

    // UI events (riêng của PlayerHealthSystem)
    public event System.Action<float, float> OnShieldChanged;

    [Header("UI Binding")]
    [SerializeField] private BarUI healthBar;
    [SerializeField] private BarUI shieldBar;
    private DeathUI deathUI;

    // -------- Animation / Death ----------
    [Header("Body Animator (optional)")]
    [Tooltip("Animator của thân nhân vật (tuỳ chọn).")]
    public Animator animator;
    [Tooltip("Trigger để phát anim chết (vd: DieBack). Bỏ trống nếu không dùng.")]
    public string deathTriggerName = "DieBack";

    [Header("Control on Death")]
    [Tooltip("Tự tắt CharacterController khi chết (không bắt buộc nếu chỉ xoay 1/4 vòng).")]
    public bool disableCharacterControllerOnDeath = false;

    [Tooltip("Các script sẽ bị tắt khi chết (PlayerMovement/Look/WeaponSwitcher…).")]
    public MonoBehaviour[] disableOnDeath;

    [Header("Multi Animator (điều khiển nhiều Animator cùng lúc)")]
    [SerializeField] private MultiAnimatorController multi;   // kéo trong Inspector hoặc auto-find ở Awake

    [Header("Simple Quarter-Fall (no ragdoll)")]
    [Tooltip("Bật kiểu ngã đơn giản: xoay 90° về sau, camera (con) sẽ nhìn lên trời.")]
    public bool simpleQuarterFall = true;
    [Tooltip("Thời gian xoay 1/4 vòng (giây).")]
    public float fallDuration = 0.8f;
    [Tooltip("Góc xoay về sau (để nhìn lên trời: -90).")]
    public float fallBackDegrees = -90f;
    [Tooltip("Đường cong easing cho chuyển động.")]
    public AnimationCurve fallEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // ------------- runtime -------------
    private float _lastDamageTime = -999f;
    private CharacterController _cc;
    private bool _dead = false;
    private Vector3 _lastHitDir = Vector3.zero; // lưu hướng đòn gần nhất (dùng nếu muốn mở rộng)

    // ------------- lifecycle -----------
    void Awake()
    {
        if (!multi) multi = GetComponentInChildren<MultiAnimatorController>(true);
    }

    protected override void Start()
    {
        // Stats
        if (stats != null) ApplyStatsFromSO(stats);
        else ApplyFallbackByClass(playerClass);

        currentShield = Mathf.Clamp(currentShield <= 0 ? maxShield : currentShield, 0f, maxShield);

        _cc = GetComponent<CharacterController>();

        base.Start();      // Base init currentHealth = maxHealth + bắn OnHealthChanged (ở base)
        BroadcastShield(); // bắn OnShieldChanged(currentShield, maxShield)

        // Subscribe UI
        OnHealthChanged += HandleHealthChanged;   // event từ BaseHealthSystem — chỉ subscribe, không Invoke
        OnShieldChanged += HandleShieldChanged;

        // Tự lấy UI (nếu có SelectorSpawner)
        if (SelectorSpawner.Instance != null)
        {
            healthBar = SelectorSpawner.Instance.HealthBar ?? healthBar;
            shieldBar = SelectorSpawner.Instance.ShieldBar ?? shieldBar;
        }
        deathUI = FindAnyObjectByType<DeathUI>(FindObjectsInactive.Include);

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
        if (_dead) return;

        // Regen shield
        if (shieldRegenPerSecond > 0f && Time.time >= _lastDamageTime + Mathf.Max(0f, shieldRegenDelay))
        {
            if (currentShield < maxShield)
            {
                currentShield = Mathf.Min(maxShield, currentShield + shieldRegenPerSecond * Time.deltaTime);
                BroadcastShield();
            }
        }
    }

    // -------- UI Handlers ----------
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

    // -------- Stats loading ----------
    private void ApplyStatsFromSO(PlayerStats s)
    {
        maxHealth = Mathf.Max(1f, s.maxHealth);
        maxShield = Mathf.Max(0f, s.maxShield);
        shieldRegenPerSecond = Mathf.Max(0f, s.shieldRegenPerSecond);
        shieldRegenDelay = Mathf.Max(0f, s.shieldRegenDelay);

        if (s.walkSpeed > 0f) baseMoveSpeed = s.walkSpeed;
    }

    private void ApplyFallbackByClass(PlayerClass cls)
    {
        maxHealth = 100f;
        switch (cls)
        {
            case PlayerClass.Sniper: maxShield = 20f; baseMoveSpeed = 6f; break;
            case PlayerClass.Soldier: maxShield = 40f; baseMoveSpeed = 5f; break;
            case PlayerClass.Tanker: maxShield = 60f; baseMoveSpeed = 4f; break;
        }
    }

    // -------- API ----------
    public float GetCurrentMoveSpeed(bool isHeavyWeapon) => isHeavyWeapon ? baseMoveSpeed * 0.8f : baseMoveSpeed;

    // Hỗ trợ TakeDamage(hitPoint)
    public override void TakeDamage(float damage) => TakeDamage(damage, Vector3.zero);

    public override void TakeDamage(float damage, Vector3 hitPoint)
    {
        if (damage <= 0f || _dead) return;
        _lastDamageTime = Time.time;

        if (hitPoint != Vector3.zero)
        {
            Vector3 center = transform.position + Vector3.up * 0.9f;
            _lastHitDir = (center - hitPoint).normalized;
        }

        float remainingDamage = damage;

        // Shield absorb trước
        if (currentShield > 0f)
        {
            float shieldAbsorb = Mathf.Min(currentShield, remainingDamage);
            currentShield -= shieldAbsorb;
            remainingDamage -= shieldAbsorb;
            BroadcastShield();
        }

        if (remainingDamage > 0f)
        {
            // Base sẽ tự Invoke các event của nó
            base.TakeDamage(remainingDamage, hitPoint);

            // (tuỳ chọn) hiệu ứng hit cho toàn bộ animator
            if (multi) multi.SetTriggerAll("Hit");
        }
        else
        {
            // Shield hấp thụ hết — cập nhật UI health trực tiếp
            HandleHealthChanged(currentHealth, maxHealth);
        }
    }

    public void AddShield(float amount)
    {
        if (amount <= 0f) return;
        currentShield = Mathf.Clamp(currentShield + amount, 0f, maxShield);
        BroadcastShield();
    }

    private void BroadcastShield() => OnShieldChanged?.Invoke(currentShield, maxShield);

    // -------- Death ----------
    protected override void Die()
    {
        if (_dead) return;
        _dead = true;
        Debug.Log("Player Dead!");

        // 1) Tắt input/CC theo cấu hình
        if (disableCharacterControllerOnDeath && _cc) _cc.enabled = false;
        if (disableOnDeath != null)
            foreach (var m in disableOnDeath) if (m) m.enabled = false;

        // 2) Gửi lệnh cho các Animator qua MULTI (nếu có)
        if (multi)
        {
            // Cài cờ chung (nếu bạn có tham số IsDead ở tất cả controller)
            multi.SetBoolAll("IsDead", true);

            // Crossfade về state Death (nếu có)
            multi.CrossFadeAll("Death", 0.15f);

            // Trigger chuyên biệt (nếu controller có)
            multi.SetTriggerAll("DieBack");
        }

        // 3) (tuỳ chọn) Animator thân riêng
        if (animator != null && !string.IsNullOrEmpty(deathTriggerName))
        {
            try
            {
                animator.applyRootMotion = true;
                animator.SetTrigger(deathTriggerName);
            }
            catch { /* ignore */ }
        }

        // 4) Xoay 1/4 vòng về sau (ưu tiên)
        if (simpleQuarterFall)
            StartCoroutine(SimpleQuarterFallRoutine());

        // 5) Game Over
        if (GameManager.Instance != null)
            GameManager.Instance.ChangeState(GameManager.GameState.GameOver);
    }

    // -------- Respawn ----------
    public void Respawn(Vector3 position, Quaternion rotation, bool fullHeal = true)
    {
        _dead = false;

        if (_cc) _cc.enabled = true;
        if (animator) animator.enabled = true;

        if (disableOnDeath != null)
            foreach (var m in disableOnDeath) if (m) m.enabled = true;

        transform.SetPositionAndRotation(position, rotation);

        // Reset toàn bộ Animator qua MULTI
        if (multi)
        {
            multi.SetBoolAll("IsDead", false);
            multi.PlayAll("Idle", 0, 0f); // về Idle nếu có
            multi.ResetTriggerAll("DieBack");
            multi.ResetTriggerAll("Hit");
        }

        if (fullHeal)
        {
            currentHealth = maxHealth;
            currentShield = maxShield;
        }
        BroadcastShield();
        HandleHealthChanged(currentHealth, maxHealth);
    }

    // -------- 1/4 vòng cung xoay về sau ----------
    private IEnumerator SimpleQuarterFallRoutine()
    {
        Quaternion startRot = transform.rotation;
        Quaternion targetRot = startRot * Quaternion.AngleAxis(fallBackDegrees, transform.right);

        float t = 0f;
        float dur = Mathf.Max(0.0001f, fallDuration);

        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float k = fallEase != null ? fallEase.Evaluate(Mathf.Clamp01(t)) : Mathf.Clamp01(t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, k);
            yield return null;
        }

        transform.rotation = targetRot; // chốt góc cuối: nhìn lên trời
    }

    [ContextMenu("Test")] //Test
    public void Respawn()
    {
        CoinManager.Instance.RemoveCoins(Random.Range(100, 800)); // sự trừng phạt

        currentHealth = maxHealth;
        BroadcastHealth();

        currentShield = maxShield;
        BroadcastShield();

        GetComponent<PlayerMovement>().enabled = true; //Test
    }
}
