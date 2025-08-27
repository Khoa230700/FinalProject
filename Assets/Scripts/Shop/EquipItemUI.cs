using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipItemUI : MonoBehaviour, ISelectHandler, IPointerClickHandler
{
    private static EquipItemUI currentSelected;

    [Header("UI Elements")]
    public Transform upgradeBarParent;
    public Image avatar;
    public TMP_Text ammo;
    public TMP_Text priceText;
    public Image sliderBar;

    //Cache
    private GunData gunData = null;
    private MeleeData meleeData = null;
    private PlayerHealth playerHealthRef = null;
    private PlayerShield playerShieldRef = null;
    private IWeapon weaponRef = null;
    private int meleeLevel = 0;
    private string currentItemType = null;
    private float lastClickTime = 0f;

    //References
    private EquipDescriptionsUI equipDescriptionsUI;
    private ShopManager shopManager;
    private Animator animator;

    private void Awake()
    {
        equipDescriptionsUI = FindAnyObjectByType<EquipDescriptionsUI>();
        shopManager = FindAnyObjectByType<ShopManager>();
        animator = GetComponent<Animator>();
    }

    public void UpdateGunSlotUI(PlayerShoot gun, int currentAmmo, int reserveAmmo)
    {
        if (gun == null)
        {
            ClearUI();
            return;
        }

        currentItemType = "Gun";
        gunData = gun.gunData;
        weaponRef = gun;
        playerHealthRef = null;

        avatar.sprite = gunData.gunSprite;
        ammo.text = $"{currentAmmo}/{reserveAmmo}";
        ammo.gameObject.SetActive(true);

        // Update refill price
        UpdateRefillPrice(gun);
    }

    public void UpdateMeleeSlotUI(MeleeWeapon melee, int level)
    {
        if (melee == null)
        {
            ClearUI();
            return;
        }

        currentItemType = "Melee";
        meleeData = melee.data;
        meleeLevel = level;
        weaponRef = melee;
        playerHealthRef = null;

        avatar.sprite = meleeData.weaponSprite;
        ammo.gameObject.SetActive(false);

        // Hide refill price for melee weapons
        if (priceText != null)
            priceText.gameObject.SetActive(false);
    }

    public void UpdateStatSlotUI(object statObject, string type)
    {
        currentItemType = type;
        weaponRef = null;
        gunData = null;
        meleeData = null;
        playerHealthRef = null;
        playerShieldRef = null;

        float current = 0f;
        float max = 1f;

        switch (type)
        {
            case "Health":
                if (statObject is PlayerHealth ph)
                {
                    playerHealthRef = ph;
                    current = ph.currentHealth;
                    max = ph.maxHealth;

                    UpdateHealPrice(ph);
                }
                break;

            case "Shield":
                if (statObject is PlayerShield ps)
                {
                    playerShieldRef = ps;
                    current = ps.currentShield;
                    max = ps.maxShield;

                    UpdateShieldPrice(ps);
                }
                break;
        }

        ammo.text = $"{(int)current}/{(int)max}";
        sliderBar.fillAmount = current / max;
        ammo.gameObject.SetActive(true);
    }

    private void UpdateRefillPrice(PlayerShoot gun)
    {
        if (priceText == null || shopManager == null) return;

        if (shopManager.NeedsRefill(gun))
        {
            int refillCost = shopManager.GetRefillCost(gun);
            int availableCoins = CoinManager.Instance.GetCoins();

            // Show partial refill cost if can't afford full
            if (availableCoins < refillCost && availableCoins > 0)
            {
                int maxAffordableBullets = availableCoins / gun.gunData.bulletRefillCost;
                int partialCost = maxAffordableBullets * gun.gunData.bulletRefillCost;
                priceText.text = $"$ {partialCost}";
            }
            else
            {
                priceText.text = $"$ {refillCost}";
            }

            priceText.gameObject.SetActive(true);

            // Change color based on affordability
            bool canAfford = CoinManager.Instance.HasEnoughCoins(gun.gunData.bulletRefillCost);
            priceText.color = canAfford ? new Color(0.392f, 0.698f, 0.812f) : Color.red;
        }
        else
        {
            priceText.text = "Full Ammo";
            priceText.color = new Color(0.392f, 0.698f, 0.812f);
            priceText.gameObject.SetActive(true);
        }
    }

    private void UpdateHealPrice(PlayerHealth playerHealth)
    {
        if (priceText == null || shopManager == null) return;

        if (shopManager.NeedsHeal(playerHealth))
        {
            int healCost = shopManager.GetHealCost(playerHealth);
            int availableCoins = CoinManager.Instance.GetCoins();

            // Show partial heal cost if can't afford full
            if (availableCoins < healCost && availableCoins >= shopManager.healCostPerHP)
            {
                int maxAffordableHP = availableCoins / shopManager.healCostPerHP;
                int partialCost = maxAffordableHP * shopManager.healCostPerHP;
                priceText.text = $"$ {partialCost}";
            }
            else
            {
                priceText.text = $"$ {healCost}";
            }

            priceText.gameObject.SetActive(true);

            // Change color based on affordability
            bool canAfford = CoinManager.Instance.HasEnoughCoins(shopManager.healCostPerHP);
            priceText.color = canAfford ? new Color(0.392f, 0.698f, 0.812f) : Color.red;
        }
        else
        {
            priceText.text = "Full Health";
            priceText.color = new Color(0.392f, 0.698f, 0.812f);
            priceText.gameObject.SetActive(true);
        }
    }
    private void UpdateShieldPrice(PlayerShield playerShield)
    {
        if (priceText == null || shopManager == null) return;

        if (shopManager.NeedsShield(playerShield))
        {
            int shieldCost = shopManager.GetShieldCost(playerShield);
            int availableCoins = CoinManager.Instance.GetCoins();

            // Show partial shield cost if can't afford full
            if (availableCoins < shieldCost && availableCoins >= shopManager.shieldCostPerPoint)
            {
                int maxAffordableHP = availableCoins / shopManager.healCostPerHP;
                int partialCost = maxAffordableHP * shopManager.healCostPerHP;
                priceText.text = $"$ {partialCost}";
            }
            else
            {
                priceText.text = $"$ {shieldCost}";
            }

            priceText.gameObject.SetActive(true);

            // Change color based on affordability
            bool canAfford = CoinManager.Instance.HasEnoughCoins(shopManager.healCostPerHP);
            priceText.color = canAfford ? new Color(0.392f, 0.698f, 0.812f) : Color.red;
        }
        else
        {
            priceText.text = "Full Shield";
            priceText.color = new Color(0.392f, 0.698f, 0.812f);
            priceText.gameObject.SetActive(true);
        }
    }

    private void UpdateGunDescription()
    {
        equipDescriptionsUI.UpdateDescriptionUI(gunData: gunData);

        // Setup refill button
        if (equipDescriptionsUI.RefillButton != null)
        {
            equipDescriptionsUI.RefillButton.onClick.AddListener(RefillClicked);

            // Update refill button text and interactability
            var gun = weaponRef as PlayerShoot;
            if (gun != null)
            {
                bool needsRefill = shopManager.NeedsRefill(gun);
                int refillCost = shopManager.GetRefillCost(gun);
                int availableCoins = CoinManager.Instance.GetCoins();
                bool canAffordSome = availableCoins >= gun.gunData.bulletRefillCost;

                // Update button text
                TMP_Text buttonText = equipDescriptionsUI.RefillButton.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    if (!needsRefill)
                    {
                        buttonText.text = "Full Ammo";
                    }
                    else if (availableCoins < refillCost && canAffordSome)
                    {
                        int maxAffordableBullets = availableCoins / gun.gunData.bulletRefillCost;
                        int partialCost = maxAffordableBullets * gun.gunData.bulletRefillCost;
                        buttonText.text = $"Refill ({partialCost})";
                    }
                    else
                    {
                        buttonText.text = $"Refill ({refillCost})";
                    }
                }

                // Update button interactability
                equipDescriptionsUI.RefillButton.interactable = needsRefill && canAffordSome;
            }
        }

        // Setup upgrade button
        if (equipDescriptionsUI.UpgradeButton != null)
        {
            equipDescriptionsUI.UpgradeButton.onClick.AddListener(UpgradeClicked);
        }
    }

    private void UpdateMeleeDescription()
    {
        equipDescriptionsUI.UpdateDescriptionUI(meleeData: meleeData, meleeLevel: meleeLevel);

        // Setup upgrade button
        if (equipDescriptionsUI.UpgradeButton != null)
        {
            equipDescriptionsUI.UpgradeButton.onClick.AddListener(UpgradeClicked);
        }
    }

    private void UpdateStatDescription<T>(T statRef, string statType)
    {
        if (statRef == null) return;

        // Cập nhật UI description (sử dụng EquipDescriptionsUI chung)
        if (statType == "Health" && statRef is PlayerHealth health)
        {
            equipDescriptionsUI.UpdateDescriptionUI(playerHealth: health);
        }
        else if (statType == "Shield" && statRef is PlayerShield shield)
        {
            equipDescriptionsUI.UpdateDescriptionUI(playerShield: shield);
        }

        // Setup Refill/Heal/Recharge button
        if (equipDescriptionsUI.RefillButton != null)
        {
            equipDescriptionsUI.RefillButton.onClick.RemoveAllListeners(); // tránh trùng listener

            equipDescriptionsUI.RefillButton.onClick.AddListener(() =>
            {
                if (statType == "Health" && statRef is PlayerHealth h)
                    shopManager.HealPlayer(h);
                else if (statType == "Shield" && statRef is PlayerShield s)
                    shopManager.ShieldPlayer(s);

                // Cập nhật lại UI sau khi dùng
                UpdateStatDescription(statRef, statType);
            });

            int cost = 0;
            bool needsRefill = false;
            int availableCoins = CoinManager.Instance.GetCoins();
            bool canAffordSome = false;

            if (statType == "Health" && statRef is PlayerHealth h2)
            {
                cost = shopManager.GetHealCost(h2);
                needsRefill = shopManager.NeedsHeal(h2);
                canAffordSome = availableCoins >= shopManager.healCostPerHP;
            }
            else if (statType == "Shield" && statRef is PlayerShield s2)
            {
                cost = shopManager.GetShieldCost(s2);
                needsRefill = shopManager.NeedsShield(s2);
                canAffordSome = availableCoins >= shopManager.shieldCostPerPoint;
            }

            // Update button text
            TMP_Text buttonText = equipDescriptionsUI.RefillButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                if (!needsRefill)
                {
                    buttonText.text = $"Full {statType}";
                }
                else if (availableCoins < cost && canAffordSome)
                {
                    int maxAffordable = (statType == "Health") ? availableCoins / shopManager.healCostPerHP
                                                                : availableCoins / shopManager.shieldCostPerPoint;
                    int partialCost = maxAffordable * ((statType == "Health") ? shopManager.healCostPerHP : shopManager.shieldCostPerPoint);
                    buttonText.text = $"{statType} ({partialCost})";
                }
                else
                {
                    buttonText.text = $"{statType} ({cost})";
                }
            }

            // Update interactable
            equipDescriptionsUI.RefillButton.interactable = needsRefill && canAffordSome;
        }
    }

    public void RefillClicked()
    {
        if (weaponRef == null) return;

        bool success = shopManager.RefillAmmo(weaponRef as PlayerShoot);

        // Update UI after refill attempt
        if (success && weaponRef is PlayerShoot gun)
        {
            UpdateGunSlotUI(gun, gun.currentAmmo, gun.reserveAmmo);

            // Refresh the selection to update button states
            ApplySelection();
        }
    }

    public void RestoreClicked()
    {
        bool success = false;

        if (playerHealthRef != null)
        {
            success = shopManager.HealPlayer(playerHealthRef);
            if (success)
                UpdateStatSlotUI(playerHealthRef, "Health");
        }
        else if (playerShieldRef != null)
        {
            success = shopManager.ShieldPlayer(playerShieldRef);
            if (success)
            {
                UpdateStatSlotUI(playerShieldRef, "Shield");
                Debug.Log("here");
            }
        }

        if (success)
        {
            // Refresh selection để cập nhật button states
            ApplySelection();
        }
    }

    private void UpgradeClicked()
    {
        switch (currentItemType)
        {
            case "Gun":
                Debug.Log($"Upgrading {weaponRef.GetType().Name}");
                break;
            case "Melee":
                Debug.Log($"Upgrading {weaponRef.GetType().Name}");
                break;
            case "Health":
            case "Shield":
                Debug.Log("Upgrading Health/Shield item");
                break;
        }
    }

    private void ClearUI()
    {
        gunData = null;
        meleeData = null;
        weaponRef = null;
        playerHealthRef = null;

        if (avatar != null)
            avatar.sprite = null;
        if (ammo != null)
            ammo.text = "";
        if (priceText != null)
            priceText.gameObject.SetActive(false);
    }

    public void OnSelect(BaseEventData eventData)
    {
        ApplySelection();
    }

    private void ApplySelection()
    {
        if (equipDescriptionsUI == null) return;

        if (currentSelected != null && currentSelected != this)
        {
            currentSelected.animator.SetBool("IsSelected", false);
        }

        currentSelected = this;
        animator.SetBool("IsSelected", true);

        equipDescriptionsUI.ClearButtonListeners();

        switch (currentItemType)
        {
            case "Gun":
                if (gunData != null) UpdateGunDescription();
                break;
            case "Melee":
                if (meleeData != null) UpdateMeleeDescription();
                break;
            case "Health":
            case "Shield":
                if (playerHealthRef != null) UpdateStatDescription(playerHealthRef, "Health");
                else if (playerShieldRef != null) UpdateStatDescription(playerShieldRef, "Shield");
                break;
            default:
                equipDescriptionsUI.HideDescription();
                break;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Time.time - lastClickTime < 0.3f)
        {
            switch (currentItemType)
            {
                case "Gun":
                    RefillClicked();
                    break;
                case "Health":
                case "Shield":
                    RestoreClicked();
                    break;
            }
        }

        lastClickTime = Time.time;
    }
}
