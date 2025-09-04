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
    public float currentValue;
    public float maxValue;

    [Header("Settings")]
    public float delayedFillSpeed = 0.5f;
    public BarDirection barDirection = BarDirection.Left;

    [Header("References")]
    public Image barImage;
    [SerializeField] private Image delayedBarImage;
    [SerializeField] private TextMeshProUGUI textObject;

    private Coroutine delayedRoutine;
    private float oldValue;

    private void Start()
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

    //* Cập nhật giao diện
    public void UpdateUI()
    {
        barImage.fillAmount = currentValue / maxValue;
        if (textObject != null) textObject.text = currentValue.ToString("F0");
    }

    //* Thiết lập hướng di chuyển của thanh 
    private void SetBarDirection()
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

    //* Thiết lập giá trị của thanh
    public void SetValue(float newValue)
    {
        if (delayedRoutine != null) StopCoroutine(delayedRoutine);

        oldValue = currentValue;
        currentValue = Mathf.Clamp(newValue, 0, maxValue);

        UpdateUI();

        if (delayedBarImage == null) return;
        if (currentValue >= oldValue || currentValue <= 0)
            delayedBarImage.fillAmount = barImage.fillAmount;
        else
            delayedRoutine = StartCoroutine(AnimateDelayedBar(currentValue));
    }

    //* Thiết lập giá trị thanh với độ trễ
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
        delayedRoutine = null;
    }
    
    public void SetMaxValue(float newMax)
    {
        maxValue = newMax;
        currentValue = newMax;
        UpdateUI();
    }
}
