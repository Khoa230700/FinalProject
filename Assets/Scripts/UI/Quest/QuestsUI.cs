using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestsUI : MonoBehaviour
{
    [SerializeField] private GameObject questItemPrefab;

    private Dictionary<string, QuestItemUI> questItemsMap = new();

    public void UpdateUI(string questId, string questTitle, List<string> questSteps)
    {
        if (!questItemsMap.TryGetValue(questId, out QuestItemUI questItemUI))
        {
            GameObject questItemGO = Instantiate(questItemPrefab, this.transform);
            questItemGO.name = questTitle;

            questItemUI = questItemGO.GetComponent<QuestItemUI>();
            questItemUI.Setup(questTitle);

            questItemsMap.Add(questId, questItemUI);
        }

        questItemUI.UpdateUI(questSteps);
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
