using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestManager : MonoBehaviour, ISaveLoad
{
    public static QuestManager Instance { get; private set; }

    [Header("Quests")]
    public List<Quest> activeQuests = new();
    public List<QuestSO> allQuests = new();
    public List<string> completedQuestIDs = new();
    public bool verbose = true;

    // Events
    public Action<Quest> OnQuestStarted;
    public Action<Quest> OnQuestCompleted;
    public Action<Quest, QuestObjective> OnObjectiveUpdated;
    public Action OnWaveSpawned;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        allQuests = Resources.LoadAll("Quests", typeof(QuestSO)).OfType<QuestSO>().ToList();
    }

    private void Start()
    {
        SaveLoadManager.Instance?.Register(this);

        StartCoroutine(CheckAutoStartQuests());
        OnWaveSpawned += WaveSpawned;
    }

    private void OnDestroy()
    {
        OnWaveSpawned -= WaveSpawned;
        SaveLoadManager.Instance?.Unregister(this);
    }

    // CORE
    public void StartQuest(string questID)
    {
        QuestSO questSO = allQuests.Find(q => q.questID == questID);
        if (!questSO) return;
        if (!CheckQuestRequirements(questSO)) return;
        if (completedQuestIDs.Contains(questID) || activeQuests.Any(q => q.questSO.questID == questID)) return;

        Quest newQuest = new Quest(questSO);
        newQuest.status = QuestStatus.Active;
        activeQuests.Add(newQuest);

        OnQuestStarted?.Invoke(newQuest);
        if(verbose) Debug.Log($"Quest started: <b>{questSO.questName}</b>");

        SaveLoadManager.Instance?.MarkDirty();
    }

    public bool UpdateQuestProgress(QuestObjectiveType type, string targetID, int amount = 1)
    {
        bool updated = false;
        List<Quest> questsToComplete = new();

        foreach (var quest in activeQuests)
        {
            if (quest.status != QuestStatus.Active) continue;

            foreach (var obj in quest.objectives)
            {
                if (obj.type == type && obj.targetID == targetID && !obj.isCompleted)
                {
                    quest.UpdateObjective(obj.objectiveID, amount);
                    OnObjectiveUpdated?.Invoke(quest, obj);
                    updated = true;

                    if (quest.IsCompleted())
                        questsToComplete.Add(quest);
                    break;
                }
            }
        }

        foreach (var quest in questsToComplete)
        {
            CompleteQuest(quest);
        }

        if (updated)
            SaveLoadManager.Instance?.MarkDirty();

        return updated;
    }

    public void CompleteQuest(Quest quest)
    {
        GiveRewards(quest);

        quest.status = QuestStatus.Completed;
        activeQuests.Remove(quest);
        completedQuestIDs.Add(quest.questSO.questID);

        OnQuestCompleted?.Invoke(quest);
        if(verbose) Debug.Log($"Quest completed: <b>{quest.questSO.questName}</b>");

        SaveLoadManager.Instance?.MarkDirty();
    }

    private void GiveRewards(Quest quest)
    {
        // if (quest.questSO.expReward > 0)
        //     Debug.Log($"Received {quest.questSO.expReward} EXP");

        if (quest.questSO.coinReward > 0)
        {
            CoinManager.Instance.AddCoins(quest.questSO.coinReward);
            if(verbose) Debug.Log(quest.questSO.coinReward + " coins");
        }
    }

    // HELPER
    private void WaveSpawned()
    {
        StartCoroutine(CheckAutoStartQuests());
    }

    private IEnumerator CheckAutoStartQuests()
    {
        yield return new WaitForSeconds(0.5f);
        string currentScene = SceneManager.GetActiveScene().name;

        var autoStartQuests = allQuests.Where(q => q.autoStart &&
                        (string.IsNullOrEmpty(q.autoStartScene) || q.autoStartScene == currentScene) &&
                        !completedQuestIDs.Contains(q.questID) &&
                        !activeQuests.Any(aq => aq.questSO.questID == q.questID)) // Thêm dòng này
                        .ToList();

        foreach (var questData in autoStartQuests)
        {
            StartQuest(questData.questID);
        }
    }

    private bool CheckQuestRequirements(QuestSO questSO)
    {
        foreach (QuestSO prereq in questSO.prerequisiteQuests)
        {
            if (!completedQuestIDs.Contains(prereq.questID))
                return false;
        }
        return true;
    }

    // ISaveLoad
    public void SaveToData(GameData data)
    {
        data.questData = new QuestData
        {
            completedQuestIDs = new List<string>(completedQuestIDs),
            activeQuests = new List<QuestDataSO>()
        };

        foreach (var quest in activeQuests)
        {
            QuestDataSO questData = new QuestDataSO
            {
                questID = quest.questSO.questID,
                status = quest.status,
                objectives = new List<ObjectiveData>()
            };

            foreach (var obj in quest.objectives)
            {
                questData.objectives.Add(new ObjectiveData
                {
                    objectiveID = obj.objectiveID,
                    currentAmount = obj.currentAmount,
                    isCompleted = obj.isCompleted
                });
            }
            data.questData.activeQuests.Add(questData);
        }
    }

    public void LoadFromData(GameData data)
    {
        activeQuests.Clear();
        completedQuestIDs.Clear();

        if (data?.questData == null) return;

        completedQuestIDs.AddRange(data.questData.completedQuestIDs);

        foreach (var questSave in data.questData.activeQuests)
        {
            QuestSO questSO = allQuests.Find(q => q.questID == questSave.questID);
            if (questSO != null)
            {
                Quest quest = new Quest(questSO);
                quest.status = questSave.status;

                foreach (var objSave in questSave.objectives)
                {
                    var obj = quest.objectives.Find(o => o.objectiveID == objSave.objectiveID);
                    if (obj != null)
                    {
                        obj.currentAmount = objSave.currentAmount;
                        obj.isCompleted = objSave.isCompleted;
                    }
                }
                activeQuests.Add(quest);
            }
        }
    }
}
