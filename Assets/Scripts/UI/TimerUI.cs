using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image leftBar;
    [SerializeField] private Image rightBar;
    [SerializeField] private TextMeshProUGUI[] timeTexts;
    [SerializeField] private Animator waveUI;

    public void UpdateUI(string text, float fillAmount)
    {
        gameObject.SetActive(true);

        foreach (var txt in timeTexts)
        {
            if (txt != null)
                txt.text = text;
        }

        if (leftBar) leftBar.fillAmount = Mathf.Clamp01(fillAmount);
        if (rightBar) rightBar.fillAmount = Mathf.Clamp01(fillAmount);

        if (fillAmount <= 0f)
        {
            waveUI.Play("In");
            gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        StartCoroutine(StartWaveCountdown(10f));
    }

    IEnumerator StartWaveCountdown(float countdown)
    {
        float timer = countdown;

        while (timer > 0f)
        {
            int seconds = Mathf.CeilToInt(timer);
            UpdateUI($"NEXT WAVE IN {seconds}s", seconds / countdown);

            yield return null;
            timer -= Time.deltaTime;
        }

        UpdateUI("WAVE STARTED!", 0f);
    }
}
