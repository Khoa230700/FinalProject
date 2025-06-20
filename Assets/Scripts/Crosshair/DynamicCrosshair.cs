using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DynamicCrosshair : MonoBehaviour
{
    [Range(0f, 256f)] public float distance = 32f;

    [Header("Crosshair Parts")]
    [SerializeField] private Image top;
    [SerializeField] private Image down;
    [SerializeField] private Image left;
    [SerializeField] private Image right;

    public void SetDistance(float distance)
    {
        left.rectTransform.anchoredPosition = new Vector2(-distance, 0f);
        right.rectTransform.anchoredPosition = new Vector2(distance, 0f);
        down.rectTransform.anchoredPosition = new Vector2(0f, -distance);
        top.rectTransform.anchoredPosition = new Vector2(0f, distance);

        this.distance = distance;
    }

    public void SetColor(Color color)
    {
        top.color = down.color = left.color = right.color = color;
    }

    private void OnValidate()
    {
        SetDistance(distance);
    }
}
