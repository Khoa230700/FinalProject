using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PropertyUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text textValue;
    [SerializeField] private Slider sliderValue;
    [SerializeField] private Slider sliderPreview;

    public void SetValue(float value, float maxValue = 100f, string format = "0")
    {
        if (textValue != null)
            textValue.text = value.ToString(format);

        if (sliderValue != null)
        {
            sliderValue.maxValue = maxValue;
            sliderValue.value = value;
        }

        if (sliderPreview != null)
            sliderPreview.value = 0;
    }

    public void SetPreview(float previewValue, float maxValue = 100f)
    {
        if (sliderPreview == null) return;

        sliderPreview.maxValue = maxValue;
        sliderPreview.value = previewValue;
        sliderPreview.gameObject.SetActive(true);
    }

    public void HidePreview()
    {
        if (sliderPreview != null)
            sliderPreview.gameObject.SetActive(false);
    }
}
