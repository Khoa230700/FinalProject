using UnityEngine;

public class testDan : MonoBehaviour
{
    public float lifespan = 10f; // How long the bullet will exist before being destroyed

    void Start()
    {
        Destroy(gameObject, lifespan); // Destroy the bullet after 'lifespan' seconds
    }

    void OnTriggerEnter(Collider other)
    {
        // Example: If the bullet hits an enemy
        if (other.CompareTag("HeadShot"))
        {
            Debug.Log("Hit head");
            // Add damage logic here (e.g., call a method on the enemy)
            other.GetComponent<EnemyM>().TakeDamage(50);
            Destroy(gameObject); // Destroy the bullet on impact
        }
        else if (other.CompareTag("BodyShot"))
        {
            Debug.Log("Hit body" );
            // Add damage logic here (e.g., call a method on the enemy)
            other.GetComponent<EnemyM>().TakeDamage(10);
            Destroy(gameObject); // Destroy the bullet on impact
        }
        // Example: If the bullet hits environment (not the player or other bullets)
        else if (!other.CompareTag("Player") && !other.CompareTag("Bullet"))
        {
            Debug.Log("Bullet hit something else: " + other.name);
            Destroy(gameObject); // Destroy the bullet on impact
        }
    }

    // Use OnCollisionEnter if your bullet has a non-trigger collider
    //void OnCollisionEnter(Collision collision)
    //{
    //    // Example: If the bullet hits an enemy with a non-trigger collider
    //    if (collision.gameObject.CompareTag("HeadShot"))
    //    {
    //        Debug.Log("Physical hit head" );
    //        // Add damage logic here
    //        Destroy(gameObject);
    //    }
    //    else if (collision.gameObject.CompareTag("BodyShot"))
    //    {
    //        Debug.Log("Physical hit body");
    //        // Add damage logic here
    //        Destroy(gameObject);
    //    }
    //    // Example: If the bullet hits environment
    //    else if (!collision.gameObject.CompareTag("Player") && !collision.gameObject.CompareTag("Bullet"))
    //    {
    //        Debug.Log("Bullet physically hit something else: " + collision.gameObject.name);
    //        Destroy(gameObject);
    //    }
    //}
}
