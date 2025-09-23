using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class EndUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject gainPanel;
    [SerializeField] private GameObject coinGroup;
    [SerializeField] private GameObject deathGroup;
    [SerializeField] private TextMeshProUGUI coinGainText;
    [SerializeField] private TextMeshProUGUI deathCountText;
    [SerializeField] private GameObject buttonsGroup;
    [SerializeField] private bool isVictory = true;

    private Sequence sequence;

    void OnEnable()
    {
        PauseGameUI.Instance.Pause();
    }

    [ContextMenu("Show Gain")]
    public void ShowGain()
    {
        SetEndMap();

        int sessionCoins = CoinManager.Instance.GetSessionCoins();
        int deathCount = SelectorSpawner.Instance.Player.GetComponent<PlayerHealthSystem>().GetDeathCount();
        int killCount = 0;

        Debug.Log($"Coins: {sessionCoins}, Deaths: {deathCount}, Kills: {killCount}");

        if (sessionCoins <= 0 && deathCount <= 0 && killCount <= 0)
        {
            gainPanel.SetActive(false);
            buttonsGroup.SetActive(true);
            return;
        }

        gainPanel.SetActive(true);
        buttonsGroup.SetActive(false);

        coinGroup.SetActive(false);
        deathGroup.SetActive(false);

        coinGainText.text = "0";
        deathCountText.text = "0";

        // Reset tween
        sequence?.Kill();
        sequence = DOTween.Sequence();
        sequence.SetUpdate(true);

        // Coin
        if (sessionCoins > 0)
        {
            int lastValue = -1;

            sequence.AppendCallback(() =>
            {
                coinGroup.SetActive(true);
            });

            sequence.Append(
                DOTween.To(() => 0, x =>
                {
                    coinGainText.text = $" {x:N0}";
                    if (x != lastValue) // chỉ phát khi số thay đổi
                    {
                        AudioManager.Instance.PlaySFX("CoinCount");
                        lastValue = x;
                    }
                },
                sessionCoins, 1.5f)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
            );
        }

        // Death
        if (deathCount >= 0)
        {
            sequence.AppendCallback(() => deathGroup.SetActive(true));
            sequence.Append(
                DOTween.To(() => 0, x =>
                {
                    deathCountText.text = $" {x}";
                },
                deathCount, 1f)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
            );
        }

        sequence.OnComplete(() =>
        {
            buttonsGroup.SetActive(true);
        })
        .Play();
    }

    public void SetEndMap()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string key = sceneName + "_Completed";

        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
    }

    public void PlayAudio()
    {
        if (isVictory) AudioManager.Instance.PlaySFX("Victory");
        else AudioManager.Instance.PlaySFX("Failed");
    }
}
