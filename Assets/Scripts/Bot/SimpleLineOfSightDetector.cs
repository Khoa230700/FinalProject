using UnityEngine;

public class SimpleLineOfSightDetector : IEnemyDetector
{
    private readonly Transform origin;
    private readonly float range;

    public SimpleLineOfSightDetector(Transform origin, float range)
    {
        this.origin = origin;
        this.range = range;
    }

    public GameObject FindNearestVisibleEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closest = null;
        float minDist = Mathf.Infinity;

        foreach (var enemy in enemies)
        {
            float dist = Vector3.Distance(origin.position, enemy.transform.position);
            if (dist < range && HasLineOfSight(enemy.transform))
            {
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = enemy;
                }
            }
        }

        return closest;
    }

    private bool HasLineOfSight(Transform target)
    {
        Vector3 direction = (target.position - origin.position).normalized;
        float distance = Vector3.Distance(origin.position, target.position);
        return Physics.Raycast(origin.position, direction, out RaycastHit hit, distance)
               && hit.collider.CompareTag("Enemy");
    }
}