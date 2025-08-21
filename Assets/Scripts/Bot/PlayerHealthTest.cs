using UnityEngine;

public class PlayerHealthTest : MonoBehaviour
{

    public float maxHealth = 100f;
    public float currentHealth;

    public bool IsDown { get; private set; } = false;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        // Ấn phím K để giết player test
        if (Input.GetKeyDown(KeyCode.K) && !IsDown)
        {
            TakeDamage(maxHealth);
        }
    }

    public void TakeDamage(float amount)
    {
        if (IsDown) return;

        currentHealth -= amount;
        Debug.Log("Player took damage: " + amount + " => HP = " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        IsDown = true;
        currentHealth = 0;
        Debug.Log("⚠️ Player Down!");
    }

    public void Revive()
    {
        IsDown = false;
        currentHealth = maxHealth * 0.5f; // hồi 50% máu
        Debug.Log("✅ Player revived! HP = " + currentHealth);
    }
}
