using UnityEngine;
using System.Collections;

public class PlayerShoot : MonoBehaviour, IWeapon, IReloadable
{
    [Header("Data & References")]
    public GunData gunData;
    public Transform shootPoint;
    public Animator armsAnimator;
    public WeaponUI weaponUI;
    public ShopUI shopUI;

    [Header("Aiming")]
    public CSGOScope csgoScope;

    [SerializeField] private ParticleSystem muzzleFlashParticle;

    [Header("Audio")]
    public AudioSource gunAudioSource;
    public AudioClip reloadSound;
    public AudioClip emptyMagSound;

    [HideInInspector] public int currentAmmo;
    [HideInInspector] public int reserveAmmo;
    public bool isReloading { get; private set; }
    public bool IsShooting { get; private set; }
    public bool IsSwitchingWeapon { get; private set; }

    private Coroutine shotResetCoroutine;
    public bool IsReadyToShoot { get; private set; } = true;

    public static System.Action<Vector3> OnAnyHit;

    private int _lastShotFrame = -1;
    private bool initialized = false;
    private GunUpgradeState upgrade;

    int MagazineSize => upgrade ? upgrade.MagazineSize : gunData.magazineSize;
    float Damage => upgrade ? upgrade.Damage : gunData.damage;
    float Range => upgrade ? upgrade.Range : gunData.range;
    float ReloadTime => upgrade ? upgrade.ReloadTime : gunData.reloadTime;
    float SpreadAngle => upgrade ? upgrade.SpreadAngle : gunData.spreadAngle;
    float SemiAutoIv => upgrade ? upgrade.SemiAutoMinInterval : gunData.semiAutoMinInterval;

    private bool didClick = false;

    void Awake()
    {
        upgrade = GetComponent<GunUpgradeState>();
        shopUI = FindAnyObjectByType<ShopUI>();
    }

    public void Initialize()
    {
        if (initialized) return;
        currentAmmo = MagazineSize;
        reserveAmmo = gunData.reserveAmmo;
        initialized = true;
    }

    void Start()
    {
        currentAmmo = MagazineSize;
        reserveAmmo = gunData.reserveAmmo;
        weaponUI?.UpdateAmmoUI(currentAmmo, reserveAmmo);
        if (weaponUI != null) weaponUI.lastAmmoCount = currentAmmo; // sync ban đầu
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
            weaponUI.lastAmmoCount = currentAmmo;

            // Subscribe vào upgrade event
            if (upgrade != null)
            {
                upgrade.OnLevelChanged.RemoveListener(OnGunUpgraded); // Remove trước để tránh duplicate
                upgrade.OnLevelChanged.AddListener(OnGunUpgraded);
            }
        }
    }

    public void OnDeselected()
    {
        // Unsubscribe khi deselect
        if (upgrade != null)
            upgrade.OnLevelChanged.RemoveListener(OnGunUpgraded);
    }

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
        IsShooting = false;

        if (IsShotAnimPlaying())
            return;

        CrossfadeToIdle();
    }

    public void ShootOneBullet()
    {
        if (PauseGameUI.isPause || isReloading || shopUI.isOpen) return;

        if (currentAmmo <= 0)
        {
            PlayAtGun(emptyMagSound);
            return;
        }

        if (!didClick && QuestManager.Instance.UpdateQuestProgress(QuestObjectiveType.Interact, "TutorialShoot"))
        {
            didClick = true;
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
        PlayAtGun(gunData.shootSound);

        int pellets = (gunData.gunType == GunType.Shotgun) ? gunData.pelletCount : 1;
        for (int i = 0; i < pellets; i++)
        {
            Vector3 dir = (pellets == 1)
                ? shootPoint.forward
                : GetSpreadDirection(shootPoint.forward, SpreadAngle);

            Ray ray = new Ray(shootPoint.position, dir);
            if (Physics.Raycast(ray, out RaycastHit hit, Range, ~0, QueryTriggerInteraction.Collide))
            {
                var hb = hit.collider.GetComponent<Hitbox>() ?? hit.collider.GetComponentInParent<Hitbox>();
                if (hb != null)
                {
                    float dmg = Damage; // để Hitbox quyết định headshot multiplier
                    hb.OnHit(dmg, hit.point, hit.normal);
                    OnAnyHit?.Invoke(hit.point);
                }
                else
                {
                    // Fallback: nếu vật thể không có Hitbox, thử gọi trực tiếp
                    var boss = hit.collider.GetComponentInParent<BossHealth>();
                    if (boss != null)
                    {
                        boss.TakeDamage(Damage);
                    }
                    else
                    {
                        var enemy = hit.collider.GetComponentInParent<EnemyM>();
                        if (enemy != null) enemy.TakeDamage(Damage);
                    }
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
            if (gunData.gunType == GunType.SniperRifle && cooldown <= 0f)
                cooldown = 1f;
        }
        StartCoroutine(ShotCooldown(cooldown));
    }

    private bool IsShotAnimPlaying()
    {
        var st = armsAnimator.GetCurrentAnimatorStateInfo(0);
        return (st.IsName("Shot") || st.IsName("AimingShot")) && st.normalizedTime < 0.98f;
    }

    private void CrossfadeToIdle()
    {
        if (csgoScope != null && csgoScope.IsScoped)
            armsAnimator.CrossFade("AimingIdle", 0.08f);
        else
            armsAnimator.CrossFade("Idle", 0.08f);
    }

    private void PlayAtGun(AudioClip clip)
    {
        if (clip == null) return;
        if (gunAudioSource != null)
        {
            gunAudioSource.PlayOneShot(clip);
        }
        else
        {
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
        while (!armsAnimator.GetCurrentAnimatorStateInfo(0).IsName("Shot")
            && !armsAnimator.GetCurrentAnimatorStateInfo(0).IsName("AimingShot"))
            yield return null;

        var st = armsAnimator.GetCurrentAnimatorStateInfo(0);
        float wait = st.length / Mathf.Max(0.0001f, st.speed);
        yield return new WaitForSeconds(wait);

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
        PlayAtGun(reloadSound);

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

private void OnGunUpgraded(int newLevel)
{
    if (weaponUI == null) return;

    int oldMagSize = weaponUI.bulletImages?.Count ?? 0;
    int newMagSize = MagazineSize;
    
    // Nếu magazine size thay đổi, recreate UI
    if (oldMagSize != newMagSize)
    {
        weaponUI.CreateBulletUI();
        
        // Nếu magazine size tăng và magazine cũ đã full, thêm ammo
        if (newMagSize > oldMagSize && currentAmmo == oldMagSize)
        {
            int additionalAmmo = newMagSize - oldMagSize;
            int ammoToAdd = Mathf.Min(additionalAmmo, reserveAmmo);
            currentAmmo += ammoToAdd;
            reserveAmmo -= ammoToAdd;
        }
    }
    
    weaponUI.UpdateAmmoUI(currentAmmo, reserveAmmo);
}
}
