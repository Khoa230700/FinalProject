using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PointerEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Events")]
    [SerializeField] private UnityEvent enterEvent;
    [SerializeField] private UnityEvent exitEvent;

    public void OnPointerEnter(PointerEventData eventData)
    {
        enterEvent?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        exitEvent?.Invoke();
    }
}
