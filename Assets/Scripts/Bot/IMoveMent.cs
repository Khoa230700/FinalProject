using UnityEngine;

public interface IMovement
{
    void MoveTo(Vector3 position);
    void StopMoving();
}

public interface IAttackStrategy
{
    void Attack(GameObject target);
    bool CanAttack();
}

public interface IEnemyDetector
{
    GameObject FindNearestVisibleEnemy();
}
