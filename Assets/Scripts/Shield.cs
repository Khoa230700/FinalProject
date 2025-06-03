using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class Shield : MonoBehaviour
{
    [Header("Values")]
    public float maxShield = 100;
    private float currentShield;

    [Header("Regeneration")]
    public bool useRegen;
    public float regenRate;
    public float regenDelay;
    public float secPerRegen;

    [Header("References")]
    [SerializeField] private BarUI barUI;

    private Coroutine regenRoutine;

    private void Start()
    {
        UpdateShield(maxShield);
    }

    //* Test
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
            UpdateShield(Random.Range(10f, 30f));
    }

    //* Nhận sát thương và trả về lượng sát thương dư thừa
    public float TakeDamage(float damage)
    {
        //* Khởi động tái tạo khi nhận sát thương
        if (regenRoutine != null) StopCoroutine(regenRoutine);
        if (useRegen) regenRoutine = StartCoroutine(RegenRoutine());

        if (currentShield <= 0) return damage;

        float damageAbsorbed = Mathf.Min(damage, currentShield); //* Sát thương hấp thụ bởi lá chắn
        UpdateShield(-damageAbsorbed);

        return damage - damageAbsorbed; //* Sát thương dư thừa
    }

    //* Tái tạo lá chắn theo thời gian
    private IEnumerator RegenRoutine()
    {
        yield return new WaitForSeconds(regenDelay);

        while (useRegen && currentShield < maxShield)
        {
            yield return new WaitForSeconds(secPerRegen);

            UpdateShield(regenRate);
        }

        regenRoutine = null;
    }

    //* Hồi lá chắn (+ hồi, - trừ)
    public void UpdateShield(float amount)
    {
        currentShield = Mathf.Clamp(currentShield + amount, 0, maxShield);
        UpdateUI();
    }

    private void UpdateUI()
    {
        barUI.SetValue(currentShield);
    }

    public bool HasShield() => currentShield > 0;
}
