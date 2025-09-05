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

    // Purchase
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
            AudioManager.Instance.PlaySFX("Purchase");

            return true;
        }
        return false;
    }

    // AMMO
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

    // HEALTH
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

    // SHIELD
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

    // GUN
    public bool UpgradeGun(GunUpgradeState gunUpgradeState)
    {
        if (gunUpgradeState == null) return false;

        int upgradeCost = GetGunUpgradeCost(gunUpgradeState);

        if (!CanUpgradeGun(gunUpgradeState) || !CoinManager.Instance.HasEnoughCoins(upgradeCost))
            return false;

        CoinManager.Instance.RemoveCoins(upgradeCost);
        AudioManager.Instance.PlaySFX("Purchase");
        SaveLoadManager.Instance?.QueueAutosave(1.0f);

        gunUpgradeState.LevelUp();

        return true;
    }

    public int GetGunUpgradeCost(GunUpgradeState gunUpgradeState)
    {
        if (gunUpgradeState == null) return 0;

        int currentLevel = gunUpgradeState.level;
        int maxLevel = gunUpgradeState.maxLevel;

        if (currentLevel >= maxLevel) return 0;

        return Mathf.RoundToInt(gunUpgradeBaseCost * Mathf.Pow(gunUpgradeCostMultiplier, currentLevel));
    }

    public bool CanUpgradeGun(GunUpgradeState gunUpgradeState)
    {
        if (gunUpgradeState == null) return false;
        return gunUpgradeState.level < gunUpgradeState.maxLevel;
    }

    // MELEE
    public bool UpgradeMelee(MeleeWeapon melee)
    {
        if (melee == null) return false;

        int upgradeCost = GetMeleeUpgradeCost(melee);

        if (!CanUpgradeMelee(melee) || !CoinManager.Instance.HasEnoughCoins(upgradeCost)) return false;

        CoinManager.Instance.RemoveCoins(upgradeCost);
        AudioManager.Instance.PlaySFX("Purchase");
        SaveLoadManager.Instance?.QueueAutosave(1.0f);
        
        melee.level++;

        return true;
    }

    public int GetMeleeUpgradeCost(MeleeWeapon melee)
    {
        if (melee.level >= melee.maxLevel) return 0;
        return Mathf.RoundToInt(meleeUpgradeCost * Mathf.Pow(upgradeCostMultiplier, melee.level));
    }

    public bool CanUpgradeMelee(MeleeWeapon melee)
    {
        return melee.level < melee.maxLevel;
    }
}