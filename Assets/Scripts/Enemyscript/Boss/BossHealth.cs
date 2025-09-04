using UnityEngine;
using System.Collections;

public class BossHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth { get; private set; }
    
    public bool isPhase2 { get; private set; } = false;

    public delegate void PhaseChangeHandler();
    public event PhaseChangeHandler OnPhase2Enter;

    public Animator animator;
    private bool isDead = false;

    //Sound
    private EnemySoundController soundController;

    private void Start()
    {
        currentHealth = maxHealth;
        soundController = GetComponent<EnemySoundController>();
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

        if (currentHealth <= 0)
        {
            isDead = true;
            StopAttack();
            StartCoroutine(Die());
        }
    }

    //private void EnableAttackScripts(bool enabled)
    //{

    //}

    public void StopAttack()
    {
        //EnableAttackScripts(false);

        var script = GetComponent<BossAi>().enabled = false;
        

        var collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false; // tránh nhận đạn tiếp tục
        }
        soundController.PlayDeathSound();
    }


    IEnumerator Die()
    {
        if (animator != null)
            animator.SetBool("isAlive", false);

        yield return new WaitForSeconds(3f);

       
        gameObject.SetActive(false);
    }
}
