using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject questItemPf;
    public GameObject questObjectivePf;

    // Map quest → quest UI object
    private Dictionary<Quest, GameObject> questToUI = new();
    // Map objective → objective UI object
    private Dictionary<QuestObjective, GameObject> objectiveToUI = new();

    private void Start()
    {
        // Đăng ký sự kiện
        QuestManager.Instance.OnQuestStarted += OnQuestStarted;
        QuestManager.Instance.OnQuestCompleted += OnQuestCompleted;
        QuestManager.Instance.OnObjectiveUpdated += OnObjectiveUpdated;

        // Hiển thị sẵn các quest đang active
        foreach (var quest in QuestManager.Instance.activeQuests)
        {
            CreateQuestItem(quest);
        }
    }

    private void OnDestroy()
    {
        // Hủy đăng ký sự kiện
        QuestManager.Instance.OnQuestStarted -= OnQuestStarted;
        QuestManager.Instance.OnQuestCompleted -= OnQuestCompleted;
        QuestManager.Instance.OnObjectiveUpdated -= OnObjectiveUpdated;
    }

    // =====================
    // QUEST EVENTS
    // =====================
    private void OnQuestStarted(Quest quest)
    {
        CreateQuestItem(quest);
    }

    private void OnQuestCompleted(Quest quest)
    {
        if (questToUI.TryGetValue(quest, out var questItem))
        {
            questItem.transform.Find("Quest Title - Text").GetComponent<TMP_Text>();
        }
    }

    // =====================
    // OBJECTIVE EVENTS
    // =====================
    private void OnObjectiveUpdated(Quest quest, QuestObjective objective)
    {
        if (objectiveToUI.TryGetValue(objective, out var objItem))
        {
            UpdateObjectiveUI(objItem, objective);
        }
        else
        {
            // Nếu objective chưa có UI, tạo mới
            if (questToUI.TryGetValue(quest, out var questItem))
            {
                Transform objectiveListParent = questItem.transform.Find("List Quest");
                CreateObjectiveItem(objective, objectiveListParent);
            }
        }
    }

    // =====================
    // UI CREATION
    // =====================
    private void CreateQuestItem(Quest quest)
    {
        if (questToUI.ContainsKey(quest))
            return;

        GameObject questItem = Instantiate(questItemPf, transform);
        TMP_Text questTitle = questItem.transform.Find("Quest Title - Text").GetComponent<TMP_Text>();
        Transform objectiveListParent = questItem.transform.Find("List Quest");
        Animator animator = questItem.GetComponent<Animator>();

        questTitle.text = quest.questData.questName;

        questToUI[quest] = questItem;

        foreach (var objective in quest.objectives)
        {
            CreateObjectiveItem(objective, objectiveListParent);
        }

        animator.Play("In");
    }

    private void CreateObjectiveItem(QuestObjective objective, Transform parent)
    {
        GameObject objItem = Instantiate(questObjectivePf, parent);
        UpdateObjectiveUI(objItem, objective);
        objectiveToUI[objective] = objItem;
    }

    // =====================
    // UI UPDATE HELPERS
    // =====================
    private void UpdateObjectiveUI(GameObject objItem, QuestObjective objective)
    {
        TextMeshProUGUI objText = objItem.GetComponentInChildren<TextMeshProUGUI>();
        string progressText = $"{objective.currentAmount}/{objective.requiredAmount}";
        objText.text = $"{objective.description} ({progressText})";

        if (objective.isCompleted)
        {
            objText.fontStyle = FontStyles.Strikethrough;
        }
    }
}
