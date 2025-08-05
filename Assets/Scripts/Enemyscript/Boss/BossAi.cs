using UnityEngine;
using UnityEngine.AI;

public class BossAi : MonoBehaviour
{
    public Transform player;
    public NavMeshAgent agent;

    [Header("Ranges")]
    public float detectRange = 20f;
    public float meleeRange = 2f;
    public float slamRange = 5f;

    [Header("Cooldowns")]
    public float rangeAttackCooldown = 5f;
    public float slamAttackCooldown = 10f;

    private float lastRangeAttackTime = 10f;
    private float lastSlamAttackTime;

    //range attack
    public GameObject rangedProjectile;
    public Transform firePoint;
    public float bulletSpeed = 50f;
    public float bulletTimelife = 7f;

    public string playerTag = "Player";
    public Transform playerAimTarget; //aim player

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    void Update()
    {
        

        //Priority 
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > detectRange)
        {
            agent.isStopped = true;
            return;
        }



        bool canSlam = distance <= slamRange && Time.time - lastSlamAttackTime >= slamAttackCooldown;
        bool canRange = Time.time - lastRangeAttackTime >= rangeAttackCooldown;
        bool canMelee = distance <= meleeRange;

        // Priority order: Slam > Range > Melee
        if (canSlam)
        {
            SlamAttack();
            lastSlamAttackTime = Time.time;
        }
        else if (canRange)
        {
            RangeAttack();
            lastRangeAttackTime = Time.time;
        }
        else if (canMelee)
        {
            MeleeAttack();

        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        //Vector3 direction = (player.position - transform.position).normalized;
        //direction.y = 0;
        //if (direction != Vector3.zero)
        //    transform.forward = direction;
    }

    void MeleeAttack()
    {
        agent.isStopped = true;
        // Play melee animation
        Debug.Log("Boss performs melee attack!");
        // Damage logic here
        player.GetComponent<testPlayerHealth>().TakeDamage(10);
    }

    void RangeAttack()
    {
        agent.isStopped = true;
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        // Play ranged attack animation
        Debug.Log("Boss performs range attack!");
        var bullet = Instantiate(rangedProjectile, firePoint.position, firePoint.rotation);
        bullet.GetComponent<Rigidbody>().AddForce(firePoint.forward * bulletSpeed);

        //
        Vector3 targetPosition = player.transform.position + Vector3.up * -9f;
        //direction from firepoint to player
        Vector3 direction = (playerAimTarget.position - firePoint.position).normalized;

        Destroy(bullet, bulletTimelife);
    }

    void SlamAttack()
    {
        agent.isStopped = true;
        // Play slam animation
        Debug.Log("Boss performs slam attack!");
        // Area damage effect here
        Collider[] colliders = Physics.OverlapSphere(transform.position, 4f);
        foreach (Collider collider in colliders)
        {
            if (collider.GetComponent<PlayerHealth>())
            {
                collider.GetComponent<PlayerHealth>().TakeDamage(20);
            }
        }

        //GameObject explo = Instantiate(explosion, transform.position, transform.rotation);
    }
}
