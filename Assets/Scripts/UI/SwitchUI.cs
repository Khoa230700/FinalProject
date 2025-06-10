using System;
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
    private string prefsKey;

    private void Start()
    {
        animator = GetComponent<Animator>();
        button = GetComponent<Button>();
        button.onClick.AddListener(AnimateSwitch);

        prefsKey = switchTag + "Switch";

        if (saveValue)
        {
            string savedValue = PlayerPrefs.GetString(prefsKey, isOn ? "true" : "false");
            bool savedState = savedValue == "true";
            SetState(savedState);
        }
        else
        {
            SetState(isOn);
        }
    }

    public void AnimateSwitch()
    {
        SetState(!isOn);
    }

    private void SetState(bool state)
    {
        isOn = state;

        if (isOn)
        {
            animator.Play("On");
            OnEvents?.Invoke();
            if (saveValue) PlayerPrefs.SetString(prefsKey, "true");
        }
        else
        {
            animator.Play("Off");
            OffEvents?.Invoke();
            if (saveValue) PlayerPrefs.SetString(prefsKey, "false");
        }
    }
}
