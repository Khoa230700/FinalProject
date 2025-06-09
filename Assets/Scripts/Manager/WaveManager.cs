using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class WaveContent
    {
        public int normalZombie;
        public int explodeZombie;
        public int scorpionZombie;
    }

    [Header("Wave Settings")]
    [SerializeField] private List<Transform> spawnPoints; // Danh sách vị trí spawn zombies
    [SerializeField] private GameObject normalZombiePrefab;
    [SerializeField] private GameObject explodeZombiePrefab;
    [SerializeField] private GameObject scorpionZombiePrefab;

    [SerializeField] private float spawnDelay = 0.5f; // Delay mỗi lần spoawn
    [Header("Wave For Map 1")]
    [SerializeField] private List<WaveContent> waves; // Setup từng wave cố định

    private int currentWaveIndex = 0;
    private int enemiesRemaining;
    private Queue<GameObject> spawnQueue = new Queue<GameObject>();

    private void Start()
    {
        
    }

    // Bắt đầu wave
    private void StartWave()
    {
        if(currentWaveIndex >= waves.Count)
        {
            Debug.Log("Hoàn thành map 1!");
            EvenBus.WaveCompleted();
            return;
        }
        WaveContent wave = waves[currentWaveIndex];
        spawnQueue.Clear();

        for (int i = 0; i < wave.normalZombie; i++)
            spawnQueue.Enqueue(normalZombiePrefab);
        for (int i = 0; i < wave.explodeZombie; i++)
            spawnQueue.Enqueue(explodeZombiePrefab);
        for (int i = 0; i < wave.scorpionZombie; i++)
            spawnQueue.Enqueue(scorpionZombiePrefab);

        enemiesRemaining = spawnQueue.Count;

        EvenBus.WaveStarted(currentWaveIndex + 1);
        InvokeRepeating(nameof(SpawnEnemy), 0f, spawnDelay);
    }
    
    private void SpawnEnemy()
    {
        if( spawnQueue.Count == 0)
        {
            CancelInvoke(nameof(SpawnEnemy));
            return;
        }

        GameObject prefab = spawnQueue.Dequeue();
        int index = Random.Range(0, spawnPoints.Count);
        Instantiate(prefab, spawnPoints[index].position, Quaternion.identity);
        enemiesRemaining--;

        if (enemiesRemaining <= 0)
        {
            CancelInvoke(nameof(SpawnEnemy));
            currentWaveIndex++;

            Debug.Log($"Wave {currentWaveIndex} complete! Spawn shop, hàng chờ 60s");

            // Sau mỗi wave có 60s để tìm Shop 
            Invoke(nameof(StartWave), 60f);
        }
    }
    private void OnEnemyKilled(int count)
    {
        // Hook cộng điểm, cập nhật UI nếu cần.
    }

    private void OnDestroy()
    {
        EvenBus.OnEnemyKilled -= OnEnemyKilled;
    }
}
}
