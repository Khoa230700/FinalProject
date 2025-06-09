using System;

public static class EvenBus 
{
    public static event Action OnWaveCompleted; // Hoàn thành 1 wave
    public static event Action<int> OnWaveStarted; // Bắt đầu new wave, truyền số
    public static event Action OnPlayerDied; // Player dead
    public static event Action<int> OnEnemyKilled; // truyền số lượng enemy bị diệt
    public static event Action OnGameOver; // End game

    public static void WaveCompleted() => OnWaveCompleted?.Invoke();
    public static void WaveStarted(int wave) => OnWaveStarted?.Invoke(wave);
    public static void PlayerDied() => OnPlayerDied?.Invoke();
    public static void EnemyKilled(int count) => OnEnemyKilled?.Invoke(count);
    public static void GameOver() => OnGameOver?.Invoke();
}
