using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quest")]
public class QuestSO : ScriptableObject
{
    [Header("Infomations")]
    public string questID;
    public string questName;
    [TextArea(3, 5)] public string description;

    [Header("Requirements")]
    public List<QuestObjective> objectives = new();
    public List<QuestSO> prerequisiteQuests = new();
    public int requiredLevel = 1;

    [Header("Rewards")]
    public int expReward;
    public int coinReward;

    [Header("Settings")]
    public bool isSaved = true;
    public bool autoStart = false;
    public string autoStartScene = "";

#if UNITY_EDITOR
    void OnValidate()
    {
        if (string.IsNullOrEmpty(questID))
        {
            questID = questName + Guid.NewGuid().ToString();
        }
    }
#endif
}
