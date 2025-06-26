using UnityEngine;

public class HitEnemyBody : MonoBehaviour
{
    public EnemyM enemyM;
    public GameObject blood;
    public float bloodLifetime = 1f;
    // void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Bullet"))
    //        {
    //        enemyM.GetComponent<EnemyM>().TakeDamage(10);
    //    }
    //}

    void onHit()
    {
        Ray ray = new Ray();
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            enemyM.GetComponent<EnemyM>().TakeDamage(30);
            GameObject losthp = Instantiate(blood,transform.position, Quaternion.identity);
            Destroy(losthp, bloodLifetime);
        }
    }
}
