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

    private void Start()
    {
        HideDescription();
    }

    public void SetGunDescriptionUI(GunData gunData)
    {
        if (gunData == null) return;
        ResetUI();

        // Info
        nameText.text = gunData.gunName;
        typeText.text = gunData.gunType.ToString();
        // priceText.text = $"${gunData.price}"; //TODO
        avatarImage.sprite = gunData.gunSpriteFullColor;

        // Properties
        damageUI.SetValue(gunData.damage, 100f);
        rangeUI.SetValue(gunData.range, 100f);
        magSizeUI.SetValue(gunData.magazineSize, 100f);
        speedUI.SetValue(gunData.roundsPerSecond, 20f, "0.0");
        reloadUI.SetValue(gunData.reloadTime, 10f, "0.0");
    }

    public void SetMeleeDescriptionUI(MeleeData meleeData, int level)
    {
        if (meleeData == null) return;
        ResetUI();

        // Info
        nameText.text = meleeData.weaponName;
        typeText.text = "Melee";
        // priceText.text = $"${meleeData.price}"; //TODO
        avatarImage.sprite = meleeData.weaponSpriteFullColor;

        // Properties
        damageUI.SetValue(meleeData.GetDamage(level), 100f);
        rangeUI.SetValue(meleeData.GetRange(level), 10f);
        magSizeUI.gameObject.SetActive(false);
        speedUI.SetValue(1f / meleeData.GetCooldown(level), 10f, "0.0");
        reloadUI.gameObject.SetActive(false);
    }

    private void ResetUI()
    {
        avatarImage.gameObject.SetActive(true);
        damageUI.gameObject.SetActive(true);
        rangeUI.gameObject.SetActive(true);
        magSizeUI.gameObject.SetActive(true);
        speedUI.gameObject.SetActive(true);
        reloadUI.gameObject.SetActive(true);
    }

    public void HideDescription()
    {
        nameText.text = "Name";
        typeText.text = "Type";
        priceText.text = "$";

        avatarImage.gameObject.SetActive(false);
        damageUI.gameObject.SetActive(false);
        rangeUI.gameObject.SetActive(false);
        magSizeUI.gameObject.SetActive(false);
        speedUI.gameObject.SetActive(false);
        reloadUI.gameObject.SetActive(false);
    }
}
