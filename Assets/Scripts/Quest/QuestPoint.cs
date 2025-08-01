using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class QuestPoint : MonoBehaviour
{
    [Header("Quests")]
    [SerializeField] private QuestInfoSO questInfoForPoint;

    private bool isPlayerInRange = false;
    private string questId;
    private QuestState currentQuestState;

    private void Awake()
    {
        questId = questInfoForPoint.questId;
    }

    private void OnEnable()
    {
        Debug.Log(GameEventsManager.Instance);
        GameEventsManager.Instance.questEvents.OnQuestStateChanged += QuestStateChanged;
    }

    private void OnDisable()
    {
        GameEventsManager.Instance.questEvents.OnQuestStateChanged -= QuestStateChanged;
    }

    private void QuestStateChanged(Quest quest)
    {
        if (quest.info.questId.Equals(questId))
        {
            currentQuestState = quest.state;
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
            Debug.Log("Player exited quest point range: " + gameObject.name);
        }
    }

    void Update()
    {
        if (isPlayerInRange && KeyBindingManager.Instance.GetKeyDown("Interact"))
        {
            
        }
    }
}
