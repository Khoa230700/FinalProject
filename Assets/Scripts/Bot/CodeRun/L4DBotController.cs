using UnityEngine;
using UnityEngine.AI;

public class L4DBotController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform player;
    [SerializeField] Transform firePoint;
    [SerializeField] ParticleSystem bulletParticleSystem;
    [SerializeField] WFX_LightFlicker wFX_LightFlicker;
    [SerializeField] TargetableEnemy hitdame;

    [Header("AI Settings")]
    [SerializeField] float detectionRange = 20f;
    [SerializeField] float fireRate = 0.5f;
    [SerializeField] float followPlayerDistance = 8f;   // khoảng cách để theo player khi không có zombie
    [SerializeField] float combatStoppingDistance = 0f; // khoảng cách khi bắn zombie

    private NavMeshAgent agent;
    private Animator animator;
    private float fireCooldown;
    private GameObject currentTarget;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = SelectorSpawner.Instance.Player.transform;
    }

    private void Update()
    {
        fireCooldown -= Time.deltaTime;

        // --- Ưu tiên tìm zombie ---
        currentTarget = FindNearestVisibleZombie();
        if (currentTarget != null)
        {
            HandleCombat();
            return;
        }

        // --- Không có zombie → theo player ---
        HandleFollowPlayer();
    }

    private void HandleCombat()
    {
        float distToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
        if (distToTarget > detectionRange)
        {
            currentTarget = null;
            return;
        }

        // Dừng bắn tại chỗ
        agent.stoppingDistance = combatStoppingDistance;
        agent.isStopped = true;
        agent.SetDestination(transform.position);
        AudioBotManager.Instance.StopBotSound();

        // Xoay mặt về phía zombie
        Vector3 lookPos = new Vector3(currentTarget.transform.position.x, transform.position.y, currentTarget.transform.position.z);
        transform.LookAt(lookPos);

        animator.SetFloat("Horizontal", 0f);
        animator.SetFloat("Vertical", 0f);
        animator.SetBool("isMoving", false);

        if (fireCooldown <= 0f)
        {
            animator.SetTrigger("shoot");
            Shoot(currentTarget.transform);
            AudioBotManager.Instance.ShootSound();
            fireCooldown = fireRate;
        }
    }

    private void HandleFollowPlayer()
    {
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        agent.stoppingDistance = followPlayerDistance;

        if (distToPlayer > agent.stoppingDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            AudioBotManager.Instance.PlayBotSound();

            Vector3 localVelocity = transform.InverseTransformDirection(agent.velocity);
            animator.SetFloat("Horizontal", localVelocity.x);
            animator.SetFloat("Vertical", localVelocity.z);
            animator.SetBool("isMoving", true);
        }
        else
        {
            agent.isStopped = true;
            animator.SetFloat("Horizontal", 0f);
            animator.SetFloat("Vertical", 0f);
            animator.SetBool("isMoving", false);
            AudioBotManager.Instance.StopBotSound();
        }
    }

    private GameObject FindNearestVisibleZombie()
    {
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject zombie in zombies)
        {
            if (zombie == null) continue;

            float dist = Vector3.Distance(transform.position, zombie.transform.position);
            if (dist < detectionRange && HasLineOfSight(zombie.transform))
            {
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = zombie;
                }
            }
        }

        return closest;
    }

    private bool HasLineOfSight(Transform target)
    {
        Vector3 targetPoint = GetAimPoint(target);
        Vector3 direction = (targetPoint - firePoint.position).normalized;
        float distance = Vector3.Distance(firePoint.position, targetPoint);

        if (Physics.Raycast(firePoint.position, direction, out RaycastHit hit, distance))
        {
            return hit.collider.CompareTag("Enemy");
        }
        return false;
    }

    private void Shoot(Transform target)
    {
        if (firePoint == null || target == null) return;

        Vector3 aimPoint = GetAimPoint(target);
        Vector3 dir = (aimPoint - firePoint.position).normalized;

        if (Physics.Raycast(firePoint.position, dir, out RaycastHit hit, detectionRange))
        {
            if (bulletParticleSystem != null)
            {
                bulletParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                bulletParticleSystem.Play();
            }

            if (wFX_LightFlicker != null)
            {
                wFX_LightFlicker.FlickerOnce();
            }

            // 1. Hitbox (child)
            var hb = hit.collider.GetComponent<Hitbox>()
                  ?? hit.collider.GetComponentInChildren<Hitbox>()
                  ?? hit.collider.GetComponentInParent<Hitbox>();
            if (hb != null)
            {
                hb.ownerHealthSystem.TakeDamage(20);
                hb.OnHit(20, hit.point);
                return; // Ưu tiên hitbox → return luôn
            }

            // 2. Enemy thường (script ở parent hoặc chính collider)
            var enemy = hit.collider.GetComponent<EnemyM>()
                     ?? hit.collider.GetComponentInParent<EnemyM>()
                     ?? hit.collider.GetComponentInChildren<EnemyM>();
            if (enemy != null)
            {
                enemy.TakeDamage(20);
                return;
            }

            // 3. Boss
            var boss = hit.collider.GetComponent<BossHealth>()
                     ?? hit.collider.GetComponentInParent<BossHealth>()
                     ?? hit.collider.GetComponentInChildren<BossHealth>();
            if (boss != null)
            {
                boss.TakeDamage(50);
                return;
            }
        }
    }

    private Vector3 GetAimPoint(Transform target)
    {
        hitdame = target.gameObject.GetComponentInChildren<TargetableEnemy>();
        if (hitdame != null && hitdame.aimTarget != null)
            return hitdame.aimTarget.position;

        Collider col = target.GetComponent<Collider>();
        if (col != null) return col.bounds.center;

        return target.position + Vector3.up * 1.2f;
    }
}