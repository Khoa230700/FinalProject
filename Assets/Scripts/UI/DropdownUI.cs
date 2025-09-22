using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class DropdownUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string selectorTag = "Tag"; // Resolutions, RefreshRate, FPS, WindowMode, Quality, Shadows...
    [SerializeField] private bool saveValue;

    [Header("Dropdown")]
    [SerializeField] private TMP_Dropdown dropdown;

    [Header("Items")]
    [SerializeField] private List<ItemSelector> itemSelectors = new();

    private string prefsKey;
    private int index = 0;

    private void Start()
    {
        prefsKey = selectorTag + "DropdownSelector";
        itemSelectors.Clear();

        if (selectorTag == "Resolutions")
        {
            Resolution[] resolutions = Screen.resolutions;

            foreach (var res in resolutions)
            {
                string title = $"{res.width}x{res.height}";
                Resolution currentRes = res;

                var selector = new ItemSelector { title = title };
                selector.OnValueChange.AddListener(() =>
                {
                    Screen.SetResolution(currentRes.width, currentRes.height, Screen.fullScreen, currentRes.refreshRate);
                });

                itemSelectors.Add(selector);
            }

            Resolution current = Screen.currentResolution;
            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutions[i].width == current.width &&
                    resolutions[i].height == current.height)
                {
                    index = i;
                    break;
                }
            }
        }
        else if (selectorTag == "RefreshRate")
        {
            Resolution[] resolutions = Screen.resolutions;
            int width = Screen.currentResolution.width;
            int height = Screen.currentResolution.height;

            foreach (var res in resolutions)
            {
                if (res.width == width && res.height == height)
                {
                    string title = $"{res.refreshRate} Hz";
                    Resolution currentRes = res;

                    var selector = new ItemSelector { title = title };
                    selector.OnValueChange.AddListener(() =>
                    {
                        Screen.SetResolution(width, height, Screen.fullScreen, currentRes.refreshRate);
                    });

                    itemSelectors.Add(selector);
                }
            }

            int currentRate = Screen.currentResolution.refreshRate;
            for (int i = 0; i < itemSelectors.Count; i++)
            {
                if (itemSelectors[i].title.Contains($"{currentRate}"))
                {
                    index = i;
                    break;
                }
            }
        }
        else if (selectorTag == "FPS")
        {
            int[] fpsOptions = { 30, 60, 120, 144, 240, -1 }; // -1 = VSync / Unlimited
            foreach (int fps in fpsOptions)
            {
                string title = (fps == -1) ? "Unlimited" : $"{fps} FPS";
                int cap = fps;

                var selector = new ItemSelector { title = title };
                selector.OnValueChange.AddListener(() =>
                {
                    Application.targetFrameRate = cap;
                });

                itemSelectors.Add(selector);
            }

            index = Array.IndexOf(fpsOptions, Application.targetFrameRate);
            if (index < 0) index = 0;
        }
        else if (selectorTag == "WindowMode")
        {
            string[] modes = { "Fullscreen", "Borderless", "Windowed" };
            foreach (string mode in modes)
            {
                string title = mode;
                string currentMode = mode;

                var selector = new ItemSelector { title = title };
                selector.OnValueChange.AddListener(() =>
                {
                    if (currentMode == "Fullscreen")
                        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                    else if (currentMode == "Borderless")
                        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                    else
                        Screen.fullScreenMode = FullScreenMode.Windowed;
                });

                itemSelectors.Add(selector);
            }

            index = (int)Screen.fullScreenMode;
            if (index >= itemSelectors.Count) index = 0;
        }
        else if (selectorTag == "Shadows")
        {
            string[] shadowOptions = { "Disable", "Hard Only", "All" };
            for (int i = 0; i < shadowOptions.Length; i++)
            {
                int shadowIndex = i;
                var selector = new ItemSelector { title = shadowOptions[i] };
                selector.OnValueChange.AddListener(() =>
                {
                    if (shadowIndex == 0)
                        QualitySettings.shadows = ShadowQuality.Disable;
                    else if (shadowIndex == 1)
                        QualitySettings.shadows = ShadowQuality.HardOnly;
                    else
                        QualitySettings.shadows = ShadowQuality.All;
                });

                itemSelectors.Add(selector);
            }

            index = (int)QualitySettings.shadows;
        }
        else
        {
            index = PlayerPrefs.GetInt(prefsKey, 0);
        }

        if (saveValue)
            index = PlayerPrefs.GetInt(prefsKey, index);

        CreateDropdown();

        if (itemSelectors.Count > 0 && index < itemSelectors.Count)
        {
            dropdown.value = index;
            itemSelectors[index].OnValueChange?.Invoke();
            if (saveValue)
                PlayerPrefs.SetInt(prefsKey, index);
        }

        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    private void CreateDropdown()
    {
        dropdown.ClearOptions();

        List<string> optionTitles = new();
        foreach (var item in itemSelectors)
        {
            optionTitles.Add(item.title);
        }

        dropdown.AddOptions(optionTitles);
    }

    private void OnDropdownValueChanged(int selectedIndex)
    {
        index = selectedIndex;

        if (index >= 0 && index < itemSelectors.Count)
        {
            itemSelectors[index].OnValueChange?.Invoke();
            if (saveValue)
                PlayerPrefs.SetInt(prefsKey, index);
        }
    }
}
