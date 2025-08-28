using UnityEngine;

public class BossHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth { get; private set; }
    
    public bool isPhase2 { get; private set; } = false;

    public delegate void PhaseChangeHandler();
    public event PhaseChangeHandler OnPhase2Enter;

    private void Start()
    {
        currentHealth = maxHealth;
    }
    

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);

        if (!isPhase2 && currentHealth <= maxHealth / 2f)
        {
            isPhase2 = true;
            OnPhase2Enter?.Invoke();
        }
    }
}
