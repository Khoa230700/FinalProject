using UnityEngine;

[CreateAssetMenu(fileName = "NewMeleeData", menuName = "Weapon/Melee Data")]
public class MeleeData : ScriptableObject
{
    [Header("Identity/UI")]
    public string weaponName = "Knife";
    public Sprite weaponSprite;
    public Sprite weaponSpriteFullColor;

    [Header("Core Stats")]
    public float baseDamage = 40f;
    public float baseRange = 2.2f;
    public float baseRadius = 0.25f;
    public float baseHitCooldown = 0.5f;
    [Tooltip("Số bước quét (SphereCast) trong một cú vung")]
    public int sweepSteps = 6;
    public LayerMask hitMask = ~0;

    [Header("Timing / Anim")]
    [Tooltip("Delay tới “damage window” (giây) – có thể thay bằng Animation Event)")]
    public float swingDelay = 0.12f;
    public string animGet = "Get";
    public string animHide = "Hide";
    public string[] animSwings = { "Attack_1", "Attack_2" };
    public string animSwing = "Attack_1";

    [Header("FX/SFX")]
    public AudioClip swingSfx;
    public AudioClip hitSfx;

    [Header("Upgrade Curves (tuỳ chọn)")]
    [Tooltip("Hệ số nhân theo level (0 = level1). Nếu rỗng = 1.0f")]
    public AnimationCurve damageByLevel = AnimationCurve.Linear(0, 1f, 5, 1.8f);
    public AnimationCurve rangeByLevel = AnimationCurve.Linear(0, 1f, 5, 1.15f);
    public AnimationCurve cdByLevel = AnimationCurve.Linear(0, 1f, 5, 0.7f);

    public int GetMaxLevel()
    {
        float maxTime = 0f;

        if (damageByLevel != null && damageByLevel.length > 0)
            maxTime = Mathf.Max(maxTime, damageByLevel.keys[damageByLevel.length - 1].time);

        if (rangeByLevel != null && rangeByLevel.length > 0)
            maxTime = Mathf.Max(maxTime, rangeByLevel.keys[rangeByLevel.length - 1].time);

        if (cdByLevel != null && cdByLevel.length > 0)
            maxTime = Mathf.Max(maxTime, cdByLevel.keys[cdByLevel.length - 1].time);

        return Mathf.RoundToInt(maxTime);
    }

    public float GetDamage(int level) => baseDamage * EvalOrOne(damageByLevel, level);
    public float GetRange(int level) => baseRange * EvalOrOne(rangeByLevel, level);
    public float GetCooldown(int level) => baseHitCooldown * (cdByLevel != null ? Mathf.Clamp(cdByLevel.Evaluate(level), 0.1f, 10f) : 1f);

    public float GetMaxDamage() => GetDamage(GetMaxLevel());
    public float GetMaxRange() => GetRange(GetMaxLevel());
    public float GetMaxCooldown() => GetCooldown(GetMaxLevel());

    float EvalOrOne(AnimationCurve c, int lvl)
    {
        return c != null ? Mathf.Max(0.01f, c.Evaluate(lvl)) : 1f;
    }
}
