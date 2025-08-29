using UnityEngine;

public class BossSkillCollision : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<testPlayerHealth>().TakeDamage(20);
        }
    }
}
