using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Quest DataBase")]
    public List<QuestData> allQuests = new();

    [Header("Current Quests")]
    public List<Quest> activeQuests = new();
    public List<string> completedQuestIDs = new();

    //Events
    public Action<Quest> OnQuestStarted;
    public Action<Quest> OnQuestCompleted;
    public Action<Quest, QuestObjective> OnObjectiveUpdated;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartQuest("Test"); //Test
    }

    //BẮT ĐẦU QUEST
    public bool StartQuest(string questID)
    {
        QuestData questData = allQuests.Find(q => q.questID == questID);

        //Tìm quest data
        if (!questData)
        {
            Debug.Log($"Không tìm thấy quest với ID: {questID}");
            return false;
        }

        //Kiểm tra quest đã hoàn thành hoặc đang active
        if (completedQuestIDs.Contains(questID) || activeQuests.Any(q => q.questData.questID == questID))
        {
            Debug.Log($"QuestID: {questID} đã hoàn thành hoặc đang active");
            return false;
        }

        //Kiểm tra điều kiện
        if (!CheckQuestRequirements(questData))
        {
            return false;
        }

        //Tạo quest mới và thêm vào danh sách active
        Quest newQuest = new Quest(questData);
        newQuest.status = QuestStatus.Active;
        activeQuests.Add(newQuest);

        OnQuestStarted?.Invoke(newQuest);
        Debug.Log($"Đã bắt đầu quest: {questData.questName}");

        return true;
    }

    //CẬP NHẬT TIẾN ĐỘ QUEST
    public void UpdateQuestProgress(QuestObjectiveType type, string targetID, int amount = 1)
    {
        foreach (var quest in activeQuests)
        {
            if (quest.status != QuestStatus.Active)
                continue;

            foreach (var obj in quest.objectives)
            {
                if (obj.type == type && obj.targetID == targetID && !obj.isCompleted)
                {
                    quest.UpdateObjective(obj.objectiveID, amount);
                    OnObjectiveUpdated?.Invoke(quest, obj);

                    //Kiểm tra quest hoàn thành
                    if (quest.IsCompleted())
                    {
                        CompleteQuest(quest);
                    }

                    break;
                }
            }
        }
    }

    //HOÀN THÀNH QUEST
    public void CompleteQuest(Quest quest)
    {
        quest.status = QuestStatus.Completed;
        OnQuestCompleted?.Invoke(quest);
        Debug.Log($"Quest hoàn thành: {quest.questData.questName}");
    }

    //NỘP QUEST VÀ NHẬN THƯỞNG
    public bool TurnInQuest(string questID)
    {
        Quest quest = activeQuests.Find(q => q.questData.questID == questID);
        if (quest == null || quest.status != QuestStatus.Completed)
        {
            return false;
        }

        //Trao thưởng
        GiveRewards(quest.questData);

        //Chuyển quest sang trại thái đã nộp
        quest.status = QuestStatus.TurnedIn;
        activeQuests.Remove(quest);
        completedQuestIDs.Add(quest.questData.questID);

        Debug.Log($"Đã nộp quest: {quest.questData.questName}");
        return true;
    }

    //TRAO THƯỞNG
    private void GiveRewards(QuestData questData)
    {
        // Thêm EXP
        if (questData.expReward > 0)
        {
            // PlayerManager.Instance.AddExperience(questData.expReward);
            Debug.Log($"Nhận được {questData.expReward} EXP");
        }

        // Thêm Gold
        if (questData.goldReward > 0)
        {
            // PlayerManager.Instance.AddGold(questData.goldReward);
            Debug.Log($"Nhận được {questData.goldReward} Gold");
        }
    }

    //KIỂM TRA ĐIỀU KIỆN QUEST
    private bool CheckQuestRequirements(QuestData questData)
    {
        // Kiểm tra level yêu cầu
        // if (PlayerManager.Instance.GetLevel() < questData.requiredLevel)
        //     return false;

        // Kiểm tra prerequisite quests
        foreach (string prereqID in questData.prerequisiteQuests)
        {
            if (!completedQuestIDs.Contains(prereqID))
                return false;
        }

        return true;
    }

    // // Lấy quest đang active
    // public Quest GetActiveQuest(string questID)
    // {
    //     return activeQuests.Find(q => q.questData.questID == questID);
    // }
    
    // // Lấy tất cả quest có thể bắt đầu
    // public List<QuestData> GetAvailableQuests()
    // {
    //     return allQuests.Where(q => 
    //         !completedQuestIDs.Contains(q.questID) && 
    //         !activeQuests.Any(aq => aq.questData.questID == q.questID) &&
    //         CheckQuestRequirements(q)
    //     ).ToList();
    // }
}
