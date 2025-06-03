using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMangerUI : MonoBehaviour
{
    [Header("Keys")]
    public KeyCode blurUIKey = KeyCode.Escape;

    [Header("References")]
    [SerializeField] private BackgroundBlurUI backgroundBlurUI;
    [SerializeField] private Animator pauseAnimator;

    private void Update()
    {
        if (Input.GetKeyDown(blurUIKey))
        {
            bool isBlur = backgroundBlurUI.ToggleBlur();
            Debug.Log(isBlur);
            if (isBlur)
            {
                pauseAnimator.Play("Window In");
            }
            else
            {
                pauseAnimator.Play("Window Out");
            }
        }
    }
}
