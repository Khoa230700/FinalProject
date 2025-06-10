using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GunData gunData;
    public Transform shootPoint;
    public Animator armsAnimator;

    private int currentAmmo;
    private float nextTimeToFire = 0f;

    private bool isShooting = false;
    private bool isRecharge = false;

    [SerializeField] private ParticleSystem muzzleFlashParticle;

    void Start()
    {
        currentAmmo = gunData.magazineSize;
    }

    void Update()
    {
        if (gunData.fireMode == GunFireMode.FullAuto)
        {
            if (!isRecharge && Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
            {
                TryShoot();
            }
        }
        else if (gunData.fireMode == GunFireMode.SemiAuto)
        {
            if (!isRecharge && Input.GetButtonDown("Fire1") && Time.time >= nextTimeToFire)
            {
                TryShoot();
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && !isRecharge)
        {
            Reload();
        }

        if (isShooting)
        {
            AnimatorStateInfo stateInfo = armsAnimator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Shot") && stateInfo.normalizedTime >= 0.01f)
            {
                isShooting = false;
                armsAnimator.ResetTrigger("Shot");
                armsAnimator.SetTrigger("Idle");
            }
        }

        if (isRecharge)
        {
            AnimatorStateInfo stateInfo = armsAnimator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Recharge") && stateInfo.normalizedTime >= 0.95f)
            {
                isRecharge = false;
                armsAnimator.ResetTrigger("Recharge");
                armsAnimator.SetTrigger("Idle");
            }
        }
    }

    void TryShoot()
    {
        if (currentAmmo > 0)
        {
            Shoot();
            nextTimeToFire = Time.time + 1f / gunData.fireRate;
        }
        else
        {
            Debug.Log("Out of ammo!");
        }
    }

    void Shoot()
    {
        currentAmmo--;

        armsAnimator.SetTrigger("Shot");
        isShooting = true;

        GameObject bullet = Instantiate(gunData.bulletPrefab, shootPoint.position, shootPoint.rotation);
        BulletRaycast bulletScript = bullet.GetComponent<BulletRaycast>();
        if (bulletScript != null)
        {
            bulletScript.InitFromGunData(gunData, shootPoint);
        }

        // Muzzle Flash
        if (muzzleFlashParticle != null)
        {
            muzzleFlashParticle.Play();
        }

        // Âm thanh
        if (gunData.shootSound)
        {
            AudioSource.PlayClipAtPoint(gunData.shootSound, shootPoint.position);
        }
    }


    void Reload()
    {
        armsAnimator.SetTrigger("Recharge");
        isRecharge = true;
        currentAmmo = gunData.magazineSize;
    }
}
