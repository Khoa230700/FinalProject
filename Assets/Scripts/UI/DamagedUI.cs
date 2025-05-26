using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamagedUI : MonoBehaviour
{
    [Header("Blood Screen")]
    [SerializeField] private ImageFader bloodScreenFader;

    [Header("Damage Indicator")]
    [SerializeField] private RectTransform damageIndicator;
    [SerializeField] private ImageFader damageIndicatorFader;
    [SerializeField] private float indicatorDistance = 128f;

    private Vector3 lastHitPoint;
    public Transform player;

    private void Start()
    {
        player ??= GameObject.FindGameObjectWithTag("Player")?.transform;
        Health health = player.GetComponent<Health>();

        if (health != null)
            health.OnTakeDamage += OnTakeDamage;
    }

    private void OnTakeDamage(float delta, Vector3 hitPoint)
    {
        if (delta >= 0f) return;

        if (bloodScreenFader != null)
            bloodScreenFader.DoFadeCycle(this, Mathf.Abs(delta) / 100f);

        if (hitPoint != Vector3.zero && damageIndicatorFader != null)
        {
            lastHitPoint = hitPoint;
            damageIndicatorFader.DoFadeCycle(this, 1f);
        }
    }

    private void Update()
    {
        if (damageIndicator == null || !damageIndicatorFader.Fading)
            return;

        Vector3 lookDir = Vector3.ProjectOnPlane(player.forward, Vector3.up).normalized;
        Vector3 dirToHit = Vector3.ProjectOnPlane(lastHitPoint - player.position, Vector3.up).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, lookDir);

        float angle = Vector3.Angle(lookDir, dirToHit) * Mathf.Sign(Vector3.Dot(right, dirToHit));

        damageIndicator.localEulerAngles = Vector3.forward * angle;
        damageIndicator.localPosition = damageIndicator.up * indicatorDistance;
    }

    [Serializable]
    public class ImageFader
    {
        public bool Fading { get; private set; }

        [SerializeField] private Image image;
        [SerializeField][Range(0f, 1f)] private float minAlpha = 0.4f;
        [SerializeField] private float fadeInSpeed = 25f;
        [SerializeField] private float fadeOutSpeed = 0.3f;
        [SerializeField] private float fadeOutPause = 0.5f;

        private Coroutine fadeCoroutine;

        public void DoFadeCycle(MonoBehaviour parent, float targetAlpha)
        {
            if (image == null) return;

            targetAlpha = Mathf.Clamp01(Mathf.Max(Mathf.Abs(targetAlpha), minAlpha));

            if (fadeCoroutine != null)
                parent.StopCoroutine(fadeCoroutine);

            fadeCoroutine = parent.StartCoroutine(FadeRoutine(targetAlpha));
        }

        private IEnumerator FadeRoutine(float targetAlpha)
        {
            Fading = true;

            while (Mathf.Abs(image.color.a - targetAlpha) > 0.01f)
            {
                Color c = image.color;
                c.a = Mathf.Lerp(c.a, targetAlpha, fadeInSpeed * Time.deltaTime);
                image.color = c;
                yield return null;
            }

            yield return new WaitForSeconds(fadeOutPause);

            while (image.color.a > 0.01f)
            {
                Color c = image.color;
                c.a = Mathf.Lerp(c.a, 0f, fadeOutSpeed * Time.deltaTime);
                image.color = c;
                yield return null;
            }

            image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);
            Fading = false;
        }
    }
}
