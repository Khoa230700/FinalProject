using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldShopItemStrategy : IShopItemStrategy
{
    public void UpdateSlot(ShopEquipItemUI ui, object item, int level = 0, int currentAmmo = 0, int reserveAmmo = 0)
    {
        var shield = item as PlayerHealthSystem;
        float current = shield.CurrentShield, max = shield.MaxShield;

        ui.ammo.text = $"{(int)current}/{(int)max}";
        if (ui.sliderBar) ui.sliderBar.fillAmount = current / max;
        ui.ammo.gameObject.SetActive(true);

        UpdatePrice(ui, item);
    }

    public void UpdatePrice(ShopEquipItemUI ui, object item)
    {
        var shield = item as PlayerHealthSystem;
        string label = "";
        int cost = 0;

        if (!ui.shopManager.NeedsShield(shield))
        {
            label = "Full Shield";
        }
        else
        {
            cost = ui.shopManager.GetShieldCost(shield);
            label = $"$ {cost}";
        }

        ui.priceText.text = label;
        ui.priceText.color = ui.shopManager.NeedsShield(shield) && CoinManager.Instance.HasEnoughCoins(cost)
            ? Color.red
            : new Color(0.392f, 0.698f, 0.812f);
        ui.priceText.gameObject.SetActive(true);
    }

    public void UpdateDescriptionButton(ShopEquipItemUI ui, object item)
    {
        ui.UpdateShieldButton(item as PlayerHealthSystem);
    }

    public void UpdateDescription(ShopEquipItemUI ui, object item, int level)
    {
        var shield = item as PlayerHealthSystem;
        ui.descriptionUI.UpdateDescriptionUI(shield: shield);
        ui.descriptionUI.RefillButton?.onClick.AddListener(() => ui.RestoreStat(shield, "Shield"));
        UpdateDescriptionButton(ui, shield);
    }

    public void RefreshUI(ShopEquipItemUI ui, object item, int level)
    {
        var shield = item as PlayerHealthSystem;
        UpdateSlot(ui, shield);
        if (ShopEquipItemUI.currentSelected == ui) UpdateDescriptionButton(ui, shield);
    }

    public void DoubleClick(ShopEquipItemUI ui, object item)
    {
        ui.RestoreStat(item as PlayerHealthSystem, "Shield");
    }
}
