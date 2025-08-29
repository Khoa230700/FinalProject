using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PropertyUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text textValue;
    [SerializeField] private Slider sliderValue;
    [SerializeField] private Slider sliderPreview;

    private string originalText;

    public void SetValue(float value, float maxValue = 100f, string format = "0")
    {
        if (textValue != null)
        {
            textValue.text = value.ToString(format);
            originalText = textValue.text;
        }

        if (sliderValue != null)
        {
            sliderValue.maxValue = maxValue;
            sliderValue.value = value;
        }

        HidePreview();
    }

    public void SetPreview(float previewValue, float maxValue = 100f, string format = "0")
    {
        if (sliderPreview == null) return;

        originalText = textValue.text;

        sliderPreview.maxValue = maxValue;
        sliderPreview.value = previewValue;

        if (textValue != null)
        {
            textValue.text = previewValue.ToString(format);
            textValue.color = Color.yellow; // Highlight preview value
        }

        sliderPreview.gameObject.SetActive(true);
    }

    public void HidePreview()
    {
        sliderPreview.value = 0;
        textValue.text = originalText;
        textValue.color = Color.white;
    }
}
