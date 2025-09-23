using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;


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
    private EndUI failedUI;

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

    // ==== KNOCKBACK ====
    [Header("Knockback")]
    [SerializeField] private float knockbackGravity = 20f;
    [SerializeField] private float defaultUpForce = 3f;     // lực hất lên
    [SerializeField] private List<Behaviour> disableOnKnockback = new List<Behaviour>(); // KÉO SCRIPT VÀO ĐÂY
    [SerializeField] private bool disableCharacterControllerDuringKnockback = false;

    [Tooltip("Nếu bật: tốc độ ngang (force) được giữ không đổi trong suốt thời gian knockback -> điều khiển quãng đường dễ hơn.")]
    [SerializeField] private bool constantHorizontalSpeed = true;

    private Coroutine knockbackCo;
    private bool _inKnockback;

    // ==== BURN (Damage over Time) ====
    [Header("Damage Over Time")]
    [SerializeField] private float minBurnTickInterval = 1f;
    private Coroutine burnCo;
    private float burnDps;
    private float burnRemain;

    // ------------- runtime -------------
    private float _lastDamageTime = -999f;
    private CharacterController _cc;
    private bool _dead = false;
    private Vector3 _lastHitDir = Vector3.zero; // lưu hướng đòn gần nhất (dùng nếu muốn mở rộng)
    private int deathCount = 0;

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

        // cập nhật UI lần đầu
        HandleHealthChanged(currentHealth, maxHealth);
        HandleShieldChanged(currentShield, maxShield);

        deathCount = 0;
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

    // =================== KNOCKBACK PUBLIC API ===================

    /// <summary>Đẩy ra phía sau lưng player (ngược hướng nhìn).</summary>
    public void ApplyKnockbackBackwards(float force, float duration, float upForce = -1f)
    {
        if (upForce < 0f) upForce = defaultUpForce;
        Vector3 dir = -(transform.forward);
        ApplyKnockback(dir, force, duration, upForce);
    }

    /// <summary>Đẩy xa khỏi một nguồn (ví dụ: vị trí boss).</summary>
    public void ApplyKnockbackFrom(Vector3 sourcePosition, float force, float duration, float upForce = -1f)
    {
        if (upForce < 0f) upForce = defaultUpForce;
        Vector3 dir = (transform.position - sourcePosition);
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f) dir.Normalize();
        else dir = -(transform.forward);
        ApplyKnockback(dir, force, duration, upForce);
    }

    /// <summary>Knockback với đầu vào là tốc độ ngang (m/s). Nếu bật constantHorizontalSpeed, speed giữ nguyên trong suốt duration.</summary>
    public void ApplyKnockback(Vector3 direction, float speed, float duration, float upForce = -1f)
    {
        if (speed <= 0f || duration <= 0f) return;
        if (upForce < 0f) upForce = defaultUpForce;

        if (knockbackCo != null) StopCoroutine(knockbackCo);
        knockbackCo = StartCoroutine(KnockbackRoutine(direction.normalized, speed, duration, upForce));
    }

    /// <summary>Knockback theo QUÃNG ĐƯỜNG mục tiêu (m), nội suy ra tốc độ = distance/duration.</summary>
    public void ApplyKnockbackDistance(Vector3 direction, float distance, float duration, float upForce = -1f)
    {
        float speed = distance / Mathf.Max(0.0001f, duration);
        ApplyKnockback(direction, speed, duration, upForce);
    }

    public void ApplyKnockbackBackwardsDistance(float distance, float duration, float upForce = -1f)
    {
        Vector3 dir = -(transform.forward);
        ApplyKnockbackDistance(dir, distance, duration, upForce);
    }

    public void ApplyKnockbackFromDistance(Vector3 sourcePosition, float distance, float duration, float upForce = -1f)
    {
        Vector3 dir = (transform.position - sourcePosition);
        dir.y = 0f;
        dir = (dir.sqrMagnitude > 0.0001f) ? dir.normalized : -(transform.forward);
        ApplyKnockbackDistance(dir, distance, duration, upForce);
    }

    // =================== KNOCKBACK CORE ===================

    private IEnumerator KnockbackRoutine(Vector3 dir, float speed, float duration, float upForce)
    {
        _inKnockback = true;
        SetBehavioursEnabled(disableOnKnockback, false);

        if (_cc == null) _cc = GetComponent<CharacterController>();
        bool prevCCEnabled = _cc != null ? _cc.enabled : false;
        if (disableCharacterControllerDuringKnockback && _cc) _cc.enabled = false;

        float t = 0f;
        float vy = upForce; // nảy lên một chút

        while (t < duration)
        {
            t += Time.deltaTime;

            // --- THÀNH PHẦN NGANG ---
            Vector3 horiz;
            if (constantHorizontalSpeed)
            {
                // Giữ tốc độ ngang không đổi -> quãng đường ≈ speed * duration
                horiz = dir * speed;
            }
            else
            {
                // Giảm dần (ease-out) như phiên bản cũ
                float k = 1f - Mathf.Clamp01(t / duration);
                horiz = dir * (speed * k);
            }

            // --- THÀNH PHẦN DỌC (trọng lực) ---
            vy -= knockbackGravity * Time.deltaTime;

            Vector3 delta = (horiz + Vector3.up * vy) * Time.deltaTime;

            // --- DỊCH CHUYỂN ---
            if (_cc != null && !disableCharacterControllerDuringKnockback && _cc.enabled)
                _cc.Move(delta);
            else
                transform.position += delta;

            yield return null;
        }

        // hồi lại
        if (disableCharacterControllerDuringKnockback && _cc) _cc.enabled = prevCCEnabled;
        SetBehavioursEnabled(disableOnKnockback, true);
        _inKnockback = false;
    }

    private void SetBehavioursEnabled(List<Behaviour> list, bool enabled)
    {
        if (list == null) return;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null) continue;
            list[i].enabled = enabled;
        }
    }

    // =================== BURN ===================

    // Gọi từ Boss: dps = sát thương mỗi giây, duration = thời gian thiêu đốt
    public void ApplyBurn(float dps, float duration)
    {
        if (dps <= 0f || duration <= 0f) return;

        burnDps = dps;
        burnRemain = duration;

        if (burnCo == null) burnCo = StartCoroutine(BurnRoutine());
    }

    private IEnumerator BurnRoutine()
    {
        float tickAcc = 0f;

        while (burnRemain > 0f && !_dead)
        {
            float dt = Time.deltaTime;
            burnRemain -= dt;
            tickAcc += dt;

            // Gây sát thương mỗi giây (tick), an toàn với FPS thấp/cao
            if (tickAcc >= minBurnTickInterval)
            {
                float ticks = Mathf.Floor(tickAcc / minBurnTickInterval);
                float damage = burnDps * ticks; // 5/s * số tick
                tickAcc -= ticks * minBurnTickInterval;

                // Gây dmg "chuẩn" vào hệ shield/health sẵn có
                TakeDamage(damage, Vector3.zero);
            }

            yield return null;
        }

        burnCo = null;
    }

    // -------- Death ----------
    protected override void Die()
    {
        if (_dead) return;
        _dead = true;
        Debug.Log("Player Dead!");

        deathCount++;

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

        // // 5) Game Over
        // if (GameManager.Instance != null)
        //     GameManager.Instance.ChangeState(GameManager.GameState.GameOver);
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
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName == "Map2")
        {
            deathUI = FindAnyObjectByType<DeathUI>(FindObjectsInactive.Include);
            Debug.Log(deathUI);
            deathUI.Show();
        }
        else
        {
            failedUI = FindObjectsByType<EndUI>(FindObjectsInactive.Include,
                                                FindObjectsSortMode.None)
                                                .FirstOrDefault(e => e.CompareTag("Failed"));
            Debug.Log(failedUI);
            failedUI.gameObject.SetActive(true);
        }
    }

    public int GetDeathCount() => deathCount;

    [ContextMenu("Test")] //Test
    public void Respawn()
    {
        CoinManager.Instance.RemoveCoins(Random.Range(100, 800)); // sự trừng phạt
        Respawn(transform.position, transform.rotation, true);
    }
}
