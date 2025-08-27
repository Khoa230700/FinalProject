using System.Net.NetworkInformation;
using UnityEngine;

public class DamegeMelee : MonoBehaviour
{
  

   

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Hit Enemy");

            // Tìm script EnemyM ở chính collider hoặc cha của nó
            EnemyM enemy = other.GetComponent<EnemyM>() ?? other.GetComponentInParent<EnemyM>();
            //if (enemy == null)
            //    enemy = other.GetComponentInParent<EnemyM>();

            if (enemy != null)
            {
                enemy.TakeDamage(10f);
            }
            else
            {
                Debug.LogWarning($"Không tìm thấy EnemyM trên {other.name}");
            }
        }
    }
}
