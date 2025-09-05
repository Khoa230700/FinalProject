using UnityEngine;
using System;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    [Header("Coin Settings")]
    [SerializeField] private int currentCoins = 0;
    [SerializeField] private int sessionCoins = 0;

    public event Action<int, int> OnCoinChanged;

    private void Awake()
    {
        Instance = this;
        LoadCoins();
        sessionCoins = 0; // reset khi bắt đầu session
    }

    public void AddCoins(int amount)
    {
        if (amount > 0) sessionCoins += amount;

        int newAmount = Mathf.Max(0, currentCoins + amount);
        OnCoinChanged?.Invoke(currentCoins, newAmount);

        currentCoins = newAmount;
        SaveCoins();
    }

    public void RemoveCoins(int amount) => AddCoins(-amount);
    public int GetCoins() => currentCoins;
    public int GetSessionCoins() => sessionCoins;
    public bool HasEnoughCoins(int amount) => currentCoins >= amount;

    public void SaveCoins()
    {
        SaveLoadData.Data.coins = currentCoins;
        SaveLoadManager.Instance?.MarkDirty();
    }

    public void LoadCoins()
    {
        int old = currentCoins;
        currentCoins = SaveLoadData.Data.coins;

        OnCoinChanged?.Invoke(old, currentCoins);
    }
}
