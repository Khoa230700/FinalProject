using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PointerEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Events")]
    [SerializeField] private UnityEvent enterEvent;
    [SerializeField] private UnityEvent exitEvent;
    [SerializeField] private UnityEvent clickEvent;

    public void OnPointerEnter(PointerEventData eventData)
    {
        enterEvent?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        exitEvent?.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        clickEvent?.Invoke();
    }
}
