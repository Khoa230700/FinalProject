using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SwitchUI : MonoBehaviour
{
    [Header("Settings")]
    public string switchTag = "Tag";
    [SerializeField] private bool isOn;
    [SerializeField] private bool saveValue;

    [Header("Events")]
    [SerializeField] private UnityEvent OnEvents;
    [SerializeField] private UnityEvent OffEvents;

    private Animator animator;
    private Button button;
    private string stringPrefs = "Switch";

    private void Start()
    {
        animator ??= GetComponent<Animator>();
        button ??= GetComponent<Button>();

        button.onClick.AddListener(AnimateSwitch);

        if (saveValue)
        {
            if (PlayerPrefs.GetString(switchTag + stringPrefs) == "")
            {
                if (isOn)
                {
                    animator.Play("On");
                    isOn = true;
                    PlayerPrefs.SetString(switchTag + stringPrefs, "true");
                }
                else
                {
                    animator.Play("Off");
                    isOn = false;
                    PlayerPrefs.SetString(switchTag + stringPrefs, "false");
                }
            }
            else if (PlayerPrefs.GetString(switchTag + stringPrefs) == "true")
            {
                animator.Play("On");
                isOn = true;
            }
            else if (PlayerPrefs.GetString(switchTag + stringPrefs) == "false")
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
            if (saveValue) PlayerPrefs.SetString(switchTag + stringPrefs, "false");
        }
        else
        {
            animator.Play("On");
            isOn = true;
            OnEvents?.Invoke();
            if (saveValue) PlayerPrefs.SetString(switchTag + stringPrefs, "true");
        }
    }
}
