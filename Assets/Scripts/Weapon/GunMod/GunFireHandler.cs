using UnityEngine;
using System.Collections;

public class GunFireHandler : MonoBehaviour
{
    [Header("References")]
    public PlayerShoot playerShoot;

    [Header("Burst Settings")]
    public int burstCount = 3;
    public float burstFireRate = 0.1f;
    public float burstCooldown = 0.3f;

    [Header("Full-Auto Settings")]
    public float roundsPerSecond = 5f;

    private GunData gunData;
    private int modeIndex = 0;
    private float nextAutoTime = 0f;
    private Coroutine burstRoutine;
    private float nextBurstTime = 0f;

    void Start()
    {
        gunData = playerShoot.gunData;
        modeIndex = System.Array.IndexOf(gunData.availableFireModes, gunData.fireMode);
        roundsPerSecond = gunData.roundsPerSecond;
    }

    void Update()
    {
        if (PauseGameUI.isPause) return;

        if (gunData.availableFireModes.Length > 1 && Input.GetKeyDown(KeyCode.B))
        {
            modeIndex = (modeIndex + 1) % gunData.availableFireModes.Length;
            gunData.fireMode = gunData.availableFireModes[modeIndex];
        }

        if (Input.GetKeyDown(KeyCode.R)) playerShoot.Reload();

        switch (gunData.availableFireModes[modeIndex])
        {
            case GunFireMode.SemiAuto:
                if (Input.GetMouseButtonDown(0) && playerShoot.IsReadyToShoot)
                {
                    playerShoot.ShootOneBullet();
                }
                break;

            case GunFireMode.FullAuto:
                if (Input.GetMouseButtonDown(0))
                    playerShoot.StartShooting();
                if (Input.GetMouseButtonUp(0))
                    playerShoot.StopShooting();

                if (Input.GetMouseButton(0) && Time.time >= nextAutoTime)
                {
                    playerShoot.ShootOneBullet();
                    nextAutoTime = Time.time + 1f / roundsPerSecond;
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
        for (int i = 0; i < burstCount; i++)
        {
            if (playerShoot.currentAmmo <= 0) break;
            // Chờ tới khi animation sẵn sàng
            while (!playerShoot.IsReadyToShoot) yield return null;
            playerShoot.ShootOneBullet();
            if (i < burstCount - 1)
                yield return new WaitForSeconds(burstFireRate);
        }
        nextBurstTime = Time.time + burstCooldown;
        burstRoutine = null;
    }
}
