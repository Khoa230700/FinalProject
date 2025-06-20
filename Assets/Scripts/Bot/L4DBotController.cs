using UnityEngine;
using UnityEngine.AI;

public class L4DBotController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform player;
    [SerializeField] Transform firePoint;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] ParticleSystem bulletParticleSystem;
    [SerializeField] WFX_LightFlicker wFX_LightFlicker;
    [SerializeField] AudioClip bulletAudioClip;
    [SerializeField] AudioSource audioSource;

    [Header("AI Settings")]
    [SerializeField] float followDistance = 8f;
    [SerializeField] float detectionRange = 15f;
    [SerializeField] float fireRate = 1f;
    [SerializeField] float bulletSpeed = 25f;

    private NavMeshAgent agent;
    private Animator animator;
    private float fireCooldown = 0f;
    private GameObject currentTarget;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        //agent.updateRotation = false; // Ta tự xử lý quay mặt
    }

    private void Update()
    {
        fireCooldown -= Time.deltaTime;

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        // Nếu player quá xa thì chạy theo player, không bắn
        if (distToPlayer > followDistance)
        {
            currentTarget = null; // Bỏ target vì phải theo player

            agent.isStopped = false;
            agent.SetDestination(player.position);

            Vector3 localVelocity = transform.InverseTransformDirection(agent.velocity);
            animator.SetFloat("Horizontal", localVelocity.x);
            animator.SetFloat("Vertical", localVelocity.z);
            animator.SetBool("isMoving", true);

            return; // ưu tiên theo player nên bỏ qua các bước tiếp theo
        }

        // Player gần thì đứng yên hoặc bắn zombie
        currentTarget = FindNearestVisibleZombie();

        if (currentTarget != null)
        {
            float distToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (distToTarget <= detectionRange)
            {
                // Đứng yên khi bắn zombie
                agent.isStopped = true;
                agent.SetDestination(transform.position);

                // Quay mặt ngang về phía zombie
                Vector3 lookPos = new Vector3(currentTarget.transform.position.x, transform.position.y, currentTarget.transform.position.z);
                transform.LookAt(lookPos);

                animator.SetFloat("Horizontal", 0f);
                animator.SetFloat("Vertical", 0f);
                animator.SetBool("isMoving", false);

                if (fireCooldown <= 0f)
                {
                    animator.SetTrigger("shoot");
                    Shoot(currentTarget.transform);
                    fireCooldown = fireRate;
                }

                return; // ưu tiên bắn zombie
            }
            else
            {
                // Zombie ra khỏi tầm → bỏ target, đứng yên chờ player (hoặc di chuyển nếu cần)
                currentTarget = null;
            }
        }

        // Không có target và player gần → đứng yên
        agent.isStopped = true;
        agent.SetDestination(transform.position);
        animator.SetFloat("Horizontal", 0f);
        animator.SetFloat("Vertical", 0f);
        animator.SetBool("isMoving", false);
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

        //Debug.DrawRay(firePoint.position, direction * distance, Color.red);

        int layerMask = LayerMask.GetMask("Default"); // Sửa nếu enemy ở layer khác
        if (Physics.Raycast(firePoint.position, direction, out RaycastHit hit, distance, layerMask))
        {
            return hit.collider.CompareTag("Enemy");
        }

        return false;
    }

    private void Shoot(Transform target)
    {
        if (bulletPrefab && firePoint)
        {
            Vector3 aimPoint = GetAimPoint(target);
            Vector3 dir = (aimPoint - firePoint.position).normalized;

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(dir));

            if (bulletParticleSystem != null)
            {
                bulletParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                bulletParticleSystem.Play();
            }
            if (wFX_LightFlicker != null)
            {
                wFX_LightFlicker.FlickerOnce();
            }

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
                rb.linearVelocity = dir * bulletSpeed;
        }
    }

    private Vector3 GetAimPoint(Transform target)
    {
        TargetableEnemy et = target.GetComponent<TargetableEnemy>();
        if (et != null && et.aimTarget != null)
            return et.aimTarget.position;

        Collider col = target.GetComponent<Collider>();
        if (col != null)
            return col.bounds.center + Vector3.up * (col.bounds.extents.y * 0.5f);

        return target.position + Vector3.up * 1.2f;
    }
}