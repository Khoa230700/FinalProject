using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Settings")]
    public int healCostPerHP = 1;

    //AMMO
    public bool RefillAmmo(PlayerShoot gun)
    {
        if (gun == null) return false;

        int availableCoins = CoinManager.Instance.GetCoins();
        if (availableCoins <= 0) return false;

        int bulletsNeeded = gun.gunData.magazineSize + gun.gunData.reserveAmmo - (gun.currentAmmo + gun.reserveAmmo);
        if (bulletsNeeded <= 0) return false;

        int maxAffordableBullets = availableCoins / gun.gunData.bulletRefillCost;
        int bulletsToBuy = Mathf.Min(bulletsNeeded, maxAffordableBullets);
        int totalCost = bulletsToBuy * gun.gunData.bulletRefillCost;

        if (bulletsToBuy > 0)
        {
            gun.AddAmmo(bulletsToBuy);
            CoinManager.Instance.RemoveCoins(totalCost);
            return true;
        }

        return false;
    }

    public int GetRefillCost(PlayerShoot gun)
    {
        if (gun == null) return 0;

        int bulletsNeeded = gun.gunData.magazineSize + gun.gunData.reserveAmmo - (gun.currentAmmo + gun.reserveAmmo);
        return bulletsNeeded * gun.gunData.bulletRefillCost;
    }

    public bool NeedsRefill(PlayerShoot gun)
    {
        if (gun == null) return false;
        return (gun.currentAmmo + gun.reserveAmmo) < (gun.gunData.magazineSize + gun.gunData.reserveAmmo);
    }

    //HEALTH
    public bool HealPlayer(PlayerHealth player)
    {
        if (player == null) return false;

        int availableCoins = CoinManager.Instance.GetCoins();
        if (availableCoins <= 0) return false;

        int missingHP = (int)(player.maxHealth - player.currentHealth);
        if (missingHP <= 0) return false;

        int maxAffordableHP = availableCoins / healCostPerHP;
        int healAmount = Mathf.Min(missingHP, maxAffordableHP);
        int cost = healAmount * healCostPerHP;

        if (healAmount > 0)
        {
            player.UpdateHealth(healAmount);
            CoinManager.Instance.RemoveCoins(cost);
            return true;
        }

        return false;
    }

    public int GetHealCost(PlayerHealth player)
    {
        if (player == null) return 0;

        int missingHP = (int)(player.maxHealth - player.currentHealth);
        return missingHP * healCostPerHP;
    }

    public bool NeedsHealing(PlayerHealth player)
    {
        if (player == null) return false;
        return player.currentHealth < player.maxHealth;
    }
}
