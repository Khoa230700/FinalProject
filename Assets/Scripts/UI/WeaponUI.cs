using System;
using System.Collections.Generic;
using DG.Tweening;
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

    [HideInInspector] public GunData gunData;

    private List<Image> bulletImages = new();
    [HideInInspector] public int lastAmmoCount = 0;

    void Awake()
    {
        DOTween.SetTweensCapacity(500, 200);
    }

    //* Tạo hình ảnh các viên đạn trong UI
    public void CreateBulletUI()
    {
        foreach (Transform child in BulletsGroup.transform)
            Destroy(child.gameObject);

        bulletImages.Clear();
        fireMode.FireModeImage.gameObject.SetActive(true);

        for (int i = 0; i < gunData.magazineSize; i++)
        {
            var bullet = Instantiate(BulletImage, BulletsGroup.transform);
            bullet.color = NormalBulletColor;
            bulletImages.Add(bullet);
        }

        // Sync ban đầu = full đạn
        lastAmmoCount = gunData.magazineSize;
    }

    //* Cập nhật UI số lượng đạn
    public void UpdateAmmoUI(int currentAmmo, int totalAmmo)
    {
        currentAmmo = Mathf.Clamp(currentAmmo, 0, gunData.magazineSize);
        storageTxt.text = totalAmmo.ToString();

        bool isLowAmmo = currentAmmo <= gunData.magazineSize * (lowAmmoPercent / 100f);

        for (int i = 0; i < bulletImages.Count; i++)
        {
            // Kill tween cũ để tránh chồng chéo
            bulletImages[i].DOKill();
            bulletImages[i].transform.DOKill();

            if (i < currentAmmo)
            {
                // Đạn còn
                bulletImages[i].DOColor(isLowAmmo ? LowAmmoBulletColor : NormalBulletColor, 0.1f);

                // Đạn mới được nạp
                if (i >= lastAmmoCount)
                {
                    bulletImages[i].transform.localScale = Vector3.one * 0.7f;

                    Sequence reloadSeq = DOTween.Sequence();
                    reloadSeq.Append(bulletImages[i].DOColor(NormalBulletColor, 0.1f));
                    reloadSeq.Append(bulletImages[i].DOColor(isLowAmmo ? LowAmmoBulletColor : NormalBulletColor, 0.2f));
                    reloadSeq.Join(bulletImages[i].transform.DOScale(1.2f, 0.15f).SetEase(Ease.OutBack));
                    reloadSeq.Append(bulletImages[i].transform.DOScale(1f, 0.1f).SetEase(Ease.InBack));
                }
                else
                {
                    bulletImages[i].transform.DOScale(1f, 0.1f);
                }
            }
            else
            {
                if (bulletImages[i].color != BulletConsumedColor)
                {
                    Sequence seq = DOTween.Sequence();
                    seq.Append(bulletImages[i].DOColor(Color.red, 0.1f));
                    seq.Append(bulletImages[i].DOColor(BulletConsumedColor, 0.2f));
                    seq.Join(bulletImages[i].transform.DOScale(0.7f, 0.2f).SetEase(Ease.InBack));
                }
            }
        }

        // Lưu lại ammo count
        lastAmmoCount = currentAmmo;

        // Hiệu ứng cho storage text
        storageTxt.transform.DOKill();
        storageTxt.transform.localScale = Vector3.one;
        storageTxt.transform
            .DOScale(1.2f, 0.15f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                storageTxt.transform.DOScale(1f, 0.15f).SetEase(Ease.InBack);
            });
    }

    public void ClearUI()
    {
        foreach (Transform child in BulletsGroup.transform)
            Destroy(child.gameObject);
        bulletImages.Clear();

        storageTxt.text = string.Empty;

        if (fireMode.FireModeImage != null)
            fireMode.FireModeImage.gameObject.SetActive(false);

        lastAmmoCount = 0;
    }

    public void SetFireMode(GunFireMode mode) => fireMode.SetFireModeSprite(mode);
    public void SetWeaponSprite(Sprite weaponSprite) => weaponImage.sprite = weaponSprite;
}
