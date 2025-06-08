using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string sliderTag = "Tag";
    [SerializeField] private float defaultValue = 1;
    [SerializeField] private bool saveValue;
    [SerializeField] private bool showValue = true;

    [Header("Resource")]
    [SerializeField] private TextMeshProUGUI valueText;

    private Slider slider;
    private string stringPrefs = "Slider";

    private void Start()
    {
        slider = GetComponent<Slider>();

        if (!showValue) valueText.enabled = false;
        
        slider.value = PlayerPrefs.HasKey(sliderTag + stringPrefs)
                     ? PlayerPrefs.GetFloat(sliderTag + stringPrefs)
                     : defaultValue;
                         
        UpdateValueText(slider.value);

        slider.onValueChanged.AddListener(value =>
        {
            UpdateValueText(value);
            if (saveValue)
                PlayerPrefs.SetFloat(sliderTag + stringPrefs, value);
        });
    }

    private void UpdateValueText(float value)
    {
        valueText.text = Mathf.RoundToInt(value).ToString();
    }
}
