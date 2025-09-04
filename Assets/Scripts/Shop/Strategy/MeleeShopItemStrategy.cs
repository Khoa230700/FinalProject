using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MeleeShopItemStrategy : IShopItemStrategy
{
    public void UpdateSlot(ShopEquipItemUI ui, object item, int level = 0, int currentAmmo = 0, int reserveAmmo = 0)
    {
        var melee = item as MeleeWeapon;
        ui.avatar.sprite = melee.data.weaponSprite;
        ui.ammo.gameObject.SetActive(false);

        if (ui.shopUpgradeBarUI != null && ui.shopManager != null)
        {
            ui.upgradeBarParent.gameObject.SetActive(true);
            ui.shopUpgradeBarUI.SetupUpgradeBar(level, melee.maxLevel);
        }

        UpdatePrice(ui, item);
    }

    public void UpdatePrice(ShopEquipItemUI ui, object item)
    {
        var melee = item as MeleeWeapon;
        string label = "";
        bool canUpgrade = ui.shopManager.CanUpgradeMelee(melee);
        int upgradeCost = 0;

        if (!canUpgrade)
        {
            label = "Max Level";
        }
        else
        {
            upgradeCost = ui.shopManager.GetMeleeUpgradeCost(melee);
            label = $"$ {upgradeCost}";
        }

        ui.priceText.text = label;
        ui.priceText.color = CoinManager.Instance.HasEnoughCoins(upgradeCost)
            ? new Color(0.392f, 0.698f, 0.812f)
            : Color.red;
        ui.priceText.gameObject.SetActive(true);
    }

    public void UpdateDescriptionButton(ShopEquipItemUI ui, object item)
    {
        var melee = item as MeleeWeapon;
        ui.UpdateMeleeUpgradeButton(melee);
    }

    public void UpdateDescription(ShopEquipItemUI ui, object item, int level)
    {
        var melee = item as MeleeWeapon;
        ui.descriptionUI.UpdateDescriptionUI(melee: melee.data, meleeLevel: level);
        ui.descriptionUI.UpgradeButton?.onClick.AddListener(() => ui.UpgradeMelee(melee));
        UpdateDescriptionButton(ui, melee);

        if (ui.descriptionUI.UpgradeButton != null)
        {
            EventTrigger trigger = ui.descriptionUI.UpgradeButton.gameObject.GetComponent<EventTrigger>();
            if (trigger != null) trigger.triggers.Clear();
        }

        if (ui.descriptionUI.UpgradeButton != null && ui.shopManager.CanUpgradeMelee(melee))
        {
            EventTrigger trigger = ui.descriptionUI.UpgradeButton.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = ui.descriptionUI.UpgradeButton.gameObject.AddComponent<EventTrigger>();
            trigger.triggers.Clear();

            var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener((_) => ui.descriptionUI.ShowPreview(melee, "Melee"));
            trigger.triggers.Add(enter);

            var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener((_) => ui.descriptionUI.HidePreview());
            trigger.triggers.Add(exit);
        }
    }

    public void RefreshUI(ShopEquipItemUI ui, object item, int level)
    {
        var melee = item as MeleeWeapon;
        UpdateSlot(ui, melee, melee.level);
        if (ShopEquipItemUI.currentSelected == ui) UpdateDescriptionButton(ui, melee);
    }

    public void DoubleClick(ShopEquipItemUI ui, object item)
    {
        var melee = item as MeleeWeapon;
        ui.UpgradeMelee(melee);
    }
}
