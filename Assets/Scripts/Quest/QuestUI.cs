using System.Collections;
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
        QuestManager.Instance.OnQuestStarted += OnQuestStarted;
        QuestManager.Instance.OnQuestCompleted += OnQuestCompleted;
        QuestManager.Instance.OnObjectiveUpdated += OnObjectiveUpdated;

        // Hiển thị Active Quests
        foreach (var quest in QuestManager.Instance.activeQuests)
        {
            CreateQuestItem(quest);
        }

        // Hiển thị Completed Quests
        // foreach (var questID in QuestManager.Instance.completedQuestIDs)
        // {
        //     QuestSO questSO = QuestManager.Instance.allQuests.Find(q => q.questID == questID);
        //     if (questSO != null)
        //     {
        //         Quest completedQuest = new Quest(questSO);
        //         CreateQuestItem(completedQuest, true);
        //     }
        // }
    }

    private void OnDestroy()
    {
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

        if (quest.IsCompleted())
        {
            OnQuestCompleted(quest);
        }
    }

    private void OnQuestCompleted(Quest quest)
    {
        UpdateQuestUI(quest);

        foreach (var objective in quest.objectives)
        {
            UpdateObjectiveUI(objectiveToUI[objective], objective, true);
        }

        if (questToUI.TryGetValue(quest, out var questItem))
        {
            Animator animator = questItem.GetComponent<Animator>();
            if (animator != null)
            {
                StartCoroutine(PlayAndDestroy(questItem, animator, "Out"));
            }
        }
    }

    private IEnumerator PlayAndDestroy(GameObject questItem, Animator animator, string stateName)
    {
        yield return new WaitForSeconds(3f);

        animator.Play(stateName);

        float length = animator.GetCurrentAnimatorClipInfo(0).Length;
        yield return new WaitForSeconds(length + 0.1f);

        Destroy(questItem);
    }

    // =====================
    // OBJECTIVE EVENTS
    // =====================
    private void OnObjectiveUpdated(Quest quest, QuestObjective objective)
    {
        if (objectiveToUI.TryGetValue(objective, out var objItem))
        {
            UpdateObjectiveUI(objItem, objective, objective.isCompleted);
        }

        if (quest.IsCompleted())
        {
            UpdateQuestUI(quest);
        }
    }

    // =====================
    // UI CREATION
    // =====================
    private void CreateQuestItem(Quest quest, bool isCompleted = false)
    {
        if (questToUI.ContainsKey(quest))
            return;

        GameObject questItem = Instantiate(questItemPf, transform);
        TMP_Text questTitle = questItem.transform.Find("Quest Title - Text").GetComponent<TMP_Text>();
        Transform objectiveListParent = questItem.transform.Find("List Quest");
        Animator animator = questItem.GetComponent<Animator>();

        questTitle.text = quest.questSO.questName;

        if (isCompleted)
        {
            questTitle.text += "- Completed";
            foreach (var objective in quest.objectives)
            {
                CreateObjectiveItem(objective, objectiveListParent, true);
            }
        }
        else
        {
            foreach (var objective in quest.objectives)
            {
                CreateObjectiveItem(objective, objectiveListParent, objective.isCompleted);
            }
        }

        questToUI[quest] = questItem;

        animator.Play("In");
    }

    private void CreateObjectiveItem(QuestObjective objective, Transform parent, bool isCompleted)
    {
        GameObject objItem = Instantiate(questObjectivePf, parent);
        UpdateObjectiveUI(objItem, objective, isCompleted);
        objectiveToUI[objective] = objItem;
    }

    // =====================
    // UI UPDATE HELPERS
    // =====================
    private void UpdateObjectiveUI(GameObject objItem, QuestObjective objective, bool isCompleted)
    {
        TextMeshProUGUI objText = objItem.GetComponentInChildren<TextMeshProUGUI>();
        Animator objAnimator = objItem.GetComponent<Animator>();

        if (isCompleted)
        {
            objAnimator.Play("In");
            string progressText = $"{objective.requiredAmount}/{objective.requiredAmount}";
            objText.text = $"{objective.description} ({progressText})";
        }
        else
        {
            string progressText = $"{objective.currentAmount}/{objective.requiredAmount}";
            objText.text = $"{objective.description} ({progressText})";
        }

        if (objective.requiredAmount == 0)
        {
            objText.text = $"{objective.description}";
        }
    }

    private void UpdateQuestUI(Quest quest)
    {
        if (questToUI.TryGetValue(quest, out var questItem))
        {
            TMP_Text questTitle = questItem.transform.Find("Quest Title - Text").GetComponent<TMP_Text>();
            questTitle.text = quest.questSO.questName;

            if (quest.status == QuestStatus.Completed && quest.IsCompleted())
            {
                questTitle.text += " - Completed";
            }
        }
    }
}
