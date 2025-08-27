using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class DamageEvent : UnityEvent<float, Vector3> { } // delta (<0) và hitPoint

public abstract class BaseHealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] protected float maxHealth = 100f;
    protected float currentHealth;

    [Header("Armor (Shield) Settings")]
    [SerializeField] protected float armor = 0f;

    // UI / gameplay events
    public event Action<float, float> OnHealthChanged; // (current, max)
    public event Action<float> OnArmorChanged;         // (current)
    [Header("Events")]
    public DamageEvent OnTakeDamage = new DamageEvent(); // (delta, hitPoint) — delta âm khi nhận dmg

    // Public read-only properties (cho UI khác đọc)
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float Armor => armor;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        BroadcastHealth();
        BroadcastArmor();
    }

    /// <summary>
    /// Giữ overload cũ — nếu caller không có hitPoint, dùng Vector3.zero
    /// </summary>
    public virtual void TakeDamage(float damage)
    {
        TakeDamage(damage, Vector3.zero);
    }

    /// <summary>
    /// Gọi khi nhận sát thương; armor hấp thụ trước, phần dư trừ vào máu.
    /// Phát sự kiện OnTakeDamage (delta âm) cho UI như DamagedUI.
    /// </summary>
    public virtual void TakeDamage(float damage, Vector3 hitPoint)
    {
        float incoming = Mathf.Max(0f, damage);
        if (incoming <= 0f) return;

        Debug.Log($"{gameObject.name} nhận {incoming} sát thương");

        // 1) Armor hấp thụ trước
        float absorbedByArmor = Mathf.Min(armor, incoming);
        if (absorbedByArmor > 0f)
        {
            armor -= absorbedByArmor;
            BroadcastArmor();
        }

        // 2) Phần còn lại trừ vào máu
        float remaining = incoming - absorbedByArmor;
        if (remaining > 0f)
        {
            float before = currentHealth;
            currentHealth = Mathf.Max(0f, currentHealth - remaining);
            BroadcastHealth();

            // 3) Chết khi về 0 (sau thay đổi từ >0 về 0)
            if (before > 0f && currentHealth <= 0f)
            {
                Die();
            }
        }

        // 4) Thông báo cho UI bị thương (delta âm; dùng tổng sát thương nhận)
        OnTakeDamage?.Invoke(-incoming, hitPoint);
    }

    public virtual void Heal(float amount)
    {
        if (amount <= 0f || currentHealth <= 0f) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
        BroadcastHealth();
    }

    // Tuỳ chọn: thêm API quản lý armor
    public virtual void AddArmor(float amount)
    {
        if (amount <= 0f) return;
        armor += amount;
        BroadcastArmor();
    }

    public virtual void SetArmor(float value)
    {
        armor = Mathf.Max(0f, value);
        BroadcastArmor();
    }

    protected void BroadcastHealth()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    protected void BroadcastArmor()
    {
        OnArmorChanged?.Invoke(armor);
    }

    protected abstract void Die();
}
