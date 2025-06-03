using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairManagerUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject staticCrosshair;
    [SerializeField] private DynamicCrosshair dynamicCrosshair;

    public void SetStaticCrosshair(bool active)
    {
        staticCrosshair.SetActive(active);
        dynamicCrosshair.gameObject.SetActive(!active);
    }

    public void SetDynamicCrosshair(bool active)
    {
        staticCrosshair.SetActive(!active);
        dynamicCrosshair.gameObject.SetActive(active);
    }
}
