// GunFireHandler.cs
using UnityEngine;
using System;
using System.Collections;

public class GunFireHandler : MonoBehaviour
{
    [Header("References")]
    public PlayerShoot playerShoot;

    private GunData gunData;
    private int modeIndex;
    private float nextAutoTime;
    private float nextBurstTime;
    private Coroutine burstRoutine;

    void Start()
    {
        // Lấy data từ PlayerShoot
        gunData = playerShoot.gunData;
        modeIndex = Array.IndexOf(gunData.availableFireModes, gunData.fireMode);
    }

    void Update()
    {
        if (PauseGameUI.isPause) return;

        // Chuyển fire mode khi bấm B
        if (gunData.availableFireModes.Length > 1 && Input.GetKeyDown(KeyCode.B))
        {
            modeIndex = (modeIndex + 1) % gunData.availableFireModes.Length;
            gunData.fireMode = gunData.availableFireModes[modeIndex];
        }

        // Reload
        if (Input.GetKeyDown(KeyCode.R))
            playerShoot.Reload();

        // Xử lý input theo fireMode
        switch (gunData.fireMode)
        {
            case GunFireMode.SemiAuto:
                if (Input.GetMouseButtonDown(0) && playerShoot.IsReadyToShoot)
                    playerShoot.ShootOneBullet();
                break;

            case GunFireMode.FullAuto:
                if (Input.GetMouseButtonDown(0))
                    playerShoot.StartShooting();
                if (Input.GetMouseButtonUp(0))
                    playerShoot.StopShooting();

                if (Input.GetMouseButton(0) && Time.time >= nextAutoTime)
                {
                    playerShoot.ShootOneBullet();
                    nextAutoTime = Time.time + 1f / gunData.roundsPerSecond;
                }
                break;

            case GunFireMode.Burst:
                if (burstRoutine == null &&
                    Input.GetMouseButtonDown(0) &&
                    Time.time >= nextBurstTime)
                {
                    burstRoutine = StartCoroutine(BurstSequence());
                }
                break;

            case GunFireMode.Safety:
                // Không làm gì khi ở chế độ Safety
                break;
        }
    }

    private IEnumerator BurstSequence()
    {
        for (int i = 0; i < gunData.burstCount; i++)
        {
            if (playerShoot.currentAmmo <= 0) break;
            while (!playerShoot.IsReadyToShoot)
                yield return null;

            playerShoot.ShootOneBullet();

            if (i < gunData.burstCount - 1)
                yield return new WaitForSeconds(gunData.burstFireRate);
        }

        nextBurstTime = Time.time + gunData.burstCooldown;
        burstRoutine = null;
    }
}
