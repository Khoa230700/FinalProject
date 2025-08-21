using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TimerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image leftBar;
    [SerializeField] private Image rightBar;
    [SerializeField] private TextMeshProUGUI[] timeTexts;
    [SerializeField] private Animator waveUI;

    private int timerForShop = 0;
    private int lastSecondPlayed = -1;

    private void Awake()
    {
        // Ẩn UI ngay khi vào game để wave 1 không bị hiện
        HideUI();
        gameObject.SetActive(false);
    }

    public void UpdateUI(string text, float fillAmount, int seconds = 0)
    {
        timerForShop = seconds;
        if (seconds <= 5f && seconds != lastSecondPlayed)
        {
            lastSecondPlayed = seconds;
            AudioManager.Instance.PlaySFX("Tick");
        }

        foreach (var txt in timeTexts)
        {
            if (txt != null) txt.text = text;
        }

        if (leftBar) leftBar.fillAmount = Mathf.Clamp01(fillAmount);
        if (rightBar) rightBar.fillAmount = Mathf.Clamp01(fillAmount);

        if (fillAmount <= 0f && waveUI != null)
        {
            waveUI.Play("In"); // hiệu ứng khi bắt đầu wave
        }
    }

    public void HideUI()
    {
        foreach (var txt in timeTexts)
        {
            if (txt != null) txt.text = "";
        }
        if (leftBar) leftBar.fillAmount = 0f;
        if (rightBar) rightBar.fillAmount = 0f;
    }

    // tiện dụng để WaveManager bật/tắt khối UI
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
    
    public int GetTimerForShop() => timerForShop;
}

// private void Start()
// {
//     StartCoroutine(StartWaveCountdown(10f));
// }

// IEnumerator StartWaveCountdown(float countdown)
// {
//     float timer = countdown;

//     while (timer > 0f)
//     {
//         int seconds = Mathf.CeilToInt(timer);
//         UpdateUI($"NEXT WAVE IN {seconds}s", seconds / countdown, seconds);

//         yield return null;
//         timer -= Time.deltaTime;
//     }

//     UpdateUI("WAVE STARTED!", 0f);
// }
