using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Globalization;

public class CoinShopUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private GameObject coinChangePf;
    [SerializeField] private Transform coinChangeTf;

    private Tween coinTween;

    private void Start()
    {
        coinText.text = CoinManager.Instance.GetCoins().ToString("N0", new CultureInfo("de-DE")); ;
        CoinManager.Instance.OnCoinChanged += UpdateCoin;
    }

    private void OnDestroy()
    {
        CoinManager.Instance.OnCoinChanged -= UpdateCoin;
    }

    private void UpdateCoin(int oldValue, int newValue)
    {
        int diff = newValue - oldValue;
        if (diff == 0) return;

        //Coin Change
        var go = Instantiate(coinChangePf, coinChangeTf).GetComponent<TextMeshProUGUI>();
        go.fontSize = coinText.fontSize;

        go.text = diff > 0 ? $"+{diff}" : diff.ToString();
        go.color = diff > 0 ? new Color32(222, 226, 84, 255) : Color.red;

        go.rectTransform.DOScale(1.2f, 0.25f)
                        .SetEase(Ease.OutBack)
                        .OnComplete(() => go.rectTransform.DOScale(0.6f, 0.15f));
        go.rectTransform.DOAnchorPosY(60f, 1f);
        go.DOFade(0f, 1f)
          .OnComplete(() => Destroy(go));

        //Coin Text
        coinTween?.Kill();

        Color targetColor = diff > 0 ? new Color32(222, 226, 84, 255) : Color.red;

        coinTween = DOTween.To(() => oldValue, x =>
            {
                coinText.text = x.ToString("N0", new CultureInfo("de-DE"));
            }, newValue, 0.5f)
            .OnStart(() =>
            {
                coinText.DOColor(targetColor, 0.2f);
            })
            .OnComplete(() =>
            {
                coinText.DOColor(Color.white, 0.5f);
            });
    }
}
