using UnityEngine;
using TMPro;
using DG.Tweening;

public class EndUI : MonoBehaviour
{
    [SerializeField] private GameObject gainPanel;
    [SerializeField] private TextMeshProUGUI coinGainText;

    private Tween coinTween;
    private MeshMouseLook meshMouseLook;

    private void Start()
    {
        meshMouseLook = FindAnyObjectByType<MeshMouseLook>();

    }

    void OnEnable()
    {
        meshMouseLook.Show();
        Time.timeScale = 0f;
    }

    public void ShowGain()
    {
        gainPanel.SetActive(true);

        int sessionCoins = CoinManager.Instance.GetSessionCoins();

        coinGainText.text = "0";

        coinTween?.Kill();

        coinTween = DOTween.To(() => 0, x =>
        {
            coinGainText.text = $"$ {x:N0}";
        },
        sessionCoins, 1.5f)
        .SetEase(Ease.OutQuad)
        .SetUpdate(true);
    }
}
