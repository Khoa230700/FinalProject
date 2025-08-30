// GunData.cs
using UnityEngine;

public enum GunType { Handgun, AssaultRifle, SniperRifle, Shotgun }
public enum GunFireMode { SemiAuto, FullAuto, Burst, Safety }
public enum GunSlot { Primary, Secondary }

[CreateAssetMenu(fileName = "NewGunData", menuName = "Gun/Gun Data")]
public class GunData : ScriptableObject
{
    [Header("Identity")]
    public string gunName;
    public GunType gunType;
    public GunSlot gunSlot;
    public Sprite gunSprite;            // Hình ảnh cho UI
    public Sprite gunSpriteFullColor;   // Hình ảnh cho UI

    [Header("Ballistics (base)")]
    public int damage = 20;             // base damage (per pellet với shotgun)
    public float range = 100f;
    [Tooltip("Pellet count per shot (only for Shotgun)")]
    public int pelletCount = 1;
    [Tooltip("Spread angle in degrees (only for Shotgun)")]
    public float spreadAngle = 0f;

    [Header("Fire Mode Settings")]
    [Tooltip("FullAuto: rounds per second")]
    public float roundsPerSecond = 5f;
    [Tooltip("Burst: number of shots per burst")]
    public int burstCount = 3;
    [Tooltip("Burst: time between shots in a burst")]
    public float burstFireRate = 0.1f;
    [Tooltip("Burst: cooldown after completing a burst")]
    public float burstCooldown = 0.3f;

    public GunFireMode fireMode;
    public GunFireMode[] availableFireModes;

    [Header("Fire Rate Limits")]
    [Tooltip("Khoảng cách tối thiểu giữa 2 phát (SemiAuto). 0 = không giới hạn")]
    public float semiAutoMinInterval = 0f; // đặt 1.0 cho sniper

    [Header("Magazine (base)")]
    public int magazineSize = 30;
    public int reserveAmmo = 90;
    public float reloadTime = 2.0f;

    [Header("Scope")]
    public bool hasScope;
    [Tooltip("Field of view when scoped")]
    public float scopeZoom = 10f;

    [Header("UI & Effects")]
    public GameObject tracerPrefab;
    public AudioClip shootSound;

    [Header("Shop")]
    public int bulletRefillCost = 10; // cost mỗi viên đạn

    // ========================= UPGRADE CURVES (giống Melee) =========================
    // Level 0 tương đương level 1 cho người chơi (đây là index), bạn có thể xem curve ở cột X.
    [Header("Upgrade Curves (nhân hệ số theo level)")]
    [Tooltip("Damage multiplier theo level (0 = level1).")]
    public AnimationCurve damageByLevel = AnimationCurve.Linear(0, 1f, 5, 1.6f);

    [Tooltip("Range multiplier theo level.")]
    public AnimationCurve rangeByLevel = AnimationCurve.Linear(0, 1f, 5, 1.2f);

    [Tooltip("RoundsPerSecond multiplier theo level (lớn hơn = bắn nhanh hơn).")]
    public AnimationCurve rofByLevel = AnimationCurve.Linear(0, 1f, 5, 1.25f);

    [Tooltip("ReloadTime multiplier theo level (nhỏ hơn = nạp nhanh hơn).")]
    public AnimationCurve reloadByLevel = AnimationCurve.Linear(0, 1f, 5, 0.8f);

    [Tooltip("MagazineSize multiplier theo level.")]
    public AnimationCurve magazineByLevel = AnimationCurve.Linear(0, 1f, 5, 1.5f);

    [Tooltip("Spread multiplier theo level (nhỏ hơn = chính xác hơn).")]
    public AnimationCurve spreadByLevel = AnimationCurve.Linear(0, 1f, 5, 0.85f);

    [Tooltip("Semi-auto interval multiplier (nhỏ hơn = bắn nhịp nhanh hơn).")]
    public AnimationCurve semiAutoIntervalByLevel = AnimationCurve.Linear(0, 1f, 5, 0.8f);

    // Optional cho scope zoom (tuỳ game design):
    [Tooltip("Scope zoom multiplier (nhỏ hơn = zoom sâu hơn, tuỳ bạn có dùng hay không).")]
    public AnimationCurve scopeZoomByLevel = AnimationCurve.Linear(0, 1f, 5, 0.85f);

    // ========================= Getters theo level =========================
    private static float EvalOrOne(AnimationCurve c, int lvl)
    {
        if (c == null) return 1f;
        // clamp tối thiểu tránh 0 hoặc âm khi nhân
        return Mathf.Max(0.01f, c.Evaluate(lvl));
    }

    public float GetDamage(int level) => damage * EvalOrOne(damageByLevel, level);
    public float GetRange(int level) => range * EvalOrOne(rangeByLevel, level);
    public float GetRoundsPerSecond(int level) => roundsPerSecond * EvalOrOne(rofByLevel, level);
    public float GetReloadTime(int level) => reloadTime * EvalOrOne(reloadByLevel, level);
    public int GetMagazineSize(int level) => Mathf.Max(1, Mathf.RoundToInt(magazineSize * EvalOrOne(magazineByLevel, level)));
    public float GetSpreadAngle(int level) => spreadAngle * EvalOrOne(spreadByLevel, level);
    public float GetSemiAutoMinInterval(int level) => semiAutoMinInterval * EvalOrOne(semiAutoIntervalByLevel, level);
    public float GetScopeZoom(int level) => scopeZoom * EvalOrOne(scopeZoomByLevel, level);

    // ====== Getters theo max level ======
    public float GetMaxDamage(int maxLevel) => GetDamage(maxLevel);
    public float GetMaxRange(int maxLevel) => GetRange(maxLevel);
    public float GetMaxRoundsPerSecond(int maxLevel) => GetRoundsPerSecond(maxLevel);
    public float GetMaxReloadTime(int maxLevel) => GetReloadTime(maxLevel);
    public int GetMaxMagazineSize(int maxLevel) => GetMagazineSize(maxLevel);
    public float GetMaxSpreadAngle(int maxLevel) => GetSpreadAngle(maxLevel);
    public float GetMaxSemiAutoMinInterval(int maxLevel) => GetSemiAutoMinInterval(maxLevel);
    public float GetMaxScopeZoom(int maxLevel) => GetScopeZoom(maxLevel);

}
