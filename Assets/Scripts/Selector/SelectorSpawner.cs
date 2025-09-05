using System;
using UnityEngine;

public class SelectorSpawner : MonoBehaviour
{
    public static SelectorSpawner Instance { get; private set; }

    [Header("Prefabs")]
    public PlayerSelectorUI[] playerPfs;

    [Header("UI")]
    [SerializeField] private BarUI healthBar;
    [SerializeField] private BarUI shieldBar;
    [SerializeField] private WeaponUI weaponUI;
    [SerializeField] private PlayerClassNameUI playerClassNameUI;

    public GameObject Player { get; private set; }
    public BarUI HealthBar => healthBar;
    public BarUI ShieldBar => shieldBar;
    public WeaponUI WeaponUI => weaponUI;

    private int selectedIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        selectedIndex = PlayerPrefs.GetInt("CharacterHSelector", 0);
        if (selectedIndex < 0 || selectedIndex >= playerPfs.Length) selectedIndex = 0;

        // Spawn player
        Player = Instantiate(playerPfs[selectedIndex].playerPrefab, transform.position, transform.rotation);

        playerClassNameUI.UpdateUI(playerPfs[selectedIndex]);
    }
}

[Serializable]
public class PlayerSelectorUI
{
    public GameObject playerPrefab;
    public Sprite avatar;
    public Sprite classAvatar;
    public string name;
}
