using Unity.VisualScripting;
using UnityEngine;



public class EnemyBullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 3f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player hit by projectile!");
            // Deal damage to player here
            Destroy(gameObject);
        }
    }
    
}
