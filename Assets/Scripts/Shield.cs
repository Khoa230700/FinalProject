using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class Shield : MonoBehaviour
{
    [Header("Values")]
    public float maxShield = 100;
    private float currentShield;

    [Header("Regeneration")]
    public bool useRegen;
    public float regenRate = 5f;
    public float regenDelay = 3f;
    // public float secPerRegen = 1f;

    [Header("References")]
    [SerializeField] private BarUI bar;

    private void Start()
    {
        currentShield = maxShield;
        if (useRegen) StartCoroutine(RegenRoutine());
        UpdateUI();
    }

    //Test
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
            RestoreShield(Random.Range(10f, 30f));
    }

    public float TakeDamage(float damage)
    {
        StopAllCoroutines();
        if (useRegen) StartCoroutine(RegenRoutine());

        float damageAbsorbed = Mathf.Min(damage, currentShield);
        currentShield = Mathf.Clamp(currentShield - damageAbsorbed, 0, maxShield);
        UpdateUI();

        return damage - damageAbsorbed;
    }

    public void RestoreShield(float amount)
    {
        currentShield = Mathf.Clamp(currentShield + amount, 0, maxShield);
        UpdateUI();
    }

    private IEnumerator RegenRoutine()
    {
        while (useRegen)
        {
            if (currentShield < maxShield)
            {
                yield return new WaitForSeconds(regenDelay);

                while (currentShield < maxShield)
                {
                    currentShield = Mathf.Clamp(currentShield + regenRate, 0, maxShield);
                    UpdateUI();

                    yield return null;
                    // yield return new WaitForSeconds(secPerRegen);
                }
            }
            else
            {
                yield return null;
            }
        }
    }

    private void UpdateUI()
    {
        bar?.SetValue(currentShield);
    }
    
    public bool HasShield() => currentShield > 0;
}
