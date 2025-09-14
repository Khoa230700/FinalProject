using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour, ISaveLoad
{
    public static WeaponManager Instance { get; private set; }

    private List<IWeapon> weapons = new();
    private GameObject player;
    private int selectedIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        player = SelectorSpawner.Instance?.Player;
        selectedIndex = PlayerPrefs.GetInt("CharacterHSelector", 0);
        CacheWeapons();

        SaveLoadManager.Instance?.Register(this);
    }

    private void OnDestroy()
    {
        SaveLoadManager.Instance?.Unregister(this);
    }

    private void CacheWeapons()
    {
        weapons.Clear();
        weapons.AddRange(player.GetComponentsInChildren<IWeapon>(true));
    }

    public IReadOnlyList<IWeapon> GetWeapons() => weapons;

    // ISaveLoad
    public void SaveToData(GameData data)
    {
        var charData = data.GetCharacterData(selectedIndex);

        var weaponData = new WeaponData();
        foreach (var weapon in weapons)
        {
            if (weapon is PlayerShoot gun)
            {
                var upgrade = gun.GetComponent<GunUpgradeState>();
                weaponData.guns.Add(new GunSave
                {
                    gunId = gun.gunData.gunName,
                    level = upgrade != null ? upgrade.level : 0,
                    slot = gun.gunData.gunSlot
                });
            }
            else if (weapon is MeleeWeapon melee)
            {
                weaponData.melee = new MeleeSave
                {
                    meleeId = melee.data.weaponName,
                    level = melee.level
                };
            }
        }

        charData.weaponData = weaponData;
    }

    public void LoadFromData(GameData data)
    {
        var charData = data.GetCharacterData(selectedIndex);

        if (charData.weaponData == null) return;

        var weaponData = charData.weaponData;

        // Load guns
        foreach (var save in weaponData.guns)
        {
            foreach (var weapon in weapons)
            {
                if (weapon is PlayerShoot gun && gun.gunData.gunName == save.gunId)
                {
                    var upgrade = gun.GetComponent<GunUpgradeState>();
                    if (upgrade != null) upgrade.level = save.level;
                }
            }
        }

        // Load melee
        var meleeSave = weaponData.melee;
        if (meleeSave != null && !string.IsNullOrEmpty(meleeSave.meleeId))
        {
            foreach (var weapon in weapons)
            {
                if (weapon is MeleeWeapon melee && melee.data.weaponName == meleeSave.meleeId)
                {
                    melee.level = meleeSave.level;
                }
            }
        }
    }
}
