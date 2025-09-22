using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map2SpawnController : MonoBehaviour
{
    [SerializeField] private List<WaveData> waves;
    [SerializeField] private float timeBetweenWaves = 30f;
    [SerializeField] private SpawnManager spawnManager;

    private int  currentWaveIndex = 0;
    public bool isBetweenWaves   = false;

    public void SetWaves(List<WaveData> newWaves) => waves = newWaves;

    public void StartGame()
    {
        currentWaveIndex = 0;
        StartCoroutine(HandleWave());
    }

    private IEnumerator HandleWave()
    {
        while (currentWaveIndex < waves.Count)
        {
            spawnManager.SpawnWave(waves[currentWaveIndex]);

            yield return new WaitUntil(() => spawnManager.ActiveEnemyCount == 0);

            // nếu còn wave sau thì nghỉ và đếm ngược
            if (currentWaveIndex < waves.Count - 1)
            {
                isBetweenWaves = true;
            }

            currentWaveIndex++;
        }
    }

    private IEnumerator WaveCountdown(float countdown)
    {
        float timer = countdown;

        while (timer >= 0f && isBetweenWaves)
        {
            int seconds = Mathf.FloorToInt(timer);
            yield return null;
            timer -= Time.deltaTime;
        }
    }
    public void SkipBreak()
    {
        if (isBetweenWaves) isBetweenWaves = false;
    }
}
