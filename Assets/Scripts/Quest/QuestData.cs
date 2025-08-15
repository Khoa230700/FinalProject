using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quest")]
public class QuestData : ScriptableObject
{
    [Header("Thông tin Quest")]
    public string questID;
    public string questName;
    [TextArea(3, 5)] public string description;

    [Header("Yêu cầu")]
    public List<QuestObjective> objectives = new List<QuestObjective>();

    [Header("Phần thưởng")]
    public int expReward;
    public int goldReward;

    [Header("Điều kiện")]
    public List<string> prerequisiteQuests = new List<string>();
    public int requiredLevel = 1;

    void OnValidate()
    {
        if (string.IsNullOrEmpty(questID))
        {
            questID = questName + Guid.NewGuid().ToString();
        }
    }
}
