using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

public class NewEnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;


    public float maxHealth = 100f;
    public float currentHealth;
    public int attackDamage = 15;
    public float attackSpeed = 1.5f;
    private float nextAttackTime = 0f;


    public float patrolRadius = 20f;
    public float patrolWaitTime = 3f;
    private Vector3 patrolDestination;
    private bool patrolPointSet;
    private float patrolTimer;


    public float detectionRadius = 15f;
    public float fieldOfViewAngle = 120f;


    public float attackRange = 2f;


    public float fleeHealthThreshold = 0.3f;
    public float fleeDistance = 25f;

    public enum AIState { Patrol, Chase, Attack, Flee }
    public AIState currentState;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("Không tìm thấy NavMeshAgent trên " + gameObject.name);
            enabled = false;
            return;
        }


        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
            else
            {
                Debug.LogError("Không tìm thấy Player.");
                enabled = false;
                return;
            }
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        patrolTimer = patrolWaitTime;
        SwitchState(AIState.Patrol);
    }

    void Update()
    {
        if (player == null || agent == null || !agent.isOnNavMesh) return;


        if (currentHealth <= maxHealth * fleeHealthThreshold && currentState != AIState.Flee)
        {
            SwitchState(AIState.Flee);
        }


        switch (currentState)
        {
            case AIState.Patrol:
                PatrolBehavior();
                DetectPlayer();
                break;
            case AIState.Chase:
                ChaseBehavior();
                DetectPlayer();
                break;
            case AIState.Attack:
                AttackBehavior();
                break;
            case AIState.Flee:
                FleeBehavior();
                break;
        }
    }

    void SwitchState(AIState newState)
    {
        if (currentState == newState && newState != AIState.Flee) return;

        currentState = newState;



        switch (newState)
        {
            case AIState.Patrol:
                agent.speed = 3.5f;
                agent.stoppingDistance = 0.5f;
                patrolPointSet = false;
                break;
            case AIState.Chase:
                agent.speed = 5f;
                agent.stoppingDistance = attackRange * 0.9f;
                break;
            case AIState.Attack:
                agent.speed = 0f;
                agent.ResetPath();
                break;
            case AIState.Flee:
                agent.speed = 6f;
                agent.stoppingDistance = 0.5f;

                break;
        }
    }

    void DetectPlayer()
    {
        if (player == null || currentState == AIState.Flee) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRadius)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (angleToPlayer < fieldOfViewAngle / 2f)
            {

                RaycastHit hit;

                if (Physics.Raycast(transform.position + Vector3.up * 1.0f, directionToPlayer, out hit, detectionRadius, ~obstacleLayer)) // Bỏ qua layer vật cản
                {
                    if (hit.collider.gameObject.layer == playerLayer.value || hit.collider.CompareTag("Player"))
                    {
                        if (distanceToPlayer <= attackRange)
                        {
                            SwitchState(AIState.Attack);
                        }
                        else
                        {
                            SwitchState(AIState.Chase);
                        }
                        return;
                    }
                }
            }
        }


        if (currentState == AIState.Chase)
        {
            SwitchState(AIState.Patrol);
        }
    }

    void PatrolBehavior()
    {
        if (!patrolPointSet)
        {
            SearchNewPatrolPoint();
        }

        if (patrolPointSet && agent.isOnNavMesh)
        {
            agent.SetDestination(patrolDestination);
        }


        if (agent.isOnNavMesh && agent.remainingDistance < agent.stoppingDistance + 0.2f && patrolPointSet)
        {
            patrolTimer -= Time.deltaTime;
            if (patrolTimer <= 0)
            {
                patrolPointSet = false;
                patrolTimer = patrolWaitTime;
            }
        }
    }

    void SearchNewPatrolPoint()
    {
        float randomX = Random.Range(-patrolRadius, patrolRadius);
        float randomZ = Random.Range(-patrolRadius, patrolRadius);
        Vector3 randomPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, patrolRadius * 1.5f, NavMesh.AllAreas))
        {
            patrolDestination = hit.position;
            patrolPointSet = true;
        }
        else
        {

            patrolPointSet = false;
        }
    }

    void ChaseBehavior()
    {
        if (player == null)
        {
            SwitchState(AIState.Patrol);
            return;
        }

        if (agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            SwitchState(AIState.Attack);
        }

        else if (distanceToPlayer > detectionRadius * 1.2f)
        {
            SwitchState(AIState.Patrol);
        }
    }

    void AttackBehavior()
    {
        if (player == null)
        {
            SwitchState(AIState.Patrol);
            return;
        }


        agent.SetDestination(transform.position);
        transform.LookAt(player.position);

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);


        if (distanceToPlayer > attackRange + 0.5f)
        {

            if (currentHealth > maxHealth * fleeHealthThreshold)
            {
                SwitchState(AIState.Chase);
            }
            else
            {
                SwitchState(AIState.Flee);
            }
            return;
        }


        if (Time.time >= nextAttackTime)
        {

            Debug.Log(gameObject.name + " tấn công " + player.name + " gây " + attackDamage + " sát thương.");


            player.GetComponent<testPlayerHealth>().TakeDamage(attackDamage);

            nextAttackTime = Time.time + 1f / attackSpeed;
        }
    }

    void FleeBehavior()
    {
        if (player == null)
        {
            SwitchState(AIState.Patrol);
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);




        if (distanceToPlayer < fleeDistance)
        {
            Vector3 directionFromPlayer = (transform.position - player.position).normalized;
            Vector3 targetFleePosition = transform.position + directionFromPlayer * (fleeDistance - distanceToPlayer + 2.0f); // +2.0f để cố gắng chạy xa hơn một chút

            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetFleePosition, out hit, fleeDistance * 0.5f, NavMesh.AllAreas))
            {
                if (agent.isOnNavMesh) agent.SetDestination(hit.position);
            }
            else
            {

                Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;

                if (Vector3.Dot(randomDir, directionFromPlayer) < 0.3f)
                {
                    randomDir = -directionFromPlayer + randomDir * 0.5f;
                    randomDir.Normalize();
                }

                Vector3 alternativeFleePos = transform.position + randomDir * fleeDistance * 0.7f;
                if (NavMesh.SamplePosition(alternativeFleePos, out hit, fleeDistance * 0.5f, NavMesh.AllAreas))
                {
                    if (agent.isOnNavMesh) agent.SetDestination(hit.position);
                }
            }
        }
        else
        {

            if (agent.isOnNavMesh) agent.ResetPath();
        }


    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log(gameObject.name + " nhận " + amount + " sát thương. Máu còn lại: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }

        else if (currentHealth <= maxHealth * fleeHealthThreshold && currentState != AIState.Flee)
        {
            SwitchState(AIState.Flee);
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " đã chết.");

        if (agent != null) agent.enabled = false;
        this.enabled = false;




        // GetComponent<Animator>()?.SetTrigger("Die");
        // GetComponent<Collider>()?.enabled = false; 
    }


    void OnDrawGizmosSelected()
    {
        if (agent == null) return;

        // Patrol Radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);

        // Detection Radius
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Attack Range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Flee Distance
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, fleeDistance);

        // Field of View
        Gizmos.color = Color.cyan;
        Vector3 forward = transform.forward;
        Quaternion leftRayRotation = Quaternion.AngleAxis(-fieldOfViewAngle / 2, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(fieldOfViewAngle / 2, Vector3.up);
        Vector3 leftRayDirection = leftRayRotation * forward;
        Vector3 rightRayDirection = rightRayRotation * forward;
        Gizmos.DrawRay(transform.position + Vector3.up * 1.0f, leftRayDirection * detectionRadius);
        Gizmos.DrawRay(transform.position + Vector3.up * 1.0f, rightRayDirection * detectionRadius);

#if UNITY_EDITOR
        UnityEditor.Handles.color = new Color(0, 1, 1, 0.05f);
        UnityEditor.Handles.DrawSolidArc(transform.position + Vector3.up * 1.0f, Vector3.up, leftRayDirection, fieldOfViewAngle, detectionRadius);
#endif
    }
}
