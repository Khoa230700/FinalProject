using System;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Settings")]
    public int healCostPerHP = 1;
    public int shieldCostPerPoint = 1;

    // Generic method for purchasing resources
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
        int bulletsNeeded = gun.gunData.magazineSize + gun.gunData.reserveAmmo - (gun.currentAmmo + gun.reserveAmmo);
        return bulletsNeeded * gun.gunData.bulletRefillCost;
    }

    public bool NeedsRefill(PlayerShoot gun)
    {
        return gun != null && (gun.currentAmmo + gun.reserveAmmo) < (gun.gunData.magazineSize + gun.gunData.reserveAmmo);
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
}
