using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PropertyUI : MonoBehaviour
{
    [SerializeField] private TMP_Text textValue;
    [SerializeField] private Slider sliderValue;

    public void SetValue(float value, float maxValue = 100f, string format = "0")
    {
        if (textValue != null)
        {
            textValue.text = value.ToString(format);
        }

        if (sliderValue != null)
        {
            sliderValue.maxValue = maxValue;
            sliderValue.value = value;
        }
    }
}
