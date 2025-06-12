using UnityEngine;
using System.Collections.Generic;
using System;
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

    public void UpdateWeaponUI(int index)
    {
        var activateWeapon = weaponList[index];
        var playerShoot = activateWeapon.GetComponentInChildren<PlayerShoot>();

        if (playerShoot != null)
        {
            playerShoot.weaponUI = weaponUI;
            weaponUI.gunData = playerShoot.gunData;

            weaponUI.SetFireMode(playerShoot.gunData.fireMode);
            weaponUI.SetWeaponSprite(playerShoot.gunData.gunSprite);

            weaponUI.CreateBulletUI();

            StartCoroutine(DelayUpdateUI(playerShoot));
        }
    }

    private IEnumerator DelayUpdateUI(PlayerShoot playerShoot)
    {
        yield return null;

        weaponUI.UpdateAmmoUI(playerShoot.currentAmmo, playerShoot.gunData.reserveAmmo);
    }
}
