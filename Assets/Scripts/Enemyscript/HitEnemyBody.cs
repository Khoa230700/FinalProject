using UnityEngine;

public class HitEnemyBody : MonoBehaviour
{
    public EnemyM enemyM;
     void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
            {
            enemyM.GetComponent<EnemyM>().TakeDamage(10);
        }
    }
}
