using UnityEngine;
using System.Collections;

public class PlayerShoot : MonoBehaviour
{
    [Header("Data & References")]
    public GunData gunData;
    public Transform shootPoint;
    public Animator armsAnimator;
    public WeaponUI weaponUI;

    [Header("Aiming")]
    public CSGOScope csgoScope;               // Kéo thả trong Inspector nếu có scope

    [SerializeField] private ParticleSystem muzzleFlashParticle;

    [HideInInspector] public int currentAmmo;
    [HideInInspector] public int reserveAmmo;
    public bool isReloading { get; private set; }
    public bool IsShooting { get; private set; }
    public bool IsSwitchingWeapon { get; private set; }

    private Coroutine shotResetCoroutine;

    void Start()
    {
        currentAmmo = gunData.magazineSize;
        reserveAmmo = gunData.reserveAmmo;
        weaponUI?.UpdateAmmoUI(currentAmmo, reserveAmmo);
    }

    public bool IsReadyToShoot { get; private set; } = true;

    public void StartShooting()
    {
        if (!IsShooting)
            IsShooting = true;
    }

    public void StopShooting()
    {
        if (IsShooting)
        {
            IsShooting = false;
            // Nếu đang scoped thì về AimingIdle, còn lại về Idle
            if (csgoScope != null && csgoScope.IsScoped)
                armsAnimator.CrossFade("AimingIdle", 0.08f);
            else
                armsAnimator.CrossFade("Idle", 0.08f);
        }
    }

    public void ShootOneBullet()
    {
        if (PauseGameUI.isPause || isReloading || currentAmmo <= 0)
            return;

        currentAmmo--;

        // Chọn animation tùy scoped hay không
        bool scoped = (csgoScope != null && csgoScope.IsScoped);
        string anim = scoped ? "AimingShot" : "Shot";
        armsAnimator.SetBool("Walk", false);
        armsAnimator.SetBool("Run", false);
        armsAnimator.Play(anim, 0, 0f);

        muzzleFlashParticle?.Play();
        if (gunData.shootSound != null)
            AudioSource.PlayClipAtPoint(gunData.shootSound, shootPoint.position);

        int count = (gunData.gunType == GunType.Shotgun)
                    ? gunData.pelletCount
                    : 1;

        for (int i = 0; i < count; i++)
        {
            Vector3 dir = (count == 1)
                          ? shootPoint.forward
                          : GetSpreadDirection(shootPoint.forward, gunData.spreadAngle);

            Ray ray = new Ray(shootPoint.position, dir);
            if (Physics.Raycast(ray, out RaycastHit hit, gunData.range))
            {
                var hb = hit.collider.GetComponent<Hitbox>();
                if (hb != null && hb.ownerHealthSystem != null)
                {
                    float dmg = gunData.damage;
                    if (hb.hitboxType == Hitbox.HitboxType.Head) dmg *= 2f;
                    hb.ownerHealthSystem.TakeDamage(dmg);
                    hb.OnHit(dmg, hit.point);
                }
            }
        }

        weaponUI?.UpdateAmmoUI(currentAmmo, reserveAmmo);

        if (shotResetCoroutine != null) StopCoroutine(shotResetCoroutine);
        shotResetCoroutine = StartCoroutine(ResetShotAnimation());
    }

    private Vector3 GetSpreadDirection(Vector3 forward, float angle)
    {
        float x = Random.Range(-angle, angle);
        float y = Random.Range(-angle, angle);
        return Quaternion.Euler(x, y, 0) * forward;
    }

    private IEnumerator ResetShotAnimation()
    {
        // Đợi clip "Shot" hoặc "AimingShot" kết thúc
        while (!armsAnimator.GetCurrentAnimatorStateInfo(0).IsName("Shot")
               && !armsAnimator.GetCurrentAnimatorStateInfo(0).IsName("AimingShot"))
            yield return null;

        float wait = armsAnimator.GetCurrentAnimatorStateInfo(0).length
                   / armsAnimator.GetCurrentAnimatorStateInfo(0).speed;
        yield return new WaitForSeconds(wait);

        if (csgoScope != null && csgoScope.IsScoped)
        {
            // Giữ AimingIdle giữa các shot khi scoped
            armsAnimator.CrossFade("AimingIdle", 0.08f);
        }
        else if (!Input.GetMouseButton(0) || gunData.fireMode != GunFireMode.FullAuto)
        {
            StopShooting();
        }
    }

    public void Reload()
    {
        if (isReloading || currentAmmo >= gunData.magazineSize || reserveAmmo <= 0)
            return;

        isReloading = true;
        armsAnimator.SetTrigger("Recharge");
        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        yield return new WaitForSeconds(gunData.reloadTime);
        int need = gunData.magazineSize - currentAmmo;
        int used = Mathf.Min(need, reserveAmmo);
        currentAmmo += used;
        reserveAmmo -= used;
        isReloading = false;
        weaponUI?.UpdateAmmoUI(currentAmmo, reserveAmmo);
    }

    public void CancelReload()
    {
        if (!isReloading) return;
        isReloading = false;
        armsAnimator.ResetTrigger("Recharge");
        armsAnimator.SetTrigger("Idle");
    }

    public IEnumerator SwitchOut()
    {
        IsSwitchingWeapon = true;
        armsAnimator.SetTrigger("Hide");
        yield return new WaitForSeconds(0.3f);
    }

    public IEnumerator SwitchIn()
    {
        armsAnimator.SetTrigger("Get");
        yield return new WaitForSeconds(0.3f);
        IsSwitchingWeapon = false;
    }
}
