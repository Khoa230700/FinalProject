using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class WeaponSwitcher : MonoBehaviour
{
    public WeaponUI weaponUI;
    [SerializeField] private List<GameObject> weaponList = new List<GameObject>();

    private int currentWeaponIndex = 0;

    void Start()
    {
        ActivateWeapon(currentWeaponIndex);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchWeapon(2);
    }

    void SwitchWeapon(int index)
    {
        if (index >= weaponList.Count || index == currentWeaponIndex) return;

        for (int i = 0; i < weaponList.Count; i++)
        {
            weaponList[i].SetActive(i == index);
        }

        currentWeaponIndex = index;

        UpdateWeaponUI(index);
    }

    void ActivateWeapon(int index)
    {
        for (int i = 0; i < weaponList.Count; i++)
        {
            weaponList[i].SetActive(i == index);
        }

        UpdateWeaponUI(index);
    }

    private void UpdateWeaponUI(int index)
    {
        var activateWeapon = weaponList[index];
        var playerShoot = activateWeapon.GetComponentInChildren<PlayerShoot>();

        if (playerShoot)
        {
            playerShoot.weaponUI = weaponUI;
            weaponUI.gunData = playerShoot.gunData;

            weaponUI.SetWeaponSprite(playerShoot.gunData.gunSprite);
            weaponUI.SetFireMode(playerShoot.gunData.fireMode);

            weaponUI.CreateBulletUI();
            DelayedAmmoUpdate(playerShoot);
        }
    }

    private IEnumerator DelayedAmmoUpdate(PlayerShoot playerShoot)
    {
        yield return null; // chờ 1 frame
        weaponUI.UpdateAmmoUI(playerShoot.currentAmmo, playerShoot.gunData.reserveAmmo);
    }
}

