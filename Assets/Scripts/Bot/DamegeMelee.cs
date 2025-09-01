using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class DamegeMelee : MonoBehaviour
{
    [Header("Melee Settings")]
    public float damage = 10f;
    public float attackRange = 1.8f;     // tầm chém
    public float attackRadius = 0.6f;    // bán kính
    public float cooldown = 1f;          // delay giữa 2 cú chém
    public LayerMask enemyMask;          // layer Enemy

    [Header("Refs")]
    public Transform swingOrigin;        // đặt ở tay/dao/rìu
 

    [Header("Debug")]
    public bool drawGizmos = true;

    private bool canAttack = true;

    private void Update()
    {
        // Auto tìm enemy trong tầm → chém
        if (canAttack)
        {
            Collider[] hits = Physics.OverlapSphere(
                swingOrigin.position + transform.forward * attackRange * 0.5f,
                attackRadius,
                enemyMask,
                QueryTriggerInteraction.Ignore);

            if (hits.Length > 0)
            {
                StartCoroutine(DoAttack(hits));
            }
        }
    }

    private IEnumerator DoAttack(Collider[] hits)
    {
        canAttack = false;

        yield return new WaitForSeconds(0.2f);

        HashSet<GameObject> damaged = new HashSet<GameObject>();

        Debug.Log($"[BotMelee] Quét được {hits.Length} collider trong vùng chém");

        foreach (var col in hits)
        {
            EnemyM enemy = col.GetComponent<EnemyM>() ?? col.GetComponentInParent<EnemyM>();

            if (enemy != null && !damaged.Contains(enemy.gameObject))
            {
                damaged.Add(enemy.gameObject);

                Debug.Log($"[BotMelee] Gây {damage} dame lên {enemy.name} | Máu trước: {enemy.currentHealth}");

                enemy.TakeDamage(damage);

                Debug.Log($"[BotMelee] Máu sau: {enemy.currentHealth}");
            }
            else
            {
                Debug.LogWarning($"[BotMelee] Collider '{col.name}' (Layer: {LayerMask.LayerToName(col.gameObject.layer)}) KHÔNG có EnemyM");
            }
        }

        yield return new WaitForSeconds(cooldown);
        canAttack = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos || swingOrigin == null) return;

        Vector3 center = swingOrigin.position + transform.forward * attackRange * 0.5f;
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(center, attackRadius);
    }
}
