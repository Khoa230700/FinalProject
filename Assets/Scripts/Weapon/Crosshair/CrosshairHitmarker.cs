using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CrosshairHitmarker : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup hitGroup;      // đặt trên Hitmarker root (alpha 0 mặc định)
    public float showTime = 0.08f;    // sáng nhanh
    public float fadeTime = 0.12f;    // tắt nhanh

    Coroutine _co;

    void OnEnable() => PlayerShoot.OnAnyHit += OnHit;
    void OnDisable() => PlayerShoot.OnAnyHit -= OnHit;

    void OnHit(Vector3 worldPoint)
    {
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(Flash());
    }

    IEnumerator Flash()
    {
        hitGroup.gameObject.SetActive(true);
        hitGroup.alpha = 1f;
        yield return new WaitForSeconds(showTime);

        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            hitGroup.alpha = 1f - (t / fadeTime);
            yield return null;
        }
        hitGroup.alpha = 0f;
        hitGroup.gameObject.SetActive(false);
    }
}
