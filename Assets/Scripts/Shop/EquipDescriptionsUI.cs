using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipDescriptionsUI : MonoBehaviour
{
    [Header("General Info")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text typeText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Image avatarImage;

    [Header("Properties")]
    [SerializeField] private PropertyUI damageUI;
    [SerializeField] private PropertyUI rangeUI;
    [SerializeField] private PropertyUI magSizeUI;
    [SerializeField] private PropertyUI speedUI;
    [SerializeField] private PropertyUI reloadUI;
    [SerializeField] private PropertyUI reserveUI;

    [Header("Action Buttons")]
    [SerializeField] private Button refillButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Sprite healthIconSprite; // Assign health icon in inspector


    public Button RefillButton => refillButton;
    public Button UpgradeButton => upgradeButton;

    private void Start()
    {
        HideDescription();
    }

    public void UpdateDescriptionUI(GunData gunData = null, MeleeData meleeData = null, int meleeLevel = 0, PlayerHealth playerHealth = null)
    {
        if (gunData == null && meleeData == null && playerHealth == null)
        {
            HideDescription();
            return;
        }

        ShowDescription();

        if (gunData != null)
        {
            // --- Gun ---
            nameText.text = gunData.gunName;
            typeText.text = gunData.gunType.ToString();
            avatarImage.sprite = gunData.gunSpriteFullColor;

            SetGunProperties(gunData);

            ShowButton(refillButton);
            ShowButton(upgradeButton);
        }
        else if (meleeData != null)
        {
            // --- Melee ---
            nameText.text = meleeData.weaponName;
            typeText.text = "Melee";
            avatarImage.sprite = meleeData.weaponSpriteFullColor;

            SetMeleeProperties(meleeData, meleeLevel);

            HideButton(refillButton);
            ShowButton(upgradeButton);
        }
        else if (playerHealth != null)
        {
            nameText.text = "Medical Kit";
            typeText.text = "Health";
            avatarImage.sprite = healthIconSprite; // Use health icon

            SetHealthProperties();

            ShowButton(refillButton); // Will be used as "Heal" button
            HideButton(upgradeButton);
        }
    }

    private void SetGunProperties(GunData gunData)
    {
        damageUI.SetValue(gunData.damage, 100f);
        rangeUI.SetValue(gunData.range, 100f);
        magSizeUI.SetValue(gunData.magazineSize, 100f);
        speedUI.SetValue(gunData.roundsPerSecond, 20f, "0.0");
        reloadUI.SetValue(gunData.reloadTime, 10f, "0.0");
        reserveUI.SetValue(gunData.reserveAmmo);

        // Show all properties for guns
        magSizeUI.gameObject.SetActive(true);
        reloadUI.gameObject.SetActive(true);
        reserveUI.gameObject.SetActive(true);
    }

    private void SetMeleeProperties(MeleeData meleeData, int level)
    {
        damageUI.SetValue(meleeData.GetDamage(level), 100f);
        rangeUI.SetValue(meleeData.GetRange(level), 10f);
        speedUI.SetValue(1f / meleeData.GetCooldown(level), 10f, "0.0");

        // Hide irrelevant properties for melee
        magSizeUI.gameObject.SetActive(false);
        reloadUI.gameObject.SetActive(false);
        reserveUI.gameObject.SetActive(false);
    }

    private void SetHealthProperties()
    {
        // Hide all UI elements
        avatarImage.gameObject.SetActive(true);
        damageUI.gameObject.SetActive(false);
        rangeUI.gameObject.SetActive(false);
        magSizeUI.gameObject.SetActive(false);
        speedUI.gameObject.SetActive(false);
        reloadUI.gameObject.SetActive(false);
        reserveUI.gameObject.SetActive(false);
    }

    private void ShowDescription()
    {
        avatarImage.gameObject.SetActive(true);
        damageUI.gameObject.SetActive(true);
        rangeUI.gameObject.SetActive(true);
        magSizeUI.gameObject.SetActive(true);
        speedUI.gameObject.SetActive(true);
        reloadUI.gameObject.SetActive(true);
        reserveUI.gameObject.SetActive(false);
    }

    public void HideDescription()
    {
        // Reset text
        nameText.text = "Name";
        typeText.text = "Type";
        priceText.text = "";

        // Hide all UI elements
        avatarImage.gameObject.SetActive(false);
        damageUI.gameObject.SetActive(false);
        rangeUI.gameObject.SetActive(false);
        magSizeUI.gameObject.SetActive(false);
        speedUI.gameObject.SetActive(false);
        reloadUI.gameObject.SetActive(false);
        reserveUI.gameObject.SetActive(false);

        // Hide all buttons
        HideButton(refillButton);
        HideButton(upgradeButton);
    }

    private void ShowButton(Button button)
    {
        if (button != null)
        {
            button.gameObject.SetActive(true);
            button.interactable = true;
        }
    }

    private void HideButton(Button button)
    {
        if (button != null)
        {
            button.gameObject.SetActive(false);
            button.onClick.RemoveAllListeners(); // Clean up listeners
        }
    }

    public void ClearButtonListeners()
    {
        if (refillButton != null)
            refillButton.onClick.RemoveAllListeners();
        if (upgradeButton != null)
            upgradeButton.onClick.RemoveAllListeners();
    }
}
