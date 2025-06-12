using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct FireModeSprite
{
    public Image FireModeImage;
    public Sprite safetyModeSprite;
    public Sprite semiAutoModeSprite;
    public Sprite fullAutoModeSprite;
    public Sprite burstModeSprite;

    public void SetFireModeSprite(GunFireMode mode)
    {
        FireModeImage.sprite = mode switch
        {
            GunFireMode.Safety => safetyModeSprite,
            GunFireMode.SemiAuto => semiAutoModeSprite,
            GunFireMode.FullAuto => fullAutoModeSprite,
            GunFireMode.Burst => burstModeSprite,
            _ => FireModeImage.sprite
        };
    }
}

public class WeaponUI : MonoBehaviour
{
    [Header("Settings")]
    [Range(0, 100)] public float lowAmmoPercent;

    [Header("Color")]
    public Color normalBulletColor = Color.white;
    public Color lowAmmoBulletColor = Color.red;
    public Color bulletConsumedColor = Color.black;

    [Header("References")]
    [SerializeField] private Image weaponImage;
    [SerializeField] private GridLayoutGroup bulletsGroup;
    [SerializeField] private Image pfBulletImage;
    [SerializeField] private FireModeSprite fireMode;
    [SerializeField] private TextMeshProUGUI storageTxt;

    [HideInInspector] public GunData gunData;
    [SerializeField] private List<Image> bulletImages = new();

    //* Tạo hình ảnh các viên đạn trong giao diện người dùng
    public void CreateBulletUI()
    {
        foreach (Transform child in bulletsGroup.transform)
            Destroy(child.gameObject);

        bulletImages.Clear();

        for (int i = 0; i < gunData.magazineSize; i++)
        {
            var bullet = Instantiate(pfBulletImage, bulletsGroup.transform);
            bullet.color = normalBulletColor;
            bulletImages.Add(bullet);
        }
    }

    //* Cập nhật giao diện người dùng với số lượng đạn hiện tại và tổng số đạn
    public void UpdateAmmoUI(int currentAmmo, int totalAmmo)
    {
        currentAmmo = Mathf.Clamp(currentAmmo, 0, totalAmmo);
        storageTxt.text = totalAmmo.ToString();

        bool isLowAmmo = currentAmmo <= gunData.magazineSize * (lowAmmoPercent / 100f);

        //* Cập nhật màu sắc của các hình ảnh viên đạn dựa trên số lượng đạn hiện tại
        for (int i = 0; i < bulletImages.Count; i++)
        {
            bulletImages[i].color = i < currentAmmo
                ? (isLowAmmo ? lowAmmoBulletColor : normalBulletColor)
                : bulletConsumedColor;
        }
    }

    public void SetFireMode(GunFireMode mode) => fireMode.SetFireModeSprite(mode);
    public void SetWeaponSprite(Sprite weaponSprite) => weaponImage.sprite = weaponSprite;
}
