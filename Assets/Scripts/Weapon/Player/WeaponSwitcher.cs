using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class WeaponSwitcher : MonoBehaviour
{
    public WeaponUI weaponUI;
    [SerializeField] private List<GameObject> weaponList = new List<GameObject>();

    private readonly List<IWeapon> weapons = new List<IWeapon>();
    private int currentWeaponIndex = 0;
    private bool isSwitching = false;

    public IWeapon Current => (weapons.Count > 0) ? weapons[currentWeaponIndex] : null;

    void Awake()
    {
        weapons.Clear();
        foreach (var go in weaponList)
        {
            var w = go.GetComponentInChildren<IWeapon>();
            if (w == null) Debug.LogError($"{go.name} missing IWeapon component");
            weapons.Add(w);
        }

        weaponUI = SelectorSpawner.Instance.WeaponUI;

        ActivateWeapon(currentWeaponIndex);
    }

    void Update()
    {
        if (isSwitching) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) StartCoroutine(SwitchWeaponRoutine(0));
        if (Input.GetKeyDown(KeyCode.Alpha2)) StartCoroutine(SwitchWeaponRoutine(1));
        if (Input.GetKeyDown(KeyCode.Alpha3)) StartCoroutine(SwitchWeaponRoutine(2));

        // Bắn / Đánh
        if (Input.GetMouseButtonDown(0)) Current?.FireOnce();
        if (Input.GetMouseButton(0)) Current?.StartFiring();
        if (Input.GetMouseButtonUp(0)) Current?.StopFiring();
    }

    private IEnumerator SwitchWeaponRoutine(int newIndex)
    {
        if (newIndex < 0 || newIndex >= weaponList.Count || newIndex == currentWeaponIndex)
            yield break;

        isSwitching = true;

        var prev = weapons[currentWeaponIndex];

        // Hủy reload nếu là vũ khí bắn đạn
        if (prev is IReloadable r) r.CancelReload();

        prev.OnDeselected();
        yield return prev.SwitchOut(this);

        ActivateWeapon(newIndex);
        currentWeaponIndex = newIndex;

        var cur = weapons[currentWeaponIndex];
        // OnSelected đã được gọi trong ActivateWeapon => KHÔNG gọi lại ở đây
        yield return cur.SwitchIn(this);

        isSwitching = false;
    }

    void ActivateWeapon(int index)
    {
        for (int i = 0; i < weaponList.Count; i++)
            weaponList[i].SetActive(i == index);

        weapons[index].OnSelected(weaponUI);
        // Nếu dùng crosshair riêng theo vũ khí, mỗi IWeapon nên tự set trong OnSelected.
    }
}
