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
    private float nextBurstTime;
    private Coroutine burstRoutine;

    void Start()
    {
        if (playerShoot == null)
        {
            Debug.LogError("[GunFireHandler] Missing PlayerShoot reference.");
            enabled = false; return;
        }

        gunData = playerShoot.gunData;
        if (gunData == null)
        {
            Debug.LogError("[GunFireHandler] Missing GunData on PlayerShoot.");
            enabled = false; return;
        }

        // xác định index fire mode hiện tại trong danh sách cho phép
        modeIndex = Array.IndexOf(gunData.availableFireModes, gunData.fireMode);
        if (modeIndex < 0) modeIndex = 0;
    }

    void Update()
    {
        if (PauseGameUI.isPause) return;

        // Chuyển fire mode khi bấm B (nếu có nhiều mode)
        if (gunData.availableFireModes != null &&
            gunData.availableFireModes.Length > 1 &&
            Input.GetKeyDown(KeyCode.B))
        {
            modeIndex = (modeIndex + 1) % gunData.availableFireModes.Length;
            gunData.fireMode = gunData.availableFireModes[modeIndex];

            // cập nhật UI (nếu có)
            playerShoot.weaponUI?.SetFireMode(gunData.fireMode);

            // nếu chuyển sang Safety thì dừng bắn ngay
            if (gunData.fireMode == GunFireMode.Safety)
                playerShoot.StopFiring();
        }

        // Reload
        if (Input.GetKeyDown(KeyCode.R))
            playerShoot.Reload();

        // Xử lý input theo fireMode
        switch (gunData.fireMode)
        {
            case GunFireMode.SemiAuto:
                // PlayerShoot tự khóa nhịp bằng IsReadyToShoot + cooldown
                if (Input.GetMouseButtonDown(0) && playerShoot.IsReadyToShoot)
                    playerShoot.ShootOneBullet();
                break;

            case GunFireMode.FullAuto:
                // Giao cho PlayerShoot lo vòng bắn theo RPS
                if (Input.GetMouseButtonDown(0))
                    playerShoot.StartFiring();
                if (Input.GetMouseButtonUp(0))
                    playerShoot.StopFiring();
                break;

            case GunFireMode.Burst:
                // Mở một chuỗi bắn burst nếu đủ điều kiện
                if (burstRoutine == null &&
                    Input.GetMouseButtonDown(0) &&
                    Time.time >= nextBurstTime)
                {
                    burstRoutine = StartCoroutine(BurstSequence());
                }
                break;

            case GunFireMode.Safety:
                // Không bắn
                break;
        }
    }

    private IEnumerator BurstSequence()
    {
        // bắn theo cấu hình: burstCount, burstFireRate
        for (int i = 0; i < gunData.burstCount; i++)
        {
            if (playerShoot.currentAmmo <= 0) break;

            // chờ đến khi PlayerShoot cho phép (tôn trọng cooldown/tình trạng reload)
            while (!playerShoot.IsReadyToShoot || playerShoot.isReloading || PauseGameUI.isPause)
                yield return null;

            playerShoot.ShootOneBullet();

            if (i < gunData.burstCount - 1)
                yield return new WaitForSeconds(gunData.burstFireRate);
        }

        // cooldown giữa các lần burst
        nextBurstTime = Time.time + gunData.burstCooldown;
        burstRoutine = null;
    }

    void OnDisable()
    {
        if (burstRoutine != null)
        {
            StopCoroutine(burstRoutine);
            burstRoutine = null;
        }
        // đảm bảo dừng bắn khi handler bị tắt
        if (playerShoot != null) playerShoot.StopFiring();
    }
}
