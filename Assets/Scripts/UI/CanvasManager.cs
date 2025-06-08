using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class CanvasManager : MonoBehaviour
{
    [Header("Resources")]
    [SerializeField] private CanvasScaler canvasScaler;

    void Start()
    {
        canvasScaler ??= GetComponent<CanvasScaler>();
    }

    public void ScaleCanvas(int scale = 1080)
    {
        canvasScaler.referenceResolution = new Vector2(canvasScaler.referenceResolution.x, scale);
    }


    public void EnableHUD(bool enable)
    {
        gameObject.SetActive(enable);
    }
}
