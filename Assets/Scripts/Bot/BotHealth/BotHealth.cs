using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BotHealth : HealthBase, IDamageable
{
    [Header("Regeneration")]
    public bool useRegen;
    public float regenRate;
    public float regenDelay;
    public float secPerRegen;
    private Coroutine regenRoutine;
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Collider mainCollider;
    [SerializeField] private MonoBehaviour[] aiScripts;
    [SerializeField] private BotShield shield;

    protected override void Start()
    {
        base.Start();
        shield ??= GetComponent<BotShield>();
    }

    // Nhận damage (cho cả enemy và súng gọi chung)
    public override void TakeDamage(float damage, float penetrationPercent = 0f, Vector3 hitPoint = default)
    {
        // Reset regen khi bị trúng đòn
        if (regenRoutine != null) StopCoroutine(regenRoutine);
        if (useRegen) regenRoutine = StartCoroutine(RegenRoutine());

        penetrationPercent = Mathf.Clamp01(penetrationPercent / 100f);

        float damageThroughShield = damage * (1f - penetrationPercent); // dame vào shield
        float damageBypassShield = damage * penetrationPercent;        // dame xuyên giáp
        float leftoverDamage = (shield != null && shield.HasShield())
            ? shield.TakeDamage(damageThroughShield)
            : damageThroughShield;

        float finalHealthDamage = leftoverDamage + damageBypassShield;

        OnTakeDamage?.Invoke(-finalHealthDamage, hitPoint);

        UpdateHealth(-finalHealthDamage);
    }

    // Regen máu
    private IEnumerator RegenRoutine()
    {
        yield return new WaitForSeconds(regenDelay);

        while (useRegen && currentHealth < maxHealth)
        {
            yield return new WaitForSeconds(secPerRegen);
            UpdateHealth(regenRate);
        }

        regenRoutine = null;
    }

    protected override void UpdateHealth(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        if (currentHealth <= 0)
            Die();
    }

    protected override void Die()
    {
        Debug.Log("Bot Die!");

        // Stop agent
        if (agent != null) agent.isStopped = true;

        // Disable AI scripts rõ ràng
        foreach (var s in aiScripts)
        {
            if (s != null) s.enabled = false;
        }

        // Disable collider chính
        if (mainCollider != null) mainCollider.enabled = false;

        // Play animation chết
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
    }

    public void TakeDamage(float amount)
    {
        TakeDamage(amount, 0f, Vector3.zero);
    }
}