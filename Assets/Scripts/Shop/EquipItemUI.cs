using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipItemUI : MonoBehaviour, ISelectHandler, IPointerClickHandler
{
    private static EquipItemUI currentSelected;

    [Header("UI")]
    public Transform upgradeBarParent;
    public Image avatar;
    public TMP_Text ammo, priceText;
    public Image sliderBar;

    // Cache
    private object currentItem;
    private string itemType;
    private int meleeLevel;
    private float lastClickTime;

    // References
    private EquipDescriptionsUI descriptionUI;
    private ShopManager shopManager;
    private Animator animator;

    private void Awake()
    {
        descriptionUI = FindAnyObjectByType<EquipDescriptionsUI>();
        shopManager = FindAnyObjectByType<ShopManager>();
        animator = GetComponent<Animator>();
    }

    public void UpdateSlot(object item, string type, int level = 0, int currentAmmo = 0, int reserveAmmo = 0)
    {
        currentItem = item;
        itemType = type;
        meleeLevel = level;

        if (item == null)
        {
            ClearUI();
            return;
        }

        switch (type)
        {
            case "Gun":
                SetupGunSlot(item as PlayerShoot, currentAmmo, reserveAmmo);
                break;
            case "Melee":
                SetupMeleeSlot(item as MeleeWeapon, level);
                break;
            case "Health":
                SetupStatSlot(item as PlayerHealth, type);
                break;
            case "Shield":
                SetupStatSlot(item as PlayerShield, type);
                break;
        }
    }

    private void SetupGunSlot(PlayerShoot gun, int currentAmmo, int reserveAmmo)
    {
        avatar.sprite = gun.gunData.gunSprite;
        ammo.text = $"{currentAmmo}/{reserveAmmo}";
        ammo.gameObject.SetActive(true);
        UpdatePrice(gun);
    }

    private void SetupMeleeSlot(MeleeWeapon melee, int level)
    {
        avatar.sprite = melee.data.weaponSprite;
        ammo.gameObject.SetActive(false);
        priceText?.gameObject.SetActive(false);
    }

    private void SetupStatSlot<T>(T stat, string type) where T : class
    {
        float current = 0f, max = 1f;

        switch (type)
        {
            case "Health" when stat is PlayerHealth health:
                current = health.currentHealth;
                max = health.maxHealth;
                UpdateStatPrice(health, shopManager.GetHealCost(health), shopManager.NeedsHeal(health), shopManager.healCostPerHP);
                break;
            case "Shield" when stat is PlayerShield shield:
                current = shield.currentShield;
                max = shield.maxShield;
                UpdateStatPrice(shield, shopManager.GetShieldCost(shield), shopManager.NeedsShield(shield), shopManager.shieldCostPerPoint);
                break;
        }

        ammo.text = $"{(int)current}/{(int)max}";
        if (sliderBar) sliderBar.fillAmount = current / max;
        ammo.gameObject.SetActive(true);
    }

    private void UpdatePrice(PlayerShoot gun)
    {
        if (priceText == null || shopManager == null) return;

        bool needsRefill = shopManager.NeedsRefill(gun);
        if (!needsRefill)
        {
            priceText.text = "Full Ammo";
            priceText.color = new Color(0.392f, 0.698f, 0.812f);
        }
        else
        {
            int cost = shopManager.GetRefillCost(gun);
            int available = CoinManager.Instance.GetCoins();
            
            if (available < cost && available > 0)
            {
                int maxBullets = available / gun.gunData.bulletRefillCost;
                cost = maxBullets * gun.gunData.bulletRefillCost;
            }

            priceText.text = $"$ {cost}";
            priceText.color = CoinManager.Instance.HasEnoughCoins(gun.gunData.bulletRefillCost) 
                ? new Color(0.392f, 0.698f, 0.812f) : Color.red;
        }
        
        priceText.gameObject.SetActive(true);
    }

    private void UpdateStatPrice<T>(T stat, int fullCost, bool needs, int costPerUnit)
    {
        if (priceText == null) return;

        if (!needs)
        {
            priceText.text = itemType == "Health" ? "Full Health" : "Full Shield";
            priceText.color = new Color(0.392f, 0.698f, 0.812f);
        }
        else
        {
            int available = CoinManager.Instance.GetCoins();
            int cost = available < fullCost && available >= costPerUnit 
                ? (available / costPerUnit) * costPerUnit 
                : fullCost;

            priceText.text = $"$ {cost}";
            priceText.color = available >= costPerUnit 
                ? new Color(0.392f, 0.698f, 0.812f) : Color.red;
        }
        
        priceText.gameObject.SetActive(true);
    }

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
                SetupGunButtons(gun);
                break;
            case "Melee" when currentItem is MeleeWeapon melee:
                descriptionUI.UpdateDescriptionUI(melee: melee.data, meleeLevel: meleeLevel);
                SetupMeleeButtons();
                break;
            case "Health" when currentItem is PlayerHealth health:
                descriptionUI.UpdateDescriptionUI(health: health);
                SetupStatButtons(health, "Health");
                break;
            case "Shield" when currentItem is PlayerShield shield:
                descriptionUI.UpdateDescriptionUI(shield: shield);
                SetupStatButtons(shield, "Shield");
                break;
            default:
                descriptionUI?.HideDescription();
                break;
        }
    }

    private void SetupGunButtons(PlayerShoot gun)
    {
        descriptionUI.RefillButton?.onClick.AddListener(() => HandleRefill(gun));
        descriptionUI.UpgradeButton?.onClick.AddListener(() => Debug.Log($"Upgrading {gun.GetType().Name}"));
        
        UpdateGunButtonState(gun);
    }

    private void SetupMeleeButtons()
    {
        descriptionUI.UpgradeButton?.onClick.AddListener(() => Debug.Log($"Upgrading Melee"));
    }

    private void SetupStatButtons<T>(T stat, string type)
    {
        descriptionUI.RefillButton?.onClick.AddListener(() => HandleStatRestore(stat, type));
        UpdateStatButtonState(stat, type);
    }

    private void UpdateGunButtonState(PlayerShoot gun)
    {
        if (descriptionUI.RefillButton == null) return;

        bool needsRefill = shopManager.NeedsRefill(gun);
        bool canAfford = CoinManager.Instance.GetCoins() >= gun.gunData.bulletRefillCost;
        int fullCost = shopManager.GetRefillCost(gun);
        int availableCoins = CoinManager.Instance.GetCoins();

        var buttonText = descriptionUI.RefillButton.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            if (!needsRefill)
            {
                buttonText.text = "Full Ammo";
            }
            else if (availableCoins < fullCost && canAfford)
            {
                int maxBullets = availableCoins / gun.gunData.bulletRefillCost;
                int partialCost = maxBullets * gun.gunData.bulletRefillCost;
                buttonText.text = $"Refill ({partialCost})";
            }
            else
            {
                buttonText.text = $"Refill ({fullCost})";
            }
        }
        
        descriptionUI.RefillButton.interactable = needsRefill && canAfford;
    }

    private void UpdateStatButtonState<T>(T stat, string type)
    {
        if (descriptionUI.RefillButton == null) return;

        bool needs = type == "Health" ? shopManager.NeedsHeal(stat as PlayerHealth) 
                                     : shopManager.NeedsShield(stat as PlayerShield);
        int costPerUnit = type == "Health" ? shopManager.healCostPerHP : shopManager.shieldCostPerPoint;
        int fullCost = type == "Health" ? shopManager.GetHealCost(stat as PlayerHealth) 
                                       : shopManager.GetShieldCost(stat as PlayerShield);
        int availableCoins = CoinManager.Instance.GetCoins();
        bool canAfford = availableCoins >= costPerUnit;

        var buttonText = descriptionUI.RefillButton.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            if (!needs)
            {
                buttonText.text = $"Full {type}";
            }
            else if (availableCoins < fullCost && canAfford)
            {
                int maxAffordable = availableCoins / costPerUnit;
                int partialCost = maxAffordable * costPerUnit;
                buttonText.text = $"{type} ({partialCost})";
            }
            else
            {
                buttonText.text = $"{type} ({fullCost})";
            }
        }
        
        descriptionUI.RefillButton.interactable = needs && canAfford;
    }

    private void HandleRefill(PlayerShoot gun)
    {
        if (shopManager.RefillAmmo(gun))
        {
            // Update slot UI immediately
            UpdateSlot(gun, "Gun", 0, gun.currentAmmo, gun.reserveAmmo);
            
            // Update button state immediately
            UpdateGunButtonState(gun);
            
            // Refresh price display
            UpdatePrice(gun);
        }
    }

    private void HandleStatRestore<T>(T stat, string type)
    {
        bool success = type == "Health" ? shopManager.HealPlayer(stat as PlayerHealth)
                                       : shopManager.ShieldPlayer(stat as PlayerShield);
        if (success)
        {
            // Update slot UI immediately
            UpdateSlot(stat, type);
            
            // Update button state immediately
            UpdateStatButtonState(stat, type);
            
            // Refresh price display
            if (type == "Health")
                UpdateStatPrice(stat as PlayerHealth, shopManager.GetHealCost(stat as PlayerHealth), 
                              shopManager.NeedsHeal(stat as PlayerHealth), shopManager.healCostPerHP);
            else
                UpdateStatPrice(stat as PlayerShield, shopManager.GetShieldCost(stat as PlayerShield), 
                              shopManager.NeedsShield(stat as PlayerShield), shopManager.shieldCostPerPoint);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ApplySelection();
        
        if (Time.time - lastClickTime < 0.3f) // Double click
        {
            switch (itemType)
            {
                case "Gun" when currentItem is PlayerShoot gun:
                    HandleRefill(gun);
                    break;
                case "Health":
                case "Shield":
                    HandleStatRestore(currentItem, itemType);
                    break;
            }
        }
        lastClickTime = Time.time;
    }

    public void RefreshUI()
    {
        switch (itemType)
        {
            case "Gun" when currentItem is PlayerShoot gun:
                UpdateSlot(gun, "Gun", 0, gun.currentAmmo, gun.reserveAmmo);
                if (currentSelected == this) UpdateGunButtonState(gun);
                break;
            case "Health" when currentItem is PlayerHealth health:
                UpdateSlot(health, "Health");
                if (currentSelected == this) UpdateStatButtonState(health, "Health");
                break;
            case "Shield" when currentItem is PlayerShield shield:
                UpdateSlot(shield, "Shield");
                if (currentSelected == this) UpdateStatButtonState(shield, "Shield");
                break;
        }
    }

    private void ClearUI()
    {
        currentItem = null;
        itemType = null;
        avatar.sprite = null;
        ammo.text = "";
        priceText?.gameObject.SetActive(false);
    }
}
