using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GunShopItemStrategy : IShopItemStrategy
{
    public void UpdateSlot(ShopEquipItemUI ui, object item, int level = 0, int currentAmmo = 0, int reserveAmmo = 0)
    {
        var gun = item as PlayerShoot;
        ui.avatar.sprite = gun.gunData.gunSprite;
        ui.ammo.text = $"{currentAmmo}/{reserveAmmo}";
        ui.ammo.gameObject.SetActive(true);

        var upgradeState = gun.GetComponent<GunUpgradeState>();
        if (upgradeState != null && ui.shopUpgradeBarUI != null)
        {
            ui.upgradeBarParent.gameObject.SetActive(true);
            ui.shopUpgradeBarUI.SetupUpgradeBar(upgradeState.level, upgradeState.maxLevel);
        }
        else
        {
            ui.upgradeBarParent.gameObject.SetActive(false);
        }

        UpdatePrice(ui, item);
    }

    public void UpdatePrice(ShopEquipItemUI ui, object item)
    {
        var gun = item as PlayerShoot;

        bool needsAmmo = ui.shopManager.NeedsRefill(gun);
        int refillCost = needsAmmo ? ui.shopManager.GetRefillCost(gun) : 0;

        if (needsAmmo && refillCost > 0)
        {
            ui.priceText.text = $"$ {refillCost}";
            ui.priceText.color = CoinManager.Instance.HasEnoughCoins(refillCost)
                ? new Color(0.392f, 0.698f, 0.812f)
                : Color.red;
        }
        else
        {
            ui.priceText.text = "Full Ammo";
            ui.priceText.color = new Color(0.392f, 0.698f, 0.812f);
        }
        ui.priceText.gameObject.SetActive(true);
    }

    public void UpdateDescriptionButton(ShopEquipItemUI ui, object item)
    {
        var gun = item as PlayerShoot;
        var upgradeState = gun.GetComponent<GunUpgradeState>();
        ui.UpdateGunRefillButton(gun);
        ui.UpdateGunUpgradeButton(gun, upgradeState);

        if (ui.descriptionUI.UpgradeButton != null)
        {
            EventTrigger trigger = ui.descriptionUI.UpgradeButton.gameObject.GetComponent<EventTrigger>();
            if (trigger != null) trigger.triggers.Clear();
        }

        if (ui.descriptionUI.UpgradeButton != null
            && ui.shopManager.CanUpgradeGun(upgradeState)
            && upgradeState.level < upgradeState.maxLevel)
        {
            EventTrigger trigger = ui.descriptionUI.UpgradeButton.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = ui.descriptionUI.UpgradeButton.gameObject.AddComponent<EventTrigger>();
            trigger.triggers.Clear();

            var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener((_) => ui.descriptionUI.ShowPreview(gun, "Gun"));
            trigger.triggers.Add(enter);

            var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener((_) => ui.descriptionUI.HidePreview());
            trigger.triggers.Add(exit);
        }
    }

    public void UpdateDescription(ShopEquipItemUI ui, object item, int level)
    {
        var gun = item as PlayerShoot;
        var upgradeState = gun.GetComponent<GunUpgradeState>();
        int gunLevel = upgradeState?.level ?? 0;

        ui.descriptionUI.UpdateDescriptionUI(gun: gun.gunData, gunLevel: gunLevel, upgradeState: upgradeState);

        ui.descriptionUI.ClearButtonListeners();
        ui.descriptionUI.RefillButton?.onClick.AddListener(() => ui.RefillAmmo(gun));
        ui.descriptionUI.UpgradeButton?.onClick.AddListener(() => ui.UpgradeGun(gun, upgradeState));

        UpdateDescriptionButton(ui, gun);
    }

    public void RefreshUI(ShopEquipItemUI ui, object item, int level)
    {
        var gun = item as PlayerShoot;
        var upgradeState = gun.GetComponent<GunUpgradeState>();
        int newLevel = upgradeState?.level ?? 0;

        UpdateSlot(ui, gun, newLevel, gun.currentAmmo, gun.reserveAmmo);

        if (ShopEquipItemUI.currentSelected == ui)
        {
            UpdateDescriptionButton(ui, gun);
            UpdatePrice(ui, gun);
        }
    }

    public void DoubleClick(ShopEquipItemUI ui, object item)
    {
        if (item is PlayerShoot gun)
        {
            ui.RefillAmmo(gun);
        }
    }
}
// public void HandleActionUpgradeRefill(ShopEquipItemUI ui, object item) { var gun = item as PlayerShoot; var upgradeState = gun.GetComponent<GunUpgradeState>(); if (upgradeState != null && ui.shopManager.CanUpgradeGun(upgradeState) && CoinManager.Instance.HasEnoughCoins(ui.shopManager.GetGunUpgradeCost(upgradeState))) { ui.UpgradeGun(gun, upgradeState); } else { ui.RefillAmmo(gun); } }
// public void UpdatePriceUIUpgradeRefill(ShopEquipItemUI ui, object item) { var gun = item as PlayerShoot; var upgradeState = gun.GetComponent<GunUpgradeState>(); int refillCost = 0, upgradeCost = 0; bool canRefill = true, canUpgrade = true; string label = ""; bool needsAmmo = ui.shopManager.NeedsRefill(gun); if (!needsAmmo) { canRefill = false; } else refillCost = ui.shopManager.GetRefillCost(gun); if (upgradeState != null && ui.shopManager.CanUpgradeGun(upgradeState)) upgradeCost = ui.shopManager.GetGunUpgradeCost(upgradeState); else canUpgrade = false; if (canUpgrade && (!canRefill || upgradeCost <= refillCost * 2)) { label = canUpgrade && CoinManager.Instance.HasEnoughCoins(upgradeCost) ? $"$ {upgradeCost}" : "Max Level"; } else if (canRefill) { label = $"$ {refillCost}"; } else { label = "Full Ammo"; canRefill = false; } ui.priceText.text = label; bool affordable = true; bool prioritizeUpgrade = canUpgrade && (!canRefill || upgradeCost <= refillCost * 2); affordable = prioritizeUpgrade ? CoinManager.Instance.HasEnoughCoins(upgradeCost) : CoinManager.Instance.HasEnoughCoins(refillCost); ui.priceText.color = affordable ? Color.red : new Color(0.392f, 0.698f, 0.812f); ui.priceText.gameObject.SetActive(true); }

