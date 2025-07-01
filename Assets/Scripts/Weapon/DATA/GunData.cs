using UnityEngine;

public enum GunType { Pistol, Rifle, Shotgun, Sniper, SMG, LMG }
public enum GunFireMode { SemiAuto, FullAuto, Burst, Safety }

[CreateAssetMenu(fileName = "NewGunData", menuName = "Gun/Gun Data")]
public class GunData : ScriptableObject
{
    public string gunName;
    public GunType gunType;

    [Header("Stats")]
    public int damage;
    public float range;

    [Tooltip("Số viên/giây. Ví dụ: 5 = bắn 5 viên mỗi giây")]
    public float roundsPerSecond = 5f;

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
    public Sprite gunSprite;
    public GameObject bulletPrefab;
    public AudioClip shootSound;
    public GameObject tracerPrefab;

    [Header("Scope")]
    public bool hasScope;
    public float scopeZoom;

    [Header("Crosshair")]
    public CrosshairData crosshairData;
}