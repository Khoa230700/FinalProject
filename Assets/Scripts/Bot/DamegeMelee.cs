using UnityEngine;

public class MeleeTrigger : MonoBehaviour
{
    public float damage = 50f;

    private void OnTriggerEnter(Collider other)
    {
        // 1. Enemy thường
        EnemyM enemy = other.GetComponent<EnemyM>()
                    ?? other.GetComponentInParent<EnemyM>()
         ?? other.GetComponentInChildren<EnemyM>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Debug.Log($"[MeleeTrigger] Gây {damage} damage lên {enemy.name}");
            return;
        }

        // 2. Boss
        BossHealth boss = other.GetComponent<BossHealth>()
                        ?? other.GetComponentInParent<BossHealth>();
        if (boss != null)
        {
            boss.TakeDamage(damage);
            Debug.Log($"[MeleeTrigger] Gây {damage} damage lên Boss: {boss.name}");
            return;
        }

        // 3. Hitbox nâng cao
        Hitbox hb = other.GetComponent<Hitbox>()
                 ?? other.GetComponentInParent<Hitbox>();
        if (hb != null && hb.ownerHealthSystem != null)
        {
            hb.ownerHealthSystem.TakeDamage(damage);
            hb.OnHit(damage, other.transform.position);
            Debug.Log($"[MeleeTrigger] Gây {damage} damage qua Hitbox: {hb.ownerHealthSystem.name}");
        }
    }
}