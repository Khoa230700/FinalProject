using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SwitchUI : MonoBehaviour
{
    [Header("Settings")]
    public string switchTag = "Switch";
    [SerializeField] private bool isOn;
    [SerializeField] private bool saveValue;

    [Header("Events")]
    [SerializeField] private UnityEvent OnEvents;
    [SerializeField] private UnityEvent OffEvents;

    private Animator animator;
    private Button button;

    private void Start()
    {
        animator ??= GetComponent<Animator>();
        button ??= GetComponent<Button>();

        button.onClick.AddListener(AnimateSwitch);

        if (saveValue)
        {
            if (PlayerPrefs.GetString(switchTag + "Switch") == "")
            {
                if (isOn)
                {
                    animator.Play("On");
                    isOn = true;
                    PlayerPrefs.SetString(switchTag + "Switch", "true");
                }
                else
                {
                    animator.Play("Off");
                    isOn = false;
                    PlayerPrefs.SetString(switchTag + "Switch", "false");
                }
            }
            else if (PlayerPrefs.GetString(switchTag + "Switch") == "true")
            {
                animator.Play("On");
                isOn = true;
            }
            else if (PlayerPrefs.GetString(switchTag + "Switch") == "false")
            {
                animator.Play("Off");
                isOn = false;
            }
        }
        else
        {
            if (isOn)
            {
                animator.Play("On");
                isOn = true;
            }
            else
            {
                animator.Play("Off");
                isOn = false;
            }
        }
    }

    public void AnimateSwitch()
    {
        if (isOn)
        {
            animator.Play("Off");
            isOn = false;
            OffEvents?.Invoke();
            if (saveValue) PlayerPrefs.SetString(switchTag + "Switch", "false");
        }
        else
        {
            animator.Play("On");
            isOn = true;
            OnEvents?.Invoke();
            if (saveValue) PlayerPrefs.SetString(switchTag + "Switch", "true");
        }
    }
}
