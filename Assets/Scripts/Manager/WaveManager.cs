using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private List<WaveData> waves;
    [SerializeField] private float timeBetweenWaves = 10f;
    [SerializeField] private SpawnManager spawnManager;

    [Header("UI References")]
    [SerializeField] private TimerUI timerUI;

    private int  currentWaveIndex = 0;
    private bool isBetweenWaves   = false;

    public void SetWaves(List<WaveData> newWaves) => waves = newWaves;

    public void StartGame()
    {
        currentWaveIndex = 0;

        // đảm bảo UI tắt hẳn ở wave 1
        if (timerUI != null)
        {
            timerUI.HideUI();
            timerUI.SetVisible(false);
        }

        StartCoroutine(HandleWave());
    }

    private IEnumerator HandleWave()
    {
        while (currentWaveIndex < waves.Count)
        {
            // Spawn wave hiện tại (wave 1 sẽ không có UI đếm)
            spawnManager.SpawnWave(waves[currentWaveIndex]);

            // chờ hết quái
            yield return new WaitUntil(() => spawnManager.ActiveEnemyCount == 0);

            // nếu còn wave sau thì nghỉ và đếm ngược
            if (currentWaveIndex < waves.Count - 1)
            {
                isBetweenWaves = true;

                if (timerUI != null)
                {
                    timerUI.HideUI();
                    timerUI.SetVisible(true); // bật UI để đếm
                }

                yield return StartCoroutine(WaveCountdown(timeBetweenWaves));

                isBetweenWaves = false;

                if (timerUI != null)
                {
                    timerUI.HideUI();
                    timerUI.SetVisible(false); // tắt lại sau khi đếm xong
                }
            }

            currentWaveIndex++;
        }
    }

    private IEnumerator WaveCountdown(float countdown)
    {
        float timer = countdown;

        while (timer > 0f && isBetweenWaves)
        {
            int seconds = Mathf.CeilToInt(timer);

            if (timerUI != null)
                timerUI.UpdateUI($"NEXT WAVE IN {seconds}s", timer / countdown);

            yield return null;
            timer -= Time.deltaTime;
        }
    }

    public void SkipBreak()
    {
        if (isBetweenWaves) isBetweenWaves = false;
    }
}
