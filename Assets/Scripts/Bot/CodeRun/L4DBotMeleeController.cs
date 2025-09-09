using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class L4DBotMeleeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform player;
    [SerializeField] NavMeshAgent agent;
    public Animator animator;

    [Header("Combat Settings")]
    [SerializeField] float attackCooldown = 1f;
    [SerializeField] float attackRange = 2f;
    [SerializeField] float visionRange = 10f;

    private float lastAttackTime;
    private int attackCombo;
    private GameObject currentTarget;
    private bool isAttacking = false;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        player = SelectorSpawner.Instance.Player.transform;
    }

    private void Update()
    {
        fireAI();
    }

    private void fireAI()
    {
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        // 1. Tìm zombie trước
        currentTarget = FindNearestVisibleZombie();

        if (currentTarget != null)
        {
            float distToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (distToTarget <= attackRange)
            {
                // Đứng lại chém
                agent.isStopped = true;
                transform.LookAt(new Vector3(currentTarget.transform.position.x, transform.position.y, currentTarget.transform.position.z));

                animator.SetBool("isMoving", false);
                animator.SetFloat("Horizontal", 0f);
                animator.SetFloat("Vertical", 0f);

                TriggerAttack();
                return;
            }
            else
            {
                // Di chuyển lại gần zombie
                agent.isStopped = false;
                agent.stoppingDistance = attackRange;
                agent.SetDestination(currentTarget.transform.position);

                Vector3 localVelocity = transform.InverseTransformDirection(agent.velocity);
                animator.SetBool("isMoving", true);
                animator.SetFloat("Horizontal", localVelocity.x);
                animator.SetFloat("Vertical", localVelocity.z);
                AudioBotManager.Instance.PlayBotSound();
                return;
            }
        }

        // 2. Không có zombie → theo player
        agent.stoppingDistance = 2f; // đứng gần player
        if (distToPlayer > agent.stoppingDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);

            Vector3 localVelocity = transform.InverseTransformDirection(agent.velocity);
            animator.SetBool("isMoving", true);
            animator.SetFloat("Horizontal", localVelocity.x);
            animator.SetFloat("Vertical", localVelocity.z);
            AudioBotManager.Instance.PlayBotSound();
        }
        else
        {
            agent.isStopped = true;
            animator.SetBool("isMoving", false);
            animator.SetFloat("Horizontal", 0f);
            animator.SetFloat("Vertical", 0f);
            AudioBotManager.Instance.StopBotSound();
        }
    }

    private void TriggerAttack()
    {
        if (isAttacking) return; // đang đánh thì ko đánh tiếp
        if (Time.time - lastAttackTime < attackCooldown) return; // chưa hết cooldown

        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        attackCombo = Random.Range(0, 3);
        animator.SetInteger("AttackCombo", attackCombo);

        animator.ResetTrigger("Attack");
        animator.SetTrigger("Attack");
        AudioBotManager.Instance.MeleeSound();

        Debug.Log($"Melee Attack Triggered with Combo: {attackCombo}");

        // giả sử mỗi đòn đánh mất 1 giây
        yield return new WaitForSeconds(1f);

        isAttacking = false;
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

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, distance))
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
            return col.bounds.center;

        return target.position;
    }
}