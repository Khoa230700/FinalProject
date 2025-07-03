using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GunData gunData;
    public Transform shootPoint;
    public Animator armsAnimator;
    public WeaponUI weaponUI;

    public int currentAmmo;
    private float nextTimeToFire = 0f;

    private bool isRecharge = false;
    private bool isShootingAnimation = false;

    public bool IsShooting => isShootingAnimation;

    [SerializeField] private ParticleSystem muzzleFlashParticle;

    void Start()
    {
        currentAmmo = gunData.magazineSize;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 0 = chuột trái
        {
            TryShoot();
        }
    }

    public void TryShoot()
    {
        if (PauseGameUI.isPause) return;

        if (currentAmmo > 0)
        {
            Shoot();
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

        if (muzzleFlashParticle != null)
            muzzleFlashParticle.Play();

        if (gunData.shootSound)
            AudioSource.PlayClipAtPoint(gunData.shootSound, shootPoint.position);

        Ray ray = new Ray(shootPoint.position, shootPoint.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            Hitbox hitbox = hit.collider.GetComponent<Hitbox>();
            if (hitbox != null && hitbox.ownerHealthSystem != null)
            {
                float finalDamage = gunData.damage;
                if (hitbox.hitboxType == Hitbox.HitboxType.Head)
                    finalDamage *= 2f;

                hitbox.ownerHealthSystem.TakeDamage(finalDamage);
                hitbox.OnHit(finalDamage, hit.point);
            }
        }

        weaponUI.UpdateAmmoUI(currentAmmo, gunData.reserveAmmo);
    }

    public void CancelReload()
    {
        if (isRecharge)
        {
            isRecharge = false;
            armsAnimator.ResetTrigger("Recharge");
            armsAnimator.SetTrigger("Idle");
        }
    }
}
