using System;
using System.Collections.Generic;
using System.Security.Claims;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [Header("List of quests for scene")]
    [SerializeField] private QuestInfoSO[] questList;

    [Header("Reference")]
    [SerializeField] private QuestsUI questsUI;

    private Dictionary<string, Quest> questMap;
    [SerializeField] private int currentPlayerLevel; // tam thoi

    private void Awake()
    {
        questMap = CreateQuestMap(questList);
        Debug.Log("Quest Map Created with " + questMap.Count + " quests.");
    }

    private void OnEnable()
    {
        GameEventsManager.Instance.questEvents.OnStartQuest += StartQuest;
        GameEventsManager.Instance.questEvents.OnAdvanceQuest += AdvanceQuest;
        GameEventsManager.Instance.questEvents.OnCompleteQuest += CompleteQuest;

        GameEventsManager.Instance.questEvents.OnQuestStepStateChange += QuestStepStateChange;

        GameEventsManager.Instance.playerEvents.OnPlayerLevelChange += PlayerLevelChange;
    }

    private void OnDisable()
    {
        GameEventsManager.Instance.questEvents.OnStartQuest -= StartQuest;
        GameEventsManager.Instance.questEvents.OnAdvanceQuest -= AdvanceQuest;
        GameEventsManager.Instance.questEvents.OnCompleteQuest -= CompleteQuest;

        GameEventsManager.Instance.questEvents.OnQuestStepStateChange -= QuestStepStateChange;

        GameEventsManager.Instance.playerEvents.OnPlayerLevelChange -= PlayerLevelChange;

    }

    private void Start()
    {
        foreach (var quest in questMap.Values)
        {
            GameEventsManager.Instance.questEvents.QuestStateChange(quest);

            if (quest.questState == QuestState.InProgress)
            {
                quest.InstantiateCurrentQuestStep(transform);
            }
        }
    }

    private void Update()
    {
        foreach (var quest in questMap.Values)
        {
            if (quest.questState == QuestState.RequirementsNotMet && CheckRequirementsMet(quest))
            {
                ChangeQuestState(quest.questInfo.questId, QuestState.CanStart);
            }

            if (quest.questState == QuestState.CanStart && quest.questInfo.autoStart)
            {
                StartQuest(quest.questInfo.questId);
            }
        }
    }

    private void PlayerLevelChange(int level)
    {
        currentPlayerLevel = level;
    }

    private bool CheckRequirementsMet(Quest quest)
    {
        bool meetsRequirements = true;

        if (quest.questInfo.requiredPlayerLevel > currentPlayerLevel)
        {
            meetsRequirements = false;
        }

        foreach (var questPrerequisite in quest.questInfo.questPrerequisites)
        {
            if (GetQuestById(questPrerequisite.questId).questState != QuestState.Completed)
            {
                meetsRequirements = false;
                break;
            }
        }

        return meetsRequirements;
    }

    private void ChangeQuestState(string id, QuestState newState)
    {
        Quest quest = GetQuestById(id);
        if (quest != null)
        {
            quest.questState = newState;
            GameEventsManager.Instance.questEvents.QuestStateChange(quest);
        }
    }

    private void StartQuest(string id)
    {
        Quest quest = GetQuestById(id);
        quest?.InstantiateCurrentQuestStep(transform);
        ChangeQuestState(id, QuestState.InProgress);
    }

    private void AdvanceQuest(string id)
    {
        Quest quest = GetQuestById(id);

        quest?.MoveToNextStep();

        if (quest.IsCurrentStepExists())
        {
            quest.InstantiateCurrentQuestStep(transform);
        }
        else
        {
            CompleteQuest(id);
        }
    }

    private void CompleteQuest(string id)
    {
        Quest quest = GetQuestById(id);
        ClaimReward(quest);
        ChangeQuestState(id, QuestState.Completed);
    }

    private void ClaimReward(Quest quest)
    {
        Debug.Log($"Quest {quest.questInfo.questName} completed! Claiming rewards...");
    }

    private void QuestStepStateChange(string questId, int stepIndex, QuestStepState questStepState)
    {
        Quest quest = GetQuestById(questId);
        quest.StoreQuestStepState(stepIndex, questStepState);
        ChangeQuestState(questId, quest.questState);
    }

    public Dictionary<string, Quest> CreateQuestMap(QuestInfoSO[] allQuests = null)
    {
        if (allQuests == null || allQuests.Length == 0)
        {
            allQuests = Resources.LoadAll<QuestInfoSO>("Quests");
        }

        Dictionary<string, Quest> idToQuestMap = new Dictionary<string, Quest>();
        foreach (QuestInfoSO questInfo in allQuests)
        {
            if (!idToQuestMap.ContainsKey(questInfo.questId))
            {
                idToQuestMap.Add(questInfo.questId, LoadQuest(questInfo));
            }
        }

        return idToQuestMap;
    }

    private Quest GetQuestById(string questId)
    {
        if (questMap.TryGetValue(questId, out Quest quest))
        {
            return quest;
        }
        return null;
    }

    private void OnApplicationQuit()
    {
        foreach (var quest in questMap.Values)
        {
            SaveQuest(quest);
        }
    }

    private void SaveQuest(Quest quest)
    {
        try
        {
            QuestData questData = quest.GetQuestData();
            string serializedData = JsonUtility.ToJson(questData);

            // PlayerPrefs.SetString(quest.questInfo.questId, serializedData); //Test
            Debug.Log(serializedData);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save quest with id " + quest.questInfo.questId + ": " + e);
        }
    }

    private Quest LoadQuest(QuestInfoSO questInfo)
    {
        Quest quest = null;
        try
        {
            if (PlayerPrefs.HasKey(questInfo.questId) && false)
            {
                string serializedData = PlayerPrefs.GetString(questInfo.questId); //Test
                QuestData questData = JsonUtility.FromJson<QuestData>(serializedData);
                quest = new Quest(questInfo, questData.questState, questData.questStepIndex, questData.questStepStates);
            }
            else
            {
                quest = new Quest(questInfo);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to load quest with id " + questInfo.questId + ": " + e);
        }
        return quest;
    }

    private void OnGUI()
    {
        if (questMap == null) return;

        GUILayout.Label("Active Quests:");
        foreach (var kvp in questMap)
        {
            GUILayout.Label($"Quest ID: {kvp.Key}, Name: {kvp.Value.questInfo.questName}, State: {kvp.Value.questState}");
        }
    }

}
