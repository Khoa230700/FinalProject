using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Nor,
    Fat,
    Taill
}
[System.Serializable]
public class EnemySpawnInfo
{
    public EnemyType enemyType;
    public int count;
}

[System.Serializable]
public class WaveData
{
    public List<EnemySpawnInfo> enemies;
}
