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
    public float slamRange = 15f;
    public float triggerRange = 15f;

    [Header("Cooldowns")]
    public float rangeAttackCooldown = 5f;
    public float slamAttackCooldown = 10f;
    public float fireCooldown = 15f;

    private float lastRangeAttackTime = -10f;
    private float lastSlamAttackTime = -10f;
    private float nextFireTime = 0f;
    public float Shoutcooldown = 10f;

    [Header("Fire Breath")]
    public float channelTime = 3f;
    public float damagePerSecond = 10f;
    public float minFireBreathDistance = 5f;
    public ParticleSystem fireFX;
    public Collider fireDamageArea;
    private bool isChanneling = false;

    [Header("Ranged Attack")]
    public GameObject rangedProjectile;
    public Transform firePoint;
    public float bulletSpeed = 40f;
    public float bulletTimelife = 7f;
    public Transform playerAimTarget;

    [Header("Misc")]
    public string playerTag = "Player";
    public float rotationSpeed = 5f;

    [Header("Shout")]
    public float Shoutrange = 6f;
    public float lastShoutTime = 5f;

    //Health ref
    private BossHealth bossHealth;
    private bool isPhase2 = false;


    //Skill Slam
    public GameObject particlePrefab;
    public GameObject colliderPrefab;
    private float particlePrefabTimelife = 3f;
    private float colliderPrefabTimelife = 2f;

    public Animator enemyAnimation;

    //fix
    private bool isAttacking = false;


    private float destinationUpdateRate = 0.5f;
    private float nextDestinationUpdateTime = 0f;



    //Sound
    private EnemySoundController soundController;
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        //enemyAnimation = GetComponent<Animator>();
        fireFX.Stop();
        fireDamageArea.enabled = false;

        bossHealth = GetComponent<BossHealth>();
        bossHealth.OnPhase2Enter += EnterPhase2;

        soundController = GetComponent<EnemySoundController>();
        if (player == null)
        {
            player = GameObject.FindWithTag("Player").transform;
        }
    }

    private void Update()
    {
        //Debug.Log($"Distance: {distance:F2} | CanFire: {canFire} | LOS: {HasLineOfSight()} | Time: {Time.time} | NextFireTime: {nextFireTime} | isAttacking: {isAttacking}");
        //Debug.DrawRay(transform.position + Vector3.up, (player.position - transform.position).normalized * triggerRange, Color.red);
        if (Input.GetKeyDown(KeyCode.H))
        {
            bossHealth.TakeDamage(30);
        }

        if (isChanneling || isAttacking)
        {
            RotateTowardsTarget();
            return; // Block other logic while attacking
        }

        float distance = Vector3.Distance(transform.position, player.position);

        

        bool canSlam = distance <= slamRange && Time.time - lastSlamAttackTime >= slamAttackCooldown;
        bool canFire = distance <= triggerRange /*&& HasLineOfSight()*/ && Time.time >= nextFireTime;
        //bool canFire = distance >= fireBreathMinRange && distance <= triggerRange && HasLineOfSight() && Time.time >= nextFireTime;
        //bool canFire = distance >= minFireBreathDistance &&
        //       distance <= triggerRange &&
        //       HasLineOfSight() &&
        //       Time.time >= nextFireTime &&
        //       !isAttacking;
        bool canShout = isPhase2 && distance <= Shoutrange && Time.time - lastShoutTime >= Shoutcooldown;
        bool canMelee = distance <= meleeRange;
        bool canRange = Time.time - lastRangeAttackTime >= rangeAttackCooldown;

        // Phase-based priority
        if (canSlam)
        {
            SlamAttack();
            lastSlamAttackTime = Time.time;
        }
        else if (canFire)
        {
            Debug.Log("Fire breath conditions met, starting...");
            agent.isStopped = true;
            StartFireBreath();
            nextFireTime = Time.time + fireCooldown;
        }
        else if (canShout)
        {
            Shout();
            lastShoutTime = Time.time;
        }
        else if (canMelee)
        {
            MeleeAttack();
        }
        else
        {
            if (!agent.hasPath || agent.remainingDistance < 0.5f || Time.time >= nextDestinationUpdateTime)
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
                nextDestinationUpdateTime = Time.time + destinationUpdateRate;
            }
        }
    }

    // --- Attack Methods ---

    void MeleeAttack()
    {
        //agent.isStopped = true;
        //Debug.Log("Boss performs melee attack!");
        player.GetComponent<PlayerHealthSystem>().TakeDamage(5);
    }

    //void RangeAttack()
    //{
    //    //agent.isStopped = true;
    //    //Debug.Log("Boss performs range attack!");
    //    GameObject bullet = Instantiate(rangedProjectile, firePoint.position, firePoint.rotation);
    //    bullet.GetComponent<Rigidbody>().AddForce(firePoint.forward * bulletSpeed);
    //    Destroy(bullet, bulletTimelife);
    //}

    void SlamAttack()
    {

        isAttacking = true;
        agent.isStopped = true;

        GameObject partic = Instantiate(particlePrefab, firePoint.position, firePoint.rotation);
        GameObject collid = Instantiate(colliderPrefab, firePoint.position, firePoint.rotation);
        collid.GetComponent<Rigidbody>().AddForce(firePoint.forward * bulletSpeed);
        Destroy(partic, particlePrefabTimelife);
        Destroy(collid, colliderPrefabTimelife);
        //Debug.Log("Boss performs slam attack!");
        //Collider[] colliders = Physics.OverlapSphere(transform.position, 4f);
        //foreach (Collider col in colliders)
        //{
        //    if (col.GetComponent<PlayerHealth>())
        //        col.GetComponent<PlayerHealth>().TakeDamage(20);
        //}
        enemyAnimation.SetTrigger("Slam");
        Invoke(nameof(EndAttack), 2f);
        soundController.PlayAttackSound();
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

        soundController.PlayAttackSound2();
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



    void Shout()
    {

        isAttacking = true;
        agent.isStopped = true;
        //Debug.Log("Boss uses SHOUT!");
        enemyAnimation.SetTrigger("Shout");

        Collider[] hitPlayers = Physics.OverlapSphere(transform.position, Shoutrange);
        foreach (var hit in hitPlayers)
        {
            if (hit.CompareTag("Player"))
            {
                hit.GetComponent<PlayerHealthSystem>().TakeDamage(15);
                //  trigger a stun, knockback
            }
        }
        Invoke(nameof(EndAttack), 3f);
    }
    void ResumeMovement()
    {
        agent.isStopped = false;
    }


    void EnterPhase2()
    {
        isPhase2 = true;
        Debug.Log("Boss has entered Phase 2!");
    }

    void EndAttack()
    {
        isAttacking = false;

        if (agent != null)
        {
            agent.ResetPath();              // Clear any existing path
            agent.isStopped = false;       // Resume movement
            agent.SetDestination(player.position); // Force chase to resume
        }

        nextDestinationUpdateTime = Time.time + destinationUpdateRate; // Restart chase timing
    }
}
