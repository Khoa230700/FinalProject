using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopEquipItemUI : MonoBehaviour, ISelectHandler, IPointerClickHandler
{
    private static ShopEquipItemUI currentSelected;

    [Header("UI")]
    public Transform upgradeBarParent;
    public Image avatar;
    public TMP_Text ammo, priceText;
    public Image sliderBar;
    public GameObject emptyPanel;

    // Cache
    private object currentItem;
    private string itemType;
    private int meleeLevel;
    private float lastClickTime;

    // References
    private ShopEquipDescriptionsUI descriptionUI;
    private ShopManager shopManager;
    private Animator animator;
    private ShopUpgradeBarUI shopUpgradeBarUI;

    private void Awake()
    {
        descriptionUI = FindAnyObjectByType<ShopEquipDescriptionsUI>();
        shopManager = FindAnyObjectByType<ShopManager>();
        animator = GetComponent<Animator>();
        if (upgradeBarParent != null) shopUpgradeBarUI = upgradeBarParent.GetComponent<ShopUpgradeBarUI>();
    }

    // ==============================
    // SETUP SLOT
    // ==============================
    public void UpdateSlot(object item, string type, int level = 0, int currentAmmo = 0, int reserveAmmo = 0)
    {
        currentItem = item;
        itemType = type;
        meleeLevel = level;

        if (item == null)
        {
            HideUI();
            return;
        }
        ShowUI();

        switch (type)
        {
            case "Gun":    SetupGunSlot(item as PlayerShoot, currentAmmo, reserveAmmo); break;
            case "Melee":  SetupMeleeSlot(item as MeleeWeapon, level); break;
            case "Health": SetupStatSlot(item as PlayerHealthSystem, "Health"); break;
            case "Shield": SetupStatSlot(item as PlayerHealthSystem, "Shield"); break;
        }
    }

    private void SetupGunSlot(PlayerShoot gun, int currentAmmo, int reserveAmmo)
    {
        avatar.sprite = gun.gunData.gunSprite;
        ammo.text = $"{currentAmmo}/{reserveAmmo}";
        ammo.gameObject.SetActive(true);
        UpdatePriceUI("Gun", gun);
    }

    private void SetupMeleeSlot(MeleeWeapon melee, int level)
    {
        avatar.sprite = melee.data.weaponSprite;
        ammo.gameObject.SetActive(false);

        if (shopUpgradeBarUI != null && shopManager != null)
        {
            upgradeBarParent.gameObject.SetActive(true);
            shopUpgradeBarUI.SetupUpgradeBar(level, melee.data.maxLevel);
        }

        UpdatePriceUI("Melee", melee);
    }

    private void SetupStatSlot(PlayerHealthSystem stat, string type)
    {
        float current = type == "Health" ? stat.CurrentHealth : stat.CurrentShield;
        float max     = type == "Health" ? stat.MaxHealth     : stat.MaxShield;

        ammo.text = $"{(int)current}/{(int)max}";
        if (sliderBar) sliderBar.fillAmount = current / max;
        ammo.gameObject.SetActive(true);

        UpdatePriceUI(type, stat);
    }

    // ==============================
    // GENERIC PRICE UPDATE
    // ==============================
    private void UpdatePriceUI(string type, object target)
    {
        if (priceText == null || shopManager == null) return;

        int cost = 0;
        bool canDo = true;
        string label = "";

        switch (type)
        {
            case "Gun":
                var gun = target as PlayerShoot;
                bool needsAmmo = shopManager.NeedsRefill(gun);
                if (!needsAmmo)
                {
                    label = "Full Ammo";
                    canDo = false;
                }
                else
                {
                    cost = shopManager.GetRefillCost(gun);
                    label = $"$ {cost}";
                }
                break;

            case "Melee":
                var melee = target as MeleeWeapon;
                if (!shopManager.CanUpgrade(melee.level, melee.data.maxLevel))
                {
                    label = "Max Level";
                    canDo = false;
                }
                else
                {
                    cost = shopManager.GetUpgradeCost(melee.level, melee.data.maxLevel);
                    label = $"$ {cost}";
                }
                break;

            case "Health":
                var health = target as PlayerHealthSystem;
                bool needsHP = shopManager.NeedsHeal(health);
                if (!needsHP)
                {
                    label = "Full Health";
                    canDo = false;
                }
                else
                {
                    cost = shopManager.GetHealCost(health);
                    label = $"$ {cost}";
                }
                break;

            case "Shield":
                var shield = target as PlayerHealthSystem;
                bool needsShield = shopManager.NeedsShield(shield);
                if (!needsShield)
                {
                    label = "Full Shield";
                    canDo = false;
                }
                else
                {
                    cost = shopManager.GetShieldCost(shield);
                    label = $"$ {cost}";
                }
                break;
        }

        priceText.text = label;
        priceText.color = (canDo && !CoinManager.Instance.HasEnoughCoins(cost))
            ? Color.red
            : new Color(0.392f, 0.698f, 0.812f);

        priceText.gameObject.SetActive(true);
    }

    // ==============================
    // GENERIC BUTTON STATE
    // ==============================
    private void UpdateButtonState(string type, object target)
    {
        if (descriptionUI == null) return;

        Button button = (type == "Melee") ? descriptionUI.UpgradeButton : descriptionUI.RefillButton;
        if (button == null) return;

        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        int cost = 0;
        bool canDo = true;
        string label = "";

        switch (type)
        {
            case "Gun":
                var gun = target as PlayerShoot;
                bool needsAmmo = shopManager.NeedsRefill(gun);
                if (!needsAmmo)
                {
                    label = "Full Ammo";
                    canDo = false;
                }
                else
                {
                    cost = shopManager.GetRefillCost(gun);
                    label = $"Refill (${cost})";
                }
                break;

            case "Melee":
                var melee = target as MeleeWeapon;
                if (!shopManager.CanUpgrade(melee.level, melee.data.maxLevel))
                {
                    label = "Max Level";
                    canDo = false;
                }
                else
                {
                    cost = shopManager.GetUpgradeCost(melee.level, melee.data.maxLevel);
                    label = $"Upgrade (${cost})";
                }
                break;

            case "Health":
                var health = target as PlayerHealthSystem;
                if (!shopManager.NeedsHeal(health))
                {
                    label = "Full Health";
                    canDo = false;
                }
                else
                {
                    cost = shopManager.GetHealCost(health);
                    label = $"Heal (${cost})";
                }
                break;

            case "Shield":
                var shield = target as PlayerHealthSystem;
                if (!shopManager.NeedsShield(shield))
                {
                    label = "Full Shield";
                    canDo = false;
                }
                else
                {
                    cost = shopManager.GetShieldCost(shield);
                    label = $"Shield (${cost})";
                }
                break;
        }

        if (buttonText) buttonText.text = label;
        button.interactable = canDo && CoinManager.Instance.HasEnoughCoins(cost);
    }

    // ==============================
    // EVENT HANDLERS
    // ==============================
    public void OnSelect(BaseEventData eventData) => ApplySelection();

    private void ApplySelection()
    {
        if (currentSelected != null && currentSelected != this)
            currentSelected.animator.SetBool("IsSelected", false);

        currentSelected = this;
        animator.SetBool("IsSelected", true);

        descriptionUI?.ClearButtonListeners();
        UpdateDescription();
    }

    private void UpdateDescription()
    {
        switch (itemType)
        {
            case "Gun" when currentItem is PlayerShoot gun:
                descriptionUI.UpdateDescriptionUI(gun: gun.gunData);
                descriptionUI.RefillButton?.onClick.AddListener(() => HandleRefill(gun));
                UpdateButtonState("Gun", gun);
                break;

            case "Melee" when currentItem is MeleeWeapon melee:
                descriptionUI.UpdateDescriptionUI(melee: melee.data, meleeLevel: meleeLevel);
                descriptionUI.UpgradeButton?.onClick.AddListener(() => HandleMeleeUpgrade(melee));
                UpdateButtonState("Melee", melee);
                break;

            case "Health" when currentItem is PlayerHealthSystem health:
                descriptionUI.UpdateDescriptionUI(health: health);
                descriptionUI.RefillButton?.onClick.AddListener(() => HandleStatRestore(health, "Health"));
                UpdateButtonState("Health", health);
                break;

            case "Shield" when currentItem is PlayerHealthSystem shield:
                descriptionUI.UpdateDescriptionUI(shield: shield);
                descriptionUI.RefillButton?.onClick.AddListener(() => HandleStatRestore(shield, "Shield"));
                UpdateButtonState("Shield", shield);
                break;

            default: descriptionUI?.HideDescription(); break;
        }
    }

    // ==============================
    // ACTIONS
    // ==============================
    private void HandleMeleeUpgrade(MeleeWeapon melee)
    {
        if (shopManager.UpgradeMelee(melee))
        {
            meleeLevel = melee.level;
            UpdateSlot(melee, "Melee", melee.level);
            descriptionUI.UpdateDescriptionUI(melee: melee.data, meleeLevel: melee.level);
            UpdateButtonState("Melee", melee);

            if (shopUpgradeBarUI != null)
            {
                shopUpgradeBarUI.UpdateLevel(melee.level);
                shopUpgradeBarUI.PlayAnimation(melee.level - 1);
            }
        }
    }

    private void HandleRefill(PlayerShoot gun)
    {
        if (shopManager.RefillAmmo(gun))
        {
            UpdateSlot(gun, "Gun", 0, gun.currentAmmo, gun.reserveAmmo);
            UpdateButtonState("Gun", gun);
        }
    }

    private void HandleStatRestore(PlayerHealthSystem stat, string type)
    {
        bool success = type == "Health" ? shopManager.HealPlayer(stat) : shopManager.ShieldPlayer(stat);
        if (success)
        {
            UpdateSlot(stat, type);
            UpdateButtonState(type, stat);
        }
    }

    // ==============================
    // REFRESH
    // ==============================
    public void RefreshUI()
    {
        switch (itemType)
        {
            case "Gun" when currentItem is PlayerShoot gun:
                UpdateSlot(gun, "Gun", 0, gun.currentAmmo, gun.reserveAmmo);
                if (currentSelected == this) UpdateButtonState("Gun", gun);
                break;

            case "Melee" when currentItem is MeleeWeapon melee:
                UpdateSlot(melee, "Melee", melee.level);
                if (currentSelected == this) UpdateButtonState("Melee", melee);
                break;

            case "Health" when currentItem is PlayerHealthSystem health:
                UpdateSlot(health, "Health");
                if (currentSelected == this) UpdateButtonState("Health", health);
                break;

            case "Shield" when currentItem is PlayerHealthSystem shield:
                UpdateSlot(shield, "Shield");
                if (currentSelected == this) UpdateButtonState("Shield", shield);
                break;
        }
    }

    // ==============================
    // HELPERS
    // ==============================
    private void HideUI()
    {
        currentItem = null;
        itemType = null;
        emptyPanel.SetActive(true);
        if (upgradeBarParent != null) upgradeBarParent.gameObject.SetActive(false);
    }

    private void ShowUI() => emptyPanel.SetActive(false);

    public void OnPointerClick(PointerEventData eventData)
    {
        ApplySelection();
        if (Time.time - lastClickTime < 0.3f) // Double click
        {
            switch (itemType)
            {
                case "Gun" when currentItem is PlayerShoot gun: HandleRefill(gun); break;
                case "Melee" when currentItem is MeleeWeapon melee: HandleMeleeUpgrade(melee); break;
                case "Health": HandleStatRestore(currentItem as PlayerHealthSystem, "Health"); break;
                case "Shield": HandleStatRestore(currentItem as PlayerHealthSystem, "Shield"); break;
            }
        }
        lastClickTime = Time.time;
    }
}
