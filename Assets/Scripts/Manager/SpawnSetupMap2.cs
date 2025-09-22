using System.Collections.Generic;
using UnityEngine;

public class SpawnSetupMap2 : MonoBehaviour
{
    public Map2SpawnController map2SpawnController;

    private void Start()
    {
        List<WaveData> waves = new List<WaveData>();

        waves.Add(new WaveData
        {
            enemies = new List<EnemySpawnInfo>
            {
                new EnemySpawnInfo { enemyType = EnemyType.Nor, count = 10 },
                new EnemySpawnInfo { enemyType = EnemyType.Fat, count = 5 }
            }
        });

        waves.Add(new WaveData
        {
            enemies = new List<EnemySpawnInfo>
            {
                new EnemySpawnInfo { enemyType = EnemyType.Nor, count = 10 },
                new EnemySpawnInfo { enemyType = EnemyType.Fat, count = 5 }
            }
        });
        map2SpawnController.SetWaves(waves);
        map2SpawnController.StartGame();
    }
}
