using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    // public Image healthBar; // Tùy chọn: thanh máu UI

    void Start()
    {
        currentHealth = maxHealth;
        // UpdateHealthBar();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Đảm bảo máu không âm hoặc vượt tối đa
        Debug.Log(gameObject.name + " nhận " + amount + " sát thương. Máu hiện tại: " + currentHealth);
        // UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " đã chết!");
        // Thêm logic game over, hồi sinh, v.v.
        // Tạm thời: gameObject.SetActive(false);
    }

    // void UpdateHealthBar()
    // {
    //     if (healthBar != null)
    //     {
    //         healthBar.fillAmount = currentHealth / maxHealth;
    //     }
    // }
}
