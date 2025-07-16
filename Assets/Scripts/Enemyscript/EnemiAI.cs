using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Linq;
using NUnit;

public class EnemiAI : MonoBehaviour
{
    public Transform player;
    NavMeshAgent agent;
    Animator enemyAnimation;


    public float maxHealth = 100f;
    public float currentHealth;
    public int attackDamage = 15;
    public float attackSpeed = 1.5f;
    private float nextAttackTime = 0f;

    public float attackCooldown = 1.5f;
    private float lastAttackTime;
    public float attackRange = 2f;
    public float chaseRange = 40f;


    //new
    public float detectionRadius = 10f;
    private Transform currentTarget;
    public float moveSpeed = 4f;
    public LayerMask targetLayer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyAnimation = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
        //agent.destination = player.position;
        enemyAnimation.SetFloat("speed", agent.velocity.magnitude);

        

        FindClosestTarget();

        if (currentTarget != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.position);

            if (distance > attackRange)
            {
                ChaseTarget();
            }
            else
            {
                Attack();
            }
        }
    }
    

    
    void Attack()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            // damage player()
            player.GetComponent<PlayerHealth>().TakeDamage(10, 0,this.transform.position);
            Debug.Log("Enemy attacks the player!");
        }
        enemyAnimation.SetTrigger("attack");
    }


    void FindClosestTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, targetLayer);
        if (hits.Length == 0)
        {
            currentTarget = null;
            return;
        }

        Transform closest = hits
            .OrderBy(h => Vector3.Distance(transform.position, h.transform.position))
            .First().transform;

        currentTarget = closest;
    }

    void ChaseTarget()
    {
        Vector3 direction = (currentTarget.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        transform.LookAt(currentTarget); // Optional: face the target
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
