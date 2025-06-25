using UnityEngine;
using UnityEngine.AI;

public class L4DBotMeleeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform player;


    [Header("AI Settings")]
    [SerializeField] float visionRange = 20f;
    [SerializeField] float attackRange = 3f;

    private NavMeshAgent agent;
    private Animator animator;
    private GameObject currentTarget;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        //agent.updateRotation = false; // Ta tự xử lý quay mặt
    }

    private void Update()
    {

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        // Nếu player quá xa → đi theo player
        if (distToPlayer > agent.stoppingDistance && !animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            currentTarget = null;
            agent.isStopped = false;
            agent.SetDestination(player.position);

            Vector3 localVelocity = transform.InverseTransformDirection(agent.velocity);
            animator.SetFloat("Horizontal", localVelocity.x);
            animator.SetFloat("Vertical", localVelocity.z);
            animator.SetBool("isMoving", true);
            return;
        }

        // Nếu player đủ gần → bắt đầu tìm zombie
        currentTarget = FindNearestVisibleZombie();

        if (currentTarget != null)
        {
            float distToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);

            // Nếu zombie trong tầm nhìn nhưng chưa đủ gần để tấn công → đi tới zombie
            if (distToTarget > attackRange)
            {
                // CHƯA ĐỦ GẦN → ĐI LẠI
                agent.isStopped = false;
                agent.SetDestination(currentTarget.transform.position);

                Vector3 localVelocity = transform.InverseTransformDirection(agent.velocity);
                animator.SetFloat("Horizontal", localVelocity.x);
                animator.SetFloat("Vertical", localVelocity.z);
                animator.SetBool("isMoving", true);
            }
            else
            {
                // ĐỦ GẦN → DỪNG LẠI TẤN CÔNG
                agent.isStopped = true;

                Vector3 lookPos = new Vector3(currentTarget.transform.position.x, transform.position.y, currentTarget.transform.position.z);
                transform.LookAt(lookPos);

                animator.SetFloat("Horizontal", 0f);
                animator.SetFloat("Vertical", 0f);
                animator.SetBool("isMoving", false);
                animator.SetTrigger("Attack");
            }

            return;
        }

        // Không thấy zombie → đứng yên
        agent.isStopped = true;
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
            if (dist < visionRange && HasLineOfSight(zombie.transform))
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
        Vector3 direction = (targetPoint - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, targetPoint);

        Debug.DrawRay(transform.position, direction * distance, Color.red);

        int layerMask = LayerMask.GetMask("Default"); 
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, distance, layerMask))
        {
            return hit.collider.CompareTag("Enemy");
        }

        return false;
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