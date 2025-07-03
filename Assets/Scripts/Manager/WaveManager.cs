using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawnManager : MonoBehaviour
{
    public Transform[] spawnPoints;

    private int currentWave = 0;
    private int activeEnemies = 0;
    private bool waitingForNextWave = false;

    public float timeBetweenWaves = 60f;
    private bool isSkipping = false;

    private Dictionary<string, int>[] waveConfigs = new Dictionary<string, int>[]
    {
        new Dictionary<string, int> { { "normal", 10 } },
        new Dictionary<string, int> { { "normal", 30 }, { "shot", 5 } },
        new Dictionary<string, int> { { "normal", 30 }, { "shot", 5 }, { "bomb", 5 } }
    };

    void Start()
    {
        StartCoroutine(GameLoop());
    }

    void Update()
    {
        if (waitingForNextWave && Input.GetKeyDown(KeyCode.N))
        {
            isSkipping = true;
        }
    }

    IEnumerator GameLoop()
    {
        yield return null; 

        while (currentWave < waveConfigs.Length)
        {
            // Đợi đến khi enemy wave trước bị tiêu diệt
            if (currentWave > 0)
            {
                yield return new WaitUntil(() => activeEnemies <= 0);
                waitingForNextWave = true;

                float timer = 0f;
                while (timer < timeBetweenWaves && !isSkipping)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }

                waitingForNextWave = false;
                isSkipping = false;
            }

            SpawnWave(currentWave);
            currentWave++;
        }
    }

    void SpawnWave(int waveIndex)
    {
        StartCoroutine(SpawnWaveGradually(waveIndex));
    }

    IEnumerator SpawnWaveGradually(int waveIndex)
    {
        var config = waveConfigs[waveIndex];
        float delayBetweenSpawns = 0.3f;

        foreach (var kvp in config)
        {
            string tag = kvp.Key;
            int count = kvp.Value;

            if (tag == "normal")
            {
                
                int perPoint = count / spawnPoints.Length;

                for (int i = 0; i < spawnPoints.Length; i++)
                {
                    for (int j = 0; j < perPoint; j++)
                    {
                        Transform spawnPoint = spawnPoints[i];
                        SpawnEnemyFromPool(tag, spawnPoint);
                        yield return new WaitForSeconds(delayBetweenSpawns);
                    }
                }
            }
            else
            {
                // Những loại còn lại thì spawn ngẫu nhiên như cũ
                for (int i = 0; i < count; i++)
                {
                    Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                    SpawnEnemyFromPool(tag, spawnPoint);
                    yield return new WaitForSeconds(delayBetweenSpawns);
                }
            }
        }

        Debug.Log($"Wave {waveIndex + 1} spawn completed. Total active: {activeEnemies}");
    }

    void SpawnEnemyFromPool(string tag, Transform spawnPoint)
    {
        GameObject enemy = ObjectPooler.Instance.SpawnFromPool(tag, spawnPoint.position, Quaternion.identity);

        if (enemy == null)
        {
            Debug.LogWarning($"No enemy available in pool with tag: {tag}");
            return;
        }

        EnemyTracker tracker = enemy.GetComponent<EnemyTracker>();
        if (tracker != null)
        {
            tracker.OnDeath = null;
            tracker.OnDeath = OnEnemyDeath;
        }

        activeEnemies++;
    }



    void OnEnemyDeath()
    {
        activeEnemies--;
        if (activeEnemies < 0) activeEnemies = 0;
    }
}
