using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestsUI : MonoBehaviour
{
    [SerializeField] private GameObject questItemPrefab;

    private Dictionary<string, QuestItemUI> questItemsMap = new();

    private void OnEnable()
    {
        GameEventsManager.Instance.questEvents.OnQuestStateChange += UpdateUI;
    }

    private void OnDisable()
    {
        GameEventsManager.Instance.questEvents.OnQuestStateChange -= UpdateUI;
    }

    public void UpdateUI(Quest quest)
    {
        string questId = quest.questInfo.questId;
        string questTitle = quest.questInfo.questName;
        string questStatus = quest.GetStatus();

        if (!questItemsMap.ContainsKey(questId))
        {
            GameObject questItemGO = Instantiate(questItemPrefab, this.transform);
            questItemGO.name = questTitle;
            QuestItemUI questItemUI = questItemGO.GetComponent<QuestItemUI>();
            questItemsMap.Add(questId, questItemUI);
        }

        questItemsMap[questId].UpdateUI(questId, questTitle, questStatus);
    }

    public void ClearAllQuest()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        questItemsMap.Clear();
    }
}
