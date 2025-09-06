using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class L4DBotMeleeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform player;
    [SerializeField] NavMeshAgent agent;
    private Animator animator;

    [Header("Combat Settings")]
    [SerializeField] float attackCooldown ;
    [SerializeField] float attackRange ;
    [SerializeField] float visionRange ;

    public float lastAttackTime;
    public int attackCombo;
    private GameObject currentTarget;
    private bool isAttacking = false;
    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        player = SelectorSpawner.Instance.Player.transform;
    }

    private void Update()
    {
        float distToPlayer = Vector3.Distance(transform.position, player.position);
       
        // 1. Nếu xa player → chạy lại gần
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

        // 2. Player gần → tìm zombie quanh player
        currentTarget = FindNearestVisibleZombie();
        
       if (currentTarget != null)
{
    float distToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);

    if (distToTarget <= attackRange)
    {
        agent.isStopped = true;
        transform.LookAt(new Vector3(currentTarget.transform.position.x, transform.position.y, currentTarget.transform.position.z));

        animator.SetBool("isMoving", false);
        animator.SetFloat("Horizontal", 0f);
        animator.SetFloat("Vertical", 0f);

               
        TriggerAttack();
                
                return;
    }
        }

        // 3. Không có zombie → đứng cạnh player
        agent.isStopped = true;
        animator.SetBool("isMoving", false);
        animator.SetFloat("Horizontal", 0f);
        animator.SetFloat("Vertical", 0f);
        AudioBotManager.Instance.StopBotSound();
    }

    private void TriggerAttack()
    {
        if (isAttacking) return; // đang đánh thì ko đánh tiếp

        StartCoroutine(AttackRoutine());
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
            return col.bounds.center + Vector3.up * (col.bounds.extents.y * 0.5f);

        return target.position;
    }
    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        attackCombo = Random.Range(0, 3);
        animator.SetInteger("AttackCombo", attackCombo);

        animator.ResetTrigger("Attack");   // reset trước
        animator.SetTrigger("Attack");     // set lại
        AudioBotManager.Instance.MeleeSound();

        Debug.Log($"Attack Triggered with Combo: {attackCombo}");

        // chờ animator đánh xong (ví dụ giả sử mỗi đòn dài 1s)
        yield return new WaitForSeconds(1f);

        // sau đó chờ thêm cooldown
        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
    }
}