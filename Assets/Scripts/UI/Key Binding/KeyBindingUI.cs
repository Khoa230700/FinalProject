 using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KeyBindingUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string actionName;
    [SerializeField] private bool isPrimary;
    [SerializeField] private bool saveValue;

    [Header("Resource")]
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private KeyBindingPopup popup;

    private string prefsKey = "KeyBinding";
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();

        LoadKey();
        UpdateText();

        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        popup?.Show(OnComplete);
    }

    private void OnComplete(KeyCode keyCode)
    {
        SetKey(keyCode);
        UpdateText();

        if (saveValue) SaveKey(keyCode);
    }

    private void SaveKey(KeyCode keyCode)
    {
        string key = actionName + (isPrimary ? "Primary" : "Secondary") + prefsKey;
        PlayerPrefs.SetInt(key, (int)keyCode);
    }

    private void LoadKey()
    {
        string key = actionName + (isPrimary ? "Primary" : "Secondary") + prefsKey;
        if (PlayerPrefs.HasKey(key))
        {
            KeyCode keyCode = (KeyCode)PlayerPrefs.GetInt(key, (int)KeyCode.None);
            KeyBindingManager.Instance.SetKey(actionName, keyCode, isPrimary);
        }
    }

    public void SetKey(KeyCode keyCode)
    {
        KeyBindingManager.Instance.SetKey(actionName, keyCode, isPrimary);
    }

    public void UpdateText()
    {
        var binding = KeyBindingManager.Instance.GetKeyBinding(actionName);
        text.text = FormatKeyCode(isPrimary ? binding.primary : binding.secondary);
    }

    private string FormatKeyCode(KeyCode keyCode)
    {
        return (keyCode == KeyCode.None) || (keyCode == KeyCode.Escape) ? "-" : keyCode.ToString().Replace("Alpha", "");
    }
}
