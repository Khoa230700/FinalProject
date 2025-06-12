using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GunData gunData;
    public Transform shootPoint;
    public Animator armsAnimator;
    [HideInInspector] public WeaponUI weaponUI;


    public int currentAmmo;
    private float nextTimeToFire = 0f;

    //private bool isShooting = false;
    private bool isRecharge = false;

    private bool isShootingAnimation = false;
    private bool isHoldingFire = false;

    public bool IsShooting => isShootingAnimation || isHoldingFire;

    [SerializeField] private ParticleSystem muzzleFlashParticle;

    void Start()
    {
        currentAmmo = gunData.magazineSize;
    }

    void Update()
    {
        // Kiểm tra input
        if (gunData.fireMode == GunFireMode.FullAuto)
        {
            isHoldingFire = Input.GetButton("Fire1"); // giữ trạng thái nhấn chuột

            if (!isRecharge && isHoldingFire && Time.time >= nextTimeToFire)
            {
                TryShoot();
            }
        }
        else if (gunData.fireMode == GunFireMode.SemiAuto)
        {
            isHoldingFire = Input.GetButton("Fire1");

            if (!isRecharge && Input.GetButtonDown("Fire1") && Time.time >= nextTimeToFire)
            {
                TryShoot();
            }
        }
        else
        {
            isHoldingFire = false; // không bắn
        }

        if (Input.GetKeyDown(KeyCode.R) && !isRecharge)
        {
            Reload();
        }

        if (isShootingAnimation)
        {
            AnimatorStateInfo stateInfo = armsAnimator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Shot") && stateInfo.normalizedTime >= 0.01f)
            {
                isShootingAnimation = false;
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
        isShootingAnimation = true;

        if (gunData.tracerPrefab != null)
        {
            GameObject tracer = Instantiate(gunData.tracerPrefab);
            BulletTracer bt = tracer.GetComponent<BulletTracer>();
            if (bt != null)
            {
                bt.Init(shootPoint.position, shootPoint.forward);
            }
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

        weaponUI.UpdateAmmoUI(currentAmmo, gunData.reserveAmmo);
    }


    void Reload()
    {
        armsAnimator.SetTrigger("Recharge");
        isRecharge = true;
        currentAmmo = gunData.magazineSize;
        weaponUI.UpdateAmmoUI(currentAmmo, gunData.reserveAmmo);
    }
}
