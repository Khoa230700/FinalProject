using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private List<WaveData> waves;
    [SerializeField] private float timeBetweenWaves = 10f;
    [SerializeField] private SpawnManager spawnManager;

    private int currentWaveIndex = 0;
    private bool isBetweenWaves = false;

    public void SetWaves(List<WaveData> newWaves)
    {
        waves = newWaves;
    }
    public void StartGame()
    {
        currentWaveIndex = 0;
        StartCoroutine(HandleWave());
    }

    private IEnumerator HandleWave()
    {
        while (currentWaveIndex < waves.Count)
        {
            // Spawn wave hiện tại
            spawnManager.SpawnWave(waves[currentWaveIndex]);

            // Chờ cho wave này kết thúc (tất cả enemy chết)
            yield return new WaitUntil(() => spawnManager.ActiveEnemyCount == 0);

            // Nếu chưa phải wave cuối thì nghỉ
            if (currentWaveIndex < waves.Count - 1)
            {
                isBetweenWaves = true;
                float timer = timeBetweenWaves;
                while (timer > 0f && isBetweenWaves)
                {
                    timer -= Time.deltaTime;
                    yield return null;
                }
                isBetweenWaves = false;
            }

            // Sang wave tiếp theo
            currentWaveIndex++;
        }
    }

    public void SkipBreak()
    {
        if (isBetweenWaves)
            isBetweenWaves = false;
    }
}
