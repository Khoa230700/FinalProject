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

    [Header("Audio")]
    public AudioSource audioSource;          // đặt trên weapon (hoặc Player)
    public AudioClip shotClip;               // nếu trống → dùng gunData.shootSound
    public AudioClip emptyClip;              // click khi hết đạn
    public AudioClip reloadStartClip;        // bắt đầu nạp
    public AudioClip reloadEndClip;          // kết thúc nạp
    public AudioClip drawClip;               // rút súng
    public AudioClip holsterClip;            // cất súng
    [Range(0f, 1f)] public float shotVolume = 1f;
    [Range(0.8f, 1.2f)] public float shotPitch = 1f;

    [HideInInspector] public int currentAmmo;
    [HideInInspector] public int reserveAmmo;
    public bool isReloading { get; private set; }
    public bool IsShooting { get; private set; }
    public bool IsSwitchingWeapon { get; private set; }

    private Coroutine shotResetCoroutine;
    private Coroutine fireLoopCoroutine;     // loop cho FullAuto
    public bool IsReadyToShoot { get; private set; } = true;

    private int _lastShotFrame = -1;
    private bool initialized = false;

    private GunUpgradeState upgrade;

    public System.Action OnShotFired;                // crosshair bloom
    public System.Action<Vector3, bool> OnBulletHit; // hit marker

    // stat đã upgrade (nếu có)
    int MagazineSize => upgrade ? upgrade.MagazineSize : gunData.magazineSize;
    float Damage => upgrade ? upgrade.Damage : gunData.damage;
    float Range => upgrade ? upgrade.Range : gunData.range;
    float ReloadTime => upgrade ? upgrade.ReloadTime : gunData.reloadTime;
    float SpreadAngle => upgrade ? upgrade.SpreadAngle : gunData.spreadAngle;
    float SemiAutoIv => upgrade ? upgrade.SemiAutoMinInterval : gunData.semiAutoMinInterval;
    float RoundsPerSec => upgrade ? upgrade.RoundsPerSecond : gunData.roundsPerSecond;

    void Awake() { upgrade = GetComponent<GunUpgradeState>(); }

    public void Initialize()
    {
        if (initialized) return;
        currentAmmo = MagazineSize;
        reserveAmmo = gunData.reserveAmmo;
        initialized = true;
    }

    void Start()
    {
        if (!audioSource) audioSource = GetComponent<AudioSource>();
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
        // rút súng
        PlayOneShot(drawClip, 1f);
    }

    public void OnDeselected() { }

    public void StartFiring()
    {
        if (IsShooting) return;
        IsShooting = true;

        if (gunData != null && gunData.fireMode == GunFireMode.FullAuto)
        {
            if (fireLoopCoroutine != null) StopCoroutine(fireLoopCoroutine);
            fireLoopCoroutine = StartCoroutine(FullAutoLoop());
        }
    }

    public void StopFiring()
    {
        if (!IsShooting) return;

        IsShooting = false;

        if (fireLoopCoroutine != null)
        {
            StopCoroutine(fireLoopCoroutine);
            fireLoopCoroutine = null;
        }

        if (csgoScope != null && csgoScope.IsScoped)
            armsAnimator.CrossFade("AimingIdle", 0.08f);
        else
            armsAnimator.CrossFade("Idle", 0.08f);
    }

    public void FireOnce() => ShootOneBullet();

    public Coroutine SwitchOut(MonoBehaviour runner) => runner.StartCoroutine(SwitchOutRoutine());
    public Coroutine SwitchIn(MonoBehaviour runner) => runner.StartCoroutine(SwitchInRoutine());

    private IEnumerator SwitchOutRoutine()
    {
        IsSwitchingWeapon = true;
        StopFiring();
        armsAnimator.SetTrigger("Hide");
        PlayOneShot(holsterClip, 1f);
        yield return new WaitForSeconds(0.3f);
    }

    private IEnumerator SwitchInRoutine()
    {
        armsAnimator.SetTrigger("Get");
        // draw clip đã phát ở OnSelected để chắc chắn vang khi bật active
        yield return new WaitForSeconds(0.3f);
        IsSwitchingWeapon = false;
    }
    // --------- /IWeapon adapters ---------

    private IEnumerator FullAutoLoop()
    {
        float rps = Mathf.Max(0.01f, RoundsPerSec);
        float interval = 1f / rps;

        while (IsShooting && !isReloading && currentAmmo > 0 && gunData.fireMode == GunFireMode.FullAuto)
        {
            if (IsReadyToShoot && !PauseGameUI.isPause)
                ShootOneBullet();

            yield return new WaitForSeconds(interval);
        }

        fireLoopCoroutine = null;
    }

    public void ShootOneBullet()
    {
        if (PauseGameUI.isPause || isReloading) return;

        if (currentAmmo <= 0)
        {
            // click empty
            PlayOneShot(emptyClip, 1f);
            return;
        }

        if (Time.frameCount == _lastShotFrame) return;
        _lastShotFrame = Time.frameCount;

        if (!IsReadyToShoot) return;
        IsReadyToShoot = false;

        currentAmmo--;

        bool scoped = (csgoScope != null && csgoScope.IsScoped);
        string anim = scoped ? "AimingShot" : "Shot";
        armsAnimator.SetBool("Walk", false);
        armsAnimator.SetBool("Run", false);
        armsAnimator.Play(anim, 0, 0f);

        muzzleFlashParticle?.Play();

        // shot SFX (ưu tiên field; nếu trống dùng trong GunData)
        var clip = shotClip ? shotClip : gunData.shootSound;
        if (clip)
        {
            if (audioSource)
            {
                audioSource.pitch = shotPitch;
                audioSource.PlayOneShot(clip, shotVolume);
            }
            else
            {
                AudioSource.PlayClipAtPoint(clip, shootPoint.position, shotVolume);
            }
        }

        OnShotFired?.Invoke();

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
                    bool isHead = (hb.hitboxType == Hitbox.HitboxType.Head);
                    if (isHead) dmg *= 2f;

                    hb.ownerHealthSystem.TakeDamage(dmg);
                    hb.OnHit(dmg, hit.point);

                    OnBulletHit?.Invoke(hit.point, isHead);
                }
            }
        }

        weaponUI?.UpdateAmmoUI(currentAmmo, reserveAmmo);

        if (shotResetCoroutine != null) StopCoroutine(shotResetCoroutine);
        shotResetCoroutine = StartCoroutine(ResetShotAnimation());

        float cooldown = 0f;
        if (gunData.fireMode == GunFireMode.SemiAuto)
        {
            cooldown = SemiAutoIv;
            if (gunData.gunType == GunType.SniperRifle && cooldown <= 0f) cooldown = 1f;
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
            StopFiring();
    }

    public void Reload()
    {
        if (isReloading || currentAmmo >= MagazineSize || reserveAmmo <= 0)
            return;

        isReloading = true;

        if (IsShooting) StopFiring();

        armsAnimator.SetTrigger("Recharge");
        PlayOneShot(reloadStartClip, 1f);
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
        PlayOneShot(reloadEndClip, 1f);
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

        int magazineSpace = MagazineSize - currentAmmo;
        int toMagazine = Mathf.Min(amount, magazineSpace);
        currentAmmo += toMagazine;
        bulletsAdded += toMagazine;
        amount -= toMagazine;

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

    private void PlayOneShot(AudioClip clip, float vol)
    {
        if (!clip) return;
        if (audioSource) audioSource.PlayOneShot(clip, vol);
        else AudioSource.PlayClipAtPoint(clip, shootPoint ? shootPoint.position : transform.position, vol);
    }
}
