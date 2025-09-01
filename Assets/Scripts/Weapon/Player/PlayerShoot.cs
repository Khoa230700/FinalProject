using UnityEngine;
using System.Collections;

public class PlayerShoot : MonoBehaviour, IWeapon, IReloadable
{
    [Header("Data & References")]
    public GunData gunData;
    public Transform shootPoint;
    public Animator armsAnimator;
    public WeaponUI weaponUI;

    [Header("Aiming")]
    public CSGOScope csgoScope;

    [SerializeField] private ParticleSystem muzzleFlashParticle;

    [HideInInspector] public int currentAmmo;
    [HideInInspector] public int reserveAmmo;
    public bool isReloading { get; private set; }
    public bool IsShooting { get; private set; }
    public bool IsSwitchingWeapon { get; private set; }

    private Coroutine shotResetCoroutine;
    public bool IsReadyToShoot { get; private set; } = true;

    // guard to avoid double-shot in same frame
    private int _lastShotFrame = -1;
    private bool initialized = false;

    // NEW: optional upgrade state
    private GunUpgradeState upgrade;

    // --------- Helpers (đọc stat đã nâng cấp nếu có) ---------
    int MagazineSize => upgrade ? upgrade.MagazineSize : gunData.magazineSize;
    float Damage => upgrade ? upgrade.Damage : gunData.damage;
    float Range => upgrade ? upgrade.Range : gunData.range;
    float ReloadTime => upgrade ? upgrade.ReloadTime : gunData.reloadTime;
    float SpreadAngle => upgrade ? upgrade.SpreadAngle : gunData.spreadAngle;
    float SemiAutoIv => upgrade ? upgrade.SemiAutoMinInterval : gunData.semiAutoMinInterval;

    void Awake()
    {
        upgrade = GetComponent<GunUpgradeState>();
    }

    public void Initialize()
    {
        if (initialized) return;
        currentAmmo = MagazineSize;           // dùng magazine size đã nâng cấp
        reserveAmmo = gunData.reserveAmmo;    // reserve giữ theo data gốc (tuỳ design)
        initialized = true;
    }

    void Start()
    {
        // Khởi tạo đạn với magazine size đã nâng cấp
        currentAmmo = MagazineSize;
        reserveAmmo = gunData.reserveAmmo;
        weaponUI?.UpdateAmmoUI(currentAmmo, reserveAmmo);
    }

    // --------- IWeapon adapters ---------
    public void OnSelected(WeaponUI ui)
    {
        weaponUI = ui;
        if (weaponUI != null)
        {
            weaponUI.gunData = gunData;
            weaponUI.SetWeaponSprite(gunData.gunSprite);
            weaponUI.SetFireMode(gunData.fireMode);
            weaponUI.CreateBulletUI();
            weaponUI.UpdateAmmoUI(currentAmmo, reserveAmmo);
        }
    }

    public void OnDeselected() { }

    public void StartFiring() => StartShooting();
    public void StopFiring() => StopShooting();
    public void FireOnce() => ShootOneBullet();

    public Coroutine SwitchOut(MonoBehaviour runner)
        => runner.StartCoroutine(SwitchOutRoutine());
    public Coroutine SwitchIn(MonoBehaviour runner)
        => runner.StartCoroutine(SwitchInRoutine());

    private IEnumerator SwitchOutRoutine()
    {
        IsSwitchingWeapon = true;
        armsAnimator.SetTrigger("Hide");
        yield return new WaitForSeconds(0.3f);
    }

    private IEnumerator SwitchInRoutine()
    {
        armsAnimator.SetTrigger("Get");
        yield return new WaitForSeconds(0.3f);
        IsSwitchingWeapon = false;
    }
    // --------- /IWeapon adapters ---------

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
            if (csgoScope != null && csgoScope.IsScoped)
                armsAnimator.CrossFade("AimingIdle", 0.08f);
            else
                armsAnimator.CrossFade("Idle", 0.08f);
        }
    }

    public void ShootOneBullet()
    {
        if (PauseGameUI.isPause || isReloading || currentAmmo <= 0) return;

        // prevent multiple calls in the same frame
        if (Time.frameCount == _lastShotFrame) return;
        _lastShotFrame = Time.frameCount;

        if (!IsReadyToShoot) return;
        IsReadyToShoot = false; // lock immediately

        currentAmmo--;

        bool scoped = (csgoScope != null && csgoScope.IsScoped);
        string anim = scoped ? "AimingShot" : "Shot";
        armsAnimator.SetBool("Walk", false);
        armsAnimator.SetBool("Run", false);
        armsAnimator.Play(anim, 0, 0f);

        muzzleFlashParticle?.Play();
        if (gunData.shootSound != null)
            AudioSource.PlayClipAtPoint(gunData.shootSound, shootPoint.position);

        int pellets = (gunData.gunType == GunType.Shotgun) ? gunData.pelletCount : 1;
        for (int i = 0; i < pellets; i++)
        {
            Vector3 dir = (pellets == 1)
                ? shootPoint.forward
                : GetSpreadDirection(shootPoint.forward, SpreadAngle);

            Ray ray = new Ray(shootPoint.position, dir);
            if (Physics.Raycast(ray, out RaycastHit hit, Range))
            {
                var hb = hit.collider.GetComponent<Hitbox>();
                if (hb != null && hb.ownerHealthSystem != null)
                {
                    float dmg = Damage;
                    if (hb.hitboxType == Hitbox.HitboxType.Head) dmg *= 2f;
                    hb.ownerHealthSystem.TakeDamage(dmg);
                    hb.OnHit(dmg, hit.point);
                }
            }
        }

        weaponUI?.UpdateAmmoUI(currentAmmo, reserveAmmo);

        if (shotResetCoroutine != null) StopCoroutine(shotResetCoroutine);
        shotResetCoroutine = StartCoroutine(ResetShotAnimation());

        // cooldown cho SemiAuto (sniper ~1s nếu chưa set)
        float cooldown = 0f;
        if (gunData.fireMode == GunFireMode.SemiAuto)
        {
            cooldown = SemiAutoIv;
            if (gunData.gunType == GunType.SniperRifle && cooldown <= 0f)
                cooldown = 1f;
        }
        StartCoroutine(ShotCooldown(cooldown));
    }

    private IEnumerator ShotCooldown(float t)
    {
        if (t > 0f) yield return new WaitForSeconds(t);
        IsReadyToShoot = true;
    }

    private Vector3 GetSpreadDirection(Vector3 forward, float angle)
    {
        float x = Random.Range(-angle, angle);
        float y = Random.Range(-angle, angle);
        return Quaternion.Euler(x, y, 0) * forward;
    }

    private IEnumerator ResetShotAnimation()
    {
        while (!armsAnimator.GetCurrentAnimatorStateInfo(0).IsName("Shot")
            && !armsAnimator.GetCurrentAnimatorStateInfo(0).IsName("AimingShot"))
            yield return null;

        float wait = armsAnimator.GetCurrentAnimatorStateInfo(0).length
                   / armsAnimator.GetCurrentAnimatorStateInfo(0).speed;
        yield return new WaitForSeconds(wait);

        if (csgoScope != null && csgoScope.IsScoped)
            armsAnimator.CrossFade("AimingIdle", 0.08f);
        else if (!Input.GetMouseButton(0) || gunData.fireMode != GunFireMode.FullAuto)
            StopShooting();
    }

    public void Reload()
    {
        if (isReloading || currentAmmo >= MagazineSize || reserveAmmo <= 0)
            return;

        isReloading = true;
        armsAnimator.SetTrigger("Recharge");
        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        yield return new WaitForSeconds(ReloadTime);

        int need = MagazineSize - currentAmmo;
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

    //================ SHOP ==================
    public int AddAmmo(int amount)
    {
        if (amount <= 0) return 0;

        int bulletsAdded = 0;

        // Add to current magazine (dựa theo MagazineSize đã upgrade)
        int magazineSpace = MagazineSize - currentAmmo;
        int toMagazine = Mathf.Min(amount, magazineSpace);
        currentAmmo += toMagazine;
        bulletsAdded += toMagazine;
        amount -= toMagazine;

        // Add to reserve (giữ capacity theo gunData.reserveAmmo)
        if (amount > 0)
        {
            int reserveSpace = gunData.reserveAmmo - reserveAmmo;
            int toReserve = Mathf.Min(amount, reserveSpace);
            reserveAmmo += toReserve;
            bulletsAdded += toReserve;
        }

        weaponUI?.UpdateAmmoUI(currentAmmo, reserveAmmo);
        return bulletsAdded;
    }

    public bool NeedsRefill()
    {
        return currentAmmo < MagazineSize || reserveAmmo < gunData.reserveAmmo;
    }

    public int GetAmmoNeeded()
    {
        int currentAmmoNeeded = MagazineSize - currentAmmo;
        int reserveAmmoNeeded = gunData.reserveAmmo - reserveAmmo;
        return Mathf.Max(0, currentAmmoNeeded) + Mathf.Max(0, reserveAmmoNeeded);
    }
}
