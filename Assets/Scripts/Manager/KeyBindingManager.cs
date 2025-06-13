using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KeyBindingManager : MonoBehaviour
{
    public static KeyBindingManager Instance;


    [Header("Settings")]
    [SerializeField] private bool saveValue;

    public List<KeyBinding> bindings = new();
    public event Action OnValueChange;

    private string prefsKey = "KeyBinding";
    private int count;

    private void Awake()
    {
        Instance = this;
        LoadKey();
    }

    public void SetKey(string actionName, KeyCode keyCode, bool isPrimary)
    {
        foreach (var binding in bindings)
        {
            if (binding.primary == keyCode)
                binding.primary = KeyCode.None;

            if (binding.secondary == keyCode)
                binding.secondary = KeyCode.None;
        }

        var currentBinding = bindings.Find(a => a.actionName == actionName);
        if (currentBinding != null)
        {
            if (isPrimary)
                currentBinding.primary = keyCode;
            else
                currentBinding.secondary = keyCode;
        }
        else
        {
            if (isPrimary)
                bindings.Add(new KeyBinding(actionName, keyCode, KeyCode.None));
            else
                bindings.Add(new KeyBinding(actionName, KeyCode.None, keyCode));
        }

        if (saveValue) SaveKey();

        OnValueChange?.Invoke();
    }

    public void SaveKey()
    {
        PlayerPrefs.SetInt("Count" + prefsKey, bindings.Count);

        for (int i = 0; i < bindings.Count; i++)
        {
            var binding = bindings[i];

            PlayerPrefs.SetString($"{i}_Action" + prefsKey, binding.actionName);
            PlayerPrefs.SetInt($"{i}_Primary" + prefsKey, (int)binding.primary);
            PlayerPrefs.SetInt($"{i}_Secondary" + prefsKey, (int)binding.secondary);
        }

        PlayerPrefs.Save();
    }

    public void LoadKey()
    {
        bindings.Clear();

        count = PlayerPrefs.GetInt("Count" + prefsKey, 0);

        for (int i = 0; i < count; i++)
        {
            string actionName = PlayerPrefs.GetString($"{i}_Action" + prefsKey, $"Action{i}");

            KeyCode primary = (KeyCode)PlayerPrefs.GetInt($"{i}_Primary" + prefsKey, (int)KeyCode.None);
            KeyCode secondary = (KeyCode)PlayerPrefs.GetInt($"{i}_Secondary" + prefsKey, (int)KeyCode.None);

            bindings.Add(new KeyBinding(actionName, primary, secondary));
        }
    }

    public bool GetKeyUp(string actionName)
    {
        var binding = bindings.Find(a => a.actionName == actionName);
        return Input.GetKeyUp(binding.primary) || Input.GetKeyUp(binding.secondary);
    }

    public bool GetKeyDown(string actionName)
    {
        var binding = bindings.Find(a => a.actionName == actionName);
        return Input.GetKeyDown(binding.primary) || Input.GetKeyDown(binding.secondary);
    }

    public bool GetKey(string actionName)
    {
        var binding = bindings.Find(a => a.actionName == actionName);
        return Input.GetKey(binding.primary) || Input.GetKey(binding.secondary);
    }

    public float GetAxis(string axisName)
    {
        switch (axisName)
        {
            case "Horizontal":
                if (GetKey("Move Left")) return -1f;
                if (GetKey("Move Right")) return 1f;
                return 0f;
            case "Vertical":
                if (GetKey("Move Backward")) return -1f;
                if (GetKey("Move Forward")) return 1f;
                return 0f;
            default:
                return 0f;
        }
    }

    public KeyBinding GetKeyBinding(string actionName) => bindings.Find(a => a.actionName == actionName);
}

[Serializable]
public class KeyBinding
{
    public string actionName;
    public KeyCode primary;
    public KeyCode secondary;

    public KeyBinding(string actionName, KeyCode primary, KeyCode secondary)
    {
        this.actionName = actionName;
        this.primary = primary;
        this.secondary = secondary;
    }
}
