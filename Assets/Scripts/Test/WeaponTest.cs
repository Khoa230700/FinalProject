using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WeaponTest : MonoBehaviour
{
    [Header("Ammo")]
    public int currentAmmo;
    public int maxMagSize = 30;
    public int totalAmmo = 90;
    [Header("Settings")]
    public float fireRate = 0.2f;
    public float reloadTime = 2f;
    public GunFireMode currentFireMode = GunFireMode.SemiAuto;

    [Header("References")]
    public WeaponUI weaponUI;
    public Sprite weaponSprite;

    private float nextFireTime = 0f;
    private bool isReloading = false;
    public bool isShooting = false;

    private void Start()
    {
        currentAmmo = maxMagSize;
        weaponUI.SetFireMode(currentFireMode);
        weaponUI.SetWeaponSprite(weaponSprite);
        weaponUI.UpdateAmmoUI(currentAmmo, totalAmmo);
    }

    private void Update()
    {
        if (isReloading) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            currentFireMode = GetNextFireMode(currentFireMode);
            weaponUI.SetFireMode(currentFireMode);
        }

        if (Input.GetKeyDown(KeyCode.R))
            StartCoroutine(Reload());

        HandleShootingInput();
    }

    private GunFireMode GetNextFireMode(GunFireMode current) =>
        (GunFireMode)(((int)current + 1) % Enum.GetNames(typeof(GunFireMode)).Length);

    private void HandleShootingInput()
    {
        if (isReloading) return;

        if ((currentAmmo <= 0 && totalAmmo <= 0) || currentFireMode == GunFireMode.Safety)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (currentFireMode == GunFireMode.SemiAuto)
            {
                FireOnce();
            }
            else if (currentFireMode == GunFireMode.Burst)
            {
                StartCoroutine(BurstFire(3, 0.1f));
            }
        }

        if (currentFireMode == GunFireMode.FullAuto && Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            FireOnce();
            nextFireTime = Time.time + fireRate;
        }
    }

    private IEnumerator SetShootingState(float duration)
    {
        isShooting = true;
        yield return new WaitForSeconds(duration);
        isShooting = false;
    }

    private void FireOnce()
    {
        if (currentAmmo <= 0 || totalAmmo <= 0) return;

        currentAmmo--;
        weaponUI.UpdateAmmoUI(currentAmmo, totalAmmo);
        StartCoroutine(SetShootingState(0.1f));
    }

    private IEnumerator BurstFire(int shots, float interval)
    {
        for (int i = 0; i < shots; i++)
        {
            if (currentAmmo <= 0) break;
            FireOnce();
            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator Reload()
    {
        if (currentAmmo == maxMagSize || totalAmmo <= 0)
            yield break;

        isReloading = true;
        yield return new WaitForSeconds(reloadTime);

        int neededAmmo = maxMagSize - currentAmmo;
        int ammoToLoad = Mathf.Min(neededAmmo, totalAmmo);

        currentAmmo += ammoToLoad;
        totalAmmo -= ammoToLoad;

        weaponUI.UpdateAmmoUI(currentAmmo, totalAmmo);
        isReloading = false;
    }
}
