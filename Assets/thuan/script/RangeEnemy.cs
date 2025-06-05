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
    public float bulletSpeed;

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

        // Face the player
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
        }
        animator.SetTrigger("rangeattack");
    }

    void MeleeAttack()
    {
        // You can add animations and damage logic here
        Debug.Log("Enemy uses melee attack!");
        animator.SetTrigger("meleeattack");
    }
}
