using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class DamegeMelee : MonoBehaviour
{

    [Header("Melee Settings")]
    public float damage = 20f;
    public float attackRange = 1.8f;
    public float attackRadius = 0.6f;
    public float cooldown = 1f;

    [Header("Refs")]
    public Transform swingOrigin;  // điểm gốc chém
    public LayerMask enemyMask;    // Layer Enemy

    private bool canAttack = true;

    public void Attack()
    {
        if (!canAttack) return;
        StartCoroutine(DoAttack());
    }

    private IEnumerator DoAttack()
    {
        canAttack = false;

        // delay khớp animation (vd: lúc vung dao chạm mục tiêu)
        yield return new WaitForSeconds(0.2f);

        // vị trí quét phía trước
        Vector3 center = swingOrigin.position + transform.forward * attackRange * 0.5f;

        Collider[] hits = Physics.OverlapSphere(center, attackRadius, enemyMask);
        Debug.Log($"[MeleeDamage] Quét được {hits.Length} collider trong vùng chém");

        HashSet<GameObject> damaged = new HashSet<GameObject>();

        foreach (var col in hits)
        {
            EnemyM enemy = col.GetComponent<EnemyM>()
             ?? col.GetComponentInChildren<EnemyM>()
             ?? col.GetComponentInParent<EnemyM>();

            if (enemy != null && !damaged.Contains(enemy.gameObject))
            {
                damaged.Add(enemy.gameObject);

                Debug.Log($"[MeleeDamage] Gây {damage} damage lên {col.name}");
                enemy.TakeDamage(damage);
            }
            else
            {
                Debug.LogWarning($"[MeleeDamage] {col.name} không có EnemyHealth");
            }
        }

        yield return new WaitForSeconds(cooldown);
        canAttack = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (swingOrigin == null) return;

        Vector3 center = swingOrigin.position + transform.forward * attackRange * 0.5f;
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(center, attackRadius);
    }

}
