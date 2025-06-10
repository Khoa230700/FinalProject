using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyBindingPopup : MonoBehaviour
{
    private Animator animator;
    private bool isListen;
    private Action<KeyCode> OnComplete;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (isListen)
        {
            foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keyCode))
                {
                    if (keyCode == KeyCode.Escape)
                    {
                        Hide();
                        return;
                    }

                    Complete(keyCode);
                    return;
                }
            }
        }
    }

    public void Show(Action<KeyCode> callback)
    {
        OnComplete = callback;

        animator.Play("In");
        isListen = true;
    }

    public void Hide()
    {
        isListen = false;
        animator.Play("Out");
    }

    private void Complete(KeyCode keyCode)
    {
        Hide();
        OnComplete?.Invoke(keyCode);
    }
}
