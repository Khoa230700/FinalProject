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

    // Cờ báo đang trong chuỗi bắn
    public bool IsShooting { get; private set; }

    // Thời gian cho các animation switch (nếu bạn có)
    [Header("Switch Animations")]
    [SerializeField] private float hideDuration = 0.5f;
    [SerializeField] private float getDuration = 0.5f;
    public bool IsSwitchingWeapon { get; private set; }

    void Start()
    {
        currentAmmo = gunData.magazineSize;
        reserveAmmo = gunData.reserveAmmo;
        weaponUI.UpdateAmmoUI(currentAmmo, reserveAmmo);
    }

    #region SWITCH WEAPON (nếu dùng)
    public IEnumerator SwitchOut()
    {
        IsSwitchingWeapon = true;
        armsAnimator.SetTrigger("Hide");
        yield return new WaitForSeconds(hideDuration);
    }

    public IEnumerator SwitchIn()
    {
        armsAnimator.SetTrigger("Get");
        yield return new WaitForSeconds(getDuration);
        IsSwitchingWeapon = false;
    }
    #endregion

    public void ShootOneBullet()
    {
        if (PauseGameUI.isPause || isReloading || currentAmmo <= 0)
            return;

        // → Bắt đầu bắn
        IsShooting = true;

        // Giảm đạn, trigger Shot
        currentAmmo--;
        armsAnimator.ResetTrigger("Shot");
        armsAnimator.SetTrigger("Shot");

        // Muzzle + âm thanh
        if (muzzleFlashParticle != null) muzzleFlashParticle.Play();
        if (gunData.shootSound != null)
            AudioSource.PlayClipAtPoint(gunData.shootSound, shootPoint.position);

        // Raycast gây damage…
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

        weaponUI.UpdateAmmoUI(currentAmmo, reserveAmmo);

        // Chờ xong clip “Shot” rồi mới xét có reset về Idle hay không
        StartCoroutine(ResetShootingAfterAnimation());
    }

    /// <summary>
    /// Sau khi clip “Shot” kết thúc:
    /// - Nếu là Semi-Auto → luôn về Idle, IsShooting=false  
    /// - Nếu là Full-Auto và bạn vẫn giữ chuột → giữ IsShooting=true, không về Idle  
    /// - Nếu là Full-Auto và bạn đã buông chuột → về Idle, IsShooting=false
    /// </summary>
    private IEnumerator ResetShootingAfterAnimation()
    {
        AnimatorStateInfo info = armsAnimator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(info.length);

        bool stillHolding = Input.GetMouseButton(0);
        bool isFullAuto = gunData.fireMode == GunFireMode.FullAuto;

        if (!isFullAuto || !stillHolding)
        {
            armsAnimator.SetTrigger("Idle");
            IsShooting = false;
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
        weaponUI.UpdateAmmoUI(currentAmmo, reserveAmmo);
    }

    public void CancelReload()
    {
        if (!isReloading) return;
        isReloading = false;
        armsAnimator.ResetTrigger("Recharge");
        armsAnimator.SetTrigger("Idle");
    }
}
