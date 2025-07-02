using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class suicideEnemy : MonoBehaviour
{
    //public Transform player;
    NavMeshAgent agent;
    Animator enemyAnimation;

   
    
    public int attackDamage = 15;
    public float attackSpeed = 1.5f;
    private float nextAttackTime = 0f;

    public float attackCooldown = 4f;
    private float lastAttackTime;
    public float attackRange = 2f;
    public float chaseRange = 10f;

    public GameObject explosion;
    public float explosionlifetime = 5f;

    //
    public float detectionRadius = 10f;
    private Transform currentTarget;
    public float moveSpeed = 4f;
    public LayerMask targetLayer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyAnimation = GetComponent<Animator>();
        //float distance = Vector3.Distance(transform.position, player.position);
        //if (distance <= attackRange)
        //{
        //    Attack();
        //}
    }

    void Update()
    {
        
        enemyAnimation.SetFloat("speed", agent.velocity.magnitude);

        

        //if (distance <= chaseRange)
        //{
        //agent.SetDestination(player.position);

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
        //}
        //else
        //{
        //    agent.ResetPath(); 
        //}
    }



    void Attack()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            // damage player()
            Debug.Log("Enemy attacks the player!");


            Collider[] colliders = Physics.OverlapSphere(transform.position, 4f);
            foreach (Collider collider in colliders)
            {
                if (collider.GetComponent<testPlayerHealth>())
                {
                    collider.GetComponent<testPlayerHealth>().TakeDamage(50);
                }
            }

            GameObject explo = Instantiate(explosion, transform.position, transform.rotation);
            Destroy(explo, explosionlifetime);
            enemyAnimation.SetTrigger("attack");
            StartCoroutine(DestroyAfterDelay());
        }
    }
    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }

    //new
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
