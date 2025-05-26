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

    public void SetFireModeSprite(FireMode mode)
    {
        FireModeImage.sprite = mode switch
        {
            FireMode.Safety => SafetyModeSprite,
            FireMode.SemiAuto => SemiAutoModeSprite,
            FireMode.FullAuto => FullAutoModeSprite,
            FireMode.Burst => BurstModeSprite,
            _ => FireModeImage.sprite
        };
    }
}

public class WeaponUI : MonoBehaviour
{
    [Header("Settings")]
    [Range(0, 100)] public float lowAmmoPercent = 30;

    [Header("Color")]
    public Color NormalBulletColor = Color.white;
    public Color LowAmmoBulletColor = Color.red;
    public Color BulletConsumedColor = Color.black;

    [Header("References")]
    public Image weaponImage;
    public GridLayoutGroup BulletsGroup;
    public Image BulletImage;
    public FireModeSprite fireMode;
    public TextMeshProUGUI storageTxt;
    public WeaponTest weaponTest;

    private readonly List<Image> bulletImages = new();

    private void Start()
    {
        weaponTest ??= FindFirstObjectByType<WeaponTest>();
        CreateBulletImages();
    }

    private void CreateBulletImages()
    {
        foreach (Transform child in BulletsGroup.transform)
            Destroy(child.gameObject);

        bulletImages.Clear();

        for (int i = 0; i < weaponTest.maxMagSize; i++)
        {
            var bullet = Instantiate(BulletImage, BulletsGroup.transform);
            bullet.color = NormalBulletColor;
            bulletImages.Add(bullet);
        }
    }

    public void UpdateAmmoUI(int currentAmmo, int totalAmmo)
    {
        currentAmmo = Mathf.Clamp(currentAmmo, 0, weaponTest.maxMagSize);
        storageTxt.text = totalAmmo.ToString();

        bool isLowAmmo = currentAmmo <= weaponTest.maxMagSize * (lowAmmoPercent / 100f);

        for (int i = 0; i < bulletImages.Count; i++)
        {
            bulletImages[i].color = i < currentAmmo
                ? (isLowAmmo ? LowAmmoBulletColor : NormalBulletColor)
                : BulletConsumedColor;
        }
    }

    public void SetFireMode(FireMode mode) => fireMode.SetFireModeSprite(mode);
    public void SetWeaponSprite(Sprite weaponSprite) => weaponImage.sprite = weaponSprite;
}
