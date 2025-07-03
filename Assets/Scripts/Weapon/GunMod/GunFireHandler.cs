using UnityEngine;
using System;
using System.Collections;

public class GunFireHandler : MonoBehaviour
{
    [Header("References")]
    public PlayerShoot playerShoot;

    [Header("Burst Settings")]
    [Tooltip("Tổng số viên bắn khi burst")]
    public int burstCount = 3;
    [Tooltip("Khoảng giây giữa mỗi viên khi burst")]
    public float burstFireRate = 0.1f;
    [Tooltip("Thời gian chờ sau khi burst kết thúc mới được bắn tiếp")]
    public float burstCooldown = 0.3f;

    [Header("Full-Auto Settings")]
    [Tooltip("Số viên bắn mỗi giây (chỉ Full-Auto)")]
    public float roundsPerSecond = 5f;

    private GunData gunData;
    private int modeIndex = 0;
    private float nextAutoTime = 0f;
    private Coroutine burstRoutine;
    private float nextBurstTime = 0f;

    void Start()
    {
        gunData = playerShoot.gunData;
        modeIndex = Array.IndexOf(gunData.availableFireModes, gunData.fireMode);
        roundsPerSecond = gunData.roundsPerSecond;
    }

    void Update()
    {
        if (PauseGameUI.isPause) return;

        if (gunData.availableFireModes.Length > 1 && Input.GetKeyDown(KeyCode.B))
        {
            modeIndex = (modeIndex + 1) % gunData.availableFireModes.Length;
            Debug.Log(">>> Switched to " + gunData.availableFireModes[modeIndex]);
        }

        if (Input.GetKeyDown(KeyCode.R))
            playerShoot.Reload();

        switch (gunData.availableFireModes[modeIndex])
        {
            case GunFireMode.SemiAuto:
                if (Input.GetMouseButtonDown(0))
                {
                    playerShoot.ShootOneBullet();
                }
                break;

            case GunFireMode.FullAuto:
                if (burstRoutine == null
                    && Input.GetMouseButton(0)
                    && Time.time >= nextAutoTime)
                {
                    nextAutoTime = Time.time + 1f / roundsPerSecond;
                    playerShoot.ShootOneBullet();
                }
                break;

            case GunFireMode.Burst:
                if (burstRoutine == null && Input.GetMouseButtonDown(0) && Time.time >= nextBurstTime)
                {
                    burstRoutine = StartCoroutine(BurstSequence());
                }
                break;

            case GunFireMode.Safety:
                break;
        }
    }

    IEnumerator BurstSequence()
    {
        Debug.Log($">>> BurstSequence start ({burstCount} shots)");

        playerShoot.ShootOneBullet();

        for (int i = 1; i < burstCount; i++)
        {
            yield return new WaitForSeconds(burstFireRate);
            playerShoot.ShootOneBullet();
        }

        Debug.Log(">>> BurstSequence end");
        nextBurstTime = Time.time + burstCooldown;
        burstRoutine = null;
    }
}