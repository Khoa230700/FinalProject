using System.Collections;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private Animator shopAnimator;
    [SerializeField] private GameObject canvasSetting;
    private PressKeyEvent pressKeyEvent;

    private bool isOpen = false;
    private Coroutine currentRoutine;

    private void Start()
    {
        pressKeyEvent = canvasSetting.GetComponent<PressKeyEvent>();
    }

    // public void Toggle()
    // {
    //     if (currentRoutine != null)
    //     {
    //         StopCoroutine(currentRoutine);
    //     }

    //     if (isOpen)
    //         currentRoutine = StartCoroutine(HideCoroutine());
    //     else
    //         currentRoutine = StartCoroutine(ShowCoroutine());

    //     isOpen = !isOpen;
    // }

    public void Show()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowCoroutine());
        isOpen = true;
    }

    public void Hide()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(HideCoroutine());
        isOpen = false;
    }

    private IEnumerator ShowCoroutine()
    {
        canvasSetting.SetActive(false);
        pressKeyEvent.enabled = false;

        shopAnimator.Play("In");

        yield return new WaitForSeconds(shopAnimator.GetCurrentAnimatorStateInfo(0).length);

        currentRoutine = null;
    }

    private IEnumerator HideCoroutine()
    {
        shopAnimator.Play("Out");

        yield return new WaitForSeconds(shopAnimator.GetCurrentAnimatorStateInfo(0).length);

        canvasSetting.SetActive(true);
        pressKeyEvent.enabled = true;

        currentRoutine = null;
    }

    public bool IsOpen => isOpen;
}
