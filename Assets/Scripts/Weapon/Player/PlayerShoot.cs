using UnityEngine;
using System.Collections;

public class PlayerShoot : MonoBehaviour
{
    [Header("Data & References")]
    public GunData gunData;
    public Transform shootPoint;
    public Animator armsAnimator;
    public WeaponUI weaponUI;
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

    public bool IsReadyToShoot { get; private set; } = true; // Chặn spam bắn

    public void StartShooting()
    {
        if (!IsShooting)
        {
            IsShooting = true;
        }
    }

    public void StopShooting()
    {
        if (IsShooting)
        {
            IsShooting = false;
            armsAnimator.CrossFade("Idle", 0.08f);
        }
    }

    public void ShootOneBullet()
    {
        if (PauseGameUI.isPause || isReloading || currentAmmo <= 0)
            return;

        currentAmmo--;

        // Tắt anim run, ép chạy shot
        armsAnimator.SetBool("Walk", false);
        armsAnimator.SetBool("Run", false);
        armsAnimator.Play("Shot", 0, 0f);

        if (muzzleFlashParticle != null) muzzleFlashParticle.Play();
        if (gunData.shootSound != null)
            AudioSource.PlayClipAtPoint(gunData.shootSound, shootPoint.position);

        if (muzzleFlashParticle != null) muzzleFlashParticle.Play();
        if (gunData.shootSound != null)
            AudioSource.PlayClipAtPoint(gunData.shootSound, shootPoint.position);

        // Raycast...
        Ray ray = new Ray(shootPoint.position, shootPoint.forward);
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

        weaponUI?.UpdateAmmoUI(currentAmmo, reserveAmmo);

        // Luôn chờ đúng thời lượng anim, rồi unlock (bắn viên tiếp theo)
        if (shotResetCoroutine != null) StopCoroutine(shotResetCoroutine);
        shotResetCoroutine = StartCoroutine(ResetShotAnimation());
    }

    private IEnumerator ResetShotAnimation()
    {
        // Đợi hết clip shot
        while (!armsAnimator.GetCurrentAnimatorStateInfo(0).IsName("Shot"))
            yield return null;
        float wait = armsAnimator.GetCurrentAnimatorStateInfo(0).length / armsAnimator.GetCurrentAnimatorStateInfo(0).speed;
        yield return new WaitForSeconds(wait);

        // KHÔNG được về Idle nếu còn giữ chuột và ở chế độ auto!
        if (!Input.GetMouseButton(0) || gunData.fireMode != GunFireMode.FullAuto)
        {
            StopShooting();
        }
        // Nếu vẫn giữ chuột (auto), thì IsShooting vẫn true, không reset Idle.
    }

    // --- Reload và Switch giữ nguyên, không cho bắn khi isReloading/IsSwitchingWeapon ---

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
        if (reserveAmmo >= need)
        {
            currentAmmo += need;
            reserveAmmo -= need;
        }
        else
        {
            currentAmmo += reserveAmmo;
            reserveAmmo = 0;
        }
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
