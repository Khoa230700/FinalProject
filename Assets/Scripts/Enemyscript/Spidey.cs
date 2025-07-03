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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stopDistance;
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
        //enemyAnimation.SetTrigger("attack");
    }

    void HookAttack()
    {
        if (Time.time - lastAttackTime1 >= attackCooldown1)
        {
            lastAttackTime1 = Time.time;
            enemy.ThrowHook(player);
        }
    }
}
