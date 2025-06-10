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

    [Header("Resource")]
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private KeyBindingPopup popup;

    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();

        UpdateText();

        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        popup?.Show(SetKey);
    }

    public void SetKey(KeyCode keyCode)
    {
        KeyBindingManager.Instance.SetKey(actionName, keyCode, isPrimary);
        UpdateText();
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
