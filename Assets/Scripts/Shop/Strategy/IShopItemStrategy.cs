public interface IShopItemStrategy
{
    void UpdateSlot(ShopEquipItemUI ui, object item, int level = 0, int currentAmmo = 0, int reserveAmmo = 0);
    void UpdatePrice(ShopEquipItemUI ui, object item);
    void UpdateDescriptionButton(ShopEquipItemUI ui, object item);
    void UpdateDescription(ShopEquipItemUI ui, object item, int level);
    void RefreshUI(ShopEquipItemUI ui, object item, int level);
    void DoubleClick(ShopEquipItemUI ui, object item); // double-click action
}
