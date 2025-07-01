using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class DropdownUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string selectorTag = "Tag";
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

        if (selectorTag == "Resolutions")
        {
            Resolution[] resolutions = Screen.resolutions;
            itemSelectors.Clear();

            foreach (var res in resolutions)
            {
                string resolutionTitle = $"{res.width}x{res.height}";
                Resolution currentRes = res;

                var selector = new ItemSelector
                {
                    title = resolutionTitle
                };
                selector.OnValueChange.AddListener(() =>
                {
                    // Screen.SetResolution(currentRes.width, currentRes.height, Screen.fullScreen);
                    Debug.Log("Set resolution: " + resolutionTitle);
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

            if (saveValue)
                index = PlayerPrefs.GetInt(prefsKey, index);
        }
        else
        {
            index = PlayerPrefs.GetInt(prefsKey, 0);
        }

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
