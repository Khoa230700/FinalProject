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

            
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>().TakeDamage(10);
        }
    }
}
