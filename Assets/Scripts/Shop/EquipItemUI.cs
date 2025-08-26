using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipItemUI : MonoBehaviour, ISelectHandler
{
    private static EquipItemUI currentSelected;

    [Header("UI Elements")]
    public Transform upgradeBarParent;
    public Image avatar;
    public TMP_Text ammo;
    public TMP_Text priceText;

    //Cache
    private GunData gunData = null;
    private MeleeData meleeData = null;
    private int meleeLevel;
    private IWeapon weaponRef;

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

        gunData = gun.gunData;
        weaponRef = gun;

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

        meleeData = melee.data;
        meleeLevel = level;
        weaponRef = melee;

        avatar.sprite = meleeData.weaponSprite;
        ammo.gameObject.SetActive(false);

        // Hide refill price for melee weapons
        if (priceText != null)
            priceText.gameObject.SetActive(false);
    }

    private void UpdateRefillPrice(PlayerShoot gun)
    {
        if (priceText == null || shopManager == null) return;

        if (gun.NeedsRefill())
        {
            int refillCost = shopManager.GetRefillCost(gun);
            priceText.text = $"$ {refillCost}";
            priceText.gameObject.SetActive(true);

            // Change color based on affordability
            bool canAfford = CoinManager.Instance.HasEnoughCoins(refillCost);
            priceText.color = canAfford ? new Color(0.392f, 0.698f, 0.812f) : Color.red;
        }
        else
        {
            priceText.text = "Full Ammo";
            priceText.color = new Color(0.392f, 0.698f, 0.812f);
            priceText.gameObject.SetActive(true);
        }
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

        if (gunData != null)
        {
            UpdateGunDescription();
        }
        else if (meleeData != null)
        {
            UpdateMeleeDescription();
        }
        else
        {
            equipDescriptionsUI.HideDescription();
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
                bool needsRefill = gun.NeedsRefill();
                int refillCost = shopManager.GetRefillCost(gun);
                bool canAfford = CoinManager.Instance.HasEnoughCoins(refillCost);

                // Update button text
                TMP_Text buttonText = equipDescriptionsUI.RefillButton.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    if (!needsRefill)
                        buttonText.text = "Full Ammo";
                    else
                        buttonText.text = $"Refill ({refillCost})";
                }

                // Update button interactability
                equipDescriptionsUI.RefillButton.interactable = needsRefill && canAfford;
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

    private void UpgradeClicked()
    {
        Debug.Log($"Upgrading {weaponRef.GetType().Name}");
    }

    private void ClearUI()
    {
        gunData = null;
        meleeData = null;
        weaponRef = null;

        if (avatar != null)
            avatar.sprite = null;
        if (ammo != null)
            ammo.text = "";
        if (priceText != null)
            priceText.gameObject.SetActive(false);
    }
}
