using System.Collections;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private GameObject canvasSetting;

    [Header("EquipItemUI Slots")]
    [SerializeField] private EquipItemUI primaryUI;
    [SerializeField] private EquipItemUI secondaryUI;
    [SerializeField] private EquipItemUI meleeUI;
    [SerializeField] private EquipItemUI medicUI;
    [SerializeField] private EquipItemUI shieldUI;

    // State
    public bool isOpen = false;
    public bool canOpen = true;
    private Coroutine currentRoutine;

    // Cached references
    private Animator shopAnimator;
    private PressKeyEvent pressKeyEvent;
    private EquipDescriptionsUI equipDescriptionsUI;
    private IWeapon[] allWeapon;
    private PlayerHealth playerHealth;
    private PlayerShield playerShield;

    private void Start()
    {
        pressKeyEvent = canvasSetting?.GetComponent<PressKeyEvent>();
        equipDescriptionsUI = FindAnyObjectByType<EquipDescriptionsUI>();
        shopAnimator = GetComponent<Animator>();

        allWeapon = GameObject.FindWithTag("Player")?.GetComponentsInChildren<IWeapon>(true);
        playerHealth = GameObject.FindWithTag("Player")?.GetComponent<PlayerHealth>();
        playerShield = GameObject.FindWithTag("Player")?.GetComponent<PlayerShield>();
    }

    public void Show()
    {
        if (isOpen || !canOpen) return;
        
        isOpen = true;

        // Update weapon UIs
        foreach (var weapon in allWeapon)
        {
            UpdateWeaponUI(weapon);
        }

        UpdateStatUI();

        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(ShowCoroutine());
    }

    public void Hide()
    {
        isOpen = false;

        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(HideCoroutine());
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
        if (gun.currentAmmo == 0 && gun.reserveAmmo == 0)
        {
            gun.Initialize();
        }

        switch (gun.gunData.gunSlot)
        {
            case GunSlot.Primary:
                primaryUI.UpdateGunSlotUI(gun, gun.currentAmmo, gun.reserveAmmo);
                break;
            case GunSlot.Secondary:
                secondaryUI.UpdateGunSlotUI(gun, gun.currentAmmo, gun.reserveAmmo);
                break;
        }
    }

    private void UpdateMeleeUI(MeleeWeapon melee)
    {
        if (meleeUI != null)
            meleeUI.UpdateMeleeSlotUI(melee, melee.level);
    }

    private void UpdateStatUI()
    {
        if (medicUI != null && playerHealth != null)
        {
            medicUI.UpdateStatSlotUI(playerHealth, "Health");
        }

        if (shieldUI != null && playerShield != null)
        {
            shieldUI.UpdateStatSlotUI(playerShield, "Shield");
        }
    }

    private IEnumerator ShowCoroutine()
    {
        canvasSetting.SetActive(false);
        pressKeyEvent.enabled = false;

        shopAnimator.Play("In");
        yield return new WaitForSeconds(shopAnimator.GetCurrentAnimatorStateInfo(0).length);

        currentRoutine = null;
    }

    private IEnumerator HideCoroutine()
    {
        shopAnimator.Play("Out");
        yield return new WaitForSeconds(shopAnimator.GetCurrentAnimatorStateInfo(0).length);

        canvasSetting.SetActive(true);
        pressKeyEvent.enabled = true;

        equipDescriptionsUI.HideDescription();

        currentRoutine = null;
    }
}
