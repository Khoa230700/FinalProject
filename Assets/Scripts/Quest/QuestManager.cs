using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Quests")]
    public List<Quest> activeQuests = new();
    public List<QuestSO> allQuests = new();
    public List<string> completedQuestIDs = new();


    [Header("Settings")]
    [SerializeField] private bool autoStart = true;
    [SerializeField] private bool autoSave = true;

    //Events
    public Action<Quest> OnQuestStarted;
    public Action<Quest> OnQuestCompleted;
    public Action<Quest, QuestObjective> OnObjectiveUpdated;
    public Action OnWaveSpawned;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        allQuests = Resources.LoadAll("Quests", typeof(QuestSO)).OfType<QuestSO>().ToList();
        LoadQuestData();

        if (autoStart) StartCoroutine(CheckAutoStartQuests());

        OnWaveSpawned += WaveSpawned;
    }

    private void OnDestroy()
    {
        if (autoSave) SaveQuestData();

        OnWaveSpawned -= WaveSpawned;
    }

    private void WaveSpawned()
    {
        StartCoroutine(CheckAutoStartQuests());
    }

    //KIỂM TRA CÁC QUEST AUTOSTART
    private IEnumerator CheckAutoStartQuests()
    {
        yield return new WaitForSeconds(0.5f);

        string currentScene = SceneManager.GetActiveScene().name;

        var autoStartQuests = allQuests.Where(q =>
            q.autoStart &&
            (string.IsNullOrEmpty(q.autoStartScene) || q.autoStartScene == currentScene)
        ).ToList();

        foreach (var questData in autoStartQuests)
        {
            StartQuest(questData.questID);
        }
    }

    //BẮT ĐẦU QUEST
    public void StartQuest(string questID)
    {
        QuestSO questSO = allQuests.Find(q => q.questID == questID);

        if (!questSO) return;
        if (!CheckQuestRequirements(questSO)) return;
        if (completedQuestIDs.Contains(questID) || activeQuests.Any(q => q.questSO.questID == questID)) return;

        //Tạo quest mới và thêm vào danh sách active
        Quest newQuest = new Quest(questSO);
        newQuest.status = QuestStatus.Active;
        activeQuests.Add(newQuest);

        OnQuestStarted?.Invoke(newQuest);
        Debug.Log($"Bắt đầu quest: <b>{questSO.questName.ToUpper()}</b>");

        if (autoSave) SaveQuestData();
    }

    //CẬP NHẬT TIẾN ĐỘ QUEST
    public void UpdateQuestProgress(QuestObjectiveType type, string targetID, int amount = 1)
    {
        bool hasUpdated = false;
        List<Quest> questsToComplete = new();

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
                    hasUpdated = true;

                    //Kiểm tra quest hoàn thành
                    if (quest.IsCompleted())
                    {
                        questsToComplete.Add(quest);
                    }

                    break;
                }
            }
        }

        foreach (var quest in questsToComplete)
        {
            CompleteQuest(quest);
        }

        if (hasUpdated && autoSave) SaveQuestData();
    }

    //HOÀN THÀNH QUEST
    public void CompleteQuest(Quest quest)
    {
        GiveRewards(quest);

        quest.status = QuestStatus.Completed;
        activeQuests.Remove(quest);
        completedQuestIDs.Add(quest.questSO.questID);

        OnQuestCompleted?.Invoke(quest);
        Debug.Log($"Quest hoàn thành: <b>{quest.questSO.questName.ToUpper()}</b>");

        if (autoSave) SaveQuestData();

        OnWaveSpawned?.Invoke();
    }

    //TRAO THƯỞNG
    private void GiveRewards(Quest quest)
    {
        // Thêm EXP
        if (quest.questSO.expReward > 0)
        {
            // PlayerManager.Instance.AddExperience(questData.expReward);
            Debug.Log($"Nhận được {quest.questSO.expReward} EXP");
        }

        // Thêm Coin
        if (quest.questSO.coinReward > 0)
        {
            CoinManager.Instance.AddCoins(quest.questSO.coinReward);
        }
    }

    //KIỂM TRA ĐIỀU KIỆN QUEST
    private bool CheckQuestRequirements(QuestSO questSO)
    {
        // Kiểm tra level yêu cầu
        // if (PlayerManager.Instance.GetLevel() < questData.requiredLevel)
        //     return false;

        // Kiểm tra prerequisite quests
        foreach (QuestSO prereq in questSO.prerequisiteQuests)
        {
            if (!completedQuestIDs.Contains(prereq.questID))
                return false;
        }

        return true;
    }

    //SAVE
    public void SaveQuestData()
    {
        SaveLoadUtils.Data.questData = new QuestData();

        //Lưu các quest đang active
        foreach (var quest in activeQuests)
        {
            QuestDataSO questSave = new QuestDataSO
            {
                questID = quest.questSO.questID,
                status = quest.status
            };

            foreach (var objective in quest.objectives)
            {
                questSave.objectives.Add(new ObjectiveData
                {
                    objectiveID = objective.objectiveID,
                    currentAmount = objective.currentAmount,
                    isCompleted = objective.isCompleted
                });
            }

            SaveLoadUtils.Data.questData.activeQuests.Add(questSave);
        }

        //Lưu id quest đã hoàn thành
        SaveLoadUtils.Data.questData.completedQuestIDs = completedQuestIDs;

        SaveLoadUtils.Save(EncryptionType.None);
    }

    //LOAD
    public void LoadQuestData()
    {
        QuestData questData = SaveLoadUtils.Load(EncryptionType.None).questData;

        if (questData == null)
        {
            return;
        }

        //Clear current data
        activeQuests.Clear();
        completedQuestIDs.Clear();

        //Load completed quest IDs
        completedQuestIDs.AddRange(questData.completedQuestIDs);

        //Load active quests
        foreach (var questSave in questData.activeQuests)
        {
            QuestSO questSO = allQuests.Find(q => q.questID == questSave.questID);
            if (questSO != null)
            {
                Quest quest = new Quest(questSO);
                quest.status = questSave.status;

                // Load objective progress
                for (int i = 0; i < quest.objectives.Count && i < questSave.objectives.Count; i++)
                {
                    var objSave = questSave.objectives.Find(o => o.objectiveID == quest.objectives[i].objectiveID);
                    if (objSave != null)
                    {
                        quest.objectives[i].currentAmount = objSave.currentAmount;
                        quest.objectives[i].isCompleted = objSave.isCompleted;
                    }
                }

                activeQuests.Add(quest);
            }
        }

        Debug.Log($"Đã load quest data: {questData.activeQuests.Count} active quests, {questData.completedQuestIDs.Count} completed quests");
    }


    //DEBUGS
    [ContextMenu("Save All Quests")]
    public void SaveAllQuests()
    {
        SaveQuestData();
    }

    [ContextMenu("Clear All Quest")]
    public void ClearAllQuests()
    {
        allQuests.Clear();
        activeQuests.Clear();
        completedQuestIDs.Clear();
    }

    [ContextMenu("Load All Quests")]
    public void LoadAllQuests()
    {
        allQuests = Resources.LoadAll("Quests", typeof(QuestSO)).OfType<QuestSO>().ToList();
    }
}
