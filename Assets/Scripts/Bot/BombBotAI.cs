using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class BombBotAI : MonoBehaviour
{
    public Rigidbody rb;
    public CapsuleCollider capsuleCollider;
    public Animator animator;
    protected float speed = 5f;
    protected float cooldownBeaten;
    private bool isWalking = true;


    void Start()
    {
       
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
            animator.SetFloat("Vertical", value: 1f);
            animator.SetBool("Block", false);
        }

    }

    private void BeHit()
    {
        animator.SetBool("isMoving", false);
        animator.SetFloat("Vertical", value: 0f);
        animator.SetBool("Block",true);
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") )
        {
            isWalking = false;
            //rb.isKinematic = true; 
            BeHit();
       
            Debug.Log("Hit Enemy – Stop");
        }
        else
        {
            if (other.CompareTag("Player"))
            {
                isWalking = true;
                //rb.isKinematic = false;
                
                Debug.Log("Hit Player – moveon");
            }
        }
    }
   
}

