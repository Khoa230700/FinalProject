using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipItemUI : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public TMP_Text costText;
    public Transform upgradeBarParent;
    public Image avatar;
    public TMP_Text ammo;
    public Button refillButton;

    private GunData gunData;
    private MeleeData meleeData;
    private int meleeLevel;
    private EquipDescriptionsUI equipDescriptionsUI;
    private ShopUI shopUI;
    private IWeapon weaponRef;

    private void Start()
    {
        equipDescriptionsUI = FindAnyObjectByType<EquipDescriptionsUI>();
        shopUI = FindAnyObjectByType<ShopUI>();
    }

    public void SetFromGunData(GunData gun, int currentAmmo, int reserveAmmo)
    {
        if (gun == null) return;
        gunData = gun;

        avatar.sprite = gun.gunSprite;
        ammo.text = $"{currentAmmo}/{reserveAmmo}";
    }

    public void SetFromMeleeData(MeleeData melee, int level)
    {
        if (melee == null) return;
        meleeData = melee;
        meleeLevel = level;

        avatar.sprite = melee.weaponSprite;
        ammo.gameObject.SetActive(false);
    }

    public void BindWeapon(IWeapon weapon)
    {
        weaponRef = weapon;
    }

    private void OnRefillClicked()
    {
        if (weaponRef != null)
        {
            shopUI.Refill(weaponRef);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (gunData != null)
        {
            equipDescriptionsUI.SetGunDescriptionUI(gunData);
            refillButton.gameObject.SetActive(true);

            refillButton.onClick.RemoveAllListeners();
            refillButton.onClick.AddListener(OnRefillClicked);
        }
        else if (meleeData != null)
        {
            equipDescriptionsUI.SetMeleeDescriptionUI(meleeData, meleeLevel);
            refillButton.gameObject.SetActive(false);
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        var current = EventSystem.current.currentSelectedGameObject;

        // Nếu không chọn gì hoặc chọn object khác ngoài EquipItemUI → hủy deselect
        if (current == null || current.GetComponent<EquipItemUI>() == null)
        {
            StartCoroutine(ReselectNextFrame());
        }
    }

    private System.Collections.IEnumerator ReselectNextFrame()
    {
        yield return null; // đợi 1 frame
        EventSystem.current.SetSelectedGameObject(gameObject);
    }
}
