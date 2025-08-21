using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestTracker : MonoBehaviour
{
    [Header("Quest Tracking")]
    public QuestObjectiveType trackingType;
    public string targetID;

    private void Start()
    {
        if (string.IsNullOrEmpty(targetID))
        {
            Debug.LogError("targetID không thể null hoặc empty");
        }
    }

    //GỌI KHI ĐỐI TƯỢNG BỊ TIÊU DIỆT
    public void OnKilled()
    {
        if (trackingType == QuestObjectiveType.Kill)
        {
            QuestManager.Instance.UpdateQuestProgress(QuestObjectiveType.Kill, targetID);
        }
    }

    //GỌI KHI ĐỐI TƯỢNG BỊ THU THẬP
    public void OnCollected()
    {
        if (trackingType == QuestObjectiveType.Collect)
        {
            QuestManager.Instance.UpdateQuestProgress(QuestObjectiveType.Collect, targetID);
        }
    }

    //GỌI KHI TƯƠNG TÁC
    public void OnInteracted()
    {
        if (trackingType == QuestObjectiveType.Interact)
        {
            QuestManager.Instance.UpdateQuestProgress(QuestObjectiveType.Interact, targetID);
        }
    }

    //GỌI KHI ĐẾN VỊ TRÍ
    private void OnTriggerEnter(Collider other)
    {
        if (trackingType == QuestObjectiveType.Reach && other.CompareTag("Player"))
        {
            QuestManager.Instance.UpdateQuestProgress(QuestObjectiveType.Reach, targetID);
        }
    }
}
