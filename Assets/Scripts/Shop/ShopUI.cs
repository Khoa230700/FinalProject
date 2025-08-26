using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private Animator shopAnimator;
    [SerializeField] private GameObject canvasSetting;

    //Slot equips
    [SerializeField] private EquipItemUI primaryUI;
    [SerializeField] private EquipItemUI secondaryUI;
    [SerializeField] private EquipItemUI meleeUI;
    [SerializeField] private EquipItemUI medicUI;
    [SerializeField] private EquipItemUI shieldUI;

    private bool isOpen = false;
    private PressKeyEvent pressKeyEvent;
    public IWeapon[] allWeapon; //Test
    private Coroutine currentRoutine;

    private void Start()
    {
        pressKeyEvent = canvasSetting.GetComponent<PressKeyEvent>();
        allWeapon = GameObject.FindWithTag("Player").GetComponentsInChildren<IWeapon>(true);

        UpdateUI();//test
    }

    public void Show()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        UpdateUI();

        // EventSystem.current.SetSelectedGameObject(primaryUI.gameObject);

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

        currentRoutine = null;
    }

    private void UpdateUI()
    {
        foreach (var weapon in allWeapon)
        {
            if (weapon is PlayerShoot gun)
            {
                switch (gun.gunData.gunSlot)
                {
                    case GunSlot.Primary:
                        primaryUI.SetFromGunData(gun.gunData, gun.currentAmmo, gun.reserveAmmo);
                        primaryUI.BindWeapon(gun);
                        break;
                    case GunSlot.Secondary:
                        secondaryUI.SetFromGunData(gun.gunData, gun.currentAmmo, gun.reserveAmmo);
                        secondaryUI.BindWeapon(gun);
                        break;
                }
            }
            else if (weapon is MeleeWeapon melee)
            {
                meleeUI.SetFromMeleeData(melee.data, melee.level);
                meleeUI.BindWeapon(melee);
            }
        }
    }


    public bool IsOpen => isOpen;
    public void Refill(IWeapon weapon)
    {
        if (weapon is PlayerShoot gun)
        {
            // TODO: kiểm tra tiền trước khi mua
            gun.Refill();
        }

        UpdateUI();
    }
}
