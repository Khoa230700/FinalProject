using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour, ISaveLoad
{
    public static WeaponManager Instance { get; private set; }

    private List<IWeapon> weapons = new();
    private GameObject player;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        player = SelectorSpawner.Instance?.Player;
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
        if (data.weaponData == null)
            data.weaponData = new WeaponData();

        data.weaponData.guns.Clear();

        foreach (var weapon in weapons)
        {
            if (weapon is PlayerShoot gun)
            {
                var upgrade = gun.GetComponent<GunUpgradeState>();
                data.weaponData.guns.Add(new GunSave
                {
                    gunId = gun.gunData.gunName,
                    level = upgrade != null ? upgrade.level : 0,
                    slot = gun.gunData.gunSlot
                });
            }
            else if (weapon is MeleeWeapon melee)
            {
                data.weaponData.melee = new MeleeSave
                {
                    meleeId = melee.data.weaponName,
                    level = melee.level
                };
            }
        }
    }

    public void LoadFromData(GameData data)
    {
        if (data.weaponData == null) return;

        // Guns
        foreach (var save in data.weaponData.guns)
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

        // Melee
        var meleeSave = data.weaponData.melee;
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
