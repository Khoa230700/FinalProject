using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quest")]
public class QuestData : ScriptableObject
{
    [Header("Infomations")]
    public string questID;
    public string questName;
    [TextArea(3, 5)] public string description;

    [Header("Required")]
    public List<QuestObjective> objectives = new List<QuestObjective>();

    [Header("Reward")]
    public int expReward;
    public int goldReward;

    [Header("Condition")]
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
