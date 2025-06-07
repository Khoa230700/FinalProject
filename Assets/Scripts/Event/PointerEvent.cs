using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PointerEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Action")]
    [SerializeField] private UnityEvent enterAction;
    [SerializeField] private UnityEvent exitAction;

    public void OnPointerEnter(PointerEventData eventData)
    {
        enterAction?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        exitAction?.Invoke();
    }
}
