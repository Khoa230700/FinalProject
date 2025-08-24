using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class L4DBotMeleeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform player;
    private int AttackCombo;
    private float lastAttackTime;
    [SerializeField] float stopdistang;
    [SerializeField] float attackCooldown = 1.2f;
    [Header("AI Settings")]
    [SerializeField] float visionRange = 10f;
    [SerializeField] float attackRange = 3f;
    public float detectionRadius = 10f;

    public NavMeshAgent agent;
    private Animator animator;
    private GameObject currentTarget;

    private void Start()
    {
        //animator = GetComponent<Animator>();
        animator = GetComponentInChildren<Animator>();
        //agent.updateRotation = false; // Ta tự xử lý quay mặt
    }

    private void Update()
    {

        float distToPlayer = Vector3.Distance(transform.position, player.position);
        // Debug.Log("Distance to Player: " + distToPlayer);

        // 1. Nếu player còn xa → chạy lại gần player
        if (distToPlayer > agent.stoppingDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            currentTarget = null;

            Vector3 localVelocity = transform.InverseTransformDirection(agent.velocity);
            animator.SetBool("isMoving", true);
            animator.SetFloat("Horizontal", localVelocity.x);
            animator.SetFloat("Vertical", localVelocity.z);
            AudioBotManager.Instance.PlayBotSound();
            return;
        }

        // 2. Player đã gần → đứng yên cạnh player, chỉ phản ứng khi có zombie lọt vào tầm
        currentTarget = FindNearestVisibleZombie();

        if (currentTarget != null)
        {
            float distToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (distToTarget <= attackRange) // chỉ đánh khi zombie tự lại gần
            {
                agent.isStopped = true;
                transform.LookAt(new Vector3(currentTarget.transform.position.x, transform.position.y, currentTarget.transform.position.z));
                animator.SetBool("isMoving", false);
                animator.SetFloat("Horizontal", 0f);
                animator.SetFloat("Vertical", 0f);
                StartCoroutine(TriggerAttack());
                return;
            }
        }

        // 3. Không có zombie gần → đứng yên cạnh player
        agent.isStopped = true;
        animator.SetBool("isMoving", false);
        animator.SetFloat("Horizontal", 0f);
        animator.SetFloat("Vertical", 0f);
        AudioBotManager.Instance.StopBotSound();
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

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, distance))
        {
            return hit.collider.CompareTag("Enemy");
        }
        return false;
    }
    IEnumerator TriggerAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown)
            yield break;

        lastAttackTime = Time.time;
        AttackCombo = Random.Range(0, 3);
        animator.SetInteger("AttackCombo", AttackCombo);
        yield return null;
        animator.SetTrigger("Attack");
        //AudioBotManager.Instance.MeleeSound();
        Debug.Log("Attack Triggered with Combo: " + AttackCombo);
    }

    private Vector3 GetAimPoint(Transform target)
    {
        TargetableEnemy et = target.GetComponent<TargetableEnemy>();
        if (et != null && et.aimTarget != null)
            return et.aimTarget.position;

        Collider col = target.GetComponent<Collider>();
        if (col != null)
            return col.bounds.center + Vector3.up * (col.bounds.extents.y * 0.5f);

        EnemiAI ai = target.GetComponent<EnemiAI>();
        if (ai != null && target.gameObject.CompareTag("Enemy"))
            return target.position/* + Vector3.up * 1.5f*/;

        return target.position /*+ Vector3.up * 1.2f*/;
    }
    bool HasReachedDestination()
    {
        return !agent.pathPending &&
               agent.remainingDistance <= agent.stoppingDistance &&
               (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f);
    }
  
}