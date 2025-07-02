using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PanelButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Animator buttonAnimator;

    private void Start()
    {
        buttonAnimator = GetComponent<Animator>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Normal to Pressed"))
            buttonAnimator.Play("Dissolve to Normal");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Normal to Pressed"))
            buttonAnimator.Play("Normal to Dissolve");
    }
}
