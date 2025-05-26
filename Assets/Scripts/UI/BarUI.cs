using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum BarDirection { Left, Right, Top, Bottom }

[ExecuteInEditMode]
public class BarUI : MonoBehaviour
{
    [Header("Values")]
    [Range(0, 100)] public float currentValue;
    public float maxValue = 100;

    [Header("Settings")]
    public bool addPrefix;
    public string prefix = "";
    public bool addSuffix;
    public string suffix = "%";
    public float delayedFillSpeed = 0.5f;
    [Range(0, 5)] public int decimals = 0;
    public BarDirection barDirection = BarDirection.Left;

    [Header("References")]
    public Image barImage;
    [SerializeField] private Image delayedBarImage;
    [SerializeField] private TextMeshProUGUI textObject;

    private Coroutine delayedRoutine;

    void Start()
    {
        SetBarDirection();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying)
            return;

        SetBarDirection();
        UpdateUI();
    }
#endif

    public void UpdateUI()
    {
        if (barImage != null) { barImage.fillAmount = currentValue / maxValue; }
        if (delayedBarImage != null && delayedRoutine == null)  delayedBarImage.fillAmount = barImage.fillAmount;
        if (textObject != null) { UpdateText(textObject); }
    }

    void UpdateText(TextMeshProUGUI txt)
    {
        string valueText = currentValue.ToString("F" + decimals);

        if (addPrefix)
            valueText = prefix + valueText;

        if (addSuffix)
            valueText += suffix;

        txt.text = valueText;
    }

    void SetBarDirection()
    {
        if (barImage != null)
        {
            barImage.type = Image.Type.Filled;

            if (barDirection == BarDirection.Left)
            {
                barImage.fillMethod = Image.FillMethod.Horizontal;
                barImage.fillOrigin = 0;
            }
            else if (barDirection == BarDirection.Right)
            {
                barImage.fillMethod = Image.FillMethod.Horizontal;
                barImage.fillOrigin = 1;
            }
            else if (barDirection == BarDirection.Top)
            {
                barImage.fillMethod = Image.FillMethod.Vertical;
                barImage.fillOrigin = 1;
            }
            else if (barDirection == BarDirection.Bottom)
            {
                barImage.fillMethod = Image.FillMethod.Vertical;
                barImage.fillOrigin = 0;
            }
        }
    }

    public void SetValue(float newValue)
    {
        if (delayedRoutine != null)
            StopCoroutine(delayedRoutine);

        currentValue = newValue;
        UpdateUI();

        if (delayedBarImage != null)
            delayedRoutine = StartCoroutine(AnimateDelayedBar(newValue));
    }

    private IEnumerator AnimateDelayedBar(float targetValue)
    {
        float startFill = delayedBarImage.fillAmount;
        float endFill = targetValue / maxValue;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * delayedFillSpeed;
            delayedBarImage.fillAmount = Mathf.Lerp(startFill, endFill, t);
            yield return null;
        }

        delayedBarImage.fillAmount = endFill;
    }
}