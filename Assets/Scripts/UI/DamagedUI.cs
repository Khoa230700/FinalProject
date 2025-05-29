using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamagedUI : MonoBehaviour
{
    [Header("Blood Screen")]
    [SerializeField] private ImageFader bloodScreenFader;

    [Header("Indicator")]
    [SerializeField] private RectTransform damageIndicator;
    [SerializeField] private ImageFader damageIndicatorFader;
    [SerializeField] private float indicatorDistance = 128f;

    private Transform player;
    private Vector3 lastHitPoint;
    private Health playerHealth;

    private void Start()
    {
        player ??= GameObject.FindGameObjectWithTag("Player").transform;
        playerHealth = player.GetComponent<Health>();
        playerHealth?.OnTakeDamage.AddListener(OnTakeDamage);
    }

    private void OnDestroy()
    {
        playerHealth?.OnTakeDamage.RemoveListener(OnTakeDamage);
    }

    //* Goi khi nhân vật bị thương
    private void OnTakeDamage(float delta, Vector3 hitPoint)
    {
        if (delta >= 0f) return;  //* Chỉ xử lý khi nhận sát thương

        bloodScreenFader.DoFadeCycle(this, Mathf.Abs(delta) / 200f); //* Tổng lượng chịu đựng (giáp + máu) là 200

        if (hitPoint != Vector3.zero)
        {
            lastHitPoint = hitPoint;
            damageIndicatorFader.DoFadeCycle(this, 1f); //* Hiện thị chỉ báo sát thương với alpha = 1
        }
    }

    private void Update()
    {
        if (!damageIndicatorFader.Fading) return;

        Vector3 lookDir = Vector3.ProjectOnPlane(player.forward, Vector3.up).normalized; //* Hướng nhìn của người chơi
        Vector3 dirToHit = Vector3.ProjectOnPlane(lastHitPoint - player.position, Vector3.up).normalized; //* Hướng từ nhân vật đến điểm va chạm
        Vector3 right = Vector3.Cross(Vector3.up, lookDir); //* Phương ngang của nhân vật, (bên phải)

        float angle = Vector3.Angle(lookDir, dirToHit) * Mathf.Sign(Vector3.Dot(right, dirToHit)); //* Góc giữa hướng nhìn và hướng đến điểm va chạm, với dấu hiệu để xác định bên trái hay phải

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
}
