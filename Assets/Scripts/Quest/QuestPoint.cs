using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class QuestPoint : MonoBehaviour
{
    [Header("Quests")]
    [SerializeField] private QuestInfoSO questInfoForPoint;

    [Header("Settings")]
    [SerializeField] private bool startPoint = true;
    [SerializeField] private bool completePoint = true;

    private bool isPlayerInRange = false;
    private string questId;
    private QuestState currentQuestState;

    private void Awake()
    {
        questId = questInfoForPoint.questId;
    }

    private void OnEnable()
    {
        GameEventsManager.Instance.questEvents.OnQuestStateChange += QuestStateChanged;
    }

    private void OnDisable()
    {
        GameEventsManager.Instance.questEvents.OnQuestStateChange -= QuestStateChanged;
    }

    private void QuestStateChanged(Quest quest)
    {
        if (quest.questInfo.questId.Equals(questId))
        {
            currentQuestState = quest.questState;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("Player entered quest point range: " + gameObject.name);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    void Update()
    {
        if (isPlayerInRange && KeyBindingManager.Instance.GetKeyDown("Interact"))
        {
            if (currentQuestState == QuestState.CanStart && startPoint)
            {
                GameEventsManager.Instance.questEvents.StartQuest(questId);
            }
            else if (currentQuestState == QuestState.CanComplete && completePoint)
            {
                GameEventsManager.Instance.questEvents.CompleteQuest(questId);
            }
            else
            {
                Debug.Log("Cannot interact with quest point: " + gameObject.name + ". Current state: " + currentQuestState);
            }
        }
    }
}
