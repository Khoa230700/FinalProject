using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class HorizontalSelectorUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int defaultIndex = 0;
    [SerializeField] private string selectorTag = "Tag";
    [SerializeField] private bool saveValue;

    [Header("Indicators")]
    [SerializeField] private Transform indicatorParent;
    [SerializeField] private GameObject indicatorPrefab;

    [Header("Item")]
    [SerializeField] private List<ItemSelector> itemSelectors = new();

    private int index = 0;
    private TextMeshProUGUI text, textHelper;
    private Animator animator;
    private GameObject onObj, offObj;
    private string stringPrefs = "HSelector";

    private void Start()
    {
        text ??= transform.Find("Text").GetComponent<TextMeshProUGUI>();
        textHelper ??= transform.Find("Text Helper").GetComponent<TextMeshProUGUI>();
        animator ??= GetComponent<Animator>();

        if (saveValue)
        {
            if (PlayerPrefs.HasKey(selectorTag + stringPrefs))
            {
                defaultIndex = PlayerPrefs.GetInt(selectorTag + stringPrefs);
            }
            else
            {
                PlayerPrefs.SetInt(selectorTag + stringPrefs, defaultIndex);
            }
        }

        text.text = itemSelectors[defaultIndex].title;
        textHelper.text = text.text;
        index = defaultIndex;

        CreateIndicatorUI();
    }

    private void CreateIndicatorUI()
    {
        foreach (Transform child in indicatorParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < itemSelectors.Count; i++)
        {
            GameObject go = Instantiate(indicatorPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            go.transform.SetParent(indicatorParent, false);
            go.name = itemSelectors[i].title;

            onObj = go.transform.Find("On").gameObject;
            offObj = go.transform.Find("Off").gameObject;

            if (i == index)
            {
                onObj.SetActive(true);
                offObj.SetActive(false);
            }
            else
            {
                onObj.SetActive(false);
                offObj.SetActive(true);
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
