using UnityEngine;
using System;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    [Header("Coin Settings")]
    [SerializeField] private int currentCoins = 0;

    public event Action<int, int> OnCoinChanged;

    private void Awake()
    {
        Instance = this;

        LoadCoins();
    }

    public void AddCoins(int amount)
    {
        int newAmount = Mathf.Max(0, currentCoins + amount);

        OnCoinChanged?.Invoke(currentCoins, newAmount);

        currentCoins = newAmount;
        SaveCoins();
    }


    public void RemoveCoins(int amount)
    {
        AddCoins(-amount);
    }

    public int GetCoins()
    {
        return currentCoins;
    }

    public bool HasEnoughCoins(int amount)
    {
        return currentCoins >= amount;
    }

    private void SaveCoins()
    {
        PlayerPrefs.SetInt("PlayerCoins", currentCoins);
        PlayerPrefs.Save();
    }

    private void LoadCoins()
    {
        currentCoins = PlayerPrefs.GetInt("PlayerCoins", 0);
    }
}