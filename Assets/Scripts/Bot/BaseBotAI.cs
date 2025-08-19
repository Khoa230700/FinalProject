using UnityEngine;
using UnityEngine.AI;

public abstract class BaseBotAI : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] protected float detectionRange = 10f;

    protected GameObject currentTarget;
    protected Animator animator;
    protected NavMeshAgent agent;
    protected Transform player;

    protected IEnemyDetector enemyDetector;
    protected IAttackStrategy attackStrategy;
    protected IMovement movementStrategy;

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    protected virtual void Update()
    {
        UpdateBehavior();
    }

    protected abstract void UpdateBehavior();
}