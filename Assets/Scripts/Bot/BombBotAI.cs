using UnityEngine;

public class BombBotAI : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody rb;
    public CapsuleCollider capsuleCollider;
    public Animator animator;

    [Header("Movement")]
    public float speed = 2f;

    private bool isWalking = true;
    private bool isBlocked = false;

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
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (isWalking && !isBlocked)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.forward * speed;

            animator.SetBool("isMoving", true);
            animator.SetFloat("Vertical", 1f);
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
            animator.SetBool("isMoving", false);
            animator.SetFloat("Vertical", 0f);
        }
    }

    private void BeHit()
    {
        isWalking = false;   // dừng lại
        isBlocked = true;    // đang bị block

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        animator.SetBool("Block", true);
        animator.SetBool("isMoving", false);

        Debug.Log("[BombBotAI] Bot bị hit → vào trạng thái Block");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            BeHit();
            Debug.Log("Hit Enemy – Bot Blocked");
        }
        else if (other.CompareTag("Player"))
        {
            // Khi Player chạm → bot trở lại bình thường ngay
            isWalking = true;
            isBlocked = false;
            rb.isKinematic = false;

            animator.SetBool("Block", false);
            animator.SetBool("isMoving", true);
            animator.SetFloat("Vertical", 1f);

            Debug.Log("Hit Player – Bot thoát Block và di chuyển lại");
        }
    }
}