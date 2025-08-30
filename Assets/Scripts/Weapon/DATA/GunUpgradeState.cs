using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class GunUpgradeState : MonoBehaviour
{
    public GunData gunData;
    [Min(0)] public int level = 0;
    public int maxLevel => gunData ? GetMaxLevel() : 0;

    [Header("Events")]
    public UnityEvent<int> OnLevelChanged;

    // Thêm property để lấy maxLevel từ gunData

    private int GetMaxLevel()
    {
        // Lấy max level từ curve có độ dài lớn nhất
        float maxTime = 0f;
        
        if (gunData.damageByLevel != null && gunData.damageByLevel.length > 0)
            maxTime = Mathf.Max(maxTime, gunData.damageByLevel.keys[gunData.damageByLevel.length - 1].time);
            
        if (gunData.rangeByLevel != null && gunData.rangeByLevel.length > 0)
            maxTime = Mathf.Max(maxTime, gunData.rangeByLevel.keys[gunData.rangeByLevel.length - 1].time);
            
        if (gunData.rofByLevel != null && gunData.rofByLevel.length > 0)
            maxTime = Mathf.Max(maxTime, gunData.rofByLevel.keys[gunData.rofByLevel.length - 1].time);
            
        if (gunData.reloadByLevel != null && gunData.reloadByLevel.length > 0)
            maxTime = Mathf.Max(maxTime, gunData.reloadByLevel.keys[gunData.reloadByLevel.length - 1].time);
            
        if (gunData.magazineByLevel != null && gunData.magazineByLevel.length > 0)
            maxTime = Mathf.Max(maxTime, gunData.magazineByLevel.keys[gunData.magazineByLevel.length - 1].time);

        return Mathf.RoundToInt(maxTime);
    }

    // Wrapper properties không thay đổi
    public float Damage => gunData ? gunData.GetDamage(level) : 0f;
    public float Range => gunData ? gunData.GetRange(level) : 0f;
    public float RoundsPerSecond => gunData ? gunData.GetRoundsPerSecond(level) : 0f;
    public float ReloadTime => gunData ? gunData.GetReloadTime(level) : 0f;
    public int MagazineSize => gunData ? gunData.GetMagazineSize(level) : 1;
    public float SpreadAngle => gunData ? gunData.GetSpreadAngle(level) : 0f;
    public float SemiAutoMinInterval => gunData ? gunData.GetSemiAutoMinInterval(level) : 0f;
    public float ScopeZoom => gunData ? gunData.GetScopeZoom(level) : 10f;

    // Max value properties - sử dụng MaxLevel thay vì maxLevel
    public float MaxDamage => gunData ? gunData.GetMaxDamage(maxLevel) : 0f;
    public float MaxRange => gunData ? gunData.GetMaxRange(maxLevel) : 0f;
    public float MaxRoundsPerSecond => gunData ? gunData.GetMaxRoundsPerSecond(maxLevel) : 0f;
    public float MaxReloadTime => gunData ? gunData.GetMaxReloadTime(maxLevel) : 0f;
    public int MaxMagazineSize => gunData ? gunData.GetMaxMagazineSize(maxLevel) : 1;
    public float MaxSpreadAngle => gunData ? gunData.GetMaxSpreadAngle(maxLevel) : 0f;
    public float MaxSemiAutoMinInterval => gunData ? gunData.GetMaxSemiAutoMinInterval(maxLevel) : 0f;
    public float MaxScopeZoom => gunData ? gunData.GetMaxScopeZoom(maxLevel) : 10f;

    public void SetLevel(int newLevel)
    {
        int clamped = Mathf.Clamp(newLevel, 0, maxLevel); // Clamp với MaxLevel
        if (clamped == level) return;
        level = clamped;
        OnLevelChanged?.Invoke(level);
    }

    public void LevelUp() => SetLevel(level + 1);
}
