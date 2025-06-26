using UnityEngine;
using UnityEngine.AI;

public class RangeEnemy : MonoBehaviour
{
    public Transform player;
    public float stopDistance = 10f;
    public float meleeDistance = 2f;
    public float moveSpeed = 3.5f;
    public float fireRate = 1f;

    public GameObject rangedProjectile;
    public Transform firePoint;
    public float bulletSpeed = 10f;
    public float bulletTimelife = 7f;

    private float nextFireTime = 0f;
    private NavMeshAgent agent;

    public Animator animator;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stopDistance;
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        
        

        if (distance <= meleeDistance)
        {
            agent.isStopped = true;
            MeleeAttack();
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
    }

    //void MeleeAttack()
    //{
        
    //    Debug.Log("Enemy uses melee attack!");
    //    animator.SetTrigger("meleeattack");
    //    GameObject player = GameObject.FindGameObjectWithTag("Player");
    //    var health = player.GetComponent<PlayerHealth>();
    //    health.TakeDamage(0.1f, 0, transform.position);

    //}
    void MeleeAttack()
    {

        Debug.Log("Enemy uses melee attack!");
        animator.SetTrigger("meleeattack");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        var health = player.GetComponent<testPlayerHealth>();
        health.TakeDamage(1);

    }
}
