using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopEquipDescriptionsUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText, typeText, levelText;
    [SerializeField] private Image avatarImage;

    [Header("Properties")]
    [SerializeField] private PropertyUI[] properties; // [damage, range, magSize, speed, reload, reserve]

    [Header("Action Buttons")]
    [SerializeField] private Button refillButton, upgradeButton;

    [Header("Icons")]
    [SerializeField] private Sprite healthIcon, shieldIcon;

    public Button RefillButton => refillButton;
    public Button UpgradeButton => upgradeButton;

    private void Start() => HideDescription();

    public void UpdateDescriptionUI(GunData gun = null, int gunLevel = 0, MeleeData melee = null, int meleeLevel = 0,
                                    PlayerHealthSystem health = null, PlayerHealthSystem shield = null)
    {
        if (IsAllNull(gun, melee, health, shield))
        {
            HideDescription();
            return;
        }

        ShowDescription();

        if (gun != null) SetupGun(gun, gunLevel);
        else if (melee != null) SetupMelee(melee, meleeLevel);
        else if (health != null) SetupHealth();
        else if (shield != null) SetupShield();
    }

    private bool IsAllNull(params object[] objects)
    {
        foreach (var obj in objects)
            if (obj != null) return false;
        return true;
    }

    private void SetupGun(GunData gun, int level)
    {
        SetBasicInfo(gun.gunName, gun.gunType.ToString(), gun.gunSpriteFullColor);

        // Show level for guns
        levelText.text = $"Level: {level + 1}"; // Display level as 1-based
        levelText.gameObject.SetActive(true);

        // Use level-adjusted stats
        properties[0].SetValue(gun.GetDamage(level), 100f);                    // damage
        properties[1].SetValue(gun.GetRange(level), 100f);                     // range
        properties[2].SetValue(gun.GetMagazineSize(level), 100f);              // magSize
        properties[3].SetValue(gun.GetRoundsPerSecond(level), 20f, "0.0");     // speed
        properties[4].SetValue(gun.GetReloadTime(level), 10f, "0.0");          // reload
        properties[5].SetValue(gun.reserveAmmo);                               // reserve (unchanged)

        SetPropertiesActive(true, true, true, true, true, true);
        SetButtonsActive(true, true); // Both refill and upgrade available for guns
    }

    private void SetupMelee(MeleeData melee, int level)
    {
        SetBasicInfo(melee.weaponName, "Melee", melee.weaponSpriteFullColor);

        levelText.text = $"Level: {level + 1}"; // Display level as 1-based
        levelText.gameObject.SetActive(true);

        properties[0].SetValue(melee.GetDamage(level), 100f);
        properties[1].SetValue(melee.GetRange(level), 10f);
        properties[3].SetValue(1f / melee.GetCooldown(level), 10f, "0.0");

        SetPropertiesActive(true, true, false, true, false, false);
        SetButtonsActive(false, true); // Only upgrade available for melee
    }

    private void SetupHealth()
    {
        levelText.gameObject.SetActive(false);

        SetBasicInfo("Medical Kit", "Health", healthIcon);
        SetPropertiesActive(false, false, false, false, false, false);
        SetButtonsActive(true, false); // Only refill available for health
    }

    private void SetupShield()
    {
        levelText.gameObject.SetActive(false);

        SetBasicInfo("Shield Kit", "Shield", shieldIcon);
        SetPropertiesActive(false, false, false, false, false, false);
        SetButtonsActive(true, false); // Only refill available for shield
    }

    private void SetBasicInfo(string name, string type, Sprite avatar)
    {
        nameText.text = name;
        typeText.text = type;
        avatarImage.sprite = avatar;
    }

    private void SetPropertiesActive(bool damage, bool range, bool mag, bool speed, bool reload, bool reserve)
    {
        bool[] states = { damage, range, mag, speed, reload, reserve };
        for (int i = 0; i < properties.Length && i < states.Length; i++)
            properties[i].gameObject.SetActive(states[i]);
    }

    private void SetButtonsActive(bool refill, bool upgrade)
    {
        SetButtonState(refillButton, refill);
        SetButtonState(upgradeButton, upgrade);
    }

    private void SetButtonState(Button button, bool active)
    {
        if (button == null) return;
        button.gameObject.SetActive(active);
        if (active) button.interactable = true;
        else button.onClick.RemoveAllListeners();
    }

    private void ShowDescription()
    {
        avatarImage.gameObject.SetActive(true);
        // Properties will be shown/hidden individually based on item type
    }

    public void HideDescription()
    {
        nameText.text = "Name";
        typeText.text = "Type";
        levelText.text = "";

        avatarImage.gameObject.SetActive(false);
        levelText.gameObject.SetActive(false);
        
        foreach (var prop in properties)
            prop.gameObject.SetActive(false);

        SetButtonsActive(false, false);
    }

    public void ClearButtonListeners()
    {
        refillButton?.onClick.RemoveAllListeners();
        upgradeButton?.onClick.RemoveAllListeners();
    }
}