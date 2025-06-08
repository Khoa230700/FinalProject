using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class HorizontalSelectorUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string selectorTag = "Tag";
    [SerializeField] private bool saveValue;

    [Header("Indicators")]
    [SerializeField] private Transform indicatorParent;
    [SerializeField] private GameObject indicatorPrefab;

    [Header("Item")]
    [SerializeField] private List<ItemSelector> itemSelectors = new();

    private int index = 0, previousIndex;
    private TextMeshProUGUI text, textHelper;
    private Animator animator;
    private GameObject onObj, offObj;
    private string prefsKey;

    private void Start()
    {
        text ??= transform.Find("Text").GetComponent<TextMeshProUGUI>();
        textHelper ??= transform.Find("Text Helper").GetComponent<TextMeshProUGUI>();
        animator ??= GetComponent<Animator>();
        
        prefsKey = selectorTag + "HSelector";

        index = PlayerPrefs.GetInt(prefsKey, 0);

        UpdateText();
        CreateIndicatorUI();
        
        if (itemSelectors.Count > 0)
            itemSelectors[index].OnValueChange?.Invoke();
    }

    private void CreateIndicatorUI()
    {
        foreach (Transform child in indicatorParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < itemSelectors.Count; i++)
        {
            GameObject go = Instantiate(indicatorPrefab, indicatorParent);
            go.name = itemSelectors[i].title;

            onObj = go.transform.Find("On")?.gameObject;
            offObj = go.transform.Find("Off")?.gameObject;

            if (onObj && offObj)
            {
                onObj.SetActive(i == index);
                offObj.SetActive(!(i == index));
            }
        }
    }

    public void BackClick()
    {
        if (itemSelectors.Count == 0) return;

        textHelper.text = text.text;
        previousIndex = index;

        if (index > 0) index--;
        if (index != previousIndex) UpdateSelection("Back");
    }

    public void NextClick()
    {
        if (itemSelectors.Count == 0) return;

        textHelper.text = text.text;
        previousIndex = index;

        if (index < itemSelectors.Count - 1) index++;
        if (index != previousIndex) UpdateSelection("Next");
    }

    private void UpdateSelection(string animationName)
    {
        UpdateText();
        itemSelectors[index].OnValueChange?.Invoke();
        UpdateIndicator();

        animator.Play(null);
        animator.StopPlayback();
        animator.Play(animationName);

        if (saveValue) PlayerPrefs.SetInt(prefsKey, index);
    }

    private void UpdateText()
    {
        if (itemSelectors.Count > 0)
        {
            text.text = itemSelectors[index].title;
            textHelper.text = text.text;
        }
    }

    public void UpdateIndicator()
    {
        for (int i = 0; i < itemSelectors.Count; i++)
        {
            Transform child = indicatorParent.GetChild(i);
            onObj = child.Find("On")?.gameObject;
            offObj = child.Find("Off")?.gameObject;

            if (onObj && offObj)
            {
                onObj.SetActive(i == index);
                offObj.SetActive(!(i == index));
            }
        }
    }
}

[Serializable]
public class ItemSelector
{
    public string title = "Title";
    public UnityEvent OnValueChange = new();
}
