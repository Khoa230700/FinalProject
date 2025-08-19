using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class DamagedFader
{
    public bool Fading { get; private set; }

    [SerializeField] private Image image;
    [SerializeField][Range(0f, 1f)] private float minAlpha = 0.4f;
    [SerializeField] private float fadeInSpeed = 25f;
    [SerializeField] private float fadeOutSpeed = 0.3f;
    [SerializeField] private float fadeOutPause = 0.5f;

    private Coroutine fadeCoroutine;

    //* Hiện hình ảnh dựa trên sát thương nhận được
    public void DoFadeCycle(MonoBehaviour parent, float targetAlpha)
    {
        targetAlpha = Mathf.Clamp01(Mathf.Max(Mathf.Abs(targetAlpha), minAlpha));

        if (fadeCoroutine != null)
            parent.StopCoroutine(fadeCoroutine);

        fadeCoroutine = parent.StartCoroutine(FadeRoutine(targetAlpha));
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        Fading = true;

        //* Fade In
        while (Mathf.Abs(image.color.a - targetAlpha) > 0.01f)
        {
            Color c = image.color;
            c.a = Mathf.Lerp(c.a, targetAlpha, fadeInSpeed * Time.deltaTime);
            image.color = c;
            yield return null;
        }

        //* Pause
        yield return new WaitForSeconds(fadeOutPause);

        //* Fade Out
        while (image.color.a > 0.01f)
        {
            Color c = image.color;
            c.a = Mathf.Lerp(c.a, 0f, fadeOutSpeed * Time.deltaTime);
            image.color = c;
            yield return null;
        }

        //* Đặt alpha về 0 và kết thúc quá trình hiệnhiện
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);
        Fading = false;
    }
}
