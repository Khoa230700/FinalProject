using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class RangeEnemy : MonoBehaviour
{
    public Transform player;
    public float stopDistance = 10f;
    public float meleeDistance = 2f;
    
    public float fireRate = 1f;

    public GameObject rangedProjectile;
    public Transform firePoint;
    public float bulletSpeed = 10f;
    public float bulletTimelife = 7f;

    private float nextFireTime = 0f;
    private NavMeshAgent agent;

    public Animator animator;

    //
    public float detectionRadius = 10f;
    private Transform currentTarget;
    public float moveSpeed = 4f;
    public LayerMask targetLayer;

    //sound
    private EnemySoundController soundController;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stopDistance;

        soundController = GetComponent<EnemySoundController>();
        if (player == null)
        {
            player = GameObject.FindWithTag("Player").transform;
        }
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        
        

        if (distance <= meleeDistance)
        {
            agent.isStopped = true;
            //MeleeAttack();
        }
        else if (distance <= stopDistance)
        {
            agent.isStopped = true;
            RangedAttack();
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }

        
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
            transform.forward = direction;
    }

    void RangedAttack()
    {
        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + 1f / fireRate;
            var bullet = Instantiate(rangedProjectile, firePoint.position, firePoint.rotation);
            bullet.GetComponent<Rigidbody>().AddForce(firePoint.forward * bulletSpeed);
            Destroy(bullet, bulletTimelife);
            
        }
        animator.SetTrigger("rangeattack");
        soundController.PlayAttackSound();
    }

    //void MeleeAttack()
    //{
        
    //    Debug.Log("Enemy uses melee attack!");
    //    animator.SetTrigger("meleeattack");
    //    GameObject player = GameObject.FindGameObjectWithTag("Player");
    //    var health = player.GetComponent<PlayerHealth>();
    //    health.TakeDamage(0.1f, 0, transform.position);

    //}
    //void MeleeAttack()
    //{

    //    Debug.Log("Enemy uses melee attack!");
    //    animator.SetTrigger("meleeattack");
    //    GameObject player = GameObject.FindGameObjectWithTag("Player");
    //    var health = player.GetComponent<testPlayerHealth>();
    //    health.TakeDamage(1);

    //}

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
