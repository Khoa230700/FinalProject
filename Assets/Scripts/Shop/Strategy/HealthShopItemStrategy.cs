using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthShopItemStrategy : IShopItemStrategy
{
    public void UpdateSlot(ShopEquipItemUI ui, object item, int level = 0, int currentAmmo = 0, int reserveAmmo = 0)
    {
        var health = item as PlayerHealthSystem;
        float current = health.CurrentHealth, max = health.MaxHealth;

        ui.ammo.text = $"{(int)current}/{(int)max}";
        if (ui.sliderBar) ui.sliderBar.fillAmount = current / max;
        ui.ammo.gameObject.SetActive(true);

        UpdatePrice(ui, item);
    }

    public void UpdatePrice(ShopEquipItemUI ui, object item)
    {
        var health = item as PlayerHealthSystem;
        string label = "";
        int cost = 0;

        if (!ui.shopManager.NeedsHeal(health))
        {
            label = "Full Health";
        }
        else
        {
            cost = ui.shopManager.GetHealCost(health);
            label = $"$ {cost}";
        }

        ui.priceText.text = label;
        ui.priceText.color = ui.shopManager.NeedsHeal(health) && CoinManager.Instance.HasEnoughCoins(cost)
            ? Color.red
            : new Color(0.392f, 0.698f, 0.812f);
        ui.priceText.gameObject.SetActive(true);
    }

    public void UpdateDescriptionButton(ShopEquipItemUI ui, object item)
    {
        ui.UpdateHealthButton(item as PlayerHealthSystem);
    }

    public void UpdateDescription(ShopEquipItemUI ui, object item, int level)
    {
        var health = item as PlayerHealthSystem;
        ui.descriptionUI.UpdateDescriptionUI(health: health);
        ui.descriptionUI.RefillButton?.onClick.AddListener(() => ui.RestoreStat(health, "Health"));
        UpdateDescriptionButton(ui, health);
    }

    public void RefreshUI(ShopEquipItemUI ui, object item, int level)
    {
        var health = item as PlayerHealthSystem;
        UpdateSlot(ui, health);
        if (ShopEquipItemUI.currentSelected == ui) UpdateDescriptionButton(ui, health);
    }

    public void DoubleClick(ShopEquipItemUI ui, object item)
    {
        ui.RestoreStat(item as PlayerHealthSystem, "Health");
    }
}
