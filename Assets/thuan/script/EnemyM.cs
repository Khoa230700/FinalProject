using UnityEngine;
using System.Collections;

public class EnemyM : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public Animator animator;

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
            animator.SetBool("isAlive",false);
            StopAttack();
            StartCoroutine(Die());
        }
    }

    //void Die()
    //{
    //    Debug.Log(gameObject.name + " đã chết!");

    //    Destroy(gameObject);
    //    // Tạm thời: gameObject.SetActive(false);
    //}

    // void UpdateHealthBar()
    // {
    //     if (healthBar != null)
    //     {
    //         healthBar.fillAmount = currentHealth / maxHealth;
    //     }
    // }
    IEnumerator Die()
    {
        
        yield return new WaitForSeconds(2);
        Destroy(gameObject);
    }


    public void StopAttack()
    {
        var attackScripts = new MonoBehaviour[]
        {
            GetComponent<EnemiAI>(),
            GetComponent<RangeEnemy>(),
            GetComponent<suicideEnemy>()
        };
        foreach (var script in attackScripts)
        {
            if (script != null)
                script.enabled = false;
        }
    }
}

