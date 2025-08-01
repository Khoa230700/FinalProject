using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [Header("List of quests for every scene")]
    [SerializeField] private QuestInfoSO[] questList;

    private Dictionary<string, Quest> questMap;

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
    }

    private void OnDisable()
    {
        GameEventsManager.Instance.questEvents.OnStartQuest -= StartQuest;
        GameEventsManager.Instance.questEvents.OnAdvanceQuest -= AdvanceQuest;
        GameEventsManager.Instance.questEvents.OnCompleteQuest -= CompleteQuest;
    }

    private void Start()
    {
        foreach (var quest in questMap.Values)
        {
            GameEventsManager.Instance.questEvents.QuestStateChanged(quest);
        }
    }

    private void StartQuest(string id)
    {
        Debug.Log("Starting quest: " + id);
    }

    private void AdvanceQuest(string id)
    {
        Debug.Log("Advancing quest: " + id);
    }

    private void CompleteQuest(string id)
    {
        Debug.Log("Completing quest: " + id);
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
                idToQuestMap.Add(questInfo.questId, new Quest(questInfo));
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
}
