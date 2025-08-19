using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints; // 10 spawn point
    [SerializeField] private ObjectPooler objectPooler; // Pool chứa các enemy prefab

    public int ActiveEnemyCount { get; private set; }

    private Dictionary<EnemyType, string> enemyTagMap;

    private void Awake()
    {
        // Map Enum -> Tag (phải khớp tag trong ObjectPooler)
        enemyTagMap = new Dictionary<EnemyType, string>
        {
            { EnemyType.Nor, "Nor" },
            { EnemyType.Fat, "Fat" },
            { EnemyType.Taill, "Taill" }
        };
    }

    public void SpawnWave(WaveData waveData)
    {
        ActiveEnemyCount = 0;

        foreach (EnemySpawnInfo info in waveData.enemies)
        {
            SpawnEnemyType(info.enemyType, info.count);
        }
    }

    private void SpawnEnemyType(EnemyType type, int count)
    {
        if (count <= 0) return;

        // Nếu không tìm thấy tag → báo lỗi
        if (!enemyTagMap.ContainsKey(type))
        {
            Debug.LogError($"SpawnManager: Không tìm thấy mapping cho enemy type {type}");
            return;
        }

        string tag = enemyTagMap[type];

        List<int> usedSpawnIndexes = new List<int>();

        for (int i = 0; i < count; i++)
        {
            // Nếu đã dùng hết spawn point thì reset
            if (usedSpawnIndexes.Count >= spawnPoints.Length)
                usedSpawnIndexes.Clear();

            // Chọn spawnpoint chưa dùng trong vòng chia này
            int spawnIndex;
            do
            {
                spawnIndex = Random.Range(0, spawnPoints.Length);
            } while (usedSpawnIndexes.Contains(spawnIndex));

            usedSpawnIndexes.Add(spawnIndex);

            // Lấy enemy từ pool
            GameObject enemy = objectPooler.SpawnFromPool(tag, spawnPoints[spawnIndex].position, Quaternion.identity);

            if (enemy == null)
            {
                Debug.LogError($"SpawnManager: Không spawn được enemy với tag '{tag}' - Kiểm tra ObjectPooler");
                continue;
            }

            EnemyTracker tracker = enemy.GetComponent<EnemyTracker>();
            if (tracker != null)
            {
                tracker.OnDeath += OnEnemyDeath;
            }
            else
            {
                Debug.LogError($"SpawnManager: Prefab '{enemy.name}' không có EnemyTracker!");
            }

            ActiveEnemyCount++;
        }
    }

    private void OnEnemyDeath()
    {
        ActiveEnemyCount--;
    }
}
