using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Globalization;

public class CoinUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private GameObject coinChangePf;
    [SerializeField] private Transform coinChangeTf;

    private Tween coinTween;

    private void Start()
    {
        coinText.text = CoinManager.Instance.GetCoins().ToString("N0", new CultureInfo("de-DE"));
        CoinManager.Instance.OnCoinChanged += UpdateCoin;
    }

    private void OnDestroy()
    {
        CoinManager.Instance.OnCoinChanged -= UpdateCoin;
    }

    private void UpdateCoin(int oldValue, int newValue)
    {
        Canvas.ForceUpdateCanvases();
        int diff = newValue - oldValue;
        if (diff == 0) return;

        var go = Instantiate(coinChangePf, coinChangeTf).GetComponent<TextMeshProUGUI>();
        go.fontSize = coinText.fontSize;

        go.text = diff > 0 ? $"+{diff}" : diff.ToString();
        go.color = diff > 0 ? new Color32(222, 226, 84, 255) : Color.red;

        //Effect
        coinTween?.Kill();
        coinText.DOFade(0f, 0.2f).OnComplete(() =>
        {
            go.DOFade(1f, 0.8f).OnComplete(() =>
                    {
                        coinText.DOFade(1f, 0.4f);
                        coinText.text = newValue.ToString("N0", new CultureInfo("de-DE"));
                        Destroy(go);
                    });
        });
    }
}
