using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Values")]
    public float maxHealth = 100;
    private float currentHealth;

    [Header("Regeneration")]
    public bool useRegen;
    public float regenRate;
    public float regenDelay;
    public float secPerRegen;

    [Header("References")]
    [SerializeField] private BarUI barUI;
    [SerializeField] private Shield shield;

    // Events
    public UnityEvent<float, Vector3> OnTakeDamage;

    private Coroutine regenRoutine;

    private void Start()
    {
        shield ??= GetComponent<Shield>();
        UpdateHealth(maxHealth);
    }

    //* Test
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            TakeDamage(200f);
        if (Input.GetKeyDown(KeyCode.X))
            TakeDamage(Random.Range(10f, 30f), 30f, Random.insideUnitSphere * 5f); // 30% xuyên lá chắn, điểm va chạm ngẫu nhiên 
        if (Input.GetKeyDown(KeyCode.H))
            UpdateHealth(Random.Range(10f, 30f));
    }

    //* Nhận sát thương qua lá chắn và tính toán sát thương còn lại, thêm khả năng xuyên lá chắn , thêm vào điểm va chạm
    public void TakeDamage(float damage, float penetrationPercent = 0f, Vector3 hitPoint = default)
    {
        //* Khởi động tái tạo khi nhận sát thương
        if (regenRoutine != null) StopCoroutine(regenRoutine);
        if (useRegen) regenRoutine = StartCoroutine(RegenRoutine());

        penetrationPercent = Mathf.Clamp01(penetrationPercent / 100f);

        float damageThroughShield = damage * (1f - penetrationPercent); //* Sát thương vào lá chắn
        float damageBypassShield = damage * penetrationPercent; //* Sát thương xuyên qua lá chắn
        float leftoverDamage = (shield != null && shield.HasShield()) //* Sát thương còn lại sau khi lá chắn hấp thụ
            ? shield.TakeDamage(damageThroughShield)
            : damageThroughShield;

        float finalHealthDamage = leftoverDamage + damageBypassShield; //* Tổng sát thương vào máu

        // OnTakeDamage?.Invoke(-finalHealthDamage, hitPoint); //* Gọi sự kiện khi MÁU nhận sát thương
        OnTakeDamage?.Invoke(-damage, hitPoint); //* Gọi sự kiện khi nhận sát thương

        UpdateHealth(-finalHealthDamage);

    }

    //* Tái tạo máu theo thời gian
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

    //* Hồi máu (+ hồi, - trừ)
    private void UpdateHealth(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UpdateUI();

        if (currentHealth <= 0)
            Death();
    }

    private void UpdateUI()
    {
        barUI.SetValue(currentHealth);
    }

    private void Death()
    {
        Debug.Log("DEATH!");
    }
}
