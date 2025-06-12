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
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); 
        Debug.Log(gameObject.name + " nhận " + amount + " sát thương. Máu hiện tại: " + currentHealth);
        

        if (currentHealth <= 0)
        {
            //animator.SetBool("isAlive",false);
            StopAttack();
            StartCoroutine(Die());
        }
    }

    
    IEnumerator Die()
    {
        animator.SetBool("isAlive", false);
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

