using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SoundFXUI : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [SerializeField] private bool isSoundOnHover = true;
    [SerializeField] private bool isSoundOnClick = true;
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSoundOnHover) return;

        AudioManager.Instance.PlaySFX("Hover");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isSoundOnClick) return;

        AudioManager.Instance.PlaySFX("Click");
    }
}
