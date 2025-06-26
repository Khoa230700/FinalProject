using UnityEngine;
using UnityEngine.AI;

public class EnemySpider : MonoBehaviour
{
    public float zigzagSpeed = 3f;
    public float zigzagFrequency = 3f;
    public float forwardSpeed = 2f;
    public float detectionRange = 8f;
    public float jumpForce = 10f;

    private Rigidbody rb;
    //private Transform player;
    private bool hasJumped = false;
    private float startTime;

    NavMeshAgent agent;
    public Transform player;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        startTime = Time.time;
    }

    void Update()
    {
        agent.destination = player.position;
        if (hasJumped || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRange)
        {
            JumpToPlayer();
        }
        //else
        //{
        //    MoveZigzag();
        //}
    }

    void MoveZigzag()
    {
        float zigzagOffset = Mathf.Sin((Time.time - startTime) * zigzagFrequency) * zigzagSpeed;
        Vector3 sideMovement = transform.right * zigzagOffset;
        Vector3 forwardMovement = transform.forward * forwardSpeed;

        Vector3 move = (forwardMovement + sideMovement) * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }

    void JumpToPlayer()
    {
        hasJumped = true;

        Vector3 direction = (player.position - transform.position).normalized;
        Vector3 jumpVector = new Vector3(direction.x, 1f, direction.z); // Add upward force
        rb.AddForce(jumpVector.normalized * jumpForce, ForceMode.Impulse);
    }
}
