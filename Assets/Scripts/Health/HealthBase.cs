using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class HealthBase : MonoBehaviour
{
    public float maxHealth = 100;
    protected float currentHealth;
    public UnityEvent<float, Vector3> OnTakeDamage;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
    }

    public abstract void TakeDamage(float damage, float penetrationPercent = 0f, Vector3 hitPoint = default);
    protected abstract void UpdateHealth(float amount);
    protected abstract void Die();
}
