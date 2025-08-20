using UnityEngine;
using System;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    [Header("Coin Settings")]
    [SerializeField] private int currentCoins = 0;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadCoins();
    }

    public int GetCoins()
    {
        return currentCoins;
    }

    public bool HasEnoughCoins(int amount)
    {
        return currentCoins >= amount;
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;

        currentCoins += amount;
        SaveCoins();

        Debug.Log($"Added {amount} coins. Total: {currentCoins}");
    }

    public void SpendCoins(int amount)
    {
        if (amount <= 0) return;

        if (currentCoins >= amount)
        {
            currentCoins -= amount;
            SaveCoins();

            Debug.Log($"Spent {amount} coins. Remaining: {currentCoins}");
        }
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

    // Method for testing/debugging
    [ContextMenu("Add 100 Coins")]
    public void AddTestCoins()
    {
        AddCoins(100);
    }

    [ContextMenu("Remove 50 Coins")]
    public void RemoveTestCoins()
    {
        SpendCoins(50);
    }
}