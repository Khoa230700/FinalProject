using System.Collections.Generic;
using UnityEngine;

public class WaveSetup : MonoBehaviour
{
    public WaveManager waveManager;

    private void Start()
    {
        // Tạo dữ liệu wave
        List<WaveData> waves = new List<WaveData>();

        // Wave 1
        waves.Add(new WaveData
        {
            enemies = new List<EnemySpawnInfo>
            {
                new EnemySpawnInfo { enemyType = EnemyType.Nor, count = 20 }
            }
        });

        // Wave 2
        waves.Add(new WaveData
        {
            enemies = new List<EnemySpawnInfo>
            {
                new EnemySpawnInfo { enemyType = EnemyType.Nor, count = 25 },
                new EnemySpawnInfo { enemyType = EnemyType.Fat, count = 5 }
            }
        });

        // Wave 3
        waves.Add(new WaveData
        {
            enemies = new List<EnemySpawnInfo>
            {
                new EnemySpawnInfo { enemyType = EnemyType.Nor, count = 25 },
                new EnemySpawnInfo { enemyType = EnemyType.Fat, count = 6 },
                new EnemySpawnInfo { enemyType = EnemyType.Taill, count = 4 }
            }
        });

        waveManager.SetWaves(waves);
        waveManager.StartGame();
    }
}
