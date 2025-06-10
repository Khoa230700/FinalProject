using UnityEngine;

public class EnemyHealthSystem : BaseHealthSystem
{
    [SerializeField] private int goldReward = 10;

    protected override void Die()
    {
        Debug.Log("Enemy died!");
        EvenBus.EnemyKilled(goldReward);
        gameObject.SetActive(false); // Tái sử dụng ObjectPooling
    }
}
