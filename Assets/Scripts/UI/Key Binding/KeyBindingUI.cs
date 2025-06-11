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
    [SerializeField] private bool useNameParent = true;
    [SerializeField] private bool isPrimary;

    [Header("Resource")]
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private KeyBindingPopup popup;

    private Button button;
    private RectTransform parent;

    private void Start()
    {
        button = GetComponent<Button>();
        text ??= GetComponentInChildren<TextMeshProUGUI>();
        popup ??= FindFirstObjectByType<KeyBindingPopup>();
        parent = transform.parent.GetComponentInParent<RectTransform>();

        if (parent && useNameParent) actionName = parent.name;

        UpdateText();

        button.onClick.AddListener(OnClick);
        KeyBindingManager.Instance.OnValueChange += UpdateText;
    }

    private void OnClick()
    {
        popup?.Show(SetKey, actionName);
    }

    public void SetKey(KeyCode keyCode)
    {
        KeyBindingManager.Instance.SetKey(actionName, keyCode, isPrimary);
    }

    public void UpdateText()
    {
        var binding = KeyBindingManager.Instance.GetKeyBinding(actionName);
        text.text = binding != null ? FormatKeyCode(isPrimary ? binding.primary : binding.secondary) : "-";
    }

    private string FormatKeyCode(KeyCode keyCode)
    {
        return (keyCode == KeyCode.None) || (keyCode == KeyCode.Escape) ? "-" : keyCode.ToString().Replace("Alpha", "");
    }
}
