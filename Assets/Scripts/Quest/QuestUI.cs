using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject questItemPf;
    public GameObject questObjectivePf;

    private List<GameObject> questItems = new();
    private List<GameObject> objectiveItems = new();

    private void Start()
    {
        QuestManager.Instance.OnQuestStarted += RefreshQuestList;
        QuestManager.Instance.OnQuestCompleted += RefreshQuestList;
        QuestManager.Instance.OnObjectiveUpdated += OnObjectiveUpdated;

        RefreshQuestList();
    }

    private void OnDestroy()
    {
        QuestManager.Instance.OnQuestStarted -= RefreshQuestList;
        QuestManager.Instance.OnQuestCompleted -= RefreshQuestList;
        QuestManager.Instance.OnObjectiveUpdated -= OnObjectiveUpdated;
    }

    private void RefreshQuestList(Quest quest = null)
    {
        //Xóa quest item UI đang có
        foreach (GameObject item in questItems)
        {
            Destroy(item);
        }
        questItems.Clear();

        //Tạo quest item UI
        foreach (Quest activeQuest in QuestManager.Instance.activeQuests)
        {
            if (activeQuest.status == QuestStatus.Active || activeQuest.status == QuestStatus.Completed)
            {
                CreateQuestItem(activeQuest);
            }
        }
    }

    private void CreateQuestItem(Quest quest)
    {
        GameObject questItem = Instantiate(questItemPf, transform);
        TMP_Text questTitle =  questItem.transform.Find("Quest Title - Text").GetComponent<TMP_Text>();
        Transform objectiveListParent = questItem.transform.Find("List Quest");

        questTitle.text = quest.questData.questName;

        if (quest.status == QuestStatus.Completed)
        {
            questTitle.color = Color.green;
        }

        questItems.Add(questItem);

        foreach (var objective in quest.objectives)
        {
            CreateObjectiveItem(objective, objectiveListParent);
        }
    }

    private void CreateObjectiveItem(QuestObjective objective, Transform objectiveListParent)
    { 
        GameObject objItem = Instantiate(questObjectivePf, objectiveListParent);

        TextMeshProUGUI objText = objItem.GetComponentInChildren<TextMeshProUGUI>();
        string progressText = $"{objective.currentAmount}/{objective.requiredAmount}";
        objText.text = $"• {objective.description} ({progressText})";

        if (objective.isCompleted)
        {
            objText.color = Color.green;
            objText.fontStyle = FontStyles.Strikethrough;
        }

        objectiveItems.Add(objItem);
    }

    private void OnObjectiveUpdated(Quest quest, QuestObjective objective)
    {
        RefreshQuestList();
    }
}
