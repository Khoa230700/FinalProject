using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundBlurUI : MonoBehaviour
{
    [Header("Settings")]
    [Range(0.0f, 10)] public float blurValue = 5.0f;
    [Range(0.1f, 50)] public float animationSpeed = 25;
    public string customProperty = "_Size";

    [Header("References")]
    public Material blurMaterial;

    private float currentBlurValue;
    private Coroutine blurInCoroutine;
    private Coroutine blurOutCoroutine;
    private bool isBlur = true;

    private void Start()
    {
        customProperty ??= "_Size";
        blurMaterial.SetFloat(customProperty, 0);
    }

    //* Blur in
    private IEnumerator BlurIn()
    {
        currentBlurValue = blurMaterial.GetFloat(customProperty);

        while (currentBlurValue <= blurValue)
        {
            currentBlurValue += Time.deltaTime * animationSpeed;

            if (currentBlurValue >= blurValue)
                currentBlurValue = blurValue;

            blurMaterial.SetFloat(customProperty, currentBlurValue);
            yield return null;
        }

        blurInCoroutine = null;
    }

    //* Blur out
    private IEnumerator BlurOut()
    {
        currentBlurValue = blurMaterial.GetFloat(customProperty);

        while (currentBlurValue >= 0)
        {
            currentBlurValue -= Time.deltaTime * animationSpeed;

            if (currentBlurValue <= 0)
                currentBlurValue = 0;

            blurMaterial.SetFloat(customProperty, currentBlurValue);
            yield return null;
        }

        blurOutCoroutine = null;
    }

    private void BlurInAnim()
    {
        if (blurOutCoroutine != null)
            StopCoroutine(blurOutCoroutine);
        blurInCoroutine = StartCoroutine(BlurIn());
        isBlur = true;
    }

    private void BlurOutAnim()
    {
        if (blurInCoroutine != null)
            StopCoroutine(blurInCoroutine);
        blurOutCoroutine = StartCoroutine(BlurOut());
        isBlur = false;
    }

    public bool ToggleBlur()
    {
        if (isBlur)
        {
            BlurInAnim();
        }
        else
        {
            BlurOutAnim();
        }

        isBlur = !isBlur;
        return !isBlur;
    }
}
