using UnityEngine;
using System.Collections;

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

        // Cho AI dừng hoạt động trước (nếu có)
        var tr = GetComponent<EnemyTracker>();
        if (tr) tr.Die();

        // 1) PHÁT TIẾNG CHẾT TRƯỚC
        float wait = 1.2f; // mặc định fallback
        if (soundController != null)
        {
            soundController.PlayDeathSound();
            if (soundController.deathClip != null)
                wait = soundController.deathClip.length;
        }

        // 2) CHỜ THEO REALTIME (tránh kẹt khi pause)
        float end = Time.realtimeSinceStartup + wait;
        while (Time.realtimeSinceStartup < end) yield return null;

        // 3) TẮT OBJECT SAU KHI PHÁT XONG
        gameObject.SetActive(false);
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
