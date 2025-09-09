using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class DamegeMelee : MonoBehaviour
{
    [Header("Melee Settings")]
    public float damage = 10f;
    public float attackRange = 1.8f;
    public float attackRadius = 0.6f;
    public float cooldown = 1f;
    public LayerMask enemyMask;

    [Header("Refs")]
    public Transform swingOrigin;

    [Header("Debug")]
    public bool drawGizmos = true;
    private bool canAttack = true;

    public void Attack()
    {
        if (!canAttack) return;

        Vector3 center = swingOrigin.position + transform.forward * attackRange * 0.5f;
        Collider[] hits = Physics.OverlapSphere(center, attackRadius, enemyMask, QueryTriggerInteraction.Ignore);

        Debug.Log($"[BotMelee] Attack() gọi - tìm thấy {hits.Length} collider trong vùng chém");

        if (hits.Length > 0)
            StartCoroutine(DoAttack(hits));
        else
            StartCoroutine(Cooldown());
    }

    private IEnumerator DoAttack(Collider[] hits)
    {
        canAttack = false;

        yield return new WaitForSeconds(0.2f); // delay animation

        HashSet<GameObject> damaged = new HashSet<GameObject>();

        foreach (var col in hits)
        {
            EnemyM enemy = col.GetComponent<EnemyM>() ?? col.GetComponentInChildren<EnemyM>();

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

    private IEnumerator Cooldown()
    {
        canAttack = false;
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
