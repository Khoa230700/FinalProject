using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairHitMarker : MonoBehaviour
{
    [Header("Refs")]
    public PlayerShoot shooter;     // nghe sự kiện OnBulletHit
    public Image marker;            // ảnh hitmarker (dấu X / +)
    public Color normalColor = Color.white;
    public Color headshotColor = Color.red;

    [Header("Timing")]
    public float showTime = 0.12f;

    RectTransform markerRect;
    RectTransform canvasRoot;
    Coroutine showRoutine;

    void Awake()
    {
        if (!marker) marker = GetComponent<Image>();
        markerRect = marker.rectTransform;
        var canvas = marker.GetComponentInParent<Canvas>();
        if (canvas && canvas.rootCanvas) canvasRoot = canvas.rootCanvas.GetComponent<RectTransform>();
        marker.enabled = false;
    }

    void OnEnable()
    {
        if (!shooter) shooter = FindAnyObjectByType<PlayerShoot>();
        if (shooter != null) shooter.OnBulletHit += OnBulletHit; // <— event thêm trong PlayerShoot
    }

    void OnDisable()
    {
        if (shooter != null) shooter.OnBulletHit -= OnBulletHit;
    }

    void OnBulletHit(Vector3 worldPoint, bool isHeadshot)
    {
        if (!shooter || !shooter.csgoScope || !shooter.csgoScope.enabled)
        {
            // vẫn hiển thị dù không scope; không bắt buộc csgoScope
        }

        var cam = Camera.main;
        if (!cam) return;

        Vector3 screen = cam.WorldToScreenPoint(worldPoint);
        if (screen.z <= 0f) return;

        var canvas = marker.GetComponentInParent<Canvas>();
        if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            markerRect.position = screen;
        else
        {
            Camera uiCam = canvas.worldCamera;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRoot, screen, uiCam, out var lp))
                markerRect.anchoredPosition = lp;
            else
                markerRect.position = screen;
        }

        marker.color = isHeadshot ? headshotColor : normalColor;
        if (showRoutine != null) StopCoroutine(showRoutine);
        showRoutine = StartCoroutine(Flash());
    }

    IEnumerator Flash()
    {
        marker.enabled = true;
        yield return new WaitForSeconds(showTime);
        marker.enabled = false;
        showRoutine = null;
    }
}
