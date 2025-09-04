using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopEquipItemUI : MonoBehaviour, ISelectHandler, IPointerClickHandler
{
    public static ShopEquipItemUI currentSelected;

    [Header("UI")]
    public Transform upgradeBarParent;
    public Image avatar;
    public TMP_Text ammo, priceText;
    public Image sliderBar;
    public GameObject emptyPanel;

    [HideInInspector] public ShopEquipDescriptionsUI descriptionUI;
    [HideInInspector] public ShopManager shopManager;
    [HideInInspector] public Animator animator;
    [HideInInspector] public ShopUpgradeBarUI shopUpgradeBarUI;

    private object currentItem;
    private IShopItemStrategy currentStrategy;
    private int itemLevel;
    private float lastClickTime;

    private void Awake()
    {
        descriptionUI = FindAnyObjectByType<ShopEquipDescriptionsUI>();
        shopManager = FindAnyObjectByType<ShopManager>();
        animator = GetComponent<Animator>();
        if (upgradeBarParent != null) shopUpgradeBarUI = upgradeBarParent.GetComponent<ShopUpgradeBarUI>();
    }

    public void UpdateSlot(object item, string type, int level = 0, int currentAmmo = 0, int reserveAmmo = 0)
    {
        currentItem = item;
        itemLevel = level;
        currentStrategy = CreateStrategy(type);

        if (item == null)
        {
            HideUI();
            return;
        }

        ShowUI();
        currentStrategy.UpdateSlot(this, item, level, currentAmmo, reserveAmmo);
    }

    private void UpdateDescription() => currentStrategy?.UpdateDescription(this, currentItem, itemLevel);
    public void RefreshUI() => currentStrategy?.RefreshUI(this, currentItem, itemLevel);

    // UI EVENTS
    public void OnSelect(BaseEventData eventData) => ApplySelection();

    private void ApplySelection()
    {
        if (currentSelected != null && currentSelected != this)
            currentSelected.animator.SetBool("IsSelected", false);

        currentSelected = this;
        animator.SetBool("IsSelected", true);

        descriptionUI?.ClearButtonListeners();
        descriptionUI?.HidePreview();
        UpdateDescription();
        RefreshUI();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ApplySelection();
        if (Time.time - lastClickTime < 0.3f)
        {
            currentStrategy?.DoubleClick(this, currentItem);
        }
        lastClickTime = Time.time;
    }

    // ACTIONS
    public void UpgradeGun(PlayerShoot gun, GunUpgradeState upgradeState)
    {
        if (upgradeState != null && shopManager.UpgradeGun(upgradeState))
        {
            itemLevel = upgradeState.level;
            UpdateSlot(gun, "Gun", upgradeState.level, gun.currentAmmo, gun.reserveAmmo);
            descriptionUI.UpdateDescriptionUI(gun: gun.gunData, gunLevel: upgradeState.level, upgradeState: upgradeState);

            descriptionUI.ShowPreview(gun, "Gun");

            currentStrategy.UpdateDescriptionButton(this, gun);

            if (shopUpgradeBarUI != null)
            {
                shopUpgradeBarUI.UpdateLevel(upgradeState.level);
                shopUpgradeBarUI.PlayAnimation(upgradeState.level - 1);
            }
        }
    }

    public void UpgradeMelee(MeleeWeapon melee)
    {
        if (shopManager.UpgradeMelee(melee))
        {
            itemLevel = melee.level;
            UpdateSlot(melee, "Melee", melee.level);
            descriptionUI.UpdateDescriptionUI(melee: melee.data, meleeLevel: melee.level);
            currentStrategy.UpdateDescriptionButton(this, melee);

            if (shopUpgradeBarUI != null)
            {
                shopUpgradeBarUI.UpdateLevel(melee.level);
                shopUpgradeBarUI.PlayAnimation(melee.level - 1);
            }
        }
    }

    public void RefillAmmo(PlayerShoot gun)
    {
        if (shopManager.RefillAmmo(gun))
        {
            UpdateSlot(gun, "Gun", itemLevel, gun.currentAmmo, gun.reserveAmmo);
            currentStrategy.UpdateDescriptionButton(this, gun);
        }
    }

    public void RestoreStat(PlayerHealthSystem stat, string type)
    {
        if (type == "Health" ? shopManager.HealPlayer(stat) : shopManager.ShieldPlayer(stat))
        {
            UpdateSlot(stat, type);
            currentStrategy.UpdateDescriptionButton(this, stat);
        }
    }

    // BUTTON
    public void UpdateGunRefillButton(PlayerShoot gun)
    {
        bool needsAmmo = shopManager.NeedsRefill(gun);
        int cost = needsAmmo ? shopManager.GetRefillCost(gun) : 0;
        UpdateDescriptionButton(descriptionUI.RefillButton, "Full Ammo", "Refill", cost, needsAmmo);
    }

    public void UpdateGunUpgradeButton(PlayerShoot gun, GunUpgradeState upgradeState)
    {
        bool canUpgrade = upgradeState != null && shopManager.CanUpgradeGun(upgradeState);
        int cost = canUpgrade ? shopManager.GetGunUpgradeCost(upgradeState) : 0;
        UpdateDescriptionButton(descriptionUI.UpgradeButton, "Max Level", "Upgrade", cost, canUpgrade);
    }

    public void UpdateMeleeUpgradeButton(MeleeWeapon melee)
    {
        bool canUpgrade = shopManager.CanUpgradeMelee(melee);
        int cost = canUpgrade ? shopManager.GetMeleeUpgradeCost(melee) : 0;
        UpdateDescriptionButton(descriptionUI.UpgradeButton, "Max Level", "Upgrade", cost, canUpgrade);
    }

    public void UpdateHealthButton(PlayerHealthSystem health)
    {
        bool needsHeal = shopManager.NeedsHeal(health);
        int cost = needsHeal ? shopManager.GetHealCost(health) : 0;
        UpdateDescriptionButton(descriptionUI.RefillButton, "Full Health", "Heal", cost, needsHeal);
    }

    public void UpdateShieldButton(PlayerHealthSystem shield)
    {
        bool needsShield = shopManager.NeedsShield(shield);
        int cost = needsShield ? shopManager.GetShieldCost(shield) : 0;
        UpdateDescriptionButton(descriptionUI.RefillButton, "Full Shield", "Shield", cost, needsShield);
    }

    // HELPER
    private IShopItemStrategy CreateStrategy(string type)
    {
        return type switch
        {
            "Gun" => new GunShopItemStrategy(),
            "Melee" => new MeleeShopItemStrategy(),
            "Health" => new HealthShopItemStrategy(),
            "Shield" => new ShieldShopItemStrategy(),
            _ => null
        };
    }

    private void HideUI()
    {
        currentItem = null;
        emptyPanel.SetActive(true);
        if (upgradeBarParent != null) upgradeBarParent.gameObject.SetActive(false);
    }

    private void ShowUI() => emptyPanel.SetActive(false);

    private void UpdateDescriptionButton(Button button, string labelWhenFull, string actionName, int cost, bool canDoAction)
    {
        if (button == null) return;

        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();

        if (!canDoAction)
        {
            if (buttonText) buttonText.text = labelWhenFull;
            button.interactable = false;
        }
        else
        {
            if (buttonText) buttonText.text = $"{actionName} (${cost})";
            button.interactable = CoinManager.Instance.HasEnoughCoins(cost);
        }
    }
}
