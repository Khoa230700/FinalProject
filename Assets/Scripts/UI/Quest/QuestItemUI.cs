using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class QuestItemUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Transform listRoot;
    [SerializeField] private GameObject questStepPrefab;

    private bool isOpen = false;
    private Dictionary<string, TextMeshProUGUI> questStepsMap = new();

    void Start()
    {
        animator ??= GetComponent<Animator>();
    }

    void Update()
    {
        if (KeyBindingManager.Instance.GetKeyDown("Open Quests"))
        {
            ToggleUI();
        }
    }

    public void UpdateUI(string id, string title, string status)
    {
        titleText.text = title;

        if(!questStepsMap.ContainsKey(id))
        {
            GameObject questStepGO = Instantiate(questStepPrefab, listRoot);
            questStepGO.name = status;
            TextMeshProUGUI questStepText = questStepGO.GetComponentInChildren<TextMeshProUGUI>();
            questStepsMap.Add(id, questStepText);
        }

        questStepsMap[id].text = status;
    }

    private void ToggleUI()
    {
        isOpen = !isOpen;
        animator.Play(isOpen ? "In" : "Out");
    }
}
