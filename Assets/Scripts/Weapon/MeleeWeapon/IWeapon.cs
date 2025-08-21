using UnityEngine;

public interface IWeapon
{
    bool IsSwitchingWeapon { get; }
    void OnSelected(WeaponUI weaponUI);   // cập nhật UI/crosshair nếu cần
    void OnDeselected();

    void StartFiring();   // giữ chuột (nếu cần)
    void StopFiring();    // nhả chuột
    void FireOnce();      // click 1 phát (melee/sell semi)

    Coroutine SwitchOut(MonoBehaviour runner); // cho phép WeaponSwitcher chạy coroutine
    Coroutine SwitchIn(MonoBehaviour runner);
}
