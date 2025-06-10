using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KeyBindingManager : MonoBehaviour
{
    #region Singleton
    public static KeyBindingManager Instance;
    private void Awake()
    {
        Instance = this;
    }
    #endregion

    [Header("Settings")]
    [SerializeField] private bool saveValue;

    public List<KeyBinding> bindings = new();

    private string prefsKey = "KeyBinding";

    private void Start()
    {
        LoadKey();
    }

    public void SetKey(string actionName, KeyCode keyCode, bool isPrimary)
    {
        var binding = bindings.Find(a => a.actionName == actionName);
        if (binding != null)
        {
            if (isPrimary) binding.primary = keyCode;
            else binding.secondary = keyCode;
        }
        else
        {
            if (isPrimary) bindings.Add(new KeyBinding(actionName, keyCode, KeyCode.None));
            else bindings.Add(new KeyBinding(actionName, KeyCode.None, keyCode));
        }

        if (saveValue) SaveKey();
    }

    public void SaveKey()
    {
        for (int i = 0; i < bindings.Count; i++)
        {
            var binding = bindings[i];
            PlayerPrefs.SetInt(binding.actionName + "Primary" + prefsKey, (int)binding.primary);
            PlayerPrefs.SetInt(binding.actionName + "Secondary" + prefsKey, (int)binding.secondary);
        }

        PlayerPrefs.SetInt("Count" + prefsKey, bindings.Count);
    }

    public void LoadKey()
    {
        bindings.Clear();

        int count = PlayerPrefs.GetInt("Count" + prefsKey, 0);
        
        for (int i = 0; i < count; i++)
        {
            var binding = bindings[i];
            KeyCode primary = (KeyCode)PlayerPrefs.GetInt(binding.actionName + "Primary" + prefsKey, (int)binding.primary);
            KeyCode secondary = (KeyCode)PlayerPrefs.GetInt(binding.actionName + "Secondary" + prefsKey, (int)binding.secondary);
            
            bindings.Add(new KeyBinding(binding.actionName, primary, secondary));
        }
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

    public bool GetKeyUp(string actionName)
    {
        var binding = bindings.Find(a => a.actionName == actionName);
        return Input.GetKeyUp(binding.primary) || Input.GetKeyUp(binding.secondary);
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
