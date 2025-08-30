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
    private int itemLevel; // Now used for both melee and gun levels
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
        itemLevel = level;

        if (item == null)
        {
            HideUI();
            return;
        }
        ShowUI();

        switch (type)
        {
            case "Gun": SetupGunSlot(item as PlayerShoot, currentAmmo, reserveAmmo); break;
            case "Melee": SetupMeleeSlot(item as MeleeWeapon, level); break;
            case "Health": SetupStatSlot(item as PlayerHealthSystem, "Health"); break;
            case "Shield": SetupStatSlot(item as PlayerHealthSystem, "Shield"); break;
        }
    }

    private void SetupGunSlot(PlayerShoot gun, int currentAmmo, int reserveAmmo)
    {
        avatar.sprite = gun.gunData.gunSprite;
        ammo.text = $"{currentAmmo}/{reserveAmmo}";
        ammo.gameObject.SetActive(true);

        // Get gun upgrade state and setup upgrade bar
        var upgradeState = gun.GetComponent<GunUpgradeState>();
        if (upgradeState != null && shopUpgradeBarUI != null && shopManager != null)
        {
            itemLevel = upgradeState.level;
            upgradeBarParent.gameObject.SetActive(true);
            shopUpgradeBarUI.SetupUpgradeBar(upgradeState.level, shopManager.GetMaxGunLevel(gun));
        }
        else
        {
            upgradeBarParent.gameObject.SetActive(false);
        }

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
        float max = type == "Health" ? stat.MaxHealth : stat.MaxShield;

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

        int refillCost = 0, upgradeCost = 0;
        bool canRefill = true, canUpgrade = true;
        string label = "";

        switch (type)
        {
            case "Gun":
                var gun = target as PlayerShoot;
                var upgradeState = gun.GetComponent<GunUpgradeState>();
                
                // Check refill status
                bool needsAmmo = shopManager.NeedsRefill(gun);
                if (!needsAmmo)
                {
                    canRefill = false;
                }
                else
                {
                    refillCost = shopManager.GetRefillCost(gun);
                }

                // Check upgrade status
                if (upgradeState != null && shopManager.CanUpgradeGun(upgradeState))
                {
                    upgradeCost = shopManager.GetGunUpgradeCost(upgradeState);
                }
                else
                {
                    canUpgrade = false;
                }

                // Display the most relevant action
                if (canUpgrade && (!canRefill || upgradeCost <= refillCost * 2)) // Prioritize upgrade if affordable
                {
                    label = canUpgrade && CoinManager.Instance.HasEnoughCoins(upgradeCost) ? $"${upgradeCost}" : "Max Level";
                }
                else if (canRefill)
                {
                    label = $"$ {refillCost}";
                }
                else
                {
                    label = "Full Ammo";
                    canRefill = false;
                }
                break;

            case "Melee":
                var melee = target as MeleeWeapon;
                if (!shopManager.CanUpgrade(melee.level, melee.data.maxLevel))
                {
                    label = "Max Level";
                    canUpgrade = false;
                }
                else
                {
                    upgradeCost = shopManager.GetUpgradeCost(melee.level, melee.data.maxLevel);
                    label = $"$ {upgradeCost}";
                }
                break;

            case "Health":
                var health = target as PlayerHealthSystem;
                bool needsHP = shopManager.NeedsHeal(health);
                if (!needsHP)
                {
                    label = "Full Health";
                    canRefill = false;
                }
                else
                {
                    refillCost = shopManager.GetHealCost(health);
                    label = $"$ {refillCost}";
                }
                break;

            case "Shield":
                var shield = target as PlayerHealthSystem;
                bool needsShield = shopManager.NeedsShield(shield);
                if (!needsShield)
                {
                    label = "Full Shield";
                    canRefill = false;
                }
                else
                {
                    refillCost = shopManager.GetShieldCost(shield);
                    label = $"$ {refillCost}";
                }
                break;
        }

        priceText.text = label;
        
        // Color logic
        bool affordable = true;
        if (type == "Gun")
        {
            var gun = target as PlayerShoot;
            var upgradeState = gun.GetComponent<GunUpgradeState>();
            bool prioritizeUpgrade = canUpgrade && (!canRefill || upgradeCost <= refillCost * 2);
            affordable = prioritizeUpgrade ? CoinManager.Instance.HasEnoughCoins(upgradeCost) : CoinManager.Instance.HasEnoughCoins(refillCost);
        }
        else if (type == "Melee")
        {
            affordable = canUpgrade && CoinManager.Instance.HasEnoughCoins(upgradeCost);
        }
        else
        {
            affordable = canRefill && CoinManager.Instance.HasEnoughCoins(refillCost);
        }

        priceText.color = affordable ? new Color(0.392f, 0.698f, 0.812f) : Color.red;
        priceText.gameObject.SetActive(true);
    }

    // ==============================
    // GENERIC BUTTON STATE
    // ==============================
    private void UpdateButtonState(string type, object target)
    {
        if (descriptionUI == null) return;

        switch (type)
        {
            case "Gun":
                var gun = target as PlayerShoot;
                var upgradeState = gun.GetComponent<GunUpgradeState>();
                
                UpdateGunRefillButton(gun);
                UpdateGunUpgradeButton(gun, upgradeState);
                break;

            case "Melee":
                var melee = target as MeleeWeapon;
                UpdateMeleeUpgradeButton(melee);
                break;

            case "Health":
                var health = target as PlayerHealthSystem;
                UpdateHealthButton(health);
                break;

            case "Shield":
                var shield = target as PlayerHealthSystem;
                UpdateShieldButton(shield);
                break;
        }
    }

    private void UpdateGunRefillButton(PlayerShoot gun)
    {
        Button refillButton = descriptionUI.RefillButton;
        if (refillButton == null) return;

        TMP_Text buttonText = refillButton.GetComponentInChildren<TMP_Text>();
        bool needsAmmo = shopManager.NeedsRefill(gun);
        
        if (!needsAmmo)
        {
            if (buttonText) buttonText.text = "Full Ammo";
            refillButton.interactable = false;
        }
        else
        {
            int cost = shopManager.GetRefillCost(gun);
            if (buttonText) buttonText.text = $"Refill (${cost})";
            refillButton.interactable = CoinManager.Instance.HasEnoughCoins(cost);
        }
    }

    private void UpdateGunUpgradeButton(PlayerShoot gun, GunUpgradeState upgradeState)
    {
        Button upgradeButton = descriptionUI.UpgradeButton;
        if (upgradeButton == null) return;

        TMP_Text buttonText = upgradeButton.GetComponentInChildren<TMP_Text>();
        
        if (upgradeState == null || !shopManager.CanUpgradeGun(upgradeState))
        {
            if (buttonText) buttonText.text = "Max Level";
            upgradeButton.interactable = false;
        }
        else
        {
            int cost = shopManager.GetGunUpgradeCost(upgradeState);
            if (buttonText) buttonText.text = $"Upgrade (${cost})";
            upgradeButton.interactable = CoinManager.Instance.HasEnoughCoins(cost);
        }
    }

    private void UpdateMeleeUpgradeButton(MeleeWeapon melee)
    {
        Button button = descriptionUI.UpgradeButton;
        if (button == null) return;

        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        
        if (!shopManager.CanUpgrade(melee.level, melee.data.maxLevel))
        {
            if (buttonText) buttonText.text = "Max Level";
            button.interactable = false;
        }
        else
        {
            int cost = shopManager.GetUpgradeCost(melee.level, melee.data.maxLevel);
            if (buttonText) buttonText.text = $"Upgrade (${cost})";
            button.interactable = CoinManager.Instance.HasEnoughCoins(cost);
        }
    }

    private void UpdateHealthButton(PlayerHealthSystem health)
    {
        Button button = descriptionUI.RefillButton;
        if (button == null) return;

        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        
        if (!shopManager.NeedsHeal(health))
        {
            if (buttonText) buttonText.text = "Full Health";
            button.interactable = false;
        }
        else
        {
            int cost = shopManager.GetHealCost(health);
            if (buttonText) buttonText.text = $"Heal (${cost})";
            button.interactable = CoinManager.Instance.HasEnoughCoins(cost);
        }
    }

    private void UpdateShieldButton(PlayerHealthSystem shield)
    {
        Button button = descriptionUI.RefillButton;
        if (button == null) return;

        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        
        if (!shopManager.NeedsShield(shield))
        {
            if (buttonText) buttonText.text = "Full Shield";
            button.interactable = false;
        }
        else
        {
            int cost = shopManager.GetShieldCost(shield);
            if (buttonText) buttonText.text = $"Shield (${cost})";
            button.interactable = CoinManager.Instance.HasEnoughCoins(cost);
        }
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
                var upgradeState = gun.GetComponent<GunUpgradeState>();
                int gunLevel = upgradeState?.level ?? 0;
                
                descriptionUI.UpdateDescriptionUI(gun: gun.gunData, gunLevel: gunLevel);
                
                descriptionUI.RefillButton?.onClick.AddListener(() => HandleRefill(gun));
                descriptionUI.UpgradeButton?.onClick.AddListener(() => HandleGunUpgrade(gun, upgradeState));

                // Add hover preview for gun upgrade (only if can upgrade)
                if (descriptionUI.UpgradeButton != null && upgradeState != null && shopManager.CanUpgradeGun(upgradeState))
                {
                    EventTrigger trigger = descriptionUI.UpgradeButton.gameObject.GetComponent<EventTrigger>();
                    if (trigger == null) trigger = descriptionUI.UpgradeButton.gameObject.AddComponent<EventTrigger>();
                    trigger.triggers.Clear();

                    var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
                    enter.callback.AddListener((_) => ShowGunPreview(gun, upgradeState));
                    trigger.triggers.Add(enter);

                    var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
                    exit.callback.AddListener((_) => HidePreview());
                    trigger.triggers.Add(exit);
                }

                UpdateButtonState("Gun", gun);
                break;

            case "Melee" when currentItem is MeleeWeapon melee:
                descriptionUI.UpdateDescriptionUI(melee: melee.data, meleeLevel: itemLevel);
                descriptionUI.UpgradeButton?.onClick.AddListener(() => HandleMeleeUpgrade(melee));

                // Add hover preview for melee upgrade (only if can upgrade)
                if (descriptionUI.UpgradeButton != null && shopManager.CanUpgrade(melee.level, melee.data.maxLevel))
                {
                    EventTrigger trigger = descriptionUI.UpgradeButton.gameObject.GetComponent<EventTrigger>();
                    if (trigger == null) trigger = descriptionUI.UpgradeButton.gameObject.AddComponent<EventTrigger>();
                    trigger.triggers.Clear();

                    var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
                    enter.callback.AddListener((_) => ShowMeleePreview(melee));
                    trigger.triggers.Add(enter);

                    var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
                    exit.callback.AddListener((_) => HidePreview());
                    trigger.triggers.Add(exit);
                }

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

            default: 
                descriptionUI?.HideDescription(); 
                break;
        }
    }

    // ==============================
    // ACTIONS
    // ==============================
    private void HandleGunUpgrade(PlayerShoot gun, GunUpgradeState upgradeState)
    {
        if (upgradeState != null && shopManager.UpgradeGun(upgradeState))
        {
            itemLevel = upgradeState.level;
            
            // Update slot display
            UpdateSlot(gun, "Gun", upgradeState.level, gun.currentAmmo, gun.reserveAmmo);
            
            // Update description with new stats
            descriptionUI.UpdateDescriptionUI(gun: gun.gunData, gunLevel: upgradeState.level);
            UpdateButtonState("Gun", gun);

            // Update upgrade bar
            if (shopUpgradeBarUI != null)
            {
                shopUpgradeBarUI.UpdateLevel(upgradeState.level);
                shopUpgradeBarUI.PlayAnimation(upgradeState.level - 1);
            }
        }
    }

    private void HandleMeleeUpgrade(MeleeWeapon melee)
    {
        if (shopManager.UpgradeMelee(melee))
        {
            itemLevel = melee.level;
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
            UpdateSlot(gun, "Gun", itemLevel, gun.currentAmmo, gun.reserveAmmo);
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
    // PREVIEW METHODS
    // ==============================
    private void ShowGunPreview(PlayerShoot gun, GunUpgradeState upgradeState)
    {
        // Double-check that we can actually upgrade
        if (upgradeState == null || !shopManager.CanUpgradeGun(upgradeState)) return;

        int nextLevel = upgradeState.level + 1;
        var gunData = gun.gunData;

        // Find properties in description panel and show preview
        var properties = descriptionUI.GetComponentsInChildren<PropertyUI>();
        
        foreach (var prop in properties)
        {
            // Match by name or component order - adjust based on your UI setup
            if (prop.name.Contains("Damage") || prop.transform.GetSiblingIndex() == 0)
            {
                prop.SetPreview(gunData.GetDamage(nextLevel), 100f);
            }
            else if (prop.name.Contains("Range") || prop.transform.GetSiblingIndex() == 1)
            {
                prop.SetPreview(gunData.GetRange(nextLevel), 100f);
            }
            else if (prop.name.Contains("MagSize") || prop.transform.GetSiblingIndex() == 2)
            {
                prop.SetPreview(gunData.GetMagazineSize(nextLevel), 100f);
            }
            else if (prop.name.Contains("Speed") || prop.transform.GetSiblingIndex() == 3)
            {
                prop.SetPreview(gunData.GetRoundsPerSecond(nextLevel), 20f, "0.0");
            }
            else if (prop.name.Contains("Reload") || prop.transform.GetSiblingIndex() == 4)
            {
                prop.SetPreview(gunData.GetReloadTime(nextLevel), 10f, "0.0");
            }
        }
    }

    private void ShowMeleePreview(MeleeWeapon melee)
    {
        // Double-check that we can actually upgrade
        if (melee == null || !shopManager.CanUpgrade(melee.level, melee.data.maxLevel)) return;

        int nextLevel = melee.level + 1;
        var properties = descriptionUI.GetComponentsInChildren<PropertyUI>();

        foreach (var prop in properties)
        {
            if (prop.name.Contains("Damage") || prop.transform.GetSiblingIndex() == 0)
            {
                prop.SetPreview(melee.data.GetDamage(nextLevel), 100f);
            }
            else if (prop.name.Contains("Range") || prop.transform.GetSiblingIndex() == 1)
            {
                prop.SetPreview(melee.data.GetRange(nextLevel), 10f);
            }
            else if (prop.name.Contains("Speed") || prop.transform.GetSiblingIndex() == 3)
            {
                float nextSpeed = 1f / melee.data.GetCooldown(nextLevel);
                prop.SetPreview(nextSpeed, 10f, "0.0");
            }
        }
    }

    private void HidePreview()
    {
        foreach (var prop in descriptionUI.GetComponentsInChildren<PropertyUI>())
        {
            prop.HidePreview();
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
                var upgradeState = gun.GetComponent<GunUpgradeState>();
                int level = upgradeState?.level ?? 0;
                UpdateSlot(gun, "Gun", level, gun.currentAmmo, gun.reserveAmmo);
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
                case "Gun" when currentItem is PlayerShoot gun:
                    var upgradeState = gun.GetComponent<GunUpgradeState>();
                    // Prioritize upgrade if available and affordable
                    if (upgradeState != null && shopManager.CanUpgradeGun(upgradeState) && 
                        CoinManager.Instance.HasEnoughCoins(shopManager.GetGunUpgradeCost(upgradeState)))
                    {
                        HandleGunUpgrade(gun, upgradeState);
                    }
                    else
                    {
                        HandleRefill(gun);
                    }
                    break;
                    
                case "Melee" when currentItem is MeleeWeapon melee: 
                    HandleMeleeUpgrade(melee); 
                    break;
                    
                case "Health": 
                    HandleStatRestore(currentItem as PlayerHealthSystem, "Health"); 
                    break;
                    
                case "Shield": 
                    HandleStatRestore(currentItem as PlayerHealthSystem, "Shield"); 
                    break;
            }
        }
        lastClickTime = Time.time;
    }
}