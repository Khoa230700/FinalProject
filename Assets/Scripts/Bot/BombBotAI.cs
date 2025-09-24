using UnityEngine;

public class BombBotAI : MonoBehaviour
{
    public Rigidbody rb;
    public CapsuleCollider capsuleCollider;
    public Animator animator;
    public float speed = 2f;
    private bool isWalking = true;

    private BotHealth botHealth;

    void Awake()
    {
        botHealth = GetComponent<BotHealth>();
    }

    void OnEnable()
    {
        if (botHealth != null)
            botHealth.OnDamaged += BeHit;
    }

    void OnDisable()
    {
        if (botHealth != null)
            botHealth.OnDamaged -= BeHit;
    }

    void Update()
    {
        CanWalk();
    }

    private void CanWalk()
    {
        if (isWalking)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.forward * speed;
            animator.SetBool("isMoving", true);
            animator.SetFloat("Vertical", 1f);
            animator.SetBool("Block", false);
        }
    }

    private void BeHit()
    {
        isWalking = false;
        animator.SetBool("isMoving", false);
        animator.SetFloat("Vertical", 0f);
        animator.SetBool("Block", true);

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            isWalking = false;
            BeHit();
            Debug.Log("Hit Enemy – Stop");
        }
        else if (other.CompareTag("Player"))
        {
            isWalking = true;
            Debug.Log("Hit Player – move on");
        }
    }

}

