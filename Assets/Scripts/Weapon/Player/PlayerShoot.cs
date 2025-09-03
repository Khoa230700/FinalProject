using UnityEngine;
using System.Collections;

public class PlayerShoot : MonoBehaviour, IWeapon, IReloadable
{
    [Header("Data & References")]
    public GunData gunData;                  // Chứa damage, range, fire mode, v.v... (đang dùng)
    public Transform shootPoint;             // Điểm bắn, dùng cho raycast & PlayClipAtPoint fallback
    public Animator armsAnimator;
    public WeaponUI weaponUI;

    [Header("Aiming")]
    public CSGOScope csgoScope;              // Nếu có scope ngắm

    [SerializeField] private ParticleSystem muzzleFlashParticle;

    [Header("Audio")]
    [Tooltip("AudioSource gắn trên prefab súng (khuyến nghị). Nếu để trống sẽ fallback sang PlayClipAtPoint tại shootPoint).")]
    public AudioSource gunAudioSource;
    [Tooltip("Âm thanh khi nạp đạn.")]
    public AudioClip reloadSound;
    [Tooltip("Âm thanh khi bóp cò nhưng hết đạn.")]
    public AudioClip emptyMagSound;

    [HideInInspector] public int currentAmmo;
    [HideInInspector] public int reserveAmmo;
    public bool isReloading { get; private set; }
    public bool IsShooting { get; private set; }
    public bool IsSwitchingWeapon { get; private set; }

    private Coroutine shotResetCoroutine;
    public bool IsReadyToShoot { get; private set; } = true;

    public static System.Action<Vector3> OnAnyHit; // point

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
        currentAmmo = MagazineSize;         // dùng magazine size đã nâng cấp
        reserveAmmo = gunData.reserveAmmo;  // reserve theo data gốc (tuỳ design)
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

    public Coroutine SwitchOut(MonoBehaviour runner) => runner.StartCoroutine(SwitchOutRoutine());
    public Coroutine SwitchIn(MonoBehaviour runner) => runner.StartCoroutine(SwitchInRoutine());

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
        if (!IsShooting) return;

        // Đánh dấu đã dừng bắn, nhưng KHÔNG cắt ngang nếu clip Shot/AimingShot còn đang chạy
        IsShooting = false;

        if (IsShotAnimPlaying())
            return; // để ResetShotAnimation đưa về Idle khi clip kết thúc

        CrossfadeToIdle();
    }

    public void ShootOneBullet()
    {
        if (PauseGameUI.isPause || isReloading) return;

        // Hết đạn: phát "click" và thoát sớm
        if (currentAmmo <= 0)
        {
            PlayAtGun(emptyMagSound);
            return;
        }

        // prevent multiple calls in the same frame
        if (Time.frameCount == _lastShotFrame) return;
        _lastShotFrame = Time.frameCount;

        if (!IsReadyToShoot) return;
        IsReadyToShoot = false; // khoá bắn ngay lập tức

        currentAmmo--;

        bool scoped = (csgoScope != null && csgoScope.IsScoped);
        string anim = scoped ? "AimingShot" : "Shot";
        armsAnimator.SetBool("Walk", false);
        armsAnimator.SetBool("Run", false);
        armsAnimator.Play(anim, 0, 0f);

        // Hiệu ứng & âm thanh bắn
        muzzleFlashParticle?.Play();
        PlayAtGun(gunData.shootSound);

        // Raycast (pellets cho shotgun)
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

                    OnAnyHit?.Invoke(hit.point); // <--- phát tín hiệu
                }
            }
        }

        weaponUI?.UpdateAmmoUI(currentAmmo, reserveAmmo);

        if (shotResetCoroutine != null) StopCoroutine(shotResetCoroutine);
        shotResetCoroutine = StartCoroutine(ResetShotAnimation());

        // cooldown cho SemiAuto (giữ nguyên logic khoá bắn của bạn trong GunData)
        float cooldown = 0f;
        if (gunData.fireMode == GunFireMode.SemiAuto)
        {
            cooldown = SemiAutoIv;
            if (gunData.gunType == GunType.SniperRifle && cooldown <= 0f)
                cooldown = 1f;
        }
        StartCoroutine(ShotCooldown(cooldown));
    }

    // ==================== Helpers cho Cách A ====================
    private bool IsShotAnimPlaying()
    {
        var st = armsAnimator.GetCurrentAnimatorStateInfo(0);
        // normalizedTime < 1 tức là clip chưa chạy xong
        return (st.IsName("Shot") || st.IsName("AimingShot")) && st.normalizedTime < 0.98f;
    }

    private void CrossfadeToIdle()
    {
        if (csgoScope != null && csgoScope.IsScoped)
            armsAnimator.CrossFade("AimingIdle", 0.08f);
        else
            armsAnimator.CrossFade("Idle", 0.08f);
    }
    // ===========================================================

    private void PlayAtGun(AudioClip clip)
    {
        if (clip == null) return;

        // Ưu tiên AudioSource gắn trên súng để kiểm soát volume/spatial
        if (gunAudioSource != null)
        {
            gunAudioSource.PlayOneShot(clip);
        }
        else
        {
            // Fallback: phát tại vị trí shootPoint (3D)
            var pos = (shootPoint != null) ? shootPoint.position : transform.position;
            AudioSource.PlayClipAtPoint(clip, pos);
        }
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
        // Chờ đến khi state Shot/AimingShot thực sự được play
        while (!armsAnimator.GetCurrentAnimatorStateInfo(0).IsName("Shot")
            && !armsAnimator.GetCurrentAnimatorStateInfo(0).IsName("AimingShot"))
            yield return null;

        // Chờ hết thời lượng state hiện tại
        var st = armsAnimator.GetCurrentAnimatorStateInfo(0);
        float wait = st.length / Mathf.Max(0.0001f, st.speed); // tránh chia 0
        yield return new WaitForSeconds(wait);

        // Clip đã xong:
        // - Nếu vẫn giữ chuột và là FullAuto, để vòng lặp bắn tiếp quản lý (không ép về Idle).
        // - Ngược lại, đưa về Idle/AimingIdle.
        bool stillHolding = Input.GetMouseButton(0);
        if (!(stillHolding && gunData.fireMode == GunFireMode.FullAuto))
        {
            CrossfadeToIdle();
        }
    }

    public void Reload()
    {
        if (isReloading || currentAmmo >= MagazineSize || reserveAmmo <= 0)
            return;

        isReloading = true;

        // Phát âm thanh nạp đạn ngay khi bắt đầu (tuỳ chỉnh lại thời điểm nếu animation yêu cầu)
        PlayAtGun(reloadSound);

        armsAnimator.SetTrigger("Recharge");
        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        // Chờ đúng thời lượng reload để add đạn
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

        // Add vào băng đạn hiện tại (dựa theo MagazineSize đã upgrade)
        int magazineSpace = MagazineSize - currentAmmo;
        int toMagazine = Mathf.Min(amount, magazineSpace);
        currentAmmo += toMagazine;
        bulletsAdded += toMagazine;
        amount -= toMagazine;

        // Add vào reserve (giữ capacity theo gunData.reserveAmmo)
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
