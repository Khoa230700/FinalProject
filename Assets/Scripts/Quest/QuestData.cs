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

    [Header("Rewards")]
    public int expReward;
    public int goldReward;

    [Header("Conditions")]
    public List<string> prerequisiteQuests = new List<string>();
    public int requiredLevel = 1;

    [Header("Settings")]
    public bool autoStart = false;
    public string autoStartScene = "";
    public float autoStartDelay = 0f;

    void OnValidate()
    {
        if (string.IsNullOrEmpty(questID))
        {
            questID = questName + Guid.NewGuid().ToString();
        }
    }
}
