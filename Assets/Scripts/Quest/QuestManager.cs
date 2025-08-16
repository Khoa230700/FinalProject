using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Mathematics;
using UnityEditor.Overlays;
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
    public Action OnQuestDataLoaded;

    private Coroutine autoSaveCoroutine;

    private void Awake()
    {
        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
        allQuests = Resources.LoadAll("Quests", typeof(QuestSO)).OfType<QuestSO>().ToList();
    }

    private void Start()
    {
        LoadQuestData();

        if (autoStart) StartCoroutine(CheckAutoStartQuests());
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (autoSave) SaveQuestData();

        if (autoSaveCoroutine != null) StopCoroutine(autoSaveCoroutine);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (autoStart)
        {
            StartCoroutine(CheckAutoStartQuests());
        }
    }

    //KIỂM TRA CÁC QUEST AUTOSTART
    private IEnumerator CheckAutoStartQuests()
    {
        yield return new WaitForSeconds(0.5f); //Đợi scene load

        string currentScene = SceneManager.GetActiveScene().name;

        //Tìm quét auto start cho scene hiện tại
        var autoStartQuests = allQuests.Where(q => q.autoStart &&
                                            (string.IsNullOrEmpty(q.autoStartScene) ||
                                            q.autoStartScene == currentScene)).ToList();

        foreach (var questData in autoStartQuests)
        {
            //Kiểm tra quest chưa được start và thỏa mãn điều kiện
            if (!completedQuestIDs.Contains(questData.questID) &&
                !activeQuests.Any(q => q.questSO.questID == questData.questID))
            {
                if (questData.autoStartDelay > 0)
                {
                    yield return new WaitForSeconds(questData.autoStartDelay);
                }

                if (CheckQuestRequirements(questData))
                {
                    StartQuest(questData.questID);
                    Debug.Log($"[Auto Start] Quest đã tự động bắt đầu: {questData.questName}");
                }
            }
        }
    }

    //BẮT ĐẦU QUEST
    public bool StartQuest(string questID)
    {
        QuestSO questSO = allQuests.Find(q => q.questID == questID);

        //Tìm quest progress
        if (!questSO)
        {
            Debug.Log($"Không tìm thấy quest với ID: {questID}");
            return false;
        }

        //Kiểm tra quest đã hoàn thành hoặc đang active
        if (completedQuestIDs.Contains(questID) || activeQuests.Any(q => q.questSO.questID == questID))
        {
            Debug.Log($"QuestID: {questID} đã hoàn thành hoặc đang active");
            return false;
        }

        //Kiểm tra điều kiện
        if (!CheckQuestRequirements(questSO))
        {
            return false;
        }

        //Tạo quest mới và thêm vào danh sách active
        Quest newQuest = new Quest(questSO);
        newQuest.status = QuestStatus.Active;
        activeQuests.Add(newQuest);

        OnQuestStarted?.Invoke(newQuest);
        Debug.Log($"Bắt đầu quest: {questSO.questName}");

        if (autoSave) SaveQuestData();

        return true;
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
        OnQuestCompleted?.Invoke(quest);
        GiveRewards(quest);
        Debug.Log($"Quest hoàn thành: {quest.questSO.questName}");
    }

    //TRAO THƯỞNG
    private void GiveRewards(Quest quest)
    {
        //Chuyển quest sang trại thái đã nộp
        quest.status = QuestStatus.Completed;
        activeQuests.Remove(quest);
        completedQuestIDs.Add(quest.questSO.questID);

        // Thêm EXP
        if (quest.questSO.expReward > 0)
        {
            // PlayerManager.Instance.AddExperience(questData.expReward);
            Debug.Log($"Nhận được {quest.questSO.expReward} EXP");
        }

        // Thêm Gold
        if (quest.questSO.goldReward > 0)
        {
            // PlayerManager.Instance.AddGold(questData.goldReward);
            Debug.Log($"Nhận được {quest.questSO.goldReward} Gold");
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
        QuestData questData = new();

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

            questData.activeQuests.Add(questSave);
        }

        //Lưu id quest đã hoàn thành
        questData.completedQuestIDs = new List<string>(completedQuestIDs);
        questData.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


        //Convert to JSON and save
        SaveLoadUtils.Save("QuestData", questData, EncryptionType.AES);
        // string json = JsonUtility.ToJson(saveData, true);
        // File.WriteAllText(savePath, json);
    }

    //LOAD
    public void LoadQuestData()
    {
        QuestData questData = SaveLoadUtils.Load<QuestData>("QuestData", EncryptionType.AES);
        // string json = File.ReadAllText(savePath);

        if (questData == null)
        {
            Debug.Log("Không thể load quest save data");
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
            else
            {
                Debug.LogWarning($"Không tìm thấy quest data cho ID: {questSave.questID}");
            }
        }

        OnQuestDataLoaded?.Invoke();
        Debug.Log($"Đã load quest data: {questData.activeQuests.Count} active quests, {questData.completedQuestIDs.Count} completed quests");
    }

    // Debug methods
    [ContextMenu("Save Quest Data")]
    public void SaveQuestDataDebugXOR()
    {
        SaveQuestData();
    }

    [ContextMenu("Load Quest Data")]
    public void LoadQuestDataDebug()
    {
        LoadQuestData();
    }

    [ContextMenu("Load Quest Resource")]
    public void LoadQuestResourceDebug()
    {
        allQuests = Resources.LoadAll("Quests", typeof(QuestSO)).OfType<QuestSO>().ToList();
        foreach (var quest in allQuests)
        {
            StartQuest(quest.questID);
        }
    }

    [ContextMenu("Clear Save Data")]
    public void ClearSaveData()
    {
        string savePath = Application.persistentDataPath + "/QuestData.json";
        allQuests.Clear();
        activeQuests.Clear();
        completedQuestIDs.Clear();

        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }
    }
}
