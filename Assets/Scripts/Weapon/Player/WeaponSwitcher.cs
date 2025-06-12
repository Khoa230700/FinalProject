using UnityEngine;
using System.Collections.Generic;

public class WeaponSwitcher : MonoBehaviour
{
    [SerializeField] private List<GameObject> weaponList = new List<GameObject>();

    private int currentWeaponIndex = 0;

    void Start()
    {
        ActivateWeapon(currentWeaponIndex);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchWeapon(2);
    }

    void SwitchWeapon(int index)
    {
        if (index >= weaponList.Count || index == currentWeaponIndex) return;

        for (int i = 0; i < weaponList.Count; i++)
        {
            weaponList[i].SetActive(i == index);
        }

        currentWeaponIndex = index;
    }

    void ActivateWeapon(int index)
    {
        for (int i = 0; i < weaponList.Count; i++)
        {
            weaponList[i].SetActive(i == index);
        }
    }
}
