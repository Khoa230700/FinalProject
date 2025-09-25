using UnityEngine;
using System.Collections;

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
        // Khi bị hit thì chạy coroutine block
        StartCoroutine(BlockRoutine());
    }

    private IEnumerator BlockRoutine()
    {
        isBlocked = true;
        isWalking = false;

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        animator.SetBool("Block", true);

        // Chờ tới khi thật sự vào state Block
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        while (!info.IsName("Block"))
        {
            yield return null;
            info = animator.GetCurrentAnimatorStateInfo(0);
        }

        Debug.Log($"[BombBotAI] Bắt đầu Block trong {info.length} giây");

        // Chờ hết thời gian state Block
        yield return new WaitForSeconds(info.length);

        // Đảm bảo ra khỏi state Block
        while (info.IsName("Block"))
        {
            yield return null;
            info = animator.GetCurrentAnimatorStateInfo(0);
        }

        animator.SetBool("Block", false);

        rb.isKinematic = false;
        isBlocked = false;
        isWalking = true;

        Debug.Log("[BombBotAI] Kết thúc Block, bot tiếp tục di chuyển");
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            BeHit();
            Debug.Log("Hit Enemy – Blocked");
        }
        else if (other.CompareTag("Player"))
        {
            isWalking = true;
            Debug.Log("Hit Player – Move on");
        }
    }
}