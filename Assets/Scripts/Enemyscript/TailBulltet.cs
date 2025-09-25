using UnityEngine;

public class TailBulltet : MonoBehaviour
{
    //private void Ontrigger(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        //Debug.Log("Player inside fire area");
    //        // Apply damage over time
    //        var health = other.GetComponent<PlayerHealth>().TakeDamage(10);

            
    [SerializeField]private float damage = 2f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealthSystem>().TakeDamage(damage);
            //other.GetComponent<testPlayerHealth>().TakeDamage(10);
        }
    }
}
