using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public EnemyM ownerHealthSystem;
    public GameObject bloodEffect;
    public enum HitboxType { Body, Head }
    public HitboxType hitboxType;

    public void OnHit(float damage, Vector3 hitPoint)
    {
        ownerHealthSystem.TakeDamage(damage);

        if (bloodEffect != null)
        {
            GameObject blood = Instantiate(bloodEffect, hitPoint, Quaternion.identity);
            Destroy(blood, 2f);
        }
    }

}
