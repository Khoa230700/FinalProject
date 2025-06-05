using UnityEngine;
using UnityEngine.AI;

public class L4DBotController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform player;
    [SerializeField] Transform firePoint;
    [SerializeField] GameObject bulletPrefab;

    [Header("AI Settings")]
    [SerializeField] float followDistance = 3f;
    [SerializeField] float detectionRange = 15f;
    [SerializeField] float fireRate = 1f;
    [SerializeField] float bulletSpeed = 25f;

    private NavMeshAgent agent;
    private Animator animator;
    private float fireCooldown = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        fireCooldown -= Time.deltaTime;
        GameObject target = FindNearestVisibleZombie();

        if (target != null)
        {
            Debug.Log(target.name + " is in range");
            agent.SetDestination(transform.position);
            transform.LookAt(target.transform);

           
            animator.SetFloat("Horizontal", 0f);
            animator.SetFloat("Vertical", 0f);
            animator.SetBool("isMoving", false);

            if (fireCooldown <= 0f)
            {
                animator.SetTrigger("shoot");
                Shoot(target.transform);
                fireCooldown = fireRate;
            }
        }
        else
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist > followDistance)
            {
                agent.SetDestination(player.position);

                // ✅ Tính hướng di chuyển tương đối với bot
                Vector3 velocity = agent.velocity;
                Vector3 localVelocity = transform.InverseTransformDirection(velocity);
                float inputX = localVelocity.x;
                float inputY = localVelocity.z;

                animator.SetFloat("Horizontal", inputX);
                animator.SetFloat("Vertical", inputY);
                animator.SetBool("isMoving", true);
            }
            else
            {
                agent.SetDestination(transform.position);

                animator.SetFloat("Horizontal", 0f);
                animator.SetFloat("Vertical", 0f);
                animator.SetBool("isMoving", false);
            }
        }

        GameObject FindNearestVisibleZombie()
        {
            GameObject[] zombies = GameObject.FindGameObjectsWithTag("Enemy");
            GameObject closest = null;
            float minDist = Mathf.Infinity;

            foreach (GameObject zombie in zombies)
            {
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

        bool HasLineOfSight(Transform target)
        {
            Vector3 direction = (target.position - firePoint.position).normalized;
            float distance = Vector3.Distance(firePoint.position, target.position);
            Debug.DrawRay(firePoint.position, direction * distance, Color.red);
            if (Physics.Raycast(firePoint.position, direction, out RaycastHit hit, distance))
            {
                return hit.collider.CompareTag("Enemy");
            }

            return false;
        }

        void Shoot(Transform target)
        {
            if (bulletPrefab && firePoint)
            {
                Vector3 dir = (target.position - firePoint.position).normalized;
                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(dir));
                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                rb.linearVelocity = dir * bulletSpeed;
            }
        }
    }
}
