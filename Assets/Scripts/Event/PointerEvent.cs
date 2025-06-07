using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PointerEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Events")]
    [SerializeField] private UnityEvent enterEvent;
    [SerializeField] private UnityEvent enteEvent;

    public void OnPointerEnter(PointerEventData eventData)
    {
        enterEvent?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        enteEvent?.Invoke();
    }
}
