using UnityEngine;
using System.Collections;

public class BossHealth : MonoBehaviour
{
    [SerializeField]private float maxHealth = 100f;
    [SerializeField]private GameObject endUI;
    [SerializeField]private BarUI healthBar;
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
        healthBar.SetValue(currentHealth);
        healthBar.SetMaxValue(maxHealth);
        soundController = GetComponent<EnemySoundController>();
    }
    

    public void TakeDamage(float amount)
    {
        Debug.Log("Boss ăn dmg");
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);
        healthBar.SetValue(currentHealth);

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


        endUI.SetActive(true);
        gameObject.SetActive(false);
    }
}
