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

    [Header("Quest DataBase")]
    public List<QuestData> allQuests = new();

    [Header("Current Quests")]
    public List<Quest> activeQuests = new();
    private List<string> completedQuestIDs = new();


    [Header("Settings")]
    [SerializeField] private bool autoStart = true;
    [SerializeField] private bool autoSave = true;
    [SerializeField] private float autoSaveInterval = 60f; // 60 giây

    //Events
    public Action<Quest> OnQuestStarted;
    public Action<Quest> OnQuestCompleted;
    public Action<Quest, QuestObjective> OnObjectiveUpdated;
    public Action OnQuestDataLoaded;

    private Coroutine autoSaveCoroutine;
    private string savePath;

    private void Awake()
    {
        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (allQuests == null || allQuests.Count <= 0)        
        {
            allQuests = Resources.LoadAll("Quests", typeof(QuestData)).OfType<QuestData>().ToList();
        }
    }

    private void Start()
    {
        savePath = Application.persistentDataPath + "/QuestSave.json";
        LoadQuestData();

        if (autoSave) autoSaveCoroutine = StartCoroutine(AutoSaveCoroutine());
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
                !activeQuests.Any(q => q.questData.questID == questData.questID))
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
        Debug.Log($"Bắt đầu quest: {questData.questName}");

        if (autoSave) SaveQuestData();

        return true;
    }

    //CẬP NHẬT TIẾN ĐỘ QUEST
    public void UpdateQuestProgress(QuestObjectiveType type, string targetID, int amount = 1)
    {
        bool hasUpdated = false;

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
                        CompleteQuest(quest);
                    }

                    break;
                }
            }
        }

        if (hasUpdated && autoSave) SaveQuestData();
    }

    //HOÀN THÀNH QUEST
    public void CompleteQuest(Quest quest)
    {
        quest.status = QuestStatus.Completed;
        GiveRewards(quest.questData);

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

        Debug.Log($"Nộp quest: {quest.questData.questName}");

        if (autoSave) SaveQuestData();

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

    //SAVE
    public void SaveQuestData()
    {
        try
        {
            QuestSaveData saveData = new QuestSaveData();

            //Lưu các quest đang active
            foreach (var quest in activeQuests)
            {
                QuestSave questSave = new QuestSave
                {
                    questID = quest.questData.questID,
                    status = quest.status
                };

                foreach (var objective in quest.objectives)
                {
                    questSave.objectives.Add(new ObjectiveSave
                    {
                        objectiveID = objective.objectiveID,
                        currentAmount = objective.currentAmount,
                        isCompleted = objective.isCompleted
                    });
                }

                saveData.activeQuests.Add(questSave);
            }

            //Lưu id quest đã hoàn thành
            saveData.completedQuestIDs = new List<string>(completedQuestIDs);
            saveData.currentScene = SceneManager.GetActiveScene().name;
            saveData.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


            //Convert to JSON and save
            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(savePath, json);

            Debug.Log($"Quest data đã được lưu: {savePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Lỗi khi lưu quest data: {e.Message}");
        }
    }

    //LOAD
    public void LoadQuestData()
    {
        try
        {
            if (!File.Exists(savePath))
            {
                Debug.Log("Không tìm thấy file save quest data");
                return;
            }

            string json = File.ReadAllText(savePath);
            QuestSaveData saveData = JsonUtility.FromJson<QuestSaveData>(json);

            if (saveData == null)
            {
                Debug.LogError("Không thể load quest save data");
                return;
            }

            //Clear current data
            activeQuests.Clear();
            completedQuestIDs.Clear();

            //Load completed quest IDs
            completedQuestIDs.AddRange(saveData.completedQuestIDs);

            //Load active quests
            foreach (var questSave in saveData.activeQuests)
            {
                QuestData questData = allQuests.Find(q => q.questID == questSave.questID);
                if (questData != null)
                {
                    Quest quest = new Quest(questData);
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
            Debug.Log($"Đã load quest data: {saveData.activeQuests.Count} active quests, {saveData.completedQuestIDs.Count} completed quests");
        }
        catch (Exception e)
        {
            Debug.LogError($"Lỗi khi load quest data: {e.Message}");
        }
    }

    private IEnumerator AutoSaveCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSaveInterval);
            SaveQuestData();
        }
    }

    // Debug methods
    [ContextMenu("Save Quest Data")]
    public void SaveQuestDataDebug()
    {
        SaveQuestData();
    }

    [ContextMenu("Load Quest Data")]
    public void LoadQuestDataDebug()
    {
        LoadQuestData();
    }

    [ContextMenu("Load Quest")]
    public void LoadQuestResourceDebug()
    {
        allQuests = Resources.LoadAll("Quests", typeof(QuestData)).OfType<QuestData>().ToList();
        savePath = Application.persistentDataPath + "/QuestSave.json";
        Debug.Log(savePath);
    }

    [ContextMenu("Clear Save Data")]
    public void ClearSaveData()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Quest save data đã được xóa");
        }
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
