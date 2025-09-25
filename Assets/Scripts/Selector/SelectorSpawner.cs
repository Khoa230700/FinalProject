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

    // giữ reference crosshair runtime
    private CrosshairBloomController _crosshair;

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

        // TÌM crosshair ở trong prefab player (kể cả inactive)
        _crosshair = Player.GetComponentInChildren<CrosshairBloomController>(true);

        playerClassNameUI.UpdateUI(playerPfs[selectedIndex]);

        // (tuỳ chọn) đảm bảo crosshair bật khi mới vào scene
        _crosshair?.Show();
    }
    // ====== API cho Timeline (Signal Receiver bind vào 2 hàm này) ======
    public void HideCrosshair() => _crosshair?.Hide();
    public void ShowCrosshair() => _crosshair?.Show();
}

[Serializable]
public class PlayerSelectorUI
{
    public GameObject playerPrefab;
    public Sprite avatar;
    public Sprite classAvatar;
    public string name;
}
