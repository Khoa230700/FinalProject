using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopUI : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator shopAnimator;

    [Header("UI References")]
    [SerializeField] private GameObject canvasSetting;

    [Header("Equipment Slots")]
    [SerializeField] private EquipItemUI primaryUI;
    [SerializeField] private EquipItemUI secondaryUI;
    [SerializeField] private EquipItemUI meleeUI;
    [SerializeField] private EquipItemUI medicUI;
    [SerializeField] private EquipItemUI shieldUI;

    // State
    private bool isOpen = false;
    private Coroutine currentRoutine;

    // Cached references
    private PressKeyEvent pressKeyEvent;
    private EquipDescriptionsUI equipDescriptionsUI;
    public IWeapon[] allWeapon; //Test - should be private in production

    public bool IsOpen => isOpen;

    #region Unity Lifecycle

    private void Start()
    {
        InitializeReferences();
        CacheWeapons();
    }

    #endregion

    #region Public Methods

    public void Show()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        UpdateAllEquipmentUI();
        currentRoutine = StartCoroutine(ShowCoroutine());
        isOpen = true;
    }

    public void Hide()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(HideCoroutine());
        isOpen = false;
    }

    #endregion

    #region Private Methods

    private void InitializeReferences()
    {
        if (canvasSetting != null)
            pressKeyEvent = canvasSetting.GetComponent<PressKeyEvent>();

        equipDescriptionsUI = FindAnyObjectByType<EquipDescriptionsUI>();
    }

    private void CacheWeapons()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            allWeapon = player.GetComponentsInChildren<IWeapon>(true);
        }
    }

    private void UpdateAllEquipmentUI()
    {
        if (allWeapon == null) return;

        foreach (var weapon in allWeapon)
        {
            UpdateWeaponUI(weapon);
        }
    }

    private void UpdateWeaponUI(IWeapon weapon)
    {
        if (weapon == null) return;

        switch (weapon)
        {
            case PlayerShoot gun:
                UpdateGunUI(gun);
                break;
            case MeleeWeapon melee:
                UpdateMeleeUI(melee);
                break;
        }
    }

    private void UpdateGunUI(PlayerShoot gun)
    {
        var equipUI = GetEquipUIForGun(gun.gunData.gunSlot);
        if (equipUI != null)
        {
            equipUI.UpdateGunUI(gun.gunData, gun.currentAmmo, gun.reserveAmmo);
            equipUI.BindWeapon(gun);
        }
    }

    private void UpdateMeleeUI(MeleeWeapon melee)
    {
        if (meleeUI != null)
        {
            meleeUI.UpdateMeleeUI(melee.data, melee.level);
            meleeUI.BindWeapon(melee);
        }
    }

    private EquipItemUI GetEquipUIForGun(GunSlot gunSlot)
    {
        return gunSlot switch
        {
            GunSlot.Primary => primaryUI,
            GunSlot.Secondary => secondaryUI,
            _ => null
        };
    }

    #endregion

    #region Coroutines

    private IEnumerator ShowCoroutine()
    {
        // Disable other UI
        SetCanvasSettingActive(false);
        SetPressKeyEventEnabled(false);

        // Play animation
        shopAnimator.Play("In");
        yield return new WaitForSeconds(GetAnimationLength());

        currentRoutine = null;
    }

    private IEnumerator HideCoroutine()
    {
        // Play animation
        shopAnimator.Play("Out");
        yield return new WaitForSeconds(GetAnimationLength());

        // Re-enable other UI
        SetCanvasSettingActive(true);
        SetPressKeyEventEnabled(true);

        // Hide description panel
        if (equipDescriptionsUI != null)
            equipDescriptionsUI.HideDescription();

        currentRoutine = null;
    }

    private float GetAnimationLength()
    {
        if (shopAnimator != null)
            return shopAnimator.GetCurrentAnimatorStateInfo(0).length;
        return 0f;
    }

    private void SetCanvasSettingActive(bool active)
    {
        if (canvasSetting != null)
            canvasSetting.SetActive(active);
    }

    private void SetPressKeyEventEnabled(bool enabled)
    {
        if (pressKeyEvent != null)
            pressKeyEvent.enabled = enabled;
    }

    #endregion
}
