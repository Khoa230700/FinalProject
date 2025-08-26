using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipItemUI : MonoBehaviour, ISelectHandler
{
    [Header("UI Elements")]
    public TMP_Text costText;
    public Transform upgradeBarParent;
    public Image avatar;
    public TMP_Text ammo;

    private GunData gunData;
    private MeleeData meleeData;
    private int meleeLevel;
    private IWeapon weaponRef;

    // Cached references
    private EquipDescriptionsUI equipDescriptionsUI;

    private void Awake()
    {
        // Cache references early
        equipDescriptionsUI = FindAnyObjectByType<EquipDescriptionsUI>();
    }

    #region Public Methods

    public void UpdateGunUI(GunData gun, int currentAmmo, int reserveAmmo)
    {
        if (gun == null)
        {
            ClearUI();
            return;
        }

        gunData = gun;
        meleeData = null; // Clear melee data

        avatar.sprite = gun.gunSprite;
        ammo.text = $"{currentAmmo}/{reserveAmmo}";
        ammo.gameObject.SetActive(true);
    }

    public void UpdateMeleeUI(MeleeData melee, int level)
    {
        if (melee == null)
        {
            ClearUI();
            return;
        }

        meleeData = melee;
        meleeLevel = level;
        gunData = null; // Clear gun data

        avatar.sprite = melee.weaponSprite;
        ammo.gameObject.SetActive(false);
    }

    public void BindWeapon(IWeapon weapon)
    {
        weaponRef = weapon;
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (equipDescriptionsUI == null) return;

        // Clear previous button listeners
        equipDescriptionsUI.ClearButtonListeners();

        if (gunData != null)
        {
            SetupGunSelection();
        }
        else if (meleeData != null)
        {
            SetupMeleeSelection();
        }
        else
        {
            equipDescriptionsUI.HideDescription();
        }
    }

    #endregion

    #region Private Methods

    private void SetupGunSelection()
    {
        equipDescriptionsUI.UpdateDescriptionUI(gunData: gunData);

        // Setup refill button
        if (equipDescriptionsUI.RefillButton != null)
        {
            equipDescriptionsUI.RefillButton.onClick.AddListener(OnRefillClicked);
        }
    }

    private void SetupMeleeSelection()
    {
        equipDescriptionsUI.UpdateDescriptionUI(meleeData: meleeData, meleeLevel: meleeLevel);

        // Setup upgrade button
        if (equipDescriptionsUI.UpgradeButton != null)
        {
            equipDescriptionsUI.UpgradeButton.onClick.AddListener(OnUpgradeClicked);
        }
    }

    private bool CanAffordRefill(IWeapon weapon)
    {
        // TODO: Implement money check
        // return PlayerMoney.Instance.HasEnoughMoney(gun.gunData.refillCost);
        return true; // Temporary
    }

    public void Refill(IWeapon weapon)
    {
        if (weapon == null) return;
        if (CanAffordRefill(weapon)) return;

        if (weapon is PlayerShoot gun)
        {
            // TODO: Check money before purchasing
            gun.Refill();
            // TODO: Deduct money
            UpdateGunUI(gun.gunData, gun.currentAmmo, gun.reserveAmmo);
        }
        else
        {
            // TODO: Show insufficient funds message
            Debug.Log("Insufficient funds for refill");
        }
    }

    private void OnRefillClicked()
    {
        if (weaponRef != null)
        {
            Refill(weaponRef);
        }
    }

    private void OnUpgradeClicked()
    {
        if (weaponRef is MeleeWeapon meleeWeapon)
        {
            // TODO: Implement upgrade logic
            // Example: meleeWeapon.Upgrade();
            // Then update UI: UpdateMeleeUI(meleeWeapon.data, meleeWeapon.level);
            Debug.Log($"Upgrading {meleeWeapon.data.weaponName}");
        }
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
    }

    #endregion
}
