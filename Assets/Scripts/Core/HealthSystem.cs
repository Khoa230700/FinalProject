using System;
using UnityEngine;

public abstract class BaseHealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] protected float maxHealth = 100f;
    protected float currentHealth;

    [Header("Armor Settings")]
    [SerializeField] protected float armor = 0f;

    public event Action<float, float> OnHealthChanged;
    public event Action<float> OnArmorChanged;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        BroadcastHealth();
    }

    public virtual void TakeDamage(float damage)
    {
        float remainingDamage = damage;
        if(armor > 0)
        {
            float armorAbsorb = Mathf.Min(armor, remainingDamage);
            armor -= armorAbsorb;
            remainingDamage -= armorAbsorb;
            BroadcastArmor();
        }
        if(remainingDamage > 0)
        {
            currentHealth -= remainingDamage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            BroadcastHealth();
        }
        if (currentHealth < 0)
            Die();
    }  
    
    public virtual void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        BroadcastHealth();
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
