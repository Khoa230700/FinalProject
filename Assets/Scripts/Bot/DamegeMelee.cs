using System.Net.NetworkInformation;
using UnityEngine;

public class DamegeMelee : MonoBehaviour
{
  

   

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyM>().TakeDamage(10f);

        }
    }
}
