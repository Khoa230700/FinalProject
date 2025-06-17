using Unity.VisualScripting;
using UnityEngine;

public class HitEnemyHead : MonoBehaviour
{
    public EnemyM enemyM;
    public ParticleSystem particle;


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            enemyM.GetComponent<EnemyM>().TakeDamage(50);
            particle.Play();
        }
    }


    void onHit()
    {
        Ray ray = new Ray();
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            enemyM.GetComponent<EnemyM>().TakeDamage(50);
            particle.Play();
        }
    }
}
