using UnityEngine;
using System.Collections;

public class PlayerShoot : MonoBehaviour
{
    public GunData gunData;
    public Transform shootPoint;
    public Animator armsAnimator;
    public WeaponUI weaponUI;

    [HideInInspector] public int currentAmmo;
    [HideInInspector] public int reserveAmmo;
    public bool isReloading = false;
    public bool IsShooting;

    [SerializeField] private ParticleSystem muzzleFlashParticle;

    void Start()
    {
        currentAmmo = gunData.magazineSize;
        reserveAmmo = gunData.reserveAmmo;
        weaponUI.UpdateAmmoUI(currentAmmo, reserveAmmo);
    }

    public void ShootOneBullet()
    {
        if (PauseGameUI.isPause || isReloading || currentAmmo <= 0)
        {
            Debug.Log($"Cannot shoot: Paused: {PauseGameUI.isPause}, Reloading: {isReloading}, CurrentAmmo: {currentAmmo}");
            return;
        }

        currentAmmo--;
        armsAnimator.ResetTrigger("Shot"); // Reset trước
        armsAnimator.SetTrigger("Shot");

        if (muzzleFlashParticle != null)
        {
            muzzleFlashParticle.Play();
            Debug.Log("muzzle play");
        }

        if (gunData.shootSound != null)
            AudioSource.PlayClipAtPoint(gunData.shootSound, shootPoint.position);

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
        //Debug.Log($"Shot fired! Ammo left: {currentAmmo}");
        armsAnimator.SetTrigger("Idle");
    }

    public void Reload()
    {
        if (isReloading) return;
        if (currentAmmo >= gunData.magazineSize) return;
        if (reserveAmmo <= 0) return;

        isReloading = true;
        armsAnimator.SetTrigger("Recharge");
        StartCoroutine(ReloadRoutine());
    }

    IEnumerator ReloadRoutine()
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
