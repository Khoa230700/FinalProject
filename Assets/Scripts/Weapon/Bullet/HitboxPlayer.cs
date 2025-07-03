using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public EnemyM ownerHealthSystem;
    public enum HitboxType { Body, Head }
    public HitboxType hitboxType;

    public GameObject bloodEffect;

    public void OnHit(float damage, Vector3 hitPoint)
    {
        if (bloodEffect != null)
        {
            GameObject blood = Instantiate(bloodEffect, hitPoint, Quaternion.identity);
            Destroy(blood, 1.5f);
        }
    }
}
