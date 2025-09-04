using UnityEngine;
using UnityEngine.AI;

public class Spidey : MonoBehaviour
{
    public EnemyHookThrow enemy;

    private NavMeshAgent agent;
    public Transform player;

    public float lastAttackTime = 0f;
    public float attackCooldown = 2f;

    public float lastAttackTime1 = 0f;
    public float attackCooldown1 = 2f;

    public float meleeDistance = 2f;
    public float stopDistance = 20f;
    public Animator animator;
    // sound
    private EnemySoundController soundController;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stopDistance;
        soundController = GetComponent<EnemySoundController>();
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= meleeDistance)
        {
            agent.isStopped = true;
            MeleeAttack();
        }
        else if (distance <= stopDistance)
        {
            agent.isStopped = true;
            HookAttack();
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
            transform.forward = direction;
    }





    void MeleeAttack()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            // damage player()
            Debug.Log("Enemy attacks the player!");
        }
        animator.SetTrigger("MeleeAttack");
        soundController.PlayAttackSound2();
    }

    void HookAttack()
    {
        if (Time.time - lastAttackTime1 >= attackCooldown1)
        {
            lastAttackTime1 = Time.time;
            enemy.ThrowHook(player);
        }
        animator.SetTrigger("Hook");
        soundController.PlayAttackSound();
    }
}
