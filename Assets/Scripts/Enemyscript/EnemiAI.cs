using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Linq;
using NUnit;

public class EnemiAI : MonoBehaviour
{
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public int damage = 10;

    private float lastAttackTime;



    //
    public float detectionRadius = 10f;
    private Transform currentTarget;
    public float moveSpeed = 4f;
    public LayerMask targetLayer;


    private void Start()
    {
        //player = GameObject.FindWithTag("Player").transform;
    }
    void Update()
    {
        //IDamageable target = GetClosestDamageableInRange();

        //if (target != null && Time.time >= lastAttackTime + attackCooldown)
        //{
        //    target.TakeDamage(damage);
        //    lastAttackTime = Time.time;
        //}

        FindClosestTarget();

        if (currentTarget != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.position);

            if (distance > attackRange)
            {
                ChaseTarget();
            }
            else
            {
                Attack();
            }
        }
    }

    IDamageable GetClosestDamageableInRange()
    {
        IDamageable[] targets = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<IDamageable>().ToArray();

        IDamageable closest = null;
        float minDistance = Mathf.Infinity;

        foreach (IDamageable target in targets)
        {
            float distance = Vector3.Distance(transform.position, ((MonoBehaviour)target).transform.position);
            if (distance <= attackRange && distance < minDistance)
            {
                closest = target;
                minDistance = distance;
            }
        }

        return closest;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }


    //
    void FindClosestTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, targetLayer);
        if (hits.Length == 0)
        {
            currentTarget = null;
            return;
        }

        Transform closest = hits
            .OrderBy(h => Vector3.Distance(transform.position, h.transform.position))
            .First().transform;

        currentTarget = closest;
    }

    void ChaseTarget()
    {
        Vector3 direction = (currentTarget.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        transform.LookAt(currentTarget); // Optional: face the target
    }

    void Attack()
    {
        IDamageable target = GetClosestDamageableInRange();

        if (target != null && Time.time >= lastAttackTime + attackCooldown)
        {
            target.TakeDamage(damage);
            lastAttackTime = Time.time;
        }
        //enemyAnimation.SetTrigger("attack");
    }
}
