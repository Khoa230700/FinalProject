using UnityEngine;

public class EnemyTracker : MonoBehaviour
{
    // public Action OnDeath;
    private QuestTracker questTracker;

    public System.Action OnDeath { get; internal set; }

    private void Start()
    {
        questTracker = GetComponent<QuestTracker>();
    }

    public void Die()
    {
        // OnDeath?.Invoke();
        questTracker?.OnKilled();
        SpawnManager.Instance.OnEnemyDeath();
        CoinManager.Instance.AddCoins(Random.Range(5, 15)); // Thêm 5-15 coin khi enemy chết
        // Debug.Log($"{gameObject.name} has died.");
        // gameObject.SetActive(false);
    }
}