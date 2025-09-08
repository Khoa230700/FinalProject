using UnityEngine;
using System;
using VInspector;

public class CoinManager : MonoBehaviour, ISaveLoad
{
    public static CoinManager Instance { get; private set; }

    [Header("Coin Settings")]
    [SerializeField] private int currentCoins = 0;
    [SerializeField] private int sessionCoins = 0;

    public event Action<int, int> OnCoinChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        SaveLoadManager.Instance?.Register(this);
    }

    private void OnDestroy()
    {
        SaveLoadManager.Instance?.Unregister(this);
    }

    public void AddCoins(int amount)
    {
        if (amount > 0) sessionCoins += amount;

        int oldAmount = currentCoins;
        currentCoins = Mathf.Max(0, currentCoins + amount);

        OnCoinChanged?.Invoke(oldAmount, currentCoins);
        SaveLoadManager.Instance?.MarkDirty();
    }

    public void RemoveCoins(int amount) => AddCoins(-amount);
    public int GetCoins() => currentCoins;
    public int GetSessionCoins() => sessionCoins;
    public bool HasEnoughCoins(int amount) => currentCoins >= amount;

    // ISaveLoad
    public void SaveToData(GameData data)
    {
        data.coins = currentCoins;
    }

    public void LoadFromData(GameData data)
    {
        if (data != null)
        {
            int oldAmount = currentCoins;
            currentCoins = data.coins;
            sessionCoins = 0;

            OnCoinChanged?.Invoke(oldAmount, currentCoins);
        }
    }

    [Button("Add Test Coins")]
    public void AddTestCoins()
    {
        AddCoins(UnityEngine.Random.Range(100, 1000));
    }
}
