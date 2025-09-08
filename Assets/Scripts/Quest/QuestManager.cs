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

    // Events
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
        // Debug.Log(completedQuestIDs.Count);
        if (autoStart) StartCoroutine(CheckAutoStartQuests());
        OnWaveSpawned += WaveSpawned;
    }

    private void OnDestroy()
    {
        OnWaveSpawned -= WaveSpawned;
    }

    private void WaveSpawned()
    {
        StartCoroutine(CheckAutoStartQuests());
    }

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

    // BẮT ĐẦU QUEST
    public void StartQuest(string questID)
    {
        QuestSO questSO = allQuests.Find(q => q.questID == questID);

        if (!questSO) return;
        if (!CheckQuestRequirements(questSO)) return;
        if (completedQuestIDs.Contains(questID) || activeQuests.Any(q => q.questSO.questID == questID)) return;

        Debug.Log($"{completedQuestIDs.Contains(questID)} {questID}");

        Quest newQuest = new Quest(questSO);
        newQuest.status = QuestStatus.Active;
        activeQuests.Add(newQuest);

        OnQuestStarted?.Invoke(newQuest);
        Debug.Log($"Bắt đầu quest: <b>{questSO.questName.ToUpper()}</b>");

        if (autoSave) SaveLoadManager.Instance.MarkDirty();
    }

    // CẬP NHẬT TIẾN ĐỘ QUEST
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

                    break; // chỉ update 1 objective
                }
            }
        }

        foreach (var quest in questsToComplete)
        {
            CompleteQuest(quest);
        }

        if (updated && autoSave)
            SaveLoadManager.Instance.MarkDirty();

        return updated;
    }

    // HOÀN THÀNH QUEST
    public void CompleteQuest(Quest quest)
    {
        GiveRewards(quest);

        quest.status = QuestStatus.Completed;
        activeQuests.Remove(quest);
        completedQuestIDs.Add(quest.questSO.questID);

        OnQuestCompleted?.Invoke(quest);
        Debug.Log($"Quest hoàn thành: <b>{quest.questSO.questName.ToUpper()}</b>");

        if (autoSave) SaveLoadManager.Instance.MarkDirty();
    }

    // TRAO THƯỞNG
    private void GiveRewards(Quest quest)
    {
        if (quest.questSO.expReward > 0)
            Debug.Log($"Nhận được {quest.questSO.expReward} EXP");

        if (quest.questSO.coinReward > 0)
            CoinManager.Instance.AddCoins(quest.questSO.coinReward);
    }

    // KIỂM TRA ĐIỀU KIỆN QUEST
    private bool CheckQuestRequirements(QuestSO questSO)
    {
        foreach (QuestSO prereq in questSO.prerequisiteQuests)
        {
            if (!completedQuestIDs.Contains(prereq.questID))
                return false;
        }
        return true;
    }
}
