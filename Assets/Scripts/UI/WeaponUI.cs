using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct FireModeSprite
{
    public Image FireModeImage;
    public Sprite SafetyModeSprite;
    public Sprite SemiAutoModeSprite;
    public Sprite FullAutoModeSprite;
    public Sprite BurstModeSprite;

    public void SetFireModeSprite(GunFireMode mode)
    {
        FireModeImage.sprite = mode switch
        {
            GunFireMode.Safety => SafetyModeSprite,
            GunFireMode.SemiAuto => SemiAutoModeSprite,
            GunFireMode.FullAuto => FullAutoModeSprite,
            GunFireMode.Burst => BurstModeSprite,
            _ => FireModeImage.sprite
        };
    }
}

public class WeaponUI : MonoBehaviour
{
    [Header("Settings")]
    [Range(0, 100)] public float lowAmmoPercent;

    [Header("Color")]
    public Color NormalBulletColor = Color.white;
    public Color LowAmmoBulletColor = Color.red;
    public Color BulletConsumedColor = Color.black;

    [Header("References")]
    [SerializeField] private Image weaponImage;
    [SerializeField] private GridLayoutGroup BulletsGroup;
    [SerializeField] private Image BulletImage;
    [SerializeField] private FireModeSprite fireMode;
    [SerializeField] private TextMeshProUGUI storageTxt;

    private readonly List<Image> bulletImages = new();
    private PlayerShoot playerShoot;
    private GunData gunData;

    private void Start()
    {
        playerShoot = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<PlayerShoot>();
        gunData = playerShoot.gunData;

        CreateBulletUI();
    }

    //* Tạo hình ảnh các viên đạn trong giao diện người dùng
    private void CreateBulletUI()
    {
        foreach (Transform child in BulletsGroup.transform)
            Destroy(child.gameObject);

        bulletImages.Clear();

        for (int i = 0; i < gunData.magazineSize; i++)
        {
            var bullet = Instantiate(BulletImage, BulletsGroup.transform);
            bullet.color = NormalBulletColor;
            bulletImages.Add(bullet);
        }
    }

    //* Cập nhật giao diện người dùng với số lượng đạn hiện tại và tổng số đạn
    public void UpdateAmmoUI(int currentAmmo, int totalAmmo)
    {
        currentAmmo = Mathf.Clamp(currentAmmo, 0, gunData.reserveAmmo);
        storageTxt.text = totalAmmo.ToString();

        bool isLowAmmo = currentAmmo <= gunData.magazineSize * (lowAmmoPercent / 100f);

        //* Cập nhật màu sắc của các hình ảnh viên đạn dựa trên số lượng đạn hiện tại
        for (int i = 0; i < bulletImages.Count; i++)
        {
            bulletImages[i].color = i < currentAmmo
                ? (isLowAmmo ? LowAmmoBulletColor : NormalBulletColor)
                : BulletConsumedColor;
        }
    }

    public void SetFireMode(GunFireMode mode) => fireMode.SetFireModeSprite(mode);
    public void SetWeaponSprite(Sprite weaponSprite) => weaponImage.sprite = weaponSprite;
}
