using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class GunUpgradeState : MonoBehaviour
{
    public GunData gunData;
    [Min(0)] public int level = 0;  // 0 = level1 theo cách đặt curve

    [Header("Events")]
    public UnityEvent<int> OnLevelChanged;

    // ====== Wrapper: lấy chỉ số đã nâng cấp ======
    public float Damage => gunData ? gunData.GetDamage(level) : 0f;
    public float Range => gunData ? gunData.GetRange(level) : 0f;
    public float RoundsPerSecond => gunData ? gunData.GetRoundsPerSecond(level) : 0f;
    public float ReloadTime => gunData ? gunData.GetReloadTime(level) : 0f;
    public int MagazineSize => gunData ? gunData.GetMagazineSize(level) : 1;
    public float SpreadAngle => gunData ? gunData.GetSpreadAngle(level) : 0f;
    public float SemiAutoMinInterval => gunData ? gunData.GetSemiAutoMinInterval(level) : 0f;
    public float ScopeZoom => gunData ? gunData.GetScopeZoom(level) : 10f;

    public void SetLevel(int newLevel)
    {
        int clamped = Mathf.Max(0, newLevel);
        if (clamped == level) return;
        level = clamped;
        OnLevelChanged?.Invoke(level);
    }

    public void LevelUp() => SetLevel(level + 1);
}
