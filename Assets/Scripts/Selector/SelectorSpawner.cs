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

        // Spawn player
        selectedIndex = PlayerPrefs.GetInt("CharacterHSelector", 0);
        if (selectedIndex < 0 || selectedIndex >= playerPfs.Length) selectedIndex = 0;

        Player = Instantiate(playerPfs[selectedIndex].playerPrefab, transform.position, transform.rotation);

        playerClassNameUI.UpdateUI(playerPfs[selectedIndex]);
    }

    [ContextMenu("Respawn")] // Test
    public void RespawnAtLastSave()
    {
        if (SaveLoadData.Data.playerData == null)
        {
            Debug.LogWarning("Không có dữ liệu player để respawn!");
            return;
        }

        var pd = SaveLoadData.Data.playerData;
        Vector3 pos = new Vector3(pd.posX, pd.posY, pd.posZ);
        float rotY = pd.rotY;

        SpawnAt(pos, rotY);
    }

    private void SpawnAt(Vector3 pos, float rotY)
    {
        // Xóa player cũ nếu có
        if (Player != null) Destroy(Player);

        Quaternion rot = Quaternion.Euler(0, rotY, 0);
        Player = Instantiate(playerPfs[selectedIndex].playerPrefab, pos, rot);
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
