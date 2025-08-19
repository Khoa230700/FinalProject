using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class BombBotAI : MonoBehaviour
{
    protected Rigidbody rb;
    protected CapsuleCollider capsuleCollider;
    protected Animator animator;
    protected float speed = 2f;
    protected float cooldownBeaten;
    private bool isWalking = true;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();
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
        if (other.CompareTag("Enemy"))
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
    ////private GameObject FindNearestEnemy()
    //{
    //    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
    //    GameObject nearestEnemy = null;
    //    float minDistance = Mathf.Infinity;
    //    foreach (GameObject enemy in enemies)
    //    {
    //        float distance = Vector3.Distance(transform.position, enemy.transform.position);
    //        if (distance < minDistance)
    //        {
    //            minDistance = distance;
    //            nearestEnemy = enemy;
    //        }
    //    }
    //    return nearestEnemy;
    //}
}

