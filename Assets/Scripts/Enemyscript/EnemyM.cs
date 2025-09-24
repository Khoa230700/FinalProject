using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class EnemyM : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public Animator animator;

    private EnemyTracker tracker;
    private bool isDead = false;
    //sound
    private EnemySoundController soundController;
    void OnEnable()
    {
        currentHealth = maxHealth;
        isDead = false;

        if (animator != null)
            animator.SetBool("isAlive", true);

        EnableAttackScripts(true);
    }

    void Awake()
    {
        tracker = GetComponent<EnemyTracker>();
        soundController = GetComponent<EnemySoundController>();
    }

    private void Update()
    {
        //test
        if (Input.GetKeyDown(KeyCode.K) && (gameObject.tag != "Spidey"))
        {
            TakeDamage(1000);
        }
        if(Input.GetKeyDown(KeyCode.M) && (gameObject.tag == "Spidey"))
        {
            TakeDamage(1000);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        // Debug.Log(currentHealth);

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth <= 0)
        {
            isDead = true;
            StopAttack();
            StartCoroutine(Die());
        }
    }

    IEnumerator Die()
    {
        animator.SetBool("isAlive", false);

        Debug.Log("Here1");

        GetComponent<EnemyTracker>().Die();
        yield return new WaitForSeconds(2f);

        Debug.Log("Here3");
        gameObject.SetActive(false);
        soundController.PlayDeathSound();
    }

    public void StopAttack()
    {
        EnableAttackScripts(false);

        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false; // optional: disable agent
        }

        var collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false; // tránh nhận đạn tiếp tục
        }
    }

    private void EnableAttackScripts(bool enabled)
    {
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = enabled;

        var collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = true;

        var attackScripts = new MonoBehaviour[]
        {
            GetComponent<EnemiAI>(),
            GetComponent<RangeEnemy>(),
            GetComponent<suicideEnemy>(),
            GetComponent<Spidey>()
        };
        foreach (var script in attackScripts)
        {
            if (script != null)
                script.enabled = enabled;
        }
    }
}
