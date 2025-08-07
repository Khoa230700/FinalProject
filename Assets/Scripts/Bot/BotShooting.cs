using UnityEngine;
public class BotShooting : BaseBotAI
{
    [SerializeField] Transform firePoint;
    [SerializeField] ParticleSystem muzzleFlash;
    protected override void Start()
    {
        base.Start();
        enemyDetector = new SimpleLineOfSightDetector(transform, detectionRange);
        attackStrategy = new ShootAttackStrategy(firePoint, muzzleFlash, 1f);
        movementStrategy = new NavMeshMovementStrategy(agent);
    }
    protected override void UpdateBehavior()
    {
        currentTarget = enemyDetector.FindNearestVisibleEnemy();
        if (currentTarget != null)
        {
            movementStrategy.StopMoving();
            transform.LookAt(currentTarget.transform.position);
            if (attackStrategy.CanAttack())
            {
                attackStrategy.Attack(currentTarget);
            }
        }
        else
        {
            movementStrategy.MoveTo(player.position);
        }
    }
}
