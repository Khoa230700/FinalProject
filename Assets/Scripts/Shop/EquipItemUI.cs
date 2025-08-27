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
    private int meleeLevel;
    private IWeapon weaponRef;
    private string currentItemType;
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

    public void UpdateHealthSlotUI(PlayerHealth playerHealth)
    {
        if (playerHealth == null)
        {
            ClearUI();
            return;
        }

        currentItemType = "Health";
        playerHealthRef = playerHealth;
        weaponRef = null;
        gunData = null;
        meleeData = null;

        ammo.text = $"{(int)playerHealth.currentHealth}/{(int)playerHealth.maxHealth}";
        sliderBar.fillAmount = playerHealth.currentHealth / playerHealth.maxHealth;
        ammo.gameObject.SetActive(true);

        // Update heal price
        UpdateHealPrice(playerHealth);
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

        if (shopManager.NeedsHealing(playerHealth))
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

    private void UpdateHealthDescription()
    {
        equipDescriptionsUI.UpdateDescriptionUI(playerHealth: playerHealthRef);

        // Setup heal button (reuse RefillButton for healing)
        if (equipDescriptionsUI.RefillButton != null)
        {
            equipDescriptionsUI.RefillButton.onClick.AddListener(HealClicked);

            if (playerHealthRef != null)
            {
                bool needsHealing = shopManager.NeedsHealing(playerHealthRef);
                int healCost = shopManager.GetHealCost(playerHealthRef);
                int availableCoins = CoinManager.Instance.GetCoins();
                bool canAffordSome = availableCoins >= shopManager.healCostPerHP;

                // Update button text
                TMP_Text buttonText = equipDescriptionsUI.RefillButton.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    if (!needsHealing)
                    {
                        buttonText.text = "Full Health";
                    }
                    else if (availableCoins < healCost && canAffordSome)
                    {
                        int maxAffordableHP = availableCoins / shopManager.healCostPerHP;
                        int partialCost = maxAffordableHP * shopManager.healCostPerHP;
                        buttonText.text = $"Heal ({partialCost})";
                    }
                    else
                    {
                        buttonText.text = $"Heal ({healCost})";
                    }
                }

                // Update button interactability
                equipDescriptionsUI.RefillButton.interactable = needsHealing && canAffordSome;
            }
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

    public void HealClicked()
    {
        if (playerHealthRef == null) return;

        bool success = shopManager.HealPlayer(playerHealthRef);

        // Update UI after heal attempt
        if (success)
        {
            UpdateHealthSlotUI(playerHealthRef);

            // Refresh the selection to update button states
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
                Debug.Log("Upgrading Health/Medical item");
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
                if (playerHealthRef != null) UpdateHealthDescription();
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
                    HealClicked();
                    break;
            }
        }

        lastClickTime = Time.time;
    }
}
