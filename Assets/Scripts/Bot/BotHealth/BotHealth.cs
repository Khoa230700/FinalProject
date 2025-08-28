using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class BotHealth : HealthBase  //IDamageable
{
    [Header("Regeneration")]
    public bool useRegen;
    public float regenRate;
    public float regenDelay;
    public float secPerRegen;
    private Coroutine regenRoutine;
    [SerializeField] private BotShield shield;

    protected override void Start()
    {
        base.Start();
        shield ??= GetComponent<BotShield>();
    }

    //* Nh?n sát th??ng qua lá ch?n và tính toán sát th??ng còn l?i, thêm kh? n?ng xuyên lá ch?n , thêm vào ?i?m va ch?m
    public override void TakeDamage(float damage, float penetrationPercent = 0f, Vector3 hitPoint = default)
    {
        //* Kh?i ??ng tái t?o khi nh?n sát th??ng
        if (regenRoutine != null) StopCoroutine(regenRoutine);
        if (useRegen) regenRoutine = StartCoroutine(RegenRoutine());

        penetrationPercent = Mathf.Clamp01(penetrationPercent / 100f);

        float damageThroughShield = damage * (1f - penetrationPercent); //* Sát th??ng vào lá ch?n
        float damageBypassShield = damage * penetrationPercent; //* Sát th??ng xuyên qua lá ch?n
        float leftoverDamage = (shield != null && shield.HasShield()) //* Sát th??ng còn l?i sau khi lá ch?n h?p th?
        ? shield.TakeDamage(damageThroughShield)
            : damageThroughShield;

        float finalHealthDamage = leftoverDamage + damageBypassShield; //* T?ng sát th??ng vào máu

        OnTakeDamage?.Invoke(-finalHealthDamage, hitPoint); //* G?i s? ki?n khi MÁU nh?n sát th??ng
        //OnTakeDamage?.Invoke(-damage, hitPoint); //* G?i s? ki?n khi nh?n sát th??ng

        UpdateHealth(-finalHealthDamage);

    }

    //* Tái t?o máu theo th?i gian
    private IEnumerator RegenRoutine()
    {
        yield return new WaitForSeconds(regenDelay);

        while (useRegen && currentHealth < maxHealth)
        {
            // // yield return null;
            yield return new WaitForSeconds(secPerRegen);

            UpdateHealth(regenRate);
        }

        regenRoutine = null;
    }

    //* H?i máu (+ h?i, - tr?)
    protected override void UpdateHealth(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
      

        if (currentHealth <= 0)
            Die();
    }

    protected override void Die()
    {
        Debug.Log("Die!");
    }

    public void TakeDamage(int amount)
    {
       
    }
}
