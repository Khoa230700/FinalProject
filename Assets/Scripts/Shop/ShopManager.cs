using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public bool RefillAmmo(PlayerShoot gun, int bulletsToBuy = -1)
    {
        if (gun == null) return false;

        if (bulletsToBuy == -1)
            bulletsToBuy = gun.GetAmmoNeeded();

        bulletsToBuy = Mathf.Min(bulletsToBuy, gun.GetAmmoNeeded());
        if (bulletsToBuy <= 0) return false;

        int cost = CalculateRefillCost(gun, bulletsToBuy);
        if (!CoinManager.Instance.HasEnoughCoins(cost)) return false;

        CoinManager.Instance.RemoveCoins(cost);
        int added = gun.AddAmmo(bulletsToBuy);

        Debug.Log($"Refilled {added} bullets for {gun.gunData.name}. Cost: {cost} coins");
        return true;
    }

    public bool CanRefill(PlayerShoot gun, int bulletsToBuy)
    {
        if (gun == null) return false;
        int cost = CalculateRefillCost(gun, bulletsToBuy);
        return CoinManager.Instance.HasEnoughCoins(cost);
    }

    public int CalculateRefillCost(PlayerShoot gun, int bulletsToBuy)
    {
        if (gun == null) return 0;

        int maxBullets = gun.GetAmmoNeeded();
        if (bulletsToBuy == -1) bulletsToBuy = maxBullets;

        // Nếu refill full thì áp dụng cost cho max
        if (bulletsToBuy >= maxBullets)
        {
            int fullCost = maxBullets * gun.gunData.bulletRefillCost;
            return fullCost;
        }

        return bulletsToBuy * gun.gunData.bulletRefillCost;
    }

    public int GetRefillCost(PlayerShoot gun, int bulletsToBuy = -1)
    {
        if (gun == null) return 0;
        return CalculateRefillCost(gun, bulletsToBuy);
    }
}
