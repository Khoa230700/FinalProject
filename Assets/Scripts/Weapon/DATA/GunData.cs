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
    public Sprite gunSprite;           // Hình ảnh cho UI
    public Sprite gunSpriteFullColor;           // Hình ảnh cho UI

    [Header("Ballistics")]
    public int damage;
    public float range;
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

    [Header("Magazine")]
    public int magazineSize;
    public int reserveAmmo;
    public float reloadTime;

    [Header("Scope")]
    public bool hasScope;
    [Tooltip("Field of view when scoped")]
    public float scopeZoom = 10f;

    [Header("UI & Effects")]
    public GameObject tracerPrefab;
    public AudioClip shootSound;
}
