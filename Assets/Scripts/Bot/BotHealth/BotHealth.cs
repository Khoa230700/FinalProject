using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class BotHealth : HealthBase , IDamageable
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

    //* Nh?n s�t th??ng qua l� ch?n v� t�nh to�n s�t th??ng c�n l?i, th�m kh? n?ng xuy�n l� ch?n , th�m v�o ?i?m va ch?m
    public override void TakeDamage(float damage, float penetrationPercent = 0f, Vector3 hitPoint = default)
    {
        //* Kh?i ??ng t�i t?o khi nh?n s�t th??ng
        if (regenRoutine != null) StopCoroutine(regenRoutine);
        if (useRegen) regenRoutine = StartCoroutine(RegenRoutine());

        penetrationPercent = Mathf.Clamp01(penetrationPercent / 100f);

        float damageThroughShield = damage * (1f - penetrationPercent); //* S�t th??ng v�o l� ch?n
        float damageBypassShield = damage * penetrationPercent; //* S�t th??ng xuy�n qua l� ch?n
        float leftoverDamage = (shield != null && shield.HasShield()) //* S�t th??ng c�n l?i sau khi l� ch?n h?p th?
        ? shield.TakeDamage(damageThroughShield)
            : damageThroughShield;

        float finalHealthDamage = leftoverDamage + damageBypassShield; //* T?ng s�t th??ng v�o m�u

        OnTakeDamage?.Invoke(-finalHealthDamage, hitPoint); //* G?i s? ki?n khi M�U nh?n s�t th??ng
        //OnTakeDamage?.Invoke(-damage, hitPoint); //* G?i s? ki?n khi nh?n s�t th??ng

        UpdateHealth(-finalHealthDamage);

    }

    //* T�i t?o m�u theo th?i gian
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

    //* H?i m�u (+ h?i, - tr?)
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

    public void TakeDamage(float amount)
    {
       
    }
}
