using UnityEngine;

public enum GunType { Pistol, Rifle, Shotgun, Sniper, SMG, LMG }
public enum GunFireMode { SemiAuto, FullAuto, Burst }

[CreateAssetMenu(fileName = "NewGunData", menuName = "Gun/Gun Data")]
public class GunData : ScriptableObject
{
    public string gunName;
    public GunType gunType;

    [Header("Stats")]
    public int damage;
    public float range;
    public float fireRate;
    public float accuracy;
    public int magazineSize;
    public int reserveAmmo;
    public float reloadTime;
    public GunFireMode fireMode;
    public float recoil;
    public float bulletSpeed;
    public float penetrationPower;
    public float weight;

    [Header("Visuals & Audio")]
    public GameObject bulletPrefab;
    public AudioClip shootSound;

    [Header("Scope")]
    public bool hasScope;
    public float scopeZoom;
}
