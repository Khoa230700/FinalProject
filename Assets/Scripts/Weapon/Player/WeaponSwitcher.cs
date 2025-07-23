using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class WeaponSwitcher : MonoBehaviour
{
    public WeaponUI weaponUI;
    [SerializeField] private CrosshairManager crosshairManager;
    [SerializeField] private List<GameObject> weaponList = new List<GameObject>();

    private List<PlayerShoot> playerShoots = new List<PlayerShoot>();
    private int currentWeaponIndex = 0;

    private bool isSwitching = false;   // <--- lock input khi switch

    void Awake()
    {
        foreach (var weapon in weaponList)
        {
            playerShoots.Add(weapon.GetComponentInChildren<PlayerShoot>());
        }

        crosshairManager ??= FindAnyObjectByType<CrosshairManager>();

        ActivateWeapon(currentWeaponIndex);
    }

    void Update()
    {
        if (isSwitching) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) StartCoroutine(SwitchWeaponRoutine(0));
        if (Input.GetKeyDown(KeyCode.Alpha2)) StartCoroutine(SwitchWeaponRoutine(1));
        if (Input.GetKeyDown(KeyCode.Alpha3)) StartCoroutine(SwitchWeaponRoutine(2));
    }

    private IEnumerator SwitchWeaponRoutine(int newIndex)
    {
        // kiểm tra hợp lệ
        if (newIndex < 0 || newIndex >= weaponList.Count
         || newIndex == currentWeaponIndex)
            yield break;

        isSwitching = true;

        var prevShoot = playerShoots[currentWeaponIndex];
        prevShoot.CancelReload();

        // 1) Chạy Hide animation của vũ khí cũ
        yield return StartCoroutine(prevShoot.SwitchOut());

        // 2) Thật sự bật/tắt GameObject
        ActivateWeapon(newIndex);
        currentWeaponIndex = newIndex;

        // 3) Update UI & crosshair (ActivateWeapon đã gọi UpdateWeaponUI rồi)
        //    nếu bạn muộn UI sau Get, thì có thể gọi UpdateWeaponUI ở đây

        // 4) Chạy Get animation của vũ khí mới
        var newShoot = playerShoots[currentWeaponIndex];
        yield return StartCoroutine(newShoot.SwitchIn());

        isSwitching = false;
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
        var playerShoot = playerShoots[index];

        playerShoot.weaponUI = weaponUI;

        weaponUI.gunData = playerShoot.gunData;
        weaponUI.SetFireMode(playerShoot.gunData.fireMode);
        weaponUI.SetWeaponSprite(playerShoot.gunData.gunSprite);
        weaponUI.CreateBulletUI();

        StartCoroutine(DelayUpdateUI(playerShoot));
        crosshairManager.SetCrosshairData(playerShoot.gunData.crosshairData);
        crosshairManager.SetPlayerShoot(playerShoot);
    }


    private IEnumerator DelayUpdateUI(PlayerShoot playerShoot)
    {
        yield return null;

        weaponUI.UpdateAmmoUI(playerShoot.currentAmmo, playerShoot.gunData.reserveAmmo);
    }
}
