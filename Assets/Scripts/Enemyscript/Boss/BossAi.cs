using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class BossAi : MonoBehaviour
{
    //new version
    public Transform player;
    public NavMeshAgent agent;

    [Header("Ranges")]
    public float detectRange = 20f;
    public float meleeRange = 2f;
    public float slamRange = 5f;
    public float triggerRange = 10f;

    [Header("Cooldowns")]
    public float rangeAttackCooldown = 5f;
    public float slamAttackCooldown = 10f;
    public float fireCooldown = 15f;

    private float lastRangeAttackTime = -10f;
    private float lastSlamAttackTime = -10f;
    private float nextFireTime = 0f;

    [Header("Fire Breath")]
    public float channelTime = 3f;
    public float damagePerSecond = 10f;
    public ParticleSystem fireFX;
    public Collider fireDamageArea;
    private bool isChanneling = false;

    [Header("Ranged Attack")]
    public GameObject rangedProjectile;
    public Transform firePoint;
    public float bulletSpeed = 50f;
    public float bulletTimelife = 7f;
    public Transform playerAimTarget;

    [Header("Misc")]
    public string playerTag = "Player";
    public float rotationSpeed = 5f;

    public Animator enemyAnimation;

    //fix
    private float destinationUpdateRate = 0.5f;
    private float nextDestinationUpdateTime = 0f;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        //enemyAnimation = GetComponent<Animator>();
        fireFX.Stop();
        fireDamageArea.enabled = false;
    }

    private void Update()
    {
        if (isChanneling)
        {
            RotateTowardsTarget(); // Optional: look at player while channeling
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > detectRange)
        {
            agent.isStopped = true;
            return;
        }

        bool canSlam = distance <= slamRange && Time.time - lastSlamAttackTime >= slamAttackCooldown;
        bool canFire = distance <= triggerRange && HasLineOfSight() && Time.time >= nextFireTime;
        bool canMelee = distance <= meleeRange;
        bool canRange = Time.time - lastRangeAttackTime >= rangeAttackCooldown;

        // Attack priority: Slam > FireBreath > Melee > Chase
        if (canSlam)
        {
            SlamAttack();
            lastSlamAttackTime = Time.time;
        }
        else if (canFire)
        {
            agent.isStopped = true;
            StartFireBreath();
            nextFireTime = Time.time + fireCooldown;
        }
        else if (canMelee)
        {
            MeleeAttack();
        }
        else
        {
            // Default: Chase
            agent.isStopped = false;
            //if (!agent.hasPath || agent.remainingDistance < 0.5f)
            //{
            //    agent.SetDestination(player.position);
            //}
            if (Time.time >= nextDestinationUpdateTime)
            {
                agent.SetDestination(player.position);
                nextDestinationUpdateTime = Time.time + destinationUpdateRate;
            }
        }
    }

    // --- Attack Methods ---

    void MeleeAttack()
    {
        //agent.isStopped = true;
        Debug.Log("Boss performs melee attack!");
        player.GetComponent<testPlayerHealth>()?.TakeDamage(10);
    }

    void RangeAttack()
    {
        //agent.isStopped = true;
        //Debug.Log("Boss performs range attack!");
        GameObject bullet = Instantiate(rangedProjectile, firePoint.position, firePoint.rotation);
        bullet.GetComponent<Rigidbody>().AddForce(firePoint.forward * bulletSpeed);
        Destroy(bullet, bulletTimelife);
    }

    void SlamAttack()
    {
        //agent.isStopped = true;
        //Debug.Log("Boss performs slam attack!");
        //Collider[] colliders = Physics.OverlapSphere(transform.position, 4f);
        //foreach (Collider col in colliders)
        //{
        //    if (col.GetComponent<PlayerHealth>())
        //        col.GetComponent<PlayerHealth>().TakeDamage(20);
        //}
        enemyAnimation.SetTrigger("Slam");
    }

    // --- Fire Breath ---

    public void StartFireBreath()
    {
        if (isChanneling) return;

        isChanneling = true;
        fireFX.Play();
        fireDamageArea.enabled = true;

        Invoke(nameof(StopFireBreath), channelTime);
        enemyAnimation.SetBool("FireBreath",true);
    }

    void StopFireBreath()
    {
        fireFX.Stop();
        fireDamageArea.enabled = false;
        isChanneling = false;

        // Resume movement
        //agent.isStopped = false;
        //agent.SetDestination(player.position);

        // Debug
        //Debug.Log("Stopped fire breath, resuming movement.");
        enemyAnimation.SetBool("FireBreath", false);
    }

    

    bool HasLineOfSight()
    {
        Ray ray = new Ray(transform.position + Vector3.up, (player.position - transform.position).normalized);
        if (Physics.Raycast(ray, out RaycastHit hit, triggerRange))
        {
            return hit.collider.CompareTag("Player");
        }
        return false;
    }

    void RotateTowardsTarget()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }
}
