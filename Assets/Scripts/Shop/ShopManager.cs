using System;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Resource Settings")]
    public int healCostPerHP = 1;
    public int shieldCostPerPoint = 1;

    [Header("Upgrade Settings")]
    public float upgradeCostMultiplier = 1.5f;
    public int meleeUpgradeCost = 50;

    [Header("Gun Upgrade Settings")]
    public int gunUpgradeBaseCost = 100;
    public float gunUpgradeCostMultiplier = 1.8f;
    public int maxGunLevel = 5; // Default max level for guns without specific limits

    // ===========================
    // Generic Purchase Method
    // ===========================
    private bool PurchaseResource<T>(T target, Func<T, int> getCost, Func<T, bool> needsResource,
                                    Func<T, int, bool> applyResource, int costPerUnit)
    {
        if (target == null) return false;

        int available = CoinManager.Instance.GetCoins();
        if (available <= 0 || !needsResource(target)) return false;

        int fullCost = getCost(target);
        int maxAffordable = available / costPerUnit;
        int actualCost = Mathf.Min(fullCost, maxAffordable * costPerUnit);
        int amount = actualCost / costPerUnit;

        if (amount > 0 && applyResource(target, amount))
        {
            CoinManager.Instance.RemoveCoins(actualCost);
            return true;
        }
        return false;
    }

    // ===========================
    // AMMO
    // ===========================
    public bool RefillAmmo(PlayerShoot gun)
    {
        return PurchaseResource(gun, GetRefillCost, NeedsRefill,
            (g, bullets) => { g.AddAmmo(bullets); return true; },
            gun?.gunData.bulletRefillCost ?? 0);
    }

    public int GetRefillCost(PlayerShoot gun)
    {
        if (gun == null) return 0;

        var upgradeState = gun.GetComponent<GunUpgradeState>();
        int magazineSize = upgradeState != null ? upgradeState.MagazineSize : gun.gunData.magazineSize;

        int bulletsNeeded = magazineSize + gun.gunData.reserveAmmo - (gun.currentAmmo + gun.reserveAmmo);
        return bulletsNeeded * gun.gunData.bulletRefillCost;
    }

    public bool NeedsRefill(PlayerShoot gun)
    {
        if (gun == null) return false;

        var upgradeState = gun.GetComponent<GunUpgradeState>();
        int magazineSize = upgradeState != null ? upgradeState.MagazineSize : gun.gunData.magazineSize;

        return (gun.currentAmmo + gun.reserveAmmo) < (magazineSize + gun.gunData.reserveAmmo);
    }

    // ===========================
    // HEALTH
    // ===========================
    public bool HealPlayer(PlayerHealthSystem health)
    {
        return PurchaseResource(health, GetHealCost, NeedsHeal,
            (p, amount) => { p.Heal(amount); return true; }, healCostPerHP);
    }

    public int GetHealCost(PlayerHealthSystem health)
    {
        return health == null ? 0 : (int)(health.MaxHealth - health.CurrentHealth) * healCostPerHP;
    }

    public bool NeedsHeal(PlayerHealthSystem health)
    {
        return health != null && health.CurrentHealth < health.MaxHealth;
    }

    // ===========================
    // SHIELD
    // ===========================
    public bool ShieldPlayer(PlayerHealthSystem shield)
    {
        return PurchaseResource(shield, GetShieldCost, NeedsShield,
            (s, amount) => { s.AddShield(amount); return true; }, shieldCostPerPoint);
    }

    public int GetShieldCost(PlayerHealthSystem shield)
    {
        return shield == null ? 0 : (int)(shield.MaxShield - shield.CurrentShield) * shieldCostPerPoint;
    }

    public bool NeedsShield(PlayerHealthSystem shield)
    {
        return shield != null && shield.CurrentShield < shield.MaxShield;
    }

    // ===========================
    // GUN UPGRADE
    // ===========================
    public bool UpgradeGun(GunUpgradeState gunUpgradeState)
    {
        if (gunUpgradeState == null) return false;

        int currentLevel = gunUpgradeState.level;
        int maxLevel = GetMaxGunLevel(gunUpgradeState);
        int upgradeCost = GetGunUpgradeCost(gunUpgradeState);

        if (!CanUpgradeGun(gunUpgradeState) || !CoinManager.Instance.HasEnoughCoins(upgradeCost))
            return false;

        CoinManager.Instance.RemoveCoins(upgradeCost);
        gunUpgradeState.LevelUp();

        return true;
    }

    public bool UpgradeGun(PlayerShoot gun)
    {
        if (gun == null) return false;

        var upgradeState = gun.GetComponent<GunUpgradeState>();
        return upgradeState != null && UpgradeGun(upgradeState);
    }

    public int GetGunUpgradeCost(GunUpgradeState gunUpgradeState)
    {
        if (gunUpgradeState == null) return 0;

        int currentLevel = gunUpgradeState.level;
        int maxLevel = GetMaxGunLevel(gunUpgradeState);

        if (currentLevel >= maxLevel) return 0;

        return Mathf.RoundToInt(gunUpgradeBaseCost * Mathf.Pow(gunUpgradeCostMultiplier, currentLevel));
    }

    public int GetGunUpgradeCost(PlayerShoot gun)
    {
        if (gun == null) return 0;
        var upgradeState = gun.GetComponent<GunUpgradeState>();
        return GetGunUpgradeCost(upgradeState);
    }

    public bool CanUpgradeGun(GunUpgradeState gunUpgradeState)
    {
        if (gunUpgradeState == null) return false;
        return gunUpgradeState.level < GetMaxGunLevel(gunUpgradeState);
    }

    public bool CanUpgradeGun(PlayerShoot gun)
    {
        if (gun == null) return false;
        var upgradeState = gun.GetComponent<GunUpgradeState>();
        return CanUpgradeGun(upgradeState);
    }

    public int GetMaxGunLevel(GunUpgradeState gunUpgradeState)
    {
        // You can add a maxLevel field to GunUpgradeState or GunData if needed
        // For now, use the global maxGunLevel setting
        return maxGunLevel;
    }

    public int GetMaxGunLevel(PlayerShoot gun)
    {
        if (gun == null) return 0;
        var upgradeState = gun.GetComponent<GunUpgradeState>();
        return GetMaxGunLevel(upgradeState);
    }

    // ===========================
    // MELEE UPGRADE (existing)
    // ===========================
    public bool UpgradeMelee(MeleeWeapon melee)
    {
        if (melee == null) return false;

        int currentLevel = melee.level;
        int maxLevel = melee.data.maxLevel;
        int upgradeCost = GetUpgradeCost(currentLevel, maxLevel);

        if (!CanUpgrade(currentLevel, maxLevel) || !CoinManager.Instance.HasEnoughCoins(upgradeCost)) return false;

        CoinManager.Instance.RemoveCoins(upgradeCost);
        melee.level++;

        return true;
    }

    public int GetUpgradeCost(int currentLevel, int maxLevel)
    {
        if (currentLevel >= maxLevel) return 0;
        return Mathf.RoundToInt(meleeUpgradeCost * Mathf.Pow(upgradeCostMultiplier, currentLevel));
    }

    public bool CanUpgrade(int currentLevel, int maxLevel)
    {
        return currentLevel < maxLevel;
    }
}