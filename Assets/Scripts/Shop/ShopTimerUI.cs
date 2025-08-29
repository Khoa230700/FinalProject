using UnityEngine;
using TMPro;

public class ShopTimerUI : MonoBehaviour
{
    [SerializeField] private TimerUI timerUI;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private ShopUI shopUI;

    void Update()
    {
        if (timerUI == null || timerText == null || shopUI == null) return;

        float timer = timerUI.GetTimerForShop();

        if (timer <= 0f)
        {
            timerText.text = $"00:00";
            shopUI.canOpen = false;
            // if (shopUI.isOpen) shopUI.Hide(); //Test
            return;
        }

        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f);

        timerText.text = $"{minutes:00}:{seconds:00}";
        timerText.color = (timer <= 5f) ? Color.red : Color.white;

        shopUI.canOpen = true;
    }
}
