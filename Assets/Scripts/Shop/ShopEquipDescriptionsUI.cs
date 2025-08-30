using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopEquipDescriptionsUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText, typeText, levelText;
    [SerializeField] private Image avatarImage;

    [Header("Properties")]
    [SerializeField] private ShopPropertyUI[] properties; // [damage, range, magSize, speed, reload, reserve]

    [Header("Action Buttons")]
    [SerializeField] private Button refillButton, upgradeButton;

    [Header("Icons")]
    [SerializeField] private Sprite healthIcon, shieldIcon;

    public Button RefillButton => refillButton;
    public Button UpgradeButton => upgradeButton;

    private void Start() => HideDescription();

    public void UpdateDescriptionUI(GunData gun = null, int gunLevel = 0, GunUpgradeState upgradeState = null,
                                MeleeData melee = null, int meleeLevel = 0,
                                PlayerHealthSystem health = null, PlayerHealthSystem shield = null)
    {
        if (new object[] { gun, melee, health, shield }.All(slot => slot == null))
        {
            HideDescription();
            return;
        }

        ShowDescription();

        if (gun != null) SetupGun(gun, gunLevel, upgradeState);
        else if (melee != null) SetupMelee(melee, meleeLevel);
        else if (health != null) SetupHealth();
        else if (shield != null) SetupShield();
    }

    // SETUP
    private void SetupGun(GunData gun, int level, GunUpgradeState upgradeState = null)
    {
        SetBasicInfo(gun.gunName, gun.gunType.ToString(), gun.gunSpriteFullColor);

        // Show level for guns
        levelText.text = $"Level: {level}";
        levelText.gameObject.SetActive(true);

        properties[0].SetValue(gun.GetDamage(level), upgradeState.MaxDamage); // damage 0
        properties[1].SetValue(gun.GetRange(level), upgradeState.MaxRange); // range 1
        properties[2].SetValue(gun.GetMagazineSize(level), upgradeState.MaxMagazineSize); // magSize 2
        properties[3].SetValue(gun.GetRoundsPerSecond(level), upgradeState.MaxRoundsPerSecond, "0.0"); // speed 3
        properties[4].SetValue(gun.GetReloadTime(level), upgradeState.MaxReloadTime, "0.0"); // reload 4
        properties[5].SetValue(gun.reserveAmmo); // reserve 5

        SetPropertiesActive(true, true, true, true, true, true);
        SetButtonsActive(true, true);
    }

    private void SetupMelee(MeleeData melee, int level)
    {
        SetBasicInfo(melee.weaponName, "Melee", melee.weaponSpriteFullColor);

        levelText.text = $"Level: {level}";
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
        SetButtonsActive(true, false);
    }

    private void SetupShield()
    {
        levelText.gameObject.SetActive(false);

        SetBasicInfo("Shield Kit", "Shield", shieldIcon);
        SetPropertiesActive(false, false, false, false, false, false);
        SetButtonsActive(true, false);
    }

    // PREVIEW
    public void ShowPreview(object item, string itemType)
    {
        if (itemType == "Gun" && item is PlayerShoot gun)
        {
            var upgradeState = gun.GetComponent<GunUpgradeState>();
            if (upgradeState == null || upgradeState.level >= upgradeState.maxLevel)
                return;

            int nextLevel = upgradeState.level + 1;

            foreach (var prop in properties)
            {
                if (prop.name.Contains("Damage"))
                {
                    float currentDamage = upgradeState.Damage;
                    float nextDamage = gun.gunData.GetDamage(nextLevel);
                    float maxDamage = upgradeState.MaxDamage;

                    if (Mathf.Abs(nextDamage - currentDamage) > 0.01f)
                        prop.SetPreview(nextDamage, maxDamage);
                }
                else if (prop.name.Contains("Range"))
                {
                    float currentRange = upgradeState.Range;
                    float nextRange = gun.gunData.GetRange(nextLevel);
                    float maxRange = upgradeState.MaxRange;

                    if (Mathf.Abs(nextRange - currentRange) > 0.01f)
                        prop.SetPreview(nextRange, maxRange);
                }
                else if (prop.name.Contains("MagSize"))
                {
                    int currentMagSize = upgradeState.MagazineSize;
                    int nextMagSize = gun.gunData.GetMagazineSize(nextLevel);
                    int maxMagSize = upgradeState.MaxMagazineSize;

                    if (nextMagSize != currentMagSize)
                        prop.SetPreview(nextMagSize, maxMagSize);
                }
                else if (prop.name.Contains("Speed"))
                {
                    float currentSpeed = upgradeState.RoundsPerSecond;
                    float nextSpeed = gun.gunData.GetRoundsPerSecond(nextLevel);
                    float maxSpeed = upgradeState.MaxRoundsPerSecond;

                    if (Mathf.Abs(nextSpeed - currentSpeed) > 0.01f)
                        prop.SetPreview(nextSpeed, maxSpeed, "0.0");
                }
                else if (prop.name.Contains("Reload"))
                {
                    float currentReload = upgradeState.ReloadTime;
                    float nextReload = gun.gunData.GetReloadTime(nextLevel);
                    float maxReload = upgradeState.MaxReloadTime;

                    if (Mathf.Abs(nextReload - currentReload) > 0.01f)
                        prop.SetPreview(nextReload, maxReload, "0.0");
                }
            }
        }
        else if (itemType == "Melee" && item is MeleeWeapon melee)
        {
            if (melee.level >= melee.maxLevel) return;

            int currentLevel = melee.level;
            int nextLevel = currentLevel + 1;

            foreach (var prop in properties)
            {
                if (prop.name.Contains("Damage"))
                {
                    float currentDamage = melee.data.GetDamage(currentLevel);
                    float nextDamage = melee.data.GetDamage(nextLevel);

                    if (Mathf.Abs(nextDamage - currentDamage) > 0.01f)
                        prop.SetPreview(nextDamage, 100f);
                }
                else if (prop.name.Contains("Range"))
                {
                    float currentRange = melee.data.GetRange(currentLevel);
                    float nextRange = melee.data.GetRange(nextLevel);

                    if (Mathf.Abs(nextRange - currentRange) > 0.01f)
                        prop.SetPreview(nextRange, 10f);
                }
                else if (prop.name.Contains("Speed"))
                {
                    float currentSpeed = 1f / melee.data.GetCooldown(currentLevel);
                    float nextSpeed = 1f / melee.data.GetCooldown(nextLevel);

                    if (Mathf.Abs(nextSpeed - currentSpeed) > 0.01f)
                        prop.SetPreview(nextSpeed, 10f, "0.0");
                }
            }
        }
    }

    public void HidePreview()
    {
        foreach (var prop in properties)
        {
            prop.HidePreview();
        }
    }

    // HELPER
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
        // Refill button
        refillButton.gameObject.SetActive(refill);
        if (refill)
            refillButton.interactable = true;
        else
            refillButton.onClick.RemoveAllListeners();

        //Upgrade button
        upgradeButton.gameObject.SetActive(upgrade);
        if (upgrade)
            upgradeButton.interactable = true;
        else
            upgradeButton.onClick.RemoveAllListeners();
    }

    private void ShowDescription()
    {
        avatarImage.gameObject.SetActive(true);
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