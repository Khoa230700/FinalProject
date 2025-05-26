using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Values")]
    public float maxHealth = 100;
    private float currentHealth;

    [Header("Regeneration")]
    public bool useRegen;
    public float regenRate = 5f;
    // public float regenDelay = 3f;
    public float secPerRegen = 1f;

    [Header("References")]
    [SerializeField] private BarUI barUI;
    [SerializeField] private Shield shield;

    // Events
    public delegate void OnTakeDamageEvent(float delta, Vector3 hitPoint);
    public event OnTakeDamageEvent OnTakeDamage;

    private void Start()
    {
        currentHealth = maxHealth;
        shield ??= GetComponent<Shield>();
        if (useRegen) StartCoroutine(RegenRoutine());
        UpdateUI();
    }

    //Test
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            TakeDamage(Random.Range(10f, 300f));
        if (Input.GetKeyDown(KeyCode.X))
            TakeDamage(Random.Range(10f, 30f), 30f); // 30% xuyên giáp
        if (Input.GetKeyDown(KeyCode.H))
            RestoreHealth(Random.Range(10f, 30f));
    }

    public void TakeDamage(float damage, float penetrationPercent = 0f, Vector3 hitPoint = default)
    {
        penetrationPercent = Mathf.Clamp01(penetrationPercent / 100f);

        float damageThroughShield = damage * (1f - penetrationPercent);
        float damageBypassShield = damage * penetrationPercent;

        float leftoverDamage = (shield != null && shield.HasShield())
            ? shield.TakeDamage(damageThroughShield)
            : damageThroughShield;

        float finalHealthDamage = leftoverDamage + damageBypassShield;

        currentHealth = Mathf.Clamp(currentHealth - finalHealthDamage, 0, maxHealth);
        UpdateUI();

        OnTakeDamage?.Invoke(-finalHealthDamage, hitPoint); // Events

        if (currentHealth <= 0)
            Death();
    }

    public void RestoreHealth(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UpdateUI();
    }

    private IEnumerator RegenRoutine()
    {
        while (useRegen)
        {
            if (currentHealth < maxHealth)
            {
                // yield return new WaitForSeconds(regenDelay);

                // while (currentHealth < maxHealth)
                // {
                currentHealth = Mathf.Clamp(currentHealth + regenRate, 0, maxHealth);
                UpdateUI();

                // yield return null;
                yield return new WaitForSeconds(1f);
                // }
            }
            else
            {
                yield return null;
            }
        }
    }

    private void UpdateUI()
    {
        barUI?.SetValue(currentHealth);
    }

    private void Death()
    {
        Debug.Log("DEATH!");
    }
}
