using UnityEngine;

public class testTrigger : MonoBehaviour
{
    public float damagePerSecond = 10f;
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Debug.Log("Player inside fire area");
            // Apply damage over time
            var health = other.GetComponent<PlayerHealth>();
            
            if (health != null)
            {
                health.TakeDamage(damagePerSecond * Time.deltaTime);
                //health.TakeDamage(10);
            }
        }
    }
}
