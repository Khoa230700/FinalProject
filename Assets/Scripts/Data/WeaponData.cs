using System;
using System.Collections.Generic;

[Serializable]
public class WeaponData
{
    public List<GunSave> guns = new();
    public MeleeSave melee = new();
}

[Serializable]
public class GunSave
{
    public string gunId; //GunData.gunName 
    public int level;
    public GunSlot slot; 
}

[Serializable]
public class MeleeSave
{
    public string meleeId; //MeleeData.weaponName
    public int level;
}