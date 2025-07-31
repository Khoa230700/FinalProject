using System.Net.NetworkInformation;
using UnityEngine;

public class DameAxe: MonoBehaviour
{
    public EnemyM enemyM;

    public void Update()
    {
        Debug.Log("mau" + enemyM.currentHealth);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyM>().TakeDamage(10f);
            
        }
    }
}
