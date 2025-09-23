using UnityEngine;
using UnityEngine.AI;

public class BotHealth : BaseHealthSystem, IDamageable
{
    [Header("Bot Settings")]
    [SerializeField] private float maxShield = 0f;
    [SerializeField] private float currentShield = 0f;
    public float MaxShield => maxShield;
    public event System.Action OnDamaged;
    public float CurrentShield => currentShield;

    [SerializeField] private float shieldRegenPerSecond = 0f;
    [SerializeField] private float shieldRegenDelay = 3f;
    private float _lastDamageTime = -999f;

    [Header("Movement Settings")]
    [SerializeField] private float baseMoveSpeed = 4f;
    public float BaseMoveSpeed => baseMoveSpeed;
    public Animator anim;
    private bool isDead = false;
    [Header("UI Binding")]
    [SerializeField] private BarUI healthBar;
    [SerializeField] private BarUI shieldBar;
    [SerializeField] private GameObject failedUI;

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
        OnDamaged?.Invoke();
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
        if (isDead) return;
        isDead = true;

        GameObject root = transform.root.gameObject;

        // 1. Ngắt NavMeshAgent nếu có
        var agent = root.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // 2. Disable toàn bộ script trong cây bot
        foreach (var comp in root.GetComponentsInChildren<MonoBehaviour>())
        {
            if (comp != this) comp.enabled = false;
        }

        // 3. Đổi toàn bộ layer sang Default (bot chết không bị tấn công nữa)
        ChangeLayerRecursively(root, LayerMask.NameToLayer("Default"));

        // 4. Play animation chết
        if (anim != null)
        {
            anim.ResetTrigger("Dead"); // clear trước
            anim.SetBool("Block", false); // tắt trạng thái block
            anim.SetTrigger("Dead");      // chạy anim chết
        }

        failedUI.SetActive(true);
    }

    private void ChangeLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            ChangeLayerRecursively(child.gameObject, newLayer);
        }
    }
}