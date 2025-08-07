using UnityEngine;
using UnityEngine.AI;

public class ShootAttackStrategy : IAttackStrategy
{
    private readonly Transform firePoint;
    private readonly ParticleSystem muzzleFlash;
    private readonly float cooldown;
    private float lastAttackTime;

    public ShootAttackStrategy(Transform firePoint, ParticleSystem muzzleFlash, float cooldown)
    {
        this.firePoint = firePoint;
        this.muzzleFlash = muzzleFlash;
        this.cooldown = cooldown;
    }

    public bool CanAttack()
    {
        return Time.time - lastAttackTime > cooldown;
    }

    public void Attack(GameObject target)
    {
        if (!CanAttack()) return;

        lastAttackTime = Time.time;

        Vector3 dir = (target.transform.position - firePoint.position).normalized;
        if (Physics.Raycast(firePoint.position, dir, out RaycastHit hit, maxDistance: 100f))
        {
            muzzleFlash?.Play();
            var hb =  hit.collider.gameObject.GetComponentInChildren<Hitbox>(); 
            hb?.ownerHealthSystem.TakeDamage(20);
        }
    }
}
